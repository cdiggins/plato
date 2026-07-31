---
id: plato-296
title: Add a catalog of common space-warp deformations to Plato
type: idea
status: in-progress
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed:
links: [docs/plato-library-roadmap-ideas.md, submodules/Plato/stdlib/transforms.concepts.plato, submodules/Plato/stdlib/functional.concepts.plato, submodules/Plato/stdlib/fields.concepts.plato, ara3d-sdk/examples/Ara3D.Studio.Examples/Modifiers/Deformers.cs, tracker/issues/studio-197.md, tracker/issues/studio-103.md, tracker/issues/plato-273.md, tracker/issues/ara3d-056.md]
---

## Idea
Ship a Plato stdlib catalog of common **space warps** — pure `Point → Point` maps as first-class values — so any `Deformable2D`/`Deformable3D` type gets Bend/Twist/Taper/etc. for free via the existing `Deform` primitive. Today the formulas live as C# Studio modifiers (`Deformers.cs`); Plato has the concept seam but no named warp library. Interpretation: this is the **analytic warp catalog** (Barr-style, noise displace, FFD-lite), not mesh-edit deformers (Laplacian, cage-solve) which belong elsewhere.

## Assumptions
- `Deformable3D.Deform(Self, Function1<Point3D, Point3D>)` remains the application seam (`stdlib/transforms.concepts.plato`).
- Named warps are useful as **first-class values** (compose, invert where possible, feed SDF domain ops and mesh deformers from one definition).
- Weights / falloffs are **scalar fields**, not parameters baked into every warp — see § Weights below.
- Scalar-field falloff is desirable but not required for v1 — unweighted warps still earn their keep.
- Studio modifiers can thin to wrappers over Plato-generated warps once bodies ship (legacy or forward stdlib).

## Design decisions
- **Expression shape — decided 2026-07-29.** Parametric structs implementing a thin concept, not free-function-only catalogs.
  ```plato
  concept Deformation3D inherits Procedural<Point3D, Point3D> { }
  concept Deformation2D inherits Procedural<Point2D, Point2D> { }

  type Twist3D implements Deformation3D { Axis: Vector3; Amount: Angle; }
  // Eval inherited from Procedural — no Map, no second verb
  ```
  Rationale: pairs with `Deformable*`; reuses `Eval` (house verb for function-like values); discoverable; constrains `Compose` / weighted apply later. Name is `Deformation3D`/`Deformation2D`, not bare `Deformation` or `Warp`/`Modifier`.
- **Eval argument order — decided 2026-07-29.** Self (deformation) first, point second: `d.Eval(p)`, matching every other `Procedural`. Do **not** require both orders on the concept. Optional library sugar only: `Deform(p: Point3D, d: Deformation3D) => d.Eval(p)` so `p.Deform(twist)` parallels `p.Transform(m)`.
- **Apply lift.** One library overload bridges concept → existing seam:
  `Deform(geom: Deformable3D, d: Deformation3D): Self => geom.Deform(p => d.Eval(p))`.
- **Weights / falloff — decided 2026-07-29.** Do not put weight on `Deformation3D`. Modulate via `ScalarField3D` (already `Field<Point3D, Number>` → `Procedural`): one combinator produces a new deformation or is an apply overload — see § Weights. Must not bake falloff into each warp's fields.
- **Multiply / Compose — decided 2026-07-29.** `Compose(first, second)` applies first then second (same as transforms). `Multiply(a, b)` aliases Compose (enables `a * b`). `Multiply(d, t: Number)` / `Multiply(t, d)` scale strength via `p.Lerp(d.Eval(p), t)`. Results are `MappingDeformation2D/3D` carriers holding a `Function1` (concept fields are not available).
- **Normalized domain** — still open: world units vs unit-box `[0,1]³` via `InverseLerp(bounds)` (Studio Twist/Skew pattern). Unit-box reusable across meshes; world-axis clearer for SDF domain warps.
- **Normals / Jacobian** — still open: document "recompute normals after Deform" vs `DeformWithJacobian` for analytic warps with closed-form derivatives.
- **Where it lives** — new `deformations*.plato` in forward `stdlib/` vs bodies first in `stdlib-legacy`. Prefer forward vocabulary + legacy bodies only if codegen consumers need them now.
- **SDF reuse** — same Twist/Bend as IQ domain ops on distance fields vs separate mesh-oriented APIs. Prefer one warp library consumed by both.

## Weights / falloffs / scalar fields
Universal modulator (roadmap §0.1): a weight is just a `ScalarField3D` — sphere/box/cone falloff, painted soft-select, noise mask, distance-to-mesh, analysis heatmaps. Same `Eval` vocabulary as deformations.

Core identity (pointwise blend toward the warped point):
```plato
// Combinator: deformation × weight → deformation (keeps Deform(geom, d) unary)
type WeightedDeformation3D implements Deformation3D
{
    Warp: Deformation3D;   // or existential / generic once available
    Weight: ScalarField3D;
}
Eval(w: WeightedDeformation3D, p: Point3D): Point3D
    => p.Lerp(w.Warp.Eval(p), w.Weight.Eval(p));

// Or apply overload (same math, no wrapper type):
Deform(geom: Deformable3D, d: Deformation3D, w: ScalarField3D): Self
    => geom.Deform(p => p.Lerp(d.Eval(p), w.Eval(p)));
```
Prefer the **combinator type** when graphs need to store "twist with sphere falloff" as one value; prefer the **overload** for call-site convenience. Both are thin; neither changes `Twist3D` itself.

Falloff catalog is separate content under `ScalarField3D` (e.g. `SphereFalloff`, `BoxFalloff`, `SmoothStepRamp`), reusable by cloners, colorize, soft-select — not deformer-specific. Clamp / remap weight to `[0,1]` once in the combinator or as field adapters (`Clamp01`, `Invert`, `Multiply` fields).

## Related
- [docs/plato-library-roadmap-ideas.md](../../docs/plato-library-roadmap-ideas.md) §0.1, §3.1 — prior brainstorm (effectors + warp catalog).
- [transforms.concepts.plato](../../submodules/Plato/stdlib/transforms.concepts.plato) — `Deformable2D`/`Deformable3D`.
- [functional.concepts.plato](../../submodules/Plato/stdlib/functional.concepts.plato) — `Procedural.Eval` (inherited by deformations).
- [fields.concepts.plato](../../submodules/Plato/stdlib/fields.concepts.plato) — `ScalarField3D` (weight / falloff side).
- [Deformers.cs](../../ara3d-sdk/examples/Ara3D.Studio.Examples/Modifiers/Deformers.cs) — Twist, Skew, Spherify, Cubify, Noise (C# formulas to port).
- [studio-197](studio-197.md) — gizmos for Taper/Bend/Twist/Skew (Studio UX; wants Plato-backed math).
- [studio-103](studio-103.md) — Deform taxonomy (Bend, Twist, Taper, Squeeze, Lattice, Noise).
- [plato-273](plato-273.md) — Noise stdlib (Noise displace depends on it).
- [ara3d-056](ara3d-056.md) — capability lattice / `IDeformable3D` notes.

## Approaches
Short term: `Deformation3D`/`2D` concepts + Barr trio + Shear as param structs with `Eval`; identity-at-zero laws; `Deform(geom, d)` lift.
Long term: `WeightedDeformation3D` (or apply overload) + falloff field catalog; FFD lattice; path/surface deform; analytic Jacobians; Studio modifiers become one-liners; SDF domain warps share the same `Eval`.
Adjacent ideas worth their own issue:
- Scalar falloff / effector field catalog under `ScalarField3D` (roadmap §0.1) — shared by deform, clone, colorize.
- Path deform / curve-space warp (needs RMF frames — roadmap §2.3).
- Soft-select + paint-weight channels (`mesh-attributes.plato` already sketches influence weights) as `ScalarField3D` adapters.

## Case against
- **Duplicate of Studio scripts** — deformers already exist as hot-reload C#; Plato port may not unlock new demos until Studio rewires. Counter: one definition for C#/GLSL/conformance; Studio scripts are the wrong source of truth for math.
- **Catalog sprawl** — dozens of named warps without composition primitives. Counter: keep the first cut to Barr + Shear + Wave + Spherify; treat FFD/path as separate issues.
- **Function-valued fields still thin** — without solid `Procedural` value story in graphs, named warp types may be awkward to store dynamically. Counter: parametric structs + `Eval` work today; existentials/lambdas are a later optimization.
- **Verdict: pursue** — thin, high-leverage content on an existing seam; park FFD/path/solver deformers until the Barr catalog earns its keep.

## Bedrock
Strengthens the **`Deformable*.Deform` + `Procedural.Eval` seams**: every warp is `Deformation3D` (= `Procedural<Point3D,Point3D>`), applied only through `Deform`; weights are `ScalarField3D` composed outside the warp. Same grain as `Transform` lifting through `Deform` in `intervals-transforms-transformable.library.plato`. **Verdict: simplest-along-the-grain** — must NOT invent a second apply path; must NOT bake falloff/bounds into each warp's fields; must NOT add `Map` beside `Eval`.

## Done means
- [x] `Deformation2D`/`Deformation3D` concepts exist (thin `Procedural<Point*,Point*>` aliases) in forward stdlib.
- [x] Twist, Bend, Taper, Shear are param structs implementing `Deformation3D` (plus Shear2D/Taper2D/Twist2D/Spherify3D); zero rate/strength is identity by construction.
- [x] `Deform(Deformable3D, Deformation3D)` lift works without type-specific geometry code (also 2D + weighted ScalarField overloads + point sugar).
- [x] Documented contract for normals (recompute after deform; no Jacobian yet) in `deformations.library.plato`.
- [ ] Studio `Deformers.cs` Twist/Skew either call into the Plato surface or are explicitly deferred with a follow-up issue.

## Simplest possible implementation
`Deformation3D`/`2D` concept stubs + four param structs (`Twist3D`, …) with `Eval` bodies in `deformations*.plato`; conformance identity laws; no falloff, no FFD, no Studio wiring.

Pros:
- Immediate reusable math; dogfoods `Deformable3D` and `Procedural`.
- Tiny; portable to GLSL later.
- Falloff slots in later without touching warp types.

Cons:
- No falloff → less demo polish.
- Studio still duplicates until rewired.
- Bend/Taper parameter conventions need one careful choice (Barr vs DCC defaults).
