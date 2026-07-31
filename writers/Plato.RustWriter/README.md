# Plato.RustWriter

Generates Rust source code from a Plato compilation. This project mirrors the
architecture of `Plato.TypeScriptWriter` (which itself mirrors `Plato.CSharpWriter`):

| Plato.TypeScriptWriter | Plato.RustWriter | Role |
|---|---|---|
| `TypeScriptWriter` | `RustWriter` | Top level orchestrator: walks the compilation and produces output files |
| `TypeScriptTypeWriter` | `RustTypeWriter` | Writes code for one type (or standalone functions); resolves type names |
| `TypeScriptConcreteTypeWriter` | `RustConcreteTypeWriter` | Writes a concrete type (struct + impl, or extension trait for a primitive) |
| `TypeScriptFunctionInfo` | `RustFunctionInfo` | Computes signatures for a function instance |
| `TypeScriptFunctionBodyWriter` | `RustFunctionBodyWriter` | Recursively translates the Plato symbol tree into expressions/statements |
| `ITypeToTypeScript` | `ITypeToRust` | Type-name resolution abstraction |

## Usage

```csharp
using Ara3D.Geometry.RustWriter;

var writer = compilation.ToRust(outputFolder);
foreach (var kv in writer.Files)
    outputFolder.RelativeFile(kv.Key).WriteAllText(kv.Value.ToString());
```

Or via `Plato.CLI` (note: the output folder must already exist):

```
Plato.CLI [inputFolder] [outputFolder] --rust
```

## Output model

A single self-contained module `plato.rs` is produced. It contains, in order:

1. A hand-written intrinsics prelude: the `IArray<T>` trait, the Vec-backed
   `Arr<T>` struct (with `At`, `Count`, `Map`, `Reduce`), and an `Intrinsics`
   module (`MakeArray`, `Range`).
2. One `pub trait` per Plato concept (declaration only; `inherits` becomes
   supertraits, and every concept trait requires `Copy`). No `impl Trait for
   Type` blocks are generated — inherent methods carry all behavior.
3. Generated IArray library functions, when present: element-generic functions
   become methods in an extra `impl<T: Copy> Arr<T>` block; concrete-element
   functions become free functions.
4. A `pub mod Constants` with one zero-argument function per library constant
   (`Constants::Pi()` — Rust has no computed statics).
5. **Extension traits** for the Plato primitives.
6. One `pub struct` + inherent `impl` per remaining concrete type.

The generated file starts with `#![allow(non_snake_case, ...)]`: the Plato API
keeps PascalCase (`v.Length()`, `pub X`) for parity with the C# and TypeScript
output. The demo's point is that the Plato calls read identically in all three
languages.

### Fluent syntax on native values

Plato's primitives map to native Rust types, each with its own extension trait:

| Plato | Rust | Trait |
|---|---|---|
| Number | `f64` | `NumberExt` |
| Integer | `i64` | `IntegerExt` |
| Boolean | `bool` | `BooleanExt` |
| String | `String` | `StringExt` |
| Character | `char` | `CharacterExt` |

This gives fluent syntax on plain values with zero wrappers:

```rust
(0.5).Turns().Cos()
x.Sqrt().Clamp(0.0, 1.0)
a.Lerp(b, t)
```

Callers need the traits in scope: emit everything in one module and
`use crate::plato::*;` in consuming code.

**Improvement over TypeScript**: `Number` (f64) and `Integer` (i64) are
distinct Rust types, so the TypeScript name-collision problem (both share
`Number.prototype`) disappears — `Integer.Compare` and `Number.Compare`
coexist. The claimed-name set only guards against true duplicates within one
trait.

### Methods, not getters

Declared fields are public struct fields (`v.X`). Every Plato function becomes
a method taking `self` by value: every generated struct derives
`Clone, Copy, PartialEq, Debug` (+ `Default` when all fields have one).
Single-parameter functions are zero-argument methods (`v.Length()`).

Concrete structs get: `new`, `With{Field}` functions, `Create` (associated
factory), `Equals`/`NotEquals` (via `PartialEq`), and their Plato member
functions. `Default` replaces the TypeScript `static get Default`; the body
writer emits `T::default()` for type-as-value symbols. Bodiless intrinsics get
native implementations from `RustTypeWriter.TryGetIntrinsicBody` (`f64::sqrt`,
comparisons, trig on `Angle`); anything unknown becomes `unimplemented!()`.

### Body translation specifics

- Expression bodies become `{ expr }` (Rust expression bodies).
- Conditional expressions become `if c { a } else { b }`.
- **Number literals are forced to a decimal point** (`1` → `1.0`): Rust has no
  implicit int-to-float conversion. This was the single most likely source of
  compile errors.
- Integer literals used as receivers get an `i64` suffix (`(1i64).Compare(x)`)
  so method resolution finds `IntegerExt`.
- Zero-argument constants become `Constants::Pi()` (path syntax with parens).
- Static calls become `Type::F(x)`; constructor calls become `T::new(args)`
  (bare type name — generic arguments are inferred).
- Lambdas become closures: `|a, b| body`.
- Receivers that are literals, conditionals, lambdas, or assignments are
  parenthesized.

### Other conventions

- `implements` clauses are omitted (no trait impls in the POC).
- Statics (Plato first parameter `_`) become associated functions
  (`Type::Name(...)`); they are skipped on primitives.
- No overloading: colliding names are skipped with a `// Skipped:` comment.
- The array concept (`Array` or `IArray` depending on dialect) maps to the
  generated `Arr<T>`.

## Known limitations

- Concept traits are declaration-only; no `impl Trait for Type` generation.
- `std::ops` operator impls (`a + b` on vectors) are not generated.
- String/Character support is minimal (the geometry library does not use them).
- `Arr` is eager (Vec-backed), unlike the lazy TypeScript `Arr`.
