# Plato — Codebase Assessment & Status

*Written 2026-07-10 for Christopher Diggins (AI-authored, Claude). A candid state-of-the-code
assessment after the type-checker/TIR increment 3 landed: what works, what changed since the
legacy snapshot, and what to simplify — with special attention to what makes the codebase easy
or hard for agents to work on. Where it recommends, it recommends; the decisions are yours.*

---

## Where the generated C# lives

| What | Where |
|---|---|
| **Production output (default style)** | `ara3d-sdk/src/Plato.Generated/` — 164 checked-in files; `_Vector3.g.cs` etc. per type, plus `Interfaces.g.cs`, `Constants.g.cs`, `Extensions.g.cs` |
| Extension-style golden (V2 adoption shape) | `submodules/Plato/golden/Plato.Generated.V2/` |
| Handwritten runtime it sits on | `submodules/Plato/Plato.Intrinsics/` (source of truth; `ara3d-sdk/src/Plato.Intrinsics` is the byte-identical synced copy) |
| Conformance outputs | `submodules/Plato/conformance/*/Generated/` — script-produced, gitignored |

The checked-in copy is exact: `tools/regen-plato.ps1` regenerates and diffs on every change
(currently 164 identical / 0 differing, intrinsics 22/0).

## The shape of the codebase

~4,300 lines of Plato stdlib (`stdlib-legacy/`, 30 files) compile through an ~8,500-line compiler
(`PlatoCompiler/`, of which ~2,600 is the new `Checking/` checker+TIR) into ~4,900 lines of C#
writer (`Plato.CSharpWriter/`), landing on a handwritten intrinsics runtime. TS/Rust writers
exist but are out of scope. Everything is .NET 8.

## What works well today

1. **The byte-identity discipline.** Every emitter change is gated by regenerating the whole
   library and diffing against the checked-in golden. This is the single best property of the
   codebase: it converts "did I break codegen?" from a judgment call into a yes/no answer, and it
   is what made it possible to swap the entire body-emission engine (below) with zero risk.
2. **The gate battery.** `check-all.ps1` runs 8 gates (regen diff, lint, four conformance suites,
   SDK build, geometry tests) and prints a PASS/FAIL table. An agent — or you, six months from
   now — can verify the whole world in ~2.5 minutes. All 8 pass today.
3. **A real type checker now exists and is load-bearing.** `Normalize → Constrain → Solve →
   Elaborate → Monomorphize` resolves 745/823 stdlib functions with zero errors and produces a
   fully-typed IR (TIR). As of increment 3, **every default-style member body (1,914 of them) is
   emitted from the TIR** — the legacy guess-at-emit-time path no longer produces the bulk of the
   library. The checker never throws; failures are located diagnostics pointing at expressions.
4. **The conformance/law system.** `Law_*`/`Witness_*` functions in `stdlib-legacy-tests/` are
   executed reflectively against every implementing type. The `KnownFailures.json` manifest is
   **empty** — the 36-entry bug wave is fully burned down, and a fixed-but-still-listed entry
   fails the build, so the manifest can't rot.
5. **The stdlib itself.** Concept-based (interfaces with default implementations), genuinely
   generic (one `MapComponents` serves every vector-like type), and readable. The `stdlib as
   oracle` strategy — maturing the checker against the real library instead of synthetic tests —
   paid off repeatedly.
6. **Documentation-as-handoff.** `docs/type-checker-handoff.md` plus four scope docs let a fresh
   agent continue multi-week compiler work with no chat history. This increment was executed
   exactly that way, and the pattern is worth keeping for every workstream.

## What changed since the legacy snapshot (`stdlib-snapshot-2026-07-09`, frozen 2026-07-09)

- **Compiler bugs fixed for real**: the additive-associativity parser bug (miscompiled `a - b + c`),
  the `Vector3(0.0)` prefix-conversion papercut, `Point2D.Subtract`, dead where-clauses, `Time`
  measure obligations — the whole catalogued wave; conformance went from 36 quarantined failures
  to 0.
- **Library content**: Sdf3D (signed-distance primitives + CSG), ScalarField3D function-valued
  fields + combinators, 2D vector parity (Cross/Perpendicular/Rotate), interpolation helpers,
  affine (`unique`) List/Buffer builder types with runtime checks.
- **The entire checker/TIR arc** (8 commits, this one pending): shadow-mode checker → typed IR →
  monomorphization off the `ReifiedFunction` oracle → differential-proven TIR emitter → **TIR as
  the production default**. The default output is byte-for-byte what the legacy writer produced,
  but it is now generated from a resolved, typed representation instead of syntax heuristics.
- **Emitter options matured**: extension style, scalar erasure, component-unrolling optimizer —
  each with its own conformance suite.
- Toolchain standardized on .NET 8 everywhere.

## What to improve (ranked by leverage, agent-oriented)

1. **Finish retiring the legacy body writer.** Today there are *two* emission engines: the TIR
   writer (default-style member bodies) and `CSharpFunctionBodyWriter` (static bodies in
   `Constants.g.cs`/`Extensions.g.cs`, plus the extension/scalar/optimize styles). The legacy one
   carries the heuristics the TIR was built to kill: `HasArgList` parens-guessing,
   `MovedNoArgNames`, property-vs-method guessing, scalar re-inference. Until the TIR covers
   static bodies and the other styles, none of that can be deleted, and every agent must
   understand both engines. This is the highest-leverage simplification available: one emission
   path, driven by types, with ~1,500 lines of heuristics deleted at the end.
2. **Declare the handwritten intrinsics to the compiler.** The 78 functions the checker can't
   fully resolve are mostly calls to handwritten members it cannot see (`Matrix4x4.CreateTranslation`,
   `Number.MinValue`). They emit fine (name+shape), but they are typed holes. Declaring those
   signatures in `stdlib-legacy/intrinsics.plato` would (a) close the checker gap, (b) let the
   permissive rules added in increment 3 (`Self` unifies with anything; unresolved calls emit
   syntactically) be *tightened back* toward soundness, and (c) unblock trustworthy TS/Rust
   emission, which needs types, not shapes.
3. **Fix the CLI's silent-success trap.** `Plato.CLI` exits 0 even when compilation fails. Every
   script compensates by counting output files. For agents this is the classic silent-failure
   hazard; a non-zero exit on errors is a one-day fix that removes a whole class of wasted runs.
4. **Reset the process-global counters.** `SymbolRewriter.NextId` (`_var{N}` temporaries) and the
   `{Library}_{N}` ids never reset between `WriteAll` runs, so two generations in one process
   differ on names alone. It's masked (the CLI runs fresh per invocation) and the tests
   canonicalize around it, but it's a landmine — reset per `WriteAll` in both writers at once.
5. **Prune stale agent guidance.** `submodules/Plato/CLAUDE.md` still says the associativity bug
   is unfixed ("DO NOT fix until Phase 4" — it was fixed in `392dfa8`) and understates the CLI
   flags (no `--no-tir`). Stale CLAUDE.md is worse than none for agents: it gets followed.
   Similarly: `PROGRESS.md` now holds two finished missions' logs; archive them.
6. **Consolidate the resolution machinery.** There are now *three* answers to "which function does
   this call bind to": the writer's `ChooseBestFunction` (Analysis), the reifier
   (`ReifiedType`/`ReifiedFunction`), and the checker's solver. They agree today because the
   gates force them to, but they are separate codebases with separate bugs. Long-term, the
   checker should be the single authority and the other two should consume its output.
7. **Delete vestigial code.** `PlatoWinFormsEditor`, the commented-out JavaScript writer,
   `CHANGE_PRECISION` ifdefs, the commented `Constructors.g.cs` block, `stdlib-legacy/unused/`. Each
   is small; together they are noise an agent has to read past.
8. **Quiet the test output.** Every stdlib-compiling test prints hundreds of parser log lines,
   which makes grepping test output for real signal genuinely painful (this cost me time
   repeatedly this session). Route the parser logger to null under test.

## The options, in plain language

**CLI** (`Plato.CLI <input> <output> [flags]`):

| Flag | What it does |
|---|---|
| *(no flags)* | Default C# style: one partial struct per type with instance members. This is production — what `Plato.Generated` contains. Bodies come from the typed IR (since increment 3). |
| `--no-tir` | Use the old syntax-heuristic body writer instead of the typed IR. Output is identical; this is the escape hatch / A-B switch. |
| `--csharp-style=extensions` | Library functions become classic C# extension methods in static classes instead of struct members. The intended future shape (V2). |
| `--scalar=float` | Erase the `Number`/`Integer`/`Boolean` wrapper structs; generated code uses raw `float`/`int`/`bool`. Requires extensions style. For performance/interop. |
| `--optimize` | Unroll component-wise operations (`MapComponents` on a `Vector3` becomes three field expressions instead of an array round-trip). |
| `--typescript` / `--rust` | The other backends. Exist, compile, out of scope, and — worth saying plainly — will stay second-class until they consume the TIR. |
| `lint <folder>` | Static checks LINT001–005 (unused fields, duplicate signatures, …). |

**Gates** (from the studio root):

| Command | What it proves |
|---|---|
| `tools\regen-plato.ps1` | The compiler still produces byte-for-byte the checked-in library, and the intrinsics copies match. **The** gate. |
| `tools\check-all.ps1` | Everything: regen, lint, all four conformance suites, SDK build, geometry tests. Run once at the end of any change. |
| `dotnet test submodules/Plato/PlatoTests` | Compiler internals: checker, TIR, and the three flip invariants (TIR bodies byte-identical; flag-on == flag-off library; fallback count == 0). 80/80. |

## Bottom line

The codebase is in the best state it has ever been: gates green across the board, zero quarantined
bugs, and a real type checker in production for the main emission path. Its biggest structural
debt is *duplication with itself* — two body writers, three resolution mechanisms, four styles —
all held consistent by tests rather than by construction. The next increment that retires the
legacy writer is where the simplification payoff lands; everything above it on the list is either
quick (CLI exit code, stale docs, counters) or compounding (intrinsic declarations feed the
checker, the checker feeds every backend).
