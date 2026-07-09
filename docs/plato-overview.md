# Plato Overview 

Written on July 8th, 2026 by Christopher Diggins

## What is Plato 

Plato is a simple programming language that allows you to easily  
produce high performance libraries that run on different platforms and target languages.

It was designed originally to help produce numerical and geometry libraries such as those used
in CAD, games, and 3D content creation tools with minimal boilerplate.  

## Motivation

Everywhere people reinvent geometry and numerical libraries multiple times, with varying degrees of completeness and correctness. Within a company building 3D products there can be hundreds of implementations of the same function.  

At Ara 3D we wanted a single high-performance geometric and mathematical library that is 
succinct, expressive, easily verified, and that can be used in many different contexts, by experts, non-programmers, and AI.    

We needed a programming language that has the following properties:

1. easily transpiled to multiple other languages 
2. low boilerplate, redundancy, and complexity
3. has no penalty for polymorphism or functional programming 

## Design Goals

Plato design principles: 
- statically typed
- easy to learn and understand for anyone with little or no programming experience 
- transpiles to multiple targets (focused primarily on C# and TypeScript) 
- recognizable syntax 
- immutable data structures by default
- safe mutation via affine types 
- minimal boilerplate
- consistent syntax and code layout
- functional programming with minimal performance penalty
- declarating concept functions have zero overhead 

Additional use cases:

## Language Details

Plato is a pure, statically-typed functional language for defining algebraic data types and the operations over them, which a compiler amplifies into a large, [monomorphized](https://en.wikipedia.org/wiki/Monomorphization), zero-dispatch library into various target languages.

- all functions are declared outside of types extension methods
- functions declared on concepts are available to all implementing types
- The resulting code can be converted in 
- concepts are similar to Haskell Type Classes and Rust Traits 
- types only declare fields
- function with single arguments can be called like properties
- three kinds:
    - **`type`** — pure data. Fields only, no methods, no visibility, no inheritance. `type Circle { Center: Point2D; Radius: Number; }`
    - **`interface`** — abstract data types with a `Self` type, which compile to F-bounded generic interfaces in C# (`IVector<Self> where Self : IVector<Self>` in [Interfaces.g.cs:60](ara3d-sdk/src/Plato.Generated/Interfaces.g.cs:60)). These are **type classes**, not OO interfaces — there is no boxing, no vtable, no heterogeneous collections through them.
    - **`library`** — free functions, callable with dot syntax on their first argument. A function written once against an interface (`Lerp(a: IVectorLike, b: IVectorLike, t: Number)`) is stamped concretely into every implementing struct.
- tuples construct any type structurally (`(t.Cos, t.Sin)` builds a `Point2D`); 
- a function named after a type becomes an implicit conversion (`Point3D(v: Vector3)` in geometry.library becomes `implicit operator Point3D(Vector3)` in the output); 
- operator overloading falls out of well-known names (`Add` → `+`); 
- the `_: Type` first-parameter convention produces static members. [intrinsics.plato](ara3d-sdk/plato-src/intrinsics.plato) is the FFI: a declaration-only library that binds `System.Numerics` and `MathF` by signature, which is an unusually clean foreign-function story — the host platform is just another library whose bodies happen to live elsewhere.

## Generate C# Code

- Struct have strict layout 
- Operators are generated automatically from names
- Auto-generated:
    - constructors
    - destructors
    - serialization to/from JSON strings
    - implicit converters to/from value tuples   
- Functions are specialized 

##  Lineage: what it follows from

Plato is a considered synthesis rather than an invention from whole cloth, and its ancestry is legible:

- **Haskell type classes / Swift protocols with Self / Rust traits** — the interface model. The algebraic hierarchy in [core.interfaces.plato](ara3d-sdk/plato-src/core.interfaces.plato) (`IAdditive` as an Abelian group, `IMultiplicative` as a monoid, `IArithmetic` as roughly a field) is the Haskell numeric class hierarchy redone by someone who cares about geometry more than number theory — and the deliberate splitting of `IMeasure`, `ICoordinate`, and `IVector` (points aren't vectors; angles aren't numbers; measures add but don't multiply) is closer to the *dimensional analysis* tradition of F#'s units of measure.
- **D and Nim** — uniform function call syntax as the central ergonomic move, here grounded concretely in C# extension methods as the compilation substrate.
- **Eiffel** — the uniform access principle (nullary functions are properties; `v.Magnitude` doesn't reveal whether it's stored or computed).
- **GLSL/HLSL** — the ergonomics of the standard library: swizzles (`v.ZXY`), scalar-vector promotion, `Lerp`/`SmoothStep`/`Fract`, component-wise everything. Plato reads like shader math for CPU code.
- **ML family** — immutability, expression-orientation, tuples as the universal glue.
- **C# itself** — and here there's a genuine vindication story: .NET 7's generic math (`INumber<TSelf>`, static abstract interface members) arrived at essentially the same F-bounded design Plato had already committed to. The C# team independently converged on your compilation target's shape. That is strong evidence the core abstraction is right; it also means raw C# can now express the *semantics* Plato targets — just at several times the verbosity and with the CRTP constraint contagion Plato hides.

The closest *spiritual* relatives, though, aren't general-purpose languages at all — they're the domain-owning DSLs: Halide (image pipelines), Futhark (GPU array code), Faust (audio DSP), Slang (shaders). Each says: "this domain's code is written once, in a restricted pure language, and compiled into whatever host needs it." Plato is that, for geometric/numeric kernels. That's the right comparison class, and it reframes every question below.

## What it does well

**The boilerplate-to-semantics ratio is exceptional.** The whole standard library — the algebraic hierarchy, vectors, matrices, quaternions, intervals, bounds, transforms, meshes, colors, coordinate systems, and a genuinely delightful museum of classical curves with Wikipedia citations inline ([curves.plato](ara3d-sdk/plato-src/curves.plato) has cardioids, limaçons, trisectrices, torus knots) — is ~3,500 lines. The math reads like the math: `Cardoid(t: Angle): Number => 1.0 + t.Cos;` is one line from the textbook formula. Very few languages let a geometry library be this close to its own specification.

**Zero-cost abstraction on a platform that historically didn't have it.** The compiler monomorphizes interface-generic functions into direct struct members with `AggressiveInlining` — no interface dispatch, no boxing, `StructLayout(Sequential, Pack=1)` for deterministic memory layout and interop. This is Rust's compilation model grafted onto .NET, obtained by construction rather than by JIT heroics. The generated structs are `partial`, leaving a clean escape hatch for handwritten members, and the generated extension classes even provide `System.Numerics.Vector3` interop overloads so the library composes with the ecosystem rather than fighting it.

**Semantic typing that catches the bugs geometry code actually has.** `Angle` is not `Number` — trig functions exist only on `Angle`, inverse trig *returns* `Angle`, and constructors like `Degrees`/`Turns`/`Gradians` make the unit explicit at the point of use. `Point3D − Point3D = Vector3` via `IDifference<T>`. Degree/radian confusion and point/vector confusion are two of the most common real-world 3D bug classes, and the type system eliminates both at essentially no syntactic cost.

**Restriction as a feature.** No mutation, no `this`, no visibility, no virtual dispatch, no exceptions in the surface language, no reflection. Every one of these removals is what makes the multi-target promise *credible* — most transpilers die on the "which subset of the source language actually ports" problem, and Plato dodges it by never admitting the unportable constructs in the first place. The same restrictions make every function trivially testable and parallelizable.

## What it's best at — and what it would be bad at

**Best at:** exactly what it's used for. Pure computational kernels over small value types: geometry, vector/matrix algebra, parametric curves and surfaces, signed distance fields, color spaces, interpolation, bounds/interval logic, procedural generation. The [procedurals.plato](ara3d-sdk/plato-src/procedurals.plato) design note — modeling curves, surfaces, SDFs, volumes, and textures uniformly as `IProcedural<TDomain, TRange>` — is the most intellectually interesting idea in the codebase: one interface unifying continuous geometry as functions, with union/intersection/difference as combinators. A CAD kernel's mathematical layer, a game engine's math library, a shader-shared lighting model: this is the sweet spot.

**Bad at:** general-purpose or systems programming:

- **Error handling and partiality.** There are no sum types, no `Option`/`Result`, no pattern matching. Ray-triangle intersection can miss; matrix inversion can fail. Today that's handled by convention (`CanInvert: Boolean` alongside `Invert`, or the C#-side `Tuple2<Matrix3x2, Boolean>`), which is the weakest part of the type system's story.
- **Strings, I/O, dates, protocols** — absent, correctly.
- **Double precision.** `Number` compiles to `float` (the MathF binding, and `3.40282347E+38` limits in [constants.plato](ara3d-sdk/plato-src/constants.plato)). For the BIM/geospatial world Ara 3D lives in, large-coordinate models genuinely need doubles; a precision-generic story (or a compiler switch) is conspicuously missing.

## Compared to Other Languages

| | Verdict in one line |
|---|---|
| **C# + generic math** | The same semantics are now *possible* in raw C#, but at 5–10× the verbosity, with contagious `where T : INumber<T>` constraints everywhere, and no path to other targets. Plato is the ergonomic and portable frontend to a design C# itself validated. |
| **F#** | Closest .NET philosophical cousin (immutability, units of measure, terseness). But F#'s statically-resolved type parameters (SRTP) — its equivalent of Plato's implicit generics — are notoriously painful, and Fable targets only JS. Plato trades F#'s generality for a dramatically simpler mental model in its niche. |
| **Rust** | Shares traits + monomorphization + immutability-by-default. But ownership/borrowing is pure overhead for pure math, and Rust can't be the *source* that also becomes C# and GLSL. Rust is a better *target* for Plato than competitor. |
| **Julia** | The scientific-computing incumbent for "math that reads like math." Dynamic, JIT-latency-bound, heavyweight runtime — unembeddable in the places Plato's output goes (Unity, plugins, shaders). |
| **GLSL/HLSL/Slang** | Plato's standard library is basically shader-language ergonomics; Slang proves the "one source, many graphics targets" market exists. A GLSL backend would make Plato the only language whose CPU and GPU geometry code are literally the same source. |
| **Haskell** | The type-class model without laziness, monads, or the learning cliff. Plato is arguably "type classes for people who will never say the word 'functor.'" |

The honest competitive summary: Plato's niche has no direct incumbent. Its real competitor is *"just write the C# library by hand and port it when needed"* — which is precisely the cost equation the README cites as the founding motivation, and which the AI section below revisits.

## What could be improved

- A native type checker with source-anchored diagnostics
- A test suite for the library 
- golden-file tests for the compiler.
- Make ambiguity an error (or well-defined)
- Optimizer
- Option to generate double precision as output (separate namespace)
- published operator-precedence table and grammar; 
- packaging that lets the compiler run outside the mono-repo;
- finishing the commented-out [procedurals library](ara3d-sdk/plato-src/procedurals.plato:88 

### Known Issues 

- **Unary minus appears to bind looser than `+`.** `FromOne(x: IVector) => -x + One` ([vectors.plato:182](ara3d-sdk/plato-src/vectors.plato:182)) compiles to `this.Add(One).Negative` — i.e. `−(x+1)`, not `1−x` ([_Vector3.g.cs:88](ara3d-sdk/src/Plato.Generated/_Vector3.g.cs:88)). Same pattern in `SmoothStep`: `x.Sqr * (-x.Twice + 3.0)` ([algebra.plato:149](ara3d-sdk/plato-src/algebra.plato:149)) becomes `x²·(−(2x+3))` instead of `x²·(3−2x)`. Two independent confirmations in shipping generated code — either a parser precedence bug or an undocumented and very dangerous precedence rule.

- **`Degrees(x: Integer)` returns turns** — [angles.plato:4](ara3d-sdk/plato-src/angles.plato:4) says `=> x.Number.Turns` (copy-paste from the line above). `90.Degrees` is a full 90 revolutions.

- **`Barycentric` is algebraically wrong:** `(v1 + (v2 - v1)) * uv.X + ...` ([algebra.plato:5](ara3d-sdk/plato-src/algebra.plato:5)) simplifies to `v2·u + (v3−v1)·v` — the `v1` base term is lost inside the parenthesization. Confirmed in the generated output.

- **`MinNumber`/`MaxNumber` are sign-swapped** in [constants.plato:10](ara3d-sdk/plato-src/constants.plato:10), and `Lissajous` ignores its `a` parameter and produces a constant `y` ([curves.plato:220](ara3d-sdk/plato-src/curves.plato:220)).

- **`Magnitude` diverges from `Length`:** generic `MagnitudeSquared` is `SumSqrComponents / NumComponents` ([core.library.plato:38](ara3d-sdk/plato-src/core.library.plato:38)) — an RMS, not a Euclidean norm — so the same `Vector3` answers `Length = 5` (intrinsic) and `Magnitude ≈ 2.89` for `(3,4,0)`, and generic `Normalize` doesn't produce unit vectors on the non-intrinsic path.

**As a design: it's already succeeded once.** The convergence with .NET generic math, and the industry's parallel movement toward restricted multi-target kernel DSLs (Slang most visibly), both say the bet was correct. The open question was never whether the design is right — it's whether a one-person language can cross the tooling chasm. Which brings us to the section where that calculus has genuinely changed.

## Plato in the age of agentic AI — the economy of tokens

**Because agents generate *drafts*, and compilers generate *guarantees*.** An agent-translated port is N codebases that each need review and that drift independently with every change — the maintenance cost didn't disappear, it moved into review, multiplied by the number of targets. Plato's transpilation is deterministic: one reviewed source, N outputs correct by construction. In an agentic world the scarce resources are no longer keystrokes; they are **human review attention and model context**. Plato is, almost accidentally, optimized for both:

**Token density.** The measured numbers: the complete Plato corpus is ~137K characters (~34K tokens) versus ~943K characters (~236K tokens) of generated C# — **6.9× denser**, and that's against *one* target; add JS and Rust backends and the effective ratio is ~20×. The practical consequence is qualitative, not just quantitative: **the entire language — every type, every interface, every function in the standard library — fits comfortably in a single model context window.** The generated C# does not. An agent working on Plato source never needs retrieval, never sees a partial view, never hallucinates an API that's out of frame. Whole-corpus-in-context is a categorical reliability improvement, and almost no real library in any mainstream language has it.

**Leverage per emitted token.** Agents are billed and bottlenecked per token *generated*. One line of Plato — `Cardoid(t: Angle): Number => 1.0 + t.Cos;` — fans out into concrete members across every implementing type and (eventually) every target. The alternative task, "add this operation consistently to 160 C# files," is the canonical agent failure mode: partial propagation, inconsistent edits, missed call sites. The DSL-plus-generator architecture converts an agent-hostile task (maintain global consistency by hand) into an agent-friendly one (edit a single declaration; determinism does the fan-out). In effect, **Plato gives an LLM the same thing it gives a human: a place to stand where one token of intent moves kilobytes of implementation.**

**Regularity beats familiarity.** The obvious objection — "no model has Plato in its training data" — matters less than it seems. Plato's syntax is a TypeScript/C# hybrid with near-zero surface novelty; the semantics are the interesting part, and the ~34K-token corpus *is* the semantics, cheap to supply in context. LLMs are excellent at unfamiliar-but-regular languages given in-context exemplars; they are much worse at familiar-but-irregular ones. Plato is extremely regular: one way to define data, one way to define behavior, no statement/expression duality to speak of, no hidden state. And the standard library's habit of citing Wikipedia over each formula is, unintentionally, ideal grounding for a model — the source is its own reference documentation, which also neatly dissolves the "docs are out of date" problem: 3,500 lines that are simultaneously the spec, the docs, and the implementation are the most token-economical documentation format that exists.

**The purity dividend.** Agentic coding is a generate → verify → repair loop, and its throughput is set by the quality of the verification signal. Pure functions over immutable values are the best possible substrate for that loop: every function is a unit test target, properties are easy to state (`Lerp(a,b,1) = b`), evaluation is deterministic, and there is no environment to mock. The bugs I found in §6 are all *one-line property violations* — precisely the kind of thing an agent with a test harness finds and fixes mechanically. A `plato test` command with property-based testing would make the language close to self-healing under agent maintenance.

**The current bottleneck, restated for the agent era.** Today the repair signal arrives as C# errors pointing at generated code the agent didn't write. That round-trip — Plato edit → generate → C# error → mentally back-map — is the slowest and most error-prone segment of the loop for humans and agents alike. The native type checker isn't just developer experience; it's the difference between an agent iterating on Plato directly versus an agent constantly falling back to reasoning about the C# output. **In 2026, the highest-ROI feature of any niche language is a fast checker with precise, source-anchored errors, because that's what makes the language cheaper than its host for an agent to work in.**

**The strategic reframe.** As agents write more of the world's code, the premium shifts towards languages that are succinct to *read*, cheap to *verify*, and safe to *regenerate from*. Compact sources of truth with deterministic elaboration are the asset. Plato was designed for a small company that couldn't afford to maintain five ports. It turns out that architecture — a small, pure, reviewable kernel language above a deterministic multiplier — is also roughly what you'd design *from scratch* for AI-maintained software. The niche-language adoption barrier (nobody wants to learn it) erodes when agents do most of the writing; the niche-language value proposition (density, purity, portability) compounds. 

## Bottom line

Plato is a carefully designed  domain language whose three-construct core (pure data, type classes, UFCS libraries) was independently validated by C#'s own evolution, and whose standard library achieves a spec-like density — that almost no production library in any language matches. It is genuinely excellent at pure geometric and numeric kernels.