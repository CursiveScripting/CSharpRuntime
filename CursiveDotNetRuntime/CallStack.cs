using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public class CallStack
    {
        const int MaxStackSize = 100;
        private Stack<StackFrame> frames = new Stack<StackFrame>();
        public IReadOnlyCollection<StackFrame> Frames { get { return frames; } }

        internal virtual Task Push(StackFrame frame)
        {
            if (frames.Count >= MaxStackSize)
                throw new InvalidOperationException($"The maximum call depth ({MaxStackSize}) has been exceeded. Possible infinite loop detected.");

            frames.Push(frame);
            return Task.CompletedTask;
        }

        internal StackFrame Pop()
        {
            return frames.Pop();
        }
    }

    internal class DebugStack : CallStack
    {
        public DebugStack(Func<Process, Step, Task> stepEntered)
        {
            StepEntered = stepEntered;
        }

        private Func<Process, Step, Task> StepEntered { get; }

        internal override async Task Push(StackFrame frame)
        {
            await StepEntered(frame.Process, frame.Step);
            await base.Push(frame);
        }
    }
}