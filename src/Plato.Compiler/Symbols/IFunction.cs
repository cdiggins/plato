using System.Collections.Generic;
using System.Linq;

namespace Ara3D.Geometry.Compiler.Symbols
{
    public enum FunctionType
    {
        Intrinsic,
        Field,
        Constructor,
        Cast,
        Interface,
        Library,
        Lambda,
        // A synthesized per-case static factory of a sum type (wave-2, plato-232):
        // PathSegment2D.Move(p). Emitted as a static factory, called qualified.
        SumFactory,
    }

    public interface IFunction
    {
        string Name { get; }
        int NumParameters { get; }
        Symbol Body { get; }
        string GetParameterName(int n);
        TypeExpression GetParameterType(int n);
        TypeExpression ReturnType { get; }
        TypeDef OwnerType { get; }
        FunctionType FunctionType { get; }
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
            + $"): {self.ReturnType} [{self.FunctionType}];";

        public static IEnumerable<TypeExpression> GetParameterTypes(this IFunction self)
            => GetParameters(self).Select(tuple => tuple.Item2);
    }
}