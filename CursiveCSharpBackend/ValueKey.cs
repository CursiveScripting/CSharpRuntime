using System;

namespace Cursive
{
    public class ValueKey : IComparable<ValueKey>, IEquatable<ValueKey>
    {
        public ValueKey(string name, DataType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public DataType Type { get; }

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
}
