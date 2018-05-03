using Cursive.Debugging;
using System;

namespace Cursive
{
    public class CursiveRunException : Exception
    {
        public CursiveRunException(CallStack stack, string message)
            : base(message)
        {
            CallStack = stack;
        }

        public CursiveRunException(CallStack stack, string message, Exception innerException)
            : base(message, innerException)
        {
            CallStack = stack;
        }

        public CallStack CallStack { get; }
    }
}
