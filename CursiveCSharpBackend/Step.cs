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
        protected internal Dictionary<string, object> fixedInputs = new Dictionary<string, object>();
        protected internal Dictionary<string, string> inputMapping = new Dictionary<string, string>();

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