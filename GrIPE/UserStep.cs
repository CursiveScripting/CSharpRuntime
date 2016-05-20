using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class UserStep : Step
    {
        public UserStep(Process childProcess)
        {
            this.ChildProcess = childProcess;
        }

        protected internal Process ChildProcess { get; private set; }
        protected internal Step DefaultReturnPath { get; private set; }

        protected internal SortedList<string, object> fixedInputs = new SortedList<string, object>();
        protected internal SortedList<string, string> inputMapping = new SortedList<string, string>();
        protected internal SortedList<string, Step> returnPaths = new SortedList<string, Step>();

        public void SetInputParameter(string parameterName, object value)
        {
            fixedInputs[parameterName] = value;
        }

        public void MapInputParameter(string parameterName, string sourceName)
        {
            inputMapping[parameterName] = sourceName;
        }
        
        public void AddReturnPath(string name, Step nextStep)
        {
            returnPaths.Add(name, nextStep);
        }

        public void SetDefaultReturnPath(Step nextStep)
        {
            DefaultReturnPath = nextStep;
        }

        public override Step Run(Model workspace)
        {
            Model inputs = new Model(), outputs;

            // set up fixed input parameters
            foreach (var kvp in fixedInputs)
                inputs[kvp.Key] = kvp.Value;

            // map any other input parameters in from the workspace
            foreach (var kvp in inputMapping)
                inputs[kvp.Key] = workspace[kvp.Value];
            
            // actually run the process, with the inputs named as it expects
            var outputName = ChildProcess.Run(inputs, out outputs);

            // map any output parameters back out into the workspace
            if (outputs != null)
                foreach (var kvp in outputMapping)
                    workspace[kvp.Value] = outputs[kvp.Key];

            Step output;
            if (outputName == null || !returnPaths.TryGetValue(outputName, out output))
                return DefaultReturnPath;

            return output;
        }
    }
}