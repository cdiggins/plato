using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Minimal host surface for <see cref="TirInliner"/> so C# / C++ / CUDA (and later GLSL)
/// can share β-reduction without depending on the full C# emitter.
/// </summary>
public interface ITirInlineHost
{
    bool InlineCalls { get; }

    /// <summary>Optional diagnostic aggregator; null means off.</summary>
    InlineReport InlineReport { get; }

    Compiler.Compilation Compilation { get; }

    TirFunction TryGetGroundTirByTypeName(FunctionDef original, string concreteTypeName);

    TirFunction TryGetStaticTir(FunctionDef original);

    /// <summary>True when <paramref name="name"/> is a concrete emittable type (not an interface / type var).</summary>
    bool IsConcreteTypeName(string name);

    /// <summary>True for Plato scalar wrappers that erase (Number, Integer, Boolean, …).</summary>
    bool IsScalarPrimitiveName(string name);

    /// <summary>
    /// Known instance/static member names on <paramref name="typeName"/>, or null when unknown
    /// (treat as trusted — same as a missing C# extension plan).
    /// </summary>
    ISet<string> TryGetKnownMemberNames(string typeName);
}
