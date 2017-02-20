using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursive
{
    public class Parameter : IComparable<Parameter>, IEquatable<Parameter>
    {
        public Parameter(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; private set; }
        public Type Type { get; private set; }

        public int CompareTo(Parameter other)
        {
            int val = Name.CompareTo(other.Name);

            if (val != 0)
                return val;

            return Type.FullName.CompareTo(other.Type.FullName);
        }

        public bool Equals(Parameter other)
        {
            return Name == other.Name && Type == other.Type;
        }
    }
}
