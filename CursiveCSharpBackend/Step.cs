using System.Collections.Generic;

namespace Cursive
{
    abstract class Step
    {
        protected Step(string name)
        {
            Name = name;
        }

        public abstract Step Run(ValueSet variables);

        public string Name { get; }
        protected internal ValueSet FixedInputs { get; } = new ValueSet();
        protected internal Dictionary<string, string> InputMapping { get; } = new Dictionary<string, string>();

        public void SetInputParameter(string parameterName, object value)
        {
            FixedInputs[parameterName] = value;
        }

        public void MapInputParameter(string parameterName, string sourceName)
        {
            InputMapping[parameterName] = sourceName;
        }
    }
}