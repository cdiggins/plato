using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class ReifiedType
    {
        public TypeDefinition Type { get; }
        public TypeExpression Self => Type.ToTypeExpression();
        public HashSet<ReifiedFunction> Functions { get; } = new HashSet<ReifiedFunction>();
        public string Name => Type.Name;
        
        public ReifiedType(TypeDefinition type)
        {
            Verifier.AssertNotNull(type, nameof(type));
            Verifier.Assert(type.IsConcrete(), "Is concrete type");
            Type = type;

            // Add functions generated from field
            foreach (var field in type.Fields)
            {
                Functions.Add(CreateFunction(Type, field.Function, x => x.Name == "Self" ? Self : x));    
            }

            Verifier.AssertEquals(type.Methods.Count, 0);

            // Add functions for each concept 
            foreach (var concept in Type.GetAllImplementedConcepts())
            {
                var conceptDef = concept.Definition;

                var _typeArgs = new Dictionary<TypeParameterDefinition, TypeExpression>();

                // TODO: this should be a compilation error. 
                Verifier.AssertEquals(concept.TypeArgs.Count, conceptDef.TypeParameters.Count);

                for (var i = 0; i < concept.TypeArgs.Count; i++)
                {
                    var typeArg = concept.TypeArgs[i];
                    var typeParam = conceptDef.TypeParameters[i];
                    _typeArgs.Add(typeParam, typeArg);
                }

                TypeExpression LocalMapType(TypeExpression tes)
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

        public ReifiedFunction CreateFunction(TypeDefinition ownerType, FunctionDefinition functionDefinition, Func<TypeExpression, TypeExpression> map)
        {
            var r = new ReifiedFunction(functionDefinition, this,
                functionDefinition.Parameters.Select(p => p.Type.Replace(map)).ToList(),
                functionDefinition.ReturnType.Replace(map));

            //r.Verify();
            return r;
        }

        public void AddConceptLibraryFunction(TypeDefinition library, FunctionDefinition functionDefinition)
        {
            Verifier.Assert(functionDefinition.Parameters.Count > 0);
            var pt = functionDefinition.Parameters[0].Type;
            Verifier.Assert(pt.Definition.IsConcept());
            Verifier.Assert(library.IsLibrary());

            TypeExpression LocalMapType(TypeExpression te)
                => te.Equals(pt) ? Type.ToTypeExpression() : te;

            var r = CreateFunction(library, functionDefinition, LocalMapType);
            Functions.Add(r);
        }

        public void AddConcreteTypeLibraryFunction(TypeDefinition library, FunctionDefinition functionDefinition)
        {
            Verifier.Assert(functionDefinition.Parameters.Count > 0);
            var pt = functionDefinition.Parameters[0].Type;
            Verifier.Assert(pt.Definition.IsConcrete());
            Verifier.Assert(library.IsLibrary());

            var r = CreateFunction(library, functionDefinition, x => x);
            Functions.Add(r);
        }
    }

    public static class ReificationExtensions
    {
        public static void VerifyIsReified(this TypeExpression tes)
        {
            tes.Definition.VerifyIsReified();
            foreach (var ta in tes.TypeArgs)
                ta.VerifyIsReified();
        }

        public static void VerifyIsReified(this TypeDefinition tds)
        {
            Verifier.AssertNotNull(tds, "Type definition");
            Verifier.Assert(tds, d => !(d is TypeParameterDefinition), "Not is type parameter definition");
            Verifier.Assert(tds, d => d.IsConcrete(), "Is concrete type");
        }

        public static TypeExpression Replace(this TypeExpression self,
            Func<TypeExpression, TypeExpression> map)
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
                : new TypeExpression(self.Definition, args);
        }
    }
}