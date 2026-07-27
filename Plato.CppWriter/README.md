# Plato.CppWriter

Proof-of-concept C++ / CUDA backend. Like `Plato.GlslWriter` it is TIR-only: every body
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
| `Vector2D/3D/4D` | `float2/float3/float4`; `X/Y/Z/W` become `.x/.y/.z/.w`; constructed with `make_floatN` |
| other non-generic concrete types | aggregate `struct`, constructed positionally (`Angle{ 1.0f }`), plus generated field-wise `operator==` / `operator!=` |
| every function | a **free function**: `v.Length` → `Length(v)`. C++ overloading lets `Add(float2,float2)` and `Add(float3,float3)` coexist |
| static members (`_: T`) | free functions that keep the `_` parameter as a type tag (`UnitX(float2{})`) so overloads stay distinct |
| constants | zero-argument functions (`Pi()`) |
| bodiless intrinsics | `<cmath>` (`sqrtf`, `powf`, `fmodf`, …) and the `plato::` helpers (`dot_`, `cross_`, `length_`, `mix_`, `clamp_`, …); float-field structs (`Point2D`, …) lower componentwise / via `make_floatN` |
| `Equals` / `NotEquals` / `GetHashCode` | generated for every representable type (`==` / `!=` / structural mix) |
| `IArrayLike` value types | structural `At` / `Count` / `NumComponents` / `Components` from the fields; `At` **clamps** out of range (device code cannot trap); `Components` returns `floatN` or a synthesized `FixedArray_N_T` POD |

`Number` is `float`, not `double`, in both dialects: CUDA device code is float-first and the two
outputs have to agree.

### Two things C++ forces that GLSL did not

- **Type-named conversion functions are renamed.** `Matrix4x4(Transform3D)` would hide the type
  `Matrix4x4`, after which the type could not be named in a later signature. Those emit as
  `ToMatrix4x4`, and call sites follow. A type-named call is only treated as *construction* when
  its arguments are scalar components.
- **Conversions are explicit.** C++ has no implicit user-defined conversion, so a `TirCoerce`
  between struct types — and a returned value whose type differs from the declared return type —
  becomes a real call to the library's own conversion function (or a field swizzle when the
  types are matching-arity float aggregates ↔ `floatN`).

## Only-what-compiles: the prune pass

Every emitted body records the free functions it calls, with argument types. After collection the
writer drops, to a fixpoint, any function that calls a signature nothing emitted (allowing the
int→float promotion C++ would apply on its own). A skipped function therefore cannot take its
callers down with it — which is what makes "the whole standard library compiles" achievable rather
than aspirational.

## Not representable (functions using these are skipped)

- **Lambdas and function values** — `--inline` β-reduces many away (M5); residual closures are
  still skipped (functors deferred). Without `--inline`, function values remain unsupported.
- **`IArray` and array literals** — no dynamic array value type yet. Planned as a simple
  Plato-defined data type (not `std::vector`); see plato-239 M4.
- **`String` / `Character`** — planned as simple Plato-defined data types (not `std::string`);
  see plato-239 M3.
- **`FieldNames` / `FieldValues` / `TypeName` / `ToString` / `GetType`** — need String (and
  sometimes arrays).
- **`CreateFromComponents` / `CreateFromComponent`** — need an array value type.

## Status

| Input | Emitted | Skipped | Notes |
|---|---|---|---|
| `demos/plato-src` | **87** | 0 | with `--inline` (same without) |
| `plato-src` (full standard library) | **865** | 1660 | with `--inline` (compile-gate default) |
| same, no `--inline` | 870 | 1655 | post-M5 plumbing, inline off |

M5 wires shared `TirInliner` into `--cpp`/`--cuda`. Skip count does **not** improve yet:
β-reduction still leaves many residual closures (functors deferred), and a few bodies that
emitted without inline now skip after partial specialization. The single biggest remaining
lever is residual-functor emission, then M3/M4.

Verification lives in `../Plato.CppWriter.Tests` (gates run with `inlineCalls: true`).
