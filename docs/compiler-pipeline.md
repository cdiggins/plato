# Plato compiler pipeline

This document describes how a `.plato` source file becomes generated code, the intermediate
representations (IRs) it passes through, and the type-checker passes that were added to sit
between the bound symbol graph and code generation.

It is aimed at people working *on the compiler*. For the language itself see
[`docs/plato-for-agents.md`](plato-for-agents.md) and the [README](../README.md).

## Overview

```
 source (.plato)
      │  parse            Parakeet grammar + AstNodeFactory
      ▼
 AST                      Ara3D.Geometry.AST.*            (syntax, spans)
      │  bind             SymbolFactory
      ▼
 Symbol graph            Plato.Compiler/Symbols/*          (names resolved, types unresolved)
      │  normalize        Checking/Normalizer             ← NEW
      ▼
 Normalized symbol graph  (canonical, checker-ready)
      │  constrain        Checking/ConstraintGenerator    ← NEW
      ▼
 Constraint system        (a type var per expression + equality/overload constraints)
      │  solve            Checking/Solver                 ← NEW
      ▼
 Substitution + diagnostics
      │  elaborate        Checking/Elaborator            → Typed IR (TIR)
      ▼
 TIR (typed, resolved, coercions explicit)
      │  monomorphize     Checking/Monomorphizer         (per concrete instantiation, ground)
      ▼
 monomorphized TIR
      │  emit             DEFAULT C# style member bodies: TirCSharpBodyWriter (UseTir, on by
                          default). Everything else (static bodies, extension/scalar/optimize
                          styles, TS/Rust): the legacy writers, consuming the symbol graph.
```

The passes live in [`Plato.Compiler/Checking/`](../src/Plato.Compiler/Checking). They matured in
**shadow mode** against the real standard library (the "stdlib as oracle" strategy); since
increment 3 the TIR is the **production emit path for default-style member bodies**, proven
byte-identical to the legacy writer (the byte-identity gate `tools/regen-plato.ps1` now exercises
it). The checker also reports located diagnostics for every function.

## The IRs

| IR | Where | What it carries | Mutable? |
|---|---|---|---|
| **AST** | `Plato.AST` | Pure syntax + source locations. Operators, member access, indexers are already present as syntax. | no |
| **Symbol graph** | `Plato.Compiler/Symbols` | Names resolved to `DefSymbol`s; scopes; `TypeExpression`s (nominal, `Def` + `TypeArgs`). Calls point at whole overload groups (`FunctionGroupRefSymbol`). Types of expressions are **not** yet resolved. | no (rewrites build new trees) |
| **Normalized symbol graph** | same types, produced by `Normalizer` | The symbol graph after canonicalization (below). Same node kinds, guaranteed invariants. | no |
| **Constraint system** | `Checking/ConstraintSystem` | A `TypeExpression` (usually a fresh `$`-variable) for every expression, plus equality and overload constraints, plus generation-time diagnostics. | accumulated |
| **Substitution** | `Checking/Solver` | A binding for each unification variable; the solved type of any expression is `Zonk(ExprTypes[e])`. | union-find map |

The type representation is **nominal** throughout: `TypeExpression` = a `TypeDef` plus type
arguments. Unification variables are simply `TypeExpression`s whose `Def.Kind == TypeVariable`
(their names start with `$`). There is no separate structural type language — the checker solves
directly over the same types the rest of the compiler uses.

## What the front-end already desugars (important)

A normalization pass in many compilers lowers operators and method-call sugar. **In Plato that has
already happened before we ever see a Symbol**, in `AstNodeFactory` and `SymbolFactory`:

| Surface | Becomes |
|---|---|
| `a + b`, `-x` | `Add(a, b)`, `Negative(x)` — a `FunctionCall` to a group |
| `a.b` (member access) | `FunctionCall b(a)` with `HasArgList = false` (receiver first) |
| `a.b(c, d)` (UFCS) | `FunctionCall b(a, c, d)` with `HasArgList = true` |
| `a[i]` (indexer) | `At(a, i)` |
| `(e)` (parentheses) | dropped |
| `default` | `FunctionCall` on a `KeywordRefSymbol` (a nullary intrinsic) |

So the `Normalizer` does **not** re-lower any of this. Its job is the *residual* rewrites and,
mainly, guaranteeing the invariants the constrain pass depends on.

## Pass 1 — Normalize (`Checking/Normalizer.cs`)

`Normalizer.NormalizeFunction(FunctionDef)` returns a canonical copy of a function body. Rules:

- **R1** strip any residual `Parenthesized`: `(e)` → `e`
- **R2** unwrap a single-element `MultiStatement`: `Multi(s)` → `s`
- **R3** **eta-expand** a `FunctionGroupRefSymbol` used in *value* position (not as a call's callee)
  into a lambda, when every overload shares one arity ≥ 1:
  `g` → `(p0, …, pN-1) => g(p0, …, pN-1)`. This means the constrain pass only ever meets an overload
  set in callee position; every first-class use of a function is an ordinary lambda.

The pass is **behavior-preserving** (Plato is pure, so eta-expansion is sound), **idempotent**, and
**identity-preserving** — it never clones a `DefSymbol` that is referenced by identity (parameters,
local variables), so references keep resolving. The original graph the emitter consumes is left
untouched; the normalized form is a parallel artifact exposed as `Compilation.NormalizedFunctions` /
`Compilation.GetNormalizedFunction(fd)`.

### Invariants (`Checking/NormalizationInvariants.cs`)

`NormalizationInvariants.Check(symbol)` is the executable contract — it returns zero violations on
any normalized tree:

- **NORM1** no `Parenthesized` remains
- **NORM2** no eta-expandable function-group reference remains in value position
- **NORM3** no single-element `MultiStatement` remains
- **NORM4** every `FunctionCall` callee is a callable form (group ref, lambda, parameter/variable
  reference, or the `default` keyword)

## Pass 2 — Constrain (`Checking/ConstraintGenerator.cs`)

Walks a normalized function body and produces a `ConstraintSystem`. It is **bidirectional**:

- `Synthesize(e)` infers a type upward (literals → their scalar type; parameter/variable references
  → their declared type; `new T(…)` → `T`; array literals → `Array<$E>`; conditionals → a fresh
  result variable with both branches checked against it; …).
- `Check(e, expected)` pushes an expected type downward — into conditional branches and the
  `default` keyword (whose type is *entirely* contextual).

An overloaded call `f(a, b)` becomes an **`OverloadConstraint`**: the argument types, a fresh result
variable, and the candidate `FunctionDef`s of the group.

Details that matter (added in increment 3):

- **Unannotated lambda parameters and locals carry the placeholder type `Any`** from binding — the
  generator mints a fresh inference hole per unannotated lambda parameter / `var` local and uses it
  for every reference, so the enclosing HOF signature (or the initializer) determines them.
- **A type used as a value** (`Self.CreateFromComponents(…)`, `Number.MinValue`'s receiver) is
  typed as the referenced type itself, so interface parameters can bind through it.
- **A bare reference to a unique nullary function** (a constant) is typed as its return type — the
  writer's `Constants.<Name>` rule, mirrored.
- **RETURN positions emit a `CoercionConstraint`** (soft), not an equality: Plato's generated C#
  admits an implicit conversion at the return boundary (see the solver below).

## Pass 3 — Solve (`Checking/Solver.cs`)

Consumes the constraint system and produces a substitution plus located diagnostics. Properties:

- **Total** — never throws. A clash, no-match, ambiguity, or recursive type is a `CheckDiagnostic`
  carrying the originating `Symbol` (so it can point at a source location). This is the entire ROI:
  it converts "mysterious error in generated C#" into "the checker points at the expression".
- **Occurs-checked** — a variable is never bound to a type containing it, so there are no infinite
  types (`$a ~ Array<$a>` is reported, not looped — contrast the concatenative engine's `(rec N)`).
- **Nominal unification** — only `$`-type-variables are flexible; declared type parameters (rigid)
  and named types match by name; type arguments unify structurally.
- **Tiered argument matching** — an argument matches a parameter, in order of preference:
  1. **exact** (unify), 2. **generic** (bind a `$`-variable), 3. **interface** (the argument's type
  implements an interface parameter, via `TypeExpression.IsImplementing`; the return type is refined
  to the concrete argument where it names the same interface — Plato's "Self" behavior), 4.
  **conversion** (an implicit cast relation exists, via `Compilation.TypeRelations`). Each tier has a
  cost; the lowest total cost wins, so a concrete overload beats a generic one and an exact match
  beats a conversion.
- **Deferred-commitment overload resolution** — an overloaded call is resolved only once its
  argument types are ground. Candidates are trial-matched on a scratch substitution and ranked by
  cost; then:
  - a unique lowest-cost candidate → **commit** (bind args↔params, result↔return);
  - zero viable → **`CHK201` no-match** (error);
  - a lowest-cost tie with a common return type → bind the result, **`CHK202`** (info);
  - a lowest-cost tie with different return types → **`CHK203` ambiguous** (error) — *reported, not
    silently resolved to the first candidate*.

  Generic candidates are **instantiated with fresh variables per call**, so each use of an overload
  is independent.

### Diagnostic codes

| Code | Meaning |
|---|---|
| `CHK101` | cannot unify two named types (clash) |
| `CHK102` | type-argument count mismatch |
| `CHK103` | recursive type (occurs-check failure) |
| `CHK201` | no overload matches the argument types |
| `CHK202` | multiple overloads match with a common return type (info) |
| `CHK203` | ambiguous call — overloads match with differing return types |
| `CHK204` | bounded-polymorphic call — concrete overloads tie on an unbound variable; deferred to monomorphization (info) |
| `CHK205` | a call on a bare type parameter that its declared bounds do not supply |
| `CHK206` | a call whose arguments do not satisfy a bound the callee declares on its own signature |
| `CHK309` | a type argument does not satisfy the bound declared on that parameter |
| `CHK310` | a declared `where` bound does not name an interface |

### Declared type-parameter bounds

Both `interface` and `type` declarations may bound their parameters
(`type Tween<T> where T: Interpolatable`), and a library FUNCTION may bound its own signature
variables (`DeCasteljau(xs: Array<$T>, t: Number): $T where $T: Interpolatable` — plato-393). The
declaration-level bounds land on `TypeParameterDef.Constraints` and the function-level ones on
`FunctionDef.DeclaredBounds`; `TypeConstraints.InheritedBounds` unions the two sources, so every
consumer downstream reads one thing. They are read in three places, all in `Checking/`:

- **`TypeConstraintChecker`** — a declaration-level pass, run like `SumTypeChecker` and
  `ExistentialConceptChecker`. Every construction the declaration writes (implements/inherits
  clauses, field types, sum-case field types, method signatures) must supply arguments that satisfy
  the bounds, or `CHK309`; a bound that is not an interface is `CHK310`.
- **The solver's bound-licensed member lookup** — a bare type parameter stands in for an interface
  parameter only when one of its bounds carries that interface. A library signature's variables
  inherit the bounds of the constructed types they appear in (`x: Tween<$T>` gives
  `$T: Interpolatable`), which is what makes an operation on a bare `$T` well-typed.
- **The solver's candidate viability rule** — this is where ARGUMENT SATISFACTION for a function
  bound is enforced. A candidate whose arguments matched is still rejected when what they bound its
  variable to fails the bound the callee declares, so `DeCasteljau` over an `Array<String>` does not
  resolve. Reported `CHK206` rather than `CHK201`, because the signature matched and only the bound
  failed. (A bound on a TYPE is checked at its construction site instead, by `CHK309`: a type is
  written, a function is called, and each is checked where it is used.)

Bounds restrict where they are declared and change nothing where they are not: an *unbounded*
parameter is as permissive as it was before bounds were read. An unlicensed call on a *bounded*
parameter is `CHK205`, an error — a declaration may not promise what the type system cannot check.
It still RESOLVES, so elaboration and emission are unaffected and no `CHK201` cascade follows;
only the report is raised.

`Checking/TypeConstraints.cs` holds the single reading of a bound that all three consumers share,
so the construction-site gate and the two solver licences cannot disagree. The EMISSION side reads
the same file: `TirEmitSource.IsOpenGenericEmittable` licenses a body whose call dispatches on a
bare receiver exactly when a bound supplies the member, and the C# writer renders the matching
`where` clause (`writers/Plato.CSharpWriter/CSharpBoundWriter.cs`). Which declarations' bounds
reach C# is the one predicate `TypeConstraints.EmittedToCSharp` — concrete types today, interface
interfaces excluded — read by the emission licence and the writer alike, so a body is never
licensed by a bound the emitted signature does not carry. Decision:
[`tracker/decisions/2026-08-01-declared-type-parameter-bounds-are-verified-and-emitted.md`](../tracker/decisions/2026-08-01-declared-type-parameter-bounds-are-verified-and-emitted.md).

### Scope

The solver handles exact/generic unification, interface satisfaction with Self-style
return refinement, implicit casts, and generic-interface element inference. Increment 3 extended it
substantially; the mechanisms now in place:

- **Interface-method `Self` instantiation** — a candidate that is an interface method (`Divide(self:
  Self, other: Number): Self` on `IScalarArithmetic`) instantiates `Self` (anywhere in its
  signature) as a fresh interface variable constrained to the owning interface, with the interface's
  type parameters as fresh holes. Matching binds the variable to the concrete argument, so a `Self`
  return refines to the receiver's type — the "Self" behavior.
- **Closure-walking interface satisfaction** (solver-local; `TypeExtensions.IsImplementing` is left
  untouched for the production writer) — satisfaction walks the argument's transitive
  Implements/Inherits closure with per-level type-argument substitution and, on a name match,
  unifies the found instance's arguments with the interface's, binding element holes
  (`IVectorLike : IArrayLike<Number>` binds `$T = Number`). Works when the argument is itself an
  interface. The older post-commit element refinement remains as a backstop.
- **Permissive `Self`** — `Self` unifies with anything, binding nothing: it is a placeholder the
  reifier replaces per concrete type, so at generic-check time `Self ~ IArrayLike<T>` is
  satisfiable-by-substitution; monomorphization grounds it.
- **HOF scheduling** — a function-shaped argument (`Function{N}` with holes) does not block
  overload resolution: the chosen candidate's signature determines the lambda's parameter types
  (checking-mode inference). Forced resolutions re-enter the fixpoint one at a time, so each
  commitment can unblock the rest.
- **Return coercions** (`CoercionConstraint`) — unify, else accept a cast relation
  (`Vector3` for `Point3D`), a `Tuple{N}` for a same-shape concrete struct (binding the tuple's
  element holes from the struct's fields — the generated structs carry an implicit tuple
  conversion), or a value for an interface it implements; else report `CHK101`.

It fully resolves **745 of 823** stdlib function bodies with zero errors (the
`SolverResolvesSomeStdLibFunctionsCleanly` test prints the live figure); the remainder are
*reported*, never crashed — mostly calls into handwritten intrinsic members the compiler cannot
see, which the elaborator emits as *syntactic* calls (name + shape).

## Orchestration and use

`Checking/TypeChecker.cs` ties the passes together for one function
(`normalize → constrain → solve`) and returns a `TypeCheckResult` (`Diagnostics`, `Succeeded`,
`TypeOf(expression)`). `TypeChecker.CheckAll()` runs over every bodied function in a `Compilation`.

```csharp
var checker = new TypeChecker(compilation);
foreach (var result in checker.CheckAll())
    foreach (var d in result.Diagnostics)
        Console.WriteLine(d);   // e.g. "[Error] CHK201: No overload of 'Add' matches (Boolean, Boolean) (at FunctionCall #123)"
```

## Tests

All in `PlatoTests`:

- `NormalizerTests` — parser-free unit tests for R1–R3, idempotence, and the invariant verifier.
- `CheckerTests` — parser-free unit tests for unification, occurs check, generic instantiation, and
  the four overload-resolution outcomes.
- `CheckerIntegrationTests` — builds a real `Compilation` from `stdlib-legacy` and asserts: the stdlib
  compiles; normalization invariants hold and are idempotent across every function; the solver is
  total and every diagnostic is located; and a non-trivial subset of real functions resolves cleanly.

Run just these:

```
dotnet test submodules/Plato/PlatoTests/PlatoTests.csproj \
  --filter "FullyQualifiedName~Checker|FullyQualifiedName~Normalizer"
```
