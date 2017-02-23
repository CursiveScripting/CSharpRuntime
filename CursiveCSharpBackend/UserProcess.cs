using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cursive
{
    class UserProcess : Process
    {
        public UserProcess(string name, string description, IReadOnlyCollection<Parameter> inputs, IReadOnlyCollection<Parameter> outputs, ValueSet defaultVariables, StartStep firstStep, IEnumerable<Step> allSteps)
            : base(description)
        {
            Name = name;
            Inputs = inputs;
            Outputs = outputs;
            DefaultVariables = defaultVariables;
            FirstStep = firstStep;
            Steps = allSteps;
        }

        public string Name { get; }

        public override IReadOnlyCollection<Parameter> Inputs { get; }
        public override IReadOnlyCollection<Parameter> Outputs { get; }
        private ValueSet DefaultVariables { get; }

        internal StartStep FirstStep { get; }
        internal IEnumerable<Step> Steps { get; }
        
        public override string Run(ValueSet inputs, out ValueSet outputs)
        {
            ValueSet variables = DefaultVariables.Clone();

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
        
    }
}