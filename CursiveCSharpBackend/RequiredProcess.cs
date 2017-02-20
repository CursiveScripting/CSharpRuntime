using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursive
{
    public class RequiredProcess : Process
    {
        public RequiredProcess(string description, Parameter[] inputs, Parameter[] outputs, string[] returnPaths)
            : base(description)
        {
            this.inputs = inputs == null ? null : Array.AsReadOnly(inputs);
            this.outputs = outputs == null ? null : Array.AsReadOnly(outputs);
            this.returnPaths = Array.AsReadOnly(returnPaths == null ? new string[] { null } : returnPaths);
        }
        
        internal Process ActualProcess { get; set; }
        private ReadOnlyCollection<string> returnPaths;
        private ReadOnlyCollection<Parameter> inputs, outputs;

        public override string Run(Model inputs, out Model outputs)
        {
            return ActualProcess.Run(inputs, out outputs);
        }

        public override ReadOnlyCollection<string> ReturnPaths { get { return returnPaths; } }

        public override ReadOnlyCollection<Parameter> Inputs { get { return inputs; } }

        public override ReadOnlyCollection<Parameter> Outputs { get { return outputs; } }
    }
}