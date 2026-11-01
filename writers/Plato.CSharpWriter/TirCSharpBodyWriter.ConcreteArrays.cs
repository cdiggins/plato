// Spells array Count and element reads for --concrete-arrays (TKT-1093).
using System.Collections.Generic;
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

    /// <summary>The C# for a list's element count, as an <c>int</c>. A Heap, Stack or Borrowed receiver is a
    /// <c>T[]</c> or a span and says <c>.Length</c>, with no type test (--owned-arrays).</summary>
    private string ListCount(string list, ArrayStorage storage = ArrayStorage.List)
        => storage != ArrayStorage.List ? $"{list}.Length"
            : _tw.Writer.ConcreteArrays ? $"{CSharpWriter.ConcreteArrayAccessClass}.Count({list})" : $"{list}.Count";

    /// <summary>The C# for a list's element at an <c>int</c> index. A Heap, Stack or Borrowed receiver is indexed
    /// directly: an index out of range throws <c>IndexOutOfRangeException</c> where the interface throws
    /// <c>ArgumentOutOfRangeException</c> (plan open question 1).</summary>
    private string ListAt(string list, string index, ArrayStorage storage = ArrayStorage.List)
        => storage != ArrayStorage.List ? $"{list}[{index}]"
            : _tw.Writer.ConcreteArrays ? $"{CSharpWriter.ConcreteArrayAccessClass}.At({list}, {index})" : $"{list}[{index}]";

    /// <summary>The storage of the array a node produces, as the storage plan recorded it (List without a plan).
    /// A coercion between two array types prints as its inner node, so it has the inner node's storage.</summary>
    private ArrayStorage StorageOf(TirNode node)
        => _plan == null || node == null ? ArrayStorage.List
            : node is TirCoerce c && TirRewrite.IsListType(c.Type) && TirRewrite.IsListType(TirRewrite.StripCoerce(c.Inner)?.Type)
                ? StorageOf(c.Inner)
                : _plan.Of(node);

    // The C# element type each lowered array loop declared its temporary with, and each Heap alias local of one, for
    // the declaration of the alias and the cast of a Represent.
    private readonly Dictionary<string, string> _tempElementTypes = new(System.StringComparer.Ordinal);
    private readonly Dictionary<Ara3D.Geometry.Compiler.Symbols.VariableDef, string> _aliasElementTypes = new();

    /// <summary>The C# element type of the Heap array a node names (a lowered-loop temporary or a Heap alias local
    /// of one), or null when the node is neither.</summary>
    private string HeapElementType(TirNode node)
        => TirRewrite.StripCoerce(node) switch
        {
            TirTempRef t when _tempElementTypes.TryGetValue(t.Name, out var named) => named,
            TirVariable { Def: { } def } when _aliasElementTypes.TryGetValue(def, out var aliased) => aliased,
            _ => null,
        };

    /// <summary>Records <paramref name="def"/> as a Heap alias with the given element type and returns its declared type.</summary>
    private string RecordHeapAlias(Ara3D.Geometry.Compiler.Symbols.VariableDef def, string element)
    {
        _aliasElementTypes[def] = element;
        return $"{element}[]";
    }

    /// <summary>Prints a <see cref="TirRepresent"/> as the conversion the table of the plan names: Heap to List is a
    /// cast to the interface, Heap to Borrowed wraps the array, Stack to Borrowed casts the span.</summary>
    private void WriteRepresent(TirRepresent r)
    {
        var elem = HeapElementType(r.Inner) ?? CSharpTypeName(TirRewrite.ListElementType(r.Type));
        var (open, close) = (r.From, r.To) switch
        {
            (ArrayStorage.Heap, ArrayStorage.List) => ($"((IReadOnlyList<{elem}>)", ")"),
            (ArrayStorage.Heap, ArrayStorage.Borrowed) => ($"new System.ReadOnlySpan<{elem}>(", ")"),
            (ArrayStorage.Stack, ArrayStorage.Borrowed) => ($"((System.ReadOnlySpan<{elem}>)", ")"),
            _ => throw new System.InvalidOperationException($"no conversion from {r.From} to {r.To}"),
        };
        Write(open);
        WriteNode(r.Inner);
        Write(close);
    }

    /// <summary>Whether a node's emitted C# type is IReadOnlyList: a Plato Array that is not a
    /// struct coerced to one (a struct would be boxed by the helper's interface parameter).</summary>
    private static bool IsInterfaceList(TirNode node)
        => TirRewrite.IsListType(node?.Type) && TirRewrite.IsListType(TirRewrite.StripCoerce(node)?.Type);
}
