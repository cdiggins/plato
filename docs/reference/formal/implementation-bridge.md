# Bridging the proofs to Plato

The checked [core model](core-language.md) provides useful examples of what a theorem can state. It is not a formal specification of the full language. The essential missing artifact is a **refinement proof**: a precise mapping from a source construct and its implementation to a modeled term, together with a proof that the mapping preserves the intended observations.

## Correspondence and deliberate differences

| Core object | Plato correspondence | Boundary |
|---|---|---|
| Integer and Boolean expression literals | Literal forms in the source language | Core integers have no machine-width bound. |
| `add` on integer values | A resolved integer addition operation | No overloads, wrappers, promotions, overflow, or host exceptions are modeled. |
| `cond` | TIR conditional shape and source `? :` | No conversion insertion or statement lowering is modeled. |
| `andThen` | Shape of Boolean conjunction lowering | No proof connects actual name/type-based recognition to the abbreviation. |
| `fault(T)` | Abstract stand-in for a computation that can fail | Not source syntax; all fault details collapse into one outcome. |
| `HasType` | A syntax-directed typing judgment | No claim that it is Plato's checker or a subset closed under all real conversions. |
| `fold` | An example recursive optimizer | It is not a model of a specific production optimization pass. |

Read [TirShortCircuit](../../../src/Plato.Compiler/Checking/TirShortCircuit.cs), [Interpreter](../../../src/Plato.Interpreter/Evaluation/Interpreter.cs), and [TirCSharpBodyWriter](../../../writers/Plato.CSharpWriter/TirCSharpBodyWriter.cs) as implementation evidence. Reading their code is not a formal correspondence proof. Non-C# writers are excluded as semantic evidence because they are proof-of-concept backends.

## Open proof obligations

These are possible extensions to the formal development, not claims that the language is broken:

- **Names and binding:** define environments, variable identities, substitution, and scope. Prove that elaboration preserves binding and that substituting a value preserves typing.
- **Functions and nontermination:** define calls and recursion with a semantics that distinguishes divergence, runtime faults, and stuck terms. Then prove genuine one-step preservation and progress, allowing explicit fault states.
- **Numeric profiles:** model the selected finite integer and floating representations, conversion checks, NaNs, infinities, signed zero, and rounding. Prove only rewrites valid under those rules. Mathematical integer or real algebra cannot silently replace machine arithmetic.
- **Nominal data and sum types:** define construction, field projection, tags, payload validity, and match lowering. Account for whether the subject is evaluated once or repeatedly.
- **Interfaces, conversions, and inference:** model member obligations, conversion choice, overload priority, generic constraints, specialization, and diagnostics. State the intended completeness target before attempting an inference completeness theorem.
- **Arrays and builders:** model eager element evaluation, bounds faults, traversal order, ownership, and resource obligations before proving fusion or loop lowering correct.
- **Source to TIR to C#:** specify each representation and translation. Prove that accepted source terms have well-typed TIR, then that the emitted program preserves observations under a stated runtime contract. Include failure cases and degraded generated bodies.

A proof of the bounded core is a foundation for this work; it cannot replace it. Likewise, unit tests and differential interpreter/C# tests can expose disagreement, but agreement on tested inputs is not a universal semantic theorem.

## Specification improvements worth considering

The following are proposed improvements to the specification and verification process, not proposed changes to accepted programs:

- Publish a precise observable-outcome contract: which fault distinctions, evaluation counts, evaluation order, divergence, and host obligations must transformations preserve?
- Give each compiler pass a local input invariant and output relation, so proofs and tests can target a documented boundary.
- Keep source typing distinct from optimized-tree typing; otherwise constant folding may accidentally legitimize ill-typed dead branches.
- Give numeric rewriting rules explicit profile assumptions rather than importing mathematical identities without rounding conditions.
- Add a reproducible Lean check to reference validation if maintaining the formal track becomes an ongoing commitment.

The supervisor records actionable issues and proposals in the repository tracker. This page explains the proof gaps; it does not establish that every gap is a compiler defect.

## Related work

[CompCert's semantic preservation account](https://compcert.org/motivations.html) describes connecting compiler transformations to observable source and target behavior. The scale and obligations of that connection are substantially larger than proving properties of a teaching calculus. [Software Foundations' small-step presentation](https://softwarefoundations.cis.upenn.edu/plf-current/Smallstep.html) provides a natural next step for adding divergence and intermediate execution states.
