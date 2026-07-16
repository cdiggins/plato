// Minimal scalar wrapper structs. Under --scalar=float the generated code erases
// Number/Integer/Boolean to float/int/bool but still emits wrapper-cast bridges
// (`((Number)a) + ((Number)b)`) that the full library satisfies via Plato.Intrinsics.V2.
// These tiny stand-ins provide exactly the conversions and operators those bridges use.

using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    public readonly partial struct Number
    {
        public readonly float Value;
        [MethodImpl(AggressiveInlining)] public Number(float value) => Value = value;
        [MethodImpl(AggressiveInlining)] public static implicit operator Number(float f) => new(f);
        [MethodImpl(AggressiveInlining)] public static implicit operator float(Number n) => n.Value;
        [MethodImpl(AggressiveInlining)] public static Number operator +(Number a, Number b) => new(a.Value + b.Value);
        [MethodImpl(AggressiveInlining)] public static Number operator -(Number a, Number b) => new(a.Value - b.Value);
        [MethodImpl(AggressiveInlining)] public static Number operator *(Number a, Number b) => new(a.Value * b.Value);
        [MethodImpl(AggressiveInlining)] public static Number operator /(Number a, Number b) => new(a.Value / b.Value);
        [MethodImpl(AggressiveInlining)] public static Number operator -(Number x) => new(-x.Value);
        [MethodImpl(AggressiveInlining)] public static bool operator <(Number a, Number b) => a.Value < b.Value;
        [MethodImpl(AggressiveInlining)] public static bool operator >(Number a, Number b) => a.Value > b.Value;
    }

    public readonly partial struct Integer
    {
        public readonly int Value;
        [MethodImpl(AggressiveInlining)] public Integer(int value) => Value = value;
        [MethodImpl(AggressiveInlining)] public static implicit operator Integer(int i) => new(i);
        [MethodImpl(AggressiveInlining)] public static implicit operator int(Integer n) => n.Value;
        [MethodImpl(AggressiveInlining)] public static Integer operator +(Integer a, Integer b) => new(a.Value + b.Value);
        [MethodImpl(AggressiveInlining)] public static bool operator <(Integer a, Integer b) => a.Value < b.Value;
        [MethodImpl(AggressiveInlining)] public static bool operator >(Integer a, Integer b) => a.Value > b.Value;
    }

    public readonly partial struct Boolean
    {
        public readonly bool Value;
        [MethodImpl(AggressiveInlining)] public Boolean(bool value) => Value = value;
        [MethodImpl(AggressiveInlining)] public static implicit operator Boolean(bool b) => new(b);
        [MethodImpl(AggressiveInlining)] public static implicit operator bool(Boolean b) => b.Value;
    }
}
