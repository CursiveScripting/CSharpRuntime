using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    internal class DebugCallStack : CallStack
    {
        public DebugCallStack(Func<StackFrame, Task> stepEntered, int maxStackSize = 100)
            : base(maxStackSize)
        {
            StepEntered = stepEntered;
        }

        private Func<StackFrame, Task> StepEntered { get; }

        protected override async Task Push(StackFrame frame)
        {
            if (StepEntered != null)
            {
                await StepEntered(frame);
            }

            await base.Push(frame);
        }
    }
}