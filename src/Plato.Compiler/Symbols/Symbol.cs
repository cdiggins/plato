using System;
using System.Collections.Generic;
using System.Threading;

namespace Ara3D.Geometry.Compiler.Symbols
{
    /// <summary>
    /// A symbol is either a definition, an expression, a statement, a type definition, or a type expression.
    /// Symbols are different from AST in that they are resolved. 
    /// </summary>
    public abstract class Symbol
    {
        // Unique per process, also when several compilations run at once (the warm law server
        // compiles one program per folder set, concurrently): a plain NextId++ handed two symbols
        // the same Id, and since equality is by Id, an expression took another symbol for itself
        // ("Circular expression"). A long, so a server that compiles for days never wraps.
        public long Id { get; } = Interlocked.Increment(ref _nextId);
        private static long _nextId;

        public override bool Equals(object obj) 
            => ReferenceEquals(this, obj) || (obj is Symbol sym && sym.Id == Id);
        
        public override int GetHashCode() 
            => Id.GetHashCode();

        public abstract IEnumerable<Symbol> GetChildSymbols();

        public abstract string Name { get; }

        public abstract Symbol Rewrite(Func<Symbol, Symbol> f);
    }
}