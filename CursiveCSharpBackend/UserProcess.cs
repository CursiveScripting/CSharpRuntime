using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursive
{
    class UserProcess : Process
    {
        public UserProcess(string name, string description, Step firstStep, IEnumerable<Step> allSteps)
            : base(description)
        {
            this.Name = name;
            this.firstStep = firstStep;
            this.Steps = allSteps;
        }

        public string Name { get; private set; }
        private Step firstStep;
        internal IEnumerable<Step> Steps { get; private set; }
        
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

            if (lastStep is StopStep)
            {
                var end = lastStep as StopStep;
                outputs = end.GetOutputs();
                return end.ReturnValue;
            }

            throw new InvalidOperationException("The last step of a completed process wasn't an EndStep");
        }

        public override ReadOnlyCollection<string> ReturnPaths
        {
            get
            {
                List<string> paths = new List<string>();
                foreach (var endStep in EndSteps)
                    paths.Add(endStep.ReturnValue);

                return paths.AsReadOnly();
            }
        }

        internal IEnumerable<StopStep> EndSteps
        {
            get
            {
                foreach (var step in Steps)
                    if (step is StopStep)
                        yield return step as StopStep;
            }
        }
        internal IEnumerable<UserStep> UserSteps
        {
            get
            {
                foreach (var step in Steps)
                    if (step is UserStep)
                        yield return step as UserStep;
            }
        }
        
        private List<Parameter> inputs = new List<Parameter>();
        private List<Parameter> outputs = new List<Parameter>();

        public void AddInput(Workspace workspace, string name, string typeName)
        {
            var type = workspace.GetType(typeName);
            inputs.Add(new Parameter(name, type.SystemType));
        }

        public void AddOutput(Workspace workspace, string name, string typeName)
        {
            var type = workspace.GetType(typeName);
            outputs.Add(new Parameter(name, type.SystemType));
        }

        public void AddVariable(Workspace workspace, string name, string typeName, string initialValue = null)
        {
            var type = workspace.GetType(typeName);
            // TODO: initialise internal variable
        }

        public override ReadOnlyCollection<Parameter> Inputs
        {
            get { return inputs.AsReadOnly(); }
        }

        public override ReadOnlyCollection<Parameter> Outputs
        {
            get { return outputs.AsReadOnly(); }
        }
    }
}