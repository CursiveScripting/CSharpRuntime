using Cursive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace CursiveCSharpBackend.Services
{
    internal static class ProcessLoadingService
    {
        internal static bool LoadProcesses(Workspace workspace, XmlDocument doc, out List<string> errors)
        {
            var success = true;
            errors = new List<string>();

            validationErrors = errors;
            doc.Schemas.Add("http://cursive.ftwinston.com", "processes.xsd");
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

            var inputs = new List<Parameter>();
            var outputs = new List<Parameter>();
            var defaultVariables = new ValueSet();
            var inputsByName = new Dictionary<string, Parameter>();
            var outputsByName = new Dictionary<string, Parameter>();
            var variablesByName = new Dictionary<string, Parameter>();

            RequiredProcess wrapper;
            if (!workspace.RequiredProcesses.TryGetValue(processName, out wrapper))
                wrapper = null;

            bool success = true;

            foreach (XmlElement input in inputNodes)
            {
                var type = workspace.GetType(input.GetAttribute("type"));
                var name = input.GetAttribute("name");
                Parameter param;

                if (wrapper != null)
                {
                    param = wrapper.Inputs.FirstOrDefault(p => p.Name == name);
                    if (param == null)
                    {
                        errors.Add(string.Format("Process '{0}' is required by the workspace, but has an input not defined in the workspace: '{1}'", processName, name));
                        success = false;
                        continue;
                    }
                    else if (param.Type != type)
                    {
                        errors.Add(string.Format("Process '{0}' is required by the workspace, but its '{1}' input is of type '{2}', where the workspace expects type '{3}'", processName, name, type.Name, param.Type.Name));
                        success = false;
                        continue;
                    }
                }
                else
                    param = new Parameter(name, type);

                inputs.Add(param);
                inputsByName.Add(name, param);
            }
            foreach (XmlElement output in outputNodes)
            {
                var type = workspace.GetType(output.GetAttribute("type"));
                var name = output.GetAttribute("name");
                Parameter param;

                if (wrapper != null)
                {
                    param = wrapper.Outputs.FirstOrDefault(p => p.Name == name);
                    if (param == null)
                    {
                        errors.Add(string.Format("Process '{0}' is required by the workspace, but has an output not defined in the workspace: '{1}'", processName, name));
                        success = false;
                        continue;
                    }
                    else if (param.Type != type)
                    {
                        errors.Add(string.Format("Process '{0}' is required by the workspace, but its '{1}' output is of type '{2}', where the workspace expects type '{3}'", processName, name, type.Name, param.Type.Name));
                        success = false;
                        continue;
                    }
                }
                else
                    param = new Parameter(name, type);

                outputs.Add(param);
                outputsByName.Add(name, param);
            }
            foreach (XmlElement variable in variableNodes)
            {
                var type = workspace.GetType(variable.GetAttribute("type"));
                var name = variable.GetAttribute("name");
                var definition = new Parameter(name, type);
                variablesByName.Add(name, definition);

                var initialValue = variable.GetAttribute("initialValue");

                if (initialValue != null && definition.Type is FixedType)
                    defaultVariables[definition] = (definition.Type as FixedType).Deserialize(initialValue);
                else
                    defaultVariables[definition] = definition.Type.GetDefaultValue();
            }

            if (wrapper != null)
            {
                foreach (var input in wrapper.Inputs)
                    if (!inputsByName.ContainsKey(input.Name))
                    {
                        errors.Add(string.Format("Process '{0}' is required by the workspace, but is missing the required input '{1}'", processName, input.Name));
                        success = false;
                    }
                foreach (var output in wrapper.Outputs)
                    if (!outputsByName.ContainsKey(output.Name))
                    {
                        errors.Add(string.Format("Process '{0}' is required by the workspace, but is missing the required output '{1}'", processName, output.Name));
                        success = false;
                    }
            }

            XmlElement steps = processNode.SelectSingleNode("Steps") as XmlElement;
            var stepsByName = new Dictionary<string, Step>();
            loadingSteps = new List<UserStepLoadingInfo>();

            StartStep firstStep = null; XmlElement firstStepNode = null;
            foreach (XmlElement stepNode in steps.ChildNodes)
            {
                var step = LoadProcessStep(stepNode, inputsByName, outputsByName, variablesByName, loadingSteps, errors);
                if (step == null)
                {
                    success = false;
                    continue;
                }

                if (step is StartStep)
                {
                    firstStep = step as StartStep;
                    firstStepNode = stepNode;
                }
                else
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

            UserProcess process = new UserProcess(processName, desc, inputs, outputs, defaultVariables, firstStep, stepsByName.Values);
            workspace.Processes.Add(processName, process);

            if (wrapper != null)
                wrapper.ActualProcess = process;

            return process;
        }

        private static bool LinkChildProcess(Dictionary<string, Process> processes, UserStepLoadingInfo stepInfo, List<string> errors)
        {
            var step = stepInfo.Step;
            var processName = stepInfo.Element.GetAttribute("process");
            Process process;
            if (!processes.TryGetValue(processName, out process))
            {
                errors.Add(string.Format("Child process name not recognised for '{0}' step: {1}", step.Name, processName));
                return false;
            }

            step.ChildProcess = process;
            bool success = true;

            var fixedInputs = stepInfo.Element.SelectNodes("FixedInput");
            foreach (XmlElement inputNode in fixedInputs)
            {
                var name = inputNode.GetAttribute("name");
                Parameter input = process.Inputs.FirstOrDefault(p => p.Name == name);
                if (input == null)
                {
                    errors.Add(string.Format("Input name not recognised for '{0}' step calling '{1}' process: {2}", step.Name, processName, name));
                    success = false;
                    continue;
                }
                
                if (!(input.Type is FixedType))
                {
                    errors.Add(string.Format("Only a FixedType can be used as a fixed input parameter. '{0}' type used for '{1}' step's '{2}' parameter is not a FixedType.", input.Type.Name, step.Name, name));
                    success = false;
                    continue;
                }
                
                var strValue = inputNode.GetAttribute("value");
                object value = (input.Type as FixedType).Deserialize(strValue);
                step.SetInputParameter(input, value);
            }

            foreach (var inputInfo in stepInfo.InputsToMap)
            {
                Parameter input = process.Inputs.FirstOrDefault(p => p.Name == inputInfo.Key);
                if (input == null)
                {
                    errors.Add(string.Format("The '{0}' step tries to map '{1}' variable to non-existant input '{2}'", step.Name, inputInfo.Value.Name, inputInfo.Key));
                    success = false;
                    continue;
                }

                if (input.Type != inputInfo.Value.Type)
                {
                    errors.Add(string.Format("The '{0}' step tries to map the '{1}' variable to the '{2}' input, but these have different types ('{3}' and '{4}')", step.Name, inputInfo.Value.Name, inputInfo.Key, inputInfo.Value.Type.Name, input.Type.Name));
                    success = false;
                    continue;
                }

                step.MapInputParameter(input, inputInfo.Value);
            }

            foreach (var outputInfo in stepInfo.OutputsToMap)
            {
                Parameter output = process.Outputs.FirstOrDefault(p => p.Name == outputInfo.Key);
                if (output == null)
                {
                    errors.Add(string.Format("The '{0}' step tries to map non-existant output '{2}' to '{1}' variable", step.Name, outputInfo.Value.Name, outputInfo.Key));
                    success = false;
                    continue;
                }

                if (output.Type != outputInfo.Value.Type)
                {
                    errors.Add(string.Format("The '{0}' step tries to map the '{2}' output to the '{1}' variable, but these have different types ('{3}' and '{4}')", step.Name, outputInfo.Value.Name, outputInfo.Key, outputInfo.Value.Type.Name, output.Type.Name));
                    success = false;
                    continue;
                }

                step.MapOutputParameter(output, outputInfo.Value);
            }

            return success;
        }

        private static Step LoadProcessStep(XmlElement stepNode, Dictionary<string, Parameter> inputs, Dictionary<string, Parameter> outputs, Dictionary<string, Parameter> variables, List<UserStepLoadingInfo> loadingSteps, List<string> errors)
        {
            var name = stepNode.GetAttribute("ID");
            var mapInputs = stepNode.SelectNodes("MapInput");
            var mapOutputs = stepNode.SelectNodes("MapOutput");
            bool success = true;
            Step step;

            if (stepNode.Name == "Start")
            {
                step = new StartStep(name);
                foreach (XmlElement output in mapOutputs)
                {
                    var paramName = output.GetAttribute("name");
                    var variableName = output.GetAttribute("destination");

                    Parameter inputParam, variableParam;
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

                    if (inputParam.Type != variableParam.Type)
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
                    returnValue = null;

                step = new StopStep(name, returnValue);
                foreach (XmlElement input in mapInputs)
                {
                    var paramName = input.GetAttribute("name");
                    var variableName = input.GetAttribute("source");

                    Parameter outputParam, variableParam;
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

                    if (outputParam.Type != variableParam.Type)
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

                foreach (XmlElement input in mapInputs)
                {
                    var paramName = input.GetAttribute("name");
                    var variableName = input.GetAttribute("source");

                    Parameter variableParam;
                    if (!variables.TryGetValue(variableName, out variableParam))
                    {
                        errors.Add(string.Format("Step '{0}' tries to map non-existant variable '{2}' to input parameter '{1}'", name, paramName, variableName));
                        success = false;
                        continue;
                    }

                    stepInfo.InputsToMap.Add(paramName, variableParam);
                }
                foreach (XmlElement output in mapOutputs)
                {
                    var paramName = output.GetAttribute("name");
                    var variableName = output.GetAttribute("destination");

                    Parameter variableParam;
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
                    errors.Add(string.Format("Return path target of '{0}' step not recognised: {1}", step.Name, pathStepName));
                    return false;
                }

                step.DefaultReturnPath = returnStep;
                return true;
            }

            if (step is StartStep)
            {
                errors.Add(string.Format("Start step '{0}' doesn't have a single, unnamed return path", step.Name));
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
                    errors.Add(string.Format("'{0}' return path target of '{1}' step not recognised: {2}", pathName, step.Name, pathStepName));
                    success = false;
                    continue;
                }

                if (userStep.ReturnPaths.ContainsKey(pathName))
                {
                    errors.Add(string.Format("Step '{0}' contains multiple '{1}' return paths. Return path names must be unique.", step.Name, pathName));
                    success = false;
                    continue;
                }

                userStep.AddReturnPath(pathName, returnStep);
            }

            return success;
        }
    }
}
