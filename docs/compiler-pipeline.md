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
 Symbol graph            PlatoCompiler/Symbols/*          (names resolved, types unresolved)
      │  normalize        Checking/Normalizer             ← NEW
      ▼
 Normalized symbol graph  (canonical, checker-ready)
      │  constrain        Checking/ConstraintGenerator    ← NEW
      ▼
 Constraint system        (a type var per expression + equality/overload constraints)
      │  solve            Checking/Solver                 ← NEW
      ▼
 Substitution + diagnostics
      ┊  elaborate        (planned) → fully-typed IR
      ┊  monomorphize     (planned)
      ┊  emit             CSharpWriter / RustWriter / TypeScriptWriter (today: consume the
                          symbol graph + Types/Analysis directly)
```

The three **NEW** passes live in [`PlatoCompiler/Checking/`](../PlatoCompiler/Checking) and run in
**shadow mode**: they compute a normalized, fully-constrained, solved view of every function and
report diagnostics, but they do **not** feed code generation yet. This is deliberate — it lets the
checker mature against the real standard library (the "stdlib as oracle" strategy) with zero risk to
the production emitter. The off-pipeline byte-identity gate (`tools/regen-plato.ps1`) still passes.

## The IRs

| IR | Where | What it carries | Mutable? |
|---|---|---|---|
| **AST** | `Plato.AST` | Pure syntax + source locations. Operators, member access, indexers are already present as syntax. | no |
| **Symbol graph** | `PlatoCompiler/Symbols` | Names resolved to `DefSymbol`s; scopes; `TypeExpression`s (nominal, `Def` + `TypeArgs`). Calls point at whole overload groups (`FunctionGroupRefSymbol`). Types of expressions are **not** yet resolved. | no (rewrites build new trees) |
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
- `Check(e, expected)` pushes an expected type downward — into conditional branches, return
  positions, and the `default` keyword (whose type is *entirely* contextual).

An overloaded call `f(a, b)` becomes an **`OverloadConstraint`**: the argument types, a fresh result
variable, and the candidate `FunctionDef`s of the group. The declared return type of the enclosing
function is imposed as a `Check` on the body, so the body's type must match the signature.

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
  1. **exact** (unify), 2. **generic** (bind a `$`-variable), 3. **concept** (the argument's type
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

### Scope

The solver handles exact/generic unification, concept (interface) satisfaction with Self-style
return refinement, implicit casts, and **generic-concept element inference** — when an argument
directly implements a generic interface (`List<Number>` against `IArray<$T>`), the element variable
binds (`$T = Number`). Element inference runs **post-commit and best-effort**: it only sharpens the
result type, and never changes which overload was chosen, so it cannot introduce a false mismatch on
a type variable shared with another parameter. It fully resolves **246 of 823** stdlib function
bodies with zero errors (the `SolverResolvesSomeStdLibFunctionsCleanly` test prints the live figure);
the remainder are *reported*, never crashed.

Remaining: element inference through *transitive* (multi-level) concept chains is best-effort — a
one-level substitution can leave it under-determined — and the larger **elaborate → monomorphize →
emit** retargeting (letting the writers consume a fully-typed IR and drop their emit-time
heuristics) is still ahead.

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
- `CheckerIntegrationTests` — builds a real `Compilation` from `plato-src` and asserts: the stdlib
  compiles; normalization invariants hold and are idempotent across every function; the solver is
  total and every diagnostic is located; and a non-trivial subset of real functions resolves cleanly.

Run just these:

```
dotnet test submodules/Plato/PlatoTests/PlatoTests.csproj \
  --filter "FullyQualifiedName~Checker|FullyQualifiedName~Normalizer"
```
