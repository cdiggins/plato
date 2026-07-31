---
id: plato-315
title: Carry legacy geometry concepts into stdlib: Curve generics, indexed-face access, unwelded face arrays, Procedural doc table; retire or wire Dimensioned
type: problem
status: done
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [submodules/Plato/stdlib-legacy/geometry.interfaces.plato, submodules/Plato/stdlib/geometry.concepts.plato, submodules/Plato/stdlib/curves.concepts.plato, submodules/Plato/stdlib/meshes.concepts.plato, submodules/Plato/stdlib/functional.concepts.plato, submodules/Plato/stdlib/geometry.library.plato, submodules/Plato/stdlib/meshes-lines-points.plato, submodules/Plato/stdlib/spatial-patches.plato, submodules/Plato/stdlib/topology.concepts.plato, tracker/issues/plato-320.md]
---

## Issue

One-time triage of `stdlib-legacy/geometry.interfaces.plato` (~40 interfaces, one deep
single-rooted lattice) against the forward `stdlib` geometry concepts, deciding which legacy
ideas still carry value and closing the question of the rest so it does not get re-litigated.
The new stack (orthogonal capability mixins: `geometry.concepts` markers +
`geometry-measures.concepts` + `geometry-queries.concepts`, `Procedural` unification, first-class
deformations, 13-representation `Transformable` lift in
`intervals-transforms-transformable.library.plato:44-97` vs legacy's single line at
`stdlib-legacy/meshes.library.plato:84`) is strictly better in structure and materially richer in
surface (`SupportMappable*`, `DifferentiableCurve*`, `FramedCurve3D`, `ArcLengthParameterized`,
`HalfEdgeNavigable`, bounded-SDF culling). Five legacy ideas are worth carrying forward, one
dead-concept class needs a decision, and the rest is explicitly left behind.

## Impact

The concrete cost today is duplication already in the tree: the Eval-only curve helpers are
written 2-3 times each (`curves.library.plato` — PointAt/StartPoint/MidPoint/EndPoint per
dimension; `curves-sampling.library.plato:32-53` — three `Sample` bodies differing only in range
type), and every future generic curve algorithm inherits that triplication. Secondary cost:
`QuadMesh3D` / `PolygonMesh3D` / `LineSet3D` have no concept-level face access (only
`TriangleMesh3D` does, via `TriangulatedGeometry3D`), so indexed-geometry algorithms are
per-type only. Third: stdlib has no unwelded face representation at all, so CSG / marching-cubes
/ tessellation / STL-import output has nowhere to live except a `TriangleMesh3D` carrying a
3n-entry identity index array — wasted storage that also falsely claims vertex sharing.
Deferral is safe but compounds as P6/P8 library bodies grow.

## Affected code

- `submodules/Plato/stdlib/curves.concepts.plato:16-30` — Curve1D/2D/3D as three unrelated
  `Procedural<Number, T>` instantiations; `PeriodicCurve<TRange>` (:52) and
  `ArcLengthParameterized<TPoint>` (:112) are the generic form already bolted on sideways.
- `submodules/Plato/stdlib/curves.library.plato`, `curves-sampling.library.plato` — the
  triplicated Eval-only bodies a generic root would deduplicate.
- `submodules/Plato/stdlib/fields.concepts.plato:16` — `Field<TDomain, TValue> inherits
  Procedural` with concrete instantiating children: the exact precedent proving the
  `Curve<TRange>` shape is expressible and survives the writers.
- `submodules/Plato/stdlib/meshes.concepts.plato:26-32` — `TriangulatedGeometry3D` is
  triangle-only, accessor-shaped; `submodules/Plato/stdlib/meshes.plato` QuadMesh3D /
  PolygonMesh3D and `meshes-lines-points.plato` LineSet3D have no face-access concept.
- `submodules/Plato/stdlib/geometry.concepts.plato:42` — `Dimensioned`; derived bodies
  `IsPoint/IsCurve/IsSurface/IsVolume` in `geometry.library.plato:31-50` are unreachable
  (zero `implements ... Dimensioned` tree-wide, verified by grep 2026-07-29).
- `submodules/Plato/stdlib-legacy/procedurals.plato:1-76` — the domain x range doc table worth
  lifting; its library body (:88-129) is 100% commented out (the combinators never worked).
- `submodules/Plato/stdlib-legacy/geometry.interfaces.plato`, `geometry.types.plato`,
  `meshes.library.plato` — the source stack being triaged.

## Cause / analysis

The forward stdlib was rebuilt from scratch (plato-257/plato-293) rather than ported, so a few
legacy generalizations were dropped without an explicit decision, and one concept
(`Dimensioned`) was declared to replace the legacy dimension-in-the-lattice encoding but never
wired to any type. Everything below was verified against source 2026-07-29; two claims from the
original recommendation were found wrong and are corrected here (see Non-goals items 1 and the
Polyline note).

### Carry forward, ranked

1. **`Curve<TRange>` — generic curve root** (effort S, risk low). Add
   `concept Curve<TRange> inherits Procedural<Number, TRange>` to `curves.concepts.plato`;
   re-root `Curve1D/2D/3D` on `Curve<Number>` / `Curve<Point2D>` / `Curve<Point3D>` (keeping
   their `Geometry2D/3D` parents); re-root `PeriodicCurve<TRange>` on it. Expressible today:
   `Field<TDomain,TValue>` + `ScalarField2D` is the identical pattern and already passes lint
   and the writers. Honest scope of the win: only Eval-only helpers deduplicate generically
   (PointAt family, Sample); Chord/ChordLength/VelocityAt need `Between`/`Magnitude` on TRange
   and stay per-dimension unless the concept grows `where TRange:` bounds (possible — precedent
   `algebra-metric.concepts.plato:49` — but a separate decision; do not add bounds in the first
   pass). File-partitioning: fits `curves.concepts.plato` (currently 8 concepts, cap 12).

2. **Generic indexed-face access** (effort M, risk low). Legacy
   `IIndexedGeometry3D<IndexT>` (`stdlib-legacy/geometry.interfaces.plato:224-228`) let
   `meshes.library.plato:7-9` write Line/Triangle/Quad extraction once for all indexed
   geometry. The forward equivalent should be **accessor-shaped, not array-shaped**, matching
   the deliberate `TriangulatedGeometry3D` design: add
   `concept FaceIndexedGeometry3D<TFace> { FaceCount; FaceAt(x, i): TFace; PositionAt(x, v): Point3D }`
   (typed `FaceIndex`/`VertexIndex` args per CONVENTIONS), implement on `TriangleMesh3D`
   (TriangleFace), `QuadMesh3D` (QuadFace), `LineSet3D` (VertexPair); consider re-rooting
   `TriangulatedGeometry3D` as `FaceIndexedGeometry3D<TriangleFace>`. `PolygonMesh3D` stays out
   (jagged faces have no fixed TFace; its `Jagged` access already covers it). Do NOT port the
   legacy `Points/FaceIndices` array-returning shape — it fights the accessor convention the
   new file chose. Extending implemented concepts obliges every implementor (LIBRARIES.md rule
   6): fill the new obligations in the same change.

3. **Lift the `procedurals.plato` doc prose — table only** (effort S, risk none). The
   domain x range table (legacy :49-75: `Procedural<Vector2,Number>` = 2D SDF,
   `Procedural<Integer3,Vector3>` = voxel color volume, ...) is genuinely good orientation and
   `functional.concepts.plato` (36 lines) has none of it. Constraint: STYLE_GUIDE forbids
   essay comments — lift a compressed (~12-line) table into the `functional.concepts.plato`
   header banner, updated to forward names (`Point2D` not `Vector2`, note which rows already
   have named concepts: ScalarField2D, ImplicitRegion2D, ...). Do NOT port the legacy
   combinators (Combine/Map/MapDomain/Compose/Union/Threshold): they are 100% commented out in
   legacy (never worked), and `fields-implicits-core.library.plato` /
   `implicit-sdf-operators.library.plato` already provide the working SDF/field combinators.

4. **Unwelded face-array types: `TriangleArray3D`, `QuadArray3D`** (effort S, risk low;
   user-requested 2026-07-29, promoted from the original review's non-goal). Legacy's soup types
   were `Points3D/Lines3D/Triangles3D/Quads3D` (`stdlib-legacy/geometry.types.plato:203-228`).
   Triaged individually against current stdlib:
   - `Points3D` — **already present** as `PointCloud3D` (`meshes-lines-points.plato:31`), same
     representation under a better name. Do not re-add; that is the `IDistanceField` two-names
     error.
   - `Lines3D` — **nearly covered** by `LineSet3D` (`meshes-lines-points.plato:12`), which is
     indexed (`Positions` + `Array<VertexPair>`) rather than unwelded, so it is a different
     representation and not merely a rename. Still: no consumer demands the unwelded line form.
     Leave out until one appears.
   - `Triangles3D` / `Quads3D` — **real gap.** `Array<Triangle3D>` and `Array<Quad3D>` appear
     nowhere in stdlib, though the element types exist (`spatial-patches.plato:24,35`). The
     unwelded form is the canonical output of CSG, marching cubes, tessellation, and STL import,
     and carries a distinct invariant — *no vertex sharing*. Forcing it into `TriangleMesh3D`
     requires a 3n-entry identity index array: pure waste that also lies about sharing.

   **Naming** (settled 2026-07-29 after the "soup" spelling was rejected): use
   `TriangleArray3D` / `QuadArray3D`, field `Triangles: Array<Triangle3D>`, in a new
   `meshes-face-arrays.plato` per the one-kind-per-file rule. Rationale — `Array` is this
   library's word for immutable direct storage, which is exactly the invariant, and it puts the
   contrast with `TriangleMesh3D` on the axis that matters (array-of-faces vs indexed-into-
   positions). `TriangleList3D` is **excluded**: `List<T>` is a real stdlib type, a `unique`
   mutable growable builder with `Add`/`Freeze` (`primitives-builders.plato:48`), so the name
   would advertise the opposite of an immutable value. `TriangleSet3D` is excluded because `Set`
   already means *indexed* here (`LineSet3D`). Runner-up if the invariant should be louder than
   the storage: `UnweldedTriangles3D`, which also teaches the `Weld()` conversion verb (`Weld`
   is unused in stdlib geometry; `WeldParameters` in `engineering-machine-elements.plato:76` is
   an unrelated fabrication noun) — costs a plural head noun against convention. Note
   `topology-adjacency.plato:27` `UndirectedEdgeList` is a pre-existing `List`-suffix wart; do
   not extend the pattern, and review that name separately.

   **Concepts to implement:** `Value`, `Geometry3D`, `Meshable3D` (weld to `TriangleMesh3D`),
   `Bounded3D`, `PointSet3D`, `Deformable3D`. **Must NOT implement `MeshElementCounts`** —
   `topology.concepts.plato:23-27` forbids guessing a deduplicated undirected edge count, and an
   unwelded array has 3n edge slots with unknown dedupe, so it would hand back a confidently
   wrong Euler characteristic. A type that cannot satisfy the concept is the design working as
   intended, not a constraint to engineer around (see plato-320 for what happens when an
   implementor guesses instead).

5. **`PrimitiveCollection2D/3D<TPrimitive>` — dependent on item 4** (originally rank 2,
   downgraded to conditional in review, re-promoted to dependent once item 4 was accepted;
   effort S, risk low). Legacy `IPrimitiveGeometry2D/3D<T>` (`Primitives(x: Self): Array<T>`).
   With item 4 landed the concept has 2-3 real implementors (`TriangleArray3D`, `QuadArray3D`,
   plus `PointCloud3D` if its `Positions` is read as the primitive array), so the zero-
   implementor objection — the `Dimensioned` failure mode, and STYLE_GUIDE's
   no-speculative-abstraction rule — no longer applies. Order matters: land item 4 first, then
   this, never this alone. The `where T:` element bound (legacy's `IGeometricPrimitive2D/3D`
   fixed-point-count constraint) is expressible (`algebra-metric.concepts.plato:49` precedent)
   but is a separate decision; the first pass should be unconstrained.

### Decision to make: dead concepts carrying live derived bodies

`Dimensioned` (`geometry.concepts.plato:42`) has zero implementors, yet
`geometry.library.plato:31-50` ships four derived predicates on it — an unreachable API
surface. Legacy encoded intrinsic dimension in the lattice (`IShape` = topological dim 1); the
forward design made it a queryable and then nobody wired it. Either (a) populate it: make the
family concepts (`Curve2D/3D`, `Surface`, `Solid`, `PointSet*`...) inherit `Dimensioned` and
supply constant bodies per family in `geometry.library.plato`, or (b) retire the concept and
its four predicates. (a) is small and makes the fact queryable in generic code; (b) is honest
if no consumer materializes. Do not leave the current half-state.

**`Dimensioned` is not alone (found 2026-07-29).** `MeshIncidence`
(`topology.concepts.plato:45`) also has **zero implementors** tree-wide, while
`meshes-topology.library.plato:66-118` ships **eleven** derived bodies on it — a larger
unreachable surface than `Dimensioned`'s four. The middle rung of the topology ladder is
currently decoration, and its absence is what let `PolygonMesh3D` take a closed-manifold
shortcut on its edge count instead of the prescribed incidence route (**plato-320**). Unlike
`Dimensioned`, the answer here is not a coin flip: the rung has a named implementor waiting
(`PolygonMesh3D`), so populate rather than retire. Treat both under one decision so the class is
closed at once, and count the fix as satisfying plato-320's option 2.

### Non-goals — the leave-behind list (do not re-litigate)

- **`Closed(x): Boolean` capability concept ("ClosureQueryable")** — DROPPED; the original
  recommendation's motivating claim is factually wrong: forward `Polyline2D/3D/N`
  (`polygons-polylines.plato:11-34`) all carry a `Closed: Boolean` field, exposed as an
  accessor and consumed by `polygons-polylines.library.plato` (`ChainLength(p.Closed)`).
  Closure-as-data exists where it is data; closure-as-guarantee is the `ClosedShape` /
  `ClosedCurve*` / `ClosedSurface` markers. Revisit only if a generic consumer appears.
- The `IShape/IShape2D/IShape3D/IOpenShape2D/IClosedShape3D` lattice — pure
  {dim} x {open,closed} x {kind} naming cross-product; the orthogonal markers replaced it.
- `IOpenCurve2D/3D` twins — `Curve2D/3D` + `OpenShape` marker covers it; stdlib deliberately
  has only the `ClosedCurve*` refinements.
- `IDistanceField2D/3D` with a separate `Distance()` beside `Eval` — two names for one
  function; `SignedDistanceField2D/3D` (Eval only, `DistanceAt` alias in the library) is right.
- `ISolid inherits IProceduralSurface` — conflates a solid with one UV parameterization of its
  boundary; `Solid` marker + `ClosedSurface` is right.
- `IImplicitProcedural<TDomain>` — only two instantiations ever; concrete `ImplicitRegion2D` +
  `ImplicitVolume3D` are fine.
- `IExplicitSurface` — already present as `HeightFieldSurface`, better named and correctly
  domained on `Point2D` rather than normalized UV.
- Legacy `procedurals` library combinators — never functional (fully commented out); the
  forward field/SDF libraries own that ground.

## Priority

p2: item 1 removes duplication that already exists and grows with every P6 body; items 2-3 are
cheap and unblock generic mesh algorithms in P8. Nothing is release-blocking (codegen still
ships from stdlib-legacy), so not p1. Deferral cost is linear, not explosive.

## Dependencies

- Touches: `curves.concepts.plato` + curve libraries (P6 package), `meshes.concepts.plato` +
  mesh libraries (P8), `geometry.concepts.plato`/`geometry.library.plato` (P5),
  `functional.concepts.plato` (P2). Concurrent stdlib package work would collide; land as
  small separate commits per item.
- Gate: `Plato.CLI lint stdlib` zero errors + the ForwardStdLib test (`tools/check-stdlib-fast.ps1`)
  after each item.
- **plato-320** (`PolygonMesh3D.UndirectedEdgeCount` closed-manifold guess) shares the
  `topology.concepts` / `meshes-topology.library` seam: it is an independent correctness bug and
  should land first, but the dead-`MeshIncidence` half of the decision above is also its option-2
  fix path. Do not run both concurrently.

## Fix approaches

1. Land items 1-3, then item 4 followed by item 5, then the dead-concept decision — six small
   independent commits (preferred — each is separately revertible and gate-checked). Only the
   4-before-5 order is mandatory.
2. Fold items 1-2 into the next P6/P8 library-body wave (risks scope drift; LIBRARIES.md rule 6
   requires concept extension to be its own justified change anyway).
3. Capture-only: accept this issue as the record and do nothing until a consumer forces each
   item (defensible for item 2; wasteful for item 1, whose duplication is already paid daily).

## Bedrock

The invariant this strengthens: **every generic family in stdlib has a generic root when a
generic algorithm exists** (`Field` has one; curves currently do not), and **no concept ships
without an implementor** (the `Dimensioned` half-state violates it; item 4's downgrade enforces
it prospectively). Seam: `curves.concepts.plato` / `meshes.concepts.plato` concept roots, which
every P6/P8 library body constrains on — future curve/mesh algorithms get written once instead
of per-dimension. Verdict: **simplest-along-the-grain** — the simple version must NOT port
legacy's array-shaped `IIndexedGeometry3D` surface or add `where` bounds to `Curve<TRange>` in
the first pass, so the accessor-shaped and unconstrained designs stay reachable.

## Done means

- [x] `Curve<TRange>` added; Curve1D/2D/3D + PeriodicCurve re-rooted; at least the
      triplicated `Sample` bodies collapsed to one generic; lint + ForwardStdLib gate pass.
      (7d4c12e) `Sample` x3 and `SamplePeriodic` x2 collapsed to one body each.
      **Correction to the analysis above:** `curves.concepts.plato` was at **12/12**
      declarations, not 8/12 — the file was full, so the capability concepts moved to a new
      `curves-capabilities.concepts.plato`, matching the stem of the library that already
      implements them. The split was required to land the item at all.
- [x] **DROPPED by decision, not deferred — `FaceIndexedGeometry3D<TFace>` should not be added.**
      Superseded by the `MeshIncidence` box below, which delivers what this item wanted. Three
      findings from attempting it:
      (1) It cannot declare `FaceCount`. `TriangleMesh3D` / `QuadMesh3D` already take
      `FaceCount` from `MeshElementCounts`, and same-name members arriving from two implemented
      concepts collide — the tree already documents this at
      `meshes-geometry.library.plato:69-72`, where `IsEmpty` had to become `HasNoTriangles` for
      exactly that reason. So the generic concept would carry accessors but no count.
      (2) Without `TFace`'s arity nothing is derivable generically. The legacy win this item
      cited (`stdlib-legacy/meshes.library.plato:7-9`) is **three separate functions**, one per
      arity — not one generic body. Re-checking the legacy source, the generic root was never
      what bought that.
      (3) The member that would give it teeth is `FaceCorners(face): Array<VertexIndex>` — which
      is precisely `MeshIncidence.VerticesOfFace`, already declared and unimplemented.
      Verdict: skip the new concept, populate `MeshIncidence` instead (next box). Note
      `TriangulatedGeometry3D`'s own obligations have no bodies for `TriangleMesh3D` today
      either; they are part of the 228 open LINT001 findings.
- [x] Compressed domain x range table present in `functional.concepts.plato` header. (7d4c12e)
- [x] `TriangleArray3D` / `QuadArray3D` added in `meshes-face-arrays.plato` with `Meshable3D`
      welding to `TriangleMesh3D`, and NOT implementing `MeshElementCounts`; gate passes.
      (815883e) Bodies for `Points` / `ToTriangleMesh` / `Deform` / `Primitives` landed with
      them; `Bounds` comes free from `PointSet3D` and is tight.
- [x] `PrimitiveCollection3D<TPrimitive>` added on top of them; never landed before the
      face-array types. (815883e) 3D only — no 2D counterpart, since no 2D implementor exists
      and a zero-implementor concept is the failure mode this issue is closing.
- [x] `Dimensioned` deleted along with its four derived bodies (`geometry.library.plato`);
      no unreachable derived surface remains from it. (fe6b67a) Retire chosen over populate:
      intrinsic dimension is already implied by family membership (Curve*/Surface/Solid), and
      the number is ambiguous for the types that would supply it — `Polygon2D` is dimension 2
      read as a region, 1 read as its boundary. Re-adding costs three lines.
- [x] **`MeshIncidence` populated** — `PolygonMesh3D` implements all six queries in the new
      `meshes-polygon-incidence.library.plato` (0aa7331), so the eleven derived bodies in
      `meshes-topology.library.plato:66-118` are reachable and the dead-concept class is fully
      closed. Design notes: every body is a corner scan rather than a ring walk, so unlike
      `meshes-polygon-corners.library.plato` none of them require a closed manifold — a boundary
      edge reports one incident face. `FacesOfUndirectedEdge` reads the naming corner and its
      twin instead of scanning, keeping it quadratic rather than cubic; the other vertex-side
      scans are cubic, which is the same trade the corner library already took.
      Known limitation, documented in the file rather than hidden: `TwinCorner` resolves to a
      single opposite corner, so an edge shared by 3+ faces is unrepresentable and
      `IsNonManifoldUndirectedEdge` can never fire for this type.
      **Verification limit:** type-checked only (lint `--strict` 0 errors, checker ratchet no
      regression). Forward conformance cannot execute, so the six queries have no runtime test;
      this is the same limit recorded on plato-320's outstanding box.
- [x] Non-goals list above acknowledged (no re-adds). The `ClosureQueryable` correction stands:
      `Polyline2D/3D/N` do carry `Closed`.

## Simplest fix

Item 1 alone: add the three-line `Curve<TRange>` concept, re-root four concepts, collapse the
three `Sample` bodies. Gets the daily-paid duplication; gives up the mesh-access and doc wins
until later.

## Prevention

- A lint rule flagging concepts with zero implementors AND derived library bodies would catch
  this class mechanically — candidate /track-idea. Two instances found so far (`Dimensioned`: 4
  bodies; `MeshIncidence`: 11), and the second one directly enabled a correctness bug
  (plato-320), so this is not a tidiness rule.
- LIBRARIES.md rule 6 (grep `implements <Concept>` before extending) already prevents the
  reverse failure; this issue adds the forward direction.
