// Provides the Bytes primitive: an immutable view of a byte array, and its host operations.
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// An immutable view of a byte array: the array, a start and a count. Text whose characters are
    /// bytes (a STEP file is ASCII) without a UTF-16 copy and without a wrapper call per element:
    /// <see cref="At"/> is a range test and an array read, and <see cref="SubBytes"/> makes a view of
    /// the same array. The default value is the empty view. A host wraps a <c>byte[]</c> with
    /// <see cref="Wrap(byte[])"/> without copying it, and must not change the array while a view of
    /// it is alive (the view is immutable only as far as nobody writes the array).
    /// The Plato-visible members are the intrinsics of <c>stdlib/foundation/intrinsics.library.plato</c>:
    /// <c>Count</c>, <c>At</c>, <c>SubBytes</c>, <c>FromIntegers</c> (<see cref="BytesIntrinsics"/>), the
    /// comparison operators, <c>Hash</c> and <c>NumberFromBytes</c>.
    /// </summary>
    public readonly partial struct Bytes
    {
        // -------------------------------------------------------------------------------
        // Fields: a byte array and the range of it that this view shows. Always in range.
        // -------------------------------------------------------------------------------

        private readonly byte[] _array;
        private readonly int _start;
        private readonly int _count;

        // -------------------------------------------------------------------------------
        // Host entry
        // -------------------------------------------------------------------------------

        private Bytes(byte[] array, int start, int count)
            => (_array, _start, _count) = (array, start, count);

        /// <summary>The whole array as a Bytes, not copied. A null array is the empty view.</summary>
        [MethodImpl(AggressiveInlining)]
        public static Bytes Wrap(byte[] array)
            => array == null ? default : new Bytes(array, 0, array.Length);

        /// <summary>The bytes <c>array[start .. start + count - 1]</c> as a Bytes, not copied. A range outside the array throws.</summary>
        public static Bytes Wrap(byte[] array, int start, int count)
        {
            if (array == null)
            {
                if (start != 0 || count != 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
                return default;
            }
            if (start < 0 || count < 0 || start > array.Length - count)
                throw new ArgumentOutOfRangeException(nameof(count), "the range is outside the array");
            return new Bytes(array, start, count);
        }

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Bytes(byte[] array) => Wrap(array);

        /// <summary>The viewed bytes, for a host that reads them directly.</summary>
        [MethodImpl(AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan()
            => _array == null ? default : new ReadOnlySpan<byte>(_array, _start, _count);

        /// <summary>A new array holding the viewed bytes.</summary>
        public byte[] ToArray() => AsSpan().ToArray();

        // -------------------------------------------------------------------------------
        // The kernel
        // -------------------------------------------------------------------------------

        // Property, not method: `Count` is the `Countable` interface member (see String.Count).
        public Integer Count { [MethodImpl(AggressiveInlining)] get => _count; }

        /// <summary>The byte at <c>n</c>, 0 to 255. <c>n</c> outside the view throws: the check is against the
        /// view's own count, so a view never reads the bytes of the array beyond its range.</summary>
        [MethodImpl(AggressiveInlining)]
        public Integer At(Integer n)
        {
            var i = n.Value;
            if ((ulong)i >= (ulong)_count)
                ThrowIndex(i, _count);
            return _array[_start + (int)i];
        }

        public Integer this[Integer n] { [MethodImpl(AggressiveInlining)] get => At(n); }

        // Not NoInlining: the JIT then sees that the callee never returns and moves the call to a cold block after the
        // loop; NoInlining hides that and leaves the call in the loop body (measured, DOTNET_JitDisasm).
        [System.Diagnostics.CodeAnalysis.DoesNotReturn]
        private static void ThrowIndex(IntegerValue i, int count)
            => throw new IndexOutOfRangeException($"Bytes.At: index {i} is outside 0..{count - 1}");

        /// <summary>The bytes at the indices <c>start</c> to <c>start + count - 1</c> that exist, as a view of the same
        /// array: clamped to the view at both ends, so a count of zero or less, or a range wholly outside, gives the
        /// empty view. Never fails and never copies.</summary>
        [MethodImpl(AggressiveInlining)]
        public Bytes SubBytes(Integer start, Integer count)
        {
            long first = start.Value;
            long n = count.Value;
            if (first < 0)
            {
                n += first;
                first = 0;
            }
            if (n <= 0 || first >= _count)
                return default;
            if (n > _count - first)
                n = _count - first;
            return new Bytes(_array, _start + (int)first, (int)n);
        }

        // -------------------------------------------------------------------------------
        // Equality and order: byte by byte, ordinal, a prefix before its extension.
        // -------------------------------------------------------------------------------

        // Equals, NotEquals, == and != are generated over SequenceEquals (the generated half of the partial
        // struct spells them with the Plato Boolean); the order operators are here, as for String.
        public bool SequenceEquals(Bytes other) => AsSpan().SequenceEqual(other.AsSpan());
        public override int GetHashCode() => (int)Fnv1a(AsSpan());

        public static bool operator <=(Bytes a, Bytes b) => a.AsSpan().SequenceCompareTo(b.AsSpan()) <= 0;
        public static bool operator >=(Bytes a, Bytes b) => a.AsSpan().SequenceCompareTo(b.AsSpan()) >= 0;
        public static bool operator <(Bytes a, Bytes b) => a.AsSpan().SequenceCompareTo(b.AsSpan()) < 0;
        public static bool operator >(Bytes a, Bytes b) => a.AsSpan().SequenceCompareTo(b.AsSpan()) > 0;

        // -------------------------------------------------------------------------------
        // Hash and number
        // -------------------------------------------------------------------------------

        /// <summary>The 32-bit FNV-1a hash: the unsigned value in a 64-bit Integer, the same 32 bits read as
        /// signed in a 32-bit one. (<c>Hash</c> on Bytes in the contract.)</summary>
        public Integer Hash() => (IntegerValue)Fnv1a(AsSpan());

        internal static uint Fnv1a(ReadOnlySpan<byte> bytes)
        {
            var h = 2166136261u;
            foreach (var b in bytes)
                h = (h ^ b) * 16777619u;
            return h;
        }

        /// <summary>The correctly rounded Number the bytes spell, parsed in one pass in the invariant culture; NaN when
        /// the bytes are not in the P21 real forms or the value rounds to infinity. (<c>NumberFromBytes</c> in the contract.)</summary>
        public Number NumberFromBytes()
            => ByteForms.TryParseNumber(AsSpan(), out var v) ? v : NumberValue.NaN;

        public override string ToString()
            => System.Text.Encoding.Latin1.GetString(AsSpan());
    }

    /// <summary>The text forms the byte parse accepts: the grammar of <c>TextForms</c> over bytes.</summary>
    internal static class ByteForms
    {
        private static bool IsDigit(byte c) => c >= (byte)'0' && c <= (byte)'9';

        /// <summary>Sign, digits with an optional point, optional exponent; nothing else.</summary>
        internal static bool IsRealForm(ReadOnlySpan<byte> t)
        {
            var i = 0;
            var n = t.Length;
            if (i < n && (t[i] == (byte)'+' || t[i] == (byte)'-'))
                i++;
            var digits = 0;
            while (i < n && IsDigit(t[i])) { i++; digits++; }
            if (i < n && t[i] == (byte)'.')
            {
                i++;
                while (i < n && IsDigit(t[i])) { i++; digits++; }
            }
            if (digits == 0)
                return false;
            if (i < n && (t[i] == (byte)'e' || t[i] == (byte)'E'))
            {
                i++;
                if (i < n && (t[i] == (byte)'+' || t[i] == (byte)'-'))
                    i++;
                var exponentDigits = 0;
                while (i < n && IsDigit(t[i])) { i++; exponentDigits++; }
                if (exponentDigits == 0)
                    return false;
            }
            return i == n;
        }

        /// <summary>The value of a real-form range, false when the form is wrong or the value overflows to infinity.</summary>
        internal static bool TryParseNumber(ReadOnlySpan<byte> t, out NumberValue value)
        {
            value = 0;
            return IsRealForm(t)
                && NumberValue.TryParse(t, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value)
                && NumberValue.IsFinite(value);
        }
    }

    /// <summary>The Bytes member that takes an Array: the host half of <c>FromIntegers</c>.
    /// Array is <c>IReadOnlyList</c> in the C# runtime, so the receiver is the list, as for MapRange.</summary>
    public static class BytesIntrinsics
    {
        /// <summary>A Bytes over a new array holding the integers in order, each reduced to its low 8 bits.</summary>
        public static Bytes FromIntegers(this IReadOnlyList<Integer> xs)
        {
            var bytes = new byte[xs.Count];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)xs[i].Value;
            return Bytes.Wrap(bytes);
        }
    }
}
