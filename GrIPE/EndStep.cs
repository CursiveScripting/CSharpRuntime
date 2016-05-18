using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class EndStep<Model> : Step<Model>
    {
        public EndStep(string output)
        {
            Output = output;
        }

        public string Output { get; private set; }

        public override Step<Model> Run(Model model)
        {
            return null;
        }
    }
}