// How an array value is held in the emitted C#: the closed set the storage plan chooses from.
namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// The representation of one array value in the C# a body becomes (--owned-arrays, TKT-1152).
/// Chosen by <see cref="TirStorageResolver"/>, recorded in a <see cref="StoragePlan"/>, verified by
/// <see cref="StorageChecker"/> and spelled by the body writer, which decides nothing itself.
/// </summary>
public enum ArrayStorage
{
    /// <summary><c>IReadOnlyList&lt;T&gt;</c>: what every array is today. The default for anything the plan does not name.</summary>
    List,
    /// <summary>An owned <c>T[]</c> the body allocated with its own element type.</summary>
    Heap,
    /// <summary><c>Span&lt;T&gt;</c> over a hoisted <c>stackalloc</c> slot, with a heap fallback past the bound.</summary>
    Stack,
    /// <summary><c>ReadOnlySpan&lt;T&gt;</c>: a parameter of a span twin, or a local copied from one.</summary>
    Borrowed,
}
