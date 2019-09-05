using Manatee.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cursive
{
    public abstract class DataType
    {
        protected DataType(string name, Color color, DataType extends = null, Regex validation = null, string guidance = null)
        {
            Name = name;
            Color = color;
            Extends = extends;
            Validation = validation;
            Guidance = guidance;
        }

        [JsonMapTo("name")]
        public string Name { get; }

        [JsonIgnore]
        public Color Color { get; }

        [JsonMapTo("color")]
        private string ColorCode => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

        [JsonMapTo("guidance")]
        public string Guidance { get; }

        [JsonIgnore]
        public DataType Extends { get; }

        [JsonMapTo("extends")]
        private string ExtendsName => Extends?.Name;

        [JsonIgnore]        
        public Regex Validation { get; }

        [JsonMapTo("validation")]
        private string ValidationPattern => Validation?.ToString();

        public abstract object GetDefaultValue();

        protected static object GetTypeDefault(Type t)
        {
            Func<object> f = GetTypeDefault<object>;
            return f.Method.GetGenericMethodDefinition().MakeGenericMethod(t).Invoke(null, null);
        }

        private static T GetTypeDefault<T>() { return default(T); }

        public bool IsAssignableTo(DataType destinationType)
        {
            DataType test = this;

            do
            {
                if (test == destinationType)
                    return true;

                test = test.Extends;
            } while (test != null);

            return false;
        }
    }

    public class DataType<T> : DataType
    {
        public DataType(string name, Color color, DataType extends = null, Func<T> getDefault = null)
            : this(name, color, extends, getDefault, null) { }

        protected DataType(string name, Color color, DataType extends = null, Func<T> getDefault = null, Regex validation = null, string guidance = null)
            : base(name, color, extends, validation, guidance)
        {
            GetDefault = getDefault;
        }

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
        public FixedType(string name, Color color, Regex validation, Func<string, T> deserialize, Func<T> getDefault = null, DataType extends = null, string guidance = null)
            : base(name, color, extends, getDefault, validation, guidance)
        {
            DeserializationFunction = deserialize;
        }
        
        public object Deserialize(string value)
        {
            return DeserializationFunction(value);
        }
        
        private Func<string, T> DeserializationFunction { get; }
    }

    // TODO: run these off of enums?
    public class LookupType : DataType<string>, IDeserializable
    {
        public LookupType(string name, Color color, IEnumerable<string> options, string guidance = null)
            : base(name, color, null, null, null, guidance)
        {
            Options = options.ToArray();
        }

        [JsonMapTo("options")]
        public string[] Options { get; }

        public object Deserialize(string value)
        {
            return value;
        }
    }
}
