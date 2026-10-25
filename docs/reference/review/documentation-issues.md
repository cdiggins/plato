# Documentation defects and improvement register

This register records confusing or conflicting documentation discovered during the reference
audit. [docs-525](../../../tracker/issues/docs-525.md) tracks correction of the original sources.
These are source-inspection findings, not newly executed algorithm reproductions. The linked
original document is the place to correct the claim; this register makes the required work visible.
The new reference explains the boundary rather than silently resolving disputed semantics.

## Language and compiler documentation

| Affected source | Problem and consequence | Proposed correction or missing decision |
| --- | --- | --- |
| [Agent instructions](../../../AGENTS.md), sum restriction | They forbid generic sums and describe CHK306 as unconditional. An agent may reject valid programs or recreate a retired restriction. | Reconcile with the generic-sum decision, default checker setting and current SEMANTICS; distinguish a missing shipped Maybe declaration from a missing language capability. |
| [SEMANTICS](../../SEMANTICS.md), runtime interfaces | A blanket prohibition on runtime interface values conflicts with existential interface-typed field storage recognized by the checker and C# writer. | State the exact constraint-versus-stored-view distinction and define supported operations, boxing, container behavior and interpreter parity. |
| [SEMANTICS](../../SEMANTICS.md), loop desugaring | It groups foreach with supported loops, but AST translation does not consume the collection or bind Current. | Give foreach an explicit incomplete status until a proper contract and executable fixtures exist. Do not treat parser acceptance as execution. |
| [Compiler pipeline](../../compiler-pipeline.md) | It describes historical shadow checking, optional TIR and legacy body-writer paths. An implementer could reproduce a retired architecture. | Describe the sole C# typed-intermediate-representation body path and interpreter consumers; move historical rollout details to archive. |
| [Checker/TIR comments](../../../src/Plato.Compiler/Checking/Tir.cs) and nearby pass headers | Comments claim no writer consumes typed IR, despite production consumers. | Replace obsolete rollout commentary with the invariant each pass establishes and links to actual consumers. |
| [SEMANTICS](../../SEMANTICS.md), overload summary | It omits weighted aggregate costs, derived-parameter tie-breaking, variable-return ties and polymorphic deferral. Different implementations may choose different functions. | Specify candidate ordering, costs and tie behavior; decide whether equal result types are a sufficient coherence condition. |
| [SEMANTICS](../../SEMANTICS.md), matching | A mathematical case-analysis description does not specify the number of evaluations of a complex subject. Lowering can duplicate its subtree. | Decide the evaluation contract and add cost/fault-sensitive fixtures; document the chosen lowering explicitly. |
| [SEMANTICS](../../SEMANTICS.md), lexical contract | Shared parser rules, suffix/escape processing, literal overflow and unsupported initializer forms are not a standalone scanner specification. | Add lexical positive/negative fixtures and explicit semantics for each supported literal spelling; mark recovery-only syntax separately. |
| [SEMANTICS](../../SEMANTICS.md), safety language | Capability satisfaction, generic fallbacks and affine ownership are described more strongly than the present static checks establish in every path. | Separate intended discipline, lint diagnostics, runtime checks and proved properties. State permissive compatibility paths explicitly. |
| [Agent instructions](../../../AGENTS.md), [agent overview](../../plato-for-agents.md), C# member shape | Blanket property-free/method wording can be read as applying to stored fields and runtime property exceptions too. | Describe generated computed no-argument methods, actual readonly stored fields and pinned handwritten runtime properties separately, with C# examples. |
| [SEMANTICS](../../SEMANTICS.md), derived immutable setters | The derived value-surface description does not distinguish C# `With...` methods from source functions. A minimal source call to undeclared `WithLeft` fails resolution. | Specify whether these are intended source operations or host conveniences. Source examples must define their update function or reconstruct the record until source binding is supplied. |
| [SEMANTICS](../../SEMANTICS.md), generic receiver boundary | The generics description does not state that a bare signature variable in receiver position currently fails library classification before type checking, even with bounds. | Document the implemented constructed/interface receiver path and replace the raw exception with a supported path or located rejection; do not imply arbitrary first-parameter polymorphism is already usable. |
| [Test-fixture helper comment](../../../tests/PlatoTests/CheckerTestSupport.cs), isolated outputs | Its source fallback is described as supporting private artifact folders, but output-first lookup accepts an assembly directory named PlatoTests before reaching that fallback. | Correct the lookup under [repo-531](../../../tracker/issues/repo-531.md), and verify normal and private layouts rather than treating missing fixtures as algorithm failures. |

The [implementation discrepancy register](../implementation/discrepancies.md) links the compiler
and tests behind the language findings. Correcting prose must not silently decide an open language
question; decisions need the appropriate tracker item and, when material, an architecture record.

## Library documentation and source comments

| Affected source | Problem and consequence | Proposed correction or missing decision |
| --- | --- | --- |
| [Verification guide](../../../stdlib/VERIFICATION.md), execution ladder | Early prose implies execution requires C# conversion, overlooking the interpreter backend. | Explain law execution independently of backend and link each actual command. |
| [Verification guide](../../../stdlib/VERIFICATION.md), conformance status | Earlier failure claims conflict with later text saying the claims are stale. Readers cannot know which stage is usable. | Replace duplicate live-status paragraphs with the policy, current command and recorded gate output. |
| [Verification guide](../../../stdlib/VERIFICATION.md), scope | It references deleted legacy test folders. | Remove retired paths and name the executable suites that actually own coverage. |
| [Verification guide](../../../stdlib/VERIFICATION.md), ratchet authority | A closing authority table and manifest-policy prose name different homes for ceilings. | Point to the actual policy reader/manifest and describe tests as enforcement rather than maintaining a second value. |
| [Standard-library overview](../../../stdlib/README.md), backend/performance claims | Backend priority and near-zero-overhead prose can imply demonstrated support or costs beyond the evidence. | Distinguish design goals, primary C# behavior, POC targets and measured costs. |
| [Library guide](../../../stdlib/LIBRARIES.md), host signatures and commands | Some builder-signature locations and studio-relative commands describe an older layout. | Link current intrinsic declarations and repo-local commands, checking each path. |
| [Vector declarations](../../../stdlib/foundation/vectors.types.plato), intrinsic ownership | Header/comments refer to composite operations as intrinsic-hub obligations despite the primitive-only kernel. | Link reference bodies and current override ownership; audit VectorN wording too. |
| [Angle bodies](../../../stdlib/foundation/angles.library.plato), Angle construction | A header calls Angle construction intrinsic despite the primitive-only contract. | Describe the actual declaration/body and conversion mechanism. |
| [Quantity bodies](../../../stdlib/foundation/quantities.library.plato) | Intrinsic-operator wording and old missing-stub tallies imply obsolete ownership and status. | State semantic operations and their owners; move measurements to generated logs. |
| [Random/distribution bodies](../../../stdlib/foundation/random.library.plato), Von Mises CDF | An opening comment says the CDF is omitted, while a later approximate body exists. | Document the truncated-series approximation, parameter domain and error limitations. |
| [Surface bodies](../../../stdlib/geometry/surfaces.library.plato), offset normals | A claim of exactness for differentiable surfaces overstates central differences, which retain truncation and rounding error. | State finite-difference step/error assumptions and approximation limits. |
| [Surface declarations](../../../stdlib/geometry/surfaces.types.plato) and [bodies](../../../stdlib/geometry/surfaces.library.plato), moving frames | Intended rotation-minimizing frames conflict with fixed-up implementations. | State intended and implemented frames separately; choose and test a compatible algorithm before promising transport behavior. |
| [Point-cloud bodies](../../../stdlib/geometry/pointclouds.library.plato), normals | Unchanged normals are described as exact for rigid mappings, but rotations require normal rotation. | Correct the claim and repair normal transport under [stdlib-528](../../../tracker/issues/stdlib-528.md). |
| [Kinematics bodies](../../../stdlib/future/kinematics.library.plato), average speed | Endpoint chord length divided by time is called average speed, even for a closed moving path. | Name it a chord rate or compute arc-length average speed; distinguish speed from displacement rate. |
| [Joint declarations](../../../stdlib/future/joints.types.plato) and [bodies](../../../stdlib/future/joints.library.plato) | The type allows a negative static-world attachment, while bodies require valid body indices and an explicit zero-mass world body. | Choose one world-attachment representation and apply it consistently. |
| [Geospatial declarations](../../../stdlib/future/geo-spatial.types.plato) and [bodies](../../../stdlib/future/geo-spatial.library.plato) | Geodesic polygon-edge language conflicts with planar longitude/latitude tests; antimeridian cases are not handled. | State the planar approximation, domain limits and wrap assumptions, or implement the promised geodesic model. |

[stdlib-526](../../../tracker/issues/stdlib-526.md) captures the algorithm limitations associated
with the library review. It does not claim that correcting a comment fixes the algorithm.

## Improvements to documentation structure and evidence

- Give every important semantic rule a minimal accepted example and a minimal rejected example.
  This makes supported syntax and diagnostic boundaries testable rather than implied by prose.
- Keep intended contracts, implementation mechanisms and test evidence in separate paragraphs.
  A source comment alone cannot establish that a scientific formula or a compiler pass is correct.
- Attach preconditions and numerical failure modes to the operation readers actually use,
  including degeneracy, zero-length inputs, empty collections and profile-dependent rounding.
- Identify coordinate-frame conventions at conversions and composition boundaries, rather than
  relying on a distant geometry overview to prevent silent sign/transposition errors.
- Generate inventories from source and link gate results instead of copying counts and status
  into prose. The reference generator checks local links, source coverage and freshness.
- Give related-work comparisons a named mapping and a named divergence, with primary references.
  Similar syntax is not evidence of identical semantics or historical influence.
- For formal claims, state the model, assumptions and checker command next to the theorem.
  A theorem about a restricted calculus must not become an undocumented claim about all Plato.
- Document each compiler pass's input and output invariants, and distinguish source typing from
  optimized-tree typing. Folding away a rejected branch does not make invalid source well-typed.
- Define the observations that execution-preserving transformations must retain, including fault
  distinctions, evaluation counts, order and divergence. State numerical profile assumptions.
- Keep a reproducible pinned theorem-checker command alongside formal sources, and make its
  verification status independent of Markdown generation and example execution.

## Related work

The [Rust Reference](https://doc.rust-lang.org/reference/) is a useful example of focused
construct-level specification. [Software Foundations](https://softwarefoundations.cis.upenn.edu/)
illustrates how prose, rules and executable proof artifacts can explain each other. Plato needs
the additional distinction between language rules and a library's numerical or geometric contracts.
