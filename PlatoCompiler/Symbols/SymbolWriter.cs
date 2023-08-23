using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;
using Plato.Compiler.Types;

namespace Plato.Compiler.Symbols
{
    public abstract class SymbolWriter<T> : CodeBuilder<T> where T : SymbolWriter<T>
    {
        public TypeFactory Factory { get; }

        protected SymbolWriter(TypeFactory factory)
        {
            Factory = factory;
        }

        public T Write(IEnumerable<Symbol> symbols)
            => symbols.Aggregate((T)this, (writer, symbol) => writer.Write(symbol));

        public T WriteCommaList(IEnumerable<Symbol> symbols)
        {
            var r = (T)this;
            var first = true;
            foreach (var s in symbols)
            {
                if (!first)
                    r = r.Write(", ");
                first = false;
                r = r.Write(s);
            }

            return r;
        }


        public T Write(Symbol symbol)
        {
            switch (symbol)
            {
                case Definition definition:
                    return Write(definition);
                case Expression expression:
                    return Write(expression);
                case TypeDefinition typeDefinition:
                    return Write(typeDefinition);
                case TypeExpression typeExpression:
                    return Write(typeExpression);
                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol));
            }
        }

        public abstract T Write(TypeExpression typeExpression);
        public abstract T Write(Expression expr);
        public abstract T Write(TypeDefinition typeDefinition);
        public abstract T Write(Definition definition);
    }
}