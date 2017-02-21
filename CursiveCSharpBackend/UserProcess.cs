using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cursive
{
    class UserProcess : Process
    {
        public UserProcess(string name, string description, StartStep firstStep, IEnumerable<Step> allSteps)
            : base(description)
        {
            Name = name;
            FirstStep = firstStep;
            Steps = allSteps;
        }

        public string Name { get; }
        internal StartStep FirstStep { get; }
        internal IEnumerable<Step> Steps { get; }
        
        public override string Run(ValueSet inputs, out ValueSet outputs)
        {
            ValueSet variables = InitializeVariables();

            FirstStep.SetInputs(inputs);
            Step currentStep = FirstStep, lastStep = null;

            while (currentStep != null)
            {
                lastStep = currentStep;
                currentStep = currentStep.Run(variables);
            }

            if (lastStep is StopStep)
            {
                var end = lastStep as StopStep;
                outputs = end.GetOutputs();
                return end.ReturnValue;
            }

            throw new InvalidOperationException("The last step of a completed process wasn't an EndStep");
        }

        public override IReadOnlyCollection<string> ReturnPaths
        {
            get
            {
                List<string> paths = new List<string>();
                foreach (var endStep in EndSteps)
                    paths.Add(endStep.ReturnValue);
                return paths;
            }
        }

        public override IReadOnlyCollection<Parameter> Inputs { get { return inputs; } }
        public override IReadOnlyCollection<Parameter> Outputs { get { return outputs; } }

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

        private ValueSet InitializeVariables()
        {
            var values = new ValueSet();
            // TODO: default values, based on the above AddVariable method
            return values;
        }
    }
}