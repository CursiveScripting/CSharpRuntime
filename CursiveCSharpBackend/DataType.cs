using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursive
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

    public abstract class FixedType : DataType
    {
        public abstract object Deserialize(string value);
    }

    public class FixedType<T> : FixedType
    {
        public FixedType(string name, Func<string, T> deserialize, Func<T, string> serialize)
        {
            Name = name;
            DeserializationFunction = deserialize;
            Serialize = serialize;
        }

        public override object Deserialize(string value)
        {
            return DeserializationFunction(value);
        }

        public override Type SystemType { get { return typeof(T); } }
        private Func<string, T> DeserializationFunction { get; set; }
        public Func<T, string> Serialize { get; private set; }
    }
}
