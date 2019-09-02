using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public class SystemProcess : Process
    {
        public SystemProcess(string name, string description, Func<ValueSet, Task<ProcessResult>> operation, IReadOnlyCollection<Parameter> inputs, IReadOnlyCollection<Parameter> outputs, IReadOnlyCollection<string> returnPaths, string folder = null)
            : base(name, description, folder, inputs, outputs, returnPaths)
        {
            Operation = operation;
        }

        public SystemProcess(string name, string description, Func<ValueSet, ProcessResult> operation, IReadOnlyCollection<Parameter> inputs, IReadOnlyCollection<Parameter> outputs, IReadOnlyCollection<string> returnPaths, string folder = null)
            : base(name, description, folder, inputs, outputs, returnPaths)
        {
            Operation = vals => Task.FromResult(operation(vals));
        }

        public delegate Task<ProcessResult> SystemStep(ValueSet input);

        private Func<ValueSet, Task<ProcessResult>> Operation { get; }

        internal override async Task<ProcessResult> Run(ValueSet inputs, CallStack stack = null)
        {
            try
            {
                return await Operation(inputs);
            }
            catch (Exception e)
            {
                throw new CursiveRunException(stack, $"An error occurred running system process \"{Name}\"", e);
            }
        }
    }
}