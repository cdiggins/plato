using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Ast;

namespace Plato.Compiler.Symbols
{
    public static class PrimitiveTypeDefinitions
    {
        public static TypeDefinition Error = Create("Error");
        public static TypeDefinition Tuple = Create("Tuple");
        public static TypeDefinition Function = Create("Function");

        public static IEnumerable<TypeDefinition> AllPrimitives =>
            new[] { Error, Tuple, Function };

        public static TypeDefinition Create(string name)
            => new TypeDefinition(TypeKind.Primitive, name);

        public static TypeDefinition Create(string name, int numParams)
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