namespace Cursive
{
    public struct StackFrame
    {
        internal StackFrame(UserProcess process, Step step, ValueSet variables)
        {
            Process = process;
            Step = step;
            Variables = variables;
        }

        internal UserProcess Process { get; }

        public Step Step { get; }

        public ValueSet Variables { get; }
    }
}