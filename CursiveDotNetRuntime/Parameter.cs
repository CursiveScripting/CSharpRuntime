using Manatee.Json;
using Manatee.Json.Serialization;
using System;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]

namespace Cursive
{
    public class Parameter : IJsonSerializable
    {
        internal Parameter(string name, DataType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public DataType Type { get; }

        public void FromJson(JsonValue json, JsonSerializer serializer) => throw new NotImplementedException("Cannot deserialize a parameter without having data types available");

        public JsonValue ToJson(JsonSerializer serializer)
        {
            return new JsonObject
            {
                { "name", Name },
                { "type", Type.Name },
            };
        }
    }

    public class Parameter<T> : Parameter
    {
        public Parameter(string name, DataType<T> type)
            : base(name, type)
        { }
    }
}
