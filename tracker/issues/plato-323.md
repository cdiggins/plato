---
id: plato-323
title: Forward stdlib body-level C# errors (~7.5k) unmasked once declarations compile
type: bug
status: ready
priority: p2
effort: L
risk: med
area: plato
sprint: 
created: 2026-07-29
closed:
links: [tracker/issues/plato-308.md, tracker/issues/plato-310.md, tracker/issues/plato-311.md, submodules/Plato/Plato.CSharpWriter]
---

## Issue

With plato-310, plato-311, and plato-308 Root 2 landed (Plato submodule `e2d8b83`), the forward
conformance build has ZERO declaration-stage errors for the first time — and that unmasked
~7,510 body-level errors that were present all along. csc compiles in stages: any
declaration-stage error (the CS0535/CS0557/CS0315 the earlier issues tracked) suppresses the
method-body binding stage entirely, so these never appeared in any earlier count. plato-308's
"324 errors" was only ever the declaration layer.

Measured 2026-07-29 in an isolated Plato worktree at HEAD (+ the fixes since landed), recipe
`--csharp-style=extensions --scalar=float --optimize --optimize-arrays --inline --methods --loops
--no-properties --static-abstract`, 1244 `.g.cs`:

- 2274 × CS1061 — missing member/extension, e.g. `Point2D[]` has no `BoundsOfPoints`,
  `IReadOnlyList<Point2D>` has no `DeCasteljau`: library functions referenced by bodies did not
  monomorphize into the extension classes the call sites expect.
- 1952 × CS0119 — method group used as a value, e.g. `ArrayExtensions.Range(int)` "is a method,
  which is not valid in the given context": no-paren property-style reads of moved members.
- 1092 × CS0315 — `float` fails the `Index`-constrained generic, e.g.
  `Vector2D.At<_T0>(_T0)` with no boxing from `float` to `Index`: index-operator lowering
  (`[]` / `.At`) resolving the wrong overload/instantiation.
- 686 × CS0176 — static member accessed via instance; 642 × CS0117 — member not found;
  296 × CS0030; 134 × CS1503; 132 × CS0029; 96 × CS1929; 86 × CS1662; plus a tail
  (CS1501/CS0019/CS1955/CS0103/CS0428/CS0023).

Independence proven: fixing the last declaration error (CS0557) two opposite ways — suppressing
the generated Angle converters vs. deleting the handwritten ones — unmasks the identical error
set, so these are pre-existing body-emission gaps, not fallout from the declaration fixes.

## Progress (2026-07-29)

Measured in an isolated Plato worktree, full forward recipe, 1245 `.g.cs`. Note the counts
below are UNIQUE (file, line, code, message) errors — the 7,510 in the section above was the raw
MSBuild count, which repeats every error once per target framework pass and per referencing
project. "Locations" = distinct (file, line), the honest defect-site count.

| after | total | locations | CS1061 | CS0119 | CS1929 | CS0176 | CS0117 | CS0030 | CS1503 | CS1501 | tail |
|---|---|---|---|---|---|---|---|---|---|---|---|
| baseline | 2253 | 1921 | 515 | 915 | 53 | 245 | 204 | 107 | 54 | 23 | 137 |
| fix 1 (`9dd7cea`) | 1890 | 1648 | 512 | **0** | 473 | 472 | 204 | 99 | 55 | 23 | 52 |
| fix 2 (`54e3d5a`) | 1204 | 1105 | 509 | 0 | 473 | **0** | **0** | 100 | 55 | 23 | 44 |
| fix 3 (`e57c034`) | 774 | 683 | 509 | 0 | **48** | 0 | 0 | 100 | 50 | 23 | 44 |
| fix 4 (`c303db2`) | 752 | 660 | 509 | 0 | 48 | 0 | 0 | 100 | 50 | **1** | 44 |
| fix 5 (array recv, `e69a69d`) | 342 | 293 | **85** | 0 | 49 | 0 | 0 | 110 | 50 | 1 | 47 |
| fix 6 (recv-aware, `ac8d3e5`) | 201 | 152 | 61 | 0 | 53 | 0 | 0 | **0** | 50 | 1 | 36 |

**Net: 2253 → 201 errors, 1921 → 152 locations (−92 %) across six commits, every gate green
at each one.**

Fix 6's row is measured WITH a probe that neutralizes [plato-326](plato-326.md) — see cluster 6
below. Without it csc reports only 4 declaration-stage CS0535 and the body stage is suppressed
entirely, which is not a comparable number.

The CS1929/CS0176 *rises* at fix 1 are cascade unmasking, not regressions: a diff of error
LOCATIONS against the baseline shows zero new ones at every step (csc reports one error per
expression, so removing the first error on a line exposes the next).

**Cluster 1 — CS0119 (915 → 0), `9dd7cea`.** The uniform rendering rule (a no-arg member renders
with property syntax iff its name is on the struct surface) was decided globally BY NAME. Under
`--scalar=float` the five scalar wrappers have no generated struct at all, so every no-arg member
of `Number`/`Integer`/... is an extension method on the primitive and always needs `()`. A
genuine field on an unrelated struct — `Histogram.Range` — therefore stole the parentheses from
`ArrayExtensions.Range(this int)` at every scalar call site. Fixed by
`CSharpWriter.IsStructSurfaceProperty(ownerTypeName, name)`, which applies the rule with the
receiver/owner type in hand; its three consumers now route through it.

**Cluster 2 — CS0176 + CS0117 + CS1955 (684 → 0), `54e3d5a`.** Plato's `_` receiver is the
type-level idiom (`FromOffset(_: Point2D, v: Vector2D)`, `Pi(_: Number)`) and
`CSharpFunctionInfo` already emits those members as C# statics — but the body writer emitted a
value receiver as an instance call. `TirCSharpBodyWriter.TryWriteTypeLevelCall` now rewrites such
a call to `{ns}.{ReceiverType}.{Name}(rest)`, dropping the ignored receiver and mapping an erased
primitive back to its wrapper. `CSharpWriter.IgnoredFunctions` is excluded — the `IArrayLike`
scaffolding declares a `_` receiver but the emitter generates no static for it and the handwritten
runtime provides `v.NumComponents()`; rewriting it moved the diff-gated goldens. Separately,
`Plato.Intrinsics.V2` `Number` gained `Pi`/`Tau`/`E`/`Epsilon`, which the forward stdlib declares
as `_`-receiver intrinsics and the runtime never had (12 API-snapshot additions, no losses).

**Cluster 3 — CS1929 (473 → 48), `e57c034`.** Not a monomorphization failure, which is what the
error text suggested (csc blamed `CollectionsIndexable.Map(TriangleFace, Func<VertexIndex, _T0>)`,
an unrelated candidate). **C# does not apply a user-defined implicit conversion to an extension
method's RECEIVER.** The recipe erases the scalar wrappers, so every generated element-wise
extension is declared on the primitive (`IntegerExtensions.ToNumber(this int)`) — but the
handwritten combinators handed callers the WRAPPER: `ArrayExtensions.Range(this int)` returned
`ReadOnlyList<Integer>`, `MapRange`/`MapIndices`/`Map` took `Func<Integer, …>`. So
`count.Range().Map(i => i.ToNumber())` was unbindable. `Integer` is now erased from every
index/element position that flows OUT to a caller's lambda; the `Integer`-*receiver* overloads
stay (a wrapper-typed receiver has no other way in). Diagnosed with a throwaway probe .cs in the
Generated folder that forced csc to name the inferred types — much faster than reading the writer.

**Cluster 4 — CS1501 (23 → 1), `c303db2`.** `System.HashCode.Combine` stops at 8 arguments and
the generated `GetHashCode` passes one per field; the forward stdlib has structs with 9, 10 and 28.
Added the accumulator form of `Intrinsics.CombineHashCodes`.

**Cluster 5 — CS1061 (509 → 85), array-receiver library functions.** DONE. Details below; the
remaining-by-root list that follows is measured AFTER it.

The forward stdlib spells a list receiver with the CONCRETE `Array<T>` type
(`BoundsOfPoints(points: Array<Point2D>): Bounds2D`); stdlib-legacy spells the same shape with the
`IArray<T>` CONCEPT, and `WriteInterfaceLibraryMethods` already emitted the concept spelling as a
classic extension method in `Extensions.g.cs`. Only the concept arm existed, so the concrete arm
was emitted NOWHERE — `Array` is in `CSharpWriter.IgnoredTypes` (no `ExtensionStylePlan`) and moved
members are only discovered while writing a concrete type's own file. The fix is that one predicate
(`CSharpWriter.IsListExtensionReceiver`): `ToCSharpTypeName` already maps `Array<T>` →
`IReadOnlyList<T>`, so both spellings emit identically, and the three receiver spellings at call
sites (`IReadOnlyList<Point2D>`, `Point2D[]`, `List<Point3D>`) are all implicitly convertible to
the one emission. Body-less array functions stay out — those are the array INTRINSICS the
handwritten `Ara3D.Collections` runtime owns.

Emitting the FIRST such function exposed a latent body-writer bug that produced ONE syntax error
and thereby masked the entire conformance build (parse stage precedes bind stage, same staging
lesson as this issue's headline): a lambda whose body the capture hoist left as a bare `TirReturn`
rendered `(i) return e;`. `TirCSharpBodyWriter` now writes `=> { return e; }` for any
statement-shaped lambda body, not only for a `TirBlock`.

Generic array receivers (`DeCasteljau(xs: Array<$T>, t: Number)`) emit as generic extension
methods and are worth it — 32 CS1061 fixed for 1 new error, the unconstrained-`_T0` body tracked as
[plato-328](plato-328.md). That is the ONLY new error location in the whole build (all other
per-code rises below are cascade unmasking on lines that already carried an error).

Regression fixture: `PlatoTests/ArrayReceiverLibraryEmissionTests.cs` (3 tests — concrete receiver,
generic receiver, body-less receiver NOT emitted).

**Cluster 6 — CS0030 (110 → 0) + CS0428 (7 → 0) + CS1061 (85 → 61), `ac8d3e5`. Golden rewrite
authorized by Christopher.** The general form of cluster 1: rendering decided by the RECEIVER's own
struct surface rather than the global name union. Three coupled parts, because the first alone moved
nothing:

1. **Resolve against the receiver's plan** (`GetExtensionPlanByTypeName(owner).KeptNoArgPropertyNames`).
   A receiver no plan describes — a concept interface, a generic type variable, an `IgnoredTypes`
   collection — has no struct surface at all, so it now falls through to the GLOBALS only
   (`Count`/`NumRows`/`NumColumns` and the sum-type flattened fields, which no plan owns). The old
   union fallback is what pinned property syntax onto `mesh.VertexCount()` /
   `edges.UndirectedEdgeCount()` — the whole CS0428 cluster.
2. **Demotion no longer records a property.** This was the reason part 1 measured 110 → 109 on its
   own. `ExtensionStylePlan.DemoteMovedNames` added every globally-conflicted name to the plan's
   `KeptNoArgPropertyNames`, so the global collision was re-injected into each plan and the
   per-receiver lookup found it again. Coming back into the struct is a PLACEMENT decision (a C#
   instance member silently hides a same-name extension method) and stays global; under
   `--no-properties` a kept generated member still emits as a METHOD. One field named `Amount` had
   been pinning property syntax onto `Amount(x: Angle)` and its 60 sibling quantity projections.
3. **A primitive type's surface is what the HANDWRITTEN struct exposes**, not what Plato declares —
   the writer emits no fields for such a type. `CSharpWriter.PrimitiveSurfaceOverrides` records the
   four members where `Plato.Intrinsics.V2` disagrees with the declaration: `Angle.Radians` is a
   method there (the `type Angle implements Quantity { Radians: Number; }` field was the other ~80
   CS0030), while `Matrix4x4.Translation`/`.Rotation` and `Number.Angle` are properties without
   being declared fields. A blanket "primitive surface = `PrimitiveFieldNames` only" rule was
   measured and REJECTED: `stdlib-legacy` declares `Matrix*.Row*` and `Plane.Normal`/`D` as fields
   and V2 keeps those as properties, so it produced 39 × CS1955 + 21 × CS1501. Filed as
   [plato-331](plato-331.md).

Zero new defect SITES. Every per-code rise (CS1929 49 → 53) is a re-spelling of a plato-326
`RowCount` site: `x.RowCount` (CS1061) became `x.RowCount()` (CS1929). On `Matrix3x2`/`Matrix4x4` the
writer no longer emits the broken forwarder `int MatrixLike.RowCount() => RowCount;`, so the miss
moved from the body stage to the DECLARATION stage as 4 × CS0535 — which suppresses body binding, so
**[plato-326](plato-326.md) now gates honest forward measurement** and was escalated to p2. Probe
recipe for measuring past it is in that issue.

Golden movement: **101 of 184** `Plato.Generated.Unoptimized` + 100 of 184 `.Optimized` refreshed
(the pre-measurement predicted ~88; the extra ~13 come from parts 1 and 2, which were not in the
originally-sketched 15-line version). Reviewed by INVARIANT rather than by sampling: normalizing away
exactly the empty parens, the property-getter wrapper, the `MethodImpl` attribute and the
now-redundant `T ISome.N() => N;` explicit forwarders makes all 201 files byte-identical to the
previous goldens — there is nothing else in the diff. Three shapes only:

```
public T N { get => e; }        ->  public T N() => e;
x.N                             ->  x.N()
T ISome.N() => N; + property     ->  public T N()   (satisfies the obligation directly)
```

`Ara3D.SDK.ConformanceTests` stayed 205/205 with all 15 law-bearing types still discovered — the
reflection law contract survives the reshaping, which was the real risk. Regression fixture:
`PlatoTests/ReceiverAwareRenderingTests.cs` (4 tests — one build in which `f.Amount` and
`x.Amount()` coexist, plus the field itself unaffected); the two `EmitSnapshotTests` stub pins now
pin the method form. LINT014's counts are unchanged (280 forward / 111 legacy) because its predicate
is about the COLLISION, which still costs member placement; its doc comment and the test summary were
updated to say that instead of "pre-measures a withheld fix" ([plato-327](plato-327.md) follow-up 1).

### Remaining 201, by root (measured, not guessed)

1. **~50 CS1503 + ~14 CS0029 + ~15 CS0019 — wrapper/underlying pairs not bridged**:
   `Vector3D` vs `Vector3`, `Angle` vs `Number`, `Number3` vs `Number`, `Vector2` vs `Number2`;
   plus `Number * Vector2D` and `Matrix4x4 * Vector3D` operators that do not exist. A conversion /
   broadcast-operator surface gap, mostly in the handwritten runtime.
2. **61 CS1061 left, no longer one root.** By member: `Vector` 21, `Distance` 21, `Base` 5,
   `LayerCount` 4, `X0`/`X1`/`X2`/`Pow2`/`Pow3`/`Broadcast` 10 between them. `AtWrapped` (24),
   `RowCount` (24, now CS1929/CS0535 — [plato-326](plato-326.md)) and every array-receiver name are
   GONE. **`Vector` + `Distance` (42, i.e. 70 % of what is left in this code) look like one shape and
   are the obvious next cluster.**
3. **53 CS1929**, led by `ColumnCount` 8 + `RowCount` 8 ([plato-326](plato-326.md)), then `Hash` 7,
   `Compare` 3, and a long tail of one- and two-hit concept members (`One`/`Zero`/`Concatenate`/
   `Lerp`/`ToInteger`/`ToNumber`/`Inverse`/`Max`...). Mostly the same wrapper-receiver /
   implicit-conversion gap as root 1: C# will not apply a user-defined conversion to an extension
   method's receiver (the cluster-3 lesson).
4. **4 CS0103 / 2 CS0023 / 1 CS1501 / 1 CS1662 tail.** Three of the CS0103 are "the name 'self' does
   not exist" in `_PolygonMesh3D` / `_QuadFace` / `_TriangleFace` — a moved-body re-qualification
   miss worth one look. CS1501 is 1 × [plato-328](plato-328.md).

### Reproduce

```
.\tools\regen-forward-conformance.ps1 -Codegen
dotnet build submodules\Plato\conformance\Plato.ForwardConformanceTests -c Release
```
Measure against an ISOLATED worktree, never the shared tree — concurrent sessions rewrite
`stdlib/` mid-flight. Recipe: worktree with `-c core.longpaths=true`, copy `parakeet` in and
delete the copied `parakeet\.git` FILE, and after copying a changed compiler source into the
worktree **bump its LastWriteTime**, because `Copy-Item` preserves the source mtime and MSBuild
will otherwise reuse a stale assembly.

> ### ⚠️ DO NOT junction `ara3d-sdk` into the worktree. This destroyed the repo once.
>
> `parakeet\Parakeet\Ara3D.Parakeet.csproj` references `..\..\..\..\ara3d-sdk\src\Ara3D.Utils`,
> so earlier revisions of this recipe told agents to create directory junctions pointing at
> `C:\Users\cdigg\git\studio\ara3d-sdk` (at `<parent>\`, `<worktree>\`, and four levels above
> `<worktree>\parakeet\Parakeet\`). **On 2026-07-30 that deleted 2,092 tracked files from the
> real `ara3d-sdk` submodule** — a recursive scratch cleanup (`Remove-Item -Recurse -Force` /
> worktree removal) followed a junction and deleted the TARGET's contents, including both frozen
> V1 roots and the submodule's `.git`. Recovered from `.git\modules\ara3d-sdk`
> (`git -C ara3d-sdk checkout -- .`), except one frozen file whose pre-deletion bytes existed
> only in the working tree.
>
> Safe alternatives, in order of preference:
> 1. **Don't build `parakeet` in the worktree.** Copy the prebuilt `parakeet\**\bin\Release`
>    outputs in, or point the worktree's Plato.AST at the main tree's built Parakeet assemblies.
> 2. Make the worktree path put the real `ara3d-sdk` at the right relative depth (place the
>    worktree inside a scratch dir that is a sibling of the repo), so no link is needed.
> 3. If a link is unavoidable, use a **file** symlink to the `.csproj` or a `Directory.Build.props`
>    override rather than a directory junction — and never let any recursive delete run above it.
>
> Whatever you do: recursive deletes in scratch space must be preceded by
> `Get-ChildItem -Recurse -Force | Where-Object LinkType` and refuse to proceed if any link exists.
Group by CS code AND diff error SHAPES against the previous run: a code's count rising while no new
shape appears is cascade unmasking, not a regression. Compare by **(file, code, message)**, NOT by
(file, line) — any change that adds or removes a declaration shifts every line number below it, and
a line-keyed diff then reports dozens of phantom "new locations" (fix 6 showed 46 phantom against 4
real).

**Since fix 6 a probe is required.** 4 declaration-stage CS0535 ([plato-326](plato-326.md)) suppress
the whole body stage, so drop this into the `Generated` folder before building, and never commit it:

```csharp
namespace Ara3D.Geometry
{
    public partial struct Matrix3x2 { public int RowCount() => 3; }
    public partial struct Matrix4x4 { public int RowCount() => 4; }
}
```

### Filed out of scope

- [plato-326](plato-326.md) — `RowCount`/`ColumnCount` vs the handwritten `IReadOnlyList2D`
  `NumRows`/`NumColumns`: a stdlib naming mismatch, not a writer defect. **Escalated to p2 by
  fix 6**: it is now a declaration-stage error and therefore gates all forward measurement.
- [plato-331](plato-331.md) — `Plato.Intrinsics.V2` still exposes ten members as properties in a
  method-form runtime; the writer hard-codes four of them in `PrimitiveSurfaceOverrides`.
- [plato-328](plato-328.md) — a generic library function's inferred concept constraint never
  reaches the emitted `where` clause (`DeCasteljau<_T0>` calls `.Lerp` on an unbounded `_T0`).

## Impact

This is now the sole blocker for [plato-308](plato-308.md)'s executable law runner (and
transitively plato-306's 11 affine laws). The forward stdlib remains verifiable only by lint +
checker ratchet.

## Affected code

- `submodules/Plato/Plato.CSharpWriter/TirCSharpBodyWriter.cs` + `ExtensionStyleWriter.cs` — body
  emission and moved-member/extension monomorphization.
- Repro: `.\tools\regen-forward-conformance.ps1 -Codegen` then
  `dotnet build submodules\Plato\conformance\Plato.ForwardConformanceTests -c Release`.

## Cause / analysis

Not one bug: at least three clusters (missing extension monomorphizations; property-vs-method
spelling of moved members; Index/`At` lowering). Sizes are inflated by fan-out — one missing
extension method can account for hundreds of call-site errors — so the real defect count is much
smaller than 7.5k. Start by fixing the top one or two clusters and re-measuring.

## Priority

p2, same chain as plato-308. Effort unknown until the clusters are sampled — file-level counts
put GeometryMeasures / NumericStructures* / CurvesSampling first.

## Dependencies

- Blocks: [plato-308](plato-308.md) (now rescoped to this + wiring).
- Touches: `Plato.CSharpWriter` body emission — shared with legacy generation; `regen-generated.ps1`
  byte-identity applies to every change.

## Fix approaches

1. Cluster-by-cluster burn-down: sample one error per CS code, root-cause in the writer, fix,
   regenerate, re-measure. Fan-out means counts should collapse quickly.
2. Add a writer-level regression fixture per fixed cluster (small .plato that exercises the shape).

## Bedrock

Two lessons worth keeping.

1. **Declaration errors hide body errors** — a "324-error" build can be a 7,500-error build in
   disguise, so error counts across csc stages are not comparable. plato-308's history records this,
   and fix 6 hit it again from the other side: fixing 141 body errors while introducing 4
   declaration errors made the build report "4 errors", which is not progress, it is a blindfold.
2. **A global name set is the wrong shape for a per-receiver question.** Every cluster in this issue
   except 4 traces to the emitter deciding something about a member (how it renders, where it lives,
   what its receiver's shape is) from a name-keyed set unioned across all types. The fix each time
   was to carry the receiver's identity to the decision point. `MovedNoArgNames` is the one such set
   that is legitimately global — member hiding really is a whole-struct property.

No architectural leverage beyond the burn-down itself.

## Done means

- [ ] `dotnet build Plato.ForwardConformanceTests -c Release` → 0 errors.
- [ ] `.\tools\regen-forward-conformance.ps1 -Test` runs the law runner (plato-308's box).
- [ ] `regen-generated.ps1` clean or deliberately refreshed per change; `Ara3D.SDK.ConformanceTests` 0 fail.

## Prevention

- Wiring the forward conformance BUILD into `check-all.ps1` once green (already noted in plato-308).
- Per-cluster regression fixtures in the writer test suite.
