using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public abstract class Process
    {
        protected Process(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public string Run(Model input)
        {
            Model output;
            return Run(input, out output);
        }

        public string Name { get; protected set; }
        public string Description { get; protected set; }

        public abstract string Run(Model input, out Model output);
        
        public abstract ReadOnlyCollection<string> ReturnPaths { get; }

        public abstract ReadOnlyCollection<string> Inputs { get; }

        public abstract ReadOnlyCollection<string> Outputs { get; }
    }
}