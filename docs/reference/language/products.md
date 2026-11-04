# Products, fields, and construction

A **product type** groups fields that all exist together. `Point2D` stores both
coordinates. An uncertainty record stores both an estimate and its uncertainty.
Fields are in declaration order. That order is also the constructor argument order.

```plato
type Reading {
    Estimate: Number;
    Uncertainty: Number;
}

library Readings {
    Create(_: Reading, x: Number, u: Number): Reading => Reading(x, u);
    Shift(r: Reading, d: Number): Reading => Reading(r.Estimate + d, r.Uncertainty);
}
```

The compiler derives the constructor and the field-access functions. `r.Estimate`
reads a field. `Shift` returns a new record and keeps the uncertainty. It does not
mutate `r`.

The C# writer also derives immutable `With...` methods, equality, hashing, tuple
conversion, and deconstruction. Those host conveniences are not all automatically
bound as Plato source functions. A minimal source fixture that calls an undeclared `WithEstimate` fails
resolution. Define the update function in source when the program needs it. See
[C# mapping](../implementation/csharp.md) and the
[discrepancy register](../implementation/discrepancies.md).

## Nominal identity, structural construction

If `Reading` and `Interval` both contain two `Number` fields, they remain different
types. A tuple in an expected `Reading` position can construct a `Reading`:

```plato
FromPair(x: Number, u: Number): Reading => (x, u);
```

The structural bridge is the tuple. That conversion does not make two product
types implicitly interchangeable. A tuple expression needs the library's `TupleN` declarations.
It is not an anonymous record independent of the input program. The declared
tuple family supports field counts through ten. The conversion is implemented in
[Solver coercion](../../../src/Plato.Compiler/Checking/Solver.Coercion.cs). The
[Rust struct reference](https://doc.rust-lang.org/reference/items/structs.html)
also separates named data from tuple-shaped data. Plato's tuple bridge is its own
rule.

Let `fields(R) = (T₀, …, Tₙ₋₁)` denote a product's ordered field types. The
intended construction rule is:

```text
Γ ⊢ e₀ : T₀  ...  Γ ⊢ eₙ₋₁ : Tₙ₋₁
-------------------------------------------------
Γ ⊢ (e₀, ..., eₙ₋₁) ⇝ R : R
```

`⇝ R` means "coerces to the expected type R." The rule does not require matching
field names. Order and compatible field types matter. Swapping two fields of the
same type type-checks and changes the meaning. Use a named constructor for values
that are easy to reverse.

## Default values

`default` takes its type from context. `default(T)` names the type where the
grammar and the front end support that spelling. A default is a representation-level
value. It is not evidence that a mathematical invariant holds. A zero matrix need
not be invertible. A default interval need not match an application's domain
convention. The interpreter builds defaults in
[Dispatch](../../../src/Plato.Interpreter/Evaluation/Dispatch.cs). Generated C#
uses the host's default. Interpreter defaults include an empty `String` or `Array`,
product fields defaulted recursively, and the first sum case with default payloads.
A function default can contain an internal null representation. Host collection
defaults need their own parity checks. A default is not a checked constructor.

## Sealed construction and proof-like values

```plato
type CheckedSample sealed by CheckSample {
    Sample: Reading;
}
```

`sealed by` names the functions allowed to construct the type. A function is
authorized when its name is in that list and its return type is the sealed type.
Every overload that meets both conditions is authorized. The declaration has to
resolve each permitted name to a suitable function. The check is in
[SealedConstruction](../../../src/Plato.Compiler/Checking/SealedConstruction.cs)
and [its tests](../../../tests/PlatoTests/Checking/SealedConstructionTests.cs).

Outside those functions, the compiler rejects a direct constructor, `new`,
tuple-to-product conversion, a sum case, and `default` construction with CHK311.
Reading a field, matching a sealed sum, and calling an authorized function stay
legal. The form can express "a value exists only after a check." The type checker
does not prove that the named function performs that check. The invariant is a
proof obligation on the function's implementation. Sealed construction resembles a
smart constructor in functional programming: the compiler enforces an allowlist of
source function names.

The boundary is Plato source checking. Generated C# constructors remain public, so
a host program can bypass the discipline. A sealed type is not a security boundary.
It is not a machine-checked refinement type inside an arbitrary C# program.

## Grammar

The product declaration productions, copied from [PlatoGrammar.cs](../evidence/grammar-source.md). The same `Type` rule also accepts the sum form; see [sums](sums.md). `unique` is rejected for arbitrary user types ([unsupported forms](diagnostics.md)).

```text
public Rule Type => Node(UniqueKeyword.Optional() + (PrimitiveKeyword | Keyword("type")) + AdvanceOnFail + Identifier + TypeParameterList + ConstraintList + ImplementsList + SealedList +
                         (SumTypeBody | Braced(FieldDeclaration.ZeroOrMore(), AdvanceOnFail)));
public Rule FieldDeclaration => Node(Identifier + Sym(":") + TypeExpr + EOS);
public Rule SealedList => Node(Optional(Keyword("sealed") + AdvanceOnFail + Keyword("by") + List(Identifier)));
public Rule ImplementsList => Node(Optional(Keyword("implements") + AdvanceOnFail + List(TypeExpr)));
public Rule UniqueKeyword => Node(Keyword("unique"));
public Rule PrimitiveKeyword => Node(Keyword("primitive"));
```

## Meaning

A product groups named fields that all exist at once. The compiler derives a constructor in declaration order and a
read function for each field. Two products with the same fields are still different types. The authority is
[SEMANTICS section 2](../../SEMANTICS.md#2-type--products-and-sums).

## Common mistake

The constructor takes every field, in order. A beginner builds a record with fewer arguments:

```plato
type Reading {
    Estimate: Number;
    Uncertainty: Number;
}

library Bad {
    Make(x: Number): Reading => Reading(x);
}
```

This program is rejected. The file parses and `lint --strict` accepts it, so the rejection comes only when a law reaches
`Make`:

```text
error      Bad.Law_M  case 1:   cannot resolve 'Reading' for (Number) (in Make < Law_M)
```

The message does not say that `Reading` takes two arguments or which is missing. Pass every field:

```plato
library Good {
    Make(x: Number): Reading => Reading(x, 0.0);
}
```

A misspelt field, `r.Estimat`, stops the build with the same four-line `RESOLVE ... Could not find symbol Estimat`
output shown in [functions](functions.md). As there, the message does not suggest `Estimate`; that is a candidate for the
language-improvements list.
