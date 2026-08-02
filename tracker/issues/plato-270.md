---
id: plato-270
title: Differential: Ara3D.Geometry vs Plato stdlib
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [ara3d-056, docs/plato-library-review.md, ara3d-sdk/src/Ara3D.Geometry, submodules/Plato/plato-src]
---

## Idea

Do a structured differential between **`Ara3D.Geometry`** (handwritten + Plato-generated C#) and the **Plato stdlib** (shipping and/or v3): overlapping types, Plato-only vocabulary, C#-only capabilities, and where the "source of truth" should live for each domain. Complements the Plato-internal old/new diff ([plato-269](plato-269.md)) and the capability-lattice idea ([ara3d-056](ara3d-056.md)).

## Assumptions

- Studio/runtime still lean on `Ara3D.Geometry`; Plato is not a full replacement yet.
- Some `Ara3D.Geometry` types are already Plato-generated (`Plato.Generated`); the diff must separate generated vs handwritten.
- [docs/plato-library-review.md](../../docs/plato-library-review.md) is prior honest assessment, not a live inventory.

## Design decisions

- **Which Plato tree** — compare against `plato-src` (what codegen uses), `plato-src-v3` (forward vocab), or both columns.
- **Unit of comparison** — type names vs namespaces/domains vs operations (methods/extension surface).
- **Outcome** — gap list for porting to Plato vs keep-in-C# forever list vs dual-maintain rules.
- **Generated boundary** — treat `Plato.Generated` as Plato side or as a third bucket.

## Related

- [ara3d-056](ara3d-056.md) — capability interfaces in C# aimed at future Plato interfaces.
- [docs/plato-library-review.md](../../docs/plato-library-review.md) — qualitative review.
- [plato-269](plato-269.md) — internal Plato differential; do that first or in parallel with clear columns.
- [ara3d-022](ara3d-022.md) — surfacing geometry as Studio tools (consumer pressure).

## Approaches

Short term: domain-by-domain table (vectors, meshes, SDF, BREP, …) with present/absent/partial on each side.
Long term: policy doc — what must be authored in Plato vs may stay C#.
Adjacent: codegen coverage report (which Plato types never appear in `Plato.Generated` consumers).

## Case against

- **Apples and oranges.** C# has implementation, IO, mutability escapes; Plato is pure — a type-name diff overstates "missing."
- **ara3d-056 already aims here** for the interface lattice; another report may duplicate.
- **Moving targets** on both sides make a static diff rot unless tied to a decision.

**Verdict: pursue** as a **decision-oriented** gap brief (what to port, what not to), not an exhaustive API dump. Prefer after or alongside [plato-269](plato-269.md) so "Plato" means a chosen tree.

## Bedrock

Strengthens the **Plato ↔ Ara3D.Geometry ownership boundary** — which library is allowed to grow which domains. **Verdict: simplest-along-the-grain** — domain tables + keep/port/defer verdicts; must NOT rewrite either library as part of the differential.

## Done means

- [ ] Written differential covering major geometry domains
- [ ] Explicit keep-in-C# / port-to-Plato / defer calls
- [ ] Generated vs handwritten C# called out where it matters

## Simplest possible implementation

One markdown doc: for each `Ara3D.Geometry` folder/domain, note Plato counterpart status and a one-line verdict.

Pros: actionable; bounded  
Cons: qualitative; easy to miss niche types
