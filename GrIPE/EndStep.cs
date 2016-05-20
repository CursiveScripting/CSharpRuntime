using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class EndStep : Step
    {
        public EndStep(string returnPath = "")
        {
            ReturnPath = returnPath;
        }

        internal string ReturnPath { get; private set; }
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
            foreach (var kvp in outputMapping)
                outputs[kvp.Key] = workspace[kvp.Value];
            
            return null;
        }
    }
}