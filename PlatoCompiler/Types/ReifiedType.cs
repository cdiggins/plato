using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;
using Ptarmigan.Utils;

namespace Plato.Compiler.Types
{
    public class ReifiedFunction
    {
        public TypeDefinitionSymbol OwnerTypeSymbol { get; set; }
        public IReadOnlyList<TypeExpressionSymbol> ParameterTypes { get; set; }
        public TypeExpressionSymbol ReturnType { get; set; }
        public FunctionDefinition Symbol { get; set; }
        public ReifiedType OwnerType { get; set; }

        public void Verify()
        {
            ReturnType.VerifyIsReified();
            foreach (var p in ParameterTypes)
                p.VerifyIsReified();
        }
    }

    public class ReifiedType
    {
        public TypeDefinitionSymbol Symbol { get; }
        public IReadOnlyList<ReifiedFunction> Functions { get; }

        public ReifiedType(TypeDefinitionSymbol symbol)
        {
            // Iterate over all of the fields 
            symbol.IsConcreteType();
            Symbol = symbol;

            var functions = new List<ReifiedFunction>();
            foreach (var concept in Symbol.GetAllImplementedConcepts())
            {
                var conceptDef = concept.Definition;
                foreach (var f in conceptDef.Functions)
                {
                    functions.Add(CreateFunction(conceptDef, f));
                }
            }

            Functions = functions;
        }

        public ReifiedFunction CreateFunction(TypeDefinitionSymbol ownerTypeSymbol, FunctionDefinition functionDefinition)
        {
            var r = new ReifiedFunction()
            {
                OwnerType = this,
                OwnerTypeSymbol = ownerTypeSymbol,
                ParameterTypes = functionDefinition.Parameters.Select(p => Convert(p.Type)).ToList(),
                ReturnType = Convert(functionDefinition.ReturnType),
                Symbol = functionDefinition
            };
            r.Verify();
            return r;
        }

        public TypeExpressionSymbol Convert(TypeExpressionSymbol tes)
        {
            if (tes.Name == "Self")
                return Symbol.ToTypeExpression();
            return tes;
        }
    }

    public static class ReificationExtensions
    {
        public static void VerifyIsReified(this TypeExpressionSymbol tes)
        {
            tes.Definition.VerifyIsReified();
            foreach (var ta in tes.TypeArgs)
                ta.VerifyIsReified();
        }

        public static void VerifyIsReified(this TypeDefinitionSymbol tds)
        {
            Verifier.AssertNotNull(tds, "Type definition");
            Verifier.Assert(tds, d => d.IsConcreteType(), "IsConcreteType");
        }
    }
}