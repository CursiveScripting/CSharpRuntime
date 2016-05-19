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

        protected internal SortedList<string, string> outputMapping = new SortedList<string, string>();

        public void MapOutputParameter(string parameterName, string destinationName)
        {
            outputMapping[parameterName] = destinationName;
        }
    }
}