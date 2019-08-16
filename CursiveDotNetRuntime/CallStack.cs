using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public class CallStack
    {
        public CallStack(int maxStackSize = 100)
        {
            MaxStackSize = maxStackSize;
        }

        public int MaxStackSize { get; }

        private readonly Stack<StackFrame> frames = new Stack<StackFrame>();

        public IReadOnlyCollection<StackFrame> Frames { get; }

        public ValueSet CurrentVariables => frames.Peek().Variables;

        internal async Task EnterStep(UserProcess process, Step step, ValueSet variables)
        {
            if (frames.Count >= MaxStackSize)
                throw new CursiveRunException(this, $"The maximum call depth ({MaxStackSize}) has been exceeded. Possible infinite loop detected.");

            var frame = CreateFrame(process, step, variables);
            await Push(frame);
        }

        internal virtual StackFrame CreateFrame(UserProcess process, Step step, ValueSet variables)
        {
            return new StackFrame(process, step, variables);
        }

        protected virtual Task Push(StackFrame frame)
        {
            frames.Push(frame);
            return Task.CompletedTask;
        }

        internal void ExitStep()
        {
            frames.Pop();
        }
    }
}