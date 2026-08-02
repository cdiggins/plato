---
id: plato-365
title: Retire non-scalar primitives: PrimitiveTypes = scalars only, System.Numerics behind intrinsic bridge
type: debt
status: idea
priority: p2
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-30
closed:
links: [submodules/Plato/Plato.CSharpWriter/CSharpWriter.cs, submodules/Plato/Plato.CSharpWriter/CSharpConcreteTypeWriter.cs, tracker/issues/plato-308.md, tracker/issues/plato-234.md, tracker/issues/plato-330.md, tracker/issues/plato-331.md]
---

## Issue

The writer's `PrimitiveTypes` list (`Plato.CSharpWriter/CSharpWriter.cs:612`) decides
primitiveness by string match and currently includes non-scalar types: `Angle -> float`,
`Matrix3x2`, `Matrix4x4`, `Quaternion`, `Plane`, `Vector2/3/4` (System.Numerics), and
`Vector8` (Vector256). This creates four overlapping, unchecked sources of truth for one name:

1. The name list itself — invisible in the language. `type Plane` in
   `stdlib/lines-planes.types.plato:15` looks like a normal declaration but is silently
   overridden to `System.Numerics.Plane`.
2. The handwritten runtime (`Plato.Intrinsics.V2`) — the real shapes.
3. Stdlib redeclarations the checker trusts (`Angle { Radians: Number }` in
   `quantities-geometric.types.plato:10`, `Matrix4x4 { Row1: Number4 }` in
   `matrices.types.plato:36` vs System.Numerics `M11..M44`) — nothing verifies the runtime
   matches.
4. A redundant fourth vector family: intrinsic `Vector2/3/4` carries no semantic distinction
   from `Number2/3/4` (tuple) / `Vector2D/3D` (displacement) / `Point2D/3D` (position), and is
   not even declared in the forward stdlib (zero `type Vector[234]` declarations; only a README
   mention and a comment).

`Angle` is the worst case: on the primitive list mapped to `float`, ALSO a handwritten wrapper
struct, ALSO a stdlib type with a `Radians` field. Triple story on one name. Legacy is no longer
generated, so the original System.Numerics-visibility rationale no longer applies.

Decision (this issue): **primitive = scalar only.** `PrimitiveTypes` keeps exactly
`Number, Integer, Boolean, Character, String, Dynamic, Type, Function0-9`. Every type with
fields is a generated struct from its stdlib declaration. System.Numerics stays as an
implementation detail inside handwritten intrinsic bodies, reached via the already-existing
implicit-conversion seam `IntrinsicVectorBridges` (`CSharpConcreteTypeWriter.cs:1066`).

## Impact

- Shape drift between stdlib declaration and runtime is currently silent corruption; after the
  change it is a C# compile error in generated output, caught by the ForwardStdLib gate.
- The C++ writer's primitive story collapses to a scalar map (`float/int/bool/char/string/function`)
  — no System.Numerics-analog design questions leak into the second backend.
- `Angle -> float` erasure currently destroys the radians/scalar type distinction in generated C#
  and makes Angle unrepresentable in GLSL (plato-234); both fixed by making it an ordinary
  single-field struct.
- Perf preserved: hot ops (matrix multiply, quaternion ops, dot/normalize) still run on
  System.Numerics inside intrinsic implementations; bridge conversions are same-bits
  aggressive-inlined constructor calls, upgradeable to `Unsafe.As` bit-casts if profiling ever
  demands.

## Affected code

- `Plato.CSharpWriter/CSharpWriter.cs:612` — `PrimitiveTypes` dictionary; entries to delete: lines 631-639 (`Angle`, `Matrix3x2`, `Matrix4x4`, `Quaternion`, `Plane`, `Vector2/3/4/8`).
- `Plato.CSharpWriter/CSharpConcreteTypeWriter.cs:1066` — `IntrinsicVectorBridges`; gains `Matrix3x2`, `Matrix4x4`, `Quaternion` entries (row-wise conversion).
- `Plato.CSharpWriter/CSharpConcreteTypeWriter.cs:1115` — `PrimitiveFieldNames`; consulted by the bridge writer.
- `Plato.Intrinsics.V2` (handwritten runtime) — `Angle` wrapper struct deletable; matrix/quaternion/plane surface becomes internal to intrinsic bodies rather than the generated types themselves.
- `stdlib/intrinsics-transforms.library.plato`, `stdlib/intrinsics-vectors.library.plato` — the declared op surface the handwritten implementations must satisfy.
- `stdlib/lines-planes.types.plato:15`, `stdlib/quantities-geometric.types.plato:10`, `stdlib/matrices.types.plato:36` — declarations that become authoritative.

## Cause / analysis

The list grew from the original goal: surface System.Numerics types directly so the JIT's
SIMD-accelerated ops apply. That predates (a) the forward stdlib's own vocabulary
(Number2/3/4 + Vector2D/3D landed with their own semantics), (b) the
`IntrinsicVectorBridges` seam (plato-308 work) which already proves generated structs can
convert to System.Numerics at the call boundary, and (c) the C++ backend goal, where a
name-list of .NET types is pure liability. Legacy generation is retired, so nothing still
requires the System.Numerics names to be Plato-visible.

## Priority

**p2.** Not gating shipped code, but every stdlib content wave filed on top of the current setup
deepens the fragility (Matrix4x4 row/field mismatch is one un-checked refactor away from silent
wrong bits), and the C++ writer should not be started before this lands. Defer past plato-308/323
burn-down — those sessions own the same files.

## Dependencies

- Blocked by: [plato-323](plato-323.md) / [plato-308](plato-308.md) — same files
  (`Plato.CSharpWriter`, `Plato.Intrinsics.V2`) are under active burn-down; landing this
  mid-burn-down would invalidate their per-shape error baselines.
- Blocks: any C++ writer start; clean fix for [plato-234](plato-234.md) (GLSL Angle).
- Touches: `Plato.CSharpWriter` (writer-collision warning from plato-294 applies — coordinate
  before editing), `Plato.Intrinsics.V2`, ForwardStdLib gate baselines.

## Fix approaches

1. **Three increments, cheap first** (recommended; detailed in Done means): (a) drop
   `Vector2/3/4/8` + `Plane` + `Angle` from the list and regenerate — near-zero forward usage of
   the first five, Angle becomes an ordinary struct; (b) checker tripwire forbidding stdlib
   declarations that shadow a writer primitive name; (c) matrix/quaternion increment — bridge
   entries + intrinsic-transforms handwritten audit.
2. **Big-bang removal** — one pass, all nine entries. Smaller total diff but couples the easy
   deletions to the matrix work and lands one giant regeneration on the gates.
3. **Keep matrices primitive, align shapes instead** — teach the checker the M11..M44 shape.
   Rejected: preserves the name-list mechanism and does nothing for C++.

## Bedrock

Strengthens the single seam this list was working around: `IntrinsicVectorBridges` +
handwritten intrinsic bodies as the *only* place generated code meets System.Numerics. After
the change, stdlib declarations are the sole shape authority, and any handwritten/declared
mismatch is a compile error in the ForwardStdLib gate instead of a silent override. Makes every
future backend (C++ first) cheaper: primitive story = scalar map, nothing else.

**Verdict: simplest-along-the-grain.** The simple fix must NOT special-case matrix field
layouts in the writer (no M11..M44 knowledge outside `Plato.Intrinsics.V2`) and must NOT keep
`Angle` erasing to bare `float` — both would re-create the invisible-primitiveness mechanism
this issue exists to delete.

## Done means

- [ ] `PrimitiveTypes` contains only `Number, Integer, Boolean, Character, String, Dynamic, Type, Function0-9`.
- [ ] `Plane`, `Angle`, `Matrix3x2`, `Matrix4x4`, `Quaternion` generate as ordinary structs from their stdlib declarations; matrix/quaternion hot ops route through intrinsic implementations that bridge to System.Numerics internally.
- [ ] Handwritten `Angle` wrapper struct deleted from `Plato.Intrinsics.V2`.
- [x] Checker/lint tripwire: a stdlib `type` declaration whose name collides with a writer primitive is an error.
      **Landed 2026-07-30 as LINT015** (`PlatoCompiler/Analysis/Linter.cs`
      `CheckPrimitiveNameShadowing`). Fires when a type declares FIELDS and its name is on
      `PlatoCompiler/Types/WriterPrimitiveNames.All` — the compiler-side canonical copy of
      `CSharpWriter.PrimitiveTypes`, held in sync by
      `PlatoTests/LinterPrimitiveShadowingTests.WriterPrimitiveTableMatchesTheCompilerCopy`
      (set equality, both directions). Fieldless declarations are exempt: that is how
      `primitives.types.plato` states an intrinsic's interface surface, and nothing is discarded.
      Severity is Error except for `WriterPrimitiveNames.KnownShadowedByStdlib`
      (`Angle`, `Plane`, `Matrix3x2`, `Matrix4x4`, `Quaternion`) which report as Warnings so the
      gate stays green while increments (a) and (c) land. **Delete a name from that list when it
      leaves the primitive list** — a stale entry is a test failure by design.
- [ ] ForwardStdLib gate green (or error count not regressed vs the plato-308 baseline at time of landing, measured per-shape).
- [ ] C++/GLSL note recorded: primitive map for a new backend = scalar map only (one line in `docs/plato-for-agents.md` or the library map).

## Prevention

- The tripwire box above is the class-level prevention: shadowing a primitive name becomes
  impossible to do silently, for any future type.
- [plato-330](plato-330.md) (lint intrinsic obligations against the intrinsics API snapshot)
  covers the remaining seam — handwritten intrinsic bodies vs declared intrinsic libraries —
  and becomes the *only* trust boundary left after this lands.
