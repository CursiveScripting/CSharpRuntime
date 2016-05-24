using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    class UserStep : Step
    {
        public UserStep(string name, Process process)
            : base(name)
        {
            ChildProcess = process;
        }

        protected internal Process ChildProcess { get; internal set; }
        protected internal Step DefaultReturnPath { get; private set; }

        protected internal SortedList<string, string> outputMapping = new SortedList<string, string>();
        protected internal SortedList<string, Step> returnPaths = new SortedList<string, Step>();

        public void MapOutputParameter(string parameterName, string destinationName)
        {
            outputMapping[parameterName] = destinationName;
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