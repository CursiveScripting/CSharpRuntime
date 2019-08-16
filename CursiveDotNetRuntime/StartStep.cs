using System.Threading.Tasks;

namespace Cursive
{
    internal class StartStep : ReturningStep
    {
        public StartStep(string id)
            : base(id) { }

        internal override StepType StepType => StepType.Start;

        public Task<Step> Run(CallStack stack, ValueSet inputs)
        {
            var variables = stack.CurrentVariables.Values;

            foreach (var kvp in OutputMapping)
                variables[kvp.Value.Name] = inputs.Values[kvp.Key];

            return Task.FromResult(DefaultReturnPath);
        }
    }
}