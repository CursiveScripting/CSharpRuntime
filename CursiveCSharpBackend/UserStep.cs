using System;
using System.Collections.Generic;

namespace Cursive
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

        protected internal Dictionary<string, string> OutputMapping { get; } = new Dictionary<string, string>();
        protected internal Dictionary<string, Step> ReturnPaths { get; } = new Dictionary<string, Step>();

        public void MapOutputParameter(string parameterName, string destinationName)
        {
            OutputMapping[parameterName] = destinationName;
        }
        
        public void AddReturnPath(string name, Step nextStep)
        {
            ReturnPaths.Add(name, nextStep);
        }

        public void SetDefaultReturnPath(Step nextStep)
        {
            DefaultReturnPath = nextStep;
        }

        public override Step Run(ValueSet variables)
        {
            // set up fixed input parameters
            ValueSet inputs = FixedInputs.Clone(), outputs;

            // map any other input parameters in from the workspace
            foreach (var kvp in InputMapping)
                inputs[kvp.Key] = variables[kvp.Value];
            
            // actually run the process, with the inputs named as it expects
            var returnPath = ChildProcess.Run(inputs, out outputs);

            // map any output parameters back out into the workspace
            if (outputs != null)
                foreach (var kvp in OutputMapping)
                    variables[kvp.Value] = outputs[kvp.Key];

            Step nextStep;
            if (returnPath == null || !ReturnPaths.TryGetValue(returnPath, out nextStep))
                return DefaultReturnPath;

            return nextStep;
        }
    }
}