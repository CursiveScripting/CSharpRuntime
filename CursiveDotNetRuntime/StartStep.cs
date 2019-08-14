using Cursive.Debugging;
using System.Threading.Tasks;

namespace Cursive
{
    internal class StartStep : ReturningStep
    {
        public StartStep(string id)
            : base(id) { }
        
        private ValueSet inputs;

        public void SetInputs(ValueSet inputs)
        {
            this.inputs = inputs;
        }

        public override Task<Step> Run(ValueSet variables, CallStack stack)
        {
            foreach (var kvp in OutputMapping)
                variables[kvp.Value] = inputs[kvp.Key];

            return Task.FromResult(DefaultReturnPath);
        }
    }
}