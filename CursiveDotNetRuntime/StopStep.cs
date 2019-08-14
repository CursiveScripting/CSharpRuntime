using Cursive.Debugging;
using System.Threading.Tasks;

namespace Cursive
{
    internal class StopStep : Step
    {
        public StopStep(string id, string returnValue = null)
            : base(id)
        {
            ReturnValue = returnValue;
        }

        public string ReturnValue { get; private set; }
        private ValueSet outputs;

        public ValueSet GetOutputs()
        {
            ValueSet outputs = this.outputs;
            this.outputs = null;
            return outputs;
        }

        public override Task<Step> Run(ValueSet variables, CallStack stack)
        {
            outputs = new ValueSet(stack);
            foreach (var kvp in InputMapping)
                outputs[kvp.Key] = variables[kvp.Value];
            return Task.FromResult<Step>(null);
        }
    }
}