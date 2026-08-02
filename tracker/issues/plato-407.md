---
id: plato-407
title: Simplify rules for duplicated bodies an interface already covers
type: idea
status: idea
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-02
closed:
links: []
---

## Idea

The two simplifications landed in `1ae4ecc` and `f5fc7ca` were both found by
hand, and both are mechanically detectable. `plato_simplify` could carry them.

**A concrete body identical to one an implemented interface already derives.**
`Hash(self: SdfNodeIndex) => self.Value.Hash` was byte-identical to
`Hash(self: IIndex)` in `collections.library.plato`, and SdfNodeIndex
implements IIndex. Same for FieldNodeIndex. Detection: for each concrete
function, walk the receiver's interface closure for a same-name, same-remaining-
parameter function whose body matches after renaming the receiver. Report the
concrete one as removable.

**A family of identical bodies whose receivers share an interface.** Seven
`Width(self: <raster>) => self.Size.Width` bodies over the seven IImage types;
twelve `Centroid(self: <shape>) => self.Center`; seven
`ToTriangleMesh(s: <solid>) => s.ToPolygonMesh.ToTriangleMesh`. Detection:
group library functions by (name, body modulo receiver name, remaining
parameters), and for groups above some size report the interfaces every
receiver shares. This one proposes rather than rewrites — collapsing it may
need a new interface, as it did for ToPolygonMesh and Center — so it belongs in
a report mode, not under `apply`.

Note the limit found while doing this by hand (compiler-408): a body written
against an interface-typed parameter binds against the interface, so the second
rule must only propose collapses where the shared body needs no re-resolution.

## Done means

- [ ] Rule 1 implemented, previewed and applied over the shipping tiers
- [ ] Rule 2 implemented as a report
