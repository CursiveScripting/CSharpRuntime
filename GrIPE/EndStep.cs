using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class EndStep : Step
    {
        public EndStep(string returnPath)
        {
            this.returnPath = returnPath;
        }

        private string returnPath;
        private Model outputs;

        public string GetOutputs(Model workspace, out Model outputs)
        {
            outputs = this.outputs;
            this.outputs = null;
            return returnPath;
        }

        public override Step Run(Model workspace)
        {
            outputs = new Model();
            foreach (var kvp in outputMapping)
                workspace[kvp.Value] = outputs[kvp.Key];
            
            return null;
        }
    }
}