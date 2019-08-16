using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public class SystemProcess : Process
    {
        public SystemProcess(string name, string description, SystemStep operation, IReadOnlyCollection<Parameter> inputs, IReadOnlyCollection<Parameter> outputs, IReadOnlyCollection<string> returnPaths, string folder = null)
            : base(name, description, folder, inputs, outputs, returnPaths)
        {
            Operation = operation;
        }

        public delegate Task<Response> SystemStep(ValueSet input);

        private SystemStep Operation { get; }

        internal override async Task<Response> Run(ValueSet inputs, CallStack stack = null)
        {
            try
            {
                return await Operation(inputs);
            }
            catch (Exception e)
            {
                throw new CursiveRunException(stack, $"An error occurred running system process ${Name}", e);
            }
        }
    }
}