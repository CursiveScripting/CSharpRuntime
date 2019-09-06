using Manatee.Json;
using Manatee.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cursive
{
    public abstract class Process : IJsonSerializable
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
        
        public string Name { get; }

        public string Description { get; }

        public string Folder { get; }

        public IReadOnlyList<Parameter> Inputs { get; }

        public IReadOnlyList<Parameter> Outputs { get; }

        public IReadOnlyList<string> ReturnPaths { get; }

        public void FromJson(JsonValue json, JsonSerializer serializer) => throw new NotImplementedException("Cannot directly deserialize processes");

        public JsonValue ToJson(JsonSerializer serializer)
        {
            var output = new JsonObject
            {
                { "name", Name },
            };

            if (!string.IsNullOrEmpty(Description))
                output.Add("description", Description);

            if (Folder != null)
                output.Add("folder", Folder);

            if (Inputs != null && Inputs.Count > 0)
                output.Add("inputs", Inputs.Select(i => i.ToJson(serializer)).ToJson());

            if (Outputs != null && Outputs.Count > 0)
                output.Add("outputs", Outputs.Select(o => o.ToJson(serializer)).ToJson());

            if (ReturnPaths != null && ReturnPaths.Count > 0)
                output.Add("returnPaths", ReturnPaths.ToJson());

            return output;
        }

        internal abstract Task<ProcessResult> Run(ValueSet inputs, CallStack stack);
    }
}