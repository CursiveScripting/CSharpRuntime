using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public abstract class Process<Model>
    {
        public abstract string Run(Model model);
        public abstract string[] GetPossibleOutputs(); // the UI needs this, but the runtime doesn't. Perhaps it doesn't hurt to keep it around anyway...
    }
}