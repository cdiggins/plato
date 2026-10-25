// Checks resolved intrinsic provenance before an optional semantic rewrite.
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public static class TirIntrinsicIdentity
{
    public static bool IsIntrinsic(FunctionDef function, Compilation compilation, string name)
        => function != null && function.Name == name && function.Body == null
           && function.FunctionType == FunctionType.Intrinsic
           && compilation.LibraryDefinitionsByName.TryGetValue("Intrinsics", out var library)
           && ReferenceEquals(function.OwnerType, library)
           && library.Functions.Any(candidate => ReferenceEquals(candidate, function));
}
