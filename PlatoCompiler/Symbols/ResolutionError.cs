using Plato.AST;

namespace Plato.Compiler.Symbols
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