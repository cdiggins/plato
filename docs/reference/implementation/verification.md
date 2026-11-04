# Verification evidence and reproducible examples

A reference claim should be supported by the stage that can actually test it. A parser fixture establishes accepted syntax. It does not establish correct arithmetic. A generated program that compiles establishes host type compatibility. It does not establish equivalence to the interpreter. A passing finite law run supplies counterexample-search evidence. It is not a universal mathematical theorem.

The [Rust reference](https://doc.rust-lang.org/reference/) and the [C# specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions) separate a language's specified semantics from what one compiler accepts. This repository uses executable fixtures and implementation evidence. The [formal track](../formal/README.md) adds narrower machine-checkable arguments where they exist.

## Stage-specific evidence

| Stage | Question it answers | What remains unchecked |
|---|---|---|
| Parse | Does this text produce the intended syntax tree? | Name/type validity and execution |
| Resolve | Do names refer to the intended declarations? | Full body compatibility |
| Lint | Do authoring obligations and restrictions hold? | Every semantic/runtime property |
| Check | Do inferred types, overloads, bounds, sums, and sealed paths satisfy checker rules? | Soundness of permissive fallbacks and runtime bodies |
| Elaborate/verify TIR | Are selected calls/conversions explicit and IR invariants met? | Host lowering equivalence |
| Interpret | Does the typed program execute on these inputs? | All inputs and C# agreement |
| Generate and build C# | Is the rendered library accepted by the supported host compiler/runtime? | Functional correctness |
| Differential laws | Do interpreter and compiled runs agree on tested cases? | Universal equality, shared bugs |
| Formal proof | Does a precisely stated theorem follow in its stated calculus? | Features and implementation paths outside the model |

The normative library verification ladder lives in [VERIFICATION](../../../stdlib/VERIFICATION.md). This page does not duplicate gate counts or ratchet ceilings.

## Reproducible source examples

[Example sources](examples/README.md) accompany the reference. The small core fixtures use a shared explicit primitive prelude. Examples that require the standard library say so. An example is not verified because it appears in a code fence. The [2026-10-25 integration record](verification-2026-10-25.md) lists the checks that were actually run.

Useful commands from the repository root:

```powershell
# Primitive prelude plus one isolated example group:
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll lint docs/reference/implementation/examples/prelude docs/reference/implementation/examples/products --strict

# Interpreter law execution for a chosen group:
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll run docs/reference/implementation/examples/prelude docs/reference/implementation/examples/products --laws

# Foundation validation and generated C# compilation:
.\tools\regen-foundation.ps1 -WhatIf
```

The first two require a built CLI. The last command uses the repository's recorded-build workflow. A nonzero code-diff report is informational when compilation succeeds. Repository-wide gate scheduling belongs to the integration supervisor, not to each documentation author.

## Build-output layout and fixture discovery

The reference examples take explicit source roots, so the helper script can use an isolated CLI assembly through `--cli`. Some repository tests instead locate fixtures with [CheckerTestSupport.FindFolder](../../../tests/PlatoTests/CheckerTestSupport.cs). Its output-first search can mistake an isolated `bin/PlatoTests` directory for the source test folder, before trying the source-file fallback. Missing fixtures in that layout are not failed numerical laws. Use the normal layout for those gates until [repo-531](../../../tracker/issues/repo-531.md) fixes the lookup, and preserve the failing report if experimenting with output locations.

The studio battery also checks frozen copies of the older runtime in the SDK checkout. That checksum contract is separate from Plato's writable forward sources and generated caches. A failure there requires reviewing the named SDK artifacts. It does not authorize a silent checksum rebaseline, or an edit to an unrelated checkout to make this reference's gates green.

## Regression coverage

Keep positive and negative cases for nominal mismatch, tuple construction by field order, generic case freshness, match exhaustiveness, conversion ranking, generic rigidity, bound satisfaction through inherited interfaces, sealed construction bypass paths, lazy Boolean RHS behavior, and loop local identity. Include tests where C# would accept a program through a different implicit conversion than the solver selected.

For numbers, test both profiles and include values near rounding, overflow, and index boundaries. For builders, test use after Freeze and unsafe storage or capture attempts. For an optimizer, compare faults and evaluation behavior as well as successful result values. Report unresolved or degraded bodies explicitly.
