// Parametric surfaces — a scene catalog over `stdlib/geometry/surfaces.{types,library}.plato`,
// `surfaces-shapes.{types,library}.plato`, `surfaces-solids.concepts.plato` and
// `differential-geometry.types.plato`.
//
// Every point on every surface here is one call to a generated `Eval` from
// `src/plato/plato.g.ts`. This file builds the defining parameters (radii,
// exponents, domains, generator curves, control nets), walks a (u, v) lattice,
// and repacks the answers into Three.js buffers. `ClosedU` / `ClosedV` decide
// whether the lattice seams shut. No parameterization is written out here — a
// surface that does not evaluate keeps its member name in the status line and
// says `UNAVAILABLE (…)`, the pattern `src/demos/polygons.ts` uses for
// triangulation.
//
// Three gaps in the emitted library are visible from this page; each is named at
// the reading that would have used it:
//
//   * `BezierPatch.Eval` and `BSplineSurface.Eval` throw. Both bodies read the
//     control net's `RowCount` / `ColumnCount` as Plato properties, and the
//     TypeScript writer emits them without call parentheses, so the body sees a
//     function object where it wants a count.
//   * `CornerPoint00/10/01/11` and `CenterPoint` — the derived
//     `IParametricSurface` samples in `surfaces.library.plato` — are emitted as
//     `this.Eval(new Tuple2(0.5, 0.5))`. `Tuple2` carries `X0`/`X1`, not
//     `U`/`V`, so every one of them either throws or returns NaN on every
//     surface. The scenes call `Eval` with a real `UvCoordinate` instead and say
//     so.
//   * No concrete type implements `IDifferentiableSurface`, so the whole
//     `FirstFundamentalE/F/G`, `NormalAtCenter`, `TangentUAtCenter`,
//     `IsOrthogonalAt` family is emitted on nothing, and the record types in
//     `differential-geometry.types.plato` have no producing body anywhere in the
//     stdlib. The curvature scene says exactly that, and states what it did
//     instead.
//
// Cost. `build` runs on every slider tick and a lattice is quadratic, so the
// default resolutions are modest (40 x 40 for a gallery, 30 x 30 for the
// curvature scene, which spends ten evaluations per node on its difference
// stencil) and every resolution slider is capped. The wireframe is repacked from
// the mesh lattice rather than resampled, so it costs no extra evaluations.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { fromArray, parametricGeometry, polylineGeometry } from '../shared/mesh.js';
import { palette, type ViewerOptions } from '../shared/viewer.js';
import {
  Angle,
  AngleInterval,
  BezierPatch,
  BilinearPatch,
  BSplineSurface,
  BohemianDome,
  Bounds2D,
  BoySurface,
  BreatherSurface,
  CatalanSurface,
  Catenoid,
  CircularArc2D,
  Cone,
  ConicalFrustum,
  CoonsPatch,
  CrossCap,
  CubicBezier3D,
  Cylinder,
  DiniSurface,
  Direction3D,
  DupinCyclide,
  Ellipsoid,
  EnneperSurface,
  ExtrudedSurface,
  FirstFundamentalForm,
  Frame3D,
  Helicoid,
  Helix,
  HennebergSurface,
  HyperbolicParaboloid,
  HyperboloidOfOneSheet,
  HyperboloidOfTwoSheets,
  InfiniteLine3D,
  KleinBottle,
  KnotVector,
  KuenSurface,
  LoftedSurface,
  MobiusStrip,
  MonkeySaddle,
  Number2,
  Number3,
  Number4,
  NumberInterval,
  OffsetSurface,
  PlanarPatch,
  PluckerConoid,
  Point2D,
  Point3D,
  Polyline3D,
  Pseudosphere,
  Quaternion,
  RomanSurface,
  RuledSurface,
  ScherkSurface,
  SeashellSurface,
  SecondFundamentalForm,
  Sphere,
  SphericalHarmonicSurface,
  SphericalPatch,
  Superellipsoid,
  Superformula2D,
  Superformula3D,
  Supertoroid,
  SurfaceOfRevolution,
  SurfacePoint3D,
  Torus,
  TangentSpace3D,
  TrefoilKnot,
  TubeSurface,
  SweptSurface,
  UvCoordinate,
  Vector3D,
  WhitneyUmbrella,
  type IArray,
  type IArray2D,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// Same contract as `src/demos/polygons.ts`: a member that throws is a gap in the
// emitted library, not a fact about the geometry, so the reading keeps the
// member's name and the failure rather than substituting an answer.

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
const n4 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(4);
const p3s = (p: Point3D): string => `(${n2(p.X)}, ${n2(p.Y)}, ${n2(p.Z)})`;
const v3s = (v: Vector3D): string => `(${n2(v.X)}, ${n2(v.Y)}, ${n2(v.Z)})`;

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
// The shape every generated parametric surface has in TypeScript.
//
// `IParametricSurface<Self>` is self-typed in the emitted library — its bases
// reach down to `IEquatable<Self>` — so no concrete surface structurally
// satisfies the emitted interface for any other type. The three members this
// page actually needs are the same on every one of them.

interface ParametricSurface {
  Eval(uv: UvCoordinate): Point3D;
  ClosedU(): boolean;
  ClosedV(): boolean;
}

/**
 * The same self-typing makes `ICurve3D<Self>` / `ICurve2D<Self>` / the
 * `IParametricSurface` field of `OffsetSurface` unassignable from any concrete
 * curve or surface. The cast records that the constraint is a writer artefact,
 * not a claim about the value: the bodies that consume these fields call only
 * `Eval`, `ClosedU` and `ClosedV`, all of which the value really has.
 */
function asGenerated<T>(value: object): T {
  return value as unknown as T;
}

const TAU = Math.PI * 2;
const clamp01 = (x: number): number => (x < 0 ? 0 : x > 1 ? 1 : x);

/**
 * A lattice sampler over one generated `Eval`. It counts the samples that come
 * back non-finite instead of hiding them: `OffsetSurface` over a sphere, for
 * instance, reads its base normal from a central difference that degenerates at
 * the pole, which the library header says outright. The substituted point is the
 * origin and the count goes in the status line.
 */
class Sampler {
  nonFinite = 0;
  count = 0;
  constructor(private readonly surface: ParametricSurface) {}
  readonly eval = (u: number, v: number): Point3D => {
    this.count++;
    const p = this.surface.Eval(new UvCoordinate(u, v));
    if (!Number.isFinite(p.X) || !Number.isFinite(p.Y) || !Number.isFinite(p.Z)) {
      this.nonFinite++;
      return new Point3D(0, 0, 0);
    }
    return p;
  };
}

// ---------------------------------------------------------------------------
// Presentation

function smoothMaterial(color = palette.surface): THREE.MeshStandardMaterial {
  return new THREE.MeshStandardMaterial({
    color,
    roughness: 0.4,
    metalness: 0.06,
    flatShading: false,
    side: THREE.DoubleSide,
  });
}

function lineMaterial(color: number, opacity = 1): THREE.LineBasicMaterial {
  return new THREE.LineBasicMaterial({ color, transparent: opacity < 1, opacity });
}

function segments(coordinates: number[], color: number, opacity = 1): THREE.LineSegments {
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(coordinates, 3));
  return new THREE.LineSegments(geometry, lineMaterial(color, opacity));
}

function dots(points: readonly Point3D[], color: number, size = 6): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.X, p.Y, p.Z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(geometry, new THREE.PointsMaterial({ color, size, sizeAttenuation: false }));
}

function chain(points: readonly Point3D[], color: number): THREE.Line {
  return new THREE.Line(polylineGeometry(points), lineMaterial(color));
}

/**
 * Parameter lines repacked out of the lattice `parametricGeometry` just built.
 * Its vertex order is row-major over (u, v) — v outer, u inner, `uSteps + 1`
 * columns — so every isoline is already in the buffer and the wireframe costs no
 * further evaluations.
 */
function latticeWire(
  geometry: THREE.BufferGeometry,
  uSteps: number,
  vSteps: number,
  lines: number,
): THREE.BufferGeometry {
  const position = geometry.getAttribute('position') as THREE.BufferAttribute;
  const uCount = uSteps + 1;
  const at = (i: number, j: number): number[] => {
    const k = j * uCount + i;
    return [position.getX(k), position.getY(k), position.getZ(k)];
  };
  const out: number[] = [];
  const uStride = Math.max(1, Math.round(uSteps / lines));
  const vStride = Math.max(1, Math.round(vSteps / lines));
  for (let j = 0; j <= vSteps; j += vStride) {
    for (let i = 0; i < uSteps; i++) out.push(...at(i, j), ...at(i + 1, j));
  }
  for (let i = 0; i <= uSteps; i += uStride) {
    for (let j = 0; j < vSteps; j++) out.push(...at(i, j), ...at(i, j + 1));
  }
  const wire = new THREE.BufferGeometry();
  wire.setAttribute('position', new THREE.Float32BufferAttribute(out, 3));
  return wire;
}

/**
 * Centre and scale an object into the camera's reach. The catalog spans four
 * orders of magnitude — a unit sphere and a breather surface over a domain
 * thirty-seven parameter units wide — and every scene shares one camera.
 */
function framed(object: THREE.Object3D, radius = 1.5): THREE.Object3D {
  const box = new THREE.Box3().setFromObject(object);
  if (box.isEmpty()) return object;
  const centre = box.getCenter(new THREE.Vector3());
  const size = box.getSize(new THREE.Vector3());
  const extent = Math.max(size.x, size.y, size.z);
  if (!Number.isFinite(extent) || extent <= 0 || !Number.isFinite(centre.lengthSq())) return object;
  object.position.sub(centre);
  const group = new THREE.Group();
  group.scale.setScalar((2 * radius) / extent);
  group.add(object);
  return group;
}

/** A parametric surface as a shaded lattice plus its parameter lines. */
function surfaceObject(
  surface: ParametricSurface,
  steps: number,
  options: { wire?: boolean; color?: number; colors?: (u: number, v: number) => THREE.Color } = {},
): { object: THREE.Object3D; sampler: Sampler; geometry: THREE.BufferGeometry } {
  const sampler = new Sampler(surface);
  const geometry = parametricGeometry(
    sampler.eval,
    steps,
    steps,
    surface.ClosedU(),
    surface.ClosedV(),
  );
  const material = smoothMaterial(options.color);
  if (options.colors) {
    const values: number[] = [];
    for (let j = 0; j <= steps; j++) {
      for (let i = 0; i <= steps; i++) {
        const c = options.colors(i / steps, j / steps);
        values.push(c.r, c.g, c.b);
      }
    }
    geometry.setAttribute('color', new THREE.Float32BufferAttribute(values, 3));
    material.vertexColors = true;
  }
  const group = new THREE.Group();
  group.add(new THREE.Mesh(geometry, material));
  if (options.wire !== false) {
    group.add(new THREE.LineSegments(latticeWire(geometry, steps, steps, 12), lineMaterial(0x11202f, 0.65)));
  }
  return { object: group, sampler, geometry };
}

/** The readings every gallery scene shares. */
function surfaceReadings(
  entry: { label: string; member: string; fields: string },
  surface: ParametricSurface,
  sampler: Sampler,
): Reading[] {
  return [
    note('surface', `${entry.label} — ${entry.fields}`),
    reading('Eval(0.5, 0.5)', () => p3s(surface.Eval(new UvCoordinate(0.5, 0.5)))),
    reading('Eval(0, 0)', () => p3s(surface.Eval(new UvCoordinate(0, 0)))),
    reading('ClosedU / ClosedV', () => `${surface.ClosedU()} / ${surface.ClosedV()}`),
    note('lattice samples', `${sampler.count}`),
    note(
      'non-finite',
      sampler.nonFinite === 0 ? '0' : `${sampler.nonFinite} — ${entry.member} returned NaN there`,
    ),
    // The derived corner/centre samples of `surfaces.library.plato` are emitted
    // against `Tuple2`, whose fields are X0/X1 rather than U/V, so they are the
    // one family on this page that no surface can answer.
    reading('CenterPoint', () => {
      const p = (surface as { CenterPoint?: () => Point3D }).CenterPoint?.();
      if (!p) throw new Error('member not emitted');
      if (!Number.isFinite(p.X)) throw new Error('NaN — Eval called with Tuple2, not UvCoordinate');
      return p3s(p);
    }),
  ];
}

// ---------------------------------------------------------------------------
// The named surface catalog
//
// One entry per generated type. `make` turns the four shared shape sliders into
// that type's own fields; `fields` names them, so the status line always says
// what the sliders are actually driving.

interface Shape {
  size: number;
  section: number;
  shape: number;
  extent: number;
}

interface Entry {
  label: string;
  member: string;
  fields: string;
  make(p: Shape): ParametricSurface;
}

const bounds2 = (halfX: number, halfY: number): Bounds2D =>
  new Bounds2D(new Point2D(-halfX, -halfY), new Point2D(halfX, halfY));
const interval = (a: number, b: number): NumberInterval => new NumberInterval(a, b);
const angles = (a: number, b: number): AngleInterval => new AngleInterval(new Angle(a), new Angle(b));
const unitFrame = (): Frame3D =>
  new Frame3D(
    new Point3D(0, 0, 0),
    new Direction3D(new Vector3D(1, 0, 0)),
    new Direction3D(new Vector3D(0, 1, 0)),
    new Direction3D(new Vector3D(0, 0, 1)),
  );

const QUADRICS: Entry[] = [
  {
    label: 'Torus',
    member: 'Torus.Eval',
    fields: 'MajorRadius = size, MinorRadius = section',
    make: p => new Torus(new Point3D(0, 0, 0), new Direction3D(new Vector3D(0, 0, 1)), p.size, p.section),
  },
  {
    label: 'Sphere',
    member: 'Sphere.Eval',
    fields: 'Radius = size',
    make: p => new Sphere(new Point3D(0, 0, 0), p.size),
  },
  {
    label: 'Ellipsoid',
    member: 'Ellipsoid.Eval',
    fields: 'SemiAxes = (size, section, shape)',
    make: p =>
      new Ellipsoid(
        new Point3D(0, 0, 0),
        new Number3(p.size, p.section, p.shape * 0.6),
        new Quaternion(0, 0, 0, 1),
      ),
  },
  {
    label: 'Hyp. paraboloid',
    member: 'HyperbolicParaboloid.Eval',
    fields: 'SemiAxes = (size, section), Domain = ±extent',
    make: p =>
      new HyperbolicParaboloid(new Number2(p.size, p.section + 0.3), bounds2(p.extent, p.extent)),
  },
  {
    label: 'Hyperboloid 1',
    member: 'HyperboloidOfOneSheet.Eval',
    fields: 'SemiAxes = (size, size, section), ParameterRange = ±extent',
    make: p =>
      new HyperboloidOfOneSheet(
        new Number3(p.size * 0.6, p.size * 0.6, p.section + 0.3),
        interval(-p.extent, p.extent),
      ),
  },
  {
    label: 'Hyperboloid 2',
    member: 'HyperboloidOfTwoSheets.Eval',
    fields: 'SemiAxes = (size, size, section), ParameterRange = 0..extent',
    make: p =>
      new HyperboloidOfTwoSheets(
        new Number3(p.size * 0.6, p.size * 0.6, p.section + 0.3),
        interval(0, p.extent),
      ),
  },
  {
    label: 'Superellipsoid',
    member: 'Superellipsoid.Eval',
    fields: 'SemiAxes = (size, size, section), exponents = shape',
    make: p =>
      new Superellipsoid(
        new Number3(p.size, p.size, p.section + 0.4),
        p.shape * 0.6,
        p.shape * 0.6,
      ),
  },
  {
    label: 'Supertoroid',
    member: 'Supertoroid.Eval',
    fields: 'MajorRadius = size, MinorRadius = section, exponents = shape',
    make: p => new Supertoroid(p.size, p.section, p.shape * 0.6, p.shape * 0.6),
  },
  {
    label: 'Superformula',
    member: 'Superformula3D.Eval',
    fields: 'two Superformula2D factors: Symmetry from extent, exponents from shape',
    make: p =>
      new Superformula3D(
        new Superformula2D(Math.round(2 + p.extent * 4), 1, p.shape * 2, p.shape * 2, p.size, p.size),
        new Superformula2D(Math.round(2 + p.extent * 2), 1, p.shape * 2, p.shape * 2, p.size, p.size),
      ),
  },
  {
    label: 'Dupin cyclide',
    member: 'DupinCyclide.Eval',
    fields: 'MajorRadius = size + 0.6, MinorRadius = section, Offset = shape',
    make: p => new DupinCyclide(p.size + 0.6, p.section, p.shape * 0.3),
  },
  {
    label: 'Seashell',
    member: 'SeashellSurface.Eval',
    fields: 'TurnCount = extent, Height = size, TubeRadius = section',
    make: p => new SeashellSurface(1 + p.extent * 2, p.size * 1.6, p.section * 0.6, p.section * 0.25),
  },
  {
    label: 'Sph. harmonic',
    member: 'SphericalHarmonicSurface.Eval',
    fields: 'Frequencies from extent and shape, Exponents from size',
    make: p =>
      new SphericalHarmonicSurface(
        new Number4(
          Math.round(1 + p.extent * 3),
          Math.round(1 + p.shape * 2),
          Math.round(1 + p.extent * 2),
          Math.round(1 + p.shape * 3),
        ),
        new Number4(p.size, p.size + 1, p.size, p.size + 1),
      ),
  },
];

const REVOLUTION: Entry[] = [
  {
    label: 'Catenoid',
    member: 'Catenoid.Eval',
    fields: 'WaistRadius = size, ParameterRange = ±extent',
    make: p => new Catenoid(p.size * 0.6, interval(-p.extent, p.extent)),
  },
  {
    label: 'Helicoid',
    member: 'Helicoid.Eval',
    fields: 'Radii = 0.02..size, TurnCount = extent, Pitch = section',
    make: p => new Helicoid(interval(0.02, p.size), 0.5 + p.extent, p.section),
  },
  {
    label: 'Pseudosphere',
    member: 'Pseudosphere.Eval',
    fields: 'Radius = size, ParameterRange = 0..extent',
    make: p => new Pseudosphere(p.size, interval(0, p.extent * 2)),
  },
  {
    label: 'Dini',
    member: 'DiniSurface.Eval',
    fields: 'Radius = size, Twist = section, TurnCount = extent, Inclinations ⊂ (0, ½ turn)',
    make: p =>
      new DiniSurface(p.size, p.section * 0.5, 1 + p.extent, angles(0.08, Math.PI - 0.5 * p.shape)),
  },
  {
    label: 'Kuen',
    member: 'KuenSurface.Eval',
    fields: 'Scale = size, ParameterRange = ±extent, Inclinations ⊂ (0, ½ turn)',
    make: p =>
      new KuenSurface(p.size, interval(-p.extent * 4, p.extent * 4), angles(0.15, Math.PI - 0.4 * p.shape)),
  },
  {
    label: 'Breather',
    member: 'BreatherSurface.Eval',
    fields: 'Wavenumber = shape (strictly 0..1), Domain = ±extent',
    make: p => new BreatherSurface(clamp01(p.shape * 0.4), bounds2(p.extent * 13, p.extent * 37)),
  },
  {
    label: 'Enneper',
    member: 'EnneperSurface.Eval',
    fields: 'Scale = size, Domain = ±extent',
    make: p => new EnneperSurface(p.size * 0.5, bounds2(p.extent * 1.4, p.extent * 1.4)),
  },
  {
    label: 'Scherk',
    member: 'ScherkSurface.Eval',
    fields: 'Scale = size, Domain strictly inside the square where both cosines are positive',
    make: p => new ScherkSurface(p.size, bounds2(p.size * 1.4, p.size * 1.4)),
  },
  {
    label: 'Henneberg',
    member: 'HennebergSurface.Eval',
    fields: 'Scale = size, Domain = ±extent',
    make: p =>
      new HennebergSurface(
        p.size * 0.4,
        new Bounds2D(new Point2D(-p.extent * 0.8, 0.05), new Point2D(p.extent * 0.8, 1.5)),
      ),
  },
  {
    label: 'Catalan',
    member: 'CatalanSurface.Eval',
    fields: 'Scale = size, Domain = ±extent',
    make: p =>
      new CatalanSurface(
        p.size * 0.3,
        new Bounds2D(new Point2D(-p.extent * 6, -1), new Point2D(p.extent * 6, 1)),
      ),
  },
  {
    label: 'Monkey saddle',
    member: 'MonkeySaddle.Eval',
    fields: 'Scale = size, Domain = ±extent',
    make: p => new MonkeySaddle(p.size * 0.7, bounds2(p.extent, p.extent)),
  },
];

const ONE_SIDED: Entry[] = [
  {
    label: 'Klein bottle',
    member: 'KleinBottle.Eval',
    fields: 'Radius = size, TubeRadius = section',
    make: p => new KleinBottle(p.size, p.section),
  },
  {
    label: 'Mobius strip',
    member: 'MobiusStrip.Eval',
    fields: 'Radius = size, Width = section, HalfTwistCount = round(extent · 3)',
    make: p => new MobiusStrip(unitFrame(), p.size, p.section * 1.5, Math.max(1, Math.round(p.extent * 3))),
  },
  {
    label: 'Boy surface',
    member: 'BoySurface.Eval',
    fields: 'Scale = size',
    make: p => new BoySurface(p.size),
  },
  {
    label: 'Roman surface',
    member: 'RomanSurface.Eval',
    fields: 'Scale = size',
    make: p => new RomanSurface(p.size),
  },
  {
    label: 'Cross-cap',
    member: 'CrossCap.Eval',
    fields: 'Radius = size',
    make: p => new CrossCap(p.size),
  },
  {
    label: 'Whitney umbrella',
    member: 'WhitneyUmbrella.Eval',
    fields: 'Domain = ±extent',
    make: p => new WhitneyUmbrella(bounds2(p.extent, p.extent)),
  },
  {
    label: 'Bohemian dome',
    member: 'BohemianDome.Eval',
    fields: 'EllipseRadii = (size, section), CircleRadius = shape',
    make: p => new BohemianDome(new Number2(p.size, p.section + 0.3), p.shape * 0.5),
  },
  {
    label: 'Plucker conoid',
    member: 'PluckerConoid.Eval',
    fields: 'Radius = size, Height = section, FoldCount = round(extent · 3)',
    make: p => new PluckerConoid(p.size, p.section, Math.max(1, Math.round(p.extent * 3))),
  },
];

const SHAPE_CONTROLS: Control[] = [
  { key: 'size', label: 'Size', kind: 'slider', min: 0.2, max: 2, step: 0.01, def: 1 },
  { key: 'section', label: 'Section', kind: 'slider', min: 0.05, max: 1.2, step: 0.01, def: 0.4 },
  { key: 'shape', label: 'Shape', kind: 'slider', min: 0.1, max: 2, step: 0.01, def: 1 },
  { key: 'extent', label: 'Extent', kind: 'slider', min: 0.2, max: 2, step: 0.01, def: 1 },
  { key: 'res', label: 'Lattice', kind: 'slider', min: 8, max: 96, step: 4, def: 40 },
  { key: 'wire', label: 'Parameter lines', kind: 'toggle', def: 1 },
];

function shapeOf(params: Params): Shape {
  return {
    size: params.size,
    section: params.section,
    shape: params.shape,
    extent: params.extent,
  };
}

/** One gallery scene over a slice of the catalog. */
function gallery(spec: {
  id: string;
  title: string;
  description: string;
  entries: Entry[];
  plato: string[];
  def?: number;
}): Scene {
  return sceneOf({
    id: spec.id,
    title: spec.title,
    description: spec.description,
    plato: spec.plato,
    viewer: { orthographic: false, grid: false, spin: true, distance: 4.6 },
    controls: [
      {
        key: 'which',
        label: 'Surface',
        kind: 'select',
        options: spec.entries.map(e => e.label),
        def: spec.def ?? 0,
      },
      ...SHAPE_CONTROLS,
    ],
    build(params: Params): Built {
      const entry = spec.entries[Math.min(spec.entries.length - 1, Math.max(0, Math.round(params.which)))];
      const steps = Math.round(params.res);

      // A member that throws is a finding, not a crash: the scene keeps the
      // member's name and reports the failure, exactly as the patch scene does
      // for the two control-net patches whose bodies do not run.
      let surface: ParametricSurface | null = null;
      const constructed = reading(entry.member, () => {
        surface = entry.make(shapeOf(params));
        surface.Eval(new UvCoordinate(0.5, 0.5));
        return 'evaluates';
      });
      if (surface === null) {
        return {
          object: dots([new Point3D(0, 0, 0)], palette.line, 4),
          readings: [note('surface', `${entry.label} — ${entry.fields}`), constructed],
        };
      }
      const built = surfaceObject(surface, steps, { wire: params.wire !== 0 });
      return {
        object: framed(built.object),
        readings: surfaceReadings(entry, surface, built.sampler),
      };
    },
  });
}

const quadrics = gallery({
  id: 'quadrics',
  title: 'Quadrics, superquadrics and blends',
  description:
    'The closed-form named surfaces of surfaces-shapes.types.plato that are quadrics, surfaces of ' +
    'revolution, superquadrics or CAD blends: every one stores only its defining parameters, sits in ' +
    'canonical position, and answers Eval over the unit square. The lattice seams shut wherever ' +
    'ClosedU or ClosedV says it does — both, for the torus, the supertoroid and the Dupin cyclide.',
  entries: QUADRICS,
  plato: [
    'Torus.Eval',
    'Sphere.Eval',
    'Ellipsoid.Eval',
    'HyperbolicParaboloid.Eval',
    'HyperboloidOfOneSheet.Eval',
    'HyperboloidOfTwoSheets.Eval',
    'Superellipsoid.Eval',
    'Supertoroid.Eval',
    'Superformula3D.Eval',
    'DupinCyclide.Eval',
    'SeashellSurface.Eval',
    'SphericalHarmonicSurface.Eval',
    'IParametricSurface.ClosedU',
    'IParametricSurface.ClosedV',
  ],
});

const minimal = gallery({
  id: 'minimal',
  title: 'Minimal and pseudospherical',
  description:
    'The two families differential geometry names after their curvature: the minimal surfaces, whose ' +
    'mean curvature vanishes everywhere (catenoid, helicoid, Enneper, Scherk, Henneberg, Catalan), and ' +
    'the pseudospherical ones, whose Gaussian curvature is a negative constant (pseudosphere, Dini, ' +
    'Kuen, breather). Several carry preconditions on the domain they are cut from; the sliders stay ' +
    'inside them, and the curvature scene measures the claim.',
  entries: REVOLUTION,
  plato: [
    'Catenoid.Eval',
    'Helicoid.Eval',
    'Pseudosphere.Eval',
    'DiniSurface.Eval',
    'KuenSurface.Eval',
    'BreatherSurface.Eval',
    'EnneperSurface.Eval',
    'ScherkSurface.Eval',
    'HennebergSurface.Eval',
    'CatalanSurface.Eval',
    'MonkeySaddle.Eval',
  ],
  def: 3,
});

const oneSided = gallery({
  id: 'one-sided',
  title: 'Non-orientable and self-intersecting',
  description:
    'The classical immersions. Each type denotes an immersed image rather than an embedded shape — the ' +
    'self-intersections are part of the definition, not an artefact of the sampling, and none of these ' +
    'surfaces has a well-defined inside. The Klein bottle reports itself closed in both parameters, so ' +
    'the lattice seams shut twice; the Mobius strip closes in U only, and an odd HalfTwistCount is what ' +
    'makes the return trip land flipped.',
  entries: ONE_SIDED,
  plato: [
    'KleinBottle.Eval',
    'MobiusStrip.Eval',
    'BoySurface.Eval',
    'RomanSurface.Eval',
    'CrossCap.Eval',
    'WhitneyUmbrella.Eval',
    'BohemianDome.Eval',
    'PluckerConoid.Eval',
  ],
});

// ---------------------------------------------------------------------------
// Patches and control nets

/**
 * An `IArray2D<Point3D>` over a generated grid — the control net `BezierPatch`
 * and `BSplineSurface` take. `Row` and `Count` are not on the emitted
 * `IArray2D` interface but the generated bodies call them, so they are here too.
 */
class ControlNet {
  constructor(
    readonly columns: number,
    readonly rows: number,
    private readonly at: (column: number, row: number) => Point3D,
  ) {}
  At(column: number, row: number): Point3D {
    return this.at(column, row);
  }
  ColumnCount(): number {
    return this.columns;
  }
  RowCount(): number {
    return this.rows;
  }
  Count(): number {
    return this.columns * this.rows;
  }
  Row(row: number): IArray<Point3D> {
    return fromArray(Array.from({ length: this.columns }, (_, c) => this.at(c, row)));
  }
  Map(mapping: (p: Point3D) => Point3D): ControlNet {
    return new ControlNet(this.columns, this.rows, (c, r) => mapping(this.at(c, r)));
  }
  points(): Point3D[] {
    const out: Point3D[] = [];
    for (let r = 0; r < this.rows; r++) for (let c = 0; c < this.columns; c++) out.push(this.at(c, r));
    return out;
  }
  /** The net drawn as a grid of segments. */
  object(color: number): THREE.Object3D {
    const out: number[] = [];
    const push = (a: Point3D, b: Point3D): void => {
      out.push(a.X, a.Y, a.Z, b.X, b.Y, b.Z);
    };
    for (let r = 0; r < this.rows; r++)
      for (let c = 0; c + 1 < this.columns; c++) push(this.at(c, r), this.at(c + 1, r));
    for (let c = 0; c < this.columns; c++)
      for (let r = 0; r + 1 < this.rows; r++) push(this.at(c, r), this.at(c, r + 1));
    const group = new THREE.Group();
    group.add(segments(out, color, 0.85), dots(this.points(), color, 7));
    return group;
  }
}

function bumpyNet(columns: number, rows: number, height: number, spread: number): ControlNet {
  return new ControlNet(columns, rows, (c, r) => {
    const x = ((c / (columns - 1)) * 2 - 1) * spread;
    const y = ((r / (rows - 1)) * 2 - 1) * spread;
    return new Point3D(x, y, height * Math.sin(c * 1.7) * Math.cos(r * 1.3));
  });
}

const PATCH_LABELS = ['Bilinear', 'Coons', 'Planar', 'Spherical', 'Bezier', 'B-spline'];

const patches = sceneOf({
  id: 'patches',
  title: 'Patches and control nets',
  description:
    'The rectangular surface pieces of surfaces.types.plato, each drawn beside the data that defines ' +
    'it: BilinearPatch over its four corners, CoonsPatch over the four boundary curves it blends ' +
    '(a Coons patch is the two ruled surfaces between opposite pairs minus the bilinear patch both ' +
    'contain), PlanarPatch over a Frame3D and a Bounds2D, and SphericalPatch as a latitude/longitude ' +
    'window. BezierPatch and BSplineSurface are here with their control nets, and their Eval throws — ' +
    'both bodies read the net\'s RowCount / ColumnCount as Plato properties and the TypeScript writer ' +
    'emits them without call parentheses.',
  plato: [
    'BilinearPatch.Eval',
    'CoonsPatch.Eval',
    'PlanarPatch.Eval',
    'SphericalPatch.Eval',
    'BezierPatch.Eval',
    'BSplineSurface.Eval',
    'CubicBezier3D.Eval',
    'IParametricSurface.ClosedU',
  ],
  viewer: { orthographic: false, grid: false, spin: true, distance: 4.6 },
  controls: [
    { key: 'which', label: 'Patch', kind: 'select', options: PATCH_LABELS, def: 0 },
    { key: 'lift', label: 'Corner lift', kind: 'slider', min: -1.2, max: 1.2, step: 0.01, def: 0.6 },
    { key: 'bow', label: 'Boundary bow', kind: 'slider', min: -1.5, max: 1.5, step: 0.01, def: 0.8 },
    { key: 'res', label: 'Lattice', kind: 'slider', min: 4, max: 64, step: 2, def: 32 },
    { key: 'net', label: 'Show net', kind: 'toggle', def: 1 },
    { key: 'wire', label: 'Parameter lines', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const which = Math.min(PATCH_LABELS.length - 1, Math.max(0, Math.round(params.which)));
    const steps = Math.round(params.res);
    const lift = params.lift;
    const bow = params.bow;
    const showNet = params.net !== 0;

    const group = new THREE.Group();
    const extra: Reading[] = [];
    let member = '';
    let surface: ParametricSurface | null = null;

    if (which === 0) {
      const patch = new BilinearPatch(
        new Point3D(-1, -1, 0),
        new Point3D(1, -1, lift),
        new Point3D(-1, 1, lift),
        new Point3D(1, 1, -lift),
      );
      member = 'BilinearPatch.Eval';
      surface = patch;
      if (showNet) {
        const net = new ControlNet(2, 2, (c, r) =>
          r === 0 ? (c === 0 ? patch.P00 : patch.P10) : c === 0 ? patch.P01 : patch.P11,
        );
        group.add(net.object(palette.surfaceAlt));
      }
      extra.push(note('corners', `${p3s(patch.P00)} ${p3s(patch.P11)}`));
    } else if (which === 1) {
      const bottom = new CubicBezier3D(
        new Point3D(-1, -1, 0),
        new Point3D(-0.34, -1, bow),
        new Point3D(0.34, -1, -bow),
        new Point3D(1, -1, 0),
      );
      const top = new CubicBezier3D(
        new Point3D(-1, 1, 0),
        new Point3D(-0.34, 1, -bow),
        new Point3D(0.34, 1, bow),
        new Point3D(1, 1, 0),
      );
      const left = new CubicBezier3D(
        new Point3D(-1, -1, 0),
        new Point3D(-1 - lift, -0.34, bow * 0.5),
        new Point3D(-1 - lift, 0.34, bow * 0.5),
        new Point3D(-1, 1, 0),
      );
      const right = new CubicBezier3D(
        new Point3D(1, -1, 0),
        new Point3D(1 + lift, -0.34, -bow * 0.5),
        new Point3D(1 + lift, 0.34, -bow * 0.5),
        new Point3D(1, 1, 0),
      );
      member = 'CoonsPatch.Eval';
      surface = new CoonsPatch(
        asGenerated(bottom),
        asGenerated(right),
        asGenerated(top),
        asGenerated(left),
      );
      if (showNet) {
        for (const curve of [bottom, right, top, left]) {
          const points = Array.from({ length: 33 }, (_, i) => curve.Eval(i / 32));
          group.add(chain(points, palette.surfaceAlt));
        }
      }
      extra.push(reading('Bottom.Eval(0.5)', () => p3s(bottom.Eval(0.5))));
    } else if (which === 2) {
      member = 'PlanarPatch.Eval';
      surface = new PlanarPatch(unitFrame(), bounds2(1 + lift * 0.5, 1));
      extra.push(note('Frame3D', 'origin at 0, axes = the canonical basis'));
    } else if (which === 3) {
      member = 'SphericalPatch.Eval';
      surface = new SphericalPatch(
        new Sphere(new Point3D(0, 0, 0), 1),
        angles(0, TAU * (0.35 + 0.3 * Math.abs(bow))),
        angles(0.15, Math.PI - 0.15),
      );
      extra.push(note('window', 'Azimuths from bow, Inclinations just inside the poles'));
    } else if (which === 4) {
      const net = bumpyNet(4, 4, lift * 0.9, 1);
      member = 'BezierPatch.Eval';
      surface = new BezierPatch(asGenerated<IArray2D<Point3D>>(net));
      if (showNet) group.add(net.object(palette.surfaceAlt));
      extra.push(note('ControlPoints', `${net.columns} x ${net.rows}`));
    } else {
      const net = bumpyNet(4, 4, lift * 0.9, 1);
      const knots = new KnotVector(fromArray([0, 0, 0, 0, 1, 1, 1, 1]));
      member = 'BSplineSurface.Eval';
      surface = new BSplineSurface(asGenerated<IArray2D<Point3D>>(net), 3, 3, knots, knots);
      if (showNet) group.add(net.object(palette.surfaceAlt));
      extra.push(note('ControlPoints', `${net.columns} x ${net.rows}, degree 3 x 3`));
    }

    const readings: Reading[] = [note('patch', PATCH_LABELS[which])];
    const evaluated = reading(member, () => {
      const built = surfaceObject(surface as ParametricSurface, steps, { wire: params.wire !== 0 });
      group.add(built.object);
      return `${built.sampler.count} samples, ${built.sampler.nonFinite} non-finite`;
    });
    readings.push(evaluated);
    readings.push(
      reading('Eval(0.5, 0.5)', () => p3s((surface as ParametricSurface).Eval(new UvCoordinate(0.5, 0.5)))),
      reading(
        'ClosedU / ClosedV',
        () => `${(surface as ParametricSurface).ClosedU()} / ${(surface as ParametricSurface).ClosedV()}`,
      ),
      ...extra,
    );

    // A patch whose Eval throws still has its defining data to show, so the net
    // alone is a legitimate scene — but never an empty one.
    if (group.children.length === 0) group.add(dots([new Point3D(0, 0, 0)], palette.line, 4));
    return { object: framed(group), readings };
  },
});

// ---------------------------------------------------------------------------
// Generated surfaces: revolve, extrude, rule, sweep, tube, loft, offset

const GENERATED_LABELS = ['Revolve', 'Extrude', 'Rule', 'Sweep', 'Tube', 'Loft', 'Offset'];

function profileCurve(bow: number): CubicBezier3D {
  return new CubicBezier3D(
    new Point3D(-1, -1, 0),
    new Point3D(-0.34, -1, bow),
    new Point3D(0.34, -1, -bow),
    new Point3D(1, -1, 0),
  );
}

const generated = sceneOf({
  id: 'generated',
  title: 'Generated surfaces',
  description:
    'The surfaces defined by carrying a generator through space, from surfaces.library.plato: a ' +
    'CircularArc2D revolved about an axis, a CubicBezier3D extruded and ruled, a circle swept along a ' +
    'Helix, a TubeSurface around a TrefoilKnot, a LoftedSurface across three Polyline3D sections, and ' +
    'an OffsetSurface displaced along its base normal. The generator is drawn in orange. Two of these ' +
    'carry the approximations the library states at their bodies: the sweeps use a fixed-up frame ' +
    'rather than a rotation-minimizing one, and OffsetSurface reads its base normal from a central ' +
    'difference of Base.Eval, which degenerates at a pole — the non-finite count reports it.',
  plato: [
    'SurfaceOfRevolution.Eval',
    'ExtrudedSurface.Eval',
    'RuledSurface.Eval',
    'SweptSurface.Eval',
    'TubeSurface.Eval',
    'LoftedSurface.Eval',
    'OffsetSurface.Eval',
    'CircularArc2D.Eval',
    'CubicBezier3D.Eval',
    'Helix.Eval',
    'TrefoilKnot.Eval',
    'SurfaceOfRevolution.ClosedU',
  ],
  viewer: { orthographic: false, grid: false, spin: true, distance: 4.6 },
  controls: [
    { key: 'which', label: 'Generator', kind: 'select', options: GENERATED_LABELS, def: 0 },
    { key: 'size', label: 'Size', kind: 'slider', min: 0.2, max: 1.6, step: 0.01, def: 0.8 },
    { key: 'bow', label: 'Bow / sweep', kind: 'slider', min: -1.5, max: 1.5, step: 0.01, def: 0.9 },
    { key: 'turn', label: 'Turns / offset', kind: 'slider', min: 0.15, max: 2.5, step: 0.01, def: 1 },
    { key: 'res', label: 'Lattice', kind: 'slider', min: 8, max: 80, step: 4, def: 40 },
    { key: 'gen', label: 'Show generator', kind: 'toggle', def: 1 },
    { key: 'wire', label: 'Parameter lines', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const which = Math.min(GENERATED_LABELS.length - 1, Math.max(0, Math.round(params.which)));
    const steps = Math.round(params.res);
    const showGenerator = params.gen !== 0;
    const group = new THREE.Group();
    const extra: Reading[] = [];
    let member: string;
    let surface: ParametricSurface;

    const drawCurve = (curve: { Eval(t: number): Point3D }): void => {
      if (!showGenerator) return;
      group.add(chain(Array.from({ length: 129 }, (_, i) => curve.Eval(i / 128)), palette.surfaceAlt));
    };

    if (which === 0) {
      const arc = new CircularArc2D(new Point2D(1.1, 0), params.size * 0.5, angles(0, TAU));
      member = 'SurfaceOfRevolution.Eval';
      surface = new SurfaceOfRevolution(
        asGenerated(arc),
        new InfiniteLine3D(new Point3D(0, 0, 0), new Direction3D(new Vector3D(0, 0, 1))),
        angles(0, TAU * clamp01(params.turn / 2.5)),
      );
      if (showGenerator) {
        const profile = Array.from({ length: 129 }, (_, i) => {
          const p = arc.Eval(i / 128);
          return new Point3D(p.X, 0, p.Y);
        });
        group.add(chain(profile, palette.surfaceAlt));
      }
      extra.push(
        note('Profile', 'CircularArc2D: X is the radial distance from the axis, Y runs along it'),
        reading('ClosedU (whole turn?)', () => String(surface.ClosedU())),
      );
    } else if (which === 1) {
      const curve = profileCurve(params.bow);
      member = 'ExtrudedSurface.Eval';
      surface = new ExtrudedSurface(
        asGenerated(curve),
        new Direction3D(new Vector3D(0, 1, params.turn * 0.4)),
        2 * params.size + 0.6,
      );
      drawCurve(curve);
      extra.push(note('Distance', n2(2 * params.size + 0.6)));
    } else if (which === 2) {
      const bottom = profileCurve(params.bow);
      const top = new CubicBezier3D(
        new Point3D(-1, 1, params.size),
        new Point3D(-0.34, 1, params.size - params.bow),
        new Point3D(0.34, 1, params.size + params.bow),
        new Point3D(1, 1, params.size),
      );
      member = 'RuledSurface.Eval';
      surface = new RuledSurface(asGenerated(bottom), asGenerated(top));
      drawCurve(bottom);
      drawCurve(top);
      extra.push(note('rulings', 'straight segments joining Start and End at equal parameter'));
    } else if (which === 3) {
      const section = new CircularArc2D(new Point2D(0, 0), params.size * 0.4, angles(0, TAU));
      const path = new Helix(unitFrame(), 0.9, params.bow * 0.15, angles(0, TAU * params.turn * 1.6));
      member = 'SweptSurface.Eval';
      surface = new SweptSurface(asGenerated(section), asGenerated(path));
      drawCurve(path);
      extra.push(note('frame', 'fixed-up, not rotation-minimizing — stated at the body'));
    } else if (which === 4) {
      const path = new TrefoilKnot(unitFrame(), params.size * 0.7);
      member = 'TubeSurface.Eval';
      surface = new TubeSurface(asGenerated(path), 0.08 + Math.abs(params.bow) * 0.12);
      drawCurve(path);
      extra.push(reading('ClosedU', () => String(surface.ClosedU())));
    } else if (which === 5) {
      const section = (y: number, scale: number, lift: number): Polyline3D =>
        new Polyline3D(
          fromArray(
            Array.from({ length: 8 }, (_, i) => {
              const a = (i / 8) * TAU;
              return new Point3D(Math.cos(a) * scale, y, Math.sin(a) * scale + lift);
            }),
          ),
          true,
        );
      member = 'LoftedSurface.Eval';
      surface = new LoftedSurface(
        fromArray([
          section(-1.2, params.size, 0),
          section(0, params.size * (0.4 + Math.abs(params.bow)), params.turn * 0.3),
          section(1.2, params.size * 0.7, 0),
        ]),
        false,
      );
      extra.push(note('Sections', '3 closed Polyline3D of 8 points; U walks a section by index'));
    } else {
      const base = new Torus(
        new Point3D(0, 0, 0),
        new Direction3D(new Vector3D(0, 0, 1)),
        1,
        params.size * 0.35,
      );
      member = 'OffsetSurface.Eval';
      surface = new OffsetSurface(asGenerated(base), (params.turn - 1) * 0.3);
      extra.push(
        note('Base', 'Torus'),
        reading('inherited closure', () => `${surface.ClosedU()} / ${surface.ClosedV()}`),
      );
    }

    const built = surfaceObject(surface, steps, { wire: params.wire !== 0 });
    group.add(built.object);

    return {
      object: framed(group),
      readings: [
        note('surface', GENERATED_LABELS[which]),
        reading(member, () => p3s(surface.Eval(new UvCoordinate(0.5, 0.5)))),
        reading('ClosedU / ClosedV', () => `${surface.ClosedU()} / ${surface.ClosedV()}`),
        note('lattice samples', String(built.sampler.count)),
        note(
          'non-finite',
          built.sampler.nonFinite === 0
            ? '0'
            : `${built.sampler.nonFinite} — ${member} returned NaN there`,
        ),
        ...extra,
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Solids: the same Eval, plus the measures the type carries

interface SolidEntry {
  label: string;
  member: string;
  make(size: number, section: number): ParametricSurface & {
    Volume(): number;
    SurfaceArea(): number;
    Centroid(): Point3D;
    Bounds(): { Min: Point3D; Max: Point3D };
    Contains(p: Point3D): boolean;
    IsTube(): boolean;
    IsFullyClosed(): boolean;
    IsOpenPatch(): boolean;
  };
}

const SOLIDS: SolidEntry[] = [
  {
    label: 'Cylinder',
    member: 'Cylinder',
    make: (size, section) =>
      new Cylinder(new Point3D(0, 0, 0), new Direction3D(new Vector3D(0, 0, 1)), section + 0.2, size * 2),
  },
  {
    label: 'Cone',
    member: 'Cone',
    make: (size, section) =>
      new Cone(new Point3D(0, 0, size), new Direction3D(new Vector3D(0, 0, -1)), size * 2, section + 0.2),
  },
  {
    label: 'Frustum',
    member: 'ConicalFrustum',
    make: (size, section) =>
      new ConicalFrustum(
        new Point3D(0, 0, 0),
        new Direction3D(new Vector3D(0, 0, 1)),
        size * 2,
        section + 0.5,
        section * 0.4 + 0.1,
      ),
  },
  { label: 'Sphere', member: 'Sphere', make: size => new Sphere(new Point3D(0, 0, 0), size) },
  {
    label: 'Ellipsoid',
    member: 'Ellipsoid',
    make: (size, section) =>
      new Ellipsoid(
        new Point3D(0, 0, 0),
        new Number3(size, section + 0.4, size * 0.6),
        new Quaternion(0, 0, 0, 1),
      ),
  },
  {
    label: 'Torus',
    member: 'Torus',
    make: (size, section) =>
      new Torus(new Point3D(0, 0, 0), new Direction3D(new Vector3D(0, 0, 1)), size, section),
  },
];

const solids = sceneOf({
  id: 'solids',
  title: 'Closure and measures',
  description:
    'The solids of solids.types.plato carry a parametric Eval as well as their closed-form measures, so ' +
    'the same value answers both questions. The lattice is the surface Eval draws; Volume, SurfaceArea, ' +
    'Centroid and Bounds beside it are the type\'s own closed forms, and Contains is sampled against a ' +
    'probe on the Z axis. IsFullyClosed, IsTube and IsOpenPatch classify the seam pair — a torus is ' +
    'closed in both parameters, a cylinder in one, so the torus alone reads fully closed.',
  plato: [
    'Cylinder.Eval',
    'Cone.Eval',
    'ConicalFrustum.Eval',
    'Sphere.Eval',
    'Ellipsoid.Eval',
    'Torus.Eval',
    'ISolid.Volume',
    'ISolid.SurfaceArea',
    'IGeometry3D.Bounds',
    'IGeometry3D.Centroid',
    'ISolid.Contains',
    'IParametricSurface.IsFullyClosed',
    'IParametricSurface.IsTube',
    'IParametricSurface.IsOpenPatch',
  ],
  viewer: { orthographic: false, grid: false, spin: true, distance: 4.6 },
  controls: [
    { key: 'which', label: 'Solid', kind: 'select', options: SOLIDS.map(s => s.label), def: 0 },
    { key: 'size', label: 'Size', kind: 'slider', min: 0.3, max: 1.6, step: 0.01, def: 0.9 },
    { key: 'section', label: 'Section', kind: 'slider', min: 0.05, max: 1.2, step: 0.01, def: 0.4 },
    { key: 'probe', label: 'Probe Z', kind: 'slider', min: -2, max: 2, step: 0.01, def: 0 },
    { key: 'res', label: 'Lattice', kind: 'slider', min: 8, max: 80, step: 4, def: 40 },
    { key: 'wire', label: 'Parameter lines', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const entry = SOLIDS[Math.min(SOLIDS.length - 1, Math.max(0, Math.round(params.which)))];
    const solid = entry.make(params.size, params.section);
    const steps = Math.round(params.res);
    const built = surfaceObject(solid, steps, { wire: params.wire !== 0 });
    const probe = new Point3D(0, 0, params.probe);

    const group = new THREE.Group();
    group.add(built.object);
    group.add(dots([probe], solid.Contains(probe) ? palette.accent : palette.surfaceAlt, 10));

    const bounds = solid.Bounds();
    return {
      object: framed(group),
      readings: [
        note('solid', entry.label),
        reading(`${entry.member}.Eval(0.5, 0.5)`, () => p3s(solid.Eval(new UvCoordinate(0.5, 0.5)))),
        reading('SurfaceArea', () => n4(solid.SurfaceArea())),
        reading('Volume', () => n4(solid.Volume())),
        reading('Centroid', () => p3s(solid.Centroid())),
        reading('Bounds', () => `${p3s(bounds.Min)}..${p3s(bounds.Max)}`),
        reading(`Contains(0, 0, ${n2(params.probe)})`, () => String(solid.Contains(probe))),
        reading('ClosedU / ClosedV', () => `${solid.ClosedU()} / ${solid.ClosedV()}`),
        reading(
          'IsFullyClosed / IsTube / IsOpenPatch',
          () => `${solid.IsFullyClosed()} / ${solid.IsTube()} / ${solid.IsOpenPatch()}`,
        ),
        note('non-finite', String(built.sampler.nonFinite)),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Differential geometry on a surface
//
// This is the one scene whose subject the emitted library cannot answer, so it
// says so at every reading rather than pretending otherwise.
//
// `differential-geometry.types.plato` declares SurfacePoint3D, TangentSpace3D,
// FirstFundamentalForm, SecondFundamentalForm, PrincipalCurvatures and
// SurfaceCurvature, and the emitted classes are exactly those records — no
// library anywhere in stdlib produces one from a surface. The derived family
// that would have (`Surfaces.FirstFundamentalE/F/G`, `NormalAtCenter`,
// `TangentUAtCenter`, `TangentVAtCenter`, `IsOrthogonalAt`) takes an
// `IDifferentiableSurface` first parameter, and no concrete type implements that
// interface, so the writer monomorphizes them onto nothing at all.
//
// What this scene does instead, and what it does not:
//
//   * Every position is `Eval`. The partial derivatives are central differences
//     of `Eval` on the same stencil `Surfaces.ParameterStep` uses, which is
//     precisely what `OffsetSurface.Eval` does in the stdlib for the same
//     reason: `IParametricSurface` exposes no derivative.
//   * E, F, G and L, M, N are then the dot products the library's own
//     `FirstFundamentalE/F/G` bodies spell out, taken with the generated
//     `Vector3D.Dot`, `Cross`, `MagnitudeSquared` and `Normalize`, and packed
//     into the generated `FirstFundamentalForm` / `SecondFundamentalForm`
//     records.
//   * K = (LN - M²)/(EG - F²) and H = (EN - 2FM + GL)/(2(EG - F²)) are the only
//     arithmetic here that the stdlib does not spell out somewhere, and they are
//     labelled as such. `PrincipalCurvatures` and `SurfaceCurvature` are left
//     unbuilt: their direction fields would have to be invented.

/** Matches `Surfaces.ParameterStep` in surfaces.library.plato. */
const STEP = 0.001;

interface Jet {
  position: Point3D;
  du: Vector3D;
  dv: Vector3D;
  duu: Vector3D;
  duv: Vector3D;
  dvv: Vector3D;
  normal: Vector3D;
}

/**
 * The second-order jet of a surface at (u, v), by central differences of the
 * generated `Eval`. The stencil is shifted inward at the boundary so that it
 * stays centred rather than collapsing onto a one-sided difference.
 */
function jetAt(surface: ParametricSurface, u: number, v: number): Jet {
  const cu = Math.min(1 - STEP, Math.max(STEP, u));
  const cv = Math.min(1 - STEP, Math.max(STEP, v));
  const at = (a: number, b: number): Point3D => surface.Eval(new UvCoordinate(a, b));
  const p = at(cu, cv);
  const pu = at(cu + STEP, cv);
  const mu = at(cu - STEP, cv);
  const pv = at(cu, cv + STEP);
  const mv = at(cu, cv - STEP);
  const pp = at(cu + STEP, cv + STEP);
  const pm = at(cu + STEP, cv - STEP);
  const mp = at(cu - STEP, cv + STEP);
  const mm = at(cu - STEP, cv - STEP);

  const du = mu.Between(pu).Divide(2 * STEP);
  const dv = mv.Between(pv).Divide(2 * STEP);
  const centre = p.PositionVector().Multiply(2);
  const duu = pu.PositionVector().Add(mu.PositionVector()).Subtract(centre).Divide(STEP * STEP);
  const dvv = pv.PositionVector().Add(mv.PositionVector()).Subtract(centre).Divide(STEP * STEP);
  const duv = pp
    .PositionVector()
    .Subtract(pm.PositionVector())
    .Subtract(mp.PositionVector())
    .Add(mm.PositionVector())
    .Divide(4 * STEP * STEP);
  return { position: p, du, dv, duu, duv, dvv, normal: du.Cross(dv).Normalize() };
}

function firstForm(jet: Jet): FirstFundamentalForm {
  return new FirstFundamentalForm(jet.du.MagnitudeSquared(), jet.du.Dot(jet.dv), jet.dv.MagnitudeSquared());
}

function secondForm(jet: Jet): SecondFundamentalForm {
  return new SecondFundamentalForm(
    jet.duu.Dot(jet.normal),
    jet.duv.Dot(jet.normal),
    jet.dvv.Dot(jet.normal),
  );
}

/** Gaussian and mean curvature from the two forms — see the block comment. */
function curvatures(first: FirstFundamentalForm, second: SecondFundamentalForm): {
  gaussian: number;
  mean: number;
} {
  const denominator = first.E * first.G - first.F * first.F;
  if (!Number.isFinite(denominator) || Math.abs(denominator) < 1e-12) {
    return { gaussian: NaN, mean: NaN };
  }
  return {
    gaussian: (second.L * second.N - second.M * second.M) / denominator,
    mean:
      (first.E * second.N - 2 * first.F * second.M + first.G * second.L) / (2 * denominator),
  };
}

const CURVATURE_SUBJECTS: { label: string; member: string; make(p: Shape): ParametricSurface; expect: string }[] = [
  {
    label: 'Torus',
    member: 'Torus.Eval',
    expect: 'K changes sign: positive outside the ring, negative on the inner wall',
    make: p => new Torus(new Point3D(0, 0, 0), new Direction3D(new Vector3D(0, 0, 1)), p.size, p.section),
  },
  {
    label: 'Sphere',
    member: 'Sphere.Eval',
    expect: 'K = 1 / Radius² everywhere, |H| = 1 / Radius (sign follows Xu × Xv)',
    make: p => new Sphere(new Point3D(0, 0, 0), p.size),
  },
  {
    label: 'Catenoid',
    member: 'Catenoid.Eval',
    expect: 'minimal: H = 0 everywhere',
    make: p => new Catenoid(p.size * 0.6, interval(-p.extent, p.extent)),
  },
  {
    label: 'Helicoid',
    member: 'Helicoid.Eval',
    expect: 'minimal: H = 0 everywhere',
    make: p => new Helicoid(interval(0.02, p.size), 0.5 + p.extent, p.section),
  },
  {
    label: 'Enneper',
    member: 'EnneperSurface.Eval',
    expect: 'minimal: H = 0 everywhere',
    make: p => new EnneperSurface(p.size * 0.5, bounds2(p.extent * 1.4, p.extent * 1.4)),
  },
  {
    label: 'Pseudosphere',
    member: 'Pseudosphere.Eval',
    expect: 'constant K = -1 / Radius²',
    make: p => new Pseudosphere(p.size, interval(0.05, p.extent * 2)),
  },
  {
    label: 'Dini',
    member: 'DiniSurface.Eval',
    expect: 'constant negative K while it twists',
    make: p => new DiniSurface(p.size, p.section * 0.5, 1 + p.extent, angles(0.08, Math.PI - 0.5 * p.shape)),
  },
  {
    label: 'Monkey saddle',
    member: 'MonkeySaddle.Eval',
    expect: 'K < 0 away from the origin; the origin is a degenerate critical point',
    make: p => new MonkeySaddle(p.size * 0.7, bounds2(p.extent, p.extent)),
  },
  {
    label: 'Hyp. paraboloid',
    member: 'HyperbolicParaboloid.Eval',
    expect: 'K < 0 everywhere — the doubly ruled saddle',
    make: p => new HyperbolicParaboloid(new Number2(p.size, p.section + 0.3), bounds2(p.extent, p.extent)),
  },
];

const NEGATIVE = new THREE.Color(palette.surface);
const NEUTRAL = new THREE.Color(0x2a3444);
const POSITIVE = new THREE.Color(palette.surfaceAlt);

function diverging(t: number): THREE.Color {
  if (!Number.isFinite(t)) return new THREE.Color(0x6b7280);
  const clamped = Math.max(-1, Math.min(1, t));
  const target = clamped < 0 ? NEGATIVE : POSITIVE;
  return NEUTRAL.clone().lerp(target, Math.abs(clamped));
}

function arrow(origin: Point3D, direction: Vector3D, length: number, color: number): THREE.Object3D {
  const v = new THREE.Vector3(direction.X, direction.Y, direction.Z);
  if (v.lengthSq() === 0 || !Number.isFinite(v.lengthSq())) return new THREE.Group();
  return new THREE.ArrowHelper(
    v.normalize(),
    new THREE.Vector3(origin.X, origin.Y, origin.Z),
    length,
    color,
    length * 0.26,
    length * 0.16,
  );
}

const differential = sceneOf({
  id: 'differential',
  title: 'Differential geometry on a surface',
  description:
    'The lattice coloured by Gaussian or mean curvature, with the tangent frame drawn at a probe. ' +
    'differential-geometry.types.plato declares SurfacePoint3D, TangentSpace3D, FirstFundamentalForm, ' +
    'SecondFundamentalForm, PrincipalCurvatures and SurfaceCurvature, but no library in stdlib produces ' +
    'one from a surface, and the derived FirstFundamentalE/F/G family takes an IDifferentiableSurface ' +
    'that no concrete type implements — so those members are emitted on nothing, and the status line ' +
    'says so. What is measured here comes from central differences of the generated Eval on the same ' +
    'ParameterStep the stdlib uses for OffsetSurface, packed into the generated form records with ' +
    'Vector3D.Dot, Cross and MagnitudeSquared. The claims are checkable: the catenoid, helicoid and ' +
    'Enneper surface should read mean curvature zero, and the pseudosphere constant negative Gaussian.',
  plato: [
    'Torus.Eval',
    'Sphere.Eval',
    'Catenoid.Eval',
    'Helicoid.Eval',
    'EnneperSurface.Eval',
    'Pseudosphere.Eval',
    'DiniSurface.Eval',
    'MonkeySaddle.Eval',
    'HyperbolicParaboloid.Eval',
    'Point3D.Between',
    'Point3D.PositionVector',
    'Vector3D.Dot',
    'Vector3D.Cross',
    'Vector3D.MagnitudeSquared',
    'Vector3D.Normalize',
    'FirstFundamentalForm',
    'SecondFundamentalForm',
    'TangentSpace3D',
    'SurfacePoint3D',
  ],
  viewer: { orthographic: false, grid: false, spin: true, distance: 4.6 },
  controls: [
    {
      key: 'which',
      label: 'Surface',
      kind: 'select',
      options: CURVATURE_SUBJECTS.map(s => s.label),
      def: 0,
    },
    { key: 'field', label: 'Colour by', kind: 'select', options: ['Gaussian K', 'Mean H'], def: 0 },
    { key: 'size', label: 'Size', kind: 'slider', min: 0.3, max: 2, step: 0.01, def: 1 },
    { key: 'section', label: 'Section', kind: 'slider', min: 0.08, max: 1.2, step: 0.01, def: 0.4 },
    { key: 'shape', label: 'Shape', kind: 'slider', min: 0.1, max: 2, step: 0.01, def: 1 },
    { key: 'extent', label: 'Extent', kind: 'slider', min: 0.2, max: 2, step: 0.01, def: 1 },
    { key: 'gain', label: 'Colour gain', kind: 'slider', min: 0.05, max: 6, step: 0.05, def: 1 },
    { key: 'probeU', label: 'Probe u', kind: 'slider', min: 0, max: 1, step: 0.005, def: 0.3 },
    { key: 'probeV', label: 'Probe v', kind: 'slider', min: 0, max: 1, step: 0.005, def: 0.6 },
    { key: 'res', label: 'Lattice', kind: 'slider', min: 8, max: 48, step: 2, def: 30 },
  ],
  build(params: Params): Built {
    const subject =
      CURVATURE_SUBJECTS[Math.min(CURVATURE_SUBJECTS.length - 1, Math.max(0, Math.round(params.which)))];
    const surface = subject.make(shapeOf(params));
    const steps = Math.round(params.res);
    const useGaussian = Math.round(params.field) === 0;
    const gain = params.gain;

    // One jet per lattice node, cached so the colour walk and the reported
    // extremes share it.
    const field: number[] = [];
    let finite = 0;
    let lowest = Number.POSITIVE_INFINITY;
    let highest = Number.NEGATIVE_INFINITY;
    let meanAbs = 0;
    for (let j = 0; j <= steps; j++) {
      for (let i = 0; i <= steps; i++) {
        const jet = jetAt(surface, i / steps, j / steps);
        const k = curvatures(firstForm(jet), secondForm(jet));
        const value = useGaussian ? k.gaussian : k.mean;
        field.push(value);
        if (Number.isFinite(value)) {
          finite++;
          lowest = Math.min(lowest, value);
          highest = Math.max(highest, value);
          meanAbs += Math.abs(value);
        }
      }
    }
    if (finite > 0) meanAbs /= finite;
    const scale = meanAbs > 1e-9 ? 1 / meanAbs : 1;

    const built = surfaceObject(surface, steps, {
      wire: false,
      colors: (u, v) => {
        const i = Math.round(u * steps);
        const j = Math.round(v * steps);
        return diverging(field[j * (steps + 1) + i] * scale * gain);
      },
    });

    const group = new THREE.Group();
    group.add(built.object);

    // The tangent frame at the probe, as the two generated records that describe
    // it. Their fields are exactly the values above — nothing is re-derived to
    // fill them in.
    const probeJet = jetAt(surface, params.probeU, params.probeV);
    const normalDirection = new Direction3D(probeJet.normal);
    const tangentSpace = new TangentSpace3D(
      probeJet.position,
      probeJet.du,
      probeJet.dv,
      normalDirection,
    );
    const surfacePoint = new SurfacePoint3D(
      probeJet.position,
      normalDirection,
      probeJet.du,
      probeJet.dv,
      new UvCoordinate(params.probeU, params.probeV),
    );
    const span = new THREE.Box3().setFromObject(built.object).getSize(new THREE.Vector3()).length();
    const armLength = Number.isFinite(span) && span > 0 ? span * 0.16 : 0.4;
    group.add(
      arrow(tangentSpace.Origin, tangentSpace.TangentU, armLength, palette.line),
      arrow(tangentSpace.Origin, tangentSpace.TangentV, armLength, palette.accent),
      arrow(tangentSpace.Origin, tangentSpace.Normal.Vector, armLength * 1.3, palette.surfaceAlt),
      dots([surfacePoint.Position], 0xffffff, 8),
    );

    const first = firstForm(probeJet);
    const second = secondForm(probeJet);
    const k = curvatures(first, second);

    return {
      object: framed(group),
      readings: [
        note('surface', `${subject.label} — ${subject.expect}`),
        note('colour', useGaussian ? 'Gaussian K' : 'Mean H'),
        note('lattice', `${steps} x ${steps}, 9 Eval per node for the stencil`),
        note('range over lattice', finite === 0 ? 'none finite' : `${n4(lowest)} .. ${n4(highest)}`),
        note('mean |value|', n4(meanAbs)),
        reading('FirstFundamentalForm at probe', () => `E ${n4(first.E)} F ${n4(first.F)} G ${n4(first.G)}`),
        reading(
          'SecondFundamentalForm at probe',
          () => `L ${n4(second.L)} M ${n4(second.M)} N ${n4(second.N)}`,
        ),
        note('Gaussian K at probe', n4(k.gaussian)),
        note('Mean H at probe', n4(k.mean)),
        reading('TangentSpace3D.Normal', () => v3s(tangentSpace.Normal.Vector)),
        reading('SurfacePoint3D.Position', () => p3s(surfacePoint.Position)),
        note(
          'Surfaces.FirstFundamentalE/F/G',
          'UNAVAILABLE (first parameter is IDifferentiableSurface; no concrete type implements it, so the member is emitted on nothing)',
        ),
        note(
          'PrincipalCurvatures / SurfaceCurvature',
          'declared in differential-geometry.types.plato; no library body produces either from a surface',
        ),
      ],
    };
  },
});

const demo: Demo = {
  title: 'Parametric surfaces',
  subtitle:
    'surfaces.{types,library}.plato · surfaces-shapes.{types,library}.plato · differential-geometry.types.plato',
  scenes: [quadrics, minimal, oneSided, patches, generated, solids, differential],
};

mountDemo(demo, { orthographic: false, grid: false, spin: true, distance: 4.6 });

// The page never imports this; it exists so an offline script can call every
// scene's `build` without a WebGL context.
export { demo };
