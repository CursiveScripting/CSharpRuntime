﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GrIPE
{
    public class Workspace
    {
        private SortedList<string, DataType> typesByName = new SortedList<string, DataType>();
        private SortedList<string, DataType> typesByType = new SortedList<string, DataType>();
        private SortedList<string, Process> processes = new SortedList<string, Process>();

        public void AddDataType(DataType dt)
        {
            typesByName.Add(dt.Name, dt);
            typesByType.Add(dt.SystemType.FullName, dt);
        }

        public void AddSystemProcess(string name, Func<Workspace, SystemProcess> process)
        {
            processes.Add(name, process(this));
        }

        public void AddUserProcess(UserProcess process)
        {
            processes.Add(process.Name, process);
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

        public Process GetProcess(string name)
        {
            Process process;
            if (!processes.TryGetValue(name, out process))
                return null;
            return process;
        }

        public bool Validate(out List<string> errors)
        {
            bool valid = true;
            List<string> processErrors;

            errors = new List<string>();
            foreach (var process in processes.Values)
            {
                if (!(process is UserProcess))
                    continue;

                var userProcess = process as UserProcess;
                if (!userProcess.Validate(this, out processErrors))
                    foreach (var error in processErrors)
                        errors.Add(string.Format("{0}: {1}", userProcess.Name, error));
            }

            return valid;
        }

        public void Clear()
        {
            var toRemove = new List<string>();
            foreach (var kvp in processes)
                if (kvp.Value is UserProcess)
                    toRemove.Add(kvp.Key);

            foreach (var key in toRemove)
                processes.Remove(key);
        }

        public bool LoadUserProcesses(XmlDocument doc, out List<string> errors)
        {
            var success = true;
            errors = new List<string>();

            // TODO: check doc is valid (we need a schema!)

            var processes = doc.SelectNodes("/Processes/Process");
            var allUserSteps = new List<Tuple<UserStep, XmlElement>>();
            foreach (XmlElement process in processes)
            {
                List<Tuple<UserStep, XmlElement>> userSteps;
                if (!LoadUserProcess(process, out userSteps, errors))
                    success = false;

                allUserSteps.AddRange(userSteps);
            }

            foreach (var step in allUserSteps)
            {
                if (!LinkChildProcess(step.Item1, step.Item2, errors))
                    success = false;
            }
            return success;
        }

        private bool LoadUserProcess(XmlElement processNode, out List<Tuple<UserStep, XmlElement>> stepsAndNodes, List<string> errors)
        {
            var name = processNode.GetAttribute("name");
            var desc = processNode.SelectSingleNode("Description").InnerText;

            XmlElement steps = processNode.SelectSingleNode("Steps") as XmlElement;
            var stepsByName = new SortedList<string, Step>();
            stepsAndNodes = new List<Tuple<UserStep, XmlElement>>();

            foreach (XmlElement stepNode in steps.ChildNodes)
            {
                var step = LoadProcessStep(stepNode);
                stepsByName.Add(step.Name, step);
                
                if (step is UserStep)
                    stepsAndNodes.Add(new Tuple<UserStep, XmlElement>(step as UserStep, stepNode));
            }

            foreach (var step in stepsAndNodes)
            {
                if (!LoadStepLinks(step.Item1, step.Item2, stepsByName, errors))
                    return false;
            }

            var firstStepName = steps.GetAttribute("firstStep");
            Step firstStep;
            if (!stepsByName.TryGetValue(firstStepName, out firstStep))
            {
                errors.Add(string.Format("firstStep name not recognised for '{0}' process: {1}", name, firstStepName));
                return false;
            }

            UserProcess process = new UserProcess(name, desc, firstStep, stepsByName.Values);

            var inputs = processNode.SelectNodes("Input");
            var outputs = processNode.SelectNodes("Output");

            foreach (XmlElement input in inputs)
                process.AddInput(this, input.InnerText, input.GetAttribute("type"));
            foreach (XmlElement output in outputs)
                process.AddOutput(this, output.InnerText, output.GetAttribute("type"));

            this.processes.Add(name, process);
            return true;
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
            var name = stepNode.GetAttribute("name");
            var mapInputs = stepNode.SelectNodes("MapInput");

            if (stepNode.Name == "EndStep")
            {
                var returnValue = stepNode.GetAttribute("returnValue");

                var step = new EndStep(name, returnValue);
                foreach (XmlElement input in mapInputs)
                    step.MapInputParameter(input.GetAttribute("name"), input.GetAttribute("destination"));
                return step;
            }
            else
            {
                var mapOutputs = stepNode.SelectNodes("MapOutput");
                var step = new UserStep(name, null);

                foreach (XmlElement input in mapInputs)
                    step.MapInputParameter(input.GetAttribute("name"), input.GetAttribute("source"));
                foreach (XmlElement output in mapOutputs)
                    step.MapOutputParameter(output.GetAttribute("name"), output.GetAttribute("destination"));

                return step;
            }
        }

        private bool LoadStepLinks(UserStep step, XmlElement stepNode, SortedList<string, Step> stepsByName, List<string> errors)
        {
            var returnPaths = stepNode.SelectSingleNode("ReturnPaths") as XmlElement;

            var defaultReturnStepName = returnPaths.GetAttribute("default");
            Step returnStep;
            if (!stepsByName.TryGetValue(defaultReturnStepName, out returnStep))
            {
                errors.Add(string.Format("Default return path of '{0}' step not recognised: {1}", step.Name, defaultReturnStepName));
                return false;
            }
            step.SetDefaultReturnPath(returnStep);

            foreach (XmlElement path in returnPaths.ChildNodes)
            {
                var pathName = path.GetAttribute("name");
                var pathStepName = path.GetAttribute("value");

                if (!stepsByName.TryGetValue(pathStepName, out returnStep))
                {
                    errors.Add(string.Format("'{0}' return path of '{1}' step not recognised: {2}", pathName, step.Name, pathStepName));
                    return false;
                }

                step.AddReturnPath(pathName, returnStep);
            }

            return true;
        }
    }
}