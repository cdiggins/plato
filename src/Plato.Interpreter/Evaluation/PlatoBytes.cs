// The interpreter's value for the Bytes primitive: an immutable view of a byte array.
using System.Globalization;
using System.Text;

namespace Ara3D.Geometry.Interpreter;

/// <summary>An immutable view of a byte array: the array, a start and a count, as the C# runtime's
/// <c>Ara3D.Geometry.Bytes</c> is. Slicing shares the array. Equal when the viewed bytes are equal,
/// whatever the array or start; ordered ordinally. The Plato-visible members are the intrinsics
/// <c>Count</c>, <c>At</c>, <c>SubBytes</c>, <c>FromIntegers</c>, <c>LessThanOrEquals</c>, <c>Hash</c> and
/// <c>NumberFromBytes</c> (<c>stdlib/foundation/intrinsics.library.plato</c>).</summary>
public sealed class PlatoBytes : IEquatable<PlatoBytes>
{
    public static readonly PlatoBytes Empty = new PlatoBytes(Array.Empty<byte>(), 0, 0);

    private readonly byte[] _array;
    private readonly int _start;

    public int Count { get; }

    /// <summary>A view of <paramref name="array"/> from <paramref name="start"/>, <paramref name="count"/> bytes; the array is shared, not copied.</summary>
    public PlatoBytes(byte[] array, int start, int count)
    {
        if (array == null || start < 0 || count < 0 || start > array.Length - count)
            throw new ArgumentOutOfRangeException(nameof(count), "the range is outside the array");
        (_array, _start, Count) = (array, start, count);
    }

    public ReadOnlySpan<byte> Span => new ReadOnlySpan<byte>(_array, _start, Count);

    /// <summary>The byte at <paramref name="n"/>; a fault outside the view, never a read of the array beyond it.</summary>
    public byte At(long n)
    {
        if ((ulong)n >= (ulong)Count)
            throw new EvaluationException($"Bytes.At: index {n} is outside 0..{Count - 1}");
        return _array[_start + (int)n];
    }

    /// <summary>The bytes at start to start + count - 1 that exist, a view of the same array.</summary>
    public PlatoBytes Sub(long start, long count)
    {
        if (start < 0)
        {
            count += start;
            start = 0;
        }
        if (count <= 0 || start >= Count)
            return Empty;
        if (count > Count - start)
            count = Count - start;
        return new PlatoBytes(_array, _start + (int)start, (int)count);
    }

    /// <summary>The 32-bit FNV-1a hash of the bytes as an unsigned value.</summary>
    public uint Fnv1a()
    {
        var h = 2166136261u;
        foreach (var b in Span)
            h = (h ^ b) * 16777619u;
        return h;
    }

    public bool Equals(PlatoBytes other) => other != null && Span.SequenceEqual(other.Span);
    public override bool Equals(object obj) => obj is PlatoBytes o && Equals(o);
    public override int GetHashCode() => (int)Fnv1a();

    /// <summary>The bytes as Latin-1 text between b" and ", for a report.</summary>
    public override string ToString()
    {
        var sb = new StringBuilder("b\"");
        foreach (var b in Span)
            sb.Append(b >= 32 && b < 127 && b != 34 && b != 92 ? (char)b : '.');
        return sb.Append('"').ToString();
    }
}
