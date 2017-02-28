using System.Collections.Generic;

namespace Cursive
{
    internal abstract class ReturningStep : Step
    {
        protected ReturningStep(string name)
            : base(name) { }

        internal Step DefaultReturnPath { get; set; }
    }
}