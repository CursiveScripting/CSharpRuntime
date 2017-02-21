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
        public static readonly SystemProcess Print = new SystemProcess(
            (ValueSet inputs, out ValueSet outputs) =>
            {
                Console.WriteLine(inputs["message"]);
                outputs = null;
                return string.Empty;
            },
            "Write a message to the system console.",
            new Parameter[] { new Parameter("message", typeof(string)) },
            null,
            null
        );
    }
}
