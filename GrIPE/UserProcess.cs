using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class UserProcess<Model> : Process<Model>
    {
        private Step<Model> FirstStep;

        public UserProcess(Step<Model> firstStep)
        {
            FirstStep = firstStep;
        }

        public override string Run(Model model)
        {
            var currentStep = FirstStep.Run(model);
            var lastStep = FirstStep;

            while (currentStep != null)
            {
                lastStep = currentStep;
                currentStep = currentStep.Run(model);
            }

            if (lastStep is EndStep<Model>)
                return (lastStep as EndStep<Model>).Output;

            throw new InvalidOperationException("The last step of a completed process wasn't an EndStep");
        }

        public override string[] GetPossibleOutputs()
        {
            throw new NotImplementedException("Need to work out how to calculate this... probably have to loop through all possible EndSteps.");
        }
    }
}