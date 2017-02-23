using System;
using System.Text.RegularExpressions;

namespace Cursive
{
    public abstract class DataType
    {
        public string Name { get; protected set; }
        public Regex Validation { get; protected set; }
        public abstract Type SystemType { get; }

        public abstract object GetDefaultValue();

        public static object GetTypeDefault(Type t)
        {
            Func<object> f = GetTypeDefault<object>;
            return f.Method.GetGenericMethodDefinition().MakeGenericMethod(t).Invoke(null, null);
        }
        private static T GetTypeDefault<T>() { return default(T); }
    }

    public class DataType<T> : DataType
    {
        public DataType(string name, Func<T> getDefault = null)
        {
            Name = name;
            GetDefault = getDefault;
        }

        public override Type SystemType { get { return typeof(T); } }

        private Func<T> GetDefault { get; }

        public override object GetDefaultValue()
        {
            if (GetDefault == null)
                return GetTypeDefault(typeof(T));
            return GetDefault();
        }
    }

    public interface IDeserializable
    {
        object Deserialize(string value);
    }

    public class FixedType<T> : DataType<T>, IDeserializable
    {
        public FixedType(string name, Regex validation, Func<string, T> deserialize, Func<T> getDefault = null)
            : base(name, getDefault)
        {
            Validation = validation;
            DeserializationFunction = deserialize;
        }
        
        public object Deserialize(string value)
        {
            return DeserializationFunction(value);
        }
        
        private Func<string, T> DeserializationFunction { get; }
    }
}
