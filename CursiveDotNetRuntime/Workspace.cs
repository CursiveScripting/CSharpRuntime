using Newtonsoft.Json;
using System.Collections.Generic;

namespace Cursive
{
    public class Workspace
    {
        [JsonProperty(PropertyName = "types")]
        public List<DataType> Types { get; } = new List<DataType>();

        [JsonProperty(PropertyName = "requiredProcesses")]
        public List<RequiredProcess> RequiredProcesses { get; } = new List<RequiredProcess>();

        [JsonProperty(PropertyName = "systemProcesses")]
        public List<SystemProcess> SystemProcesses { get; } = new List<SystemProcess>();

        [JsonIgnore]
        internal List<UserProcess> UserProcesses { get; } = new List<UserProcess>();
    }
}
