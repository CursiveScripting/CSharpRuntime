﻿using Cursive.Debugging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursive
{
    public class SystemProcess : Process
    {
        public SystemProcess(string name, string description, SystemStep operation, IReadOnlyCollection<ValueKey> inputs, IReadOnlyCollection<ValueKey> outputs, IReadOnlyCollection<string> returnPaths, string folder = null)
            : base(name, description, folder)
        {
            Operation = operation;
            Inputs = inputs;
            Outputs = outputs;
            ReturnPaths = returnPaths;
        }

        public delegate Task<Response> SystemStep(ValueSet input);

        private SystemStep Operation { get; }
        public override IReadOnlyCollection<string> ReturnPaths { get; }
        public override IReadOnlyCollection<ValueKey> Inputs { get; }
        public override IReadOnlyCollection<ValueKey> Outputs { get; }

        internal override async Task<Response> Run(ValueSet inputs, CallStack stack = null)
        {
            try
            {
                return await Operation(inputs);
            }
            catch (Exception e)
            {
                throw new CursiveRunException(stack, $"An error occurred running system process ${Name}", e);
            }
        }
    }
}