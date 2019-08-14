using Newtonsoft.Json;
using System;

namespace Cursive
{
    public class ValueKey : IComparable<ValueKey>, IEquatable<ValueKey>
    {
        internal ValueKey(string name, DataType type)
        {
            Name = name;
            Type = type;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        [JsonIgnore]
        public DataType Type { get; }

        [JsonProperty(PropertyName = "type")]
        private string TypeName => Type.Name;

        public int CompareTo(ValueKey other)
        {
            int val = Name.CompareTo(other.Name);

            if (val != 0)
                return val;

            return Type.Name.CompareTo(other.Type.Name);
        }

        public bool Equals(ValueKey other)
        {
            return Name == other.Name && Type == other.Type;
        }
    }

    public class ValueKey<T> : ValueKey
    {
        public ValueKey(string name, DataType<T> type)
            : base(name, type)
            { }
    }
}
