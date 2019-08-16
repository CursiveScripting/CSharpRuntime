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

        internal override StepType StepType => StepType.Stop;

        public string ReturnValue { get; }

        public Task<ValueSet> Run(CallStack stack)
        {
            var variables = stack.CurrentVariables.Values;

            var outputs = new ValueSet(InputMapping.ToDictionary(m => m.Key, m => variables[m.Value.Name]));

            return Task.FromResult<ValueSet>(outputs);
        }
    }
}