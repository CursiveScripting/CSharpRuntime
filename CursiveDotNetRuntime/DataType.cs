using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cursive
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
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

        [JsonProperty(PropertyName = "name", Order = 1)]
        public string Name { get; }

        [JsonIgnore]
        public Color Color { get; }

        [JsonProperty(PropertyName = "color", Order = 2)]
        private string ColorCode => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

        [JsonProperty(PropertyName = "guidance", Order = 3)]
        public string Guidance { get; }

        [JsonIgnore]
        public DataType Extends { get; }

        [JsonProperty(PropertyName = "extends")]
        private string ExtendsName => Extends?.Name;

        [JsonIgnore]        
        public Regex Validation { get; }

        [JsonProperty(PropertyName = "validation")]
        private string ValidationPattern => Validation?.ToString();

        [JsonIgnore]
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
        public DataType(string name, Color color, DataType extends = null, Func<T> getDefault = null)
            : this(name, color, extends, getDefault, null) { }

        protected DataType(string name, Color color, DataType extends = null, Func<T> getDefault = null, Regex validation = null, string guidance = null)
            : base(name, color, extends, validation, guidance)
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

        [JsonProperty(PropertyName = "options")]
        public string[] Options { get; }

        public object Deserialize(string value)
        {
            return value;
        }
    }
}
