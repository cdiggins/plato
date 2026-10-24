# Plato duplication audit — 2026-10-24

The best first changes are to share function-selection policy across the five writers and finish consolidating the constraint-codegen test helpers. Greenhouse found useful evidence, but raw clone counts greatly overstate useful refactoring work. No production or test implementation was changed by this audit.

Audit: repo-492. Implementation follow-up: [repo-493](../../tracker/issues/repo-493.md). Tool improvement proposal: [repo-494](../../tracker/issues/repo-494.md).

## Method and limits

Used the existing Release build of dotnet-greenhouse's CLI in the sibling checkout. Its exact pass normalizes method bodies and groups matching hashes; it does not resolve whether identical names bind to identical operations. Its similarity pass identifies structural and normalized behavioral patterns, not semantic equivalence.

The whole-directory exact scan returned 2,999 clusters in 14.23 seconds. It included `.temp` backups, `.claude` worktrees and submodule sources. Retaining only members under `src/`, `writers/`, and `tests/`, excluding `bonepile`, `Generated`, `obj` and `bin`, and dropping groups with fewer than two remaining members left 84 clusters. These are candidates, not 84 recommended changes. Generated C# is filtered by Greenhouse itself. The audit covers C# methods, not Plato-language bodies, project configuration, or all embedded source strings.

A scoped emission-test similarity scan at threshold 1 returned seven clusters in 0.95 seconds. One involved the pre-existing untracked ArrayLikeCodegenTests.cs; that ongoing work is excluded from recommendations. Existing edits to CSharpConcreteTypeWriter.cs, status reports, `.greenhouse/`, and another mission's PROGRESS.md were preserved. No cached output or test execution is presented as fresh validation.

## Ranked opportunities

### 1. Share writer function selection — high confidence, small implementation

`writers/Plato.CSharpWriter/PlatoAnalyzer.cs:20-56` contains ScoreFunction and ChooseBestFunction. The same two method bodies appear in the C++, GLSL, Rust and TypeScript analyzers at lines 25-61. All five have actual callers. These methods rank intrinsics before concrete implementations, then use interface distance; they remove duplicate implementation IDs and return the first best candidate with its tie count.

Put this policy in the existing compiler analysis layer, which every writer already consumes. Retain thin backend wrappers if needed for public API compatibility. This removes four maintenance copies of each algorithm (about 144 repeated source-span lines before replacement scaffolding). It also prevents a precedence fix from reaching only one backend.

Validation: tests for empty candidates, repeated implementation IDs, each precedence level, interface distance, and stable tie ordering/counts; compare representative output from every affected writer. Do not accidentally change first-wins behavior while introducing a shared helper.

### 2. Finish sharing generated-code test setup — high confidence, small implementation

`tests/PlatoTests/Emission/FunctionConstraintCodegenTests.cs:172-201` and `TypeConstraintCodegenTests.cs:219-248` repeat New, Call and Unwrap. These adapt ordinary CLR values to generated scalar wrappers for executable assertions. SumTypeCodegenTests.cs also repeats Unwrap. The constraint fixtures duplicate the small Prelude string and almost identical Emit methods (lines 76-97 and 98-119); Greenhouse's behavioral similarity pass caught those despite different temporary-folder and output names.

Extract a small generated-value reflection helper and a source-to-writer fixture helper. Reuse `tests/PlatoTests/CSharpTestCompilation.cs`, the compilation utility already introduced by completed repo-489; do not create another Roslyn compilation harness. Preserve fixture-specific writer options and resolution assertions.

Keep type-bound, function-bound, sum-type and negative-control assertions distinct: their overlapping machinery does not make the language features redundant. Run the affected fixtures and verify they still compile and execute generated methods, not merely inspect emitted strings.

### 3. Parameterize array-receiver checks — high confidence, very small implementation

`tests/PlatoTests/Emission/ArrayReceiverLibraryEmissionTests.cs:92-105` has three identical-shaped tests differing in expected name/signature/message. They check concrete receivers, generic receivers and declared-only stubs. Use named test cases or a shared assertion helper, retaining all three expectations and readable failure names.

Each currently calls EmitExtensions. Sharing a safely immutable emitted string could avoid repeated setup, but parameterization alone does not reduce compilation work. Measure before claiming faster tests; do not cache mutable compiler/writer objects across tests.

### 4. Share recognition of generated forwarding lambdas — useful, medium risk

TryGetEtaForwardedName is identical in `writers/Plato.CSharpWriter/TirCSharpBodyWriter.cs:126-142`, `writers/Plato.RustWriter/TirRustBodyWriter.cs:43-59` and `writers/Plato.TypeScriptWriter/TirTypeScriptBodyWriter.cs:53-69`. It recognizes compiler-generated `_eta` parameters passed unchanged and in order to one call.

A compiler-layer typed-IR query could own recognition, while each backend owns syntax emission. Check parameter identity, arity, ordering and the synthesized-name guard, including a user-written forwarding lambda that must remain a lambda. Do not introduce a universal emitter base class just to share this query.

### 5. Consolidate cross-project test input helpers — useful, medium scope

`tests/Plato.CppWriter.Tests/PlatoSource.cs:24-31` and `tests/PlatoTests/CheckerTestSupport.cs:113-122` repeat ParseFile. `tests/Plato.Interpreter.Tests/LawRunnerSelectionTests.cs:41-47` and `tests/Plato.Navigation.Tests/WarmLawRunnerTests.cs:43-49` repeat foundation-folder lookup.

A narrow test-support component could share parsing, fixture-location and temporary-file lifetime rules without making test projects reference one another. Preserve the difference between lookup from AppContext.BaseDirectory and lookup from CallerFilePath: the latter deliberately supports builds with artifacts outside the repository. Test missing input, parse failure, and external artifact locations. This deserves a separate dependency decision after the smaller within-project cleanup.

## Matches that should not drive a mechanical merge

- `src/Plato.Interpreter/Evaluation/CallMemo.cs:183-190` and `Values.cs:202-209` have identical SequenceEquals bodies but bind Equals differently. Memo-key equality compares double bits and precise type information; language-value equality uses different floating-point and type rules. Keep the policies separate. A common loop would need an explicit comparer and may offer too little benefit.
- WriteArgs repeats across all five typed-IR body writers, but calls backend-specific WriteNode methods. A small delimiter helper is possible; a shared emitter hierarchy would cost more than this repeated loop justifies.
- RustConcreteTypeWriter and TypeScriptConcreteTypeWriter repeat interface-member loops. TypeScript additionally emits field-generated array members before claiming names. Shared discovery may be worthwhile later, but merging whole pipelines could change emission order and which overload wins. Existing TypeScript overload and array-emission issues must remain visible.
- Trivial getters, throw-only placeholders and common assertion shapes are poor extraction targets. Exact syntax duplication is not evidence that a test's behavioral coverage is redundant.
- The stable seed hash shared by the interpreter and conformance fixture is small and crosses different runtime/reflection models. Keep its determinism pinned; avoid introducing a new dependency merely to save that loop.

## Greenhouse improvements

Explicit source-root/exclusion selection and a file-scope summary would prevent backup copies from dominating results. Exact-only structured output and ranking by repeated nontrivial tokens would improve triage without triggering full-tree similarity. The documentation says trivial one-liners are skipped, but the exact MethodBodyIndex indexes every nonempty body. Correct the contract or add a threshold.

These are recorded in repo-494. No Greenhouse code change was required for this investigation: scoped scans plus explicit filtering recovered useful evidence. The existing bounded MCP duplicate analyzer should be reused if implementing richer CLI controls. Do not treat either exact matches or similarity=1 as permission to auto-merge code.

## Verification and rerun

| Check actually run | Result | Seconds |
|---|---|---:|
| Greenhouse exact scan of checkout | Completed; exit 1 denotes findings | 14.23 |
| Greenhouse emission similarity, threshold 1 | Completed; exit 1 denotes findings | 0.95 |
| Manual source/caller review | Ranked candidates and rejected semantic false positive | — |
| Build, unit tests, studio integration gates | Not run: analysis and tracker documentation only | — |

From the Plato root, replace `$gh` if the tool checkout differs:

```powershell
$gh = 'C:\Users\cdigg\git\dotnet-greenhouse\src\Greenhouse.Cli\bin\Release\net8.0\Greenhouse.Cli.dll'
dotnet $gh analyze writers --format duplicates
dotnet $gh analyze tests --format duplicates
dotnet $gh analyze src --format duplicates
dotnet $gh analyze tests/PlatoTests/Emission --format similar --min-similarity 1
```

Scoped commands avoid hidden backup trees; manually exclude `src/Plato.Intrinsics/bonepile`. Separate scans do not detect cross-root pairs. To reproduce the combined audit, run `analyze . --format duplicates`, apply the membership filter above, then discard singleton groups. Raw session evidence is in `.temp/duplication-audit/exact.txt`, `filtered.json` and `emission-similar.txt`; these ignored files are working evidence, while this report preserves the findings.
