---
id: plato-366
title: Derive Wrapper<T> obligations for single-field types (retire the 98-line quantity projection file)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-350, plato-277, plato-306, submodules/Plato/stdlib/foundation/quantities-projections.library.plato, submodules/Plato/stdlib/foundation/quantities.concepts.plato, submodules/Plato/stdlib/foundation/numeric-structures-components.library.plato]
---

## Idea
`stdlib/foundation/quantities-projections.library.plato` is 236 lines containing 98 bodies
and zero decisions: two mechanical lines per concrete quantity type, one reading the sole
field and one calling the auto-generated constructor.

```plato
Amount(x: Length): Number => x.Meters;
FromAmount(_: Length, x: Number): Length => new Length(x);
```

Both halves are derivable from the type's shape. Introduce a first-class
single-field-wrapper (newtype) interface and have the binder satisfy its two obligations
automatically for any type with exactly one field:

```plato
interface Wrapper<T> { Unwrap(x: Self): T; Wrap(_: Self, x: T): Self; }
interface Quantity inherits Value, Comparable, Hashable, Additive, Scalable, Interpolatable, Wrapper<Number> { }
```

`Amount`/`FromAmount` then either retire in favour of `Unwrap`/`Wrap`, or stay as two
generic one-liners in `quantities.library.plato`, and the whole projection file deletes.

Measured shape of the problem (2026-07-30, `stdlib/`):
- **49** types declared exactly `type X implements ... { Field: Number; }`
- **50** of the tree's **97** constructor-shaped `Name(_: T, ...)` fills live in this one file
- the remaining 47 include the 7 per-type `FromComponents` fills in
  `numeric-structures-components.library.plato` — the same pattern at arity > 1

## Assumptions
- The reading half is already free when names match: `x.Radians` resolves because `Radians`
  is a field. The obligation is only unmet because the field is named per-unit, not `Amount`.
- The constructing half is unmet because Plato has no generic `new Self(x)` inside a
  interface-generic body (`PlatoCompiler/Symbols/Definitions.cs` `SelfType`; no construction
  path over `SelfType` exists).
- The per-type auto-constructor (`new Length(x)`) already exists in the emitters, so
  synthesis has a target to bind to — this is a binder/obligation-satisfaction change,
  not a new code path in the writers.
- Unit-named fields (`Meters`, `Radians`, `Kilograms`) are load-bearing documentation and
  must survive whatever lands.

## Design decisions
- **Trigger** — implicit (any type with exactly one field) vs explicit (`type Length
  implements Quantity derives Wrapper<Number> { ... }`). Implicit deletes more lines and is
  unambiguous at arity 1; explicit keeps the type readable about what it gained and matches
  the `derives` direction floated in [plato-350](plato-350.md).
- **Naming** — retire `Amount`/`FromAmount` for `Unwrap`/`Wrap`, or keep the domain names as
  two generic aliases over `Wrapper<Number>`. Retiring is cleaner; keeping preserves the
  reading of `x.Amount` in ~20 downstream generic bodies in `quantities.library.plato`.
- **Scope of `T`** — `Wrapper<Number>` only, or any single field type (`Wrapper<Integer>`,
  `Wrapper<Array<Number>>`)? The general form subsumes part of [plato-350](plato-350.md);
  the narrow form is a one-week change.
- **Relation to generic `new Self(x)`** — deriving `Wrap` is the narrow special case.
  Solving generic Self-construction instead would additionally kill the 7 `FromComponents`
  fills and every future constructor-shaped fill (97 total), at the cost of Self-constructor
  arity checking in the solver. Narrow first vs general first is the real fork.

## Related
- [plato-350](plato-350.md) — same question for `Indexable`: derive `At`/`Count` from a
  single collection field. Sibling, not duplicate: that one is about collection shape, this
  one about scalar newtypes. If a `derives` keyword lands, both should use it.
- [plato-277](plato-277.md) — stdlib interface-gap burn-down; this is one concrete gap with a
  measured line count.
- [plato-306](plato-306.md) — generic `Difference` defaults via an optional conversion
  interface; same technique (add an interface so obligations become generic one-liners), already
  in-progress, so its outcome is evidence for or against this approach.
- `stdlib/foundation/quantities.library.plato` — the 10 generic obligations already written
  once against `Amount`/`FromAmount`; the prior art proving the pair is the right primitive.
- `stdlib/foundation/vectors.concepts.plato` — `FromComponents(_: Self, xs)` is the arity-N
  form of the same ignored-receiver constructor idiom.

## Approaches
Short term: narrow `Wrapper<Number>`, implicit trigger at exactly one field, `Amount`/
`FromAmount` kept as two generic aliases. Deletes `quantities-projections.library.plato`
whole; no downstream call sites change.

Long term: generalize to `Wrapper<T>` for any sole field, which subsumes the single-Array
half of [plato-350](plato-350.md); or go past it to generic `new Self(...)` and retire the
ignored-receiver constructor idiom tree-wide.

Adjacent ideas worth their own issue:
- Fieldwise `Hashable`/`Equatable` defaults (already noted as adjacent on plato-350).
- Lint rule flagging a library file whose bodies are all mechanical projections, so the next
  instance of this pattern is caught at authoring time rather than at 236 lines.

## Bedrock
Strengthens the **obligation-satisfaction seam** in the binder
(`PlatoCompiler/Symbols/` + `ConceptGrounding.cs`): today an obligation is met only by an
explicitly written function or a name-matching field, so every structurally-derivable
member costs one hand-written line per type. Adding a derivation rule makes "the shape of
the type satisfies the interface" expressible, which is what both this and
[plato-350](plato-350.md) need. Cheaper afterwards: every future newtype (`Ratio`,
`Probability`, `Strain`) joins `Quantity` for free instead of adding two lines to a growing
mechanical file.

Verdict: **simplest-along-the-grain**.

The simple version must NOT: trigger on multi-field types (that is plato-350's harder
question, and wrong field order there is worse than the boilerplate), and must NOT hard-code
`Number` inside the binder rule — the rule reads the sole field's declared type so widening
to `Wrapper<T>` later is a validation change, not a rewrite.

## Done means
- [ ] `stdlib/foundation/quantities-projections.library.plato` deleted
- [ ] All 49 single-`Number`-field quantity types satisfy `Quantity` with zero per-type lines
- [ ] `check-stdlib-fast.ps1` lint + checker ratchet no worse than the pre-change baseline
- [ ] Forward-conformance stage 1 still reports 0 symbol resolution errors
- [ ] Derivation rule (trigger, opt-out, what it does NOT cover) written down in
      `stdlib/CONVENTIONS.md`

## Simplest possible implementation
Binder rule: when a type has exactly one field and implements an interface declaring
`Unwrap(x: Self): T` / `Wrap(_: Self, x: T): Self` where `T` equals the sole field's type,
synthesize both fills — `Unwrap` as the field read, `Wrap` as a call to the existing
auto-constructor. Add `Wrapper<Number>` to `interface Quantity`'s inherits list, keep
`Amount`/`FromAmount` as two generic one-liners in `quantities.library.plato`, delete the
projection file.

- Pros: −236 lines and −50 of 97 constructor-shaped fills; every future quantity type is one
  line total; no downstream call site changes; no writer changes.
- Cons/risks: a type gains members not visible in its declaration (the readability objection
  raised on plato-350); an accidentally single-field type could silently satisfy an interface it
  did not mean to; the 47 remaining constructor-shaped fills are untouched, so the idiom
  survives and the tree now has two ways to meet the same kind of obligation.

## Case against
- Boilerplate here is dumb but honest, and `git`-visible: 98 lines that anyone can read and
  verify. Synthesis moves that correctness argument into the compiler.
- The bigger prize is generic `new Self(x)` (97 fills, not 50). Doing the narrow version
  first may make the general one harder to justify later.
- Verdict: **pursue** narrow `Wrapper<Number>`, but decide the generic-`new Self` fork first
  — if that is on the roadmap within a quarter, skip this and do it once.
