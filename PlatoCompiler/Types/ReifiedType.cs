using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;
using Ptarmigan.Utils;

namespace Plato.Compiler.Types
{
    public class ReifiedType
    {
        public TypeDefinitionSymbol TypeSymbol { get; }
        public TypeExpressionSymbol Self => TypeSymbol.ToTypeExpression();
        public HashSet<ReifiedFunction> Functions { get; } = new HashSet<ReifiedFunction>();
        public string Name => TypeSymbol.Name;
        
        public ReifiedType(TypeDefinitionSymbol typeSymbol)
        {
            Verifier.AssertNotNull(typeSymbol, nameof(typeSymbol));
            Verifier.Assert(typeSymbol.IsConcreteType(), "Is concrete type");
            TypeSymbol = typeSymbol;

            // Add functions generated from field
            foreach (var field in typeSymbol.Fields)
            {
                Functions.Add(CreateFunction(TypeSymbol, field.Function, x => x.Name == "Self" ? Self : x));    
            }

            Verifier.AssertEquals(typeSymbol.Methods.Count, 0);

            // Add functions for each concept 
            foreach (var concept in TypeSymbol.GetAllImplementedConcepts())
            {
                var conceptDef = concept.Definition;

                var _typeArgs = new Dictionary<TypeParameterDefinition, TypeExpressionSymbol>();

                // TODO: this should be a compilation error. 
                Verifier.AssertEquals(concept.TypeArgs.Count, conceptDef.TypeParameters.Count);

                for (var i = 0; i < concept.TypeArgs.Count; i++)
                {
                    var typeArg = concept.TypeArgs[i];
                    var typeParam = conceptDef.TypeParameters[i];
                    _typeArgs.Add(typeParam, typeArg);
                }

                TypeExpressionSymbol LocalMapType(TypeExpressionSymbol tes)
                {
                    if (tes.Name == "Self")
                        return Self;
                    if (tes.Definition is TypeParameterDefinition tpd)
                    {
                        if (_typeArgs.ContainsKey(tpd))
                            return _typeArgs[tpd];

                        // NOTE: this is possible in generic types like TupleX and Array1. 
                        //throw new Exception($"Did not find type parameter {tpd} in local type arguments");
                    }
                    return tes;
                }

                foreach (var m in conceptDef.Methods)
                {
                    Functions.Add(CreateFunction(conceptDef, m.Function, LocalMapType));
                }
            }

            // TODO: add library functions 
            // Multiple passes. 
        }

        public ReifiedFunction CreateFunction(TypeDefinitionSymbol ownerTypeSymbol, FunctionDefinition functionDefinition, Func<TypeExpressionSymbol, TypeExpressionSymbol> map)
        {
            var r = new ReifiedFunction(functionDefinition, this,
                functionDefinition.Parameters.Select(p => p.Type.Replace(map)).ToList(),
                functionDefinition.ReturnType.Replace(map));

            //r.Verify();
            return r;
        }

        public void AddConceptLibraryFunction(TypeDefinitionSymbol library, FunctionDefinition functionDefinition)
        {
            Verifier.Assert(functionDefinition.Parameters.Count > 0);
            var pt = functionDefinition.Parameters[0].Type;
            Verifier.Assert(pt.Definition.IsConcept());
            Verifier.Assert(library.IsLibrary());

            TypeExpressionSymbol LocalMapType(TypeExpressionSymbol te)
                => te.Equals(pt) ? TypeSymbol.ToTypeExpression() : te;

            var r = CreateFunction(library, functionDefinition, LocalMapType);
            Functions.Add(r);
        }

        public void AddConcreteTypeLibraryFunction(TypeDefinitionSymbol library, FunctionDefinition functionDefinition)
        {
            Verifier.Assert(functionDefinition.Parameters.Count > 0);
            var pt = functionDefinition.Parameters[0].Type;
            Verifier.Assert(pt.Definition.IsConcreteType());
            Verifier.Assert(library.IsLibrary());

            var r = CreateFunction(library, functionDefinition, x => x);
            Functions.Add(r);
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
            Verifier.Assert(tds, d => !(d is TypeParameterDefinition), "Not is type parameter definition");
            Verifier.Assert(tds, d => d.IsConcreteType(), "Is concrete type");
        }

        public static TypeExpressionSymbol Replace(this TypeExpressionSymbol self,
            Func<TypeExpressionSymbol, TypeExpressionSymbol> map)
        {
            var r = map(self);

            // If the map returned a new symbol, we return it. 
            if (!ReferenceEquals(self, r))
                return r;

            // If there are no type arguments we just return the original symbol
            if (self.TypeArgs.Count == 0)
                return self;

            // We are going to have to recursively replace the type arguments 
            var args = self.TypeArgs.Select(ta => ta.Replace(map)).ToArray();

            // If nothing changed, just return the original type. 
            return args.Zip(self.TypeArgs, ReferenceEquals).All(b => b) 
                ? self 
                : new TypeExpressionSymbol(self.Definition, args);
        }
    }
}