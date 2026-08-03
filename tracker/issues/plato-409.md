---
id: plato-409
title: FieldJet2D/3D: value+gradient jets, differentiable SDFs, transforms, ray marching, and the missing distance primitives
type: idea
status: in-progress
priority: p2
effort: L
risk: low
area: plato
sprint: 
created: 2026-08-02
closed:
links: [stdlib/geometry/implicit-sdf.concepts.plato, stdlib/geometry/implicit-sdf.types.plato, stdlib/geometry/implicit-sdf.library.plato, stdlib/geometry/fields.concepts.plato, stdlib/geometry/fields-implicits.library.plato, tracker/issues/plato-287.md, tracker/issues/plato-296.md]
---

## Idea

The implicit/SDF corner of the stdlib has distance functions but not the vocabulary that
makes them usable. Five gaps, one umbrella, because they share a single seam: **what a
scalar field reports at a point.**

Today a field reports a bare `Number`. The proposal is a *jet* — the mathematical name for
a truncated Taylor expansion at a point, of which the 1-jet is value plus first derivative:

```
type FieldJet2D implements IValue { Value: Number; Gradient: Vector2D; }
type FieldJet3D implements IValue { Value: Number; Gradient: Vector3D; }
```

`FieldJet` is deliberately named for *fields*, not for SDFs — the same pair is the natural
return of any differentiable scalar field (noise, potentials, metaballs), and the SDF case
is just the one where the gradient carries the extra meaning of "outward surface normal".

The five gaps, in the order they should land:

1. **Transforms.** No `Transformed(sdf, transform)` exists anywhere in the stdlib. Every
   distance primitive is welded to the origin. Fixed by inverse-transforming the query
   point; a uniform scale also divides the returned distance.
2. **Differentiable SDF interfaces.** `fields.concepts.plato` already declares
   `IDifferentiableScalarField2D/3D` with `GradientAt`, and `fields-implicits.library.plato`
   already derives `SurfaceNormalAt` / `BoundaryNormalAt` / `SlopeAt` /
   `SteepestAscentDirectionAt` / `IsStationaryAt` from it. `ISignedDistanceField2D/3D` do not
   inherit it, so none of that reaches an SDF. Connecting the two is declaration-only.
3. **Ray marching.** Nothing marches a ray against a distance field, though `RayHit3D` and
   `RayQuery3D` already exist as the result and query types. This is the feature that turns
   the SDF collection into something renderable, and it consumes (2) for the hit normal.
4. **`FieldJet` proper.** Value and gradient from one traversal, and a jet-level mirror of
   the existing distance-operator family so derivatives compose exactly rather than by
   finite difference.
5. **The missing primitives.** 3D has sphere / box / rounded box / torus / capsule / capped
   cylinder / plane. 2D has *none*, which breaks the 2D-3D symmetry every other geometry
   pair in the stdlib maintains. 3D is additionally missing cone, capped cone, and ellipsoid.

## Assumptions

- The gradient is wanted mainly as a **normal**: shading, offsetting, contact response, and
  projecting a point onto the surface. If that stops being true the jet loses most of its
  value.
- A conservative/approximate gradient is acceptable where an exact one is unavailable —
  `SampledSdf2D/3D` can only ever difference its lattice.
- Ray marching is the intended consumer. If SDFs stay unrendered, (3) and therefore much of
  (4)'s performance argument evaporate.
- `Transform3D` / `Transform2D` (or the pose types) can be inverted cheaply enough to sit
  inside a per-sample call. If inversion is expensive, the transform wrapper must store the
  inverse, not the forward transform.

## Design decisions

- **Jet as a type vs. two calls** — a `FieldJet3D` return means one traversal of an
  `SdfTree3D` and one neighborhood fetch for a `SampledSdf3D`, versus two for separate
  `Eval` / `GradientAt`. Against: a second parallel operator family to keep in sync with the
  existing `UnionDistance` / `SmoothUnionDistance` set, and callers who only want distance
  pay for a `Vector3D` they discard. **Decision: ship (1)–(3) first and let a real ray
  marcher tell us whether the double traversal actually costs anything.**
- **Where the differentiable interface attaches** — a new
  `IDifferentiableSignedDistanceField2D/3D` inheriting both `ISignedDistanceField*` and
  `IDifferentiableScalarField*` (mirrors the `IBoundedSignedDistanceField*` pattern that
  plato-287 established), versus making `ISignedDistanceField*` differentiable outright.
  The latter is a lie for `SampledSdf*` and for any lower-bound field. **Decision: separate
  interface, following the bounded precedent.**
- **Numeric fallback** — a `NumericGradientAt(self, point, epsilon)` body on plain
  `ISignedDistanceField*` (central difference, six evals; or the four-eval tetrahedron)
  gives every field a normal at 4-6x evaluation cost. Worth having as an explicit,
  explicitly-named fallback; not worth making the default.
- **Non-differentiability at CSG seams** — plain `Union`/`Intersection` are `Min`/`Max`, whose
  gradient is undefined on the seam where the two branches tie. The jet must pick a side
  (a subgradient) and the doc comment must say so. The smooth variants are differentiable
  everywhere, which is a real argument for preferring them that the stdlib does not
  currently make.
- **Transform and exactness** — non-uniform scale destroys the distance property (the result
  is a bound, not a distance). Either reject non-uniform scale at the type level or document
  the result as a lower bound. The stdlib already has "exact distances or conservative lower
  bounds" language in `implicit-sdf.concepts.plato` to lean on.

## Related

- [plato-287](plato-287.md) — added `BoundedSdf2D/3D` and the `IBoundedSignedDistanceField*`
  interfaces. Its "interface + wrapper, bounds are hints not semantics" shape is the
  precedent this issue follows for the differentiable interfaces. Its own "Adjacent" note
  already called for porting analytic primitives, which is gap (5) here.
- [plato-296](plato-296.md) — space-warp deformation catalog. Overlaps gap (1) and the
  domain-warping SDF modifiers (`SdfTwistModifier3D`, `SdfBendModifier3D`,
  `SdfElongationModifier*`, `SdfRepetitionModifier*`) that already live in
  `implicit-sdf.types.plato`. A jet through a domain warp needs the warp's Jacobian, so if
  both land the warp catalog should carry one.
- [stdlib/geometry/fields.concepts.plato](../../stdlib/geometry/fields.concepts.plato) —
  `IDifferentiableScalarField2D/3D`, the interfaces gap (2) hooks into.
- [stdlib/geometry/fields-implicits.library.plato](../../stdlib/geometry/fields-implicits.library.plato) —
  the gradient-derived helper bodies that light up for free, and the existing SDF
  query surface (`DistanceAt`, `IsInside`, `Contains`, …).
- [stdlib/geometry/implicit-sdf.library.plato](../../stdlib/geometry/implicit-sdf.library.plato) —
  the distance-operator and primitive families a jet mirror would parallel.

## Approaches

**Short term** — land in dependency order, each independently useful:

1. `Transformed` on the SDF wrappers plus a `TransformedSdf2D/3D` type. Unblocks everything
   and needs no new concepts.
2. The two `IDifferentiableSignedDistanceField*` interfaces plus analytic `GradientAt`
   bodies for the primitives (sphere is `(p - center).Normalize`; plane is its own normal).
   Declaration-heavy, logic-light.
3. `RayMarch` / sphere tracing returning the existing `RayHit3D`.

**Long term** — `FieldJet2D/3D` and the jet-level operator mirror; second-order jets (adding
the Hessian) would give curvature and hence curvature-adaptive meshing; forward-mode
automatic differentiation over a dual-number scalar would let the primitives be written once
and differentiated for free, but that needs generic-over-numeric support Plato may not have.

**Adjacent, worth their own issue if pursued:**

- Mesh extraction (marching cubes / dual contouring) from an SDF — the other major consumer
  besides ray marching, and it also wants normals.
- `EikonalResidualAt`: exact distance fields satisfy `|∇f| = 1`, so gradient magnitude is a
  free correctness probe on any field claiming exactness. One line, once (2) lands.
- SDF ambient occlusion and soft shadows — cheap and famous, but arguably renderer policy
  rather than stdlib geometry.

## Operator survey against Inigo Quilez's catalog

Checked the library against https://iquilezles.org/articles/distfunctions/ and its 2D
companion. What was already present: the four Boolean combinations, the three smooth ones,
round, onion, elongate, displace, twist, cheap-bend, repetition and limited repetition.

What was missing, and why the gaps clustered where they did:

- **The 2D-to-3D lifts — extrusion and revolution.** These are the structural gap. They are
  how a planar profile becomes a solid, and without them the planar primitive family this
  issue added would have been decorative. Both landed as portable pieces rather than
  field-level combinators: extrusion as a VALUE operator (`ExtrudedDistance`, taking an
  already-sampled planar distance plus the out-of-plane coordinate) and revolution as a
  DOMAIN operator (`RevolvedPoint`, mapping a spatial point to the planar query point). That
  split keeps both lambda-free, which is what lets them port to GLSL/C++/CUDA alongside the
  primitives.
- **Mirroring** (`opSymX` / `opSymXZ`): pure domain folds, landed for both dimensions.
- **Scale and transform** (`opScale`, `opTransform`): covered by `PlacedSdf*`.
- **The smooth family was 3D-union-only.** `SmoothUnion` existed for `FunctionSdf3D` and
  `BoundedSdf3D` and nothing else — no planar form, and no smooth intersection or difference
  at any level despite the scalar kernels for both existing in `implicit-sdf.library.plato`
  the whole time. Also no `ExclusiveOr` above the scalar layer, and no planar `Shell`. All
  filled in, at both the `FunctionSdf` and the `BoundedSdf` level, with bound propagation for
  the latter.
- **Not ported:** chamfer / stairs / column unions, which are from Mercury's `hg_sdf` rather
  than IQ, and the exotic primitives (octahedron, solid angle, triangle, quad). None of them
  block anything; they are catalog-filling work whenever it is wanted.

`ScalarFunctionField2D/3D` in `fields-implicits.library.plato` keeps the same
3D-smooth-union-only asymmetry the SDF family had. Left alone deliberately: those are generic
scalar fields, where CSG is a borrowed metaphor rather than the point of the type.

## Bedrock

The seam is **what a field reports at a point**, owned by `fields.concepts.plato`. Today it
is a bare `Number`, and every derivative-consuming caller is forced to re-derive gradients
by finite difference on its own terms. Naming the 1-jet as a type puts that on one
declaration, and makes second-order jets an extension of an existing shape rather than a
second invention.

The transform gap strengthens a different and more urgent seam: right now the *only* way to
place an SDF is to bake position into the primitive's parameters, which is why the primitive
list keeps wanting to grow. A transform wrapper makes each primitive canonical and lets
placement compose, which shrinks gap (5) rather than feeding it.

Verdict: **simplest-along-the-grain.** The simple version must NOT bake gradient reporting
into `ISignedDistanceField*` itself, and must NOT let `RayMarch` compute normals with its own
private finite-difference helper — that is exactly the duplication the jet exists to prevent.
Ray marching must consume `GradientAt` through the interface from day one, even while
`FieldJet` itself remains unbuilt.

## Done means

- [ ] An SDF primitive can be placed away from the origin by a transform, and the placed
      field's distances agree with the primitive's under the inverse transform
- [x] `ISignedDistanceField2D/3D` implementors can be asked for a gradient, and the existing
      `SurfaceNormalAt` / `BoundaryNormalAt` / `SlopeAt` helpers resolve against an SDF
- [ ] A ray can be marched against a 3D SDF, returning a populated `RayHit3D` whose normal
      comes from the interface gradient, not a private helper
- [x] The 2D primitive family matches the 3D one in coverage
- [x] `plato_check` clean across the touched files (no new lint, type, or style diagnostics)

Boxes 1 and 3 are landed and type-check but are **not executable-verified**: the law runner
(Stage 2 of `tools/regen-forward-conformance.ps1`) does not run yet, blocked on plato-308, and
the field types the two claims are about — `PlacedSdf*`, `FunctionSdf*` — each store a lambda
the law harness cannot instantiate even once it does. `stdlib/tests/implicit-sdf.laws.plato`
covers what the harness CAN construct: the closed-form primitives and the scalar distance
operators. Closing this item needs either plato-308 plus a way to construct a field instance in
a law, or a hand-written test outside the packet.

Box 2 is verified structurally rather than behaviourally: `RayMarch` calls `SurfaceNormalAt`
on an `IDifferentiableSignedDistanceField3D` receiver and the corpus type-checks with zero
failing functions, which is what proves the interface inheritance actually reaches the
gradient-derived helpers in `fields-implicits.library.plato`.

## Simplest possible implementation

Land only steps 1–3 of the short-term plan and stop. Do not build `FieldJet2D/3D`; leave it
as the recorded design for when a profiler asks for it. Concretely: one transform wrapper
type per dimension, two interfaces with no new members, analytic `GradientAt` bodies for the
seven existing 3D primitives, one `NumericGradientAt` fallback body, and one `RayMarch`.

**What you get** — placeable shapes, normals everywhere, and a renderable pipeline, with no
new operator family to keep synchronized and no change to existing distance semantics.

**What you give up / risk** — two traversals per shaded sample, which matters only under a
real renderer; a subgradient ambiguity at CSG seams that has to be documented rather than
solved; and the `SampledSdf*` gradient stays finite-difference-only regardless.
