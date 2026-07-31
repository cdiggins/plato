---
id: plato-332
title: "Checker gap: qualified sum-type payload constructors (Sum.Case(x)) fail CHK201"
type: bug
status: done
priority: p2
effort: "?"
risk: "?"
area: plato
sprint:
created: 2026-07-30
closed: 2026-07-30
links: [plato-232, plato-302, submodules/Plato/stdlib/brep.plato]
---

## Symptoms / impact

Calling a sum-type case constructor in qualified form — `BrepCurve.Line(seg)` — fails the type checker with CHK201, while the bare synthesized factory form `Line(seg)` type-checks fine. Codegen supports the qualified form; only the checker rejects it. Found by the plato-302 BREP agent (2026-07-30) while building `BrepCurve = Line(LineSegment3D)` values: the checker resolves the receiver first (treats `BrepCurve` as a value receiver for a `.Line` member lookup) instead of recognizing the sum-type-qualified constructor path.

Impact: stdlib and user code must use bare factories, which collide-prone (a bare `Line(...)` competes with any other `Line` in scope) and read worse than the qualified form. Inconsistency between checker and codegen means valid-by-codegen programs are rejected.

## Affected code

- Sum types shipped in [plato-232](plato-232.md) (2026-07-27); `match` lowers to conditionals, CHK306 bars generic sums.
- Checker: receiver-first resolution path that raises CHK201 (see TypeChecker call resolution; exact site to be located when picking this up).
- First trip site: `stdlib/brep.plato` / `brep-primitives.library.plato` ([plato-302](plato-302.md)) — worked around with bare factories.

## Root-cause notes

Checker resolves `X.F(a)` by resolving `X` as an expression/receiver, then looking up member `F` — it never tries "X is a sum type name, F is one of its cases." Codegen has that path; checker doesn't. Fix belongs in call resolution: when the receiver resolves to a *type name* that is a sum type and the member matches a case name, treat as case-constructor application.

## Fix approaches

1. **Checker-side special case** (preferred): in call resolution, before raising CHK201, test receiver-is-sum-type-name + member-is-case; type as the sum. Small, local.
2. Desugar `Sum.Case(x)` → bare synthesized factory early (parser/AST normalization) so checker never sees the qualified form. Risk: loses qualification for diagnostics/tooling.
3. Do nothing + lint rule steering to bare factories. Rejected: keeps checker/codegen inconsistent and keeps bare-name collision risk.

## Simplest fix

Approach 1 plus a regression test: a sum type with a case name that collides with an unrelated library function, constructed both bare and qualified; both must check and emit identically.

## Resolution (2026-07-30)

Fixed via approach 1 in the Plato submodule (commit noted below):

- `PlatoCompiler/Checking/SumCaseAccess.cs` — new `ResolvePayload`: recognizes `Sum.Case(args)` (receiver NAMES the sum type, member names a payload case, trailing-arg count == field count).
- `PlatoCompiler/Checking/ConstraintGenerator.cs` (`SynthesizeCall`, FunctionGroupRefSymbol branch) — tried before the ordinary `OverloadConstraint`: resolves the TRAILING args against the sum's own synthesized `SumFactory` candidates only, so the qualified form means exactly the named case even under bare-name collision.
- `PlatoCompiler/Checking/Elaborator.cs` (`ElaborateCall`) — payload branch mirroring the nullary one: emits a StaticMethod `TirCall` rendering `BrepCurve.Line(seg)`.
- Regression: `plato-test-sum/qualified-payload-case.plato` (qualified + bare + colliding `Lines.Line`) wired into `PlatoTests/SumTypeCheckingTests.cs` (positive fixture + `QualifiedPayloadCaseConstructor_TypeChecksClean`).

Plato commit: 3642aa2 (fix + test in one commit).
