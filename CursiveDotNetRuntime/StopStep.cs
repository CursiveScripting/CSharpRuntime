using System.Linq;
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
        private ValueSet Outputs { get; set; }

        public ValueSet GetOutputs() // do away with this, instead different Run method
        {
            ValueSet outputs = Outputs;
            Outputs = null;
            return outputs;
        }

        public override Task<Step> Run(CallStack stack)
        {
            var variables = stack.CurrentVariables.Values;

            Outputs = new ValueSet(InputMapping.ToDictionary(m => m.Key, m => variables[m.Value.Name]));

            return Task.FromResult<Step>(null);
        }
    }
}