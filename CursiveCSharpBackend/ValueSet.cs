using System;
using System.Collections;
using System.Collections.Generic;

namespace Cursive
{
    public class ValueSet : IEnumerable<KeyValuePair<Parameter, object>>
    {
        Dictionary<Parameter, object> elements = new Dictionary<Parameter, object>();

        public object this[Parameter key]
        {
            get
            {
                object o;
                if (elements.TryGetValue(key, out o))
                    return o;
                return null;
            }
            set
            {
                elements[key] = value;
            }
        }

        public bool HasElement(Parameter param)
        {
            return elements.ContainsKey(param);
        }

        public IEnumerator<KeyValuePair<Parameter, object>> GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        internal ValueSet Clone()
        {
            ValueSet other = new ValueSet();
            foreach (var kvp in elements)
                other[kvp.Key] = kvp.Value;
            return other;
        }
    }
}
