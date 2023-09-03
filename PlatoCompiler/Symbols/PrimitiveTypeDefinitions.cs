using System;
using Plato.Compiler.Ast;

namespace Plato.Compiler.Symbols
{
    public static class PrimitiveTypeDefinitions
    {
        public static TypeDefinitionSymbol Lambda = Create("Lambda");
        public static TypeDefinitionSymbol Function = Create("Function");
        public static TypeDefinitionSymbol Self = Create("Self");
        public static TypeDefinitionSymbol Tuple = Create("Tuple");
        public static TypeDefinitionSymbol Error = Create("Error");

        public static TypeDefinitionSymbol Create(string name)
            => new TypeDefinitionSymbol(TypeKind.Primitive, name);

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