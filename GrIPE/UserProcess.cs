using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class UserProcess : Process
    {
        private Step FirstStep;

        public UserProcess(Step firstStep)
        {
            FirstStep = firstStep;
        }
        
        public override string Run(Model inputs, out Model outputs)
        {
            var currentStep = FirstStep.Run(inputs);
            var lastStep = FirstStep;

            while (currentStep != null)
            {
                lastStep = currentStep;
                currentStep = currentStep.Run(inputs);
            }

            if (lastStep is EndStep)
                return (lastStep as EndStep).GetOutputs(out outputs);

            throw new InvalidOperationException("The last step of a completed process wasn't an EndStep");
        }

        public override ReadOnlyCollection<string> GetReturnPaths()
        {
            throw new NotImplementedException("Need to work out how to calculate this... probably have to loop through all possible EndSteps.");
        }

        public override ReadOnlyCollection<string> ListInputs()
        {
            throw new NotImplementedException();
        }

        public override ReadOnlyCollection<string> ListOutputs()
        {
            throw new NotImplementedException();
        }
    }
}