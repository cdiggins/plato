---
id: plato-394
title: A function bound on a receiver type's own parameter emits an unsatisfiable C# constraint
type: problem
status: idea
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-01
closed:
links: [writers/Plato.CSharpWriter/CSharpFunctionInfo.cs, src/Plato.Compiler/Checking/TirEmitSource.cs, plato-393]
---

## Issue

`plato-393` gave library functions a `where` clause on their own signature variables. There is one
shape where the clause is CHECKED but cannot be EMITTED, and the writer does not notice:

```plato
type Bag<T> { Item: T; }                       // unbounded, deliberately

library Ops
{
    Mix(xs: Bag<$T>, t: Number): $T where $T: Interpolatable => xs.Item.Lerp(xs.Item, t);
}
```

In extension style a library function whose FIRST parameter reads exactly `Owner<a1..an>` is
emitted as a member of that struct, and `CSharpFunctionInfo.RebindReceiverTypeVariables` folds the
function's `$T` into the struct's own parameter `T` — the variable stops being the method's
generic. `InheritedConstraints` then (correctly, for an inherited bound) drops the clause, on the
grounds that the STRUCT carries it. For a bound the FUNCTION declared, the struct carries nothing:
`Bag<T>` is unbounded. The result is a real body calling `Lerp` on an unconstrained `T`, which does
not compile.

`TirEmitSource.IsOpenGenericEmittable` licenses the body because the declared bound now reaches it
through `TypeConstraints.InheritedBounds` — the licence is right at the language level and wrong at
the C# level, because the emitted signature is missing the clause the licence assumed.

## Impact

Zero today, and not on any shipping path:

- The forward stdlib's only function bound is `DeCasteljau(xs: Array<$T>, ...)` in
  `stdlib/geometry/splines-bezier.library.plato`. `Array` is a `primitive`, not an emitted struct,
  so no rebind happens — and `geometry` is not an emitted tier at all.
- The foundation tier (the one project in `generated/`) carries no function bound; its regeneration
  after plato-393 was timestamp-only.

So this is a trap for the next author, not a live defect: write a function bound over a
user-defined generic concrete receiver in an EMITTED tier and you get C# that does not compile,
with nothing between you and the compiler error.

## Fix approaches

1. **Reject it at declaration time.** A function bound whose variable is (only) a type argument of
   a constructed concrete type in receiver position must be declared on THAT TYPE instead — the
   author's real intent, and the thing that makes every use site honest. A CHK diagnostic saying
   exactly that. Smallest surface, and it never produces bad C#.
2. **Refuse the body instead.** Teach `IsOpenGenericEmittable` (or the writer) that a declared
   function bound only licenses while the variable is still the METHOD's own generic, so the body
   degrades to a throwing stub as it did before. Honest, but silent — the author gets a runtime
   throw rather than an explanation.
3. **Emit the clause on the struct.** Wrong: it would change the type globally, for every mention
   of `Bag<T>`, on the say-so of one member.

Option 1 is the one that matches `plato-382`'s bedrock — a declaration may not promise what the
type system cannot check.

## Done means

- [ ] A function bound that cannot reach the emitted signature is either rejected at declaration
      time or refused at emission, with a test covering the `Bag<$T>` shape above.
