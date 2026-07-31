---
id: plato-234
title: GLSL writer drops every Angle-typed function and struct (Angle unrepresentable)
type: bug
status: done
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-27
closed: 2026-07-27
links: [submodules/Plato/Plato.GlslWriter/GlslWriter.cs, submodules/Plato/plato-src/primitives.plato, submodules/Plato/plato-src/intrinsics.plato, submodules/Plato/plato-src/angles.plato, plato-138]
---

## Issue
`GlslWriter` has no GLSL representation for Plato's `Angle`. `Angle` is a compiler-known
primitive declared with **zero fields** (`type Angle implements IMeasure { }`,
`plato-src/primitives.plato:10`), so it is filtered out of the struct candidate set by
`ComputeStructs()` (`c.TypeDef.Fields.Count > 0`), and it is absent from `NativePrimitives`.
`GlslTypeName` therefore throws `GlslUnsupportedException` for it and every function whose
signature mentions `Angle` is skipped.

Observed on the full stdlib (`dotnet run --project submodules/Plato/Plato.CLI -c Release --
submodules/Plato/plato-src <out> --glsl`): **1259 emitted / 939 skipped**, of which
89 skips read exactly `type 'Angle' is not representable in GLSL`.

## Impact
Angle is the root of the largest single representable-type gap in the GLSL backend:

- 89 direct function skips (`type 'Angle' is not representable`).
- 39 struct-level drops (`field type not representable`), most of them Angle-rooted:
  `ColorHSV`, `ColorHSL`, `SphericalCoordinate`, `PolarCoordinate`, `LogPolarCoordinate`,
  `CylindricalCoordinate`, `HorizontalCoordinate`, `GeoCoordinate`,
  `GeoCoordinateWithAltitude`, `AnglePair`, `Lissajous`, `Arc`, `Sector`, `Chord`,
  `Segment`, ...
- Cascading function skips from those dropped structs (e.g. 24 more from `PolyLine2D`).

Practical cost: no trig-driven shader code (polar/spherical mapping, HSV/HSL colour, arcs
and sectors, rotation helpers) survives to GLSL — exactly the code a shader author reaches
for. It also caps the emitted/skipped ratio that plato-138 tracks as the GLSL coverage
metric.

## Affected code
- `submodules/Plato/Plato.GlslWriter/GlslWriter.cs:95` — `NativePrimitives` has only
  `Number`/`Integer`/`Boolean`; no `Angle`.
- `submodules/Plato/Plato.GlslWriter/GlslWriter.cs:270` — `ComputeStructs()` candidate
  filter `TypeParameters.Count == 0 && Fields.Count > 0` excludes the fieldless `Angle`.
- `submodules/Plato/Plato.GlslWriter/GlslWriter.cs:283` — struct fixpoint drops every
  struct with an `Angle` field.
- `submodules/Plato/Plato.GlslWriter/GlslWriter.cs:586` — `Number.Acos/Asin/Atan` emit
  `Angle(acos(x))`, a call to a function that does not exist in the output.
- `submodules/Plato/Plato.GlslWriter/GlslWriter.cs:595` — `Angle.Cos/Sin/Tan` emit
  `cos(a.Radians)`, a field read that does not exist in the output.
- `submodules/Plato/Plato.GlslWriter/GlslWriter.cs:627` — `Atan2` emits `Angle(atan(a,b))`.
- `submodules/Plato/plato-src/primitives.plato:10` — `Angle` declared fieldless.
- `submodules/Plato/plato-src/intrinsics.plato:423-444` — the bodiless `Angle` intrinsic
  surface (`Angle(x: Number)`, `Radians`, `Cos`, `Sin`, `Tan`, arithmetic, comparisons).
- `submodules/Plato/plato-src/angles.plato` — `Turns`/`Degrees`/`Gradians` overloaded in
  both directions (`Number -> Angle` and `Angle -> Number`).

## Cause / analysis
Two independent gaps compound:

1. **Representation.** `Angle` is a compiler-known primitive with no fields, so it fits
   neither the "native map" path nor the "concrete type becomes a struct" path. The writer
   was built assuming those two paths are exhaustive; a fieldless non-native primitive
   falls through both.
2. **Intrinsic bodies assume a representation that was never chosen.** `TryGetIntrinsicBody`
   already hardcodes `Angle(acos(x))` and `a.Radians`, i.e. it assumes `Angle` is a
   one-field struct with a `Radians` field. Nothing emits that struct, so those bodies were
   dead text: any function reaching them was already being skipped for the type error
   first. Whatever representation is chosen, these three sites must move with it or the
   output stops compiling.

## Priority
p2. Severity is high for a shipped backend (largest single coverage gap, and it silences
whole categories of shader-relevant maths), but the GLSL writer is a generation-side
feature with no runtime blast radius, and the workaround for a caller is to hand-write the
trig. Effort is small (one map entry plus a handful of intrinsic strings). Deferral cost is
flat, not compounding — but every stdlib addition that uses `Angle` widens the gap for free,
so doing it now is cheap and doing it later is the same cost with a worse ratio meanwhile.

## Dependencies
- Blocks: plato-138 (GLSL coverage) — the emitted/skipped ratio cannot improve materially
  while Angle is unrepresentable.
- Touches: `Plato.GlslWriter` only; no change to the C#/Rust writers or to `plato-src`
  is required by the recommended fix.

## Fix approaches
1. **Map `Angle` to `float` in `NativePrimitives`** (recommended). `Angle` is documented as
   "internally stored as Radians", so `float` *is* the representation. GLSL has no newtypes,
   so this is what a shader author writes by hand. Angle arithmetic, comparison and the
   trig intrinsics all lower to native operators and builtins for free. Cost: the `Angle`
   type distinction is erased in the output, which collapses the two-way conversion
   overloads (see Bedrock).
2. **Emit `Angle` as a one-field struct** (`struct Angle { float Radians; };`). Preserves
   the type distinction and matches the existing hardcoded intrinsic bodies, but GLSL has no
   operator overloading, so every `Angle + Angle`, `Angle * Number` and `Angle < Angle`
   would have to route through emitted free functions, and `TryWriteOperator` would have to
   stop treating them as native. Strictly more code, strictly worse GLSL.
3. Special-case `Angle` in `ComputeStructs()` only (synthesise a field). Same downsides as
   (2) plus a lie in the type model.

## Bedrock
The invariant the writer should hold is: **the set of natively mapped types and the set of
emitted structs together cover every type that appears in an emitted signature** — a type
that is neither must cause a skip, never a dangling reference. Today `TryGetIntrinsicBody`
violates that from the other side: it hand-writes `Angle(...)` and `.Radians` for a type
the writer never maps or emits, so the invariant only holds by accident (the type check
fires first). Mapping `Angle -> float` and rewriting those three intrinsic sites in the same
change puts the intrinsic table back under the same type map as everything else, which is
what makes the next primitive (`Time`? `Ratio`?) a one-line addition rather than an
archaeology exercise. Verdict: **simplest-along-the-grain**.

What the simple fix must NOT do: it must not leave `Angle` half-mapped — no intrinsic body
may name `Angle(...)` or `.Radians` after the change, and no new representation may be
introduced for `Angle` anywhere except `NativePrimitives`.

## Done means
- [x] `Angle` maps to `float`; zero skips reading `type 'Angle' is not representable`
- [x] `Angle.Radians` is an identity intrinsic; `Angle.Cos/Sin/Tan` and
      `Number.Acos/Asin/Atan/Atan2` no longer emit `Angle(...)` or `.Radians`
- [x] Full-stdlib regen improves emitted/skipped vs the 1259/939 baseline, with the
      Angle-rooted struct drops (ColorHSV, PolarCoordinate, Arc, ...) gone
- [x] No new skip *category* appears, or any new one is understood and recorded here
- [x] `submodules/Plato/demos/glsl/gen.ps1` still emits the demo libraries at 0 skips
- [x] `Plato.GlslWriter/README.md` type-mapping table lists `Angle -> float`

## Outcome (2026-07-27)
Fixed by approach 1 (`Angle -> float`), plus the duplicate-signature drop is now recorded
instead of swallowed.

Full stdlib, `plato-src`:

| | before | after |
|---|---|---|
| emitted | 1259 | **1433** (+174) |
| skipped | 939 | 960 |
| `type 'Angle' is not representable` | 89 | **0** |
| `field type not representable` (struct drops) | 39 | **14** |
| `type 'X' is not representable` (all) | 404 | 334 |

The skipped count rises by 21 despite 174 more emissions because reaching previously
unreachable functions exposes their *own* reasons (lambdas 300 → 321, unmapped intrinsics
110 → 123, static members 85 → 92) and because one **new skip category was introduced
deliberately**:

- `overload erases to an already-emitted GLSL signature` — **75 entries, all
  Angle-attributable** (measured: zero such collisions exist without the Angle mapping).
  `TryEmitFunction` already dropped duplicate signatures first-wins; it did so *silently*.
  Erasing `Angle` to `float` makes that path live, so the drop is now recorded.

Of the 75: ~51 are `Angle.<numeric op>` (`Add`, `Lerp`, `Sqrt`, `Min`, ...) whose bodies are
byte-identical to the `Number` overload that won — harmless. 20 are curve `Eval`s where the
`Angle` (radians) body won over the `Number` (turns) adapter — verified the *compilable* one
won: had the `Number` adapter won, its body `Eval(curve, Turns(t))` would have been
self-recursive and illegal in GLSL. That outcome is luck, not design (see plato-235).
3 change meaning: `Angle.Turns`, `Angle.Degrees`, `Angle.Gradians` lose to their
`Number -> Angle` inverses, so those names now mean the `Number -> Angle` direction only.
Documented in `Plato.GlslWriter/README.md` under *Erased types*.

All 14 remaining struct drops are `IArray`-rooted (`TriangleMesh3D`, `Polygon`, `Points2D`,
...); none are Angle-rooted. Newly emitted structs include `ColorHSV`, `ColorHSL`,
`SphericalCoordinate`, `PolarCoordinate`, `LogPolarCoordinate`, `CylindricalCoordinate`,
`HorizontalCoordinate`, `GeoCoordinate`, `AnglePair`, `Lissajous`, `Arc`, `Sector`, `Chord`,
`Segment`.

Demos: `demos/glsl/gen.ps1` regenerated all **8** libraries at **0 skips** each (the README
still said "four"; corrected). All 8 compile, link and render in WebGL2 — verified live at
`http://localhost:8912`, every demo reporting `PASS`. The complete text diff across all
eight outputs is exactly the `Angle` representation change and nothing else
(`struct Angle` dropped, `Angle(e)` → `(e)`, `x.Radians` → `x`), so the outputs are
semantically identical; the demo `_core.plato` had declared its own one-field
`type Angle { Radians: Number; }`, which the native map now supersedes.

## Simplest fix
Add `{ "Angle", "float" }` to `NativePrimitives` and update the four intrinsic sites
(`Angle.Cos/Sin/Tan` to `cos({a})`/`sin({a})`/`tan({a})`, add `Angle.Radians` as `{a}`,
drop the `Angle(...)` wrapper from `Number.Acos/Asin/Atan` and `Atan2`).
- What you get: ~150 previously skipped functions and ~a dozen structs, at native GLSL
  quality (no wrapper struct, no accessor calls).
- What you give up / risk: `Angle` and `Number` become the same GLSL type, so overloads
  distinguished only by `Angle` vs `Number` collapse to one signature. The real cases are
  `Turns`/`Degrees`/`Gradians`, declared in `plato-src/angles.plato` in *both* directions
  (`Number -> Angle` scales by `TwoPi`; `Angle -> Number` divides by it). `TryEmitFunction`
  resolves duplicate signatures by first-wins and **silently** drops the loser, so one of
  each pair would be emitted with the other's name and no record. Must be surfaced (record
  the drop in `Skipped`) rather than left silent.

## Prevention
- The dangling-intrinsic class of bug (a `TryGetIntrinsicBody` string naming a type or field
  the writer does not emit) is invisible today because the type check masks it. A cheap
  guard: after emission, scan the output for identifiers that are neither declared functions,
  declared structs nor GLSL builtins — worth filing as its own capture-only issue.
- No GLSL output is compiled in CI; the demo `gen.ps1` path is the only compile check and it
  covers four small libraries. A "compile the full stdlib output with a GLSL validator"
  gate would have caught the dangling `Angle(...)` immediately.
