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
        protected internal Dictionary<ValueKey, ValueKey> InputMapping { get; } = new Dictionary<ValueKey, ValueKey>();
        protected internal Dictionary<ValueKey, ValueKey> OutputMapping { get; } = new Dictionary<ValueKey, ValueKey>();

        public void SetInputParameter(ValueKey parameter, object value)
        {
            FixedInputs[parameter] = value;
        }

        public void MapInputParameter(ValueKey parameter, ValueKey source)
        {
            InputMapping[parameter] = source;
        }

        public void MapOutputParameter(ValueKey parameter, ValueKey destination)
        {
            OutputMapping[parameter] = destination;
        }
    }
}