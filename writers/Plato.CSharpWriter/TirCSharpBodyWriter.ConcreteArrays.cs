// Spells array Count and element reads for --concrete-arrays (TKT-1093).
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    // An Array<T> value is an IReadOnlyList<T> in the emitted C#, and almost every one is a T[] at
    // run time (MapRange, MakeArray, every lowered loop, the host's tables). The JIT cannot
    // devirtualize an array's IReadOnlyList Count and indexer, so each is an interface call into
    // SZArrayHelper. With --concrete-arrays the reads go through ConcreteArrayAccess, which tests
    // for an exact T[] (one method-table compare the JIT folds where it knows the type) and reads
    // the array directly; any other list, and an index out of range, takes the interface path, so
    // values and exceptions are unchanged. Off, the spelling is the interface's own.

    /// <summary>The C# for a list's element count, as an <c>int</c>.</summary>
    private string ListCount(string list)
        => _tw.Writer.ConcreteArrays ? $"{CSharpWriter.ConcreteArrayAccessClass}.Count({list})" : $"{list}.Count";

    /// <summary>The C# for a list's element at an <c>int</c> index.</summary>
    private string ListAt(string list, string index)
        => _tw.Writer.ConcreteArrays ? $"{CSharpWriter.ConcreteArrayAccessClass}.At({list}, {index})" : $"{list}[{index}]";

    /// <summary>Whether a node's emitted C# type is IReadOnlyList: a Plato Array that is not a
    /// struct coerced to one (a struct would be boxed by the helper's interface parameter).</summary>
    private static bool IsInterfaceList(TirNode node)
        => TirRewrite.IsListType(node?.Type) && TirRewrite.IsListType(TirRewrite.StripCoerce(node)?.Type);
}
