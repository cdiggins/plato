---
id: plato-288
title: Add Axis3D / SignedAxis3D enum-like sum types to forward stdlib
type: idea
status: done
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed: 2026-07-28
links: [plato-272, submodules/Plato/docs/plato-sum-types-design-2026-07-27.md, submodules/Plato/stdlib/CONVENTIONS.md, submodules/Plato/stdlib/cameras.plato, submodules/Plato/stdlib/vectors.plato, submodules/Plato/stdlib/transforms.plato]
---

## Idea
Add enum-like (nullary-case) sum types for cardinal axes to the forward stdlib (`submodules/Plato/stdlib`): `Axis3D` (unsigned: X/Y/Z direction lines) and `SignedAxis3D` (the six oriented cube-face normals), plus an `Axes` library with total conversions out (`Vector3D`, `Direction3D`, `Number3`, `Point3D`, `Ordinal: Integer`) and axis algebra (`Component(v, axis)`, `Tangent`/`Bitangent` cycling, `Opposite`, `Sign`). Retires the CONVENTIONS.md clause "axis selectors are plain `Integer`" — kd-tree split axes, `UpAxis`/`ForwardAxis` fields, and longest-extent queries become typed.

## Assumptions
- Enum-shaped sums already work end-to-end: `cameras.plato` ships `CameraProjection = Perspective | Orthographic | ...`; match lowers to conditionals (plato-232).
- Case names are scoped to the sum type (design doc §3.2 — `CameraProjection.Orthographic` and `FisheyeMapping.Orthographic` coexist today), so case naming is a style choice, not a collision constraint.
- Conversions are one-way: `Vector3D(axis)` is total; the reverse is partial and must be a snap (`NearestAxis`), not a cast (CONVENTIONS.md A1 — no Optional).

## Design decisions
- **Case naming — DECIDED 2026-07-28 (user):** short case names with the qualified constructor form — `Axis3D.X`. So `type Axis3D = X | Y | Z;`, `type Axis2D = X | Y;`, `type SignedAxis3D = PosX | NegX | PosY | NegY | PosZ | NegZ;`. Cases are scoped to their sum type (design doc §3.2), so `Axis3D.X` and `Axis2D.X` coexist. Verified in the landing: the parser/checker accept the short cases; match arms (`X => ...`) resolve against the subject's sum type with no bare-case ambiguity, and lint stayed 0 parse / 0 symbol-resolution errors. Chosen over `XAxis | YAxis | ZAxis` and `XAxis3D | ...` (the §4.4 bare-constructor future is not used — construction is qualified `Axis3D.X`).
- **Case naming (superseded options)** — `XAxis3D | YAxis3D | ZAxis3D` (globally unique but `Axis3D.XAxis3D` stutters); middle form `XAxis | YAxis | ZAxis` (bare-safe, mild stutter). Session recommendation had been the middle form; user chose the short form above.
- **One type or two** — unsigned + signed as separate types (recommended: most consumers — kd-split, swizzle, extent — want 3 cases, not 6) vs one 6-case type only.
- **Not a Vector** — `Axis3D` must NOT implement `Vector` even though the casts are cheap: `X + Y` must not typecheck. Plain `Value`.
- **2D sibling** — add `Axis2D = XAxis | YAxis` in the same file or defer.
- **Relation to plato-272 axis constants** — Constants' unit-axis values (`XAxis3D(): Vector3D` / `Direction3D` statics) become `Axis3D.X.Vector3D` derivations or stay independent; avoid two spellings of "unit X".

## Related
- [plato-272](plato-272.md) — Constants library plans unit-axis constants; overlaps on "what is the canonical unit X axis" — coordinate the two surfaces.
- [submodules/Plato/docs/plato-sum-types-design-2026-07-27.md](../../submodules/Plato/docs/plato-sum-types-design-2026-07-27.md) — §3.2 case-name scoping, §4.4 open bare-vs-qualified constructors (drives the naming decision).
- [submodules/Plato/stdlib/CONVENTIONS.md](../../submodules/Plato/stdlib/CONVENTIONS.md) — "axis selectors are plain Integer" clause to retire; A1 no-Optional rule shapes the reverse-conversion API.
- [submodules/Plato/stdlib/cameras.plato](../../submodules/Plato/stdlib/cameras.plato) — precedent enum-like sums in stdlib.
- [submodules/Plato/stdlib/vectors.plato](../../submodules/Plato/stdlib/vectors.plato) — `Direction3D` (wraps `Vector3D`), `Number3` targets.
- [submodules/Plato/stdlib/transforms.plato](../../submodules/Plato/stdlib/transforms.plato) — `Quaternion(aa: AxisAngle)` precedent for target-type-named conversion functions; `RotationAboutX/Y/Z` could take an axis parameter.
- plato-src-v2 `09-coordinate-systems.plato` had `Axis3D = { Origin; Direction }` (a geometric line) — name collision to resolve; if the geometric one returns, call it `AxisLine3D`.

## Approaches
Short term: new `axes.plato` (~40 lines) with the two sum types + `Axes`/`SignedAxes` libraries; update CONVENTIONS.md typed-index section; lint green.
Long term: sweep `Integer` axis selectors (kd-tree nodes, `UpAxis`/`ForwardAxis` conventions types) to `Axis3D`/`SignedAxis3D`; `NearestAxis(v): SignedAxis3D` snap; axis-parameterized rotation constructors.
Adjacent ideas worth their own issue: `Axis2D`; swizzle/component-permutation vocabulary built on `Ordinal`.

## Bedrock
Strengthens the **typed-selector invariant** in CONVENTIONS.md: today axis selectors are the documented exception ("plain Integer") to the everything-is-typed rule; this removes the exception, and every future spatial structure (octrees, BVH splits, grid strides) gets a non-mixable axis type for free. **Verdict: simplest-along-the-grain** — must NOT make Axis3D implement Vector, must NOT add a partial `Axis3D(v)` cast, must NOT fold signed and unsigned into one type.

## Done means
- [x] `axes.plato` in `stdlib` with `Axis3D`, `Axis2D`, `SignedAxis3D`, conversion + algebra libraries (`Axes` / `Axes2D` / `SignedAxes`); case-naming decision recorded in the file header
- [x] `lint submodules\Plato\stdlib` still 0 parse / 0 resolution errors (exit 0; the new file added zero lint findings — total held at 2759)
- [x] CONVENTIONS.md typed-index section updated (axis-selector exception rescoped: axis selectors are now `Axis3D`/`Axis2D`/`SignedAxis3D`; plain `Integer` remains only for CSR/offset/count/bitmask)
- [x] At least one existing `Integer` axis selector site migrated or explicitly deferred with a note — **explicitly deferred**: this landing is pure addition; the call-site sweep (kd-tree split axes, `UpAxis`/`ForwardAxis` fields, longest-extent queries) is left for a follow-up. New code prefers `Axis3D`/`Axis2D`/`SignedAxis3D`; existing `Integer` selectors migrate opportunistically.

## Simplest possible implementation
One file, two sum types, two libraries, no call-site sweep; CONVENTIONS.md note saying new code prefers `Axis3D` while existing `Integer` selectors migrate opportunistically.
- Pros: pure addition, zero risk to existing vocabulary; unblocks kd-tree/grid typing immediately.
- Cons: two conventions coexist until the sweep; naming decision locked early while §4.4 (bare constructors) is still open.
