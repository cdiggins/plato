---
id: plato-325
title: "Triage the 35 LINT013 findings: interfaces with unreachable derived surface"
type: debt
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed:
links: [submodules/Plato/PlatoCompiler/Analysis/Linter.cs, tracker/issues/plato-315.md, tracker/issues/plato-308.md]
---

## Issue

`LINT013` (added under plato-315, `d1eb8df`) reports an interface that no concrete type
implements but that library bodies dispatch on: derived API no caller can reach. On the
forward stdlib it reports **35 interfaces**, and `LINT008` — the Info-severity "no concrete
implementer" rule it splits from — drops to **zero**.

That second number is the finding. `LINT008` was Info by explicit design, on the reasoning
that implementor-less interfaces "describe the shape of the vocabulary rather than a defect."
There is no such benign population: every implementor-less interface in the folder has bodies
hanging off it. The Info tier was hiding 35 unreachable surfaces, not documenting 35
declarations-ahead-of-implementation.

## Impact

Between them the 35 interfaces carry roughly 150 derived library functions that no concrete
type can currently reach. Some are load-bearing-looking API a user would expect to work:

- `RayIntersectable2D` — 9 bodies
- `DifferentiableCurve2D` — 8; `DifferentiableSurface` — 7
- `PeriodicCurve` — 6; `Kinematic2D` / `Kinematic3D` — 6 each
- `Sliceable` — 5; `ParameterDomain` — 5; both `DifferentiableScalarField*` — 5 each
- the container family (`Concatenable`, `SetLike`, `MapLike`, `StackLike`, `QueueLike`) — 1 each

Nothing is broken at runtime today because the forward stdlib does not execute at all
(plato-308), which is precisely why this accumulated unnoticed.

## Affected code

- `submodules/Plato/PlatoCompiler/Analysis/Linter.cs` — `LINT013` and the `LINT008` split.
- Interface files carrying findings: `algebra-metric.interfaces`, `collections-containers.interfaces`
  (6), `core-logic.interfaces`, `curves-capabilities.interfaces` (4), `fields-differentiable.interfaces`
  (4), `fields-time-varying.interfaces` (4), `functional.interfaces` (3), `kinematics.interfaces` (2),
  `paths.interfaces`, `pointclouds-voxels.interfaces`, `rigid-dynamics.interfaces` (2),
  `spatial-queries.interfaces` (3), `surfaces-solids.interfaces` (2), `transforms.interfaces`.

## Cause / analysis

Three distinct causes are mixed in the 35, and triage means sorting them apart — a blanket
"implement everything" or "delete everything" answer is wrong:

1. **Genuinely missing `implements` clauses.** A concrete type satisfies the interface's members
   but never declares it. Cheapest fix, no new bodies.
2. **Nominal-vs-structural satisfaction.** `Transformable<TTransform>`
   (`transforms.concepts.plato:35`) is the clearest case: **no type anywhere declares
   `implements Transformable`**. A `Deformable2D` satisfies it implicitly through the thirteen
   derived `Transform(self: Deformable2D, transform: X)` bodies in
   `intervals-transforms-transformable.library.plato`, whose own header comment says a
   Deformable "also satisfies `Transformable<T>` for all thirteen T" (:41). If dispatch is
   nominal — which `GetImplementers` and `LINT001` both imply — then `ThenTransform` is
   unreachable despite that comment, and the fix is a real `implements` clause somewhere.
   **This sub-question should be settled first**, because the answer reclassifies every
   generic interface in the list (`Periodic`, `PeriodicCurve`, `Transformable`).
3. **Vocabulary genuinely ahead of its types.** The container interfaces (`SetLike`, `MapLike`,
   `StackLike`, `QueueLike`) have no backing types at all — the `Dimensioned` situation, where
   retiring the interface with its bodies is the honest move.

## Fix approaches

1. **Settle nominal-vs-structural first, then sort the remaining 34 into the three buckets
   above and land one commit per bucket** (preferred). The classification is most of the work;
   each fix is then mechanical.
2. **Drive it off the number: work down from the highest body count** (RayIntersectable2D 9,
   DifferentiableCurve2D 8, DifferentiableSurface 7). Recovers the most reachable API per
   commit, but keeps re-deciding the same classification question per interface.
3. **Do nothing until plato-308 lands.** Defensible — none of this can be *executed* wrong
   today. Rejected as the primary plan: the pile is what made `MeshIncidence` invisible, and it
   grows with every new interface.

## Priority

p2 proposed. No runtime consequence while plato-308 blocks execution, so not p1 — but this is
the measurement that says how much of the forward vocabulary is currently decorative, and it
wants answering before the conformance suite goes green and turns 150 unreachable functions
into 150 untested ones.

## Dependencies

- Cause 2 (nominal vs structural interface satisfaction) is a language-semantics question; it may
  want its own ADR rather than being decided inside this issue.
- plato-308 gates any runtime verification of the bodies this issue makes reachable.

## Done means

- [ ] Nominal-vs-structural satisfaction settled and written down (issue note or ADR).
- [ ] All 35 findings classified into missing-`implements` / implicit-satisfaction / retire.
- [ ] Each bucket landed; `LINT013` count reduced to the deliberate remainder, with any
      remaining findings justified in this issue.
- [ ] `tools/check-stdlib-fast.ps1` passes after each commit.

## Simplest fix

Take only the retire bucket (the container interfaces with no backing types): deletes interfaces
plus their bodies, drops the count, and needs no semantics decision.

## Prevention

`LINT013` itself is the prevention — it is a Warning, so it lands in the lint ratchet metric
rather than the Info pile that hid this class. Keep it that way.
