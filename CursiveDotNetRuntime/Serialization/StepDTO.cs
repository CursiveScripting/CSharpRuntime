using Manatee.Json.Serialization;
using System.Collections.Generic;

namespace Cursive.Serialization
{
    internal class StepDTO
    {
        [JsonMapTo("id")]
        public string ID { get; set; }

        [JsonMapTo("type")]
        public string Type { get; set; }

        [JsonMapTo("x")]
        public int X { get; set; }

        [JsonMapTo("y")]
        public int Y { get; set; }

        [JsonMapTo("process")]
        public string InnerProcess { get; set; }

        [JsonMapTo("name")]
        public string PathName { get; set; }

        [JsonMapTo("inputs")]
        public Dictionary<string, string> Inputs { get; set; }

        [JsonMapTo("outputs")]
        public Dictionary<string, string> Outputs { get; set; }

        [JsonMapTo("returnPaths")]
        public Dictionary<string, string> ReturnPaths { get; set; }

        [JsonMapTo("returnPath")]
        public string ReturnPath { get; set; }
    }
}
