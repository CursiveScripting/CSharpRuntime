using System;
using System.Collections.Generic;

namespace Cursive
{
    public abstract class Process
    {
        protected Process(string description, string folder)
        {
            Description = description;
            Folder = folder;
        }

        public string Run(ValueSet input)
        {
            ValueSet output;
            return Run(input, out output);
        }

        public string Description { get; }
        public string Folder { get; }

        public abstract IReadOnlyCollection<string> ReturnPaths { get; }
        public abstract IReadOnlyCollection<ValueKey> Inputs { get; }
        public abstract IReadOnlyCollection<ValueKey> Outputs { get; }

        public abstract string Run(ValueSet input, out ValueSet output);
    }
}