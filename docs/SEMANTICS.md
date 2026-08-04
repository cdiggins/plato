# Plato language semantics

**The normative reference for what Plato constructs mean.** Audience: library authors and
agents writing `.plato` code. It describes the language the compiler and type checker accept
*today* (2026-08-03) — nothing aspirational. For operations and codegen see
[`plato-for-agents.md`](plato-for-agents.md); for checker internals see
[`compiler-pipeline.md`](compiler-pipeline.md) and [`type-checker-handoff.md`](type-checker-handoff.md);
for design rationale see [`plato-overview.md`](plato-overview.md).

One caution up front: the grammar is deliberately more permissive than the language. The parser
(`parakeet/Parakeet.Grammars/PlatoGrammar.cs`) accepts C#-style forms — `throw`, `try`, `switch`,
`yield`, `break`/`continue`, `is`/`as`, string interpolation — for error recovery and familiarity.
Those forms are **not part of the language** this document defines: the checked, portable language
is the subset below, which is what every backend can emit. If a construct is not in this document,
do not rely on it.

The line falls at the elaborator (`Plato.Compiler/Checking/Elaborator.cs`): a statement form is in
the language exactly when `ElaborateStatement` has a case for it. Block, `return`, expression
statement, `if`, loop and comment do; everything else stops at the parser.

## 1. The model

A Plato program is a set of top-level declarations of three kinds — `type`, `interface`
(`concept` remains an accepted alias for the same keyword), and `library` — compiled
together from a folder of `.plato` files. There are no modules, imports, or namespaces: every
declaration in the compilation shares one global scope, and declaration order never matters.
Library names are organizational only; they do not scope names. Interface names use an `I`
prefix (`IEquatable`, `IOrderable`, …).

Everything is an immutable value and every function is pure: no mutation, no I/O, no exceptions,
no null, no reflection, no observable evaluation order. A function's meaning is exactly the value
it returns for its arguments. Typing is static and **nominal** — types match by name, not
structure (with one deliberate exception: tuple construction, §6). Generic and interface-generic
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
type Circle implements IClosedShape
{
    Center: Point2D;
    Radius: Number;
}
```

From a field list the compiler derives the full value surface: a constructor taking the fields in
order, conversions to and from the same-shape tuple, deconstruction, structural equality and
hashing, and immutable per-field setters (`c.WithRadius(2.0)` returns a new `Circle`). Types may
be generic (`type Tuple2<T0, T1> { X0: T0; X1: T1; }`), and a generic type's parameters may carry
`where` bounds (§5).

The `implements` list names interfaces the type satisfies (§4), and is what makes every
interface-generic function available on the type. Note the obligation is enforced *softly*: a
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

Rules (v1, per [`design/plato-sum-types-design-2026-07-27.md`](design/plato-sum-types-design-2026-07-27.md)):
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
expression (`=> expr;`). A block body is also legal: `var` locals (typed by their initializers)
followed by a `return`:

```plato
Lerp(a: Number, b: Number, t: Number): Number {
    var d = b - a;
    return a + d * t;
}
```

A block body may also use `if` statements, `while` loops, and assignment to a local already
introduced by `var`. These elaborate to `TirIf`, `TirLoop` and `TirAssign`, and every backend
writer emits them — C#, C++, Rust, TypeScript and GLSL each carry a `TirLoop` case. `do`, `for`
and `foreach` desugar to the same loop node in `AstNodeFactory`.

They exist for the affine builders `List<T>` and `Buffer<T>`, whose contract — the effect
classification, the construction and rebinding conventions, and the lint rules that police them —
is stated once in `stdlib/foundation/primitives.plato`. Filling a `Buffer<T>` slot by slot is an
imperative algorithm, and rebinding the builder (`xs = xs.Set(i, v)`) is assignment. Purity is
preserved by
uniqueness, not by the absence of assignment — a builder has exactly one reference, so an update
to it is linear rather than observable mutation of shared state.

Reach for them only when that is what you are doing. The expression form, `match`, and the array
combinators are the default; `stdlib/STYLE_GUIDE.md` states the preference and its ordering.

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

**Overloading** is by signature; functions from all libraries and interfaces with the same name form
one global overload group, resolved per call site (§6).

## 4. `interface` — type classes and `Self`

An `interface` declares a capability: a set of function signatures over an implicit type variable
`Self`, which stands for whichever concrete type implements the interface:

```plato
interface IOrderable inherits IEquatable
{
    LessThanOrEquals(a: Self, b: Self): Boolean;
}
```

These are type classes (Haskell classes, Rust traits, Swift protocols) — **not** OO interfaces.
There is no vtable, no boxing, and no heterogeneous collection of "IOrderables"; an interface is a
compile-time constraint, not a runtime value type. `inherits` composes interfaces (implementing the
child obligates the parents' members too); interfaces may be generic (`interface
IProcedural<TDomain, TRange>`).

A type satisfies an interface when every member is available for it — from an intrinsic, a library
function, or another interface's stamped functions. Satisfaction is checked over the transitive
`implements`/`inherits` closure with type-argument substitution (so a type implementing
`IVector`, which inherits an indexable interface over `Number`, satisfies `IIndexable<T>` with
`T = Number`).

`Self` refines: an interface function that takes or returns `Self` takes and returns *the receiver's
concrete type* at every use site. `Divide(self: Self, other: Number): Self` called on a
`Vector3D` returns `Vector3D`, not an interface. This is the whole trick — generic code, concrete
types, resolved at compile time.

**Using interfaces as parameter types.** In a library function signature, an interface in parameter
position means "any implementing type", and introduces an implicit generic constraint:

```plato
Lerp(a: IInterpolatable, b: IInterpolatable, t: Number): IInterpolatable
    => a /* … */;
```

All `IInterpolatable` occurrences in one signature denote the *same* concrete type, and the return
refines to it. The function is monomorphized into every implementing type. When parameters must
be allowed to differ in type, use explicit type variables (§5).

## 5. Generics

Explicit type variables are spelled `$T` in library signatures:

```plato
Zip(xs: Array<$T1>, ys: Array<$T2>, f: Function2<$T1, $T2, $T3>): Array<$T3>;
```

`$`-variables are inferred per call site; declared type parameters on `type`/`interface`
declarations (`<T0, T1>` — no `$`) are rigid within that declaration. Function values are typed
`Function0<TR>` … `FunctionN<T0, …, TR>`; lambdas `(a, b) => expr` and `x => expr` are the only
way to produce them, appear only as arguments to higher-order functions, and their parameter
types are inferred from the target signature (annotations are not written). Passing a named
function where a function value is expected eta-expands it to a lambda automatically.

**Declared bounds (`where`).** Both `interface` and `type` declarations may bound their parameters,
between the parameter list and the `implements`/`inherits` list:

```plato
type Tween<T>
    where T: IInterpolatable
    implements ITimeVarying<T>
{
    From: T;
    To: T;
}
```

A bound must name an interface (else CHK310). It means two things, both checked:

- **Every construction must satisfy it.** `Tween<String>` is rejected wherever it is written —
  field type, signature, or nested inside another type argument — with CHK309, because `String`
  does not implement `IInterpolatable`. Satisfaction is the same transitive closure walk interface
  satisfaction uses.
- **It licenses operations on the bare parameter.** `Lerp` on a value of type `T` is well-typed
  inside a declaration bounded by `IInterpolatable`, and a library signature's `$`-variables inherit
  the bounds of the types they appear in — `Sample(x: Tween<$T>, t: Duration): $T` sees
  `$T: IInterpolatable`. A call on a bounded parameter that no bound supplies is an error (CHK205),
  though it still resolves; an *unbounded* parameter is unrestricted, since bounds are optional.

**A function may bound its own signature variables.** The clause sits after the return type and
before the body — the last thing in the signature, the same slot it occupies on `type` and
`interface` — and names the variable exactly as the signature spells it, with the `$`:

```plato
DeCasteljau(xs: Array<$T>, t: Number): $T where $T: IInterpolatable
    => xs.Count <= 1 ? xs[0] : ...;
```

This is the only way to require an operation on a bare element of an *unbounded* container:
`Array<T>` is a primitive with no bound to inherit, so before this the reduction had to be written
once per element type. A declared function bound joins the inherited ones as a second source of the
same thing, so it licenses calls inside the body identically, and it is emitted as the C# `where`
clause on the generated method. Its extra obligation is at the CALL SITE: whatever the arguments
bind the variable to must satisfy the bound, or the call does not resolve (CHK206) — a bound on a
function is a precondition on inference, checked where inference happens. A bound naming a variable
the signature never mentions constrains nothing and is reported by the linter (LINT002).

After type checking, **monomorphization** grounds everything: each interface-generic or
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

**`&&` and `||` DO NOT SHORT-CIRCUIT.** They are exactly `And` and `Or`: ordinary two-argument
calls whose arguments are both evaluated before the call, and the backends emit them that way.
Nothing about the C-family spelling carries over. A guard written
`i >= 0 && xs[i] > 0` evaluates `xs[i]` even when `i` is negative — the same trap `All`/`Any`
carry as folds (`plato-intrinsics-surface.md`).

Where the second operand is only defined under the first, use the conditional expression or an
`if` statement, both of which take a real branch:

```plato
// Guarded:
i >= 0 ? xs[i] > 0 : false

// NOT guarded — indexes xs even when i is negative:
i >= 0 && xs[i] > 0
```

**Overload and name resolution.** A call resolves against the global overload group for its name.
Each argument is matched against the candidate's parameter at the best applicable tier, in order
of preference:

1. **exact** — the types unify;
2. **generic** — a `$`-variable binds;
3. **interface** — the argument's type implements the interface named by the parameter (with
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

- **Mutation of values** — a `type` is immutable; "setters" (`WithRadius`) return new values, and
  there is no compound assignment and no `++`/`--`. Assignment to a `var` local *is* in the
  language (§3), and the affine builders are the reason it is there.
- **`switch`, `yield`, `break`/`continue`** — they parse, but `ElaborateStatement` has no case for
  them, so they never reach TIR. Use the conditional expression, `match`, and the array
  combinators. (`if`, `while` and their desugarings are in the language — see §3.)
- **Exceptions and null** — no `throw`/`try`/`catch`, no null values, no `?.`. Partiality is
  handled by convention today (`CanInvert: Boolean` alongside `Invert`); `Option`-style sums are
  now expressible but blocked on generic sums.
- **Generic sum types** — rejected in v1 (CHK306); monomorphic sums only.
- **`unique` on your own types** — the modifier is hard-rejected on any type other than the two
  intrinsic builders `List<T>` and `Buffer<T>` (`Plato.Compiler/Symbols/UniqueTypes.cs`). Those
  two do have semantics; the affine discipline over them is runtime-checked and lint-checked
  today, not statically enforced. `stdlib/foundation/primitives.plato` is the authority.
- **OO** — no inheritance between types, no virtual dispatch, no visibility modifiers, no
  heterogeneous collections through interfaces, no runtime type tests (`is`/`as` are not language).
- **Double precision** — `Number` is 32-bit in every current backend; there is no `Double`.
- **I/O, strings-as-workhorse, modules, reflection** — out of scope by design; the host language
  owns them.

## Verifying a claim

The stdlib is the oracle: `stdlib/` (current idiom) and `plato-test-sum/` (sum/match
fixtures) exercise everything above and compile in the repo gates. To check a construct, write a
minimal `.plato` file and run the front end over its folder:

```
dotnet run --project src/Plato.CLI -c Release -- lint <folder>
```

If this document and the compiler disagree, the compiler is right — fix the document (and say so
in the change that revealed it). Language changes (e.g. generic sums, `unique`) must update this
file in the same change that lands them.
