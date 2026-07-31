---
id: plato-311
title: Concept in type position (existential) has no defined C# lowering
type: problem
status: done
priority: p2
effort: L
risk: high
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [tracker/issues/plato-308.md, tracker/issues/plato-310.md, submodules/Plato/stdlib/surfaces-generated.plato, submodules/Plato/Plato.CSharpWriter, submodules/Plato/PlatoTests/ForwardStdLibCheckerTests.cs]
---

## Issue

Split out of [plato-308](plato-308.md) Root 1 after analysis on 2026-07-29. A Plato concept used
as a **constraint** lowers cleanly (monomorphization grounds `Self`). A concept used **in type
position** — field type, return type, parameter type of a library function — is an *existential*
("some type implementing C"), and the F-bounded lowering
`interface C<Self> where Self : C<Self>` cannot express it. The checker accepts these programs;
the C# emitter has no defined semantics and improvises:

- `Path: Curve3D;` (`stdlib/surfaces-generated.plato:62`) emits as
  `Curve3D<SweptSurface> Path` (`_SweptSurface.g.cs:17`) — the **enclosing type** plugged into
  `Self`, an unsatisfiable type ("a curve whose Self is a swept surface"). All 166 CS0315 in
  plato-308 are this, across 12 concept-typed fields in `surfaces-generated.plato`,
  `surfaces-patches.plato` (CoonsPatch), `solids-generated.plato` (SweptSolid).
- Library methods over bare-concept receivers monomorphize onto mapped intrinsics with the same
  hole (see [plato-310](plato-310.md) for the mechanical arity bugs entangled in the same files).

This is a language/codegen design gap, not a stdlib defect: storing "some curve" in a surface type
is reasonable vocabulary. Redesigning the library to appease the C# encoding was considered and
rejected (options C/D below).

## Impact

Gates the forward conformance build ([plato-308](plato-308.md)), and with it every forward-stdlib
law run, including plato-306's 11 affine laws. Beyond the immediate build: until this has defined
semantics, any forward-stdlib author can write a concept-typed field that lints and type-checks
clean and produces garbage C#, discovered only 1232 files later.

## Affected code

- `submodules/Plato/Plato.CSharpWriter` — concept-to-interface lowering (single F-bounded form today).
- `submodules/Plato/stdlib/surfaces-generated.plato:20,30,41-42,61-62,71`, `stdlib/surfaces-patches.plato:77-80`, `stdlib/solids-generated.plato:53` — the 12 existential fields.
- `submodules/Plato/PlatoTests/ForwardStdLibCheckerTests.cs:24-39` — documents the sibling cluster (tuple-literal returns of bare concepts) and the `Self`-unification permissiveness this design must not break.
- `submodules/Plato/stdlib/curves.concepts.plato:28` — `Curve3D`: `Geometry3D + Procedural<Number, Point3D>`, the canonical existential-stored concept.

## Cause / analysis

Rust and Swift hit the identical fork and both split the two uses: constraint-use (`impl Trait` /
`some P`, monomorphized) vs type-use (`dyn Trait` / `any P`, dispatched), with an object-safety
rule deciding which members survive into the dynamic view. Plato's concepts as constraints are the
`some` side and already work; the `any` side simply has no lowering.

## Decision (2026-07-29)

**Option A — dual-interface lowering** (approved by Christopher in conversation):

- For each concept `C`, emit non-generic `interface C` carrying the **object-safe subset**
  (members where `Self` appears only as the receiver; `Self`-returning members either return the
  non-generic view or are excluded — decide during implementation) alongside
  `interface C<Self> : C where Self : C<Self>`.
- Concept in constraint position lowers to `C<Self>` as today; concept in type position lowers to
  non-generic `C`.
- A concept with an empty object-safe surface gets no non-generic view; using it in type position
  becomes a compile-time diagnostic instead of downstream C# garbage.
- Accepted costs: struct implementers box when stored in a `C`-typed field (modeling-time types,
  not hot loops — hot paths stay monomorphized); binary methods (`Compare(Self)`) invisible
  through the view; `Pack=1` blittability already void for these types (delegate fields precede).

Rejected alternatives, recorded for the ADR: (B) erasure to generated `AnyC` delegate-holding
structs — loses stored-curve identity/inspectability, weak delegate equality; (C) making storage
types generic (`SweptSurface<TPath>`) — arity infects every consumer signature, vocabulary bends
to the encoding; (D) banning concept fields in favor of `Function1`-typed fields — same identity
loss as B paid in the vocabulary itself.

## Priority

p2 — same deliverable chain as plato-308/306. Effort L: emitter surgery plus object-safety
computation plus golden refresh. Risk: medium-high — shared writer with the legacy generation;
byte-identity of `Generated/` goldens is the tripwire.

## Dependencies

- Blocks: [plato-308](plato-308.md) (the 166 CS0315), [plato-306](plato-306.md) transitively.
- Blocked by: nothing hard; [plato-310](plato-310.md) is independent but both must land for a green build.
- Touches: `Plato.CSharpWriter` (shared with legacy generation — `regen-generated.ps1` diff gate),
  potentially the checker for the "no object-safe surface used in type position" diagnostic.

## Fix approaches

Decision made (Option A above). Implementation order:
1. Object-safety computation over concept members in the writer (or checker, if the diagnostic lands there).
2. Dual emission: non-generic view + F-bounded interface inheriting it.
3. Type-position rendering switches to the non-generic view.
4. Diagnostic for type-position use of a view-less concept.
5. Golden refresh + conformance gates.

## Bedrock

Gives the language a defined `some`/`any` split at the one seam where Plato meets a nominal target
language — the concept-lowering boundary in `Plato.CSharpWriter`. Every future backend (GLSL/C++/
CUDA writers) inherits a precise question ("what is your `any C` representation?") instead of an
undefined behavior. Closing this issue should produce an ADR in `tracker/decisions/`.
**Verdict: right** — this is the invariant fix; plato-310 covers the mechanical remainder.

## Progress (2026-07-29)

WIP save point committed: Plato `f859808` (writer + `ConceptGrounding` + `ExistentialConceptChecker`),
ADR studio `55c27d6`. **Attribution: `f859808` also contains the whole [plato-310](plato-310.md)
emitter fix** — two agents edited the same five writer files concurrently and the changes were
interleaved, so the save point covers both issues despite the plato-311 subject line.

Measured after that commit:

- CS0315/CS0305 cluster **gone: 300 -> 0**. Existential fields now render as the bare view name.
- Forward conformance Stage 1 clean (0 resolution errors), codegen 1240 `.g.cs`,
  `tools\check-stdlib-fast.ps1` PASS (lint --strict + checker ratchet).
- CS0305/CS0315 both 0; **398 errors remain, and the code shape alone is misleading**: the gate
  criterion "only CS0535/CS0557 left" would read as PASS, but classifying the 392 CS0535 by the
  interface named in each message gives **342 non-generic view members (this issue's own new hole)**
  vs ~50 generic-interface members (the genuine plato-308 Root 2 class). Top view holes:
  `Quantity.Amount()` 100, `ProbabilityDistribution.Mean()`/`Variance()` 38 each,
  `Image.Width()`/`Height()`/`Size()` 14 each, `LightSource.CastsShadows()` 14, `Camera.Near()` 12.
- **New blocker: 392 CS0535 + 6 CS0557** (was ~20 CS0535 pre-change). Concrete implementers get
  no implementation for the *view* members — `Quantity.Amount()`, `Hashable.Hash()`,
  `IntervalLike<Angle>.Start()`, worst files `_Matrix4x4`/`_Matrix3x2` (10 each), the camera
  types (8 each). Struct emission satisfies only the F-bounded interface; it must also satisfy
  the inherited non-generic view (or the view members must be declared so the generic
  implementations serve both).
- `tools\regen-generated.ps1`: **drift is exactly one file per variant, `Interfaces.g.cs`, and it is
  purely additive** — new non-generic views (`IArrayLike<T>`, `IVectorLike`, `INumerical`, `IVector`,
  `IMeasure`, ...) plus base-list additions on the existing F-bounded interfaces; 183/184 files
  identical, 0 missing, 0 extra, no member removed. The design's "purely additive" claim holds at
  the interface level.
- **But the refresh cannot be taken yet, and this is the important finding: the same view-member
  hole regresses the *legacy* SDK.** Applied the refresh and built both Generated variants:
  **26 CS0535 each** (`Point2D` missing `IPointGeometry2D.Points()`, `LineMesh3D` missing
  `IIndexedGeometry3D<Integer2>.FaceIndices()`, ...). Reverted `Generated/` to baseline so the tree
  stays buildable. So the struct-emission fix is not merely a forward-stdlib concern — it gates the
  golden refresh, and the golden refresh gates every legacy consumer.
- With goldens at baseline: `check-frozen-v1.ps1` PASS (0 changed of 210),
  `Ara3D.SDK.ConformanceTests` 205/205.

Next step = view-member implementation in struct emission (forward the view members to the existing
generic implementations), which should clear 342 forward CS0535 and the 26 legacy ones together;
then refresh goldens and re-run all four gates.

## Done means

- [x] ADR in `tracker/decisions/` recording the dual-interface design and the object-safety rule — `tracker/decisions/2026-07-29-existential-concepts-dual-interface.md`.
- [ ] The 166 CS0315 errors are gone: `_SweptSurface.g.cs` stores `Curve3D Path` (non-generic view), all 12 existential fields likewise.
- [ ] Type-position use of a concept with no object-safe surface produces a diagnostic, with a test.
- [ ] `tools\regen-generated.ps1` clean or goldens refreshed deliberately in the same change; `Ara3D.SDK.ConformanceTests` 0 fail.
- [ ] Remaining `Plato.ForwardConformanceTests` build errors attributable only to [plato-310](plato-310.md) / plato-308 Root 2.

## Prevention

- The view-less-concept diagnostic (Done box 3) prevents silent garbage for future concepts.
- `ForwardStdLibCheckerTests` ratchet already covers the checker side; wiring the conformance build
  into `check-all.ps1` (plato-308 Prevention) covers the emitter side.
