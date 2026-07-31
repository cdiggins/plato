---
id: plato-352
title: Strengthen provenance surface types (revolve/loft/extrude/sweep)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-308, plato-277, plato-336, stdlib/surfaces-generated.plato]
---

## Idea
User wants provenance-preserving surface types: RevolvedSurface, LoftedSurface, ExtrudedSurface, SweptSurface, and similar — types that remember how they were built. **Evaluation:** these types already exist in `stdlib/surfaces-generated.plato` (`SurfaceOfRevolution`, `ExtrudedSurface`, `LoftedSurface`, `SweptSurface`, …) and implement `ParametricSurface`. The live question is quality/completeness of that design (concept obligations, evaluation, mesh, forward-C# compile), not greenfield invention.

## Assumptions
- Provenance types beat baking everything to grids/meshes early (edit distance, parameter tweaks).
- plato-308 shows forward stdlib struggles with these types (CS0315 Self/Curve3D constraints).
- plato-277 lists them among concept-gap / algorithm backlog items.

## Design decisions
- **Keep distinct types vs one GenerativeSurface sum** — distinct preserves provenance; sum eases dispatch.
- **Curve fields** — concrete Curve3D vs concept-constrained Self (root of CS0315).
- **Obligations** — ParametricSurface enough vs Meshable3D/DifferentialSurface required at birth.

## Related
- `stdlib/surfaces-generated.plato` — types already present.
- [plato-308](plato-308.md) — forward compile errors on these types.
- [plato-277](plato-277.md) — concept-gap burn-down mentions these surfaces.
- [plato-336](plato-336.md) — provenance loss on triangulate/hull (parallel theme).

## Approaches
Short term: inventory each generated surface — fields, implements, Eval/Mesh status; fix the worst obligation gaps.
Long term: BREP sum cases citing these (plato-302); deform-preserves-provenance rules.
Adjacent: Solid siblings (ExtrudedSolid, etc.).

## Bedrock
Strengthens the **generative vs tessellated** seam (same as BREP exact-vs-discrete). Verdict: **right** to keep provenance types; quality work is obligation/eval completeness, not adding empty shells.

## Done means
- [ ] Written quality scorecard per type (fields OK? Eval? Mesh? concepts?)
- [ ] Top breakage (e.g. CS0315 pattern) has a filed fix path linked
- [ ] Documented when provenance must decay (Deform policy)

## Simplest possible implementation
Scorecard doc + fix one type end-to-end (ExtrudedSurface) as template.
- Pros: grounds the idea; reusable pattern.
- Cons: other types stay uneven.

## Case against
- Types already exist; filing "I want them" duplicates inventory — work belongs on plato-308/277.
- Provenance types that cannot Eval/Mesh are documentation debt.
- Too many parallel generative forms without shared tests.
- Verdict: **pursue** as a *quality/burn-down* umbrella pointing at existing types; **do not** invent duplicates. Idea quality: **good intent, already partially shipped** — treat as hardening, not greenfield.
