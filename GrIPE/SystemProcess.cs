using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class SystemProcess<Model> : Process<Model>
    {
        public SystemProcess(Func<Model, string> operation, params string[] possibleOutputs)
        {
            Operation = operation;
            PossibleOutputs = possibleOutputs;
        }

        private Func<Model, string> Operation;
        private string[] PossibleOutputs;

        public override string Run(Model model)
        {
            return Operation(model);
        }

        public override string[] GetPossibleOutputs()
        {
            return PossibleOutputs;
        }
    }
}