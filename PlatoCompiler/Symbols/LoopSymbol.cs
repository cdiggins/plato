using System.Collections.Generic;
using System.Linq;
using Plato.AST;

namespace Plato.Compiler.Symbols
{
    public class LoopSymbol : Symbol
    {
        public LoopSymbol(AstNode node)
        { }

        public override IEnumerable<Symbol> GetChildSymbols()
            => Enumerable.Empty<Symbol>();
    }
}