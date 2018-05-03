using Cursive.Debugging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private bool NoReturnPaths = true;
        public void AddReturnPath(string name, Step nextStep)
        {
            ReturnPaths.Add(name, nextStep);
            NoReturnPaths = false;
        }

        public override async Task<Step> Run(ValueSet variables, CallStack stack)
        {
            // set up fixed input parameters
            ValueSet inputs = FixedInputs.Clone();

            // map any other input parameters in from variables
            foreach (var kvp in InputMapping)
                inputs[kvp.Key] = variables[kvp.Value];
            
            // actually run the process, with the inputs named as it expects
            var response = await ChildProcess.Run(inputs, stack);
            string returnPath = response.ReturnPath;
            ValueSet outputs = response.Outputs;

            // map any output parameters back out into variables
            if (outputs != null)
                foreach (var kvp in OutputMapping)
                    variables[kvp.Value] = outputs[kvp.Key];

            if (returnPath == null)
            {
                if (NoReturnPaths)
                    return DefaultReturnPath;

                if (ChildProcess is SystemProcess)
                    throw new CursiveRunException(stack, $"System process {(ChildProcess as SystemProcess).Name} unexpectedly returned a null value");
                else
                    throw new CursiveRunException(stack, $"Step {Name} unexpectedly returned a null value");
            }

            Step nextStep;
            if (!ReturnPaths.TryGetValue(returnPath, out nextStep))
            {
                if (ChildProcess is SystemProcess)
                    throw new CursiveRunException(stack, $"System process {(ChildProcess as SystemProcess).Name} returned an unexpected value: {returnPath}");
                else
                    throw new CursiveRunException(stack, $"Step {Name} returned an unexpected value: {returnPath}");
            }

            return nextStep;
        }
    }
}