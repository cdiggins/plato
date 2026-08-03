---
id: plato-295
title: Prefer array literals over MapRange for fixed corners/points
type: debt
status: done
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [submodules/Plato/stdlib/planar-triangles.library.plato, submodules/Plato/stdlib/lines.library.plato, submodules/Plato/stdlib/spatial-patches.library.plato, submodules/Plato/stdlib/spatial-simplices.library.plato, submodules/Plato/stdlib/lines-planes.library.plato, submodules/Plato/stdlib-legacy/meshes.library.plato, submodules/Plato/docs/SEMANTICS.md, tracker/issues/plato-231.md]
---

## Issue

Forward `stdlib/` builds small fixed collections with `N.MapRange(i => i == 0 ? … : …)`
and nested ternaries instead of the language's array literal `[a, b, …]`. Example:
`Parallelogram2D.Corners` indexes a constant 4 with MapRange rather than listing the
four corner expressions. Behavior is correct; the form is harder to read and hides that
the length is a compile-time constant. Shipping `stdlib-legacy` already uses literals
for the same kind of API (`Points`/`Corners` on lines, triangles, quads, bounds).

## Impact

Authors and reviewers of geometry stdlib code hit nested ternaries on every corners/
points helper. New forward-stdlib bodies copy the MapRange pattern (see lesson note that
deferred Bounds corner enumeration for lack of literals — literals exist; habit didn't
catch up). Low runtime impact today; fixed-size literal form is also the better input for
future array/const-generic optimizations ([plato-231](plato-231.md)).

## Affected code

Canonical anti-pattern (constant count + index switch):

- `submodules/Plato/stdlib/planar-triangles.library.plato:203` — `Corners(Parallelogram2D)` via `4.MapRange` + ternaries
- `submodules/Plato/stdlib/planar-triangles.library.plato:37` — `Points(Triangle2D)` via `3.MapRange`
- `submodules/Plato/stdlib/planar-triangles.library.plato:174` — Quad points via `4.MapRange`
- `submodules/Plato/stdlib/lines.library.plato:23,27` — segment endpoints via `2.MapRange`
- `submodules/Plato/stdlib/spatial-patches.library.plato:50,127` — triangle/quad points
- `submodules/Plato/stdlib/spatial-simplices.library.plato:19` — tet vertices via `4.MapRange`
- `submodules/Plato/stdlib/lines-planes.library.plato:143` — `Slabs(Bounds3D)` via `3.MapRange`

Idiom already in shipping library:

- `submodules/Plato/stdlib-legacy/meshes.library.plato:16–21` — `Points` as `[x.A, x.B, …]`
- `submodules/Plato/stdlib-legacy/geometry.library.plato:155–175` — `Corners(Bounds2D/3D)` as literals

Semantics: `submodules/Plato/docs/SEMANTICS.md:184` — `[a, b, c]` → `Array<T>`.

Out of scope: `MapRange` over a **runtime** count (`points.Count.MapRange`, polygon
kernels, etc.) — those need the HOF.

## Cause / analysis

Forward stdlib bodies were written (or ported) in a MapRange-first style; array literals
were underused even though the grammar and legacy library already support them. Possible
early uncertainty about literal availability (lesson note "Corner enumeration deferred
(no array literal)") left the ternary form in place after literals were fine.

## Priority

**p3** — readability/idiom debt, not a defect; does not block features. Frequency is
steady but localized to a handful of fixed-arity helpers. Safe to defer; cost of deferral
is mainly copy-paste of the anti-pattern into new geometry APIs.

## Dependencies

- Blocked by: none (literals already work).
- Blocks: none hard; soft ally of [plato-231](plato-231.md) (fixed-size array lowering).
- Touches: forward `stdlib/` geometry libraries only for the sweep; leave runtime-count MapRange alone.

## Fix approaches

1. **Mechanical rewrite** of the listed sites to `[e0, e1, …]` matching legacy `Points`/`Corners` style. Smallest, clearest.
2. **Lint/convention** — document in `stdlib/CONVENTIONS.md` "fixed arity → literal; variable arity → MapRange" and optionally a later LINT rule. Prevents recurrence; more work.
3. **Do nothing until const-generics / optimize-arrays** ([plato-231](plato-231.md)) — misses the readability win now for no gain.

## Bedrock

Strengthen the stdlib idiom seam already proven in `stdlib-legacy/meshes.library.plato`:
fixed-arity geometry accessors are array literals. Keep MapRange for data-dependent
lengths. Verdict: **simplest-along-the-grain** — rewrite the constant-`N.MapRange`+ternary
sites only; do not invent a new corners API or change return types.

## Done means

- [x] Every listed `N.MapRange(i => i == 0 ? …)` fixed-arity helper in Affected code uses `[…]` (or an explicit decision that a site should stay MapRange, noted in the issue)
- [x] `dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint submodules\Plato\stdlib` still clean for touched files (0 parse errors; remaining findings are pre-existing LINT003/etc.)
- [x] Array-literal preference recorded in [`stdlib/STYLE_GUIDE.md`](../../submodules/Plato/stdlib/STYLE_GUIDE.md) (plato-299)

Also replaced the two non-stdlib hits from the same search: `csg/csg.plato` `Single` → `[f]`, `earcut/earcut.plato` `NoTriangles` → `[]`. Left `KeepIf`'s `(cond ? 1 : 0).MapRange` (not a constant).

## Simplest fix

Replace each constant-count MapRange+ternary with an array literal, e.g.

`Corners(g: Parallelogram2D) => [g.Origin, g.Origin.Add(g.SideA), g.Origin.Add(g.SideA).Add(g.SideB), g.Origin.Add(g.SideB)]`

Pros: readable, matches legacy, tiny diff. Cons: none material; slightly longer lines on
Bounds3D-style eight-corner lists (already fine in legacy).

## Prevention

- Document the convention once (approach 2).
- No new test class required — lint + existing shape of APIs is enough; a witness that
  `Corners` length equals field arity already exists for Bounds in legacy-tests if needed later.
- Related idea already filed: [plato-231](plato-231.md) (type-level naturals / fixed arrays) —
  this debt is independent surface cleanup, not blocked by it.
