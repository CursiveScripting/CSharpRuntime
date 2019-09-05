using Manatee.Json.Serialization;

namespace Cursive.Serialization
{
    internal class ParameterDTO
    {
        [JsonMapTo("name")]
        public string Name { get; set; }

        [JsonMapTo("type")]
        public string Type { get; set; }
    }
}
