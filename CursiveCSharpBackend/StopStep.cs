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
        private Model outputs;

        public Model GetOutputs()
        {
            Model outputs = this.outputs;
            this.outputs = null;
            return outputs;
        }

        public override Step Run(Model workspace)
        {
            outputs = new Model();
            foreach (var kvp in inputMapping)
                outputs[kvp.Key] = workspace[kvp.Value];
            
            return null;
        }
    }
}