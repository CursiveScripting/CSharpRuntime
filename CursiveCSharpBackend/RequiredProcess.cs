using CursiveCSharpBackend.Services;
using System.Collections.Generic;

namespace Cursive
{
    public class RequiredProcess : Process
    {
        public RequiredProcess(string description, IReadOnlyCollection<ValueKey> inputs, IReadOnlyCollection<ValueKey> outputs, IReadOnlyCollection<string> returnPaths)
            : base(description)
        {
            Inputs = inputs;
            Outputs = outputs;
            ReturnPaths = returnPaths;
        }
        
        internal Process ActualProcess { get; set; }
        public override IReadOnlyCollection<string> ReturnPaths { get; }
        public override IReadOnlyCollection<ValueKey> Inputs { get; }
        public override IReadOnlyCollection<ValueKey> Outputs { get; }

        public override string Run(ValueSet inputs, out ValueSet outputs)
        {
            DebuggingService.StartNewCall(this);
            return ActualProcess.Run(inputs, out outputs);
        }
    }
}