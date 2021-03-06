﻿using Manatee.Json;
using Manatee.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cursive.Serialization
{
    internal static class ProcessLoadingService
    {
        public static bool LoadProcesses(Workspace workspace, string processJson, bool validateSchema, out List<string> errors)
        {
            var serializer = new JsonSerializer();

            var jsonData = JsonValue.Parse(processJson);

            if (validateSchema)
            {
                var validationResult = Schemas.Processes.Value.Validate(jsonData);

                if (!validationResult.IsValid)
                {
                    JsonValue errorJson = validationResult.ToJson(serializer);

                    errors = new List<string>
                    {
                        $"Process data is not valid: {errorJson}"
                    };
                    return false;
                }
            }

            var processData = serializer.Deserialize<UserProcessDTO[]>(jsonData);

            return LoadUserProcesses(workspace, processData, out errors);
        }

        private static bool LoadUserProcesses(Workspace workspace, UserProcessDTO[] processData, out List<string> errors)
        {
            var typesByName = workspace.Types.ToDictionary(t => t.Name);

            var processes = CreateProcesses(typesByName, processData, out errors);
            if (errors != null)
                return false;

            var processesByName = GetProcessesByName(processes, workspace.SystemProcesses, out errors);
            if (errors != null)
                return false;

            if (!LoadSteps(processData, processesByName, out errors))
                return false;

            if (!ApplyProcessesToWorkspace(workspace, processes, out errors))
                return false;

            errors = null;
            return true;
        }

        private static Dictionary<string, Process> GetProcessesByName(
            List<UserProcess> userProcesses,
            IList<SystemProcess> systemProcesses,
            out List<string> errors
        )
        {
            if (!AreNamesUnique(userProcesses, out errors))
                return null;

            var processesByName = userProcesses.ToDictionary(p => p.Name, p => p as Process);

            errors = new List<string>();
            foreach (var process in systemProcesses)
            {
                if (processesByName.ContainsKey(process.Name))
                    errors.Add($"Process name is already used by a system process: {process.Name}");
                else
                    processesByName.Add(process.Name, process);
            }

            if (errors.Any())
                return null;

            errors = null;
            return processesByName;
        }

        private static bool ApplyProcessesToWorkspace(Workspace workspace, List<UserProcess> processes, out List<string> errors)
        {
            ClearUserProcesses(workspace);

            workspace.UserProcesses.AddRange(processes);

            errors = new List<string>();

            foreach (var required in workspace.RequiredProcesses)
            {
                var implementations = processes.Where(p => p.Name == required.Name).ToArray();

                if (implementations.Length == 0)
                {
                    errors.Add($"No implementation of required process: {required.Name}");
                }
                else if (implementations.Length > 1)
                {
                    errors.Add($"Multiple implementations of required process: {required.Name}");
                }
                else
                {
                    required.Implementation = implementations[0];
                }
            }

            if (errors.Any())
            {
                ClearUserProcesses(workspace);
                return false;
            }

            errors = null;
            return true;
        }

        private static bool LoadSteps(UserProcessDTO[] processData, Dictionary<string, Process> processesByName, out List<string> errors)
        {
            var stepErrors = new List<string>();

            foreach (var process in processData)
            {
                var userProcess = processesByName[process.Name] as UserProcess;

                if (LoadProcessSteps(userProcess, process.Steps, processesByName, stepErrors))
                {
                    CheckUnassignedVariables(userProcess, stepErrors);
                }
            }

            if (stepErrors.Any())
            {
                errors = stepErrors;
                return false;
            }

            errors = null;
            return true;
        }

        private static bool LoadProcessSteps(UserProcess process, List<StepDTO> steps, Dictionary<string, Process> processesByName, List<string> errors)
        {
            var stepsById = new Dictionary<string, Step>();

            var stepsWithInputs = new List<Tuple<StepDTO, Step, IReadOnlyList<Parameter>>>();
            var stepsWithOutputs = new List<Tuple<StepDTO, ReturningStep, IReadOnlyList<Parameter>>>();

            int initialErrorCount = errors.Count;

            foreach (var stepData in steps)
            {
                if (stepsById.ContainsKey(stepData.ID))
                {
                    errors.Add($"Process \"{process.Name}\" has multiple steps with ID {stepData.ID}");
                    continue;
                }

                switch (stepData.Type)
                {
                    case "start":
                        {
                            var step = new StartStep(stepData.ID);

                            if (process.FirstStep == null)
                                process.FirstStep = step;
                            else
                                errors.Add($"Process \"{process.Name}\" has multiple start steps");

                            stepsWithOutputs.Add(new Tuple<StepDTO, ReturningStep, IReadOnlyList<Parameter>>(stepData, step, process.Inputs));
                            stepsById.Add(step.ID, step);
                            break;
                        }
                    case "stop":
                        {
                            var step = new StopStep(stepData.ID, stepData.PathName);
                            stepsWithInputs.Add(new Tuple<StepDTO, Step, IReadOnlyList<Parameter>>(stepData, step, process.Outputs));
                            stepsById.Add(step.ID, step);
                            break;
                        }
                    case "process":
                        {
                            if (!processesByName.TryGetValue(stepData.InnerProcess, out Process innerProcess))
                            {
                                errors.Add($"Unrecognised process \"{stepData.InnerProcess}\" on step {stepData.ID} in process \"{process.Name}\"");
                                break;
                            }

                            var step = new UserStep(stepData.ID, innerProcess);
                            stepsWithInputs.Add(new Tuple<StepDTO, Step, IReadOnlyList<Parameter>>(stepData, step, innerProcess.Inputs));
                            stepsWithOutputs.Add(new Tuple<StepDTO, ReturningStep, IReadOnlyList<Parameter>>(stepData, step, innerProcess.Outputs));
                            stepsById.Add(step.ID, step);
                            break;
                        }
                    default:
                        errors.Add($"Invalid type \"{stepData.Type}\" on step {stepData.ID} in process \"{process.Name}\"");
                        break;
                }
            }

            var variablesByName = process.Variables.ToDictionary(v => v.Name);

            foreach (var stepInfo in stepsWithInputs)
            {
                var stepData = stepInfo.Item1;
                var step = stepInfo.Item2;
                var parameters = stepInfo.Item3;

                if (stepData.Inputs != null)
                    MapParameters(stepData.Inputs, step, parameters, variablesByName, process, true, errors);
            }

            foreach (var stepInfo in stepsWithOutputs)
            {
                var stepData = stepInfo.Item1;
                var step = stepInfo.Item2;
                var parameters = stepInfo.Item3;

                if (stepData.Outputs != null)
                    MapParameters(stepData.Outputs, step, parameters, variablesByName, process, false, errors);

                if (stepData.ReturnPath != null)
                {
                    if (stepsById.TryGetValue(stepData.ReturnPath, out Step destStep))
                        step.DefaultReturnPath = destStep;
                    else
                        errors.Add($"Step {step.ID} in process \"{process.Name}\" tries to connect to non-existent step \"{stepData.ReturnPath}\"");
                }
                else if (stepData.ReturnPaths != null && step is UserStep userStep)
                {
                    var expectedReturnPaths = userStep.ChildProcess.ReturnPaths;

                    var mappedPaths = new HashSet<string>();

                    foreach (var returnPath in stepData.ReturnPaths)
                    {
                        if (!expectedReturnPaths.Contains(returnPath.Key))
                            errors.Add($"Step {step.ID} in process \"{process.Name}\" tries to map unexpected return path \"{returnPath.Key}\"");
                        else if (stepsById.TryGetValue(returnPath.Value, out Step destStep))
                        {
                            if (mappedPaths.Contains(returnPath.Key))
                            {
                                errors.Add($"Step {step.ID} in process \"{process.Name}\" tries to map the \"{returnPath.Key}\" return path multiple times");
                                continue;
                            }

                            userStep.ReturnPaths[returnPath.Key] = destStep;
                            mappedPaths.Add(returnPath.Key);
                        }
                        else
                            errors.Add($"Step {step.ID} tries to connect to non-existent step \"{returnPath.Value}\" in process \"{process.Name}\"");
                    }

                    foreach (var path in expectedReturnPaths)
                        if (!mappedPaths.Contains(path))
                            errors.Add($"Step {step.ID} in process \"{process.Name}\" fails to map the \"{path}\" return path");
                }
            }

            return errors.Count == initialErrorCount;
        }

        private static void MapParameters(
            Dictionary<string, string> paramData,
            Step step,
            IReadOnlyList<Parameter> parameters,
            Dictionary<string, Variable> variablesByName,
            UserProcess process,
            bool isInputParam,
            List<string> errors
        )
        {
            foreach (var param in paramData)
            {
                Parameter parameter = parameters.FirstOrDefault(p => p.Name == param.Key);
                if (parameter == null)
                {
                    var paramType = isInputParam ? "input" : "output";
                    errors.Add($"Step {step.ID} tries to map non-existent {paramType} \"{param.Key}\" in process \"{process.Name}\"");
                    continue;
                }

                if (!variablesByName.TryGetValue(param.Value, out Variable variable))
                {
                    var paramType = isInputParam ? "input" : "output";
                    errors.Add($"Step {step.ID} tries to map an {paramType} to non-existent variable \"{param.Value}\" in process \"{process.Name}\"");
                    continue;
                }

                DataType fromType, toType;
                Dictionary<string, Variable> mapping;

                if (isInputParam)
                {
                    fromType = variable.Type;
                    toType = parameter.Type;
                    mapping = step.InputMapping;
                }
                else
                {
                    fromType = parameter.Type;
                    toType = variable.Type;
                    mapping = step.OutputMapping;
                }

                if (!fromType.IsAssignableTo(toType))
                {
                    var message = isInputParam
                        ? $"Step {step.ID} tries to map the \"{param.Value}\" variable to its \"{param.Key}\" input, but their types are not compatible ({variable.Type.Name} and {parameter.Type.Name}), in process \"{process.Name}\""
                        : $"Step {step.ID} tries to map its \"{param.Key}\" output to the \"{param.Value}\" variable, but their types are not compatible ({parameter.Type.Name} and {variable.Type.Name}), in process \"{process.Name}\"";
                    
                    errors.Add(message);
                    continue;
                }

                mapping[parameter.Name] = variable;
            }

            if (isInputParam)
                foreach (var param in parameters)
                    if (!step.InputMapping.ContainsKey(param.Name))
                    {
                        errors.Add($"Step {step.ID} fails to map \"{param.Name}\" input parameter in process \"{process.Name}\"");
                    }
        }

        private static bool CheckUnassignedVariables(UserProcess process, List<string> stepErrors)
        {
            var unassignedVariables = new HashSet<Variable>(
                process.Variables
                    .Where(v => v.InitialValue == null)
            );

            var visitedSteps = new HashSet<Step>();

            return CheckUnassignedVariables(process, process.FirstStep, visitedSteps, unassignedVariables, stepErrors);
        }

        private static bool CheckUnassignedVariables(UserProcess process, ReturningStep currentStep, HashSet<Step> visitedSteps, HashSet<Variable> unassignedVariables, List<string> errors)
        {
            visitedSteps.Add(currentStep);

            unassignedVariables = new HashSet<Variable>(unassignedVariables);

            // remove variables that currentStep's outputs connect to from the unassigned list
            foreach (var variable in currentStep.OutputMapping.Values)
                unassignedVariables.Remove(variable);

            var allValid = true;

            ICollection<Step> nextSteps;
            
            if (currentStep.DefaultReturnPath != null)
            {
                nextSteps = new Step[] { currentStep.DefaultReturnPath };
            }
            else if (currentStep.StepType == StepType.Process)
            {
                nextSteps = (currentStep as UserStep).ReturnPaths.Values;
            }
            else
            {
                errors.Add($"Step {currentStep.ID} in process \"{process.Name}\" has no return paths");
                return false;
            }

            foreach (var nextStep in nextSteps)
            {
                if (visitedSteps.Contains(nextStep))
                    continue; // already processed this step, don't do it again

                // check each input of nextStep, if it touches anything in unassignedVariables, that's not valid
                foreach (var variable in nextStep.InputMapping.Values)
                {
                    if (unassignedVariables.Contains(variable))
                    {
                        errors.Add($"Step {currentStep.ID} in process \"{process.Name}\" uses a variable before it is assigned: {variable.Name}");
                        return false; // once an uninitialized variable is used, stop down this branch
                    }
                }

                if (nextStep.StepType == StepType.Process
                    && !CheckUnassignedVariables(process, nextStep as ReturningStep, visitedSteps, unassignedVariables, errors))
                {
                    allValid = false;
                }
            }

            return allValid;
        }

        private static bool AreNamesUnique(List<UserProcess> processes, out List<string> errors)
        {
            errors = new List<string>();

            var usedNames = new HashSet<string>();

            foreach (var process in processes)
            {
                if (usedNames.Contains(process.Name))
                {
                    errors.Add($"Multiple processes have the same name: {process.Name}");
                    continue;
                }

                usedNames.Add(process.Name);
            }

            return !errors.Any();
        }

        private static List<UserProcess> CreateProcesses(
            Dictionary<string, DataType> typesByName,
            UserProcessDTO[] processData,
            out List<string> errors
        )
        {
            var creationErrors = new List<string>();

            var processes = processData.Select(p => CreateProcess(p, typesByName, creationErrors)).ToList();

            if (creationErrors.Any())
            {
                errors = creationErrors;
                return null;
            }

            errors = null;
            return processes;
        }

        private static UserProcess CreateProcess(UserProcessDTO processData, Dictionary<string, DataType> typesByName, List<string> errors)
        {
            var inputs = LoadParameters(processData.Inputs, typesByName, errors, processData, "input");

            var outputs = LoadParameters(processData.Outputs, typesByName, errors, processData, "output");

            var variables = LoadVariables(processData, typesByName, errors);

            return new UserProcess
            (
                processData.Name,
                processData.Description,
                inputs,
                outputs,
                processData.ReturnPaths,
                variables
            );
        }

        private static Parameter[] LoadParameters(
            IEnumerable<ParameterDTO> parameters,
            Dictionary<string, DataType> typesByName,
            List<string> errors,
            UserProcessDTO process,
            string paramType
        )
        {
            var paramsWithTypes = parameters.Select(
                p => new Tuple<ParameterDTO, DataType>
                (
                    p,
                    typesByName.ContainsKey(p.Type)
                        ? typesByName[p.Type]
                        : null
                )
            ).ToArray();

            var errorParams = paramsWithTypes
                .Where(i => i.Item2 == null)
                .Select(i => i.Item1);

            foreach (var param in errorParams)
                errors.Add($"Unrecognised type \"{param.Type}\" used by {paramType} of process {process.Name}");

            return paramsWithTypes
                .Select(i => new Parameter(i.Item1.Name, i.Item2))
                .ToArray();
        }

        private static List<Variable> LoadVariables(UserProcessDTO process, Dictionary<string, DataType> typesByName, List<string> errors)
        {
            var variables = new List<Variable>();

            var usedNames = new HashSet<string>();

            foreach (var variableData in process.Variables)
            {
                if (usedNames.Contains(variableData.Name))
                {
                    errors.Add($"Multiple variables of process {process.Name} use the same name: {variableData.Name}");
                    continue;
                }

                usedNames.Add(variableData.Name);

                if (!typesByName.TryGetValue(variableData.Type, out DataType dataType))
                {
                    errors.Add($"Unrecognised type \"{variableData.Type}\" used by variable {variableData.Name} of process {process.Name}");
                    continue;
                }

                var value = variableData.InitialValue != null && dataType is IDeserializable
                    ? (dataType as IDeserializable).Deserialize(variableData.InitialValue)
                    : dataType.GetDefaultValue();

                variables.Add(new Variable(variableData.Name, dataType, value));
            }

            return variables;
        }

        public static void ClearUserProcesses(Workspace workspace)
        {
            foreach (var process in workspace.RequiredProcesses)
                process.Implementation = null;

            workspace.UserProcesses.Clear();
        }
    }
}
