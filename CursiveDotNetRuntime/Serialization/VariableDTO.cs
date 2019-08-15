using Newtonsoft.Json;

namespace Cursive.Serialization
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class VariableDTO : ParameterDTO
    {
        [JsonProperty(PropertyName = "x")]
        public int X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public int Y { get; set; }

        [JsonProperty(PropertyName = "initialValue")]
        public string InitialValue { get; set; }
    }
}
