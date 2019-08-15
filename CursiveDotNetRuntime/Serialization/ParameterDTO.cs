using Newtonsoft.Json;

namespace Cursive.Serialization
{
    internal class ParameterDTO
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}
