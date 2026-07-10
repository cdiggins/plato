# Type-checker + TIR backend — handoff

A native type-checker front-end and Typed-IR (TIR) backend for the Plato compiler. As of
**increment 3** the TIR is the **production emit path for the default C# style**: every
member-instance body with Plato source is emitted from the monomorphized TIR (`UseTir` defaults
to **true**), byte-identical to the legacy symbol-graph writer. `tools/regen-plato.ps1` = 164
identical / 0 differing at every commit — the gate now exercises the TIR path.

This doc is the durable handoff. A fresh agent (any model) should be able to continue from here by
reading this plus the four scope docs and the code it points to. **Read this first, then the linked
docs, then the code — do not rely on any prior chat history.**

## Pipeline

```
source → AST → bound Symbol graph → Normalize → Constrain → Solve → Elaborate → Monomorphize → Emit
                                                                     (default-style member bodies: TIR; everything else: legacy writer)
```

Everything from **Normalize** onward lives in `PlatoCompiler/Checking/` (the checker + IR) and
`Plato.CSharpWriter/TirCSharpBodyWriter.cs` + `TirLambdaCaptureRewriter.cs` (the TIR→C# writer).

## Read these first (the durable context)

- `docs/compiler-pipeline.md` — normalize / constrain / solve, the IRs, invariants, diagnostic codes.
- `docs/elaborate-emit-plan.md` — the elaborate → monomorphize → emit phase scope + the TIR node shape.
- `docs/monomorphize-plan.md` — monomorphization off the `ReifiedFunction` oracle + residual grounding.
- `docs/emit-retarget-plan.md` — the TIR→C# emit, the differential method, the flip.
- Code: `PlatoCompiler/Checking/{Normalizer, NormalizationInvariants, CheckerModel, ConstraintGenerator,
  Solver, TypeChecker, Tir, Elaborator, TypeSubstitution, Monomorphizer}.cs`;
  `Plato.CSharpWriter/{TirCSharpBodyWriter, TirLambdaCaptureRewriter}.cs`; the `UseTir` flag in
  `Plato.CSharpWriter/{CSharpWriter, CSharpTypeWriter}.cs`.

## State (all done + committed)

| Pass | State | Key numbers (live in test output) |
|---|---|---|
| **Normalize** | done | invariant-enforcement + eta-expansion; behavior-preserving, idempotent |
| **Constrain** | done | bidirectional; unannotated lambda params/locals ("Any") become inference holes; type-as-value; nullary-constant groups typed; RETURN positions emit coercion (not equality) constraints |
| **Solve** | done | total; tiered matching exact<generic<concept<conversion; concept-method `Self` instantiated as concept-constrained var (Self-return refinement); concept satisfaction walks the transitive Implements/Inherits closure with per-level type-arg substitution and binds element holes; `Self` unifies permissively (grounded at monomorphize); HOF scheduling (function-shaped args don't block resolution; forced resolutions re-enter the fixpoint); return coercions (cast relation / tuple→same-shape struct / value→implemented interface). **745/823** stdlib functions resolve with zero errors |
| **Elaborate** | done | **823/823 elaborate fully** (no unresolved nodes): local `var` decls (`TirLet`), type-as-value (`TirTypeRef`, namespace-qualified for raw `TypeExpression`s), bare constant groups (zero-arg `TirCall` → `Constants.X`), bare multi-overload names (`TirName`), and SYNTACTIC calls (null-callee `TirCall`: name + shape, for targets the compiler cannot see, e.g. handwritten intrinsic statics). Recorded call signatures re-zonked live |
| **Monomorphize** | done | **3247** instantiations (3245 reified 1:1 + 2 non-reified concrete-first-param library functions); **1998/2014** bodied instantiations fully ground. Grounding = declared-signature pairing + solver-ZONKED-signature pairing (terminal-form vars) + element-instance walk (`IArrayLike<$T>` ↔ `Vector3` binds `$T→Number`) + post-specialization residual grounding (return positions vs the reified/Self-refined return incl. pre-coercion call types; leftover interface instances vs Self's concept instances). Re-dispatch preserves the call-site `EmissionKind` (shape) |
| **Emit** | **DEFAULT** | `UseTir` **on by default** (CLI `--no-tir` = legacy path). **The default C# style is single-engine**: member bodies emit from the fully-ground monomorphized TIR and STATIC bodies (`Constants.g.cs`, `Extensions.g.cs`) from the generic elaborated TIR — 2111 bodies total, **fallback = 0** (asserted by `EmitFlagOnTests` + `FallbackDiagnosticsTests`); the legacy writer's only default-style job is the fixed throw-stub for body-less functions. Flag-on vs flag-off **164/164 files**, compared EXACTLY (the `_var{N}` counter now resets per `WriteAll`); `regen-plato.ps1` green on the TIR path. Lambda-capture hoist mirrored on TIR (`TirLambdaCaptureRewriter`) |

## Hard rules (from `CLAUDE.md`)

1. **Byte-identity is THE gate.** From `C:\Users\cdigg\git\studio`, `.\tools\regen-plato.ps1` must end
   `Result: … in sync.` (**164 identical, 0 differing**; Intrinsics 22/0). It runs the DEFAULT path —
   which is now the TIR path. Never let default output drift.
2. **.NET 8 everywhere.** Do not reintroduce net9.
3. **No commits unless asked.** Never touch/stage: `parakeet/`, `.temp/`, `COMMIT_MSG.txt`,
   `*.csproj.user`, `docs/additions.plato`.
4. Generated code compiles on net8.0, default LangVersion.

## Build / test / gate (from `C:\Users\cdigg\git\studio\submodules\Plato`; use absolute paths)

- Build compiler: `dotnet build PlatoCompiler/Plato.Compiler.csproj -c Debug`
- Build codegen path: `dotnet build Plato.CLI/Plato.CLI.csproj -c Release`
- Full test suite (currently **80/80**): `dotnet test PlatoTests/PlatoTests.csproj -c Debug`
- Flip-invariant gates: `--filter "FullyQualifiedName~EmitDifferential|FullyQualifiedName~EmitFlagOn|FullyQualifiedName~FallbackDiagnostics"`
- Byte-identity gate (PowerShell, from studio root): `.\tools\regen-plato.ps1`
- Legacy body writer: `Plato.CLI … --no-tir`
- **Gotcha:** dotnet build servers accumulate and lock files; a stalled build or a `CS2012 … being used
  by another process` means run `dotnet build-server shutdown` and retry (not a code error).

## What increment 3 actually found (differs from the old prediction)

The 1368-body fallback was **not** mostly the two deferred monomorphize classes. Measured
classification (`FallbackDiagnosticsTests`) showed 1247/1304 were **solver-level unresolved
overloads** in generic library bodies — five root causes, all fixed in the checker: unannotated
lambda params bound to the placeholder type `Any`; concept-method candidates with rigid `Self`
params never matching interface receivers; concept satisfaction failing on type-arg-count mismatch
before walking the closure (`IVectorLike` vs `IArrayLike<$G>`); return-position implicit
conversions failing hard unification (tuple→struct, `Vector3`→`Point3D`); and bare
constant/multi-overload group references left untyped. The residual `$`-element-type class was
real but downstream of a *snapshot* problem: `CommitCandidate` records signatures zonked at commit
time, so later unifications didn't land — fixed by live re-zonking at elaboration plus zonked-
signature pairing at monomorphize. The "field-wise-generated concept impl" class turned out not to
block emission at all (emission is name+shape; re-dispatch is only an identity refinement).

## Next increments (in order of leverage)

1. ~~Retire the legacy body writer for the remaining default-style bodies~~ **DONE 2026-07-10**:
   static bodies (`Constants.g.cs`, `Extensions.g.cs`) emit from the generic elaborated TIR
   (`TirCSharpBodyWriter` static mode; `CSharpWriter.TryGetStaticTir`).
2. **Extend to the other styles** (extension / scalar / optimize) and the TS/Rust writers. Only
   then can the emit-time heuristics (`HasArgList` guessing, `MovedNoArgNames`, property
   guessing, scalar re-inference) actually be **deleted** — today they are dead for the whole
   default style, but every other style still runs them.
3. **Checker completeness**: 78/823 stdlib functions still carry located diagnostics (they emit
   fine — syntactic calls — but the checker's view is incomplete: mostly calls into handwritten
   intrinsic members it cannot see, plus a few genuine ambiguities worth surfacing to the author).
4. ~~Process-global name counters~~ **DONE 2026-07-10**: `SymbolRewriter.NextId` resets per
   `WriteAll` in all three writers; the Symbol-id leak into output (`Geometry_15` in the retired
   "AMBIGUOUS FUNCTIONS" comments) is gone.

## Commit history (`main`, most recent last)

```
7ffa342  type-checker front-end: normalize, constrain, solve (shadow mode)
adb113a  Solver: concept satisfaction, Self return refinement, implicit casts
59355ac  Solver: generic-concept element inference (post-commit refinement)
a83e40a  Elaborate pass: Typed IR (TIR) in shadow mode
c116195  Monomorphize pass: TIR specialization + driver (shadow mode)
6c95998  Emit probe: TIR→C# body writer + differential; standardize on .NET 8
43503b7  Emit increment 2: taxonomy → 100%, off-by-default UseTir flag
c7b1e1e  docs: type-checker + TIR backend handoff
<next>   Increment 3: checker closes the fallback (745/823 clean), TIR emit is the default
```
