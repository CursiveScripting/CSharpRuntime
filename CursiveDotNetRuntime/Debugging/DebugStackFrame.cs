namespace Cursive.Debugging
{
    public struct DebugStackFrame : IStackFrame
    {
        internal DebugStackFrame(Process process, Step step, ValueSet variables)
        {
            Process = process;
            Step = step;
            Variables = variables;
        }

        public Process Process { get; }
        public Step Step { get; }
        public ValueSet Variables { get; }
    }
}