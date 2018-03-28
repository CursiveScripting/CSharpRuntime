using System;
using System.Collections.Generic;
using System.Xml;

namespace Cursive
{
    internal class UserStepLoadingInfo
    {
        public UserStepLoadingInfo(UserStep step, XmlElement element)
        {
            Step = step;
            Element = element;
        }

        internal UserStep Step { get; }
        internal XmlElement Element { get; }

        internal Dictionary<string, string> FixedInputs { get; } = new Dictionary<string, string>();
        internal Dictionary<string, ValueKey> InputsToMap { get; } = new Dictionary<string, ValueKey>();
        internal Dictionary<string, ValueKey> OutputsToMap { get; } = new Dictionary<string, ValueKey>();
    }
}