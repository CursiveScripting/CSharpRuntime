using Cursive.Debugging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]

namespace Cursive
{
    public class ValueSet : IEnumerable<KeyValuePair<ValueKey, object>>
    {
        private CallStack Stack { get; }
        private Dictionary<ValueKey, object> Elements { get; } = new Dictionary<ValueKey, object>();

        internal ValueSet(CallStack callStack = null)
        {
            Stack = callStack;
        }

        internal object this[ValueKey key]
        {
            get
            {
                object o;
                if (!Elements.TryGetValue(key, out o))
                    throw new CursiveRunException(Stack, $"Value not found: {key.Name} / {key.Type.Name}");
                return o;
            }
            set
            {
                Elements[key] = value;
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
            return Elements.ContainsKey(param);
        }

        public IEnumerator<KeyValuePair<ValueKey, object>> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        internal ValueSet Clone()
        {
            ValueSet other = new ValueSet(Stack);
            foreach (var kvp in Elements)
                other[kvp.Key] = kvp.Value;
            return other;
        }
    }
}
