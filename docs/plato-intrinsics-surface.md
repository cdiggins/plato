# Plato intrinsic surface â€” every function a backend must supply

An **intrinsic** is a function the language declares but does not define: a signature
terminated by `;` with no `=>` body, inside a `library` block. The host runtime supplies the
implementation. This file is the complete contract a backend must satisfy â€” for C# that is the
handwritten `Plato.Intrinsics`; for a new backend (C++, GLSL, Rust) it is the porting
checklist.

**The rule: an intrinsic may mention only `primitive` types.** Number, Integer, Boolean,
Character, String, Array, Buffer, List, Function0-9, Dynamic, Type â€” the set declared in
`foundation/primitives.plato`. Anything else has a portable Plato body instead.

**139 intrinsics in one file** (stdlib as of 2026-07-30): `foundation/intrinsics.library.plato`
is the only file in the library that contains bodiless declarations, and it stays that way.
Every other bodiless signature in the stdlib sits inside a `concept`, where it is an obligation
on the implementer rather than a host contract.

The one documented exception is `Array2D` / `Array3D`, which appear below. They are declared as
opaque field-less types, so their construction and traversal cannot yet be expressed in Plato.
When they gain an honest layout (or a representation contract), those signatures move out too.

## What is NOT here any more

Until 2026-07-30 this file also listed 189 signatures on `Angle`, `Number2/3/4/8`,
`Vector2D/3D`, `Matrix3x2/4x4` and `Quaternion` â€” types that are ordinary `type` declarations,
not primitives. Those now have portable Plato **reference bodies**, which are the definition of
their semantics:

| Types | Reference bodies |
|---|---|
| `Angle` (trig, arithmetic) and the `Angle`-returning inverse trig on `Number` | `foundation/angle-trig.library.plato` |
| `Number2`, `Number3`, `Number4`, `Number8` | `foundation/vectors-tuples-ops.library.plato` |
| `Vector2D`, `Vector3D` | `foundation/vectors-geometric-ops.library.plato` |
| `Matrix3x2`, `Matrix4x4` | `foundation/matrices-ops.library.plato` |
| `Quaternion` | `foundation/rotations-ops.library.plato` |
| `CombineHash`, used by the per-type `Hash` bodies | `foundation/hashing.library.plato` |

A backend is free to substitute a verified native implementation (C# uses `System.Numerics`)
through its representation map and override table â€” that is an optimization, and the reference
body remains the semantics it must agree with. Backend-side reconciliation is `plato-368`.

Note the split this created in the trig surface. The scalar kernel is intrinsic and works in
radians (`Cos(self: Number): Number`); the inverse family carries a `Radians` suffix
(`AcosRadians`) because the public `Acos(self: Number): Angle` shares the receiver type and
differs only in return type. The `Angle`-typed surface is Plato, in `angle-trig.library.plato`.

Policy (from `intrinsics.library.plato`): a function may be declared intrinsic only if
every priority-1..4 backend â€” C#, C++, CUDA, TypeScript â€” can supply it natively or with a
trivial shim. Host-specific things (C# SIMD types, IEEE `nextafter`-grade functions,
midpoint-rounding variants) are deliberately excluded.

This list is GENERATED. Regenerate rather than hand-edit, so it cannot drift:

```bash
grep -rEc "^\s+[A-Za-z_][A-Za-z0-9_]*\(.*\)\s*:\s*[^;=]+;\s*$" stdlib/*/*.library.plato | grep -v ":0$"
```

## `foundation/intrinsics.library.plato` (139)

**Number**

- `Abs(self: Number): Number`
- `Cbrt(self: Number): Number`
- `Ceiling(self: Number): Number`
- `Clamp(self: Number, min: Number, max: Number): Number`
- `CopySign(self: Number, y: Number): Number`
- `Exp(self: Number): Number`
- `Floor(self: Number): Number`
- `FusedMultiplyAdd(self: Number, y: Number, z: Number): Number`
- `Log(self: Number, newBase: Number): Number`
- `Log10(self: Number): Number`
- `Log2(self: Number): Number`
- `Min(self: Number, other: Number): Number`
- `Max(self: Number, other: Number): Number`
- `NaturalLog(self: Number): Number`
- `Pow(self: Number, power: Number): Number`
- `Reciprocal(self: Number): Number`
- `Round(self: Number, digits: Integer): Number`
- `Sign(self: Number): Integer`
- `Sqrt(self: Number): Number`
- `Square(self: Number): Number`
- `Truncate(self: Number): Number`
- `Cos(self: Number): Number`
- `Sin(self: Number): Number`
- `Tan(self: Number): Number`
- `Cosh(self: Number): Number`
- `Sinh(self: Number): Number`
- `Tanh(self: Number): Number`
- `AcosRadians(self: Number): Number`
- `AcoshRadians(self: Number): Number`
- `AsinRadians(self: Number): Number`
- `AsinhRadians(self: Number): Number`
- `AtanRadians(self: Number): Number`
- `Atan2Radians(self: Number, x: Number): Number`
- `AtanhRadians(self: Number): Number`
- `Add(a: Number, b: Number): Number`
- `Subtract(a: Number, b: Number): Number`
- `Multiply(a: Number, b: Number): Number`
- `Divide(a: Number, b: Number): Number`
- `Modulo(a: Number, b: Number): Number`
- `Negative(n: Number): Number`
- `Equals(a: Number, b: Number): Boolean`
- `NotEquals(a: Number, b: Number): Boolean`
- `LessThan(a: Number, b: Number): Boolean`
- `LessThanOrEquals(a: Number, b: Number): Boolean`
- `GreaterThan(a: Number, b: Number): Boolean`
- `GreaterThanOrEquals(a: Number, b: Number): Boolean`
- `Compare(a: Number, b: Number): Integer`
- `Hash(self: Number): Integer`
- `MinValue(_: Number): Number`
- `MaxValue(_: Number): Number`
- `Epsilon(_: Number): Number`
- `Zero(_: Number): Number`
- `One(_: Number): Number`
- `Pi(_: Number): Number`
- `Tau(_: Number): Number`
- `E(_: Number): Number`
- `IsNaN(self: Number): Boolean`
- `IsInfinite(self: Number): Boolean`
- `IsFinite(self: Number): Boolean`
- `ToInteger(self: Number): Integer`

**Integer**

- `Abs(self: Integer): Integer`
- `Add(a: Integer, b: Integer): Integer`
- `Subtract(a: Integer, b: Integer): Integer`
- `Multiply(a: Integer, b: Integer): Integer`
- `Divide(a: Integer, b: Integer): Integer`
- `Modulo(a: Integer, b: Integer): Integer`
- `Negative(n: Integer): Integer`
- `Sign(self: Integer): Integer`
- `Min(a: Integer, b: Integer): Integer`
- `Max(a: Integer, b: Integer): Integer`
- `Equals(a: Integer, b: Integer): Boolean`
- `NotEquals(a: Integer, b: Integer): Boolean`
- `LessThan(a: Integer, b: Integer): Boolean`
- `LessThanOrEquals(a: Integer, b: Integer): Boolean`
- `GreaterThan(a: Integer, b: Integer): Boolean`
- `GreaterThanOrEquals(a: Integer, b: Integer): Boolean`
- `Compare(a: Integer, b: Integer): Integer`
- `Hash(self: Integer): Integer`
- `BitwiseAnd(a: Integer, b: Integer): Integer`
- `BitwiseOr(a: Integer, b: Integer): Integer`
- `BitwiseXor(a: Integer, b: Integer): Integer`
- `BitwiseNot(x: Integer): Integer`
- `ShiftLeft(x: Integer, bits: Integer): Integer`
- `ShiftRight(x: Integer, bits: Integer): Integer`
- `MinValue(_: Integer): Integer`
- `MaxValue(_: Integer): Integer`
- `Zero(_: Integer): Integer`
- `One(_: Integer): Integer`
- `ToNumber(self: Integer): Number`
- `Range(self: Integer): Array<Integer>`

**Boolean**

- `And(a: Boolean, b: Boolean): Boolean`
- `Or(a: Boolean, b: Boolean): Boolean`
- `Not(b: Boolean): Boolean`
- `ExclusiveOr(a: Boolean, b: Boolean): Boolean`
- `Equals(a: Boolean, b: Boolean): Boolean`
- `Compare(a: Boolean, b: Boolean): Integer`
- `Hash(self: Boolean): Integer`

**Array**

- `All(xs: Array<$T>, f: Function1<$T, Boolean>): Boolean`
- `Any(xs: Array<$T>, f: Function1<$T, Boolean>): Boolean`
- `Append(self: Array<$T>, value: $T): Array<$T>`
- `AtModulo(xs: Array<$T>, n: Integer): $T`
- `CartesianProduct(columns: Array<$TColumn>, rows: Array<$TRow>, func: Function2<$TColumn, $TRow, $TResult>): Array2D<$TResult>`
- `Concatenate(xs: Array<$T>, ys: Array<$T>): Array<$T>`
- `Drop(xs: Array<$T>, n: Integer): Array<$T>`
- `EveryNth(self: Array<$T>, n: Integer): Array<$T>`
- `FlatMap(xs: Array<$T1>, f: Function1<$T1, Array<$T2>>): Array<$T2>`
- `MakeArray2D(columns: Integer, rows: Integer, f: Function2<Integer, Integer, $T>): Array2D<$T>`
- `Map(xs: Array<$T1>, f: Function1<$T1, $T2>): Array<$T2>`
- `MapPairs(xs: Array<$T>, f: Function2<$T, $T, $TR>): Array<$TR>`
- `MapTriplets(xs: Array<$T>, f: Function3<$T, $T, $T, $TR>): Array<$TR>`
- `MapQuartets(xs: Array<$T>, f: Function4<$T, $T, $T, $T, $TR>): Array<$TR>`
- `MapRange(n: Integer, f: Function1<Integer, $T>): Array<$T>`
- `Prepend(self: Array<$T>, value: $T): Array<$T>`
- `Reduce(xs: Array<$T>, acc: $U, f: Function2<$U, $T, $U>): $U`
- `Repeat(n: Integer, x: $T): Array<$T>`
- `Skip(xs: Array<$T>, n: Integer): Array<$T>`
- `Slice(xs: Array<$T>, from: Integer, to: Integer): Array<$T>`
- `Slices(xs: Array<$T>, size: Integer): Array2D<$T>`
- `SubArray(xs: Array<$T>, from: Integer, count: Integer): Array<$T>`
- `Take(xs: Array<$T>, n: Integer): Array<$T>`
- `TakeLast(xs: Array<$T>, n: Integer): Array<$T>`
- `Zip(xs: Array<$T1>, ys: Array<$T2>, f: Function2<$T1, $T2, $T3>): Array<$T3>`
- `Zip(xs: Array<$T1>, ys: Array<$T2>, zs: Array<$T3>, f: Function3<$T1, $T2, $T3, $T4>): Array<$T4>`

**Array2D and Array3D — the documented exception to the primitive-only rule**

- `At(xs: Array2D<$T>, i: Integer): $T`
- `At(xs: Array3D<$T>, i: Integer): $T`
- `Column(self: Array2D<$T>, col: Integer): Array<$T>`
- `Row(self: Array2D<$T>, row: Integer): Array<$T>`
- `Map(xs: Array2D<$T1>, f: Function1<$T1, $T2>): Array2D<$T2>`

**Growable builder: unique type List<T>.**

- `Count(xs: List<$T>): Integer`
- `At(xs: List<$T>, n: Integer): $T`
- `Add(xs: List<$T>, x: $T): List<$T>`
- `AddRange(xs: List<$T>, values: Array<$T>): List<$T>`
- `Set(xs: List<$T>, i: Integer, x: $T): List<$T>`
- `Freeze(xs: List<$T>): Array<$T>`
- `EmptyList(xs: Array<$T>): List<$T>`

**Fixed-size builder: unique type Buffer<T>.**

- `Count(xs: Buffer<$T>): Integer`
- `At(xs: Buffer<$T>, n: Integer): $T`
- `Set(xs: Buffer<$T>, i: Integer, x: $T): Buffer<$T>`
- `Freeze(xs: Buffer<$T>): Array<$T>`


