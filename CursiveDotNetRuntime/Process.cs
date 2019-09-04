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
            IReadOnlyList<Parameter> inputs,
            IReadOnlyList<Parameter> outputs,
            IReadOnlyList<string> returnPaths
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
        public IReadOnlyList<Parameter> Inputs { get; }

        [JsonProperty(PropertyName = "outputs", Order = 5)]
        public IReadOnlyList<Parameter> Outputs { get; }

        [JsonProperty(PropertyName = "returnPaths", Order = 6)]
        public IReadOnlyList<string> ReturnPaths { get; }

        internal abstract Task<ProcessResult> Run(ValueSet inputs, CallStack stack);
    }
}