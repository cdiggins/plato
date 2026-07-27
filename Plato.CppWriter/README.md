# Plato.CppWriter

Proof-of-concept C++ / CUDA backend. Like `Plato.GlslWriter` it is TIR-only: every body
renders from the monomorphized Typed IR, there is no legacy symbol-graph fallback.

## Usage

```
Plato.CLI [inputFolder] [outputFolder] --cpp
Plato.CLI [inputFolder] [outputFolder] --cuda
```

`--cpp` writes `plato.hpp` (portable C++17), `--cuda` writes `plato.cu` (nvcc). Each file is
self-contained: preamble, structs, structural equality, all function prototypes, all function
definitions, then a trailing comment block listing everything skipped and why. Prototypes come
first, so definition order never matters.

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
| constants | zero-argument functions (`Pi()`) |
| bodiless intrinsics | `<cmath>` (`sqrtf`, `powf`, `fmodf`, …) and the `plato::` helpers (`dot_`, `cross_`, `length_`, `mix_`, `clamp_`, …) |
| `IArrayLike` value types | structural `At` / `Count` / `NumComponents` generated from the fields; `At` **clamps** out of range (device code cannot trap) |

`Number` is `float`, not `double`, in both dialects: CUDA device code is float-first and the two
outputs have to agree.

### Two things C++ forces that GLSL did not

- **Type-named conversion functions are renamed.** `Matrix4x4(Transform3D)` would hide the type
  `Matrix4x4`, after which the type could not be named in a later signature. Those emit as
  `ToMatrix4x4`, and call sites follow. A type-named call is only treated as *construction* when
  its arguments are scalar components.
- **Conversions are explicit.** C++ has no implicit user-defined conversion, so a `TirCoerce`
  between struct types — and a returned value whose type differs from the declared return type —
  becomes a real call to the library's own conversion function.

## Only-what-compiles: the prune pass

Every emitted body records the free functions it calls, with argument types. After collection the
writer drops, to a fixpoint, any function that calls a signature nothing emitted (allowing the
int→float promotion C++ would apply on its own). A skipped function therefore cannot take its
callers down with it — which is what makes "the whole standard library compiles" achievable rather
than aspirational.

## Not representable (functions using these are skipped)

- **Lambdas and function values** — not lowered in V1. `Map`/`Reduce`/`Sum` and friends need the
  `--inline` beta reducer to specialize function values away first.
- **`IArray` and array literals** — no array value type in V1.
- **`String` / `Character`** — no string type in V1.
- **Static member functions** (first parameter `_`).
- **Intrinsics over user structs** — `Modulo` on a `Point2D`, `Dot` on a `Point3D`: the native
  lowerings only apply to scalars and the vector types.

## Status

| Input | Emitted | Skipped |
|---|---|---|
| `demos/plato-src` (66 functions) | **66** | 0 |
| `plato-src` (full standard library) | **461** | 1722 |

Both compile clean with MSVC (`/std:c++17 /W3`). The stdlib skips are dominated by the callee
cascade (715), unrepresentable types (404) and lambdas (300) — closing the lambda gap is the
single biggest lever, exactly as for the GLSL backend.

Verification lives in `../Plato.CppWriter.Tests`.
