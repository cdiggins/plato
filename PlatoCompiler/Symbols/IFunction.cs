using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler.Symbols
{
    public interface IFunction
    {
        string Name { get; }
        int NumParameters { get; }
        Expression Body { get; }
        string GetParameterName(int n);
        TypeExpression GetParameterType(int n);
        TypeExpression ReturnType { get; }
        TypeDefinition OwnerType { get; }
    }

    public static class FunctionExtensions
    {
        public static IReadOnlyList<(string, TypeExpression)> GetParameters(this IFunction self) 
            => Enumerable
                .Range(0, self.NumParameters)
                .Select(i => (self.GetParameterName(i), self.GetParameterType(i)))
                .ToList();

        public static string GetSignature(this IFunction self) =>
            $"{self.OwnerType}.{self.Name}("
            + string.Join(",", self.GetParameters().Select(p => $"{p.Item1}: {p.Item2}"))
            + $"): {self.ReturnType};";

        public static IEnumerable<TypeExpression> GetParameterTypes(this IFunction self)
            => GetParameters(self).Select(tuple => tuple.Item2);
    }
}