using Cursive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace CursiveRuntime.Services
{
    internal static class ProcessLoadingService
    {
        internal static bool LoadProcesses(Workspace workspace, XmlDocument doc, out List<string> errors)
        {
            var success = true;
            errors = new List<string>();

            var schemaResourceName = "Cursive.processes.xsd";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(schemaResourceName))
            using (XmlReader reader = XmlReader.Create(stream))
            {
                doc.Schemas.Add("http://cursive.ftwinston.com", reader);
            }

            validationErrors = errors;
            doc.Validate(ValidationEventHandler);
            validationErrors = null;

            var processNodes = doc.SelectNodes("/Processes/Process");
            var loadedProcesses = new List<UserProcess>();
            var allUserSteps = new List<UserStepLoadingInfo>();
            foreach (XmlElement processNode in processNodes)
            {
                List<UserStepLoadingInfo> userSteps;
                var process = LoadUserProcess(workspace, processNode, out userSteps, errors);
                if (process == null)
                {
                    success = false;
                    continue;
                }

                loadedProcesses.Add(process);
                allUserSteps.AddRange(userSteps);
            }
            
            foreach (var step in allUserSteps)
            {
                if (!LinkChildProcess(workspace.Processes, step, errors))
                    success = false;
            }

            foreach (var required in workspace.RequiredProcesses)
            {
                if (required.Value.ActualProcess == null)
                {
                    success = false;
                    errors.Add(string.Format("{0}: No implementation of this required process", required.Key));
                    continue;
                }
            }

            foreach (var process in loadedProcesses)
            {
                List<string> processErrors;
                if (!ProcessValidationService.ValidateProcess(workspace, process, out processErrors))
                {
                    success = false;
                    foreach (var error in processErrors)
                        errors.Add(string.Format("{0}: {1}", process.Name, error));
                }
            }

            return success;
        }

        private static List<string> validationErrors = null;
        private static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            validationErrors.Add(string.Format("schema validation error: {0}", e.Message));
        }
        
        private static UserProcess LoadUserProcess(Workspace workspace, XmlElement processNode, out List<UserStepLoadingInfo> loadingSteps, List<string> errors)
        {
            var processName = processNode.GetAttribute("name");
            var desc = processNode.SelectSingleNode("Description").InnerText;

            var inputNodes = processNode.SelectNodes("Input");
            var outputNodes = processNode.SelectNodes("Output");
            var variableNodes = processNode.SelectNodes("Variable");
            var returnPathNodes = processNode.SelectNodes("ReturnPath");

            var inputs = new List<ValueKey>();
            var outputs = new List<ValueKey>();
            var defaultVariables = new ValueSet();
            var inputsByName = new Dictionary<string, ValueKey>();
            var outputsByName = new Dictionary<string, ValueKey>();
            var variablesByName = new Dictionary<string, ValueKey>();
            var returnPaths = new HashSet<string>();

            RequiredProcess wrapper;
            if (!workspace.RequiredProcesses.TryGetValue(processName, out wrapper))
                wrapper = null;

            bool success = LoadProcessParameters(workspace, inputNodes, wrapper?.Inputs, inputs, inputsByName, processName, "input", errors);
            success |= LoadProcessParameters(workspace, outputNodes, wrapper?.Outputs, outputs, outputsByName, processName, "output", errors);

            foreach (XmlElement variable in variableNodes)
            {
                var type = workspace.GetType(variable.GetAttribute("type"));
                var name = variable.GetAttribute("name");
                var definition = new ValueKey(name, type);
                if (variablesByName.ContainsKey(name))
                {
                    errors.Add(string.Format("Process '{0}' has multiple variables with the same name: '{1}'", processName, name));
                    success = false;
                    continue;
                }
                variablesByName.Add(name, definition);

                var initialValue = variable.HasAttribute("initialValue") ? variable.GetAttribute("initialValue") : null;

                if (initialValue != null && definition.Type is IDeserializable)
                    defaultVariables[definition] = (definition.Type as IDeserializable).Deserialize(initialValue);
                else
                    defaultVariables[definition] = definition.Type.GetDefaultValue();
            }
            foreach (XmlElement returnPath in returnPathNodes)
            {
                var pathName = returnPath.GetAttribute("name");
                if (returnPaths.Contains(pathName))
                {
                    errors.Add(string.Format("Process '{0}' has multiple return paths with the same name: '{1}'. Multiple stop steps can return the same path, but the path must only be defined once.", processName, pathName));
                    success = false;
                    continue;
                }
                returnPaths.Add(pathName);
            }

            if (wrapper != null)
            {
                if (!CompareToRequiredSignature(processName, inputsByName.Keys, wrapper.Inputs, "input", errors, val => val.Name, (set, val) => set.Any(p => p.Name == val)))
                    success = false;

                if (!CompareToRequiredSignature(processName, outputsByName.Keys, wrapper.Outputs, "output", errors, val => val.Name, (set, val) => set.Any(p => p.Name == val)))
                    success = false;

                if (!CompareToRequiredSignature(processName, returnPaths, wrapper.ReturnPaths, "return path", errors, val => val, (set, val) => set.Contains(val)))
                    success = false;
            }

            XmlElement steps = processNode.SelectSingleNode("Steps") as XmlElement;
            var stepsByName = new Dictionary<string, Step>();
            loadingSteps = new List<UserStepLoadingInfo>();

            StartStep firstStep = null; XmlElement firstStepNode = null;
            foreach (XmlElement stepNode in steps.ChildNodes)
            {
                var step = LoadProcessStep(stepNode, inputsByName, outputsByName, variablesByName, returnPaths, loadingSteps, errors);
                if (step == null)
                {
                    success = false;
                    continue;
                }

                if (step is StartStep)
                {
                    firstStep = step as StartStep;
                    firstStepNode = stepNode;
                    continue;
                }
                
                stepsByName.Add(step.Name, step);
            }

            if (firstStep == null)
            {
                errors.Add(string.Format("Process '{0}' lacks a Start step", processName));
                success = false;
            }
            else if (!LoadReturnPaths(firstStep, firstStepNode, stepsByName, errors))
                success = false;

            foreach (var step in loadingSteps)
                if (!LoadReturnPaths(step.Step, step.Element, stepsByName, errors))
                    success = false;

            if (!success)
                return null;

            UserProcess process = new UserProcess(processName, desc, inputs, outputs, returnPaths.ToArray(), defaultVariables, firstStep, stepsByName.Values);
            workspace.Processes.Add(processName, process);

            if (wrapper != null)
                wrapper.ActualProcess = process;

            return process;
        }

        private static bool LoadProcessParameters(Workspace workspace, XmlNodeList parameterNodes, IReadOnlyCollection<ValueKey> requiredParams, List<ValueKey> parameters, Dictionary<string, ValueKey> parametersByName, string processName, string parameterType, List<string> errors)
        {
            bool success = true;
            foreach (XmlElement parameterNode in parameterNodes)
            {
                var type = workspace.GetType(parameterNode.GetAttribute("type"));
                var name = parameterNode.GetAttribute("name");
                ValueKey param;

                if (requiredParams != null)
                {
                    param = requiredParams.FirstOrDefault(p => p.Name == name);
                    if (param == null)
                    {
                        errors.Add(string.Format("Process '{0}' is required by the workspace, but has an {2} not defined in the workspace: '{1}'", processName, name, parameterType));
                        success = false;
                        continue;
                    }
                    else if (!type.IsAssignableFrom(param.Type))
                    {
                        errors.Add(string.Format("Process '{0}' is required by the workspace, but its '{1}' {3} is of type '{2}', where the workspace expects type '{3}'", processName, name, type.Name, param.Type.Name, parameterType));
                        success = false;
                        continue;
                    }
                }
                else
                    param = new ValueKey(name, type);

                parameters.Add(param);
                parametersByName.Add(name, param);
            }
            return success;
        }

        private static bool CompareToRequiredSignature<T>(string processName, ICollection<string> paramNames, IReadOnlyCollection<T> wrapperParams, string paramType, List<string> errors, Func<T, string> getName, Func<IReadOnlyCollection<T>, string, bool> contains)
        {
            bool success = true;

            int numWrapperParams;
            if (wrapperParams != null)
            {
                numWrapperParams = wrapperParams.Count;
                foreach (var param in wrapperParams)
                    if (!paramNames.Contains(getName(param)))
                    {
                        errors.Add(string.Format("Process '{0}' is required by the workspace, but is missing the required {2} '{1}'", processName, getName(param), paramType));
                        success = false;
                    }
            }
            else
                numWrapperParams = 0;

            if (paramNames.Count != numWrapperParams)
                foreach (var param in paramNames)
                    if (wrapperParams == null || !contains(wrapperParams, param))
                    {
                        errors.Add(string.Format("Process '{0}' is required by the workspace, but contains unexpected {2} '{1}'", processName, param, paramType));
                        success = false;
                    }

            return success;
        }

        private static bool LinkChildProcess(Dictionary<string, Process> processes, UserStepLoadingInfo stepInfo, List<string> errors)
        {
            var step = stepInfo.Step;
            var processName = stepInfo.Element.GetAttribute("process");
            Process process;
            if (!processes.TryGetValue(processName, out process))
            {
                errors.Add(string.Format("Child process name not recognised for step {0}: {1}", step.Name, processName));
                return false;
            }

            step.ChildProcess = process;
            bool success = true;

            foreach (var inputInfo in stepInfo.InputsToMap)
            {
                ValueKey input = process.Inputs.FirstOrDefault(p => p.Name == inputInfo.Key);
                if (input == null)
                {
                    errors.Add(string.Format("Step {0} tries to map '{1}' variable to non-existant input '{2}'", step.Name, inputInfo.Value.Name, inputInfo.Key));
                    success = false;
                    continue;
                }

                if (!inputInfo.Value.Type.IsAssignableFrom(input.Type))
                {
                    errors.Add(string.Format("Step {0} tries to map the '{1}' variable to the '{2}' input, but these have different types ('{3}' and '{4}')", step.Name, inputInfo.Value.Name, inputInfo.Key, inputInfo.Value.Type.Name, input.Type.Name));
                    success = false;
                    continue;
                }

                step.MapInputParameter(input, inputInfo.Value);
            }

            foreach (var outputInfo in stepInfo.OutputsToMap)
            {
                ValueKey output = process.Outputs.FirstOrDefault(p => p.Name == outputInfo.Key);
                if (output == null)
                {
                    errors.Add(string.Format("Step {0} tries to map non-existant output '{2}' to '{1}' variable", step.Name, outputInfo.Value.Name, outputInfo.Key));
                    success = false;
                    continue;
                }

                if (!output.Type.IsAssignableFrom(outputInfo.Value.Type))
                {
                    errors.Add(string.Format("Step {0} step tries to map the '{2}' output to the '{1}' variable, but these have different types ('{3}' and '{4}')", step.Name, outputInfo.Value.Name, outputInfo.Key, outputInfo.Value.Type.Name, output.Type.Name));
                    success = false;
                    continue;
                }

                step.MapOutputParameter(output, outputInfo.Value);
            }

            return success;
        }

        private static Step LoadProcessStep(XmlElement stepNode, Dictionary<string, ValueKey> inputs, Dictionary<string, ValueKey> outputs, Dictionary<string, ValueKey> variables, HashSet<string> returnPaths, List<UserStepLoadingInfo> loadingSteps, List<string> errors)
        {
            var name = stepNode.GetAttribute("ID");
            var inputNodes = stepNode.SelectNodes("Input");
            var outputNodes = stepNode.SelectNodes("Output");
            bool success = true;
            Step step;
            
            if (stepNode.Name == "Start")
            {
                step = new StartStep(name);
                foreach (XmlElement output in outputNodes)
                {
                    var paramName = output.GetAttribute("name");
                    var variableName = output.GetAttribute("destination");

                    ValueKey inputParam, variableParam;
                    if (!inputs.TryGetValue(paramName, out inputParam))
                    {
                        errors.Add(string.Format("The start step tries to map non-existant input parameter '{0}' to variable '{1}'", paramName, variableName));
                        success = false;
                        continue;
                    }

                    if (!variables.TryGetValue(variableName, out variableParam))
                    {
                        errors.Add(string.Format("The start step tries to map input parameter '{0}' to non-existant variable '{1}'", paramName, variableName));
                        success = false;
                        continue;
                    }

                    if (!variableParam.Type.IsAssignableFrom(inputParam.Type))
                    {
                        errors.Add(string.Format("The start step tries to map the '{1}' input to the '{0}' variable, but these have different types ('{2}' and '{3}')", variableParam.Name, inputParam.Name, variableParam.Type.Name, inputParam.Type.Name));
                        success = false;
                        continue;
                    }

                    step.MapOutputParameter(inputParam, variableParam);
                }
            }
            else if (stepNode.Name == "Stop")
            {
                var returnValue = stepNode.GetAttribute("name");
                if (returnValue == string.Empty)
                {
                    returnValue = null;

                    if (returnPaths.Count > 0)
                    {
                        errors.Add(string.Format("The stop step '{0}' doesn't specify a return path name, but this process uses named return paths.", name));
                        success = false;
                    }
                }
                else if (!returnPaths.Contains(returnValue))
                {
                    if (returnPaths.Count == 0)
                        errors.Add(string.Format("The stop step '{0}' specifies a return path name ('{1}'), but this process doesn't use named return paths.", name, returnValue));
                    else
                        errors.Add(string.Format("The stop step '{0}' specifies a return path name ('{1}') that isn't valid for this process.", name, returnValue));

                    success = false;
                }

                step = new StopStep(name, returnValue);
                foreach (XmlElement input in inputNodes)
                {
                    var paramName = input.GetAttribute("name");
                    var variableName = input.GetAttribute("source");

                    ValueKey outputParam, variableParam;
                    if (!outputs.TryGetValue(paramName, out outputParam))
                    {
                        errors.Add(string.Format("The stop step '{0}' tries to map variable '{2}' to non-existant output parameter '{1}'", name, paramName, variableName));
                        success = false;
                        continue;
                    }

                    if (!variables.TryGetValue(variableName, out variableParam))
                    {
                        errors.Add(string.Format("The stop step '{0}' tries to map non-existant variable '{2}' to output parameter '{1}'", name, paramName, variableName));
                        success = false;
                        continue;
                    }

                    if (!outputParam.Type.IsAssignableFrom(variableParam.Type))
                    {
                        errors.Add(string.Format("The stop step '{0}' tries to map the '{1}' variable to the '{2}' output , but these have different types ('{3}' and '{4}')", name, variableParam.Name, outputParam.Name, variableParam.Type.Name, outputParam.Type.Name));
                        success = false;
                        continue;
                    }

                    step.MapInputParameter(outputParam, variableParam);
                }
            }
            else
            {
                var userStep = new UserStep(name, null);
                var stepInfo = new UserStepLoadingInfo(userStep, stepNode);
                step = userStep;

                foreach (XmlElement inputNode in inputNodes)
                {
                    var paramName = inputNode.GetAttribute("name");
                    var variableName = inputNode.GetAttribute("source");

                    ValueKey variableParam;
                    if (!variables.TryGetValue(variableName, out variableParam))
                    {
                        errors.Add(string.Format("Step '{0}' tries to map non-existant variable '{2}' to input parameter '{1}'", name, paramName, variableName));
                        success = false;
                        continue;
                    }

                    stepInfo.InputsToMap.Add(paramName, variableParam);
                }
                foreach (XmlElement outputNode in outputNodes)
                {
                    var paramName = outputNode.GetAttribute("name");
                    var variableName = outputNode.GetAttribute("destination");

                    ValueKey variableParam;
                    if (!variables.TryGetValue(variableName, out variableParam))
                    {
                        errors.Add(string.Format("Step '{0}' tries to map output parameter '{1}' to non-existant variable '{2}'", name, paramName, variableName));
                        success = false;
                        continue;
                    }

                    stepInfo.OutputsToMap.Add(paramName, variableParam);
                }

                if (success)
                    loadingSteps.Add(stepInfo);
            }
            
            return success ? step : null;
        }

        private static bool LoadReturnPaths(ReturningStep step, XmlElement stepNode, Dictionary<string, Step> stepsByName, List<string> errors)
        {
            var singlePathNode = stepNode.SelectSingleNode("ReturnPath") as XmlElement;
            if (singlePathNode != null)
            {
                // this step has a single, unnamed return path
                var pathStepName = singlePathNode.GetAttribute("targetStepID");

                Step returnStep;
                if (!stepsByName.TryGetValue(pathStepName, out returnStep))
                {
                    errors.Add(string.Format("Return path target of step {0} not recognised: {1}", step.Name, pathStepName));
                    return false;
                }

                step.DefaultReturnPath = returnStep;
                return true;
            }

            if (step is StartStep)
            {
                errors.Add(string.Format("Start step {0} doesn't have a single, unnamed return path", step.Name));
                return false;
            }

            var userStep = step as UserStep;
            var pathNodes = stepNode.SelectNodes("NamedReturnPath");
            bool success = true;

            foreach (XmlElement pathNode in pathNodes)
            {
                var pathName = pathNode.GetAttribute("name");
                var pathStepName = pathNode.GetAttribute("targetStepID");

                Step returnStep;
                if (!stepsByName.TryGetValue(pathStepName, out returnStep))
                {
                    errors.Add(string.Format("'{0}' return path target of step {1} not recognised: {2}", pathName, step.Name, pathStepName));
                    success = false;
                    continue;
                }

                if (userStep.ReturnPaths.ContainsKey(pathName))
                {
                    errors.Add(string.Format("Step {0} contains multiple '{1}' return paths. Return path names must be unique.", step.Name, pathName));
                    success = false;
                    continue;
                }

                userStep.AddReturnPath(pathName, returnStep);
            }

            return success;
        }
    }
}
