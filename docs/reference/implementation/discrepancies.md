# Discrepancies and unfinished specification work

This register keeps contradictions visible while providing a usable reference. It does not silently rewrite normative rules or claim that every observed implementation behavior is a desirable language feature. “Observed” below means inspected in named source or tests, not a claim that an integrated gate was run by this page's author.

## Classification for follow-up

| Item | Classification | Existing authority to improve |
|---|---|---|
| Generic sums rejected by default | Stale instruction | AGENTS language facts and stdlib convention wording must distinguish compiler capability from vocabulary policy |
| No runtime interface values | Documentation/source contradiction and open semantic boundary | SEMANTICS model/interfaces/non-features need constraint-versus-stored-value distinction |
| Foreach included with working loop desugarings | Suspected compiler gap plus overbroad documentation | SEMANTICS function-body loop paragraph needs end-to-end support qualifications |
| TIR described as unused shadow output | Stale implementation comments and pipeline prose | compiler-pipeline and checker/IR class summaries should describe actual consumers |
| Every C# no-argument member is a method | Overbroad host-mapping documentation | AGENTS, SEMANTICS and plato-for-agents need fields/runtime-property exceptions |
| Overload summary omits tie details | Specification completeness improvement | SEMANTICS overload paragraph should state specificity, weighted totals, polymorphic deferral and ordering |
| Permissive generic/null/depth cases | Open type-system design and proof boundary | Type-checker handoff and proof claims must state preconditions and fallbacks |
| Match subject can repeat after lowering | Open evaluation contract and possible compiler defect | SEMANTICS match section needs an explicit evaluation-multiplicity rule |
| Unique ownership is not statically enforced | Proof/expectation clarification | Any purity/type-safety prose must name the builder discipline's assumed enforcement boundary |
| Lexical/control-flow boundaries | Specification completeness improvement | SEMANTICS and syntax docs need scanner, literal, block-return and definite-assignment rules |

## Generic sums: stale non-generic restriction

**Conflict.** The repository agent guide says generic sums are rejected with CHK306, while [SEMANTICS](../../SEMANTICS.md) allows them and [SumTypeChecker](../../../src/Plato.Compiler/Checking/SumTypeChecker.cs) disables that restriction by default. [GenericSumTests](../../../tests/Plato.Interpreter.Tests/GenericSumTests.cs) exercise generic cases.

**Reader rule.** Generic sums are implemented by default; a host can opt into a monomorphic restriction. A library convention discouraging a particular optional-result type is a separate matter. Update stale instructions rather than implementing the historical default.

## Interfaces: constraints versus existential field storage

**Conflict.** Normative prose broadly denies runtime interface values/heterogeneous interface collections. [ExistentialConceptChecker](../../../src/Plato.Compiler/Checking/ExistentialConceptChecker.cs) explicitly recognizes interface-typed fields, including nested field type arguments, as existential storage when not grounded by the owning type; it rejects only view-less surfaces with CHK308. [ConceptGrounding](../../../src/Plato.Compiler/Symbols/ConceptGrounding.cs) shares that distinction with C# rendering.

**Consequence.** A clean-room implementation following only the blanket prose would reject some intended source declarations. Conversely, assuming general OO interfaces would accept too much. The reference distinguishes signature constraints from the implemented stored-view mechanism. Full runtime value semantics, boxing consequences, interpreter parity, and the exact usable surface require normative reconciliation.

## Foreach: incomplete AST translation

**Conflict.** [SEMANTICS](../../SEMANTICS.md) includes foreach among loop desugarings. [AstNodeFactory.ToAst(CstForEachStatement)](../../../src/Plato.AST/AstNodeFactory.cs) creates an uninitialized variable and a `MoveNext` call with a null argument; it does not lower the collection expression or bind each current element.

**Consequence.** Parser recognition and a loop node do not demonstrate usable foreach execution. Prefer while/combinators until a collection-iteration contract and end-to-end fixture establish the behavior. This is an implementation gap, not a recommendation to copy the placeholder into a new compiler.

## TIR architecture: stale shadow-mode comments

**Conflict.** [TypeChecker](../../../src/Plato.Compiler/Checking/TypeChecker.cs), [Tir](../../../src/Plato.Compiler/Checking/Tir.cs), [Elaborator](../../../src/Plato.Compiler/Checking/Elaborator.cs), and [Monomorphizer](../../../src/Plato.Compiler/Checking/Monomorphizer.cs) retain comments saying their output feeds no writer. [CSharpWriter](../../../writers/Plato.CSharpWriter/CSharpWriter.cs) actually uses TIR emission sources, and [Interpreter](../../../src/Plato.Interpreter/Evaluation/Interpreter.cs) executes elaborated TIR.

**Reader rule.** Follow current consumers. There is one C# body path from typed IR; the historical alternate body writer is not an architecture to reproduce.

## C# member spelling: “property-free” omits exceptions

**Stale documentation.** The agent guide and [plato-for-agents](../../plato-for-agents.md) say every no-argument member is a method. [CSharpConcreteTypeWriter](../../../writers/Plato.CSharpWriter/CSharpConcreteTypeWriter.cs) emits stored product fields as public readonly fields, and [CSharpFunctionInfo](../../../writers/Plato.CSharpWriter/CSharpFunctionInfo.cs) exempts the struct surface from ordinary method emission. [Number](../../../src/Plato.Intrinsics/Number.cs) retains intrinsic properties, with [CSharpWriter.PrimitiveSurfaceOverrides](../../../writers/Plato.CSharpWriter/CSharpWriter.cs) recording host exceptions.

**Consequence.** A consumer following the blanket claim would incorrectly write `position.X()` for a stored field or `number.Sqrt()` for a retained runtime property. The correction is to distinguish ordinary computed methods, stored fields, and runtime surface exceptions; the retired flags do not restore a second general rendering mode.

## Overloads: summary omits important tie behavior

**Gap.** The normative exact/generic/interface/conversion summary does not fully state [Solver](../../../src/Plato.Compiler/Checking/Solver.cs)'s summed numeric costs, derived-parameter specificity tie-break, all-variable-return acceptance, or CHK204 polymorphic deferral. A common-return tie selects the first candidate and reports CHK202.

**Consequence.** Equal result types do not by themselves prove equal behavior of candidate bodies. Candidate ordering and redispatch coherence need explicit specification and fixtures. An implementation claiming order-independent overload semantics must prove more than common result-type agreement.

## Generic capability use and resource guards

**Gap.** Known bounds restrict operations, but unbounded generic parameters retain permissive compatibility. [Solver.Unification](../../../src/Plato.Compiler/Checking/Solver.Unification.cs) also treats null/depth-limit situations permissively, and `Self` is compatible before grounding.

**Consequence.** A successful checker run is not a formal proof of parametricity or type safety for every permissive path. Formal statements must restrict their calculus or model these fallbacks explicitly. A stricter language mode is a possible design improvement, not an existing guarantee.

## Match evaluation and canonical sequencing

**Gap.** [Elaborator.ElaborateMatch](../../../src/Plato.Compiler/Checking/Elaborator.cs) reuses the elaborated subject subtree in tag tests and payload projections. It does not introduce a single-evaluation binding. Thus an expensive subject expression can run repeatedly in lowered execution.

**Consequence.** Mathematical “inspect one sum value” intuition is insufficient to infer a once-only evaluation guarantee. Bind a complex subject to a local. A normative evaluation rule and lowering fixture should settle the intended contract, including faults and builders, before stating a whole-language semantics-preservation theorem.

## Host-derived setters are not automatically source functions

**Conflict.** SEMANTICS describes immutable per-field setters as part of the derived value surface.
The C# writer emits `With...` methods, but a minimal source program calling an undeclared
`WithLeft` fails symbol resolution. `SymbolFactory` derives accessors, constructors and conversion
relations, not a source library declaration for every C# convenience.

**Consequence.** A source example must explicitly define its update function or construct the
replacement record. A clean-room implementation needs an explicit decision about whether these
setters are intended source features or host-only conveniences. The runnable product fixture
defines its `WithLeft` body visibly rather than assuming automatic binding.

## Bare generic receiver classification fails before checking

**Observed.** A library function `F(x: $T): Integer where $T: IReferenceValue` throws from
`Compilation.AddLibraryFunctionsToReifiedTypes` because its first parameter is neither a concrete
type nor an interface. A bound does not alter that receiver classification.

**Consequence.** Use a constructed receiver or a concrete/interface receiver with a separate
generic parameter in tested examples. A clear rejection or supported lowering is needed; a raw
exception must not be mistaken for a successful generic typing rule.

## Ownership enforcement boundary

**Gap.** [Primitive declarations](../../../stdlib/foundation/primitives.plato) describe affine builder ownership, forbid capture/storage, and require rebind/consume patterns. The present implementation combines lint checks and runtime invalidation; it does not statically prove unique ownership for all programs.

**Consequence.** Builder-based purity claims assume adherence to this discipline and correctness of the runtime. A formal type-safety/uniqueness proof must exclude these builders or provide a real affine typing judgment and show implementation correspondence.

## Lexical and control-flow completeness

**Gap.** Parser rules include forms outside checked semantics; lexical detail depends partly on shared Parakeet rules. Literal suffix/escape/overflow handling, arbitrary initializer forms, recursive type admissibility, exhaustive block-return analysis, and definite local assignment are not independently specified here to standard-quality completeness.

**Consequence.** The [reading grammar](grammar.md) and examples give an implementable core, but this reference does not claim a finished clean-room standard for every accepted parser input. Each gap should be settled with an explicit decision and minimal positive/negative fixtures rather than inferred from C# acceptance.

## Mathematical theorems versus executable laws

A `Law_*` function exercised over witnesses and generated values is an executable test property. It is not a universal proof. A formal theorem about a restricted calculus is likewise not a proof about the entire compiler/runtime. The [formal track](../formal/README.md) must identify its trusted assumptions and feature boundary. No full type-safety, inference-completeness, optimizer-correctness, or cross-backend-equivalence theorem is asserted by this reference.
