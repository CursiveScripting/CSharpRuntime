using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class Parameter
    {
        public Parameter(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; private set; }
        public string Type { get; private set; }
    }
}
