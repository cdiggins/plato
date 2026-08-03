---
id: plato-437
title: Cloth: mass-spring substeps are quadratic, and CollideWith keeps only one of three overloads
type: bug
status: ready
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

Two defects found by the cloth demo page (`plato-426`'s demo box), the first
thing to execute `stdlib/future/cloth.library.plato`. **The solver itself is
correct** — a 10x10 sheet settles and holds maximum stretch at 1.012 x rest for
twelve seconds, PBD holds where the explicit solver runs away, and contacts land
exactly on `Plane.Distance + Thickness`. These are the two things around it.

## 1. Mass-spring substeps are quadratic

`SubStepMassSpring`'s output is a lazy `MapRange`, and the next substep reads it
O(vertices x springs) times before anything materializes it. Measured at 10x10:
**1.1 s per frame at two substeps against 9 ms at one.**

PBD is unaffected — its substeps each end in an eager sweep — so this is
specific to the explicit path. It is an instance of `plato-436` (the general
lazy-`Arr` defect) and may be fixed by fixing that; it is recorded separately
because the mass-spring solver's whole purpose is to be the comparison against
PBD, and a comparison that cannot run above one substep is a weaker comparison
than the library intended.

## 2. CollideWith keeps one of three overloads, and one loss silently works

Both `Cloth3D.CollideWith` and `ClothMesh3D.CollideWith` keep only the `Sphere`
form; the `Plane` and `ISignedDistanceField3D` forms carry
`// Skipped: overload or duplicate member` (plato-419 defect 3).

The surviving body is
`Vertices.Map(v => v.MovedTo(argument.ProjectOutOf(v.Position, Thickness.Meters)))`,
and `Plane.ProjectOutOf(point, offset)` happens to have exactly that arity — so
**a plane argument runs the correct plane body by accident of matching shape**.
The demo agent verified this numerically rather than assuming it: the resting
height lands on `Plane.Distance + Cloth3D.Thickness` to three decimals at two
different thicknesses, which no other body produces.

**This is luck, not correctness.** The emitted signature says `Sphere`, so the
call needs a cast, and any future change to either overload's arity turns a
working call into a silently wrong one with no diagnostic. The SDF form takes an
extra `epsilon` and gets no such luck, so nothing can use it.

Two further skips are harmless and are recorded only so nobody re-investigates
them: `Cloth3D.WithVertices` and `ClothVertex.WithPinned` are dropped as
duplicates of the record `With` functions, which are byte-identical.

## Done means

- [ ] Two substeps of the mass-spring solver cost about twice one substep
- [ ] `CollideWith` emits its `Plane` and SDF overloads, so the plane case works
      by dispatch rather than by arity coincidence
- [ ] A cloth scene collides against an `ISignedDistanceField3D`
