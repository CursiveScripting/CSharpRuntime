using System;

namespace Cursive
{
    public class Parameter : IComparable<Parameter>, IEquatable<Parameter>
    {
        public Parameter(string name, DataType type)
        {
            this.Name = name;
            this.Type = type;
        }

        public Parameter(string name, Type type)
        {
            throw new NotImplementedException("Need to get the type from the workspace... but don't have one of those");
        }

        public string Name { get; private set; }
        public DataType Type { get; private set; }

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
