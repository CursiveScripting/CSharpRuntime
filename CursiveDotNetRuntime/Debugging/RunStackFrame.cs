namespace Cursive.Debugging
{
    public struct RunStackFrame : IStackFrame
    {
        internal RunStackFrame(Process process, Step step)
        {
            Process = process;
            Step = step;
        }

        public Process Process { get; }
        public Step Step { get; }
    }
}