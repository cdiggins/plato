# Plato

**Write geometry once. Run it everywhere.**

**Plato** is a small, pure, statically-typed language for writing geometric and numeric libraries once, and compiling them into fast, idiomatic code for other platforms. The C# backend is in daily production use — it generates the geometry library consumed by the [Ara 3D SDK](https://github.com/ara3d/ara3d-sdk) — and the **TypeScript and Rust backends are working proofs of concept**, each with a browser demo you can try right now.

### ▶ [Try the live demos](https://cdiggins.github.io/plato/)

Twelve geometry algorithms — Delaunay triangulation, convex hulls, BVHs, half-edge meshes, raycasting, and more — written once in Plato and running in your browser two different ways, from the same source file:

- **[TypeScript sample browser](https://cdiggins.github.io/plato/typescript/)** — Plato compiled to TypeScript, rendered with Three.js.
- **[Rust sample browser](https://cdiggins.github.io/plato/rust/)** — the same Plato source compiled to Rust, built to WebAssembly, computing every sample live in the browser.

Both pass the same conformance suite: identical algorithms, identical seeds, identical results. To run them locally, see [Demos](#demos) below.

## What it is

Plato looks like a typed scripting language and compiles like a systems language. Everything is an immutable value, every function is pure, and a compiler amplifies terse declarations into a large, monomorphized, zero-dispatch library: the entire Plato standard library is ~3,500 lines of source, which becomes over 11,000 lines of C#. A three-field vector type comes out the other side as a struct with ~340 members.

What that buys you:

- **One source of truth.** Your math library exists once. Every port is generated, so the ports can't drift.
- **Native-feeling output.** C# gets packed structs and aggressive inlining; TypeScript gets fluent methods on plain `number`s; Rust gets extension traits. Each target reads like it was written by hand, by someone who likes that language.
- **Performance by construction.** Interface-generic code is monomorphized into direct calls — no boxing, no virtual dispatch, no runtime abstraction penalty.
- **Correctness by construction.** Distinct types for `Angle` vs. `Number` and `Point` vs. `Vector` turn geometry's classic bugs into compile errors; purity makes every function a trivial unit-test target.

## A taste of it

This is real code from the standard library — a circle, defined as data plus functions:

```plato
type Circle
    implements IAngularCurve2D, IClosedCurve2D
{
    Center: Point2D;
    Radius: Number;
}

library AngularCurves2D
{
    UnitCircle(t: Angle): Point2D
        => (t.Cos, t.Sin);

    Circle(t: Angle, center: Point2D, radius: Number): Point2D
        => t.UnitCircle * radius + center;

    Eval(curve: Circle, t: Angle): Point2D
        => t.Circle(curve.Center, curve.Radius);
}
```

From those few lines, `Circle` gets a constructor, tuple conversions, deconstruction, equality, hashing, `ToString`, and immutable `WithCenter`/`WithRadius` setters — and because it implements `ICurve2D`, it can be sampled, converted to a polyline, and composed with everything else written against curves, none of which was written for `Circle` specifically.

Note the types doing quiet work: `Angle` is not `Number` (trig functions exist only on angles, so degree/radian confusion is impossible), and `Point2D` is not `Vector2` (subtracting two points yields a vector). These distinctions catch the bug classes geometry code actually has.

## The whole language is three constructs

**`type`** — pure data. Fields only: no methods, no visibility modifiers, no inheritance.

**`interface`** — abstract data types with a `Self` type. These are type classes, not OO interfaces: they compile to constrained generics and concrete struct members, never boxing or virtual dispatch.

**`library`** — free functions, callable with dot syntax on their first argument. A function with no arguments is invoked like a property.

Write a function once against an interface, and every implementing type gets it:

```plato
Lerp(a: IVectorLike, b: IVectorLike, t: Number): IVectorLike
    => a.ZipComponents(b, (a1, b1) => a1.Lerp(b1, t));
```

Vectors, points, colors, angles, sizes — anything vector-like — now interpolates, with the code stamped directly into each type. Interfaces in a signature become inferred generic constraints; when you need finer control (parameters that may differ in type), explicit type variables are available:

```plato
Zip(xs: IArray<$T1>, ys: IArray<$T2>, f: Function2<$T1, $T2, $T3>): IArray<$T3>;
```

A handful of conveniences carry a lot of weight:

- Tuples construct any type structurally: `(t.Cos, t.Sin)` builds a `Point2D`.
- A function named after a type defines a conversion, and an implicit cast: `Point3D(v: Vector3): Point3D => ...`
- Operators come from well-known function names: define `Add`, get `+`.
- Array literals: `[x.A, x.B, x.C]`.

## What the compiler produces

The C# backend emits value types built for performance and interop:

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct Vector3 : IVector<Vector3>
{
    [MethodImpl(AggressiveInlining)]
    public static implicit operator Vector3((Number, Number, Number) value) => ...
    [MethodImpl(AggressiveInlining)]
    public Vector3 Lerp(Vector3 b, Number t) => ...
    // ~340 public members, generated from a 3-field declaration
}
```

Interface-generic functions are monomorphized into direct, aggressively-inlined members — Rust's compilation model on .NET. Structs have deterministic memory layout, are declared `partial` so handwritten code can extend them, and interoperate with `System.Numerics` (which the core vector and matrix types wrap, SIMD included). Performance is comparable to well-written C#, and a Plato-level optimizer is planned to close the remaining gaps.

## Why Plato

Plato is developed at [Ara 3D](https://ara3d.com). We do computational geometry across many languages and platforms, and rewriting the same high-performance math library for each one — then keeping the copies in sync — was too costly for a small company. No existing language offered the right mix of simplicity, abstraction, and performance, so we built one: a single source of truth for geometry, with deterministic translation to wherever it needs to run.

The restrictions are the point. No mutation, no `this`, no visibility rules, no virtual dispatch, no exceptions, no reflection. Every removal is what makes retargeting credible — most transpilers die on "which subset of the source language actually ports," and Plato never admits the unportable constructs in the first place. The same restrictions make every function trivially testable and safe to parallelize.

## Where it shines, and where it doesn't

Plato is built for pure computational kernels over small value types: vector and matrix algebra, parametric curves and surfaces, signed distance fields, bounds and intervals, color spaces, transforms, meshes, procedural generation. The standard library in [`plato-src`](plato-src) reads remarkably close to the math it implements — curve definitions are one line from the textbook formulas they cite.

It is deliberately **not** a general-purpose language. There are no strings to speak of, no I/O, no mutation, no application state. Stateful systems, UI, protocols, and algorithms that are naturally imperative belong in the host language; Plato is the layer the host calls into.

## Where it comes from

Plato synthesizes ideas with long pedigrees: type classes (Haskell, Swift protocols, Rust traits), uniform function call syntax (D, Nim), the uniform access principle (Eiffel), and the ergonomics of shader languages (swizzles, `Lerp`, component-wise everything). The design got some outside validation when .NET 7's generic math (`INumber<TSelf>`) independently converged on the same self-constrained generic interfaces Plato compiles to. Its closest spiritual relatives are domain-owning DSLs like Halide, Futhark, and Slang: write a domain's code once, in a restricted pure language, and compile it into whatever host needs it.

## Plato and AI-assisted development

Plato was designed before coding agents were the norm, but its architecture is almost exactly what you would design for them.

**It fits in a context window.** The complete language and standard library is ~34K tokens. The generated C# is ~236K — for one target. An agent working on Plato source holds the *entire* semantic surface of the library in context at once: no retrieval, no partial views, no hallucinated APIs. Almost no production library in any mainstream language has this property.

**Maximum intent per token.** One declaration fans out into hundreds of concrete members across every implementing type and, eventually, every target. The alternative — "apply this change consistently across 160 files" — is the canonical agent failure mode. Plato turns the hardest agent task (global consistency) into the easiest one (edit one line; determinism does the rest).

**It verifies well.** Agentic coding is a generate–check–repair loop, and pure functions over immutable values are the ideal substrate: every function is a unit-test target, properties are easy to state, and evaluation is deterministic. And since the source is compact, formula-shaped, and cites its references inline, it doubles as its own documentation — the most token-economical format there is.

As agents write more of the world's code, the premium shifts from languages that are easy to write toward languages that are dense to read, cheap to verify, and safe to regenerate from. That is the niche Plato occupies.

## Compiler pipeline

A `.plato` file becomes generated code through a series of intermediate representations, each
produced by a single pass. The full internals guide is
[`docs/compiler-pipeline.md`](docs/compiler-pipeline.md); the shape is:

```
source → AST → Symbol graph → Normalize → Constrain → Solve → ⟨elaborate → monomorphize → emit⟩
```

- **AST** (`Plato.AST`) — syntax with source spans. Operators, member access (`a.b`), UFCS
  (`a.b(c)`), and indexers are already lowered here: `a + b` is `Add(a, b)`, `a.Magnitude` is a
  receiver-first call.
- **Symbol graph** (`PlatoCompiler/Symbols`) — names resolved to definitions, scopes established,
  nominal `TypeExpression`s in place. Calls still point at whole overload *groups*; expression types
  are not yet resolved.
- **Normalize** (`PlatoCompiler/Checking/Normalizer`) — canonicalizes the symbol graph for the
  checker (strips residual parentheses, eta-expands first-class function references into lambdas) and
  guarantees a small set of invariants. Behavior-preserving and idempotent.
- **Constrain** (`…/ConstraintGenerator`) — bidirectional walk that assigns a type variable to every
  expression and emits equality and overload constraints.
- **Solve** (`…/Solver`) — nominal unification with an occurs check and deferred-commitment overload
  resolution. It is *total*: a clash, no-match, or ambiguity becomes a **located diagnostic** that
  points at the offending expression, instead of surfacing later as an error in generated C#.

The last three passes are new and currently run in **shadow mode** — they compute a fully-typed,
diagnosed view of every function but do not yet feed code generation, so the production emitter's
byte-for-byte output is unchanged. The solver already does exact/generic unification, concept
(interface) satisfaction with Self-style return refinement, implicit casts, and generic-concept
element inference; the elaboration that lets the backends consume a fully-typed IR is the next
increment.

## Status

The language design is stabilizing after a few years of iteration. The Plato-to-C# compiler is in daily production use: it generates the geometry library consumed by the [Ara 3D SDK](https://github.com/ara3d/ara3d-sdk) (`ara3d-sdk/src/Plato.Generated` when built inside the [Ara 3D studio](https://github.com/ara3d/studio) monorepo). Honest caveats:

- The C# backend is production; the TypeScript and Rust backends are working proofs of concept — they compile a curated demo library and pass a shared conformance suite (see the [live demos](https://cdiggins.github.io/plato/)), but haven't consumed the full standard library yet.
- The compiler builds standalone in this repository; the studio monorepo consumes it via `submodules/Plato`.
- Type errors still ultimately surface through the C# compiler against generated code, but a native type-checker front-end (normalize → constrain → solve, with located diagnostics) now runs in shadow mode over the standard library; wiring it into the pipeline is in progress. See [`docs/compiler-pipeline.md`](docs/compiler-pipeline.md).
- A visual data-flow syntax (**PlatoFlow**) is under development.

The compiler is open source and was built alongside the [Parakeet parsing library](https://github.com/ara3d/parakeet).

## Demos

The multi-target geometry sample browsers are **[live on GitHub Pages](https://cdiggins.github.io/plato/)**
([TypeScript](https://cdiggins.github.io/plato/typescript/) ·
[Rust + WASM](https://cdiggins.github.io/plato/rust/)), deployed automatically
by [`.github/workflows/pages.yml`](.github/workflows/pages.yml).

The source lives in [`demos/`](demos/): one shared
[`geometry.plato`](demos/plato-src/geometry.plato), a TypeScript + Three.js
sample browser, and a Rust crate with a WebAssembly demo. To run them locally
(Node 20+ is the only requirement — the WASM module is checked in, so the Rust
demo doesn't even need a Rust toolchain):

```bat
:: TypeScript sample browser — http://localhost:5173
cd demos/typescript/geometry-samples
npm install
npm run dev

:: Rust + WASM sample browser — http://localhost:8873
cd demos/rust/geometry-samples
node web/serve.mjs
```

See [`demos/README.md`](demos/README.md) for tests, regeneration from Plato
source, and how to add a sample.

## Contributing

Good projects for motivated and patient people:

- Property-based tests for the standard library
- Plato-to-Plato optimizers (e.g., compile-time unrolling of component-wise operations)
- Growing the TypeScript and Rust backends from proof of concept to the full standard library
- New backends: GLSL, C++, Dart, Java, Go
- Editor support: LSP, VS Code, Visual Studio

## Feedback

Always welcome, in any form. Find me on [LinkedIn](https://www.linkedin.com/in/cdiggins/) or at [cdiggins@gmail.com](mailto:cdiggins@gmail.com).
