using System.Threading.Tasks;

namespace Cursive
{
    internal class StopStep : Step
    {
        public StopStep(string name, string returnValue = null)
            : base(name)
        {
            ReturnValue = returnValue;
        }

        internal string ReturnValue { get; private set; }
        private ValueSet outputs;

        public ValueSet GetOutputs()
        {
            ValueSet outputs = this.outputs;
            this.outputs = null;
            return outputs;
        }

        public override Task<Step> Run(ValueSet variables, CallStack stack)
        {
            outputs = new ValueSet();
            foreach (var kvp in InputMapping)
                outputs[kvp.Key] = variables[kvp.Value];
            foreach (var kvp in FixedInputs)
                outputs[kvp.Key] = kvp.Value;
            return Task.FromResult<Step>(null);
        }
    }
}