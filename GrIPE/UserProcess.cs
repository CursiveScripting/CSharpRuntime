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
            {
                var end = lastStep as EndStep;
                outputs = end.GetOutputs();
                return end.ReturnPath;
            }

            throw new InvalidOperationException("The last step of a completed process wasn't an EndStep");
        }

        public override ReadOnlyCollection<string> ReturnPaths
        {
            get
            {
                List<string> paths = new List<string>();
                foreach (var endStep in GetAllEndSteps())
                    paths.Add(endStep.ReturnPath);

                return paths.AsReadOnly();
            }
        }

        public bool Validate(out string error)
        {
            // what validation rules should we have?

            // 1. every mapped parameter mapped into a step should first have been set by every path that could lead to that point

            // 2. every step with multiple return paths must either have EVERY path mapped, or have a default return path mapped.

            // 3. every output must be set by each end step

            error = null;
            return true;
        }

        internal IEnumerable<EndStep> GetAllEndSteps()
        {
            // should all steps not be stored directly by this process?
            throw new NotImplementedException();
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