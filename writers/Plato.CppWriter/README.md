# Plato.CppWriter

Proof-of-interface C++ / CUDA backend. Like `Plato.GlslWriter` it is TIR-only: every body
renders from the monomorphized Typed IR, there is no legacy symbol-graph fallback.

## Usage

```
Plato.CLI [inputFolder] [outputFolder] --cpp
Plato.CLI [inputFolder] [outputFolder] --cuda
Plato.CLI [inputFolder] [outputFolder] --cpp --inline
```

`--cpp` writes `plato.hpp` (portable C++17), `--cuda` writes `plato.cu` (nvcc). Each file is
self-contained: preamble, structs, structural equality, reflection helpers, all function
prototypes, all function definitions, then a trailing comment block listing everything skipped
and why. Prototypes come first, so definition order never matters.

`--inline` runs the shared `TirInliner` β-reducer before body emit (same as the C# path), so
`Map`/`Reduce`/`Sum` and friends can specialize function values away. Off by default; the
compile-gate tests turn it on.

## One emitter, two dialects

**The generated code is identical for both dialects — only the preamble differs.** That is the
central design decision, and `Dialects_Differ_Only_In_The_Preamble` in the test project enforces
it. The body writer always emits `float2/float3/float4`, `make_floatN(...)`, native operators and
`plato::` math helpers; the preamble is what makes those mean the right thing:

| | C++ | CUDA |
|---|---|---|
| vector types | declared in the preamble, layout-compatible with CUDA's | `#include <cuda_runtime.h>` |
| `PLATO_FN` | `inline` | `__host__ __device__ inline` |
| vector operators | preamble | preamble (CUDA deliberately ships none; `helper_math.h` is sample-only) |

## Output model

| Plato | C++ / CUDA |
|---|---|
| `Number` / `Integer` / `Boolean` | `float` / `int` / `bool` |
| `Character` | `char` (device+host POD) |
| `String` | fixed-capacity POD `String { char data[PLATO_STRING_CAP]; int len; }` (default cap 64); literals via `make_string_n`; **not** `std::string` |
| `Array<T>` / `IArray<T>` | fixed-capacity POD `Array<T> { T data[PLATO_ARRAY_CAP]; int count; }` (default cap 64); literals via `make_array(...)`; **not** `std::vector` |
| `Angle` | synthesized `struct Angle { float Radians; }` (Plato type is fieldless; matches `.Radians` intrinsics) |
| `Vector2D/3D/4D` | `float2/float3/float4`; `X/Y/Z/W` become `.x/.y/.z/.w`; constructed with `make_floatN` |
| other non-generic concrete types | aggregate `struct`, constructed positionally (`Point2D{ … }`), plus generated field-wise `operator==` / `operator!=` |
| every function | a **free function**: `v.Length` → `Length(v)`. C++ overloading lets `Add(float2,float2)` and `Add(float3,float3)` coexist |
| static members (`_: T`) | free functions that keep the `_` parameter as a type tag (`UnitX(float2{})`) so overloads stay distinct |
| constants | zero-argument functions (`Pi()`) |
| bodiless intrinsics | `<cmath>` (`sqrtf`, `powf`, `fmodf`, …) and the `plato::` helpers (`dot_`, `cross_`, `length_`, `mix_`, `clamp_`, …); float-field structs (`Point2D`, …) lower componentwise / via `make_floatN` |
| `Equals` / `NotEquals` / `GetHashCode` / `TypeName` / `ToString` | generated for representable types; **ToString formats values** (numbers, vectors, structs) into the String buffer via hand-rolled append helpers (no iostream / snprintf) |
| `IArrayLike` value types | structural `At` / `Count` / `NumComponents` / `Components` / `Reverse` / `CreateFrom*`; field-expanded templates for `MapComponents` / `ZipComponents` / `All*` / `Any*`; `At` **clamps** out of range; non-floatN `Components` returns `Array<T>` |

`Number` is `float`, not `double`, in both dialects: CUDA device code is float-first and the two
outputs have to agree.

**Why fixed `String` / `Array` instead of heap?** Identical C++/CUDA bodies and no device heap:
inline buffers match the String story. Raise `PLATO_STRING_CAP` / `PLATO_ARRAY_CAP` if needed.
Oversized `Range` / `Map` / `FlatMap` results clamp to the cap.

**Array surface in the preamble:** `Count` / `At` / `Range` / `MapRange` / `Map` / `Zip` (2- and
3-arg) / `Reduce` / `All` / `Any` / `Reverse` / `FlatMap` / `Concatenate` / `Append` / `Prepend`
plus `make_array` / `make_array_empty`.

### Two things C++ forces that GLSL did not

- **Type-named conversion functions are renamed.** `Matrix4x4(Transform3D)` would hide the type
  `Matrix4x4`, after which the type could not be named in a later signature. Those emit as
  `ToMatrix4x4`, and call sites follow. A type-named call is only treated as *construction* when
  its arguments are scalar components.
- **Conversions are explicit.** C++ has no implicit user-defined conversion, so a `TirCoerce`
  between struct types — and a returned value whose type differs from the declared return type —
  becomes a real call to the library's own conversion function (or a field swizzle when the
  types are matching-arity float aggregates ↔ `floatN`). `Angle` ↔ `float` uses `.Radians` /
  `Angle{ … }`.

## Only-what-compiles: the prune pass

Every emitted body records the free functions it calls, with argument types. After collection the
writer drops, to a fixpoint, any function that calls a signature nothing emitted (allowing the
int→float promotion C++ would apply on its own). A skipped function therefore cannot take its
callers down with it — which is what makes "the whole standard library compiles" achievable rather
than aspirational.

## Not representable (functions using these are skipped)

- **`Array2D` / `Array3D` / `MakeArray2D`** — still no multi-dimensional POD.
- **`FieldNames` / `FieldValues` / `GetType`** — need `Type` (ignored) and/or richer reflection.
- **`WithNext` and related zipper helpers** — not yet in the preamble; callers prune away.
- **Large arrays** — anything needing more than `PLATO_ARRAY_CAP` elements clamps or skips
  (mesh builders with big `MapRange` still limited).
- **`CreateFromComponents` / `CreateFromComponent` on fully dynamic inputs** — fixed-size and
  `Array<T>` overloads are generated for IArrayLike; remaining dynamic cases may still skip.

## Status

| Input | Emitted | Skipped | Notes |
|---|---|---|---|
| `demos/stdlib-legacy` | **153** | 0 | with `--inline` + Array + rich ToString |
| `stdlib-legacy` (full standard library) | **1716** | 1364 | with `--inline` + Array + rich ToString (compile-gate) |
| same, post M3/M4 (plato-239) | 1638 | 1367 | fixed String + IArrayLike only |
| same, post-functor (pre M3/M4) | 1090 | 1507 | |
| same, M5 only (pre-functor) | 865 | 1660 | |

Skip delta vs post M3/M4: **+78 emitted / −3 skipped** (stdlib). Demos: **147 → 153**.
(Emitted rises more than skipped falls because newly representable `Array` fields unlock many
struct members that still prune on `WithNext` / tuples / oversized builders.)

Verification lives in `../Plato.CppWriter.Tests` (gates run with `inlineCalls: true`).
