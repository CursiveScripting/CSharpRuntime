using Cursive.Debugging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public abstract class Process
    {
        protected Process(string name, string description, string folder)
        {
            Name = name;
            Description = description;
            Folder = folder;
        }
        
        public string Name { get; }
        public string Description { get; }
        public string Folder { get; }

        public abstract IReadOnlyCollection<string> ReturnPaths { get; }
        public abstract IReadOnlyCollection<ValueKey> Inputs { get; }
        public abstract IReadOnlyCollection<ValueKey> Outputs { get; }

        internal abstract Task<Response> Run(ValueSet inputs, CallStack stack);
    }
}