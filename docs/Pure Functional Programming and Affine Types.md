# Pure Functional Programming and Affine Types in Plato

Pure functional programming is sometimes perceived being too complex, or inefficient. 

Truth is that it can be both simple and efficient, especially when used in a domain specific language. 

Plato demonstrates that pure functional programming when applied to a constrained domain 
- such as geometry and numerical computing - can be both efficient and simple.

## Why Pure Functional Code Matters 

The following are some of the most compelling benefits of pure funtional code for both human developers and agentic AI. 

**Testing is simplified.** - A pure function needs no database, no mock server, no setup. Give it inputs, check the output. That's the entire test. Once a pure function has been validated correct for a set of inputs - it will always produce the same result.

**Code is easier to understand.** - With pure functions, everything a function can do is in its inputs and outputs. You don't have to hold the whole program in your head to know a piece is correct. 

**Concurrency is trivial.** - Most multi-threading bugs come from two things touching the same mutable data at once. If nothing is mutable and nothing is shared, that entire category of bug disappears.

**Caching is free and safe.** - When the same input always gives the same output, you can remember past results and skip the work. 

**Refactoring is low-risk.** - Pure functions have no hidden wires to the rest of the program. You can move them, rename them, and recombine them without spooky action at a distance.

## What "pure" actually means

A function is **pure** when:

1. **Its output depends only on its inputs.** Same inputs → same output, every single time.
2. **It has no side effects.** Calling it doesn't change anything the outside world can notice.

Example of a *side effect* include: writing to a file, printing to the screen, updating a global variable, mutating a list someone else is holding, sending a network request, reading the current time. 

These two interfaces are called:

1. Referential Transparency
2. Immutability

## Referential Transparency

**Referential transparency** — the fancy name for a simple superpower: you can replace any call with its result and the program means exactly the same thing. Since `add(2, 3): Integer` is always `5`, you can swap `5` in wherever it appears. 
You can't do that with `getNextId(): Id`, because it *does* something.

## Immutability 

In pure code you don't change data; you produce new data. Instead of pushing onto a list:

```
list.push(x)          // mutates the list everyone shares
```

you build a new list and leave the original alone:

```
list = list.push(x)
```

The problem with this, is that it can become inefficient if done naively. 

An elegant solution to the problem exists called affine types.  

## Affine Types 

An *affine type* is a type whose values may be used **at most once**. Once a value
has been consumed — passed to a function, bound to another name, stored in a
structure — it cannot be referenced again.

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



That's it. Pure functional programming is building software mostly out of functions like `add` and mostly avoiding functions like `getNextId`.


## The honest tradeoffs

Purity is not free, and anyone selling it as pure upside is selling something.

**The real world is one giant side effect.** Files, screens, networks, databases, sensors — the entire point of most software is to touch these. A purely pure program that never affects anything is a very elegant space heater. So pure languages have to answer: where do the effects go?

The usual answer is **"functional core, imperative shell."** You keep the decision-making logic pure — the part with all the tricky rules — and push the messy effectful stuff (I/O, the outside world) to a thin layer at the edges. The core is where bugs hide and where testing pays off, so that's the part you make pure. Some languages go further and *track* effects in the type system (monads in Haskell, effect systems elsewhere), which is powerful but has a real learning curve.

**Naive immutability can be slow.** If you copy a whole list every time you add an item, you'll drown in allocations. Making immutable code fast requires the compiler to be clever — sharing structure behind the scenes, or proving a value is used only once so it can be updated in place instead of copied. (Plato uses **affine types** for exactly this; that's its own topic.)

## Why Plato is pure functional

Plato is a pure, strict functional language. Concretely that means: values are immutable, there are no effectful primitives baked into the language, and everything you write is an *expression* that produces a value rather than a *statement* that performs an action.

Why build it that way? Because of what Plato is *for*: **math and geometry.** Think about what a geometry operation actually is — "take this shape and bevel its edges," "rotate this mesh," "subtract this solid from that one." Every one of those is naturally a pure function: a shape goes in, a new shape comes out, and nothing hidden changes. There is no good reason for "rotate a cube" to also mutate some global counter. The domain is *already* pure; Plato just declines to pretend otherwise.

That purity buys Plato things that matter for its job:

- **The same source means the same thing everywhere.** Plato compiles to several very different targets. Because a pure function depends on nothing but its inputs — no platform globals, no hidden runtime state — the meaning survives the trip from one target to another.
- **Results are reproducible.** The same model, evaluated twice, is identical. For CAD, simulation, and rendering, that determinism is a feature, not a nicety.
- **Fast where it counts.** Affine types let the compiler turn "make a fresh copy" into "update in place" when it's provably safe, so you write clean value-semantics code and still get performance.

Plato is *not* total — you can write a loop that runs forever, and it supports general recursion. It's a real, Turing-complete language, not a restricted calculator. Purity comes from having no effectful primitives and immutable values, not from forbidding loops.

## Pure functional in the age of agentic AI

Here's the part that's genuinely in motion right now. When AI agents write and modify a growing share of our code, does purity matter more, or less?

**A case for *more* relevant:**

- **Machine-checkable is the whole game.** A pure function can be understood, tested, and verified *in isolation* — you don't have to simulate the entire program's state to know what it does. That's exactly the kind of local, bounded reasoning both an AI agent and its human reviewer can actually do reliably. Smaller blast radius per change.
- **Safe to just run.** Pure code with no side effects can be executed in a sandbox to check whether it's correct, without risking your files, your network, or your data. When a machine is generating code you haven't read yet, "running it can't hurt anything" is worth a lot.
- **Generate-and-verify loops love determinism.** Agents work by trying something, checking it, and trying again. Same-input-same-output makes that check cheap and trustworthy, and makes property-based testing and near-formal verification practical.

**A case for *less* relevant:**

- **One of purity's classic selling points was human ergonomics** — keeping a program simple enough to fit in one person's head. Machines are, frankly, better than we are at tracking sprawling mutable state across a big codebase. To the extent that was the benefit, it weakens.
- **The work agents do is drenched in effects.** Calling APIs, editing files, running tools — an agent's real job is almost entirely side effects. A strictly pure language can feel mismatched with that world if it doesn't have a clean story for the messy edges.
- **Training data favors the incumbents.** Models are most fluent in the languages they've seen most — JavaScript, Python, Java — all imperative. A niche pure language simply gets weaker AI assistance today, because there are fewer examples to have learned from.

**Where this nets out.** Purity's center of gravity is shifting — away from "helps a human hold it in their head" and toward "gives a machine a substrate it can verify, cache, sandbox, and transform safely." For a domain language like Plato, aimed at deterministic geometry and math, that shift arguably makes purity *more* valuable, not less: the code an agent generates is exactly the code you want to be reproducible and independently checkable. The honest catch is tooling and familiarity, which still favor the mainstream impure languages — and will until the examples catch up.

## Takeaway

Pure functional programming isn't an academic affectation. It's a single, practical bet: **remove hidden surprises so that code does only what its inputs say it does.** That makes software easier for a person to trust — and, increasingly, easier for a machine to verify. Plato makes that bet because geometry is already pure at heart, and because in a world where more code is written by agents, "you can check this in isolation" is turning from a nicety into the main event.

---

Save to `docs/plato-pure-functional-programming.md`? Or want edits first — tone, length, the AI section?