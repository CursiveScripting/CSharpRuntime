using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public abstract class Step<Model>
    {
        public abstract Step<Model> Run(Model model);
    }
}