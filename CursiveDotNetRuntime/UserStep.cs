using Newtonsoft.Json;
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

        internal override StepType StepType => StepType.Process;

        public Process ChildProcess { get; set; }

        public Dictionary<string, Step> ReturnPaths { get; } = new Dictionary<string, Step>();

        public async Task<Step> Run(CallStack stack)
        {
            var variables = stack.CurrentVariables.Values;

            // map input parameters in from variables
            var childInputs = new ValueSet(InputMapping.ToDictionary(m => m.Key, m => variables[m.Value.Name]));

            // actually run the process, with the inputs named as it expects
            var response = await ChildProcess.Run(childInputs, stack);
            string returnPath = response.ReturnPath;
            var childOutputs = response.Outputs;

            // map any output parameters back out into variables
            if (childOutputs != null)
                foreach (var kvp in OutputMapping)
                    variables[kvp.Value.Name] = childOutputs.Values[kvp.Key];

            if (returnPath == null)
            {
                if (!ReturnPaths.Any())
                    return DefaultReturnPath;

                if (ChildProcess is SystemProcess)
                    throw new CursiveRunException(stack, $"System process {(ChildProcess as SystemProcess).Name} unexpectedly returned a null value");
                else
                    throw new CursiveRunException(stack, $"Step {ID} unexpectedly returned a null value");
            }

            if (!ReturnPaths.TryGetValue(returnPath, out Step nextStep))
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