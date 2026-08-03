// Rigid bodies — a scene catalog over `stdlib/future/rigid-dynamics.{types,library}.plato`
// and `collision.{types,library}.plato`.
//
// This is a SIMULATION page: six of its seven scenes declare `tick`, and every
// one of them is the same three lines per frame —
//
//     world = world.StepBallScene(radii, ground, groundBody, mu, e, tolerance);
//     world = materialized(world);            // see "the lazy fold" below
//     writeMatrices(object, world.Bodies);    // repack, the demo's only job
//
// `StepBallScene` is the whole per-frame loop of a ball-and-plane scene: it
// re-runs the narrow phase against the current poses, carries the previous
// frame's accumulated impulses onto the matching rows (`WarmStartFrom`), and
// steps. Nothing here integrates, detects, solves or corrects anything; the
// demo builds the initial bodies, hands the settings the sliders name, and reads
// `Bodies[i].Center` and `.Orientation` back out.
//
// WHAT THE SCENES ARE FOR. Every knob on this page is a field of
// `TimeStepSettings` or `ContactSolverSettings` — the two records whose values
// the library's own design notes argue for. Each scene isolates one of them and
// puts the argument on screen:
//
//   * `stack`        warm starting: the same stack with `Constraints` carried
//                    across the frame boundary and with them cleared.
//   * `correction`   the split impulse: `BaumgarteFactor`, `PenetrationSlop`
//                    and `MaxCorrectionSpeed` against a deliberately
//                    interpenetrating spawn.
//   * `restitution`  five worlds, restitution 0 to 0.9, against the analytic
//                    apex e^2 h, plus the `RestitutionThreshold` cut-off.
//   * `iterations`   Gauss-Seidel convergence: the same heavy-on-light stack at
//                    1, 4 and 16 `VelocityIterations`.
//   * `friction`     four lanes down one tilted plane at four Coulomb
//                    coefficients, reading the slip speed at the contact.
//   * `impulse`      `SolverBody3D.ApplyImpulse` as a periodic blast.
//   * `narrowphase`  the contact pipeline, still: manifolds, rows, the friction
//                    basis, and a live census of which pair tests survived the
//                    TypeScript writer.
//
// THE LAZY FOLD, which is the one thing that will bite anyone extending this
// file. `Step` ends in `IntegratePoses`, which is `Indices.Map(i => Bodies[i]
// .IntegratePoseWith(corrections[i].Velocity, dt))` — and `Map` on the emitted
// `Arr` is LAZY. `corrections` is itself a `Map` over `Bodies`, so reading one
// body of frame n reads TWO bodies of frame n-1: the cost of one `Center` is
// 2^n. Measured on a free-falling ball with no contact rows, one `Center` read
// costs 0.9 ms at frame 0, 64 ms at frame 15, and 75 SECONDS at frame 25. Every
// scene here therefore calls `materialized` on `Bodies` and `Constraints` after
// each step, which is what the prelude's "write folds eagerly" note asks for and
// makes a frame linear in the body count again.
//
// COST. `BallSceneManifolds` is O(n^2) with no broad phase (deferred to
// plato-428) and a solver pass is rows times bodies, so the body counts here are
// deliberately modest: measured 0.46 ms/frame at 10 balls, 1.10 at 20, 3.36 at
// 40, all at eight velocity iterations. The caps on the sliders come from those
// numbers.
//
// WORLD AXES. Plato's world is Z-up (`UniformGravity3D.Earth` is (0, 0, -9.807))
// and Three.js's stage is Y-up, so the repack swaps the two: a point (x, y, z)
// is drawn at (x, z, y). That swap is a reflection, so a body's orientation
// quaternion (x, y, z, w) is drawn as (x, z, y, -w) — the same rotation read in
// a left-handed frame. Both conversions live in `toThree` / `spinToThree` and
// nowhere else.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { polygonMeshGeometry } from '../shared/mesh.js';
import { palette, surfaceMaterial, type ViewerOptions } from '../shared/viewer.js';
import {
  AffineTransform3D,
  AppliedImpulse3D,
  BodyIndex,
  Box3D,
  Capsule3D,
  ContactConstraint3D,
  ContactManifold3D,
  ContactSolverSettings,
  Cylinder,
  Direction3D,
  Duration,
  Impulse,
  Intrinsics,
  Length,
  Mass,
  Plane,
  Point3D,
  PolygonMesh3D,
  Pose3D,
  Proportion,
  Quaternion,
  RadialImpulse3D,
  RigidWorld3D,
  Size3D,
  SolverBody3D,
  SpatialVelocity3D,
  Speed,
  Sphere,
  TimeStepSettings,
  UniformGravity3D,
  Vector3D,
  type IArray,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// The house pattern from `polygons.ts`: a member that throws or returns NaN is a
// gap in the emitted library, and the status line keeps its name and says so
// rather than substituting a hand-rolled answer. `future` is the tier that is
// neither linted nor converted to C#, so this page uses it more than most.

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

const line = (readings: readonly Reading[]): string =>
  readings.map(r => `${r.label} ${r.value}`).join('  ·  ');

const n1 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(1);
const n2 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(2);
const n3 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(3);
const n4 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(4);
const sci = (x: number): string => (x === 0 ? '0' : x.toExponential(1));
const mm = (metres: number): string => `${(metres * 1000).toFixed(1)} mm`;

const clampIndex = (raw: number, count: number): number =>
  Math.min(count - 1, Math.max(0, Math.round(raw)));

// ---------------------------------------------------------------------------
// Arrays
//
// `Intrinsics.MakeArray` is variadic; the range mapper is used instead so a body
// count is never a spread argument list. `materialized` is the eager form and is
// the reason this page runs at all — see the header.

const arrayOf = <T,>(xs: readonly T[]): IArray<T> => Intrinsics.Range(xs.length).Map(i => xs[i]);

function toList<T>(xs: IArray<T>): T[] {
  const out: T[] = [];
  for (let i = 0; i < xs.Count(); i++) out.push(xs.At(i));
  return out;
}

/** The same array with every element evaluated once, collapsing the lazy chain. */
const materialized = <T,>(xs: IArray<T>): IArray<T> => arrayOf(toList(xs));

const EMPTY_ROWS: IArray<ContactConstraint3D> = arrayOf<ContactConstraint3D>([]);

/**
 * The array-receiver members of `rigid-dynamics.library.plato` and
 * `collision.library.plato`. The TypeScript writer emits them in
 * extension-method position but never emits the functions, so the prelude
 * supplies the bodies and `IArray<T>` in `plato.g.ts` still declares only
 * At / Count / Map / Reduce. These two interfaces are how this file names them
 * without reaching for `any`.
 */
interface BodyArray extends IArray<SolverBody3D> {
  /** `ReplacedAt(xs, index, value)` — the primitive the whole solver fold runs on. */
  ReplacedAt(index: number, value: SolverBody3D): IArray<SolverBody3D>;
  /** Every ball-versus-ball manifold of a scene, plus every ball against one plane. */
  BallSceneManifolds(
    radii: IArray<number>,
    ground: Plane,
    groundBody: BodyIndex,
    friction: number,
    restitution: Proportion,
  ): IArray<ContactManifold3D>;
}

const bodyArray = (xs: IArray<SolverBody3D>): BodyArray => xs as BodyArray;

/**
 * The two `SolverBody3D.ApplyImpulse` overloads the writer dropped. Naming them
 * here is what lets the `impulse` scene CALL them and report what came back,
 * rather than assuming either way.
 */
interface DroppedImpulseOverloads {
  ApplyImpulse(applied: AppliedImpulse3D): SolverBody3D;
  ApplyImpulse(blast: RadialImpulse3D): SolverBody3D;
}

const asDropped = (body: SolverBody3D): DroppedImpulseOverloads =>
  body as unknown as DroppedImpulseOverloads;

/**
 * `Vector3D.Transform(Quaternion)` is a dropped overload — the emitted
 * signature takes an `AffineTransform3D` and the prelude re-dispatches on the
 * runtime argument — so the cast is the house spelling from `transforms.ts`.
 */
const inFrame = (v: Vector3D, q: Quaternion): Vector3D =>
  v.Transform(q as unknown as AffineTransform3D);

/**
 * `Sphere.Collide` kept only the ball-versus-ball body; the plane, box and
 * capsule overloads were dropped. The prelude re-dispatches the plane form, and
 * the `narrowphase` scene calls the other two through this cast precisely so it
 * can report what they do.
 */
const collideWith = (sphere: Sphere, other: unknown): IArray<unknown> =>
  sphere.Collide(other as Sphere) as unknown as IArray<unknown>;

// ---------------------------------------------------------------------------
// The rig: one world, its parallel radii, and the coefficients a step takes
//
// `SolverBody3D` carries no shape — that is what lets one solver serve every
// shape — so a ball scene keeps its radii in an array parallel to `Bodies`, in
// body order, exactly as `BallOf` expects. Body 0 is always the ground: an
// immovable `StaticBody`, excluded from the ball-versus-ball tests, and its
// radius entry is never read.

const GROUND_BODY = new BodyIndex(0);
const FLAT_GROUND = new Plane(new Direction3D(new Vector3D(0, 0, 1)), 0);

/** A plane through the origin tilted by `radians` about the world Y axis. */
const tiltedGround = (radians: number): Plane =>
  new Plane(
    new Direction3D(new Vector3D(Math.sin(radians), 0, Math.cos(radians)).Normalize()),
    0,
  );

interface BallSpec {
  center: Point3D;
  radius: number;
  mass: number;
  /**
   * Exponential velocity decay rates, one per second. `DampingFactor` turns one
   * into exp(-rate dt), which is the exact solution of dv/dt = -rate v and is
   * therefore stable at any step size — unlike the (1 - rate dt) a first-order
   * reading gives, which goes negative once the step is long enough. Zero is
   * the honest default, and it is what makes a ball rolling on an infinite
   * level plane roll forever: nothing in this library models rolling
   * resistance.
   */
  linearDamping?: number;
  angularDamping?: number;
}

interface Rig {
  world: RigidWorld3D;
  radii: IArray<number>;
  specs: BallSpec[];
  ground: Plane;
  friction: number;
  restitution: Proportion;
  tolerance: Length;
  /** False clears `Constraints` before each step, which is warm starting off. */
  warm: boolean;
}

interface RigOptions {
  balls: BallSpec[];
  ground?: Plane;
  step?: TimeStepSettings;
  solver?: ContactSolverSettings;
  friction?: number;
  restitution?: number;
  tolerance?: number;
  warm?: boolean;
}

/**
 * A dynamic solver body for one ball. `Sphere.PrincipalMoments(mass)` is the
 * library's 2/5 m r^2 — a ball's tensor is a multiple of the identity, so it
 * stays diagonal under any rotation and `SolverBody3D`'s inverse-inertia
 * diagonal is exact rather than a principal-frame approximation.
 */
function ballBody(spec: BallSpec): SolverBody3D {
  const mass = Mass.FromAmount(spec.mass);
  const moments = new Sphere(spec.center, spec.radius).PrincipalMoments(mass);
  return new Pose3D(spec.center, Quaternion.Identity())
    .DynamicBody(mass, moments)
    .WithLinearDamping(spec.linearDamping ?? 0)
    .WithAngularDamping(spec.angularDamping ?? 0);
}

function makeRig(options: RigOptions): Rig {
  const specs = options.balls;
  const bodies: SolverBody3D[] = [new Pose3D(new Point3D(0, 0, 0), Quaternion.Identity()).StaticBody()];
  const radii: number[] = [0];
  for (const spec of specs) {
    bodies.push(ballBody(spec));
    radii.push(spec.radius);
  }
  return {
    world: new RigidWorld3D(
      arrayOf(bodies),
      EMPTY_ROWS,
      UniformGravity3D.Earth().Acceleration,
      options.step ?? TimeStepSettings.SixtyHertz(),
      options.solver ?? ContactSolverSettings.Settled(),
    ),
    radii: arrayOf(radii),
    specs,
    ground: options.ground ?? FLAT_GROUND,
    friction: options.friction ?? 0.5,
    restitution: new Proportion(options.restitution ?? 0.2),
    tolerance: Length.FromAmount(options.tolerance ?? 0.02),
    warm: options.warm ?? true,
  };
}

/** One fixed step of one rig, materialized. */
function stepRig(rig: Rig): void {
  const base = rig.warm ? rig.world : rig.world.WithConstraints(EMPTY_ROWS);
  const next = base.StepBallScene(
    rig.radii,
    rig.ground,
    GROUND_BODY,
    rig.friction,
    rig.restitution,
    rig.tolerance,
  );
  rig.world = new RigidWorld3D(
    materialized(next.Bodies),
    materialized(next.Constraints),
    next.Gravity,
    next.TimeStep,
    next.Solver,
  );
}

/** The settings blocks the sliders assemble, spelled once. */
const timeStep = (velocityIterations: number, positionIterations: number): TimeStepSettings =>
  new TimeStepSettings(new Duration(1 / 60), 1, Math.round(velocityIterations), Math.round(positionIterations), 4);

const solverSettings = (opts: {
  slop?: number;
  baumgarte?: number;
  maxCorrection?: number;
  threshold?: number;
}): ContactSolverSettings =>
  new ContactSolverSettings(
    Length.FromAmount(opts.slop ?? 0.005),
    opts.baumgarte ?? 0.2,
    Speed.FromAmount(opts.maxCorrection ?? 3),
    Speed.FromAmount(opts.threshold ?? 1),
  );

// ---------------------------------------------------------------------------
// Readings taken off a world
//
// Total kinetic energy is the honest stability reading: under gravity and
// contact it must decay to nothing, and a solver that injects energy shows it
// here before anything is visible on screen. The rotational half goes through
// the body's own frame — the inverse-inertia diagonal is expressed there, which
// is exactly what `AngularResponse` does with it.

function kineticEnergy(world: RigidWorld3D): number {
  let total = 0;
  for (const body of toList(world.Bodies)) {
    if (body.InverseMass <= 0) continue;
    total += (0.5 * body.Velocity.Linear.MagnitudeSquared()) / body.InverseMass;
    const spin = inFrame(body.Velocity.Angular, body.Orientation.Inverse());
    const inverse = body.InverseInertia;
    if (inverse.X > 0) total += (0.5 * spin.X * spin.X) / inverse.X;
    if (inverse.Y > 0) total += (0.5 * spin.Y * spin.Y) / inverse.Y;
    if (inverse.Z > 0) total += (0.5 * spin.Z * spin.Z) / inverse.Z;
  }
  return total;
}

function penetrationStats(world: RigidWorld3D): { max: number; mean: number; rows: number } {
  const rows = toList(world.Constraints);
  let max = 0;
  let sum = 0;
  for (const row of rows) {
    const depth = row.Penetration.Amount();
    max = Math.max(max, depth);
    sum += depth;
  }
  return { max, mean: rows.length > 0 ? sum / rows.length : 0, rows: rows.length };
}

/** The impulse the solver is currently holding along every contact normal. */
function normalImpulseTotal(world: RigidWorld3D): number {
  let total = 0;
  for (const row of toList(world.Constraints)) total += row.NormalImpulse.Amount();
  return total;
}

// ---------------------------------------------------------------------------
// Presentation
//
// Z-up to Y-up, and the two objects every simulation scene draws: one
// `InstancedMesh` for the balls, so a tick writes matrices and never geometry,
// and one `THREE.Points` holding the body centres.
//
// That Points cloud is not decoration. `npm run scenes` catches a diverged
// solver by scanning `geometry.attributes.position` for a non-finite value, and
// an InstancedMesh keeps its state in `instanceMatrix`, which that scan does not
// reach — so a page drawn only with instances would go green while every body
// sat at NaN. The centres buffer is the simulation state in a place the gate can
// see. It is drawn at one pixel and sits inside its own ball, so it costs
// nothing visually.

const toThree = (p: Point3D): THREE.Vector3 => new THREE.Vector3(p.X, p.Z, p.Y);

/** The same rotation read in the swapped (left-handed) frame. */
const spinToThree = (q: Quaternion): THREE.Quaternion => new THREE.Quaternion(q.X, q.Z, q.Y, -q.W);

const TINTS = [palette.surface, palette.surfaceAlt, palette.accent, palette.line, 0xc98bd4, 0xd9d36a];

/**
 * The unit ball every instance is a scaled copy of, from the library's own
 * solid: an icosahedron pushed onto the unit sphere. Flat-shaded facets are
 * what makes a rolling ball read as rolling rather than sliding, which the
 * friction scene depends on. Built per `build` call because the viewer disposes
 * the geometry of whatever it replaces.
 */
const unitBallGeometry = (): THREE.BufferGeometry =>
  polygonMeshGeometry(PolygonMesh3D.Icosahedron().ProjectedToUnitSphere());

interface Draw {
  center: Point3D;
  orientation: Quaternion;
  radius: number;
}

class BallCloud {
  readonly group = new THREE.Group();
  private readonly mesh: THREE.InstancedMesh;
  private readonly centres: THREE.Points;
  private readonly buffer: Float32Array;
  private readonly matrix = new THREE.Matrix4();
  private readonly position = new THREE.Vector3();
  private readonly scale = new THREE.Vector3();

  constructor(count: number, tints: readonly number[]) {
    const capacity = Math.max(1, count);
    this.mesh = new THREE.InstancedMesh(unitBallGeometry(), surfaceMaterial(0xffffff), capacity);
    this.mesh.count = count;
    for (let i = 0; i < count; i++) {
      this.mesh.setColorAt(i, new THREE.Color(tints[i % tints.length]));
    }
    if (this.mesh.instanceColor) this.mesh.instanceColor.needsUpdate = true;

    this.buffer = new Float32Array(capacity * 3);
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.BufferAttribute(this.buffer, 3));
    geometry.setDrawRange(0, count);
    this.centres = new THREE.Points(
      geometry,
      new THREE.PointsMaterial({ color: 0x0d1117, size: 1, sizeAttenuation: false }),
    );
    this.centres.name = 'body-centres';
    this.group.add(this.mesh, this.centres);
  }

  write(draws: readonly Draw[]): void {
    for (let i = 0; i < draws.length; i++) {
      const draw = draws[i];
      this.position.set(draw.center.X, draw.center.Z, draw.center.Y);
      this.scale.setScalar(draw.radius);
      this.matrix.compose(this.position, spinToThree(draw.orientation), this.scale);
      this.mesh.setMatrixAt(i, this.matrix);
      this.buffer[i * 3] = draw.center.X;
      this.buffer[i * 3 + 1] = draw.center.Z;
      this.buffer[i * 3 + 2] = draw.center.Y;
    }
    this.mesh.instanceMatrix.needsUpdate = true;
    this.centres.geometry.attributes.position.needsUpdate = true;
  }
}

/**
 * The contact rows drawn as their normals, each scaled by the accumulated
 * normal impulse the solver is holding there — so a stack shows the load it is
 * carrying growing toward the bottom.
 */
class ContactOverlay {
  readonly object: THREE.LineSegments;
  private readonly buffer: Float32Array;
  private readonly capacity: number;

  constructor(capacity: number) {
    this.capacity = Math.max(1, capacity);
    this.buffer = new Float32Array(this.capacity * 6);
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.BufferAttribute(this.buffer, 3));
    geometry.setDrawRange(0, 0);
    this.object = new THREE.LineSegments(
      geometry,
      new THREE.LineBasicMaterial({ color: 0xffd479 }),
    );
  }

  write(rows: readonly ContactConstraint3D[], scale: number): void {
    const shown = Math.min(rows.length, this.capacity);
    for (let i = 0; i < shown; i++) {
      const row = rows[i];
      const length = 0.04 + scale * row.NormalImpulse.Amount();
      const tip = row.Point.Add(row.Normal.Vector.Multiply(length));
      this.buffer[i * 6] = row.Point.X;
      this.buffer[i * 6 + 1] = row.Point.Z;
      this.buffer[i * 6 + 2] = row.Point.Y;
      this.buffer[i * 6 + 3] = tip.X;
      this.buffer[i * 6 + 4] = tip.Z;
      this.buffer[i * 6 + 5] = tip.Y;
    }
    this.object.geometry.setDrawRange(0, shown * 2);
    this.object.geometry.attributes.position.needsUpdate = true;
  }
}

function segments(coordinates: readonly number[], color: number, opacity = 1): THREE.LineSegments {
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(coordinates as number[], 3));
  return new THREE.LineSegments(
    geometry,
    new THREE.LineBasicMaterial({ color, transparent: opacity < 1, opacity }),
  );
}

function dots(points: readonly Point3D[], color: number, size = 8): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.X, p.Z, p.Y);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(geometry, new THREE.PointsMaterial({ color, size, sizeAttenuation: false }));
}

/**
 * The ground, drawn from the plane itself: the in-plane axes are the contact
 * frame's own `Direction3D.Tangent` and `Bitangent` — the same two vectors every
 * constraint row stores its friction impulse in — and the origin is the world
 * origin projected onto the plane by `Plane.ClosestPoint`.
 */
function groundObject(plane: Plane, half: number, cells: number): THREE.Object3D {
  const tangent = plane.Normal.Tangent();
  const bitangent = plane.Normal.Bitangent(tangent);
  const origin = plane.ClosestPoint(new Point3D(0, 0, 0));
  const at = (u: number, v: number): Point3D =>
    origin.Add(tangent.Vector.Multiply(u)).Add(bitangent.Vector.Multiply(v));

  const group = new THREE.Group();
  const grid: number[] = [];
  for (let i = 0; i <= cells; i++) {
    const t = -half + (2 * half * i) / cells;
    const a = at(t, -half);
    const b = at(t, half);
    const c = at(-half, t);
    const d = at(half, t);
    grid.push(a.X, a.Z, a.Y, b.X, b.Z, b.Y, c.X, c.Z, c.Y, d.X, d.Z, d.Y);
  }
  group.add(segments(grid, 0x2b3444, 0.9));

  const corners = [at(-half, -half), at(half, -half), at(half, half), at(-half, half)];
  const face: number[] = [];
  for (const [i, j, k] of [[0, 1, 2], [0, 2, 3]] as const) {
    for (const c of [corners[i], corners[j], corners[k]]) face.push(c.X, c.Z, c.Y);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(face, 3));
  geometry.computeVertexNormals();
  const material = surfaceMaterial(0x1b2230);
  material.transparent = true;
  material.opacity = 0.85;
  group.add(new THREE.Mesh(geometry, material));
  return group;
}

/** A short horizontal tick at a height, for the apex markers. */
function heightMarker(x: number, y: number, z: number, span: number, color: number): THREE.LineSegments {
  return segments([x - span, z, y, x + span, z, y], color);
}

// ---------------------------------------------------------------------------
// The scene shell
//
// `build` is the RESET: every piece of mutable state a scene has is created
// inside the `build` call, so a parameter change restarts the simulation from
// its initial conditions and no two scenes can share a world. `tick` mutates the
// object `build` returned and never allocates a new one.

interface Built {
  object: THREE.Object3D;
  /** The status line right after `build`, before any stepping. */
  initial: string;
  /** One animation frame; the returned string replaces the status line. */
  step(seconds: number): string;
}

interface SimSpec {
  id: string;
  title: string;
  description: string;
  plato: string[];
  controls?: Control[];
  viewer?: ViewerOptions;
  build(params: Params): Built;
}

function simScene(spec: SimSpec): Scene {
  let latest: Built | null = null;
  return {
    id: spec.id,
    title: spec.title,
    description: spec.description,
    plato: spec.plato,
    controls: spec.controls,
    viewer: spec.viewer,
    build(params: Params): THREE.Object3D {
      latest = spec.build(params);
      return latest.object;
    },
    status(): string {
      return latest?.initial ?? '';
    },
    tick(seconds: number): string {
      return latest ? latest.step(seconds) : '';
    },
  };
}

/** A still scene, for the one page entry that has nothing to advance. */
function stillScene(spec: {
  id: string;
  title: string;
  description: string;
  plato: string[];
  controls?: Control[];
  viewer?: ViewerOptions;
  build(params: Params): { object: THREE.Object3D; readings: Reading[] };
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
      return line(latest);
    },
  };
}

/**
 * The fixed-step clock. `TimeStepSettings` names both halves of it — the step
 * length and `MaxStepsPerFrame`, which bounds catch-up so a tab that was in the
 * background does not resume by simulating the time it was away. The shell
 * already clamps the elapsed seconds; this is the library's own second bound.
 */
class Clock {
  private accumulated = 0;
  private cost = 0;
  frames = 0;

  constructor(private readonly settings: TimeStepSettings) {}

  /** Runs `advance` for each fixed step this frame owes, and times them. */
  run(seconds: number, advance: () => void): number {
    const dt = this.settings.FixedDeltaTime.Seconds;
    const cap = Math.max(1, this.settings.MaxStepsPerFrame);
    this.accumulated += seconds;
    let steps = 0;
    const started = performance.now();
    while (this.accumulated >= dt && steps < cap) {
      advance();
      this.accumulated -= dt;
      steps++;
    }
    if (steps >= cap) this.accumulated = 0;
    if (steps > 0) {
      const ms = performance.now() - started;
      this.cost = this.frames === 0 ? ms : this.cost * 0.9 + ms * 0.1;
      this.frames++;
    }
    return steps;
  }

  get msPerFrame(): number {
    return this.cost;
  }

  get seconds(): number {
    return this.frames * this.settings.FixedDeltaTime.Seconds;
  }
}

const STAGE: ViewerOptions = { distance: 7, grid: false, spin: false };
const WIDE_STAGE: ViewerOptions = { distance: 14, grid: false, spin: false };

// ---------------------------------------------------------------------------
// Scene 1 — a stack settles

const stack = simScene({
  id: 'stack',
  title: 'A stack settles',
  description:
    'Balls dropped into a column on a ground plane, stepped by RigidWorld3D.StepBallScene — the whole per-frame ' +
    'loop of a ball-and-plane scene: BallSceneManifolds detects, Flatten turns manifolds into rows, WarmStartFrom ' +
    'copies the previous frame\'s accumulated impulses onto the rows describing the same contact, and Step solves ' +
    'and integrates. Turn WARM STARTING off and the demo clears RigidWorld3D.Constraints before each frame, so ' +
    'every row starts from zero impulse: the stack then sinks and never stops moving, because the solver spends its ' +
    'whole iteration budget rediscovering the support impulses it already found. Watch the kinetic energy — warm it ' +
    'falls to nothing and the penetration sits exactly on PenetrationSlop, cold it holds a nonzero value forever and ' +
    'the overlap is several times the slop. The contact overlay draws each row\'s normal scaled by the impulse it is ' +
    'carrying, which is why the arrows grow toward the bottom of the stack. Two honest caveats the sliders let you ' +
    'find: a column of SPHERES is only marginally stable, so any spawn jitter topples it — a manifold of one point ' +
    'per pair cannot resist toppling, which is the whole reason a box-versus-plane manifold is a set of points; and ' +
    'a ball that has rolled clear onto the plane rolls forever, because nothing in this library models rolling ' +
    'resistance, so the energy reading stops falling once anything escapes.',
  plato: [
    'RigidWorld3D.StepBallScene',
    'RigidWorld3D.BallSceneConstraints',
    'SolverBody3D.BallSceneManifolds',
    'ContactManifold3D.Flatten',
    'ContactConstraint3D.WarmStartFrom',
    'ContactConstraint3D.SameContact',
    'RigidWorld3D.Step',
    'RigidWorld3D.WithConstraints',
    'Pose3D.DynamicBody',
    'Pose3D.StaticBody',
    'Sphere.PrincipalMoments',
    'UniformGravity3D.Earth',
    'TimeStepSettings.SixtyHertz',
    'ContactSolverSettings.Settled',
  ],
  viewer: STAGE,
  controls: [
    { key: 'count', label: 'Balls', kind: 'slider', min: 2, max: 12, step: 1, def: 6 },
    { key: 'drop', label: 'Drop gap (m)', kind: 'slider', min: 0, max: 0.4, step: 0.01, def: 0.08 },
    { key: 'restitution', label: 'Restitution', kind: 'slider', min: 0, max: 0.9, step: 0.05, def: 0.2 },
    { key: 'friction', label: 'Friction', kind: 'slider', min: 0, max: 1, step: 0.05, def: 0.5 },
    { key: 'jitter', label: 'Spawn jitter (m)', kind: 'slider', min: 0, max: 0.12, step: 0.005, def: 0 },
    { key: 'warm', label: 'Warm starting', kind: 'toggle', def: 1 },
    { key: 'contacts', label: 'Contact rows', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.count);
    const radius = 0.3;
    const balls: BallSpec[] = [];
    for (let i = 0; i < count; i++) {
      // A deterministic wobble, so the stack is a stack and not a knife edge —
      // a perfectly aligned column is the one case a solver never has to fight.
      const wobble = params.jitter * Math.sin(i * 2.399963);
      balls.push({
        center: new Point3D(wobble, params.jitter * Math.cos(i * 2.399963), radius + i * (2 * radius + params.drop)),
        radius,
        mass: 1,
      });
    }
    const rig = makeRig({
      balls,
      friction: params.friction,
      restitution: params.restitution,
      warm: params.warm >= 0.5,
    });

    const object = new THREE.Group();
    object.add(groundObject(rig.ground, 3, 12));
    const cloud = new BallCloud(count, TINTS);
    object.add(cloud.group);
    // Always in the tree, so the viewer disposes it; the toggle hides it.
    const overlay = new ContactOverlay(count * 3 + 8);
    overlay.object.visible = params.contacts >= 0.5;
    object.add(overlay.object);

    const clock = new Clock(rig.world.TimeStep);
    let peakEnergy = 0;

    const draws = (): Draw[] =>
      toList(rig.world.Bodies)
        .slice(1)
        .map((body, i) => ({ center: body.Center, orientation: body.Orientation, radius: balls[i].radius }));

    const report = (): string => {
      const energy = kineticEnergy(rig.world);
      peakEnergy = Math.max(peakEnergy, energy);
      const pen = penetrationStats(rig.world);
      const top = toList(rig.world.Bodies)[count].Center.Z;
      const ideal = radius + (count - 1) * 2 * radius;
      return line([
        note('bodies', `${count} dynamic + 1 static`),
        note('rows', String(pen.rows)),
        note('kinetic energy', `${sci(energy)} J`),
        note('peak', `${sci(peakEnergy)} J`),
        note('max penetration', mm(pen.max)),
        note('mean penetration', mm(pen.mean)),
        note('PenetrationSlop', mm(rig.world.Solver.PenetrationSlop.Amount())),
        note('normal impulse held', `${n2(normalImpulseTotal(rig.world))} N s`),
        note('stack top', `${n3(top)} m of ${n3(ideal)} (sag ${mm(ideal - top)})`),
        note('warm starting', rig.warm ? 'on' : 'OFF — Constraints cleared each frame'),
        note('sim time', `${n1(clock.seconds)} s`),
        note('ms/frame', n2(clock.msPerFrame)),
      ]);
    };

    cloud.write(draws());
    overlay.write(toList(rig.world.Constraints), 0.08);

    return {
      object,
      initial: report(),
      step(seconds: number): string {
        clock.run(seconds, () => stepRig(rig));
        cloud.write(draws());
        overlay.write(toList(rig.world.Constraints), 0.08);
        return report();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 2 — the split impulse

const correction = simScene({
  id: 'correction',
  title: 'Split impulse — what the position pass does',
  description:
    'The stack is SPAWNED INTERPENETRATING, which is the case that separates a positional correction from a bias. ' +
    'Overlap is removed by SolvePositions: a copy of the bodies is taken at rest, PositionIterations normal-only ' +
    'sweeps drive each row toward Length.CorrectionSpeed, and the resulting twists reach the pose through ' +
    'IntegratePoseWith and are then discarded — no energy enters the real velocities, which is what the kinetic ' +
    'energy reading shows. Set BaumgarteFactor to 0 and the position pass does nothing: the stack stays exactly as ' +
    'interpenetrated as it spawned, forever, because the velocity solve has no way to fix a position error. ' +
    'PenetrationSlop is the overlap deliberately left uncorrected so collision detection keeps finding the contact; ' +
    'MaxCorrectionSpeed caps how fast a deeply overlapped body may crawl out, so it is not ejected.',
  plato: [
    'RigidWorld3D.SolvePositions',
    'RigidWorld3D.SolvePositionPass',
    'RigidWorld3D.SolvePositionRow',
    'SolverBody3D.IntegratePoseWith',
    'SolverBody3D.AtRest',
    'Length.CorrectionSpeed',
    'ContactSolverSettings.WithBaumgarteFactor',
    'ContactSolverSettings.WithPenetrationSlop',
    'ContactSolverSettings.WithMaxCorrectionSpeed',
    'RigidWorld3D.StepBallScene',
  ],
  viewer: STAGE,
  controls: [
    { key: 'count', label: 'Balls', kind: 'slider', min: 2, max: 8, step: 1, def: 4 },
    { key: 'overlap', label: 'Spawn overlap', kind: 'slider', min: 0, max: 0.9, step: 0.05, def: 0.65 },
    { key: 'baumgarte', label: 'BaumgarteFactor', kind: 'slider', min: 0, max: 1, step: 0.05, def: 0.2 },
    { key: 'slop', label: 'PenetrationSlop (mm)', kind: 'slider', min: 0, max: 40, step: 1, def: 5 },
    { key: 'maxSpeed', label: 'MaxCorrectionSpeed', kind: 'slider', min: 0.1, max: 5, step: 0.1, def: 3 },
    { key: 'posIter', label: 'PositionIterations', kind: 'slider', min: 0, max: 8, step: 1, def: 3 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.count);
    const radius = 0.3;
    const spacing = 2 * radius * (1 - params.overlap);
    const balls: BallSpec[] = [];
    for (let i = 0; i < count; i++) {
      balls.push({
        center: new Point3D(0, 0, radius * (1 - params.overlap) + i * spacing),
        radius,
        mass: 1,
      });
    }
    const solver = solverSettings({
      slop: params.slop / 1000,
      baumgarte: params.baumgarte,
      maxCorrection: params.maxSpeed,
    });
    const rig = makeRig({
      balls,
      solver,
      step: timeStep(8, params.posIter),
      friction: 0.5,
      restitution: 0.1,
    });

    const object = new THREE.Group();
    object.add(groundObject(rig.ground, 3, 12));
    const cloud = new BallCloud(count, TINTS);
    object.add(cloud.group);
    const overlay = new ContactOverlay(count * 3 + 8);
    object.add(overlay.object);

    const clock = new Clock(rig.world.TimeStep);
    const spawnOverlap = 2 * radius - spacing;
    let peakEnergy = 0;

    const draws = (): Draw[] =>
      toList(rig.world.Bodies)
        .slice(1)
        .map(body => ({ center: body.Center, orientation: body.Orientation, radius }));

    const report = (): string => {
      const pen = penetrationStats(rig.world);
      const energy = kineticEnergy(rig.world);
      peakEnergy = Math.max(peakEnergy, energy);
      const rows = toList(rig.world.Constraints);
      const deepest = rows.reduce<ContactConstraint3D | null>(
        (worst, row) => (worst === null || row.Penetration.Amount() > worst.Penetration.Amount() ? row : worst),
        null,
      );
      const seconds = rig.world.TimeStep.FixedDeltaTime.Seconds;
      return line([
        note('spawn overlap', mm(spawnOverlap)),
        note('rows', String(pen.rows)),
        note('max penetration', mm(pen.max)),
        note('mean penetration', mm(pen.mean)),
        note('BaumgarteFactor', n2(rig.world.Solver.BaumgarteFactor)),
        note('PenetrationSlop', mm(rig.world.Solver.PenetrationSlop.Amount())),
        note('MaxCorrectionSpeed', `${n1(rig.world.Solver.MaxCorrectionSpeed.Amount())} m/s`),
        note('PositionIterations', String(rig.world.TimeStep.PositionIterations)),
        deepest === null
          ? note('Length.CorrectionSpeed', 'no rows')
          : reading('Length.CorrectionSpeed(deepest)', () =>
              `${n3(deepest.Penetration.CorrectionSpeed(rig.world.Solver, seconds))} m/s`,
            ),
        note('kinetic energy', `${sci(energy)} J`),
        note('peak', `${sci(peakEnergy)} J`),
        note('sim time', `${n1(clock.seconds)} s`),
        note('ms/frame', n2(clock.msPerFrame)),
      ]);
    };

    cloud.write(draws());
    overlay.write(toList(rig.world.Constraints), 0.08);

    return {
      object,
      initial: report(),
      step(seconds: number): string {
        clock.run(seconds, () => stepRig(rig));
        cloud.write(draws());
        overlay.write(toList(rig.world.Constraints), 0.08);
        return report();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 3 — restitution
//
// Five INDEPENDENT worlds, one per coefficient, because `StepBallScene` takes
// one restitution for the whole scene: per-body materials would need per-pair
// coefficients, and the pair coefficient is what `Manifold` carries. Five worlds
// of one ball each cost less than one world of five.

const RESTITUTIONS = [0, 0.2, 0.45, 0.7, 0.9];

const restitution = simScene({
  id: 'restitution',
  title: 'Restitution, and the threshold that stops the buzz',
  description:
    'Five worlds side by side, identical but for the restitution handed to StepBallScene. The pale marker in each ' +
    'lane is the analytic first apex e^2 h; the bright one is where the ball actually reached. They agree to a few ' +
    'per cent, which is the claim that Number.RestitutionTarget resolves the bounce from the approach speed ' +
    'measured BEFORE the solver runs — read inside the iteration loop it would sample a velocity the loop had ' +
    'already changed, and the bounce would decay with the iteration count. RestitutionThreshold is the other half: ' +
    'a contact closing slower than it asks for zero separation speed, so a settling ball stops instead of buzzing ' +
    'at the frame rate forever, since gravity re-supplies exactly the approach speed restitution hands back. Drop ' +
    'the threshold toward zero and watch the low-restitution lanes never quite come to rest.',
  plato: [
    'Number.RestitutionTarget',
    'ContactSolverSettings.WithRestitutionThreshold',
    'ContactManifold3D.ConstraintRow',
    'ContactManifold3D.ConstraintRows',
    'RigidWorld3D.StepBallScene',
    'Sphere.Collide',
    'Proportion',
  ],
  viewer: { distance: 10, grid: false, spin: false },
  controls: [
    { key: 'height', label: 'Drop height (m)', kind: 'slider', min: 0.8, max: 3.5, step: 0.1, def: 2 },
    { key: 'radius', label: 'Radius (m)', kind: 'slider', min: 0.15, max: 0.4, step: 0.01, def: 0.28 },
    { key: 'threshold', label: 'RestitutionThreshold', kind: 'slider', min: 0.02, max: 2, step: 0.02, def: 1 },
    { key: 'markers', label: 'Apex markers', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const radius = params.radius;
    const height = params.height;
    const solver = solverSettings({ threshold: params.threshold });
    const lanes = RESTITUTIONS.map((e, i) =>
      makeRig({
        balls: [{ center: new Point3D(0, (i - 2) * 1.1, height), radius, mass: 1 }],
        solver,
        restitution: e,
        friction: 0.4,
      }),
    );

    const object = new THREE.Group();
    object.add(groundObject(FLAT_GROUND, 3.2, 12));
    const cloud = new BallCloud(lanes.length, TINTS);
    object.add(cloud.group);

    // The analytic prediction, drawn once: a ball leaving the floor at the
    // fraction e of its arrival speed reaches e^2 of the height it fell.
    const markers = new THREE.Group();
    RESTITUTIONS.forEach((e, i) => {
      const apex = radius + (height - radius) * e * e;
      markers.add(heightMarker(0, (i - 2) * 1.1, apex, 0.34, 0x55647d));
    });
    markers.visible = params.markers >= 0.5;
    object.add(markers);
    // The measured apexes, allocated here and only WRITTEN in tick: `build` is
    // the one place allowed to allocate. A lane whose apex has not been reached
    // yet keeps a zero-length segment, which draws nothing and stays finite.
    const measuredBuffer = new Float32Array(lanes.length * 6);
    const measuredGeometry = new THREE.BufferGeometry();
    measuredGeometry.setAttribute('position', new THREE.BufferAttribute(measuredBuffer, 3));
    const measuredLines = new THREE.LineSegments(
      measuredGeometry,
      new THREE.LineBasicMaterial({ color: palette.accent }),
    );
    measuredLines.visible = params.markers >= 0.5;
    object.add(measuredLines);
    const writeMeasured = (lane: number, apex: number): void => {
      const y = (lane - 2) * 1.1;
      measuredBuffer[lane * 6] = -0.34;
      measuredBuffer[lane * 6 + 1] = apex;
      measuredBuffer[lane * 6 + 2] = y;
      measuredBuffer[lane * 6 + 3] = 0.34;
      measuredBuffer[lane * 6 + 4] = apex;
      measuredBuffer[lane * 6 + 5] = y;
      measuredGeometry.attributes.position.needsUpdate = true;
    };

    const clock = new Clock(lanes[0].world.TimeStep);
    const state = lanes.map(() => ({ apex: -1, bounces: 0, rising: false, last: height }));

    const draws = (): Draw[] =>
      lanes.map(rig => {
        const body = toList(rig.world.Bodies)[1];
        return { center: body.Center, orientation: body.Orientation, radius: rig.specs[0].radius };
      });

    function observe(): void {
      lanes.forEach((rig, i) => {
        const z = toList(rig.world.Bodies)[1].Center.Z;
        const s = state[i];
        if (z > s.last && !s.rising) {
          s.rising = true;
          s.bounces++;
        }
        if (z < s.last && s.rising) {
          s.rising = false;
          if (s.apex < 0) {
            s.apex = s.last;
            writeMeasured(i, s.apex);
          }
        }
        s.last = z;
      });
    }

    const report = (): string => {
      const parts: Reading[] = RESTITUTIONS.map((e, i) => {
        const s = state[i];
        const predicted = radius + (height - radius) * e * e;
        const seen = s.apex < 0 ? '—' : n3(s.apex);
        return note(`e=${n2(e)}`, `apex ${seen} / ${n3(predicted)} m, ${s.bounces} bounces`);
      });
      const energy = lanes.reduce((sum, rig) => sum + kineticEnergy(rig.world), 0);
      const rows = lanes.reduce((sum, rig) => sum + rig.world.Constraints.Count(), 0);
      return line([
        ...parts,
        note('rows', String(rows)),
        note('kinetic energy', `${sci(energy)} J`),
        note('RestitutionThreshold', `${n2(params.threshold)} m/s`),
        note('sim time', `${n1(clock.seconds)} s`),
        note('ms/frame', n2(clock.msPerFrame)),
      ]);
    };

    cloud.write(draws());

    return {
      object,
      initial: report(),
      step(seconds: number): string {
        clock.run(seconds, () => {
          for (const rig of lanes) stepRig(rig);
          observe();
        });
        cloud.write(draws());
        return report();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 4 — Gauss-Seidel convergence

const ITERATION_SETS: number[][] = [
  [1, 4, 16],
  [1, 2, 4],
  [2, 8, 32],
];

const iterations = simScene({
  id: 'iterations',
  title: 'Velocity iterations — Gauss-Seidel, seen',
  description:
    'Three worlds of the same stack, differing only in TimeStepSettings.VelocityIterations. The stack is ' +
    'HEAVY-ON-LIGHT — a massive ball on top of light ones — because that is the case a sequential-impulse solver ' +
    'has to work at: the bottom contact must find an impulse large enough to hold the whole column, and each sweep ' +
    'only propagates the load one contact further. SolveVelocities is a fold over the iteration counter whose unit ' +
    'is SolveVelocityPass, itself a fold over the rows in order — so row n sees row n-1\'s work, which is exactly ' +
    'why a Jacobi sweep of the same rows oscillates on a stack and this one does not. At one iteration the heavy ' +
    'ball sinks through the column and the kinetic energy never reaches zero; at sixteen the stack holds at the ' +
    'penetration slop. The cost is exactly linear in the count, which the millisecond reading shows.',
  plato: [
    'RigidWorld3D.SolveVelocities',
    'RigidWorld3D.SolveVelocityPass',
    'RigidWorld3D.SolveRow',
    'RigidWorld3D.WarmStart',
    'RigidWorld3D.WarmStartRow',
    'Number.ImpulseFor',
    'SolverBody3D.DirectionalMass',
    'SolverBody3D.SeparationSpeed',
    'TimeStepSettings',
  ],
  viewer: STAGE,
  controls: [
    { key: 'set', label: 'Iterations', kind: 'select', options: ['1·4·16', '1·2·4', '2·8·32'], def: 0 },
    { key: 'height', label: 'Stack height', kind: 'slider', min: 2, max: 6, step: 1, def: 4 },
    { key: 'ratio', label: 'Top mass (kg)', kind: 'slider', min: 1, max: 200, step: 1, def: 100 },
    { key: 'contacts', label: 'Contact rows', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const counts = ITERATION_SETS[clampIndex(params.set, ITERATION_SETS.length)];
    const height = Math.round(params.height);
    const radius = 0.3;
    const columns = counts.map((velocityIterations, column) => {
      const balls: BallSpec[] = [];
      for (let i = 0; i < height; i++) {
        balls.push({
          center: new Point3D((column - 1) * 1.6, 0, radius + i * (2 * radius + 0.01)),
          radius,
          mass: i === height - 1 ? params.ratio : 1,
        });
      }
      return {
        velocityIterations,
        rig: makeRig({ balls, step: timeStep(velocityIterations, 3), friction: 0.5, restitution: 0.1 }),
      };
    });

    const object = new THREE.Group();
    object.add(groundObject(FLAT_GROUND, 3.4, 14));
    const total = height * columns.length;
    // The heavy ball is the last of each column, so it lands on a different
    // tint and reads as the load rather than as another ball.
    const tints: number[] = [];
    for (let c = 0; c < columns.length; c++) {
      for (let i = 0; i < height; i++) tints.push(i === height - 1 ? palette.surfaceAlt : palette.surface);
    }
    const cloud = new BallCloud(total, tints);
    object.add(cloud.group);
    const overlay = new ContactOverlay(total * 3 + 12);
    overlay.object.visible = params.contacts >= 0.5;
    object.add(overlay.object);

    const clock = new Clock(columns[0].rig.world.TimeStep);

    const draws = (): Draw[] => {
      const out: Draw[] = [];
      for (const { rig } of columns) {
        for (const body of toList(rig.world.Bodies).slice(1)) {
          out.push({ center: body.Center, orientation: body.Orientation, radius });
        }
      }
      return out;
    };

    const allRows = (): ContactConstraint3D[] =>
      columns.flatMap(({ rig }) => toList(rig.world.Constraints));

    const ideal = radius + (height - 1) * 2 * radius;
    const report = (): string => {
      const parts = columns.map(({ velocityIterations, rig }) => {
        const top = toList(rig.world.Bodies)[height].Center.Z;
        const pen = penetrationStats(rig.world);
        return note(
          `${velocityIterations} iter`,
          `KE ${sci(kineticEnergy(rig.world))} J, top ${n3(top)} m (sag ${mm(ideal - top)}), maxPen ${mm(pen.max)}`,
        );
      });
      return line([
        ...parts,
        note('ideal top', `${n3(ideal)} m`),
        note('top mass', `${n1(params.ratio)} kg on ${height - 1} × 1 kg`),
        note('rows', String(allRows().length)),
        note('sim time', `${n1(clock.seconds)} s`),
        note('ms/frame (all three)', n2(clock.msPerFrame)),
      ]);
    };

    cloud.write(draws());
    overlay.write(allRows(), 0.01);

    return {
      object,
      initial: report(),
      step(seconds: number): string {
        clock.run(seconds, () => {
          for (const { rig } of columns) stepRig(rig);
        });
        cloud.write(draws());
        overlay.write(allRows(), 0.01);
        return report();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 5 — friction on a slope

const FRICTIONS = [0, 0.1, 0.25, 0.6];
const SLOPE_RESET = 7;

const friction = simScene({
  id: 'friction',
  title: 'Friction on a slope — rolling, slipping, and the line between',
  description:
    'One tilted ground plane, four lanes, four Coulomb coefficients. The ground is a Plane in Hesse form and the ' +
    'grid drawn on it comes from Direction3D.Tangent and Bitangent — the same two axes each constraint row stores ' +
    'its accumulated friction impulse in, which is why the row keeps a stored basis instead of recomputing one per ' +
    'iteration: a basis that flipped between iterations would silently discard the impulse. The reading that ' +
    'matters is SLIP, the material speed of the ball at its contact point, from SolverBody3D.VelocityAt. At zero ' +
    'friction a ball slides down without ever turning and slip equals its speed; above the critical coefficient ' +
    '(2/7 tan of the tilt, for a solid ball) the contact sticks, slip falls to zero and the ball rolls — and more ' +
    'friction beyond that point changes nothing at all, because there is no relative motion left to oppose. Balls ' +
    'that run off the end are respawned at the top through Array.ReplacedAt.',
  plato: [
    'SolverBody3D.VelocityAt',
    'Direction3D.Tangent',
    'Direction3D.Bitangent',
    'Plane.ClosestPoint',
    'Plane.SignedDistance',
    'Sphere.Collide',
    'Array.ReplacedAt',
    'RigidWorld3D.WithBodies',
    'RigidWorld3D.StepBallScene',
  ],
  viewer: WIDE_STAGE,
  controls: [
    { key: 'tilt', label: 'Tilt (deg)', kind: 'slider', min: 5, max: 55, step: 1, def: 40 },
    { key: 'radius', label: 'Radius (m)', kind: 'slider', min: 0.15, max: 0.45, step: 0.01, def: 0.3 },
    { key: 'restitution', label: 'Restitution', kind: 'slider', min: 0, max: 0.6, step: 0.05, def: 0 },
  ],
  build(params: Params): Built {
    const tilt = (params.tilt * Math.PI) / 180;
    const ground = tiltedGround(tilt);
    const radius = params.radius;
    // Down-slope is gravity with its normal component removed, which is the
    // direction a ball actually runs; the lanes are laid out across it. Reading
    // it off the world's own gravity rather than off `Tangent` keeps it correct
    // for any tilt — `Tangent` picks whichever cardinal axis the normal leans on
    // least and is therefore only guaranteed to span the plane, not to point
    // downhill.
    const gravity = UniformGravity3D.Earth().Acceleration;
    const downSlope = gravity
      .Subtract(ground.Normal.Vector.Multiply(gravity.Dot(ground.Normal.Vector)))
      .Normalize();
    const across = ground.Normal.Vector.Cross(downSlope).Normalize();
    const start = (lane: number): Point3D =>
      ground
        .ClosestPoint(new Point3D(0, 0, 0))
        .Add(downSlope.Multiply(-2.2))
        .Add(across.Multiply((lane - 1.5) * 1.2))
        .Add(ground.Normal.Vector.Multiply(radius));

    const lanes = FRICTIONS.map((mu, i) =>
      makeRig({
        balls: [{ center: start(i), radius, mass: 1 }],
        ground,
        friction: mu,
        restitution: params.restitution,
      }),
    );

    const object = new THREE.Group();
    object.add(groundObject(ground, 5, 20));
    const cloud = new BallCloud(lanes.length, TINTS);
    object.add(cloud.group);

    const clock = new Clock(lanes[0].world.TimeStep);
    const respawns = lanes.map(() => 0);

    const draws = (): Draw[] =>
      lanes.map(rig => {
        const body = toList(rig.world.Bodies)[1];
        return { center: body.Center, orientation: body.Orientation, radius };
      });

    /** How far down-slope the ball has travelled, in the plane's own frame. */
    const travel = (rig: Rig, lane: number): number =>
      start(lane).Between(toList(rig.world.Bodies)[1].Center).Dot(downSlope);

    function recycle(): void {
      lanes.forEach((rig, i) => {
        if (travel(rig, i) < SLOPE_RESET) return;
        // The array primitive the whole solver fold is written on, used here for
        // the one thing a demo needs it for: putting one body back.
        rig.world = rig.world
          .WithBodies(
            materialized(
              bodyArray(rig.world.Bodies).ReplacedAt(1, ballBody({ center: start(i), radius, mass: 1 })),
            ),
          )
          .WithConstraints(EMPTY_ROWS);
        respawns[i]++;
      });
    }

    const report = (): string => {
      const parts = lanes.map((rig, i) => {
        const body = toList(rig.world.Bodies)[1];
        const contact = rig.ground.ClosestPoint(body.Center);
        const slip = reading('SolverBody3D.VelocityAt', () => n2(body.VelocityAt(contact).Magnitude()));
        return note(
          `mu=${n2(FRICTIONS[i])}`,
          `slip ${slip.value} m/s, |v| ${n2(body.Velocity.Linear.Magnitude())}, |w| ${n1(
            body.Velocity.Angular.Magnitude(),
          )} rad/s, run ${n1(travel(rig, i))} m`,
        );
      });
      const critical = (2 / 7) * Math.tan(tilt);
      return line([
        ...parts,
        note('tilt', `${n1(params.tilt)}° (tan ${n2(Math.tan(tilt))})`),
        note('rolling above mu', n3(critical)),
        note('respawns', respawns.join('/')),
        note('sim time', `${n1(clock.seconds)} s`),
        note('ms/frame', n2(clock.msPerFrame)),
      ]);
    };

    cloud.write(draws());

    return {
      object,
      initial: report(),
      step(seconds: number): string {
        clock.run(seconds, () => {
          for (const rig of lanes) stepRig(rig);
          recycle();
        });
        cloud.write(draws());
        return report();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 6 — impulses

const impulse = simScene({
  id: 'impulse',
  title: 'Impulses — a kick, and what the solver does with it',
  description:
    'A pile of balls kicked by a periodic impulse. Each kick is SolverBody3D.ApplyImpulse: a newton-second ' +
    'vector and the world point it acts at, applied to every body within the radius of the pile\'s own centre, ' +
    'falling off linearly to nothing at the edge. Move the application point off the centre of mass and the same ' +
    'impulse also spins the ball, because ApplyImpulse takes the lever arm through AngularResponse — that is the ' +
    'whole of what "applied ' +
    'at a point" means. rigid-dynamics.library.plato also declares ApplyImpulse over AppliedImpulse3D and over ' +
    'RadialImpulse3D, which is exactly this kick as a value; both are dropped overloads in the emitted library ' +
    'and the status line below calls them live and says so. Watch the kinetic energy: it spikes on the kick and ' +
    'decays back to nothing between them, which is the reading that says the contact solver is dissipating rather ' +
    'than injecting. The damping slider is SolverBody3D.LinearDamping and AngularDamping, an exponential decay rate ' +
    'per second that reaches the step through Number.DampingFactor — exp(-rate dt), the exact solution of ' +
    'dv/dt = -rate v and therefore stable at any step size. At zero it is off, and a ball kicked clear of the pile ' +
    'rolls away across the infinite plane forever, because nothing here models rolling resistance.',
  plato: [
    'SolverBody3D.ApplyImpulse',
    'SolverBody3D.AngularResponse',
    'SolverBody3D.VelocityAt',
    'SolverBody3D.WithLinearDamping',
    'SolverBody3D.WithAngularDamping',
    'Number.DampingFactor',
    'Array.ReplacedAt',
    'RigidWorld3D.WithBodies',
    'RigidWorld3D.StepBallScene',
    'AppliedImpulse3D',
    'RadialImpulse3D',
  ],
  viewer: STAGE,
  controls: [
    { key: 'count', label: 'Balls', kind: 'slider', min: 4, max: 20, step: 1, def: 12 },
    { key: 'magnitude', label: 'Impulse (N s)', kind: 'slider', min: 0, max: 8, step: 0.1, def: 3 },
    { key: 'period', label: 'Period (s)', kind: 'slider', min: 0.5, max: 4, step: 0.1, def: 1.6 },
    { key: 'offset', label: 'Off-centre (spin)', kind: 'slider', min: 0, max: 1, step: 0.05, def: 0.6 },
    { key: 'spread', label: 'Outward share', kind: 'slider', min: 0, max: 1, step: 0.05, def: 0.35 },
    { key: 'reach', label: 'Kick radius (m)', kind: 'slider', min: 0.5, max: 5, step: 0.1, def: 3 },
    { key: 'damping', label: 'Damping (per s)', kind: 'slider', min: 0, max: 4, step: 0.1, def: 1.4 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.count);
    const radius = 0.24;
    const balls: BallSpec[] = [];
    for (let i = 0; i < count; i++) {
      const ring = Math.floor(i / 6);
      const angle = (i % 6) * (Math.PI / 3) + ring * 0.5;
      const spread = 0.34 + ring * 0.34;
      balls.push({
        center: new Point3D(
          Math.cos(angle) * spread,
          Math.sin(angle) * spread,
          radius + ring * 0.5 + (i % 3) * 0.02,
        ),
        radius,
        mass: 1,
        // Without damping a ball kicked clear of the pile rolls away across the
        // infinite plane forever and never comes back into blast range, so the
        // second blast has nothing to act on. `SolverBody3D.LinearDamping` is
        // the library's own answer, applied through `DampingFactor`.
        linearDamping: params.damping,
        angularDamping: params.damping,
      });
    }
    const rig = makeRig({ balls, friction: 0.5, restitution: 0.25 });

    /** The kick follows the pile rather than a fixed point, so it keeps working. */
    const pileCentre = (): Point3D => {
      const bodies = toList(rig.world.Bodies).slice(1);
      let x = 0;
      let y = 0;
      for (const body of bodies) {
        x += body.Center.X;
        y += body.Center.Y;
      }
      return new Point3D(x / bodies.length, y / bodies.length, 0.05);
    };
    let blastCentre = pileCentre();

    const object = new THREE.Group();
    object.add(groundObject(FLAT_GROUND, 3.4, 14));
    const cloud = new BallCloud(count, TINTS);
    object.add(cloud.group);

    const clock = new Clock(rig.world.TimeStep);
    let untilBlast = 0.4;
    let blasts = 0;
    let lastTotal = 0;
    let peakEnergy = 0;

    const draws = (): Draw[] =>
      toList(rig.world.Bodies)
        .slice(1)
        .map(body => ({ center: body.Center, orientation: body.Orientation, radius }));

    /**
     * The kick, shaped like `ApplyImpulse(RadialImpulse3D)` — full magnitude at
     * the centre, falling linearly to nothing at the radius — except that the
     * direction is mostly upward with an outward share, because a purely radial
     * blast disperses the pile past its own radius after two or three goes and
     * then has nothing left to act on. The application point is offset around
     * the ball, which is the lever arm that turns a shove into a spin.
     */
    function kick(): void {
      blastCentre = pileCentre();
      const bodies = toList(rig.world.Bodies);
      let applied = 0;
      let updated = rig.world.Bodies;
      for (let i = 1; i < bodies.length; i++) {
        const body = bodies[i];
        const away = blastCentre.Between(body.Center);
        const distance = away.Magnitude();
        if (distance >= params.reach) continue;
        const falloff = 1 - distance / params.reach;
        const outward =
          distance > 0 ? away.Divide(distance).Multiply(params.spread) : new Vector3D(0, 0, 0);
        const direction = outward.Add(new Vector3D(0, 0, 1)).Normalize();
        const push = direction.Multiply(params.magnitude * falloff);
        const sideways = direction.Cross(new Vector3D(0, 0, 1));
        const lever =
          sideways.MagnitudeSquared() > 0
            ? sideways.Normalize().Multiply(radius * params.offset)
            : new Vector3D(0, 0, 0);
        updated = bodyArray(updated).ReplacedAt(i, body.ApplyImpulse(push, body.Center.Add(lever)));
        applied += push.Magnitude();
      }
      rig.world = rig.world.WithBodies(materialized(updated));
      lastTotal = applied;
      blasts++;
    }

    const report = (): string => {
      const energy = kineticEnergy(rig.world);
      peakEnergy = Math.max(peakEnergy, energy);
      const pen = penetrationStats(rig.world);
      return line([
        note('bodies', String(count)),
        note('rows', String(pen.rows)),
        note('kinetic energy', `${sci(energy)} J`),
        note('peak', `${sci(peakEnergy)} J`),
        note('kicks', String(blasts)),
        note('last kick', `${n2(lastTotal)} N s over ${count} bodies`),
        note('next in', `${n1(Math.max(0, untilBlast))} s`),
        note('max penetration', mm(pen.max)),
        reading('Number.DampingFactor(dt)', () =>
          n4(params.damping.DampingFactor(rig.world.TimeStep.FixedDeltaTime.Seconds)),
        ),
        reading('ApplyImpulse(AppliedImpulse3D)', () => {
          const body = toList(rig.world.Bodies)[1];
          const applied = new AppliedImpulse3D(new Vector3D(0, 0, 1), body.Center);
          return `${n2(asDropped(body).ApplyImpulse(applied).Velocity.Linear.Z)} m/s`;
        }),
        reading('ApplyImpulse(RadialImpulse3D)', () => {
          const body = toList(rig.world.Bodies)[1];
          const value = new RadialImpulse3D(
            blastCentre,
            Impulse.FromAmount(params.magnitude),
            Length.FromAmount(params.reach),
          );
          return `${n2(asDropped(body).ApplyImpulse(value).Velocity.Linear.Magnitude())} m/s`;
        }),
        note('ms/frame', n2(clock.msPerFrame)),
        note('sim time', `${n1(clock.seconds)} s`),
      ]);
    };

    cloud.write(draws());

    return {
      object,
      initial: report(),
      step(seconds: number): string {
        const steps = clock.run(seconds, () => stepRig(rig));
        if (steps > 0) {
          untilBlast -= steps * rig.world.TimeStep.FixedDeltaTime.Seconds;
          if (untilBlast <= 0) {
            kick();
            untilBlast += params.period;
          }
        }
        cloud.write(draws());
        return report();
      },
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 7 — the narrow phase, still
//
// The one entry on this page that does not tick. Two balls and the ground, with
// the manifold and the rows it flattens into drawn and named, plus a live census
// of the pair tests: which ones survived the TypeScript writer's overload
// dropping and which ones did not.

const narrowphase = stillScene({
  id: 'narrowphase',
  title: 'The narrow phase, opened up',
  description:
    'One frame of collision detection, held still. Two balls and a ground plane produce manifolds through ' +
    'BallSceneManifolds (every ordered pair with A below B, plus every ball against the plane, with no broad phase ' +
    '— the cost is the square of the body count, which is why this page keeps its counts modest). Flatten turns ' +
    'each manifold point into a ContactConstraint3D with its friction basis and its restitution target resolved ' +
    'once, and WarmStartFrom copies the previous frame\'s impulses onto rows matching by ordered body pair and ' +
    'contact position. Every row is drawn: the point, the normal from A toward B, and the tangent and bitangent it ' +
    'stores its friction impulse in. The census at the end of the status line calls each declared pair test and ' +
    'reports what came back — three of the six are damaged in the emitted library, and this page says which rather ' +
    'than hiding it.',
  plato: [
    'SolverBody3D.BallSceneManifolds',
    'BodyIndex.Manifold',
    'ContactManifold3D.ConstraintRows',
    'ContactManifold3D.Flatten',
    'ContactConstraint3D.WarmStartFrom',
    'ContactConstraint3D.SameContact',
    'Direction3D.Tangent',
    'Direction3D.Bitangent',
    'Number.RestitutionTarget',
    'Point3D.FreshContact',
    'Sphere.Collide',
    'Box3D.Collide',
    'Box3D.BoxCorner',
    'SolverBody3D.BallOf',
  ],
  viewer: { distance: 4, grid: false, spin: false },
  controls: [
    { key: 'gap', label: 'Ball separation (m)', kind: 'slider', min: 0.3, max: 0.9, step: 0.01, def: 0.55 },
    { key: 'lift', label: 'Lower ball height (m)', kind: 'slider', min: 0.15, max: 0.45, step: 0.005, def: 0.285 },
    { key: 'lean', label: 'Lean (m)', kind: 'slider', min: -0.4, max: 0.4, step: 0.01, def: 0.12 },
    { key: 'friction', label: 'Friction', kind: 'slider', min: 0, max: 1, step: 0.05, def: 0.5 },
    { key: 'restitution', label: 'Restitution', kind: 'slider', min: 0, max: 0.9, step: 0.05, def: 0.3 },
    { key: 'approach', label: 'Approach speed (m/s)', kind: 'slider', min: -4, max: 0, step: 0.1, def: -1.6 },
  ],
  build(params: Params) {
    const radius = 0.3;
    const lower: BallSpec = { center: new Point3D(0, 0, params.lift), radius, mass: 1 };
    const upper: BallSpec = {
      center: new Point3D(params.lean, 0, params.lift + params.gap),
      radius,
      mass: 1,
    };
    const rig = makeRig({
      balls: [lower, upper],
      friction: params.friction,
      restitution: params.restitution,
    });
    // A closing speed on the upper ball, so RestitutionTarget has something to
    // resolve: with both bodies at rest every row asks for zero.
    const moving = toList(rig.world.Bodies)[2];
    rig.world = rig.world.WithBodies(
      materialized(
        bodyArray(rig.world.Bodies).ReplacedAt(
          2,
          moving.WithVelocity(
            new SpatialVelocity3D(new Vector3D(0, 0, params.approach), new Vector3D(0, 0, 0)),
          ),
        ),
      ),
    );

    const object = new THREE.Group();
    object.add(groundObject(rig.ground, 1.6, 8));
    const cloud = new BallCloud(2, TINTS);
    object.add(cloud.group);
    cloud.write(
      toList(rig.world.Bodies)
        .slice(1)
        .map(body => ({ center: body.Center, orientation: body.Orientation, radius })),
    );

    const manifolds = toList(
      bodyArray(rig.world.Bodies).BallSceneManifolds(
        rig.radii,
        rig.ground,
        GROUND_BODY,
        rig.friction,
        rig.restitution,
      ),
    );
    const rows = toList(
      rig.world.BallSceneConstraints(
        rig.radii,
        rig.ground,
        GROUND_BODY,
        rig.friction,
        rig.restitution,
        rig.tolerance,
      ),
    );

    // Each row as its three axes, at three lengths so they are told apart.
    const axes: number[] = [];
    const push = (from: Point3D, direction: Vector3D, length: number): void => {
      const to = from.Add(direction.Multiply(length));
      axes.push(from.X, from.Z, from.Y, to.X, to.Z, to.Y);
    };
    const tangentAxes: number[] = [];
    for (const row of rows) {
      push(row.Point, row.Normal.Vector, 0.32);
      const to1 = row.Point.Add(row.Tangent.Vector.Multiply(0.2));
      const to2 = row.Point.Add(row.Bitangent.Vector.Multiply(0.2));
      tangentAxes.push(row.Point.X, row.Point.Z, row.Point.Y, to1.X, to1.Z, to1.Y);
      tangentAxes.push(row.Point.X, row.Point.Z, row.Point.Y, to2.X, to2.Z, to2.Y);
    }
    if (axes.length > 0) object.add(segments(axes, 0xffd479));
    if (tangentAxes.length > 0) object.add(segments(tangentAxes, palette.accent, 0.9));
    if (rows.length > 0) object.add(dots(rows.map(row => row.Point), 0xffffff, 10));

    // The pair-test census. `Sphere.Collide(Sphere)` and `Sphere.Collide(Plane)`
    // are what this page runs on; the other four are called here so the page
    // reports the state of the emitted library rather than assuming it.
    const probeSphere = new Sphere(new Point3D(0, 0, 0.5), 1);
    const probeBox = new Box3D(new Point3D(0, 0, 0.5), new Size3D(1, 1, 1), Quaternion.Identity());
    const lowBox = new Box3D(new Point3D(0, 0, 0.2), new Size3D(1, 1, 1), Quaternion.Identity());
    const probeCapsule = new Capsule3D(new Point3D(0, 0, 1.2), new Point3D(0, 1, 1.2), 0.5);
    const distinctCorners = (): number => {
      const seen = new Set<string>();
      for (let i = 0; i < 8; i++) {
        const c = lowBox.BoxCorner(i);
        seen.add(`${c.X},${c.Y},${c.Z}`);
      }
      return seen.size;
    };

    const readings: Reading[] = [
      note('bodies', '2 dynamic + 1 static ground'),
      reading('BallSceneManifolds', () => `${manifolds.length} manifolds`),
      reading('manifold points', () =>
        String(manifolds.reduce((sum, m) => sum + m.Points.Count(), 0)),
      ),
      reading('Flatten + WarmStartFrom', () => `${rows.length} rows`),
      ...rows.map((row, i) =>
        note(
          `row ${i}`,
          `bodies ${row.BodyA.Value}→${row.BodyB.Value}, pen ${mm(row.Penetration.Amount())}, ` +
            `target ${n3(row.TargetSeparationSpeed)} m/s, mu ${n2(row.Friction)}, e ${n2(row.Restitution.Value)}`,
        ),
      ),
      rows.length > 0
        ? reading('row 0 frame', () => {
            const row = rows[0];
            const n = row.Normal.Vector;
            const t = row.Tangent.Vector;
            const b = row.Bitangent.Vector;
            return (
              `n (${n2(n.X)}, ${n2(n.Y)}, ${n2(n.Z)})  t (${n2(t.X)}, ${n2(t.Y)}, ${n2(t.Z)})  ` +
              `b (${n2(b.X)}, ${n2(b.Y)}, ${n2(b.Z)})  n·t ${n4(n.Dot(t))}  n·b ${n4(n.Dot(b))}`
            );
          })
        : note('row 0 frame', 'no rows — the balls are apart'),
      rows.length > 1
        ? reading('SameContact(row0, row1)', () => String(rows[0].SameContact(rows[1], rig.tolerance)))
        : note('SameContact', 'needs two rows'),
      note('— pair tests —', 'the narrow phase as emitted'),
      reading('Sphere.Collide(Sphere)', () =>
        `${probeSphere.Collide(new Sphere(new Point3D(0, 0, 2), 1.6)).Count()} points (overlapping pair)`,
      ),
      reading('Sphere.Collide(Plane)', () =>
        `${collideWith(probeSphere, rig.ground).Count()} points (shimmed by the prelude)`,
      ),
      reading('Sphere.Collide(Box3D)', () => {
        const points = collideWith(probeSphere, probeBox).Count();
        return points === 0
          ? '0 points for a box overlapping by 1.0 m — WRONG, the ball-vs-ball body ran (dropped overload)'
          : `${points} points`;
      }),
      reading('Sphere.Collide(Capsule3D)', () =>
        `${collideWith(probeSphere, probeCapsule).Count()} points`,
      ),
      reading('Box3D.Collide(Plane)', () => {
        const points = lowBox.Collide(rig.ground).Count();
        return points === 4
          ? '4 points (a box lying flat)'
          : `${points} points where a flat-lying box has 4 — WRONG, see BoxCorner`;
      }),
      reading('Box3D.BoxCorner', () => {
        const distinct = distinctCorners();
        return distinct === 8
          ? '8 distinct corners'
          : `${distinct} distinct corners of 8 — WRONG, index/2 and index/4 are Integer divisions emitted as float`;
      }),
      reading('Sphere.PrincipalMoments', () =>
        n4(new Sphere(new Point3D(0, 0, 0), radius).PrincipalMoments(Mass.FromAmount(1)).X),
      ),
      reading('Box3D.PrincipalMoments', () => n4(probeBox.PrincipalMoments(Mass.FromAmount(1)).X)),
      reading('Capsule3D.PrincipalMoments', () =>
        n4(probeCapsule.PrincipalMoments(Mass.FromAmount(1)).Z),
      ),
      reading('Cylinder.PrincipalMoments', () =>
        n4(
          new Cylinder(
            new Point3D(0, 0, 0),
            new Direction3D(new Vector3D(0, 0, 1)),
            0.3,
            1,
          ).PrincipalMoments(Mass.FromAmount(1)).Z,
        ),
      ),
    ];

    return { object, readings };
  },
});

// ---------------------------------------------------------------------------

const demo: Demo = {
  title: 'Rigid bodies',
  subtitle: 'rigid-dynamics.{types,library}.plato · collision.{types,library}.plato',
  scenes: [stack, correction, restitution, iterations, friction, impulse, narrowphase],
};

mountDemo(demo);

export { demo };
