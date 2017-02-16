using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursive
{
    class StartStep : Step
    {
        public StartStep(string name)
            : base(name)
        {
        }
        
        private Model inputs;

        public Model GetInputs()
        {
            Model inputs = this.inputs;
            this.inputs = null;
            return inputs;
        }

        public override Step Run(Model workspace)
        {
            inputs = new Model();
            foreach (var kvp in inputMapping)
                workspace[kvp.Value] = inputs[kvp.Key];

            return null;
        }
    }
}