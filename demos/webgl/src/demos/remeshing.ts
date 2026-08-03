// Remeshing — a scene catalog over `stdlib/geometry/remeshing.{types,library}.plato`.
//
// The library's own design decision shapes every scene: an operation is a
// whole-mesh REBUILD driven by decisions taken against one unchanging input.
// `TriangleMeshTopology` is derived and read-only, `VertexRemap` is the tail of
// every vertex-removing pass, and the single-edge operators are the batched ones
// applied to a one-edge mask. Nothing here re-derives an edge table, a
// subdivision stencil, a Laplacian or a collapse rule: `TopologyOf`,
// `SplitEdge`/`CollapseEdge`/`FlipEdge`, `SplitLongEdges`/`CollapseShortEdges`/
// `FlipTowardValenceSix`, `LoopSubdivided`, `ButterflySubdivided`,
// `Subdivided`, `LaplacianSmoothed`, `TaubinSmoothed`, `TangentiallyRelaxed`
// and `Welded` are all generated members. The demo builds the input meshes,
// reads the results, and repacks them into Three.js buffers.
//
// ---------------------------------------------------------------------------
// COST, which is the thing that decided every size on this page
//
// The library says `TopologyOf` is quadratic in the corner count and dominates
// every pass, and since plato-436 the runtime agrees with the source: the
// emitted `Arr` memoizes, so a derived array (`rank` over `naming` over
// `twins`) computes each element once however deeply the readers stack views.
// Sizes on this page were chosen when the runtime was worse — a lazy,
// unmemoized `Arr` made materializing `CornerEdges` cubic and made
// `LaplacianSmoothed(w, s, n)` cost about 7^n — and this file used to carry
// `flat()` / `readable()` materialization helpers and step every iteration
// count one call at a time to route around that. Those workarounds are gone;
// the caches that remain (`smoothCache`, the subject caches) are UI caches
// across slider moves, not laziness repairs.
//
// ---------------------------------------------------------------------------
// WHAT DOES NOT WORK
//
// The whole quadric family is unreachable. `PlaneQuadric` and
// `Quadric.ZeroQuadric` are emitted as `new Quadric(new Tuple4(...))` — the
// record-return-written-as-a-tuple-literal defect — but `Quadric.Coefficients`
// is a `Matrix4x4`, and `Tuple4` has `X0..X3` rather than `Row1..Row4` and
// neither `Add` nor `Multiply`. So `TriangleQuadric`, `VertexQuadrics`,
// `QuadricError`, `QuadricMinimizer`, `EdgeCollapseCost`, `Decimated` and
// `DecimatedToError` all throw. Quadric decimation is the one item of the
// library this page cannot show; the coarsening scene reports each of those
// members by name with the message it actually raised, and shows the length-
// driven `CollapseShortEdges` in its place.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import {
  fromArray,
  polygonMeshEdges,
  polygonMeshGeometry,
  toArray,
  triangleMeshGeometry,
} from '../shared/mesh.js';
import { edgeMaterial, palette, surfaceMaterial } from '../shared/viewer.js';
import {
  Bounds3D,
  Direction3D,
  FaceIndex,
  IntegerVector3,
  Plane,
  Point3D,
  PolygonMesh3D,
  Sphere,
  SubdivisionSurface,
  TriangleArray3D,
  TriangleFace,
  TriangleMesh3D,
  TriangleMeshTopology,
  UndirectedEdgeIndex,
  Vector3D,
  VertexIndex,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';
import type { ViewerOptions } from '../shared/viewer.js';

// ---------------------------------------------------------------------------
// The sum types the prelude supplies
//
// `LaplacianWeighting` and `SubdivisionScheme` are both CHK320 in `plato.g.ts`
// — sum types are C#-only in v1 — so the case VALUES arrive on `globalThis`
// from `src/plato/array-ext.ts`. The dispatchers themselves are emitted
// normally: `LaplacianField` and `SubdividedOnce` are generated bodies that
// branch on `IsUniformWeights()` / `IsCatmullClark()`.
// ---------------------------------------------------------------------------

interface SumCase {
  readonly Tag: string;
}
type SumFactory = Record<string, () => SumCase>;

const prelude = globalThis as unknown as {
  LaplacianWeighting: SumFactory;
  SubdivisionScheme: SumFactory;
};

const WEIGHTING_LABELS = ['Uniform', 'Cotangent'];
const WEIGHTING_CASES = ['UniformWeights', 'CotangentWeights'];
const weightingCase = (index: number): SumCase =>
  prelude.LaplacianWeighting[WEIGHTING_CASES[clampIndex(index, WEIGHTING_CASES.length)]]();

const SCHEME_LABELS = ['Catmull-Clark', 'Loop', 'Doo-Sabin'];
const SCHEME_CASES = ['CatmullClark', 'Loop', 'DooSabin'];
const schemeCase = (index: number): SumCase =>
  prelude.SubdivisionScheme[SCHEME_CASES[clampIndex(index, SCHEME_CASES.length)]]();

function clampIndex(value: number | undefined, count: number): number {
  const i = Math.round(value ?? 0);
  return i < 0 ? 0 : i >= count ? count - 1 : i;
}

const clamp = (x: number, lo: number, hi: number): number => (x < lo ? lo : x > hi ? hi : x);

// ---------------------------------------------------------------------------
// Readings — the house pattern from `src/demos/polygons.ts`
// ---------------------------------------------------------------------------

interface Reading {
  label: string;
  value: string;
}

function reading(label: string, produce: () => string): Reading {
  try {
    return { label, value: produce() };
  } catch (error) {
    return { label, value: `UNAVAILABLE (${(error as Error).message})` };
  }
}

function note(label: string, value: string): Reading {
  return { label, value };
}

const n2 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(2);
const n3 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(3);
const n4 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(4);

interface Built {
  object: THREE.Object3D;
  readings: Reading[];
}

function sceneOf(spec: {
  id: string;
  title: string;
  description: string;
  plato: string[];
  controls?: Control[];
  viewer?: ViewerOptions;
  build(params: Params): Built;
}): Scene {
  let latest: Reading[] = [];
  return {
    id: spec.id,
    title: spec.title,
    description: spec.description,
    plato: spec.plato,
    controls: spec.controls,
    viewer: spec.viewer,
    build(params: Params): THREE.Object3D {
      const built = spec.build(params);
      latest = built.readings;
      return built.object;
    },
    status(): string {
      return latest.map(r => `${r.label} ${r.value}`).join('  ·  ');
    },
  };
}

// ---------------------------------------------------------------------------
// Materialization — see the cost note in the header
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// Measurements taken from generated members
// ---------------------------------------------------------------------------

const ORIGIN = new Point3D(0, 0, 0);

/**
 * The volume a closed mesh encloses: one signed tetrahedron per triangle over
 * the origin, each from `Point3D.SixTimesSignedVolume`. The library has no
 * `Volume` on a triangle mesh; this sums the generated per-tetrahedron term
 * rather than writing a volume formula.
 */
function signedVolume(mesh: TriangleMesh3D): number {
  const p = toArray(mesh.Positions);
  let six = 0;
  for (const f of toArray(mesh.Faces)) {
    six += ORIGIN.SixTimesSignedVolume(p[f.A.Value], p[f.B.Value], p[f.C.Value]);
  }
  return six / 6;
}

/** The farthest any of the first `count` vertices moved between two meshes. */
function drift(before: TriangleMesh3D, after: TriangleMesh3D, count: number): number {
  const a = toArray(before.Positions);
  const b = toArray(after.Positions);
  let far = 0;
  for (let i = 0; i < Math.min(count, a.length, b.length); i++) {
    far = Math.max(far, a[i].Distance(b[i]));
  }
  return far;
}

interface EdgeStats {
  edges: number;
  boundaryEdges: number;
  average: number;
  shortest: number;
  longest: number;
}

/** Every edge measured through `EdgeLength` / `IsBoundaryEdge`. */
function edgeStats(mesh: TriangleMesh3D, topology: TriangleMeshTopology): EdgeStats {
  const edges = topology.EdgeCount();
  let total = 0;
  let shortest = Infinity;
  let longest = 0;
  let boundaryEdges = 0;
  for (let e = 0; e < edges; e++) {
    const index = new UndirectedEdgeIndex(e);
    const length = mesh.EdgeLength(topology, index);
    total += length;
    shortest = Math.min(shortest, length);
    longest = Math.max(longest, length);
    if (topology.IsBoundaryEdge(index)) boundaryEdges++;
  }
  return {
    edges,
    boundaryEdges,
    average: edges === 0 ? 0 : total / edges,
    shortest: edges === 0 ? 0 : shortest,
    longest,
  };
}

interface ValenceStats {
  valences: number[];
  boundary: boolean[];
  interior: number;
  regular: number;
  min: number;
  max: number;
}

/** `VertexValences` and `BoundaryVertexFlags`, read out and summarized. */
function valenceStats(mesh: TriangleMesh3D, topology: TriangleMeshTopology): ValenceStats {
  const valences = toArray(mesh.VertexValences(topology));
  const boundary = toArray(mesh.BoundaryVertexFlags(topology));
  let interior = 0;
  let regular = 0;
  let min = Infinity;
  let max = 0;
  for (let v = 0; v < valences.length; v++) {
    if (boundary[v]) continue;
    interior++;
    if (valences[v] === 6) regular++;
    min = Math.min(min, valences[v]);
    max = Math.max(max, valences[v]);
  }
  return { valences, boundary, interior, regular, min: interior === 0 ? 0 : min, max };
}

// ---------------------------------------------------------------------------
// Subjects
//
// Building the input mesh is demo work; every pass applied to one below is a
// generated member. The polyhedra come from `PolygonMesh3D`'s own statics and
// are triangulated by the generated `ToTriangleMesh`; the open subjects are
// laid out here because the stdlib has no open-patch constructor.
// ---------------------------------------------------------------------------

/** An open square patch, `n` by `n` quads split into two triangles each. */
function sheet(n: number, height: (x: number, z: number) => number): TriangleMesh3D {
  const positions: Point3D[] = [];
  for (let j = 0; j <= n; j++) {
    for (let i = 0; i <= n; i++) {
      const x = (i / n) * 2 - 1;
      const z = (j / n) * 2 - 1;
      positions.push(new Point3D(x, height(x, z), z));
    }
  }
  const at = (i: number, j: number): VertexIndex => new VertexIndex(j * (n + 1) + i);
  const faces: TriangleFace[] = [];
  for (let j = 0; j < n; j++) {
    for (let i = 0; i < n; i++) {
      faces.push(new TriangleFace(at(i, j), at(i + 1, j), at(i + 1, j + 1)));
      faces.push(new TriangleFace(at(i, j), at(i + 1, j + 1), at(i, j + 1)));
    }
  }
  return new TriangleMesh3D(fromArray(positions), fromArray(faces));
}

const ridge = (x: number, z: number): number => 0.32 * Math.sin(3 * x) * Math.cos(3 * z);
const rough = (x: number, z: number): number =>
  0.3 * Math.sin(3 * x) * Math.cos(3 * z) + 0.11 * Math.sin(9 * x + 5 * z);

/**
 * An open triangular tube: two three-vertex rings, walls triangulated, no caps.
 * The smallest subject on which the LINK CONDITION actually refuses a collapse
 * — every boundary-ring edge sees two common neighbours where a boundary edge
 * may see only one, so `CollapseEdge` returns the mesh unchanged on six of its
 * twelve edges.
 */
function triangularTube(): TriangleMesh3D {
  const r = 0.85;
  const positions: Point3D[] = [];
  for (const y of [-0.75, 0.75]) {
    for (let i = 0; i < 3; i++) {
      const a = (i / 3) * Math.PI * 2;
      positions.push(new Point3D(r * Math.cos(a), y, r * Math.sin(a)));
    }
  }
  const faces: TriangleFace[] = [];
  for (let i = 0; i < 3; i++) {
    const a = i;
    const b = (i + 1) % 3;
    faces.push(new TriangleFace(new VertexIndex(a), new VertexIndex(b), new VertexIndex(3 + b)));
    faces.push(new TriangleFace(new VertexIndex(a), new VertexIndex(3 + b), new VertexIndex(3 + a)));
  }
  return new TriangleMesh3D(fromArray(positions), fromArray(faces));
}

/**
 * A closed lumpy ball at 32 triangles: one level of the generated
 * `LoopSubdivided` over an octahedron, then a radial modulation. Small on
 * purpose — see the cost note; a smoothing scene at 128 triangles costs more
 * than a second per iteration.
 */
function lumpyBall(): TriangleMesh3D {
  const base = (polyhedron('Octahedron').LoopSubdivided());
  const moved = toArray(base.Positions).map(p => {
    const radius = Math.hypot(p.X, p.Y, p.Z) || 1;
    const lat = Math.asin(clamp(p.Y / radius, -1, 1));
    const lon = Math.atan2(p.Z, p.X);
    const k = (1 + 0.24 * Math.sin(5 * lon) * Math.cos(4 * lat)) / radius;
    return new Point3D(p.X * k, p.Y * k, p.Z * k);
  });
  return new TriangleMesh3D(fromArray(moved), base.Faces);
}

const polyhedra: Record<string, () => PolygonMesh3D> = {
  Tetrahedron: () => PolygonMesh3D.Tetrahedron(),
  Cube: () => PolygonMesh3D.Cube(),
  Octahedron: () => PolygonMesh3D.Octahedron(),
  Dodecahedron: () => PolygonMesh3D.Dodecahedron(),
  Icosahedron: () => PolygonMesh3D.Icosahedron(),
};

const polyhedronCache = new Map<string, TriangleMesh3D>();

/** A named Platonic solid, triangulated by the generated `ToTriangleMesh`. */
function polyhedron(name: string): TriangleMesh3D {
  let hit = polyhedronCache.get(name);
  if (!hit) {
    hit = (polyhedra[name]().ToTriangleMesh());
    polyhedronCache.set(name, hit);
  }
  return hit;
}

interface Subject {
  label: string;
  closed: boolean;
  build: () => TriangleMesh3D;
}

const SUBJECTS: Subject[] = [
  { label: 'Tetrahedron (4)', closed: true, build: () => polyhedron('Tetrahedron') },
  { label: 'Tube, open (6)', closed: false, build: triangularTube },
  { label: 'Octahedron (8)', closed: true, build: () => polyhedron('Octahedron') },
  { label: 'Cube ▸ tris (12)', closed: true, build: () => polyhedron('Cube') },
  { label: 'Sheet, open (18)', closed: false, build: () => (sheet(3, ridge)) },
  { label: 'Icosahedron (20)', closed: true, build: () => polyhedron('Icosahedron') },
  { label: 'Lumpy ball (32)', closed: true, build: lumpyBall },
  { label: 'Rough sheet, open (32)', closed: false, build: () => (sheet(4, rough)) },
];

const SUBJECT_LABELS = SUBJECTS.map(s => s.label);
const subjectCache = new Map<number, TriangleMesh3D>();

function subject(index: number): TriangleMesh3D {
  const i = clampIndex(index, SUBJECTS.length);
  let hit = subjectCache.get(i);
  if (!hit) {
    hit = SUBJECTS[i].build();
    subjectCache.set(i, hit);
  }
  return hit;
}

/** The subject list restricted to a few entries, for a scene that needs fewer. */
function subjectControl(indices: number[], def: number): { control: Control; pick: (p: Params) => number } {
  return {
    control: {
      key: 'subject',
      label: 'Subject',
      kind: 'select',
      options: indices.map(i => SUBJECT_LABELS[i]),
      def,
    },
    pick: p => indices[clampIndex(p.subject, indices.length)],
  };
}

// ---------------------------------------------------------------------------
// Rendering
// ---------------------------------------------------------------------------

const VALENCE_LOW = new THREE.Color(0x4f8fd6);
const VALENCE_SIX = new THREE.Color(0xdfe6ef);
const VALENCE_HIGH = new THREE.Color(0xe0894a);
const VALENCE_BOUNDARY = new THREE.Color(0x63d6a8);

/** Blue below six, orange above, pale at six, green on the boundary. */
function valenceColor(valence: number, boundary: boolean): THREE.Color {
  if (boundary) return VALENCE_BOUNDARY;
  const away = clamp(Math.abs(valence - 6) / 3, 0, 1);
  return VALENCE_SIX.clone().lerp(valence < 6 ? VALENCE_LOW : VALENCE_HIGH, away);
}

function vertexColorMaterial(): THREE.MeshStandardMaterial {
  return new THREE.MeshStandardMaterial({
    vertexColors: true,
    roughness: 0.45,
    metalness: 0.05,
    side: THREE.DoubleSide,
  });
}

/** The indexed triangle geometry, optionally carrying a per-vertex colour. */
function meshGeometry(
  mesh: TriangleMesh3D,
  colorOf?: (vertex: number) => THREE.Color,
): THREE.BufferGeometry {
  const geometry = triangleMeshGeometry(mesh);
  if (colorOf) {
    const rgb: number[] = [];
    for (let v = 0; v < mesh.Positions.Count(); v++) {
      const c = colorOf(v);
      rgb.push(c.r, c.g, c.b);
    }
    geometry.setAttribute('color', new THREE.Float32BufferAttribute(rgb, 3));
  }
  return geometry;
}

/**
 * The mesh's real edges, drawn once each from `EdgeEndpoints` rather than three
 * per face — which is also the cheapest proof that the topology found them.
 */
function topologyEdgeGeometry(
  mesh: TriangleMesh3D,
  topology: TriangleMeshTopology,
  only?: (edge: number) => boolean,
): THREE.BufferGeometry {
  const positions = toArray(mesh.Positions);
  const out: number[] = [];
  for (let e = 0; e < topology.EdgeCount(); e++) {
    if (only && !only(e)) continue;
    const pair = topology.EdgeEndpoints(new UndirectedEdgeIndex(e));
    const a = positions[pair.A.Value];
    const b = positions[pair.B.Value];
    out.push(a.X, a.Y, a.Z, b.X, b.Y, b.Z);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(out, 3));
  return geometry;
}

/** Every face's three sides, for a mesh whose topology is not already in hand. */
function faceEdgeGeometry(mesh: TriangleMesh3D): THREE.BufferGeometry {
  const positions = toArray(mesh.Positions);
  const out: number[] = [];
  for (const f of toArray(mesh.Faces)) {
    const loop = [positions[f.A.Value], positions[f.B.Value], positions[f.C.Value]];
    for (let i = 0; i < 3; i++) {
      const a = loop[i];
      const b = loop[(i + 1) % 3];
      out.push(a.X, a.Y, a.Z, b.X, b.Y, b.Z);
    }
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(out, 3));
  return geometry;
}

/** One panel of a side-by-side comparison, moved along X and captioned by name. */
function panel(object: THREE.Object3D, x: number): THREE.Object3D {
  object.position.x = x;
  return object;
}

interface PanelOptions {
  color?: number;
  edges?: boolean;
  colorOf?: (vertex: number) => THREE.Color;
}

function meshPanel(mesh: TriangleMesh3D, x: number, options: PanelOptions = {}): THREE.Object3D {
  const group = new THREE.Group();
  const material = options.colorOf ? vertexColorMaterial() : surfaceMaterial(options.color);
  group.add(new THREE.Mesh(meshGeometry(mesh, options.colorOf), material));
  if (options.edges !== false) {
    group.add(new THREE.LineSegments(faceEdgeGeometry(mesh), edgeMaterial()));
  }
  return panel(group, x);
}

// ---------------------------------------------------------------------------
// Scene 1 — Loop against Butterfly
//
// Both take their TOPOLOGY from the same `SplitEdges` with every edge masked
// and differ only in the position array, which is what makes the comparison
// exact: same face count, same numbering, and the input vertices occupy the
// same leading block of both results. So the interpolation claim is one
// subtraction — Butterfly must leave that block at drift zero, Loop must not.
//
// Levels are capped per seed. A level multiplies faces by four and the cost of
// a level is super-quadratic in the corner count, so the last level dominates:
// one Loop level over 32 triangles is about 30 ms and over 128 about 2 s.
// ---------------------------------------------------------------------------

const SUBDIV_SEEDS = ['Tetrahedron (4)', 'Octahedron (8)', 'Cube ▸ tris (12)', 'Icosahedron (20)'];
const SUBDIV_SEED_NAMES = ['Tetrahedron', 'Octahedron', 'Cube', 'Icosahedron'];
/** Levels each seed can afford: the cap lands every one of them near 128–256 faces. */
const SUBDIV_MAX_LEVEL = [3, 2, 2, 2];

const subdivCache = new Map<string, TriangleMesh3D>();

function subdivided(seed: number, butterfly: boolean, level: number): TriangleMesh3D {
  const key = `${seed}:${butterfly ? 'B' : 'L'}:${level}`;
  const hit = subdivCache.get(key);
  if (hit) return hit;
  const previous = level <= 0 ? polyhedron(SUBDIV_SEED_NAMES[seed]) : subdivided(seed, butterfly, level - 1);
  const built =
    level <= 0 ? previous : (butterfly ? previous.ButterflySubdivided() : previous.LoopSubdivided());
  subdivCache.set(key, built);
  return built;
}

const loopVsButterfly = sceneOf({
  id: 'loop-vs-butterfly',
  title: 'Loop against Butterfly',
  description:
    'Both schemes refine every triangle into four through the same masked SplitEdges, so they differ only in where the vertices land. Loop is approximating and moves the old vertices; Butterfly is interpolating and leaves them exactly where they were, which the drift reading checks.',
  plato: [
    'TriangleMesh3D.LoopSubdivided',
    'TriangleMesh3D.ButterflySubdivided',
    'TriangleMesh3D.SplitEdges',
    'TriangleMeshTopology.AllEdgesMask',
    'PolygonMesh3D.ToTriangleMesh',
    'TriangleMesh3D.TopologyOf',
    'TriangleMesh3D.AverageEdgeLength',
  ],
  viewer: { distance: 6.2 },
  controls: [
    { key: 'seed', label: 'Seed', kind: 'select', options: SUBDIV_SEEDS, def: 1 },
    { key: 'level', label: 'Levels (capped per seed)', kind: 'slider', min: 0, max: 3, step: 1, def: 2 },
    { key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 },
    { key: 'cage', label: 'Ghost the input', kind: 'toggle', def: 1 },
  ],
  build(params) {
    const seed = clampIndex(params.seed, SUBDIV_SEEDS.length);
    const asked = clamp(Math.round(params.level ?? 2), 0, 3);
    const level = Math.min(asked, SUBDIV_MAX_LEVEL[seed]);
    const base = polyhedron(SUBDIV_SEED_NAMES[seed]);
    const loop = subdivided(seed, false, level);
    const butterfly = subdivided(seed, true, level);
    const edges = (params.edges ?? 1) !== 0;

    const group = new THREE.Group();
    group.add(meshPanel(loop, -1.35, { color: palette.surface, edges }));
    group.add(meshPanel(butterfly, 1.35, { color: palette.surfaceAlt, edges }));
    if ((params.cage ?? 1) !== 0) {
      for (const x of [-1.35, 1.35]) {
        group.add(panel(new THREE.LineSegments(faceEdgeGeometry(base), edgeMaterial(palette.line)), x));
      }
    }

    const seedVertices = base.Positions.Count();
    return {
      object: group,
      readings: [
        note('seed', `${seedVertices} v / ${base.Faces.Count()} f`),
        note('level', `${level}${level < asked ? ` (capped from ${asked})` : ''}`),
        reading('LoopSubdivided', () => `${loop.Positions.Count()} v / ${loop.Faces.Count()} f`),
        reading('ButterflySubdivided', () => `${butterfly.Positions.Count()} v / ${butterfly.Faces.Count()} f`),
        note(
          'faces',
          `${base.Faces.Count()} × 4^${level} = ${base.Faces.Count() * 4 ** level}` +
            (loop.Faces.Count() === base.Faces.Count() * 4 ** level ? ' — agrees' : ' — DISAGREES'),
        ),
        reading('Loop drift on the input vertices', () => n4(drift(base, loop, seedVertices))),
        reading('Butterfly drift on the input vertices', () => {
          const d = drift(base, butterfly, seedVertices);
          return `${n4(d)}${d === 0 ? ' — exactly interpolating' : ' — NOT interpolating'}`;
        }),
        reading('AverageEdgeLength Loop / Butterfly', () =>
          `${n4(loop.AverageEdgeLength())} / ${n4(butterfly.AverageEdgeLength())}`,
        ),
        // The size reading is what makes the two averages above legible: Loop's
        // mesh is not finer than Butterfly's, it is SMALLER — an approximating
        // scheme pulls a coarse polyhedron a long way in on the first level,
        // while an interpolating one is nailed to the input's own corners.
        reading('enclosed volume seed / Loop / Butterfly', () =>
          `${n3(signedVolume(base))} / ${n3(signedVolume(loop))} / ${n3(signedVolume(butterfly))}`,
        ),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 2 — the three named schemes, through `SubdivisionScheme`
//
// `SubdividedOnce` is the dispatcher on the sum type and `Subdivided` drives the
// level count; `SubdivisionSurface` names a cage, a scheme and a level, and
// `RefinedControlMesh` is that triple applied — so the scene builds the surface
// value and asks it for its own refinement rather than calling `Subdivided`
// directly.
//
// Catmull-Clark and Doo-Sabin both require a closed manifold for their vertex
// rules; every cage here is one. Doo-Sabin on an open mesh throws rather than
// producing a crease, which is its documented precondition, not a defect.
// ---------------------------------------------------------------------------

const CAGE_LABELS = ['Cube', 'Tetrahedron', 'Octahedron', 'Dodecahedron', 'Icosahedron'];

/**
 * How many levels a (cage, scheme) pair can afford. Every scheme multiplies the
 * corner count by four per level and every one of them is quadratic in that
 * count, so the cap is per pair rather than global: three Catmull-Clark levels
 * on a cube and three on a dodecahedron differ by a factor of six in corners
 * and by rather more in seconds. Loop gets a tighter cap because it is written
 * over the triangle topology, whose build is the expensive one.
 */
function affordableLevel(cage: number, scheme: number, asked: number): number {
  const control = polyhedra[CAGE_LABELS[cage]]();
  let level = asked;
  if (scheme === 1) {
    let triangles = 0;
    for (let f = 0; f < control.FaceCount(); f++) triangles += control.FaceArity(f) - 2;
    while (level > 0 && triangles * 4 ** level > 200) level--;
  } else {
    const corners = control.CornerCount();
    while (level > 0 && corners * 4 ** level > 1600) level--;
  }
  return level;
}

const cageCache = new Map<string, PolygonMesh3D>();

function refined(cage: number, scheme: number, level: number): PolygonMesh3D {
  const key = `${cage}:${scheme}:${level}`;
  const hit = cageCache.get(key);
  if (hit) return hit;
  const control = polyhedra[CAGE_LABELS[cage]]();
  const surface = new SubdivisionSurface(control, schemeCase(scheme) as never, level);
  const built = (surface.RefinedControlMesh());
  cageCache.set(key, built);
  return built;
}

const schemes = sceneOf({
  id: 'subdivision-schemes',
  title: 'Catmull-Clark, Loop, Doo-Sabin',
  description:
    'SubdivisionScheme dispatches SubdividedOnce three ways: Catmull-Clark emits one quadrilateral per corner, Doo-Sabin one face per original face, edge and vertex, and Loop triangulates first and refines as a triangle scheme. A SubdivisionSurface names the cage, the scheme and the level, and RefinedControlMesh is that triple applied.',
  plato: [
    'SubdivisionSurface.RefinedControlMesh',
    'PolygonMesh3D.Subdivided',
    'PolygonMesh3D.SubdividedOnce',
    'PolygonMesh3D.CatmullClarkSubdivided',
    'PolygonMesh3D.DooSabinSubdivided',
    'TriangleMesh3D.LoopSubdivided',
    'TriangleMesh3D.ToPolygonMesh',
  ],
  controls: [
    { key: 'cage', label: 'Control mesh', kind: 'select', options: CAGE_LABELS, def: 0 },
    { key: 'scheme', label: 'Scheme', kind: 'select', options: SCHEME_LABELS, def: 0 },
    { key: 'level', label: 'Levels (capped per scheme)', kind: 'slider', min: 0, max: 3, step: 1, def: 2 },
    { key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 },
    { key: 'cageLines', label: 'Ghost the cage', kind: 'toggle', def: 1 },
  ],
  build(params) {
    const cage = clampIndex(params.cage, CAGE_LABELS.length);
    const scheme = clampIndex(params.scheme, SCHEME_LABELS.length);
    const asked = clamp(Math.round(params.level ?? 2), 0, 3);
    const level = affordableLevel(cage, scheme, asked);
    const control = polyhedra[CAGE_LABELS[cage]]();
    const mesh = refined(cage, scheme, level);

    const group = new THREE.Group();
    group.add(new THREE.Mesh(polygonMeshGeometry(mesh), surfaceMaterial()));
    if ((params.edges ?? 1) !== 0) {
      group.add(new THREE.LineSegments(polygonMeshEdges(mesh), edgeMaterial()));
    }
    if ((params.cageLines ?? 1) !== 0) {
      group.add(new THREE.LineSegments(polygonMeshEdges(control), edgeMaterial(palette.line)));
    }

    const faces = mesh.FaceCount();
    const arities = new Map<number, number>();
    for (let f = 0; f < faces; f++) {
      const arity = mesh.FaceArity(f);
      arities.set(arity, (arities.get(arity) ?? 0) + 1);
    }
    const arityText = [...arities.entries()]
      .sort((a, b) => a[0] - b[0])
      .map(([arity, count]) => `${count}×${arity}-gon`)
      .join(', ');

    return {
      object: group,
      readings: [
        note('cage', `${CAGE_LABELS[cage]} — ${control.VertexCount()} v / ${control.FaceCount()} f`),
        note('scheme', SCHEME_LABELS[scheme]),
        note('level', `${level}${level < asked ? ` (capped from ${asked})` : ''}`),
        reading('RefinedControlMesh', () => `${mesh.VertexCount()} v / ${faces} f`),
        note('face arity', arityText),
        // The polygon-mesh edge census is quadratic in the corner count, so it
        // is only worth taking while the mesh is small enough to be honest about.
        faces <= 128
          ? reading('V − E + F', () => {
              const edges = mesh.UndirectedEdgeCount();
              return `${mesh.VertexCount()} − ${edges} + ${faces} = ${mesh.VertexCount() - edges + faces}`;
            })
          : note('V − E + F', `not taken above 128 faces (PolygonMesh3D.UndirectedEdgeCount is quadratic)`),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 3 — smoothing, four ways at matched iterations
//
// Connectivity never changes here, so the four panels differ only in where the
// vertices went, and the volume reading is the whole point: a Laplacian step
// shrinks, Taubin's inflating second step is what recovers most of it, and
// tangential relaxation moves vertices ALONG the surface and so leaves the
// volume where it was.
//
// Every iteration is a separate call so `smoothCache` can reuse the first n-1
// iterations when a slider moves by one; the memoized `Arr` (plato-436) makes
// passing the count straight to `LaplacianSmoothed` equally sound.
// ---------------------------------------------------------------------------

type SmoothKind = 'laplacian' | 'taubin' | 'tangential';

const smoothCache = new Map<string, TriangleMesh3D>();

function smoothed(
  subjectIndex: number,
  kind: SmoothKind,
  weighting: number,
  strength: number,
  iterations: number,
): TriangleMesh3D {
  const key = `${subjectIndex}:${kind}:${weighting}:${strength}:${iterations}`;
  const hit = smoothCache.get(key);
  if (hit) return hit;
  if (iterations <= 0) {
    const base = subject(subjectIndex);
    smoothCache.set(key, base);
    return base;
  }
  const previous = smoothed(subjectIndex, kind, weighting, strength, iterations - 1);
  const w = weightingCase(weighting) as never;
  const built =
    kind === 'laplacian'
      ? (previous.LaplacianSmoothed(w, strength, 1))
      : kind === 'taubin'
        ? (previous.TaubinSmoothed(w, strength, -(strength + 0.03), 1))
        : (previous.TangentiallyRelaxed(strength, 1));
  smoothCache.set(key, built);
  return built;
}

/** Did the two-argument `TaubinSmoothed(iterations)` survive the writer? */
const TAUBIN_DEFAULTS_FAILURE = ((): string => {
  try {
    const mesh = polyhedron('Octahedron') as unknown as { TaubinSmoothed(iterations: number): TriangleMesh3D };
    (mesh.TaubinSmoothed(1));
    return '';
  } catch (error) {
    return (error as Error).message;
  }
})();

const SMOOTH_SUBJECTS = subjectControl([6, 7, 4, 2], 0);

const smoothing = sceneOf({
  id: 'smoothing',
  title: 'Laplacian, Taubin, tangential',
  description:
    'Three smoothing passes at matched iteration counts, over connectivity none of them changes. LaplacianSmoothed shrinks a little every step; TaubinSmoothed follows each shrinking step with an inflating one and keeps far more of the volume; TangentiallyRelaxed removes the normal component of the same displacement, so vertices redistribute over the surface instead of leaving it. Boundary vertices are pinned throughout.',
  plato: [
    'TriangleMesh3D.LaplacianSmoothed',
    'TriangleMesh3D.TaubinSmoothed',
    'TriangleMesh3D.TangentiallyRelaxed',
    'TriangleMesh3D.LaplacianField',
    'TriangleMesh3D.UniformLaplacianField',
    'TriangleMesh3D.CotangentLaplacianField',
    'TriangleMesh3D.VertexNormalVectors',
    'TriangleMesh3D.BoundaryVertexFlags',
    'Point3D.SixTimesSignedVolume',
  ],
  viewer: { distance: 11, spin: false },
  controls: [
    SMOOTH_SUBJECTS.control,
    { key: 'weighting', label: 'Laplacian weighting', kind: 'select', options: WEIGHTING_LABELS, def: 0 },
    { key: 'iterations', label: 'Iterations', kind: 'slider', min: 0, max: 12, step: 1, def: 4 },
    { key: 'strength', label: 'Strength (λ)', kind: 'slider', min: 0.05, max: 0.9, step: 0.05, def: 0.5 },
    { key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 },
  ],
  build(params) {
    const index = SMOOTH_SUBJECTS.pick(params);
    const weighting = clampIndex(params.weighting, WEIGHTING_LABELS.length);
    // Cotangent weights cost about three times what uniform ones do — every
    // vertex scans every edge for its weight, and `EdgeCotangentWeights` is
    // rebuilt lazily at each read — so the iteration ceiling is per weighting.
    const ceiling = weighting === 1 ? 5 : 12;
    const asked = clamp(Math.round(params.iterations ?? 4), 0, 12);
    const iterations = Math.min(asked, ceiling);
    const strength = clamp(params.strength ?? 0.5, 0.05, 0.9);
    const base = subject(index);
    const edges = (params.edges ?? 1) !== 0;

    const laplacian = smoothed(index, 'laplacian', weighting, strength, iterations);
    const taubin = smoothed(index, 'taubin', weighting, strength, iterations);
    const tangential = smoothed(index, 'tangential', weighting, strength, iterations);

    const group = new THREE.Group();
    group.add(meshPanel(base, -3.3, { color: palette.line, edges }));
    group.add(meshPanel(laplacian, -1.1, { color: palette.surface, edges }));
    group.add(meshPanel(taubin, 1.1, { color: palette.surfaceAlt, edges }));
    group.add(meshPanel(tangential, 3.3, { color: palette.accent, edges }));

    const closed = SUBJECTS[index].closed;
    const v0 = signedVolume(base);
    const ratio = (m: TriangleMesh3D): string => (closed ? `×${n4(signedVolume(m) / v0)}` : 'open — no volume');
    const topology = base.TopologyOf();
    const boundary = toArray(base.BoundaryVertexFlags(topology));
    const boundaryDrift = (m: TriangleMesh3D): number => {
      const a = toArray(base.Positions);
      const b = toArray(m.Positions);
      let far = 0;
      for (let v = 0; v < a.length; v++) if (boundary[v]) far = Math.max(far, a[v].Distance(b[v]));
      return far;
    };

    return {
      object: group,
      readings: [
        note('panels', 'input · Laplacian · Taubin · tangential'),
        note(
          'subject',
          `${SUBJECTS[index].label} — ${base.Positions.Count()} v / ${base.Faces.Count()} f` +
            `, ${boundary.filter(Boolean).length} boundary vertices`,
        ),
        note(
          'iterations',
          `${iterations}${iterations < asked ? ` (capped from ${asked})` : ''}` +
            ` at λ ${n2(strength)}, μ ${n2(-(strength + 0.03))}, ${WEIGHTING_LABELS[weighting]} weights`,
        ),
        reading('LaplacianSmoothed volume', () => ratio(laplacian)),
        reading('TaubinSmoothed volume', () => ratio(taubin)),
        reading('TangentiallyRelaxed volume', () => ratio(tangential)),
        reading('farthest vertex moved (L / T / R)', () =>
          `${n3(drift(base, laplacian, base.Positions.Count()))} / ` +
          `${n3(drift(base, taubin, base.Positions.Count()))} / ` +
          `${n3(drift(base, tangential, base.Positions.Count()))}`,
        ),
        note(
          'boundary pinned',
          boundary.some(Boolean)
            ? `max boundary movement ${n4(Math.max(boundaryDrift(laplacian), boundaryDrift(taubin), boundaryDrift(tangential)))}`
            : 'closed subject — no boundary',
        ),
        TAUBIN_DEFAULTS_FAILURE
          ? note('TaubinSmoothed(iterations)', `UNAVAILABLE (${TAUBIN_DEFAULTS_FAILURE})`)
          : note('TaubinSmoothed(iterations)', 'the two-argument overload at λ 0.5, μ −0.53'),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 4 — isotropic remeshing, one Botsch-Kobbelt pass at a time
//
// Each pass is the library's own `IsotropicRemeshPass` — split long, collapse
// short, flip toward valence six, tangentially relax, at the 4/3 and 4/5
// factors — called as one member. (Before plato-436 the four inner passes
// chained lazily and cost minutes, so this scene used to spell them out with a
// materialization between each; the memoized `Arr` retired that.)
//
// Face counts move fast: at k = 0.5 a pass roughly quadruples them, so the walk
// stops once the mesh passes the affordability cap and the status line says so.
// ---------------------------------------------------------------------------

const isotropicPass = (mesh: TriangleMesh3D, target: number): TriangleMesh3D =>
  mesh.IsotropicRemeshPass(target);

/** Past this a pass costs seconds; the walk stops and the status line says so. */
const ISOTROPIC_FACE_CAP = 180;

const isotropicCache = new Map<string, TriangleMesh3D>();

function isotropic(subjectIndex: number, factor: number, passes: number): TriangleMesh3D {
  const key = `${subjectIndex}:${factor}:${passes}`;
  const hit = isotropicCache.get(key);
  if (hit) return hit;
  if (passes <= 0) {
    const base = subject(subjectIndex);
    isotropicCache.set(key, base);
    return base;
  }
  const previous = isotropic(subjectIndex, factor, passes - 1);
  const built =
    previous.Faces.Count() > ISOTROPIC_FACE_CAP
      ? previous
      : isotropicPass(previous, subject(subjectIndex).AverageEdgeLength() * factor);
  isotropicCache.set(key, built);
  return built;
}

const ISOTROPIC_SUBJECTS = subjectControl([4, 7, 6, 2, 5], 0);

const isotropicScene = sceneOf({
  id: 'isotropic',
  title: 'Isotropic remeshing, one pass at a time',
  description:
    'One Botsch-Kobbelt iteration is SplitLongEdges above 4/3 of the target, CollapseShortEdges below 4/5 of it, FlipTowardValenceSix, and one tangential relaxation. The target is a multiple of the mesh AverageEdgeLength: below one it refines, above one it coarsens, and coarsening needs more passes than refining because collapses conflict with each other where splits do not. Vertices are coloured by valence, so the flips are visible.',
  plato: [
    'TriangleMesh3D.SplitLongEdges',
    'TriangleMesh3D.CollapseShortEdges',
    'TriangleMesh3D.FlipTowardValenceSix',
    'TriangleMesh3D.TangentiallyRelaxed',
    'TriangleMesh3D.AverageEdgeLength',
    'TriangleMesh3D.VertexValences',
    'TriangleMesh3D.BoundaryVertexFlags',
    'TriangleMeshTopology.EdgeCount',
    'TriangleMeshTopology.IsBoundaryEdge',
    'TriangleMesh3D.EdgeLength',
  ],
  controls: [
    ISOTROPIC_SUBJECTS.control,
    { key: 'factor', label: 'Target × AverageEdgeLength', kind: 'slider', min: 0.5, max: 2, step: 0.1, def: 1.4 },
    { key: 'passes', label: 'Passes', kind: 'slider', min: 0, max: 6, step: 1, def: 1 },
    { key: 'valence', label: 'Colour by valence', kind: 'toggle', def: 1 },
    { key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 },
  ],
  build(params) {
    const index = ISOTROPIC_SUBJECTS.pick(params);
    const factor = clamp(params.factor ?? 1.4, 0.5, 2);
    const passes = clamp(Math.round(params.passes ?? 1), 0, 6);
    const base = subject(index);
    const target = base.AverageEdgeLength() * factor;
    const mesh = isotropic(index, factor, passes);

    const topology = mesh.TopologyOf();
    const valence = valenceStats(mesh, topology);
    const stats = edgeStats(mesh, topology);
    const baseStats = edgeStats(base, base.TopologyOf());

    const group = new THREE.Group();
    const colorOf =
      (params.valence ?? 1) !== 0
        ? (v: number): THREE.Color => valenceColor(valence.valences[v], valence.boundary[v])
        : undefined;
    group.add(
      new THREE.Mesh(meshGeometry(mesh, colorOf), colorOf ? vertexColorMaterial() : surfaceMaterial()),
    );
    if ((params.edges ?? 1) !== 0) {
      group.add(new THREE.LineSegments(topologyEdgeGeometry(mesh, topology), edgeMaterial()));
    }

    const capped = mesh.Faces.Count() > ISOTROPIC_FACE_CAP;
    return {
      object: group,
      readings: [
        note('subject', `${SUBJECTS[index].label} — ${base.Faces.Count()} f, average edge ${n4(baseStats.average)}`),
        note('target', `${n4(target)} (×${n2(factor)}) · split above ${n4((target * 4) / 3)} · collapse below ${n4((target * 4) / 5)}`),
        note('passes', `${passes}${capped ? ` — stopped at the ${ISOTROPIC_FACE_CAP}-face cap` : ''}`),
        reading('result', () => `${mesh.Positions.Count()} v / ${mesh.Faces.Count()} f / ${stats.edges} e`),
        reading('AverageEdgeLength', () => `${n4(stats.average)} (target ${n4(target)}, ×${n2(stats.average / target)})`),
        reading('edge spread', () => `${n4(stats.shortest)} .. ${n4(stats.longest)}`),
        reading('interior valence', () =>
          valence.interior === 0
            ? 'no interior vertices'
            : `${valence.regular}/${valence.interior} at six, range ${valence.min}..${valence.max}`,
        ),
        reading('boundary edges', () =>
          `${stats.boundaryEdges} (input had ${baseStats.boundaryEdges})` +
          (stats.boundaryEdges === baseStats.boundaryEdges ? ' — preserved' : ''),
        ),
        note('IsotropicRemeshPass', 'called as one member — the four steps above are its body'),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 5 — the batched length-driven passes, and the quadric gap
//
// Each of the three is a whole pass over one mask, and each shows its conflict
// rule directly: `CollapseShortEdges` takes only an independent set per pass —
// no two accepted collapses may have adjacent endpoints — so a second pass on
// the same threshold removes more, which is exactly why decimation is written
// as many passes rather than one large budget.
//
// Quadric decimation is what this scene would rather be showing. It is
// unreachable; the readings name each member that throws.
// ---------------------------------------------------------------------------

const BATCH_LABELS = ['SplitLongEdges', 'CollapseShortEdges', 'FlipTowardValenceSix'];

const batchCache = new Map<string, TriangleMesh3D>();

function batched(subjectIndex: number, operation: number, factor: number, passes: number): TriangleMesh3D {
  const key = `${subjectIndex}:${operation}:${factor}:${passes}`;
  const hit = batchCache.get(key);
  if (hit) return hit;
  if (passes <= 0) {
    const base = subject(subjectIndex);
    batchCache.set(key, base);
    return base;
  }
  const previous = batched(subjectIndex, operation, factor, passes - 1);
  const threshold = subject(subjectIndex).AverageEdgeLength() * factor;
  const built =
    previous.Faces.Count() > ISOTROPIC_FACE_CAP
      ? previous
      : operation === 0
        ? (previous.SplitLongEdges(threshold))
        : operation === 1
          ? (previous.CollapseShortEdges(threshold))
          : (previous.FlipTowardValenceSix());
  batchCache.set(key, built);
  return built;
}

/**
 * The quadric family, probed once at module load. `PlaneQuadric` and
 * `Quadric.ZeroQuadric` are emitted as `new Quadric(new Tuple4(…))` where
 * `Quadric.Coefficients` is declared `Matrix4x4`, so nothing downstream of them
 * can read a row or scale a form — see the file header.
 */
const QUADRIC_READINGS: Reading[] = ((): Reading[] => {
  const ball = polyhedron('Octahedron');
  const plane = new Plane(new Direction3D(new Vector3D(0, 1, 0)), 0);
  return [
    reading('Plane.PlaneQuadric', () => {
      const kind = (plane.PlaneQuadric().Coefficients as unknown as object).constructor.name;
      return kind === 'Matrix4x4' ? 'Matrix4x4' : `Coefficients is a ${kind}, not the declared Matrix4x4`;
    }),
    reading('Quadric.QuadricError', () => n4(plane.PlaneQuadric().QuadricError(new Point3D(0, 2, 0)))),
    reading('Triangle3D.TriangleQuadric', () => String(ball.Triangle(new FaceIndex(0)).TriangleQuadric())),
    reading('TriangleMesh3D.VertexQuadrics', () => String(ball.VertexQuadrics().At(0))),
    reading('TriangleMesh3D.Decimated', () => `${(ball.Decimated(4, 3)).Faces.Count()} f`),
  ];
})();

const BATCH_SUBJECTS = subjectControl([7, 4, 6, 5, 2], 0);

const coarsening = sceneOf({
  id: 'batched-passes',
  title: 'Batched passes, and the quadric gap',
  description:
    'The three length- and valence-driven passes, applied one at a time. Every decision is taken against the same input mesh, so the pass needs a conflict rule: no two accepted collapses may have adjacent endpoints, and a flip claims all four of its vertices. A rejected edge is simply reconsidered next pass, which is why the second pass at the same threshold still removes more. Quadric-error decimation belongs here too and cannot run: the readings name each member of that family and the message it raises.',
  plato: [
    'TriangleMesh3D.SplitLongEdges',
    'TriangleMesh3D.CollapseShortEdges',
    'TriangleMesh3D.FlipTowardValenceSix',
    'TriangleMesh3D.CollapseEdges',
    'TriangleMesh3D.SplitEdges',
    'TriangleMesh3D.FlipEdges',
    'TriangleMesh3D.VertexValences',
    'TriangleMesh3D.AverageEdgeLength',
    'TriangleMesh3D.VertexQuadrics',
    'TriangleMesh3D.Decimated',
    'Quadric.QuadricError',
    'Quadric.QuadricMinimizer',
  ],
  controls: [
    BATCH_SUBJECTS.control,
    { key: 'operation', label: 'Pass', kind: 'select', options: BATCH_LABELS, def: 1 },
    { key: 'factor', label: 'Threshold × AverageEdgeLength', kind: 'slider', min: 0.4, max: 2.5, step: 0.1, def: 1.1 },
    { key: 'passes', label: 'Passes', kind: 'slider', min: 0, max: 5, step: 1, def: 2 },
    { key: 'valence', label: 'Colour by valence', kind: 'toggle', def: 1 },
    { key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 },
  ],
  build(params) {
    const index = BATCH_SUBJECTS.pick(params);
    const operation = clampIndex(params.operation, BATCH_LABELS.length);
    const factor = clamp(params.factor ?? 1.1, 0.4, 2.5);
    const passes = clamp(Math.round(params.passes ?? 2), 0, 5);
    const base = subject(index);
    const threshold = base.AverageEdgeLength() * factor;

    const trail: string[] = [];
    for (let p = 0; p <= passes; p++) {
      trail.push(String(batched(index, operation, factor, p).Faces.Count()));
    }
    const mesh = batched(index, operation, factor, passes);

    const topology = mesh.TopologyOf();
    const valence = valenceStats(mesh, topology);
    const stats = edgeStats(mesh, topology);

    const group = new THREE.Group();
    const colorOf =
      (params.valence ?? 1) !== 0
        ? (v: number): THREE.Color => valenceColor(valence.valences[v], valence.boundary[v])
        : undefined;
    group.add(
      new THREE.Mesh(meshGeometry(mesh, colorOf), colorOf ? vertexColorMaterial() : surfaceMaterial()),
    );
    if ((params.edges ?? 1) !== 0) {
      group.add(new THREE.LineSegments(topologyEdgeGeometry(mesh, topology), edgeMaterial()));
    }

    return {
      object: group,
      readings: [
        note('subject', `${SUBJECTS[index].label} — ${base.Positions.Count()} v / ${base.Faces.Count()} f`),
        note(
          'pass',
          `${BATCH_LABELS[operation]}${operation === 2 ? '' : ` at ${n4(threshold)} (×${n2(factor)} average)`}`,
        ),
        note('faces per pass', trail.join(' → ')),
        reading('result', () => `${mesh.Positions.Count()} v / ${mesh.Faces.Count()} f / ${stats.edges} e`),
        reading('AverageEdgeLength', () => `${n4(stats.average)} (was ${n4(base.AverageEdgeLength())})`),
        reading('interior valence', () =>
          valence.interior === 0
            ? 'no interior vertices'
            : `${valence.regular}/${valence.interior} at six, range ${valence.min}..${valence.max}`,
        ),
        ...QUADRIC_READINGS,
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 6 — one edge at a time
//
// The single-edge operators ARE the batched ones under a one-edge mask, so this
// is the same machinery at the smallest scale — and the scale at which the
// refusals are legible. A collapse that fails the LINK CONDITION returns the
// mesh unchanged, which is correct: an edge whose endpoints share a neighbour
// that is not the apex of one of its triangles has no contraction that keeps the
// surface a manifold. The open tube is the smallest subject where that happens.
// A flip is refused on a boundary edge and wherever the diagonal it would draw
// is already an edge, which on a tetrahedron is every edge.
// ---------------------------------------------------------------------------

const OPERATOR_LABELS = ['SplitEdge', 'CollapseEdge', 'FlipEdge'];

function applyOperator(mesh: TriangleMesh3D, operator: number, edge: number): TriangleMesh3D {
  const index = new UndirectedEdgeIndex(edge);
  return operator === 0
    ? mesh.SplitEdge(index)
    : operator === 1
      ? mesh.CollapseEdge(index)
      : mesh.FlipEdge(index);
}

/** Unchanged means refused: a split always lands, the other two may not. */
function refused(before: TriangleMesh3D, after: TriangleMesh3D): boolean {
  if (before.Positions.Count() !== after.Positions.Count()) return false;
  if (before.Faces.Count() !== after.Faces.Count()) return false;
  const a = toArray(before.Faces);
  const b = toArray(after.Faces);
  return a.every((f, i) => f.Equals(b[i]));
}

const operatorCache = new Map<string, TriangleMesh3D>();

function operatorResult(subjectIndex: number, operator: number, edge: number): TriangleMesh3D {
  const key = `${subjectIndex}:${operator}:${edge}`;
  let hit = operatorCache.get(key);
  if (!hit) {
    hit = applyOperator(subject(subjectIndex), operator, edge);
    operatorCache.set(key, hit);
  }
  return hit;
}

const censusCache = new Map<string, string>();

/** How many of a subject's edges each operator actually accepts. */
function census(subjectIndex: number, operator: number): string {
  const key = `${subjectIndex}:${operator}`;
  let hit = censusCache.get(key);
  if (hit === undefined) {
    const mesh = subject(subjectIndex);
    const edges = mesh.TopologyOf().EdgeCount();
    let accepted = 0;
    for (let e = 0; e < edges; e++) if (!refused(mesh, operatorResult(subjectIndex, operator, e))) accepted++;
    hit = `${accepted} of ${edges} edges accept ${OPERATOR_LABELS[operator]}`;
    censusCache.set(key, hit);
  }
  return hit;
}

const OPERATOR_SUBJECTS = subjectControl([1, 0, 2, 4], 0);

const operators = sceneOf({
  id: 'single-edge',
  title: 'One edge: split, collapse, flip',
  description:
    'SplitEdge, CollapseEdge and FlipEdge are the batched passes under a mask that selects one edge — the mutable-style API is a special case of the batched one, not a second code path. A collapse that fails the link condition and a flip whose new diagonal already exists both return the mesh unchanged, which is the correct answer rather than a failure.',
  plato: [
    'TriangleMesh3D.SplitEdge',
    'TriangleMesh3D.CollapseEdge',
    'TriangleMesh3D.FlipEdge',
    'TriangleMeshTopology.SingleEdgeMask',
    'TriangleMeshTopology.EdgeEndpoints',
    'TriangleMeshTopology.IsBoundaryEdge',
    'TriangleMesh3D.EdgeApex',
    'TriangleMesh3D.EdgeFarApex',
    'TriangleMesh3D.EdgeLength',
    'TriangleMesh3D.EdgeMidpoint',
    'TriangleMesh3D.VertexValences',
  ],
  viewer: { spin: false, distance: 4.6 },
  controls: [
    OPERATOR_SUBJECTS.control,
    { key: 'operator', label: 'Operator', kind: 'select', options: OPERATOR_LABELS, def: 1 },
    { key: 'edge', label: 'Edge index', kind: 'slider', min: 0, max: 60, step: 1, def: 0 },
    { key: 'census', label: 'Census every edge', kind: 'toggle', def: 1 },
    { key: 'before', label: 'Ghost the input', kind: 'toggle', def: 1 },
  ],
  build(params) {
    const index = OPERATOR_SUBJECTS.pick(params);
    const operator = clampIndex(params.operator, OPERATOR_LABELS.length);
    const base = subject(index);
    const topology = base.TopologyOf();
    const edgeCount = topology.EdgeCount();
    const edge = clamp(Math.round(params.edge ?? 0), 0, Math.max(0, edgeCount - 1));
    const edgeIndex = new UndirectedEdgeIndex(edge);
    const result = operatorResult(index, operator, edge);
    const valence = valenceStats(base, topology);

    const group = new THREE.Group();
    group.add(new THREE.Mesh(meshGeometry(result), surfaceMaterial()));
    group.add(new THREE.LineSegments(faceEdgeGeometry(result), edgeMaterial()));
    if ((params.before ?? 1) !== 0) {
      group.add(new THREE.LineSegments(topologyEdgeGeometry(base, topology), edgeMaterial(palette.line)));
    }
    // The chosen edge, drawn fat over the ghost, in the INPUT positions.
    const highlight = new THREE.LineSegments(
      topologyEdgeGeometry(base, topology, e => e === edge),
      new THREE.LineBasicMaterial({ color: palette.accent, depthTest: false }),
    );
    highlight.renderOrder = 2;
    group.add(highlight);

    const pair = topology.EdgeEndpoints(edgeIndex);
    const near = base.EdgeApex(topology, edgeIndex);
    const far = base.EdgeFarApex(topology, edgeIndex);
    const wasRefused = refused(base, result);

    return {
      object: group,
      readings: [
        note('subject', `${SUBJECTS[index].label} — ${base.Positions.Count()} v / ${base.Faces.Count()} f / ${edgeCount} e`),
        reading('edge', () =>
          `${edge}: vertices ${pair.A.Value}–${pair.B.Value}` +
          `, length ${n4(base.EdgeLength(topology, edgeIndex))}` +
          `, ${topology.IsBoundaryEdge(edgeIndex) ? 'boundary' : 'interior'}`,
        ),
        reading('EdgeMidpoint', () => {
          const m = base.EdgeMidpoint(topology, edgeIndex);
          return `(${n2(m.X)}, ${n2(m.Y)}, ${n2(m.Z)})`;
        }),
        reading('apexes', () =>
          `near ${near.Value}` + (far.IsNone() ? ', far none (boundary)' : `, far ${far.Value}`),
        ),
        reading('endpoint valences', () => `${valence.valences[pair.A.Value]}, ${valence.valences[pair.B.Value]}`),
        reading(OPERATOR_LABELS[operator], () =>
          wasRefused
            ? `REFUSED — mesh returned unchanged (${operator === 1 ? 'link condition' : 'boundary, or the diagonal already exists'})`
            : `${result.Positions.Count()} v / ${result.Faces.Count()} f` +
              ` (was ${base.Positions.Count()} v / ${base.Faces.Count()} f)`,
        ),
        (params.census ?? 1) !== 0
          ? reading('census', () => census(index, operator))
          : note('census', 'off'),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 7 — welding marching-cubes output into a mesh
//
// The consumer the marching-cubes track has been missing. `MarchingCubes`
// returns a `TriangleArray3D` — independent triangles, one copy of every shared
// vertex, no connectivity at all — and `Welded(TriangleArray3D, tolerance)`
// runs it through `ToTriangleMesh` and merges. Welding never MOVES geometry:
// the survivor is the lowest-numbered member of its group and keeps its own
// position, so at a tolerance below the cell size the result is the same
// surface with a third of the vertices, and above it the merge starts eating
// real detail and dropping the faces it degenerates.
// ---------------------------------------------------------------------------

const WELD_BOUNDS = new Bounds3D(new Point3D(-1.35, -1.35, -1.35), new Point3D(1.35, 1.35, 1.35));

const soupCache = new Map<number, TriangleArray3D>();

function marchedSphere(nodes: number): TriangleArray3D {
  let hit = soupCache.get(nodes);
  if (!hit) {
    hit = new Sphere(new Point3D(0, 0, 0), 1)
      .ToSdf()
      .MarchingCubes(WELD_BOUNDS, new IntegerVector3(nodes, nodes, nodes));
    soupCache.set(nodes, hit);
  }
  return hit;
}

const weldCache = new Map<string, TriangleMesh3D>();

function welded(nodes: number, tolerance: number): TriangleMesh3D {
  const key = `${nodes}:${tolerance}`;
  let hit = weldCache.get(key);
  if (!hit) {
    hit = (marchedSphere(nodes).Welded(tolerance));
    weldCache.set(key, hit);
  }
  return hit;
}

const weldScene = sceneOf({
  id: 'welding',
  title: 'Welding marching-cubes soup',
  description:
    'MarchingCubes returns independent triangles with one copy of every shared vertex and no connectivity. Welded merges vertices within a tolerance, keeping the lowest-numbered member of each group at its own position — so nothing moves — and drops the faces the merge degenerated. Below the cell size it recovers the mesh the isosurface always was; above it, it decimates.',
  plato: [
    'TriangleArray3D.Welded',
    'TriangleMesh3D.Welded',
    'TriangleArray3D.ToTriangleMesh',
    'FunctionSdf3D.MarchingCubes',
    'Sphere.ToSdf',
    'TriangleMesh3D.TopologyOf',
    'TriangleMesh3D.AverageEdgeLength',
  ],
  controls: [
    { key: 'nodes', label: 'Marching-cubes nodes', kind: 'slider', min: 6, max: 12, step: 1, def: 8 },
    { key: 'tolerance', label: 'Weld tolerance', kind: 'slider', min: 0.001, max: 0.6, step: 0.005, def: 0.001 },
    { key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 },
    { key: 'euler', label: 'Take V − E + F (slow)', kind: 'toggle', def: 0 },
  ],
  build(params) {
    const nodes = clamp(Math.round(params.nodes ?? 8), 6, 12);
    const tolerance = clamp(params.tolerance ?? 0.001, 0.001, 0.6);
    const soup = marchedSphere(nodes);
    const mesh = welded(nodes, tolerance);
    const soupTriangles = soup.Triangles.Count();
    const cell = 2.7 / nodes;

    const group = new THREE.Group();
    group.add(new THREE.Mesh(meshGeometry(mesh), surfaceMaterial()));
    if ((params.edges ?? 1) !== 0) {
      group.add(new THREE.LineSegments(faceEdgeGeometry(mesh), edgeMaterial()));
    }

    const readings: Reading[] = [
      note('marched', `${nodes}³ nodes, cell ${n3(cell)} — ${soupTriangles} triangles`),
      note('soup', `${soupTriangles * 3} vertices, no shared vertices at all`),
      note('tolerance', `${n3(tolerance)} — ${(100 * tolerance) / cell < 1 ? '<1' : ((100 * tolerance) / cell).toFixed(0)}% of a cell`),
      reading('TriangleArray3D.Welded', () =>
        `${mesh.Positions.Count()} v / ${mesh.Faces.Count()} f` +
        ` — ×${n2(mesh.Positions.Count() / (soupTriangles * 3))} of the soup's vertices` +
        `, ${soupTriangles - mesh.Faces.Count()} faces dropped as degenerate`,
      ),
      reading('AverageEdgeLength', () => n4(mesh.AverageEdgeLength())),
      reading('TriangleMesh3D.Welded (again, same tolerance)', () => {
        const twice = (mesh.Welded(tolerance));
        return `${twice.Positions.Count()} v / ${twice.Faces.Count()} f` +
          (twice.Positions.Count() === mesh.Positions.Count() ? ' — idempotent' : ' — merged further');
      }),
    ];
    if ((params.euler ?? 0) !== 0) {
      readings.push(
        reading('V − E + F', () => {
          const topology = mesh.TopologyOf();
          const edges = topology.EdgeCount();
          const v = mesh.Positions.Count();
          const f = mesh.Faces.Count();
          return `${v} − ${edges} + ${f} = ${v - edges + f}`;
        }),
      );
    } else {
      readings.push(note('V − E + F', 'off — TopologyOf on a mesh this size costs seconds here'));
    }

    return { object: group, readings };
  },
});

// ---------------------------------------------------------------------------

const demo: Demo = {
  title: 'Remeshing',
  subtitle: 'remeshing.{types,library}.plato · surfaces.types.plato',
  scenes: [loopVsButterfly, schemes, smoothing, isotropicScene, coarsening, operators, weldScene],
};

mountDemo(demo);

export { demo };
