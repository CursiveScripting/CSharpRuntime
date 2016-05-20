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
        public SystemProcess(SystemStep operation, string name, string description, string[] inputNames, string[] outputNames, string[] returnPaths)
            : base(name, description)
        {
            this.operation = operation;
            this.inputNames = inputNames == null ? null : Array.AsReadOnly(inputNames);
            this.outputNames = outputNames == null ? null : Array.AsReadOnly(outputNames);
            this.returnPaths = returnPaths == null ? null : Array.AsReadOnly(returnPaths);
        }

        public delegate string SystemStep(Model input, out Model output);

        private SystemStep operation;
        private ReadOnlyCollection<string> returnPaths, inputNames, outputNames;

        public override string Run(Model inputs, out Model outputs)
        {
            return operation(inputs, out outputs);
        }

        public override ReadOnlyCollection<string> ReturnPaths { get { return returnPaths; } }

        public override ReadOnlyCollection<string> Inputs { get { return inputNames; } }

        public override ReadOnlyCollection<string> Outputs { get { return inputNames; } }
    }
}