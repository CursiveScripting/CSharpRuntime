using System.Collections.Generic;

namespace Cursive
{
    public class ValueSet
    {
        public ValueSet()
        {
            Values = new Dictionary<string, object>();
        }

        public ValueSet(Dictionary<string, object> values)
        {
            Values = values;
        }

        internal Dictionary<string, object> Values { get; }

        public TValue Get<TValue>(Parameter<TValue> parameter)
        {
            return (TValue)Values[parameter.Name];
        }

        public void Set<TValue>(Parameter<TValue> parameter, TValue value)
        {
            Values[parameter.Name] = value;
        }
    }
}
