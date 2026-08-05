// Polyhedron studio — the full Conway operator catalog of
// `polyhedra.library.plato` plus the demo-side vocabulary of
// `demos/webgl/plato-src/polyhedron-studio.plato`.
//
// Every solid, operator, reading and face COLOUR here is a generated member:
// the page picks indexes, reads the resulting `PolygonMesh3D` and the
// `FaceAspectColors` array beside it, and repacks both into Three.js buffers.
// The colour ramp itself (`ColorGradient.StudioRamp`) is Plato source.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { polygonMeshEdges, polygonMeshGeometry, toArray } from '../shared/mesh.js';
import { edgeMaterial, palette, surfaceMaterial } from '../shared/viewer.js';
import { Angle, PolygonMesh3D } from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Memoized construction — the generated arrays are lazy, so a chain re-runs on
// every read; building each distinct mesh once keeps the sliders instant.

const meshCache = new Map<string, PolygonMesh3D>();

function memo(key: string, make: () => PolygonMesh3D): PolygonMesh3D {
  const hit = meshCache.get(key);
  if (hit) return hit;
  if (meshCache.size > 192) meshCache.clear();
  const value = make();
  meshCache.set(key, value);
  return value;
}

// ---------------------------------------------------------------------------
// The seeds and the operator catalog.
//
// Operator order and indexes mirror `ApplyConwayOperator` in
// polyhedron-studio.plato — the dispatch is Plato's, this table only names it.

interface Seed {
  name: string;
  make(): PolygonMesh3D;
}

const SEEDS: Seed[] = [
  { name: 'Tetra', make: () => PolygonMesh3D.Tetrahedron() },
  { name: 'Cube', make: () => PolygonMesh3D.Cube() },
  { name: 'Octa', make: () => PolygonMesh3D.Octahedron() },
  { name: 'Dodeca', make: () => PolygonMesh3D.Dodecahedron() },
  { name: 'Icosa', make: () => PolygonMesh3D.Icosahedron() },
];

/** UI label per `ApplyConwayOperator` index; order is the Plato dispatch's. */
const OPERATORS = [
  'none', 'dual', 'ambo', 'truncate', 'kis', 'join', 'needle', 'zip',
  'ortho', 'meta', 'gyro', 'expand', 'snub', 'chamfer', 'propeller', 'bevel',
  'reflect',
] as const;

/** The amount each operator wants when the page picks one for the user:
 *  apex height for kis/join/needle, face shrink for expand/snub/chamfer. */
const DEFAULT_AMOUNT: Record<string, number> = {
  kis: 0.25, join: 0, needle: 0, expand: 0.414, snub: 0.4376, chamfer: 0.55,
};

const SNUB_TWIST = 0.2874;

function seedOf(params: Params, key = 'seed'): Seed {
  const index = Math.round(params[key] ?? 0);
  return SEEDS[Math.min(Math.max(index, 0), SEEDS.length - 1)] ?? SEEDS[1];
}

function seedMesh(seed: Seed): PolygonMesh3D {
  return memo(seed.name, () => seed.make());
}

/** Apply operator `op` through the generated dispatch, at the page defaults. */
function applyDefault(mesh: PolygonMesh3D, op: number): PolygonMesh3D {
  const amount = DEFAULT_AMOUNT[OPERATORS[op]] ?? 0;
  return mesh.ApplyConwayOperator(op, amount, new Angle(SNUB_TWIST));
}

// ---------------------------------------------------------------------------
// Reading a mesh — every number below is a generated member.

function countLine(mesh: PolygonMesh3D): string {
  return `V ${mesh.VertexCount()}  E ${mesh.UndirectedEdgeCount()}  F ${mesh.FaceCount()}  X ${mesh.EulerCharacteristic()}`;
}

function profileLine(mesh: PolygonMesh3D): string {
  const byArity = new Map<number, number>();
  for (let f = 0; f < mesh.FaceCount(); f++) {
    const arity = mesh.FaceArity(f);
    byArity.set(arity, (byArity.get(arity) ?? 0) + 1);
  }
  return 'faces ' + [...byArity.entries()].sort((a, b) => a[0] - b[0])
    .map(([arity, count]) => `${count}x${arity}`).join(' ');
}

/** Sphericity, edge spread and planarity, formatted for a status line. */
function readingsLine(mesh: PolygonMesh3D): string {
  const sphericity = mesh.Sphericity();
  const ratio = mesh.EdgeLengthRatio();
  const planarity = mesh.PlanarityDeviation();
  return `sphericity ${sphericity.toFixed(4)}  edge ratio ${ratio.toFixed(4)}  out-of-plane ${planarity.toExponential(1)}`;
}

// ---------------------------------------------------------------------------
// Three.js repacking

const ASPECTS = ['plain', 'sides', 'area', 'radius', 'flatness'] as const;

/**
 * Face loops as flat-shaded triangles with one colour per face, the colour
 * array coming from the generated `FaceAspectColors`. Plato's `Color` is
 * linear-light, which is exactly what a Three.js vertex-colour attribute wants.
 */
function coloredMeshGeometry(mesh: PolygonMesh3D, aspect: number): THREE.BufferGeometry {
  const faceColors = toArray(mesh.FaceAspectColors(aspect - 1));
  const positions: number[] = [];
  const colors: number[] = [];
  for (let f = 0; f < mesh.FaceCount(); f++) {
    const loop = toArray(mesh.FacePositions(f));
    const c = faceColors[f];
    for (let i = 1; i + 1 < loop.length; i++) {
      for (const p of [loop[0], loop[i], loop[i + 1]]) {
        positions.push(p.X, p.Y, p.Z);
        colors.push(c.R, c.G, c.B);
      }
    }
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  geometry.setAttribute('color', new THREE.Float32BufferAttribute(colors, 3));
  geometry.computeVertexNormals();
  return geometry;
}

function coloredMaterial(): THREE.MeshStandardMaterial {
  return new THREE.MeshStandardMaterial({
    vertexColors: true,
    roughness: 0.42,
    metalness: 0.08,
    flatShading: true,
    side: THREE.DoubleSide,
  });
}

/** The solid, coloured by aspect when one is selected, with optional edges. */
function solid(mesh: PolygonMesh3D, aspect: number, showEdges: boolean, color = palette.surface): THREE.Group {
  const group = new THREE.Group();
  group.add(
    aspect > 0
      ? new THREE.Mesh(coloredMeshGeometry(mesh, aspect), coloredMaterial())
      : new THREE.Mesh(polygonMeshGeometry(mesh), surfaceMaterial(color)),
  );
  if (showEdges) group.add(new THREE.LineSegments(polygonMeshEdges(mesh), edgeMaterial()));
  return group;
}

interface LabelArt {
  texture: THREE.Texture;
  aspect: number;
  lines: number;
}

const labelArt = new Map<string, LabelArt>();

/** Multi-line billboard text; textures cached, materials disposable. */
function label(text: string, lineHeight = 0.12, maxWidth = 2.2): THREE.Sprite {
  let art = labelArt.get(text);
  if (!art) {
    if (labelArt.size > 256) labelArt.clear();
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

const EDGE_CONTROL: Control = { key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 };
const SEED_CONTROL: Control = { key: 'seed', label: 'Seed', kind: 'select', options: SEEDS.map(s => s.name), def: 1 };
const COLOR_CONTROL: Control = { key: 'aspect', label: 'Colour faces by', kind: 'select', options: [...ASPECTS], def: 1 };

function aspectOf(params: Params): number {
  return Math.min(Math.max(Math.round(params.aspect ?? 0), 0), ASPECTS.length - 1);
}

// ---------------------------------------------------------------------------
// 1. The workbench: one seed, one operator, every dial.

const workbenchScene: Scene = {
  id: 'workbench',
  title: 'Operator workbench',
  description:
    'Any Conway operator on any Platonic seed, through the generated ApplyConwayOperator dispatch. Amount is the apex height of kis/join/needle and the face shrink of expand/snub/chamfer; explode slides every face out along its own normal; spherize slides every vertex toward the unit sphere — watch flatness colouring light up on a face-transitive result.',
  plato: [
    'PolygonMesh3D.ApplyConwayOperator',
    'PolygonMesh3D.FaceAspectColors',
    'PolygonMesh3D.Exploded',
    'PolygonMesh3D.Spherized',
    'PolygonMesh3D.ScaledToUnitCircumradius',
    'ColorGradient.StudioRamp',
  ],
  controls: [
    SEED_CONTROL,
    { key: 'op', label: 'Operator', kind: 'select', options: [...OPERATORS], def: 3 },
    { key: 'amount', label: 'Amount', kind: 'slider', min: 0, max: 1, step: 0.005, def: 0.3 },
    { key: 'twist', label: 'Snub twist (rad)', kind: 'slider', min: -0.7, max: 0.7, step: 0.001, def: SNUB_TWIST },
    COLOR_CONTROL,
    { key: 'explode', label: 'Explode', kind: 'slider', min: 0, max: 0.6, step: 0.005, def: 0 },
    { key: 'spherize', label: 'Spherize', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0 },
    EDGE_CONTROL,
  ],
  build(params) {
    const seed = seedOf(params);
    const op = Math.min(Math.max(Math.round(params.op ?? 0), 0), OPERATORS.length - 1);
    const base = memo(
      `${seed.name}#${op}@${params.amount},${params.twist}`,
      () => seedMesh(seed)
        .ApplyConwayOperator(op, params.amount, new Angle(params.twist))
        .ScaledToUnitCircumradius(),
    );
    const shaped = memo(
      `${seed.name}#${op}@${params.amount},${params.twist}~${params.spherize}!${params.explode}`,
      () => {
        const spherized = params.spherize > 0 ? base.Spherized(params.spherize) : base;
        return params.explode > 0 ? spherized.Exploded(params.explode) : spherized;
      },
    );
    const group = solid(shaped, aspectOf(params), params.edges !== 0);
    group.scale.setScalar(1.15);
    const root = new THREE.Group();
    root.add(group);
    const program = op === 0 ? seed.name : `${seed.name}.${OPERATORS[op].replace(/^./, c => c.toUpperCase())}`;
    const sprite = label(`${program}\n${countLine(shaped)}\n${profileLine(shaped)}`);
    sprite.position.set(0, 1.6, 0);
    root.add(sprite);
    return root;
  },
  status(params) {
    const seed = seedOf(params);
    const op = Math.min(Math.max(Math.round(params.op ?? 0), 0), OPERATORS.length - 1);
    const key = `${seed.name}#${op}@${params.amount},${params.twist}`;
    const mesh = meshCache.get(key);
    return mesh ? readingsLine(mesh) : '';
  },
};

// ---------------------------------------------------------------------------
// 2. Chained programs: three operators composed.

/** The reduced op list a chain stage offers (no reflect — invisible alone). */
const CHAIN_OPS = OPERATORS.slice(0, 16);

/** A chain stage is skipped rather than run when the mesh is already large:
 *  the twin-corner search is quadratic (plato-446), and meta³ of a dodecahedron
 *  is minutes, not frames. The status line says so honestly. */
const CHAIN_CORNER_CAP = 2600;

function chainOf(params: Params): number[] {
  return [params.op1 ?? 0, params.op2 ?? 0, params.op3 ?? 0]
    .map(v => Math.min(Math.max(Math.round(v), 0), CHAIN_OPS.length - 1));
}

function chainKey(seed: Seed, ops: number[]): string {
  return `${seed.name}>${ops.join('>')}`;
}

interface ChainResult {
  mesh: PolygonMesh3D;
  applied: string[];
  skipped: string[];
}

const chainCache = new Map<string, ChainResult>();

function buildChain(seed: Seed, ops: number[]): ChainResult {
  const key = chainKey(seed, ops);
  const hit = chainCache.get(key);
  if (hit) return hit;
  if (chainCache.size > 96) chainCache.clear();
  let mesh = seedMesh(seed);
  const applied: string[] = [];
  const skipped: string[] = [];
  for (const op of ops) {
    if (op === 0) continue;
    if (mesh.CornerCount() > CHAIN_CORNER_CAP) {
      skipped.push(OPERATORS[op]);
      continue;
    }
    mesh = applyDefault(mesh, op);
    applied.push(OPERATORS[op]);
  }
  const result = { mesh: mesh.ScaledToUnitCircumradius(), applied, skipped };
  chainCache.set(key, result);
  return result;
}

const chainScene: Scene = {
  id: 'chain',
  title: 'Operator chains',
  description:
    'Three Conway operators composed, each at its page default. This is the algebra the named catalog is written in: bevel is truncate∘ambo, zip is truncate∘dual, needle is kis∘dual — build either side and compare the counts. Chamfer twice for a Goldberg-flavoured hexagon field.',
  plato: [
    'PolygonMesh3D.ApplyConwayOperator',
    'PolygonMesh3D.FaceAspectColors',
    'PolygonMesh3D.EulerCharacteristic',
    'PolygonMesh3D.ScaledToUnitCircumradius',
  ],
  controls: [
    SEED_CONTROL,
    { key: 'op1', label: 'First', kind: 'select', options: [...CHAIN_OPS], def: 2 },
    { key: 'op2', label: 'Then', kind: 'select', options: [...CHAIN_OPS], def: 4 },
    { key: 'op3', label: 'Then', kind: 'select', options: [...CHAIN_OPS], def: 0 },
    { ...COLOR_CONTROL, def: 1 },
    EDGE_CONTROL,
  ],
  build(params) {
    const seed = seedOf(params);
    const { mesh, applied } = buildChain(seed, chainOf(params));
    const group = solid(mesh, aspectOf(params), params.edges !== 0);
    group.scale.setScalar(1.15);
    const root = new THREE.Group();
    root.add(group);
    const program = [seed.name, ...applied].join(' . ');
    const sprite = label(`${program}\n${countLine(mesh)}\n${profileLine(mesh)}`);
    sprite.position.set(0, 1.6, 0);
    root.add(sprite);
    return root;
  },
  status(params) {
    const { mesh, skipped } = buildChain(seedOf(params), chainOf(params));
    const warning = skipped.length ? `  SKIPPED ${skipped.join(', ')} (mesh too large)` : '';
    return readingsLine(mesh) + warning;
  },
};

// ---------------------------------------------------------------------------
// 3-4. The named catalog, coloured by aspect.

interface NamedSolid {
  name: string;
  program: string;
  make(): PolygonMesh3D;
}

const ARCHIMEDEAN: NamedSolid[] = [
  { name: 'Truncated tetrahedron', program: 'Tetrahedron.Truncate', make: () => PolygonMesh3D.TruncatedTetrahedron() },
  { name: 'Cuboctahedron', program: 'Cube.Ambo', make: () => PolygonMesh3D.Cuboctahedron() },
  { name: 'Truncated cube', program: 'Cube.Truncate', make: () => PolygonMesh3D.TruncatedCube() },
  { name: 'Truncated octahedron', program: 'Octahedron.Truncate = Cube.Zip', make: () => PolygonMesh3D.TruncatedOctahedron() },
  { name: 'Rhombicuboctahedron', program: 'Cube.Expand(sqrt(2) - 1)', make: () => PolygonMesh3D.Rhombicuboctahedron() },
  { name: 'Truncated cuboctahedron', program: 'Cube.Bevel', make: () => PolygonMesh3D.TruncatedCuboctahedron() },
  { name: 'Snub cube', program: 'Cube.Snub(0.4376, 0.2874)', make: () => PolygonMesh3D.SnubCube() },
  { name: 'Icosidodecahedron', program: 'Dodecahedron.Ambo', make: () => PolygonMesh3D.Icosidodecahedron() },
  { name: 'Truncated dodecahedron', program: 'Dodecahedron.Truncate', make: () => PolygonMesh3D.TruncatedDodecahedron() },
  { name: 'Truncated icosahedron', program: 'Icosahedron.Truncate', make: () => PolygonMesh3D.TruncatedIcosahedron() },
  { name: 'Rhombicosidodecahedron', program: 'Dodecahedron.Expand(phi / 3)', make: () => PolygonMesh3D.Rhombicosidodecahedron() },
  { name: 'Truncated icosidodecahedron', program: 'Dodecahedron.Bevel', make: () => PolygonMesh3D.TruncatedIcosidodecahedron() },
  { name: 'Snub dodecahedron', program: 'Dodecahedron.Snub(0.5621, 0.2287)', make: () => PolygonMesh3D.SnubDodecahedron() },
];

const CATALAN: NamedSolid[] = [
  { name: 'Triakis tetrahedron', program: 'TruncatedTetrahedron.Dual', make: () => PolygonMesh3D.TriakisTetrahedron() },
  { name: 'Rhombic dodecahedron', program: 'Cuboctahedron.Dual = Cube.Join', make: () => PolygonMesh3D.RhombicDodecahedron() },
  { name: 'Triakis octahedron', program: 'TruncatedCube.Dual', make: () => PolygonMesh3D.TriakisOctahedron() },
  { name: 'Tetrakis hexahedron', program: 'TruncatedOctahedron.Dual', make: () => PolygonMesh3D.TetrakisHexahedron() },
  { name: 'Deltoidal icositetrahedron', program: 'Rhombicuboctahedron.Dual', make: () => PolygonMesh3D.DeltoidalIcositetrahedron() },
  { name: 'Disdyakis dodecahedron', program: 'TruncatedCuboctahedron.Dual', make: () => PolygonMesh3D.DisdyakisDodecahedron() },
  { name: 'Pentagonal icositetrahedron', program: 'SnubCube.Dual', make: () => PolygonMesh3D.PentagonalIcositetrahedron() },
  { name: 'Rhombic triacontahedron', program: 'Icosidodecahedron.Dual', make: () => PolygonMesh3D.RhombicTriacontahedron() },
  { name: 'Triakis icosahedron', program: 'TruncatedDodecahedron.Dual', make: () => PolygonMesh3D.TriakisIcosahedron() },
  { name: 'Pentakis dodecahedron', program: 'TruncatedIcosahedron.Dual', make: () => PolygonMesh3D.PentakisDodecahedron() },
  { name: 'Deltoidal hexecontahedron', program: 'Rhombicosidodecahedron.Dual', make: () => PolygonMesh3D.DeltoidalHexecontahedron() },
  { name: 'Disdyakis triacontahedron', program: 'TruncatedIcosidodecahedron.Dual', make: () => PolygonMesh3D.DisdyakisTriacontahedron() },
  { name: 'Pentagonal hexecontahedron', program: 'SnubDodecahedron.Dual', make: () => PolygonMesh3D.PentagonalHexecontahedron() },
];

function catalogScene(id: string, title: string, description: string, solids: NamedSolid[], def: number, colorDef: number): Scene {
  return {
    id,
    title,
    description,
    plato: [
      'PolygonMesh3D.FaceAspectColors',
      'PolygonMesh3D.Sphericity',
      'PolygonMesh3D.EdgeLengthRatio',
      'PolygonMesh3D.ScaledToUnitCircumradius',
    ],
    controls: [
      { key: 'solid', label: 'Solid', kind: 'select', options: solids.map(s => s.name), def },
      { ...COLOR_CONTROL, def: colorDef },
      EDGE_CONTROL,
    ],
    build(params) {
      const index = Math.min(Math.max(Math.round(params.solid ?? 0), 0), solids.length - 1);
      const entry = solids[index];
      const mesh = memo(`${id}:${entry.name}`, () => entry.make().ScaledToUnitCircumradius());
      const group = solid(mesh, aspectOf(params), params.edges !== 0, palette.surfaceAlt);
      group.scale.setScalar(1.15);
      const root = new THREE.Group();
      root.add(group);
      const sprite = label(`${entry.name}\n${entry.program}\n${countLine(mesh)}\n${profileLine(mesh)}`);
      sprite.position.set(0, 1.6, 0);
      root.add(sprite);
      return root;
    },
    status(params) {
      const index = Math.min(Math.max(Math.round(params.solid ?? 0), 0), solids.length - 1);
      const mesh = meshCache.get(`${id}:${solids[index].name}`);
      return mesh ? readingsLine(mesh) : '';
    },
  };
}

const archimedeanScene = catalogScene(
  'archimedean',
  'Archimedean solids',
  'All thirteen, each a one-line Conway program over a Platonic seed. Colour by sides to see the face families that make each one "semi-regular": every vertex identical, faces of two or three kinds. Their edge ratio reads exactly 1 for all but the two bevels.',
  ARCHIMEDEAN, 9, 1,
);

const catalanScene = catalogScene(
  'catalan',
  'Catalan solids',
  'All thirteen duals. Face-transitive, so colouring by sides makes each one a single colour — switch to radius and the vertex rings at different distances appear instead, the very property that forbids ProjectedToUnitSphere on them.',
  CATALAN, 1, 3,
);

// ---------------------------------------------------------------------------
// 5. Dual pairs, superimposed.

const dualPairScene: Scene = {
  id: 'dual-pairs',
  title: 'Dual pairs',
  description:
    'Each Archimedean with its Catalan dual superimposed: every edge of one crosses exactly one edge of the other, and V and F swap while E holds. The pairing itself is stdlib vocabulary — Dual on the ArchimedeanSolid kind returns the CatalanSolid kind — and the meshes are the same catalog members as the other scenes.',
  plato: [
    'PolygonMesh3D.Dual',
    'PolygonMesh3D.UndirectedEdgeCount',
    'PolygonMesh3D.ScaledToUnitCircumradius',
  ],
  controls: [
    { key: 'pair', label: 'Pair', kind: 'select', options: ARCHIMEDEAN.map(s => s.name), def: 1 },
    { key: 'which', label: 'Show', kind: 'select', options: ['both', 'archimedean', 'catalan'], def: 0 },
    EDGE_CONTROL,
  ],
  build(params) {
    const index = Math.min(Math.max(Math.round(params.pair ?? 0), 0), ARCHIMEDEAN.length - 1);
    const which = Math.round(params.which ?? 0);
    const archimedean = memo(`archimedean:${ARCHIMEDEAN[index].name}`, () =>
      ARCHIMEDEAN[index].make().ScaledToUnitCircumradius());
    const catalan = memo(`catalan:${CATALAN[index].name}`, () =>
      CATALAN[index].make().ScaledToUnitCircumradius());

    const root = new THREE.Group();
    const group = new THREE.Group();
    if (which !== 2) group.add(solid(archimedean, 0, params.edges !== 0, palette.surface));
    if (which !== 1) {
      const dual = solid(catalan, 0, params.edges !== 0, palette.surfaceAlt);
      if (which === 0) {
        for (const mesh of dual.children) {
          const material = (mesh as THREE.Mesh).material as THREE.Material;
          material.transparent = true;
          material.opacity = 0.55;
        }
      }
      group.add(dual);
    }
    group.scale.setScalar(1.15);
    root.add(group);

    const a = ARCHIMEDEAN[index];
    const c = CATALAN[index];
    const sprite = label(
      `${a.name}  <->  ${c.name}\n` +
      `V ${archimedean.VertexCount()} F ${archimedean.FaceCount()}  <->  V ${catalan.VertexCount()} F ${catalan.FaceCount()}\n` +
      `E ${archimedean.UndirectedEdgeCount()} = ${catalan.UndirectedEdgeCount()}`,
    );
    sprite.position.set(0, 1.6, 0);
    root.add(sprite);
    return root;
  },
};

// ---------------------------------------------------------------------------
// 6. The chamfer road to Goldberg.

const goldbergScene: Scene = {
  id: 'goldberg',
  title: 'Chamfer to Goldberg',
  description:
    'Chamfer replaces every edge with a hexagon and keeps every face in its own plane, so iterating it grows a Goldberg-style hexagon field around the seed\'s original faces — the dodecahedron\'s twelve pentagons survive every round, exactly as on a football or a virus capsid. Colour by sides to see them.',
  plato: [
    'PolygonMesh3D.Chamfer',
    'PolygonMesh3D.FaceAspectColors',
    'PolygonMesh3D.Sphericity',
    'PolygonMesh3D.ScaledToUnitCircumradius',
  ],
  controls: [
    { key: 'seed', label: 'Seed', kind: 'select', options: ['Cube', 'Dodeca'], def: 1 },
    { key: 'rounds', label: 'Chamfer rounds', kind: 'slider', min: 0, max: 3, step: 1, def: 1 },
    { key: 'shrink', label: 'Shrink', kind: 'slider', min: 0.3, max: 0.9, step: 0.005, def: 0.55 },
    { ...COLOR_CONTROL, def: 1 },
    EDGE_CONTROL,
  ],
  build(params) {
    const seedName = Math.round(params.seed ?? 0) === 0 ? 'Cube' : 'Dodeca';
    const rounds = Math.min(Math.max(Math.round(params.rounds ?? 0), 0), 3);
    const mesh = memo(`goldberg:${seedName}x${rounds}@${params.shrink}`, () => {
      let m = seedName === 'Cube' ? PolygonMesh3D.Cube() : PolygonMesh3D.Dodecahedron();
      for (let i = 0; i < rounds; i++) m = m.Chamfer(params.shrink);
      return m.ScaledToUnitCircumradius();
    });
    const group = solid(mesh, aspectOf(params), params.edges !== 0);
    group.scale.setScalar(1.15);
    const root = new THREE.Group();
    root.add(group);
    const program = `${seedName}${'.Chamfer'.repeat(rounds)}`;
    const sprite = label(`${program}\n${countLine(mesh)}\n${profileLine(mesh)}`);
    sprite.position.set(0, 1.6, 0);
    root.add(sprite);
    return root;
  },
  status(params) {
    const seedName = Math.round(params.seed ?? 0) === 0 ? 'Cube' : 'Dodeca';
    const rounds = Math.min(Math.max(Math.round(params.rounds ?? 0), 0), 3);
    const mesh = meshCache.get(`goldberg:${seedName}x${rounds}@${params.shrink}`);
    return mesh ? readingsLine(mesh) : '';
  },
};

// ---------------------------------------------------------------------------
// 7. The sphericity ladder.

/** The programs of the ladder, gentlest first; every mesh is closed, so the
 *  generated Sphericity is meaningful for each. */
const LADDER: { name: string; make(): PolygonMesh3D }[] = [
  { name: 'Tetra', make: () => PolygonMesh3D.Tetrahedron() },
  { name: 'Cube', make: () => PolygonMesh3D.Cube() },
  { name: 'Icosa', make: () => PolygonMesh3D.Icosahedron() },
  { name: 'Icosa.Truncate', make: () => PolygonMesh3D.TruncatedIcosahedron() },
  { name: 'Dodeca.Expand', make: () => PolygonMesh3D.Rhombicosidodecahedron() },
  { name: 'Dodeca.Bevel', make: () => PolygonMesh3D.TruncatedIcosidodecahedron() },
  { name: 'Dodeca.Chamfer.Chamfer', make: () => PolygonMesh3D.Dodecahedron().Chamfer(0.57).Chamfer(0.57) },
];

const ladderScene: Scene = {
  id: 'sphericity',
  title: 'Sphericity ladder',
  description:
    'Seven closed solids in a row, each labelled with the generated Sphericity — the surface area of the equal-volume ball over its own, 1.0 only for the ball itself. Conway programs climb the ladder: more faces, rounder solid. The reading under each is EnclosedVolume, by the divergence theorem.',
  plato: [
    'PolygonMesh3D.Sphericity',
    'PolygonMesh3D.EnclosedVolume',
    'PolygonMesh3D.TotalFaceArea',
    'PolygonMesh3D.ScaledToUnitCircumradius',
  ],
  viewer: { distance: 6.4 },
  controls: [{ ...COLOR_CONTROL, def: 2 }, EDGE_CONTROL],
  build(params) {
    const root = new THREE.Group();
    const spacing = 1.5;
    LADDER.forEach((entry, i) => {
      const mesh = memo(`ladder:${entry.name}`, () => entry.make().ScaledToUnitCircumradius());
      const x = (i - (LADDER.length - 1) / 2) * spacing;
      const group = solid(mesh, aspectOf(params), params.edges !== 0);
      group.scale.setScalar(0.62);
      group.position.set(x, 0, 0);
      root.add(group);
      const sprite = label(
        `${entry.name}\nΨ ${mesh.Sphericity().toFixed(4)}\nvol ${mesh.EnclosedVolume().toFixed(3)}`,
        0.09, 1.3,
      );
      sprite.position.set(x, 1.0, 0);
      root.add(sprite);
    });
    return root;
  },
};

// ---------------------------------------------------------------------------

const demo: Demo = {
  title: 'Polyhedron studio',
  subtitle: 'polyhedra.library.plato · polyhedron-studio.plato',
  scenes: [
    workbenchScene,
    chainScene,
    archimedeanScene,
    catalanScene,
    dualPairScene,
    goldbergScene,
    ladderScene,
  ],
};

mountDemo(demo, { distance: 4.6 });

// The page never imports this; it exists so `npm run scenes` can call every
// scene's `build` without a WebGL context.
export { demo };
