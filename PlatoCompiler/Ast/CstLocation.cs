using Ara3D.Parsing.CST;
using Parakeet;

namespace Plato.Compiler.Ast
{
    public class CstLocation : ILocation
    {
        public CstNode Node { get; }
        public CstLocation(CstNode node)
            => Node = node;
    }
}