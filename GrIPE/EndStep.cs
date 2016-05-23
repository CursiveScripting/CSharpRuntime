using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class EndStep : Step
    {
        public EndStep(string name, string returnValue = "")
            : base(name)
        {
            ReturnValue = returnValue;
        }

        internal string ReturnValue { get; private set; }
        private Model outputs;

        public Model GetOutputs()
        {
            Model outputs = this.outputs;
            this.outputs = null;
            return outputs;
        }

        public override Step Run(Model workspace)
        {
            outputs = new Model();
            foreach (var kvp in inputMapping)
                outputs[kvp.Key] = workspace[kvp.Value];
            
            return null;
        }
    }
}