// Lattices — a scene catalog over `stdlib/geometry/lattices.{types,library}.plato`:
// the seven named unit cells, the tiling that welds one into world-space struts,
// the operators over that strut list, and the triply periodic minimal surfaces.
//
// The shape of the library, because it decides the shape of every scene:
//
//  - A UNIT CELL is a graph in the unit cube — `Nodes` in normalized cell
//    coordinates plus `Struts` indexing into them. Nothing about it carries a
//    world size, so `NodeValences`, `OwnedStrutCount` and `NormalizedStrutLength`
//    are answerable before any placement. `NodeValences` is the coordination
//    number in the INFINITE tiling, which is what scene one colours nodes by.
//  - A LATTICE is `StrutLattice3D(cell, bounds, counts, radius)`. `Struts()` is
//    the single tiling path: every shared strut emitted once, every shared node
//    on one world position, welded by an ownership rule rather than by a search.
//  - Every OPERATOR — `Trimmed`, `StrutRadii`, `Deformed`, `ToSdf` — takes the
//    welded `Array<Line3D>` rather than the lattice, so they compose in any
//    order and none of them re-tiles. Graded cell size is `Deformed`, not a
//    second tiling.
//  - A TPMS is the implicit counterpart. `TpmsField3D` is a scalar field and is
//    NOT a distance; `TpmsNetwork3D` (one labyrinth) and `TpmsSheet3D` (a wall)
//    divide it by a Lipschitz bound and are honest LOWER BOUNDS.
//
// Cost, because `build` runs on every parameter tick:
//
//  - `StrutSdf3D.Eval` is a `Reduce` over every strut with no acceleration
//    structure — the type comment says so — and marching cubes reads each node
//    about eight times. Measured here at ~0.75 us per strut per sample, so the
//    surface scene TRIMS FIRST, samples the field onto a node grid ONCE (the
//    house pattern from `voxels.ts`, memoized), and hands the materialized
//    volume to `SampledSdf3D.MarchingCubes`. Calling `StrutSdf3D.MarchingCubes`
//    straight through is the scene's toggle, with its own millisecond reading,
//    because the cost IS the lesson.
//  - `Struts()` and `Trimmed` are eager (the prelude's `FlatMap` materializes),
//    so one call per build is held in a local rather than re-asked. `StrutRadii`
//    and `Deformed` are `Map` views; the memoized `Arr` (plato-436) computes
//    each element once however often they are re-read.
//  - Struts are drawn as ONE `THREE.InstancedMesh` of cylinders, never one mesh
//    per strut: a lattice here runs to several thousand segments.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { toArray, triangleArrayGeometry } from '../shared/mesh.js';
import { palette, surfaceMaterial, type ViewerOptions } from '../shared/viewer.js';
import {
  Bend3D,
  Bounds3D,
  Capsule3D,
  Color,
  Direction3D,
  GradedStrutSdf3D,
  IntegerVector3,
  Intrinsics,
  ItemIndex,
  LatticeUnitCell,
  Line3D,
  NumberInterval,
  PerlinNoise3D,
  Point3D,
  SampledSdf3D,
  ScalarFunctionField3D,
  Sphere,
  Spherify3D,
  StrutLattice3D,
  StrutSdf3D,
  Taper3D,
  TpmsField3D,
  TpmsNetwork3D,
  TpmsSheet3D,
  Twist3D,
  Vector3D,
  type IArray,
  type IArray3D,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// The house pattern from `polygons.ts`: a member that throws or returns NaN is a
// gap in the emitted library, and the status line keeps its name and says so
// rather than substituting a hand-rolled answer.

interface Reading {
  label: string;
  value: string;
}

function reading(label: string, produce: () => string): Reading {
  try {
    const value = produce();
    return { label, value: /NaN/.test(value) ? `NaN from ${label}` : value };
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
const pct = (x: number): string => `${(x * 100).toFixed(1)}%`;
const iv = (v: IntegerVector3): string => `(${v.X}, ${v.Y}, ${v.Z})`;
const vec = (v: Vector3D): string => `(${n3(v.X)}, ${n3(v.Y)}, ${n3(v.Z)})`;

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

const clampIndex = (raw: number, count: number): number =>
  Math.min(count - 1, Math.max(0, Math.round(raw)));

// ---------------------------------------------------------------------------
// The array-receiver surface of `lattices.library.plato`
//
// Every operator over a welded strut list has `Array<Line3D>` in first position,
// so the TypeScript writer emitted none of them and `src/plato/array-ext.ts`
// supplies the bodies on `Arr.prototype`. `IArray<T>` in the generated module
// declares only At/Count/Map/Reduce, so the operators are reached through this
// shape — the same device `marching.ts` uses for the case table.

/** Anything the three `Trimmed` overloads accept: all three answer `Contains`. */
interface TrimRegion {
  Contains(point: Point3D): boolean;
}

/** Anything `StrutRadii` will read a radius out of: an `IScalarField3D`. */
interface ScalarField {
  Eval(point: Point3D): number;
}

interface StrutArray extends IArray<Line3D> {
  /** `TotalLength(struts)` — the length of wire the lattice is. */
  TotalLength(): number;
  /** `RelativeDensity(struts, radius, envelope)` and its graded twin. */
  RelativeDensity(radius: number, envelope: Bounds3D): number;
  RelativeDensity(radii: IArray<number>, envelope: Bounds3D): number;
  /** The three `StrutRadii` overloads: constant, field, field over a range. */
  StrutRadii(radius: number): IArray<number>;
  StrutRadii(field: ScalarField): IArray<number>;
  StrutRadii(field: ScalarField, range: NumberInterval): IArray<number>;
  /** `Trimmed(struts, region | solid | volume)` — whole struts, by midpoint. */
  Trimmed(region: TrimRegion): StrutArray;
  /** `Deformed(struts, mapping)` — the warp, and how graded cell size is said. */
  Deformed(mapping: (p: Point3D) => Point3D): StrutArray;
  /** `ToSdf(struts, radius)` and `ToSdf(struts, radii)`. */
  ToSdf(radius: number): StrutSdf3D;
  ToSdf(radii: IArray<number>): GradedStrutSdf3D;
}

/**
 * `TpmsFamily` is a sum type, and CHK320 keeps sum types out of this target, so
 * the five cases and the four functions that dispatch on them arrive on
 * `globalThis` from the prelude rather than from the generated module.
 */
interface TpmsCase {
  readonly Tag: string;
  TpmsNodalValue(period: number, p: Point3D): number;
  TpmsPartialBound(): number;
  TpmsGradientBound(period: number): number;
  TpmsNormalizedValue(period: number, level: number, p: Point3D): number;
}

const prelude = globalThis as unknown as {
  TpmsFamily: Record<string, () => TpmsCase>;
};

/** An `IArray<T>` over a plain array, for handing values back to the library. */
const arrayOf = <T,>(xs: readonly T[]): IArray<T> => Intrinsics.Range(xs.length).Map(i => xs[i]);

const struts = (xs: IArray<Line3D>): StrutArray => xs as StrutArray;

// ---------------------------------------------------------------------------
// The cells
//
// Seven named constants plus the parameterized re-entrant family. Each is
// dispatched on `LatticeUnitCell` in the source — the tree's constant idiom —
// and read here as `LatticeUnitCell.OctetTruss()`; `ReentrantHoneycomb` takes
// its fold depth as the receiver, so it is spelled `(0.25).ReentrantHoneycomb()`.

const CELL_LABELS = ['Cubic', 'BCC', 'FCC', 'Octet', 'Diamond', 'Kelvin', 'Auxetic'];

const CELL_MEMBERS = [
  'LatticeUnitCell.SimpleCubic',
  'LatticeUnitCell.BodyCenteredCubic',
  'LatticeUnitCell.FaceCenteredCubic',
  'LatticeUnitCell.OctetTruss',
  'LatticeUnitCell.DiamondCubic',
  'LatticeUnitCell.TruncatedOctahedron',
  'LatticeUnitCell.ReentrantAuxetic',
];

/** What the source says each cell is for, in one clause. */
const CELL_NOTES = [
  'coordination 6, bending-dominated, the softest',
  'coordination 8, a centre node to all eight corners',
  'coordination 12 at a corner, 4 at a face node',
  'coordination 12 everywhere, triangulated, stretch-dominated',
  'coordination 4, tetrahedral bonds at the quarter points',
  'the Kelvin cell, coordination 4 once tiled',
  'ReentrantHoneycomb(0.25) — 24-valent corners, 2-valent hinges',
];

function cellOf(index: number): LatticeUnitCell {
  switch (clampIndex(index, CELL_LABELS.length)) {
    case 0:
      return LatticeUnitCell.SimpleCubic();
    case 1:
      return LatticeUnitCell.BodyCenteredCubic();
    case 2:
      return LatticeUnitCell.FaceCenteredCubic();
    case 3:
      return LatticeUnitCell.OctetTruss();
    case 4:
      return LatticeUnitCell.DiamondCubic();
    case 5:
      return LatticeUnitCell.TruncatedOctahedron();
    default:
      return LatticeUnitCell.ReentrantAuxetic();
  }
}

const cellControl = (def = 3): Control => ({
  key: 'cell',
  label: 'Unit cell',
  kind: 'select',
  options: CELL_LABELS,
  def,
});

// ---------------------------------------------------------------------------
// Envelopes
//
// One cube and two lopsided boxes. The lopsided ones are the interesting case
// for `UniformLattice`, which divides the LONGEST axis and lets the others
// follow that cell size, so the counts it derives are what the scene reads out.

const ENVELOPE_LABELS = ['Cube', 'Slab', 'Tower'];

const CUBE = new Bounds3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
const SLAB = new Bounds3D(new Point3D(-1.5, -0.5, -1), new Point3D(1.5, 0.5, 1));
const TOWER = new Bounds3D(new Point3D(-0.6, -1.5, -0.6), new Point3D(0.6, 1.5, 0.6));

const envelopeOf = (index: number): Bounds3D =>
  [CUBE, SLAB, TOWER][clampIndex(index, ENVELOPE_LABELS.length)];

const ORIGIN = new Point3D(0, 0, 0);
const ONE_CELL = new IntegerVector3(1, 1, 1);

// ---------------------------------------------------------------------------
// Presentation
//
// A strut is an instance of one cylinder, oriented from its own two endpoints.
// Geometries are created per build because the viewer disposes the tree it is
// handed, so nothing here may be shared across rebuilds.

const UP = new THREE.Vector3(0, 1, 0);

/**
 * Every strut as one `THREE.InstancedMesh`. `radiusAt` is per strut, which is
 * what the graded scenes need; `tints` colours them individually.
 */
function strutTubes(
  segments: readonly Line3D[],
  radiusAt: (index: number) => number,
  color: number,
  tints?: readonly THREE.Color[],
): THREE.InstancedMesh {
  const geometry = new THREE.CylinderGeometry(1, 1, 1, 7, 1, false);
  const mesh = new THREE.InstancedMesh(
    geometry,
    surfaceMaterial(color),
    Math.max(1, segments.length),
  );
  mesh.count = segments.length;
  const matrix = new THREE.Matrix4();
  const direction = new THREE.Vector3();
  const midpoint = new THREE.Vector3();
  const scale = new THREE.Vector3();
  const rotation = new THREE.Quaternion();
  for (let i = 0; i < segments.length; i++) {
    const { A, B } = segments[i];
    direction.set(B.X - A.X, B.Y - A.Y, B.Z - A.Z);
    const length = direction.length();
    midpoint.set((A.X + B.X) / 2, (A.Y + B.Y) / 2, (A.Z + B.Z) / 2);
    rotation.setFromUnitVectors(UP, length > 1e-9 ? direction.divideScalar(length) : UP);
    const radius = radiusAt(i);
    scale.set(radius, Math.max(length, 1e-6), radius);
    matrix.compose(midpoint, rotation, scale);
    mesh.setMatrixAt(i, matrix);
    if (tints) mesh.setColorAt(i, tints[i]);
  }
  mesh.instanceMatrix.needsUpdate = true;
  if (mesh.instanceColor) mesh.instanceColor.needsUpdate = true;
  return mesh;
}

/** The same struts as bare lines — the cheap draw, for the large tilings. */
function strutLines(
  segments: readonly Line3D[],
  color: number,
  opacity = 1,
): THREE.LineSegments {
  const positions: number[] = [];
  for (const s of segments) positions.push(s.A.X, s.A.Y, s.A.Z, s.B.X, s.B.Y, s.B.Z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.LineSegments(
    geometry,
    new THREE.LineBasicMaterial({ color, transparent: opacity < 1, opacity }),
  );
}

/** Nodes as one instanced sphere set, tinted per node. */
function nodeSpheres(
  points: readonly Point3D[],
  radius: number,
  tints?: readonly THREE.Color[],
  color = palette.surfaceAlt,
): THREE.InstancedMesh {
  const geometry = new THREE.SphereGeometry(radius, 9, 6);
  const mesh = new THREE.InstancedMesh(geometry, surfaceMaterial(color), Math.max(1, points.length));
  mesh.count = points.length;
  const matrix = new THREE.Matrix4();
  for (let i = 0; i < points.length; i++) {
    matrix.makeTranslation(points[i].X, points[i].Y, points[i].Z);
    mesh.setMatrixAt(i, matrix);
    if (tints) mesh.setColorAt(i, tints[i]);
  }
  mesh.instanceMatrix.needsUpdate = true;
  if (mesh.instanceColor) mesh.instanceColor.needsUpdate = true;
  return mesh;
}

function segmentsOf(coordinates: number[], color: number, opacity = 1): THREE.LineSegments {
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(coordinates, 3));
  return new THREE.LineSegments(
    geometry,
    new THREE.LineBasicMaterial({ color, transparent: opacity < 1, opacity }),
  );
}

/** The twelve edges of a generated `Bounds3D`, appended to a coordinate list. */
function pushBoxEdges(out: number[], b: Bounds3D): void {
  const { X: x0, Y: y0, Z: z0 } = b.Min;
  const { X: x1, Y: y1, Z: z1 } = b.Max;
  const corners: [number, number, number][] = [
    [x0, y0, z0], [x1, y0, z0], [x1, y1, z0], [x0, y1, z0],
    [x0, y0, z1], [x1, y0, z1], [x1, y1, z1], [x0, y1, z1],
  ];
  const edges: [number, number][] = [
    [0, 1], [1, 2], [2, 3], [3, 0],
    [4, 5], [5, 6], [6, 7], [7, 4],
    [0, 4], [1, 5], [2, 6], [3, 7],
  ];
  for (const [a, c] of edges) out.push(...corners[a], ...corners[c]);
}

function isosurface(geometry: THREE.BufferGeometry, color: number, opacity = 1): THREE.Mesh {
  const material = surfaceMaterial(color);
  material.transparent = opacity < 1;
  material.opacity = opacity;
  return new THREE.Mesh(geometry, material);
}

const threeColor = (c: Color): THREE.Color =>
  new THREE.Color().setRGB(
    Math.min(1, Math.max(0, c.R)),
    Math.min(1, Math.max(0, c.G)),
    Math.min(1, Math.max(0, c.B)),
  );

const platoColor = (hex: number): Color =>
  new Color(((hex >> 16) & 255) / 255, ((hex >> 8) & 255) / 255, (hex & 255) / 255, 1);

const COLD = platoColor(0x3f7fd0);
const WARM = platoColor(0xf0a25a);
const MINT = platoColor(0x63d6a8);

/**
 * A value's colour on a cold-to-warm ramp. The parameter comes from the
 * library's own `NumberInterval.ParameterOf` (an `InverseLerp`) and the mix from
 * `Color.Lerp`, so nothing here restates either.
 */
function rampColor(range: NumberInterval, value: number): THREE.Color {
  const t = range.Extent() === 0 ? 0.5 : range.ParameterOf(value).Saturate();
  return threeColor(t < 0.5 ? COLD.Lerp(MINT, t * 2) : MINT.Lerp(WARM, (t - 0.5) * 2));
}

const SPATIAL_VIEW: ViewerOptions = { distance: 4.6, grid: false, spin: true };
const WIDE_VIEW: ViewerOptions = { distance: 8.4, grid: false, spin: true };
const STILL_VIEW: ViewerOptions = { distance: 4.6, grid: false, spin: false };

/** Min and max of an `IArray<Number>`, as a `NumberInterval` to read against. */
function spanOf(values: readonly number[]): NumberInterval {
  let low = Number.POSITIVE_INFINITY;
  let high = Number.NEGATIVE_INFINITY;
  for (const v of values) {
    if (v < low) low = v;
    if (v > high) high = v;
  }
  return new NumberInterval(Number.isFinite(low) ? low : 0, Number.isFinite(high) ? high : 0);
}

// ---------------------------------------------------------------------------
// Scene 1 — the seven cells
//
// Each cell is a one-cell `StrutLattice3D` placed in its own slot, so the world
// segments come from the same `Struts` the tilings use rather than from a second
// path. With counts of one nothing is owned by a neighbour, so the whole cell
// shows. Nodes are coloured by `NodeValences` — the coordination number in the
// infinite tiling, not the number of struts touching the node inside the cube.

interface CellSlot {
  label: string;
  cell: LatticeUnitCell;
  bounds: Bounds3D;
}

function cellSlots(reentrancy: number): CellSlot[] {
  const columns = 4;
  const pitch = 1.55;
  return CELL_LABELS.map((label, index) => {
    const column = index % columns;
    const row = Math.floor(index / columns);
    const cx = (column - (columns - 1) / 2) * pitch;
    const cy = (0.5 - row) * pitch;
    return {
      label,
      cell: index === 6 ? reentrancy.ReentrantHoneycomb() : cellOf(index),
      bounds: new Bounds3D(
        new Point3D(cx - 0.5, cy - 0.5, -0.5),
        new Point3D(cx + 0.5, cy + 0.5, 0.5),
      ),
    };
  });
}

const cells = sceneOf({
  id: 'cells',
  title: 'The seven named cells',
  description:
    'Every unit cell lattices.library.plato names, each drawn as a one-cell StrutLattice3D so the segments come ' +
    'out of the same Struts the tilings use. A cell is a graph in the unit cube — Nodes in normalized cell ' +
    'coordinates and Struts indexing into them — and each node is coloured by LatticeUnitCell.NodeValences, ' +
    'which is the coordination number ONCE TILED: how many struts meet there in the infinite lattice, computed ' +
    'from the cell alone by identifying nodes modulo the cell period. That is why the simple cubic cell reports ' +
    '6 at a corner where only three struts are visibly touching it, and why the diamond cell reports 4 at four ' +
    'corners that carry no strut inside this cube at all. OwnedStrutCount is the other half of the same rule: ' +
    'how many of the cell\'s struts an interior cell emits, the rest belonging to the neighbour that shares the ' +
    'face. The last cell is ReentrantHoneycomb at the slider\'s fold depth; ReentrantAuxetic is that at 0.25.',
  plato: [
    ...CELL_MEMBERS,
    'Number.ReentrantHoneycomb',
    'LatticeUnitCell.NodeValences',
    'LatticeUnitCell.NodeValence',
    'LatticeUnitCell.OwnedStrutCount',
    'LatticeUnitCell.NormalizedStrutLength',
    'LatticeUnitCell.StrutEndpointA',
    'LatticeUnitCell.StrutEndpointB',
    'StrutLattice3D.Struts',
    'StrutLattice3D.Nodes',
    'NumberInterval.ParameterOf',
    'Color.Lerp',
  ],
  viewer: WIDE_VIEW,
  controls: [
    { key: 'reentrancy', label: 'Auxetic fold', kind: 'slider', min: 0, max: 0.45, step: 0.01, def: 0.25 },
    { key: 'radius', label: 'Strut radius', kind: 'slider', min: 0.008, max: 0.05, step: 0.002, def: 0.022 },
    { key: 'nodes', label: 'Nodes', kind: 'toggle', def: 1 },
    { key: 'cage', label: 'Cell cube', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const slots = cellSlots(params.reentrancy);
    const object = new THREE.Group();
    const outline: number[] = [];

    // One pass to learn the valence range, so the ramp spans what is actually
    // there rather than a guessed interval.
    const valences = slots.map(slot => toArray(slot.cell.NodeValences()));
    const range = spanOf(valences.flat());

    const readings: Reading[] = [];
    let totalStruts = 0;
    let totalNodes = 0;

    slots.forEach((slot, index) => {
      const lattice = new StrutLattice3D(slot.cell, slot.bounds, ONE_CELL, params.radius);
      const segments = toArray(lattice.Struts());
      const points = toArray(lattice.Nodes());
      totalStruts += segments.length;
      totalNodes += points.length;

      object.add(strutTubes(segments, () => params.radius, palette.surface));
      if (params.nodes >= 0.5) {
        const tints = valences[index].map(v => rampColor(range, v));
        object.add(nodeSpheres(points, params.radius * 2.6, tints, 0xffffff));
      }
      if (params.cage >= 0.5) pushBoxEdges(outline, slot.bounds);

      const distinct = [...new Set(valences[index])].sort((a, b) => a - b);
      readings.push(
        reading(slot.label, () =>
          `${slot.cell.Nodes.Count()}n/${slot.cell.Struts.Count()}s owned ` +
          `${slot.cell.OwnedStrutCount()} valence ${distinct.join(',')}`,
        ),
      );
    });

    if (outline.length > 0) object.add(segmentsOf(outline, 0x2f3a4c, 0.8));

    const octet = LatticeUnitCell.OctetTruss();
    return {
      object,
      readings: [
        ...readings,
        note('drawn', `${totalStruts} struts, ${totalNodes} nodes`),
        note('valence range', `${range.Start} .. ${range.End}`),
        reading('OctetTruss NormalizedStrutLength', () =>
          n4(octet.NormalizedStrutLength(octet.Struts.At(0))),
        ),
        reading('SimpleCubic NodeValence(0)', () =>
          String(LatticeUnitCell.SimpleCubic().NodeValence(new ItemIndex(0))),
        ),
        reading('LatticeUnitCell.NodeTolerance', () => String(LatticeUnitCell.NodeTolerance())),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 2 — tiling one cell
//
// `UniformLattice` is the ergonomic constructor: it divides the LONGEST axis of
// the bounds and lets the other two follow that cell size, so the counts it
// derives are the reading that matters on a lopsided box. `Deformed` rides on
// the end because warping the tiled struts is how the library says "conform" —
// there is no second tiling path.

const WARP_LABELS = ['None', 'Twist', 'Taper', 'Bend', 'Spherify'];

const Y_AXIS = new Direction3D(new Vector3D(0, 1, 0));
const Z_AXIS = new Direction3D(new Vector3D(0, 0, 1));

/** The warp as a `Function1<Point3D, Point3D>`, from `deformations.types.plato`. */
function warpOf(index: number, strength: number): ((p: Point3D) => Point3D) | null {
  switch (clampIndex(index, WARP_LABELS.length)) {
    case 0:
      return null;
    case 1: {
      const twist = new Twist3D(ORIGIN, Y_AXIS, (strength * 0.45).Turns());
      return p => twist.Eval(p);
    }
    case 2: {
      const taper = new Taper3D(ORIGIN, Y_AXIS, strength * 0.8);
      return p => taper.Eval(p);
    }
    case 3: {
      const bend = new Bend3D(ORIGIN, Z_AXIS, Y_AXIS, strength * 0.9);
      return p => bend.Eval(p);
    }
    default: {
      const spherify = new Spherify3D(ORIGIN, 1.25, Math.abs(strength));
      return p => spherify.Eval(p);
    }
  }
}

const tiling = sceneOf({
  id: 'tiling',
  title: 'Tiling a cell, and welding it',
  description:
    'UniformLattice(cell, bounds, divisions, radius) divides the longest axis of the bounds `divisions` times ' +
    'and gives the other two the same cell size, so a lopsided envelope gets near-cubic cells and derived ' +
    'counts — CellSize and Counts below are what it worked out. It gets there through Number.CellDivisions, ' +
    'which is (extent / size).ToInteger.Max(1), and the ToInteger this target emits TRUNCATES while ' +
    'stdlib/CONVENTIONS.md says the conversion rounds; the reading below calls ToInteger(1.6) so the page ' +
    'reports which one is live rather than assuming. Where they differ a shorter axis comes out one cell short ' +
    'of the nearest-cubic answer, and the cell-aspect reading is where that surfaces — try the Slab and the ' +
    'Tower. Struts then tiles once and welds: a strut with ' +
    'both endpoints on a shared face belongs to the near-face copy, which is one predicate over one strut, no ' +
    'hash grid and no world-space tolerance. The welding shows up in the numbers — cells times OwnedStrutCount ' +
    'is what an infinite tiling would emit, and the finite lattice adds back exactly the far faces that have no ' +
    'next cell to own them. Deformed(struts, mapping) carries both endpoints of every welded strut through a ' +
    'point map: that is how a lattice is conformed to something other than its box, and how a graded CELL SIZE ' +
    'is expressed. Relative density is declared a first-order OVERESTIMATE — it counts material twice at every ' +
    'node — so it is honest only in the slender regime, which the radius slider can leave.',
  plato: [
    'LatticeUnitCell.UniformLattice',
    'Number.CellDivisions',
    'Number.ToInteger',
    'StrutLattice3D.CellSize',
    'StrutLattice3D.CellCount',
    'StrutLattice3D.Cells',
    'StrutLattice3D.Struts',
    'StrutLattice3D.Nodes',
    'StrutLattice3D.StrutCount',
    'StrutLattice3D.NodeCount',
    'StrutLattice3D.TotalStrutLength',
    'StrutLattice3D.RelativeDensity',
    'StrutLattice3D.OwnsStrut',
    'StrutLattice3D.ConformedTo',
    'StrutLattice3D.WithRadius',
    'LatticeUnitCell.OwnedStrutCount',
    'Array.TotalLength',
    'Array.RelativeDensity',
    'Array.Deformed',
    'Bounds3D.EnvelopeVolume',
    'Twist3D.Eval',
    'Taper3D.Eval',
    'Bend3D.Eval',
    'Spherify3D.Eval',
  ],
  viewer: SPATIAL_VIEW,
  controls: [
    cellControl(3),
    { key: 'envelope', label: 'Envelope', kind: 'select', options: ENVELOPE_LABELS, def: 0 },
    { key: 'divisions', label: 'Divisions', kind: 'slider', min: 1, max: 5, step: 1, def: 3 },
    { key: 'radius', label: 'Strut radius', kind: 'slider', min: 0.01, max: 0.09, step: 0.002, def: 0.03 },
    { key: 'warp', label: 'Deformed by', kind: 'select', options: WARP_LABELS, def: 0 },
    { key: 'strength', label: 'Warp strength', kind: 'slider', min: -1, max: 1, step: 0.02, def: 0.5 },
    { key: 'solid', label: 'Solid struts', kind: 'toggle', def: 1 },
    { key: 'box', label: 'Envelope box', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const cell = cellOf(params.cell);
    const envelope = envelopeOf(params.envelope);
    const divisions = Math.round(params.divisions);
    const lattice = cell.UniformLattice(envelope, divisions, params.radius);

    const tiled = struts(lattice.Struts());
    const warp = warpOf(params.warp, params.strength);
    const shown = warp === null ? tiled : struts((tiled.Deformed(warp)));
    const segments = toArray(shown);

    const object = new THREE.Group();
    if (params.solid >= 0.5) {
      object.add(strutTubes(segments, () => params.radius, palette.surface));
    } else {
      object.add(strutLines(segments, palette.line, 0.9));
    }
    if (params.box >= 0.5) {
      const outline: number[] = [];
      pushBoxEdges(outline, envelope);
      object.add(segmentsOf(outline, 0x2f3a4c));
    }

    const size = lattice.CellSize();
    const owned = cell.OwnedStrutCount();
    const cellCount = lattice.CellCount();
    // The counts `UniformLattice` derived, re-asked one axis at a time so the
    // member that derived them is named rather than implied.
    const extents = [
      envelope.Max.X - envelope.Min.X,
      envelope.Max.Y - envelope.Min.Y,
      envelope.Max.Z - envelope.Min.Z,
    ];
    const target = Math.max(...extents) / Math.max(1, divisions);
    return {
      object,
      readings: [
        note('cell', `${CELL_LABELS[clampIndex(params.cell, CELL_LABELS.length)]} — ${CELL_NOTES[clampIndex(params.cell, CELL_LABELS.length)]}`),
        reading('StrutLattice3D.Counts', () => iv(lattice.Counts)),
        note('target cell size', n3(target)),
        reading('Number.CellDivisions per axis', () =>
          extents.map(e => e.CellDivisions(target)).join(', '),
        ),
        reading('Number.ToInteger(1.6)', () => `${(1.6).ToInteger()} (2 rounds, 1 truncates)`),
        reading('StrutLattice3D.CellCount', () => String(cellCount)),
        reading('StrutLattice3D.CellSize', () => vec(size)),
        note('cell aspect', n3(Math.max(size.X, size.Y, size.Z) / Math.max(1e-9, Math.min(size.X, size.Y, size.Z)))),
        reading('LatticeUnitCell.OwnedStrutCount', () => String(owned)),
        note('cells x owned', String(cellCount * owned)),
        reading('StrutLattice3D.StrutCount', () => String(segments.length)),
        note('closed far faces', String(segments.length - cellCount * owned)),
        reading('StrutLattice3D.NodeCount', () => String(lattice.NodeCount())),
        reading('Array.TotalLength', () => n3(shown.TotalLength())),
        reading('Bounds3D.EnvelopeVolume', () => n3(envelope.EnvelopeVolume())),
        reading('Array.RelativeDensity (as drawn)', () =>
          pct(shown.RelativeDensity(params.radius, envelope)),
        ),
        reading('StrutLattice3D.RelativeDensity (undeformed)', () =>
          pct(lattice.RelativeDensity()),
        ),
        note('warp', WARP_LABELS[clampIndex(params.warp, WARP_LABELS.length)]),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 3 — grading the radius from a field
//
// `StrutRadii(struts, grading, range)` samples the field at the strut MIDPOINT,
// clamps it to [0, 1] and interpolates across the range. That is the operator
// with the most visual payoff in the library: one lattice, one field, and the
// struts get thick where the field is high. `StrutRadii(struts, field)` is the
// other overload, where the field value IS the radius in world units.

const FIELD_LABELS = ['Radial', 'Height', 'Perlin', 'Gyroid'];

interface Grading {
  /** The field the operator reads. */
  field: ScalarField;
  /** The generated members it is built from. */
  source: string;
}

/**
 * Each grading is a `ScalarFunctionField3D` over generated members: the raw
 * quantity comes from the library and `NumberInterval.ParameterOf` (an
 * `InverseLerp`) is what puts it on the 0..1 scale the range overload wants.
 */
function gradingOf(index: number, scale: number): Grading {
  switch (clampIndex(index, FIELD_LABELS.length)) {
    case 0: {
      const reach = new NumberInterval(0, 1.6 * scale);
      return {
        field: new ScalarFunctionField3D(p => reach.ParameterOf(p.Distance(ORIGIN))),
        source: 'Point3D.Distance, NumberInterval.ParameterOf',
      };
    }
    case 1: {
      const span = new NumberInterval(-1.1 * scale, 1.1 * scale);
      return {
        field: new ScalarFunctionField3D(p => span.ParameterOf(p.Y)),
        source: 'NumberInterval.ParameterOf',
      };
    }
    case 2: {
      const noise = new PerlinNoise3D(7, 0.9 / Math.max(0.2, scale));
      const signed = new NumberInterval(-0.8, 0.8);
      return {
        field: new ScalarFunctionField3D(p => signed.ParameterOf(noise.Eval(p))),
        source: 'PerlinNoise3D.Eval, NumberInterval.ParameterOf',
      };
    }
    default: {
      const gyroid = new TpmsField3D(prelude.TpmsFamily.Gyroid(), 1.1 * scale, 0);
      const signed = new NumberInterval(-1.5, 1.5);
      return {
        field: new ScalarFunctionField3D(p => signed.ParameterOf(gyroid.Eval(p))),
        source: 'TpmsField3D.Eval, NumberInterval.ParameterOf',
      };
    }
  }
}

const grading = sceneOf({
  id: 'grading',
  title: 'Grading the radius from a field',
  description:
    'One lattice, one scalar field, and StrutRadii(struts, grading, range): the field is sampled at each ' +
    'strut\'s midpoint, clamped to [0, 1], and used to interpolate between the ends of the range. Nothing ' +
    'rescales anything — that clamp is the whole of the normalization, which is why each field below is put on ' +
    'the unit scale by NumberInterval.ParameterOf first. The sibling overload StrutRadii(struts, field) skips ' +
    'the range and takes the field value AS the radius in world units; its reading is below for comparison. ' +
    'The payoff is the graded relative density: RelativeDensity(struts, radii, envelope) takes one radius per ' +
    'strut and is the number a graded lattice is actually specified by, and it is NOT the uniform density at ' +
    'the mean radius, because density goes with the square.',
  plato: [
    'Array.StrutRadii',
    'Array.RelativeDensity',
    'Array.TotalLength',
    'Line3D.Centroid',
    'ScalarFunctionField3D.Eval',
    'PerlinNoise3D.Eval',
    'TpmsField3D.Eval',
    'NumberInterval.ParameterOf',
    'NumberInterval.Lerp',
    'Point3D.Distance',
    'LatticeUnitCell.UniformLattice',
    'StrutLattice3D.Struts',
  ],
  viewer: SPATIAL_VIEW,
  controls: [
    cellControl(3),
    { key: 'divisions', label: 'Divisions', kind: 'slider', min: 1, max: 4, step: 1, def: 3 },
    { key: 'field', label: 'Grading field', kind: 'select', options: FIELD_LABELS, def: 0 },
    { key: 'scale', label: 'Field scale', kind: 'slider', min: 0.4, max: 2, step: 0.05, def: 1 },
    { key: 'thin', label: 'Range start', kind: 'slider', min: 0.004, max: 0.05, step: 0.002, def: 0.01 },
    { key: 'thick', label: 'Range end', kind: 'slider', min: 0.01, max: 0.11, step: 0.002, def: 0.07 },
    { key: 'tint', label: 'Tint by radius', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const cell = cellOf(params.cell);
    const divisions = Math.round(params.divisions);
    const lattice = cell.UniformLattice(CUBE, divisions, params.thin);
    const tiled = struts(lattice.Struts());
    const segments = toArray(tiled);

    const grade = gradingOf(params.field, params.scale);
    const range = new NumberInterval(params.thin, params.thick);
    const radii = (tiled.StrutRadii(grade.field, range));
    const values = toArray(radii);
    const observed = spanOf(values);
    const mean = values.reduce((acc, r) => acc + r, 0) / Math.max(1, values.length);

    const object = new THREE.Group();
    const tints =
      params.tint >= 0.5 ? values.map(r => rampColor(observed, r)) : undefined;
    object.add(
      strutTubes(segments, i => values[i], tints ? 0xffffff : palette.surface, tints),
    );
    const outline: number[] = [];
    pushBoxEdges(outline, CUBE);
    object.add(segmentsOf(outline, 0x2f3a4c));

    return {
      object,
      readings: [
        note('field', `${FIELD_LABELS[clampIndex(params.field, FIELD_LABELS.length)]} via ${grade.source}`),
        reading('StrutLattice3D.StrutCount', () => String(segments.length)),
        note('range asked', `${n4(range.Start)} .. ${n4(range.End)}`),
        reading('StrutRadii(field, range) span', () =>
          `${n4(observed.Start)} .. ${n4(observed.End)}`,
        ),
        note('mean radius', n4(mean)),
        reading('StrutRadii(field) at strut 0 — value IS the radius', () =>
          n4(tiled.StrutRadii(grade.field).At(0)),
        ),
        reading('Line3D.Centroid at strut 0', () => {
          const c = segments[0].Centroid();
          return `(${n2(c.X)}, ${n2(c.Y)}, ${n2(c.Z)})`;
        }),
        reading('Array.TotalLength', () => n3(tiled.TotalLength())),
        reading('RelativeDensity(radii)', () => pct(tiled.RelativeDensity(radii, CUBE))),
        reading('RelativeDensity(mean radius)', () => pct(tiled.RelativeDensity(mean, CUBE))),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 4 — trimming to a solid
//
// The three `Trimmed` overloads are one idea: keep a strut whose MIDPOINT lies
// inside the region. Nothing is clipped — clipping would need root finding and
// would break the welding at the cut — so a strut straddling the boundary
// survives or vanishes entire, and the trimmed lattice is still welded.

const TRIM_LABELS = ['Box', 'Ball', 'Rod', 'Volume', 'Gyroid'];

interface Trim {
  region: TrimRegion;
  /** Which overload of `Trimmed` this reaches. */
  overload: string;
  /** The generated members it is built from. */
  source: string;
  /** Drawn beside the struts, when there is something to draw. */
  outline?: Bounds3D;
}

function trimOf(index: number, size: number): Trim {
  switch (clampIndex(index, TRIM_LABELS.length)) {
    case 0: {
      const box = new Bounds3D(ORIGIN, ORIGIN).Expand(size);
      return {
        region: box,
        overload: 'Trimmed(struts, region: Bounds3D)',
        source: 'Bounds3D.Expand, Bounds3D.Contains',
        outline: box,
      };
    }
    case 1:
      return {
        region: new Sphere(ORIGIN, size).ToSdf(),
        overload: 'Trimmed(struts, solid: ISignedDistanceField3D)',
        source: 'Sphere.ToSdf, FunctionSdf3D.Contains',
      };
    case 2:
      return {
        region: new Capsule3D(
          new Point3D(-0.8, -0.5, -0.3),
          new Point3D(0.8, 0.55, 0.3),
          size * 0.75,
        ).ToSdf(),
        overload: 'Trimmed(struts, solid: ISignedDistanceField3D)',
        source: 'Capsule3D.ToSdf, FunctionSdf3D.Contains',
      };
    case 3:
      return {
        region: new Sphere(ORIGIN, size).ToSdf().ToVolume(),
        overload: 'Trimmed(struts, volume: IImplicitVolume3D)',
        source: 'Sphere.ToSdf, FunctionSdf3D.ToVolume, FunctionVolume3D.Contains',
      };
    default:
      return {
        region: new TpmsNetwork3D(prelude.TpmsFamily.Gyroid(), size * 1.6, 0),
        overload: 'Trimmed(struts, solid: ISignedDistanceField3D)',
        source: 'TpmsNetwork3D.Eval, TpmsNetwork3D.Contains',
      };
  }
}

const trimming = sceneOf({
  id: 'trimming',
  title: 'Trimming to a solid',
  description:
    'Trimmed keeps a strut whose MIDPOINT lies inside the region and drops the rest — whole struts, never ' +
    'clipped, because clipping would need root finding against the trimming solid and would break the welding ' +
    'at the cut. All five choices here go through the same one-line body: a Bounds3D answers Contains, an SDF ' +
    'answers Contains, an implicit volume answers Contains, and any signed distance field in the tree is ' +
    'therefore a cookie cutter — including a gyroid labyrinth, which trims a strut lattice to the inside of a ' +
    'triply periodic surface. The dropped struts stay drawn as faint lines so the midpoint rule is visible: a ' +
    'strut lying half outside is kept whole, and one lying half inside is lost whole.',
  plato: [
    'Array.Trimmed',
    'Array.TotalLength',
    'Array.RelativeDensity',
    'Line3D.Centroid',
    'Bounds3D.Contains',
    'Bounds3D.Expand',
    'Sphere.ToSdf',
    'Capsule3D.ToSdf',
    'FunctionSdf3D.Contains',
    'FunctionSdf3D.ToVolume',
    'FunctionVolume3D.Contains',
    'TpmsNetwork3D.Contains',
    'LatticeUnitCell.UniformLattice',
    'StrutLattice3D.Struts',
  ],
  viewer: SPATIAL_VIEW,
  controls: [
    cellControl(3),
    { key: 'divisions', label: 'Divisions', kind: 'slider', min: 2, max: 5, step: 1, def: 4 },
    { key: 'solid', label: 'Trim to', kind: 'select', options: TRIM_LABELS, def: 1 },
    { key: 'size', label: 'Solid size', kind: 'slider', min: 0.2, max: 1.3, step: 0.02, def: 0.72 },
    { key: 'radius', label: 'Strut radius', kind: 'slider', min: 0.01, max: 0.07, step: 0.002, def: 0.026 },
    { key: 'dropped', label: 'Show dropped', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const cell = cellOf(params.cell);
    const divisions = Math.round(params.divisions);
    const lattice = cell.UniformLattice(CUBE, divisions, params.radius);
    const tiled = struts(lattice.Struts());
    const all = toArray(tiled);

    const trim = trimOf(params.solid, params.size);
    const kept = struts(tiled.Trimmed(trim.region));
    const keptSegments = toArray(kept);

    const object = new THREE.Group();
    object.add(strutTubes(keptSegments, () => params.radius, palette.surface));
    if (params.dropped >= 0.5) {
      // The complement, by the same predicate the library used, so the two
      // pictures always agree about which strut went where.
      const lost = all.filter(s => !trim.region.Contains(s.Centroid()));
      if (lost.length > 0) object.add(strutLines(lost, 0x3c4759, 0.75));
    }
    const outline: number[] = [];
    pushBoxEdges(outline, CUBE);
    if (trim.outline) pushBoxEdges(outline, trim.outline);
    object.add(segmentsOf(outline, 0x2f3a4c));

    return {
      object,
      readings: [
        note('overload', trim.overload),
        note('region', trim.source),
        reading('struts before', () => String(all.length)),
        reading('Array.Trimmed count', () => String(keptSegments.length)),
        note('kept', pct(keptSegments.length / Math.max(1, all.length))),
        reading('TotalLength before', () => n3(tiled.TotalLength())),
        reading('TotalLength after', () => n3(kept.TotalLength())),
        reading('RelativeDensity before', () => pct(tiled.RelativeDensity(params.radius, CUBE))),
        reading('RelativeDensity after', () => pct(kept.RelativeDensity(params.radius, CUBE))),
        note('clipping', 'NONE — whole struts, by Line3D.Centroid'),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 5 — struts to a surface
//
// `ToSdf(struts, radius)` is a union of capsules and an EXACT signed distance
// field, so the marching cubes already in the tree consumes a lattice with no
// new plumbing. It is also linear in strut count with no acceleration structure,
// which the type comment states and this scene measures: the sampling pass below
// is `struts * nodes` capsule distances, and the direct call multiplies that by
// eight because the marcher reads every node once per cell that touches it.

interface Sampled {
  values: Float64Array;
  resolution: number;
  milliseconds: number;
}

const sampleCache = new Map<string, Sampled>();

/**
 * The ceiling on `field samples x struts`, in capsule distances — around a
 * second of `StrutSdf3D.Eval` at the rate measured here. Enough to let the
 * direct route be FELT without letting an untrimmed lattice at the top of both
 * sliders lock the page for a minute, which is what the unbudgeted version did.
 * The scene reports the cap when it bites rather than hiding it, because a
 * budget on `struts x samples` is precisely what the type comment's "linear in
 * strut count, with no acceleration structure" means in practice.
 */
const SAMPLE_BUDGET = 1_500_000;

/** How many capsule distances one extraction costs, per route. */
const sampleCount = (resolution: number, direct: boolean): number =>
  direct ? 8 * Math.max(0, resolution - 1) ** 3 : resolution ** 3;

/** The largest node count that fits the budget; six is the floor worth drawing. */
function affordable(resolution: number, strutCount: number, direct: boolean): number {
  let n = resolution;
  while (n > 6 && sampleCount(n, direct) * strutCount > SAMPLE_BUDGET) n -= 2;
  return n;
}

/**
 * The lattice solid evaluated on a node lattice, once. `Bounds3D.LatticeNodePosition`
 * is the same member `SampledSdf3D.NodePosition` is written against, so the grid
 * and the marcher agree by construction.
 */
function sampledOf(key: string, sdf: StrutSdf3D, resolution: number): Sampled {
  const hit = sampleCache.get(key);
  if (hit) return hit;
  const started = performance.now();
  const counts = new IntegerVector3(resolution, resolution, resolution);
  const values = new Float64Array(resolution ** 3);
  for (let k = 0; k < resolution; k++) {
    for (let j = 0; j < resolution; j++) {
      for (let i = 0; i < resolution; i++) {
        values[(k * resolution + j) * resolution + i] = sdf.Eval(
          CUBE.LatticeNodePosition(counts, i, j, k),
        );
      }
    }
  }
  const sampled: Sampled = { values, resolution, milliseconds: performance.now() - started };
  if (sampleCache.size > 3) sampleCache.clear();
  sampleCache.set(key, sampled);
  return sampled;
}

const volumeOf = (size: number, at: (i: number, j: number, k: number) => number): IArray3D<number> =>
  size.MakeArray3D(size, size, at);

const surface = sceneOf({
  id: 'surface',
  title: 'Struts to a surface',
  description:
    'ToSdf(struts, radius) sweeps a welded strut list into a StrutSdf3D — the union of capsules over ' +
    'DistanceToCapsule, which is an EXACT signed distance field, so the marching cubes and sphere tracing ' +
    'already in the tree consume a lattice unchanged. It is also a Reduce over every strut per sample with no ' +
    'acceleration structure, which the type comment says outright and this scene charges for: the field is ' +
    'sampled onto a node lattice ONCE and handed to SampledSdf3D.MarchingCubes, because the marcher reads each ' +
    'node about eight times and an unmaterialized field would pay for all of them. Turning on the direct call ' +
    'runs StrutSdf3D.MarchingCubes(bounds, nodeCounts) instead and prints what that costs. Trimming first is ' +
    'the whole discipline — the strut count is the multiplier on every sample — so this scene holds a budget ' +
    'on struts times samples and drops the node count until the extraction fits it, saying so when it does. ' +
    'Widen the trim radius with the direct call on and watch the budget take the resolution away. ' +
    'ToSdf(struts, radii) is the graded twin and gives a GradedStrutSdf3D, which is what the graded toggle ' +
    'extracts.',
  plato: [
    'Array.ToSdf',
    'StrutSdf3D.Eval',
    'StrutSdf3D.MarchingCubes',
    'GradedStrutSdf3D.Eval',
    'StrutSdf3D.Contains',
    'Point3D.DistanceToCapsule',
    'Bounds3D.LatticeNodePosition',
    'SampledSdf3D.MarchingCubes',
    'SampledSdf3D.NodeCounts',
    'IntegerVector3.MarchingCubesLattice',
    'Array.Trimmed',
    'Array.StrutRadii',
    'Sphere.ToSdf',
    'LatticeUnitCell.UniformLattice',
  ],
  viewer: SPATIAL_VIEW,
  controls: [
    cellControl(3),
    { key: 'divisions', label: 'Divisions', kind: 'slider', min: 1, max: 3, step: 1, def: 2 },
    { key: 'trim', label: 'Trim radius', kind: 'slider', min: 0.4, max: 1.4, step: 0.05, def: 0.85 },
    { key: 'radius', label: 'Strut radius', kind: 'slider', min: 0.03, max: 0.12, step: 0.005, def: 0.07 },
    { key: 'resolution', label: 'Nodes per axis', kind: 'slider', min: 10, max: 22, step: 2, def: 18 },
    { key: 'graded', label: 'Graded radii', kind: 'toggle', def: 0 },
    { key: 'direct', label: 'Direct MarchingCubes', kind: 'toggle', def: 0 },
    { key: 'wire', label: 'Struts too', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const cell = cellOf(params.cell);
    const divisions = Math.round(params.divisions);
    const asked = Math.round(params.resolution);
    const isDirect = params.direct >= 0.5;
    const lattice = cell.UniformLattice(CUBE, divisions, params.radius);
    const tiled = struts(lattice.Struts());
    const before = tiled.Count();
    const kept = struts(tiled.Trimmed(new Sphere(ORIGIN, params.trim).ToSdf()));
    const segments = toArray(kept);
    const resolution = affordable(asked, segments.length, isDirect);

    // The graded solid is the other ToSdf overload: one radius per strut, from a
    // radial grading, so the lattice thins towards the outside of the trim ball.
    const reach = new NumberInterval(params.trim, 0);
    const radii = kept.StrutRadii(
      new ScalarFunctionField3D(p => reach.ParameterOf(p.Distance(ORIGIN))),
      new NumberInterval(params.radius * 0.45, params.radius),
    );
    const gradedRadii = toArray(radii);
    const isGraded = params.graded >= 0.5;

    const uniformSdf = kept.ToSdf(params.radius);
    const gradedSdf = kept.ToSdf(radii);
    const sdf = (isGraded ? gradedSdf : uniformSdf) as StrutSdf3D;

    const object = new THREE.Group();
    const key = [
      clampIndex(params.cell, CELL_LABELS.length),
      divisions,
      params.trim.toFixed(2),
      params.radius.toFixed(3),
      resolution,
      isGraded ? 'g' : 'u',
    ].join(':');

    let triangles = 0;
    let milliseconds = 0;
    let route = '';
    const extracted = reading(
      isDirect ? 'StrutSdf3D.MarchingCubes' : 'SampledSdf3D.MarchingCubes',
      () => {
        if (isDirect) {
          // The member straight through: nodes are re-evaluated per cell corner,
          // which is where the eight-fold factor in the cost note comes from.
          const started = performance.now();
          const marched = sdf.MarchingCubes(CUBE, new IntegerVector3(resolution, resolution, resolution));
          triangles = marched.Triangles.Count();
          milliseconds = performance.now() - started;
          route = 'direct';
          if (triangles > 0) {
            object.add(isosurface(triangleArrayGeometry(marched), palette.accent, 0.95));
          }
          return `${triangles} triangles in ${milliseconds.toFixed(0)} ms`;
        }
        const sampled = sampledOf(key, sdf, resolution);
        const grid = new SampledSdf3D(
          volumeOf(resolution, (i, j, k) =>
            sampled.values[(k * resolution + j) * resolution + i],
          ),
          CUBE,
        );
        const started = performance.now();
        const marched = grid.MarchingCubes();
        triangles = marched.Triangles.Count();
        milliseconds = performance.now() - started;
        route = 'sampled';
        if (triangles > 0) {
          object.add(isosurface(triangleArrayGeometry(marched), palette.accent, 0.95));
        }
        return `${triangles} triangles, sample ${sampled.milliseconds.toFixed(0)} ms + march ${milliseconds.toFixed(0)} ms`;
      },
    );

    if (params.wire >= 0.5) object.add(strutLines(segments, palette.line, 0.35));
    const outline: number[] = [];
    pushBoxEdges(outline, CUBE);
    object.add(segmentsOf(outline, 0x2f3a4c));

    const samples = sampleCount(resolution, isDirect);
    const cost = samples * segments.length;
    const radiiSpan = spanOf(gradedRadii);
    return {
      object,
      readings: [
        note('route', route === 'direct' ? 'StrutSdf3D.MarchingCubes, straight' : 'sampled once, then SampledSdf3D.MarchingCubes'),
        reading('struts tiled', () => String(before)),
        reading('Array.Trimmed count', () => String(segments.length)),
        note('solid', isGraded ? 'GradedStrutSdf3D' : 'StrutSdf3D'),
        note('radii', isGraded ? `${n4(radiiSpan.Start)} .. ${n4(radiiSpan.End)}` : n4(params.radius)),
        note(
          'lattice',
          resolution === asked
            ? `${resolution} x ${resolution} x ${resolution}`
            : `${resolution} cubed — ASKED ${asked}, cut to fit the sample budget`,
        ),
        note(
          'capsule distances',
          `${samples} samples x ${segments.length} struts = ${(cost / 1e6).toFixed(2)}M ` +
            `(budget ${(SAMPLE_BUDGET / 1e6).toFixed(1)}M)`,
        ),
        extracted,
        reading('StrutSdf3D.Eval at centre', () => n4(uniformSdf.Eval(ORIGIN))),
        reading('GradedStrutSdf3D.Eval at centre', () => n4(gradedSdf.Eval(ORIGIN))),
        reading('StrutSdf3D.Contains(centre)', () => String(uniformSdf.Contains(ORIGIN))),
        note('acceleration structure', 'NONE — Eval is linear in strut count'),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 6 — triply periodic minimal surfaces
//
// The implicit half of the library. `TpmsField3D` is a nodal implicit and is NOT
// a distance; `TpmsNetwork3D` and `TpmsSheet3D` divide it by a per-family
// Lipschitz bound and are honest LOWER bounds.
//
// Level and thickness are therefore given here as FRACTIONS, because their world
// scale is the family's: the bound is `TpmsPartialBound * sqrt(3) * Tau / period`,
// which for Neovius is seven times Schwarz P's, so one world thickness that
// makes a wall for one family makes an empty solid for another. The largest
// nodal magnitude of a family is measured once per (family, period) — sampled
// from `TpmsField3D.Eval` itself, not tabulated here — and the two sliders are
// read against it.

const TPMS_LABELS = ['Gyroid', 'Schwarz P', 'Schwarz D', 'Neovius', 'I-WP'];
const TPMS_CASES = ['Gyroid', 'SchwarzPrimitive', 'SchwarzDiamond', 'Neovius', 'IwpSurface'];
const TPMS_SOLIDS = ['Sheet', 'Network', 'Surface'];

const familyOf = (index: number): TpmsCase =>
  prelude.TpmsFamily[TPMS_CASES[clampIndex(index, TPMS_CASES.length)]]();

const nodalCache = new Map<string, number>();

/** The largest |nodal value| a family reaches, sampled over exactly one period. */
function maxNodalOf(family: TpmsCase, period: number): number {
  const key = `${family.Tag}:${period.toFixed(3)}`;
  const hit = nodalCache.get(key);
  if (hit !== undefined) return hit;
  const field = new TpmsField3D(family, period, 0);
  const steps = 13;
  let max = 0;
  for (let k = 0; k < steps; k++) {
    for (let j = 0; j < steps; j++) {
      for (let i = 0; i < steps; i++) {
        const p = new Point3D(
          (period * i) / (steps - 1),
          (period * j) / (steps - 1),
          (period * k) / (steps - 1),
        );
        const v = Math.abs(field.Eval(p));
        if (v > max) max = v;
      }
    }
  }
  if (nodalCache.size > 12) nodalCache.clear();
  nodalCache.set(key, max);
  return max;
}

const tpms = sceneOf({
  id: 'tpms',
  title: 'Triply periodic minimal surfaces',
  description:
    'The implicit counterpart of a strut lattice: five nodal approximations that repeat in all three axes and ' +
    'divide space into two interpenetrating labyrinths. TpmsField3D is the raw implicit and implements ' +
    'IScalarField3D only — a nodal implicit is not a distance, and the library refuses to claim it is. The two ' +
    'solids divide it by a per-family Lipschitz bound and so implement ISignedDistanceField3D as an honest ' +
    'LOWER bound: TpmsNetwork3D is one labyrinth, TpmsSheet3D is a wall of finite thickness straddling the ' +
    'surface, and the sheet is what "gyroid infill" means. Because the bound is the family\'s own — ' +
    'TpmsPartialBound times root three times the angular frequency, so Neovius\'s is seven times Schwarz P\'s ' +
    '— a world thickness is not portable between families, and the two sliders here are fractions of the ' +
    'measured nodal range instead. The world Thickness they produce is in the readings, next to the bound that ' +
    'set the scale.',
  plato: [
    'TpmsField3D.Eval',
    'TpmsField3D.MarchingCubes',
    'TpmsNetwork3D.Eval',
    'TpmsNetwork3D.MarchingCubes',
    'TpmsNetwork3D.Contains',
    'TpmsSheet3D.Eval',
    'TpmsSheet3D.MarchingCubes',
    'TpmsFamily.TpmsNodalValue',
    'TpmsFamily.TpmsPartialBound',
    'TpmsFamily.TpmsGradientBound',
    'TpmsFamily.TpmsNormalizedValue',
    'Number.TpmsFrequency',
    'Bounds3D.LatticeNodePosition',
    'IntegerVector3.MarchingCubesLattice',
  ],
  viewer: SPATIAL_VIEW,
  controls: [
    { key: 'family', label: 'Family', kind: 'select', options: TPMS_LABELS, def: 0 },
    { key: 'solid', label: 'Solid', kind: 'select', options: TPMS_SOLIDS, def: 0 },
    { key: 'period', label: 'Period', kind: 'slider', min: 0.5, max: 2, step: 0.05, def: 1 },
    { key: 'level', label: 'Level (of nodal max)', kind: 'slider', min: -0.7, max: 0.7, step: 0.02, def: 0 },
    { key: 'wall', label: 'Wall (of nodal max)', kind: 'slider', min: 0.05, max: 0.8, step: 0.01, def: 0.3 },
    { key: 'resolution', label: 'Nodes per axis', kind: 'slider', min: 20, max: 40, step: 4, def: 32 },
  ],
  build(params: Params): Built {
    const family = familyOf(params.family);
    const period = params.period;
    const resolution = Math.round(params.resolution);
    const counts = new IntegerVector3(resolution, resolution, resolution);

    const maxNodal = maxNodalOf(family, period);
    const bound = family.TpmsGradientBound(period);
    // The widest wall the normalized field can describe, which is what makes a
    // fraction portable across families whose bounds differ sevenfold.
    const reach = maxNodal / bound;
    const level = params.level * maxNodal;
    const thickness = params.wall * 2 * reach;

    const field = new TpmsField3D(family, period, level);
    const network = new TpmsNetwork3D(family, period, level);
    const sheet = new TpmsSheet3D(family, period, level, thickness);
    const mode = clampIndex(params.solid, TPMS_SOLIDS.length);

    const object = new THREE.Group();
    let triangles = 0;
    const extracted = reading(
      ['TpmsSheet3D.MarchingCubes', 'TpmsNetwork3D.MarchingCubes', 'TpmsField3D.MarchingCubes'][mode],
      () => {
        // The field's marcher takes an iso level; the two solids' takes none,
        // because a signed distance field's surface is its zero set.
        const marched =
          mode === 0
            ? sheet.MarchingCubes(CUBE, counts)
            : mode === 1
              ? network.MarchingCubes(CUBE, counts)
              : field.MarchingCubes(CUBE, counts, 0);
        triangles = marched.Triangles.Count();
        if (triangles > 0) {
          object.add(
            isosurface(triangleArrayGeometry(marched), palette.surface, mode === 2 ? 0.9 : 1),
          );
        }
        return triangles > 0
          ? `${triangles} triangles`
          : 'EMPTY — no sign change on the lattice';
      },
    );

    const outline: number[] = [];
    pushBoxEdges(outline, CUBE);
    object.add(segmentsOf(outline, 0x2f3a4c));

    const probe = new Point3D(0.13, 0.27, 0.41);
    return {
      object,
      readings: [
        note('family', `${TPMS_LABELS[clampIndex(params.family, TPMS_LABELS.length)]} (${family.Tag})`),
        note('solid', TPMS_SOLIDS[mode]),
        note('period', n2(period)),
        reading('Number.TpmsFrequency', () => n3(period.TpmsFrequency())),
        reading('TpmsPartialBound', () => n2(family.TpmsPartialBound())),
        reading('TpmsGradientBound', () => n3(bound)),
        note('max |nodal| (sampled)', n3(maxNodal)),
        note('widest half-wall', n4(reach)),
        note('Level', n3(level)),
        note('Thickness', n4(thickness)),
        extracted,
        reading('TpmsNodalValue(probe)', () => n4(family.TpmsNodalValue(period, probe))),
        reading('TpmsNormalizedValue(probe)', () =>
          n4(family.TpmsNormalizedValue(period, level, probe)),
        ),
        reading('TpmsField3D.Eval(probe)', () => n4(field.Eval(probe))),
        reading('TpmsNetwork3D.Eval(probe)', () => n4(network.Eval(probe))),
        reading('TpmsSheet3D.Eval(probe)', () => n4(sheet.Eval(probe))),
        reading('TpmsNetwork3D.Contains(probe)', () => String(network.Contains(probe))),
        note('fidelity', 'LOWER BOUND — nodal implicit over a Lipschitz bound, not exact'),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 7 — the re-entrant cell
//
// `ReentrantHoneycomb(reentrancy)` is the one parameterized cell: each of the
// twelve cube edges becomes a two-segment rib folded inward at its midpoint.
// The slider sweeps the family, and the readings are what the source claims
// about it — corner nodes at coordination 24, rib nodes at 2 (the hinge, which
// is the mechanism and not a defect), and a fold of 0 collapsing the cell back
// onto simple cubic with every edge split in two.

const auxetic = sceneOf({
  id: 'auxetic',
  title: 'The re-entrant cell, swept',
  description:
    'ReentrantHoneycomb(reentrancy) is the only unit cell the library parameterizes, because re-entrancy is a ' +
    'family rather than a shape: each of the twelve cube edges becomes a two-segment rib that folds INWARD by ' +
    'the fold depth at its midpoint, and stretching the cell unfolds those ribs and pushes the other axes out ' +
    '— the negative Poisson ratio. Sweep the slider and watch the readings, not just the picture. At 0 the ' +
    'cell collapses onto SimpleCubic with each edge split in two and the corner valence drops from 24 to 6; ' +
    'anywhere above it the corners are 24-valent and the twelve rib nodes are 2-valent hinges, which is the ' +
    'mechanism. Values well below 0.5 are the useful ones, where opposite ribs would otherwise meet. The ' +
    'library describes the geometry and the coordination; it does not simulate the auxetic response, so no ' +
    'number here is a Poisson ratio.',
  plato: [
    'Number.ReentrantHoneycomb',
    'LatticeUnitCell.ReentrantAuxetic',
    'LatticeUnitCell.NodeValences',
    'LatticeUnitCell.NormalizedStrutLength',
    'LatticeUnitCell.OwnedStrutCount',
    'LatticeUnitCell.UniformLattice',
    'StrutLattice3D.Struts',
    'StrutLattice3D.Nodes',
    'StrutLattice3D.StrutCount',
    'StrutLattice3D.TotalStrutLength',
    'StrutLattice3D.RelativeDensity',
    'Array.TotalLength',
    'NumberInterval.ParameterOf',
    'Color.Lerp',
  ],
  viewer: STILL_VIEW,
  controls: [
    { key: 'reentrancy', label: 'Fold depth', kind: 'slider', min: 0, max: 0.45, step: 0.005, def: 0.25 },
    { key: 'divisions', label: 'Divisions', kind: 'slider', min: 1, max: 4, step: 1, def: 2 },
    { key: 'radius', label: 'Strut radius', kind: 'slider', min: 0.008, max: 0.05, step: 0.002, def: 0.02 },
    { key: 'nodes', label: 'Nodes by valence', kind: 'toggle', def: 1 },
    { key: 'ghost', label: 'Simple cubic ghost', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const cell = params.reentrancy.ReentrantHoneycomb();
    const divisions = Math.round(params.divisions);
    const lattice = cell.UniformLattice(CUBE, divisions, params.radius);
    const tiled = struts(lattice.Struts());
    const segments = toArray(tiled);
    const points = toArray(lattice.Nodes());

    const object = new THREE.Group();
    object.add(strutTubes(segments, () => params.radius, palette.surface));

    const valences = toArray(cell.NodeValences());
    const range = spanOf(valences);
    if (params.nodes >= 0.5) {
      // The tiled node list is a subset of the cell's, in the cell's own order
      // per cell, so a node's valence is recovered by its position in the cell.
      const perCell = cell.Nodes.Count();
      const grid = lattice.Cells();
      const owned: number[] = [];
      for (let c = 0; c < grid.Count(); c++) {
        for (let n = 0; n < perCell; n++) {
          if (lattice.OwnsNode(grid.At(c), cell.Nodes.At(n))) owned.push(valences[n]);
        }
      }
      const tints = points.map((_p, i) => rampColor(range, owned[i] ?? range.Start));
      object.add(nodeSpheres(points, params.radius * 2.2, tints, 0xffffff));
    }

    if (params.ghost >= 0.5) {
      const ghost = LatticeUnitCell.SimpleCubic().UniformLattice(CUBE, divisions, params.radius);
      object.add(strutLines(toArray(ghost.Struts()), 0x3c4759, 0.6));
    }

    const lengths = toArray(cell.Struts).map(s => cell.NormalizedStrutLength(s));
    const lengthSpan = spanOf(lengths);
    return {
      object,
      readings: [
        note('fold depth', n3(params.reentrancy)),
        note(
          'spelling',
          Math.abs(params.reentrancy - 0.25) < 1e-9
            ? 'LatticeUnitCell.ReentrantAuxetic (the named constant)'
            : `Number.ReentrantHoneycomb(${n3(params.reentrancy)})`,
        ),
        reading('cell', () => `${cell.Nodes.Count()} nodes, ${cell.Struts.Count()} struts`),
        reading('LatticeUnitCell.OwnedStrutCount', () => String(cell.OwnedStrutCount())),
        reading('NodeValences', () => {
          const distinct = [...new Set(valences)].sort((a, b) => a - b);
          return distinct.map(v => `${v}x${valences.filter(x => x === v).length}`).join(' ');
        }),
        reading('NormalizedStrutLength span', () =>
          `${n4(lengthSpan.Start)} .. ${n4(lengthSpan.End)}`,
        ),
        reading('StrutLattice3D.Counts', () => iv(lattice.Counts)),
        reading('StrutLattice3D.StrutCount', () => String(segments.length)),
        reading('StrutLattice3D.NodeCount', () => String(points.length)),
        reading('StrutLattice3D.TotalStrutLength', () => n3(lattice.TotalStrutLength())),
        reading('StrutLattice3D.RelativeDensity', () => pct(lattice.RelativeDensity())),
        note('Poisson ratio', 'NOT COMPUTED — the library describes the cell, not its response'),
      ],
    };
  },
});

const demo: Demo = {
  title: 'Lattices',
  subtitle: 'lattices.{types,library}.plato',
  scenes: [cells, tiling, grading, trimming, surface, tpms, auxetic],
};

mountDemo(demo);

export { demo };
