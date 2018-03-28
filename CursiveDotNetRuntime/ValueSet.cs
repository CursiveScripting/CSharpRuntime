using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]

namespace Cursive
{
    public class ValueSet : IEnumerable<KeyValuePair<ValueKey, object>>
    {
        Dictionary<ValueKey, object> elements = new Dictionary<ValueKey, object>();

        internal object this[ValueKey key]
        {
            get
            {
                object o;
                if (!elements.TryGetValue(key, out o))
                    throw new Exception(string.Format("Value not found: {0} / {1}", key.Name, key.Type.Name));
                return o;
            }
            set
            {
                elements[key] = value;
            }
        }

        public T Get<T>(ValueKey<T> key)
        {
            return (T)this[key];
        }

        public void Set<T>(ValueKey<T> key, T value)
        {
            this[key] = value;
        }

        public bool HasElement(ValueKey param)
        {
            return elements.ContainsKey(param);
        }

        public IEnumerator<KeyValuePair<ValueKey, object>> GetEnumerator()
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
