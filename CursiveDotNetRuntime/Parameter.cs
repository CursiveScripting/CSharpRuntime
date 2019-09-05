using Manatee.Json.Serialization;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]

namespace Cursive
{
    public class Parameter
    {
        internal Parameter(string name, DataType type)
        {
            Name = name;
            Type = type;
        }

        [JsonMapTo("name")]
        public string Name { get; }

        [JsonIgnore]
        public DataType Type { get; }

        [JsonMapTo("type")]
        private string TypeName => Type.Name;
    }

    public class Parameter<T> : Parameter
    {
        public Parameter(string name, DataType<T> type)
            : base(name, type)
        { }
    }
}
