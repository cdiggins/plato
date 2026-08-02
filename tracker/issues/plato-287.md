---
id: plato-287
title: Add BoundedSdf (SDF with finite domain bounds)
type: idea
status: done
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed: 2026-07-29
links: [submodules/Plato/stdlib/implicit-sdf.concepts.plato, submodules/Plato/stdlib/implicit-sdf-bounded.plato, submodules/Plato/stdlib/implicit-sdf-bounded.library.plato, submodules/Plato/stdlib/implicit-sdf-sampled.plato]
---

## Idea
**Is there a bounded SDF today?** Not as a first-class analytic interface. `SignedDistanceField2D/3D` are unbounded distance oracles; `SampledSdf2D/3D` carry a `Bounds` because the lattice has finite support, but there is no type/interface for "analytic (or procedural) SDF + conservative finite domain" used for culling, tiling, or CSG acceleration. Ask: should `BoundedSdf2D/3D` exist?

## Assumptions
- Many SDF ops (raymarch, mesh extraction, boolean graphs) benefit from a conservative AABB/OBB outside which the field is known empty or irrelevant.
- Bounds must be **conservative** (may be loose); incorrect tight bounds are worse than none.
- `FunctionSdf2D/3D` and combination trees (`SdfTree*`) are the main analytic forms that would gain optional bounds.

## Design decisions
- **Interface + wrapper** — `BoundedSignedDistanceField2D/3D` inherits SDF + `Bounded2D/3D`; concrete `BoundedSdf2D/3D` wraps `FunctionSdf*` + `Bounds`. `SampledSdf*` implements the bounded interfaces.
- **Semantics outside bounds** — hint-for-culling: `Eval` / `DistanceAt` always evaluate the inner field; `MayIntersect` / `IsCulledBy` / `MayContainPoint` use Bounds only.
- **2D and 3D** — both shipped.
- **CSG propagation** — Union (union of bounds), Intersection (`IntersectionOfBounds`), Difference (keep `a.Bounds`), Offset (grow by `Abs(amount)`), SmoothUnion (union grown by blend radius).

## Related
- [stdlib/implicit-sdf.concepts.plato](../../submodules/Plato/stdlib/implicit-sdf.concepts.plato) — `SignedDistanceField*` / `BoundedSignedDistanceField*`
- [stdlib/implicit-sdf-bounded.plato](../../submodules/Plato/stdlib/implicit-sdf-bounded.plato) + [library](../../submodules/Plato/stdlib/implicit-sdf-bounded.library.plato)
- [stdlib/implicit-sdf-sampled.plato](../../submodules/Plato/stdlib/implicit-sdf-sampled.plato) — `SampledSdf*` implements bounded interfaces
- [stdlib/geometry-kernels.library.plato](../../submodules/Plato/stdlib/geometry-kernels.library.plato) — `IntersectionOfBounds`

## Approaches
Short term: done (wrapper + interface + culling + CSG bound propagation).
Long term: bounds on `SdfTree` roots; mesh-extraction / raymarch APIs prefer bounded forms.
Adjacent: port primitive analytic SDFs from legacy (sphere/box have exact bounds).

## Bedrock
Strengthens the **finite-support seam** for implicits. Unbounded SDF distance semantics unchanged.

## Done means
- [x] Documented answer in stdlib: unbounded SDF vs bounded form, and that SampledSdf already carries Bounds
- [x] If pursuing: `BoundedSdf2D/3D` (or interface) declared + lint clean
- [x] At least one operation uses Bounds for culling (even a trivial `Overlaps(a.Bounds, b.Bounds)` helper)
- [x] Semantics outside bounds written in the type/interface doc comment

## Simplest possible implementation
Shipped: interface + `BoundedSdf*` wrappers + SampledSdf implements + culling ops + CSG with bound propagation + `IntersectionOfBounds`.

## Case against
- Verdict was **pursue** a thin wrapper; landed with light CSG bound propagation as the natural operators on the wrapper.
