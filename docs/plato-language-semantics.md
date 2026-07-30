# Plato language semantics

**The normative reference for what Plato constructs mean.** Audience: library authors and
agents writing `.plato` code. It describes the language the compiler and type checker accept
*today* (2026-07-28) — nothing aspirational. For operations and codegen see
[`plato-for-agents.md`](plato-for-agents.md); for checker internals see
[`compiler-pipeline.md`](compiler-pipeline.md) and [`type-checker-handoff.md`](type-checker-handoff.md);
for design rationale see [`plato-overview.md`](plato-overview.md).

One caution up front: the grammar is deliberately more permissive than the language. The parser
(`parakeet/Parakeet.Grammars/PlatoGrammar.cs`) accepts C#-style forms — `while`, `for`, `throw`,
`try`, `switch`, `is`/`as`, string interpolation — for error recovery and familiarity. Those forms
are **not part of the language** this document defines: the checked, portable language is the
subset below, which is what the standard library uses and what every backend can emit. If a
construct is not in this document, do not rely on it.

## 1. The model

A Plato program is a set of top-level declarations of three kinds — `type`, `concept`
(`interface` is an accepted alias; the older `stdlib-legacy` uses it), and `library` — compiled
together from a folder of `.plato` files. There are no modules, imports, or namespaces: every
declaration in the compilation shares one global scope, and declaration order never matters.
Library names are organizational only; they do not scope names.

Everything is an immutable value and every function is pure: no mutation, no I/O, no exceptions,
no null, no reflection, no observable evaluation order. A function's meaning is exactly the value
it returns for its arguments. Typing is static and **nominal** — types match by name, not
structure (with one deliberate exception: tuple construction, §6). Generic and concept-generic
code is **monomorphized**: the compiler stamps a concrete copy per implementing type, so there is
no runtime dispatch, no boxing, and no abstraction penalty.

Scalars: `Number` (the floating scalar — 32-bit float in all current backends), `Integer`,
`Boolean`, plus minimal `String`/`Character`. Literals: `0` is `Integer`, `0.0` is `Number`,
`true`/`false` are `Boolean`. Semantically distinct quantities get distinct types (`Angle` is not
`Number`; `Point2D` minus `Point2D` is `Vector2D`) — this is convention enforced by the stdlib's
type definitions, not special compiler machinery.

## 2. `type` — products and sums

A `type` is pure data. The braced body declares fields, and nothing else — no methods, no
visibility, no inheritance:

```plato
type Circle implements ClosedShape2D
{
    Center: Point2D;
    Radius: Number;
}
```

From a field list the compiler derives the full value surface: a constructor taking the fields in
order, conversions to and from the same-shape tuple, deconstruction, structural equality and
hashing, and immutable per-field setters (`c.WithRadius(2.0)` returns a new `Circle`). Types may
be generic (`type Tuple2<T0, T1> { X0: T0; X1: T1; }`).

The `implements` list names concepts the type satisfies (§4), and is what makes every
concept-generic function available on the type. Note the obligation is enforced *softly*: a
missing member is a linter finding (LINT001) and the generated member throws at runtime — it is
not a compile error. Treat LINT001 as an error when authoring.

### Sum types

A `=` case-list body replaces the braced field body and declares a tagged union:

```plato
type PathSegment2D
    = Move(EndPoint: Point2D)
    | Quadratic(Control: Point2D, EndPoint: Point2D)
    | Close;
```

Rules (v1, per [`plato-sum-types-design-2026-07-27.md`](plato-sum-types-design-2026-07-27.md)):
case names must be unique within the sum (CHK305); sums are **monomorphic** — a generic sum type
is rejected (CHK306). A payload-free case omits the parentheses; an all-payload-free sum is the
enum idiom (`type WindingOrder = CounterClockwise | Clockwise;`). Values are built with the
qualified per-case constructor: `PathSegment2D.Move(p)`. The only way to consume a sum is `match`
(§7).

## 3. `library` — functions

A `library` is a named collection of pure free functions:

```plato
library Circles
{
    Area(c: Circle): Number => c.Radius.Sqr * Number.Pi;
}
```

A function is `Name(p0: T0, …): R` followed by a body. The primary body form is a single
expression (`=> expr;`). A block body is also legal and is limited to `var` locals (typed by
their initializers) followed by a single `return`:

```plato
Lerp(a: Number, b: Number, t: Number): Number {
    var d = b - a;
    return a + d * t;
}
```

A declaration with **no body** (`Cos(x: Angle): Number;`) is an intrinsic obligation: the
signature binds an implementation supplied by the target runtime (the handwritten intrinsics
libraries). This is Plato's entire FFI — the host platform is a library whose bodies live
elsewhere.

**Every function's first parameter is its receiver.** `Area(c)` and `c.Area` are the same call;
`t.Lerp(a, b)` is `Lerp(t, a, b)`. This uniform function call syntax is pure sugar — member
access `a.b` *is* the call `b(a)`, and `a.b(c)` *is* `b(a, c)`; there is no other member lookup.
Consequently a **no-argument call needs no parentheses**: `v.Magnitude`, `x.Sqr` (uniform access —
callers cannot tell a computed value from a stored field, and fields are accessed the same way).

**Statics.** A first parameter named `_` means the function does not use its receiver's value,
only its type; it is called on the type name: `Number.Pi`, `Matrix4x4.CreateTranslation(v)`.
A type name in expression position (including `Self`) denotes the type for exactly this purpose.

**Overloading** is by signature; functions from all libraries and concepts with the same name form
one global overload group, resolved per call site (§6).

## 4. `concept` — type classes and `Self`

A `concept` declares a capability: a set of function signatures over an implicit type variable
`Self`, which stands for whichever concrete type implements the concept:

```plato
concept Orderable inherits Equatable
{
    LessThanOrEquals(a: Self, b: Self): Boolean;
}
```

These are type classes (Haskell classes, Rust traits, Swift protocols) — **not** OO interfaces.
There is no vtable, no boxing, and no heterogeneous collection of "Orderables"; a concept is a
compile-time constraint, not a runtime value type. `inherits` composes concepts (implementing the
child obligates the parents' members too); concepts may be generic (`concept
Procedural<TDomain, TRange>`).

A type satisfies a concept when every member is available for it — from an intrinsic, a library
function, or another concept's stamped functions. Satisfaction is checked over the transitive
`implements`/`inherits` closure with type-argument substitution (so a type implementing
`VectorLike`, which inherits an array-like concept over `Number`, satisfies `ArrayLike<T>` with
`T = Number`).

`Self` refines: a concept function that takes or returns `Self` takes and returns *the receiver's
concrete type* at every use site. `Divide(self: Self, other: Number): Self` called on a
`Vector3D` returns `Vector3D`, not an interface. This is the whole trick — generic code, concrete
types, resolved at compile time.

**Using concepts as parameter types.** In a library function signature, a concept in parameter
position means "any implementing type", and introduces an implicit generic constraint:

```plato
Lerp(a: VectorLike, b: VectorLike, t: Number): VectorLike
    => a.ZipComponents(b, (x, y) => x.Lerp(y, t));
```

All `VectorLike` occurrences in one signature denote the *same* concrete type, and the return
refines to it. The function is monomorphized into every implementing type. When parameters must
be allowed to differ in type, use explicit type variables (§5).

## 5. Generics

Explicit type variables are spelled `$T` in library signatures:

```plato
Zip(xs: Array<$T1>, ys: Array<$T2>, f: Function2<$T1, $T2, $T3>): Array<$T3>;
```

`$`-variables are inferred per call site; declared type parameters on `type`/`concept`
declarations (`<T0, T1>` — no `$`) are rigid within that declaration. Function values are typed
`Function0<TR>` … `FunctionN<T0, …, TR>`; lambdas `(a, b) => expr` and `x => expr` are the only
way to produce them, appear only as arguments to higher-order functions, and their parameter
types are inferred from the target signature (annotations are not written). Passing a named
function where a function value is expected eta-expands it to a lambda automatically.

After type checking, **monomorphization** grounds everything: each concept-generic or
`$`-generic function is instantiated per concrete type combination actually used, `Self` is
replaced by the receiver type, and the emitted code contains only direct calls on concrete types.
A program's meaning never depends on monomorphization — it is an implementation of the semantics
above, not a semantic feature — but it explains the performance model: abstraction is free.

## 6. Expressions, construction, conversions, operators

Expression forms: literals; names; calls in both spellings (`f(a, b)` / `a.f(b)`); the
conditional `cond ? a : b` (strict typing: both branches must unify to one type); array literals
`[a, b, c]` (an `Array<T>` of the unified element type); tuple expressions `(a, b)`; lambdas;
`match` (§7); `default` (the contextually-expected type's default value — its type comes entirely
from context); indexing `x[i]`, which is exactly the call `At(x, i)`.

**Tuples construct types structurally.** A tuple expression takes on any expected concrete type
with the same field shape: `(t.Cos, t.Sin)` in a `Point2D`-returning position *is* a `Point2D`.
This is the one structural hole in the nominal system, and it works at return and argument
boundaries where the expected type is known. Mechanically, `(a, b)` is a construction of the
stdlib type `Tuple2<T0, T1>` (the front end resolves it as a `Tuple2` call — a compilation
without the `TupleN` declarations from `primitives.plato` rejects tuple expressions; `TupleN`
exists up to 10 fields), which then coerces to the same-shape struct.

**A function named after a type is a conversion.** `Point3D(v: Vector3D): Point3D => …` defines
both the callable form (`v.Point3D`) and an *implicit* conversion the resolver may apply (and the
C# backend emits as an `implicit operator`). Implicit conversions participate in overload
resolution at the lowest-preference tier, and at return boundaries.

**Operators are names.** Every operator is sugar for a call to a well-known function name, and
defining the name yields the operator on your type:

| Operator | Function | Operator | Function |
|---|---|---|---|
| `+` | `Add` | `==` | `Equals` |
| `-` (binary) | `Subtract` | `!=` | `NotEquals` |
| `*` | `Multiply` | `<` `<=` `>` `>=` | `LessThan`, `LessThanOrEquals`, `GreaterThan`, `GreaterThanOrEquals` |
| `/` | `Divide` | `&&` | `And` |
| `%` | `Modulo` | `\|\|` | `Or` |
| `-` (unary) | `Negative` | `!` | `Not` |
| `&` `\|` `^` | `BitwiseAnd`, `BitwiseOr`, `XOr` | `~` | `Complement` |

Precedence is C-family (`* / %` over `+ -` over comparisons over equality over `&&` over `||`;
the table lives in `Plato.AST/Operators.cs`). Since operators are ordinary overloaded calls,
mixed-type arithmetic (`vector * scalar`) is just an `Add`/`Multiply` overload, not special
machinery.

**Overload and name resolution.** A call resolves against the global overload group for its name.
Each argument is matched against the candidate's parameter at the best applicable tier, in order
of preference:

1. **exact** — the types unify;
2. **generic** — a `$`-variable binds;
3. **concept** — the argument's type implements the concept named by the parameter (with
   `Self`-refinement of the return);
4. **conversion** — an implicit conversion exists.

Each tier costs more than the previous; the candidate with the lowest total cost wins. So a
concrete overload beats a generic one, and an exact match beats one needing a conversion. If no
candidate matches, that is an error (CHK201). If several tie at lowest cost with the same return
type, the call is accepted (CHK202, informational); if they tie with *different* return types,
the call is ambiguous and rejected (CHK203) — ambiguity is never silently resolved.

## 7. `match`

`match` is an expression (usable anywhere an expression is) and the sole eliminator for sum
types:

```plato
EndPoint(seg: PathSegment2D, start: Point2D): Point2D =>
    match (seg) {
        Move(p)         => p;
        Quadratic(c, p) => p;
        Close           => start;
    };
```

Rules (v1): the subject must be a sum type (CHK304); the arms must cover **every case exactly
once** — no missing case (CHK300), no unknown case (CHK301), no duplicate (CHK302), and **no
default/wildcard arm** (CHK307): exhaustiveness is by enumeration only. Binders are positional,
bound to the case's fields in declaration order, and the binder count must equal the field count
(CHK303) even if some binders go unused. All arm results unify to the type of the whole `match`.

Lowering: `match` is rewritten during elaboration into a chain of conditionals over the sum's
tag — no dedicated TIR node — and a sum type emits to C# as a tagged `readonly partial struct`
(an `int Kind` in declaration order, flattened `Case_Field` fields, one static factory per case).
Emission is **C#-only** in v1: the TypeScript and Rust writers reject a sum declaration with a
CHK320 comment instead of emitting garbage.

## 8. Not in the language

Explicitly absent, so nobody infers them from the C#-flavored syntax:

- **Mutation and assignment** — no `=` outside `var` initialization, no compound assignment, no
  `++`/`--`. "Setters" (`WithRadius`) return new values.
- **Statements beyond `var` + `return`** — `while`/`do`/`for`/`foreach`, `if`-statements,
  `switch`, `yield`, `break`/`continue` parse but are not in the checked language; use the
  conditional expression, `match`, and the array/functional combinators.
- **Exceptions and null** — no `throw`/`try`/`catch`, no null values, no `?.`. Partiality is
  handled by convention today (`CanInvert: Boolean` alongside `Invert`); `Option`-style sums are
  now expressible but blocked on generic sums.
- **Generic sum types** — rejected in v1 (CHK306); monomorphic sums only.
- **Affine/unique types** — the `unique` modifier parses and is reserved; it has no semantics
  yet.
- **OO** — no inheritance between types, no virtual dispatch, no visibility modifiers, no
  heterogeneous collections through concepts, no runtime type tests (`is`/`as` are not language).
- **Double precision** — `Number` is 32-bit in every current backend; there is no `Double`.
- **I/O, strings-as-workhorse, modules, reflection** — out of scope by design; the host language
  owns them.

## Verifying a claim

The stdlib is the oracle: `stdlib/` (current idiom) and `plato-test-sum/` (sum/match
fixtures) exercise everything above and compile in the repo gates. To check a construct, write a
minimal `.plato` file and run the front end over its folder:

```
dotnet run --project Plato.CLI -c Release -- lint <folder>
```

If this document and the compiler disagree, the compiler is right — fix the document (and say so
in the change that revealed it). Language changes (e.g. generic sums, `unique`) must update this
file in the same change that lands them.
