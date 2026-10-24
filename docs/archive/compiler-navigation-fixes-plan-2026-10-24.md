# Compiler and navigation failure fixes — 2026-10-24

**EXECUTED — 2026-10-24.** [Results and limitations](../reports/compiler-navigation-fixes-results-2026-10-24.md) record the completed compiler/navigation work; the external Studio frozen-artifact gate remains under repo-473.

Issue: [repo-479](../../tracker/issues/repo-479.md), closed. Contract: failures-v1. Baseline: `a1c37ede`, current branch `main`, shared checkout `C:\Users\cdigg\git\plato`.

## Ownership and acceptance

| Owner | Exclusive writable scope | Outcome |
|---|---|---|
| Compiler Sol | `src/Plato.Compiler/Checking/`, `writers/Plato.CSharpWriter/`, the two root conversion inventory tests and their baseline files, new conversion tests in `tests/PlatoTests/Checking/` | Review every conversion difference, fix invalid behavior or accept justified inventories; compiler-475 |
| Writers Sol | `writers/Plato.TypeScriptWriter/`, `tests/PlatoTests/Emission/TypeScriptEmitFlagOnTests.cs`, new TypeScript tests in that directory | Correct legacy/TIR emission and verify runtime spelling; plato-438 |
| Runtime Sol | `src/Plato.Navigation/`, `tests/Plato.Navigation.Tests/`, `src/Plato.Compiler/Symbols/` | Restore assignment references, safe simplification and meaningful rebuild performance; compiler-472, compiler-477, compiler-478 |
| Supervisor | All other paths, tracker, gate outputs, baseline fixtures and integration | Correct intrinsic-location check under stdlib-476, coordinate checks and benchmark, publish |

Agents may write checkpoints under `.temp/failure-fixes/<owner>/`. Shared manifests, tracker files, other compiler folders and project files require a supervisor handoff. No agent runs builds, restores, generators, tests against mutable shared inputs, Git staging or commits without a resource/commit grant. Supervisor owns report writes, build outputs and benchmark processes. Agents request commit turns and commit their own verified chunks with explicit pathspecs; supervisor updates issue checkboxes in the same work commit and closes completed issues immediately afterwards. No branch switches, worktrees, submodule edits or outside-repository fixes.

## Sequence and checks

Preserve the current binaries and source corpus before rebuilding. Existing original-assembly replays establish the initial failures; use focused tests during implementation. Independent investigation and edits proceed in parallel, followed by stable-input build/test windows. Keep source headers and folder documentation accurate.

Do not weaken contracts or refresh conversion inventories without explaining each difference. Do not raise a timing ceiling to conceal a slowdown. Profile parsing, binding and indexing if navigation remains slow; compare the same source snapshot before and after any optimization. Do not replace unimplemented library behavior with fake bodies.

Run complete compiler and navigation suites at integration, relevant execution tests when compiler semantics change, `tools/record-gates.py`, and studio `check-all.ps1` once. Attribute any external frozen-SDK gate failure separately; this mission does not own that checkout. Alternate baseline/current CLI generation measurements on identical inputs after other owned timed workloads stop; compare generated output and retain logs. Report periodic substantive checkpoints, actual pass/fail evidence, timing spread and limitations. Archive this plan on closure.
