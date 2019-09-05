using Manatee.Json.Serialization;

namespace Cursive.Serialization
{
    internal class VariableDTO : ParameterDTO
    {
        [JsonMapTo("x")]
        public int X { get; set; }

        [JsonMapTo("y")]
        public int Y { get; set; }

        [JsonMapTo("initialValue")]
        public string InitialValue { get; set; }
    }
}
