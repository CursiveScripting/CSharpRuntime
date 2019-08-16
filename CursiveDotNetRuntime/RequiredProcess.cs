using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public class RequiredProcess : Process
    {
        public RequiredProcess(
            string name,
            string description,
            IReadOnlyCollection<Parameter> inputs,
            IReadOnlyCollection<Parameter> outputs,
            IReadOnlyCollection<string> returnPaths,
            string folder = null
        )
            : base(name, description, folder, inputs, outputs, returnPaths)
        { }
        
        internal UserProcess Implementation { get; set; }

        internal override async Task<Response> Run(ValueSet inputs, CallStack stack)
        {
            return await Implementation.Run(inputs, stack);
        }

        public async Task<Response> Run(ValueSet inputs)
        {
            var stack = new CallStack();
            return await Run(inputs, stack);
        }

        public async Task<Response> Debug(ValueSet inputs, Func<StackFrame, Task> enteredStep)
        {
            var stack = new DebugCallStack(enteredStep);
            return await Run(inputs, stack);
        }
    }
}