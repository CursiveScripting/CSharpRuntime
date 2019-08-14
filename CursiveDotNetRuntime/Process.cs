using Cursive.Debugging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public abstract class Process
    {
        protected Process(string name, string description, string folder)
        {
            Name = name;
            Description = description;
            Folder = folder;
        }
        
        [JsonProperty(PropertyName = "name", Order = 1)]
        public string Name { get; }

        [JsonProperty(PropertyName = "description", Order = 2)]
        public string Description { get; }

        [JsonProperty(PropertyName = "folder", Order = 3)]
        public string Folder { get; }

        [JsonProperty(PropertyName = "returnPaths", Order = 6)]
        public abstract IReadOnlyCollection<string> ReturnPaths { get; }

        [JsonProperty(PropertyName = "inputs", Order = 4)]
        public abstract IReadOnlyCollection<ValueKey> Inputs { get; }

        [JsonProperty(PropertyName = "outputs", Order = 5)]
        public abstract IReadOnlyCollection<ValueKey> Outputs { get; }

        internal abstract Task<Response> Run(ValueSet inputs, CallStack stack);
    }
}