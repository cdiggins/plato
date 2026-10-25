# Formal model and proof map

The files in this section define a small mathematical language and prove properties of it with Lean. They do **not** prove that Plato's compiler, interpreter, generated C#, or standard library implements that model. This distinction is part of every theorem's scope, not a temporary disclaimer to remove after a successful checker run.

Read [the core language](core-language.md) for the objects being modeled, [typing and evaluation](typing-and-evaluation.md) for the safety and inference statements, and [rewriting](rewriting.md) for safe transformations and a counterexample. [The implementation bridge](implementation-bridge.md) lists what remains before these results could justify claims about Plato itself.

The complete proof source is [Core.lean](Core.lean). It imports Lean's standard `Init` library; it needs no Mathlib, packages, generated Plato code, external solver, or compiler plugin. [lean-toolchain](lean-toolchain) pins the checker release. A proof is accepted only after Lean checks the whole file successfully. The dated [verification checkpoint](verification-2026-10-25.md) records the actual run; this guide deliberately does not duplicate changing check results.

## What a machine-checked theorem means

A theorem has a statement and a proof term. Lean checks that the proof term has the type expressing that statement. A misspelled assumption or an oversimplified model can still make an irrelevant theorem true. Therefore the syntax, evaluation rules, scope exclusions, and connection to source behavior matter as much as the green checker result.

The proofs here introduce no new axioms and contain no unfinished `sorry` proofs. They rely on the definitions and logical foundations of the pinned Lean distribution. Standard-library tactics construct proofs that Lean checks; tactic success alone is not the trusted result.

## Checking the sources

From the repository root, with the pinned toolchain installed through elan:

```powershell
elan run leanprover/lean4:v4.19.0 lean --version
elan run leanprover/lean4:v4.19.0 lean docs/reference/formal/Core.lean
```

For the repository-local downloaded toolchain used by the checkpoint:

```powershell
& .temp/lean-proof/lean-4.19.0-windows/bin/lean.exe --version
& .temp/lean-proof/lean-4.19.0-windows/bin/lean.exe docs/reference/formal/Core.lean
```

The toolchain directory is a local working dependency, not a committed binary. The pin is also understood by [elan, Lean's toolchain manager](https://github.com/leanprover/elan). Installing Lean is separate from checking the proof.

## Related work

[Theorem Proving in Lean](https://lean-lang.org/theorem_proving_in_lean4/) explains propositions as types, induction, recursion, and the distinction between computation and axioms. Its online version can target a later Lean release; use the checked source and the pinned toolchain to reproduce these proofs.

[Software Foundations: Type Systems](https://softwarefoundations.cis.upenn.edu/plf-current/Types.html) develops safety through progress and preservation for typed arithmetic expressions. Our model uses signed mathematical integers, explicit faults, and a terminating evaluator rather than its small-step relation. [Software Foundations: Small-step Operational Semantics](https://softwarefoundations.cis.upenn.edu/plf-current/Smallstep.html) explains why a larger language needs a finer account of divergence and intermediate states. Those are open obligations here.
