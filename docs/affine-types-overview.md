# Affine Types in Plato

## What an affine type is

An *affine type* is a type whose values may be used **at most once**. Once a value
has been consumed — passed to a function, bound to another name, stored in a
structure — it cannot be referenced again.

Affine typing belongs to the family of **substructural type systems**, which are
classified by which structural rules they permit:

| Discipline   | Duplicate a value? | Discard a value? | Net usage         |
|--------------|:------------------:|:----------------:|:-----------------:|
| Unrestricted | yes                | yes              | any number of times |
| **Affine**   | no                 | yes              | at most once      |
| Relevant     | yes                | no               | at least once     |
| Linear       | no                 | no               | exactly once      |

Affine is the rule that forbids duplication but still permits discarding. That
single restriction — *no aliasing, no reuse* — is what makes the discipline
useful.

## Why affine types matter

**They make in-place mutation safe inside a pure language.** If the type system
can prove a value has exactly one live reference, the compiler is free to
overwrite that value's storage rather than allocate a fresh copy. No other part
of the program can observe the difference, because no other part holds a
reference. Destructive update happens in the machine while the source language
stays referentially transparent. For an immutable, value-oriented language, this
is the mechanism that turns "copy on every operation" into "update in place"
without weakening the semantics.

**They model resources.** File handles, buffers, sockets, and GPU resources have
a lifecycle: acquired once, released once, never used after release. An affine
type encodes "use-after-free" and "double-free" as *type errors*, caught at
compile time rather than at runtime.

**They make ownership explicit.** Reading a signature tells you whether a
function consumes its argument or merely inspects it. Ownership stops being a
comment and becomes part of the interface.

## How other languages use them

**Rust** is the mainstream example. Its ownership model is affine: a non-`Copy`
value can be *moved* — used once — after which the original binding is invalid.
Values may also be dropped without use, which is exactly what makes Rust affine
rather than linear. On top of this affine core Rust layers *borrowing* (`&T`,
`&mut T`), a region system that grants temporary access without transferring
ownership, under the rule "aliasing XOR mutation." Most of what programmers call
"the borrow checker" is this affine foundation plus borrows.

**Clean** uses *uniqueness types*, the close dual of affine types. A value typed
as unique is guaranteed to have a single reference, which Clean exploits for
in-place array update and for pure I/O.

**Haskell** (GHC `LinearTypes`) and **Idris 2** (Quantitative Type Theory, with
multiplicities 0/1/ω) sit on the *linear* side — exactly-once rather than
at-most-once — but solve the same class of problems.

**Swift** added ownership control with `borrowing` / `consuming` parameters and
non-copyable (`~Copyable`) types, giving move-only, affine-style values in an
otherwise unrestricted language.

**C++** move semantics (rvalue references) are the unenforced cousin: a
moved-from object is used once by convention, but the type system does not
prevent touching it afterward, so the guarantee is a discipline, not a proof.

## Their role in Plato

Plato is a pure, strict functional language: values are immutable and there are
no effectful primitives. Affine types fit this model without compromising it.
They give Plato two things purity alone does not:

1. **Performance parity with mutable code.** Where a value is provably used at
   most once, the compiler can lower an immutable operation to an in-place
   update. The programmer writes value semantics; the machine executes mutation
   semantics; referential transparency is preserved throughout.
2. **Single-use guarantees in the type system.** Consumption is expressed in
   signatures, so misuse of a one-shot value is a type error rather than a latent
   bug.

Crucially, affine typing is **orthogonal to purity**. It restricts *how many
times* a value is referenced, not *what effects* an expression may have. Adding
it takes nothing away from Plato's purity guarantee — it makes that guarantee
affordable.
