using System.Collections.Generic;

namespace Cursive
{
    public class RequiredProcess : Process
    {
        public RequiredProcess(string description, IReadOnlyCollection<Parameter> inputs, IReadOnlyCollection<Parameter> outputs, IReadOnlyCollection<string> returnPaths)
            : base(description)
        {
            Inputs = inputs;
            Outputs = outputs;
            ReturnPaths = returnPaths == null ? new string[] { null } : returnPaths;
        }
        
        internal Process ActualProcess { get; set; }
        public override IReadOnlyCollection<string> ReturnPaths { get; }
        public override IReadOnlyCollection<Parameter> Inputs { get; }
        public override IReadOnlyCollection<Parameter> Outputs { get; }

        public override string Run(Model inputs, out Model outputs)
        {
            return ActualProcess.Run(inputs, out outputs);
        }
    }
}