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
        private Step[] allSteps;

        public UserProcess(string name, string description, Step firstStep, params Step[] allSteps)
            : base(name, description)
        {
            this.firstStep = firstStep;
            this.allSteps = allSteps;
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
                foreach (var endStep in EndSteps)
                    paths.Add(endStep.ReturnPath);

                return paths.AsReadOnly();
            }
        }

        public bool Validate(out List<string> errors)
        {
            var success = true;
            errors = new List<string>();

            // any input/output parameter being mapped into/out of a child process must be present in that child process
            // any child process must have all the input/output parameters mapped that it expects

            // 2. each end step must set every output.
            foreach (var step in EndSteps)
            {
                foreach (var output in outputs)
                    if (!step.outputMapping.ContainsKey(output))
                    {
                        errors.Add(string.Format("The '{0}' end step doesn't set the '{1}' output.", step.ReturnPath, output));
                        success = false;
                    }
            }

            // 3. every step with multiple return paths must either have EVERY path mapped, or have a default return path mapped.
            foreach (var step in UserSteps)
            {
                if (step.DefaultReturnPath == null)
                    foreach (var path in step.ChildProcess.ReturnPaths)
                        if (!step.returnPaths.ContainsKey(path))
                        {
                            errors.Add(string.Format("The '{0}' step doesn't have a default return path, but doesn't map every possible output of it's function.", step.ChildProcess.Name));
                            success = false;
                        }
            }

            // 4. every workspace variable must always be treated as the same type by everything that reads from it or writes to it.

            // 5. every mapped parameter mapped into a step should first have been set by every path that could lead to that point.
            // [hard]

            return success;
        }

        private IEnumerable<EndStep> EndSteps
        {
            get
            {
                foreach (var step in allSteps)
                    if (step is EndStep)
                        yield return step as EndStep;
            }
        }
        private IEnumerable<UserStep> UserSteps
        {
            get
            {
                foreach (var step in allSteps)
                    if (step is UserStep)
                        yield return step as UserStep;
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