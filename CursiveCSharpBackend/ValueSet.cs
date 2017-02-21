using System;
using System.Collections;
using System.Collections.Generic;

namespace Cursive
{
    public class ValueSet : IEnumerable<KeyValuePair<string, object>>
    {
        Dictionary<string, object> elements = new Dictionary<string, object>();

        public object this[string key]
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

        public bool HasElement(string name)
        {
            return elements.ContainsKey(name);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
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
