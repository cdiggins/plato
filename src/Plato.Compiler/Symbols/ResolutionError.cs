// Carries source locations and messages for symbol resolution failures.
using Ara3D.Geometry.AST;

namespace Ara3D.Geometry.Compiler.Symbols
{
    public class ResolutionError
    {
        public AstNode Node { get; }
        public string Message { get; }

        public ResolutionError(string message, AstNode node)
        {
            Node = node;
            Message = message;
        }
    }
}