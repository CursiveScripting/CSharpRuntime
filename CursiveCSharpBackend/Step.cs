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
        protected internal Dictionary<Parameter, Parameter> InputMapping { get; } = new Dictionary<Parameter, Parameter>();
        protected internal Dictionary<Parameter, Parameter> OutputMapping { get; } = new Dictionary<Parameter, Parameter>();

        public void SetInputParameter(Parameter parameter, object value)
        {
            FixedInputs[parameter] = value;
        }

        public void MapInputParameter(Parameter parameter, Parameter source)
        {
            InputMapping[parameter] = source;
        }

        public void MapOutputParameter(Parameter parameter, Parameter destination)
        {
            OutputMapping[parameter] = destination;
        }
    }
}