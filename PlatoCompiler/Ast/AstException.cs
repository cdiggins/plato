using System;

namespace Plato.Compiler.Ast
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