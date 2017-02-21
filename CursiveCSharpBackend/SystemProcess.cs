using System.Collections.Generic;

namespace Cursive
{
    public class SystemProcess : Process
    {
        public SystemProcess(SystemStep operation, string description, IReadOnlyCollection<Parameter> inputs, IReadOnlyCollection<Parameter> outputs, IReadOnlyCollection<string> returnPaths)
            : base(description)
        {
            Operation = operation;
            Inputs = inputs;
            Outputs = outputs;
            ReturnPaths = returnPaths;
        }

        public delegate string SystemStep(Model input, out Model output);

        private SystemStep Operation { get; }
        public override IReadOnlyCollection<string> ReturnPaths { get; }
        public override IReadOnlyCollection<Parameter> Inputs { get; }
        public override IReadOnlyCollection<Parameter> Outputs { get; }

        public override string Run(Model inputs, out Model outputs)
        {
            return Operation(inputs, out outputs);
        }
    }
}