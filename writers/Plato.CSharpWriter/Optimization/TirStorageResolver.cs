// Chooses the storage of each array binding of one body (--owned-arrays).
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Pass 7 of the pipeline: runs at the body writer's entry, after its own capture rewrite, and wraps the body in a
/// <see cref="TirStorageScope"/>. This chunk assigns List to every binding (the plan is empty), so the printed body
/// is unchanged; later chunks add Heap, Stack and Borrowed choices and the <see cref="TirRepresent"/> nodes they
/// need. <see cref="StorageChecker"/> verifies the result before anything prints.
/// </summary>
public static class TirStorageResolver
{
    /// <summary>The body is wrapped as it is: the very same node, not a copy.</summary>
    public static TirStorageScope Resolve(TirNode body, FunctionDef owner)
        => new(body, new StoragePlan(owner));
}
