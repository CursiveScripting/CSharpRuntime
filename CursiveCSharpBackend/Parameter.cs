using System;

namespace Cursive
{
    public class Parameter : IComparable<Parameter>, IEquatable<Parameter>
    {
        public Parameter(string name, DataType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public DataType Type { get; }

        public int CompareTo(Parameter other)
        {
            int val = Name.CompareTo(other.Name);

            if (val != 0)
                return val;

            return Type.Name.CompareTo(other.Type.Name);
        }

        public bool Equals(Parameter other)
        {
            return Name == other.Name && Type == other.Type;
        }
    }
}
