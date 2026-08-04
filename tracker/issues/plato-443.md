---
id: plato-443
title: "Interface-typed fields bind Self to the containing type: TubeSurface.Path is ICurve3D<TubeSurface>, so no real curve fits"
type: bug
status: ready
priority: p1
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-04
closed:
links: [plato-439]
---

## Problem

The TypeScript writer models a Plato interface's `Self` type as a generic parameter:
`export interface ICurve3D<Self> extends IGeometry3D<Self>, ICurve<Point3D>`. Where a
type has a field of interface type, the writer fills that parameter with the
**containing type**:

```ts
// stdlib: type TubeSurface { Path: ICurve3D; Radius: Number; }
constructor(public readonly Path: ICurve3D<TubeSurface>, public readonly Radius: number) {}
```

`ICurve3D<TubeSurface>` means "a curve whose Self is TubeSurface", which inherits
`Equals(b: TubeSurface): Boolean` among others. No actual curve satisfies it — a
`CatmullRomCurve3D` has `Equals(other: CatmullRomCurve3D)`. The field's declared type
is inhabited by nothing the library can produce, so the type is uninstantiable
without a cast.

The count of interface-typed fields carrying a bound Self parameter in one generated
file is the number of `readonly <name>: I…<…>` matches excluding `<Self>` and `<T>` in
`demos/typescript/geometry-samples/src/plato/plato.g.ts`. Affected fields include
`TubeSurface.Path`, `OffsetSurface.Base`, `TrimmedSurface.Base` and the four
`CoonsPatch` boundary curves — the whole generated-surface family.

Found while rebuilding geometry-samples on the stdlib (plato-439): constructing a
`TubeSurface` around a `CatmullRomCurve3D` — the plainest use the type has — fails to
typecheck, and the sample carries a cast with a pointer to this issue.

## Approach

A field of interface type is existential: "some type implementing this interface", not
"this interface at the owner's Self". The parameter should be filled with a wildcard
the writer can express — `ICurve3D<any>` is the mechanical fix, or better, generate a
Self-free view interface for use in field positions and keep the Self-parameterized
one for constraint positions.

Worth checking whether the C++/Rust/GLSL writers make the same substitution; the bug
is in how the field's type is rendered, not in anything TypeScript-specific.

## Done means

- [ ] A stdlib type with an interface-typed field can be constructed from any
      implementor of that interface in generated TypeScript.
- [ ] The geometry-samples spline-tube sample drops its cast.
- [ ] Other writers checked for the same substitution.
