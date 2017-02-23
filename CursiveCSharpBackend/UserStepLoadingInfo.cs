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

        internal Dictionary<string, Parameter> InputsToMap { get; } = new Dictionary<string, Parameter>();
        internal Dictionary<string, Parameter> OutputsToMap { get; } = new Dictionary<string, Parameter>();
    }
}