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
            IReadOnlyList<Parameter> inputs,
            IReadOnlyList<Parameter> outputs,
            IReadOnlyList<string> returnPaths,
            Dictionary<string, Variable> variables
        )
            : base(name, description, null, inputs, outputs, returnPaths)
        {
            Variables = variables;
        }

        public Dictionary<string, Variable> Variables { get; }

        public StartStep FirstStep { get; internal set; }

        internal override async Task<ProcessResult> Run(ValueSet inputs, CallStack stack)
        {
            var variableValues = new ValueSet(Variables.ToDictionary(v => v.Key, v => v.Value.InitialValue));

            Step currentStep = await RunStartStep(inputs, stack, variableValues);

            while (currentStep != null)
            {
                if (currentStep.StepType == StepType.Process)
                    currentStep = await RunStep(currentStep as UserStep, stack);

                else if (currentStep.StepType == StepType.Stop)
                    return await RunStopStep(currentStep as StopStep, stack);

                else
                    throw new CursiveRunException(stack, $"Ran into unexpected step {currentStep.ID} in process \"{Name}\"");
            }

            throw new CursiveRunException(stack, $"The last step of process \"{Name}\" wasn't a stop step");
        }

        private async Task<Step> RunStartStep(ValueSet inputs, CallStack stack, ValueSet variableValues)
        {
            var step = FirstStep;

            await stack.EnterNewProcess(this, step, variableValues);

            Step nextStep = await step.Run(stack, inputs);

            stack.ExitStep();

            return nextStep;
        }

        private async Task<Step> RunStep(UserStep step, CallStack stack)
        {
            await stack.EnterStep(this, step);

            var nextStep = await step.Run(stack);

            stack.ExitStep();

            return nextStep;
        }

        private async Task<ProcessResult> RunStopStep(StopStep step, CallStack stack)
        {
            await stack.EnterStep(this, step);

            var outputs = await step.Run(stack);

            stack.ExitProcess();

            return new ProcessResult(step.ReturnValue, outputs);
        }
    }
}