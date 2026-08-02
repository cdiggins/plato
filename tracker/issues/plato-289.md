---
id: plato-289
title: Compilation ctor swallows exceptions: CompletedCompilation false with zero reported errors
type: bug
status: done
priority: p1
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-28
closed: 2026-07-28
links: [submodules/Plato/PlatoCompiler/Compilation.cs, submodules/Plato/PlatoTests/ForwardStdLibCheckerTests.cs, submodules/Plato/stdlib]
---

## Issue
`Compilation`'s constructor wraps its entire body in a catch-all. Any exception thrown after
symbol resolution leaves `CompletedCompilation` false, logs one line, and reports **zero**
resolution errors, semantic errors, or internal errors. Callers see "compilation did not
complete" with nothing to act on.

Observed 2026-07-28 while adding the forward-stdlib checker fixture: compiling
`submodules/Plato/stdlib` (84 files, 1957 functions) under Debug yields
`CompletedCompilation == false` with all three error collections empty. A `Debug.Assert` fires
and is swallowed. The log tail stops immediately after `"Grouping Reified functions by name for
faster type resolution"`, i.e. inside `AddLibraryFunctionsToReifiedTypes` / `ConcreteType`
construction, which drives the `Verifier.Assert` calls in `Analysis/FunctionInstance.cs`.

The type checker itself is unaffected — all 84 files parse and 1957 functions are checked
(40 carry diagnostics). Only the completion flag is wrong. `stdlib-legacy` completes normally,
which is why this went unnoticed: nothing had ever run a full `Compilation` over the forward
stdlib.

Repro:
```
dotnet test submodules\Plato\PlatoTests -c Debug --filter "FullyQualifiedName~ForwardStdLibParsesAndCompiles" -l "console;verbosity=detailed"
```
Currently reports `Assert.Ignore` with the captured log tail rather than a pass or a fail.

## Impact
Two distinct costs.

1. **Diagnostic black hole.** A swallowed assert is indistinguishable from a clean run that
   simply "did not complete". Any future compiler bug reaching this catch presents as silence.
   That cost is paid by every consumer of `Compilation`, not just this fixture.
2. **Blocks the forward stdlib gate.** `Plato.CLI lint` bails with "Compilation was not
   completed; cannot lint" on exactly this condition (`Plato.CLI/Program.cs`, `Lint`). Today
   `stdlib` lints only because Release builds elide `Debug.Assert`. The Debug/Release split
   means the CI story and the local story disagree — a genuine failure would be invisible in
   Release and unactionable in Debug.

Frequency: every Debug-mode `Compilation` over `stdlib`, i.e. every run of the new fixture.

## Affected code
- `submodules/Plato/PlatoCompiler/Compilation.cs:143-150` — the two catch-all handlers. `catch (AstException)` and `catch (Exception)` both log and fall through, leaving `CompletedCompilation` at its `:28` default of false.
- `submodules/Plato/PlatoCompiler/Compilation.cs:133` — `CompletedCompilation = true` sits mid-try, so anything thrown after `:100` skips it.
- `submodules/Plato/PlatoCompiler/Compilation.cs:100-102` — last successful log line; the throw is downstream of here.
- `submodules/Plato/PlatoCompiler/Analysis/FunctionInstance.cs` — the `Verifier.Assert` calls reached via `AddLibraryFunctionsToReifiedTypes` (`:97-98`); prime suspect for the firing assert.
- `submodules/Plato/PlatoCompiler/Compilation.cs:63-66` — same swallow pattern at type-definition storage, but that one at least calls `LogSymbolError`.
- `submodules/Plato/PlatoTests/ForwardStdLibCheckerTests.cs` — `ForwardStdLibParsesAndCompiles` holds the honest current state; flips green when this is fixed.

## Cause / analysis
CONFIRMED 2026-07-28 (the earlier hypothesis about interfaces with no concrete implementer /
LINT008 was WRONG — that is unrelated).

**The assert:** `PlatoCompiler/Analysis/FunctionInstance.cs:143`, in the `FunctionInstance`
constructor's `first.Type.Def.IsConcrete() || first.Type.Def.IsPrimitive()` branch:

```csharp
// NOTE: just do nothing. I don't think there are type-variables to replace.
Debug.Assert(CountTypeVars(first.Type) == 0);
```

Failing input: `Intrinsics.All(xs: Array<$T>, f: Function1<$T, Boolean>): Boolean`, reached via
`ConcreteType.CreateFunctionInstance` (`Analysis/ConcreteType.cs:61`) for the `Array` reified
type. `CountTypeVars(Array<$T>) == 1`, so the assert fires. It was a `Debug.Assert` (not
`Verifier.Assert`, which throws in every configuration), which is exactly why Release was
unaffected: under Debug it becomes a catchable `DebugAssertException` that the ctor's catch-all
then discarded.

**Why legacy never hit it.** Both libraries declare `type Array<T>` as a concrete generic. The
difference is the intrinsic signatures. `stdlib-legacy/intrinsics.plato:450+` declares the array
intrinsics against the *interface* `IArray<$T>`, which takes the `IsInterface()` branch and never
reaches line 143. The forward vocabulary dropped the `I`-prefixed interface and declares the same
functions directly on the concrete generic `Array<$T>`
(`stdlib/intrinsics.plato:397+`) — a deliberate vocabulary simplification, not a mistake.

**Verdict: compiler bug, not stdlib bug.** A library function whose first parameter is a generic
concrete type instantiated at a type variable is well-formed Plato. The branch's *comment* is
correct — there is genuinely nothing to substitute, because there is no `Self` and no
`InterfaceImplementation` to draw a replacement from — but the assert asserts something stronger
than the code needs. The free `$T` flows unchanged into `ToInstance` and
`FunctionTypeVariableAnalysis`, which is the machinery built to handle it; it is the same case the
single-parameter guard at `:162-179` explicitly permits ("`Count(xs: List<$T>)` fixes `$T` from
its argument"). Release has been eliding this assert and producing a complete, correct, lintable
compilation all along. Fix was to delete the assert and record why in the comment. No `.plato`
source was changed.

The swallow itself is older and independent: the catch-all predates the forward stdlib and
exists so a partial compilation can still be inspected. That goal is legitimate; discarding the
exception is not. `catch (Exception e)` logged `e.Message` only — no stack, no `InternalErrors`
entry. Fixed by a new `Compilation.LogInternalError(Exception, string)` that appends type +
message + stack to `InternalErrors` and logs. The two non-exceptional early returns (no trees;
halt after symbol resolution errors) also add an `InternalErrors` entry, so the invariant
"`CompletedCompilation == false` implies non-empty `InternalErrors`" is total rather than
partial.

## Priority
Recommend **high**. Severity is high (silent failure in the compiler's top-level entry point,
affecting every caller) and it currently gates the forward-stdlib lint/check story that
plato-257 vocabulary work depends on. It is cheap to fix the reporting half. Deferring compounds:
every new forward-stdlib gate built on top inherits an unreliable completion signal, and the
Debug/Release divergence means a real regression could ship unseen.

## Dependencies
- Blocks: any forward-stdlib CI gate; `Plato.CLI lint submodules/Plato/stdlib` being trustworthy in Debug.
- Touches: `PlatoCompiler/Compilation.cs` is high-traffic shared code — concurrent sessions are actively editing `PlatoCompiler` (`925c03c` landed a generic-function resolution fix the same day). Coordinate.

## Fix approaches
1. **Record, don't discard.** Push the caught exception (type, message, stack) into `InternalErrors` so `CompletedCompilation == false` always comes with at least one diagnostic. Smallest honest fix; does not diagnose the assert.
2. **Record + diagnose.** (1), plus run the fixture to identify the firing `Verifier.Assert` and either fix the invariant or convert it to a located semantic error. Closes both halves.
3. **Fail fast under test.** Add an opt-in `throwOnInternalError` so test fixtures surface the exception rather than inspecting a flag, while production callers keep partial-compilation behavior.

## Bedrock
The invariant being violated is *`CompletedCompilation == false` implies at least one reported
diagnostic* — a compiler must never fail silently. The boundary that let it through is the
constructor's catch-all at `Compilation.cs:143-150`, which converts a typed failure into an
untyped flag. Strengthening that seam means every future compiler bug reaching it arrives with
evidence attached, which is what makes options 2 and 3 cheap later.

Verdict: **simplest-along-the-grain** — take option 1 now. What it must NOT do: log to `Logger`
and return. Logging is not reporting; the error must land in `InternalErrors` where callers and
fixtures already look, or the invariant is restated rather than enforced.

## Done means
- [x] `CompletedCompilation == false` always implies a non-empty `InternalErrors` (asserted by a test) — `ForwardStdLibCheckerTests.IncompleteCompilationAlwaysReportsAnInternalError`
- [x] the firing `Debug.Assert` on `stdlib` is identified and named in this issue — `FunctionInstance.cs:143`, see Cause
- [x] `ForwardStdLibParsesAndCompiles` asserts a real pass instead of `Assert.Ignore` — now asserts `CompletedCompilation` plus empty resolution/semantic/internal errors
- [x] `Plato.CLI lint submodules/Plato/stdlib` behaves identically in Debug and Release — both exit 0, 2759 findings, 0 line-level differences over a frozen snapshot
- [x] verified by running the fixture in both configurations — Debug and Release both pass

## Simplest fix
Option 1 alone: three lines in the two catch blocks appending to `InternalErrors`.
Pro — removes the black hole immediately, near-zero risk, unblocks diagnosis of everything else.
Con — `stdlib` still does not complete; the fixture stays skipped, now with a named cause rather
than a log tail. Does not fix the underlying invariant violation.

## Prevention
- Missing test: nothing anywhere asserts the failure-implies-diagnostic invariant. That test is part of the fix, not a separate item.
- Class-level gap: `Verifier.Assert` compiled out in Release means Debug and Release disagree about what compiles. Worth its own issue — decide whether these are assertions (elidable) or validations (must not be), and pick one consistently.
- Tooling idea: a `Compilation` self-check that runs both configurations over both stdlibs in CI, so divergence fails loudly. Candidate for /track-idea.
