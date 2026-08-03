// Cloth simulation — a scene catalog over `stdlib/future/cloth.{types,library}.plato`
// and the force vocabulary of `particles.types.plato`.
//
// This is a simulation page: `build` constructs a cloth and a display, `tick`
// asks Plato for one step and repacks the answer into the buffers `build`
// allocated. Nothing here integrates, projects a constraint or resolves a
// contact — `Cloth3D.Step`, `Cloth3D.StepMassSpring` and `Cloth3D.CollideWith`
// do all three, and the state threaded across frames is exactly what
// `cloth.library.plato` says it is: a `ClothMesh3D` and a `Number` clock.
// Everything else — forces, settings, colliders — is rebuilt from the sliders
// every frame, which is also what makes a parameter change a clean reset.
//
// Four things about the emitted library shape this file, and each is named at
// the reading that would have used it:
//
//   * Every emitted step returns a LAZILY mapped vertex array.
//     `Cloth3D.SubStepMassSpring` is `Vertices.Count().MapRange(...)`,
//     `Cloth3D.CollideWith` is `Vertices.Map(...)`, and `MapRange` is
//     `Intrinsics.Range(n).Map(f)` — a view, not an array. Chaining one step
//     onto the last across frames therefore stacks views, and each spring in the
//     mass-spring gather reads two elements of the layer below, so reading one
//     vertex of layer n costs O(springs^n). `settled` below materializes the
//     vertex array once per frame, which is the "write folds eagerly" rule of
//     the studio README applied at the one seam a demo owns. Left lazy, a
//     10 x 10 mass-spring cloth costs about 1.1 s per frame at two substeps and
//     grows from there; materialized it costs about 9 ms.
//   * `Cloth3D.SubStepMassSpring` is the one step whose output is read many
//     times before it is materialized, so `SubStepCount` above 1 is quadratic in
//     the spring count even with `settled` on the outside. The mass-spring scene
//     runs at one substep and says so.
//   * `Cloth3D.CollideWith` and `ClothMesh3D.CollideWith` keep only the sphere
//     overload; the plane and signed-distance-field forms carry
//     "Skipped: overload or duplicate member". The surviving body dispatches
//     `ProjectOutOf(point, offset)` on whatever it was handed, and `Plane` has an
//     overload of exactly that arity, so a plane argument runs the plane body
//     after all — verified, not assumed, by the resting-height reading in the
//     collision scene. The field form takes an extra `epsilon` and has no such
//     luck, so no scene here uses it.
//   * `PolygonMesh3D.ClothFromMesh` cannot run at all.
//     `VerticesOfUndirectedEdge` is written in the source as a record return and
//     emitted as `new Tuple2(a, b)`, whose fields are `X0` / `X1` rather than the
//     `VertexPair`'s `A` / `B`, so `MeshSpring` and `MeshBendConstraint` both
//     read `undefined.Value`. That takes the whole mesh-derived path — one spring
//     per undirected edge, one dihedral bend constraint per interior edge — with
//     it. The topology scene calls it by name and reports the failure rather than
//     assembling a constraint graph of its own.
//
// World and camera. The library is Z-up (`CONVENTIONS.md`) and the shared viewer
// is Y-up, so every scene hangs its content on one group rotated a quarter turn
// about X. That keeps every call idiomatic — gravity is (0, 0, -9.81), a ground
// plane is normal +Z at the height it sits at — and puts Plato's z = -1.6 exactly
// on the viewer's drawn grid.
//
// Cost. The Gauss-Seidel sweep rebuilds the whole vertex array once per
// constraint, so a sweep is O(vertices x constraints) and grid size is the
// dominant knob: at three iterations a 10 x 10 sheet (100 vertices, 502 springs)
// steps in about 3 ms and a 16 x 16 one (256 vertices, 1378 springs) in about
// 19 ms. Every grid slider here is capped accordingly, and every status line
// carries the measured per-frame cost so the trade is visible rather than
// described.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { fromArray, toArray } from '../shared/mesh.js';
import { palette, type ViewerOptions } from '../shared/viewer.js';
import {
  ClothGrid3D,
  ClothMesh3D,
  ClothSolverSettings,
  DampingCoefficient,
  Direction3D,
  Duration,
  FaceIndex,
  Frequency,
  JaggedArray,
  Length,
  Mass,
  MassSpringSettings,
  ParticleDrag,
  ParticleForces3D,
  ParticleGravity,
  Plane,
  Point3D,
  PolygonMesh3D,
  Sphere,
  Speed,
  Stiffness,
  UndirectedEdgeIndex,
  Vector3D,
  VertexIndex,
  WindModel,
  type IArray,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// The house contract, from `src/demos/polygons.ts`: a member that throws is a
// gap in the emitted library, not a fact about the cloth, so the status line
// keeps the member's name and the failure rather than substituting an answer of
// its own. On this page that matters twice over — `stdlib/future` is the tier
// the repo neither lints nor converts to C#, and nothing in the repo executes
// these bodies except this page.

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

function line(readings: readonly Reading[]): string {
  return readings.map(r => `${r.label} ${r.value}`).join('  ·  ');
}

const n1 = (x: number): string => (Number.isFinite(x) ? x.toFixed(1) : 'non-finite');
const n2 = (x: number): string => (Number.isFinite(x) ? x.toFixed(2) : 'non-finite');
const n3 = (x: number): string => (Number.isFinite(x) ? x.toFixed(3) : 'non-finite');

// ---------------------------------------------------------------------------
// The state threaded across frames, and the one repack the demo owes the solver

const EMPTY = <T,>(): IArray<T> => fromArray<T>([]);

/**
 * The cloth with its vertex array materialized.
 *
 * Every emitted step hands back a lazy view over the previous step's vertices
 * (`MapRange` and `Map` are both `Intrinsics.Range(n).Map(f)`), so the frame
 * loop would otherwise build a tower of views whose depth is the frame count.
 * One `MakeArray` per frame — O(vertices), against the O(vertices x constraints)
 * the step itself costs — collapses it. This is repacking, the demo's own job;
 * the arithmetic is entirely the library's.
 */
function settled(cloth: ClothMesh3D): ClothMesh3D {
  return new ClothMesh3D(
    cloth.Cloth.WithVertices(fromArray(toArray(cloth.Cloth.Vertices))),
    cloth.Faces,
  );
}

/** What a scene tells the solver about the world it is in. */
interface ForceSpec {
  drag?: number;
  wind?: WindModel | null;
}

/**
 * `ParticleForces3D` is six arrays. Wind is modelled as a velocity field rather
 * than a force, so it reaches the cloth only through the drag term — a wind with
 * no drag beside it is correctly inert, which the wind scene is built to show.
 */
function forcesOf(spec: ForceSpec): ParticleForces3D {
  const drag = spec.drag ?? 0;
  return new ParticleForces3D(
    fromArray([new ParticleGravity(new Vector3D(0, 0, -9.81))]),
    drag > 0 ? fromArray([new ParticleDrag(drag, 0)]) : EMPTY<ParticleDrag>(),
    EMPTY(),
    EMPTY(),
    EMPTY(),
    spec.wind ? fromArray([spec.wind]) : EMPTY<WindModel>(),
  );
}

/**
 * A fixed step, not the wall clock the shell hands `tick`. The mass-spring
 * solver's stability boundary is a function of the step size, so a step that
 * wandered with the frame rate would put the comparison scene's answer at the
 * mercy of the display refresh. Wall time is measured, but only for the
 * per-frame cost the status lines report.
 */
const STEP_SECONDS = 1 / 60;

function pbdSettings(iterations: number, substeps: number, stiffness: number, damping: number)
  : ClothSolverSettings {
  return new ClothSolverSettings(
    new Duration(STEP_SECONDS),
    Math.max(1, Math.round(substeps)),
    Math.max(1, Math.round(iterations)),
    stiffness,
    damping,
  );
}

function massSpringSettings(stiffness: number, damper: number, damping: number): MassSpringSettings {
  // One substep: `SubStepMassSpring` returns a lazy array that the next substep
  // reads once per spring per vertex, so two substeps is quadratic in the spring
  // count no matter what the caller materializes afterwards.
  return new MassSpringSettings(
    new Duration(STEP_SECONDS),
    1,
    new Stiffness(stiffness),
    new DampingCoefficient(damper),
    damping,
  );
}

// ---------------------------------------------------------------------------
// Grids
//
// `ClothGrid3D` carries two edge vectors rather than a plane and a spacing, so
// the same type is a flat sheet or a hanging curtain depending on where the row
// step points. Vertex (column, row) is index row * ColumnCount + column.

/** A horizontal sheet in the z = height plane: gravity swings it down. */
function sheetGrid(count: number, width: number, height: number): ClothGrid3D {
  const step = width / (count - 1);
  return new ClothGrid3D(
    new Point3D(-width / 2, -width / 2, height),
    new Vector3D(step, 0, 0),
    new Vector3D(0, step, 0),
    count,
    count,
  );
}

/** A vertical curtain: rows run downward, so row 0 is the hanging rail. */
function curtainGrid(columns: number, rows: number, width: number, height: number): ClothGrid3D {
  return new ClothGrid3D(
    new Point3D(-width / 2, 0, height / 2),
    new Vector3D(width / (columns - 1), 0, 0),
    new Vector3D(0, 0, -height / (rows - 1)),
    columns,
    rows,
  );
}

// ---------------------------------------------------------------------------
// Measuring the result
//
// The maximum stretch ratio against rest length is the honest stability reading
// for a cloth: it is 1 for one exactly at rest, sits just above 1 for a settled
// position-based solve, and climbs through orders of magnitude as an explicit
// solver loses control — decades before any position is actually non-finite,
// which is the only thing the scenes gate can see.

interface Stretch {
  max: number;
  constraints: number;
}

function stretchOf(cloth: ClothMesh3D): Stretch {
  const constraints = cloth.Cloth.DistanceConstraints;
  const vertices = cloth.Cloth.Vertices;
  let max = 0;
  const total = constraints.Count();
  for (let i = 0; i < total; i++) {
    const c = constraints.At(i);
    const a = vertices.At(c.VertexA.Value).Position;
    const b = vertices.At(c.VertexB.Value).Position;
    const ratio = a.Distance(b) / c.RestLength.Meters;
    if (ratio > max) max = ratio;
  }
  return { max, constraints: total };
}

/**
 * A metre bound past which a cloth has diverged rather than moved. An explicit
 * solver that has gone unstable does not reach a non-finite position in one
 * step: it doubles every step or two, so it spends a few dozen frames at
 * magnitudes like 1e40 — finite, drawable, and already meaningless — before
 * anything is actually NaN. Treating the runaway as the divergence is what lets
 * the comparison scene name the frame it happened on rather than the frame the
 * floating-point format ran out.
 */
const RUNAWAY = 1e6;

function inBounds(x: number, y: number, z: number): boolean {
  return (
    Number.isFinite(x) && Number.isFinite(y) && Number.isFinite(z) &&
    Math.abs(x) <= RUNAWAY && Math.abs(y) <= RUNAWAY && Math.abs(z) <= RUNAWAY
  );
}

function isSane(cloth: ClothMesh3D): boolean {
  const vertices = cloth.Cloth.Vertices;
  for (let i = 0; i < vertices.Count(); i++) {
    const p = vertices.At(i).Position;
    if (!inBounds(p.X, p.Y, p.Z)) return false;
  }
  return true;
}

// ---------------------------------------------------------------------------
// The display
//
// `build` allocates the buffers once and `tick` writes into them: the face
// tables never move, so a frame is a position substitution and nothing else,
// which is exactly what `ToPolygonMesh` promises. The shaded surface is the
// cloth's own triangles (`Cloth3D.Faces`) kept indexed so shared vertices
// average their normals; the wireframe is the polygon face table
// `ToPolygonMesh` carries — quads for a grid-built cloth, which is the topology
// claim worth drawing.

function clothMaterial(color: number): THREE.MeshStandardMaterial {
  return new THREE.MeshStandardMaterial({
    color,
    roughness: 0.55,
    metalness: 0.04,
    flatShading: false,
    side: THREE.DoubleSide,
  });
}

interface ClothView {
  object: THREE.Object3D;
  /** Rewrite the buffers from a stepped cloth; a runaway or NaN state is refused. */
  write(cloth: ClothMesh3D): boolean;
  vertexCount: number;
  faceCount: number;
  quadCount: number;
}

function clothView(
  cloth: ClothMesh3D,
  options: { color?: number; wire?: boolean; pins?: boolean } = {},
): ClothView {
  const vertexCount = cloth.Cloth.Vertices.Count();
  const group = new THREE.Group();

  // The shaded surface: the cloth's triangles, indexed.
  const surfacePositions = new Float32Array(vertexCount * 3);
  const surface = new THREE.BufferGeometry();
  surface.setAttribute('position', new THREE.BufferAttribute(surfacePositions, 3));
  const triangles = toArray(cloth.Cloth.Faces);
  const indices: number[] = [];
  for (const f of triangles) indices.push(f.A.Value, f.B.Value, f.C.Value);
  surface.setIndex(indices);
  group.add(new THREE.Mesh(surface, clothMaterial(options.color ?? palette.surface)));

  // The wireframe: the polygon face loops, deduplicated into undirected edges.
  const polygons = cloth.ToPolygonMesh();
  const quadCount = polygons.FaceCount();
  const edges: number[] = [];
  const seen = new Set<number>();
  for (let f = 0; f < quadCount; f++) {
    const loop = toArray(polygons.VerticesOfFace(new FaceIndex(f))).map(v => v.Value);
    for (let i = 0; i < loop.length; i++) {
      const a = loop[i];
      const b = loop[(i + 1) % loop.length];
      const key = a < b ? a * vertexCount + b : b * vertexCount + a;
      if (seen.has(key)) continue;
      seen.add(key);
      edges.push(a, b);
    }
  }
  const wirePositions = new Float32Array(edges.length * 3);
  let wire: THREE.LineSegments | null = null;
  if (options.wire !== false) {
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.BufferAttribute(wirePositions, 3));
    wire = new THREE.LineSegments(
      geometry,
      new THREE.LineBasicMaterial({ color: 0x0e1a26, transparent: true, opacity: 0.75 }),
    );
    group.add(wire);
  }

  // The pinned vertices, so a pin is visible as a pin.
  const pinned: number[] = [];
  for (let i = 0; i < vertexCount; i++) if (cloth.Cloth.Vertices.At(i).Pinned) pinned.push(i);
  const pinPositions = new Float32Array(pinned.length * 3);
  let pins: THREE.Points | null = null;
  if (options.pins !== false && pinned.length > 0) {
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.BufferAttribute(pinPositions, 3));
    pins = new THREE.Points(
      geometry,
      new THREE.PointsMaterial({ color: palette.surfaceAlt, size: 7, sizeAttenuation: false }),
    );
    group.add(pins);
  }

  const write = (next: ClothMesh3D): boolean => {
    const positions = next.Cloth.Positions();
    const xs = new Float64Array(vertexCount * 3);
    for (let i = 0; i < vertexCount; i++) {
      const p = positions.At(i);
      // A refused write leaves the last drawable state in the buffers, which is
      // also what keeps `computeBoundingSphere` from squaring a runaway into NaN.
      if (!inBounds(p.X, p.Y, p.Z)) return false;
      xs[i * 3] = p.X;
      xs[i * 3 + 1] = p.Y;
      xs[i * 3 + 2] = p.Z;
    }
    surfacePositions.set(xs);
    (surface.getAttribute('position') as THREE.BufferAttribute).needsUpdate = true;
    surface.computeVertexNormals();
    surface.computeBoundingSphere();
    if (wire) {
      for (let e = 0; e < edges.length; e++) {
        const v = edges[e] * 3;
        wirePositions[e * 3] = xs[v];
        wirePositions[e * 3 + 1] = xs[v + 1];
        wirePositions[e * 3 + 2] = xs[v + 2];
      }
      (wire.geometry.getAttribute('position') as THREE.BufferAttribute).needsUpdate = true;
      wire.geometry.computeBoundingSphere();
    }
    if (pins) {
      for (let k = 0; k < pinned.length; k++) {
        const v = pinned[k] * 3;
        pinPositions[k * 3] = xs[v];
        pinPositions[k * 3 + 1] = xs[v + 1];
        pinPositions[k * 3 + 2] = xs[v + 2];
      }
      (pins.geometry.getAttribute('position') as THREE.BufferAttribute).needsUpdate = true;
      pins.geometry.computeBoundingSphere();
    }
    return true;
  };

  write(cloth);
  return { object: group, write, vertexCount, faceCount: triangles.length, quadCount };
}

/** Plato is Z-up and the viewer is Y-up; one rotation reconciles them. */
function zUp(...children: THREE.Object3D[]): THREE.Group {
  const group = new THREE.Group();
  group.rotation.x = -Math.PI / 2;
  for (const child of children) group.add(child);
  return group;
}

function sphereObject(sphere: Sphere): THREE.Object3D {
  const mesh = new THREE.Mesh(
    new THREE.SphereGeometry(sphere.Radius, 40, 26),
    new THREE.MeshStandardMaterial({ color: palette.accent, roughness: 0.6, metalness: 0.05 }),
  );
  mesh.position.set(sphere.Center.X, sphere.Center.Y, sphere.Center.Z);
  return mesh;
}

/** A plane drawn where `Plane.ProjectOutOf` puts a contact: normal +Z at Distance. */
function planeObject(plane: Plane, extent: number): THREE.Object3D {
  const geometry = new THREE.PlaneGeometry(extent, extent);
  const mesh = new THREE.Mesh(
    geometry,
    new THREE.MeshStandardMaterial({
      color: 0x1c2530,
      roughness: 0.9,
      metalness: 0,
      side: THREE.DoubleSide,
    }),
  );
  mesh.position.set(0, 0, plane.Distance);
  return mesh;
}

/**
 * `Cloth3D.CollideWith` keeps only the sphere overload — the plane and
 * signed-distance-field forms are both dropped as
 * "Skipped: overload or duplicate member". The surviving body is
 * `Vertices.Map(v => v.MovedTo(argument.ProjectOutOf(v.Position, Thickness)))`,
 * and `Plane.ProjectOutOf(point, offset)` has exactly that arity, so a plane
 * argument runs the plane body rather than a wrong one. The collision scene
 * checks that claim numerically instead of trusting it: the resting height it
 * reports is the plane's own `Distance + Thickness`, which no other body would
 * produce.
 */
function collideWithPlane(cloth: ClothMesh3D, plane: Plane): ClothMesh3D {
  return (cloth as unknown as { CollideWith(p: Plane): ClothMesh3D }).CollideWith(plane);
}

// ---------------------------------------------------------------------------
// The simulation scene shape
//
// `build` is the reset, so all mutable state lives in the object `build`
// returns — looked up here through a `WeakMap` keyed by that object, which makes
// it impossible for two scenes, or a stale scene, to share a clock.

interface Sim {
  object: THREE.Object3D;
  /**
   * Advance one frame and report. The wall time the shell measures is not an
   * argument: the step size is fixed (see `STEP_SECONDS`), so a frame is a frame.
   */
  step(params: Params): Reading[];
  /** What to say before the first step. */
  initial(): Reading[];
}

const live = new WeakMap<THREE.Object3D, Sim>();

function simulation(spec: {
  id: string;
  title: string;
  description: string;
  plato: string[];
  controls: Control[];
  viewer?: ViewerOptions;
  start(params: Params): Sim;
}): Scene {
  // One per scene, assigned by `build` — which is the reset — and read only by
  // `status`, which the shell calls straight afterwards. Every other piece of
  // mutable state lives in the closure `start` creates, and `tick` finds it
  // through the object it was handed rather than through this.
  let latest: Sim | null = null;
  return {
    id: spec.id,
    title: spec.title,
    description: spec.description,
    plato: spec.plato,
    controls: spec.controls,
    // The stage's idle rotation would read as the cloth moving.
    viewer: { spin: false, ...(spec.viewer ?? {}) },
    build(params: Params): THREE.Object3D {
      const sim = spec.start(params);
      live.set(sim.object, sim);
      latest = sim;
      return sim.object;
    },
    status(): string {
      return latest ? line(latest.initial()) : '';
    },
    tick(seconds: number, params: Params, object: THREE.Object3D): string | void {
      // `seconds` is deliberately unused — see `STEP_SECONDS`.
      void seconds;
      const sim = live.get(object);
      if (!sim) return;
      return line(sim.step(params));
    },
  };
}

/** A small exponential average, so the cost reading does not flicker. */
class Cost {
  private value = 0;
  observe(ms: number): number {
    this.value = this.value === 0 ? ms : this.value * 0.9 + ms * 0.1;
    return this.value;
  }
  get ms(): number {
    return this.value;
  }
}

const integer = (x: number, low: number, high: number): number =>
  Math.min(high, Math.max(low, Math.round(x)));

// ---------------------------------------------------------------------------
// 1. Drape
//
// The one that has to work before any of the others mean anything: a flat sheet
// pinned along one edge, released, and left to find its own hanging shape.

const drape = simulation({
  id: 'drape',
  title: 'Drape and settle',
  description:
    'A flat sheet built by ClothFromGrid — structural springs along both axes, shear springs on both ' +
    'diagonals of every cell, two-away bend springs — pinned along row 0 by WithPinnedGridRow and ' +
    'released. Cloth3D.Step predicts each substep forward under gravity and drag, then sweeps the ' +
    'distance constraints in Gauss-Seidel order until the prediction satisfies them. Pinning is not a ' +
    'special case anywhere in the solver: SolverInverseMass reports zero for a pinned vertex and every ' +
    'projection already divides by inverse mass. Watch the maximum stretch ratio: it rises while the ' +
    'sheet is falling and then holds, which is what settling looks like in a number.',
  plato: [
    'ClothGrid3D.ClothFromGrid',
    'ClothMesh3D.WithPinnedGridRow',
    'ClothMesh3D.Step',
    'Cloth3D.SubStep',
    'Array.SolveDistance',
    'Array.SweepDistance',
    'Array.ProjectDistance',
    'ClothVertex.Integrate',
    'ParticleForces3D.AccelerationAt',
    'ClothMesh3D.ToPolygonMesh',
  ],
  viewer: { distance: 4.6, grid: true },
  controls: [
    { key: 'grid', label: 'Grid', kind: 'slider', min: 6, max: 13, step: 1, def: 10 },
    // A sweep is O(vertices x constraints), so the top of these two sliders is
    // where the page is willing to spend about 50 ms a frame and no more.
    { key: 'iterations', label: 'Iterations', kind: 'slider', min: 1, max: 4, step: 1, def: 3 },
    { key: 'substeps', label: 'Substeps', kind: 'slider', min: 1, max: 2, step: 1, def: 1 },
    { key: 'stiffness', label: 'Stiffness', kind: 'slider', min: 0.1, max: 1, step: 0.05, def: 1 },
    { key: 'compliance', label: 'Compliance', kind: 'slider', min: 0, max: 0.004, step: 0.0002, def: 0 },
    { key: 'damping', label: 'Velocity damping', kind: 'slider', min: 0, max: 0.1, step: 0.005, def: 0.03 },
    { key: 'wire', label: 'Face loops', kind: 'toggle', def: 1 },
  ],
  start(params: Params): Sim {
    const count = integer(params.grid, 6, 13);
    const grid = sheetGrid(count, 1.8, 0.6);
    let cloth = settled(
      grid.ClothFromGrid(new Mass(0.05), params.compliance, new Length(0.01))
        .WithPinnedGridRow(grid, 0),
    );
    const view = clothView(cloth, { wire: params.wire !== 0 });
    const cost = new Cost();
    let clock = 0;
    let frames = 0;

    const readings = (): Reading[] => {
      const stretch = stretchOf(cloth);
      return [
        note('grid', `${count} x ${count}`),
        note('vertices / springs', `${view.vertexCount} / ${stretch.constraints}`),
        note('iterations x substeps', `${integer(params.iterations, 1, 4)} x ${integer(params.substeps, 1, 2)}`),
        reading('IterationStiffness', () =>
          n3(params.stiffness.IterationStiffness(integer(params.iterations, 1, 4))),
        ),
        note('max stretch', `${n3(stretch.max)} x rest`),
        note('clock', `${n1(clock)} s, ${frames} frames`),
        note('step cost', `${n2(cost.ms)} ms/frame`),
      ];
    };

    return {
      object: zUp(view.object),
      initial: readings,
      step(current: Params): Reading[] {
        const settings = pbdSettings(
          integer(current.iterations, 1, 4),
          integer(current.substeps, 1, 2),
          current.stiffness,
          current.damping,
        );
        const started = performance.now();
        cloth = settled(cloth.Step(forcesOf({ drag: 0.1 }), settings, clock));
        cost.observe(performance.now() - started);
        clock += settings.TimeStep.Seconds;
        frames++;
        view.write(cloth);
        return readings();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// 2. The comparison the library exists to make

const solvers = simulation({
  id: 'solvers',
  title: 'Position-based beside mass-spring',
  description:
    'The same constraint graph read two ways, stepped side by side under the same forces. On the left, ' +
    'Cloth3D.Step projects each distance constraint as a position correction after integration. On the ' +
    'right, Cloth3D.StepMassSpring turns each one into a damped Hookean force before integration and ' +
    'integrates it explicitly. Position-based dynamics cannot add energy — a projection can only move a ' +
    'vertex toward satisfying a constraint — so its stretch ratio sits at about 1.01 whatever the ' +
    'stiffness. The explicit solver is stable only while the step is shorter than the spring period: ' +
    'raise Stiffness past about 70 N/m at this mass and step and it oscillates, then diverges, in about ' +
    'a second. The right-hand cloth freezes at its last finite state when it does, and the status line ' +
    'says which frame it went.',
  plato: [
    'ClothMesh3D.Step',
    'ClothMesh3D.StepMassSpring',
    'Cloth3D.SubStepMassSpring',
    'Array.SpringAccelerationOn',
    'Array.SpringForceOn',
    'ClothVertex.Integrate',
    'MassSpringSettings',
    'ClothSolverSettings',
  ],
  viewer: { distance: 5, grid: true },
  controls: [
    { key: 'grid', label: 'Grid', kind: 'slider', min: 5, max: 9, step: 1, def: 7 },
    { key: 'stiffness', label: 'Spring k (N/m)', kind: 'slider', min: 2, max: 200, step: 2, def: 20 },
    { key: 'damper', label: 'Spring damper', kind: 'slider', min: 0, max: 0.2, step: 0.01, def: 0.05 },
    { key: 'iterations', label: 'PBD iterations', kind: 'slider', min: 1, max: 6, step: 1, def: 3 },
    { key: 'damping', label: 'Velocity damping', kind: 'slider', min: 0, max: 0.1, step: 0.005, def: 0.03 },
  ],
  start(params: Params): Sim {
    const count = integer(params.grid, 5, 9);
    const grid = sheetGrid(count, 1.2, 0.5);
    const build = (): ClothMesh3D =>
      settled(grid.ClothFromGrid(new Mass(0.05), 0, new Length(0.01)).WithPinnedGridRow(grid, 0));

    let pbd = build();
    let spring = build();
    const pbdView = clothView(pbd, { color: palette.surface });
    const springView = clothView(spring, { color: palette.surfaceAlt });
    pbdView.object.position.x = -0.75;
    springView.object.position.x = 0.75;

    const cost = new Cost();
    let clock = 0;
    let frames = 0;
    let divergedAt = -1;

    const readings = (): Reading[] => {
      const a = stretchOf(pbd);
      const b = stretchOf(spring);
      return [
        note('grid', `${count} x ${count}, ${pbdView.vertexCount} vertices, ${a.constraints} springs each`),
        note('PBD max stretch', `${n3(a.max)} x rest`),
        note(
          'mass-spring max stretch',
          divergedAt >= 0
            ? `diverged at frame ${divergedAt} — StepMassSpring ran away past ${n3(b.max)} x rest`
            : `${n3(b.max)} x rest`,
        ),
        note('Stiffness', `${n1(params.stiffness)} N/m, damper ${n2(params.damper)} Ns/m, 1 substep`),
        note('clock', `${n1(clock)} s, ${frames} frames`),
        note('step cost', `${n2(cost.ms)} ms/frame for both`),
      ];
    };

    return {
      object: zUp(pbdView.object, springView.object),
      initial: readings,
      step(current: Params): Reading[] {
        const force = forcesOf({ drag: 0.1 });
        const started = performance.now();
        pbd = settled(
          pbd.Step(
            force,
            pbdSettings(integer(current.iterations, 1, 6), 1, 1, current.damping),
            clock,
          ),
        );
        if (divergedAt < 0) {
          const next = settled(
            spring.StepMassSpring(
              force,
              massSpringSettings(current.stiffness, current.damper, current.damping),
              clock,
            ),
          );
          // A diverged explicit solver throws nothing and draws nothing. Keeping
          // the last drawable state is a presentation choice, not a repair: the
          // reading above names the frame it happened on, and the stretch ratio
          // it froze at is the last honest number the solver produced.
          if (isSane(next)) spring = next;
          else divergedAt = frames;
        }
        cost.observe(performance.now() - started);
        clock += STEP_SECONDS;
        frames++;
        pbdView.write(pbd);
        if (divergedAt < 0) springView.write(spring);
        return readings();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// 3. Contacts

const draping = simulation({
  id: 'sphere',
  title: 'Over a sphere, onto the ground',
  description:
    'A free sheet dropped over a Sphere with a Plane under it. Contacts are composed after the step — ' +
    'cloth.Step(...).CollideWith(sphere).CollideWith(ground) — because the solve settles the cloth and ' +
    'then the contact has the last word on where a vertex may be. Each contact is one more position ' +
    'move with the previous position left alone, so the Verlet difference quotient turns the push into ' +
    'the velocity change a contact should produce: the normal component goes and the tangential one ' +
    'survives, which is sliding. The two readings are the check: no vertex ends up inside the sphere, ' +
    'and the resting height is the plane\'s Distance plus the cloth\'s own Thickness, which is what ' +
    'proves the surviving CollideWith body really ran the plane projection.',
  plato: [
    'ClothMesh3D.CollideWith',
    'Cloth3D.CollideWith',
    'Sphere.ProjectOutOf',
    'Plane.ProjectOutOf',
    'ClothVertex.MovedTo',
    'ClothMesh3D.Step',
    'Cloth3D.Thickness',
  ],
  viewer: { distance: 5, grid: true },
  controls: [
    { key: 'grid', label: 'Grid', kind: 'slider', min: 8, max: 14, step: 1, def: 11 },
    { key: 'radius', label: 'Sphere radius', kind: 'slider', min: 0.25, max: 0.8, step: 0.01, def: 0.55 },
    { key: 'thickness', label: 'Thickness', kind: 'slider', min: 0.005, max: 0.08, step: 0.005, def: 0.03 },
    { key: 'drop', label: 'Drop height', kind: 'slider', min: 0.2, max: 1.4, step: 0.05, def: 0.8 },
    { key: 'iterations', label: 'Iterations', kind: 'slider', min: 1, max: 5, step: 1, def: 3 },
    { key: 'ground', label: 'Ground plane', kind: 'toggle', def: 1 },
  ],
  start(params: Params): Sim {
    const count = integer(params.grid, 8, 14);
    const thickness = params.thickness;
    const grid = sheetGrid(count, 1.7, params.drop);
    let cloth = settled(grid.ClothFromGrid(new Mass(0.05), 0, new Length(thickness)));
    const sphere = new Sphere(new Point3D(0, 0, -0.35), params.radius);
    const groundHeight = -1.6;
    const ground = new Plane(new Direction3D(new Vector3D(0, 0, 1)), groundHeight);
    const useGround = params.ground !== 0;

    const view = clothView(cloth, { pins: false });
    const scenery: THREE.Object3D[] = [view.object, sphereObject(sphere)];
    if (useGround) scenery.push(planeObject(ground, 6));

    const cost = new Cost();
    let clock = 0;
    let frames = 0;

    const readings = (): Reading[] => {
      const vertices = cloth.Cloth.Vertices;
      let penetrating = 0;
      let lowest = Number.POSITIVE_INFINITY;
      let closest = Number.POSITIVE_INFINITY;
      for (let i = 0; i < vertices.Count(); i++) {
        const p = vertices.At(i).Position;
        const gap = p.Distance(sphere.Center) - sphere.Radius;
        if (gap < thickness - 1e-6) penetrating++;
        if (gap < closest) closest = gap;
        if (p.Z < lowest) lowest = p.Z;
      }
      const stretch = stretchOf(cloth);
      return [
        note('vertices / springs', `${view.vertexCount} / ${stretch.constraints}`),
        note('Thickness', `${n3(thickness)} m`),
        note('closest gap to the sphere', `${n3(closest)} m (shell is ${n3(thickness)})`),
        note('vertices inside the sphere', `${penetrating}`),
        useGround
          ? note(
              'resting height vs plane',
              `${n3(lowest)} against Distance + Thickness = ${n3(groundHeight + thickness)}`,
            )
          : note('ground plane', 'off'),
        note('max stretch', `${n3(stretch.max)} x rest`),
        note('step cost', `${n2(cost.ms)} ms/frame, ${frames} frames`),
      ];
    };

    return {
      object: zUp(...scenery),
      initial: readings,
      step(current: Params): Reading[] {
        const started = performance.now();
        let next = cloth.Step(
          forcesOf({ drag: 0.1 }),
          pbdSettings(integer(current.iterations, 1, 5), 1, 1, 0.03),
          clock,
        );
        next = next.CollideWith(sphere);
        if (useGround) next = collideWithPlane(next, ground);
        cloth = settled(next);
        cost.observe(performance.now() - started);
        clock += STEP_SECONDS;
        frames++;
        view.write(cloth);
        return readings();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// 4. Wind is a velocity field, not a force

const wind = simulation({
  id: 'wind',
  title: 'Wind needs drag',
  description:
    'A curtain pinned along its top rail with a WindModel blowing across it. WindModel names a speed, a ' +
    'direction, a turbulence fraction and a gust frequency — and no coupling coefficient, because air ' +
    'pushes on cloth through drag, which belongs to the particle rather than to the wind. So ' +
    'ParticleForces3D.AccelerationAt evaluates the drag term on the velocity relative to the wind field, ' +
    'and with the drag slider at zero the wind moves absolutely nothing: the downwind reading stays at ' +
    '0.000 m however hard it blows. That is a modelling decision made visible rather than a bug. Raise ' +
    'the drag and the same wind takes hold, gusting at GustFrequency as the sinusoid travels downwind.',
  plato: [
    'WindModel.VelocityAt',
    'ParticleForces3D.WindVelocityAt',
    'ParticleForces3D.AccelerationAt',
    'ParticleDrag.AccelerationAt',
    'ParticleGravity.AccelerationAt',
    'ClothMesh3D.Step',
    'ClothGrid3D.ClothFromGrid',
  ],
  viewer: { distance: 4.6, grid: true },
  controls: [
    { key: 'grid', label: 'Grid', kind: 'slider', min: 6, max: 12, step: 1, def: 10 },
    { key: 'drag', label: 'Drag (start at 0)', kind: 'slider', min: 0, max: 0.8, step: 0.02, def: 0 },
    { key: 'speed', label: 'Wind speed (m/s)', kind: 'slider', min: 0, max: 14, step: 0.5, def: 7 },
    { key: 'turbulence', label: 'Gust depth', kind: 'slider', min: 0, max: 1, step: 0.05, def: 0.5 },
    { key: 'gust', label: 'Gust frequency (Hz)', kind: 'slider', min: 0, max: 2, step: 0.05, def: 0.6 },
    { key: 'iterations', label: 'Iterations', kind: 'slider', min: 1, max: 5, step: 1, def: 3 },
  ],
  start(params: Params): Sim {
    const count = integer(params.grid, 6, 12);
    const grid = curtainGrid(count, count, 1.5, 1.5);
    let cloth = settled(
      grid.ClothFromGrid(new Mass(0.03), 0, new Length(0.01)).WithPinnedGridRow(grid, 0),
    );
    const view = clothView(cloth);
    const cost = new Cost();
    let clock = 0;
    let frames = 0;
    let downwind = 0;

    const model = (current: Params): WindModel =>
      new WindModel(
        new Direction3D(new Vector3D(0, 1, 0)),
        new Speed(current.speed),
        current.turbulence,
        new Frequency(current.gust),
      );

    const readings = (): Reading[] => {
      const stretch = stretchOf(cloth);
      return [
        note('Speed', `${n1(params.speed)} m/s along +Y`),
        note('ParticleDrag.LinearCoefficient', n2(params.drag)),
        reading('WindModel.VelocityAt', () =>
          `${n2(model(params).VelocityAt(new Point3D(0, 0, 0), clock).Y)} m/s at the origin`,
        ),
        note(
          'max downwind displacement',
          params.drag === 0
            ? `${n3(downwind)} m — no drag, so the wind field couples to nothing`
            : `${n3(downwind)} m`,
        ),
        note('max stretch', `${n3(stretch.max)} x rest`),
        note('step cost', `${n2(cost.ms)} ms/frame, ${frames} frames`),
      ];
    };

    return {
      object: zUp(view.object),
      initial: readings,
      step(current: Params): Reading[] {
        const started = performance.now();
        cloth = settled(
          cloth.Step(
            forcesOf({ drag: current.drag, wind: model(current) }),
            pbdSettings(integer(current.iterations, 1, 5), 1, 1, 0.02),
            clock,
          ),
        );
        cost.observe(performance.now() - started);
        clock += STEP_SECONDS;
        frames++;
        const vertices = cloth.Cloth.Vertices;
        downwind = 0;
        for (let i = 0; i < vertices.Count(); i++) {
          const y = Math.abs(vertices.At(i).Position.Y);
          if (y > downwind) downwind = y;
        }
        view.write(cloth);
        return readings();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// 5. What an iteration buys, and what it costs

const ITERATION_COUNTS = [1, 3, 8];

const iterations = simulation({
  id: 'iterations',
  title: 'One, three and eight sweeps',
  description:
    'Three identical sheets under identical forces, differing only in IterationCount: one Gauss-Seidel ' +
    'sweep per substep on the left, three in the middle, eight on the right. More sweeps means less ' +
    'residual stretch — the reading falls monotonically left to right — and proportionally more time, ' +
    'because a sweep rebuilds the whole vertex array once per constraint and is therefore ' +
    'O(vertices x constraints). The felt stiffness does not change with the count: SubStep corrects it ' +
    'first through IterationStiffness(k, n) = 1 - (1-k)^(1/n), so the residual after n sweeps is the ' +
    'same 1 - k whatever n is, and an iteration slider changes convergence alone.',
  plato: [
    'Number.IterationStiffness',
    'Array.SolveDistance',
    'Array.SweepDistance',
    'Array.DistanceCorrection',
    'ClothSolverSettings.IterationCount',
    'ClothMesh3D.Step',
  ],
  viewer: { distance: 5.4, grid: true },
  controls: [
    { key: 'grid', label: 'Grid', kind: 'slider', min: 5, max: 9, step: 1, def: 8 },
    { key: 'stiffness', label: 'Stiffness', kind: 'slider', min: 0.2, max: 1, step: 0.05, def: 0.8 },
    { key: 'damping', label: 'Velocity damping', kind: 'slider', min: 0, max: 0.1, step: 0.005, def: 0.02 },
    { key: 'wire', label: 'Face loops', kind: 'toggle', def: 1 },
  ],
  start(params: Params): Sim {
    const count = integer(params.grid, 5, 9);
    const grid = sheetGrid(count, 1.15, 0.5);
    const cloths = ITERATION_COUNTS.map(() =>
      settled(grid.ClothFromGrid(new Mass(0.05), 0, new Length(0.01)).WithPinnedGridRow(grid, 0)),
    );
    const views = cloths.map((cloth, k) => {
      const view = clothView(cloth, {
        wire: params.wire !== 0,
        color: [palette.surfaceAlt, palette.surface, palette.accent][k],
      });
      view.object.position.x = (k - 1) * 1.35;
      return view;
    });
    const costs = ITERATION_COUNTS.map(() => new Cost());
    let clock = 0;
    let frames = 0;

    const readings = (): Reading[] => [
      note('grid', `${count} x ${count}, ${views[0].vertexCount} vertices, ` +
        `${stretchOf(cloths[0]).constraints} springs each`),
      ...ITERATION_COUNTS.map((n, k) =>
        note(
          `${n} sweep${n === 1 ? '' : 's'}`,
          `stretch ${n3(stretchOf(cloths[k]).max)} x rest, ${n2(costs[k].ms)} ms/frame`,
        ),
      ),
      reading('IterationStiffness(k, n)', () =>
        ITERATION_COUNTS.map(n => n3(params.stiffness.IterationStiffness(n))).join(' / '),
      ),
      note('clock', `${n1(clock)} s, ${frames} frames`),
    ];

    return {
      object: zUp(...views.map(v => v.object)),
      initial: readings,
      step(current: Params): Reading[] {
        const force = forcesOf({ drag: 0.1 });
        for (let k = 0; k < ITERATION_COUNTS.length; k++) {
          const started = performance.now();
          cloths[k] = settled(
            cloths[k].Step(
              force,
              pbdSettings(ITERATION_COUNTS[k], 1, current.stiffness, current.damping),
              clock,
            ),
          );
          costs[k].observe(performance.now() - started);
          views[k].write(cloths[k]);
        }
        clock += STEP_SECONDS;
        frames++;
        return readings();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// 6. Releasing a pin mid-simulation

const PIN_MODES = ['Two corners', 'Whole rail', 'Four corners'];

const pinning = simulation({
  id: 'pinning',
  title: 'Releasing a pin',
  description:
    'Pinning is a mass of infinity and nothing more: WithPinnedVertex sets a flag, IsFixed reads it, ' +
    'SolverInverseMass reports zero, and every projection in the file already divides by inverse mass. ' +
    'That makes releasing a pin mid-flight a one-line edit to the state rather than a special case in ' +
    'the solver — this scene calls WithPinnedVertex on one corner once the clock passes the release ' +
    'time, and the cloth swings from what is left. Integrate collapses a fixed vertex onto itself each ' +
    'step, so the released corner starts from rest instead of inheriting a velocity it never had.',
  plato: [
    'ClothMesh3D.WithPinnedVertex',
    'Cloth3D.WithPinnedVertex',
    'Array.WithVertex',
    'ClothVertex.WithPinned',
    'ClothVertex.IsFixed',
    'ClothVertex.SolverInverseMass',
    'ClothGrid3D.GridVertexIndex',
  ],
  viewer: { distance: 4.6, grid: true },
  controls: [
    { key: 'grid', label: 'Grid', kind: 'slider', min: 6, max: 13, step: 1, def: 10 },
    { key: 'mode', label: 'Pins', kind: 'select', options: PIN_MODES, def: 0 },
    { key: 'release', label: 'Release at (s)', kind: 'slider', min: 0.5, max: 6, step: 0.25, def: 2 },
    { key: 'iterations', label: 'Iterations', kind: 'slider', min: 1, max: 5, step: 1, def: 3 },
    { key: 'damping', label: 'Velocity damping', kind: 'slider', min: 0, max: 0.1, step: 0.005, def: 0.02 },
  ],
  start(params: Params): Sim {
    const count = integer(params.grid, 6, 13);
    const mode = integer(params.mode, 0, PIN_MODES.length - 1);
    const grid = curtainGrid(count, count, 1.5, 1.5);
    let cloth = grid.ClothFromGrid(new Mass(0.04), 0, new Length(0.01));

    // Which vertices start pinned, and which one the clock releases.
    const corner = (column: number, row: number): VertexIndex => grid.GridVertexIndex(column, row);
    let pins: VertexIndex[];
    if (mode === 1) {
      pins = Array.from({ length: count }, (_, c) => corner(c, 0));
    } else if (mode === 2) {
      pins = [corner(0, 0), corner(count - 1, 0), corner(0, count - 1), corner(count - 1, count - 1)];
    } else {
      pins = [corner(0, 0), corner(count - 1, 0)];
    }
    for (const p of pins) cloth = cloth.WithPinnedVertex(p, true);
    cloth = settled(cloth);
    const released = pins[pins.length - 1];

    // A pinned cloth's pins never move, so the view's pin markers are built from
    // the initial state and rewritten each frame like everything else.
    const view = clothView(cloth);
    const cost = new Cost();
    let clock = 0;
    let frames = 0;
    let done = false;

    const readings = (): Reading[] => {
      const vertices = cloth.Cloth.Vertices;
      let pinnedNow = 0;
      for (let i = 0; i < vertices.Count(); i++) if (vertices.At(i).IsFixed()) pinnedNow++;
      const stretch = stretchOf(cloth);
      return [
        note('mode', `${PIN_MODES[mode]} — ${pins.length} pinned at t = 0`),
        note('pinned now', `${pinnedNow} (IsFixed)`),
        note(
          'release',
          done
            ? `vertex ${released.Value} released at ${n1(params.release)} s`
            : `vertex ${released.Value} in ${n1(Math.max(0, params.release - clock))} s`,
        ),
        note('max stretch', `${n3(stretch.max)} x rest`),
        note('clock', `${n1(clock)} s, ${frames} frames`),
        note('step cost', `${n2(cost.ms)} ms/frame`),
      ];
    };

    return {
      object: zUp(view.object),
      initial: readings,
      step(current: Params): Reading[] {
        const started = performance.now();
        cloth = settled(
          cloth.Step(
            forcesOf({ drag: 0.08 }),
            pbdSettings(integer(current.iterations, 1, 5), 1, 1, current.damping),
            clock,
          ),
        );
        if (!done && clock >= current.release) {
          cloth = cloth.WithPinnedVertex(released, false);
          done = true;
        }
        cost.observe(performance.now() - started);
        clock += STEP_SECONDS;
        frames++;
        view.write(cloth);
        return readings();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// 7. Topology in, topology out — and the mesh path that does not run

/** A quad grid as a `PolygonMesh3D`, the input `ClothFromMesh` is meant to take. */
function quadMesh(count: number, width: number): PolygonMesh3D {
  const positions: Point3D[] = [];
  for (let r = 0; r < count; r++) {
    for (let c = 0; c < count; c++) {
      positions.push(
        new Point3D(
          -width / 2 + (width * c) / (count - 1),
          -width / 2 + (width * r) / (count - 1),
          0,
        ),
      );
    }
  }
  const loops: IArray<VertexIndex>[] = [];
  for (let r = 0; r + 1 < count; r++) {
    for (let c = 0; c + 1 < count; c++) {
      loops.push(
        fromArray([
          new VertexIndex(r * count + c),
          new VertexIndex(r * count + c + 1),
          new VertexIndex((r + 1) * count + c + 1),
          new VertexIndex((r + 1) * count + c),
        ]),
      );
    }
  }
  // `FromRows` is one of the prelude's array-receiver bodies, so it is installed
  // on the runtime array but declared on no emitted interface — the same cast
  // the other pages use for a prelude-only member.
  const table = (fromArray(loops) as unknown as { FromRows(): JaggedArray<VertexIndex> }).FromRows();
  return new PolygonMesh3D(fromArray(positions), table);
}

const topology = simulation({
  id: 'topology',
  title: 'Topology in, topology out',
  description:
    'ClothMesh3D pairs a Cloth3D with the polygon face table it was built from, because Cloth3D stores ' +
    'triangles and a polygon source may have quads. The solver never touches the table, so ToPolygonMesh ' +
    'is a position substitution: the wireframe here is the quad face loops of ToPolygonMesh, drawn from ' +
    'the simulated positions, while the shading is ToTriangleMesh over the same numbering. The other ' +
    'direction does not work. ClothFromMesh — one spring per undirected edge, one dihedral bend ' +
    'constraint per interior edge — cannot run, because VerticesOfUndirectedEdge is emitted as a Tuple2 ' +
    'with fields X0 and X1 where a VertexPair with A and B was meant; the reading below calls it and ' +
    'reports what came back.',
  plato: [
    'ClothMesh3D.ToPolygonMesh',
    'ClothMesh3D.ToTriangleMesh',
    'Cloth3D.Positions',
    'PolygonMesh3D.VerticesOfFace',
    'PolygonMesh3D.ClothFromMesh',
    'PolygonMesh3D.MeshClothVertices',
    'PolygonMesh3D.MeshSprings',
    'PolygonMesh3D.MeshBendConstraints',
    'PolygonMesh3D.IsInteriorEdge',
    'PolygonMesh3D.VerticesOfUndirectedEdge',
  ],
  viewer: { distance: 4.4, grid: true },
  controls: [
    { key: 'grid', label: 'Grid', kind: 'slider', min: 5, max: 11, step: 1, def: 8 },
    { key: 'iterations', label: 'Iterations', kind: 'slider', min: 1, max: 5, step: 1, def: 3 },
    // `PolygonMesh3D` answers incidence queries by scanning its packed corner
    // table, so the mesh path is cubic in corner count — the library says so at
    // `ClothFromMesh`, and the reading below measures it. A 5 x 5 quad grid
    // already costs seconds, which is why this cap is where it is.
    { key: 'mesh', label: 'Mesh path size', kind: 'slider', min: 3, max: 4, step: 1, def: 3 },
  ],
  start(params: Params): Sim {
    const count = integer(params.grid, 5, 11);
    const grid = sheetGrid(count, 1.6, 0.5);
    let cloth = settled(
      grid.ClothFromGrid(new Mass(0.05), 0, new Length(0.01)).WithPinnedGridRow(grid, 0),
    );
    const view = clothView(cloth);
    const cost = new Cost();
    let clock = 0;
    let frames = 0;

    // The mesh path, evaluated once at build: the part that works, then the
    // member that stops it.
    const meshCount = integer(params.mesh, 3, 4);
    const source = quadMesh(meshCount, 1.4);
    const meshStarted = performance.now();
    const meshReadings: Reading[] = [
      note('source mesh', `${meshCount} x ${meshCount} positions, ${source.FaceCount()} quads`),
      reading('MeshClothVertices', () =>
        `${source.MeshClothVertices(new Mass(0.05)).Count()} cloth vertices`,
      ),
      reading('IsInteriorEdge', () => {
        let interior = 0;
        const total = source.UndirectedEdgeCount();
        for (let e = 0; e < total; e++) {
          if (source.IsInteriorEdge(new UndirectedEdgeIndex(e))) interior++;
        }
        return `${interior} of ${total} undirected edges carry two faces`;
      }),
      reading('ClothFromMesh', () => {
        const built = source.ClothFromMesh(new Mass(0.05), 0, new Length(0.01));
        // Both constraint arrays are lazy, so the failure is at the first read.
        return `${built.Cloth.Vertices.Count()} vertices, ` +
          `${built.Cloth.DistanceConstraints.At(0).RestLength.Meters} m first rest length`;
      }),
    ];
    meshReadings.push(
      note('mesh path cost', `${n1(performance.now() - meshStarted)} ms — cubic in corner count`),
    );

    const readings = (): Reading[] => {
      const polygons = cloth.ToPolygonMesh();
      const stretch = stretchOf(cloth);
      return [
        note('ToPolygonMesh', `${polygons.VertexCount()} positions, ${view.quadCount} quad faces`),
        note('ToTriangleMesh', `${view.faceCount} triangles over the same numbering`),
        note('max stretch', `${n3(stretch.max)} x rest`),
        note('step cost', `${n2(cost.ms)} ms/frame, ${frames} frames`),
        ...meshReadings,
      ];
    };

    return {
      object: zUp(view.object),
      initial: readings,
      step(current: Params): Reading[] {
        const started = performance.now();
        cloth = settled(
          cloth.Step(
            forcesOf({ drag: 0.1 }),
            pbdSettings(integer(current.iterations, 1, 5), 1, 1, 0.03),
            clock,
          ),
        );
        cost.observe(performance.now() - started);
        clock += STEP_SECONDS;
        frames++;
        view.write(cloth);
        return readings();
      },
    };
  },
});

// ---------------------------------------------------------------------------

const demo: Demo = {
  title: 'Cloth',
  subtitle: 'cloth.{types,library}.plato · particles.types.plato · particles.library.plato',
  scenes: [drape, solvers, draping, wind, iterations, pinning, topology],
};

mountDemo(demo, { spin: false, grid: true, distance: 4.6 });

// The page never imports this; it exists so `npm run scenes` can call every
// scene's `build` and step every `tick` without a WebGL context.
export { demo };
