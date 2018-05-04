namespace Cursive.Debugging
{
    public class RunCallStack : CallStack<RunStackFrame>
    {
        protected override RunStackFrame CreateFrame(Process process, Step step, ValueSet variables)
        {
            return new RunStackFrame(process, step);
        }
    }
}