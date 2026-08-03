// Deformers — a scene catalog over `stdlib/geometry/deformations.*.plato`.
//
// Every warp here is a value of one of the catalog types in
// `deformations.types.plato` (Twist3D, Bend3D, Taper3D, Shear3D, Spherify3D,
// Shear2D, Taper2D, MappingDeformation2D/3D) and every point that moves is moved
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
// Cost: `build` runs on every slider tick, so the subject mesh is memoized per
// (seed, truncate rounds) and only the deform + repack is redone on a tick. The
// detail slider is capped at three rounds of `Truncate`, which puts the largest
// subject (the prism column) at 648 verts / 326 faces / 1292 triangles and every
// other seed below that. `Truncate` is the subdivider rather than `Ambo` because
// `Ambo` costs seconds past three rounds on the same seeds.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { polygonMeshEdges, polygonMeshGeometry, point2D, toArray } from '../shared/mesh.js';
import { edgeMaterial, palette, surfaceMaterial } from '../shared/viewer.js';
import {
  Angle,
  Bend3D,
  Direction2D,
  Direction3D,
  Frame3D,
  MappingDeformation2D,
  MappingDeformation3D,
  Point2D,
  Point3D,
  Polygon2D,
  PolygonMesh3D,
  RegularPolygon,
  RegularPrism,
  Shear2D,
  Shear3D,
  Sphere,
  Spherify3D,
  Taper2D,
  Taper3D,
  Twist3D,
  Vector2D,
  Vector3D,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Subject meshes
// ---------------------------------------------------------------------------

const SUBJECT_LABELS = [
  'Column (12-gon prism ▸ truncate)',
  'Ball (cube ▸ truncate ▸ unit sphere)',
  'Ball (icosahedron ▸ truncate ▸ unit sphere)',
  'Faceted (cube ▸ truncate)',
];

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
  const rounds = Math.max(1, Math.min(3, Math.round(params.detail ?? 2)));
  const key = `${kind}:${rounds}`;
  let base = subjectCache.get(key);
  if (!base) {
    const t = truncated(kind, rounds);
    base = kind === 1 || kind === 2 ? t.ProjectedToUnitSphere() : t;
    subjectCache.set(key, base);
  }
  return base;
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
  'PolygonMesh3D.Truncate',
  'PolygonMesh3D.ProjectedToUnitSphere',
];

const SUBJECT_CONTROLS: Control[] = [
  { key: 'subject', label: 'Subject', kind: 'select', options: SUBJECT_LABELS, def: 0 },
  { key: 'detail', label: 'Truncate rounds', kind: 'slider', min: 1, max: 3, step: 1, def: 2 },
  { key: 'edges', label: 'Show edges', kind: 'toggle', def: 1 },
  { key: 'ghost', label: 'Ghost the undeformed mesh', kind: 'toggle', def: 1 },
];

/**
 * Apply a point map with the generated `PolygonMesh3D.Deform` and repack it.
 * `polygonMeshGeometry` recomputes vertex normals from the moved positions,
 * which is what the library's normals contract requires.
 */
function warped(params: Params, mapping: (p: Point3D) => Point3D): THREE.Object3D {
  const base = subject(params);
  const deformed = base.Deform(mapping);
  const group = new THREE.Group();
  group.add(new THREE.Mesh(polygonMeshGeometry(deformed), surfaceMaterial()));
  if (params.edges) group.add(new THREE.LineSegments(polygonMeshEdges(deformed), edgeMaterial()));
  if (params.ghost) group.add(new THREE.LineSegments(polygonMeshEdges(base), edgeMaterial(palette.line)));
  return group;
}

// ---------------------------------------------------------------------------
// Planar scenes: a lattice and a ring, both warped by an IDeformation2D
//
// The planar catalog also has Twist2D (the polar swirl), but it has no scene:
// its generated `Eval` calls `Vector2D.Transform(Rotation2D)`, and the writer
// kept only the `Transform(AffineTransform2D)` overload, so the Rotation2D it is
// handed has no `Matrix` and the body throws on `matrix.Row1`. That is the same
// dropped-overload gap `src/plato/array-ext.ts` patches for
// `Transform(Quaternion)` on Vector3D / Point3D, one dimension down. Once the
// prelude or the writer covers it, a Twist2D scene drops straight in here.
// ---------------------------------------------------------------------------

const PLANAR_CONTROLS: Control[] = [
  { key: 'lines', label: 'Lattice lines', kind: 'slider', min: 3, max: 15, step: 2, def: 9 },
  { key: 'ring', label: 'Show ring', kind: 'toggle', def: 1 },
];

const PLANAR_EXTENT = 1.6;
const PLANAR_SAMPLES = 41;

/** Grid lines + a regular ring, every sample pushed through the deformation's `Eval`. */
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

  if (params.ring) {
    const ring = new Polygon2D(new RegularPolygon(point2D(0, 0), 1, 96, new Angle(0)).RegularPolygonVertices());
    const loop: number[] = [];
    for (const p of toArray(ring.Deform(evaluate).Points)) loop.push(p.X, p.Y, 0.001);
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.Float32BufferAttribute(loop, 3));
    group.add(new THREE.LineLoop(geometry, edgeMaterial(palette.accent)));
  }
  return group;
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
  },
  {
    id: 'shear-2d',
    title: 'Shear2D',
    description:
      'The planar catalog entry: Shear2D translates each point by the Rate vector scaled by its signed distance from Origin along Axis. The lattice and the unit ring are both pushed through the same Eval.',
    plato: ['Shear2D.Eval', 'Polygon2D.Deform', 'RegularPolygon.RegularPolygonVertices'],
    controls: [
      { key: 'rateX', label: 'Rate X', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0 },
      { key: 'rateY', label: 'Rate Y', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0.6 },
      { key: 'axis', label: 'Axis', kind: 'select', options: ['X', 'Y'], def: 0 },
      { key: 'origin', label: 'Origin along axis', kind: 'slider', min: -1.5, max: 1.5, step: 0.05, def: 0 },
      ...PLANAR_CONTROLS,
    ],
    build: params => {
      const a = planarAxis(params.axis);
      const shear = new Shear2D(planarOrigin(a, params.origin), a, new Vector2D(params.rateX, params.rateY));
      return planar(params, p => shear.Eval(p));
    },
  },
  {
    id: 'taper-2d',
    title: 'Taper2D',
    description:
      'Taper2D scales the component perpendicular to Axis by (1 + Rate * t). On a lattice it reads as a wedge; the ring becomes a teardrop.',
    plato: ['Taper2D.Eval', 'Polygon2D.Deform', 'RegularPolygon.RegularPolygonVertices'],
    controls: [
      { key: 'rate', label: 'Rate', kind: 'slider', min: -0.9, max: 1.5, step: 0.01, def: 0.6 },
      { key: 'axis', label: 'Axis', kind: 'select', options: ['X', 'Y'], def: 0 },
      { key: 'origin', label: 'Origin along axis', kind: 'slider', min: -1.5, max: 1.5, step: 0.05, def: 0 },
      ...PLANAR_CONTROLS,
    ],
    build: params => {
      const a = planarAxis(params.axis);
      const taper = new Taper2D(planarOrigin(a, params.origin), a, params.rate);
      return planar(params, p => taper.Eval(p));
    },
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
      'Polygon2D.Deform',
    ],
    controls: [
      { key: 'shearY', label: 'Shear: rate Y', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0.6 },
      { key: 'rate', label: 'Taper: rate', kind: 'slider', min: -0.9, max: 1.5, step: 0.01, def: 0.6 },
      { key: 'order', label: 'Order', kind: 'select', options: ['Shear ▸ Taper', 'Taper ▸ Shear'], def: 0 },
      { key: 'alias', label: 'Combine with', kind: 'select', options: ['Compose', 'Multiply (the * alias)'], def: 0 },
      ...PLANAR_CONTROLS,
    ],
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

// Spin is off because the page mixes spatial and planar scenes: the shared
// viewer auto-rotates about Y, which would swing the z = 0 planar scenes edge-on
// and make them vanish. The orbit camera still works; drag to face the plane.
mountDemo(demo, { distance: 8, grid: false, spin: false });
