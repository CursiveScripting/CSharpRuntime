﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursive
{
    public class Parameter
    {
        public Parameter(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; private set; }
        public Type Type { get; private set; }
    }
}