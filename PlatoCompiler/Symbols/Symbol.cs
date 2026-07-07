using System;
using System.Collections.Generic;

namespace Ara3D.Geometry.Compiler.Symbols
{
    /// <summary>
    /// A symbol is either a definition, an expression, a statement, a type definition, or a type expression.
    /// Symbols are different from AST in that they are resolved. 
    /// </summary>
    public abstract class Symbol
    {
        public int Id { get; } = NextId++;
        public static int NextId = 0;

        public override bool Equals(object obj) 
            => ReferenceEquals(this, obj) || (obj is Symbol sym && sym.Id == Id);
        
        public override int GetHashCode() 
            => Id.GetHashCode();

        public abstract IEnumerable<Symbol> GetChildSymbols();

        public abstract string Name { get; }

        public abstract Symbol Rewrite(Func<Symbol, Symbol> f);
    }
}