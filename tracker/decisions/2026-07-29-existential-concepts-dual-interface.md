---
date: 2026-07-29
title: Interfaces in type position lower to a dual interface (object-safe view + F-bounded generic)
status: accepted
superseded-by:
links: [tracker/issues/plato-311.md, tracker/issues/plato-308.md, tracker/issues/plato-310.md, submodules/Plato/Plato.CSharpWriter/CSharpTypeWriter.cs, submodules/Plato/Plato.CSharpWriter/ITypeToCSharp.cs, submodules/Plato/PlatoCompiler/Symbols/ConceptGrounding.cs, submodules/Plato/PlatoCompiler/Checking/ExistentialConceptChecker.cs]
---

## Context

Every Plato interface `C` lowers to a single F-bounded generic interface
`interface C<Self> where Self : C<Self>`. That form is exactly right when `C` is used as a
**constraint** — `Self` gets grounded by monomorphization or by an `implements`/`inherits` clause.
It has no meaning when `C` is used **in type position** — a field type, a return type, or a
parameter type of an ordinary function — because there `C` denotes an *existential* ("some type
implementing `C`, identity unknown to the reader"), and no concrete `Self` exists to plug in.

The writer had no defined semantics for that case and improvised: it substituted whatever concrete
type it happened to be writing at the time into `Self`. A field `Path: Curve3D` on `SweptSurface`
rendered as `Curve3D<SweptSurface> Path` — an unsatisfiable C# type ("a curve whose `Self` is a
surface") — because `SweptSurface` does not implement `Curve3D`; it merely stores one. This
produced 166 CS0315 errors across the 12 interface-typed fields in the forward stdlib
(`surfaces-generated.plato`, `surfaces-patches.plato`, `solids-generated.plato`), gating the
forward conformance build (plato-308) and, transitively, plato-306's affine law suite.

Rust (`impl Trait` vs. `dyn Trait`) and Swift (`some P` vs. `any P`) hit the identical fork and
both resolve it with an object-safety rule deciding what survives into the dynamic/existential
view. Plato's interfaces as constraints are already the `some` side; the `any` side had no lowering
at all.

## Decision

**Dual-interface lowering (Option A).** For every self-constrained interface `C`:

1. Compute `C`'s **object-safe surface**: the subset of `C`'s instance methods where `Self`
   appears *only* as the receiver — never in another parameter, never in the return type. A
   `_`-receiver (type-level) member is never object-safe; it isn't part of the instance view.
2. If the object-safe surface is non-empty, emit a **non-generic view interface**
   `interface C { ...object-safe members... }` alongside the existing F-bounded form, now written
   as `interface C<Self> : C where Self : C<Self>` — the F-bounded interface inherits the view, so
   nothing reachable through `C<Self>` today is lost; the view is purely additive.
3. **Self-returning members are excluded from the view, not rewritten.** A member like
   `Add(other: Self): Self` cannot be expressed non-generically without either erasing to the view
   type (losing precision — callers can no longer chain further Self-typed operations) or boxing
   awkwardly; excluding it is the simplest choice that keeps the emitted code straightforward, and
   it stays reachable through `C<Self>` for any caller that already has a grounded `Self`.
4. **Interface in constraint position still lowers to `C<Self>`, unchanged.** Interface in type
   position (a *grounded* interface, i.e. the enclosing type genuinely implements/inherits it, is
   still `C<Self>`; only an *ungrounded* — existential — reference switches to the bare `C`.
5. **An interface with an empty object-safe surface has no view at all.** Using it in type position
   is rejected with a new checker diagnostic, **CHK308**, rather than reaching the writer and
   producing more unsatisfiable C#.

### The grounded/existential test

Both the writer and the checker need the identical answer to "is this interface reference grounded
or existential?", so it lives once: `ConceptGrounding.GroundsSelf(owner, conceptDef)` in
`PlatoCompiler/Symbols/ConceptGrounding.cs`. A reference from `owner`'s point of view is grounded
when `owner IS conceptDef` (the interface's own self-reference, e.g. a `Self` return type) or
`conceptDef` is in `owner.GetAllImplementedConcepts()` (an `implements`/`inherits` clause, direct
or transitive). Anything else — most commonly a field, return type, or parameter type naming a
interface the enclosing type does not implement — is existential.

In the writer this surfaces as `ITypeToCSharp.GroundingOwner`: every writer that can render a type
(`CSharpTypeWriter`, `CSharpFunctionInfo`) now exposes the `TypeDef` it is writing on behalf of,
and `TypeToCSharpExtensions.ToCSharpType(TypeInstance)` checks `GroundsSelf` before falling back to
the old `Self`-substitution path. A grounded reference is unaffected — same F-bounded spelling as
before. An existential reference with an object-safe surface renders as the bare view name. An
existential reference with NO object-safe surface throws (a defensive fail-loud backstop — CHK308
is the intended point of rejection, before the writer is ever reached).

## Rationale

This is the one seam where Plato's structural interfaces meet a nominal target language. Giving it a
precise `some`/`any` split — instead of leaving `any` undefined — means every future backend
(GLSL, C++, Rust, CUDA writers already exist in this tree) inherits a defined question ("what is
your `any C` representation?") instead of inheriting the same silent-garbage bug plato-308 found
by accident, 1232 files into a conformance build.

Object-safety (not, say, "every member") is the right subset because it is exactly the boundary
where a non-generic view can be sound: a method that only ever consumes/produces the receiver type
needs no knowledge of which concrete type is behind the view to be called correctly. A method that
returns or accepts `Self` needs the caller to know the concrete type to use the result safely
(chaining, comparison, construction) — precisely what "some unknown type" cannot promise.

## Alternatives rejected

- **(B) Erasure to a generated `AnyC` delegate-holding struct.** Loses stored-value identity and
  inspectability (a `SweptSurface.Path` should still *be* the curve someone stored, not a bag of
  delegates over it); delegate equality is reference/weak and would silently break equality-typed
  stdlib laws.
- **(C) Making storage types generic** (`SweptSurface<TPath>`). The arity infects every consumer
  signature transitively — the vocabulary bends to the C# encoding instead of the encoding serving
  the vocabulary, which the stdlib redesign constraint explicitly rules out.
- **(D) Banning interface fields in favor of `Function1`-typed fields.** Pays the same identity loss
  as (B), and directly in the source vocabulary rather than only in the emitted encoding — worse,
  not better.

## Consequences

- **Accepted cost: boxing.** A struct implementer stored in a `C`-typed field boxes. These are
  modeling-time fields (a swept surface's path, a Coons patch's boundary curves), not hot-loop
  data — the existing monomorphized/hot paths are untouched and stay unboxed.
- **Accepted cost: binary methods invisible through the view.** `Compare(Self)`-shaped members
  never appear on a `C`-typed value; callers needing them must have (or recover) a grounded type.
- **`Pack=1` blittability was already void for these types** (delegate fields, `Function1..9`,
  already precede this change in the same structs); dual-interface lowering does not newly break
  anything that was blittable before.
- An interface author who writes an interface with **zero object-safe members** and later stores it in a
  field gets a clear CHK308 diagnostic naming the interface, not a downstream CS0315 in generated
  code — the concrete prevention mechanism plato-311 exists to add.
- `Plato.CSharpWriter` is shared between the forward and legacy (`stdlib-legacy`) recipes; the
  legacy goldens (`Generated/`) necessarily change wherever an interface-typed field/return/parameter
  existed in the legacy stdlib too — refreshed deliberately via `tools/regen-generated.ps1`'s apply
  mechanism in the same change, not left to drift silently.
