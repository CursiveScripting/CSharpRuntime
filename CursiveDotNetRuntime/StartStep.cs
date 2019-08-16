using System.Threading.Tasks;

namespace Cursive
{
    internal class StartStep : ReturningStep
    {
        public StartStep(string id)
            : base(id) { }
        
        public ValueSet Inputs { get; set; } // TODO: do away with this, instead different Run method

        public override Task<Step> Run(CallStack stack)
        {
            var variables = stack.CurrentVariables.Values;

            foreach (var kvp in OutputMapping)
                variables[kvp.Value.Name] = Inputs.Values[kvp.Key];

            return Task.FromResult(DefaultReturnPath);
        }
    }
}