# Type-checker + TIR backend — handoff

A native type-checker front-end and Typed-IR (TIR) backend for the Plato compiler, built as a
sequence of **shadow-mode** passes that culminate in a **flag-gated** C# emitter. Everything is
committed to `main` (github.com/cdiggins/plato). The default code-generation output is **byte-identical
throughout** — `tools/regen-plato.ps1` = 164 identical / 0 differing at every commit.

This doc is the durable handoff. A fresh agent (any model) should be able to continue from here by
reading this plus the four scope docs and the code it points to. **Read this first, then the linked
docs, then the code — do not rely on any prior chat history.**

## Pipeline

```
source → AST → bound Symbol graph → Normalize → Constrain → Solve → Elaborate → Monomorphize → Emit (TIR, behind UseTir flag)
```

Everything from **Normalize** onward is new. It lives in `PlatoCompiler/Checking/` (the checker + IR)
and `Plato.CSharpWriter/TirCSharpBodyWriter.cs` (the TIR→C# writer). It runs in shadow mode: it
computes a fully-typed, fully-resolved view and (behind an off-by-default flag) can emit C# from it,
but the default pipeline is unchanged.

## Read these first (the durable context)

- `docs/compiler-pipeline.md` — normalize / constrain / solve, the IRs, invariants, diagnostic codes.
- `docs/elaborate-emit-plan.md` — the elaborate → monomorphize → emit phase scope + the TIR node shape.
- `docs/monomorphize-plan.md` — monomorphization driven off the `ReifiedFunction` oracle.
- `docs/emit-retarget-plan.md` — the TIR→C# emit, the differential method, the flip criteria.
- Code: `PlatoCompiler/Checking/{Normalizer, NormalizationInvariants, CheckerModel, ConstraintGenerator,
  Solver, TypeChecker, Tir, Elaborator, TypeSubstitution, Monomorphizer}.cs`;
  `Plato.CSharpWriter/TirCSharpBodyWriter.cs`; the `UseTir` flag in `Plato.CSharpWriter/{CSharpWriter,
  CSharpTypeWriter}.cs`.

## State (all done + committed)

| Pass | State | Key numbers |
|---|---|---|
| **Normalize** | done | invariant-enforcement + eta-expansion (parser already lowers operators/UFCS/indexers); behavior-preserving, idempotent |
| **Constrain** | done | bidirectional constraint generation over `TypeExpression` |
| **Solve** | done | total (located diagnostics, never throws); tiered matching exact<generic<concept<conversion, occurs check, concept satisfaction + Self return refinement, implicit casts, generic-concept element inference. **246/823** stdlib functions resolve cleanly |
| **Elaborate** | done | per-function `TirFunction`; **823** elaborate without throwing, **549** fully; 18 `TirCoerce` (explicit implicit-conversions) |
| **Monomorphize** | done | driven off `Compilation.ReifiedFunctions` (**3245**, 1:1); **624** fully-ground TIR; concept re-dispatch **96 re-pointed / 127 deferred** (unique-match-only, never mis-dispatches) |
| **Emit** | done (probe + flag) | `TirCSharpBodyWriter`; differential **611/611 = 100%**; `UseTir` flag **off by default**; flag-ON full library **164/164 to golden**; **611 bodies from TIR / 1368 fallback** |

The headline: **the TIR emit path reproduces the current C# output exactly for the 611 fully-ground
bodies it covers**, behind a flag, with the current writer as fallback for the other 1368 — and the
whole library stays byte-identical with the flag on.

## Hard rules (from `CLAUDE.md`)

1. **Byte-identity is THE gate.** From `C:\Users\cdigg\git\studio`, `.\tools\regen-plato.ps1` must end
   `Result: … in sync.` (**164 identical, 0 differing**; Intrinsics 22/0). It runs the DEFAULT path
   (flag off). Never let default output drift.
2. **.NET 8 everywhere.** The whole toolchain was standardized on net8; do not reintroduce net9.
3. **No commits unless asked.** Never touch/stage: `parakeet/`, `plato-src/`, `.temp/`, `COMMIT_MSG.txt`,
   `*.csproj.user`, `docs/additions.plato`.
4. Generated code compiles on net8.0, default LangVersion.

## Build / test / gate (from `C:\Users\cdigg\git\studio\submodules\Plato`; use absolute paths)

- Build compiler: `dotnet build PlatoCompiler/Plato.Compiler.csproj -c Debug`
- Build codegen path: `dotnet build Plato.CLI/Plato.CLI.csproj -c Release`
- Full test suite (currently **79/79**): `dotnet test PlatoTests/PlatoTests.csproj -c Debug`
- Differential / flag-on reports: add `--filter "FullyQualifiedName~EmitDifferential|FullyQualifiedName~EmitFlagOn" --logger "console;verbosity=detailed"`
- Byte-identity gate (PowerShell, from studio root): `.\tools\regen-plato.ps1`
- Flag-on generation: `Plato.CLI … --use-tir`
- **Gotcha:** dotnet build servers accumulate and lock files; a stalled build or a `CS2012 … being used
  by another process` means run `dotnet build-server shutdown` and retry (not a code error).

## Next: increment 3 — the finish line (flip the default)

The flip's *data* criteria are already met for the default C# style (differential 100%; flag-on library
== golden). What remains before the emit-time heuristics can be **deleted** (not just shadowed):

1. **Shrink the 1368-body TIR fallback to zero (default style).** The fallback is exactly the two
   deferred monomorphize classes:
   - **residual `$`-element types the checker didn't ground** — loops back into the solver's
     generic-concept element inference (make those arg types ground so the call monomorphizes);
   - **concept methods whose concrete impl is synthesized field-wise by the writer at emit time** and
     never enters `ReifiedFunctionsByName` (e.g. `CreateFromComponent(s)`) — these must be modeled in
     the TIR so re-dispatch can find them.
   A fallback in place means the heuristics can't be removed, only shadowed — so this is the true
   prerequisite.
2. **Extend the TIR writer to the other styles** (extension / scalar / optimize) and the TS/Rust
   writers — or keep those on the current writer during a staged flip.
3. **Flip default-style**: gate on `regen-plato.ps1` green with `UseTir` on (already true today) **and**
   fallback count = 0, then delete `HasArgList` / `MovedNoArgNames` / property-guessing / scalar
   re-inference from `CSharpFunctionBodyWriter`.

**This is the hardest increment** — it reaches back into the checker. Use a strong (opus-class) model.

## Known finding (pre-existing, unrelated to this work)

The C# writer holds **process-global monotonic counters** (`_var{N}` lambda-capture temporaries;
`{Library}_{N}` ids like `Core_7` in ambiguity debug comments) that **don't reset between `WriteAll`
runs** — so two generations in one process diverge by ~58 files. It's masked today because the CLI runs
fresh per invocation (the golden is stable). Worth fixing (reset per `WriteAll`) but not blocking.

## Commit history (`main`, most recent last)

```
7ffa342  type-checker front-end: normalize, constrain, solve (shadow mode)
adb113a  Solver: concept satisfaction, Self return refinement, implicit casts
59355ac  Solver: generic-concept element inference (post-commit refinement)
a83e40a  Elaborate pass: Typed IR (TIR) in shadow mode
c116195  Monomorphize pass: TIR specialization + driver (shadow mode)
6c95998  Emit probe: TIR→C# body writer + differential; standardize on .NET 8
<next>   Emit increment 2: taxonomy → 100%, off-by-default UseTir flag
```
