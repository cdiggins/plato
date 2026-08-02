---
id: plato-326
title: Forward stdlib Grid2D/3D declares RowCount/ColumnCount; runtime provides NumRows/NumColumns
type: bug
status: ready
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-29
closed:
links: [tracker/issues/plato-323.md, submodules/Plato/stdlib/collections-indexable.concepts.plato]
---

## Issue

Found while burning down [plato-323](plato-323.md) (forward body-level C# errors). The forward
stdlib's grid interfaces declare the 2D/3D extent members as `ColumnCount` / `RowCount`:

- `submodules/Plato/stdlib/collections-indexable.concepts.plato:35-45`
- used throughout `stdlib/collections-grids.library.plato`

The handwritten runtime those interfaces are meant to bind to spells them `NumColumns` / `NumRows`
(`ara3d-sdk/src/Ara3D.Collections/FunctionalReadOnlyList2D.cs:9-10`, and
`CSharpWriter.StructSurfacePropertyNames` seeds exactly `Count` / `NumColumns` / `NumRows` as the
BCL-parity property names for that reason). The generated C# therefore emits `RowCount` reads on
`IReadOnlyList2D<float>` / `Matrix3x2` / `Matrix4x4` receivers that have no such member:

```
_Matrix3x2.g.cs(36): CS0103 The name 'RowCount' does not exist in the current context
NumericStructuresMatrix.g.cs(64): CS1061 'Matrix4x4' does not contain a definition for 'RowCount'
_SampledSdf2D.g.cs(50):  CS1061 'IReadOnlyList2D<float>' does not contain a definition for 'RowCount'
FieldsImplicitsSampled.g.cs(14-17): same, 2D and 3D
```

~24 unique errors across `_Matrix3x2`, `_Matrix4x4`, `_SampledSdf2D`, `_SampledSdf3D`,
`NumericStructuresMatrix`, `FieldsImplicitsSampled`, `_PolygonMesh3D`.

## Impact

**Escalated 2026-07-30 (was p3): this now GATES honest measurement of the forward build.** Since
plato-323 item 2 landed (Plato `ac8d3e5`), the rendering rule is receiver-aware, so the writer no
longer emits the broken explicit interface forwarder `int MatrixLike.RowCount() => RowCount;` for
`Matrix3x2` / `Matrix4x4`. The public method satisfies the obligation directly — except there is no
member to satisfy it with, so the miss moved from the BODY stage (2 x CS0103 + 1 x CS1061 per type)
to the DECLARATION stage: **4 x CS0535 `'Matrix3x2' does not implement interface member
'MatrixLike.RowCount()'`**. csc suppresses method-body binding when any declaration error is
present (plato-323's headline lesson), so those 4 errors now hide the entire remaining ~201-error
body inventory. Until this is fixed, forward measurement needs a throwaway probe .cs in the
`Generated` folder:

```csharp
namespace Ara3D.Geometry
{
    public partial struct Matrix3x2 { public int RowCount() => 3; }
    public partial struct Matrix4x4 { public int RowCount() => 4; }
}
```

Also note the remaining `IReadOnlyList2D<float>` / `IReadOnlyList3D<float>` sites now report as
CS1929 (`x.RowCount()`) rather than CS1061 (`x.RowCount`) — same defect, new spelling.

Part of the remaining plato-323 tail. Small in count but it blocks the same forward law runner,
and it is a genuine **vocabulary** decision, not a writer defect — which is why it was split out:
plato-323's scope is the emitter, and `stdlib/` is owned by concurrent sessions.

## Cause / analysis

A naming choice made in the forward vocabulary that does not match the handwritten collection
surface it must satisfy. Nothing in the emitter can bridge it: call-site syntax is decided by
name, and the writer already treats `NumRows`/`NumColumns` as the parity names.

## Fix approaches

1. **Rename in the forward stdlib** to `NumRows` / `NumColumns` (interfaces + every use in
   `collections-grids.library.plato` and friends). Matches the runtime, matches the writer's
   existing parity set, no compiler change. Note `Jagged.RowCount` in
   `stdlib/collections-jagged.library.plato:24` is a *different* member (rows of a jagged
   structure) — decide whether it renames too or stays.
2. Add `RowCount`/`ColumnCount` aliases to the handwritten `Ara3D.Collections` interfaces. Cheap
   but adds a second spelling to a shared SDK surface for one library's benefit.

Option 1 unless the forward vocabulary deliberately prefers the `*Count` spelling, in which case
the parity set in `CSharpWriter.BuildExtensionPlans` needs the aliases too.

## Simplest fix

Option 1, one rename sweep over `stdlib/`, then `.\tools\check-stdlib-fast.ps1`.

## Done means

- [ ] Forward-generated C# has no `RowCount` / `ColumnCount` binding errors.
- [ ] `.\tools\check-stdlib-fast.ps1` green (ratchet not raised).
