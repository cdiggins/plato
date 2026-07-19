# Plato.GlslWriter

Proof-of-concept GLSL backend (target: GLSL ES 3.00, `#version 300 es` — the WebGL2
dialect, a strict subset of desktop GLSL 3.30+). Mirrors the architecture of the
TypeScript/Rust POC writers but is TIR-only: every body renders from the
monomorphized Typed IR, there is no legacy symbol-graph fallback.

## Usage

```
Plato.CLI [inputFolder] [outputFolder] --glsl
```

Produces a single `plato.glsl`: structs, then all function prototypes, then all
definitions (prototypes first means definition order never matters), then a
trailing comment block listing everything that was skipped and why.

The output is a shader *library chunk*: it starts with `#version 300 es` +
`precision` directives, so a complete shader is produced by appending `in`/`out`
declarations and a `main()`.

## Output model

GLSL is C-like: no methods, no generics, no closures, no heap, no recursion.
The mapping:

| Plato | GLSL |
|---|---|
| `Number` / `Integer` / `Boolean` | `float` / `int` / `bool` |
| `Vector2D/3D/4D` | native `vec2/vec3/vec4`; `X/Y/Z/W` fields become `.x/.y/.z/.w` swizzles; constructors line up positionally |
| other non-generic concrete types | `struct` (GLSL structs get a positional constructor for free, matching `TirNew` exactly) |
| every function | a **free function**: `v.Length` → `Length(v)`. GLSL overloads by parameter types, so `Add(vec2,vec2)` and `Add(vec3,vec3)` coexist |
| operator-named calls on native scalars/vectors | inlined native operators (`a + b`); float `Modulo` → `mod(a, b)` |
| constants | zero-argument functions (`Pi()`) |
| bodiless intrinsics | GLSL built-ins (`sqrt`, `abs`, `pow`, `min`, `max`, `clamp`, `mix`, `smoothstep`, `length`, `dot`, `cross`, `normalize`, `reflect`, `cos(a.Radians)`, ...) |
| fixed-size arrays (below) | GLSL sized arrays `T[N]` |

Because the TIR is already monomorphized, interface-generic Plato code arrives
here as ground functions per concrete type — which is exactly the only form GLSL
can express. Distinct types survive: `Angle` is a real struct, so
`Cos(1.0)` still refuses to compile in GLSL, same as in C#.

## Fixed-size arrays (Tier 1 / Tier 2)

GLSL has no heap and no dynamically sized arrays, but it does have compile-time
`T[N]`. Two array forms are supported:

- **IArrayLike value types → structural accessors.** A concrete type that
  implements `IArrayLike` (or the demo marker) over N homogeneous fields — a
  `Matrix4x4`, `Vector8`, `Point3D`, a fixed control-point set — gets `At`,
  `Count`, `NumComponents` and `Components` generated directly from its fields.
  `At(i)` is an index chain that **clamps** to the last field on an out-of-range
  index (GLSL cannot trap, so clamping is the closest thing to a safe failure).
  `Components` returns a GLSL `T[N]`. This lifted +104 functions on the full
  stdlib (Matrix/Vector/Point/Integer-vector types) at zero skips.
- **Array literals → sized-array constructors.** `[a, b, c]` becomes
  `T[3](a, b, c)`. The length is fixed at compile time.

What is still impossible: an array whose length is a runtime value. That needs
either a resource binding (texture/SSBO) — arrays stop being values — or a new
Plato loop construct. The demos show the honest split: Plato supplies the
fixed-capacity type and a clamped accessor; the shader's own `main()` runs any
runtime-count loop over it.

## Not representable in GLSL (functions using these are skipped)

- **Lambdas and function values** — GLSL has no function pointers or closures.
  `Map`/`Reduce`/`Sum` and friends cannot exist in general form; the idiomatic
  GLSL answer is compile-time specialization (inline the lambda at each call
  site), which the Plato inliner (`--inline`) could do in a future increment.
- **`IArray` / array literals** — no dynamically sized arrays. Fixed-size
  `T[N]` arrays exist and could back `IArrayLike` types later.
- **`String` / `Character`** — no string type at all.
- **Recursion** — forbidden by the GLSL spec. Not yet detected by this writer;
  the downstream GLSL compiler reports it.
- **`Number` is float32** — GLSL ES has no `double`. Constants keep their full
  decimal expansion and are truncated by the GLSL compiler.
- `mod(x, y)` is floor-mod, while C# `%` is truncation-remainder: results
  differ for negative operands.

## Demos

`../demos/glsl/` holds four self-contained demo libraries, each generated to
`out/<name>.glsl` and rendered live in a WebGL2 gallery (`index.html`, served by
`gen.ps1` output + any static server). Regenerate with `demos/glsl/gen.ps1`.

| Demo | Plato aspect shown |
|---|---|
| `sdf` | vector library + `min`/`max`/smooth-min compose a CSG scene; shader raymarches `Scene(p)` |
| `mandelbrot` | user `Complex` type → GLSL struct; `Iterate(z,c)=z²+c` drives the escape loop |
| `bezier` | Tier-1 fixed array: `Bezier4` with a clamped index-chain `At`, de Casteljau `Eval` |
| `polygon` | Tier-2 bounded array: `Polygon6` fixed capacity + clamped `At`; harness loops `i < count` |

All four compile, link and render at **0 skips** each. The shared `src/_core.plato`
is the curated primitive + vector library each demo builds on.

## Verification

The original demo library (`demos/plato-src/geometry.plato`, 66 functions) and
all four `demos/glsl` libraries compile, link and render in WebGL2 with zero
skips. Full stdlib (`plato-src`): 1259 emitted / 939 skipped (see below).

## Known POC limitations

- Static member functions (first parameter `_`) are not emitted.
- `WithX` field-update functions are not generated (call sites using them fail).
- `Equals`/`NotEquals` functions are not emitted; call sites inline to `==`/`!=`
  (valid on GLSL scalars, vectors and structs).
- Duplicate signatures: first wins, silently.
- The full stdlib (`plato-src`) runs end to end but ~42% of functions still skip:
  the top reasons are lambdas / function values (no closures), unmapped
  intrinsics, and `IArray` / `FunctionN` parameter types (see the trailing
  `// Skipped` block in any full-stdlib output). Closing most of the lambda gap
  needs the `--inline` beta reducer to specialize function values away before
  emission — the single biggest lever for shader-targeting the whole library.
