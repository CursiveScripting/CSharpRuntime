using Cursive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Processes
{
    public static class IO
    {
        private static Parameter messageParam = new Parameter("message", Program.text);
        public static readonly SystemProcess Print = new SystemProcess(
            (ValueSet inputs, out ValueSet outputs) =>
            {
                Console.WriteLine(messageParam);
                outputs = null;
                return string.Empty;
            },
            "Write a message to the system console.",
            new Cursive.Parameter[] { messageParam },
            null,
            null
        );
    }
}
