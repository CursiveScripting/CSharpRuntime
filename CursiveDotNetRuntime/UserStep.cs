using Cursive.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cursive
{
    internal class UserStep : ReturningStep
    {
        public UserStep(string id, Process process)
            : base(id)
        {
            ChildProcess = process;
        }

        public Process ChildProcess { get; set; }
        
        public Dictionary<string, Step> ReturnPaths { get; } = new Dictionary<string, Step>();

        public override async Task<Step> Run(ValueSet variables, CallStack stack)
        {
            // map input parameters in from variables
            ValueSet inputs = new ValueSet();
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
                if (!ReturnPaths.Any())
                    return DefaultReturnPath;

                if (ChildProcess is SystemProcess)
                    throw new CursiveRunException(stack, $"System process {(ChildProcess as SystemProcess).Name} unexpectedly returned a null value");
                else
                    throw new CursiveRunException(stack, $"Step {ID} unexpectedly returned a null value");
            }

            Step nextStep;
            if (!ReturnPaths.TryGetValue(returnPath, out nextStep))
            {
                if (ChildProcess is SystemProcess)
                    throw new CursiveRunException(stack, $"System process {(ChildProcess as SystemProcess).Name} returned an unexpected value: {returnPath}");
                else
                    throw new CursiveRunException(stack, $"Step {ID} returned an unexpected value: {returnPath}");
            }

            return nextStep;
        }
    }
}