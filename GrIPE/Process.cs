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
        protected Process(Workspace workspace, string name, string description)
        {
            this.Workspace = workspace;
            this.Name = name;
            this.Description = description;
        }

        public string Run(Model input)
        {
            Model output;
            return Run(input, out output);
        }

        public Workspace Workspace { get; private set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        public abstract string Run(Model input, out Model output);
        
        public abstract ReadOnlyCollection<string> ReturnPaths { get; }

        public abstract ReadOnlyCollection<Parameter> Inputs { get; }

        public abstract ReadOnlyCollection<Parameter> Outputs { get; }
    }
}