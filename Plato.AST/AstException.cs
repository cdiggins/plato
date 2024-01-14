using System;

namespace Plato.AST
{
    public class AstException : Exception
    {
        public AstNode Node { get; }

        public AstException(AstNode node, string msg)
            : base(msg)
        {
            Node = node;
        }
    }
}