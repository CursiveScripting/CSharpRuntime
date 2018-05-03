using System.Threading.Tasks;

namespace Cursive
{
    public struct StackFrame
    {
        internal StackFrame(Process process, Step step)
        {
            Process = process;
            Step = step;
        }

        public Process Process { get; }
        public Step Step { get; }
    }
}