using Manatee.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
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
        
        [JsonMapTo("name")]
        public string Name { get; }

        [JsonMapTo("description")]
        public string Description { get; }

        [JsonMapTo("folder")]
        public string Folder { get; }

        [JsonMapTo("inputs")]
        public IReadOnlyList<Parameter> Inputs { get; }

        [JsonMapTo("outputs")]
        public IReadOnlyList<Parameter> Outputs { get; }

        [JsonMapTo("returnPaths")]
        public IReadOnlyList<string> ReturnPaths { get; }

        internal abstract Task<ProcessResult> Run(ValueSet inputs, CallStack stack);
    }
}