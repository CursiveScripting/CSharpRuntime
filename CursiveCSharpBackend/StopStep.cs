namespace Cursive
{
    class StopStep : Step
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

        public override Step Run(ValueSet variables)
        {
            outputs = new ValueSet();
            foreach (var kvp in InputMapping)
                outputs[kvp.Key] = variables[kvp.Value]; // TODO: actually, these should be mapped
            
            return null;
        }
    }
}