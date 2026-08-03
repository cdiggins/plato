// Polyhedra — the Conway operator catalog of `polyhedra.library.plato`,
// standing on the corner numbering of `meshes-polygon.library.plato`.
//
// Every vertex table, every operator and every named solid below is a generated
// member: the five Platonic seeds are the only coordinate tables in the tree and
// each Archimedean / Catalan solid is a one-line Conway program over one of
// them. This file builds no geometry — it selects a member, reads the resulting
// `PolygonMesh3D`, and repacks the face loops into Three.js buffers.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { polygonMeshEdges, polygonMeshGeometry, toArray } from '../shared/mesh.js';
import { edgeMaterial, palette, surfaceMaterial } from '../shared/viewer.js';
import { Angle, PolygonMesh3D } from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Memoized construction
//
// The generated arrays are lazy, so a `Dual` of a `Bevel` re-runs its whole
// chain on every read. Building each distinct mesh once keeps a toggle instant;
// `PolygonMesh3D` is immutable, so `build` stays pure.

const meshCache = new Map<string, PolygonMesh3D>();

function memo(key: string, make: () => PolygonMesh3D): PolygonMesh3D {
  const hit = meshCache.get(key);
  if (hit) return hit;
  if (meshCache.size > 256) meshCache.clear();
  const value = make();
  meshCache.set(key, value);
  return value;
}

// ---------------------------------------------------------------------------
// The seeds and the operators

interface Seed {
  name: string;
  member: string;
  make(): PolygonMesh3D;
}

const SEEDS: Seed[] = [
  { name: 'Tetrahedron', member: 'PolygonMesh3D.Tetrahedron', make: () => PolygonMesh3D.Tetrahedron() },
  { name: 'Cube', member: 'PolygonMesh3D.Cube', make: () => PolygonMesh3D.Cube() },
  { name: 'Octahedron', member: 'PolygonMesh3D.Octahedron', make: () => PolygonMesh3D.Octahedron() },
  { name: 'Dodecahedron', member: 'PolygonMesh3D.Dodecahedron', make: () => PolygonMesh3D.Dodecahedron() },
  { name: 'Icosahedron', member: 'PolygonMesh3D.Icosahedron', make: () => PolygonMesh3D.Icosahedron() },
];

const SEED_NAMES = SEEDS.map(s => s.name);
const SEED_MEMBERS = SEEDS.map(s => s.member);

function seedOf(params: Params): Seed {
  const index = Math.round(params.seed ?? 0);
  return SEEDS[Math.min(Math.max(index, 0), SEEDS.length - 1)] ?? SEEDS[1];
}

function seedMesh(seed: Seed): PolygonMesh3D {
  return memo(seed.name, () => seed.make());
}

/** The uniform-snub constants recorded at the library's own call sites. */
const SNUB_SHRINK = 0.437593286000961;
const SNUB_TWIST = 0.287413148757778;

// ---------------------------------------------------------------------------
// Reading a mesh

interface Topology {
  vertices: number;
  edges: number;
  faces: number;
  /** Face counts by arity, ascending: `[[3, 8], [4, 6]]`. */
  profile: [number, number][];
}

function topologyOf(mesh: PolygonMesh3D): Topology {
  const byArity = new Map<number, number>();
  for (let f = 0; f < mesh.FaceCount(); f++) {
    const arity = mesh.FaceArity(f);
    byArity.set(arity, (byArity.get(arity) ?? 0) + 1);
  }
  return {
    vertices: mesh.VertexCount(),
    edges: mesh.UndirectedEdgeCount(),
    faces: mesh.FaceCount(),
    profile: [...byArity.entries()].sort((a, b) => a[0] - b[0]),
  };
}

function countLine(t: Topology): string {
  return `V ${t.vertices}  E ${t.edges}  F ${t.faces}  X ${t.vertices - t.edges + t.faces}`;
}

/** `faces 8x3 6x4` — eight triangles and six quadrilaterals. */
function profileLine(t: Topology): string {
  return 'faces ' + t.profile.map(([arity, count]) => `${count}x${arity}`).join(' ');
}

/** Vertex radii, for the vertex-transitivity check the library's note turns on. */
function radiusRange(mesh: PolygonMesh3D): [number, number] {
  const radii = toArray(mesh.Positions).map(p => Math.hypot(p.X, p.Y, p.Z));
  return [Math.min(...radii), Math.max(...radii)];
}

/**
 * Worst distance from a face's own vertices to that face's plane, using the
 * generated `FaceUnitNormal` / `FaceCentroid`. Zero to rounding for every solid
 * the library emits; non-zero is exactly the damage the polyhedra note warns
 * `ProjectedToUnitSphere` does to a face-transitive Catalan.
 */
function worstPlaneDeviation(mesh: PolygonMesh3D): number {
  let worst = 0;
  for (let f = 0; f < mesh.FaceCount(); f++) {
    const normal = mesh.FaceUnitNormal(f);
    const centroid = mesh.FaceCentroid(f);
    for (const p of toArray(mesh.FacePositions(f))) {
      const d = (p.X - centroid.X) * normal.X + (p.Y - centroid.Y) * normal.Y + (p.Z - centroid.Z) * normal.Z;
      worst = Math.max(worst, Math.abs(d));
    }
  }
  return worst;
}

// ---------------------------------------------------------------------------
// Three.js repacking

function solid(mesh: PolygonMesh3D, showEdges: boolean, color = palette.surface): THREE.Group {
  const group = new THREE.Group();
  group.add(new THREE.Mesh(polygonMeshGeometry(mesh), surfaceMaterial(color)));
  if (showEdges) group.add(new THREE.LineSegments(polygonMeshEdges(mesh), edgeMaterial()));
  return group;
}

/** The seed drawn as bare edges over the result, so the reciprocal pairing of
 *  `Dual` (and the shrink of `Expand` / `Snub`) stays visible from any angle. */
function ghost(mesh: PolygonMesh3D): THREE.LineSegments {
  return new THREE.LineSegments(
    polygonMeshEdges(mesh),
    new THREE.LineBasicMaterial({
      color: palette.line,
      transparent: true,
      opacity: 0.5,
      depthTest: false,
      depthWrite: false,
    }),
  );
}

interface LabelArt {
  texture: THREE.Texture;
  aspect: number;
  lines: number;
}

const labelArt = new Map<string, LabelArt>();

/** Multi-line billboard text, clamped to `maxWidth` world units so a long line
 *  cannot run off a narrow stage. Textures are cached: the viewer disposes the
 *  sprite's material on every rebuild, and a material never owns its map. */
function label(text: string, lineHeight = 0.13, maxWidth = 1.9): THREE.Sprite {
  let art = labelArt.get(text);
  if (!art) {
    const lines = text.split('\n');
    const size = 44;
    const pad = 18;
    const face = `600 ${size}px ui-sans-serif, system-ui, -apple-system, sans-serif`;
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d')!;
    ctx.font = face;
    const width = Math.ceil(Math.max(...lines.map(l => ctx.measureText(l).width))) + pad * 2;
    const height = Math.ceil(lines.length * size * 1.3) + pad * 2;
    canvas.width = width;
    canvas.height = height;
    ctx.font = face;
    ctx.textBaseline = 'top';
    ctx.lineJoin = 'round';
    ctx.lineWidth = 7;
    ctx.strokeStyle = 'rgba(8, 10, 14, 0.9)';
    ctx.fillStyle = '#dfe8f6';
    lines.forEach((line, i) => {
      const y = pad + i * size * 1.3;
      ctx.strokeText(line, pad, y);
      ctx.fillText(line, pad, y);
    });
    const texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    art = { texture, aspect: width / height, lines: lines.length };
    labelArt.set(text, art);
  }
  const sprite = new THREE.Sprite(
    new THREE.SpriteMaterial({ map: art.texture, transparent: true, depthTest: false, depthWrite: false }),
  );
  let h = lineHeight * art.lines;
  let w = h * art.aspect;
  if (w > maxWidth) {
    h *= maxWidth / w;
    w = maxWidth;
  }
  sprite.scale.set(w, h, 1);
  return sprite;
}

/** Fit a group into the shared camera framing, using the generated radius. */
function fitTo(group: THREE.Object3D, radius: number, target = 1.15): void {
  const scale = radius > 0 ? target / radius : 1;
  group.scale.setScalar(scale);
}

const EDGE_CONTROL: Control = { key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 };
const SEED_CONTROL: Control = { key: 'seed', label: 'Seed', kind: 'select', options: SEED_NAMES, def: 1 };
const GHOST_CONTROL: Control = { key: 'ghost', label: 'Show seed', kind: 'toggle', def: 0 };
const INFO_CONTROL: Control = { key: 'info', label: 'Show counts', kind: 'toggle', def: 1 };

// ---------------------------------------------------------------------------
// Scene builders

interface OperatorSpec {
  id: string;
  title: string;
  description: string;
  plato: string[];
  /** Extra sliders, inserted between the seed selector and the toggles. */
  extras?: Control[];
  seedDefault?: number;
  ghostDefault?: number;
  /** A stable key for the memo, including whatever `extras` feed in. */
  key(seed: Seed, params: Params): string;
  apply(mesh: PolygonMesh3D, params: Params): PolygonMesh3D;
  /** The line under the solid's name, e.g. `Cube.Ambo`. */
  program(seed: Seed): string;
}

function operatorScene(spec: OperatorSpec): Scene {
  return {
    id: spec.id,
    title: spec.title,
    description: spec.description,
    plato: spec.plato,
    controls: [
      { ...SEED_CONTROL, def: spec.seedDefault ?? 1 },
      ...(spec.extras ?? []),
      EDGE_CONTROL,
      { ...GHOST_CONTROL, def: spec.ghostDefault ?? 0 },
      INFO_CONTROL,
    ],
    build(params) {
      const seed = seedOf(params);
      const source = seedMesh(seed);
      const result = memo(spec.key(seed, params), () => spec.apply(source, params));

      const group = new THREE.Group();
      group.add(solid(result, params.edges !== 0));
      if (params.ghost !== 0) group.add(ghost(source));

      const radius = Math.max(
        result.CircumscribedRadius(),
        params.ghost !== 0 ? source.CircumscribedRadius() : 0,
      );
      fitTo(group, radius);

      const root = new THREE.Group();
      root.add(group);
      if (params.info !== 0) {
        const t = topologyOf(result);
        const sprite = label(`${spec.program(seed)}\n${countLine(t)}\n${profileLine(t)}`);
        sprite.position.set(0, 1.55, 0);
        root.add(sprite);
      }
      return root;
    },
  };
}

// ---------------------------------------------------------------------------
// 1. The five seeds

const seedScene: Scene = {
  id: 'platonic-seeds',
  title: 'Platonic seeds',
  description:
    'The five regular convex polyhedra, each an explicit vertex table and face table pushed onto the unit sphere by ProjectedToUnitSphere. They are the only coordinate tables in polyhedra.library.plato — every other solid here is a Conway program over one of them.',
  plato: [...SEED_MEMBERS, 'PolygonMesh3D.ProjectedToUnitSphere', 'PolygonMesh3D.FaceArity'],
  controls: [{ ...SEED_CONTROL, def: 4 }, EDGE_CONTROL, INFO_CONTROL],
  build(params) {
    const seed = seedOf(params);
    const mesh = seedMesh(seed);
    const root = new THREE.Group();
    const group = solid(mesh, params.edges !== 0);
    fitTo(group, mesh.CircumscribedRadius());
    root.add(group);
    if (params.info !== 0) {
      const t = topologyOf(mesh);
      const sprite = label(`${seed.name}\n${countLine(t)}\n${profileLine(t)}`);
      sprite.position.set(0, 1.55, 0);
      root.add(sprite);
    }
    return root;
  },
};

// ---------------------------------------------------------------------------
// 2-7. One scene per Conway operator

const amboScene = operatorScene({
  id: 'ambo',
  title: 'Ambo (rectify)',
  description:
    'Ambo cuts every vertex back to the midpoints of its edges: one new vertex per undirected edge, one face per old face and one per old vertex. Cube gives the cuboctahedron, dodecahedron the icosidodecahedron.',
  plato: [
    'PolygonMesh3D.Ambo',
    'PolygonMesh3D.UndirectedEdgeMidpoints',
    'PolygonMesh3D.AmboFaceOfFace',
    'PolygonMesh3D.AmboFaceOfVertex',
    ...SEED_MEMBERS,
  ],
  key: seed => `${seed.name}.Ambo`,
  apply: mesh => mesh.Ambo(),
  program: seed => `${seed.name}.Ambo`,
});

const truncateScene = operatorScene({
  id: 'truncate',
  title: 'Truncate',
  description:
    'Truncate cuts each vertex off a third of the way along its edges, so every n-gon becomes a 2n-gon and every vertex becomes a face of its own degree. The corner numbering of meshes-polygon.library.plato IS the truncated mesh’s vertex numbering, so no lookup table is built.',
  plato: [
    'PolygonMesh3D.Truncate',
    'PolygonMesh3D.TruncationPoints',
    'PolygonMesh3D.TruncateFaceOfFace',
    'PolygonMesh3D.TruncateFaceOfVertex',
    ...SEED_MEMBERS,
  ],
  seedDefault: 4,
  key: seed => `${seed.name}.Truncate`,
  apply: mesh => mesh.Truncate(),
  program: seed => `${seed.name}.Truncate`,
});

const dualScene = operatorScene({
  id: 'dual',
  title: 'Dual',
  description:
    'Dual places one vertex per face by polar reciprocation about the unit sphere and one face per vertex, so V and F swap while E is unchanged. Reciprocation (not the face centroid) is what keeps every dual face planar. Turn on "Show seed" to see the reciprocal pair.',
  plato: [
    'PolygonMesh3D.Dual',
    'PolygonMesh3D.PolarVertices',
    'PolygonMesh3D.DualFaceOfVertex',
    'PolygonMesh3D.FaceUnitNormal',
    ...SEED_MEMBERS,
  ],
  ghostDefault: 1,
  key: seed => `${seed.name}.Dual`,
  apply: mesh => mesh.Dual(),
  program: seed => `${seed.name}.Dual`,
});

const expandScene = operatorScene({
  id: 'expand',
  title: 'Expand (cantellate)',
  description:
    'Expand shrinks every face toward its own centroid and stitches a quad into each pulled-apart edge, adding an n-gon per degree-n vertex. Cube at shrink = sqrt(2) - 1 is the rhombicuboctahedron; the shrink is a free parameter, and only that value makes all edges equal.',
  plato: [
    'PolygonMesh3D.Expand',
    'PolygonMesh3D.TwistedFacePoints',
    'PolygonMesh3D.ExpandFaceOfEdge',
    'PolygonMesh3D.ExpansionFaces',
    'Number.SqrtTwo',
    ...SEED_MEMBERS,
  ],
  extras: [
    { key: 'shrink', label: 'Shrink', kind: 'slider', min: 0.05, max: 1, step: 0.001, def: Number.SqrtTwo() - 1 },
  ],
  key: (seed, params) => `${seed.name}.Expand(${params.shrink})`,
  apply: (mesh, params) => mesh.Expand(params.shrink),
  program: seed => `${seed.name}.Expand(shrink)`,
});

const snubScene = operatorScene({
  id: 'snub',
  title: 'Snub',
  description:
    'Snub is expand with a twist: each face is rotated about its own normal as it shrinks, and every edge-quad splits into two triangles. It is chiral — the twist sign picks the enantiomer. Cube at shrink 0.4376 / twist 0.2874 rad is the snub cube.',
  plato: [
    'PolygonMesh3D.Snub',
    'PolygonMesh3D.TwistedCornerPoint',
    'PolygonMesh3D.SnubFacesOfEdge',
    'PolygonMesh3D.ExpansionFaces',
    ...SEED_MEMBERS,
  ],
  extras: [
    { key: 'shrink', label: 'Shrink', kind: 'slider', min: 0.05, max: 1, step: 0.001, def: SNUB_SHRINK },
    { key: 'twist', label: 'Twist (rad)', kind: 'slider', min: -0.7, max: 0.7, step: 0.001, def: SNUB_TWIST },
  ],
  key: (seed, params) => `${seed.name}.Snub(${params.shrink},${params.twist})`,
  apply: (mesh, params) => mesh.Snub(params.shrink, new Angle(params.twist)),
  program: seed => `${seed.name}.Snub(shrink, twist)`,
});

const bevelScene = operatorScene({
  id: 'bevel',
  title: 'Bevel (omnitruncate)',
  description:
    'Bevel is the Conway identity b = ta — truncate of ambo, nothing new. Cube gives the truncated cuboctahedron. Topology and planarity are exact; the vertex faces come out as rectangles rather than squares, which literal truncation cannot avoid.',
  plato: ['PolygonMesh3D.Bevel', 'PolygonMesh3D.Ambo', 'PolygonMesh3D.Truncate', ...SEED_MEMBERS],
  key: seed => `${seed.name}.Bevel`,
  apply: mesh => mesh.Bevel(),
  program: seed => `${seed.name}.Bevel  =  ${seed.name}.Ambo.Truncate`,
});

// ---------------------------------------------------------------------------
// 8-9. The named catalog

interface NamedSolid {
  name: string;
  member: string;
  program: string;
  make(): PolygonMesh3D;
}

const ARCHIMEDEAN: NamedSolid[] = [
  { name: 'Truncated tetrahedron', member: 'PolygonMesh3D.TruncatedTetrahedron', program: 'Tetrahedron.Truncate', make: () => PolygonMesh3D.TruncatedTetrahedron() },
  { name: 'Cuboctahedron', member: 'PolygonMesh3D.Cuboctahedron', program: 'Cube.Ambo', make: () => PolygonMesh3D.Cuboctahedron() },
  { name: 'Truncated cube', member: 'PolygonMesh3D.TruncatedCube', program: 'Cube.Truncate', make: () => PolygonMesh3D.TruncatedCube() },
  { name: 'Truncated octahedron', member: 'PolygonMesh3D.TruncatedOctahedron', program: 'Octahedron.Truncate', make: () => PolygonMesh3D.TruncatedOctahedron() },
  { name: 'Rhombicuboctahedron', member: 'PolygonMesh3D.Rhombicuboctahedron', program: 'Cube.Expand(sqrt(2) - 1)', make: () => PolygonMesh3D.Rhombicuboctahedron() },
  { name: 'Truncated cuboctahedron', member: 'PolygonMesh3D.TruncatedCuboctahedron', program: 'Cube.Bevel', make: () => PolygonMesh3D.TruncatedCuboctahedron() },
  { name: 'Snub cube', member: 'PolygonMesh3D.SnubCube', program: 'Cube.Snub(0.4376, 0.2874 rad)', make: () => PolygonMesh3D.SnubCube() },
  { name: 'Icosidodecahedron', member: 'PolygonMesh3D.Icosidodecahedron', program: 'Dodecahedron.Ambo', make: () => PolygonMesh3D.Icosidodecahedron() },
  { name: 'Truncated dodecahedron', member: 'PolygonMesh3D.TruncatedDodecahedron', program: 'Dodecahedron.Truncate', make: () => PolygonMesh3D.TruncatedDodecahedron() },
  { name: 'Truncated icosahedron', member: 'PolygonMesh3D.TruncatedIcosahedron', program: 'Icosahedron.Truncate', make: () => PolygonMesh3D.TruncatedIcosahedron() },
  { name: 'Rhombicosidodecahedron', member: 'PolygonMesh3D.Rhombicosidodecahedron', program: 'Dodecahedron.Expand(phi / 3)', make: () => PolygonMesh3D.Rhombicosidodecahedron() },
  { name: 'Truncated icosidodecahedron', member: 'PolygonMesh3D.TruncatedIcosidodecahedron', program: 'Dodecahedron.Bevel', make: () => PolygonMesh3D.TruncatedIcosidodecahedron() },
  { name: 'Snub dodecahedron', member: 'PolygonMesh3D.SnubDodecahedron', program: 'Dodecahedron.Snub(0.5621, 0.2287 rad)', make: () => PolygonMesh3D.SnubDodecahedron() },
];

const CATALAN: NamedSolid[] = [
  { name: 'Triakis tetrahedron', member: 'PolygonMesh3D.TriakisTetrahedron', program: 'TruncatedTetrahedron.Dual', make: () => PolygonMesh3D.TriakisTetrahedron() },
  { name: 'Rhombic dodecahedron', member: 'PolygonMesh3D.RhombicDodecahedron', program: 'Cuboctahedron.Dual', make: () => PolygonMesh3D.RhombicDodecahedron() },
  { name: 'Triakis octahedron', member: 'PolygonMesh3D.TriakisOctahedron', program: 'TruncatedCube.Dual', make: () => PolygonMesh3D.TriakisOctahedron() },
  { name: 'Tetrakis hexahedron', member: 'PolygonMesh3D.TetrakisHexahedron', program: 'TruncatedOctahedron.Dual', make: () => PolygonMesh3D.TetrakisHexahedron() },
  { name: 'Deltoidal icositetrahedron', member: 'PolygonMesh3D.DeltoidalIcositetrahedron', program: 'Rhombicuboctahedron.Dual', make: () => PolygonMesh3D.DeltoidalIcositetrahedron() },
  { name: 'Disdyakis dodecahedron', member: 'PolygonMesh3D.DisdyakisDodecahedron', program: 'TruncatedCuboctahedron.Dual', make: () => PolygonMesh3D.DisdyakisDodecahedron() },
  { name: 'Pentagonal icositetrahedron', member: 'PolygonMesh3D.PentagonalIcositetrahedron', program: 'SnubCube.Dual', make: () => PolygonMesh3D.PentagonalIcositetrahedron() },
  { name: 'Rhombic triacontahedron', member: 'PolygonMesh3D.RhombicTriacontahedron', program: 'Icosidodecahedron.Dual', make: () => PolygonMesh3D.RhombicTriacontahedron() },
  { name: 'Triakis icosahedron', member: 'PolygonMesh3D.TriakisIcosahedron', program: 'TruncatedDodecahedron.Dual', make: () => PolygonMesh3D.TriakisIcosahedron() },
  { name: 'Pentakis dodecahedron', member: 'PolygonMesh3D.PentakisDodecahedron', program: 'TruncatedIcosahedron.Dual', make: () => PolygonMesh3D.PentakisDodecahedron() },
  { name: 'Deltoidal hexecontahedron', member: 'PolygonMesh3D.DeltoidalHexecontahedron', program: 'Rhombicosidodecahedron.Dual', make: () => PolygonMesh3D.DeltoidalHexecontahedron() },
  { name: 'Disdyakis triacontahedron', member: 'PolygonMesh3D.DisdyakisTriacontahedron', program: 'TruncatedIcosidodecahedron.Dual', make: () => PolygonMesh3D.DisdyakisTriacontahedron() },
  { name: 'Pentagonal hexecontahedron', member: 'PolygonMesh3D.PentagonalHexecontahedron', program: 'SnubDodecahedron.Dual', make: () => PolygonMesh3D.PentagonalHexecontahedron() },
];

function catalogScene(
  id: string,
  title: string,
  description: string,
  solids: NamedSolid[],
  color: number,
  def = 0,
): Scene {
  return {
    id,
    title,
    description,
    plato: [...solids.map(s => s.member), 'PolygonMesh3D.ScaledToUnitCircumradius'],
    controls: [
      { key: 'solid', label: 'Solid', kind: 'select', options: solids.map(s => s.name), def },
      EDGE_CONTROL,
      INFO_CONTROL,
    ],
    build(params) {
      const index = Math.min(Math.max(Math.round(params.solid ?? 0), 0), solids.length - 1);
      const entry = solids[index];
      // ScaledToUnitCircumradius is the library's own framing member; a Catalan
      // may be scaled (a similarity) even though it may not be projected.
      const mesh = memo(`${entry.member}#unit`, () => entry.make().ScaledToUnitCircumradius());
      const root = new THREE.Group();
      const group = solid(mesh, params.edges !== 0, color);
      group.scale.setScalar(1.15);
      root.add(group);
      if (params.info !== 0) {
        const t = topologyOf(mesh);
        const sprite = label(`${entry.name}\n${entry.program}\n${countLine(t)}\n${profileLine(t)}`, 0.12);
        sprite.position.set(0, 1.55, 0);
        root.add(sprite);
      }
      return root;
    },
  };
}

const archimedeanScene = catalogScene(
  'archimedean',
  'Archimedean solids',
  'All thirteen Archimedean solids, each a one-line Conway program over a Platonic seed rather than another coordinate table. They are vertex-transitive — every vertex sits at the same radius — which is what makes ProjectedToUnitSphere meaning-preserving for them.',
  ARCHIMEDEAN,
  palette.surface,
  9,
);

const catalanScene = catalogScene(
  'catalan',
  'Catalan solids',
  'All thirteen Catalan solids, each simply the Conway Dual of its Archimedean partner. They are face-transitive, so their vertices genuinely sit at two or three different radii; the duals of the two snubs inherit their source’s chirality.',
  CATALAN,
  palette.surfaceAlt,
  1,
);

// ---------------------------------------------------------------------------
// 10. What the operators do to V, E and F

const OPERATORS: { name: string; suffix: string; apply(m: PolygonMesh3D): PolygonMesh3D }[] = [
  { name: 'none', suffix: '', apply: m => m },
  { name: 'Ambo', suffix: '.Ambo', apply: m => m.Ambo() },
  { name: 'Truncate', suffix: '.Truncate', apply: m => m.Truncate() },
  { name: 'Dual', suffix: '.Dual', apply: m => m.Dual() },
  { name: 'Expand', suffix: '.Expand', apply: m => m.Expand(Number.SqrtTwo() - 1) },
  { name: 'Snub', suffix: '.Snub', apply: m => m.Snub(SNUB_SHRINK, new Angle(SNUB_TWIST)) },
  { name: 'Bevel', suffix: '.Bevel', apply: m => m.Bevel() },
];

const topologyScene: Scene = {
  id: 'topology',
  title: 'Operator topology',
  description:
    'One Conway operator applied to all five seeds at once, with the counts read straight off the generated FaceCount / VertexCount / UndirectedEdgeCount. Dual swaps V and F and leaves E alone; ambo puts one vertex on every edge; truncate one on every corner; expand adds a quad per edge and snub two triangles. V - E + F stays 2 throughout.',
  plato: [
    'PolygonMesh3D.VertexCount',
    'PolygonMesh3D.UndirectedEdgeCount',
    'PolygonMesh3D.FaceCount',
    'PolygonMesh3D.FaceArity',
    'PolygonMesh3D.Ambo',
    'PolygonMesh3D.Truncate',
    'PolygonMesh3D.Dual',
    'PolygonMesh3D.Expand',
    'PolygonMesh3D.Snub',
    'PolygonMesh3D.Bevel',
    'PolygonMesh3D.ScaledToUnitCircumradius',
    ...SEED_MEMBERS,
  ],
  controls: [
    { key: 'op', label: 'Operator', kind: 'select', options: OPERATORS.map(o => o.name), def: 1 },
    EDGE_CONTROL,
  ],
  build(params) {
    const op = OPERATORS[Math.min(Math.max(Math.round(params.op ?? 0), 0), OPERATORS.length - 1)];
    const root = new THREE.Group();
    const ring = 1.3;
    SEEDS.forEach((seed, i) => {
      const mesh = memo(`${seed.name}${op.suffix}#unit`, () =>
        op.apply(seedMesh(seed)).ScaledToUnitCircumradius(),
      );
      const angle = (i / SEEDS.length) * Math.PI * 2;
      const x = Math.sin(angle) * ring;
      const z = Math.cos(angle) * ring;

      const group = solid(mesh, params.edges !== 0, i % 2 === 0 ? palette.surface : palette.surfaceAlt);
      group.scale.setScalar(0.4);
      group.position.set(x, 0, z);
      root.add(group);

      const t = topologyOf(mesh);
      const sprite = label(`${seed.name}${op.suffix}\n${countLine(t)}\n${profileLine(t)}`, 0.085, 0.95);
      sprite.position.set(x, 0.7, z);
      root.add(sprite);
    });
    return root;
  },
};

// ---------------------------------------------------------------------------
// 11. The Catalan warning, checked on screen

interface Subject {
  name: string;
  family: 'Archimedean' | 'Catalan';
  member: string;
  make(): PolygonMesh3D;
}

const SUBJECTS: Subject[] = [
  { name: 'Cuboctahedron', family: 'Archimedean', member: 'PolygonMesh3D.Cuboctahedron', make: () => PolygonMesh3D.Cuboctahedron() },
  { name: 'Truncated icosahedron', family: 'Archimedean', member: 'PolygonMesh3D.TruncatedIcosahedron', make: () => PolygonMesh3D.TruncatedIcosahedron() },
  { name: 'Rhombicuboctahedron', family: 'Archimedean', member: 'PolygonMesh3D.Rhombicuboctahedron', make: () => PolygonMesh3D.Rhombicuboctahedron() },
  { name: 'Rhombic dodecahedron', family: 'Catalan', member: 'PolygonMesh3D.RhombicDodecahedron', make: () => PolygonMesh3D.RhombicDodecahedron() },
  { name: 'Rhombic triacontahedron', family: 'Catalan', member: 'PolygonMesh3D.RhombicTriacontahedron', make: () => PolygonMesh3D.RhombicTriacontahedron() },
  { name: 'Deltoidal icositetrahedron', family: 'Catalan', member: 'PolygonMesh3D.DeltoidalIcositetrahedron', make: () => PolygonMesh3D.DeltoidalIcositetrahedron() },
];

const projectionScene: Scene = {
  id: 'unit-sphere',
  title: 'Unit-sphere projection',
  description:
    'The polyhedra library warns that no Conway operator normalizes its result: pushing vertices onto a common radius is only meaning-preserving for a vertex-transitive solid. Toggle the projection and read the label — an Archimedean stays planar to rounding, a face-transitive Catalan collapses two radii into one and bends every face out of plane.',
  plato: [
    'PolygonMesh3D.ProjectedToUnitSphere',
    'PolygonMesh3D.ScaledToUnitCircumradius',
    'PolygonMesh3D.CircumscribedRadius',
    'PolygonMesh3D.FaceUnitNormal',
    'PolygonMesh3D.FaceCentroid',
    ...SUBJECTS.map(s => s.member),
  ],
  controls: [
    { key: 'solid', label: 'Solid', kind: 'select', options: SUBJECTS.map(s => `${s.name} (${s.family[0]})`), def: 3 },
    { key: 'project', label: 'ProjectedToUnitSphere', kind: 'toggle', def: 0 },
    EDGE_CONTROL,
  ],
  build(params) {
    const index = Math.min(Math.max(Math.round(params.solid ?? 0), 0), SUBJECTS.length - 1);
    const subject = SUBJECTS[index];
    const project = params.project !== 0;
    const mesh = memo(`${subject.member}#${project ? 'projected' : 'plain'}`, () => {
      const raw = subject.make();
      return (project ? raw.ProjectedToUnitSphere() : raw).ScaledToUnitCircumradius();
    });

    const [rmin, rmax] = radiusRange(mesh);
    const deviation = worstPlaneDeviation(mesh);
    const color = subject.family === 'Archimedean' ? palette.surface : palette.surfaceAlt;

    const root = new THREE.Group();
    const group = solid(mesh, params.edges !== 0, color);
    group.scale.setScalar(1.15);
    root.add(group);

    const sprite = label(
      [
        `${subject.name} — ${subject.family}`,
        project ? 'ProjectedToUnitSphere: on' : 'ProjectedToUnitSphere: off',
        `vertex radii ${rmin.toFixed(4)} .. ${rmax.toFixed(4)}`,
        `worst out-of-plane ${deviation.toExponential(2)}`,
      ].join('\n'),
      0.12,
    );
    sprite.position.set(0, 1.5, 0);
    root.add(sprite);
    return root;
  },
};

// ---------------------------------------------------------------------------

const demo: Demo = {
  title: 'Polyhedra',
  subtitle: 'polyhedra.library.plato · meshes-polygon.library.plato',
  scenes: [
    seedScene,
    amboScene,
    truncateScene,
    dualScene,
    expandScene,
    snubScene,
    bevelScene,
    archimedeanScene,
    catalanScene,
    topologyScene,
    projectionScene,
  ],
};

mountDemo(demo, { distance: 4.6 });

// The page never imports this; it exists so `npm run scenes` can call every
// scene's `build` without a WebGL context.
export { demo };
