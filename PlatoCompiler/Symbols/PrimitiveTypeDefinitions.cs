using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Ast;

namespace Plato.Compiler.Symbols
{
    public static class PrimitiveTypeDefinitions
    {
        public static TypeDefinitionSymbol Self = Create("Self");
        public static TypeDefinitionSymbol Error = Create("Error");
        public static TypeDefinitionSymbol Tuple = Create("Tuple");
        public static TypeDefinitionSymbol Function = Create("Function");
        public static TypeDefinitionSymbol[] Tuples = Enumerable.Range(0, 9).Select(i => Create($"Tuple{i}", i)).ToArray();
        public static TypeDefinitionSymbol[] Functions = Enumerable.Range(0, 9).Select(i => Create($"Function{i}", i + 1)).ToArray();

        public static IEnumerable<TypeDefinitionSymbol> AllPrimitives =>
            Tuples.Concat(Functions).Concat(new[] { Self, Error, Tuple, Function });

        public static TypeDefinitionSymbol Create(string name)
            => new TypeDefinitionSymbol(TypeKind.Primitive, name);

        public static TypeDefinitionSymbol Create(string name, int numParams)
        {
            var r = Create(name);
            r.TypeParameters.AddRange(Enumerable.Range(0, numParams)
                .Select(i => new TypeParameterDefinition($"T{i}", null)));
            return r;
        }
            
        public static string GetPrimNameFromType(Type type)
        {
            if (type == null) return "Any";
            if (type.Equals(typeof(int))) return "Integer";
            if (type.Equals(typeof(float))) return "Number";
            if (type.Equals(typeof(double))) return "Number";
            if (type.Equals(typeof(bool))) return "Boolean";
            if (type.Equals(typeof(string))) return "String";
            throw new NotSupportedException(type.Name);
        }
    }
}