using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public abstract class Step
    {
        protected Step(string id)
        {
            ID = id;
        }

        public abstract Task<Step> Run(CallStack stack);

        [JsonProperty(PropertyName = "id")]
        public string ID { get; }

        [JsonProperty(PropertyName = "inputs")]
        protected internal Dictionary<string, Variable> InputMapping { get; } = new Dictionary<string, Variable>();

        [JsonProperty(PropertyName = "outputs")]
        protected internal Dictionary<string, Variable> OutputMapping { get; } = new Dictionary<string, Variable>();
    }
}