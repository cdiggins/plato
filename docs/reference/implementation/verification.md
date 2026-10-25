# Verification evidence and reproducible examples

A reference claim should be supported by the stage that can actually test it. A parser fixture establishes accepted syntax, not correct arithmetic. A generated program that compiles establishes host type compatibility, not equivalence to the interpreter. A passing finite law run supplies counterexample-search evidence, not a universal mathematical theorem.

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

[Example sources](examples/README.md) accompany the reference. The small core fixtures use a shared explicit primitive prelude; examples requiring the standard library say so. No example becomes “verified” merely because it is shown in a code fence. The supervisor's integrated run records actual checks.

Useful commands from the repository root:

```powershell
# Primitive prelude plus one isolated example group:
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll lint docs/reference/implementation/examples/prelude docs/reference/implementation/examples/products --strict

# Interpreter law execution for a chosen group:
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll run docs/reference/implementation/examples/prelude docs/reference/implementation/examples/products --laws

# Foundation validation and generated C# compilation:
.\tools\regen-foundation.ps1 -WhatIf
```

The first two require a built CLI. The last command uses the repository's recorded-build workflow; a nonzero code-diff report is informational when compilation succeeds. Repository-wide gate scheduling belongs to the integration supervisor, not each documentation author.

## High-value regression cases

Keep positive and negative cases for nominal mismatch, tuple construction by field order, generic case freshness, match exhaustiveness, conversion ranking, generic rigidity, bound satisfaction through inherited interfaces, sealed construction bypass paths, lazy Boolean RHS behavior, and loop local identity. Include tests where C# would accept a program through a different implicit conversion than the solver selected.

For numbers, test both profiles and include values near rounding/overflow/index boundaries. For builders, test use after Freeze and unsafe storage/capture attempts. For an optimizer, compare faults and evaluation behavior as well as successful result values. Report unresolved or degraded bodies explicitly.

## Related work

The [Rust reference](https://doc.rust-lang.org/reference/) and [C# specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions) illustrate the distinction between a language's specified semantics and a particular compiler's acceptance. This repository uses executable fixtures plus implementation evidence; the [formal track](../formal/README.md) adds narrower machine-checkable arguments where available.
