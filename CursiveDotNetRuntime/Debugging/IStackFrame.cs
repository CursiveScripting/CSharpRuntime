namespace Cursive.Debugging
{
    public interface IStackFrame
    {
        Process Process { get; }
        Step Step { get; }
    }
}