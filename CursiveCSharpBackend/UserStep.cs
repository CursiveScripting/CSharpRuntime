using System;
using System.Collections.Generic;

namespace Cursive
{
    internal class UserStep : ReturningStep
    {
        public UserStep(string name, Process process)
            : base(name)
        {
            ChildProcess = process;
        }

        internal Process ChildProcess { get; set; }
        
        internal Dictionary<string, Step> ReturnPaths { get; } = new Dictionary<string, Step>();

        public void AddReturnPath(string name, Step nextStep)
        {
            ReturnPaths.Add(name, nextStep);
        }

        public override Step Run(ValueSet variables)
        {
            // set up fixed input parameters
            ValueSet inputs = FixedInputs.Clone(), outputs;

            // map any other input parameters in from variables
            foreach (var kvp in InputMapping)
                inputs[kvp.Key] = variables[kvp.Value];
            
            // actually run the process, with the inputs named as it expects
            var returnPath = ChildProcess.Run(inputs, out outputs);

            // map any output parameters back out into variables
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