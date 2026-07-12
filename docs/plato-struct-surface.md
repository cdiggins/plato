# Plato intrinsics struct surface — the keep-on-struct contract (2026-07-12)

> Consolidation plan C3 / M5. When behaviour moves from struct instance methods to `<Type>Intrinsics`
> extension classes, THIS is the surface that must stay ON the struct (or otherwise remain callable
> in idiomatic C#). Enforced by `IntrinsicsApiSnapshotTests` — any change to the public surface of a
> listed type is a reviewable test diff, so a **loss** (a member vanishing from both the struct and
> its extension class) can never slip through unnoticed.

## The intrinsic types in scope

Scalar-erased (already ported to `<Type>Intrinsics` extensions): `Number`, `Integer`, `Boolean`.
No behaviour to move: `Character`, `String` (conversions + IArray obligations only).
**Not yet ported (M5 target):** `Angle`, `Vector2`, `Vector3`, `Vector4`, `Vector8`,
`Matrix3x2`, `Matrix4x4`, `Quaternion`, `Plane`.

## Stays ON the struct (never moves to an extension)

1. **Fields** — the backing data (`Value`, `X`/`Y`/`Z`, `M11`…). Not movable; they are the state.
2. **Constructors** — `new Vector3(x, y, z)`, `new Vector3(SNVector3)`, broadcast `new Vector3(x)`.
   C# constructors cannot be extensions.
3. **Operators** (`op_Addition`, `op_Multiply`, `op_Equality`, `op_UnaryNegation`, …) — C# does not
   permit extension operators on our target LangVersion, and `a + b` is core ergonomics. **Keep.**
4. **Implicit / explicit conversions** (`op_Implicit`, `op_Explicit`) — same reason; also load-bearing
   for the scalar-erasure boundary (`Number` ⇄ `float`). **Keep.**
5. **Field-style properties consumers rely on**: the component accessors (`X`, `Y`, `Z`, `M11`…),
   `Count`, and the cheap pseudo-fields. These read as data in C# (`v.X`) and moving them to methods
   would break every consumer and the cost-model's field-projection recognition. **Keep as
   properties.**
6. **Static factory / constant helpers**: `Zero`, `One`, `UnitX/Y/Z/W`, `Default`, `Identity`,
   `MinValue`, `MaxValue`, `Create*`. Idiomatic `Vector3.UnitX` must keep working. **Keep as
   statics on the struct.**
7. **BCL / framework obligations**: `IReadOnlyList<T>` members, `IEquatable<T>.Equals`,
   `GetHashCode`, `ToString`, `Deconstruct`, `DataContract`/`DataMember`. Interface obligations
   cannot be extensions. **Keep.**

## May move to `<Type>Intrinsics` extension methods

Everything else — the *behavioural* instance methods: `Cos`, `Sin`, `Normalize`, `Dot`, `Cross`,
`Transform`, `Length`, `Lerp`, the swizzles-as-methods, etc. These become
`public static R Foo(this T self, …)` in `<Type>Intrinsics`, exactly as `Number`/`Integer`/`Boolean`
already did. Consumer call syntax (`v.Normalize()`, `a.Cos()`) is unchanged because extension methods
are invoked with the same `receiver.Method()` syntax.

## The one thing consumers lose (acceptable)

Passing an instance method as a method group where a delegate is expected
(`someList.Select(v.Normalize)`) does not work for extension methods — but `v => v.Normalize()`
does, and the generated Plato surface never does this. No known consumer relies on it.

## Emitter obligation (the M5 blocker)

The emitter currently generates, per handwritten intrinsic function, a forwarder extension
`TExtensions.Foo(this T self) => self.Foo()` that also performs the `Number→float` return erasure.
Once `Foo` is itself an extension, `self.Foo()` is CS0121-ambiguous and self-recursive. The emitter
must instead emit the forwarder as an explicit static call to the intrinsic —
`TExtensions.Foo(this T self) => TIntrinsics.Foo(self)` — preserving the return erasure. That is the
emitter half of M5; the snapshot test guards the runtime half.
