using System;
using System.Text.RegularExpressions;

namespace Cursive
{
    public abstract class DataType
    {
        protected DataType(string name, DataType extends = null, Regex validation = null, string guidance = null)
        {
            Name = name;
            Extends = extends;
            Validation = validation;
            Guidance = guidance;
        }

        public string Name { get; }
        public string Guidance { get; }
        public DataType Extends { get; }
        public Regex Validation { get; }
        public abstract Type SystemType { get; }

        public abstract object GetDefaultValue();

        protected static object GetTypeDefault(Type t)
        {
            Func<object> f = GetTypeDefault<object>;
            return f.Method.GetGenericMethodDefinition().MakeGenericMethod(t).Invoke(null, null);
        }
        private static T GetTypeDefault<T>() { return default(T); }

        public bool IsAssignableFrom(DataType other)
        {
            DataType test = this;

            do
            {
                if (test == other)
                    return true;

                test = test.Extends;
            } while (test != null);

            return false;
        }
    }

    public class DataType<T> : DataType
    {
        public DataType(string name, DataType extends = null, Func<T> getDefault = null)
            : this(name, extends, getDefault, null) { }

        protected DataType(string name, DataType extends = null, Func<T> getDefault = null, Regex validation = null, string guidance = null)
            : base(name, extends, validation, guidance)
        {
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
        public FixedType(string name, Regex validation, Func<string, T> deserialize, Func<T> getDefault = null, DataType extends = null, string guidance = null)
            : base(name, extends, getDefault, validation, guidance)
        {
            DeserializationFunction = deserialize;
        }
        
        public object Deserialize(string value)
        {
            return DeserializationFunction(value);
        }
        
        private Func<string, T> DeserializationFunction { get; }
    }
}
