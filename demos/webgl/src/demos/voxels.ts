// Voxels — a scene catalog over `stdlib/geometry/voxels.{types,library}.plato`,
// `pointclouds-voxels.concepts.plato`, the regular grids and cells of
// `sampling.{types,library}.plato`, and the octrees of
// `spatial-structures.{types,library}.plato`.
//
// What the voxel library actually is, because it shapes every scene below:
// `voxels.types.plato` declares ten grid types and `voxels.library.plato` gives
// bodies to FOUR members of them — `CellCenter` on `DensityGrid3D` and
// `LevelSetGrid3D`, `NodePosition` on `SampledSdf3D`, and the marching-cubes
// entry points. Everything else in that file is the marching-cubes kernel.
// `VoxelGrid3D`, `OccupancyGrid3D`, `SparseVoxelGrid3D`, `VoxelBrick3D`,
// `BrickMap3D`, `VoxelColorGrid3D`, `DensityGrid2D` and `OccupancyGrid2D` are
// data declarations with no library bodies at all.
//
// **There is no voxelizer.** `pointclouds-voxels.concepts.plato` declares
// `IVoxel3D.ToOccupancyGrid(cellSize)` and `meshes.library.plato` writes
// `VoxelCellCount` / `VoxelOccupiedCount` against it, but no type in the stdlib
// implements the interface, so nothing can produce an occupancy grid from a
// solid. Every scene here therefore fills its grid by evaluating the SUBJECT's
// own generated field at cell positions the LIBRARY computes: the sampling loop
// is the demo's, the occupancy test (`FunctionSdf3D.Contains`), the depth
// (`DepthBelowSurfaceAt`), the cell position (`Point3D.CellCenter`,
// `Bounds3D.LatticeNodePosition`) and the readback (`MarchingCubes`) are the
// library's. No addressing arithmetic is re-derived here.
//
// Cost. A grid is cubic and `build` runs on every slider tick, so: the fill is
// memoized per (subject, shape, resolution) and read out into a `Float64Array`
// once — an `Array3D` built by `MakeArray3D` is LAZY, and marching cubes reads
// every node about eight times, so an unmaterialized volume re-evaluates the
// field tens of thousands of times per tick. Occupied cells are drawn as ONE
// `THREE.InstancedMesh`, never one mesh per cell. Resolution caps are set from
// the measured cost of the fill plus the march.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { triangleArrayGeometry } from '../shared/mesh.js';
import { palette, surfaceMaterial, type ViewerOptions } from '../shared/viewer.js';
import {
  Bounds3D,
  BrickMap3D,
  Capsule3D,
  CellIndex,
  Color,
  DensityGrid3D,
  Direction3D,
  FunctionSdf3D,
  GridCell3D,
  HalfSpace,
  IntegerInterval,
  IntegerVector3,
  Intrinsics,
  ItemIndex,
  LevelSetGrid3D,
  LooseOctree3D,
  OccupancyGrid3D,
  Octree3D,
  OctreeNode,
  Plane,
  Point3D,
  RegularGrid3D,
  SampledScalarGrid3D,
  SampledSdf3D,
  SparseVoxelGrid3D,
  SpatialNodeIndex,
  Sphere,
  Vector3D,
  VoxelBrick3D,
  VoxelColorGrid3D,
  VoxelGrid3D,
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
const n4 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(4);
const pct = (x: number): string => `${(x * 100).toFixed(1)}%`;
const pt = (p: Point3D): string => `(${n2(p.X)}, ${n2(p.Y)}, ${n2(p.Z)})`;
const iv = (v: IntegerVector3): string => `(${v.X}, ${v.Y}, ${v.Z})`;

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
// Volumes
//
// `Array3D<T>` (primitives.types.plato) is built by `Number.MakeArray3D`, whose
// receiver is the column count. The value it returns is LAZY — `At` calls the
// generator every time — and it reports its extents as FIELDS while the emitted
// `IArray3D<T>` interface declares them as methods, so both facts are wrapped
// here once instead of being re-discovered per scene.

const volumeOf = <T,>(
  size: number,
  at: (i: number, j: number, k: number) => T,
): IArray3D<T> => size.MakeArray3D(size, size, at);

const boxVolumeOf = <T,>(
  columns: number,
  rows: number,
  layers: number,
  at: (i: number, j: number, k: number) => T,
): IArray3D<T> => columns.MakeArray3D(rows, layers, at);

/** The extents an `Array3D` carries as fields. */
const extentsOf = (values: IArray3D<unknown>): IntegerVector3 => {
  const v = values as unknown as { ColumnCount: number; RowCount: number; LayerCount: number };
  return new IntegerVector3(v.ColumnCount, v.RowCount, v.LayerCount);
};

/**
 * An `IArray<T>` over a plain array. `Intrinsics.MakeArray` is variadic and a
 * voxelized solid runs to tens of thousands of occupied cells, which is past
 * what a spread argument list can carry — so the range mapper is used instead.
 */
const arrayOf = <T,>(xs: readonly T[]): IArray<T> => Intrinsics.Range(xs.length).Map(i => xs[i]);

// ---------------------------------------------------------------------------
// Subjects
//
// Every subject is a `FunctionSdf3D` built from generated members only: the
// primitive converters (`Sphere.ToSdf`, `Capsule3D.ToSdf`, `HalfSpace.ToSdf`)
// and the combinators of `implicit-sdf.library.plato` (`SmoothUnion`,
// `Difference`, `Intersection`, `Shell`). The grid is what the page is about;
// the solid only has to be interesting and cheap to evaluate.

const SUBJECT_LABELS = ['Ball', 'Rod', 'Blob', 'Bitten', 'Shell', 'Box'];

interface Subject {
  /** The value, as the readings name it. */
  name: string;
  /** The generated members it came out of. */
  source: string;
  sdf: FunctionSdf3D;
}

const origin3 = new Point3D(0, 0, 0);

function halfSpaceSdf(x: number, y: number, z: number, distance: number): FunctionSdf3D {
  const normal = new Direction3D(new Vector3D(x, y, z).Normalize());
  return new HalfSpace(new Plane(normal, distance)).ToSdf();
}

function subjectOf(index: number, u: number): Subject {
  switch (clampIndex(index, SUBJECT_LABELS.length)) {
    case 0: {
      const ball = new Sphere(origin3, 0.35 + 0.45 * u);
      return { name: `Sphere radius ${n2(ball.Radius)}`, source: 'Sphere.ToSdf', sdf: ball.ToSdf() };
    }
    case 1: {
      const rod = new Capsule3D(
        new Point3D(-0.6, -0.42, -0.22),
        new Point3D(0.6, 0.46, 0.26),
        0.1 + 0.3 * u,
      );
      return {
        name: `Capsule3D radius ${n2(rod.Radius)}`,
        source: 'Capsule3D.ToSdf',
        sdf: rod.ToSdf(),
      };
    }
    case 2: {
      const blend = 0.03 + 0.5 * u;
      const a = new Sphere(new Point3D(-0.4, -0.12, 0.02), 0.42).ToSdf();
      const b = new Sphere(new Point3D(0.36, 0.18, 0.12), 0.38).ToSdf();
      const c = new Sphere(new Point3D(0.02, -0.08, -0.44), 0.31).ToSdf();
      return {
        name: `three balls, blend ${n2(blend)}`,
        source: 'Sphere.ToSdf, FunctionSdf3D.SmoothUnion',
        sdf: a.SmoothUnion(b, blend).SmoothUnion(c, blend),
      };
    }
    case 3: {
      const bite = new Sphere(new Point3D(0.42, 0.4, 0.42), 0.22 + 0.48 * u).ToSdf();
      return {
        name: `ball less a ball of ${n2(0.22 + 0.48 * u)}`,
        source: 'Sphere.ToSdf, FunctionSdf3D.Difference',
        sdf: new Sphere(origin3, 0.78).ToSdf().Difference(bite),
      };
    }
    case 4: {
      const thickness = 0.04 + 0.16 * u;
      // Cut open with a half-space so the hollow the shell leaves is visible
      // from outside — otherwise every scene shows a solid-looking ball.
      const shell = new Sphere(origin3, 0.76).ToSdf().Shell(thickness);
      return {
        name: `shell ${n2(thickness)} thick, cut open`,
        source: 'FunctionSdf3D.Shell, HalfSpace.ToSdf, FunctionSdf3D.Difference',
        sdf: shell.Difference(halfSpaceSdf(1, 0.55, 0.75, 0.1)),
      };
    }
    default: {
      const half = 0.3 + 0.4 * u;
      const sides = [
        halfSpaceSdf(1, 0, 0, half),
        halfSpaceSdf(-1, 0, 0, half),
        halfSpaceSdf(0, 1, 0, half),
        halfSpaceSdf(0, -1, 0, half),
        halfSpaceSdf(0, 0, 1, half),
        halfSpaceSdf(0, 0, -1, half),
      ];
      return {
        name: `six half-spaces at ${n2(half)}`,
        source: 'HalfSpace.ToSdf, FunctionSdf3D.Intersection',
        sdf: sides.reduce((acc, side) => acc.Intersection(side)),
      };
    }
  }
}

// ---------------------------------------------------------------------------
// The fill
//
// The one place the demo samples anything. `DepthBelowSurfaceAt` is the
// generated member for "how far inside the surface is this point" — it is
// `Eval(point).Negative()`, which is what a DENSITY grid wants, because
// `DensityGrid3D.MarchingCubes` extracts the region AT OR ABOVE its iso level
// and a signed distance field is negative inside.
//
// Memoized per (subject, shape, resolution) so that a toggle or an iso-level
// drag costs nothing: those parameters do not change what is stored.

const WORLD = new Bounds3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
const WORLD_ORIGIN = WORLD.Min;
const WORLD_SPAN = 2;

interface Fill {
  subject: Subject;
  resolution: number;
  cellSize: number;
  /** Depth below the surface at each cell centre, row-major, materialized. */
  depths: Float64Array;
  /** The library's own `Contains` at each cell centre. */
  inside: Uint8Array;
  insideCount: number;
  deepest: number;
}

const fillCache = new Map<string, Fill>();

function fillOf(subjectIndex: number, shape: number, resolution: number): Fill {
  const key = `${clampIndex(subjectIndex, SUBJECT_LABELS.length)}:${shape.toFixed(3)}:${resolution}`;
  const hit = fillCache.get(key);
  if (hit) return hit;

  const subject = subjectOf(subjectIndex, shape);
  const n = resolution;
  const cellSize = WORLD_SPAN / n;
  const depths = new Float64Array(n * n * n);
  const inside = new Uint8Array(n * n * n);
  let insideCount = 0;
  let deepest = 0;
  for (let k = 0; k < n; k++) {
    for (let j = 0; j < n; j++) {
      for (let i = 0; i < n; i++) {
        const centre = WORLD_ORIGIN.CellCenter(cellSize, i, j, k);
        const depth = subject.sdf.DepthBelowSurfaceAt(centre);
        const slot = (k * n + j) * n + i;
        depths[slot] = depth;
        if (subject.sdf.Contains(centre)) {
          inside[slot] = 1;
          insideCount++;
          if (depth > deepest) deepest = depth;
        }
      }
    }
  }

  const fill: Fill = { subject, resolution: n, cellSize, depths, inside, insideCount, deepest };
  // Three entries is enough to keep a slider drag warm without holding on to
  // every volume the session ever built.
  if (fillCache.size > 3) fillCache.clear();
  fillCache.set(key, fill);
  return fill;
}

/** The materialized depths as the `Array3D` the grid types take. */
const depthVolume = (fill: Fill): IArray3D<number> => {
  const n = fill.resolution;
  return volumeOf(n, (i, j, k) => fill.depths[(k * n + j) * n + i]);
};

/** The same values negated: distances, which is what a level set stores. */
const distanceVolume = (fill: Fill): IArray3D<number> => {
  const n = fill.resolution;
  return volumeOf(n, (i, j, k) => -fill.depths[(k * n + j) * n + i]);
};

const occupancyVolume = (fill: Fill): IArray3D<boolean> => {
  const n = fill.resolution;
  return volumeOf(n, (i, j, k) => fill.inside[(k * n + j) * n + i] === 1);
};

/**
 * The field on the NODE lattice instead of the cell centres — what `SampledSdf3D`
 * stores. Node positions come from `Bounds3D.LatticeNodePosition`, the member
 * `SampledSdf3D.NodePosition` is written against, so the two agree by
 * construction. Memoized on the same key as the cell fill.
 */
const nodeCache = new Map<string, Float64Array>();

function nodeValuesOf(
  subjectIndex: number,
  shape: number,
  resolution: number,
  subject: Subject,
): Float64Array {
  const key = `${clampIndex(subjectIndex, SUBJECT_LABELS.length)}:${shape.toFixed(3)}:${resolution}`;
  const hit = nodeCache.get(key);
  if (hit) return hit;
  const n = resolution;
  const counts = new IntegerVector3(n, n, n);
  const values = new Float64Array(n * n * n);
  for (let k = 0; k < n; k++) {
    for (let j = 0; j < n; j++) {
      for (let i = 0; i < n; i++) {
        values[(k * n + j) * n + i] = subject.sdf.Eval(WORLD.LatticeNodePosition(counts, i, j, k));
      }
    }
  }
  if (nodeCache.size > 3) nodeCache.clear();
  nodeCache.set(key, values);
  return values;
}

/** The world position of every occupied cell, from the grid's own `CellCenter`. */
function occupiedCentres(fill: Fill, grid: DensityGrid3D): Point3D[] {
  const n = fill.resolution;
  const centres: Point3D[] = [];
  for (let k = 0; k < n; k++) {
    for (let j = 0; j < n; j++) {
      for (let i = 0; i < n; i++) {
        if (fill.inside[(k * n + j) * n + i] === 1) centres.push(grid.CellCenter(i, j, k));
      }
    }
  }
  return centres;
}

// ---------------------------------------------------------------------------
// Presentation
//
// One InstancedMesh for the cells and one LineSegments for every box outline in
// a scene: a 32-cubed grid can leave ten thousand occupied cells, and ten
// thousand `THREE.Mesh` objects would stall the page on the first tick.

function cellInstances(
  centres: readonly Point3D[],
  size: number,
  color: number,
  colors?: readonly THREE.Color[],
): THREE.InstancedMesh {
  const geometry = new THREE.BoxGeometry(size, size, size);
  const mesh = new THREE.InstancedMesh(geometry, surfaceMaterial(color), Math.max(1, centres.length));
  mesh.count = centres.length;
  const matrix = new THREE.Matrix4();
  for (let i = 0; i < centres.length; i++) {
    matrix.makeTranslation(centres[i].X, centres[i].Y, centres[i].Z);
    mesh.setMatrixAt(i, matrix);
    if (colors) mesh.setColorAt(i, colors[i]);
  }
  mesh.instanceMatrix.needsUpdate = true;
  if (mesh.instanceColor) mesh.instanceColor.needsUpdate = true;
  return mesh;
}

function segments(coordinates: number[], color: number, opacity = 1): THREE.LineSegments {
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

function dots(points: readonly Point3D[], color: number, size = 6): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.X, p.Y, p.Z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(geometry, new THREE.PointsMaterial({ color, size, sizeAttenuation: false }));
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

/** Fresh controls per scene, so no two catalogs share a `Control` object. */
const subjectControls = (): Control[] => [
  { key: 'subject', label: 'Solid', kind: 'select', options: SUBJECT_LABELS, def: 2 },
  { key: 'shape', label: 'Shape', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0.45 },
];

const SPATIAL_VIEW: ViewerOptions = { distance: 4.4, grid: false, spin: true };
const STILL_VIEW: ViewerOptions = { distance: 4.4, grid: false, spin: false };

// ---------------------------------------------------------------------------
// Scenes

const filling = sceneOf({
  id: 'filling',
  title: 'Filling a grid from a solid',
  description:
    'A signed distance field sampled onto a dense grid over the world cube [-1, 1]. The cell positions come ' +
    'from Point3D.CellCenter — the one member voxels.library.plato gives every dense grid — and the occupancy ' +
    'test is the field\'s own FunctionSdf3D.Contains. There is no voxelizer to call: IVoxel3D.ToOccupancyGrid ' +
    'is declared in pointclouds-voxels.concepts.plato but no stdlib type implements it, so the sampling loop ' +
    'is the demo\'s and everything it asks is the library\'s. The occupancy fraction is the demo: it is what ' +
    'voxelizing costs, and it climbs with the cube of the resolution while the surface climbs with the square.',
  plato: [
    'Sphere.ToSdf',
    'Capsule3D.ToSdf',
    'HalfSpace.ToSdf',
    'FunctionSdf3D.SmoothUnion',
    'FunctionSdf3D.Difference',
    'FunctionSdf3D.Intersection',
    'FunctionSdf3D.Shell',
    'FunctionSdf3D.Contains',
    'FunctionSdf3D.DepthBelowSurfaceAt',
    'Point3D.CellCenter',
    'DensityGrid3D.CellCenter',
    'Number.MakeArray3D',
    'OccupancyGrid3D',
    'VoxelGrid3D',
  ],
  viewer: SPATIAL_VIEW,
  controls: [
    ...subjectControls(),
    { key: 'resolution', label: 'Cells per axis', kind: 'slider', min: 6, max: 40, step: 2, def: 20 },
    { key: 'fillFactor', label: 'Cube fill', kind: 'slider', min: 0.3, max: 1, step: 0.02, def: 0.86 },
    { key: 'box', label: 'Grid bounds', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const fill = fillOf(params.subject, params.shape, Math.round(params.resolution));
    const n = fill.resolution;
    const values = depthVolume(fill);
    const grid = new DensityGrid3D(WORLD_ORIGIN, fill.cellSize, values);
    const occupancy = new OccupancyGrid3D(WORLD_ORIGIN, fill.cellSize, occupancyVolume(fill));
    const dense = new VoxelGrid3D<number>(WORLD_ORIGIN, fill.cellSize, values);
    const centres = occupiedCentres(fill, grid);

    const object = new THREE.Group();
    object.add(cellInstances(centres, fill.cellSize * params.fillFactor, palette.surface));
    if (params.box >= 0.5) {
      const outline: number[] = [];
      pushBoxEdges(outline, WORLD);
      object.add(segments(outline, 0x2f3a4c));
    }

    const mid = Math.floor(n / 2);
    return {
      object,
      readings: [
        note('solid', `${fill.subject.name} via ${fill.subject.source}`),
        note('lattice', `${n} x ${n} x ${n}`),
        note('cells', String(n ** 3)),
        note('cells set', String(fill.insideCount)),
        note('occupancy', pct(fill.insideCount / n ** 3)),
        note('CellSize', n4(fill.cellSize)),
        reading('DensityGrid3D.CellCenter(0,0,0)', () => pt(grid.CellCenter(0, 0, 0))),
        reading(`DensityGrid3D.CellCenter(${n - 1},${n - 1},${n - 1})`, () =>
          pt(grid.CellCenter(n - 1, n - 1, n - 1)),
        ),
        reading('OccupancyGrid3D.Occupied.At(mid)', () =>
          String(occupancy.Occupied.At(mid, mid, mid)),
        ),
        reading('VoxelGrid3D.Values.At(mid)', () => n4(dense.Values.At(mid, mid, mid))),
        reading('FunctionSdf3D.Contains(centre)', () => String(fill.subject.sdf.Contains(origin3))),
        reading('deepest DepthBelowSurfaceAt', () => n4(fill.deepest)),
        note('IVoxel3D.ToOccupancyGrid', 'NOT IMPLEMENTED BY ANY TYPE (concept only)'),
      ],
    };
  },
});

const sparsity = sceneOf({
  id: 'sparsity',
  title: 'Dense, sparse and bricked',
  description:
    'The same voxelized solid held three ways. VoxelGrid3D stores one value per cell of the whole lattice; ' +
    'SparseVoxelGrid3D stores a coordinate and a value only where a cell is occupied; BrickMap3D stores dense ' +
    'cubic VoxelBrick3D blocks, and only the blocks a solid actually touches. The stored-cell counts are what ' +
    'this scene is for — a solid fills a fraction of its bounding cube, so the sparse forms store a fraction ' +
    'of the cells, and the brick map trades a coarser granularity for one coordinate per block instead of one ' +
    'per cell. None of the three sparse types carries a single library body: voxels.library.plato defines ' +
    'their layout invariants in prose and gives them no lookup, insert or iterate member.',
  plato: [
    'VoxelGrid3D',
    'SparseVoxelGrid3D',
    'VoxelBrick3D',
    'BrickMap3D',
    'DensityGrid3D.CellCenter',
    'FunctionSdf3D.Contains',
    'Number.MakeArray3D',
    'IntegerVector3',
  ],
  viewer: SPATIAL_VIEW,
  controls: [
    ...subjectControls(),
    { key: 'resolution', label: 'Cells per axis', kind: 'slider', min: 8, max: 32, step: 4, def: 16 },
    { key: 'brick', label: 'Brick size', kind: 'select', options: ['2', '4', '8'], def: 1 },
    { key: 'bricks', label: 'Brick outlines', kind: 'toggle', def: 1 },
    { key: 'cells', label: 'Occupied cells', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const fill = fillOf(params.subject, params.shape, Math.round(params.resolution));
    const n = fill.resolution;
    const brickSize = [2, 4, 8][clampIndex(params.brick, 3)];
    const values = depthVolume(fill);
    const grid = new DensityGrid3D(WORLD_ORIGIN, fill.cellSize, values);
    const dense = new VoxelGrid3D<number>(WORLD_ORIGIN, fill.cellSize, values);

    // Sparse: one entry per occupied cell, in the Z-then-Y-then-X order the type
    // asks for, which is the order this walk already produces.
    const coordinates: IntegerVector3[] = [];
    const stored: number[] = [];
    for (let k = 0; k < n; k++) {
      for (let j = 0; j < n; j++) {
        for (let i = 0; i < n; i++) {
          const slot = (k * n + j) * n + i;
          if (fill.inside[slot] !== 1) continue;
          coordinates.push(new IntegerVector3(i, j, k));
          stored.push(fill.depths[slot]);
        }
      }
    }
    const sparse = new SparseVoxelGrid3D<number>(
      WORLD_ORIGIN,
      fill.cellSize,
      arrayOf(coordinates),
      arrayOf(stored),
    );

    // Bricked: a brick is kept when any of its cells is occupied, and it then
    // stores its whole cubic payload — that is the trade the type describes.
    const bricksPerAxis = Math.ceil(n / brickSize);
    const bricks: VoxelBrick3D[] = [];
    const brickBoxes: Bounds3D[] = [];
    const sample = (i: number, j: number, k: number): number =>
      i < n && j < n && k < n ? fill.depths[(k * n + j) * n + i] : -1;
    for (let bk = 0; bk < bricksPerAxis; bk++) {
      for (let bj = 0; bj < bricksPerAxis; bj++) {
        for (let bi = 0; bi < bricksPerAxis; bi++) {
          let touched = false;
          for (let k = 0; k < brickSize && !touched; k++) {
            for (let j = 0; j < brickSize && !touched; j++) {
              for (let i = 0; i < brickSize && !touched; i++) {
                const x = bi * brickSize + i;
                const y = bj * brickSize + j;
                const z = bk * brickSize + k;
                if (x < n && y < n && z < n && fill.inside[(z * n + y) * n + x] === 1) touched = true;
              }
            }
          }
          if (!touched) continue;
          bricks.push(
            new VoxelBrick3D(
              new IntegerVector3(bi, bj, bk),
              boxVolumeOf(brickSize, brickSize, brickSize, (i, j, k) =>
                sample(bi * brickSize + i, bj * brickSize + j, bk * brickSize + k),
              ),
            ),
          );
          // The brick's world box, from the library's own cell centres for its
          // two extreme cells rather than from a re-derived origin.
          const low = grid.CellCenter(bi * brickSize, bj * brickSize, bk * brickSize);
          const highIndex = (b: number): number => Math.min(n - 1, (b + 1) * brickSize - 1);
          const high = grid.CellCenter(highIndex(bi), highIndex(bj), highIndex(bk));
          brickBoxes.push(new Bounds3D(low, high).Expand(fill.cellSize / 2));
        }
      }
    }
    const map = new BrickMap3D(WORLD_ORIGIN, fill.cellSize, brickSize, arrayOf(bricks));

    const object = new THREE.Group();
    if (params.cells >= 0.5) {
      object.add(cellInstances(occupiedCentres(fill, grid), fill.cellSize * 0.8, palette.surface));
    }
    if (params.bricks >= 0.5) {
      const outline: number[] = [];
      for (const box of brickBoxes) pushBoxEdges(outline, box);
      object.add(segments(outline, palette.surfaceAlt, 0.75));
    }
    const world: number[] = [];
    pushBoxEdges(world, WORLD);
    object.add(segments(world, 0x2f3a4c));

    const brickCells = bricks.length * brickSize ** 3;
    return {
      object,
      readings: [
        note('solid', fill.subject.name),
        note('lattice', `${n} x ${n} x ${n}`),
        reading('VoxelGrid3D stored cells', () => String(extentsOf(dense.Values).X ** 3)),
        reading('SparseVoxelGrid3D.Coordinates', () => String(sparse.Coordinates.Count())),
        reading('SparseVoxelGrid3D.Values', () => String(sparse.Values.Count())),
        note('sparse share', pct(sparse.Coordinates.Count() / n ** 3)),
        note('BrickSize', String(brickSize)),
        reading('BrickMap3D.Bricks', () => `${map.Bricks.Count()} of ${bricksPerAxis ** 3}`),
        note('brick cells', `${brickCells} (${pct(brickCells / n ** 3)})`),
        reading('VoxelBrick3D.Coordinate', () =>
          bricks.length > 0 ? iv(map.Bricks.At(0).Coordinate) : 'no brick kept',
        ),
        reading('VoxelBrick3D.Values', () =>
          bricks.length > 0 ? iv(extentsOf(map.Bricks.At(0).Values)) : 'no brick kept',
        ),
        note('sparse library bodies', 'NONE (types only)'),
      ],
    };
  },
});

const readback = sceneOf({
  id: 'readback',
  title: 'Reading a surface back out',
  description:
    'The same grid drawn as blocky cells and marched into an isosurface, at one resolution and one iso level. ' +
    'DensityGrid3D.MarchingCubes bounds the region whose stored value is AT OR ABOVE the level, so the grid ' +
    'holds FunctionSdf3D.DepthBelowSurfaceAt — a distance field negated, positive inside — and the level ' +
    'erodes or dilates the solid without resampling anything. Raising the resolution is what buys fidelity, ' +
    'and the triangle count is what it costs. The grid\'s samples sit at CELL CENTRES, so the marched surface ' +
    'spans the centres and stops half a cell short of the grid on every side; SampledSdf3D stores the same ' +
    'field on lattice NODES and spans the whole of its Bounds, which is why the two hulls do not coincide.',
  plato: [
    'FunctionSdf3D.DepthBelowSurfaceAt',
    'Point3D.CellCenter',
    'DensityGrid3D.CellCenter',
    'DensityGrid3D.MarchingCubes',
    'LevelSetGrid3D.MarchingCubes',
    'SampledSdf3D.NodePosition',
    'SampledSdf3D.NodeSpacing',
    'SampledSdf3D.NodeCounts',
    'SampledSdf3D.MarchingCubes',
    'Bounds3D.LatticeNodePosition',
    'IntegerVector3.MarchingCubesLattice',
  ],
  viewer: SPATIAL_VIEW,
  controls: [
    ...subjectControls(),
    // Capped at 26 rather than the fill scene's 40: this scene marches the grid
    // as well as filling it, and marching the node lattice too doubles both.
    { key: 'resolution', label: 'Cells per axis', kind: 'slider', min: 6, max: 26, step: 2, def: 18 },
    { key: 'iso', label: 'Iso level', kind: 'slider', min: -0.25, max: 0.35, step: 0.01, def: 0 },
    { key: 'show', label: 'Show', kind: 'select', options: ['Cells', 'Surface', 'Both'], def: 2 },
    { key: 'nodes', label: 'Node lattice too', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const fill = fillOf(params.subject, params.shape, Math.round(params.resolution));
    const n = fill.resolution;
    const iso = params.iso;
    const show = clampIndex(params.show, 3);
    const grid = new DensityGrid3D(WORLD_ORIGIN, fill.cellSize, depthVolume(fill));
    // A level set of the same field. Its band covers the whole grid — every cell
    // holds a real distance rather than the background value — so nothing is
    // truncated and its zero set is the density grid's iso-zero surface.
    const levelSet = new LevelSetGrid3D(
      WORLD_ORIGIN,
      fill.cellSize,
      distanceVolume(fill),
      WORLD_SPAN,
      WORLD_SPAN,
    );

    // Cells at the level: the same comparison the marcher makes, asked of the
    // stored values rather than re-evaluating the field.
    const centres: Point3D[] = [];
    for (let k = 0; k < n; k++) {
      for (let j = 0; j < n; j++) {
        for (let i = 0; i < n; i++) {
          if (fill.depths[(k * n + j) * n + i] >= iso) centres.push(grid.CellCenter(i, j, k));
        }
      }
    }

    const object = new THREE.Group();
    if (show !== 1) {
      object.add(cellInstances(centres, fill.cellSize * 0.9, palette.surface));
    }

    const marched = grid.MarchingCubes(iso);
    let triangles = 0;
    const surface = reading('DensityGrid3D.MarchingCubes', () => {
      triangles = marched.Triangles.Count();
      return `${triangles} triangles`;
    });
    if (show !== 0 && triangles > 0) {
      object.add(isosurface(triangleArrayGeometry(marched), palette.accent, show === 2 ? 0.55 : 1));
    }

    const readings: Reading[] = [
      note('solid', fill.subject.name),
      note('lattice', `${n} x ${n} x ${n}`),
      note('iso level', n2(iso)),
      note('cells at or above', `${centres.length} of ${n ** 3} (${pct(centres.length / n ** 3)})`),
      surface,
      note('CellSize', n4(fill.cellSize)),
      reading('cell-centre span', () =>
        `${pt(grid.CellCenter(0, 0, 0))} .. ${pt(grid.CellCenter(n - 1, n - 1, n - 1))}`,
      ),
      reading('LevelSetGrid3D.MarchingCubes', () =>
        `${levelSet.MarchingCubes().Triangles.Count()} triangles at zero`,
      ),
    ];

    if (params.nodes >= 0.5) {
      // The same field on the node lattice: values at Bounds3D.LatticeNodePosition,
      // which is exactly what SampledSdf3D.NodePosition reads back. Materialized
      // and memoized like the cell fill, because the iso slider does not change
      // it and marching reads every node eight times.
      const nodes = nodeValuesOf(params.subject, params.shape, n, fill.subject);
      const sampled = new SampledSdf3D(
        volumeOf(n, (i, j, k) => nodes[(k * n + j) * n + i]),
        WORLD,
      );
      const nodeMarched = sampled.MarchingCubes();
      if (nodeMarched.Triangles.Count() > 0) {
        object.add(isosurface(triangleArrayGeometry(nodeMarched), palette.surfaceAlt, 0.4));
      }
      readings.push(
        reading('SampledSdf3D.NodeCounts', () => iv(sampled.NodeCounts())),
        reading('SampledSdf3D.NodeSpacing', () => n4(sampled.NodeSpacing().X)),
        reading('node span', () =>
          `${pt(sampled.NodePosition(0, 0, 0))} .. ${pt(sampled.NodePosition(n - 1, n - 1, n - 1))}`,
        ),
        reading('SampledSdf3D.MarchingCubes', () => `${nodeMarched.Triangles.Count()} triangles`),
      );
    }

    const world: number[] = [];
    pushBoxEdges(world, WORLD);
    object.add(segments(world, 0x2f3a4c));
    return { object, readings };
  },
});

const addressing = sceneOf({
  id: 'addressing',
  title: 'Grid addressing',
  description:
    'Where a point lands. Point3D.CellCenter maps a cell index to world space; the inverse comes from ' +
    'Number.NodeCoordinate (sampling.library.plato), the fractional cell coordinate whose floor is the cell ' +
    'index — the same map SampledScalarGrid3D.Eval uses to find the eight nodes it blends. The probe reports ' +
    'the GridCell3D it lands in, that cell\'s own Bounds (its centre grown by half a cell), the row-major ' +
    'CellIndex of topology.types.plato, and what the grid holds there: the trilinear blend the sampled grid ' +
    'returns beside the exact value of the field it was filled from. voxels.library.plato has no inverse ' +
    'member of its own — the cell lookup below is the sampling library\'s, and the containment check is ' +
    'Bounds3D.Contains.',
  plato: [
    'RegularGrid3D',
    'GridCell3D',
    'CellIndex',
    'CellIndex.IsPresent',
    'CellIndex.Clamp',
    'Number.NodeCoordinate',
    'Number.Floor',
    'Number.Clamp',
    'Point3D.CellCenter',
    'Bounds3D.Contains',
    'Bounds3D.Expand',
    'Bounds3D.LatticeNodePosition',
    'SampledScalarGrid3D.NodeValue',
    'SampledScalarGrid3D.Eval',
    'SampledScalarGrid3D.IsNegativeAt',
    'SampledScalarGrid3D.MarchingCubes',
    'FunctionSdf3D.Eval',
  ],
  viewer: STILL_VIEW,
  controls: [
    ...subjectControls(),
    { key: 'cells', label: 'Cells per axis', kind: 'slider', min: 2, max: 8, step: 1, def: 4 },
    { key: 'px', label: 'Probe X', kind: 'slider', min: -1.2, max: 1.2, step: 0.01, def: 0.37 },
    { key: 'py', label: 'Probe Y', kind: 'slider', min: -1.2, max: 1.2, step: 0.01, def: -0.21 },
    { key: 'pz', label: 'Probe Z', kind: 'slider', min: -1.2, max: 1.2, step: 0.01, def: 0.44 },
    { key: 'surface', label: 'Marched surface', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const subject = subjectOf(params.subject, params.shape);
    const cells = Math.round(params.cells);
    const cellSize = WORLD_SPAN / cells;
    const grid = new RegularGrid3D(WORLD, new IntegerVector3(cells, cells, cells));
    const probe = new Point3D(params.px, params.py, params.pz);

    // The node lattice is one node bigger per axis than the cell count, which is
    // what SampledScalarGrid3D.NodeColumns says and what its flat indexing needs.
    const nodes = cells + 1;
    const nodeCounts = new IntegerVector3(nodes, nodes, nodes);
    const nodeAt = (i: number, j: number, k: number): Point3D =>
      WORLD.LatticeNodePosition(nodeCounts, i, j, k);
    const sampled = new SampledScalarGrid3D(
      grid,
      volumeOf(nodes, (i, j, k) => subject.sdf.Eval(nodeAt(i, j, k))),
    );

    // The library's world-to-cell map, one axis at a time.
    const axis = (value: number, low: number, high: number): number =>
      value.NodeCoordinate(low, high, cells).Floor().ToInteger().Clamp(0, cells - 1);
    const ci = axis(probe.X, WORLD.Min.X, WORLD.Max.X);
    const cj = axis(probe.Y, WORLD.Min.Y, WORLD.Max.Y);
    const ck = axis(probe.Z, WORLD.Min.Z, WORLD.Max.Z);

    const centre = WORLD.Min.CellCenter(cellSize, ci, cj, ck);
    const cell = new GridCell3D(
      new IntegerVector3(ci, cj, ck),
      new Bounds3D(centre, centre).Expand(cellSize / 2),
    );
    const linear = new CellIndex((ck * cells + cj) * cells + ci);

    const object = new THREE.Group();

    // The whole node lattice, drawn as the three families of axis-aligned lines.
    const lattice: number[] = [];
    for (let a = 0; a < nodes; a++) {
      for (let b = 0; b < nodes; b++) {
        const lo = nodeAt(0, a, b);
        const hi = nodeAt(nodes - 1, a, b);
        lattice.push(lo.X, lo.Y, lo.Z, hi.X, hi.Y, hi.Z);
        const lo2 = nodeAt(a, 0, b);
        const hi2 = nodeAt(a, nodes - 1, b);
        lattice.push(lo2.X, lo2.Y, lo2.Z, hi2.X, hi2.Y, hi2.Z);
        const lo3 = nodeAt(a, b, 0);
        const hi3 = nodeAt(a, b, nodes - 1);
        lattice.push(lo3.X, lo3.Y, lo3.Z, hi3.X, hi3.Y, hi3.Z);
      }
    }
    object.add(segments(lattice, 0x2b3444, 0.55));

    const highlight: number[] = [];
    pushBoxEdges(highlight, cell.Bounds);
    object.add(segments(highlight, palette.surfaceAlt));

    // The eight nodes the trilinear blend reads, split by the sign the grid
    // stores at each of them.
    const negative: Point3D[] = [];
    const positive: Point3D[] = [];
    for (let k = 0; k <= 1; k++) {
      for (let j = 0; j <= 1; j++) {
        for (let i = 0; i <= 1; i++) {
          const p = nodeAt(ci + i, cj + j, ck + k);
          (sampled.NodeValue(ci + i, cj + j, ck + k) < 0 ? negative : positive).push(p);
        }
      }
    }
    if (negative.length > 0) object.add(dots(negative, palette.accent, 9));
    if (positive.length > 0) object.add(dots(positive, 0x51617d, 7));
    object.add(dots([centre], palette.surfaceAlt, 11));
    object.add(dots([probe], palette.line, 13));

    if (params.surface >= 0.5) {
      const marched = sampled.MarchingCubes(WORLD, nodeCounts, 0);
      if (marched.Triangles.Count() > 0) {
        object.add(isosurface(triangleArrayGeometry(marched), palette.surface, 0.35));
      }
    }

    return {
      object,
      readings: [
        note('probe', pt(probe)),
        reading('Bounds3D.Contains(probe)', () => String(WORLD.Contains(probe))),
        reading('Number.NodeCoordinate X', () =>
          n4(probe.X.NodeCoordinate(WORLD.Min.X, WORLD.Max.X, cells)),
        ),
        reading('GridCell3D.CellIndex', () => iv(cell.CellIndex)),
        reading('GridCell3D.Bounds', () => `${pt(cell.Bounds.Min)} .. ${pt(cell.Bounds.Max)}`),
        reading('Point3D.CellCenter', () => pt(centre)),
        reading('CellIndex.Value', () => String(linear.Value)),
        reading('CellIndex.IsPresent', () => String(linear.IsPresent())),
        reading('CellIndex.Clamp', () =>
          String(linear.Clamp(new CellIndex(0), new CellIndex(cells ** 3 - 1)).Value),
        ),
        reading('SampledScalarGrid3D.NodeValue', () => n4(sampled.NodeValue(ci, cj, ck))),
        reading('SampledScalarGrid3D.Eval(probe)', () => n4(sampled.Eval(probe))),
        reading('FunctionSdf3D.Eval(probe)', () => n4(subject.sdf.Eval(probe))),
        reading('interpolation error', () => n4(sampled.Eval(probe) - subject.sdf.Eval(probe))),
        reading('SampledScalarGrid3D.IsNegativeAt', () => String(sampled.IsNegativeAt(probe))),
        note('inverse map in voxels.library.plato', 'NONE'),
      ],
    };
  },
});

const colouring = sceneOf({
  id: 'colour',
  title: 'Colour voxels',
  description:
    'VoxelColorGrid3D — the dense colour raster of voxels.types.plato — filled from generated colour members ' +
    'and drawn one instance per stored cell. Depth shades every cell by Color.Lerp between two endpoints on ' +
    'FunctionSdf3D.DepthBelowSurfaceAt normalized by the deepest cell, so the shell of a solid reads cool and ' +
    'its core warm. Gradient reads FunctionSdf3D.GradientAt at the cell centre and mixes three endpoints with ' +
    'Color.Barycentric on the normalized components, which paints the surface normal the grid would report. ' +
    'The type itself carries no members: the grid stores the colours and the drawing is the demo\'s.',
  plato: [
    'VoxelColorGrid3D',
    'Color.Lerp',
    'Color.Barycentric',
    'FunctionSdf3D.DepthBelowSurfaceAt',
    'FunctionSdf3D.GradientAt',
    'FunctionSdf3D.Contains',
    'Vector3D.Normalize',
    'DensityGrid3D.CellCenter',
    'Number.Saturate',
    'Number.MakeArray3D',
  ],
  viewer: SPATIAL_VIEW,
  controls: [
    ...subjectControls(),
    { key: 'resolution', label: 'Cells per axis', kind: 'slider', min: 6, max: 28, step: 2, def: 18 },
    { key: 'mode', label: 'Colour by', kind: 'select', options: ['Depth', 'Gradient'], def: 0 },
    { key: 'fillFactor', label: 'Cube fill', kind: 'slider', min: 0.3, max: 1, step: 0.02, def: 0.9 },
  ],
  build(params: Params): Built {
    const fill = fillOf(params.subject, params.shape, Math.round(params.resolution));
    const n = fill.resolution;
    const byDepth = clampIndex(params.mode, 2) === 0;
    const grid = new DensityGrid3D(WORLD_ORIGIN, fill.cellSize, depthVolume(fill));

    const cold = platoColor(0x2f6fb5);
    const warm = platoColor(0xf0a25a);
    const green = platoColor(0x63d6a8);
    const deepest = Math.max(fill.deepest, 1e-6);

    const colourAt = (i: number, j: number, k: number): Color => {
      const centre = grid.CellCenter(i, j, k);
      if (byDepth) {
        const t = (fill.depths[(k * n + j) * n + i] / deepest).Saturate();
        return cold.Lerp(warm, t);
      }
      const g = fill.subject.sdf.GradientAt(centre).Normalize();
      return cold.Barycentric(warm, green, (g.X * 0.5 + 0.5).Saturate(), (g.Y * 0.5 + 0.5).Saturate());
    };

    const colours = new VoxelColorGrid3D(
      WORLD_ORIGIN,
      fill.cellSize,
      volumeOf(n, colourAt),
    );

    const centres: Point3D[] = [];
    const tints: THREE.Color[] = [];
    for (let k = 0; k < n; k++) {
      for (let j = 0; j < n; j++) {
        for (let i = 0; i < n; i++) {
          if (fill.inside[(k * n + j) * n + i] !== 1) continue;
          centres.push(grid.CellCenter(i, j, k));
          tints.push(threeColor(colours.Colors.At(i, j, k)));
        }
      }
    }

    const object = new THREE.Group();
    object.add(cellInstances(centres, fill.cellSize * params.fillFactor, 0xffffff, tints));
    const world: number[] = [];
    pushBoxEdges(world, WORLD);
    object.add(segments(world, 0x2f3a4c));

    const mid = Math.floor(n / 2);
    const midColour = colours.Colors.At(mid, mid, mid);
    return {
      object,
      readings: [
        note('solid', fill.subject.name),
        note('lattice', `${n} x ${n} x ${n}`),
        note('colours stored', String(n ** 3)),
        note('cells drawn', String(centres.length)),
        note('rule', byDepth ? 'Color.Lerp on depth' : 'Color.Barycentric on the gradient'),
        reading('deepest cell', () => n4(fill.deepest)),
        reading('VoxelColorGrid3D.Colors.At(mid)', () =>
          `(${n2(midColour.R)}, ${n2(midColour.G)}, ${n2(midColour.B)})`,
        ),
        reading('VoxelColorGrid3D.CellSize', () => n4(colours.CellSize)),
        reading('VoxelColorGrid3D.Origin', () => pt(colours.Origin)),
        note('VoxelColorGrid3D library bodies', 'NONE (type only)'),
      ],
    };
  },
});

const octrees = sceneOf({
  id: 'octree',
  title: 'Octree — the adaptive answer',
  description:
    'The same solid held in an Octree3D instead of a uniform grid. A node is split when the surface can cross ' +
    'it, which is FunctionSdf3D.IsNearLevelAt at the node\'s Bounds3D.Center with the box half-diagonal as ' +
    'the tolerance — a conservative test for a true distance field, and every part of it a generated member. ' +
    'Leaves that meet the solid hold one item each, so Octree3D.CandidatesInBounds answers a box query by ' +
    'descending only the nodes it overlaps. The leaf count against the uniform lattice at the same finest ' +
    'cell size is the point: the tree pays for the surface, the grid pays for the volume. LooseOctree3D is ' +
    'the same tree with its node bounds grown by Bounds3D.Expand — the stdlib is explicit that a loose tree\'s ' +
    'query is the inner tree\'s query unchanged, because the looseness is already baked into the stored bounds.',
  plato: [
    'Octree3D',
    'OctreeNode',
    'Octree3D.CandidatesInBounds',
    'Octree3D.CandidateCountInBounds',
    'Octree3D.HasCandidatesInBounds',
    'Octree3D.ItemCount',
    'Octree3D.IsEmpty',
    'LooseOctree3D.CandidatesInBounds',
    'LooseOctree3D.ItemCount',
    'FunctionSdf3D.IsNearLevelAt',
    'FunctionSdf3D.Contains',
    'Bounds3D.Center',
    'Bounds3D.Diagonal',
    'Bounds3D.Expand',
    'Bounds3D.Overlaps',
    'Vector3D.Magnitude',
    'Number.Half',
  ],
  viewer: SPATIAL_VIEW,
  controls: [
    ...subjectControls(),
    // Four is the cap: a level costs eight times the nodes, and every node is a
    // field evaluation plus twelve drawn edges.
    { key: 'depth', label: 'Max depth', kind: 'slider', min: 1, max: 4, step: 1, def: 3 },
    { key: 'looseness', label: 'Looseness', kind: 'slider', min: 1, max: 2, step: 0.05, def: 1.3 },
    { key: 'query', label: 'Query size', kind: 'slider', min: 0.1, max: 1.2, step: 0.05, def: 0.45 },
    { key: 'qx', label: 'Query X', kind: 'slider', min: -1, max: 1, step: 0.02, def: 0.3 },
    { key: 'inner', label: 'Interior nodes', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const subject = subjectOf(params.subject, params.shape);
    const maxDepth = Math.round(params.depth);

    // Breadth-first, so a node's eight children are consecutive — the layout
    // OctreeNode.FirstChild describes.
    const boxes: Bounds3D[] = [WORLD];
    const depths: number[] = [0];
    const firstChild: number[] = [-1];
    for (let node = 0; node < boxes.length; node++) {
      const box = boxes[node];
      if (depths[node] >= maxDepth) continue;
      const reach = box.Diagonal().Magnitude().Half();
      if (!subject.sdf.IsNearLevelAt(box.Center(), 0, reach)) continue;
      firstChild[node] = boxes.length;
      const centre = box.Center();
      for (let o = 0; o < 8; o++) {
        const min = new Point3D(
          (o & 1) === 0 ? box.Min.X : centre.X,
          (o & 2) === 0 ? box.Min.Y : centre.Y,
          (o & 4) === 0 ? box.Min.Z : centre.Z,
        );
        const max = new Point3D(
          (o & 1) === 0 ? centre.X : box.Max.X,
          (o & 2) === 0 ? centre.Y : box.Max.Y,
          (o & 4) === 0 ? centre.Z : box.Max.Z,
        );
        boxes.push(new Bounds3D(min, max));
        depths.push(depths[node] + 1);
        firstChild.push(-1);
      }
    }

    // One item per leaf the solid reaches; interior nodes carry an empty range.
    const nodes: OctreeNode[] = [];
    const leafBoxes: Bounds3D[] = [];
    let items = 0;
    for (let node = 0; node < boxes.length; node++) {
      const box = boxes[node];
      const leaf = firstChild[node] < 0;
      const reach = box.Diagonal().Magnitude().Half();
      const meets =
        leaf && (subject.sdf.Contains(box.Center()) || subject.sdf.IsNearLevelAt(box.Center(), 0, reach));
      const start = items;
      if (meets) {
        items++;
        leafBoxes.push(box);
      }
      nodes.push(
        new OctreeNode(box, new SpatialNodeIndex(firstChild[node]), new IntegerInterval(start, items)),
      );
    }
    const itemIndices = Intrinsics.Range(items).Map(i => new ItemIndex(i));
    const tree = new Octree3D(WORLD, arrayOf(nodes), itemIndices, maxDepth);

    // The loose tree: the same nodes with their bounds grown, which is how the
    // stdlib says a loose tree is built.
    const grow = params.looseness - 1;
    const loose = new LooseOctree3D(
      new Octree3D(
        WORLD.Expand(grow),
        arrayOf(
          nodes.map(node =>
            node.WithBounds(node.Bounds.Expand((grow * node.Bounds.Diagonal().X) / 2)),
          ),
        ),
        itemIndices,
        maxDepth,
      ),
      params.looseness,
    );

    const half = params.query / 2;
    const queryCentre = new Point3D(params.qx, 0, 0);
    const query = new Bounds3D(queryCentre, queryCentre).Expand(half);

    const object = new THREE.Group();
    const wanted = params.inner >= 0.5 ? boxes : leafBoxes;
    const hit: number[] = [];
    const missed: number[] = [];
    for (const box of wanted) pushBoxEdges(box.Overlaps(query) ? hit : missed, box);
    if (missed.length > 0) object.add(segments(missed, 0x38455c, 0.85));
    if (hit.length > 0) object.add(segments(hit, palette.accent));
    const queryEdges: number[] = [];
    pushBoxEdges(queryEdges, query);
    object.add(segments(queryEdges, palette.surfaceAlt));

    const leaves = leafBoxes.length;
    const uniform = 8 ** maxDepth;
    return {
      object,
      readings: [
        note('solid', subject.name),
        note('max depth', String(maxDepth)),
        note('nodes', String(nodes.length)),
        note('leaves holding an item', String(leaves)),
        note('uniform lattice at the same cell', `${uniform} cells`),
        note('leaves as a share', pct(leaves / uniform)),
        reading('Octree3D.ItemCount', () => String(tree.ItemCount())),
        reading('Octree3D.IsEmpty', () => String(tree.IsEmpty())),
        reading('Octree3D.CandidateCountInBounds', () =>
          String(tree.CandidateCountInBounds(query)),
        ),
        reading('Octree3D.HasCandidatesInBounds', () =>
          String(tree.HasCandidatesInBounds(query)),
        ),
        reading('LooseOctree3D.CandidateCountInBounds', () =>
          String(loose.CandidateCountInBounds(query)),
        ),
        reading('LooseOctree3D.ItemCount', () => String(loose.ItemCount())),
        note('query', `${pt(query.Min)} .. ${pt(query.Max)}`),
      ],
    };
  },
});

const demo: Demo = {
  title: 'Voxels',
  subtitle: 'voxels.{types,library}.plato · pointclouds-voxels.concepts.plato',
  scenes: [filling, sparsity, readback, addressing, colouring, octrees],
};

mountDemo(demo);

export { demo };
