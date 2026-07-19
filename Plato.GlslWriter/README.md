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
| bodiless intrinsics | GLSL built-ins (`sqrt`, `abs`, `pow`, `cos(a.Radians)`, ...) |

Because the TIR is already monomorphized, interface-generic Plato code arrives
here as ground functions per concrete type — which is exactly the only form GLSL
can express. Distinct types survive: `Angle` is a real struct, so
`Cos(1.0)` still refuses to compile in GLSL, same as in C#.

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

## Verification

The demo library (`demos/plato-src/geometry.plato`, 66 functions) compiles,
links and renders in WebGL2 with zero skips. Harness: a fullscreen-triangle
fragment shader whose `main` calls `UnitCircle`, `Turns`, `Normalize`,
`Reflect`, `Perpendicular`, `Lerp`, `Clamp`, `Cross`, ... on top of the
generated chunk.

## Known POC limitations

- Static member functions (first parameter `_`) are not emitted.
- `WithX` field-update functions are not generated (call sites using them fail).
- `Equals`/`NotEquals` functions are not emitted; call sites inline to `==`/`!=`
  (valid on GLSL scalars, vectors and structs).
- Duplicate signatures: first wins, silently.
- The full stdlib (`plato-src`) has not been attempted, only the curated demo
  library (same status as the TS/Rust POCs originally).
