# Indexed tables: convenient construction and traversal

Status: design proposal, not accepted or implemented. Tracking:
[compiler-533](../../tracker/issues/compiler-533.md), an idea awaiting design review.
This document records proposed semantics and tradeoffs, not performance measurements.
All new syntax below is illustrative; it is not executable Plato or a settled grammar.

## Problem and recommendation

Trees and graphs can be represented as tables whose rows contain typed indices of other
rows. This gives finite row layouts and explicit storage, but declaring index wrappers,
building tables, and traversing child links creates repetitive code.

Recommend a combination of generated typed tables, a scoped construction expression,
and a bottom-up fold over validated acyclic tables. Keep lookup explicit as `table[id]`.
Introduce each independently, first proving its ordinary-library expansion. Do not add
general recursive values, hidden allocation, or implicit table arguments as prerequisites.

The motivation includes a Plato-written interpreter, expression graphs, and geometry
structures. The fold is appropriate for eager acyclic computations; it is not a complete
execution strategy for an interpreter.

## Existing foundations and authority

[SEMANTICS.md](../SEMANTICS.md) defines sums, generic sums, local assignment, and indexing
through `At`. [Typed-index conventions](../../stdlib/CONVENTIONS.md) define the existing
cross-array reference convention. [Scalar field graphs](../../stdlib/geometry/fields.types.plato)
provide a concrete precedent: sum cases store typed child indices and the graph requires
children to precede parents. [Affine types](../affine-types.md) describe the builder discipline.

The [October generic-sum decision](../../tracker/decisions/2026-10-23-generic-sums-monomorphize-per-use.md)
already supports generic sums; this proposal does not depend on introducing them.
The [July sum design](plato-sum-types-design-2026-07-27.md) is historical context, not authority
for which recursive representations work today. This proposal does not claim that a specific
recursive declaration has been compile-tested.

## Generated typed tables

Illustrative declaration and row type:

```text
table Expressions of Expr indexed by ExprId;

type Expr
    = Literal(Value: Number)
    | Add(Left: ExprId, Right: ExprId);
```

The declaration generates a nominal integer index wrapper, a table wrapper around an array
of rows, and typed lookup and validation helpers. It must resolve together with the row
declaration so the row may name the generated index type. Physical rows remain finite:
`Add` stores indices, not embedded expressions.

`expressions[id]` remains ordinary `At(expressions, id)` syntax. Generated C# follows the
existing method-only emission contract; this does not reintroduce generated C# indexers.
Raw construction from external data requires validation before trusted lookup. Validation
checks range and required edges; an absent-index sentinel is not a valid required child.
The exact checked-lookup result API remains a design question.

An index distinguishes row domains, not table instances. `ExprId` prevents accidental use
of a vertex index, but cannot prove that an expression index belongs to this particular
expression table. Do not advertise instance ownership safety without additional machinery.
The initial model is immutable after construction, with no deletion or compaction that
silently changes the meaning of existing indices.

## Scoped construction

```text
var expression = build Expressions {
    let a = insert Expr.Literal(2.0);
    let b = insert Expr.Literal(3.0);
    let root = insert Expr.Add(a, b);
    yield root;
};
```

Each `insert` appends a row and returns its typed index. Named indices make shared children
explicit. The block returns an immutable table and a root index; the final public result
type and syntax are to be designed. Lowering uses sequential builder operations followed
by a freeze. The mutable builder cannot escape the block or be captured by a closure.
Exceptional or early completion must not expose a partly initialized successful result.

Construction does not automatically establish acyclicity if arbitrary indices can enter
the block. A validated acyclic variant must check that each required edge names an earlier
row in the same construction, or validate the resulting table before exposing its proof.
Imported tables require the same checks. A root must name a row in the returned table.

GPU-side construction requires explicit capacity and a defined capacity-exhaustion result.
CPU-side construction followed by GPU upload is a separate, simpler use case. No implicit
unbounded allocation is part of the proposal.

## Bottom-up fold

```text
var values = expressions.Fold {
    Literal(n) => n;
    Add(left, right) => left + right;
};
```

The receiver must be validated as children-first and acyclic. For a designated child edge,
the case binding supplies the child's computed result, not its index. Other payload fields
remain unchanged. A declaration annotation or schema must identify child edges explicitly;
the compiler must not guess from field names or treat every index as a recursive child.
Repeated edges and sharing are allowed; self-links and forward links fail validation.

Lowering walks rows in order, reads child results from a parallel result array, evaluates
the case body, and stores the result. The fold returns the result table; the caller can
select its root. It evaluates each stored row once, including rows unreachable from the
root. Time is linear in rows plus edges, excluding the work inside user case bodies;
result storage is linear in rows. It requires neither recursive calls nor a traversal stack.

This is an eager operation. It must not be substituted for demand-driven evaluation of
conditionals, short-circuiting, recursive guest functions, or potentially failing unused
branches. A Plato interpreter needs an explicit execution stack or another demand-driven
strategy for those behaviors. A general graph with cycles needs a different algorithm.

## Backend contract

Tables, tags, payloads, and integer edges give a representation a shader backend can lower
without recursively embedded structures. This is a proposed lowering path, not a claim of
support in the existing GLSL writer. The target must support the selected row and result
types, storage access, and resource bounds before a program can be accepted.

For a bounded target, storage may use fixed-capacity arrays or an explicitly supplied
resource. More capable targets may use buffer storage; that must not be assumed for every
GLSL profile. The physical layout may later be arrays of records or separate field arrays,
provided logical indexing and evaluation order are preserved. Unsupported capabilities
must produce a diagnostic rather than silent truncation or changed evaluation semantics.

## Alternatives and disposition

| Alternative | Disposition | Reason |
|---|---|---|
| Handwritten wrappers, builders, and loops only | Retain as baseline and prototype; reject as the entire convenience solution | Requires no new syntax and proves the expansion, but repeats the declarations and bookkeeping this proposal addresses. |
| Scoped dereferencing such as `within expressions { *id }` | Defer | Saves little over `expressions[id]` and adds an implicit lookup context, especially confusing when several tables are in scope. |
| Implicit table arguments for calls within a scope | Reject for the initial design | Changes overload resolution and hides dependencies. Explicit parameters make code easier to move and inspect. |
| Temporary located-node views containing table plus index | Defer to a library experiment | Useful for navigation, but requires care to avoid copying table storage or pretending views are stored pointers. It does not solve construction or folds. |
| Recursive-looking `indexed type Expr = ... Add(Expr, Expr)` | Defer until explicit tables establish semantics | Conceals which fields become edges and leaves mutual recursion, nested containers, ownership, equality, and copying underspecified. It could later desugar to this proposal. |
| General immutable boxes or recursive reference types | Reject as a prerequisite, not as a future language feature | Adds allocation and representation commitments across backends. It does not itself solve shader storage or recursive execution. |
| Implicit universal heap managed by the compiler | Reject for this proposal | Hides lifetime, allocation, capacity, and failure costs that GPU users need to control. |
| Expand recursive shapes to a fixed depth | Reject as the general representation | Bakes a depth bound into layout, can grow storage rapidly, and needs explicit behavior at the bound. It remains a possible specialization for deliberately bounded shapes. |
| Branded indices unique to each table instance | Defer | Would strengthen ownership guarantees but needs generative types, lifetimes, or runtime identity checks; nominal row-domain indices are a smaller starting point. |
| Automatically use a bottom-up fold for all traversals | Reject | Eager evaluation changes demand-driven behavior and cannot handle arbitrary cycles or calls. Folds must be explicitly requested over validated input. |

Rejection here means excluded from the recommended initial scope, not proof that an
alternative is impossible. Deferred options should be reconsidered only against a concrete
use case that the explicit design handles poorly.

## Questions to settle before implementation

- How are edge fields declared, including optional edges and collections of children?
- Does table construction return a named table/root aggregate or use a general result type?
- How are bounds, malformed edges, and capacity exhaustion represented consistently?
- Which ownership guarantees come from a construction scope, and which require validation?
- Does the acyclic proof use a sealed type constructed only by validation/build operations?
- How are copying between tables, remapping indices, and structural equality exposed explicitly?
- What syntax generates declarations without creating name collisions or surprising lookup?
- Which shader profiles and storage forms belong to the first supported lowering?

## Evaluation path and acceptance evidence

First prototype ordinary typed-index wrappers, explicit construction, and a children-first
loop in Plato. This separates language ergonomics from runtime capability. Then evaluate
generated table declarations, scoped construction, and folds as independent additions.
No implementation is authorized by this proposal; the linked idea must be triaged first.

Acceptance should demonstrate that the convenient and explicit forms agree on results,
sharing, ordering, and failures. Include wrong index domains, out-of-range edges, self and
forward edges, invalid roots, shared children, empty input, and exhausted capacity. Check
that builders cannot escape, and document the remaining same-domain cross-table hazard.
Compare interpreter and generated C# behavior. Any claimed GPU support needs generated
shader compilation and execution on the declared profile, including resource-bound cases.

Use an eager expression graph to validate folds and a conditional with a failing unused
branch to establish their boundary: the fold must never be presented as interchangeable
with a demand-driven interpreter.
