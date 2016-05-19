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

        protected SortedList<string, object> fixedParameters = new SortedList<string, object>();
        protected SortedList<string, string> inputMapping = new SortedList<string, string>();
        protected SortedList<string, string> outputMapping = new SortedList<string, string>();

        public void SetInputParameter(string parameterName, object value)
        {
            fixedParameters[parameterName] = value;
        }

        public void MapInputParameter(string parameterName, string sourceName)
        {
            inputMapping[parameterName] = sourceName;
        }

        public void MapOutputParameter(string parameterName, string destinationName)
        {
            outputMapping[parameterName] = destinationName;
        }
    }
}