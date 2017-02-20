using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace Cursive
{
    public class Workspace
    {
        private Dictionary<string, DataType> typesByName = new Dictionary<string, DataType>();
        private Dictionary<string, DataType> typesByType = new Dictionary<string, DataType>();
        private Dictionary<string, Process> processes = new Dictionary<string, Process>();
        private Dictionary<string, RequiredProcess> requiredProcesses = new Dictionary<string, RequiredProcess>();

        public void AddDataType(DataType dt)
        {
            typesByName.Add(dt.Name, dt);
            typesByType.Add(dt.SystemType.FullName, dt);
        }

        public void AddSystemProcess(string name, SystemProcess process)
        {
            processes.Add(name, process);
        }

        public void AddRequiredProcess(string name, RequiredProcess process)
        {
            requiredProcesses.Add(name, process);
        }

        internal DataType GetType(string name)
        {
            DataType dt;
            if (!typesByName.TryGetValue(name, out dt))
                return null;
            return dt;
        }

        internal DataType GetType(Type type)
        {
            DataType dt;
            if (!typesByType.TryGetValue(type.FullName, out dt))
                return null;
            return dt;
        }

        public void Clear()
        {
            foreach (var process in requiredProcesses.Values)
                process.ActualProcess = null;

            var toRemove = new List<string>();
            foreach (var kvp in processes)
                if (kvp.Value is UserProcess)
                    toRemove.Add(kvp.Key);

            foreach (var key in toRemove)
                processes.Remove(key);
        }

        public bool LoadProcesses(XmlDocument doc, out List<string> errors)
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
                var process = LoadUserProcess(processNode, out userSteps, errors);
                if (processes == null)
                {
                    success = false;
                    continue;
                }

                loadedProcesses.Add(process);
                allUserSteps.AddRange(userSteps);
            }

            foreach (var step in allUserSteps)
            {
                if (!LinkChildProcess(step.Item1, step.Item2, errors))
                    success = false;
            }
            
            foreach (var process in loadedProcesses)
            {
                List<string> processErrors;
                if (!process.Validate(this, out processErrors))
                {
                    success = false;
                    foreach (var error in processErrors)
                        errors.Add(string.Format("{0}: {1}", process.Name, error));
                }
            }

            foreach (var required in requiredProcesses)
            {
                UserProcess actual;
                Process lookup;
                if (!processes.TryGetValue(required.Key, out lookup))
                    actual = null;
                else
                    actual = lookup as UserProcess;

                if (actual == null)
                {
                    success = false;
                    errors.Add(string.Format("{0}: No implementation of this required process", required.Key));
                    continue;
                }

                if (!Process.SignaturesMatch(required.Value, actual))
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

        private UserProcess LoadUserProcess(XmlElement processNode, out List<Tuple<UserStep, XmlElement>> stepsAndNodes, List<string> errors)
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
                process.AddInput(this, input.GetAttribute("name"), input.GetAttribute("type"));
            foreach (XmlElement output in outputs)
                process.AddOutput(this, output.GetAttribute("name"), output.GetAttribute("type"));
            foreach (XmlElement variable in variables)
                process.AddVariable(this, variable.GetAttribute("name"), variable.GetAttribute("type"), variable.GetAttribute("initialValue"));

            this.processes.Add(name, process);
            return process;
        }

        private bool LinkChildProcess(UserStep step, XmlElement stepNode, List<string> errors)
        {
            var processName = stepNode.GetAttribute("process");
            Process process;
            if (!processes.TryGetValue(processName, out process))
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

                var type = GetType(processInput.Type);

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

        private Step LoadProcessStep(XmlElement stepNode)
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

        private bool LoadStepLinks(UserStep step, XmlElement stepNode, Dictionary<string, Step> stepsByName, List<string> errors)
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

                if (step.returnPaths.ContainsKey(pathName))
                {
                    errors.Add(string.Format("Step '{0}' contains multiple '{1}' return paths. Return path names must be unique.", step.Name, pathName));
                    success = false;
                    continue;
                }
                
                step.AddReturnPath(pathName, returnStep);
            }

            return success;
        }

        public XmlDocument WriteForClient()
        {
            var doc = new XmlDocument();

            var root = doc.CreateElement("Workspace");
            doc.AppendChild(root);

            foreach (var type in typesByName)
            {
                var node = doc.CreateElement("Type");
                node.Attributes.Append(doc.CreateAttribute("name", type.Key));

                if (type.Value.Validation != null)
                {
                    var regex = type.Value.Validation.ToString();
                    node.Attributes.Append(doc.CreateAttribute("validation", regex));
                }

                root.AppendChild(node);
            }

            foreach (var kvp in processes)
            {
                var process = kvp.Value;
                if (!(process is SystemProcess))
                    continue;

                var processNode = doc.CreateElement("SystemProcess");
                processNode.Attributes.Append(doc.CreateAttribute("name", kvp.Key));
                root.AppendChild(processNode);

                foreach (var input in process.Inputs)
                {
                    var inputNode = doc.CreateElement("Input");
                    inputNode.Attributes.Append(doc.CreateAttribute("name", input.Name));

                    var type = GetType(input.Type);
                    inputNode.Attributes.Append(doc.CreateAttribute("type", type.Name));
                    processNode.AppendChild(inputNode);
                }

                foreach (var output in process.Outputs)
                {
                    var outputNode = doc.CreateElement("Output");
                    outputNode.Attributes.Append(doc.CreateAttribute("name", output.Name));

                    var type = GetType(output.Type);
                    outputNode.Attributes.Append(doc.CreateAttribute("type", type.Name));
                    processNode.AppendChild(outputNode);
                }

                if (process.ReturnPaths.Count == 0)
                    continue;

                var returnPathsNode = doc.CreateElement("ReturnPaths");
                processNode.AppendChild(returnPathsNode);

                foreach (var path in process.ReturnPaths)
                {
                    var pathNode = doc.CreateElement("Path");
                    pathNode.Attributes.Append(doc.CreateAttribute("name", path));
                    processNode.AppendChild(pathNode);
                }
            }

            return doc;
        }
    }
}
