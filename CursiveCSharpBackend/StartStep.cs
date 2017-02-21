namespace Cursive
{
    class StartStep : Step
    {
        public StartStep(string name)
            : base(name)
        {
        }
        
        private ValueSet inputs;

        public ValueSet GetInputs()
        {
            ValueSet inputs = this.inputs;
            this.inputs = null;
            return inputs;
        }

        public override Step Run(ValueSet variables)
        {
            inputs = new ValueSet();
            foreach (var kvp in InputMapping)
                variables[kvp.Value] = inputs[kvp.Key];

            return null;
        }
    }
}