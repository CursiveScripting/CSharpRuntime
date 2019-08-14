using Cursive.Debugging;
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

        public abstract Task<Step> Run(ValueSet variables, CallStack stack);

        [JsonProperty(PropertyName = "id")]
        public string ID { get; }

        [JsonProperty(PropertyName = "inputs")]
        protected internal Dictionary<ValueKey, ValueKey> InputMapping { get; } = new Dictionary<ValueKey, ValueKey>();

        [JsonProperty(PropertyName = "outputs")]
        protected internal Dictionary<ValueKey, ValueKey> OutputMapping { get; } = new Dictionary<ValueKey, ValueKey>();
    }
}