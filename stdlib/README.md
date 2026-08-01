# Plato Standard Library

This is the standard library shipped with the Plato programming language. 

## Applications

Primary applications:
* 2D/3D geometry
* Numerical programing
* Graphics
* Rendering

Secondary applications:
* Image processing
* Scientific computing 
* Mathematics 
* Physics
* Animation 
* Motion graphics
* Engineering 
* Game development 
* Visual FX

## About Plato 

Plato is a pure functional language that is cross-compiled to multiple back-ends.

**in priority order**:

1. **C#** (primary; the reference runtime)
2. **C++**
3. **CUDA**
4. **TypeScript**
5. Others as capacity allows: **GLSL**, **Rust**, **Python**

Plato code is monomorphized and aggressively inlined. It has near zero performance overhead for using abstractions.    

Affine types (marked as `unique`) allow imperative style code without breaking purity. 

## Structure

Every file holds exactly one **kind** of declaration.

- `<stem>.concepts.plato` holds concepts
- `<stem>.types.plato` holds types
- `<stem>.library.plato` holds exactly one `library` block 

The library is separated into the following folders:

1. `stdlib/foundation`
2. `stdlib/geometry`
3. `stdlib/graphics`
4. `stdlib/future`

[`types-and-concepts.txt`](types-and-concepts.txt) is a generated index of every type and concept
in the three shipping tiers — read it to see what exists. It must be regenerated when this folder
changes; [`AGENTS.md`](AGENTS.md) states the rule and the command.

A folder may reference only itself and the folders before it; 

- `future` may reach anything, nothing reaches into `future`. 
- `foundation` reaches nothing, anything can reach into `foundation`

### `future` is declared, not shipped

`future` holds aspirational vocabulary — declarations for domains the library intends to cover
but does not implement yet (animation tracks and clips, skeletal animation and IK, rigid
dynamics, collision, kinematics, optimization, signals, geo-spatial, engineering, uncertainty).

It is held to a lower bar than the other three tiers, on purpose:

| | `foundation` / `geometry` / `graphics` | `future` |
|---|---|---|
| parses | yes | yes |
| type-checks (0 diagnostics) | yes | yes |
| linted | yes | only with an explicit flag |
| converted to C# | yes | only with an explicit flag |

So a `future` declaration must always resolve and type-check, but it is not expected to carry
bodies, discharge every concept obligation, or survive codegen. The flags that opt it back in:

- `.\tools\check-stdlib-fast.ps1 -IncludeFuture`
- `.\tools\stage-stdlib.ps1 -IncludeFuture`
- `python tools/record-gates.py --include-future`
- `PlatoTests.ForwardStdLibLintTests.SummarizeForwardStdLibLintIncludingFuture` (reporting only)

Parsing and type-checking are NOT behind a flag: `ForwardStdLibParsesAndCompiles` and
`ForwardStdLibDiagnosticCountDoesNotRegress` always read all four tiers.

A corollary for authors: a concept that a shipping tier implements must itself live in a
shipping tier, even when most of its implementers are aspirational. That is why
`TimeVarying<TValue>` sits in `graphics/time-varying.concepts.plato` while the keyframe and
track types that implement it live in `future/`.

## Conventions and style

Companion docs — read before editing this folder:

- [`CONVENTIONS.md`](CONVENTIONS.md) — what the vocabulary means (frames, winding, units)
- [`STYLE_GUIDE.md`](STYLE_GUIDE.md) — how to write declarations and bodies
- [`LIBRARIES.md`](LIBRARIES.md) — how `*.library.plato` files relate to declaration files
- [`VERIFICATION.md`](VERIFICATION.md) — how to know your edit is good: the seven-rung gate
  ladder, which command runs each rung, and the ratchets
