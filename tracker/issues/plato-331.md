---
id: plato-331
title: Plato.Intrinsics.V2 is a method-form runtime with ten property leftovers; the writer hard-codes four of them
type: debt
status: done
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-30
closed: 2026-07-30
links: [tracker/issues/plato-323.md, tracker/issues/plato-330.md, submodules/Plato/Plato.Intrinsics.V2, submodules/Plato/Plato.CSharpWriter/CSharpWriter.cs]
---

## Issue

`Plato.Intrinsics.V2` is the all-extension-methods runtime the `--no-properties` recipes target: the
point of the flag is that the generated surface, and the handwritten surface it forwards into, are
uniformly METHODS. The port is not quite complete. V2's public properties are:

| type | members | file |
|---|---|---|
| `Plane` | `Normal`, `D` | `Plane.cs:23,29` |
| `Matrix3x2` | `Row1`..`Row3` | `Matrix3x2.cs:43-45` |
| `Matrix4x4` | `Row1`..`Row4`, `Translation`, `Rotation` | `Matrix4x4.cs:59-62,140,161` |
| `Number` | `Angle` | `Number.cs:92` |
| `Vector2/3/4/8`, `Quaternion` | `X`/`Y`/`Z`/`W`/`X0..X7` | component pseudo-fields |

The component pseudo-fields are **not** the debt: `CSharpConcreteTypeWriter.PrimitiveFieldNames`
records them and the writer emits `Components() => Intrinsics.MakeArray(X, Y, Z)` reading them
unqualified, so they must stay properties. `Angle.Radians()` is correctly a method. The other ten
are the debt.

## Why it matters

The writer generates NO struct for a `CSharpWriter.PrimitiveTypes` entry (see
`CSharpConcreteTypeWriter`: the `// Fields` block is `!IsPrimitive`), so it cannot see the runtime's
shape and reads the Plato DECLARATION as a proxy: a declared field is a property/field, everything
else is a method. That proxy is right for the whole V2 surface except four members, which
plato-323 item 2 had to hard-code as `CSharpWriter.PrimitiveSurfaceOverrides`:

- `Angle.Radians` -- declared as a FIELD by the forward stdlib
  (`type Angle implements Quantity { Radians: Number; }`) but a METHOD in V2, so the declaration
  pinned property syntax onto a method at ~80 forward call sites (110 x CS0030).
- `Matrix4x4.Translation`, `Matrix4x4.Rotation`, `Number.Angle` -- not declared as fields at all
  (they are library functions in Plato) yet PROPERTIES in V2. Before the receiver-aware rendering
  rule these rode on the global name union by accident, each name happening to be a field on some
  unrelated struct.

`Plane.Normal`/`D` and `Matrix*.Row*` need no override only because `stdlib-legacy` happens to
declare them as fields, so the proxy lands on "property" by luck. That is a coincidence, not a
design -- and it is the coincidence that makes a blanket "a primitive's surface is only
`PrimitiveFieldNames`" rule wrong (measured: it produces 39 x CS1955 "non-invocable member
'Plane.Normal' cannot be used like a method" plus 21 x CS1501).

## Impact

Low and static today: the override table is four entries, documented, and pinned by
`PlatoTests/ReceiverAwareRenderingTests.cs` plus the byte-identity goldens. The cost is conceptual --
a hard-coded record of another project's member shapes inside the emitter, the kind of pin list the
C4 consolidation deliberately removed. Any future V2 port step, or any new forward-stdlib field
declaration on a primitive-backed type, can silently disagree with the table again, and the symptom
appears a thousand generated files downstream as CS0030 or CS1955.

## Fix approaches

1. **Finish the port**: convert the ten non-component properties to method form in V2 and update
   their internal callers (`Plane.WithNormal`/`WithD` read `Normal`/`D`; `Matrix4x4.Rotation` calls
   `Decompose()`), then delete `PrimitiveSurfaceOverrides`. The coupling described here has since
   **loosened**: `Plato.Generated.Unoptimized` / `.Optimized` were retired 2026-08-01
   (`../decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md`) and the goldens went
   in the 2026-07-30 retirement, so the runtime's remaining consumers are
   `Plato.Generated.Foundation.Unoptimized` and `Ara3D.SDK.ConformanceTests`. `stdlib-legacy` still
   declares `Matrix*.Row*` and `Plane.Normal/D` as fields, so if the legacy tier is ever revived
   the declaration proxy would be wrong in the other direction and this table would return with
   inverted values, unless the primitive rule changes to "surface = `PrimitiveFieldNames` only" in
   the same commit. Re-scope this approach against the current consumer set before starting.
2. **Leave the runtime alone and make the table self-policing**: keep
   `PrimitiveSurfaceOverrides` as the writer's deliberate record of the handwritten surface, and add
   a reflection test that fails when a V2 public property appears that is neither a
   `PrimitiveFieldNames` component nor an override entry. Turns a silent trap into a one-test build
   error.

## Simplest fix

Option 2: one reflection test over the V2 assembly. It removes the silent-drift risk without
touching a shared runtime or regenerating a golden. Option 1 is the real cleanup, but it should be
scheduled deliberately alongside a golden refresh, not taken as a drive-by.

## Dependencies

- Sibling of [plato-330](plato-330.md) (lint intrinsic obligations against the API snapshot): that
  one catches a runtime member that is MISSING, this one catches a runtime member whose SHAPE
  disagrees. The same snapshot file could serve both.

## Done means

- [x] Either V2 has no non-component public properties and `PrimitiveSurfaceOverrides` is deleted,
      or a test pins that every V2 public property is accounted for by the writer.
- [x] `.\tools\regen-generated.ps1` clean (or deliberately refreshed with
      `Ara3D.SDK.ConformanceTests` still 0 fail).

## Resolution (2026-07-30)

Option 2 shipped: `PlatoTests/IntrinsicsV2SurfaceTests.cs` compiles the V2 shared sources into the
test assembly (projitems import in `PlatoTests.csproj`, plus `IntrinsicsV2TestShims.cs` for the few
generated extensions V2 calls) and polices the writer's picture of the surface in both directions:

- every V2 public instance property must be a `PrimitiveFieldNames` component, a
  `PrimitiveSurfaceOverrides` entry (now public on `CSharpWriter`), or one of the nine pinned
  stdlib-legacy declaration-proxy coincidences (`Plane.Normal/D`, `Matrix3x2.Row1-3`,
  `Matrix4x4.Row1-4`);
- every override entry must match the runtime (true -> property exists; false -> no property AND
  the method exists), so a future V2 port step that strands the table also fails;
- the pinned coincidences must still exist as properties, so converting one to a method forces the
  paired override/pin update the issue describes.

Fault-injection verified (a planted V2 property fails the suite); full PlatoTests 180/180 green;
`regen-generated.ps1` clean with zero Generated diffs (the writer change is visibility-only).

Option 1 (finish the port, delete the table, flip the primitive rule) remains the real cleanup and
should ride a future scheduled golden refresh; the tests above are the tripwire until then and the
checklist for that commit.
