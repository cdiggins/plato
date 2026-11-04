// How a constant expression is cached in generated C++ (C# --cache-constants; TKT-1183).
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Plato is pure, so a closed expression has one value. C# keeps it in a static readonly field of a holder class.
    /// C++ keeps it in a function-local <c>static const</c>, which the language initialises on the first evaluation
    /// (thread-safely), so the value is computed when the first call would have computed it, never earlier, and no
    /// namespace-scope initialisation order exists to get wrong.
    /// </summary>
    public interface IConstantCachePolicy
    {
        bool Enabled { get; }

        /// <summary>Whether evaluating the closed expression costs more than the guard that reads a static: it calls, to a
        /// bounded depth, a function that iterates. Arithmetic and record construction on literals are the compiler's to
        /// fold, and a static would stop it.</summary>
        bool Worthwhile(Ara3D.Geometry.Compiler.Checking.TirNode expression, System.Func<Ara3D.Geometry.Compiler.Symbols.FunctionDef, Ara3D.Geometry.Compiler.Checking.TirFunction> bodyOf);

        /// <summary>Text before and after a function body that is a closed expression: <c>{ static const T v = ...; return v; }</c>.</summary>
        (string Open, string Close) WholeBody(string cppType);

        /// <summary>Text before and after a closed call inside a larger expression: an immediately called lambda that returns the static.</summary>
        (string Open, string Close) Site(string cppType);
    }
}
