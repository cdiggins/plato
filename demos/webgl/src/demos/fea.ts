// Finite elements — a scene catalog over `stdlib/future/finite-elements.{types,
// library}.plato`, the material and beam vocabulary of `engineering.types.plato`
// and `engineering-materials.library.plato`, and the coordinate-list
// `SparseMatrix` of `stdlib/foundation/matrices.types.plato`.
//
// This is the one library in this app that was verified by EXECUTION against
// closed-form answers before any demo existed (see `tracker/issues/plato-424.md`),
// so the page is built to print those comparisons rather than pictures alone.
// Every status line below that says "closed form" is a number the demo computed
// from first principles beside the number the library returned. They agree; the
// cube scene reproduces the smoke gate's `5.0e-6 m`, `-1.5e-6 m`, `1.0e6 Pa` and
// `-77008.5 N` exactly, and the beam reproduces `P L^3 / 3EI` to fourteen digits.
//
// Three things shape every scene here, and two of them are limits worth naming
// on the page rather than hiding:
//
// **The stress plot is faceted by construction.** A constant-strain tetrahedron
// has ONE strain and therefore one stress per cell, so `VonMisesStresses` returns
// one value per cell and the shading is flat per facet. That is the element, not
// a bug in the drawing; nodal smoothing is deferred to `plato-427`.
//
// **The solve cost is superlinear in the ITERATION COUNT, not in the model.**
// The conjugate gradient in `finite-elements.library.plato` is written over
// `Array<Number>` system vectors, and the prelude's `SystemAddScaled`,
// `SystemSubtract` and `SystemProduct` are `Zip`, which is LAZY. Each iteration
// therefore wraps the previous iterate in another view instead of materializing
// it, so reading one component of the k-th search direction walks O(k^2) of
// accumulated views and a k-iteration solve costs about O(entries * k^3). A
// 24-degree-of-freedom cube converging in 7 iterations is 14 ms; the same model
// refined to 192 degrees of freedom and 44 iterations is 21 SECONDS. That is why
// the meshes on this page are deliberately tiny, why every scene prints its own
// iteration count and solve time, and why the tightest tolerance is spent where
// the answer is checkable. It is a prelude gap (`src/plato/array-ext.ts` has an
// `eager` helper the System* installs do not use), not a defect of the Plato
// source — the same solve in the C# target has no such chain — and this page
// reports it in the status line rather than routing around it.
//
// **Real deflections are microns.** Steel under a megapascal moves five parts in
// a million, so every deformed picture is drawn at an exaggeration the status
// line states. A scale of 1 would be a still life.
//
// Cost discipline: `StiffnessMatrix` is independent of loads and restraints and
// every solve is memoized on the parameters that actually change the answer, so
// dragging an exaggeration or a colour control re-reads a cached solution and
// only re-packs buffers.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { fromArray, toArray } from '../shared/mesh.js';
import { palette, type ViewerOptions } from '../shared/viewer.js';
import {
  Area,
  Beam,
  BeamSupport,
  Color,
  Density,
  DofIndex,
  ElasticModel2D,
  ElasticModel3D,
  ElasticSolution2D,
  ElasticSolution3D,
  EngineeringMaterial,
  FaceTraction3D,
  Length,
  LinearSolveSettings,
  NodalForce2D,
  NodalForce3D,
  Point2D,
  Point3D,
  PrescribedDisplacement,
  Pressure,
  SectionProperties,
  SparseMatrix,
  TetrahedralMesh3D,
  TetrahedronCell,
  ThermalConductivity,
  TriangleFace,
  TriangleMesh2D,
  Vector2D,
  Vector3D,
  VertexIndex,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// The three sum types this page names. CHK320 drops every sum type from the
// TypeScript target, so `PlaneCondition`, `BeamRestraint` and `BeamLoad` are free
// identifiers the prelude installs on `globalThis` — `BeamLoad` is the one whose
// cases carry fields. Reached here through one typed alias apiece rather than
// scattering `as any` through the scenes.
interface SumCase {
  Tag: string;
  IsFixed(): boolean;
  IsPinned(): boolean;
  IsRoller(): boolean;
  IsFree(): boolean;
  IsPlaneStress(): boolean;
  IsPlaneStrain(): boolean;
  /** finite-elements.library.plato dispatches this on `BeamRestraint`. */
  RestrainedDofCount(): number;
}
type Sum = {
  PlaneStress(): SumCase;
  PlaneStrain(): SumCase;
  Fixed(): SumCase;
  Pinned(): SumCase;
  Roller(): SumCase;
  Free(): SumCase;
  PointForce(position: Length, magnitude: number): SumCase;
  DistributedForce(start: Length, end: Length, magnitude: number): SumCase;
  Moment(position: Length, magnitude: number): SumCase;
};
const PlaneCondition = (globalThis as unknown as { PlaneCondition: Sum }).PlaneCondition;
const BeamRestraint = (globalThis as unknown as { BeamRestraint: Sum }).BeamRestraint;
const BeamLoad = (globalThis as unknown as { BeamLoad: Sum }).BeamLoad;

// ---------------------------------------------------------------------------
// Reading a generated member
//
// The house pattern from `polygons.ts`: a member that throws or returns NaN is a
// gap in the emitted library, and the status line keeps its name and says so
// rather than substituting a hand-rolled answer. On this page that matters more
// than usual — `stdlib/future` is the tier the repo neither lints nor converts.

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

/** A solve, or the message it failed with — the scene still draws either way. */
interface Attempt<T> {
  value?: T;
  error?: string;
}

function attempt<T>(produce: () => T): Attempt<T> {
  try {
    return { value: produce() };
  } catch (error) {
    return { error: (error as Error).message };
  }
}

const n1 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(1);
const n2 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(2);
const n3 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(3);
const sci = (x: number): string => (Object.is(x, -0) ? 0 : x).toExponential(4);
const mpa = (x: number): string => `${n3(x / 1e6)} MPa`;
const ms = (x: number): string => `${x.toFixed(0)} ms`;
const pct = (x: number): string => `${(x * 100).toFixed(2)}%`;

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

const SOLID_VIEW: ViewerOptions = { distance: 3.6, grid: false, spin: false };
const PLANE_VIEW: ViewerOptions = { distance: 3.0, grid: false, spin: false, orthographic: true };

// ---------------------------------------------------------------------------
// Material
//
// One isotropic `EngineeringMaterial`, with Young's modulus, Poisson ratio and
// density under sliders. Every elastic constant the analysis uses is DERIVED
// from those by generated members — `ShearModulus`, `FirstLameParameter`,
// `BulkModulus`, `ToLameParameters` and `PlaneLameParameters` — never restated
// here.

function materialOf(youngsGPa: number, poisson: number, density = 7850): EngineeringMaterial {
  return new EngineeringMaterial(
    'Steel',
    new Density(density),
    new Pressure(youngsGPa * 1e9),
    poisson,
    new Pressure(250e6),
    new Pressure(400e6),
    1.2e-5,
    new ThermalConductivity(50),
    490,
  );
}

/** The three derived elastic constants, as one reading each. */
function materialReadings(material: EngineeringMaterial): Reading[] {
  return [
    reading('EngineeringMaterial.ShearModulus', () => mpa(material.ShearModulus().Pascals)),
    reading('.FirstLameParameter', () => mpa(material.FirstLameParameter().Pascals)),
    reading('.BulkModulus', () => mpa(material.BulkModulus().Pascals)),
    reading('.ToLameParameters', () => {
      const lame = material.ToLameParameters();
      return `lambda ${mpa(lame.FirstParameter.Pascals)}, mu ${mpa(lame.ShearModulus.Pascals)}`;
    }),
  ];
}

// Two solve settings. The tolerance is a RELATIVE residual, and because the cost
// grows with the CUBE of the iteration count (see the header), buying four more
// digits of residual can cost an order of magnitude of time. The tight setting is
// spent where the answer is compared against a closed form; the loose one is what
// the bending scenes can afford.
const TIGHT = new LinearSolveSettings(2000, 1e-10);
const LOOSE = new LinearSolveSettings(600, 1e-6);

// ---------------------------------------------------------------------------
// Tetrahedral meshes
//
// There is no tetrahedral mesher in the stdlib, so building the mesh is demo
// work: a structured lattice of boxes, each split into the six tetrahedra of the
// Kuhn decomposition about its main diagonal. Everything asked OF the mesh —
// element volume, shape gradients, stiffness, strain, stress, the deformed
// positions — is a generated member.

/** Corner bit layout: bit 0 = x, bit 1 = y, bit 2 = z, as the six tets below assume. */
const KUHN: readonly (readonly number[])[] = [
  [0, 1, 3, 7], [0, 1, 5, 7], [0, 2, 3, 7],
  [0, 2, 6, 7], [0, 4, 5, 7], [0, 4, 6, 7],
];

interface BoundaryFace {
  cell: number;
  a: number;
  b: number;
  c: number;
}

interface Solid {
  positions: Point3D[];
  cells: TetrahedronCell[];
  mesh: TetrahedralMesh3D;
  /** The triangles owned by exactly one cell: what a viewer can see. */
  boundary: BoundaryFace[];
  /** Undirected boundary edges, as node pairs. */
  edges: [number, number][];
  node(i: number, j: number, k: number): number;
  counts: [number, number, number];
  size: [number, number, number];
}

const solidCache = new Map<string, Solid>();

function tetLattice(
  nx: number,
  ny: number,
  nz: number,
  lx: number,
  ly: number,
  lz: number,
): Solid {
  const key = `${nx}:${ny}:${nz}:${lx}:${ly}:${lz}`;
  const hit = solidCache.get(key);
  if (hit) return hit;

  const node = (i: number, j: number, k: number): number =>
    (k * (ny + 1) + j) * (nx + 1) + i;
  const positions: Point3D[] = [];
  for (let k = 0; k <= nz; k++) {
    for (let j = 0; j <= ny; j++) {
      for (let i = 0; i <= nx; i++) {
        positions.push(new Point3D((i / nx) * lx, (j / ny) * ly, (k / nz) * lz));
      }
    }
  }

  const cells: TetrahedronCell[] = [];
  const corners: number[][] = [];
  for (let k = 0; k < nz; k++) {
    for (let j = 0; j < ny; j++) {
      for (let i = 0; i < nx; i++) {
        const at = (b: number): number => node(i + (b & 1), j + ((b >> 1) & 1), k + ((b >> 2) & 1));
        for (const t of KUHN) {
          const v = [at(t[0]), at(t[1]), at(t[2]), at(t[3])];
          corners.push(v);
          cells.push(
            new TetrahedronCell(
              new VertexIndex(v[0]),
              new VertexIndex(v[1]),
              new VertexIndex(v[2]),
              new VertexIndex(v[3]),
            ),
          );
        }
      }
    }
  }

  // A face shared by two cells is interior; a face named once is on the surface.
  const seen = new Map<string, BoundaryFace | null>();
  const faceKey = (a: number, b: number, c: number): string =>
    [a, b, c].sort((x, y) => x - y).join(',');
  const FACES = [[0, 1, 2], [0, 2, 3], [0, 3, 1], [1, 3, 2]];
  corners.forEach((v, cell) => {
    for (const f of FACES) {
      const a = v[f[0]];
      const b = v[f[1]];
      const c = v[f[2]];
      const k = faceKey(a, b, c);
      seen.set(k, seen.has(k) ? null : { cell, a, b, c });
    }
  });
  const boundary: BoundaryFace[] = [];
  for (const face of seen.values()) if (face) boundary.push(face);

  const edgeSeen = new Set<string>();
  const edges: [number, number][] = [];
  for (const f of boundary) {
    for (const [p, q] of [[f.a, f.b], [f.b, f.c], [f.c, f.a]] as [number, number][]) {
      const k = p < q ? `${p},${q}` : `${q},${p}`;
      if (edgeSeen.has(k)) continue;
      edgeSeen.add(k);
      edges.push([p, q]);
    }
  }

  const solid: Solid = {
    positions,
    cells,
    mesh: new TetrahedralMesh3D(fromArray(positions), fromArray(cells)),
    boundary,
    edges,
    node,
    counts: [nx, ny, nz],
    size: [lx, ly, lz],
  };
  solidCache.set(key, solid);
  return solid;
}

// ---------------------------------------------------------------------------
// Colour
//
// A three-stop ramp built from `Color.Lerp` and `Number.Saturate` — generated
// members, like everything else that computes a value on this page.

const COLD = new Color(0.16, 0.42, 0.72, 1);
const MID = new Color(0.36, 0.82, 0.64, 1);
const HOT = new Color(0.94, 0.6, 0.3, 1);

function ramp(t: number): Color {
  const u = t.Saturate();
  return u < 0.5 ? COLD.Lerp(MID, u * 2) : MID.Lerp(HOT, (u - 0.5) * 2);
}

const threeColor = (c: Color): THREE.Color =>
  new THREE.Color().setRGB(
    Math.min(1, Math.max(0, c.R)),
    Math.min(1, Math.max(0, c.G)),
    Math.min(1, Math.max(0, c.B)),
  );

const rampColor = (t: number): THREE.Color => threeColor(ramp(t));

/** Normalize a value into the ramp, tolerating a flat field. */
const spread = (value: number, low: number, high: number): number =>
  high - low < 1e-30 ? 0.5 : (value - low) / (high - low);

function vertexMaterial(): THREE.MeshStandardMaterial {
  return new THREE.MeshStandardMaterial({
    vertexColors: true,
    roughness: 0.45,
    metalness: 0.05,
    flatShading: true,
    side: THREE.DoubleSide,
  });
}

function segments(coordinates: number[], color: number, opacity = 1): THREE.LineSegments {
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(coordinates, 3));
  return new THREE.LineSegments(
    geometry,
    new THREE.LineBasicMaterial({ color, transparent: opacity < 1, opacity }),
  );
}

function dots(points: readonly THREE.Vector3[], color: number, size = 7): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.x, p.y, p.z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(
    geometry,
    new THREE.PointsMaterial({ color, size, sizeAttenuation: false }),
  );
}

/**
 * The surface of a tetrahedral mesh as flat triangles, coloured per corner.
 * Non-indexed on purpose: a per-CELL reading has to stay faceted, because a
 * constant-strain tetrahedron genuinely has one value over its whole volume.
 */
function boundaryGeometry(
  solid: Solid,
  positions: readonly Point3D[],
  colourOf: (face: BoundaryFace, node: number) => THREE.Color,
): THREE.BufferGeometry {
  const xyz: number[] = [];
  const rgb: number[] = [];
  for (const face of solid.boundary) {
    for (const n of [face.a, face.b, face.c]) {
      const p = positions[n];
      xyz.push(p.X, p.Y, p.Z);
      const c = colourOf(face, n);
      rgb.push(c.r, c.g, c.b);
    }
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(xyz, 3));
  geometry.setAttribute('color', new THREE.Float32BufferAttribute(rgb, 3));
  geometry.computeVertexNormals();
  return geometry;
}

function boundaryEdges(solid: Solid, positions: readonly Point3D[]): number[] {
  const out: number[] = [];
  for (const [p, q] of solid.edges) {
    out.push(positions[p].X, positions[p].Y, positions[p].Z);
    out.push(positions[q].X, positions[q].Y, positions[q].Z);
  }
  return out;
}

/** An arrow as a shaft plus a small cone, for loads and gradients. */
function arrow(from: THREE.Vector3, direction: THREE.Vector3, length: number, color: number): THREE.Object3D {
  const group = new THREE.Group();
  const dir = direction.clone().normalize();
  const tip = from.clone().addScaledVector(dir, length);
  group.add(segments([from.x, from.y, from.z, tip.x, tip.y, tip.z], color));
  const head = new THREE.Mesh(
    new THREE.ConeGeometry(length * 0.09, length * 0.26, 8),
    new THREE.MeshBasicMaterial({ color }),
  );
  head.position.copy(tip.clone().addScaledVector(dir, -length * 0.13));
  head.quaternion.setFromUnitVectors(new THREE.Vector3(0, 1, 0), dir);
  group.add(head);
  return group;
}

/**
 * A matrix drawn as a grid of coloured cells — the picture the sparsity and
 * element-stiffness scenes are made of. One `InstancedMesh` however many cells.
 */
function matrixPlate(
  rows: number,
  columns: number,
  extent: number,
  colourAt: (row: number, column: number) => THREE.Color | null,
): THREE.Object3D {
  const group = new THREE.Group();
  const cell = extent / Math.max(rows, columns);
  const filled: [number, number, THREE.Color][] = [];
  for (let r = 0; r < rows; r++) {
    for (let c = 0; c < columns; c++) {
      const colour = colourAt(r, c);
      if (colour) filled.push([r, c, colour]);
    }
  }
  const plate = new THREE.InstancedMesh(
    new THREE.PlaneGeometry(cell * 0.92, cell * 0.92),
    new THREE.MeshBasicMaterial({ vertexColors: false, side: THREE.DoubleSide }),
    Math.max(1, filled.length),
  );
  plate.count = filled.length;
  const matrix = new THREE.Matrix4();
  filled.forEach(([r, c, colour], i) => {
    matrix.makeTranslation(
      (c + 0.5) * cell - (columns * cell) / 2,
      (rows * cell) / 2 - (r + 0.5) * cell,
      0,
    );
    plate.setMatrixAt(i, matrix);
    plate.setColorAt(i, colour);
  });
  plate.instanceMatrix.needsUpdate = true;
  if (plate.instanceColor) plate.instanceColor.needsUpdate = true;
  group.add(plate);

  const outline: number[] = [];
  const w = (columns * cell) / 2;
  const h = (rows * cell) / 2;
  outline.push(-w, -h, 0, w, -h, 0, w, -h, 0, w, h, 0, w, h, 0, -w, h, 0, -w, h, 0, -w, -h, 0);
  group.add(segments(outline, 0x3a465c));
  return group;
}

// ---------------------------------------------------------------------------
// Solving a solid model
//
// One memoized entry point. `StiffnessMatrix` is independent of the loads and
// the restraints, but `SolveElastic` assembles it internally, so the memo is on
// everything that changes the ANSWER — and display controls (exaggeration,
// colour mode, edges) are deliberately not part of the key.

interface SolidRun {
  model: ElasticModel3D;
  solution: ElasticSolution3D;
  /** Per cell, in `Mesh.Cells` order — one flat value each. */
  vonMises: number[];
  /** Per node, in `Mesh.Positions` order. */
  magnitudes: number[];
  displacements: Vector3D[];
  entries: number;
  solveMs: number;
  stressMs: number;
}

const solidRuns = new Map<string, Attempt<SolidRun>>();

function runSolid(
  key: string,
  build: () => ElasticModel3D,
  settings: LinearSolveSettings,
): Attempt<SolidRun> {
  const hit = solidRuns.get(key);
  if (hit) return hit;
  const result = attempt<SolidRun>(() => {
    const model = build();
    const t0 = performance.now();
    const raw = model.SolveElastic(settings);
    const solveMs = performance.now() - t0;
    // `Displacements` is a lazily mapped view over the final conjugate-gradient
    // iterate, which is itself a stack of one lazy `Zip` per iteration (see the
    // header). Reading one component therefore walks the whole stack, and the
    // stress recovery, the magnitudes and the deformed mesh each read every
    // component again. Reading them ONCE into a flat array and handing the
    // downstream members a solution built over it turns four walks of that stack
    // into one — repacking an output, which is demo work, not re-deriving one.
    const displacements = toArray(raw.Displacements);
    const solution = new ElasticSolution3D(
      fromArray(displacements),
      raw.Iterations,
      raw.Residual,
      raw.Converged,
    );
    const t1 = performance.now();
    const vonMises = toArray(model.VonMisesStresses(solution)).map(p => p.Pascals);
    const stressMs = performance.now() - t1;
    return {
      model,
      solution,
      vonMises,
      magnitudes: toArray(solution.DisplacementMagnitudes()).map(l => l.Meters),
      displacements,
      entries: model.StiffnessMatrix().Entries.Count(),
      solveMs,
      stressMs,
    };
  });
  // A handful of entries keeps a slider drag warm without holding every model a
  // session ever solved.
  if (solidRuns.size > 8) solidRuns.clear();
  solidRuns.set(key, result);
  return result;
}

/** The convergence report every solve carries, as three readings. */
function convergence(
  iterations: number,
  residual: number,
  converged: boolean,
  solveMs: number,
): Reading[] {
  return [
    note('iterations', String(iterations)),
    note('relative residual', residual.toExponential(2)),
    note('converged', converged ? 'yes' : 'NO — last iterate shown'),
    note('solve', ms(solveMs)),
  ];
}

/** The exaggeration a decade slider stands for. */
const exaggeration = (decades: number): number => Math.pow(10, decades);

const COLOUR_MODES = ['Stress', 'Displace', 'Plain'];

/**
 * The corner colour of a boundary triangle under the chosen mode: per CELL for
 * von Mises (which is genuinely constant over a cell) and per NODE for
 * displacement magnitude (which genuinely is not).
 */
function solidColourer(
  mode: number,
  run: SolidRun,
  stressRange: [number, number],
  displacementRange: [number, number],
): (face: BoundaryFace, node: number) => THREE.Color {
  if (mode === 0) {
    return face => rampColor(spread(run.vonMises[face.cell], stressRange[0], stressRange[1]));
  }
  if (mode === 1) {
    return (_face, node) =>
      rampColor(spread(run.magnitudes[node], displacementRange[0], displacementRange[1]));
  }
  const flat = new THREE.Color(palette.surface);
  return () => flat;
}

function rangeOf(values: readonly number[]): [number, number] {
  let low = Infinity;
  let high = -Infinity;
  for (const v of values) {
    if (v < low) low = v;
    if (v > high) high = v;
  }
  return Number.isFinite(low) ? [low, high] : [0, 1];
}

/** Deformed positions from the library's own `DeformedMesh`, read once. */
function deformedPositions(
  mesh: TetrahedralMesh3D,
  solution: ElasticSolution3D,
  scale: number,
): Point3D[] {
  return toArray(mesh.DeformedMesh(solution, scale).Positions);
}

// ---------------------------------------------------------------------------
// Scene 1 — one cube, four closed forms
//
// The smoke gate's model, live and under sliders: a unit cube of six tetrahedra
// under a uniform traction, with a symmetry plane on each face through the
// origin. It is the case where the finite-element answer is not an approximation
// — a linear tetrahedron represents a uniform strain field EXACTLY — so every
// reading has a closed form beside it.

interface CubeSpec {
  cells: number;
  youngsGPa: number;
  poisson: number;
  sigma: number;
}

function cubeRun(spec: CubeSpec): { solid: Solid; run: Attempt<SolidRun> } {
  const n = spec.cells;
  const solid = tetLattice(n, n, n, 1, 1, 1);
  const key = `cube:${n}:${spec.youngsGPa}:${spec.poisson}:${spec.sigma}`;
  const run = runSolid(key, () => {
    const constraints: PrescribedDisplacement[] = [];
    solid.positions.forEach((p, i) => {
      if (p.X === 0) constraints.push(new PrescribedDisplacement(new DofIndex(i * 3), 0));
      if (p.Y === 0) constraints.push(new PrescribedDisplacement(new DofIndex(i * 3 + 1), 0));
      if (p.Z === 0) constraints.push(new PrescribedDisplacement(new DofIndex(i * 3 + 2), 0));
    });
    // The pulled face, one traction per surface triangle. Traction is a force per
    // unit AREA in world axes, so the same vector on every triangle is a uniform
    // pull however the face happens to be cut up.
    const tractions: FaceTraction3D[] = [];
    for (let k = 0; k < n; k++) {
      for (let j = 0; j < n; j++) {
        const a = solid.node(n, j, k);
        const b = solid.node(n, j + 1, k);
        const c = solid.node(n, j + 1, k + 1);
        const d = solid.node(n, j, k + 1);
        const face = (p: number, q: number, r: number): TriangleFace =>
          new TriangleFace(new VertexIndex(p), new VertexIndex(q), new VertexIndex(r));
        tractions.push(new FaceTraction3D(face(a, b, c), new Vector3D(spec.sigma, 0, 0)));
        tractions.push(new FaceTraction3D(face(a, c, d), new Vector3D(spec.sigma, 0, 0)));
      }
    }
    return new ElasticModel3D(
      solid.mesh,
      materialOf(spec.youngsGPa, spec.poisson),
      fromArray(constraints),
      fromArray([]),
      fromArray(tractions),
      new Vector3D(0, 0, 0),
    );
  }, TIGHT);
  return { solid, run };
}

const uniaxial = sceneOf({
  id: 'uniaxial',
  title: 'One cube, four closed forms',
  description:
    'A unit cube of six tetrahedra under a uniform traction on one face, held by a symmetry plane on each ' +
    'face through the origin — the model the smoke gate pins the whole library against. A linear tetrahedron ' +
    'represents a uniform strain exactly, so this is not an approximation converging to an answer: the axial ' +
    'displacement IS sigma L / E, the transverse contraction IS -nu sigma / E, and the von Mises stress is the ' +
    'applied stress in every one of the six cells. All three are printed beside the closed form the demo ' +
    'computed itself. Pull the Poisson slider and the cube visibly necks; push it to zero and it stops. The ' +
    'cell count is a control because refining changes nothing but cost — the answer is already exact, and the ' +
    'solve time is what climbs.',
  plato: [
    'ElasticModel3D',
    'ElasticModel3D.SolveElastic',
    'ElasticModel3D.DofCount',
    'ElasticModel3D.StiffnessMatrix',
    'ElasticModel3D.VonMisesStresses',
    'ElasticSolution3D.DisplacementMagnitudes',
    'TetrahedralMesh3D.DeformedMesh',
    'FaceTraction3D',
    'PrescribedDisplacement',
    'DofIndex',
    'LinearSolveSettings',
    'EngineeringMaterial.ToLameParameters',
    'EngineeringMaterial.ShearModulus',
    'EngineeringMaterial.FirstLameParameter',
    'EngineeringMaterial.BulkModulus',
    'Color.Lerp',
    'Number.Saturate',
  ],
  viewer: SOLID_VIEW,
  controls: [
    { key: 'cells', label: 'Cells / axis', kind: 'select', options: ['1', '2'], def: 0 },
    { key: 'sigma', label: 'Traction MPa', kind: 'slider', min: 0.2, max: 5, step: 0.1, def: 1 },
    { key: 'youngs', label: 'E (GPa)', kind: 'slider', min: 50, max: 400, step: 10, def: 200 },
    { key: 'poisson', label: 'Poisson ratio', kind: 'slider', min: 0, max: 0.45, step: 0.01, def: 0.3 },
    { key: 'exagg', label: 'Exaggeration 10^n', kind: 'slider', min: 2, max: 6, step: 0.5, def: 4.5 },
    { key: 'colour', label: 'Colour by', kind: 'select', options: COLOUR_MODES, def: 0 },
    { key: 'edges', label: 'Edges', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const cells = clampIndex(params.cells, 2) + 1;
    const sigma = params.sigma * 1e6;
    const youngs = params.youngs;
    const poisson = params.poisson;
    const scale = exaggeration(params.exagg);
    const { solid, run } = cubeRun({ cells, youngsGPa: youngs, poisson, sigma });
    const material = materialOf(youngs, poisson);

    const object = new THREE.Group();
    object.position.set(-0.5, -0.5, -0.5);

    if (!run.value) {
      object.add(
        new THREE.Mesh(
          boundaryGeometry(solid, solid.positions, () => new THREE.Color(palette.surface)),
          vertexMaterial(),
        ),
      );
      return {
        object,
        readings: [
          note('ElasticModel3D.SolveElastic', `UNAVAILABLE (${run.error})`),
          note('cells', String(solid.cells.length)),
        ],
      };
    }

    const value = run.value;
    const moved = deformedPositions(solid.mesh, value.solution, scale);
    const stressRange = rangeOf(value.vonMises);
    const displacementRange = rangeOf(value.magnitudes);
    const mode = clampIndex(params.colour, COLOUR_MODES.length);
    object.add(
      new THREE.Mesh(
        boundaryGeometry(solid, moved, solidColourer(mode, value, stressRange, displacementRange)),
        vertexMaterial(),
      ),
    );
    if (params.edges >= 0.5) object.add(segments(boundaryEdges(solid, moved), 0x1a2230));
    // The undeformed outline, so the exaggerated shape has something to be
    // exaggerated against.
    object.add(segments(boundaryEdges(solid, solid.positions), 0x36405a, 0.5));

    const far = solid.node(cells, cells, cells);
    const axial = value.displacements[far].X;
    const transverse = value.displacements[far].Y;
    const axialClosed = (sigma * 1) / (youngs * 1e9);
    const transverseClosed = (-poisson * sigma) / (youngs * 1e9);

    return {
      object,
      readings: [
        note('cells', `${solid.cells.length} tetrahedra`),
        reading('ElasticModel3D.DofCount', () => String(value.model.DofCount())),
        note('SparseMatrix entries', `${value.entries} (144 per cell)`),
        ...convergence(
          value.solution.Iterations,
          value.solution.Residual,
          value.solution.Converged,
          value.solveMs,
        ),
        note('axial u_x', `${sci(axial)} m vs sigma L / E = ${sci(axialClosed)} m`),
        note('ratio', n3(axial / axialClosed)),
        note('transverse u_y', `${sci(transverse)} m vs -nu sigma / E = ${sci(transverseClosed)} m`),
        note('ratio', transverseClosed === 0 ? 'n/a (nu = 0, nothing to contract)' : n3(transverse / transverseClosed)),
        note(
          'VonMisesStresses',
          `${mpa(stressRange[0])} .. ${mpa(stressRange[1])} vs applied ${mpa(sigma)}`,
        ),
        note('stress recovery', ms(value.stressMs)),
        note('peak displacement', `${sci(displacementRange[1])} m`),
        note('drawn at', `x${scale.toExponential(1)}`),
        note('plot faceting', 'one flat value per cell — constant-strain element, not a drawing bug'),
        ...materialReadings(material),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 2 — the same cube under its own weight

const gravity = sceneOf({
  id: 'gravity',
  title: 'Its own weight',
  description:
    'The same cube with the traction removed, hung from its top face, carrying nothing but gravity. Gravity ' +
    'is an ACCELERATION on ElasticModel3D, not a force: the body force follows from the material density and ' +
    'each element volume, and GravityContributions shares a cell\'s weight equally among its four nodes — ' +
    'which is exact rather than lumped, because a linear tetrahedron\'s shape functions each integrate to a ' +
    'quarter of its volume. The reading that proves it is the load vector total against rho V g, which is ' +
    'independent of how the cube was cut into tetrahedra. At the default steel and 9.81 m/s^2 it is the smoke ' +
    'gate\'s -77008.5 N.',
  plato: [
    'ElasticModel3D',
    'ElasticModel3D.LoadVector',
    'ElasticModel3D.LoadContributions',
    'ElasticModel3D.SolveElastic',
    'ElasticModel3D.VonMisesStresses',
    'ElasticSolution3D.DisplacementMagnitudes',
    'TetrahedralMesh3D.DeformedMesh',
    'Density',
    'PrescribedDisplacement',
  ],
  viewer: SOLID_VIEW,
  controls: [
    { key: 'cells', label: 'Cells / axis', kind: 'select', options: ['1', '2'], def: 0 },
    { key: 'g', label: 'Gravity m/s^2', kind: 'slider', min: 0, max: 30, step: 0.5, def: 9.81 },
    { key: 'density', label: 'Density kg/m^3', kind: 'slider', min: 500, max: 12000, step: 50, def: 7850 },
    { key: 'youngs', label: 'E (GPa)', kind: 'slider', min: 20, max: 400, step: 10, def: 200 },
    { key: 'exagg', label: 'Exaggeration 10^n', kind: 'slider', min: 3, max: 7, step: 0.5, def: 6 },
    { key: 'colour', label: 'Colour by', kind: 'select', options: COLOUR_MODES, def: 1 },
  ],
  build(params: Params): Built {
    const cells = clampIndex(params.cells, 2) + 1;
    const g = params.g;
    const density = params.density;
    const youngs = params.youngs;
    const solid = tetLattice(cells, cells, cells, 1, 1, 1);
    const scale = exaggeration(params.exagg);

    const key = `gravity:${cells}:${g}:${density}:${youngs}`;
    const run = runSolid(key, () => {
      const constraints: PrescribedDisplacement[] = [];
      solid.positions.forEach((p, i) => {
        if (p.Y === 1) {
          for (let r = 0; r < 3; r++) {
            constraints.push(new PrescribedDisplacement(new DofIndex(i * 3 + r), 0));
          }
        }
      });
      return new ElasticModel3D(
        solid.mesh,
        materialOf(youngs, 0.3, density),
        fromArray(constraints),
        fromArray([]),
        fromArray([]),
        new Vector3D(0, -g, 0),
      );
    }, TIGHT);

    const object = new THREE.Group();
    object.position.set(-0.5, -0.5, -0.5);

    if (!run.value) {
      object.add(
        new THREE.Mesh(
          boundaryGeometry(solid, solid.positions, () => new THREE.Color(palette.surface)),
          vertexMaterial(),
        ),
      );
      return { object, readings: [note('SolveElastic', `UNAVAILABLE (${run.error})`)] };
    }

    const value = run.value;
    const moved = deformedPositions(solid.mesh, value.solution, scale);
    const stressRange = rangeOf(value.vonMises);
    const displacementRange = rangeOf(value.magnitudes);
    const mode = clampIndex(params.colour, COLOUR_MODES.length);
    object.add(
      new THREE.Mesh(
        boundaryGeometry(solid, moved, solidColourer(mode, value, stressRange, displacementRange)),
        vertexMaterial(),
      ),
    );
    object.add(segments(boundaryEdges(solid, moved), 0x1a2230));
    object.add(segments(boundaryEdges(solid, solid.positions), 0x36405a, 0.5));

    const loadTotal = reading('ElasticModel3D.LoadVector total', () => {
      const loads = value.model.LoadVector();
      let total = 0;
      for (let i = 0; i < loads.Count(); i++) total += loads.At(i);
      return `${n1(total)} N vs rho V g = ${n1(-density * 1 * g)} N`;
    });

    return {
      object,
      readings: [
        note('cells', `${solid.cells.length} tetrahedra`),
        reading('DofCount', () => String(value.model.DofCount())),
        loadTotal,
        reading('LoadContributions', () => `${value.model.LoadContributions().Count()} DofLoads`),
        ...convergence(
          value.solution.Iterations,
          value.solution.Residual,
          value.solution.Converged,
          value.solveMs,
        ),
        note('peak displacement', `${sci(displacementRange[1])} m`),
        note('von Mises', `${mpa(stressRange[0])} .. ${mpa(stressRange[1])}`),
        note('drawn at', `x${scale.toExponential(1)}`),
        note('gravity is an acceleration', 'the body force follows from Density and element volume'),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 3 — a bar in bending, and what the element costs
//
// The scene the page would most like to be a triumph and is instead an honest
// result. A cantilever of constant-strain tetrahedra under a tip load, read
// against P L^3 / 3EI. The library is right; the ELEMENT is stiff, which is the
// first thing anybody learns about linear tetrahedra and the reason nobody ships
// them for bending.

const BAR_MESHES: [number, number, number][] = [[1, 1, 1], [2, 1, 1], [3, 1, 1]];
const BAR_LABELS = ['1x1x1', '2x1x1', '3x1x1'];

const cantilever = sceneOf({
  id: 'cantilever',
  title: 'A bar in bending',
  description:
    'A cantilever of constant-strain tetrahedra, built in at x = 0 and pulled down at its free end, coloured ' +
    'by von Mises stress and drawn deformed. The tip deflection is printed against the textbook P L^3 / 3EI ' +
    'and the ratio is nowhere near one — well under a fifth at one element, climbing steadily as the mesh is ' +
    'refined. That is not a defect in the solve: a linear tetrahedron has a CONSTANT strain, so it cannot ' +
    'represent the linearly varying bending strain through the depth at all, and a mesh one element thick is ' +
    'famously several times too stiff. The convergence is from below and it is slow, which is exactly why ' +
    'plato-427 files the hexahedral and quadratic elements. What the coarse mesh does show honestly is where ' +
    'the stress is: highest at the built-in end, near zero at the free one. Refinement is capped at three ' +
    'elements because the solve cost grows with the cube of the iteration count.',
  plato: [
    'ElasticModel3D.SolveElastic',
    'ElasticModel3D.VonMisesStresses',
    'ElasticModel3D.StiffnessMatrix',
    'NodalForce3D',
    'PrescribedDisplacement',
    'TetrahedralMesh3D.DeformedMesh',
    'ElasticSolution3D.DisplacementMagnitudes',
    'TetrahedronCell.ElementStrain',
    'StrainState3D.ToStressState',
    'StressState3D.VonMisesStress',
  ],
  viewer: { distance: 3.4, grid: false, spin: false },
  controls: [
    { key: 'mesh', label: 'Elements', kind: 'select', options: BAR_LABELS, def: 1 },
    { key: 'load', label: 'Tip load N', kind: 'slider', min: 200, max: 5000, step: 100, def: 1000 },
    { key: 'depth', label: 'Depth (m)', kind: 'slider', min: 0.15, max: 0.4, step: 0.01, def: 0.25 },
    { key: 'exagg', label: 'Exaggeration 10^n', kind: 'slider', min: 3, max: 6, step: 0.5, def: 5 },
    { key: 'colour', label: 'Colour by', kind: 'select', options: COLOUR_MODES, def: 0 },
    { key: 'edges', label: 'Edges', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const pick = clampIndex(params.mesh, BAR_MESHES.length);
    const [nx, ny, nz] = BAR_MESHES[pick];
    const span = 1;
    const depth = params.depth;
    const load = params.load;
    const solid = tetLattice(nx, ny, nz, span, depth, depth);
    const scale = exaggeration(params.exagg);

    const tipNodes: number[] = [];
    for (let k = 0; k <= nz; k++) for (let j = 0; j <= ny; j++) tipNodes.push(solid.node(nx, j, k));

    const key = `bar:${pick}:${depth}:${load}`;
    const run = runSolid(key, () => {
      const constraints: PrescribedDisplacement[] = [];
      solid.positions.forEach((p, i) => {
        if (p.X === 0) {
          for (let r = 0; r < 3; r++) {
            constraints.push(new PrescribedDisplacement(new DofIndex(i * 3 + r), 0));
          }
        }
      });
      const forces = tipNodes.map(
        n => new NodalForce3D(new VertexIndex(n), new Vector3D(0, -load / tipNodes.length, 0)),
      );
      return new ElasticModel3D(
        solid.mesh,
        materialOf(200, 0.3),
        fromArray(constraints),
        fromArray(forces),
        fromArray([]),
        new Vector3D(0, 0, 0),
      );
    }, LOOSE);

    const object = new THREE.Group();
    object.position.set(-span / 2, 0, -depth / 2);

    if (!run.value) {
      object.add(
        new THREE.Mesh(
          boundaryGeometry(solid, solid.positions, () => new THREE.Color(palette.surface)),
          vertexMaterial(),
        ),
      );
      return { object, readings: [note('SolveElastic', `UNAVAILABLE (${run.error})`)] };
    }

    const value = run.value;
    const moved = deformedPositions(solid.mesh, value.solution, scale);
    const stressRange = rangeOf(value.vonMises);
    const displacementRange = rangeOf(value.magnitudes);
    const mode = clampIndex(params.colour, COLOUR_MODES.length);
    object.add(
      new THREE.Mesh(
        boundaryGeometry(solid, moved, solidColourer(mode, value, stressRange, displacementRange)),
        vertexMaterial(),
      ),
    );
    if (params.edges >= 0.5) object.add(segments(boundaryEdges(solid, moved), 0x1a2230));
    object.add(segments(boundaryEdges(solid, solid.positions), 0x36405a, 0.45));

    // The load, drawn where it is applied.
    for (const n of tipNodes) {
      const p = moved[n];
      object.add(
        arrow(
          new THREE.Vector3(p.X, p.Y + depth * 0.9, p.Z),
          new THREE.Vector3(0, -1, 0),
          depth * 0.7,
          palette.surfaceAlt,
        ),
      );
    }

    let tip = 0;
    for (const n of tipNodes) tip += value.displacements[n].Y;
    tip /= tipNodes.length;
    const second = (depth * Math.pow(depth, 3)) / 12;
    const closed = -(load * Math.pow(span, 3)) / (3 * 200e9 * second);

    return {
      object,
      readings: [
        note('mesh', `${BAR_LABELS[pick]} boxes = ${solid.cells.length} tetrahedra`),
        reading('DofCount', () => String(value.model.DofCount())),
        note('entries', String(value.entries)),
        ...convergence(
          value.solution.Iterations,
          value.solution.Residual,
          value.solution.Converged,
          value.solveMs,
        ),
        note('tip deflection', `${sci(tip)} m`),
        note('P L^3 / 3EI', `${sci(closed)} m`),
        note('ratio', `${n3(tip / closed)} — the element is too stiff, not the solver`),
        note('EI', `${(200e9 * second).toExponential(3)} N m^2`),
        note('peak von Mises', mpa(stressRange[1])),
        note('von Mises range', `${mpa(stressRange[0])} .. ${mpa(stressRange[1])}`),
        note('peak displacement', `${sci(displacementRange[1])} m`),
        note('drawn at', `x${scale.toExponential(1)}`),
        note('nodal smoothing', 'DEFERRED (plato-427) — the plot is faceted per cell'),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Plane models
//
// The plane path is the same variational form with the third gradient component
// zero, so it reuses one element kernel. Two scenes go through it: the plate with
// a hole below, which is the classic stress concentration, and its plane
// stress / plane strain switch, which changes ONE number — the first Lame
// parameter — and nothing else.

interface Plane {
  positions: Point2D[];
  faces: TriangleFace[];
  mesh: TriangleMesh2D;
  edges: [number, number][];
}

function planeOf(positions: Point2D[], faces: TriangleFace[]): Plane {
  const seen = new Set<string>();
  const edges: [number, number][] = [];
  for (const f of faces) {
    const v = [f.A.Value, f.B.Value, f.C.Value];
    for (let i = 0; i < 3; i++) {
      const p = v[i];
      const q = v[(i + 1) % 3];
      const k = p < q ? `${p},${q}` : `${q},${p}`;
      if (seen.has(k)) continue;
      seen.add(k);
      edges.push([p, q]);
    }
  }
  return {
    positions,
    faces,
    mesh: new TriangleMesh2D(fromArray(positions), fromArray(faces)),
    edges,
  };
}

/**
 * A quarter of a plate with a central circular hole: a radial O-grid running
 * from the hole out to the quarter rectangle, graded so the elements bunch where
 * the stress does. The symmetry planes on the two cut edges make the quarter
 * stand for the whole plate, which is what makes the model small enough to solve
 * in a browser tick.
 */
const plateCache = new Map<string, Plane>();

function quarterPlate(angular: number, radial: number, half: number, hole: number): Plane {
  const key = `${angular}:${radial}:${half}:${hole}`;
  const hit = plateCache.get(key);
  if (hit) return hit;
  const positions: Point2D[] = [];
  const id = (a: number, j: number): number => j * (angular + 1) + a;
  for (let j = 0; j <= radial; j++) {
    for (let a = 0; a <= angular; a++) {
      const theta = (a / angular) * (Math.PI / 2);
      const c = Math.cos(theta);
      const s = Math.sin(theta);
      // The quarter rectangle's boundary along this ray.
      const reach = Math.min(c < 1e-9 ? 1e9 : half / c, s < 1e-9 ? 1e9 : half / s);
      const t = j / radial;
      const graded = t * t * 0.5 + t * 0.5;
      const r = hole + (reach - hole) * graded;
      positions.push(new Point2D(c * r, s * r));
    }
  }
  const faces: TriangleFace[] = [];
  for (let j = 0; j < radial; j++) {
    for (let a = 0; a < angular; a++) {
      const p = id(a, j);
      const q = id(a + 1, j);
      const r = id(a + 1, j + 1);
      const s = id(a, j + 1);
      faces.push(new TriangleFace(new VertexIndex(p), new VertexIndex(q), new VertexIndex(r)));
      faces.push(new TriangleFace(new VertexIndex(p), new VertexIndex(r), new VertexIndex(s)));
    }
  }
  const plate = planeOf(positions, faces);
  plateCache.set(key, plate);
  return plate;
}

interface PlaneRun {
  model: ElasticModel2D;
  solution: ElasticSolution2D;
  vonMises: number[];
  magnitudes: number[];
  displacements: Vector2D[];
  entries: number;
  solveMs: number;
}

const planeRuns = new Map<string, Attempt<PlaneRun>>();

function runPlane(key: string, build: () => ElasticModel2D, settings: LinearSolveSettings): Attempt<PlaneRun> {
  const hit = planeRuns.get(key);
  if (hit) return hit;
  const result = attempt<PlaneRun>(() => {
    const model = build();
    const t0 = performance.now();
    const raw = model.SolveElastic(settings);
    const solveMs = performance.now() - t0;
    // Materialized once, for the reason spelled out in `runSolid`.
    const displacements = toArray(raw.Displacements);
    const solution = new ElasticSolution2D(
      fromArray(displacements),
      raw.Iterations,
      raw.Residual,
      raw.Converged,
    );
    return {
      model,
      solution,
      vonMises: toArray(model.VonMisesStresses(solution)).map(p => p.Pascals),
      magnitudes: toArray(solution.DisplacementMagnitudes()).map(l => l.Meters),
      displacements,
      entries: model.StiffnessMatrix().Entries.Count(),
      solveMs,
    };
  });
  if (planeRuns.size > 8) planeRuns.clear();
  planeRuns.set(key, result);
  return result;
}

function planeGeometry(
  plane: Plane,
  positions: readonly Point2D[],
  colourOf: (face: number) => THREE.Color,
): THREE.BufferGeometry {
  const xyz: number[] = [];
  const rgb: number[] = [];
  plane.faces.forEach((f, i) => {
    const c = colourOf(i);
    for (const n of [f.A.Value, f.B.Value, f.C.Value]) {
      const p = positions[n];
      xyz.push(p.X, p.Y, 0);
      rgb.push(c.r, c.g, c.b);
    }
  });
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(xyz, 3));
  geometry.setAttribute('color', new THREE.Float32BufferAttribute(rgb, 3));
  return geometry;
}

function planeEdges(plane: Plane, positions: readonly Point2D[]): number[] {
  const out: number[] = [];
  for (const [p, q] of plane.edges) {
    out.push(positions[p].X, positions[p].Y, 0.001);
    out.push(positions[q].X, positions[q].Y, 0.001);
  }
  return out;
}

const PLATE_MESHES: [number, number][] = [[4, 2], [5, 2], [6, 2], [6, 3]];
const PLATE_LABELS = ['4x2', '5x2', '6x2', '6x3'];
const CONDITIONS = ['Stress', 'Strain'];

const plate = sceneOf({
  id: 'plate',
  title: 'Plane stress: a hole in a plate',
  description:
    'A quarter of a plate with a central circular hole, pulled in tension, under either plane idealization. ' +
    'The reading is the stress concentration factor: the peak von Mises stress divided by the applied far-field ' +
    'stress, against Kirsch\'s classical 3. A coarse constant-strain mesh under-predicts it badly, and the ' +
    'refinement control walks it upward — not monotonically, because at these mesh sizes which element holds ' +
    'the peak keeps changing, but the trend is unmistakable and the finest mesh this page can afford is still ' +
    'short of 3. Widening the hole raises it too, for a different reason: the net section carries the same ' +
    'force through less material. The idealization switch is the other point: plane stress and plane strain ' +
    'differ in ONE number, the first ' +
    'Lame parameter that PlaneLameParameters returns, and in nothing else — the same element kernel, the same ' +
    'assembly, the same solve. Switch it and watch lambda change while mu does not.',
  plato: [
    'ElasticModel2D',
    'ElasticModel2D.SolveElastic',
    'ElasticModel2D.DofCount',
    'ElasticModel2D.Moduli',
    'ElasticModel2D.VonMisesStresses',
    'ElasticModel2D.StiffnessMatrix',
    'EngineeringMaterial.PlaneLameParameters',
    'PlaneCondition',
    'NodalForce2D',
    'TriangleMesh2D.DeformedMesh',
    'ElasticSolution2D.DisplacementMagnitudes',
    'TriangleFace.ElementStrain',
    'StrainState2D.ToStressState',
    'StressState2D.OutOfPlaneStress',
    'StressState2D.ToStressState3D',
  ],
  viewer: PLANE_VIEW,
  controls: [
    { key: 'mesh', label: 'Mesh', kind: 'select', options: PLATE_LABELS, def: 0 },
    { key: 'condition', label: 'Plane', kind: 'select', options: CONDITIONS, def: 0 },
    { key: 'hole', label: 'Hole radius', kind: 'slider', min: 0.12, max: 0.4, step: 0.01, def: 0.25 },
    { key: 'sigma', label: 'Tension MPa', kind: 'slider', min: 0.2, max: 4, step: 0.1, def: 1 },
    { key: 'exagg', label: 'Exaggeration 10^n', kind: 'slider', min: 3, max: 6, step: 0.5, def: 4.5 },
    { key: 'edges', label: 'Mesh lines', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const pick = clampIndex(params.mesh, PLATE_MESHES.length);
    const [angular, radial] = PLATE_MESHES[pick];
    const half = 1;
    const hole = params.hole;
    const sigma = params.sigma * 1e6;
    const thickness = 0.01;
    const planeStress = clampIndex(params.condition, 2) === 0;
    const material = materialOf(200, 0.3);
    const shape = quarterPlate(angular, radial, half, hole);
    const scale = exaggeration(params.exagg);

    // The loaded edge: the outer ring's nodes at x = half, with a tributary
    // length each so the nodal forces add up to sigma * height * thickness.
    const edgeNodes: number[] = [];
    for (let a = 0; a <= angular; a++) {
      const n = a + radial * (angular + 1);
      if (Math.abs(shape.positions[n].X - half) < 1e-9) edgeNodes.push(n);
    }
    edgeNodes.sort((p, q) => shape.positions[p].Y - shape.positions[q].Y);

    const key = `plate:${pick}:${hole}:${sigma}:${planeStress}`;
    const run = runPlane(key, () => {
      const constraints: PrescribedDisplacement[] = [];
      shape.positions.forEach((p, i) => {
        if (Math.abs(p.X) < 1e-12) constraints.push(new PrescribedDisplacement(new DofIndex(i * 2), 0));
        if (Math.abs(p.Y) < 1e-12) {
          constraints.push(new PrescribedDisplacement(new DofIndex(i * 2 + 1), 0));
        }
      });
      const forces: NodalForce2D[] = [];
      edgeNodes.forEach((n, k) => {
        const y = shape.positions[n].Y;
        const before = k === 0 ? y : shape.positions[edgeNodes[k - 1]].Y;
        const after = k === edgeNodes.length - 1 ? y : shape.positions[edgeNodes[k + 1]].Y;
        const tributary = (y - before) / 2 + (after - y) / 2;
        forces.push(
          new NodalForce2D(new VertexIndex(n), new Vector2D(sigma * thickness * tributary, 0)),
        );
      });
      return new ElasticModel2D(
        shape.mesh,
        material,
        new Length(thickness),
        planeStress ? PlaneCondition.PlaneStress() : PlaneCondition.PlaneStrain(),
        fromArray(constraints),
        fromArray(forces),
        new Vector2D(0, 0),
      );
    }, LOOSE);

    const object = new THREE.Group();
    object.position.set(-half / 2, -half / 2, 0);

    if (!run.value) {
      object.add(
        new THREE.Mesh(
          planeGeometry(shape, shape.positions, () => new THREE.Color(palette.surface)),
          new THREE.MeshBasicMaterial({ vertexColors: true, side: THREE.DoubleSide }),
        ),
      );
      return { object, readings: [note('ElasticModel2D.SolveElastic', `UNAVAILABLE (${run.error})`)] };
    }

    const value = run.value;
    const moved = toArray(shape.mesh.DeformedMesh(value.solution, scale).Positions);
    const stressRange = rangeOf(value.vonMises);
    object.add(
      new THREE.Mesh(
        planeGeometry(shape, moved, i =>
          rampColor(spread(value.vonMises[i], stressRange[0], stressRange[1])),
        ),
        new THREE.MeshBasicMaterial({ vertexColors: true, side: THREE.DoubleSide }),
      ),
    );
    if (params.edges >= 0.5) object.add(segments(planeEdges(shape, moved), 0x1a2230, 0.8));
    object.add(segments(planeEdges(shape, shape.positions), 0x36405a, 0.4));
    object.add(
      dots(
        edgeNodes.map(n => new THREE.Vector3(moved[n].X, moved[n].Y, 0.002)),
        palette.surfaceAlt,
        7,
      ),
    );

    const lame = reading('EngineeringMaterial.PlaneLameParameters', () => {
      const m = value.model.Moduli();
      return `lambda ${mpa(m.FirstParameter.Pascals)}, mu ${mpa(m.ShearModulus.Pascals)}`;
    });

    return {
      object,
      readings: [
        note('idealization', planeStress ? 'PlaneStress' : 'PlaneStrain'),
        note('mesh', `${shape.faces.length} triangles, ${shape.positions.length} nodes`),
        reading('ElasticModel2D.DofCount', () => String(value.model.DofCount())),
        note('entries', `${value.entries} (36 per face)`),
        ...convergence(
          value.solution.Iterations,
          value.solution.Residual,
          value.solution.Converged,
          value.solveMs,
        ),
        note('peak von Mises', mpa(stressRange[1])),
        note('concentration Kt', `${n2(stressRange[1] / sigma)} vs Kirsch 3.00`),
        note('applied', mpa(sigma)),
        note('hole radius / half width', n2(hole / half)),
        lame,
        note('loaded edge', `${edgeNodes.length} nodes`),
        note('drawn at', `x${scale.toExponential(1)}`),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 5 — the Euler-Bernoulli beam against the textbook
//
// The cheap, checkable path, and the one place on this page where the finite
// element answer is EXACT rather than converging: a cubic Hermite element
// reproduces the exact deflected shape of an unloaded segment, so the nodal
// deflections match the closed form at ONE element and at twelve alike.

interface BeamCase {
  label: string;
  supports(span: number): unknown[];
  loads(span: number, magnitude: number): unknown[];
  /** The textbook deflection, given span, magnitude and EI. */
  closed(span: number, magnitude: number, ei: number): number;
  formula: string;
  /** Element counts that land a node on every feature. */
  even: boolean;
}

const BEAM_CASES: BeamCase[] = [
  {
    label: 'Cant P',
    supports: () => [new BeamSupport(BeamRestraint.Fixed(), new Length(0))],
    loads: (span, magnitude) => [BeamLoad.PointForce(new Length(span), magnitude)],
    closed: (span, magnitude, ei) => (magnitude * Math.pow(span, 3)) / (3 * ei),
    formula: 'P L^3 / 3EI',
    even: false,
  },
  {
    label: 'Cant UDL',
    supports: () => [new BeamSupport(BeamRestraint.Fixed(), new Length(0))],
    loads: (span, magnitude) => [
      BeamLoad.DistributedForce(new Length(0), new Length(span), magnitude / span),
    ],
    closed: (span, magnitude, ei) => ((magnitude / span) * Math.pow(span, 4)) / (8 * ei),
    formula: 'w L^4 / 8EI',
    even: false,
  },
  {
    label: 'SS P',
    supports: span => [
      new BeamSupport(BeamRestraint.Pinned(), new Length(0)),
      new BeamSupport(BeamRestraint.Roller(), new Length(span)),
    ],
    loads: (span, magnitude) => [BeamLoad.PointForce(new Length(span / 2), magnitude)],
    closed: (span, magnitude, ei) => (magnitude * Math.pow(span, 3)) / (48 * ei),
    formula: 'P L^3 / 48EI',
    even: true,
  },
  {
    label: 'SS UDL',
    supports: span => [
      new BeamSupport(BeamRestraint.Pinned(), new Length(0)),
      new BeamSupport(BeamRestraint.Roller(), new Length(span)),
    ],
    loads: (span, magnitude) => [
      BeamLoad.DistributedForce(new Length(0), new Length(span), magnitude / span),
    ],
    closed: (span, magnitude, ei) => (5 * (magnitude / span) * Math.pow(span, 4)) / (384 * ei),
    formula: '5 w L^4 / 384EI',
    even: true,
  },
  {
    label: 'Cant M',
    supports: () => [new BeamSupport(BeamRestraint.Fixed(), new Length(0))],
    loads: (span, magnitude) => [BeamLoad.Moment(new Length(span), magnitude)],
    closed: (span, magnitude, ei) => (magnitude * Math.pow(span, 2)) / (2 * ei),
    formula: 'M L^2 / 2EI',
    even: false,
  },
];

function beamOf(
  span: number,
  secondMoment: number,
  supports: unknown[],
  loads: unknown[],
): Beam {
  return new Beam(
    new Length(span),
    new SectionProperties(
      new Area(1e-4),
      new Point2D(0, 0),
      secondMoment,
      secondMoment,
      0,
      0,
      0,
      0,
    ),
    materialOf(200, 0.3),
    fromArray(supports as BeamSupport[]),
    fromArray(loads as never[]),
  );
}

interface BeamAnswer {
  positions: number[];
  deflections: number[];
  rotations: number[];
  iterations: number;
  residual: number;
  converged: boolean;
  maximum: number;
  solveMs: number;
}

const beamRuns = new Map<string, Attempt<BeamAnswer>>();

function runBeam(key: string, beam: Beam, elements: number): Attempt<BeamAnswer> {
  const hit = beamRuns.get(key);
  if (hit) return hit;
  const result = attempt<BeamAnswer>(() => {
    const t0 = performance.now();
    const solution = beam.SolveBeam(elements, new LinearSolveSettings(4000, 1e-12));
    const solveMs = performance.now() - t0;
    return {
      positions: toArray(solution.Positions).map(l => l.Meters),
      deflections: toArray(solution.Deflections),
      rotations: toArray(solution.Rotations),
      iterations: solution.Iterations,
      residual: solution.Residual,
      converged: solution.Converged,
      maximum: solution.MaximumDeflection(),
      solveMs,
    };
  });
  if (beamRuns.size > 24) beamRuns.clear();
  beamRuns.set(key, result);
  return result;
}

const beams = sceneOf({
  id: 'beam',
  title: 'The beam against the textbook',
  description:
    'The Euler-Bernoulli path, five load cases, and the closed form beside every one. This is the case where ' +
    'a finite element answer is EXACT rather than converging: a cubic Hermite element reproduces the exact ' +
    'deflected shape of a segment carrying no load between its ends, so the nodal deflections match the ' +
    'textbook formula at ONE element as well as at twelve — the sweep printed below shows the relative error ' +
    'sitting at rounding for every count. The distributed cases are exact too, because the consistent nodal ' +
    'loads are integrated from the Hermite antiderivatives rather than lumped. Two limits stated where a ' +
    'reader will see them: the model carries deflection and rotation per node and NO axial degree of freedom, ' +
    'so Pinned and Roller restrain exactly the same thing here (the toggle proves it — the answer does not ' +
    'move), and it cannot see axial force or buckling at all.',
  plato: [
    'Beam.SolveBeam',
    'Beam.BendingStiffness',
    'Beam.BeamNodePositions',
    'Beam.BeamStiffnessEntries',
    'Beam.BeamDistributedContributions',
    'Beam.BeamLoadVector',
    'Beam.BeamConstraints',
    'Beam.BeamNearestNode',
    'BeamSolution.MaximumDeflection',
    'BeamRestraint.RestrainedDofCount',
    'BeamLoad',
    'BeamSupport',
    'SectionProperties',
  ],
  viewer: { distance: 3.2, grid: false, spin: false, orthographic: true },
  controls: [
    { key: 'load', label: 'Load case', kind: 'select', options: BEAM_CASES.map(c => c.label), def: 0 },
    { key: 'elements', label: 'Elements', kind: 'slider', min: 2, max: 12, step: 2, def: 6 },
    { key: 'magnitude', label: 'Magnitude', kind: 'slider', min: 100, max: 2000, step: 50, def: 1000 },
    { key: 'stiffness', label: 'I x 10^-8 m^4', kind: 'slider', min: 1, max: 12, step: 0.5, def: 4 },
    { key: 'swap', label: 'Pinned for Roller', kind: 'toggle', def: 0 },
    { key: 'sweep', label: 'Sweep 1..8', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const pick = clampIndex(params.load, BEAM_CASES.length);
    const kase = BEAM_CASES[pick];
    const span = 2;
    const elements = Math.max(kase.even ? 2 : 1, Math.round(params.elements));
    const magnitude = params.magnitude;
    const secondMoment = params.stiffness * 1e-8;
    const swap = params.swap >= 0.5;

    // The one difference the toggle makes: a Roller becomes a Pinned support.
    // In a transverse-only model those restrain the same degree of freedom, so
    // the answer is expected to be bit-identical — that is the point.
    const supports = kase.supports(span).map(s => {
      const support = s as BeamSupport;
      return swap && !support.Restraint.IsFixed()
        ? new BeamSupport(BeamRestraint.Pinned(), support.Position)
        : support;
    });
    const beam = beamOf(span, secondMoment, supports, kase.loads(span, magnitude));
    const ei = beam.BendingStiffness();
    const closed = kase.closed(span, magnitude, ei);

    // The element count is part of the solve key but not of the beam itself, so
    // the convergence sweep below is memoized independently of the slider.
    const caseKey = `beam:${pick}:${magnitude}:${secondMoment}:${swap}`;
    const run = runBeam(`${caseKey}:${elements}`, beam, elements);

    const object = new THREE.Group();

    // The undeformed axis.
    object.add(segments([-span / 2, 0, 0, span / 2, 0, 0], 0x3d4a60));

    if (!run.value) {
      return { object, readings: [note('Beam.SolveBeam', `UNAVAILABLE (${run.error})`)] };
    }

    const value = run.value;
    // Deflections are positive DOWNWARD, so the drawn curve negates them. The
    // beam is floppy on purpose at the defaults — with EI in the thousands a
    // kilonewton really does move it a fraction of the span — so no exaggeration
    // is applied and none is claimed.
    const curve: number[] = [];
    const nodes: THREE.Vector3[] = [];
    for (let i = 0; i < value.positions.length; i++) {
      const x = value.positions[i] - span / 2;
      const y = -value.deflections[i];
      nodes.push(new THREE.Vector3(x, y, 0));
      if (i + 1 < value.positions.length) {
        curve.push(x, y, 0);
        curve.push(value.positions[i + 1] - span / 2, -value.deflections[i + 1], 0);
      }
    }
    object.add(segments(curve, palette.accent));
    object.add(dots(nodes, palette.line, 8));

    // A tangent tick at each node, drawn from the rotation the solution carries:
    // the second degree of freedom, and the reason the element is exact between
    // nodes rather than only at them.
    const tangents: number[] = [];
    const tick = span / (value.positions.length * 3);
    for (let i = 0; i < nodes.length; i++) {
      const slope = -value.rotations[i];
      tangents.push(nodes[i].x - tick, nodes[i].y - tick * slope, 0);
      tangents.push(nodes[i].x + tick, nodes[i].y + tick * slope, 0);
    }
    object.add(segments(tangents, palette.surfaceAlt, 0.85));

    // Supports and loads, so the case is readable without the label.
    for (const s of supports) {
      const support = s as BeamSupport;
      const x = support.Position.Meters - span / 2;
      object.add(segments([x, 0.02 * span, 0, x, -0.02 * span, 0], palette.surface));
      object.add(dots([new THREE.Vector3(x, 0, 0)], palette.surface, 11));
    }

    const sweep: Reading[] = [];
    if (params.sweep >= 0.5) {
      const counts = kase.even ? [2, 4, 6, 8] : [1, 2, 4, 8];
      const parts: string[] = [];
      for (const n of counts) {
        const swept = runBeam(`${caseKey}:${n}`, beam, n);
        parts.push(
          swept.value
            ? `n=${n} ${Math.abs(swept.value.maximum / closed - 1).toExponential(1)}`
            : `n=${n} UNAVAILABLE`,
        );
      }
      sweep.push(note('relative error vs the closed form', parts.join(', ')));
    }

    return {
      object,
      readings: [
        note('case', `${kase.label} — ${kase.formula}`),
        note('elements', `${elements} (${2 * (elements + 1)} degrees of freedom)`),
        reading('Beam.BendingStiffness', () => `${ei.toExponential(4)} N m^2`),
        reading('Beam.BeamStiffnessEntries', () =>
          String(beam.BeamStiffnessEntries(elements).Count()),
        ),
        reading('Beam.BeamLoadVector', () => String(beam.BeamLoadVector(elements).Count())),
        reading('Beam.BeamConstraints', () => String(beam.BeamConstraints(elements).Count())),
        ...convergence(value.iterations, value.residual, value.converged, value.solveMs),
        reading('BeamSolution.MaximumDeflection', () => `${value.maximum.toFixed(6)} m`),
        note('closed form', `${closed.toFixed(6)} m`),
        note('relative error', Math.abs(value.maximum / closed - 1).toExponential(2)),
        ...sweep,
        note('rotation at the far node', `${value.rotations[value.rotations.length - 1].toExponential(4)} rad`),
        reading('RestrainedDofCount', () =>
          `Fixed ${BeamRestraint.Fixed().RestrainedDofCount()}, Pinned ` +
          `${BeamRestraint.Pinned().RestrainedDofCount()}, Roller ` +
          `${BeamRestraint.Roller().RestrainedDofCount()}, Free ` +
          `${BeamRestraint.Free().RestrainedDofCount()}`,
        ),
        note('transverse only', 'no axial degree of freedom — Pinned and Roller coincide'),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 6 — inside one element
//
// The mechanism rather than the result, and the one scene that solves nothing.
// A single tetrahedron whose fourth vertex is under sliders: its four shape
// gradients drawn as arrows, and its 144 stiffness entries drawn as a 12x12
// plate. Both come straight out of the generated members, and both change as the
// element is distorted — including the moment it goes flat and the gradients
// blow up, which is what a degenerate element is.

const ELEMENT_SHOW = ['Element', 'Matrix', 'Both'];

const element = sceneOf({
  id: 'element',
  title: 'Inside one element',
  description:
    'One constant-strain tetrahedron and nothing else — no assembly, no solve. Its four shape-function ' +
    'gradients are drawn from the nodes they belong to: gradient i points from the face opposite node i ' +
    'towards node i, with magnitude one over the distance between them, which is why they get longer as the ' +
    'element gets flatter. They sum to zero, always, because the shape functions sum to one everywhere — the ' +
    'reading below prints that sum. Beside it is the element stiffness itself: 144 entries as a 12x12 plate, ' +
    'four nodes by three components each, coloured by magnitude and symmetric about its diagonal. Every row ' +
    'sums to zero as well, which is the statement that a rigid-body translation stores no energy. Flatten the ' +
    'element with the height slider and watch both the gradients and the entries diverge.',
  plato: [
    'TetrahedronCell.ShapeGradients',
    'TetrahedronCell.StiffnessEntries',
    'TetrahedronCell.CellVertices',
    'TetrahedronCell.ToTetrahedron',
    'TetrahedronCell.ElementStrain',
    'Tetrahedron.Volume',
    'StrainState3D.ToStressState',
    'StressState3D.VonMisesStress',
    'EngineeringMaterial.ToLameParameters',
    'SparseMatrixEntry',
    'Point3D.SixTimesSignedVolume',
  ],
  viewer: { distance: 4.4, grid: false, spin: false },
  controls: [
    { key: 'show', label: 'Show', kind: 'select', options: ELEMENT_SHOW, def: 2 },
    { key: 'height', label: 'Apex height', kind: 'slider', min: 0.08, max: 1.6, step: 0.02, def: 0.9 },
    { key: 'lean', label: 'Apex lean', kind: 'slider', min: -0.8, max: 0.8, step: 0.02, def: 0.2 },
    { key: 'poisson', label: 'Poisson ratio', kind: 'slider', min: 0, max: 0.45, step: 0.01, def: 0.3 },
    { key: 'stretch', label: 'Test stretch', kind: 'slider', min: 0, max: 0.02, step: 0.001, def: 0.01 },
  ],
  build(params: Params): Built {
    const show = clampIndex(params.show, ELEMENT_SHOW.length);
    const apex = new Point3D(params.lean, params.height, params.lean * 0.5);
    const corners = [new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 0, 1), apex];
    const positions = fromArray(corners);
    const cell = new TetrahedronCell(
      new VertexIndex(0),
      new VertexIndex(1),
      new VertexIndex(2),
      new VertexIndex(3),
    );
    const material = materialOf(200, params.poisson);
    const moduli = material.ToLameParameters();

    const object = new THREE.Group();
    object.position.set(-0.4, -0.4, -0.4);

    const gradients = attempt(() => toArray(cell.ShapeGradients(positions)));
    const entries = attempt(() => toArray(cell.StiffnessEntries(positions, moduli)));

    if (show !== 1) {
      // The tetrahedron: four triangles, drawn translucent so the gradients read.
      const faces = [[0, 1, 2], [0, 1, 3], [0, 2, 3], [1, 2, 3]];
      const xyz: number[] = [];
      for (const f of faces) for (const n of f) xyz.push(corners[n].X, corners[n].Y, corners[n].Z);
      const geometry = new THREE.BufferGeometry();
      geometry.setAttribute('position', new THREE.Float32BufferAttribute(xyz, 3));
      geometry.computeVertexNormals();
      const surface = new THREE.MeshStandardMaterial({
        color: palette.surface,
        transparent: true,
        opacity: 0.4,
        side: THREE.DoubleSide,
        flatShading: true,
      });
      object.add(new THREE.Mesh(geometry, surface));
      const wire: number[] = [];
      for (const [p, q] of [[0, 1], [1, 2], [2, 0], [0, 3], [1, 3], [2, 3]]) {
        wire.push(corners[p].X, corners[p].Y, corners[p].Z);
        wire.push(corners[q].X, corners[q].Y, corners[q].Z);
      }
      object.add(segments(wire, 0x8fa6c8));
      object.add(
        dots(corners.map(p => new THREE.Vector3(p.X, p.Y, p.Z)), palette.line, 10),
      );

      if (gradients.value) {
        const longest = Math.max(
          1e-9,
          ...gradients.value.map(g => Math.sqrt(g.X * g.X + g.Y * g.Y + g.Z * g.Z)),
        );
        gradients.value.forEach((g, i) => {
          const magnitude = Math.sqrt(g.X * g.X + g.Y * g.Y + g.Z * g.Z);
          if (magnitude < 1e-9) return;
          object.add(
            arrow(
              new THREE.Vector3(corners[i].X, corners[i].Y, corners[i].Z),
              new THREE.Vector3(g.X, g.Y, g.Z),
              (0.75 * magnitude) / longest,
              i === 0 ? palette.surfaceAlt : palette.accent,
            ),
          );
        });
      }
    }

    if (show !== 0 && entries.value) {
      const dense = new Float64Array(144);
      for (const e of entries.value) dense[e.Row * 12 + e.Column] += e.Value;
      let peak = 0;
      for (const v of dense) peak = Math.max(peak, Math.abs(v));
      const plateGroup = matrixPlate(12, 12, 1.5, (r, c) => {
        const v = dense[r * 12 + c];
        return Math.abs(v) < peak * 1e-6 ? null : rampColor(Math.abs(v) / peak);
      });
      plateGroup.position.set(show === 2 ? 1.9 : 0.4, 0.4, 0.4);
      object.add(plateGroup);
    }

    // The strain a known stretch produces, read straight back out of the element:
    // node 1 pulled along +x and nothing else moved.
    const stretch = params.stretch;
    const displacements = fromArray([
      new Vector3D(0, 0, 0),
      new Vector3D(stretch, 0, 0),
      new Vector3D(0, 0, 0),
      new Vector3D(0, 0, 0),
    ]);

    const readings: Reading[] = [
      reading('Tetrahedron.Volume', () => n3(cell.ToTetrahedron(positions).Volume())),
      reading('TetrahedronCell.CellVertices', () => String(cell.CellVertices().Count())),
      reading('ShapeGradients magnitudes', () =>
        gradients.value
          ? gradients.value
            .map(g => n2(Math.sqrt(g.X * g.X + g.Y * g.Y + g.Z * g.Z)))
            .join(', ')
          : `UNAVAILABLE (${gradients.error})`,
      ),
      reading('gradient sum (must be 0)', () => {
        if (!gradients.value) throw new Error(gradients.error);
        let x = 0;
        let y = 0;
        let z = 0;
        for (const g of gradients.value) {
          x += g.X;
          y += g.Y;
          z += g.Z;
        }
        return `(${x.toExponential(1)}, ${y.toExponential(1)}, ${z.toExponential(1)})`;
      }),
      reading('StiffnessEntries', () =>
        entries.value ? `${entries.value.length} (12 x 12)` : `UNAVAILABLE (${entries.error})`,
      ),
      reading('row 0 sum (rigid body, must be 0)', () => {
        if (!entries.value) throw new Error(entries.error);
        let total = 0;
        let scaleOf = 0;
        for (const e of entries.value) {
          if (e.Row === 0) total += e.Value;
          scaleOf = Math.max(scaleOf, Math.abs(e.Value));
        }
        return `${total.toExponential(1)} against entries of ${scaleOf.toExponential(1)}`;
      }),
      reading('symmetry K(i,j) - K(j,i)', () => {
        if (!entries.value) throw new Error(entries.error);
        const dense = new Float64Array(144);
        for (const e of entries.value) dense[e.Row * 12 + e.Column] += e.Value;
        let worst = 0;
        for (let r = 0; r < 12; r++) {
          for (let c = 0; c < 12; c++) {
            worst = Math.max(worst, Math.abs(dense[r * 12 + c] - dense[c * 12 + r]));
          }
        }
        return worst.toExponential(1);
      }),
      reading('ElementStrain under a test stretch', () => {
        const strain = cell.ElementStrain(positions, displacements);
        return `e_xx ${strain.Matrix.M11.toExponential(3)}, e_yy ${strain.Matrix.M22.toExponential(3)}`;
      }),
      reading('StrainState3D.ToStressState', () => {
        const stress = cell.ElementStrain(positions, displacements).ToStressState(moduli);
        return `sigma_xx ${mpa(stress.Matrix.M11)}`;
      }),
      reading('StressState3D.VonMisesStress', () =>
        mpa(cell.ElementStrain(positions, displacements).ToStressState(moduli).VonMisesStress().Pascals),
      ),
      reading('LameParameters', () =>
        `lambda ${mpa(moduli.FirstParameter.Pascals)}, mu ${mpa(moduli.ShearModulus.Pascals)}`,
      ),
      note('no solve here', 'this scene calls the element kernel only'),
    ];

    return { object, readings };
  },
});

// ---------------------------------------------------------------------------
// Scene 7 — the assembled matrix
//
// The representation decision, drawn. `SparseMatrix` is a COORDINATE LIST whose
// stated invariant is that the value at a position is the SUM of the entries
// naming it, so assembly is a FlatMap over the elements and nothing else: no
// scatter, no accumulation, no ordering requirement, no mutable global matrix.
// The consequence is visible here — the entry count is a multiple of the element
// count, the distinct positions are far fewer, and every shared node shows up as
// a position several entries name.

const MATRIX_MODELS = ['Cube', 'Bar', 'Plate', 'Beam'];

const assembly = sceneOf({
  id: 'assembly',
  title: 'The assembled matrix',
  description:
    'The global stiffness matrix of four different models, drawn. It is a coordinate list — a bag of (row, ' +
    'column, value) entries whose stated invariant is that the value at a position is the SUM of the entries ' +
    'naming it — so assembling is concatenating one element\'s entries after another\'s and nothing more. ' +
    'That is why the entry count below is exactly 144 times the tetrahedron count (or 36 times the triangle ' +
    'count, or 16 times the beam element count) while the distinct positions are far fewer: every position a ' +
    'shared node reaches is named once per element touching it, and the duplicates ARE the assembly. It is ' +
    'also why the conjugate gradient never indexes the matrix — reading one position of a coordinate list is ' +
    'a scan of the whole list, while multiplying by a vector is one sweep. Nothing here solves anything: ' +
    'StiffnessMatrix is independent of the loads and the restraints, which is what lets a model be assembled ' +
    'once and solved under several of them.',
  plato: [
    'SparseMatrix',
    'SparseMatrixEntry',
    'SparseMatrixEntry.IsDiagonal',
    'SparseMatrix.JacobiInverseDiagonal',
    'ElasticModel3D.StiffnessMatrix',
    'ElasticModel2D.StiffnessMatrix',
    'Beam.BeamStiffnessEntries',
    'TetrahedronCell.StiffnessEntries',
    'TriangleFace.StiffnessEntries',
  ],
  viewer: { distance: 2.6, grid: false, spin: false, orthographic: true },
  controls: [
    { key: 'model', label: 'Model', kind: 'select', options: MATRIX_MODELS, def: 0 },
    { key: 'scale', label: 'Colour', kind: 'select', options: ['Log', 'Linear'], def: 0 },
    { key: 'diagonal', label: 'Diagonal only', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const pick = clampIndex(params.model, MATRIX_MODELS.length);
    const material = materialOf(200, 0.3);

    let matrix: SparseMatrix;
    let source: string;
    if (pick === 0) {
      const solid = tetLattice(1, 1, 1, 1, 1, 1);
      const model = new ElasticModel3D(
        solid.mesh,
        material,
        fromArray([]),
        fromArray([]),
        fromArray([]),
        new Vector3D(0, 0, 0),
      );
      matrix = model.StiffnessMatrix();
      source = `${solid.cells.length} tetrahedra x 144`;
    } else if (pick === 1) {
      const solid = tetLattice(2, 1, 1, 1, 0.25, 0.25);
      const model = new ElasticModel3D(
        solid.mesh,
        material,
        fromArray([]),
        fromArray([]),
        fromArray([]),
        new Vector3D(0, 0, 0),
      );
      matrix = model.StiffnessMatrix();
      source = `${solid.cells.length} tetrahedra x 144`;
    } else if (pick === 2) {
      const shape = quarterPlate(4, 2, 1, 0.25);
      const model = new ElasticModel2D(
        shape.mesh,
        material,
        new Length(0.01),
        PlaneCondition.PlaneStress(),
        fromArray([]),
        fromArray([]),
        new Vector2D(0, 0),
      );
      matrix = model.StiffnessMatrix();
      source = `${shape.faces.length} triangles x 36`;
    } else {
      const elements = 6;
      const beam = beamOf(
        2,
        4e-8,
        [new BeamSupport(BeamRestraint.Fixed(), new Length(0))],
        [BeamLoad.PointForce(new Length(2), 1000)],
      );
      const dofs = (elements + 1) * 2;
      matrix = new SparseMatrix(dofs, dofs, beam.BeamStiffnessEntries(elements));
      source = `${elements} beam elements x 16`;
    }

    const rows = matrix.NumRows;
    const columns = matrix.NumColumns;
    const entries = toArray(matrix.Entries);
    const dense = new Float64Array(rows * columns);
    const counts = new Int32Array(rows * columns);
    for (const e of entries) {
      dense[e.Row * columns + e.Column] += e.Value;
      counts[e.Row * columns + e.Column]++;
    }
    let distinct = 0;
    let repeats = 0;
    let bandwidth = 0;
    let peak = 0;
    for (let r = 0; r < rows; r++) {
      for (let c = 0; c < columns; c++) {
        const n = counts[r * columns + c];
        if (n === 0) continue;
        distinct++;
        repeats = Math.max(repeats, n);
        bandwidth = Math.max(bandwidth, Math.abs(r - c));
        peak = Math.max(peak, Math.abs(dense[r * columns + c]));
      }
    }

    const logScale = clampIndex(params.scale, 2) === 0;
    const diagonalOnly = params.diagonal >= 0.5;
    const object = new THREE.Group();
    object.add(
      matrixPlate(rows, columns, 2.2, (r, c) => {
        if (diagonalOnly && r !== c) return null;
        const value = Math.abs(dense[r * columns + c]);
        if (counts[r * columns + c] === 0 || value < peak * 1e-9) return null;
        const t = logScale
          ? (Math.log10(value / peak) + 6) / 6
          : value / peak;
        return rampColor(t);
      }),
    );

    const jacobi = reading('SparseMatrix.JacobiInverseDiagonal', () => {
      const inverse = matrix.JacobiInverseDiagonal();
      let ones = 0;
      for (let i = 0; i < inverse.Count(); i++) if (inverse.At(i) === 1) ones++;
      return `${inverse.Count()} entries, ${ones} left at 1 (zero diagonal)`;
    });

    return {
      object,
      readings: [
        note('model', `${MATRIX_MODELS[pick]} — ${source}`),
        note('SparseMatrix', `${rows} x ${columns}`),
        reading('Entries.Count', () => String(matrix.Entries.Count())),
        note('distinct positions', `${distinct} of ${rows * columns} (${pct(distinct / (rows * columns))})`),
        note('entries per position', `up to ${repeats} — the duplicates ARE the assembly`),
        note('half bandwidth', String(bandwidth)),
        note('largest magnitude', peak.toExponential(3)),
        reading('SparseMatrixEntry.IsDiagonal', () => {
          let diagonal = 0;
          for (const e of entries) if (e.IsDiagonal()) diagonal++;
          return `${diagonal} of ${entries.length} entries`;
        }),
        jacobi,
        note('dense equivalent', `${rows * columns} numbers vs ${entries.length} entries stored`),
        note('assembly', 'a FlatMap over the elements — no scatter, no accumulation pass'),
      ],
    };
  },
});

// ---------------------------------------------------------------------------

const demo: Demo = {
  title: 'Finite elements',
  subtitle: 'finite-elements.{types,library}.plato · engineering.types.plato',
  scenes: [uniaxial, gravity, cantilever, plate, beams, element, assembly],
};

mountDemo(demo);

export { demo };
