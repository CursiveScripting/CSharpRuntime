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

        private static T GetTypeDefault<T>()
        {
            return default(T);
        }
    }

    public class DataType<T> : DataType
    {
        public DataType(string name, Func<T> getDefault = null)
        {
            Name = name;
            GetDefault = getDefault;
        }

        private Func<T> GetDefault { get; }

        public override object GetDefaultValue()
        {
            if (GetDefault == null)
                return GetTypeDefault(typeof(T));
            return GetDefault();
        }

        public override Type SystemType { get { return typeof(T); } }
    }

    public abstract class FixedType : DataType
    {
        public abstract object Deserialize(string value);
    }

    public class FixedType<T> : FixedType
    {
        public FixedType(string name, Regex validation, Func<string, T> deserialize, Func<T, string> serialize, Func<T> getDefault = null)
        {
            Name = name;
            GetDefault = getDefault;
            Validation = validation;
            DeserializationFunction = deserialize;
            Serialize = serialize;
        }

        private Func<T> GetDefault { get; }

        public override object GetDefaultValue()
        {
            if (GetDefault == null)
                return GetTypeDefault(typeof(T));
            return GetDefault();
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
