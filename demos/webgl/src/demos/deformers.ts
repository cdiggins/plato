// Deformers — a scene catalog over `stdlib/geometry/deformations.*.plato`.
//
// Every warp here is a value of one of the catalog types in
// `deformations.types.plato` (Twist3D, Bend3D, Taper3D, Shear3D, Spherify3D,
// Twist2D, Shear2D, Taper2D, MappingDeformation2D/3D) and every point that moves is moved
// by the generated `Eval` body from `deformations.library.plato`. This file
// builds the parameters, hands `Eval` to `PolygonMesh3D.Deform(mapping)` /
// `Polygon2D.Deform(mapping)`, and repacks the result — it never writes a warp
// formula of its own.
//
// Normals: the library header is explicit that `Eval` moves points only and that
// normals must be recomputed from the new positions. `polygonMeshGeometry` calls
// `computeVertexNormals`, so the flat-shaded result is correct for free; there is
// no analytic-Jacobian path in the library and none is faked here.
//
// Cost. `build` runs on every slider tick, so the subject mesh is memoized per
// (seed, truncate rounds) and a tick redoes only the deform and the repack. The
// planar subjects are memoized the same way, per subject index — none of them
// depends on a slider, so a drag only re-warps and re-measures them.
//
// Two things set the cap. `Truncate` is the subdivider rather than `Ambo`
// because `Ambo` is far more expensive on the same seeds — but `Truncate` is
// itself quadratic in vertex count, and it runs about fifteen times slower in
// the browser than under tsx, so the round that takes a second in a script is a
// multi-second freeze on the page. Every seed therefore gets its own round cap,
// chosen to land the subject near 216 vertices / ~430 triangles; the status line
// reports the rounds actually used. Separately, every generated member returns a
// lazily mapped `Positions`, so the memoized subject is read out into a flat
// array once (see `subject`) — without that a tick at the cap costs seconds
// rather than milliseconds, because `build` walks the chain three times.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import {
  fromArray,
  point2D,
  polygonMeshEdges,
  polygonMeshGeometry,
  toArray,
  triangleArrayGeometry,
} from '../shared/mesh.js';
import { rasterPlane, type Rgb } from '../shared/raster.js';
import { edgeMaterial, palette, surfaceMaterial } from '../shared/viewer.js';
import {
  Angle,
  AngleInterval,
  Annulus,
  Antiprism,
  Bend3D,
  Bounds3D,
  Capsule2D,
  Capsule3D,
  Cardioid2D,
  Circle,
  CircularArc2D,
  Direction2D,
  Direction3D,
  EllipticalArc2D,
  Frame3D,
  FunctionSdf2D,
  FunctionSdf3D,
  IntegerVector2,
  IntegerVector3,
  ItemIndex,
  Limacon2D,
  MappingDeformation2D,
  MappingDeformation3D,
  Number2,
  PerlinNoise3D,
  Point2D,
  Point3D,
  Polygon2D,
  PolygonMesh3D,
  PolygonSet2D,
  PolygonWithHoles2D,
  Polyline2D,
  RegularPolygon,
  RegularPrism,
  RegularStar2D,
  Rotation2D,
  SdfBendModifier3D,
  SdfDisplacementModifier3D,
  SdfElongationModifier2D,
  SdfElongationModifier3D,
  SdfOnionModifier,
  SdfRepetitionModifier2D,
  SdfRepetitionModifier3D,
  SdfRoundingModifier,
  SdfShellModifier,
  SdfTwistModifier3D,
  Shear2D,
  Shear3D,
  Sphere,
  Spherify3D,
  Taper2D,
  Taper3D,
  Triangle2D,
  Twist2D,
  Twist3D,
  Vector2D,
  Vector3D,
  type IArray,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';
import type { ViewerOptions } from '../shared/viewer.js';

// ---------------------------------------------------------------------------
// Subject meshes
// ---------------------------------------------------------------------------

const SUBJECT_LABELS = [
  'Column (12-gon prism ▸ truncate)',
  'Capsule (cube ▸ truncate ▸ sphere ▸ ScaleY)',
  'Capsule (icosahedron ▸ truncate ▸ sphere ▸ ScaleY)',
  'Faceted (cube ▸ truncate ▸ ScaleY)',
  'Column (7-gon antiprism ▸ truncate)',
];

/**
 * Each subject is stretched to roughly the same envelope — about a unit across,
 * about three tall — so the camera framing and the slider ranges mean the same
 * thing whichever one is picked. The prism is already that tall; the rest get
 * there through the non-uniform `ScaleY`, whose factor differs because a
 * truncated cube's extreme vertex sits at 1/sqrt(3) where a sphere's sits at 1.
 */
const SUBJECT_STRETCH = [1, 1.5, 1.5, 2.6, 1];

/**
 * How many rounds of `Truncate` each seed can afford. `Truncate` is quadratic in
 * vertex count, so the cap is per seed rather than global: the prism starts with
 * 24 vertices and the cube with 8, and one more round on the prism costs an
 * order of magnitude more than one more round on the cube for the same result.
 */
const SUBJECT_MAX_ROUNDS = [2, 3, 2, 3, 2];

/**
 * A column standing along world Y: the prism's own axis is its frame's Z, so the
 * frame is the world basis rolled a quarter turn (X, -Z, Y — right-handed).
 */
const COLUMN_FRAME = new Frame3D(
  new Point3D(0, 0, 0),
  new Direction3D(new Vector3D(1, 0, 0)),
  new Direction3D(new Vector3D(0, 0, -1)),
  new Direction3D(new Vector3D(0, 1, 0)),
);

const seeds: Array<() => PolygonMesh3D> = [
  () => new RegularPrism(COLUMN_FRAME, 12, 1, 3).ToPolygonMesh(),
  () => PolygonMesh3D.Cube(),
  () => PolygonMesh3D.Icosahedron(),
  () => PolygonMesh3D.Cube(),
  // The other polygon-derived seed: an antiprism is the same regular n-gon
  // profile as the prism, with the top ring rolled half a side so the wall is a
  // band of triangles rather than quads — a twist reads differently on it.
  () => new Antiprism(COLUMN_FRAME, 7, 1, 3).ToPolygonMesh(),
];

/**
 * `Truncate` is the subdivider: it adds a ring of vertices around every old
 * vertex, so a two-ring prism grows real resolution along its length. `Ambo`
 * would do the same but costs seconds past three rounds. Memoized per level so
 * dragging the detail slider back and forth is free.
 */
const truncateCache = new Map<string, PolygonMesh3D>();

function truncated(seed: number, rounds: number): PolygonMesh3D {
  const key = `${seed}:${rounds}`;
  const hit = truncateCache.get(key);
  if (hit) return hit;
  const mesh = rounds <= 0 ? seeds[seed]() : truncated(seed, rounds - 1).Truncate();
  truncateCache.set(key, mesh);
  return mesh;
}

const subjectCache = new Map<string, PolygonMesh3D>();

/** The undeformed subject: a densely tessellated column or ball. */
function subject(params: Params): PolygonMesh3D {
  const kind = clampIndex(params.subject, SUBJECT_LABELS.length);
  const rounds = subjectRounds(params);
  const key = `${kind}:${rounds}`;
  let base = subjectCache.get(key);
  if (!base) {
    const t = truncated(kind, rounds);
    const rounded = kind === 1 || kind === 2 ? t.ProjectedToUnitSphere() : t;
    const stretch = SUBJECT_STRETCH[kind];
    const built = stretch === 1 ? rounded : rounded.ScaleY(stretch);
    // Every generated member above returns a lazily mapped `Positions`, so by the
    // third `Truncate` a single `Positions.At(i)` walks a deep chain of Map and
    // Concatenate views — and `build` walks all of them three times per tick
    // (surface, edges, ghost). Reading the chain once into a flat array here,
    // behind the memo, is what keeps a slider tick cheap; the faces need no such
    // treatment, they are already flat enough to be free.
    base = new PolygonMesh3D(fromArray(toArray(built.Positions)), built.Faces);
    subjectCache.set(key, base);
  }
  return base;
}

/** The rounds this subject will actually run, after its own affordability cap. */
function subjectRounds(params: Params): number {
  const kind = clampIndex(params.subject, SUBJECT_LABELS.length);
  const asked = Math.max(1, Math.min(3, Math.round(params.detail ?? 2)));
  return Math.min(asked, SUBJECT_MAX_ROUNDS[kind]);
}

/** A cheap ball, used as the wireframe indicator for the falloff sphere. */
const indicatorBall = (): PolygonMesh3D => truncated(1, 2).ProjectedToUnitSphere();

// ---------------------------------------------------------------------------
// Axes and origins
// ---------------------------------------------------------------------------

const AXIS_LABELS = ['X', 'Y (long axis)', 'Z'];
const AXIS_VECTORS = [new Vector3D(1, 0, 0), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1)];

function axis(index: number): Direction3D {
  return new Direction3D(AXIS_VECTORS[clampIndex(index, 3)]);
}

/** A point at `t` along `a` from the world origin. */
function originOn(a: Direction3D, t: number): Point3D {
  return a.Vector.Multiply(t).ToPoint();
}

function clampIndex(value: number | undefined, count: number): number {
  const i = Math.round(value ?? 0);
  return i < 0 ? 0 : i >= count ? count - 1 : i;
}

// ---------------------------------------------------------------------------
// Rendering
// ---------------------------------------------------------------------------

/** The generated members every 3D scene reaches through to build its subject. */
const SUBJECT_MEMBERS = [
  'PolygonMesh3D.Deform',
  'RegularPrism.ToPolygonMesh',
  'Antiprism.ToPolygonMesh',
  'PolygonMesh3D.Truncate',
  'PolygonMesh3D.ProjectedToUnitSphere',
  'PolygonMesh3D.ScaleY',
];

const SUBJECT_CONTROLS: Control[] = [
  { key: 'subject', label: 'Subject', kind: 'select', options: SUBJECT_LABELS, def: 0 },
  { key: 'detail', label: 'Truncate rounds (capped per subject)', kind: 'slider', min: 1, max: 3, step: 1, def: 3 },
  { key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 },
  { key: 'ghost', label: 'Ghost the undeformed mesh', kind: 'toggle', def: 1 },
];

/**
 * What the last `build` measured, reported under the description. Set by the two
 * build helpers and read back by each scene's `status`, which the shell calls
 * straight after `build` on the same parameters.
 */
let lastStatus = '';
const status = (): string => lastStatus;

/**
 * Apply a point map with the generated `PolygonMesh3D.Deform` and repack it.
 * `polygonMeshGeometry` recomputes vertex normals from the moved positions,
 * which is what the library's normals contract requires.
 */
function warped(params: Params, mapping: (p: Point3D) => Point3D): THREE.Object3D {
  const base = subject(params);
  const deformed = base.Deform(mapping);

  let triangles = 0;
  for (let f = 0; f < deformed.FaceCount(); f++) triangles += deformed.FaceArity(f) - 2;
  let farthest = 0;
  for (let i = 0; i < deformed.Positions.Count(); i++) {
    farthest = Math.max(farthest, deformed.Positions.At(i).Distance(base.Positions.At(i)));
  }
  const asked = Math.max(1, Math.min(3, Math.round(params.detail ?? 2)));
  const rounds = subjectRounds(params);
  lastStatus =
    `${deformed.VertexCount()} verts · ${deformed.FaceCount()} faces · ${triangles} tris` +
    ` · truncate x${rounds}${rounds < asked ? ` (capped from ${asked})` : ''}` +
    ` · farthest vertex moved ${farthest.toFixed(3)}`;

  const group = new THREE.Group();
  group.add(new THREE.Mesh(polygonMeshGeometry(deformed), surfaceMaterial()));
  if (params.edges) group.add(new THREE.LineSegments(polygonMeshEdges(deformed), edgeMaterial()));
  if (params.ghost) group.add(new THREE.LineSegments(polygonMeshEdges(base), edgeMaterial(palette.line)));
  return group;
}

// ---------------------------------------------------------------------------
// Planar scenes: a lattice and a polygon subject, both warped by an IDeformation2D
//
// `Twist2D.Eval` reaches `Vector2D.Transform(Rotation2D)`, an overload the
// writer drops; `src/plato/array-ext.ts` dispatches it, the same patch it
// already carried for `Transform(Quaternion)` one dimension up.
//
// The subject is carried as a `PolygonSet2D` whatever it is, because that is the
// one type whose generated members cover every case in the list below: a set of
// components, each a boundary ring plus its holes. `PolygonSet2D.Deform` warps
// every ring of every component through `PolygonWithHoles2D.Deform` and
// `Polygon2D.Deform`, and `Area` / `Perimeter` chain down the same way — so a
// plain 5-gon and a plate with three holes are measured by exactly one code
// path, and the holes come along with the boundary for free.
//
// The area reading is the point of several of these maps. `Twist2D` is a
// rotation about a moving centre, so it is rigid and area is exactly preserved
// (the vertex loop is, at least — the CHORDS between vertices stretch, which is
// why perimeter grows on a coarse ring and barely moves on a fine one). A
// `Taper2D`'s local area factor is its own (1 + Rate * t), so a subject centred
// on Origin keeps its area as well — what one side gains the other loses. Drag
// Origin off centre, or pick a subject that does not straddle it, and the
// reading moves: `Sector` and `Segment` sit entirely on one side of the origin
// and report a real area change where the centred rings report none.
// ---------------------------------------------------------------------------

/**
 * The planar scenes get their own stage: an orthographic camera looking straight
 * down -Z, which is what `Scene.viewer` exists for. The spatial scenes keep the
 * page's perspective camera and its auto-spin.
 */
const PLANAR_VIEWER: ViewerOptions = { orthographic: true, grid: false, spin: false, distance: 7 };

const PLANAR_EXTENT = 1.6;
const PLANAR_SAMPLES = 41;

// --- ring construction -----------------------------------------------------

const turnsAngle = (t: number): Angle => t.Turns();
const ORIGIN_2D = point2D(0, 0);
const FULL_TURN = new AngleInterval(turnsAngle(0), turnsAngle(1));

const ring = (points: IArray<Point2D>): Polygon2D => new Polygon2D(points);

/** A region: one boundary ring and however many hole rings. */
const region = (boundary: Polygon2D, ...holes: Polygon2D[]): PolygonWithHoles2D =>
  new PolygonWithHoles2D(boundary, fromArray(holes));

const polygonSet = (...parts: PolygonWithHoles2D[]): PolygonSet2D => new PolygonSet2D(fromArray(parts));

const ngon = (sides: number, radius: number, center = ORIGIN_2D, rotation = 0): Polygon2D =>
  ring(new RegularPolygon(center, radius, sides, turnsAngle(rotation)).RegularPolygonVertices());

/**
 * `count` points along a circular arc, endpoints included — `Sample` is the
 * open-curve sampler, which is what a piece of boundary wants; the closed-curve
 * subjects below use `SamplePeriodic` instead so the seam is not duplicated.
 */
const arcPoints = (center: Point2D, radius: number, from: number, to: number, count: number): Point2D[] =>
  toArray(new CircularArc2D(center, radius, new AngleInterval(turnsAngle(from), turnsAngle(to))).Sample(count));

/** One ring from several point runs — repacking, no geometry of its own. */
const ringOf = (...runs: Point2D[][]): Polygon2D => ring(fromArray(runs.flat()));

// --- the subject catalog ---------------------------------------------------

/**
 * A range of closed rings, all of them from the stdlib's named planar shapes and
 * closed plane curves. Two things they are chosen to show:
 *
 *  - A five-gon and a five-pointed star make it obvious that `Eval` moves
 *    VERTICES, not edges: a swirl bends a 96-gon into a smooth spiral, but on a
 *    5-gon it just drags five corners around and leaves the five straight edges
 *    straight. The fine rings hide that entirely.
 *  - `Annulus`, `Plate + 3 holes` and `3 disks` deform more than one ring at a
 *    time, which is what `PolygonWithHoles2D.Deform` and `PolygonSet2D.Deform`
 *    exist for — the holes swirl with the boundary that contains them.
 *
 * Sizes are chosen against `PLANAR_EXTENT` so every subject sits inside the
 * lattice, and point counts against the tick budget: the lattice already costs
 * `2 * lines * 41` evaluations, so the densest subject here stays near 120
 * points and the whole list is memoized by index.
 */
const PLANAR_SUBJECTS: Array<{ label: string; build: () => PolygonSet2D }> = [
  { label: '5-gon', build: () => polygonSet(region(ngon(5, 1.2))) },
  { label: '12-gon', build: () => polygonSet(region(ngon(12, 1.1))) },
  { label: '96-gon', build: () => polygonSet(region(ngon(96, 1))) },
  {
    label: '5-star',
    build: () =>
      polygonSet(region(ring(new RegularStar2D(ORIGIN_2D, 1.3, 0.55, 5, turnsAngle(0.25)).RegularStarVertices()))),
  },
  {
    label: 'Ellipse',
    // The ellipse REGION type carries no vertices, so the ring is its boundary
    // curve: a full-turn EllipticalArc2D sampled periodically.
    build: () =>
      polygonSet(region(ring(new EllipticalArc2D(ORIGIN_2D, new Number2(1.35, 0.75), turnsAngle(0), FULL_TURN).SamplePeriodic(72)))),
  },
  {
    label: 'Sector',
    // A pie slice: the centre, then the arc between the two bounding radii.
    build: () => polygonSet(region(ringOf([ORIGIN_2D], arcPoints(ORIGIN_2D, 1.35, -0.17, 0.17, 33)))),
  },
  {
    label: 'Segment',
    // The same arc without the centre — the closing edge IS the chord.
    build: () => polygonSet(region(ringOf(arcPoints(ORIGIN_2D, 1.3, -0.21, 0.21, 40)))),
  },
  {
    label: 'Capsule',
    // A stadium: two half-turn arcs, one about each end of the spine.
    build: () =>
      polygonSet(
        region(
          ringOf(
            arcPoints(point2D(-0.7, 0), 0.5, 0.25, 0.75, 24),
            arcPoints(point2D(0.7, 0), 0.5, 0.75, 1.25, 24),
          ),
        ),
      ),
  },
  {
    label: 'Annulus',
    // A region with one hole. `Canonical` is what puts the hole ring into the
    // winding the type's invariant asks for, rather than reversing it here.
    build: () => polygonSet(region(ngon(64, 1.3), ngon(48, 0.62, ORIGIN_2D, 0.5)).Canonical()),
  },
  {
    label: 'Plate+3 holes',
    build: () =>
      polygonSet(
        region(
          ngon(48, 1.35),
          ngon(20, 0.32, point2D(-0.6, -0.38)),
          ngon(20, 0.32, point2D(0.6, -0.38)),
          ngon(20, 0.32, point2D(0, 0.68)),
        ).Canonical(),
      ),
  },
  { label: 'Cardioid', build: () => polygonSet(region(ring(new Cardioid2D(0.32).SamplePeriodic(96)))) },
  { label: 'Limaçon', build: () => polygonSet(region(ring(new Limacon2D(0.8, 0.5).SamplePeriodic(96)))) },
  {
    label: '3 disks',
    // A genuinely disconnected subject: PolygonSet2D.Deform maps component-wise.
    build: () =>
      polygonSet(
        region(ngon(32, 0.55, point2D(-0.75, -0.45))),
        region(ngon(32, 0.55, point2D(0.75, -0.45))),
        region(ngon(32, 0.55, point2D(0, 0.85))),
      ),
  },
];

const PLANAR_SUBJECT_LABELS = PLANAR_SUBJECTS.map(s => s.label);

/** The generated members every planar scene reaches through for its subject. */
const PLANAR_SUBJECT_MEMBERS = [
  'PolygonSet2D.Deform',
  'PolygonSet2D.Area',
  'PolygonSet2D.Perimeter',
  'PolygonWithHoles2D.Deform',
  'PolygonWithHoles2D.Area',
  'PolygonWithHoles2D.Perimeter',
  'PolygonWithHoles2D.Canonical',
  'Polygon2D.Deform',
  'Polygon2D.Area',
  'Polygon2D.Perimeter',
  'RegularPolygon.RegularPolygonVertices',
  'RegularStar2D.RegularStarVertices',
  'EllipticalArc2D.SamplePeriodic',
  'CircularArc2D.Sample',
  'Cardioid2D.SamplePeriodic',
  'Limacon2D.SamplePeriodic',
];

const PLANAR_CONTROLS: Control[] = [
  { key: 'subject', label: 'Subject', kind: 'select', options: PLANAR_SUBJECT_LABELS, def: 2 },
  { key: 'lines', label: 'Lattice lines', kind: 'slider', min: 3, max: 15, step: 2, def: 9 },
  { key: 'ring', label: 'Show subject', kind: 'toggle', def: 1 },
  { key: 'ghost', label: 'Ghost the original', kind: 'toggle', def: 0 },
];

/**
 * Read a set into flat point arrays. Every generated member returns a lazily
 * mapped `Points`, so a deformed set re-runs `Eval` once per ring per reader —
 * and each subject is read three times per tick (area, perimeter, the line
 * loops). Materializing once costs one evaluation per point and makes the rest
 * free.
 */
function materialize(s: PolygonSet2D): PolygonSet2D {
  const flat = (p: Polygon2D): Polygon2D => new Polygon2D(fromArray(toArray(p.Points)));
  const parts = toArray(s.Polygons).map(
    c => new PolygonWithHoles2D(flat(c.Boundary), fromArray(toArray(c.Holes).map(flat))),
  );
  return new PolygonSet2D(fromArray(parts));
}

/** Built once per subject: none of them depends on a slider. */
const planarCache = new Map<number, PolygonSet2D>();

function planarSubject(params: Params): PolygonSet2D {
  const index = clampIndex(params.subject, PLANAR_SUBJECTS.length);
  let subject = planarCache.get(index);
  if (!subject) {
    subject = materialize(PLANAR_SUBJECTS[index].build());
    planarCache.set(index, subject);
  }
  return subject;
}

/** Every ring of a set, boundaries first — what the line loops and counts want. */
function rings(s: PolygonSet2D): Array<{ points: Point2D[]; hole: boolean }> {
  const out: Array<{ points: Point2D[]; hole: boolean }> = [];
  for (const component of toArray(s.Polygons)) {
    out.push({ points: toArray(component.Boundary.Points), hole: false });
    for (const h of toArray(component.Holes)) out.push({ points: toArray(h.Points), hole: true });
  }
  return out;
}

function ringLoop(points: Point2D[], color: number, z: number): THREE.LineLoop {
  const loop: number[] = [];
  for (const p of points) loop.push(p.X, p.Y, z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(loop, 3));
  return new THREE.LineLoop(geometry, edgeMaterial(color));
}

/** Grid lines + the chosen polygon subject, every point pushed through `Eval`. */
function planar(params: Params, evaluate: (p: Point2D) => Point2D): THREE.Object3D {
  const group = new THREE.Group();
  const count = Math.max(3, Math.round(params.lines ?? 9));
  const positions: number[] = [];

  const push = (a: Point2D, b: Point2D): void => {
    positions.push(a.X, a.Y, 0, b.X, b.Y, 0);
  };
  const lerp = (a: number, b: number, i: number, n: number): number => a + ((b - a) * i) / (n - 1);

  for (let line = 0; line < count; line++) {
    const u = lerp(-PLANAR_EXTENT, PLANAR_EXTENT, line, count);
    let prevRow: Point2D | null = null;
    let prevCol: Point2D | null = null;
    for (let s = 0; s < PLANAR_SAMPLES; s++) {
      const v = lerp(-PLANAR_EXTENT, PLANAR_EXTENT, s, PLANAR_SAMPLES);
      const row = evaluate(point2D(v, u));
      const col = evaluate(point2D(u, v));
      if (prevRow) push(prevRow, row);
      if (prevCol) push(prevCol, col);
      prevRow = row;
      prevCol = col;
    }
  }

  const grid = new THREE.BufferGeometry();
  grid.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  group.add(new THREE.LineSegments(grid, edgeMaterial(palette.line)));

  // The subject doubles as a measurement: area and perimeter from the generated
  // members, on the warped set against the same members on the original.
  const base = planarSubject(params);
  const warpedSet = materialize(base.Deform(evaluate));
  const loops = rings(warpedSet);
  const index = clampIndex(params.subject, PLANAR_SUBJECTS.length);
  const area = warpedSet.Area();
  const wasArea = base.Area();
  const points = loops.reduce((n, r) => n + r.points.length, 0);
  lastStatus =
    `${2 * count} lattice lines · ${2 * count * PLANAR_SAMPLES} warped samples` +
    ` · ${PLANAR_SUBJECTS[index].label}: ${loops.length} ring${loops.length === 1 ? '' : 's'} / ${points} pts` +
    ` · area ${area.toFixed(3)} (was ${wasArea.toFixed(3)}, x${(area / wasArea).toFixed(3)})` +
    ` · perimeter ${warpedSet.Perimeter().toFixed(3)} (was ${base.Perimeter().toFixed(3)})`;

  if (params.ghost) {
    for (const r of rings(base)) group.add(ringLoop(r.points, palette.line, 0.0005));
  }
  if (params.ring) {
    for (const r of loops) group.add(ringLoop(r.points, r.hole ? palette.surfaceAlt : palette.accent, 0.001));
  }
  return group;
}

// ---------------------------------------------------------------------------
// Rigid and affine maps, through the same Deform seam
//
// `intervals-transforms-deformable.library.plato` defines Translate, Rotate,
// Scale and their About/per-axis variants over IDeformable2D and IDeformable3D,
// and every one of them is written as `self.Deform(p => …)` — the same seam the
// catalog deformations use. They are worth a scene next to the non-rigid ones
// because their effect on area is known in closed form, so the status line can
// check the library against arithmetic: a rotation or translation must report
// x1.000, `Scale(k)` must report k squared, `ScaleX(k)` exactly k.
//
// FINDING. `Rotate(Angle)` and `RotateAbout(center, Angle)` THROW on every
// deformable — `Cannot read properties of undefined (reading 'Row1')`. The
// writer keeps only one `RotateAbout` overload, the one taking a `Rotation2D`,
// so the generated `Rotate(angle)` body hands its `Angle` straight into the
// Rotation2D slot and the rotation is read as a matrix. Passing an explicit
// `Rotation2D` reaches the surviving overload and works, which is what the
// rotate entry below does; the status line names the member that does not.
// ---------------------------------------------------------------------------

/** Did `Rotate(Angle)` throw? Probed once, reported by the affine scene. */
const ROTATE_ANGLE_FAILURE = ((): string => {
  try {
    polygonSet(region(ngon(3, 1))).Rotate(turnsAngle(0.1)).Area();
    return '';
  } catch (error) {
    return (error as Error).message;
  }
})();

interface AffineOp {
  label: string;
  /** The generated member, named for the status line and the source panel. */
  member: string;
  set: (s: PolygonSet2D, k: number) => PolygonSet2D;
  line: (l: Polyline2D, k: number) => Polyline2D;
  /** The area factor the map must produce, from the map's own definition. */
  expect: (k: number) => number;
}

const AFFINE_CENTER = point2D(0.9, 0);

const AFFINE_OPS: AffineOp[] = [
  {
    label: 'RotateAbout',
    member: 'RotateAbout(Point2D, Rotation2D)',
    set: (s, k) => s.RotateAbout(ORIGIN_2D, new Rotation2D(turnsAngle(k * 0.5))),
    line: (l, k) => l.RotateAbout(ORIGIN_2D, new Rotation2D(turnsAngle(k * 0.5))),
    expect: () => 1,
  },
  {
    label: 'Scale',
    member: 'Scale(Number)',
    set: (s, k) => s.Scale(1 + k),
    line: (l, k) => l.Scale(1 + k),
    expect: k => (1 + k) * (1 + k),
  },
  {
    label: 'ScaleX',
    member: 'ScaleX(Number)',
    set: (s, k) => s.ScaleX(1 + k),
    line: (l, k) => l.ScaleX(1 + k),
    expect: k => 1 + k,
  },
  {
    label: 'ScaleY',
    member: 'ScaleY(Number)',
    set: (s, k) => s.ScaleY(1 + k),
    line: (l, k) => l.ScaleY(1 + k),
    expect: k => 1 + k,
  },
  {
    label: 'ScaleAbout',
    member: 'ScaleAbout(Point2D, Number)',
    set: (s, k) => s.ScaleAbout(AFFINE_CENTER, 1 + k),
    line: (l, k) => l.ScaleAbout(AFFINE_CENTER, 1 + k),
    expect: k => (1 + k) * (1 + k),
  },
  {
    label: 'Translate',
    member: 'Translate(Vector2D)',
    set: (s, k) => s.Translate(new Vector2D(k, k * 0.5)),
    line: (l, k) => l.Translate(new Vector2D(k, k * 0.5)),
    expect: () => 1,
  },
  {
    label: 'TranslateX',
    member: 'TranslateX(Number)',
    set: (s, k) => s.TranslateX(k),
    line: (l, k) => l.TranslateX(k),
    expect: () => 1,
  },
];

/**
 * The affine twin of `planar`. The lattice cannot go through a point mapping
 * here — the whole point is that the transform arrives as a member on a
 * deformable value — so each lattice line is a `Polyline2D`, which is a
 * deformable in its own right, and the same named member is applied to it.
 */
function affinePlanar(params: Params, op: AffineOp, amount: number): THREE.Object3D {
  const group = new THREE.Group();
  const count = Math.max(3, Math.round(params.lines ?? 9));
  const lerp = (a: number, b: number, i: number, n: number): number => a + ((b - a) * i) / (n - 1);
  const positions: number[] = [];

  for (let line = 0; line < count; line++) {
    const u = lerp(-PLANAR_EXTENT, PLANAR_EXTENT, line, count);
    const row: Point2D[] = [];
    const col: Point2D[] = [];
    for (let s = 0; s < PLANAR_SAMPLES; s++) {
      const v = lerp(-PLANAR_EXTENT, PLANAR_EXTENT, s, PLANAR_SAMPLES);
      row.push(point2D(v, u));
      col.push(point2D(u, v));
    }
    for (const chain of [row, col]) {
      const moved = toArray(op.line(new Polyline2D(fromArray(chain), false), amount).Points);
      for (let i = 0; i + 1 < moved.length; i++) {
        positions.push(moved[i].X, moved[i].Y, 0, moved[i + 1].X, moved[i + 1].Y, 0);
      }
    }
  }
  const grid = new THREE.BufferGeometry();
  grid.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  group.add(new THREE.LineSegments(grid, edgeMaterial(palette.line)));

  const base = planarSubject(params);
  const moved = materialize(op.set(base, amount));
  const loops = rings(moved);
  const area = moved.Area();
  const wasArea = base.Area();
  const expected = op.expect(amount);
  const measured = area / wasArea;
  const index = clampIndex(params.subject, PLANAR_SUBJECTS.length);
  lastStatus =
    `${2 * count} lattice lines · ${PLANAR_SUBJECTS[index].label} through PolygonSet2D.${op.member}` +
    ` · area ${area.toFixed(3)} (was ${wasArea.toFixed(3)}, x${measured.toFixed(4)})` +
    ` · the map's own factor is x${expected.toFixed(4)}` +
    (Math.abs(measured - expected) < 1e-6 ? ' — agrees' : ' — DISAGREES') +
    ` · perimeter ${moved.Perimeter().toFixed(3)} (was ${base.Perimeter().toFixed(3)})` +
    (op.label === 'RotateAbout' && ROTATE_ANGLE_FAILURE
      ? ` · PolygonSet2D.Rotate(Angle) UNAVAILABLE (${ROTATE_ANGLE_FAILURE.slice(0, 60)})`
      : '');

  if (params.ghost) {
    for (const r of rings(base)) group.add(ringLoop(r.points, palette.line, 0.0005));
  }
  if (params.ring) {
    for (const r of loops) group.add(ringLoop(r.points, r.hole ? palette.surfaceAlt : palette.accent, 0.001));
  }
  return group;
}

// ---------------------------------------------------------------------------
// The other deformation catalog: SDF modifiers
//
// `deformations.types.plato` holds exactly ten types and the scenes above cover
// all ten. The stdlib's second family of deformations lives in
// `implicit-sdf.types.plato` and works by a different mechanism: rather than
// moving a point set, it reshapes the FIELD.
//
//  - A DOMAIN modifier (`ApplyToDomain`) pulls the sample position back before
//    the source distance function is asked. It is the INVERSE of the map you
//    see: `SdfTwistModifier3D` un-twists the query point, which is why the
//    picture twists the other way from `Twist3D.Eval` at the same angle. It also
//    does things no point mapping on a fixed vertex set can — repetition tiles
//    ONE source shape into many copies.
//  - A VALUE modifier (`ApplyToDistance`) leaves the domain alone and edits the
//    distance instead: rounding, shells and onions change what the surface IS.
//    There is no point mapping that turns a disk into three nested rings.
//
// The library is explicit that the domain modifiers cost fidelity — repetition,
// twist, bend and displacement can all report MORE than the true distance, so
// the result is no longer a distance bound. Nothing here sphere-traces, so the
// only consequence is that the surface shown is the zero set of the reported
// value, which is what these scenes are about anyway.
// ---------------------------------------------------------------------------

const SDF2D_SOURCES: Array<{ label: string; build: () => FunctionSdf2D }> = [
  { label: 'Circle', build: () => new Circle(ORIGIN_2D, 0.26).ToSdf() },
  { label: 'Capsule', build: () => new Capsule2D(point2D(-0.2, 0), point2D(0.2, 0), 0.15).ToSdf() },
  { label: 'Triangle', build: () => new Triangle2D(point2D(-0.26, -0.2), point2D(0.26, -0.2), point2D(0, 0.3)).ToSdf() },
  { label: 'Annulus', build: () => new Annulus(ORIGIN_2D, 0.14, 0.3).ToSdf() },
];

const SDF2D_VALUE_LABELS = ['None', 'Rounding', 'Shell', 'Onion'];

/** The composed field: repetition, then elongation, then the value modifier. */
function sdfPlanarField(params: Params): (p: Point2D) => number {
  const source = SDF2D_SOURCES[clampIndex(params.source, SDF2D_SOURCES.length)].build();
  const period = Math.max(0.15, params.period ?? 0.8);
  const counts = Math.round(params.counts ?? 1);
  const repetition = new SdfRepetitionModifier2D(new Vector2D(period, period), new IntegerVector2(counts, counts));
  const elongation = new SdfElongationModifier2D(new Vector2D(params.elongate ?? 0, 0));
  const kind = clampIndex(params.value, SDF2D_VALUE_LABELS.length);
  const thickness = Math.max(0.01, params.thickness ?? 0.06);
  const rounding = new SdfRoundingModifier(thickness);
  const shell = new SdfShellModifier(thickness);
  const onion = new SdfOnionModifier(thickness, Math.max(1, Math.round(params.layers ?? 3)));
  return p => {
    const q = elongation.ApplyToDomain(repetition.ApplyToDomain(p));
    const d = source.Eval(q);
    if (kind === 1) return rounding.ApplyToDistance(d);
    if (kind === 2) return shell.ApplyToDistance(d);
    if (kind === 3) {
      // The library's onion is one layer per application: "repeat this with the
      // distance it returns to peel the next layer", Count layers deep.
      let peeled = d;
      for (let i = 0; i < onion.Count; i++) peeled = onion.ApplyToDistance(peeled);
      return peeled;
    }
    return d;
  };
}

const SDF_BAND = 0.06;

/** Presentation only: inside/outside plus distance bands and the zero contour. */
function shadeDistance(d: number): Rgb {
  const band = Math.abs(d) / SDF_BAND;
  const ramp = 0.55 + 0.45 * Math.abs((band % 2) - 1);
  if (Math.abs(d) < 0.008) return { r: 0.95, g: 1, b: 0.95 };
  return d < 0
    ? { r: 0.15 * ramp, g: 0.78 * ramp, b: 0.6 * ramp }
    : { r: 0.1 * ramp, g: 0.16 * ramp, b: 0.26 * ramp };
}

const SDF2D_RESOLUTION = 176;

function sdfPlanar(params: Params): THREE.Object3D {
  const field = sdfPlanarField(params);
  const period = Math.max(0.15, params.period ?? 0.8);
  const counts = Math.round(params.counts ?? 1);
  const extent = new SdfRepetitionModifier2D(
    new Vector2D(period, period),
    new IntegerVector2(counts, counts),
  ).RepetitionExtent();
  let inside = 0;
  const quad = rasterPlane(
    (u, v) => {
      const d = field(point2D((u - 0.5) * 2 * PLANAR_EXTENT, (v - 0.5) * 2 * PLANAR_EXTENT));
      if (d < 0) inside++;
      return shadeDistance(d);
    },
    { resolution: SDF2D_RESOLUTION, size: 2 * PLANAR_EXTENT, smooth: true },
  );
  const samples = SDF2D_RESOLUTION * SDF2D_RESOLUTION;
  const onion = new SdfOnionModifier(Math.max(0.01, params.thickness ?? 0.06), Math.max(1, Math.round(params.layers ?? 3)));
  lastStatus =
    `${samples} field samples · ${SDF2D_SOURCES[clampIndex(params.source, SDF2D_SOURCES.length)].label}` +
    ` ▸ ${SDF2D_VALUE_LABELS[clampIndex(params.value, SDF2D_VALUE_LABELS.length)]}` +
    ` · ${((100 * inside) / samples).toFixed(1)}% of the raster inside` +
    ` · RepetitionExtent (${extent.X.toFixed(2)}, ${extent.Y.toFixed(2)})` +
    (counts > 0 ? ` — ${(2 * counts + 1) ** 2} tiles` : ' — unbounded tiling') +
    ` · onion TotalDepth ${onion.TotalDepth().toFixed(3)}`;
  return quad;
}

// --- the spatial half ------------------------------------------------------

const SDF3D_SOURCES: Array<{ label: string; build: () => FunctionSdf3D }> = [
  { label: 'Sphere', build: () => new Sphere(new Point3D(0, 0, 0), 0.85).ToSdf() },
  { label: 'Capsule', build: () => new Capsule3D(new Point3D(0, -0.75, 0), new Point3D(0, 0.75, 0), 0.42).ToSdf() },
  {
    label: 'Two spheres',
    build: () =>
      new Sphere(new Point3D(0, -0.45, 0), 0.6).ToSdf().Union(new Sphere(new Point3D(0, 0.55, 0), 0.45).ToSdf()),
  },
];

const SDF3D_MODIFIER_LABELS = ['Twist', 'Bend', 'Elongate', 'Repeat', 'Displace'];

const SDF3D_MEMBERS = [
  'SdfTwistModifier3D.ApplyToDomain',
  'SdfBendModifier3D.ApplyToDomain',
  'SdfElongationModifier3D.ApplyToDomain',
  'SdfRepetitionModifier3D.ApplyToDomain',
  'SdfRepetitionModifier3D.RepetitionExtent',
  'SdfDisplacementModifier3D.ApplyToDistance',
  'FunctionSdf3D.MapDomain',
  'FunctionSdf3D.Union',
  'FunctionSdf3D.MarchingCubes',
  'Sphere.ToSdf',
  'Capsule3D.ToSdf',
  'PerlinNoise3D.Eval',
];

const SDF3D_BOUNDS = new Bounds3D(new Point3D(-1.9, -1.9, -1.9), new Point3D(1.9, 1.9, 1.9));

/**
 * Marching cubes is cubic in the node count and the field is evaluated once per
 * node, so the affordable resolution is per modifier rather than global — the
 * same shape of cap the 3D subjects already carry for `Truncate`. Repetition
 * fills the whole box with surface instead of one blob, and displacement pays
 * for a `PerlinNoise3D.Eval` at every node on top of the source; both cost
 * several times what a twist does at the same resolution. The status line
 * reports the count actually used.
 */
const SDF3D_MAX_NODES = [32, 32, 32, 28, 20];

function sdfSpatial(params: Params): THREE.Object3D {
  const source = SDF3D_SOURCES[clampIndex(params.source, SDF3D_SOURCES.length)].build();
  const kind = clampIndex(params.modifier, SDF3D_MODIFIER_LABELS.length);
  const strength = params.strength ?? 0.3;
  let field = source;
  let note = '';
  if (kind === 0) {
    const twist = new SdfTwistModifier3D(turnsAngle(strength));
    field = source.MapDomain(p => twist.ApplyToDomain(p));
    note = `AnglePerUnit ${strength.toFixed(3)} turns`;
  } else if (kind === 1) {
    const bend = new SdfBendModifier3D(strength * 3);
    field = source.MapDomain(p => bend.ApplyToDomain(p));
    note = `Curvature ${(strength * 3).toFixed(3)}`;
  } else if (kind === 2) {
    const elongate = new SdfElongationModifier3D(new Vector3D(strength * 2, 0, strength));
    field = source.MapDomain(p => elongate.ApplyToDomain(p));
    note = `Amount (${(strength * 2).toFixed(2)}, 0, ${strength.toFixed(2)})`;
  } else if (kind === 3) {
    const repetition = new SdfRepetitionModifier3D(
      new Vector3D(1.1, 1.1, 1.1),
      new IntegerVector3(1, 1, 1),
    );
    field = source.MapDomain(p => repetition.ApplyToDomain(p));
    const extent = repetition.RepetitionExtent();
    note = `27 cells, RepetitionExtent (${extent.X.toFixed(2)}, ${extent.Y.toFixed(2)}, ${extent.Z.toFixed(2)})`;
  } else {
    // The one VALUE modifier here: the distance is offset by a scalar field,
    // which the library says is no longer a bound. `Source` is an index into a
    // caller-supplied list of fields — this page supplies exactly one.
    const displacement = new SdfDisplacementModifier3D(new ItemIndex(0), strength * 2);
    const noise = new PerlinNoise3D(7, Math.max(1, Math.round(params.frequency ?? 3)));
    const sources = [noise];
    field = new FunctionSdf3D(p =>
      displacement.ApplyToDistance(source.Eval(p), sources[displacement.SourceFieldIndex()].Eval(p)),
    );
    note = `Amplitude ${(strength * 2).toFixed(2)} on PerlinNoise3D`;
  }

  const asked = Math.max(12, Math.round(params.nodes ?? 24));
  const n = Math.min(asked, SDF3D_MAX_NODES[kind]);
  const triangles = field.MarchingCubes(SDF3D_BOUNDS, new IntegerVector3(n, n, n));
  const geometry = triangleArrayGeometry(triangles);
  lastStatus =
    `${SDF3D_SOURCES[clampIndex(params.source, SDF3D_SOURCES.length)].label}` +
    ` ▸ ${SDF3D_MODIFIER_LABELS[kind]} (${note})` +
    ` · marched on ${n}x${n}x${n} nodes${n < asked ? ` (capped from ${asked})` : ''}` +
    ` · ${triangles.Triangles.Count()} triangles`;
  return new THREE.Mesh(geometry, surfaceMaterial(kind === 4 ? palette.surfaceAlt : palette.surface));
}

// ---------------------------------------------------------------------------
// Scenes
// ---------------------------------------------------------------------------

const scenes: Scene[] = [
  {
    id: 'twist-3d',
    title: 'Twist3D',
    description:
      'Twist3D rotates every slice perpendicular to Axis by AnglePerUnit times its signed distance along that axis, so points on the axis never move. Angle is built with the stdlib Turns conversion.',
    plato: ['Twist3D.Eval', 'Number.Turns', ...SUBJECT_MEMBERS],
    controls: [
      { key: 'turns', label: 'Angle per unit (turns)', kind: 'slider', min: -0.5, max: 0.5, step: 0.005, def: 0.15 },
      { key: 'axis', label: 'Axis', kind: 'select', options: AXIS_LABELS, def: 1 },
      { key: 'origin', label: 'Origin along axis', kind: 'slider', min: -2, max: 2, step: 0.05, def: 0 },
      ...SUBJECT_CONTROLS,
    ],
    build: params => {
      const a = axis(params.axis);
      const twist = new Twist3D(originOn(a, params.origin), a, params.turns.Turns());
      return warped(params, p => twist.Eval(p));
    },
    status,
  },
  {
    id: 'bend-3d',
    title: 'Bend3D',
    description:
      'Bend3D rotates about Axis by Curvature times the signed distance along Direction — the cheap circular bend. Axis and Direction must not be parallel, so picking the same one twice steps Direction to the next axis.',
    plato: ['Bend3D.Eval', ...SUBJECT_MEMBERS],
    controls: [
      { key: 'curvature', label: 'Curvature', kind: 'slider', min: -1.2, max: 1.2, step: 0.01, def: 0.35 },
      { key: 'axis', label: 'Rotation axis', kind: 'select', options: AXIS_LABELS, def: 2 },
      { key: 'dir', label: 'Progress direction', kind: 'select', options: AXIS_LABELS, def: 1 },
      { key: 'origin', label: 'Origin along direction', kind: 'slider', min: -2, max: 2, step: 0.05, def: 0 },
      ...SUBJECT_CONTROLS,
    ],
    build: params => {
      const axisIndex = clampIndex(params.axis, 3);
      let dirIndex = clampIndex(params.dir, 3);
      if (dirIndex === axisIndex) dirIndex = (dirIndex + 1) % 3;
      const a = axis(axisIndex);
      const d = axis(dirIndex);
      const bend = new Bend3D(originOn(d, params.origin), a, d, params.curvature);
      return warped(params, p => bend.Eval(p));
    },
    status,
  },
  {
    id: 'taper-3d',
    title: 'Taper3D',
    description:
      'Taper3D scales the component perpendicular to Axis by (1 + Rate * t), where t is signed distance along the axis. Rate 0 is the identity; negative Rate pinches one end and flares the other.',
    plato: ['Taper3D.Eval', ...SUBJECT_MEMBERS],
    controls: [
      { key: 'rate', label: 'Rate', kind: 'slider', min: -0.9, max: 1.5, step: 0.01, def: 0.5 },
      { key: 'axis', label: 'Axis', kind: 'select', options: AXIS_LABELS, def: 1 },
      { key: 'origin', label: 'Origin along axis', kind: 'slider', min: -2, max: 2, step: 0.05, def: 0 },
      ...SUBJECT_CONTROLS,
    ],
    build: params => {
      const a = axis(params.axis);
      const taper = new Taper3D(originOn(a, params.origin), a, params.rate);
      return warped(params, p => taper.Eval(p));
    },
    status,
  },
  {
    id: 'shear-3d',
    title: 'Shear3D',
    description:
      'Shear3D translates each point by the Rate vector scaled by its signed distance from Origin along Axis. The two sliders are the components of Rate in the plane across the axis.',
    plato: ['Shear3D.Eval', ...SUBJECT_MEMBERS],
    controls: [
      { key: 'rateX', label: 'Rate X', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0.5 },
      { key: 'rateZ', label: 'Rate Z', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0 },
      { key: 'axis', label: 'Axis', kind: 'select', options: AXIS_LABELS, def: 1 },
      { key: 'origin', label: 'Origin along axis', kind: 'slider', min: -2, max: 2, step: 0.05, def: 0 },
      ...SUBJECT_CONTROLS,
    ],
    build: params => {
      const a = axis(params.axis);
      const shear = new Shear3D(originOn(a, params.origin), a, new Vector3D(params.rateX, 0, params.rateZ));
      return warped(params, p => shear.Eval(p));
    },
    status,
  },
  {
    id: 'spherify-3d',
    title: 'Spherify3D',
    description:
      'Spherify3D lerps every point toward the sphere of Radius about Center along its radial ray. Strength 0 is the identity, 1 lands every point on the sphere — the column collapses onto a ball.',
    plato: ['Spherify3D.Eval', ...SUBJECT_MEMBERS],
    controls: [
      { key: 'strength', label: 'Strength', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0.6 },
      { key: 'radius', label: 'Radius', kind: 'slider', min: 0.3, max: 2.5, step: 0.01, def: 1.4 },
      { key: 'centerY', label: 'Center Y', kind: 'slider', min: -2, max: 2, step: 0.05, def: 0 },
      ...SUBJECT_CONTROLS,
    ],
    build: params => {
      const spherify = new Spherify3D(new Point3D(0, params.centerY, 0), params.radius, params.strength);
      return warped(params, p => spherify.Eval(p));
    },
    status,
  },
  {
    id: 'compose-3d',
    title: 'Compose two warps',
    description:
      'Compose(first, second) applies first, then second, and returns the result as a MappingDeformation3D — the carrier type the library gives combinator results. Multiply is the same function under the * alias, so the two combinator settings agree exactly; the order select does not, because a twist and a bend about different axes do not commute.',
    plato: [
      'MappingDeformation3D.Compose',
      'MappingDeformation3D.Multiply',
      'MappingDeformation3D.Eval',
      'Twist3D.Eval',
      'Bend3D.Eval',
      ...SUBJECT_MEMBERS,
    ],
    controls: [
      { key: 'turns', label: 'Twist: angle per unit (turns)', kind: 'slider', min: -0.5, max: 0.5, step: 0.005, def: 0.18 },
      { key: 'curvature', label: 'Bend: curvature', kind: 'slider', min: -1.2, max: 1.2, step: 0.01, def: 0.4 },
      { key: 'order', label: 'Order', kind: 'select', options: ['Twist ▸ Bend', 'Bend ▸ Twist'], def: 0 },
      { key: 'alias', label: 'Combine with', kind: 'select', options: ['Compose', 'Multiply (the * alias)'], def: 0 },
      ...SUBJECT_CONTROLS,
    ],
    build: params => {
      const yAxis = axis(1);
      const zAxis = axis(2);
      const twist = new Twist3D(new Point3D(0, 0, 0), yAxis, params.turns.Turns());
      const bend = new Bend3D(new Point3D(0, 0, 0), zAxis, yAxis, params.curvature);
      // Compose is declared over IDeformation3D; the generated overload is
      // monomorphic per type, so both operands are lifted into the carrier type
      // the library itself uses for combinator results.
      const asMapping = new MappingDeformation3D(p => twist.Eval(p));
      const bsMapping = new MappingDeformation3D(p => bend.Eval(p));
      const first = params.order === 0 ? asMapping : bsMapping;
      const second = params.order === 0 ? bsMapping : asMapping;
      const combined = params.alias === 0 ? first.Compose(second) : first.Multiply(second);
      return warped(params, p => combined.Eval(p));
    },
    status,
  },
  {
    id: 'weighted-3d',
    title: 'Weighted apply',
    description:
      'The library keeps falloff out of the warp: weighted apply takes an IScalarField3D as a separate modulator and lerps between p and d.Eval(p) by weight.Eval(p). The field here is a Sphere signed distance lifted to a ScalarFunctionField3D and remapped with SmoothStep, so the twist only bites inside the wireframe sphere.',
    plato: [
      'Twist3D.Eval',
      'Sphere.ToSdf',
      'FunctionSdf3D.ScalarFunctionField3D',
      'ScalarFunctionField3D.MapValue',
      'ScalarFunctionField3D.Eval',
      'Point3D.Lerp',
      'PolygonMesh3D.ScaledAboutOrigin',
      'PolygonMesh3D.Translate',
      ...SUBJECT_MEMBERS,
    ],
    controls: [
      { key: 'turns', label: 'Angle per unit (turns)', kind: 'slider', min: -0.6, max: 0.6, step: 0.005, def: 0.35 },
      { key: 'radius', label: 'Falloff radius', kind: 'slider', min: 0.4, max: 3, step: 0.02, def: 1.4 },
      { key: 'feather', label: 'Feather (inward)', kind: 'slider', min: 0.05, max: 2, step: 0.01, def: 0.8 },
      { key: 'centerY', label: 'Falloff center Y', kind: 'slider', min: -2.5, max: 2.5, step: 0.05, def: 0.9 },
      { key: 'showField', label: 'Show falloff sphere', kind: 'toggle', def: 1 },
      ...SUBJECT_CONTROLS,
    ],
    build: params => {
      const center = new Point3D(0, params.centerY, 0);
      const twist = new Twist3D(new Point3D(0, 0, 0), axis(1), params.turns.Turns());
      // The modulator is a field in its own right, never a field of the warp.
      const feather = params.feather;
      const weight = new Sphere(center, params.radius)
        .ToSdf()
        .ScalarFunctionField3D()
        .MapValue(d => d.Negative().Divide(feather).Saturate().SmoothStep());
      const group = new THREE.Group();
      group.add(warped(params, p => p.Lerp(twist.Eval(p), weight.Eval(p))));
      if (params.showField) {
        const shell = indicatorBall().ScaledAboutOrigin(params.radius).Translate(new Vector3D(0, params.centerY, 0));
        group.add(new THREE.LineSegments(polygonMeshEdges(shell), edgeMaterial(palette.accent)));
      }
      return group;
    },
    status,
  },
  {
    id: 'twist-2d',
    title: 'Twist2D (swirl)',
    description:
      'The polar twist: Twist2D rotates each point about Origin by AnglePerUnit times its distance from Origin, so the centre is pinned and the outside spins hardest. A rotation is rigid, so a fine ring keeps its area however hard you swirl it — but only the VERTICES are rotated, so pick the 5-gon and watch five corners drag while the five edges between them stay straight. The 5-star goes further: swirled hard enough its arms cross, and the reading drops, because Area is the absolute shoelace value, which on a self-intersecting ring counts by winding rather than by covered area.',
    plato: ['Twist2D.Eval', 'Number.Turns', ...PLANAR_SUBJECT_MEMBERS],
    controls: [
      { key: 'turns', label: 'Angle per unit (turns)', kind: 'slider', min: -0.5, max: 0.5, step: 0.005, def: 0.2 },
      { key: 'originX', label: 'Origin X', kind: 'slider', min: -1.5, max: 1.5, step: 0.05, def: 0 },
      { key: 'originY', label: 'Origin Y', kind: 'slider', min: -1.5, max: 1.5, step: 0.05, def: 0 },
      ...PLANAR_CONTROLS,
    ],
    viewer: PLANAR_VIEWER,
    build: params => {
      const swirl = new Twist2D(point2D(params.originX, params.originY), params.turns.Turns());
      return planar(params, p => swirl.Eval(p));
    },
    status,
  },
  {
    id: 'shear-2d',
    title: 'Shear2D',
    description:
      'The planar catalog entry: Shear2D translates each point by the Rate vector scaled by its signed distance from Origin along Axis. The lattice and the chosen subject are both pushed through the same Eval; a shear has unit determinant, so the area reading does not move.',
    plato: ['Shear2D.Eval', ...PLANAR_SUBJECT_MEMBERS],
    controls: [
      { key: 'rateX', label: 'Rate X', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0 },
      { key: 'rateY', label: 'Rate Y', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0.6 },
      { key: 'axis', label: 'Axis', kind: 'select', options: ['X', 'Y'], def: 0 },
      { key: 'origin', label: 'Origin along axis', kind: 'slider', min: -1.5, max: 1.5, step: 0.05, def: 0 },
      ...PLANAR_CONTROLS,
    ],
    viewer: PLANAR_VIEWER,
    build: params => {
      const a = planarAxis(params.axis);
      const shear = new Shear2D(planarOrigin(a, params.origin), a, new Vector2D(params.rateX, params.rateY));
      return planar(params, p => shear.Eval(p));
    },
    status,
  },
  {
    id: 'taper-2d',
    title: 'Taper2D',
    description:
      'Taper2D scales the component perpendicular to Axis by (1 + Rate * t). On a lattice it reads as a wedge; a ring becomes a teardrop. The local area factor is that same (1 + Rate * t), so a subject centred on Origin keeps its area exactly — the gains on one side cancel the losses on the other. Drag Origin off centre, or pick an off-centre subject like the 3 disks, and the status line shows the area move.',
    plato: ['Taper2D.Eval', ...PLANAR_SUBJECT_MEMBERS],
    controls: [
      { key: 'rate', label: 'Rate', kind: 'slider', min: -0.9, max: 1.5, step: 0.01, def: 0.6 },
      { key: 'axis', label: 'Axis', kind: 'select', options: ['X', 'Y'], def: 0 },
      { key: 'origin', label: 'Origin along axis', kind: 'slider', min: -1.5, max: 1.5, step: 0.05, def: 0 },
      ...PLANAR_CONTROLS,
    ],
    viewer: PLANAR_VIEWER,
    build: params => {
      const a = planarAxis(params.axis);
      const taper = new Taper2D(planarOrigin(a, params.origin), a, params.rate);
      return planar(params, p => taper.Eval(p));
    },
    status,
  },
  {
    id: 'compose-2d',
    title: 'Compose in the plane',
    description:
      'The planar half of the combinator: Compose(first, second) over two IDeformation2D values, carried by MappingDeformation2D. Shear then taper is not the same map as taper then shear.',
    plato: [
      'MappingDeformation2D.Compose',
      'MappingDeformation2D.Multiply',
      'MappingDeformation2D.Eval',
      'Shear2D.Eval',
      'Taper2D.Eval',
      ...PLANAR_SUBJECT_MEMBERS,
    ],
    controls: [
      { key: 'shearY', label: 'Shear: rate Y', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0.6 },
      { key: 'rate', label: 'Taper: rate', kind: 'slider', min: -0.9, max: 1.5, step: 0.01, def: 0.6 },
      { key: 'order', label: 'Order', kind: 'select', options: ['Shear ▸ Taper', 'Taper ▸ Shear'], def: 0 },
      { key: 'alias', label: 'Combine with', kind: 'select', options: ['Compose', 'Multiply (the * alias)'], def: 0 },
      ...PLANAR_CONTROLS,
    ],
    viewer: PLANAR_VIEWER,
    build: params => {
      const xAxis = planarAxis(0);
      const shear = new Shear2D(point2D(0, 0), xAxis, new Vector2D(0, params.shearY));
      const taper = new Taper2D(point2D(0, 0), xAxis, params.rate);
      const shearMapping = new MappingDeformation2D(p => shear.Eval(p));
      const taperMapping = new MappingDeformation2D(p => taper.Eval(p));
      const first = params.order === 0 ? shearMapping : taperMapping;
      const second = params.order === 0 ? taperMapping : shearMapping;
      const combined = params.alias === 0 ? first.Compose(second) : first.Multiply(second);
      return planar(params, p => combined.Eval(p));
    },
    status,
  },
  {
    id: 'affine-2d',
    title: 'Rigid and affine maps',
    description:
      'The transform library writes Translate, Rotate and Scale as Deform calls, so they are deformations like any other — they just happen to have a known effect on area. The status line puts the measured area ratio next to the factor the map is defined to have: 1 for a rotation or a translation, k squared for Scale(k), exactly k for ScaleX(k). Rotate(Angle) itself throws — see the status line — so the rotation entry calls the RotateAbout overload that takes a Rotation2D.',
    plato: [
      'PolygonSet2D.RotateAbout',
      'PolygonSet2D.Scale',
      'PolygonSet2D.ScaleX',
      'PolygonSet2D.ScaleY',
      'PolygonSet2D.ScaleAbout',
      'PolygonSet2D.Translate',
      'PolygonSet2D.TranslateX',
      'Polyline2D.RotateAbout',
      'Polyline2D.Scale',
      ...PLANAR_SUBJECT_MEMBERS,
    ],
    controls: [
      { key: 'op', label: 'Map', kind: 'select', options: AFFINE_OPS.map(o => o.label), def: 1 },
      { key: 'amount', label: 'Amount', kind: 'slider', min: -0.6, max: 1, step: 0.01, def: 0.4 },
      ...PLANAR_CONTROLS,
    ],
    viewer: PLANAR_VIEWER,
    build: params => affinePlanar(params, AFFINE_OPS[clampIndex(params.op, AFFINE_OPS.length)], params.amount),
    status,
  },
  {
    id: 'sdf-modifiers-2d',
    title: 'SDF modifiers (plane)',
    description:
      'The stdlib carries a second deformation catalog that acts on distance FIELDS rather than on points. A domain modifier reshapes the query position before the source is sampled — SdfRepetitionModifier2D folds the plane into a lattice cell, so one circle becomes a tiling, which no mapping of a fixed vertex set can do. A value modifier edits the distance instead: rounding inflates the shape, shell keeps a band either side of the surface, onion peels Count concentric layers. The picture is the field itself, banded by distance with the zero contour picked out.',
    plato: [
      'SdfRepetitionModifier2D.ApplyToDomain',
      'SdfRepetitionModifier2D.RepetitionExtent',
      'SdfElongationModifier2D.ApplyToDomain',
      'SdfRoundingModifier.ApplyToDistance',
      'SdfShellModifier.ApplyToDistance',
      'SdfOnionModifier.ApplyToDistance',
      'SdfOnionModifier.TotalDepth',
      'Circle.ToSdf',
      'Capsule2D.ToSdf',
      'Triangle2D.ToSdf',
      'Annulus.ToSdf',
      'FunctionSdf2D.Eval',
    ],
    controls: [
      { key: 'source', label: 'Source', kind: 'select', options: SDF2D_SOURCES.map(s => s.label), def: 0 },
      { key: 'value', label: 'Value modifier', kind: 'select', options: SDF2D_VALUE_LABELS, def: 3 },
      { key: 'period', label: 'Repetition period', kind: 'slider', min: 0.3, max: 1.6, step: 0.02, def: 0.8 },
      { key: 'counts', label: 'Repetitions per side (0 = unbounded)', kind: 'slider', min: 0, max: 4, step: 1, def: 1 },
      { key: 'elongate', label: 'Elongation along X', kind: 'slider', min: 0, max: 0.3, step: 0.005, def: 0.05 },
      { key: 'thickness', label: 'Rounding / shell / onion size', kind: 'slider', min: 0.01, max: 0.14, step: 0.002, def: 0.05 },
      { key: 'layers', label: 'Onion layers', kind: 'slider', min: 1, max: 4, step: 1, def: 2 },
    ],
    viewer: PLANAR_VIEWER,
    build: sdfPlanar,
    status,
  },
  {
    id: 'sdf-modifiers-3d',
    title: 'SDF modifiers (space)',
    description:
      'The same catalog in space, meshed with MarchingCubes. Twist and bend rotate the query point about an axis, which is the INVERSE of the map you see — so this twist turns the opposite way from Twist3D.Eval at the same angle per unit, and the surface is re-meshed rather than having its vertices dragged. Repeat tiles one source into 27 copies from a single distance function. Displace is the odd one out: it is a value modifier, offsetting the distance by a PerlinNoise3D sample, and the library warns it is no longer a distance bound.',
    plato: SDF3D_MEMBERS,
    controls: [
      { key: 'source', label: 'Source', kind: 'select', options: SDF3D_SOURCES.map(s => s.label), def: 1 },
      { key: 'modifier', label: 'Modifier', kind: 'select', options: SDF3D_MODIFIER_LABELS, def: 0 },
      { key: 'strength', label: 'Strength', kind: 'slider', min: -0.5, max: 0.5, step: 0.005, def: 0.3 },
      { key: 'frequency', label: 'Displace: noise frequency', kind: 'slider', min: 1, max: 6, step: 1, def: 3 },
      { key: 'nodes', label: 'Marching nodes per axis (capped per modifier)', kind: 'slider', min: 12, max: 32, step: 4, def: 24 },
    ],
    build: sdfSpatial,
    status,
  },
];

const PLANAR_AXES = [new Vector2D(1, 0), new Vector2D(0, 1)];

function planarAxis(index: number): Direction2D {
  return new Direction2D(PLANAR_AXES[clampIndex(index, 2)]);
}

function planarOrigin(a: Direction2D, t: number): Point2D {
  return a.Vector.Multiply(t).ToPoint();
}

const demo: Demo = {
  title: 'Deformers',
  subtitle: 'deformations.{concepts,types,library}.plato',
  scenes,
};

// The page default is the spatial stage — perspective, auto-spinning until the
// user drags. The planar scenes override it with `Scene.viewer`.
mountDemo(demo, { distance: 8, grid: false });

// The page never imports this; it exists so `npm run scenes` can call every
// scene's `build` without a WebGL context.
export { demo };
