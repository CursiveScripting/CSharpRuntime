using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public class SystemProcess : Process
    {
        public SystemProcess(SystemStep operation, string description, IReadOnlyCollection<ValueKey> inputs, IReadOnlyCollection<ValueKey> outputs, IReadOnlyCollection<string> returnPaths, string folder = null)
            : base(description, folder)
        {
            Operation = operation;
            Inputs = inputs;
            Outputs = outputs;
            ReturnPaths = returnPaths;
        }

        public string Name { get; internal set; }
        public delegate Task<Response> SystemStep(ValueSet input);

        private SystemStep Operation { get; }
        public override IReadOnlyCollection<string> ReturnPaths { get; }
        public override IReadOnlyCollection<ValueKey> Inputs { get; }
        public override IReadOnlyCollection<ValueKey> Outputs { get; }

        public override async Task<Response> Run(ValueSet inputs)
        {
            return await Operation(inputs);
        }
    }
}