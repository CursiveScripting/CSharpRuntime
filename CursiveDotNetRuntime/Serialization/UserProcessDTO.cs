using Manatee.Json.Serialization;
using System.Collections.Generic;

namespace Cursive.Serialization
{
    internal class UserProcessDTO
    {
        [JsonMapTo("name")]
        public string Name { get; set; }

        [JsonMapTo("description")]
        public string Description { get; set; }

        [JsonMapTo("folder")]
        public string Folder { get; set; }

        [JsonMapTo("inputs")]
        public List<ParameterDTO> Inputs { get; set; }

        [JsonMapTo("outputs")]
        public List<ParameterDTO> Outputs { get; set; }

        [JsonMapTo("returnPaths")]
        public List<string> ReturnPaths { get; set; }

        [JsonMapTo("variables")]
        public List<VariableDTO> Variables { get; set; }

        [JsonMapTo("steps")]
        public List<StepDTO> Steps { get; set; }
    }
}
