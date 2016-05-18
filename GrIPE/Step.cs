using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public abstract class Step
    {
        public abstract Step Run(Model model);

        // TODO: need stuff for mapping input and output parameters
    }
}