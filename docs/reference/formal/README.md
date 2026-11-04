# Formal model and proof map

The files in this section define a small mathematical language and prove properties of it with Lean. They do not prove that Plato's compiler, interpreter, generated C#, or standard library implements that model. The limit is part of every theorem. A successful checker run is not a reason to remove it.

[The core language](core-language.md) defines the objects being modeled. [Typing and evaluation](typing-and-evaluation.md) states the safety and inference results. [Rewriting](rewriting.md) states the outcome-preserving transformations and one counterexample. [The implementation bridge](implementation-bridge.md) lists what remains before these results could justify claims about Plato itself.

The proofs are in [Core.lean](Core.lean). It imports Lean's standard `Init` library. It needs no Mathlib, packages, generated Plato code, external solver, or compiler plugin. [lean-toolchain](lean-toolchain) pins the checker release. A proof is accepted only when Lean checks the whole file. The dated [verification checkpoint](verification-2026-10-25.md) records that run. This page does not copy check results, which change from run to run.

## What a machine-checked theorem means

A theorem has a statement and a proof term. Lean checks that the proof term has the type of the statement. A misspelled assumption or an oversimplified model can still make an irrelevant theorem true. The syntax, the evaluation rules, the scope exclusions, and the connection to source behavior matter as much as the green checker result.

The proofs introduce no new axioms and contain no `sorry`. They rely on the definitions and logical foundations of the pinned Lean distribution. Tactics from that distribution construct proofs that Lean checks. Tactic success alone is not the trusted result.

This model uses signed mathematical integers, explicit faults, and a terminating evaluator. These proofs are not the progress and preservation theorems for the small-step relation in [Software Foundations](https://softwarefoundations.cis.upenn.edu/plf-current/Types.html). Divergence and intermediate states remain open obligations. [The small-step chapter](https://softwarefoundations.cis.upenn.edu/plf-current/Smallstep.html) explains why a larger language needs a finer account of divergence and intermediate states.

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

The toolchain directory is a local working dependency, not a committed binary. The pin is also understood by [elan, Lean's toolchain manager](https://github.com/leanprover/elan). Installing Lean is separate from checking the proof. [Theorem Proving in Lean](https://lean-lang.org/theorem_proving_in_lean4/) explains propositions as types, induction, recursion, and the distinction between computation and axioms. Its online version can target a later Lean release, so reproduce these proofs from the checked source and the pinned toolchain.
