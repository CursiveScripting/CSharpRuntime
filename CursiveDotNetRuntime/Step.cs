using Manatee.Json.Serialization;
using System.Collections.Generic;

namespace Cursive
{
    public abstract class Step
    {
        internal Step(string id)
        {
            ID = id;
        }

        [JsonMapTo("id")]
        public string ID { get; }

        [JsonIgnore]
        internal abstract StepType StepType { get; }

        [JsonMapTo("inputs")]
        protected internal Dictionary<string, Variable> InputMapping { get; } = new Dictionary<string, Variable>();

        [JsonMapTo("outputs")]
        protected internal Dictionary<string, Variable> OutputMapping { get; } = new Dictionary<string, Variable>();
    }

    internal enum StepType
    {
        Start,
        Stop,
        Process,
    }
}