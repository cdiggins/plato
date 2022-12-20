using System;
using System.Collections.Generic;
using System.Text;

namespace PlatoAnalyzer
{
    public class PlatoDebugWriter
    {
        public StringBuilder sb;

        public PlatoDebugWriter(StringBuilder sb)
        {
            this.sb = sb;
        }

        public void Output(PlatoSyntaxNode node)
        {
            switch (node)
            {
                case PlatoExpression _:
                    break;                
                default:
                    break;
            }
        }
    }
}
