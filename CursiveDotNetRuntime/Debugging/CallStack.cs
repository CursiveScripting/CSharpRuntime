using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive.Debugging
{
    public abstract class CallStack
    {
        internal abstract Task EnterStep(Process process, Step step, ValueSet variables);
        internal abstract void ExitStep();
    }

    public abstract class CallStack<FrameType> : CallStack
    {
        const int MaxStackSize = 100;
        private Stack<FrameType> frames = new Stack<FrameType>();
        public IReadOnlyCollection<FrameType> Frames { get { return frames; } }

        protected abstract FrameType CreateFrame(Process process, Step step, ValueSet variables);

        internal override async Task EnterStep(Process process, Step step, ValueSet variables)
        {
            if (frames.Count >= MaxStackSize)
                throw new InvalidOperationException($"The maximum call depth ({MaxStackSize}) has been exceeded. Possible infinite loop detected.");

            var frame = CreateFrame(process, step, variables);
            await Push(frame);
        }

        protected virtual Task Push(FrameType frame)
        {
            frames.Push(frame);
            return Task.CompletedTask;
        }

        internal override void ExitStep()
        {
            frames.Pop();
        }
    }
}