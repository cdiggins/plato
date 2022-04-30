using System;
using System.Collections.Generic;
using System.Text;

namespace PlatoIR
{
    public class IRBuilder
    {
        public Dictionary<int, IR> Lookup { get; } = new Dictionary<int, IR>();
        public SourceLocation Source { get; }
    }
}
