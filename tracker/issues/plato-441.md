---
id: plato-441
title: "TypeScript writer silently drops same-name overloads: Ray3D.Intersect(Triangle3D) never reaches the output"
type: bug
status: ready
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-04
closed:
links: [plato-439]
---

## Problem

The TypeScript writer claims one name per receiver type: the first function wins and
every later overload is replaced by a `// Skipped: overload or duplicate member`
comment. TypeScript itself does not have this limit — a class method may declare
several overload signatures over one implementation, and the writer already knows
every signature at emit time.

The number of dropped members in one generated file is the
`Skipped: overload or duplicate member` comment count in
`demos/typescript/geometry-samples/src/plato/plato.g.ts`. The drop is silent at the
call site: the name resolves to whichever overload happened to be emitted, so a
consumer sees a type error about the argument rather than "this overload does not
exist", and no diagnostic distinguishes the two.

Found from the consumer seat (plato-439): `Intersect(r: Ray3D, tri: Triangle3D)` was
added to `lines.library.plato` beside the existing `Intersect(r: Ray3D, pl: Plane)`
and never appeared in the output. It was renamed to `Raycast(tri: Triangle3D, ...)` —
receiver moved so the name is unique — to get a TypeScript surface at all. That is a
library shape chosen to dodge a writer limitation, which is backwards.

## Approach

Group functions by (receiver, name) rather than claiming names one at a time. Emit
TypeScript overload signatures plus one implementation signature whose parameters are
the union of the group, dispatching on an argument type test — the same discriminator
the C# writer gets for free from real overload resolution. Where overloads differ only
in arity, the union collapses to optional parameters.

The array-function path (`WriteArrayMethod` / `WriteFreeArrayFunction` in
`TypeScriptTypeWriter.cs`) claims names the same way and needs the same treatment.

## Done means

- [ ] Two functions differing only in parameter types are both callable from
      generated TypeScript.
- [ ] `Raycast(tri: Triangle3D, r: Ray3D)` in `stdlib/geometry/lines.library.plato`
      moves back to `Intersect(r: Ray3D, tri: Triangle3D)`, beside its plane sibling.
- [ ] Writer behaviour covered by a test.
