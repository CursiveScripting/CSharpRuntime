using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public abstract class Process
    {
        public string Run(Model input)
        {
            Model output;
            return Run(input, out output);
        }

        public abstract string Run(Model input, out Model output);
        
        public abstract ReadOnlyCollection<string> GetReturnPaths();

        public abstract ReadOnlyCollection<string> ListInputs();

        public abstract ReadOnlyCollection<string> ListOutputs();
    }
}