---
id: compiler-519
title: Resolved library methods collide with same-named record fields in CSharp
type: bug
status: in-progress
priority: p1
effort: M
risk: med
area: compiler
sprint:
created: 2026-10-25
closed:
links: [plato-brep TKT-398]
---

## Done means

- [ ] Capture the resolved TIR and generated CSharp for both same-name collision regressions.
- [ ] Resolved named library methods retain precedence over same-named callable and data fields; ordinary field projection and unresolved function-field invocation remain correct.
- [ ] Both collision regressions pass on checker, interpreter, CSharp and differential using committed compiler binaries, with no weakened assertions or mismatches.

## Reproduced behavior

Plato-brep commit 5ab32374 preserves seven-test baseline02 against actual fc4b5
production assemblies. DataCarrier has integer field Value and a two-argument
library Value(c,n). Interpreter projection/method law passes fourteen exhaustive
cases; CSharp emits a call rejected with CS1955 (DataCarrier.Value not invocable).
That one compilation failure hides the five other laws in its combined fixture;
they are not separately measured compiled failures.

CollisionCarrier has function field WorkAt(x)=x+3 and named library WorkAt(c,n)=n+100.
Checker ResolvedCalls selects the named library method. Interpreter passes all
fourteen cases; CSharp returns false at n=-1 and differential reports a mismatch.
Solver.TryResolveFieldInvocation explicitly gives named member/library overloads
precedence, so changing the expectation to field invocation would hide the bug.

Test receipt 06c21045f527e0c668c83af4a369bd37f6b41414be5f6cf9d208f960e55235ef.
Inputs/packages/runtime identities remained unchanged; no timeout or skipped
tests. The older baseline01 aborted before execution and is unrelated evidence.

Source review suggests one shared cause: SkipFunction suppresses same-field
names before extension planning, while resolved calls retain receiver.Name(args).
This is a hypothesis pending an additive TIR/generated-text diagnostic; the
original run did not preserve those bytes. Simply removing suppression could
create illegal field/method duplicates, and extension syntax can still be
shadowed by a delegate field. Preserve checker-selected dispatch explicitly.

The interpreter stored-function fallback and local callable inference repairs
remain separately owned by TKT-398. Compiler-518 addresses generic constructors
and must not absorb this writer change. Existing plato-323/plato-327 and completed
compiler-499 cover different property/inline concerns, not this reproduction.
