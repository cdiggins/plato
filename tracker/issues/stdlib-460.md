---
id: stdlib-460
title: Three geometry-tier functions emit C# that does not compile (Contains on an existential, Constants.UniformWeights)
type: bug
status: ready
priority: "?"
effort: "?"
risk: "?"
area: stdlib
sprint: 
created: 2026-10-17
closed:
links: []
---

## Problem

Three geometry-tier bodies produce C# that does not compile under the shipping recipe:

- two `Contains` calls in `Extensions.g.cs` on a value typed by an interface (`ISignedDistanceField3D`, `IImplicitVolume3D`), which only resolve for a concrete `BoundedSdf` (CS1929);
- `TaubinSmoothed` in `Remeshing.g.cs` reads `Constants.UniformWeights`, which does not exist (CS0117).

Every consumer that builds the geometry tier hand-patches the generated files.

## Done means

- [x] The three call sites are fixed in the stdlib source, and the three tiers generate C# with no errors from them.
