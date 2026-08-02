---
id: plato-362
title: Type-token members - static fills satisfy instance obligations, both access surfaces
type: feature
status: done
priority: p1
effort: M
risk: medium
area: plato
created: 2026-07-30
closed: 2026-07-30
links: [plato-312, plato-308]
---

Executed the "D" design for type-level constants (`Zero`, `One`, `MinValue`, `MaxValue`, ...):
a member should be reachable BOTH as `Type.Member` and off a value (`x.Member`), and a
`_`-receiver (type-token) fill must be able to discharge an instance interface obligation.

## Design decisions

- Interfaces KEEP instance-form declarations (`Zero(x: Self)`). Static-abstract lowering is
  impossible for dynamic-arity types (VectorN/MatrixN read their arity from the receiver), so
  the interface obligation stays value-dispatched and each fill chooses its form per type.
- A `_` fill on a fixed-arity concrete type emits as the PAIR
  `public static T Zero()` + explicit interface impl `T Additive<T>.Zero() => Zero();`
  (CSharpConcreteTypeWriter, new case in the explicit-impl loop). Kills the CS0736 class
  structurally; both surfaces verified to coexist and dispatch correctly in C# (static +
  same-name extension + explicit interface impl + interface-value dispatch).
- Generic fills (Vector via Broadcast, MatrixN, Quantity) stay instance-form - a type-token
  constant is meaningless when the shape lives in the value.
- LINT012 updated: `_` fill vs instance obligation is now LEGAL (the writer bridges it);
  only the reverse (type-level obligation, instance fill) is still flagged.

## Shipped

- stdlib: `Zero/One(_: Number)` and `Zero/One/MinValue/MaxValue(_: Integer)` intrinsic
  declarations; Color/Complex/Proportion/Percent/Probability (4 members each) and the
  fixed-size matrix Zero fills flipped to `_` (matrix bodies now literal tuples, no longer
  bounce off receiver rows).
- Plato.Intrinsics.V2 + V3: Integer gains static `MinValue/MaxValue/Zero/One` (fields,
  matching Number's parenless-emission contract).
- Forward conformance build: 91 -> 85 errors (canonical recipe, A-B measured via
  stash/unstash); the entire Zero/One/MinValue/MaxValue missing-member class is gone.
  Stage-1 lint + checker ratchet green; legacy goldens contain zero fingerprints from this
  change (existing drift is pre-existing, see plato-363).

## Follow-up (not done here)

C#-consumer instance-sugar extensions for generated statics
(`Zero(this Color x) => Color.Zero();`) - Plato callers already get the value-receiver
surface via TryWriteTypeLevelCall, so this is C#-API polish only.
