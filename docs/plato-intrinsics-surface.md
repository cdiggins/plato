# Plato intrinsic surface — every function a backend must supply

An **intrinsic** is a function the language declares but does not define: a signature
terminated by `;` with no `=>` body, inside a `library` block. The host runtime supplies the
implementation. This file is the complete contract a backend must satisfy — for C# that is the
handwritten `Plato.Intrinsics`; for a new backend (C++, GLSL, Rust) it is the porting
checklist.

**Rule 1: an intrinsic may mention only `primitive` types.** Number, Integer, Boolean,
Character, String, Array, Buffer, List, Function0-9, Type — the set declared in
`foundation/primitives.plato`. Anything else has a portable Plato body instead.

**Rule 2 (plato-378): an intrinsic must not be expressible in Plato from the other
intrinsics.** If a portable reference body exists, the function belongs in a `*.library.plato`
file and a backend recovers native speed through its override table (plato-368), not by
re-adding a bodiless declaration. Before adding one, write the body; if the body compiles,
that is your answer.

**65 intrinsics in one file** (stdlib as of 2026-07-31): `foundation/intrinsics.library.plato`
is the only file in the library that contains bodiless declarations, and it stays that way.
Every other bodiless signature in the stdlib sits inside a `concept`, where it is an obligation
on the implementer rather than a host contract.

There is no longer an exception to rule 1. `Array2D` / `Array3D` were the last one — opaque,
field-less, so their construction and traversal could not be written in Plato. They now declare
a flat `Elements: Array<T>` store plus extents named for the obligations they discharge
(`ColumnCount` / `RowCount` / `LayerCount`), and their whole surface is ordinary Plato in
`foundation/primitives-arrays.library.plato`.

## What is NOT here any more

**Since 2026-07-31 (plato-378), 76 signatures left** for reference bodies over the surviving
kernel. The contract went from 141 to 65.

| Was intrinsic | Now a reference body in |
|---|---|
| `Abs`, `Sign`, `CopySign`, `Min`, `Max`, `Square`, `Reciprocal`, `Cbrt`, `Ceiling`, `Truncate`, `Log`, `Log2`, `Log10`, `Tan`, `Sinh`, `Cosh`, `Tanh`, the six `*Radians` inverse forms, `Zero`, `One`, `Tau`, `E`, `IsFinite` | `foundation/primitives-number.library.plato` |
| `Abs`, `Sign`, `Min`, `Max`, `Zero`, `One`, `Range` on Integer | `foundation/primitives-integer.library.plato` |
| `Map`, `MapPairs/Triplets/Quartets`, `Zip` (2 and 3), `Take`, `Skip`, `Drop`, `TakeLast`, `SubArray`, `Slice`, `EveryNth`, `AtModulo`, `Repeat`, `Append`, `Prepend`, `Concatenate`, `All`, `Any`, `Slices`, `CartesianProduct`, and the whole `Array2D`/`Array3D` surface including `MakeArray2D` | `foundation/primitives-arrays.library.plato` |
| `Equals`, `ExclusiveOr`, `Compare`, `Hash` on Boolean | `foundation/primitives.library.plato` |
| `NotEquals` on every primitive | `foundation/core-comparison.library.plato`, once, over `Equatable` |
| `LessThan`, `GreaterThan`, `GreaterThanOrEquals`, `Compare`, `Clamp` on Number and Integer | already generic over `Orderable` in `foundation/core-comparison.library.plato` — deleted outright, not re-derived |

The array kernel is the load-bearing part: `MapRange` is the ONLY constructor and every
reshaping function is a reindexing of it. `Reduce` and `FlatMap` stay because a Plato body is a
pure expression with no loop and no recursion contract (GLSL forbids recursion outright), so
the fold and the length-varying producer cannot be written.

**Cost, stated plainly.** A derived view is a closure per element. On C# these used to bind to
`Ara3D.Collections`, which is faster. The reference body fixes the SEMANTICS; recovering the
speed is the backend override table, `plato-368`, which is a prerequisite for using this file
in a hot path rather than an optimization on top of it. Likewise `All` / `Any` are folds now
and no longer short-circuit, and `Log10` / `Log2` / `Cbrt` / the inverse-trig family are
identities that typically land a few ulp off a native call.

**Until 2026-07-30 this file also listed 189 signatures** on `Angle`, `Number2/3/4/8`,
`Vector2D/3D`, `Matrix3x2/4x4` and `Quaternion` — types that are ordinary `type` declarations,
not primitives. Those have portable Plato reference bodies too:

| Types | Reference bodies |
|---|---|
| `Angle` (trig, arithmetic) and the `Angle`-returning inverse trig on `Number` | `foundation/angle-trig.library.plato` |
| `Number2`, `Number3`, `Number4`, `Number8` | `foundation/vectors-tuples-ops.library.plato` |
| `Vector2D`, `Vector3D` | `foundation/vectors-geometric-ops.library.plato` |
| `Matrix3x2`, `Matrix4x4` | `foundation/matrices-ops.library.plato` |
| `Quaternion` | `foundation/rotations-ops.library.plato` |
| `CombineHash`, used by the per-type `Hash` bodies | `foundation/hashing.library.plato` |

A backend is free to substitute a verified native implementation (C# uses `System.Numerics`)
through its representation map and override table — that is an optimization, and the reference
body remains the semantics it must agree with. Backend-side reconciliation is `plato-368`.

Note the split this created in the trig surface. The scalar kernel is intrinsic and works in
radians (`Cos(self: Number): Number`); the inverse family carries a `Radians` suffix
(`AcosRadians`) because the public `Acos(self: Number): Angle` shares the receiver type and
differs only in return type. The `Angle`-typed surface is Plato, in `angle-trig.library.plato`.

Policy (from `intrinsics.library.plato`): a function may be declared intrinsic only if
every priority-1..4 backend — C#, C++, CUDA, TypeScript — can supply it natively or with a
trivial shim. Host-specific things (C# SIMD types, IEEE `nextafter`-grade functions,
midpoint-rounding variants) are deliberately excluded.

This list is GENERATED. Regenerate rather than hand-edit, so it cannot drift:

```bash
grep -rEc "^\s+[A-Za-z_][A-Za-z0-9_]*\(.*\)\s*:\s*[^;=]+;\s*$" stdlib/*/*.library.plato | grep -v ":0$"
```

## `foundation/intrinsics.library.plato` (65)

**Number** (26)

- `Add(a: Number, b: Number): Number`
- `Subtract(a: Number, b: Number): Number`
- `Multiply(a: Number, b: Number): Number`
- `Divide(a: Number, b: Number): Number`
- `Modulo(a: Number, b: Number): Number`
- `Negative(n: Number): Number`
- `LessThanOrEquals(a: Number, b: Number): Boolean`
- `Equals(a: Number, b: Number): Boolean`
- `Hash(self: Number): Integer`
- `Floor(self: Number): Number`
- `Round(self: Number, digits: Integer): Number`
- `Sqrt(self: Number): Number`
- `Exp(self: Number): Number`
- `NaturalLog(self: Number): Number`
- `Pow(self: Number, power: Number): Number`
- `FusedMultiplyAdd(self: Number, y: Number, z: Number): Number`
- `Sin(self: Number): Number`
- `Cos(self: Number): Number`
- `Atan2Radians(self: Number, x: Number): Number`
- `MinValue(_: Number): Number`
- `MaxValue(_: Number): Number`
- `Epsilon(_: Number): Number`
- `Pi(_: Number): Number`
- `IsNaN(self: Number): Boolean`
- `IsInfinite(self: Number): Boolean`
- `ToInteger(self: Number): Integer`

**Integer** (18)

- `Add(a: Integer, b: Integer): Integer`
- `Subtract(a: Integer, b: Integer): Integer`
- `Multiply(a: Integer, b: Integer): Integer`
- `Divide(a: Integer, b: Integer): Integer`
- `Modulo(a: Integer, b: Integer): Integer`
- `Negative(n: Integer): Integer`
- `LessThanOrEquals(a: Integer, b: Integer): Boolean`
- `Equals(a: Integer, b: Integer): Boolean`
- `Hash(self: Integer): Integer`
- `BitwiseAnd(a: Integer, b: Integer): Integer`
- `BitwiseOr(a: Integer, b: Integer): Integer`
- `BitwiseXor(a: Integer, b: Integer): Integer`
- `BitwiseNot(x: Integer): Integer`
- `ShiftLeft(x: Integer, bits: Integer): Integer`
- `ShiftRight(x: Integer, bits: Integer): Integer`
- `MinValue(_: Integer): Integer`
- `MaxValue(_: Integer): Integer`
- `ToNumber(self: Integer): Number`

**Boolean** (3)

- `And(a: Boolean, b: Boolean): Boolean`
- `Or(a: Boolean, b: Boolean): Boolean`
- `Not(b: Boolean): Boolean`

**Character and String** (2)

- `LessThanOrEquals(a: Character, b: Character): Boolean`
- `LessThanOrEquals(a: String, b: String): Boolean`

**Array** (5)

- `Count(xs: Array<$T>): Integer`
- `At(xs: Array<$T>, n: Integer): $T`
- `MapRange(n: Integer, f: Function1<Integer, $T>): Array<$T>`
- `Reduce(xs: Array<$T>, acc: $U, f: Function2<$U, $T, $U>): $U`
- `FlatMap(xs: Array<$T1>, f: Function1<$T1, Array<$T2>>): Array<$T2>`

**List<T> (growable builder)** (7)

- `Count(xs: List<$T>): Integer`
- `At(xs: List<$T>, n: Integer): $T`
- `Add(xs: List<$T>, x: $T): List<$T>`
- `AddRange(xs: List<$T>, values: Array<$T>): List<$T>`
- `Set(xs: List<$T>, i: Integer, x: $T): List<$T>`
- `Freeze(xs: List<$T>): Array<$T>`
- `EmptyList(xs: Array<$T>): List<$T>`

**Buffer<T> (fixed-size builder)** (4)

- `Count(xs: Buffer<$T>): Integer`
- `At(xs: Buffer<$T>, n: Integer): $T`
- `Set(xs: Buffer<$T>, i: Integer, x: $T): Buffer<$T>`
- `Freeze(xs: Buffer<$T>): Array<$T>`
