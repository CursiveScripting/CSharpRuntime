using CursiveRuntime.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public class RequiredProcess : Process
    {
        public RequiredProcess(string description, IReadOnlyCollection<ValueKey> inputs, IReadOnlyCollection<ValueKey> outputs, IReadOnlyCollection<string> returnPaths, string folder = null)
            : base(description, folder)
        {
            Inputs = inputs;
            Outputs = outputs;
            ReturnPaths = returnPaths;
        }
        
        internal Process ActualProcess { get; set; }
        public override IReadOnlyCollection<string> ReturnPaths { get; }
        public override IReadOnlyCollection<ValueKey> Inputs { get; }
        public override IReadOnlyCollection<ValueKey> Outputs { get; }

        internal override async Task<Response> Run(ValueSet inputs, CallStack stack)
        {
            return await ActualProcess.Run(inputs, stack);
        }

        public async Task<Response> Run(ValueSet inputs)
        {
            var stack = new CallStack();
            return await Run(inputs, stack);
        }

        public async Task<Response> Debug(ValueSet inputs, Func<Process, Step, Task> enteredStep)
        {
            var stack = new DebugStack(enteredStep);
            return await Run(inputs, stack);
        }
    }
}