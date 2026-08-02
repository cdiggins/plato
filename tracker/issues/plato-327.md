---
id: plato-327
title: LINT014: identifier that is both a struct field and a no-arg library function (rendering-ambiguity trap)
type: debt
status: done
priority: p3
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [tracker/issues/plato-323.md, submodules/Plato/PlatoCompiler/Analysis/Linter.cs]
---

## Issue

The C# writer decides no-arg call-site syntax by the **uniform rendering rule**: a no-arg member
renders with property/field syntax iff its name is on the struct surface
(`CSharpWriter.StructSurfacePropertyNames`, built from the per-type field and pseudo-field names).
That set is **global and keyed by name only**, so a field on one struct decides how a same-named
member renders on *every* receiver — including receivers that have no such field and whose member
is a classic extension method needing `()`.

This is [plato-323](plato-323.md) cluster 1, measured: the single field `Histogram.Range` stole the
parentheses from `ArrayExtensions.Range(this int)` at every scalar call site in the forward
conformance build — **915 × CS0119**. It was fixed receiver-aware in Plato `9dd7cea`
(`CSharpWriter.IsStructSurfaceProperty(ownerTypeName, name)`), but only for *erased scalar*
receivers. The general per-receiver form remains unlanded (plato-323 remaining item 2, ~100 ×
CS0030 — a field named `Amount` on an image filter forces property syntax onto the handwritten
`Angle.Radians()` method), because it moves **88 of the 184** diff-gated golden files.

So the collision is still live for every non-scalar receiver, and it is invisible until a C#
build a thousand generated files downstream. LINT014 has two jobs:

1. flag the trap at authoring time, in any backend;
2. its finding count **pre-measures the blast radius** of the withheld writer fix.

## Predicate

An identifier `N` collides when:

- (a) some **concrete, non-unique** type declares a field named `N` (only a generated struct
  contributes to the struct surface); and
- (b) some **library** declares a function `N` in *no-arg member form* — exactly one parameter,
  whose type is a real named type (not a type variable, not `SelfType`), and whose parameter is
  **not** named `_`; and
- (c) at least one field owner from (a) is **unrelated** to the function's receiver: it is neither
  the receiver type itself nor a type that implements/inherits the receiver
  (`ConceptGrounding.GroundsSelf` — the same grounding test the writer and checker use).

(c) is the refinement that makes the rule about the real ambiguity rather than about naming
coincidence. Two shapes are **design, not defect**, and are excluded:

- **field forwarding** — a field `N` on `T` is exactly how `T` discharges an obligation `N`
  declared by an interface `T` implements, and a library function `N` over that interface is the derived
  surface reading it. Property syntax is correct on both sides.
- **`_` receiver** — Plato's type-level idiom, which the writer emits as a C# **static**
  (`CSharpFunctionInfo.IsStatic`, cf. LINT012). A static is never rendered with instance property
  syntax, so there is no hazard.

Anchored at the **library function** declaration: that is the side whose call sites lose their
`()`, and it collapses the finding count to one per victim function rather than one per field.

**Severity: Info.** Both sides are legal Plato and the rule fires on the existing corpus, so it
must not gate `lint --strict` nor enter the ratchet (`RatchetCount` = Error + Warning only) — the
LINT003 precedent. Pinned by a test.

## Measurement (2026-07-29) — item-2 blast radius

`dotnet run --project submodules\Plato\Plato.CLI -c Release -- lint <folder>`

| folder | LINT014 findings | distinct colliding names | lint exit |
|---|---|---|---|
| `stdlib` (forward) | **280** | 47 | 0 |
| `stdlib-legacy` (shipping, golden-gated) | **111** | 35 | 0 |

Forward `stdlib`, name(count):

```
Amount(50) Centroid(39) Bounds(32) Area(21) Variance(17) Mean(17) Length(11) Points(9)
Width(7) Height(7) Offset(6) UndirectedEdgeCount(5) Count(5) AspectRatio(5) VertexCount(4)
FaceCount(4) Dual(4) Values(3) Vertices(2) Primitives(2) Pose(2) Normal(2) Center(2)
TriangleCount(1) Translation(1) TotalWeight(1) Tangent(1) Strength(1) StandardDeviation(1)
RowCount(1) Rotation(1) Range(1) RSquared(1) PointCount(1) Origin(1) Middle(1) Magnitude(1)
Frequency(1) EulerCharacteristic(1) Edges(1) Degree(1) Cube(1) Corners(1) CapHeight(1)
Bitangent(1) BinWidth(1) Angle(1)
```

`stdlib-legacy`, name(count):

```
Points(16) Matrix(12) ClosedY(11) ClosedX(11) Center(10) Y(4) X(4) Z(3) Quaternion(3)
Normal(3) W(2) Start(2) Size(2) Row3(2) Row2(2) Row1(2) End(2) Direction(2) Closed(2)
X7(1) X6(1) X5(1) X4(1) X3(1) X2(1) X1(1) X0(1) Width(1) Translation(1) Rotation(1)
Plane(1) Height(1) FaceIndices(1) D(1) Angle(1)
```

Reading the numbers:

- The two names plato-323 named by hand are both here and both at the extremes: **`Amount` is the
  single largest cluster (50)** — exactly the ~100 CS0030 remaining item 2 — and `Range(1)` is the
  flagship cluster-1 case, now down to one because `9dd7cea` handles the scalar receiver.
- The rule found **45 more colliding names nobody had listed**. `Centroid`, `Bounds`, `Area`,
  `Variance`, `Mean` (126 findings between them) are the mass: geometry/statistics accessors that
  are a field on one type and a derived measure on an unrelated one.
- **111 findings in `stdlib-legacy` is the item-2 gate.** Legacy is the byte-identity /
  golden-diffed corpus, and 111 affected library functions is consistent with the measured
  "88 of 184 golden files move". The lint is now the cheap way to re-measure that before and
  after any per-receiver rendering change, with no codegen run.
- Legacy's `X`/`Y`/`Z`/`W`/`X0..X7`/`Row1..Row3` entries are the *pseudo-field* shape rather than
  authoring mistakes; a future refinement could partition them out if the noise matters, but they
  are real members of the same global name set the writer consults, so they belong in the count.

## Affected code

- `submodules/Plato/PlatoCompiler/Analysis/Linter.cs` — `CheckFieldVersusNoArgFunctionNames`,
  `IsNoArgMemberForm`.
- `submodules/Plato/PlatoTests/LinterFieldFunctionCollisionTests.cs` — 5 tests: positive
  (unrelated receiver reported, names both sides + library), three negatives (interface field
  forwarding, receiver-is-owner, `_` receiver), and the Info-severity pin.
- Read-only, per the `Linter` class contract: no codegen or golden impact.

## Gates

| gate | result |
|---|---|
| `dotnet test submodules\Plato\PlatoTests -c Release` | 169/0 (was 164 + 5 new) |
| `lint submodules\Plato\stdlib` | exit 0 |
| `lint submodules\Plato\stdlib-legacy` | exit 0 |
| `.\tools\check-stdlib-fast.ps1` | PASS (lint --strict + checker ratchet) |
| `.\tools\check-frozen-v1.ps1` | 210 files unchanged |

## Follow-up

Not filed as separate issues — they belong to plato-323 item 2:

1. When the per-receiver rendering rule lands, LINT014's count should fall to (near) zero for the
   names it fixes; re-run both folders as the before/after evidence.
2. Optional refinement if the corpus gets noisy: suppress the primitive pseudo-field names
   (`X`/`Y`/`Z`/`W`/`M11`/`Row*`) that the writer keeps global by construction.
