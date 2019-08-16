using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cursive
{
    internal class UserProcess : Process
    {
        public UserProcess(
            string name,
            string description,
            IReadOnlyCollection<Parameter> inputs,
            IReadOnlyCollection<Parameter> outputs,
            IReadOnlyCollection<string> returnPaths,
            Dictionary<string, Variable> variables
        )
            : base(name, description, null, inputs, outputs, returnPaths)
        {
            Variables = variables;
        }

        [JsonProperty(PropertyName = "variables")]
        public Dictionary<string, Variable> Variables { get; }

        public StartStep FirstStep { get; internal set; }

        [JsonProperty(PropertyName = "steps")]
        public List<Step> Steps { get; } = new List<Step>();

        internal override async Task<Response> Run(ValueSet inputs, CallStack stack)
        {
            var variableValues = new ValueSet(Variables.ToDictionary(v => v.Key, v => v.Value.InitialValue));

            FirstStep.Inputs = inputs;
            Step currentStep = FirstStep, lastStep = null;

            while (currentStep != null)
            {
                lastStep = currentStep;
                await stack.EnterStep(this, currentStep, variableValues);
                currentStep = await currentStep.Run(stack);
                stack.ExitStep();
            }

            if (lastStep is StopStep end)
            {
                return new Response(end.ReturnValue, end.GetOutputs());
            }

            throw new CursiveRunException(stack, "The last step of a completed process wasn't a StopStep");
        }

        public IEnumerable<StopStep> StopSteps
        {
            get
            {
                foreach (var step in Steps)
                    if (step is StopStep end)
                        yield return end;
            }
        }

        public IEnumerable<UserStep> UserSteps
        {
            get
            {
                foreach (var step in Steps)
                    if (step is UserStep userStep)
                        yield return userStep;
            }
        }
    }
}