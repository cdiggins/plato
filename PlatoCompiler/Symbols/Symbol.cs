using System.Collections.Generic;

namespace Plato.Compiler.Symbols
{
    /// <summary>
    /// A symbol is either a definition, an expression, a type definition, or a type expression. 
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
    }
}