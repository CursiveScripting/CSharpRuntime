using Cursive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursiveRuntime.Services
{
    internal static class DebuggingService
    {
        // this assumes that only a single call can ever be active at a time, in a single thread.
        private static int callDepth = 0;
        public const int MaxCallDepth = 100;

#if DEBUG
        static Stack<UserProcess> callStack;
#endif

        public static void StartNewCall(RequiredProcess process)
        {
            callDepth = 0;
#if DEBUG
            callStack = new Stack<UserProcess>();
#endif
        }

        public static void EnterProcess(UserProcess process)
        {
            callDepth++;

            if (callDepth > MaxCallDepth)
                throw new Exception(string.Format("The maximum call depth ({0}) has been exceeded. Possible infinite loop detected.", MaxCallDepth));
#if DEBUG
            callStack.Push(process);
#endif
        }

        public static void ExitProcess(UserProcess process)
        {
            callDepth--;

#if DEBUG
            if (callStack.Pop() != process)
                throw new Exception(string.Format("Unexpected process on top of stack. Removing process regardless.", MaxCallDepth));
#endif
        }

#if DEBUG
        public static IEnumerable<UserProcess> GetStack() { return callStack; }
#endif
    }
}
