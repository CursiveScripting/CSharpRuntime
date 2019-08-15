using Cursive.Debugging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    internal class UserProcess : Process
    {
        public UserProcess(string name, string description, IReadOnlyCollection<ValueKey> inputs, IReadOnlyCollection<ValueKey> outputs, IReadOnlyCollection<string> returnPaths, ValueSet defaultVariables)
            : base(name, description, null)
        {
            Inputs = inputs;
            Outputs = outputs;
            ReturnPaths = returnPaths;
            DefaultVariables = defaultVariables;
        }

        public override IReadOnlyCollection<ValueKey> Inputs { get; }
        public override IReadOnlyCollection<ValueKey> Outputs { get; }
        public override IReadOnlyCollection<string> ReturnPaths { get; }

        [JsonProperty(PropertyName = "variables")]
        private ValueSet DefaultVariables { get; }

        public StartStep FirstStep { get; internal set; }

        [JsonProperty(PropertyName = "steps")]
        public List<Step> Steps { get; } = new List<Step>();

        internal override async Task<Response> Run(ValueSet inputs, CallStack stack)
        {
            ValueSet variables = DefaultVariables.Clone();

            FirstStep.SetInputs(inputs);
            Step currentStep = FirstStep, lastStep = null;

            while (currentStep != null)
            {
                lastStep = currentStep;
                await stack.EnterStep(this, currentStep, variables);
                currentStep = await currentStep.Run(variables, stack);
                stack.ExitStep();
            }

            if (lastStep is StopStep)
            {
                var end = lastStep as StopStep;
                return new Response(end.ReturnValue, end.GetOutputs());
            }

            throw new CursiveRunException(stack, "The last step of a completed process wasn't a StopStep");
        }

        public IEnumerable<StopStep> StopSteps
        {
            get
            {
                foreach (var step in Steps)
                    if (step is StopStep)
                        yield return step as StopStep;
            }
        }

        public IEnumerable<UserStep> UserSteps
        {
            get
            {
                foreach (var step in Steps)
                    if (step is UserStep)
                        yield return step as UserStep;
            }
        }
    }
}