using System;

namespace Ara3D.Geometry.AST
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