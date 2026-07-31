---
id: plato-310
title: CSharpWriter mangles Function1-typed values: System.Func loses type args, constructor calls emit as extension calls
type: bug
status: done
priority: p2
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [tracker/issues/plato-308.md, submodules/Plato/stdlib/fields-implicits-shapes.library.plato, submodules/Plato/conformance/Plato.ForwardConformanceTests/Generated/_FunctionVolume3D.g.cs, submodules/Plato/Plato.CSharpWriter]
---

## Issue

Split out of [plato-308](plato-308.md) Root 1 after analysis on 2026-07-29: a large slice of that
issue's 134 CS0305 errors is **not** the concept-lowering design gap — it is plain emitter bugs on
`Function1`-typed values, where the Plato source is already correct.

Two observed manglings, forward-stdlib codegen (`.\tools\regen-forward-conformance.ps1 -Codegen`):

1. **Type-argument loss.** `Function1<A, B>` maps to `System.Func`, but on some emission paths the
   type arguments are dropped, producing bare non-generic `System.Func` (CS0305 "wrong generic
   arity"). Field positions come out right (`System.Func<Point3D, bool> Function` in
   `_FunctionVolume3D.g.cs:16`), but method return types, operator parameter types, and extension
   receivers come out bare (`public System.Func Complement()` at `_FunctionVolume3D.g.cs:45`;
   `this System.Func self` receivers throughout `FieldsImplicitsCore.g.cs:46-55`).
2. **Constructor-call mangling.** `FunctionVolume3D(p => self.Eval(p).Not)` — a constructor call
   with a lambda argument — emits as an extension-method call **on the lambda**:
   `((p) => ((bool)_var41.Eval(p)).Not()).FunctionVolume3D()` (`_FunctionVolume3D.g.cs:47`).

Observed vs expected: source declaration `Complement(self: FunctionVolume3D): FunctionVolume3D`
(`fields-implicits-shapes.library.plato:125`) should emit
`public FunctionVolume3D Complement() => new FunctionVolume3D(p => ...)` (modulo TIR shape); it
emits neither the right return type nor a constructor call.

## Impact

Accounts for much of the CS0305 cluster in plato-308 (files: `FieldsImplicits*`,
`ImplicitSdfTrees`, `FunctionalProcedural`, `_FunctionVolume3D`, `_FunctionRegion2D`,
`_FunctionSdf2D/3D`). Until fixed, the forward conformance suite cannot build even after the
concept-lowering design question ([plato-311](plato-311.md)) is settled — the two failure classes
overlap in the same files but are independent.

## Affected code

- `submodules/Plato/Plato.CSharpWriter` — the `Function1..4` to `System.Func` mapping (likely a
  type-rendering path that consults the mapped intrinsic name but not its arguments) and the
  TIR call-emission path that renders a constructor invocation as receiver-style extension call.
- `submodules/Plato/conformance/Plato.ForwardConformanceTests/Generated/_FunctionVolume3D.g.cs:45-50` — smallest complete repro artifact.
- `submodules/Plato/stdlib/implicit-sdf-function.plato:43` + `fields-implicits-shapes.library.plato:125-127` — the clean source that produces it.

## Cause / analysis

Speculation, to confirm in the writer: the concept-to-intrinsic map (`Function1` maps to
`System.Func`) is applied by name in some type-writing paths without carrying the instantiated
type arguments; field emission goes through a different (correct) path. The constructor mangling
looks like the TIR expresses single-field-type construction as a conversion/method application and
the extension-style writer renders it postfix. Both only bite the forward stdlib because
`stdlib-legacy` never stores or returns `FunctionN` values from library functions this way.

## Priority

p2, same as its parent plato-308: it gates the same deliverable (executing forward law runner) and
is the cheap half of Root 1. Deferral cost: plato-311's design work cannot be validated end-to-end
while these mechanical errors mask the build.

## Dependencies

- Blocks: [plato-308](plato-308.md) (build of `Plato.ForwardConformanceTests`), indirectly [plato-306](plato-306.md).
- Blocked by: nothing — independent of [plato-311](plato-311.md).
- Touches: `Plato.CSharpWriter` type/call emission — shared with the legacy generation; off-flag
  byte-identity of `Generated/` goldens must hold (or be refreshed deliberately in the same change).

## Fix approaches

1. Fix the type-rendering path to always render mapped intrinsics with their instantiated
   arguments; fix the call-emission path to detect constructor targets. Straight bug fix.
2. While there, add a writer-level assertion that a mapped generic intrinsic is never emitted with
   zero type arguments when its Plato type has arity > 0 — turns silent garbage into a codegen error.

## Bedrock

Strengthens the `Plato.CSharpWriter` invariant that a rendered type always carries the full
instantiation of its TIR type — the seam the arity assertion (approach 2) makes permanent. Verdict:
**simplest** — approach 1 + 2 together are the whole fix.

## Progress (2026-07-29)

**Attribution note:** the emitter fix for this issue is committed in Plato `f859808`, whose message
credits only [plato-311](plato-311.md). Two agents worked the same checkout concurrently and their
edits to `CSharpFunctionInfo.cs` / `CSharpTypeWriter.cs` / `ITypeToCSharp.cs` /
`TirCSharpBodyWriter.cs` / `Definitions.cs` were interleaved, so they were committed together as one
save point rather than split by guesswork. Do not read `f859808`'s subject as the scope of its diff.

Measured after that commit (`regen-forward-conformance.ps1 -Codegen`, 1240 `.g.cs`):

- **0 bare `System.Func`** anywhere in `Generated/` (grep over all 1240 files).
- Constructor form correct: `_FunctionVolume3D.g.cs:45-48` emits
  `public FunctionVolume3D Complement() => ... new FunctionVolume3D((p) => ...)` — no extension-call
  mangle on the lambda.
- **CS0305 count 134 -> 0.** The 398 remaining build errors are 392 CS0535 + 6 CS0557, none CS0305.
  Of the CS0535, 342 are plato-311's new non-generic view members and ~50 are the pre-existing
  plato-308 Root 2 class — neither attributable to this issue.

Shared-writer gates run 2026-07-29: `check-frozen-v1.ps1` PASS (0 changed of 210),
`Ara3D.SDK.ConformanceTests` 205/205, `regen-generated.ps1` drift confined to plato-311 (detail in
the boxes below). All four done-means boxes are satisfied. Remaining before close are the two
optional hardening items from Fix approaches / Prevention: the arity assertion (approach 2) and the
`Function1`-storing forward fixture in `ForwardStdLibCheckerTests`. Note the issue cannot be closed
independently of a green `Plato.ForwardConformanceTests` build only if that is made a close
condition — the errors blocking it now are plato-311 and plato-308 Root 2, not this bug.

## Done means

- [x] `_FunctionVolume3D.g.cs` regenerates with `FunctionVolume3D Complement()` returning a
      constructor call, and no bare `System.Func` appears anywhere in `Generated/` — verified 2026-07-29.
- [x] CS0305 count in `dotnet build Plato.ForwardConformanceTests` drops to only errors attributable
      to [plato-311](plato-311.md) (concept-in-type-position), each verified as such — CS0305 is 0; remainder classified above.
- [x] `tools\regen-generated.ps1` clean (legacy goldens unchanged, or refreshed deliberately with rationale) —
      **no drift attributable to this issue.** The gate reports exactly one differing file per variant,
      `Interfaces.g.cs`, and its whole diff is [plato-311](plato-311.md)'s interface emission (added
      non-generic views + base-list additions). Nothing `FunctionN`-related drifted.
- [x] `Ara3D.SDK.ConformanceTests` still 0 fail — 205/205 passed 2026-07-29 (goldens at baseline).

## Prevention

- The approach-2 writer assertion (arity-0 render of a generic intrinsic = hard error) prevents the
  whole class of silent argument loss.
- A tiny forward-stdlib fixture that stores and returns a `Function1` value, compiled in
  `ForwardStdLibCheckerTests`' codegen gate, would catch regressions at test time instead of 1232
  files later.
