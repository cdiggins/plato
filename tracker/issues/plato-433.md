---
id: plato-433
title: TpmsSheet3D thickness is not portable between families and silently yields an empty solid
type: bug
status: ready
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

`TpmsSheet3D(family, period, level, thickness)` is the "gyroid infill" type — a
wall of finite thickness around a triply periodic minimal surface. A plausible
`thickness` silently produces an **empty solid**: `MarchingCubes` returns zero
triangles, with no error, no warning and no NaN.

Found by the lattices demo (plato-421), which had to work around it to show the
type at all.

## Why it happens

`Eval` is `|nodal - Level| / TpmsGradientBound - Thickness/2`. The nodal implicit
is bounded, so the widest wall the field can describe is
`max|nodal| / TpmsGradientBound`. Past that, every sample is negative and there is
no zero crossing to march.

That number is small and, crucially, **not portable between families**, because
the gradient bound scales with a per-family partial bound that ranges from 1
(Schwarz P) to 7 (Neovius) — a sevenfold spread. For a gyroid at period 0.9 the
widest describable wall is about **0.066**, so a thickness of 0.18 — an entirely
reasonable-looking number — yields nothing.

Measured at 36 nodes per axis over the unit cube: at `thickness = 0.2 x period`,
Gyroid, Schwarz D and I-WP all go empty while Schwarz P and Neovius do not.
Schwarz D goes empty at `0.1 x period`.

**This is not a defect in the construction.** Dividing by a Lipschitz bound is
what makes these types honest lower-bound distance fields rather than nodal
implicits pretending to be distances — plato-421 chose that deliberately and the
choice is right. The problem is that the consequence is invisible to a caller.

## Fix approaches

1. **Doc comment, minimum.** Say that `Thickness` is bounded above by
   `max|nodal| / TpmsGradientBound`, that the bound is family-dependent, and that
   exceeding it yields an empty solid rather than an error.
2. **Give the caller the number.** A `MaximumSheetThickness(family, period)`
   member, so a UI can scale a slider instead of guessing. The demo computes this
   by sampling `TpmsField3D.Eval` and memoizing, which is exactly the workaround a
   library member would remove.
3. **Express thickness as a fraction of the maximum** in a second constructor, so
   the portable spelling is available and 1.0 means "the widest wall this family
   can describe at this period".

Approach 2 is the one that removes the trap without changing what the type means.

## Done means

- [ ] The `TpmsSheet3D` doc comment states the upper bound on `Thickness` and what
      exceeding it does
- [ ] A caller can obtain the maximum describable thickness for a family and
      period without sampling the field themselves
