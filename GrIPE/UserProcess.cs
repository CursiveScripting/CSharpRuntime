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
        private Step firstStep;

        public UserProcess(string description, Step firstStep)
        {
            this.Description = description;
            this.firstStep = firstStep;
        }
        
        public override string Run(Model inputs, out Model outputs)
        {
            Model workspace = inputs.Clone();

            var currentStep = firstStep.Run(workspace);
            var lastStep = firstStep;

            while (currentStep != null)
            {
                lastStep = currentStep;
                currentStep = currentStep.Run(workspace);
            }

            if (lastStep is EndStep)
                return (lastStep as EndStep).GetOutputs(workspace, out outputs);

            throw new InvalidOperationException("The last step of a completed process wasn't an EndStep");
        }

        public override ReadOnlyCollection<string> ReturnPaths
        {
            get
            {
                throw new NotImplementedException("Need to work out how to calculate this... probably have to loop through all possible EndSteps.");
            }
        }
        
        private List<string> inputs = new List<string>();
        private List<string> outputs = new List<string>();

        public void AddInput(string name)
        {
            inputs.Add(name);
        }

        public void AddOutput(string name)
        {
            outputs.Add(name);
        }

        public override ReadOnlyCollection<string> Inputs
        {
            get { return inputs.AsReadOnly(); }
        }

        public override ReadOnlyCollection<string> Outputs
        {
            get { return outputs.AsReadOnly(); }
        }
    }
}