using Cursive.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public class Workspace
    {
        [JsonProperty(PropertyName = "types")]
        public IList<DataType> Types { get; set; } = new List<DataType>();

        [JsonProperty(PropertyName = "requiredProcesses")]
        public IList<RequiredProcess> RequiredProcesses { get; set; } = new List<RequiredProcess>();

        [JsonProperty(PropertyName = "systemProcesses")]
        public IList<SystemProcess> SystemProcesses { get; set; } = new List<SystemProcess>();

        [JsonIgnore]
        internal List<UserProcess> UserProcesses { get; } = new List<UserProcess>();

        public IList<string> LoadUserProcesses(string processJson)
        {
            return ProcessLoadingService.LoadProcesses(this, processJson);
        }
    }
}
