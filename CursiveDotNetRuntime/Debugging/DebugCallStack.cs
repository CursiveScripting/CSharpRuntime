using System;
using System.Threading.Tasks;

namespace Cursive.Debugging
{
    internal class DebugCallStack : CallStack<DebugStackFrame>
    {
        public DebugCallStack(Func<DebugStackFrame, Task> stepEntered)
        {
            StepEntered = stepEntered;
        }

        private Func<DebugStackFrame, Task> StepEntered { get; }

        protected override DebugStackFrame CreateFrame(Process process, Step step, ValueSet variables)
        {
            return new DebugStackFrame(process, step, variables.Clone());
        }

        protected override async Task Push(DebugStackFrame frame)
        {
            if (StepEntered != null)
            {
                await StepEntered(frame);
            }

            await base.Push(frame);
        }
    }
}