using Newtonsoft.Json;
using System.Collections.Generic;

namespace Cursive.Serialization
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class StepDTO
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "x")]
        public int X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public int Y { get; set; }

        [JsonProperty(PropertyName = "process")]
        public string InnerProcess { get; set; }

        [JsonProperty(PropertyName = "inputs")]
        public Dictionary<string, string> Inputs { get; set; }

        [JsonProperty(PropertyName = "outputs")]
        public Dictionary<string, string> Outputs { get; set; }

        [JsonProperty(PropertyName = "returnPaths")]
        public Dictionary<string, string> ReturnPaths { get; set; }

        [JsonProperty(PropertyName = "returnPath")]
        public string ReturnPath { get; set; }
    }
}
