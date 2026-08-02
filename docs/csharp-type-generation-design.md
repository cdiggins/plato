# Generating C# types from Plato type definitions — design notes

Design guidance for the C# writer ([`writers/Plato.CSharpWriter/`](../writers/Plato.CSharpWriter/))
when it emits a concrete struct for a Plato type definition. It covers three questions: how to keep
the Plato surface from colliding with the BCL contract, what else a generated type should carry, and
what a type needs to be safe for binary and span-based interop.

This document records no measurements — no type sizes, member counts, or benchmark figures. The
authority for what the writer emits today is the writer source and its snapshot tests; the authority
for what stays on a struct rather than moving to an extension class is
[`plato-struct-surface.md`](plato-struct-surface.md).

## 1. The reserved-member problem

Plato methods that share a name with a BCL contract member — `Equals`, `GetHashCode`, `ToString` —
must return Plato wrapper types, while C# fixes their return types. **C# cannot overload on return
type**, so `Boolean Equals(T)` and `bool Equals(T)` can never coexist on one type. Exactly one form
survives; the design question is which, and how the other is reached.

### The consequence if the Plato form wins

A type whose `Equals(T)` returns the Plato `Boolean` does not satisfy `IEquatable<T>`. Every
`Dictionary`, `HashSet`, `List.Contains`, `Array.Sort`, and LINQ `Distinct` then falls back to
`Equals(object)` and boxes on each comparison. This is silent — the code is correct, only slow — so
it will not surface in tests.

**The C# form must win for every BCL contract member.**

### Preferred resolution: implicit conversion, no second member

The Plato primitives are wrappers with implicit conversions in both directions. That conversion is
already the bridge; a Plato-typed duplicate is usually unnecessary.

```csharp
public bool Equals(Transform3D other) => ...;
```

is callable from Plato-generated code as-is, because `bool` converts implicitly wherever `Boolean`
is expected. One member, no rename, no duplicate on the public surface. The same holds for
`GetHashCode` returning `int` where Plato expects `Integer`, and `ToString` returning `string` where
Plato expects `String`.

The one failure mode is member access on the result: Plato emitting `x.Equals(y).Not()` finds no
`Not` on `bool`. Two adequate fixes, either sufficient:

- extension methods on `bool` / `int` / `string` in the Plato runtime, or
- the writer inserts a wrapping cast when the call result is itself a receiver:
  `((Boolean)x.Equals(y)).Not()`.

### Fallback resolution: rename the Plato-facing member

Correct where no implicit conversion exists, or where one would be lossy or ambiguous. Rules if the
writer takes this path:

- **Rename in Plato, not in the C# backend, where possible.** If the concept's members are named so
  they never collide — as `NotEquals` already does not — no backend needs a mangler and every
  backend agrees on the name. A backend mangler leaks C#'s reserved set into a language-neutral
  library, and C++, Rust, and shading languages each have a different reserved set, so the table
  would have to be per-backend anyway.
- If it must be a backend rename, it belongs in **name resolution, not the printer** — every emitted
  call site has to resolve to the same name.
- **Collision-check** the mangled name against members the type already declares.
- **It is ABI.** Changing the scheme later breaks compiled consumers.
- Accepted cost: the renamed member is public, so IntelliSense shows two members doing one thing.

### Rejected: explicit interface implementation

`Boolean IValue<T>.Equals(T other)` hides the Plato form from the concrete type and avoids the
rename entirely — but only pays off if the writer already routes calls through generic constraints.
The writer emits concrete calls, so each Plato call site would need a cast to the interface, and
**casting a struct to an interface boxes**. That trades a naming problem for a per-call allocation.
Revisit only if call emission becomes generic-constrained end to end.

### The reserved set is larger than three names

`Equals`, `GetHashCode`, `ToString`, `GetType`, `Finalize`, `MemberwiseClone`, `ReferenceEquals`,
`CompareTo` (must return `int`), `Compare`, `Clone`, `Dispose`, `Deconstruct`, `GetEnumerator` (the
`foreach` pattern is shape-based, so a wrong `Current` type silently disables `foreach`), and
`Count` / `this[int]` where the type claims `IReadOnlyList<T>`.

Two more collision classes the writer must handle:

- **A member whose name equals its enclosing type** is CS0542. A `Matrix4x4()` conversion method is
  legal on `Transform3D` and illegal on `Matrix4x4` itself. Needs a rule — `ToMatrix4x4()`, or
  suppress the self-conversion.
- **C# keywords as field names** need an `@` prefix: `@object`, `@params`, `@event`.

## 2. Correctness obligations on the generated struct

- **`readonly struct`, not `struct`.** Readonly fields alone do not stop defensive copies through
  `in` parameters and readonly fields. Without this, `in` parameters make things worse rather than
  better.
- **`IEquatable<T>` always; `IComparable<T>` where the Plato concept is ordered.**
- **Emit `==` and `!=` alongside any `Equals` override**, or consumers get CS0660/CS0661 and cannot
  write `a == b` at all on a struct.
- **`Equals`, `GetHashCode`, and the tuple conversions must be generated from one field list**, so a
  later field addition cannot reach one and miss another.
- **Float equality is a policy decision.** A NaN field makes `x.Equals(x)` false and breaks the
  hashtable invariant. Decide per concept and state it in the generated doc comment.
- **`override bool Equals(object? obj)`** — nullable-annotated, or every consumer project warns.
- **A bit-zero default is not always a valid value.** A zeroed rotation-and-scale type has a
  degenerate quaternion and zero scale. Naming that `Default` invites its use; emit the concept's
  semantic default when one exists, and name the bit-zero value honestly otherwise.
- **`ToString` must be culture-invariant.** Formatting a Plato `Number` under a comma-decimal locale
  otherwise produces output that does not parse back. If the format claims to be JSON, it must be
  valid JSON.
- **Reject recursive struct definitions (CS0523) and implicit-conversion cycles at generation
  time**, with a writer diagnostic rather than a compiler error in generated output.
- **Implicit conversions only when lossless and cheap.** A conversion that composes several matrices
  is a named method or an explicit operator. Multiple implicit conversions between the same pair of
  types produce CS0457 ambiguity or, worse, surprising overload resolution that still compiles.

## 3. What else the generated type should carry

- **`JsonConverter<T>` per type**, rather than a hand-written parser. It is AOT-safe and it solves
  the readonly-field problem — `System.Text.Json` cannot assign readonly fields, and ignores fields
  entirely unless `IncludeFields` is set. A `[JsonConstructor]` on the generated constructor is the
  lighter alternative where full control is not needed.
- **`IParsable<T>` / `ISpanParsable<T>` paired with `IFormattable` / `ISpanFormattable`**, for the
  compact text form — not for JSON. The pair is only worth generating together, from the same field
  list, with a generated round-trip test; otherwise the two drift and the format becomes unreadable.
- **`[DataMember(Order = n)]`.** `DataContractSerializer` orders members alphabetically without it,
  so renaming a field silently breaks wire compatibility.
- **XML doc comments carried from the Plato declaration** — the generated surface is consumed
  through IntelliSense.
- **`// <auto-generated/>` and `[GeneratedCode]`**, so analyzers, StyleCop, and coverage skip the
  output.
- **Deterministic member ordering**, so regeneration produces a reviewable diff.
- **Fully-qualified names on every static call**, which is what makes the case of a field shadowing
  its own type name resolve correctly.
- **Static abstract interface members** for `Identity`, `Zero`, `One`, letting consumers write
  generic code over Plato concepts.
- **Generated round-trip tests per type**: tuple in and out, `Equals` reflexive, `GetHashCode`
  stable across construction paths, serialize and deserialize.

## 4. Binary layout

Worth pursuing for types that reach GPU buffers, memory-mapped files, or native APIs. It has to be
verified rather than merely attributed.

**Layout is opt-in per type.** Blanket `LayoutKind.Sequential` costs the runtime its field-reordering
optimization on every type that never touches native memory. Add a Plato annotation meaning "fixed
binary layout"; those types get `Sequential`, the rest are left alone.

**Do not use `Pack = 1`.** It does not improve interop — the C and C++ default is natural alignment,
so packing diverges from the layout it is meant to match. It also disables SIMD loads, makes
`Vector128` / `Vector256` intrinsics unusable on the type, and incurs unaligned-access penalties on
ARM. Use default packing and emit **explicit padding fields** where padding is needed, so the
padding is visible in the generated source and appears in diffs.

**Reject these field types in layout-marked types:**

- `bool` — one byte in managed memory, a four-byte `BOOL` under default marshalling, so `sizeof` and
  `Marshal.SizeOf` disagree. The Plato `Boolean` wraps `bool`, making this a live hazard the moment
  a `Boolean` field appears in a layout type.
- `char` — two bytes managed, `CharSet`-dependent marshalled.
- `decimal`, `DateTime`, any reference-containing field, any generic type parameter.

**The guarantee is a test, not an attribute.** Per layout-marked type, generate assertions that
`Unsafe.SizeOf<T>()` equals a checked-in constant, that `Marshal.SizeOf<T>()` equals the same
constant (this is the assertion that catches the `bool` and `char` divergence), that each field's
byte offset equals a checked-in constant, and that
`RuntimeHelpers.IsReferenceOrContainsReferences<T>()` is false. Those constants form a **layout
manifest committed to the repo** — reordering a field then fails the build rather than corrupting a
file.

**Generate the other side from the same source.** The C header and the HLSL/GLSL struct should come
from the same Plato declaration under the same padding rules. A single source of truth for layout is
the main argument for generating at all.

**A persisted layout is a file format.** It needs an append-only field policy — never reorder, never
retype — and a layout hash constant emitted into the type so a mismatched file is detected at load
rather than read as garbage.

**Endianness is out of scope.** All current targets are little-endian; the assumption is documented,
not solved.

## 5. Span and pointer surface

For layout-marked types, in rough order of payoff.

**Bulk span reinterpretation** is the largest win — it is what makes file I/O, GPU upload, and
native interop zero-copy:

```csharp
public static ReadOnlySpan<byte> AsBytes(ReadOnlySpan<Transform3D> src) => MemoryMarshal.AsBytes(src);
public static ReadOnlySpan<Transform3D> FromBytes(ReadOnlySpan<byte> src) => MemoryMarshal.Cast<byte, Transform3D>(src);
```

Two properties of `MemoryMarshal.Cast` to design around: it **truncates** a non-divisible length
rather than throwing, so a checked variant is worth generating; and it does **not** check alignment,
which is the second concrete reason to avoid `Pack = 1`.

**Unaligned single-value access**, for parsers walking a byte buffer:

```csharp
public static Transform3D Read(ReadOnlySpan<byte> src) => MemoryMarshal.Read<Transform3D>(src);
public static void Write(Span<byte> dst, in Transform3D value) => MemoryMarshal.Write(dst, in value);
```

These route through `Unsafe.ReadUnaligned` and are safe at any offset — prefer them to `Unsafe.As`
wherever the offset is not known to be aligned.

**`in` parameters** on every parameter of the generated type, operators included (C# permits `in` on
operator parameters). Only correct once the struct is `readonly`.

**`GetPinnableReference`** returning a `ref readonly` to the first field enables
`fixed (Number* p = transform)` with no `Unsafe` and no `GCHandle`. It depends on the first field
being at offset zero, which the layout manifest already asserts.

**`TryFormat(Span<char>)` and `IUtf8SpanFormattable`**, letting callers format into a stack buffer
with no allocation. Same field list and same round-trip test as `IParsable`.

**SIMD accessors only when `sizeof(T)` equals the vector width exactly.** Reinterpreting a
three-float vector as `Vector128<float>` reads past the end of the final element of any array — an
out-of-bounds read that passes tests and faults in production. Either pad the type to the vector
width deliberately, or emit nothing.

**The `unmanaged` constraint** should be satisfiable by every layout-marked type, so callers get
`stackalloc`, `NativeMemory`, and pointer arithmetic. It follows from having no reference fields,
which the generated tests already assert.

### What not to generate

- **No bitwise `Equals` or bitwise hashing** over cast byte spans. Padding bytes are indeterminate,
  so two logically equal values can compare unequal. This is only safe with *explicit* padding
  fields, which are default-initialized and therefore deterministic — a further argument for
  emitting padding rather than letting the compiler insert it.
- **No span or `ref` returned over `this` from an instance method.** Ref-safety rejects it without
  `[UnscopedRef]`, and with `[UnscopedRef]` it compiles but dangles when the receiver is a temporary.
  Emit these as static methods taking `in T`, or as extension methods on `ref T`.

### A note on `AggressiveInlining`

The writer applies it uniformly, which is right as a default. It is a hint: a large inlined
expression chain — a conversion that composes several matrices, for instance — can exceed the JIT's
inlining budget and be rejected anyway. The attribute does not make a call site free, and it is not
a substitute for the writer's own inlining pass.
