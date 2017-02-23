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
            var allUserSteps = new List<Tuple<UserStep, XmlElement>>();
            foreach (XmlElement processNode in processNodes)
            {
                List<Tuple<UserStep, XmlElement>> userSteps;
                var process = LoadUserProcess(workspace, processNode, out userSteps, errors);
                if (workspace.Processes == null)
                {
                    success = false;
                    continue;
                }

                loadedProcesses.Add(process);
                allUserSteps.AddRange(userSteps);
            }

            foreach (var step in allUserSteps)
            {
                if (!LinkChildProcess(workspace, step.Item1, step.Item2, errors))
                    success = false;
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

            foreach (var required in workspace.RequiredProcesses)
            {
                UserProcess actual;
                Process lookup;
                if (!workspace.Processes.TryGetValue(required.Key, out lookup))
                    actual = null;
                else
                    actual = lookup as UserProcess;

                if (actual == null)
                {
                    success = false;
                    errors.Add(string.Format("{0}: No implementation of this required process", required.Key));
                    continue;
                }

                if (!ProcessService.SignaturesMatch(required.Value, actual))
                {
                    success = false;
                    errors.Add(string.Format("{0}: Signature of this required process doesn't match the requirements", required.Key));
                    continue;
                }

                required.Value.ActualProcess = actual;
            }

            return success;
        }

        private static List<string> validationErrors = null;
        private static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            validationErrors.Add(string.Format("schema validation error: {0}", e.Message));
        }
        
        private static UserProcess LoadUserProcess(Workspace workspace, XmlElement processNode, out List<Tuple<UserStep, XmlElement>> stepsAndNodes, List<string> errors)
        {
            var processName = processNode.GetAttribute("name");
            var desc = processNode.SelectSingleNode("Description").InnerText;

            var inputNodes = processNode.SelectNodes("Input");
            var outputNodes = processNode.SelectNodes("Output");
            var variableNodes = processNode.SelectNodes("Variable");

            List<Parameter> inputs = new List<Parameter>(), outputs = new List<Parameter>();
            ValueSet defaultVariables = new ValueSet();
            Dictionary<string, Parameter> inputsByName = new Dictionary<string, Parameter>(), outputsByName = new Dictionary<string, Parameter>(), variablesByName = new Dictionary<string, Parameter>();

            foreach (XmlElement input in inputNodes)
            {
                var type = workspace.GetType(input.GetAttribute("type"));
                var name = input.GetAttribute("name");
                var param = new Parameter(name, type);
                inputs.Add(param);
                inputsByName.Add(name, param);
            }
            foreach (XmlElement output in outputNodes)
            {
                var type = workspace.GetType(output.GetAttribute("type"));
                var name = output.GetAttribute("name");
                var param = new Parameter(name, type);
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

            XmlElement steps = processNode.SelectSingleNode("Steps") as XmlElement;
            var stepsByName = new Dictionary<string, Step>();
            stepsAndNodes = new List<Tuple<UserStep, XmlElement>>();

            StartStep firstStep = null; XmlElement firstStepNode = null;
            foreach (XmlElement stepNode in steps.ChildNodes)
            {
                var step = LoadProcessStep(stepNode, inputsByName, outputsByName, variablesByName, errors);
                if (step is StartStep)
                {
                    firstStep = step as StartStep;
                    firstStepNode = stepNode;
                }
                else
                    stepsByName.Add(step.Name, step);
                
                if (step is UserStep)
                    stepsAndNodes.Add(new Tuple<UserStep, XmlElement>(step as UserStep, stepNode));
            }

            if (firstStep == null)
            {
                errors.Add(string.Format("Process '{0}' lacks a Start step", processName));
                return null;
            }

            if (!LoadReturnPaths(firstStep, firstStepNode, stepsByName, errors))
                return null;

            foreach (var step in stepsAndNodes)
                if (!LoadReturnPaths(step.Item1, step.Item2, stepsByName, errors))
                    return null;
            
            UserProcess process = new UserProcess(processName, desc, inputs, outputs, defaultVariables, firstStep, stepsByName.Values);

            workspace.Processes.Add(processName, process);
            return process;
        }

        private static bool LinkChildProcess(Workspace workspace, UserStep step, XmlElement stepNode, List<string> errors)
        {
            var processName = stepNode.GetAttribute("process");
            Process process;
            if (!workspace.Processes.TryGetValue(processName, out process))
            {
                errors.Add(string.Format("Child process name not recognised for '{0}' step: {1}", step.Name, processName));
                return false;
            }

            step.ChildProcess = process;

            var fixedInputs = stepNode.SelectNodes("FixedInput");
            foreach (XmlElement inputNode in fixedInputs)
            {
                var name = inputNode.GetAttribute("name");
                var processInput = process.Inputs.FirstOrDefault(i => i.Name == name);
                if (processInput == null)
                {
                    errors.Add(string.Format("Input name not recognised for '{0}' step calling '{1}' process: {2}", step.Name, processName, name));
                    return false;
                }

                var type = processInput.Type;

                if (!(type is FixedType))
                {
                    errors.Add(string.Format("Only a FixedType can be used as a fixed input parameter. '{0}' type used for '{1}' step's '{2}' parameter is not a FixedType.", type.Name, step.Name, name));
                    return false;
                }
                
                var strValue = inputNode.GetAttribute("value");
                object value = (type as FixedType).Deserialize(strValue);
                step.SetInputParameter(processInput, value);
            }

            return true;
        }

        private static Step LoadProcessStep(XmlElement stepNode, Dictionary<string, Parameter> inputs, Dictionary<string, Parameter> outputs, Dictionary<string, Parameter> variables, List<string> errors)
        {
            var name = stepNode.GetAttribute("ID");
            var mapInputs = stepNode.SelectNodes("MapInput");
            var mapOutputs = stepNode.SelectNodes("MapOutput");

            // TODO: check that referenced inputs / outputs / parameters exist, and log an error if not

            if (stepNode.Name == "Start")
            {
                var step = new StartStep(name);
                foreach (XmlElement output in mapOutputs)
                {
                    var paramName = output.GetAttribute("name");
                    var variableName = output.GetAttribute("destination");
                    step.MapOutputParameter(inputs[paramName], variables[variableName]);
                }
                return step;
            }
            else if (stepNode.Name == "Stop")
            {
                var returnValue = stepNode.GetAttribute("name");
                if (returnValue == string.Empty)
                    returnValue = null;

                var step = new StopStep(name, returnValue);
                foreach (XmlElement input in mapInputs)
                {
                    var paramName = input.GetAttribute("name");
                    var variableName = input.GetAttribute("source");
                    step.MapInputParameter(outputs[paramName], variables[variableName]);
                }
                return step;
            }
            else
            {
                var step = new UserStep(name, null);

                foreach (XmlElement input in mapInputs)
                {
                    var paramName = input.GetAttribute("name");
                    var variableName = input.GetAttribute("source");
                    step.MapInputParameter(param, variables[variableName]); // TODO: ah crap, might not yet have loaded the child process?
                }
                foreach (XmlElement output in mapOutputs)
                {
                    var paramName = output.GetAttribute("name");
                    var variableName = output.GetAttribute("destination");
                    step.MapOutputParameter(param, variables[variableName]); // TODO: ah crap, might not yet have loaded the child process?
                }

                return step;
            }
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
