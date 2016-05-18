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

        }
        
        public string GetOutputs(out Model outputs)
        {
            throw new NotImplementedException("Don't know where to get the outputs from. The return path is passed to EndStep's constructor, though.");
        }

        public override Step Run(Model model)
        {
            return null;
        }
    }
}