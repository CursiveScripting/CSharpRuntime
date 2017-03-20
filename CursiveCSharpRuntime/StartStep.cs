namespace Cursive
{
    internal class StartStep : ReturningStep
    {
        public StartStep(string name)
            : base(name) { }
        
        private ValueSet inputs;

        public void SetInputs(ValueSet inputs)
        {
            this.inputs = inputs;
        }

        public override Step Run(ValueSet variables)
        {
            foreach (var kvp in OutputMapping)
                variables[kvp.Value] = inputs[kvp.Key];

            return DefaultReturnPath;
        }
    }
}