---
id: plato-410
title: FieldJet2D/3D: a value+gradient jet type for differentiable scalar fields
type: idea
status: idea
priority: p3
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-02
closed:
links: [tracker/issues/plato-409.md, stdlib/geometry/fields.concepts.plato, stdlib/geometry/implicit-sdf.library.plato]
---

## Idea

Spun out of [plato-409](plato-409.md), which landed everything else in its scope and
deliberately left this. A *jet* is the mathematical name for a truncated Taylor expansion at
a point; the 1-jet is value plus first derivative. Today a scalar field reports only the
value, and a caller who wants both pays for two independent traversals:

```
type FieldJet2D implements IValue { Value: Number; Gradient: Vector2D; }
type FieldJet3D implements IValue { Value: Number; Gradient: Vector3D; }

EvalWithGradient(self: ..., point: Point3D): FieldJet3D
```

Named for *fields* rather than SDFs: the pair is the natural return of any differentiable
scalar field — noise, potentials, metaballs — and the signed-distance case is only the one
where the gradient additionally means "outward surface normal".

## Assumptions

- A caller exists that wants value and gradient at the same point often enough for the
  double traversal to show up. Shading a ray-marched image is that caller; nothing in the
  repo does it yet.
- The gradient stays first-order. A second-order jet (adding the Hessian, hence curvature)
  is a strictly larger design and belongs in its own issue if wanted.

## Design decisions

- **Is the double traversal actually expensive?** For `FunctionSdf*` it is two lambda calls
  and the numeric gradient already costs six, so a jet saves nothing there. It pays for
  `SdfTree*` (each `Eval` walks the whole tree) and `SampledSdf*` (each `Eval` refetches a
  lattice neighbourhood). **This should be measured before the type is built**, and the
  measurement needs a ray marcher rendering something, which now exists (`RayMarch`,
  `implicit-sdf.library.plato`).
- **The parallel operator family is the real cost.** Every `*Distance` kernel in
  `implicit-sdf.library.plato` — `UnionDistance`, `IntersectionDistance`,
  `SmoothUnionDistance`, and the rest — needs a jet-valued twin, and the two must stay in
  sync forever. That doubling, not the type, is what to weigh.
- **Seam behaviour.** `UnionJet(a, b) => a.Value < b.Value ? a : b` propagates the winning
  branch's gradient. On the tie seam this is a subgradient — a legitimate one-sided choice,
  which the doc comment must say rather than pretend the gradient exists there. The smooth
  operators blend both gradients by the same weight they blend values and are differentiable
  everywhere; this is the concrete argument for preferring them.
- **Domain-warping modifiers need a Jacobian.** `ApplyToDomain` on the twist / bend /
  repetition / elongation modifiers reshapes the position, so a jet passing through one must
  pull the gradient back through that map's derivative. Those Jacobians do not exist today
  and are the largest genuinely new work in this issue. Overlaps [plato-296](plato-296.md).

## Related

- [plato-409](plato-409.md) — parent. Landed `IDifferentiableSignedDistanceField2D/3D`,
  `GradientAt` bodies, `PlacedSdf*`, `RayMarch`, and the planar primitive family. Its
  "Design decisions" section records why the jet was deferred rather than dropped.
- [stdlib/geometry/implicit-sdf.library.plato](../../stdlib/geometry/implicit-sdf.library.plato) —
  the `GradientAt` bodies a jet would fold into `Eval`, and the distance-operator family a
  jet mirror would parallel.
- [plato-296](plato-296.md) — space-warp catalog; shares the Jacobian requirement above.

## Approaches

Short term: nothing, until a profile of `RayMarch` over an `SdfTree3D` says the second
traversal costs something. The measurement is the deliverable, not the type.

Long term: forward-mode automatic differentiation over a dual-number scalar would let every
primitive be written once and differentiated for free, retiring both the hand-written
`GradientAt` bodies and the jet operator mirror. That needs generic-over-numeric support the
language may not have — worth a language-side spike before assuming it.

## Bedrock

Same seam as the parent: **what a field reports at a point**, owned by
`fields.concepts.plato`. Naming the 1-jet puts value-plus-derivative on one declaration
instead of leaving each derivative-consuming caller to re-derive it, and makes a
second-order jet an extension of an existing shape rather than a second invention.

Verdict: **right, but not yet.** The architecture argument is real and the trigger is
absent. Building it now would add a parallel operator family maintained against no measured
benefit, which is the specific failure the parent issue avoided.

## Done means

- [ ] A profile of `RayMarch` over an `SdfTree3D` reports what fraction of time goes to the
      second traversal, so the decision rests on a number rather than on the argument above
- [ ] If that number justifies it: `FieldJet2D/3D` declared, with `EvalWithGradient` on every
      field type that has a `GradientAt` today
- [ ] The jet-level operator mirror covers every `*Distance` kernel, with the seam
      subgradient documented on each Min/Max-derived one
- [ ] `plato_check` clean

## Simplest possible implementation

Declare the two types and `EvalWithGradient` for `SdfTree3D` and `SampledSdf3D` only — the
two forms where one traversal genuinely beats two — and leave every other field type to the
existing `Eval` + `GradientAt` pair. No operator mirror at all until a composition is what
the profile blames.

**What you get** — the saving where it exists, on two types, with no parallel family to
maintain.

**What you give up / risk** — jets do not compose through CSG, so a tree of placed and
combined fields cannot carry one end to end; the type looks half-finished to a reader who
expects it everywhere.
