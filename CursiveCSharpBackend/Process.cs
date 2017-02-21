using System;
using System.Collections.Generic;

namespace Cursive
{
    public abstract class Process
    {
        protected Process(string description)
        {
            this.Description = description;
        }

        public string Run(Model input)
        {
            Model output;
            return Run(input, out output);
        }

        public string Description { get; protected set; }
        public abstract IReadOnlyCollection<string> ReturnPaths { get; }
        public abstract IReadOnlyCollection<Parameter> Inputs { get; }
        public abstract IReadOnlyCollection<Parameter> Outputs { get; }

        public abstract string Run(Model input, out Model output);
    }
}