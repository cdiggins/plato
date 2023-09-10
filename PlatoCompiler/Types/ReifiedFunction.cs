using System.Collections.Generic;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class ReifiedFunction
    {
        public TypeDefinitionSymbol OwnerTypeSymbol { get; set; }
        public IReadOnlyList<TypeExpressionSymbol> ParameterTypes { get; set; }
        public TypeExpressionSymbol ReturnType { get; set; }
        public FunctionDefinition Symbol { get; set; }
        public ReifiedType ReifiedType { get; set; }
        public string Name => Symbol.Name;

        public void Verify()
        {
            ReturnType.VerifyIsReified();
            foreach (var p in ParameterTypes)
                p.VerifyIsReified();
        }

        public override string ToString()
        {
            var paramStr = string.Join(",", ParameterTypes);
            return $"{Name}:({paramStr}) -> {ReturnType}";
        }

        public override bool Equals(object obj)
        {
            return obj is ReifiedFunction rf
                   && ReferenceEquals(ReifiedType, rf.ReifiedType)
                   && ToString() == rf.ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}