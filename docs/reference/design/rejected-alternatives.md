# Rejected alternatives

This page lists the design decisions that chose between alternatives. For each it says what was
chosen, what was turned down, and why, and it links the decision record. The record holds the
rule. If this page and a record differ, the record is right. Read a record in full before
relying on it or changing its contract.

All records are in [`tracker/decisions/`](../../../tracker/decisions/README.md). Proposals not
yet decided are in the [language improvement register](../review/language-improvements.md).

## Language and checking

**Interfaces in type position.**
Chosen: an interface used as a field, parameter, or return type lowers to a non-generic view of
its object-safe members plus the generic form. Rejected: erasing to a generated delegate-holding
struct, making storage types generic, and replacing interface fields with function-typed fields.
The first and last lose the identity of the stored value, and equality-typed library laws would
break. Making storage generic spreads a type parameter through every consumer, so the vocabulary
would bend to the encoding. Record:
[dual interface](../../../tracker/decisions/2026-07-29-existential-concepts-dual-interface.md).

**Static interface members.**
Chosen: `Zero`, `One`, `MinValue`, and `MaxValue` stay instance obligations, and static-abstract
emission is an opt-in flag for the few members that are truly type-level. Rejected: treating
every receiver-less member as type-level, which the first decision assumed. Implementing it showed
that some implementers need the receiver to know their arity. The first record is
[superseded](../../../tracker/decisions/2026-07-29-static-concept-members.md); it also turned down
a `static` keyword and dropping emitted interfaces. Authority:
[the corrected record](../../../tracker/decisions/2026-07-29-static-concept-members-corrected.md).

**Declared type-parameter bounds.**
Chosen: `where` bounds on types and library functions are both checked and emitted as real
constraints. Turned down: the previous state, where an interface could write a clause that nothing
read, so an impossible declaration failed in the C# writer and the same function was written once
per type. Record:
[bounds are verified and emitted](../../../tracker/decisions/2026-08-01-declared-type-parameter-bounds-are-verified-and-emitted.md).

**Sealed construction.**
Chosen: a `sealed by` clause names the only functions that may construct a type, and the checker
reports a diagnostic elsewhere. Rejected: private constructors with a visibility modifier, because
SEMANTICS.md excludes visibility on purpose, and halting compilation, because every other checker
error is reported rather than halting. Existing syntax (a `where` form, a naming convention, a
comment) was weighed first and each failed. Record:
[sealed by names a type's constructors](../../../tracker/decisions/2026-10-22-sealed-by-names-a-types-constructors.md).

**Generic sums.**
Chosen: generic sums instantiate at each use, as generic products do, with `$T` marking a free
variable in a library signature. Rejected: auto-generalizing a bare parameter name, one emitted
struct per instantiation, and keeping the restriction with a handwritten sum per payload type.
Each would add a rule no other generic code follows or duplicate what the C# runtime already
does. Record:
[generic sums monomorphize per use](../../../tracker/decisions/2026-10-23-generic-sums-monomorphize-per-use.md).

**Evaluation order.**
Chosen: a program may mix eager, on-demand, and deferred evaluation, and may omit unnecessary pure
work, while required results and required validation are preserved. Rejected: identical eager
evaluation everywhere as the universal target, making all calls and arrays lazy, and ignoring all
faults. Eager evaluation stays the baseline until a transformation is migrated and validated.
Record:
[mixed evaluation preserves required results](../../../tracker/decisions/2026-10-25-mixed-evaluation-preserves-required-results.md).

## Standard library

**Polyhedral duals.**
Chosen: duals are total functions between the existing kind sums. Rejected: a dual interface on
per-solid types, because there are no per-solid types to attach it to, and leaving the pairing to
meshes, which makes it uncheckable. Record:
[duals as kind-level maps](../../../tracker/decisions/2026-07-29-polyhedra-dual-kind-map.md).

**Spatial structures.**
Chosen: trees and sorting are written in Plato as ordinary library bodies, with backends
recovering native speed through the override table. Rejected: having the host construct and Plato
only query, which multiplies the work per backend, and making sort a bodiless intrinsic, which
the admission rule for intrinsics forbids when a portable body exists. Record:
[construction belongs in Plato](../../../tracker/decisions/2026-08-04-spatial-structure-construction-in-plato.md).

## C# emission

**Scalar representation.**
Chosen: wrapper scalars are the only representation. Rejected: keeping scalar erasure behind a
flag, erasing only at buffer boundaries, and erasing for the optimized recipe only. Under erasure,
distinct quantities collapse to one host type, nothing can be attached to a host primitive, and
the C# compiler can no longer re-check the type discipline independently. Reinterpreting at a
buffer boundary needs no emitter mode. Record:
[wrapper scalars are the only representation](../../../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md).

**Properties versus methods.**
Chosen: emitted no-argument members are methods, unconditionally. Turned down: a flag that
selected between properties and methods. The property form was a second output shape nobody
consumed, and the flag was tangled with scalar erasure. Record:
[property-free emission](../../../tracker/decisions/2026-08-01-property-free-emission-is-unconditional.md).

**Conversions out of single-field types.**
Chosen: unwrapping a one-field type is an implicit conversion, and wrapping is a constructor call
or a conversion declared in Plato. Rejected: the writer's implicit wrap, which silently erased
unit and index discipline, and an explicit operator, which would still be a nameless door and
would clash with declared conversions. Record:
[the mirror unwraps only](../../../tracker/decisions/2026-08-02-single-field-mirror-unwraps-only.md).

## Repository

**The frozen V1 runtime.**
Chosen: delete the frozen first runtime and drop the version suffix from the survivor. Turned down:
carrying a frozen second copy, which cost a cross-repository checksum gate and a suffix on every
live name. Record:
[retire the V1 runtime](../../../tracker/decisions/2026-07-31-retire-v1-runtime-and-freeze.md).

**Legacy conformance suite and golden diffs.**
Chosen: retire both and promote the forward conformance suite. Rejected: repairing both, and
keeping the legacy suite until the forward one is green, because repairing it buys coverage of the
library being replaced. Record:
[retire the legacy suite and goldens](../../../tracker/decisions/2026-07-30-retire-legacy-conformance-and-goldens.md).
