---
id: plato-291
title: CSharpWriter: no ground TIR for bodied concrete members blocks forward-stdlib codegen
type: bug
status: done
priority: p2
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-28
closed: 2026-07-29
links: [submodules/Plato/Plato.CSharpWriter/CSharpTypeWriter.cs, submodules/Plato/stdlib, tools/regen-forward-conformance.ps1]
---

Found 2026-07-28 while wiring the forward stdlib into testing. The forward stdlib
type-checks whole-tree (9075 functions, 0 parse / 0 resolution errors) but both the plain
and full V2 codegen recipes abort at `Plato.CSharpWriter/CSharpTypeWriter.cs:282`:

    InvalidOperationException: No ground TIR for bodied AnimationTrack.ValueAt

The writer requires every bodied concrete-type member to have a fully-ground monomorphized
TIR; the forward stdlib has bodies that do not monomorphize, and the writer throws on the
first one, aborting all output (AnimationTrack.ValueAt is only the first of an unknown
number). Whole-tree codegen is required -- a foundation-only subset gives 68 resolution
errors, so subsetting is not a workaround.

This is THE blocker for executing forward-stdlib bodies. Ready and waiting on it:
- `stdlib-tests/foundation.laws.plato` (15 Law_* functions, Plato commit aa43e19)
- `conformance/Plato.ForwardConformanceTests` (reflection law runner + KnownFailures +
  BlockerGuardTests against vacuous passes; not in any .sln)
- `tools/regen-forward-conformance.ps1` (Stage 1 type-check gate green today; Stage 2
  codegen reports this blocker with exit 2 -- flip it to `-Test` gating once fixed)

Start at AnimationTrack.ValueAt and burn down. Related: plato-277 (the FunctionInstance
single-generic-param guard was fixed the same day, Plato commit 925c03c -- affine builders
Count/Freeze/EmptyList now declarable).

## Progress 2026-07-29 -- writer blocker FIXED; codegen no longer aborts

The ground-TIR abort is resolved. The writer now DEGRADES GRACEFULLY: a bodied concrete
member with no fully-ground monomorphized TIR emits a `NotImplementedException`-throwing
stub that names the member, is COUNTED (new burn-down number surfaced in the CLI log as
`DEGRADED bodies: N`), and no longer aborts the rest of the output. Four abort sites were
converted (CSharpTypeWriter.WriteBody, CSharpConcreteTypeWriter scalar-ext, ExtensionStyleWriter
moved-member, and the TirCSharpBodyWriter ctor scalar-lower guard).

Measured population (full merged forward stdlib + laws, full V2 recipe): **44 degraded bodies**,
grouped by cause --
- 9  TimeVarying implementers (AnimationTrack/TangentTrack/Tween x {ValueAt,Change,SampleAtMidpoint}):
     bodies call `self.Sample(..)`, an interface obligation NO forward type implements. Genuine library gap.
- 19 Array4D.* (At/Map/Reduce/First/...): Array4D is an empty type implementing Indexable4D<T>
     with NO backing storage, so the inherited Indexable At/Count obligations have no implementer. Genuine gap.
- 14 CollectionsIndexable Map/Reduce over fixed vectors (Number2/3/4/8, Vector2D/3D, VectorN):
     a residual SECOND generic ($U, the result element) not determined by the receiver, so the
     monomorphized instance is not fully ground. Compiler/monomorphizer limitation (candidate follow-up).
- 2  RegularPolygon/RegularStar2D vertices: TIR grounds but the block-lambda's inner nodes are
     left untyped by the elaborator, so TirScalarLowerer.IsGroundBody rejects it. Compiler limitation.

None of the 44 are touched by `foundation.laws.plato` (laws target NumberInterval / IntervalLike /
Bounds2D / Bounds3D), so degradation does not block the payoff.

### Compiler bug fixed en route (was the SECOND abort)
Compound-statement lambda bodies (`i => { var x=..; return ..; }`) resolved to a NULL body and
emitted an empty lambda `(i) => )`. Root cause: `Plato.AST/AstNodeFactory.cs` tested
`cstLambdaBody.Expression != null` on a `CstNodeFilter` (never null) instead of `.Present`, always
taking the Expression branch; and `PlatoCompiler/Symbols/SymbolFactory.cs` bound the lambda body
with `ResolveExpr` (rejects a block). Both fixed. Byte-identity for stdlib-legacy is UNCHANGED
(regen-generated 184/0/0/0 both variants) -- legacy uses no block-bodied lambdas.

### Remaining blocker (NOT this issue's scope): forward stdlib does not yet COMPILE
Codegen now emits 1209 .g.cs, but the C# does not build: **332 structural errors**, by shape --
166 CS0315 (concrete type used as a self-constrained `Curve3D<Self>` arg -- F-bounded emission),
134 CS0305 (type-arg count mismatch), 14 CS0535 (unimplemented interface members), 8 CS0736 /
6 CS0557 / 4 CS0562 (duplicate/partial member clashes). These are pervasive young-vocabulary
type-modeling defects across the whole forward stdlib, independent of the ground-TIR blocker.
File a separate issue; Stage 2 gating in regen-forward-conformance.ps1 stays diagnostic (NOT flipped
to -Test) until the forward stdlib compiles. Laws could not be executed; KnownFailures unchanged.
