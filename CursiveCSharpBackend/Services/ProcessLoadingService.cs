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
            var name = processNode.GetAttribute("name");
            var desc = processNode.SelectSingleNode("Description").InnerText;

            XmlElement steps = processNode.SelectSingleNode("Steps") as XmlElement;
            var stepsByName = new Dictionary<string, Step>();
            stepsAndNodes = new List<Tuple<UserStep, XmlElement>>();

            string firstStepName = null;
            foreach (XmlElement stepNode in steps.ChildNodes)
            {
                var step = LoadProcessStep(stepNode);
                if (step is StartStep)
                {
                    var firstReturnPath = stepNode.SelectSingleNode("ReturnPath") as XmlElement;
                    firstStepName = firstReturnPath.GetAttribute("targetStepID");
                    continue;
                }

                stepsByName.Add(step.Name, step);

                if (step is UserStep)
                    stepsAndNodes.Add(new Tuple<UserStep, XmlElement>(step as UserStep, stepNode));
            }

            foreach (var step in stepsAndNodes)
            {
                if (!LoadStepLinks(step.Item1, step.Item2, stepsByName, errors))
                    return null;
            }

            Step firstStep;
            if (!stepsByName.TryGetValue(firstStepName, out firstStep))
            {
                errors.Add(string.Format("Return path target of Start step not recognised: {0}", firstStepName));
                return null;
            }

            UserProcess process = new UserProcess(name, desc, firstStep, stepsByName.Values);

            var inputs = processNode.SelectNodes("Input");
            var outputs = processNode.SelectNodes("Output");
            var variables = processNode.SelectNodes("Variable");

            foreach (XmlElement input in inputs)
                process.AddInput(workspace, input.GetAttribute("name"), input.GetAttribute("type"));
            foreach (XmlElement output in outputs)
                process.AddOutput(workspace, output.GetAttribute("name"), output.GetAttribute("type"));
            foreach (XmlElement variable in variables)
                process.AddVariable(workspace, variable.GetAttribute("name"), variable.GetAttribute("type"), variable.GetAttribute("initialValue"));

            workspace.Processes.Add(name, process);
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
                var strValue = inputNode.GetAttribute("value");
                var processInput = process.Inputs.FirstOrDefault(i => i.Name == name);
                if (processInput == null)
                {
                    errors.Add(string.Format("Input name not recognised for '{0}' step calling '{1}' process: {2}", step.Name, processName, name));
                    return false;
                }

                var type = workspace.GetType(processInput.Type);

                if (!(type is FixedType))
                {
                    errors.Add(string.Format("Only a FixedType can be used as a fixed input parameter. '{0}' type used for '{1}' step's '{2}' parameter is not a FixedType.", type.Name, step.Name, name));
                    return false;
                }

                object value = (type as FixedType).Deserialize(strValue);
                step.SetInputParameter(name, value);
            }

            return true;
        }

        private static Step LoadProcessStep(XmlElement stepNode)
        {
            var name = stepNode.GetAttribute("ID");
            var mapInputs = stepNode.SelectNodes("MapInput");
            var mapOutputs = stepNode.SelectNodes("MapOutput");

            if (stepNode.Name == "Start")
            {
                var step = new StartStep(name);
                // TODO: use start step's output parameters ... somehow
                foreach (XmlElement output in mapOutputs)
                    ;//step.MapOutputParameter(output.GetAttribute("name"), output.GetAttribute("destination"));
                return step;
            }
            else if (stepNode.Name == "Stop")
            {
                var returnValue = stepNode.GetAttribute("name");
                if (returnValue == string.Empty)
                    returnValue = null;

                var step = new StopStep(name, returnValue);
                foreach (XmlElement input in mapInputs)
                    step.MapInputParameter(input.GetAttribute("name"), input.GetAttribute("source"));
                return step;
            }
            else
            {
                var step = new UserStep(name, null);

                foreach (XmlElement input in mapInputs)
                    step.MapInputParameter(input.GetAttribute("name"), input.GetAttribute("source"));
                foreach (XmlElement output in mapOutputs)
                    step.MapOutputParameter(output.GetAttribute("name"), output.GetAttribute("destination"));

                return step;
            }
        }

        private static bool LoadStepLinks(UserStep step, XmlElement stepNode, Dictionary<string, Step> stepsByName, List<string> errors)
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

                step.SetDefaultReturnPath(returnStep);
                return true;
            }

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

                if (step.ReturnPaths.ContainsKey(pathName))
                {
                    errors.Add(string.Format("Step '{0}' contains multiple '{1}' return paths. Return path names must be unique.", step.Name, pathName));
                    success = false;
                    continue;
                }

                step.AddReturnPath(pathName, returnStep);
            }

            return success;
        }
    }
}
