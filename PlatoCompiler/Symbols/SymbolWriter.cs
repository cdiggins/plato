using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;

namespace Plato.Compiler.Symbols
{
    public abstract class SymbolWriter<T> : CodeBuilder<T> where T : SymbolWriter<T>
    {
        public Compiler Compiler { get; }

        protected SymbolWriter(Compiler compiler)
        {
            Compiler = compiler;
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
                case DefinitionSymbol definition:
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
        public abstract T Write(DefinitionSymbol definition);
    }
}