using Newtonsoft.Json;
using System.Collections.Generic;

namespace Cursive
{
    public abstract class Step
    {
        internal Step(string id)
        {
            ID = id;
        }

        [JsonProperty(PropertyName = "id")]
        public string ID { get; }

        [JsonIgnore]
        internal abstract StepType StepType { get; }

        [JsonProperty(PropertyName = "inputs")]
        protected internal Dictionary<string, Variable> InputMapping { get; } = new Dictionary<string, Variable>();

        [JsonProperty(PropertyName = "outputs")]
        protected internal Dictionary<string, Variable> OutputMapping { get; } = new Dictionary<string, Variable>();
    }

    internal enum StepType
    {
        Start,
        Stop,
        Process,
    }
}