> **EXECUTED 2026-07-28** — see tracker issue [plato-257](../tracker/issues/plato-257.md). Historical record; do not execute. Archived copy: [docs/archive/plato-257-lessons-v1-plan.md](archive/plato-257-lessons-v1-plan.md).

# plato-257 — Lessons V1: multi-agent authoring plan

**Status:** EXECUTED (2026-07-28). Tracker: [plato-257](../tracker/issues/plato-257.md) — closed.
**Key input:** [`stdlib/`](../stdlib/) — 80 files, 150 interfaces, 1111 types of
declared vocabulary (no bodies yet). Lessons teach the *mathematics and geometry the
vocabulary encodes*, using v3 type/interface names as the notation.

## V1 goal

Produce a **large collection of small, self-contained lessons**. Each lesson:

- stands entirely alone — **no references to other lessons, no index, no ordering,
  no "as we saw in…"**;
- is interesting and useful on its own (a reader with general programming background
  and high-school math learns one real thing);
- uses `stdlib` vocabulary as its notation for definitions and examples;
- ends with the author-agent's **recommendations for the Plato library** — things the
  act of teaching revealed as missing, awkward, or wrong in v3.

Cohesion comes later: a second pass will rewrite the survivors into a structured
curriculum with an index, cross-links, and interactive TypeScript figures
(the original plato-257 vision, still gated on plato-078). V1 deliberately skips all
of that — no site, no TS emission, no diagrams beyond ASCII/text, markdown only.

## Why v3 as input

v3 is declarations + doc comments only. That is a feature here: a lesson explains
*what a `Quaternion` is and why `Slerp` exists* — semantic content — without being able
to lean on runnable code. Writing lessons against the declared surface is also the
cheapest possible stress test of the vocabulary: every place an author-agent wants an
operation that isn't declared, or finds a declared name confusing, is exactly the
feedback the recommendations section captures.

## Output layout

```
submodules/Plato/lessons/
  v1/
    <slug>.md          one file per lesson, kebab-case slug from the catalog below
```

One file per lesson, nothing else. No index file, no shared assets, no README in V1
(the catalog in this plan is the provisional manifest). Each agent creates exactly one
new file and touches nothing else — this makes parallel agents collision-free.

## Lesson file format

```markdown
---
lesson: <slug>
title: <human title>
domain: <catalog group name>
v3-files: [08-vectors.plato, 10-rotations.plato]   # v3 files whose vocabulary is used
audience: <one line: assumed background>
status: draft-v1
---

# <Title>

<Body: see structure below.>

## Library recommendations

- **<type-of-issue>** — <specific, actionable recommendation, with the v3 file and
  declaration name it concerns, and one sentence on why the lesson surfaced it.>
```

Body structure (guideline, not straitjacket):

1. **Hook** (1–2 paragraphs): the concrete problem or surprise this idea solves.
   Never "In this lesson we will…".
2. **The idea**: the math/geometry, developed honestly. Formulas welcome (plain text
   or LaTeX-in-markdown `$...$`). Pictures as ASCII art where they help.
3. **In Plato**: how v3 encodes the idea. Show the relevant declarations (quoted from
   v3, abbreviated is fine) and 2–5 short *usage-shaped* snippets — expressions a user
   would write, e.g. `(p2 - p1).Normalize`. Snippets are illustrative Plato, not
   compiled; they must still only use names that exist in v3 (see rules).
4. **Pitfalls / fine print**: the classic mistakes (gimbal lock, lerp-vs-slerp,
   winding conventions, degenerate cases…).
5. **Try it** (optional): 2–3 pencil-and-paper or "predict the output" exercises with
   answers in a `<details>` block.
6. **Library recommendations** (required, even if the list is short).

Target length: **150–400 lines of markdown**. Small and dense beats long and thin.

## Hard rules for author agents

1. **Self-contained.** Do not mention, link, or assume any other lesson, an index, a
   chapter number, or "the textbook". Define every term you use or drop it.
2. **v3 names only.** Every Plato identifier in the lesson must exist in
   `stdlib`. Verify with the Plato navigation MCP (`plato_search_symbols`,
   `plato_definition` — see the `plato-mcp` skill) or by reading the source file.
   If the lesson *needs* an operation v3 lacks, do NOT invent it silently in the
   prose — show the gap explicitly ("v3 does not yet declare X") and put it in
   Library recommendations.
3. **Respect v3 semantics.** The doc comments and conventions in
   [`stdlib/README.md`](../stdlib/README.md) are normative: bare-number
   suffix = component count, `D` suffix = dimension of space, `Angle` never raw
   `Number`, sum types for variants, `Self` first parameter on interface functions.
   [`docs/plato-language-semantics.md`] (plato-261 reference) governs any language
   claims.
4. **Do not edit `stdlib` or anything else.** One new file in `lessons/v1/`,
   period. Recommendations are written down, not applied.
5. **No fabricated citations.** Wikipedia/textbook links welcome when the agent is
   confident of the URL; otherwise name the theorem/source without a link.
6. **Recommendations must be specific.** "Vectors could be richer" is useless.
   "`08-vectors.plato`: `Vector3D` has no `Reject(Self, Self)` companion to
   `Project` — the projection lesson needs both to explain decomposition" is the bar.
   Categories to use: `missing-function`, `missing-type`, `naming`, `doc-comment`,
   `wrong-shape`, `missing-interface`, `pedagogy` (something inexpressible/unteachable
   as declared).

## Recommendations aggregation

After a batch lands, the orchestrator (not the author agents) harvests every
`## Library recommendations` section into one review doc
(`docs/plato-257-lessons-v1-recommendations.md`), de-duplicates, and files tracker
issues for the ones worth acting on. Author agents never see or reconcile each
other's recommendations — duplication between lessons is expected and is itself
signal (three agents independently wanting `Clamp` on `Interval` is a strong vote).

## Lesson catalog (V1 candidates, ~60)

Each line = slug, one-line guidance handed to the agent, primary v3 files. Guidance
is deliberately high-level; the agent owns angle, depth, and examples.

### Foundations & vectors
| Slug | Guidance | v3 files |
|---|---|---|
| `points-vs-vectors` | Why points and vectors are different types; `Point3D − Point3D = Vector3D`; the `Difference` interface as affine geometry made typeful. | 02, 08, 11 |
| `tuples-vs-vectors` | `Number3` vs `Vector3D`: when three numbers are just three numbers; why "no `Vector3`" is a naming rule with teeth. | 08 |
| `dot-product` | The dot product as projection and as cosine; using it for "is it in front of me", facing checks, work. | 08 |
| `cross-product` | Cross product: area, normal, handedness; why it only exists in 3D (nod to `Bivector3D`). | 08, 10 |
| `norms-and-distance` | Length, distance, normalization; `Normed` and `MetricSpace` as the abstract shape; when to compare squared distances. | 02, 08 |
| `normalization-pitfalls` | The zero vector, near-zero vectors, and `Direction2D/3D` as "normalized by construction". | 08 |
| `linear-interpolation` | Lerp everywhere: numbers, points, colors; `Interpolatable`; extrapolation; why t is unitless. | 02, 08, 14 |
| `angles-as-types` | Why `Angle` is a type and not a `Number`: radians/degrees/turns bugs, wrapping, `AngleInterval`. | 06, 12 |
| `complex-numbers-rotate` | Complex multiplication as 2D rotation; the bridge to `Rotation2D` and `Rotor2D`. | 05, 10 |
| `units-in-types` | Dimensional analysis in the type system: `Length`, `Mass`, `Velocity`; what unit errors the compiler can now catch. | 06 |
| `time-is-not-a-number` | `Instant` vs `Duration`; frame time, timecode, tempo; the affine point/vector story again in 1D time. | 07 |

### Matrices & transforms
| Slug | Guidance | v3 files |
|---|---|---|
| `matrices-as-machines` | A matrix is a function; columns as images of basis vectors; reading a `Matrix3x3` at a glance. | 09 |
| `homogeneous-coordinates` | Why 3D graphics uses 4×4 matrices; points get w=1, vectors w=0; perspective divide preview. | 09, 11 |
| `trs-transforms` | Translate-rotate-scale: `Transform3D`; why composition order matters; decompose vs compose. | 13 |
| `coordinate-frames` | `Frame3D`/`Basis3D`; local vs world; change of basis as "the same point described twice". | 13 |
| `inverse-transforms` | Undoing transforms; rigid inverses are cheap; the inverse-transpose rule for normals. | 09, 13 |
| `pose-vs-transform` | `Pose3D` (position+orientation) vs full `Transform3D`; why rigid motion deserves its own type. | 13 |

### Rotations
| Slug | Guidance | v3 files |
|---|---|---|
| `euler-angles-and-gimbal-lock` | Euler angles: intuitive, order-dependent, and broken at the poles; `RotationOrder` as an explicit parameter. | 10 |
| `axis-angle` | Rotation as axis + angle; Rodrigues intuition; where it shines (physics, small rotations). | 10 |
| `quaternions-without-tears` | Quaternions as practical tools: what the 4 numbers mean, double cover, why q and −q are the same rotation. | 10 |
| `slerp` | Interpolating rotations: why lerp fails, what slerp preserves, the shortest-path flip. | 02, 10 |
| `rotors-and-bivectors` | The geometric-algebra view: `Bivector` as oriented plane, `Rotor` as rotation; how it generalizes where cross products don't. | 10 |
| `motors-dual-quaternions` | `Motor3D`: rotation+translation in one algebraic object; why skinning likes dual quaternions. | 10, 13 |

### Coordinate systems & bounds
| Slug | Guidance | v3 files |
|---|---|---|
| `polar-cylindrical-spherical` | The three classic alternative coordinate systems and when each makes a hard problem easy. | 11 |
| `barycentric-coordinates` | Barycentric coordinates: interpolation over triangles, point-in-triangle, the workhorse of rasterization. | 11 |
| `intervals-and-bounds` | `NumberInterval`, `Bounds2D/3D`: empty bounds, growing, union/intersection, why AABBs are everywhere. | 12 |
| `geo-coordinates` | Latitude/longitude as a coordinate system: `GeoCoordinate`, why Earth ruins flat-vector intuition. | 11, 68 |

### Geometry primitives
| Slug | Guidance | v3 files |
|---|---|---|
| `lines-rays-segments` | Three types for "straight": `Line`, `Ray`, `LineSegment`; parameterization; closest-point queries. | 16 |
| `planes-halfspaces` | `Plane` and `HalfSpace`; signed distance to a plane; which-side tests as the atom of clipping and BSP. | 16 |
| `triangle-geometry` | The triangle toolkit: area, normal, centroid, circumcenter; degenerate triangles. | 17, 18 |
| `circles-ellipses` | `Circle`, `Ellipse`: parameterization, tangents, why ellipse perimeter is famously hard. | 17 |
| `polygons-and-winding` | `Polygon2D`, winding order, signed area, `FillRule` (non-zero vs even-odd), holes. | 19, 41 |
| `convexity` | Convex vs concave: why convex shapes make everything easier (containment, collision, hulls). | 17, 19, 55 |
| `solid-primitives` | `Sphere`, `Box3D`, `Cylinder`, `Cone`, `Torus`, `Capsule3D`: parameterizations and surface areas/volumes. | 18 |
| `ray-intersection` | Ray vs sphere/plane/box: the algebra of the three classic intersections; `RayHit3D` as a result shape. | 16, 18, 35 |

### Curves & surfaces
| Slug | Guidance | v3 files |
|---|---|---|
| `parametric-curves` | Curves as functions of t; `Curve2D/3D` interfaces; parameter vs arc length; closed vs open. | 20 |
| `bezier-curves` | Bezier from de Casteljau up: control points, convex hull property, why fonts and UIs run on cubics. | 21, 22 |
| `interpolating-splines` | Catmull-Rom and Hermite: splines that pass *through* points; tangent choice; the animation connection. | 23 |
| `bsplines-and-nurbs` | From Bezier to B-spline to NURBS: local control, knots, weights, why CAD standardized on NURBS. | 23, 24 |
| `curvature-and-frames` | Curvature, Frenet frames, and the problem of twisting frames along a 3D curve. | 20, 22, 64 |
| `surfaces-of-revolution` | Lathe and extrude: `SurfaceOfRevolution`, `ExtrudedSurface`; how many classic solids are one curve + one sweep. | 24 |
| `helix` | The helix as the simplest truly 3D curve: pitch, handedness, springs and screws. | 22 |

### Fields, implicits & noise
| Slug | Guidance | v3 files |
|---|---|---|
| `signed-distance-fields` | SDFs: shape as a function; sign, gradient, the circle SDF derived by hand. | 26, 27 |
| `sdf-operations` | Union/intersect/subtract as min/max; smooth minimum; why booleans are trivial for SDFs and hard for meshes. | 27 |
| `noise` | Value vs gradient (Perlin) noise; octaves/fBM; why "random but smooth" needs design. | 28 |
| `scalar-vector-fields` | `ScalarField` and `VectorField`; gradient, divergence, curl in pictures. | 26 |
| `sampling-and-grids` | Regular grids as sampled fields; `Array2D` as an image of anything; bilinear reconstruction. | 29, 45 |

### Meshes & spatial structures
| Slug | Guidance | v3 files |
|---|---|---|
| `indexed-meshes` | `TriangleMesh3D`: vertex buffer + index buffer; why sharing vertices matters; typed indices (`VertexIndex`) vs raw ints. | 30, 31 |
| `mesh-normals` | Face vs vertex normals; smoothing by area/angle weighting; hard edges. | 31, 32 |
| `halfedge-topology` | Adjacency: from "triangle soup" to answering "what's next to what"; half-edges in pictures. | 30 |
| `point-clouds-voxels` | Points and voxels as the other 3D representations; when each beats a mesh. | 33 |
| `spatial-acceleration` | Grids, octrees, BVHs: why brute force dies and how spatial structures fix nearest-neighbor and raycast. | 34, 35 |

### Animation & motion
| Slug | Guidance | v3 files |
|---|---|---|
| `easing-functions` | Easing: why linear motion looks wrong; the classic ease families; `ClassicEasing`. | 36 |
| `keyframes-and-tracks` | `Keyframe<T>` / `AnimationTrack<T>`: animation as sampled functions of time; interpolation between keys. | 37 |
| `skeletal-animation` | Bones, skeletons, poses, skinning weights; the bind pose; where dual quaternions come back. | 38 |
| `springs-and-procedural-motion` | Spring-damper motion: `SpringParameters`; frequency/damping intuition; why springs beat tweens for reactive UI/camera. | 36, 39 |

### Color & imaging
| Slug | Guidance | v3 files |
|---|---|---|
| `linear-vs-gamma` | The single most common color bug: linear vs sRGB; why `Color` is linear and 8-bit is storage. | 14, 44 |
| `color-spaces` | HSV/HSL and friends: what "hue" is, why RGB is bad for picking and lerping colors. | 14, 44 |
| `images-as-functions` | `Image` interface: image as a function of position; sampling, filtering, `BlendMode` basics. | 45, 46 |

### Rendering
| Slug | Guidance | v3 files |
|---|---|---|
| `cameras-and-projection` | Perspective vs orthographic; FOV, near/far; the full point-to-pixel pipeline in one worked example. | 48 |
| `lights-and-materials` | PBR in plain words: albedo, roughness, metalness; energy conservation; what `Material` fields mean physically. | 49, 50 |

### Physics & simulation
| Slug | Guidance | v3 files |
|---|---|---|
| `rigid-bodies` | `RigidBody3D` and `MassProperties3D`: center of mass, inertia tensor intuition, linear vs angular state. | 54 |
| `collision-basics` | Broad phase vs narrow phase; separating axis idea; contact points; why capsules are beloved. | 55 |
| `numerical-integration` | Euler, semi-implicit Euler, Verlet: simulating motion step by step and why naive Euler explodes. | 53, 57 |

### Math, statistics & signals
| Slug | Guidance | v3 files |
|---|---|---|
| `random-and-distributions` | `RandomState` as a value (pure functional RNG); uniform vs normal; seeding and reproducibility. | 59 |
| `statistics-of-points` | `SummaryStatistics`, `Histogram`; mean/variance of point sets; centroid vs medoid. | 58 |
| `polynomials-and-roots` | `Polynomial`: evaluation (Horner), roots, where quadratics/cubics show up in geometry (intersections!). | 61 |
| `signals-and-sampling` | `SampledSignal`, Nyquist, aliasing; the same math behind audio and image moiré. | 60 |
| `floating-point-tolerance` | Why `a == b` is a lie for floats; `Tolerance`; absolute vs relative epsilon; robust predicates preview. | 63 |
| `optimization-basics` | Minimizing a function: gradient descent in 1D/2D pictures; where geometry needs it (closest points, fitting). | 62 |

## Agent brief template

Each author agent gets this prompt, filled in from the catalog row:

```
Write one self-contained lesson for the Plato project.

Lesson slug: <slug>
Title guidance: <guidance line from the catalog>
Primary v3 vocabulary files: <files>, under submodules/Plato/stdlib/

Read first:
- submodules/Plato/stdlib/README.md  (conventions — normative)
- the listed v3 files (the declarations + doc comments you will teach against)
- submodules/Plato/docs/plato-257-lessons-v1-plan.md, sections "Lesson file
  format" and "Hard rules for author agents" (normative for your output)

Deliverable: exactly ONE new file, submodules/Plato/lessons/v1/<slug>.md,
following the lesson file format. Create the lessons/v1 directory if needed.
Touch nothing else. Do not commit.

Constraints (repeated because they matter):
- Fully standalone: no references to other lessons, chapters, or an index.
- Every Plato identifier you use must exist in stdlib; verify via the
  plato-navigation MCP or by reading the source. Gaps go in the
  "Library recommendations" section, not silently into prose.
- 150-400 lines of markdown. End with "## Library recommendations" containing
  specific, file-and-declaration-level suggestions surfaced by writing this
  lesson (missing functions, naming problems, doc-comment fixes, shape issues).

Report back: one paragraph — what the lesson covers, plus your top 1-3 library
recommendations.
```

## Orchestration

- **Batching.** Run agents in waves of 5–8 (context/cost, and so early waves can
  refine the brief). Suggested wave 1 (spread across domains to maximize
  vocabulary coverage per wave): `points-vs-vectors`, `quaternions-without-tears`,
  `signed-distance-fields`, `bezier-curves`, `indexed-meshes`, `linear-vs-gamma`,
  `floating-point-tolerance`.
- **Isolation.** Each agent writes one new file; no shared files, no ordering
  dependencies — waves can run fully parallel. Agents do not read each other's
  lessons (independence is the point; convergent recommendations are signal).
- **After each wave** the orchestrator: (1) skims each lesson for rule violations
  (cross-references, invented identifiers — spot-check names against v3),
  (2) harvests recommendations into `docs/plato-257-lessons-v1-recommendations.md`,
  (3) commits the wave (`feat(plato-257): lessons v1 wave N — <slugs>`),
  (4) adjusts the brief if a systematic problem appeared.
- **Catalog is not sacred.** An agent may narrow its lesson's scope (better one
  sharp idea than three mushy ones) and should say so in its report. Adding new
  lesson ideas goes in the report, not in new files.

## Acceptance checklist (per lesson)

- [ ] Single new file at `lessons/v1/<slug>.md`, frontmatter complete.
- [ ] No mention of other lessons, chapters, ordering, or an index.
- [ ] Spot-checked Plato identifiers all resolve in `stdlib`.
- [ ] Teaches a real idea (a reviewer can state "what I learned" in one sentence).
- [ ] `## Library recommendations` present with ≥1 specific, file-level item
      (or an explicit "the vocabulary fully covered this lesson's needs").
- [ ] 150–400 lines; markdown renders cleanly (tables, `<details>`, math).

## Out of scope for V1 (explicitly)

- Index, table of contents, ordering, cross-links, prerequisites graph.
- TypeScript emission, runnable snippets, interactive figures, any website
  (blocked on plato-078; comes with the V2 rewrite).
- Editing `stdlib` — recommendations are recorded, triaged separately.
- Diagram tooling beyond ASCII art and math notation.
- Pedagogical sequencing/consistency between lessons (V2 rewrite's job).
