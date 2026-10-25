# Products, fields, and construction

A **product type** groups fields that all exist together. `Point2D` stores both coordinates; an uncertainty record stores both an estimate and its uncertainty. Fields appear in declaration order, which also determines constructor argument order.

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

The compiler derives the constructor and field access functions. `r.Estimate` reads a field.
`Shift` explicitly constructs a new record while preserving uncertainty; it does not mutate `r`.
The C# writer also derives immutable `With...` methods, equality, hashing, tuple conversion and
deconstruction. These host conveniences are not all automatically bound as Plato source functions:
a minimal source fixture calling an undeclared `WithEstimate` fails resolution. Define a source
update function explicitly when needed. See [C# mapping](../implementation/csharp.md) and the
[discrepancy register](../implementation/discrepancies.md) for this distinction.

## Nominal identity, structural construction

If `Reading` and `Interval` both contain two `Number` fields, they remain distinct types. A tuple in an expected `Reading` position can construct one:

```plato
FromPair(x: Number, u: Number): Reading => (x, u);
```

This does not mean every two-field type is implicitly interchangeable with every other two-field type. The structural bridge is the tuple. Tuple expressions require the library's `TupleN` declarations; they are not magical anonymous records independent of the input program. The declared tuple family supports field counts through ten.

Let `fields(R) = (T₀, …, Tₙ₋₁)` denote a product's ordered field types. The intended construction rule is:

```text
Γ ⊢ e₀ : T₀  ...  Γ ⊢ eₙ₋₁ : Tₙ₋₁
-------------------------------------------------
Γ ⊢ (e₀, ..., eₙ₋₁) ⇝ R : R
```

`⇝ R` means “coerces to the expected type R.” Matching field names are not required by this rule; order and compatible field types matter. A practical consequence: swapping two same-typed fields will type-check while changing meaning. Use a named constructor for values that are easy to reverse.

## Default values

`default` takes its type from context; `default(T)` explicitly names it where that spelling is supported by the grammar and front end. A default is a representation-level value, not evidence that a mathematical invariant holds. A zero matrix need not be invertible; a default interval need not match an application's domain convention. The interpreter constructs defaults in [Dispatch](../../../src/Plato.Interpreter/Evaluation/Dispatch.cs); generated C# uses host default behavior. Interpreter defaults include an empty String/Array, product fields recursively defaulted, and the first sum case with default payloads; function defaults can contain an internal null representation. Host collection defaults need separate parity checks. Do not treat defaults as checked constructors.

## Sealed construction and proof-like values

```plato
type CheckedSample sealed by CheckSample {
    Sample: Reading;
}
```

`sealed by` names the functions allowed to construct the type. A function is authorized when its name appears in this list **and its return type is the sealed type**. Every overload meeting those conditions is authorized. The declaration must resolve each permitted name to a suitable function.

Outside those functions, direct constructors, `new`, tuple-to-product conversion, sum cases, and `default` construction are rejected with CHK311. Reading fields, matching a sealed sum, and calling authorized functions remain legal. This can express “a value exists only after a check,” but the type checker does not prove that the named function actually performs the claimed check. The invariant is a proof obligation for its implementation.

The boundary is Plato source checking. Generated C# constructors remain public; a host can bypass the discipline. Therefore a sealed type is not a security boundary and is not a machine-checked refinement type in arbitrary C# programs.

## Evidence and related work

[SealedConstruction](../../../src/Plato.Compiler/Checking/SealedConstruction.cs) and [its tests](../../../tests/PlatoTests/Checking/SealedConstructionTests.cs) describe enforced construction paths. [Solver coercion](../../../src/Plato.Compiler/Checking/Solver.Coercion.cs) implements expected-type tuple conversion. The [Rust struct reference](https://doc.rust-lang.org/reference/items/structs.html) also distinguishes named data from tuple-shaped data; Plato's implicit tuple bridge is its own rule. Sealed construction resembles smart constructors in functional programming, with an explicit compiler-enforced allowlist of source function names.
