---
id: plato-299
title: Align Plato stdlib CONVENTIONS with SDK and Studio style guides
type: debt
status: done
priority: p1
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [submodules/Plato/stdlib/CONVENTIONS.md, submodules/Plato/stdlib/STYLE_GUIDE.md, AGENTS.md, ara3d-sdk/AGENTS.md, src/Ara3D.Studio/AGENTS.md, docs/csharp-style-guide-for-agents.md, coding-style.md, ara3d-sdk/CODING-PRINCIPLES.md, tracker/issues/plato-295.md, src/Ara3D.Studio/Ara3DStudio/Features/ViewportLabels/README.md]
---

## Issue

`submodules/Plato/stdlib/CONVENTIONS.md` is the declared single source of truth for forward-stdlib
cross-cutting rules, but it mixes (and under-specifies) two different jobs: **domain / world
semantics** vs **authoring style**. Domain rules need to be brought in line with Ara 3 D Studio
(the product decider) and System.Numerics; authoring style belongs in a sibling style doc, not
stuffed into CONVENTIONS.

## Impact

Agents and stdlib authors open CONVENTIONS first (`stdlib/README.md` cites it). Gaps and
ambiguities (world up axis vs camera view-space wording; style idioms with no home) produce
inconsistent Plato ↔ Studio ↔ SDK geometry and recurring style debt ([plato-295](plato-295.md)).
**Not safe to defer** while vocabulary bodies are still being written.

## User decisions (2026-07-29)

Capture from conversation — treat as requirements for the rewrite:

1. **World frame: Z-up, CCW winding.** Ara 3 D Studio is the decider. Plato must match Studio
   (evidence: `CameraState.Up = (0,0,1)`, axis gnomon, viewport labels README — "World space is
   Z-up"). CCW winding is already stated in CONVENTIONS; keep it and cite Studio as authority.
2. **`Matrix4x4` layout / multiplication understanding = `System.Numerics.Matrix4x4`.** Row-vector
   multiply (`v' = v M`), row storage — already the Plato CONVENTIONS matrix section; pin the
   equivalence to System.Numerics explicitly so C# interop is unambiguous.
3. **Split docs.** Semantic/world rules stay in `CONVENTIONS.md`. Authoring style (array
   literals vs MapRange, named constants, Wikipedia links in comments, commenting style,
   functional idioms from AGENTS) goes in a sibling **style** document — not CONVENTIONS.
   Final name: **`STYLE_GUIDE.md`**.

## Affected code

Domain / product sources (CONVENTIONS):

- Studio Z-up: `src/Ara3D.Studio/Ara3DStudio/Features/ViewportLabels/README.md`,
  `AxisGnomonGizmo.cs` ("Ara 3D is Z-up"), `GizmoMath.cs`, OspRay session comments
- Plato today: `stdlib/CONVENTIONS.md` — CCW + RH present; **View space** section says camera
  local `+Y` up / `-Z` forward (OpenGL view frame) without pinning **world** `+Z` up — clarify
  so it cannot be read as Y-up world
- Matrices: `stdlib/CONVENTIONS.md` "Matrices — row-vector multiplication"; `matrices.plato`
  — align wording with System.Numerics

Authoring sources → style sibling:

- `AGENTS.md`, `coding-style.md`, `ara3d-sdk/AGENTS.md`, `ara3d-sdk/CODING-PRINCIPLES.md`,
  `src/Ara3D.Studio/AGENTS.md`, `docs/csharp-style-guide-for-agents.md`
- Concrete Plato idioms: fixed-arity `[…]` ([plato-295](plato-295.md)), Wikipedia formula links,
  comment density / one-line function docs

Sinks:

- `submodules/Plato/stdlib/CONVENTIONS.md` — domain only after split; add Z-up + System.Numerics pin
- `submodules/Plato/stdlib/STYLE_GUIDE.md` — authoring style
- `submodules/Plato/stdlib/README.md` — point at both

## Cause / analysis

CONVENTIONS.md was introduced for plato-257 domain coordination gates. Authoring style lived in
root/SDK AGENTS and never got a Plato-facing home. World up was left implicit / camera-local
wording only, while Studio has long been Z-up.

## Priority

**p1 (critical)** — user-flagged. Active stdlib authoring + agent traffic.

## Dependencies

- Blocked by: none.
- Unblocks: [plato-295](plato-295.md) optional style note (lands in STYLE_GUIDE).
- Touches: `stdlib/CONVENTIONS.md`, `STYLE_GUIDE.md`, `stdlib/README.md`. Domain edits that conflict
  with Studio require ADR — Studio wins for Z-up / CCW.

## Fix approaches

1. **Split (done):** `CONVENTIONS.md` = Z-up, CCW, System.Numerics matrices, …;
   `STYLE_GUIDE.md` = authoring; README links both; each doc cross-links the other.

## Bedrock

Two seams: (1) **product-aligned world conventions** — Studio decides Z-up/CCW; Plato documents
and matches; matrices = System.Numerics. (2) **authoring style** — same house rules as SDK/
Studio, in a file agents open beside CONVENTIONS. Verdict: **simplest-along-the-grain**.

## Done means

- [x] `CONVENTIONS.md` pins **world Z-up** (Studio as authority) and **CCW** winding; view-space
      camera-local wording clarified so it does not imply Y-up world
- [x] `CONVENTIONS.md` matrix section explicitly equates layout/multiply to `System.Numerics.Matrix4x4`
- [x] Authoring rules live in `stdlib/STYLE_GUIDE.md` — arrays/literals, constants, Wikipedia
      links, commenting, AGENTS-derived functional style — not in CONVENTIONS
- [x] `stdlib/README.md` links both CONVENTIONS and STYLE_GUIDE
- [x] plato-295 array-literal preference recorded in STYLE_GUIDE

## Simplest fix

Clarify Z-up + System.Numerics in CONVENTIONS; add `STYLE_GUIDE.md`; update README; mutual
cross-links at the top of each doc.

## Prevention

- Domain convention changes that affect Studio ↔ Plato: cite Studio (or ADR) in the
  CONVENTIONS edit.
- Style changes: update `STYLE_GUIDE.md` in the same change as new idioms.
- Optional later: CLAUDE.md one-liner "read CONVENTIONS + STYLE_GUIDE before stdlib edits."
