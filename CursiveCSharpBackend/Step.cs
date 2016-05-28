using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursive
{
    abstract class Step
    {
        protected Step(string name)
        {
            Name = name;
        }

        public abstract Step Run(Model model);

        public string Name { get; private set; }
        protected internal SortedList<string, object> fixedInputs = new SortedList<string, object>();
        protected internal SortedList<string, string> inputMapping = new SortedList<string, string>();

        public void SetInputParameter(string parameterName, object value)
        {
            fixedInputs[parameterName] = value;
        }

        public void MapInputParameter(string parameterName, string sourceName)
        {
            inputMapping[parameterName] = sourceName;
        }
    }
}