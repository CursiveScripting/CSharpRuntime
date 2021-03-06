﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public class RequiredProcess : Process
    {
        public RequiredProcess(
            string name,
            string description,
            IReadOnlyList<Parameter> inputs,
            IReadOnlyList<Parameter> outputs,
            IReadOnlyList<string> returnPaths,
            string folder = null
        )
            : base(name, description, folder, inputs, outputs, returnPaths)
        { }
        
        internal UserProcess Implementation { get; set; }

        internal override async Task<ProcessResult> Run(ValueSet inputs, CallStack stack)
        {
            return await Implementation.Run(inputs, stack);
        }

        public async Task<ProcessResult> Run(ValueSet inputs)
        {
            var stack = new CallStack();
            return await Run(inputs, stack);
        }

        public async Task<ProcessResult> Debug(ValueSet inputs, Func<StackFrame, Task> enteredStep)
        {
            var stack = new DebugCallStack(enteredStep);
            return await Run(inputs, stack);
        }
    }
}