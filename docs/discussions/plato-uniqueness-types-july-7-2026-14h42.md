
Discussion with Fable 5 Max Effort on July 7, 2026 at 14:42

```
I was thinking of introducing controlled mutation into the language in the form of uniqueness types: certain types that can't be shared, but can be updated within a function, or transferred to another function. Really I just want a mutable "List" type that can't be shared, but can eventually be promoted to a regular immutable array, so mutation is carefully controlled. What are some options to do this, and is it easy to implement? Would you suggest I need to implement a type checker for Plato first?
```

Great question to be asking now, because the feature you're describing is both smaller than its academic name suggests and unusually easy in Plato specifically. Short version up front: what you want is **affine types, not full Clean-style uniqueness typing**; there are three viable implementation levels; and no — you don't need the type checker first, partly because your compiler already contains most of one internally.

## Why this is easier in Plato than in Rust or Clean

Rust's complexity comes from borrowing, and borrowing exists because references exist. Plato has no references: the only ways a value can be shared are variable binding, argument passing, lambda capture, and storage in a field. Close those four doors for one family of types and you have uniqueness — no lifetimes, no borrow checker, no alias analysis. Clean's complexity comes from *uniqueness polymorphism* (functions generic over unique/shared, unique containers of unique things). You don't need that either: you want exactly one nominal type, `List<T>`, with one exit (`Freeze: IArray<T>`). The general problem is hard; the closed, single-type version is a small feature.

The other framing worth keeping: uniqueness *preserves* purity rather than compromising it. If nobody else can observe the buffer, in-place mutation is referentially transparent — the language stays pure in its observable semantics, and mutation becomes an implementation detail. That's the Clean insight, and it means this doesn't fork Plato's identity.

## The options, cheapest first

**1. Runtime-checked builder (Clojure's transients).** Ship `List<T>` as an intrinsic with `Add`, `AddRange`, indexed write, `Count`, and `Freeze`. The C# implementation keeps a `frozen` flag; `Freeze` hands the buffer to an `IArray<T>` wrapper *without copying* and invalidates the builder; any later use throws. This is exactly `ImmutableArray<T>.Builder.MoveToImmutable()` in .NET, so the target gives you the primitive for free. Cost: a day or two. Guarantee: use-after-freeze is caught at runtime; aliasing before freeze is not caught at all — two variables bound to the same builder will both see mutations, silently. That's the purity leak, and it's why this is a first step, not the destination.

A cute strengthening: emit the builder as a C# `ref struct`. The C# compiler then rejects storing it in fields, capturing it in lambdas, or boxing it — your escape discipline, enforced by the *target's* type system before Plato has its own checker. The errors are ugly and point at generated code, but they're errors. (It constrains your codegen patterns — ref structs can't cross lambda boundaries — which is arguably the restriction you wanted anyway.)

**2. Affine checking by occurrence counting — the sweet spot.** The static rules, given Plato's value semantics, reduce to something almost syntactic:

- `List<T>` may not be a field type, a generic type argument (`IArray<List<T>>` is banned), or captured by a lambda. These are structural scans of declarations and annotations.
- A variable of unique type may occur **at most once per control-flow path**: once per branch of a conditional; in loop bodies, uses must follow the reassignment idiom (`xs = xs.Add(p)`) or expression chaining, which re-binds per iteration. This is occurrence counting on the AST, not dataflow analysis.
- Functions may take and return unique values freely — that's your "transfer." A callee that mutates must return the builder (threading), which chaining makes painless: `new List<Point3D>().AddRange(pts).Add(p).Freeze`.
- `Freeze` consumes. After it, the variable is dead — the same occurrence rule covers it, and the zero-copy buffer handoff is now *provably* safe rather than conventionally safe.

Choose **affine** (at most once) over **linear** (exactly once): dropping a builder on the floor is fine, the GC has it, and you avoid needing destructors or mandatory consumption. One more simplification: require unique locals to be explicitly annotated (`xs: List<Point3D> = ...`, or a `var!` marker) rather than inferred. Then the checker doesn't need expression typing at all to know *which* variables to count.

**3. Keep the language pure; make the optimizer do it (Koka/Lean's "functional but in-place").** Surface stays `xs.Append(x)`; the compiler detects linear-use chains and compiles them to mutation. Elegant, zero new semantics — but it's real analysis work, performance becomes non-transparent (did the optimization fire?), and for geometry kernels you want predictable allocation behavior. I'd skip it as the mechanism, but note that option 2 quietly enables it later: `Freeze`-discipline code *is* the specification of what such an optimizer would be allowed to do automatically.

**Prior art worth reading before you commit:** Futhark is the closest relative — a pure array DSL that added in-place updates via simplified Clean-style uniqueness (`*[]T` parameters, consumed on use); their docs are candid about which simplifications held up. Clojure's transients validate the runtime-checked promote pattern. Austral's spec is the best modern writeup of "linear types with a deliberately dumb checker." Swift's new noncopyable types (`~Copyable`, `consuming` parameters) are affine types shipping in a mainstream language, and Rust's `Vec` → `into_boxed_slice` is your `Freeze`. You're in well-charted territory.

## Do you need the type checker first?

No — and the reason is worth stating precisely. "Plato has no type checker" is true of *user-facing diagnostics*, but the compiler manifestly performs type-directed overload resolution internally — the generated code picks among overloads, inserts casts, and comments `// AMBIGUOUS FUNCTIONS` when it can't decide. That machinery already computes the fact the affine pass needs ("is this expression's type unique?"). So the affine checker is a **consumer of typing infrastructure you already have**, not a client of one you haven't built.

The realistic sequence:

1. **Now:** ship the runtime-checked `List<T>` intrinsic (option 1), document the affine rules as conventions, and port two or three real algorithms — polygon clipping, marching-cubes-style mesh emission, anything append-heavy that `MapRange`/`Reduce` can't express. This validates the API before any rule is frozen. Consider a fixed-size `Buffer<T>` sibling (allocate n, write by index, freeze) — a lot of geometry code fills known-size arrays in non-sequential order, and that's a different shape than a growable list.
2. **Next:** implement the affine pass (option 2) over the existing resolver — structural bans plus occurrence counting, order of a few hundred lines given your symbol tables. Runtime checks stay on as the backstop.
3. **Eventually:** when you build the real type checker (still the top roadmap item for diagnostics, agents, and ambiguity-as-error), the affine pass folds in as one more judgment. Building it second, against a working feature, will also force the factoring-out of the internal resolver that the full checker needs anyway — so the work compounds rather than being throwaway.

Two fences I'd plant deliberately, because they're where the complexity explosion lives: no unique *fields* (builders never live inside `type`s), and no uniqueness *polymorphism* (nothing is generic over unique-vs-shared). Plato's whole appeal is a three-construct language; this feature should cost one intrinsic type and one checking pass, not a fourth construct.

One happy side effect for the roadmap: affine `List<T>` makes the planned **Rust backend easier**, not harder. Today's all-shared value semantics forces a Rust backend to choose between cloning and borrowing everywhere; unique builders translate one-to-one into owned `Vec<T>` with moves — the exact idiom Rust wants. (GLSL simply won't get builders, which is correct — no dynamic allocation there anyway.)
