﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE.Processes
{
    public static class IO
    {
        public static readonly SystemProcess Print = new SystemProcess(
            (Model inputs, out Model outputs) =>
            {
                Console.WriteLine(inputs["message"]);
                outputs = null;
                return string.Empty;
            },
            "IO.Print",
            "Write a message to the system console.",
            new Parameter[] { new Parameter("message", "text") },
            null,
            null
        );
    }
}
