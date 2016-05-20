using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public abstract class DataType
    {
        public string Name { get; protected set; }
        public abstract Type SystemType { get; }
    }

    public class DataType<T> : DataType
    {
        public DataType(string name)
        {
            Name = name;
        }
        
        public override Type SystemType { get { return typeof(T); } }
    }

    public class FixedType<T> : DataType<T>
    {
        public FixedType(string name, Func<string, T> deserialize, Func<T, string> serialize)
            : base(name)
        {
            Deserialize = deserialize;
            Serialize = serialize;
        }

        public Func<string, T> Deserialize { get; private set; }
        public Func<T, string> Serialize { get; private set; }
    }
}
