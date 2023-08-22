using System;
using Plato.Compiler.Ast;

namespace Plato.Compiler.Symbols
{
    public static class PrimitiveTypeDefinitions
    {
        public static TypeDefinition Lambda = Create("Lambda");
        public static TypeDefinition Function = Create("Function");
        public static TypeDefinition Self = Create("Self");
        public static TypeDefinition Tuple = Create("Tuple");
        public static TypeDefinition Error = Create("Error");

        public static TypeDefinition Create(string name)
            => new TypeDefinition(TypeKind.Primitive, name);

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