using Newtonsoft.Json;
using System.Collections.Generic;

namespace Cursive.Serialization
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class UserProcessDTO
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "folder")]
        public string Folder { get; set; }

        [JsonProperty(PropertyName = "inputs")]
        public List<ParameterDTO> Inputs { get; set; }

        [JsonProperty(PropertyName = "outputs")]
        public List<ParameterDTO> Outputs { get; set; }

        [JsonProperty(PropertyName = "returnPaths")]
        public List<string> ReturnPaths { get; set; }

        [JsonProperty(PropertyName = "variables")]
        public List<VariableDTO> Variables { get; set; }

        [JsonProperty(PropertyName = "steps")]
        public List<StepDTO> Steps { get; set; }
    }
}
