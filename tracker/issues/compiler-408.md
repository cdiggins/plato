---
id: compiler-408
title: Transform operator aliases cannot be derived: an interface-typed parameter does not re-resolve per implementor
type: problem
status: ready
priority: p2
effort: M
risk: med
area: compiler
sprint: 
created: 2026-08-02
closed:
links: []
---

## Problem

`transforms.library.plato` carries 49 operator aliases of the form

    Multiply(t: Translation2D, p: Point2D): Point2D
        => Transform(p, t);

one per (transform representation, operand) pair, all with the same body. They
exist so each representation gets the `t * p` spelling; the work is done by that
representation's own `Transform`.

They cannot be replaced by one alias per dimension. Writing

    Multiply(t: IAffine2D, p: Point2D): Point2D
        => Transform(p, t);

fails the type check with

    CHK201 No overload of 'Transform' matches argument types (Point2D, IAffine2D)

because the body resolves against the interface, not against each implementor.
The only body that would compile is `p.Transform(t.AffineTransform2D)`, and that
is a different computation: every product would go through the general affine
matrix instead of the representation's direct body — `t * p` would stop agreeing
with `p.Transform(t)` in both cost and rounding. So the 49 stay.

## What would remove them

Either of:

- **Monomorphic re-resolution.** When a library body with an interface-typed
  parameter is monomorphized for an implementor, re-resolve the calls in that
  body at the concrete type, so `Transform(p, t)` picks the implementor's
  overload. This is the general fix and affects every derived body, so it needs
  a decision recorded before it is attempted.
- **Writer-level operator synthesis.** Have the C# writer emit `operator *` from
  the existing `Transform` overloads directly, and delete the aliases from the
  library. Narrower, but it moves a language-visible spelling into one backend.

## Done means

- [ ] A decision recorded for which route (or that the aliases stay)
- [ ] If taken: the aliases removed and `t * p` still lowers to each representation's own Transform
