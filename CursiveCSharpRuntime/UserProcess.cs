using CursiveCSharpRuntime.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cursive
{
    internal class UserProcess : Process
    {
        public UserProcess(string name, string description, IReadOnlyCollection<ValueKey> inputs, IReadOnlyCollection<ValueKey> outputs, IReadOnlyCollection<string> returnPaths, ValueSet defaultVariables, StartStep firstStep, IEnumerable<Step> allSteps)
            : base(description, null)
        {
            Name = name;
            Inputs = inputs;
            Outputs = outputs;
            ReturnPaths = returnPaths;
            DefaultVariables = defaultVariables;
            FirstStep = firstStep;
            Steps = allSteps;
        }

        public string Name { get; }

        public override IReadOnlyCollection<ValueKey> Inputs { get; }
        public override IReadOnlyCollection<ValueKey> Outputs { get; }
        public override IReadOnlyCollection<string> ReturnPaths { get; }
        private ValueSet DefaultVariables { get; }

        internal StartStep FirstStep { get; }
        internal IEnumerable<Step> Steps { get; }
        
        public override string Run(ValueSet inputs, out ValueSet outputs)
        {
            DebuggingService.EnterProcess(this);
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
                DebuggingService.ExitProcess(this);
                return end.ReturnValue;
            }

            throw new InvalidOperationException("The last step of a completed process wasn't an EndStep");
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