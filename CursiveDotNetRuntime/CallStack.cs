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

        public ValueSet CurrentVariables { get; private set; }

        internal async Task EnterNewProcess(UserProcess process, StartStep step, ValueSet variables)
        {
            CurrentVariables = variables;

            await EnterStep(process, step);
        }

        internal async Task EnterStep(UserProcess process, Step step)
        {
            if (frames.Count >= MaxStackSize)
                throw new CursiveRunException(this, $"The maximum call depth ({MaxStackSize}) has been exceeded. Possible infinite loop detected.");

            var frame = CreateFrame(process, step);

            await Push(frame);
        }

        internal virtual StackFrame CreateFrame(UserProcess process, Step step)
        {
            return new StackFrame(process, step, CurrentVariables);
        }

        protected virtual Task Push(StackFrame frame)
        {
            frames.Push(frame);
            return Task.CompletedTask;
        }

        internal void ExitProcess()
        {
            ExitStep();

            CurrentVariables = frames.Count > 0
                ? frames.Peek().Variables
                : null;
        }

        internal void ExitStep()
        {
            frames.Pop();
        }
    }
}