using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public abstract class Process
    {
        protected Process(
            string name,
            string description,
            string folder,
            IReadOnlyCollection<Parameter> inputs,
            IReadOnlyCollection<Parameter> outputs,
            IReadOnlyCollection<string> returnPaths
        )
        {
            Name = name;
            Description = description;
            Folder = folder;

            Inputs = inputs;
            Outputs = outputs;
            ReturnPaths = returnPaths;
        }
        
        [JsonProperty(PropertyName = "name", Order = 1)]
        public string Name { get; }

        [JsonProperty(PropertyName = "description", Order = 2)]
        public string Description { get; }

        [JsonProperty(PropertyName = "folder", Order = 3)]
        public string Folder { get; }

        [JsonProperty(PropertyName = "inputs", Order = 4)]
        public IReadOnlyCollection<Parameter> Inputs { get; }

        [JsonProperty(PropertyName = "outputs", Order = 5)]
        public IReadOnlyCollection<Parameter> Outputs { get; }

        [JsonProperty(PropertyName = "returnPaths", Order = 6)]
        public IReadOnlyCollection<string> ReturnPaths { get; }

        internal abstract Task<ProcessResult> Run(ValueSet inputs, CallStack stack);
    }
}