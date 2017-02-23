using System.Collections.Generic;

namespace Cursive
{
    public class SystemProcess : Process
    {
        public SystemProcess(SystemStep operation, string description, IReadOnlyCollection<ValueKey> inputs, IReadOnlyCollection<ValueKey> outputs, IReadOnlyCollection<string> returnPaths)
            : base(description)
        {
            Operation = operation;
            Inputs = inputs;
            Outputs = outputs;
            ReturnPaths = returnPaths;
        }

        public string Name { get; }
        public delegate string SystemStep(ValueSet input, out ValueSet output);

        private SystemStep Operation { get; }
        public override IReadOnlyCollection<string> ReturnPaths { get; }
        public override IReadOnlyCollection<ValueKey> Inputs { get; }
        public override IReadOnlyCollection<ValueKey> Outputs { get; }

        public override string Run(ValueSet inputs, out ValueSet outputs)
        {
            return Operation(inputs, out outputs);
        }
    }
}