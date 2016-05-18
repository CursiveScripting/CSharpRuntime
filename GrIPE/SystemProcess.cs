using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class SystemProcess : Process
    {
        public SystemProcess(SystemStep operation, string[] inputNames, string[] outputNames, params string[] returnPaths)
        {
            Operation = operation;
            ReturnPaths = returnPaths == null ? null : Array.AsReadOnly(returnPaths);
            InputNames = inputNames == null ? null : Array.AsReadOnly(inputNames);
            OutputNames = outputNames == null ? null : Array.AsReadOnly(outputNames);
        }

        public delegate string SystemStep(Model input, out Model output);

        private SystemStep Operation;
        private ReadOnlyCollection<string> ReturnPaths, InputNames, OutputNames;

        public override string Run(Model inputs, out Model outputs)
        {
            return Operation(inputs, out outputs);
        }

        public override ReadOnlyCollection<string> GetReturnPaths()
        {
            return ReturnPaths;
        }

        public override ReadOnlyCollection<string> ListInputs()
        {
            return InputNames;
        }

        public override ReadOnlyCollection<string> ListOutputs()
        {
            return OutputNames;
        }
    }
}