# Possible language improvements

This register lists design candidates from the reference audit. An entry is a candidate,
not part of Plato's accepted semantics and not authorization to change the compiler.
[plato-524](../../../tracker/issues/plato-524.md) tracks triage. The
[discrepancy register](../implementation/discrepancies.md) separately records conflicting claims
and implementation defects; correcting a stale document does not require redesigning the language.

The [Rust ownership chapter](https://doc.rust-lang.org/book/ch04-00-understanding-ownership.html)
is useful when comparing static guarantees with Plato's runtime-checked affine builders.
[Software Foundations](https://softwarefoundations.cis.upenn.edu/)
shows how preservation and progress obligations can be stated for a small calculus.
Neither comparison establishes a theorem about the full Plato compiler. The candidates below
must be tested against Plato's own scientific-library needs and existing decisions.

## Validate invariant-bearing values at construction

**Problem.** A record's name or implemented interface can imply a mathematical invariant that
its raw fields do not enforce. A direction can have non-unit magnitude, a probability can be
outside its intended range, and a polygon can be non-convex despite its type name.

**Possible improvement.** Use existing `sealed by` construction for checked wrappers where it
fits, and investigate explicit refinement predicates or checked constructor results where it
does not. A refinement predicate is a Boolean statement a value must satisfy to inhabit a type.

**Benefit.** Scientific code could distinguish checked data from assertions by the caller.
**Trade-off.** Validation costs time, requires a failure convention, and cannot automatically
prove every invariant. Sealing already provides part of the mechanism, so a new type-system
feature should first justify why named checked constructors are insufficient.

**Evidence.** [Vector declarations](../../../stdlib/foundation/vectors.types.plato),
[numeric wrappers](../../../stdlib/foundation/numbers.types.plato),
[polygon declarations](../../../stdlib/geometry/polygons.types.plato) and
[sealed-construction checks](../../../src/Plato.Compiler/Checking/SealedConstruction.cs).
**State:** captured for triage; no semantic change proposed for adoption yet.

## Separate readable shapes from constructible shapes

**Problem.** An interface can expose matrix element access without providing a way to construct
the result of a generic operation such as transpose. Reading every element does not imply the
ability to build a new value of the correct dimensions or type.

**Possible improvement.** Add a separate construction capability with an explicit result-shape
contract, rather than requiring all readable matrices to be writable or constructible.
**Benefit.** More generic numerical algorithms could have portable reference bodies.
**Trade-off.** Fixed and dynamic dimensions need different result types; a blanket `Self` result
would be wrong for transposing a rectangular matrix. This may be a library-interface improvement
without requiring a new language feature.

**Evidence.** [Matrix interfaces](../../../stdlib/foundation/matrices.concepts.plato) and the
transpose limitation in [numeric structure bodies](../../../stdlib/foundation/numeric-structures.library.plato).
**State:** captured for triage; shape/result design remains open.

## Make support stages explicit in tooling

**Problem.** Syntax can parse, bind, type-check, lower, emit C#, compile, and run at different
levels of completeness. A single success exit status can hide a later degraded body.

**Possible improvement.** Expose a machine-readable support classification and make callers
select the minimum required stage. Preserve diagnostics and source spans at each transition.
**Benefit.** Agents and users could ask for executable support without mistaking parser
acceptance or throwing stubs for an implemented feature.
**Trade-off.** This is a CLI and diagnostic contract, with compatibility implications for existing
scripts; it does not itself repair incomplete lowering.

**Evidence.** [CLI implementation](../../../src/Plato.CLI/Program.cs),
[type checker](../../../src/Plato.Compiler/Checking/TypeChecker.cs), and
[verification policy](../../../stdlib/verification.json).
**State:** captured for triage; compare existing commands before designing a new report.

## Report checker diagnostics from the command line

**Problem.** The command-line `lint` and `run` commands do not print the checker's `CHK`
diagnostics, so a beginner's mistake is often accepted. Observed while writing the language
chapters' "Common mistake" sections: a `match` missing a case is accepted, and a law reaching the
missing case can pass; a `_` arm fails only at run time; `+=` and `++` drop the function, and the
command can still exit successfully; a non-Boolean condition, a mismatched equality, a wrong
return type and a violated type-argument bound are not reported. Several resolution messages
repeat for one mistake, name an internal variable such as `$Ret0`, or point at an enclosing rule
with no position.

**Possible improvement.** Run the checker on the command-line path and print its findings with
source spans, and reject compound assignment and increment in the parser or the AST step with a
located message. Inferred from source: the sum, bound and existential checkers have no caller on
that path.

**Benefit.** The first program a beginner gets wrong is reported where it is wrong, and the
[diagnostics chapter](../language/diagnostics.md) can describe what users actually see.
**Trade-off.** Surfacing the checker can make currently accepted programs and stdlib files fail,
so it may need a staged rollout; it overlaps the support-stage contract above.

**Evidence.** The "Common mistake" sections of the [language chapters](../language/README.md),
and [what the command line shows](../language/diagnostics.md#what-the-command-line-shows).
**State:** captured for triage; confirm the missing callers before designing the change.

## Require observable-behavior conditions on rewrites

**Problem.** Purity does not mean an expression terminates, cannot fault, or may be evaluated any
number of times. Eager `MapRange` and short-circuit Boolean expressions make evaluation order
part of observable behavior. Floating arithmetic also invalidates familiar real-number identities.

**Possible improvement.** State each rewrite's preconditions using an effect and evaluation
contract: whether it preserves result, faults, divergence and element visitation order.
**Benefit.** An optimizer or proof tool would have explicit obligations before moving or deleting work.
**Trade-off.** A precise effect analysis is additional compiler work; conservative refusal may be
appropriate when a proof is unavailable. This is a candidate verification discipline, not a
claim that the existing optimizer is unsound.

**Evidence.** [Short-circuit lowering](../../../src/Plato.Compiler/Checking/TirShortCircuit.cs),
[eager array intrinsics](../../../src/Plato.Intrinsics/ArrayIntrinsics.cs), and
[language model](../../SEMANTICS.md).
**State:** captured for triage; the formal model states only its own narrower rewrite theorems.

## Distinguish declared capabilities from fulfilled capabilities

**Problem.** `implements` can advertise an interface whose required member has no usable body.
The linter reports `LINT001`, while generated code may still carry a throwing stub.
**Possible improvement.** Offer an executable-check mode in which missing obligations are hard
errors, while retaining an explicit declaration-only mode for aspirational vocabulary.
**Benefit.** A successful executable build would not advertise known missing operations.
**Trade-off.** Existing partial libraries need migration, and the intended boundary between
declaration catalogs and executable programs must be explicit.
**Evidence.** [Obligation checking](../../../src/Plato.Compiler/Analysis/Linter.cs) and
[forward verification policy](../../../stdlib/verification.json).
**State:** captured for triage; no change to diagnostic severity is made here.

## Make generic capability requirements explicit

**Problem.** Unbounded generic variables retain permissive capability lookup, whereas a bounded
variable can use only the operations licensed by its bounds. The two modes do not give an
implementer the same kind of contract.
**Possible improvement.** Infer required bounds or require explicit constraints for generic
operations, with an explained compatibility mode for existing code.
**Benefit.** Generic signatures could state all operations needed by their bodies and support
stronger inference and safety arguments.
**Trade-off.** Inferred bounds can change public signatures; explicit bounds increase annotation
burden and need a migration strategy.
**Evidence.** `BoundsPermit` in the [solver](../../../src/Plato.Compiler/Checking/Solver.cs).
**State:** captured for triage; permissive behavior remains part of implementation evidence.

## Define single evaluation of match subjects

**Problem.** The elaborator reuses the subject expression when lowering `match` to tag tests and
payload projections. An expensive subject can be evaluated repeatedly; a faulting or divergent
subject makes evaluation discipline semantically significant.
**Possible improvement.** Specify the subject's evaluation count, then retain a typed match node
or bind the subject once during lowering if single evaluation is intended.
**Benefit.** Callers and implementers could reason locally about the cost and behavior of matching.
**Trade-off.** This requires a precise source evaluation rule, an intermediate representation
change or fresh local, and compatibility tests. The audit observation is not itself a new rule.
**Evidence.** `ElaborateMatch` in the [elaborator](../../../src/Plato.Compiler/Checking/Elaborator.cs).
**State:** captured for triage; settle and reproduce observable behavior before changing lowering.
