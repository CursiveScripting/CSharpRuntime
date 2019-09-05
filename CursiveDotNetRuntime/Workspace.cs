using Cursive.Serialization;
using Manatee.Json.Serialization;
using System.Collections.Generic;

namespace Cursive
{
    public class Workspace
    {
        [JsonMapTo("types")]
        public IList<DataType> Types { get; set; } = new List<DataType>();

        [JsonMapTo("requiredProcesses")]
        public IList<RequiredProcess> RequiredProcesses { get; set; } = new List<RequiredProcess>();

        [JsonMapTo("systemProcesses")]
        public IList<SystemProcess> SystemProcesses { get; set; } = new List<SystemProcess>();

        [JsonIgnore]
        internal List<UserProcess> UserProcesses { get; } = new List<UserProcess>();

        public bool LoadUserProcesses(string processJson, bool validateSchema, out List<string> errors)
        {
            return ProcessLoadingService.LoadProcesses(this, processJson, validateSchema, out errors);
        }

        public string GetWorkspaceJson(bool prettyPrint = false)
        {
            var serializer = new JsonSerializer();

            var jsonData = serializer.Serialize(this);

            return prettyPrint
                ? jsonData.GetIndentedString()
                : jsonData.ToString();
        }

        public string GetProcessJson(bool prettyPrint = false)
        {
            var serializer = new JsonSerializer();

            var jsonData = serializer.Serialize(UserProcesses);

            return prettyPrint
                ? jsonData.GetIndentedString()
                : jsonData.ToString();
        }
    }
}
