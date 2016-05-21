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
        public SystemProcess(Workspace workspace, SystemStep operation, string description, Parameter[] inputs, Parameter[] outputs, string[] returnPaths)
            : base(description)
        {
            this.operation = operation;
            this.inputs = inputs == null ? null : Array.AsReadOnly(inputs);
            this.outputs = outputs == null ? null : Array.AsReadOnly(outputs);
            this.returnPaths = returnPaths == null ? null : Array.AsReadOnly(returnPaths);
        }

        public delegate string SystemStep(Model input, out Model output);

        private SystemStep operation;
        private ReadOnlyCollection<string> returnPaths;
        private ReadOnlyCollection<Parameter> inputs, outputs;

        public override string Run(Model inputs, out Model outputs)
        {
            return operation(inputs, out outputs);
        }

        public override ReadOnlyCollection<string> ReturnPaths { get { return returnPaths; } }

        public override ReadOnlyCollection<Parameter> Inputs { get { return inputs; } }

        public override ReadOnlyCollection<Parameter> Outputs { get { return outputs; } }
    }
}