// Parametric curves — a scene catalog over stdlib/geometry/curves.concepts.plato,
// curves.library.plato, curves-shapes.{types,library}.plato,
// splines.{types,library}.plato and differential-geometry.types.plato.
//
// Every point drawn here comes out of a generated `Eval` (or the member the
// library names for that job). Sampling the canonical [0,1] parameter and
// repacking the answers into Three.js buffers is demo work; the parametrisation
// is not. No curve formula is re-derived in this file — where a generated member
// still throws or returns NaN, the status line keeps the member's name and says
// `UNAVAILABLE (…)` rather than substituting a hand-rolled answer.
//
// The plane-curve scenes take an orthographic, grid-free, non-spinning camera;
// the spline, space-curve and frame scenes override it with an orbitable
// perspective one, because a curve in space wants a camera you can turn.
//
// Cost. `build` runs on every slider tick, so every scene caps its sample count
// (256 is the ceiling anywhere on the page) and nothing is evaluated twice: a
// scene samples its curve once and every measure it reports is taken from that
// same curve object.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { fromArray, polylineGeometry, toArray } from '../shared/mesh.js';
import { palette, type ViewerOptions } from '../shared/viewer.js';
import {
  Angle,
  AngleInterval,
  ArchimedeanSpiral2D,
  BSplineCurve3D,
  BezierCurve3D,
  ButterflyCurve2D,
  Cardioid2D,
  Catenary2D,
  CatmullRomCurve3D,
  CircleInvolute2D,
  CircularArc2D,
  CircularArc3D,
  Clothoid2D,
  ConchoidOfDeSluze2D,
  ConicalSpiral3D,
  CubicBezier2D,
  CubicBezier3D,
  Cycloid2D,
  CycloidOfCeva2D,
  Direction3D,
  EllipticalArc2D,
  EllipticalArc3D,
  Epicycloid2D,
  Epitrochoid2D,
  FermatSpiral2D,
  FigureEightKnot,
  Frame3D,
  Helix,
  HermiteCurve3D,
  Hypocycloid2D,
  Hypotrochoid2D,
  KnotVector,
  Lemniscate2D,
  Limacon2D,
  LinearSpline3D,
  Lissajous2D,
  LogarithmicSpiral2D,
  Number2,
  NumberInterval,
  NurbsCurve3D,
  Parabola2D,
  Point2D,
  Point3D,
  Polygon2D,
  QuadraticBezier2D,
  RoseCurve2D,
  SineCurve2D,
  SinusoidalSpiral2D,
  SphericalSpiral3D,
  Superformula2D,
  TcbSpline3D,
  TorusKnot,
  TrefoilKnot,
  TrisectrixOfMaclaurin2D,
  TschirnhausenCubic2D,
  Vector3D,
  VivianiCurve,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// Same contract as `src/demos/polygons.ts`: a member that throws is a gap in the
// emitted library, not a fact about the geometry, so the status line keeps the
// member's name and the failure. Two things on this page are in that state. The
// four trochoid `Eval` bodies multiply a Number by an Angle, which the
// TypeScript writer emits as a plain numeric product, so the receiver of the
// next call is a raw number — Epi- throws, Hypo- comes back NaN. And the whole
// differential-geometry family is declared but never produced: see the note on
// the frames scene.

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

/**
 * A member that cannot fail because it does not exist: the differential-geometry
 * records are declared with no library body anywhere in the tree, so there is
 * nothing to call and nothing to catch. Reported in the same shape as a throw.
 */
function missing(label: string, why: string): Reading {
  return { label, value: `UNAVAILABLE (${why})` };
}

const n4 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(4);
const p2 = (p: Point2D): string => `(${n4(p.X)}, ${n4(p.Y)})`;
const p3 = (p: Point3D): string => `(${n4(p.X)}, ${n4(p.Y)}, ${n4(p.Z)})`;

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
// Cameras

const PLANAR: ViewerOptions = { orthographic: true, grid: false, spin: false, distance: 4.2 };
const SPATIAL: ViewerOptions = { orthographic: false, grid: false, spin: true, distance: 4.6 };

/**
 * The shell's orthographic camera is sized by height alone, so a planar scene is
 * scaled to whichever half-extent is smaller, refreshed per frame because the
 * stage is resizable. Exactly the trick `src/demos/polygons.ts` uses, and for
 * the same reason: a named plane curve's natural size ranges over three orders
 * of magnitude across this page's catalog.
 */
function fitPlanar(object: THREE.Object3D): THREE.Object3D {
  const box = new THREE.Box3().setFromObject(object);
  const extent = Math.max(
    Math.abs(box.min.x),
    Math.abs(box.max.x),
    Math.abs(box.min.y),
    Math.abs(box.max.y),
  );
  if (!Number.isFinite(extent) || extent <= 0) return object;

  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute([0, 0, 0], 3));
  const probe = new THREE.Points(
    geometry,
    new THREE.PointsMaterial({ size: 0, transparent: true, opacity: 0, depthWrite: false }),
  );
  probe.onBeforeRender = (_renderer, _scene, camera): void => {
    const ortho = camera as THREE.OrthographicCamera;
    if (!ortho.isOrthographicCamera) return;
    const half = Math.min((ortho.top - ortho.bottom) / 2, (ortho.right - ortho.left) / 2);
    object.scale.setScalar(Math.min(1, (half * 0.94) / extent));
  };
  object.add(probe);
  return object;
}

/** A spatial scene is scaled once, to a fixed bounding radius the camera frames. */
function fitSpatial(object: THREE.Object3D, radius = 1.6): THREE.Object3D {
  const sphere = new THREE.Box3().setFromObject(object).getBoundingSphere(new THREE.Sphere());
  if (!Number.isFinite(sphere.radius) || sphere.radius <= 0) return object;
  const group = new THREE.Group();
  group.add(object);
  group.scale.setScalar(radius / sphere.radius);
  object.position.sub(sphere.center);
  return group;
}

// ---------------------------------------------------------------------------
// Presentation helpers

function lines(geometry: THREE.BufferGeometry, color: number): THREE.LineSegments {
  return new THREE.LineSegments(geometry, new THREE.LineBasicMaterial({ color }));
}

function segments(coordinates: number[], color: number): THREE.LineSegments {
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(coordinates, 3));
  return lines(geometry, color);
}

function dots(points: readonly Point3D[], color: number, size = 6): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.X, p.Y, p.Z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(geometry, new THREE.PointsMaterial({ color, size, sizeAttenuation: false }));
}

function dots2D(points: readonly Point2D[], color: number, size = 6, z = 0.01): THREE.Points {
  return dots(points.map(p => new Point3D(p.X, p.Y, z)), color, size);
}

/**
 * A faint cross through the origin. Every planar scene carries one, so the stage
 * is never literally empty when the curve it wants is a member that throws.
 */
function originCross(half = 1): THREE.LineSegments {
  return segments([-half, 0, -0.05, half, 0, -0.05, 0, -half, -0.05, 0, half, -0.05], 0x2a3444);
}

/** The rectangle a generated `Bounds2D` describes. */
function boundsOutline(min: Point2D, max: Point2D, color: number): THREE.LineSegments {
  const z = -0.02;
  return segments(
    [
      min.X, min.Y, z, max.X, min.Y, z,
      max.X, min.Y, z, max.X, max.Y, z,
      max.X, max.Y, z, min.X, max.Y, z,
      min.X, max.Y, z, min.X, min.Y, z,
    ],
    color,
  );
}

/** A small axis cross, for marking a computed centroid. */
function marker2D(p: Point2D, color: number, size: number): THREE.LineSegments {
  return segments(
    [p.X - size, p.Y, 0.02, p.X + size, p.Y, 0.02, p.X, p.Y - size, 0.02, p.X, p.Y + size, 0.02],
    color,
  );
}

/**
 * The sampled chain as line segments, with the pieces that left the real plane
 * dropped. Several curves in the catalog are genuinely undefined over part of
 * their canonical turn — the lemniscate's radius is imaginary where cos(2θ) is
 * negative, and the secant-based cubics run to infinity at their asymptotes — so
 * the samples there are NaN or astronomically large. Breaking the polyline is
 * display work; the samples themselves are left exactly as `Eval` returned them,
 * and the status line counts how many were dropped.
 */
function clippedPolyline2D(points: readonly Point2D[], cap: number, z = 0): THREE.BufferGeometry {
  const positions: number[] = [];
  const usable = (p: Point2D): boolean =>
    Number.isFinite(p.X) && Number.isFinite(p.Y) && Math.abs(p.X) <= cap && Math.abs(p.Y) <= cap;
  for (let i = 0; i + 1 < points.length; i++) {
    const a = points[i];
    const b = points[i + 1];
    if (!usable(a) || !usable(b)) continue;
    positions.push(a.X, a.Y, z, b.X, b.Y, z);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return geometry;
}

function finiteCount2D(points: readonly Point2D[], cap: number): number {
  let n = 0;
  for (const p of points) {
    if (Number.isFinite(p.X) && Number.isFinite(p.Y) && Math.abs(p.X) <= cap && Math.abs(p.Y) <= cap) n++;
  }
  return n;
}

// ---------------------------------------------------------------------------
// Sampling
//
// The generated `Sample(count)` / `SamplePeriodic(count)` bodies in
// `curves.library.plato` are the canonical discretizers — one body for every
// embedding, via the generic `ICurve<TRange>` root — so this page asks for them
// by name rather than looping over `Eval` itself.

/** Structural stand-ins for the interfaces the generated TypeScript erases. */
interface Curve2D {
  Eval(t: number): Point2D;
  Sample(count: number): { At(i: number): Point2D; Count(): number };
}
interface Curve3D {
  Eval(t: number): Point3D;
  Sample(count: number): { At(i: number): Point3D; Count(): number };
}

function sample2D(curve: Curve2D, count: number): Point2D[] {
  return toArray(curve.Sample(count) as never) as Point2D[];
}

function sample3D(curve: Curve3D, count: number): Point3D[] {
  return toArray(curve.Sample(count) as never) as Point3D[];
}

// ---------------------------------------------------------------------------
// Input construction
//
// Angle intervals, knot vectors and control polygons are inputs a generated
// member consumes; building them is demo work.

const origin2D = new Point2D(0, 0);
const worldFrame = new Frame3D(
  new Point3D(0, 0, 0),
  new Direction3D(new Vector3D(1, 0, 0)),
  new Direction3D(new Vector3D(0, 1, 0)),
  new Direction3D(new Vector3D(0, 0, 1)),
);

/** The sweep from `a` to `b` full turns, the form every `Angles` field wants. */
function turns(a: number, b: number): AngleInterval {
  return new AngleInterval(a.Turns(), b.Turns());
}

function span(a: number, b: number): NumberInterval {
  return new NumberInterval(a, b);
}

/** A clamped uniform knot vector: the N + D + 1 values `splines.types.plato` asks for. */
function clampedKnots(pointCount: number, degree: number): KnotVector {
  const spans = Math.max(1, pointCount - degree);
  const knots: number[] = [];
  for (let i = 0; i < pointCount + degree + 1; i++) {
    knots.push(Math.min(Math.max(i - degree, 0), spans) / spans);
  }
  return new KnotVector(fromArray(knots));
}

function clampIndex(value: number, length: number): number {
  return Math.min(Math.max(Math.round(value), 0), length - 1);
}

// ---------------------------------------------------------------------------
// Scene 1 — spline families over one control polygon

const SPLINE_FAMILIES = [
  'Bezier',
  'CubicBez',
  'Hermite',
  'CatmullRom',
  'B-spline',
  'NURBS',
  'Linear',
  'TCB',
] as const;

const SPLINE_MEMBERS = [
  'BezierCurve3D.Eval',
  'CubicBezier3D.Eval',
  'HermiteCurve3D.Eval',
  'CatmullRomCurve3D.Eval',
  'BSplineCurve3D.Eval',
  'NurbsCurve3D.Eval',
  'LinearSpline3D.Eval',
  'TcbSpline3D.Eval',
];

/** The zig-zag control polygon every family in this scene is built over. */
function controlPolygon(params: Params): Point3D[] {
  const count = Math.round(params.points);
  const pick = clampIndex(params.pick, count);
  const out: Point3D[] = [];
  for (let i = 0; i < count; i++) {
    const u = count === 1 ? 0 : i / (count - 1);
    const zig = i % 2 === 0 ? -1 : 1;
    const y = params.wave * zig + (i === pick ? params.nudge : 0);
    out.push(new Point3D((u - 0.5) * 2.3, y, params.lift * Math.sin(u * Math.PI * 2)));
  }
  return out;
}

function splineOf(family: number, points: Point3D[], params: Params): Curve3D {
  const n = points.length;
  const degree = Math.min(3, n - 1);
  const knots = clampedKnots(n, degree);
  switch (family) {
    case 0:
      return new BezierCurve3D(fromArray(points));
    case 1:
      // A single cubic arc wants exactly four points; on a longer polygon it
      // takes the two ends and their inward neighbours.
      return new CubicBezier3D(points[0], points[1], points[n - 2], points[n - 1]);
    case 2: {
      // Hermite is given tangents, not off-curve points: the end chords of the
      // polygon, scaled to the polygon's own parameter spacing. `Between` is the
      // generated displacement between two positions.
      const scale = n - 1;
      return new HermiteCurve3D(
        points[0],
        points[0].Between(points[1]).Multiply(scale),
        points[n - 1],
        points[n - 2].Between(points[n - 1]).Multiply(scale),
      );
    }
    case 3:
      return new CatmullRomCurve3D(fromArray(points), params.alpha, false);
    case 4:
      return new BSplineCurve3D(fromArray(points), degree, knots);
    case 5: {
      const weights = points.map((_, i) => (i === 0 || i === n - 1 ? 1 : params.weight));
      return new NurbsCurve3D(fromArray(points), fromArray(weights), degree, knots);
    }
    case 6:
      return new LinearSpline3D(fromArray(points), false);
    default: {
      const zeros = points.map(() => 0);
      return new TcbSpline3D(
        fromArray(points),
        fromArray(zeros),
        fromArray(zeros),
        fromArray(zeros),
        false,
      );
    }
  }
}

const splines = sceneOf({
  id: 'spline-families',
  title: 'Spline families',
  description:
    'One zig-zag control polygon, evaluated by every control-point curve in splines.library.plato. ' +
    'The interpolating families (Hermite, Catmull-Rom, Linear, TCB) pass through the points; the ' +
    'approximating ones (Bezier, B-spline, NURBS) are pulled toward them and only touch the two ends — ' +
    'which is exactly what the two endpoint distances in the status line report. De Casteljau is the ' +
    'whole story for the Bezier family and is written ONCE over a generic $T bounded by IInterpolatable, ' +
    'so points, vectors and weights all go through the same reduction; the B-spline and NURBS entries go ' +
    'through the Cox-de Boor basis recurrence over a clamped uniform knot vector, and NURBS divides by ' +
    'the summed weights, which is the projection out of homogeneous space that makes it rational. ' +
    'Catmull-Rom evaluates only its uniform parameterization: the library says so, and Alpha is ' +
    'documentation rather than behaviour until the non-uniform basis lands next to the others.',
  plato: [
    'BezierCurve3D.Eval',
    'CubicBezier3D.Eval',
    'HermiteCurve3D.Eval',
    'CatmullRomCurve3D.Eval',
    'BSplineCurve3D.Eval',
    'NurbsCurve3D.Eval',
    'LinearSpline3D.Eval',
    'TcbSpline3D.Eval',
    'BSplineCurve3D.ParameterDomain',
    'KnotVector.Domain',
    'ICurve3D.Sample',
    'ICurve3D.ToPolyline3D',
    'ICurve3D.ChordLength',
    'Point3D.Between',
  ],
  viewer: SPATIAL,
  controls: [
    { key: 'family', label: 'Family', kind: 'select', options: [...SPLINE_FAMILIES], def: 4 },
    { key: 'points', label: 'Control points', kind: 'slider', min: 4, max: 9, step: 1, def: 6 },
    { key: 'wave', label: 'Zig-zag', kind: 'slider', min: 0, max: 1.2, step: 0.01, def: 0.75 },
    { key: 'lift', label: 'Out of plane', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0.45 },
    { key: 'pick', label: 'Move point #', kind: 'slider', min: 0, max: 8, step: 1, def: 2 },
    { key: 'nudge', label: 'Move it by', kind: 'slider', min: -1.4, max: 1.4, step: 0.01, def: 0 },
    { key: 'weight', label: 'NURBS weight', kind: 'slider', min: 0.15, max: 5, step: 0.05, def: 1 },
    { key: 'alpha', label: 'C-R alpha', kind: 'slider', min: 0, max: 1, step: 0.5, def: 0.5 },
    { key: 'samples', label: 'Samples', kind: 'slider', min: 16, max: 256, step: 8, def: 128 },
  ],
  build(params: Params): Built {
    const family = clampIndex(params.family, SPLINE_FAMILIES.length);
    const points = controlPolygon(params);
    const count = points.length;
    const degree = Math.min(3, count - 1);
    const samples = Math.round(params.samples);
    const curve = splineOf(family, points, params);

    const object = new THREE.Group();
    object.add(lines(polylineGeometry(points), 0x3f4c63));
    object.add(dots(points, palette.surfaceAlt, 8));

    let traced: Point3D[] = [];
    const evaluated = reading(SPLINE_MEMBERS[family], () => {
      traced = sample3D(curve, samples);
      object.add(lines(polylineGeometry(traced), palette.line));
      return `${traced.length} samples`;
    });

    const endpoints = reading('start ↔ P0 / end ↔ Pn-1', () =>
      `${n4(curve.Eval(0).Between(points[0]).Magnitude())} / ` +
      `${n4(curve.Eval(1).Between(points[count - 1]).Magnitude())}`,
    );

    return {
      object: fitSpatial(object),
      readings: [
        note('family', SPLINE_FAMILIES[family]),
        note('control points', `${count}, degree ${degree}`),
        evaluated,
        endpoints,
        reading('StartPoint', () => p3(curve.Eval(0))),
        reading('MidPoint', () => p3(curve.Eval(0.5))),
        reading('EndPoint', () => p3(curve.Eval(1))),
        reading('polyline Length', () => n4(chainLength(traced))),
        reading('ChordLength', () => n4(points[0].Between(points[count - 1]).Magnitude())),
        family === 4 || family === 5
          ? reading('ParameterDomain', () => {
              const domain = clampedKnots(count, degree).Domain(degree);
              return `[${n4(domain.Start)}, ${n4(domain.End)}]`;
            })
          : note('knots', `${count + degree + 1} clamped uniform`),
      ],
    };
  },
});

/** The length of a sampled chain, for scenes whose curve has no Polyline wrapper handy. */
function chainLength(points: readonly Point3D[]): number {
  let total = 0;
  for (let i = 0; i + 1 < points.length; i++) total += points[i].Between(points[i + 1]).Magnitude();
  return total;
}

// ---------------------------------------------------------------------------
// The plane-curve galleries
//
// Three scenes over the same shape: a `select` of named curves, three shared
// shape sliders, and one generated `Eval` per entry. `Scale`, `Frequency` and
// `Shape` mean whatever the chosen curve's own fields call for; each entry says
// which of its fields they land in.

interface Shape {
  scale: number;
  freq: number;
  shape: number;
}

interface PlaneEntry {
  label: string;
  member: string;
  /** What the generated body computes, in one clause. */
  says: string;
  make(s: Shape): Curve2D;
  /** Display clip: beyond this the curve has left the visible plane. */
  cap?: number;
}

function planeGallery(spec: {
  id: string;
  title: string;
  description: string;
  plato: string[];
  entries: PlaneEntry[];
  scale: [number, number, number];
  freq: [number, number, number];
  shape: [number, number, number];
}): Scene {
  return sceneOf({
    id: spec.id,
    title: spec.title,
    description: spec.description,
    plato: spec.plato,
    viewer: PLANAR,
    controls: [
      {
        key: 'curve',
        label: 'Curve',
        kind: 'select',
        options: spec.entries.map(e => e.label),
        def: 0,
      },
      {
        key: 'scale',
        label: 'Scale',
        kind: 'slider',
        min: spec.scale[0],
        max: spec.scale[1],
        step: 0.01,
        def: spec.scale[2],
      },
      {
        key: 'freq',
        label: 'Frequency',
        kind: 'slider',
        min: spec.freq[0],
        max: spec.freq[1],
        step: 1,
        def: spec.freq[2],
      },
      {
        key: 'shape',
        label: 'Shape',
        kind: 'slider',
        min: spec.shape[0],
        max: spec.shape[1],
        step: 0.01,
        def: spec.shape[2],
      },
      { key: 'samples', label: 'Samples', kind: 'slider', min: 32, max: 256, step: 8, def: 192 },
    ],
    build(params: Params): Built {
      const entry = spec.entries[clampIndex(params.curve, spec.entries.length)];
      const shape: Shape = { scale: params.scale, freq: params.freq, shape: params.shape };
      const samples = Math.round(params.samples);
      const cap = entry.cap ?? 40;

      const object = new THREE.Group();
      object.add(originCross(0.35));

      let traced: Point2D[] = [];
      const evaluated = reading(entry.member, () => {
        traced = sample2D(entry.make(shape), samples);
        object.add(lines(clippedPolyline2D(traced, cap), palette.line));
        return `${traced.length} samples`;
      });
      const finite = finiteCount2D(traced, cap);

      return {
        object: fitPlanar(object),
        readings: [
          note('curve', entry.label),
          note('body', entry.says),
          // A member that returns NaN everywhere is the same kind of finding as
          // one that throws, so it is reported the same way rather than shown as
          // an empty stage with no explanation.
          finite === 0 && traced.length > 0
            ? missing(entry.member, 'returns NaN at every sample')
            : evaluated,
          note('drawn', `${finite} of ${traced.length} samples on the real plane`),
          reading('StartPoint', () => (traced.length > 0 ? p2(traced[0]) : '—')),
          reading('EndPoint', () => (traced.length > 0 ? p2(traced[traced.length - 1]) : '—')),
          note('extent', extentOf(traced, cap)),
        ],
      };
    },
  });
}

function extentOf(points: readonly Point2D[], cap: number): string {
  let minX = Infinity;
  let minY = Infinity;
  let maxX = -Infinity;
  let maxY = -Infinity;
  for (const p of points) {
    if (!Number.isFinite(p.X) || !Number.isFinite(p.Y)) continue;
    if (Math.abs(p.X) > cap || Math.abs(p.Y) > cap) continue;
    minX = Math.min(minX, p.X);
    maxX = Math.max(maxX, p.X);
    minY = Math.min(minY, p.Y);
    maxY = Math.max(maxY, p.Y);
  }
  if (!Number.isFinite(minX)) return '—';
  return `${n4(maxX - minX)} x ${n4(maxY - minY)}`;
}

const polar = planeGallery({
  id: 'polar-curves',
  title: 'Polar and figure curves',
  description:
    'The polar zoo of curves-shapes.library.plato. Most of these types implement IPolarCurve2D and cost ' +
    'exactly one RadiusAt body: the generic bridge Eval(IPolarCurve2D, t) => CartesianPositionAt(t.Turns) ' +
    'in curves.library.plato turns a radius-at-an-angle into a position for every one of them. The rest ' +
    '(cardioid, limacon, lemniscate, Lissajous, butterfly) spell out their own Eval. Several are genuinely ' +
    'undefined over part of the turn — the lemniscate\'s radius is the square root of a negative number ' +
    'where cos(2θ) < 0, and the secant cubics run to their asymptotes — so those samples come back NaN or ' +
    'enormous and the polyline is broken there rather than papered over.',
  plato: [
    'Curves.Eval(IPolarCurve2D, Number)',
    'Curves.CartesianPositionAt',
    'Curves.PolarPositionAt',
    'RoseCurve2D.RadiusAt',
    'Cardioid2D.Eval',
    'Limacon2D.Eval',
    'Lemniscate2D.Eval',
    'Lissajous2D.Eval',
    'Superformula2D.RadiusAt',
    'CycloidOfCeva2D.RadiusAt',
    'TschirnhausenCubic2D.RadiusAt',
    'ConchoidOfDeSluze2D.RadiusAt',
    'SinusoidalSpiral2D.RadiusAt',
    'TrisectrixOfMaclaurin2D.RadiusAt',
    'ButterflyCurve2D.Eval',
    'CurvesShapes.ButterflyFactor',
  ],
  entries: [
    {
      label: 'Rose',
      member: 'RoseCurve2D.RadiusAt',
      says: 'r = Radius * cos(k θ)',
      make: s => new RoseCurve2D(s.scale, Math.max(1, Math.round(s.freq))),
    },
    {
      label: 'Cardioid',
      member: 'Cardioid2D.Eval',
      says: 'r = 2 Radius (1 + cos θ)',
      make: s => new Cardioid2D(s.scale * 0.5),
    },
    {
      label: 'Limacon',
      member: 'Limacon2D.Eval',
      says: 'r = Offset + Amplitude cos θ',
      make: s => new Limacon2D(s.scale * 0.5, s.shape),
    },
    {
      label: 'Lemniscate',
      member: 'Lemniscate2D.Eval',
      says: 'r² = Scale² cos(2θ)',
      make: s => new Lemniscate2D(s.scale),
    },
    {
      label: 'Lissajous',
      member: 'Lissajous2D.Eval',
      says: 'x, y two sines at a frequency ratio',
      make: s =>
        new Lissajous2D(
          s.scale,
          s.scale,
          Math.max(1, Math.round(s.freq)),
          Math.max(1, Math.round(s.freq) + 1),
          new Angle(Math.PI / 2),
        ),
    },
    {
      label: 'Superformula',
      member: 'Superformula2D.RadiusAt',
      says: 'the Gielis superformula',
      make: s =>
        new Superformula2D(
          Math.max(1, Math.round(s.freq)),
          1,
          s.shape * 2 + 0.4,
          s.shape * 2 + 0.4,
          s.scale,
          s.scale,
        ),
    },
    {
      label: 'Ceva',
      member: 'CycloidOfCeva2D.RadiusAt',
      says: 'r = Scale (1 + 2 cos 2θ)',
      make: s => new CycloidOfCeva2D(s.scale * 0.45),
    },
    {
      label: 'Tschirnhausen',
      member: 'TschirnhausenCubic2D.RadiusAt',
      says: 'r = Scale sec(θ/3)³',
      make: s => new TschirnhausenCubic2D(s.scale * 0.35),
      cap: 5,
    },
    {
      label: 'Conchoid',
      member: 'ConchoidOfDeSluze2D.RadiusAt',
      says: 'r = sec θ + Scale cos θ',
      make: s => new ConchoidOfDeSluze2D(s.shape * 4 - 3),
      cap: 4,
    },
    {
      label: 'SinSpiral',
      member: 'SinusoidalSpiral2D.RadiusAt',
      says: 'rⁿ = Scaleⁿ cos(n θ)',
      make: s => new SinusoidalSpiral2D(s.scale, s.shape * 3 - 1.5 || 0.5),
      cap: 6,
    },
    {
      label: 'Trisectrix',
      member: 'TrisectrixOfMaclaurin2D.RadiusAt',
      says: 'r = 2 Scale / cos(θ/3)',
      make: s => new TrisectrixOfMaclaurin2D(s.scale * 0.35),
      cap: 5,
    },
    {
      label: 'Butterfly',
      member: 'ButterflyCurve2D.Eval',
      says: 'the transcendental butterfly, over its 12π period',
      make: s => new ButterflyCurve2D(s.scale * 0.4),
    },
  ],
  scale: [0.2, 1.6, 1],
  freq: [1, 9, 5],
  shape: [0, 1, 0.5],
});

const spirals = planeGallery({
  id: 'spirals-roulettes',
  title: 'Spirals and roulettes',
  description:
    'The curves traced by a steadily growing radius or by a point carried on a rolling circle. The three ' +
    'classical spirals carry their own Angles sweep and so override the generic full-turn polar mapping; ' +
    'the clothoid is the interesting one, because its Fresnel integrals have no closed form and the ' +
    'library evaluates them by composite Simpson quadrature over the tangent angle rather than by a ' +
    'truncated series. The four trochoid entries do not evaluate: the emitted body multiplies a Number by ' +
    'an Angle, which the TypeScript writer types as a plain product, so the receiver of the next call is a ' +
    'raw number — Epi- throws, Hypo- returns NaN.',
  plato: [
    'ArchimedeanSpiral2D.Eval',
    'LogarithmicSpiral2D.Eval',
    'FermatSpiral2D.Eval',
    'Clothoid2D.Eval',
    'CurvesShapes.TangentAngleAt',
    'CurvesShapes.SimpsonWeight',
    'CircleInvolute2D.Eval',
    'Cycloid2D.Eval',
    'Epicycloid2D.Eval',
    'Hypocycloid2D.Eval',
    'Epitrochoid2D.Eval',
    'Hypotrochoid2D.Eval',
  ],
  entries: [
    {
      label: 'Archimedes',
      member: 'ArchimedeanSpiral2D.Eval',
      says: 'r = StartRadius + GrowthRate θ',
      make: s => new ArchimedeanSpiral2D(0.02, s.scale * 0.12, turns(0, Math.max(1, s.freq))),
    },
    {
      label: 'Log spiral',
      member: 'LogarithmicSpiral2D.Eval',
      says: 'r = StartRadius exp(GrowthRate θ)',
      make: s =>
        new LogarithmicSpiral2D(s.scale * 0.1, s.shape * 0.3, turns(0, Math.max(1, s.freq))),
    },
    {
      label: 'Fermat',
      member: 'FermatSpiral2D.Eval',
      says: 'r = Scale sqrt θ',
      make: s => new FermatSpiral2D(s.scale * 0.35, turns(0, Math.max(1, s.freq))),
    },
    {
      label: 'Clothoid',
      member: 'Clothoid2D.Eval',
      says: 'the Fresnel integrals, by Simpson quadrature',
      make: s => new Clothoid2D(s.shape * 1.6 + 0.4, span(-s.scale * 4, s.scale * 4)),
    },
    {
      label: 'Involute',
      member: 'CircleInvolute2D.Eval',
      says: 'a taut string unwinding from a circle',
      make: s => new CircleInvolute2D(s.scale * 0.25, turns(0, Math.max(1, s.freq))),
    },
    {
      label: 'Cycloid',
      member: 'Cycloid2D.Eval',
      says: 'a rim point on a circle rolling along X',
      make: s => new Cycloid2D(s.scale * 0.35, turns(0, Math.max(1, s.freq))),
    },
    {
      label: 'Epicycloid',
      member: 'Epicycloid2D.Eval',
      says: 'a rim point rolling outside a fixed circle',
      make: s => new Epicycloid2D(s.scale, s.scale / Math.max(1, Math.round(s.freq))),
    },
    {
      label: 'Hypocycloid',
      member: 'Hypocycloid2D.Eval',
      says: 'a rim point rolling inside a fixed circle',
      make: s => new Hypocycloid2D(s.scale, s.scale / Math.max(1, Math.round(s.freq))),
    },
    {
      label: 'Epitrochoid',
      member: 'Epitrochoid2D.Eval',
      says: 'the spirograph outside the fixed circle',
      make: s =>
        new Epitrochoid2D(
          s.scale,
          s.scale / Math.max(1, Math.round(s.freq)),
          s.scale * s.shape,
        ),
    },
    {
      label: 'Hypotrochoid',
      member: 'Hypotrochoid2D.Eval',
      says: 'the spirograph inside the fixed circle',
      make: s =>
        new Hypotrochoid2D(
          s.scale,
          s.scale / Math.max(1, Math.round(s.freq)),
          s.scale * s.shape,
        ),
    },
  ],
  scale: [0.2, 1.6, 1],
  freq: [1, 9, 4],
  shape: [0, 1, 0.5],
});

const arcs = planeGallery({
  id: 'arcs-and-graphs',
  title: 'Arcs, graphs and Bezier arcs',
  description:
    'The elementary open pieces of plane geometry: conic arcs swept over an AngleInterval, the graphs of ' +
    'the standard transcendental functions swept over a NumberInterval, and the quadratic and cubic ' +
    'Bezier arcs vector graphics is built from. The two Bezier entries go through the Bernstein forms in ' +
    'library Polynomials, which are generic over INumerical — the control points are converted to ' +
    'displacements with PositionVector and back with ToPoint, and nothing degree-specific is written ' +
    'in curves-shapes.library.plato itself.',
  plato: [
    'CircularArc2D.Eval',
    'EllipticalArc2D.Eval',
    'Parabola2D.Eval',
    'Catenary2D.Eval',
    'SineCurve2D.Eval',
    'QuadraticBezier2D.Eval',
    'CubicBezier2D.Eval',
    'CurvesShapes.CirclePoint',
    'CurvesShapes.RotateBy',
    'Polynomials.QuadraticBezier',
    'Polynomials.CubicBezier',
  ],
  entries: [
    {
      label: 'Circular arc',
      member: 'CircularArc2D.Eval',
      says: 'Center + CirclePoint(Angles.Lerp(t), Radius)',
      make: s => new CircularArc2D(origin2D, s.scale, turns(0, Math.max(0.05, s.shape * 1.5))),
    },
    {
      label: 'Elliptic arc',
      member: 'EllipticalArc2D.Eval',
      says: '(a cos θ, b sin θ) tilted by Rotation',
      make: s =>
        new EllipticalArc2D(
          origin2D,
          new Number2(s.scale, s.scale * 0.55),
          new Angle(s.shape * Math.PI),
          turns(0, 1),
        ),
    },
    {
      label: 'Parabola',
      member: 'Parabola2D.Eval',
      says: 'y = Coefficient x², x over Domain',
      make: s => new Parabola2D(s.shape * 2, span(-s.scale * 1.6, s.scale * 1.6)),
    },
    {
      label: 'Catenary',
      member: 'Catenary2D.Eval',
      says: 'y = Scale cosh(x / Scale)',
      make: s => new Catenary2D(s.scale * 0.6, span(-s.scale * 2, s.scale * 2)),
    },
    {
      label: 'Sine',
      member: 'SineCurve2D.Eval',
      says: 'y = Amplitude sin(2π x / Wavelength + Phase)',
      make: s =>
        new SineCurve2D(
          s.scale * 0.7,
          Math.max(0.15, 2 / Math.max(1, Math.round(s.freq))),
          new Angle(s.shape * Math.PI * 2),
          span(-1.6, 1.6),
        ),
    },
    {
      label: 'Quad Bezier',
      member: 'QuadraticBezier2D.Eval',
      says: 'the Bernstein quadratic over P0..P2',
      make: s =>
        new QuadraticBezier2D(
          new Point2D(-s.scale, -s.scale * 0.6),
          new Point2D(0, s.scale * (0.4 + s.shape * 2)),
          new Point2D(s.scale, -s.scale * 0.6),
        ),
    },
    {
      label: 'Cubic Bezier',
      member: 'CubicBezier2D.Eval',
      says: 'the Bernstein cubic over P0..P3',
      make: s =>
        new CubicBezier2D(
          new Point2D(-s.scale, -s.scale * 0.5),
          new Point2D(-s.scale * 0.35, s.scale * (0.3 + s.shape * 2)),
          new Point2D(s.scale * 0.35, -s.scale * (0.3 + s.shape * 2)),
          new Point2D(s.scale, s.scale * 0.5),
        ),
    },
  ],
  scale: [0.2, 1.6, 1],
  freq: [1, 9, 3],
  shape: [0, 1, 0.5],
});

// ---------------------------------------------------------------------------
// Scene 5 — curves in space

interface SpaceEntry {
  label: string;
  member: string;
  says: string;
  make(s: Shape): Curve3D;
  /** Closed curves report the closure gap the marker interface promises. */
  closed?: boolean;
}

const SPACE_ENTRIES: SpaceEntry[] = [
  {
    label: 'Helix',
    member: 'Helix.Eval',
    says: 'R cos θ, R sin θ, Pitch per turn',
    make: s => new Helix(worldFrame, s.scale, s.shape * 1.2, turns(0, Math.max(1, s.freq))),
  },
  {
    label: 'Conical',
    member: 'ConicalSpiral3D.Eval',
    says: 'the winding radius interpolates while the curve climbs',
    make: s =>
      new ConicalSpiral3D(
        worldFrame,
        span(0.04, s.scale),
        s.shape * 3,
        turns(0, Math.max(1, s.freq)),
      ),
  },
  {
    label: 'Spherical',
    member: 'SphericalSpiral3D.Eval',
    says: 'a loxodrome: constant bearing, pole to pole',
    make: s => new SphericalSpiral3D(worldFrame, s.scale, Math.max(1, Math.round(s.freq))),
  },
  {
    label: 'Torus knot',
    member: 'TorusKnot.Eval',
    says: 'the (p, q) knot on a torus about the frame Z axis',
    make: s =>
      new TorusKnot(
        worldFrame,
        Math.max(1, Math.round(s.freq)),
        Math.max(2, Math.round(s.shape * 6) + 2),
        s.scale,
        s.scale * 0.35,
      ),
    closed: true,
  },
  {
    label: 'Trefoil',
    member: 'TrefoilKnot.Eval',
    says: 'sin t + 2 sin 2t, cos t - 2 cos 2t, -sin 3t',
    make: s => new TrefoilKnot(worldFrame, s.scale * 0.4),
    closed: true,
  },
  {
    label: 'Figure eight',
    member: 'FigureEightKnot.Eval',
    says: 'the unique four-crossing knot',
    make: s => new FigureEightKnot(worldFrame, s.scale * 0.4),
    closed: true,
  },
  {
    label: 'Viviani',
    member: 'VivianiCurve.Eval',
    says: 'sphere ∩ tangent cylinder of half the radius',
    make: s => new VivianiCurve(worldFrame, s.scale),
    closed: true,
  },
  {
    label: 'Circ arc 3D',
    member: 'CircularArc3D.Eval',
    says: 'a circle in the frame\'s XY plane, placed by FramePoint',
    make: s => new CircularArc3D(worldFrame, s.scale, turns(0, Math.max(0.05, s.shape * 1.5))),
  },
  {
    label: 'Ellip arc 3D',
    member: 'EllipticalArc3D.Eval',
    says: 'semi-axes along the frame\'s X and Y',
    make: s =>
      new EllipticalArc3D(worldFrame, new Number2(s.scale, s.scale * 0.5), turns(0, 1)),
  },
];

const space = sceneOf({
  id: 'space-curves',
  title: 'Curves in space',
  description:
    'The named closed-form space curves of curves-shapes.library.plato. Every formula is written in the ' +
    'curve\'s own canonical position and placed by the shared FramePoint helper — Origin + x·XAxis + ' +
    'y·YAxis + z·ZAxis — so "winds about the frame\'s Z axis" is literally what the body does. The knots ' +
    'and Viviani\'s curve are IClosedCurve3D, and ClosureGap, the derived distance between Eval(0) and ' +
    'Eval(1), is what witnesses that guarantee: it comes back at machine epsilon.',
  plato: [
    'Helix.Eval',
    'ConicalSpiral3D.Eval',
    'SphericalSpiral3D.Eval',
    'TorusKnot.Eval',
    'TrefoilKnot.Eval',
    'FigureEightKnot.Eval',
    'VivianiCurve.Eval',
    'CircularArc3D.Eval',
    'CircularArc3D.CurvePlane',
    'EllipticalArc3D.Eval',
    'CurvesShapes.FramePoint',
    'CurvesShapes.FramePlane',
    'ICurve3D.Sample',
    'ICurve3D.ToPolyline3D',
    'Polyline3D.Length',
    'Polyline3D.Bounds',
    'IClosedCurve3D.ClosureGap',
    'IClosedCurve3D.IsProperlyClosed',
  ],
  viewer: SPATIAL,
  controls: [
    {
      key: 'curve',
      label: 'Curve',
      kind: 'select',
      options: SPACE_ENTRIES.map(e => e.label),
      def: 0,
    },
    { key: 'scale', label: 'Scale', kind: 'slider', min: 0.2, max: 1.6, step: 0.01, def: 1 },
    { key: 'freq', label: 'Frequency', kind: 'slider', min: 1, max: 9, step: 1, def: 4 },
    { key: 'shape', label: 'Shape', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0.5 },
    { key: 'samples', label: 'Samples', kind: 'slider', min: 32, max: 256, step: 8, def: 192 },
  ],
  build(params: Params): Built {
    const entry = SPACE_ENTRIES[clampIndex(params.curve, SPACE_ENTRIES.length)];
    const shape: Shape = { scale: params.scale, freq: params.freq, shape: params.shape };
    const samples = Math.round(params.samples);
    const curve = entry.make(shape);

    const object = new THREE.Group();
    object.add(segments([-0.05, 0, 0, 0.05, 0, 0, 0, -0.05, 0, 0, 0.05, 0], 0x2a3444));

    let traced: Point3D[] = [];
    const evaluated = reading(entry.member, () => {
      traced = sample3D(curve, samples);
      object.add(lines(polylineGeometry(traced), palette.line));
      return `${traced.length} samples`;
    });
    if (traced.length > 0) object.add(dots([traced[0]], palette.accent, 9));

    const closure = entry.closed
      ? reading('ClosureGap', () =>
          n4((curve as unknown as { ClosureGap(): number }).ClosureGap()),
        )
      : note('ClosureGap', 'not an IClosedCurve3D');

    return {
      object: fitSpatial(object),
      readings: [
        note('curve', entry.label),
        note('body', entry.says),
        evaluated,
        reading('StartPoint', () => p3(curve.Eval(0))),
        reading('MidPoint', () => p3(curve.Eval(0.5))),
        reading('EndPoint', () => p3(curve.Eval(1))),
        reading('polyline Length', () => n4(chainLength(traced))),
        reading('ChordLength', () => n4(curve.Eval(0).Between(curve.Eval(1)).Magnitude())),
        closure,
        note('extent', extentOf3D(traced)),
      ],
    };
  },
});

function extentOf3D(points: readonly Point3D[]): string {
  if (points.length === 0) return '—';
  const box = new THREE.Box3();
  for (const p of points) box.expandByPoint(new THREE.Vector3(p.X, p.Y, p.Z));
  const size = box.getSize(new THREE.Vector3());
  return `${n4(size.x)} x ${n4(size.y)} x ${n4(size.z)}`;
}

// ---------------------------------------------------------------------------
// Scene 6 — frames along a curve
//
// What the stdlib actually offers here is worth stating plainly, because the
// scene is shaped by it. `differential-geometry.types.plato` declares
// FrenetFrame2D/3D, RotationMinimizingFrame3D, DarbouxFrame3D, CurveJet2D/3D,
// OsculatingCircle2D and CurvatureComb2D — but they are records, and NO library
// anywhere in the tree produces one from a curve. `curves.concepts.plato`
// declares IDifferentiableCurve2D/3D and IFramedCurve3D, whose derived helpers
// (UnitTangentAt, CurvatureVectorAt, FrameAt, …) all sit on obligations —
// TangentAt, CurvatureAt, TorsionAt, FrameAt — that no concrete curve type in
// the stdlib implements. So there is nothing to call for any of them.
//
// What IS derivable, and what this scene draws, is the finite-difference
// velocity `VelocityAt(t, h)` that curves.library.plato defines for every
// ICurve3D, completed to an orthonormal triad by two more generated members:
// `PlaneTangent` (any unit vector perpendicular to this one, crossed against
// whichever world axis it is least aligned with, from polygons.library.plato)
// and `Cross`. That is a stable reference frame, not a Frenet or a
// rotation-minimizing one, and the description says so.

const FRAME_CURVES: SpaceEntry[] = [
  SPACE_ENTRIES[0],
  SPACE_ENTRIES[1],
  SPACE_ENTRIES[3],
  SPACE_ENTRIES[4],
  SPACE_ENTRIES[5],
  SPACE_ENTRIES[6],
];

const frames = sceneOf({
  id: 'frames',
  title: 'Tangents and frames',
  description:
    'A triad at evenly spaced parameters along a space curve. The blue axis is the velocity from ' +
    'ICurve3D.VelocityAt(t, h) — the central-difference estimate curves.library.plato derives for every ' +
    'curve from Eval alone — and the two transverse axes come from PlaneTangent, the generated ' +
    '"any unit vector perpendicular to this one" that never degenerates, crossed back against the ' +
    'tangent. That is a stable reference frame, NOT a Frenet frame: FrenetFrame3D, ' +
    'RotationMinimizingFrame3D, DarbouxFrame3D, CurveJet2D/3D, OsculatingCircle2D and CurvatureComb2D ' +
    'are declared as records with no library body anywhere, and no concrete curve implements the ' +
    'TangentAt / CurvatureAt / TorsionAt / FrameAt obligations the differentiable-curve interfaces ' +
    'declare, so curvature and torsion have nothing to call. The status line names every one of them.',
  plato: [
    'ICurve3D.VelocityAt',
    'ICurve3D.SecantBetween',
    'ICurve3D.Chord',
    'ICurve3D.ChordLength',
    'ICurve3D.Sample',
    'Vector3D.Normalize',
    'Vector3D.Magnitude',
    'Polygons.PlaneTangent',
    'Vector3D.Cross',
    'FrenetFrame3D',
    'RotationMinimizingFrame3D',
    'DarbouxFrame3D',
    'CurveJet3D',
    'OsculatingCircle2D',
    'CurvatureComb2D',
  ],
  viewer: SPATIAL,
  controls: [
    {
      key: 'curve',
      label: 'Curve',
      kind: 'select',
      options: FRAME_CURVES.map(e => e.label),
      def: 0,
    },
    { key: 'scale', label: 'Scale', kind: 'slider', min: 0.2, max: 1.6, step: 0.01, def: 1 },
    { key: 'freq', label: 'Frequency', kind: 'slider', min: 1, max: 9, step: 1, def: 3 },
    { key: 'shape', label: 'Shape', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0.5 },
    { key: 'frames', label: 'Frames', kind: 'slider', min: 4, max: 48, step: 1, def: 18 },
    { key: 'size', label: 'Triad size', kind: 'slider', min: 0.02, max: 0.5, step: 0.01, def: 0.16 },
    { key: 'step', label: 'Difference step', kind: 'slider', min: -5, max: -1.5, step: 0.1, def: -3 },
    { key: 'samples', label: 'Samples', kind: 'slider', min: 32, max: 256, step: 8, def: 192 },
  ],
  build(params: Params): Built {
    const entry = FRAME_CURVES[clampIndex(params.curve, FRAME_CURVES.length)];
    const shape: Shape = { scale: params.scale, freq: params.freq, shape: params.shape };
    const curve = entry.make(shape);
    const velocities = curve as unknown as { VelocityAt(t: number, h: number): Vector3D };
    const frameCount = Math.round(params.frames);
    const size = params.size;
    const h = Math.pow(10, params.step);

    const object = new THREE.Group();
    let traced: Point3D[] = [];
    const evaluated = reading(entry.member, () => {
      traced = sample3D(curve, Math.round(params.samples));
      object.add(lines(polylineGeometry(traced), 0x44566f));
      return `${traced.length} samples`;
    });

    const tangentLines: number[] = [];
    const normalLines: number[] = [];
    const binormalLines: number[] = [];
    let slowest = Infinity;
    let fastest = 0;
    const triads = reading('VelocityAt', () => {
      for (let i = 0; i < frameCount; i++) {
        // Kept off the endpoints: a central difference needs t ± h in range.
        const t = h + ((1 - 2 * h) * i) / Math.max(1, frameCount - 1);
        const p = curve.Eval(t);
        const velocity = velocities.VelocityAt(t, h);
        const speed = velocity.Magnitude();
        if (!Number.isFinite(speed) || speed <= 0) continue;
        slowest = Math.min(slowest, speed);
        fastest = Math.max(fastest, speed);
        const tangent = velocity.Normalize();
        const normal = tangent.PlaneTangent();
        const binormal = tangent.Cross(normal);
        push(tangentLines, p, tangent, size);
        push(normalLines, p, normal, size * 0.7);
        push(binormalLines, p, binormal, size * 0.7);
      }
      return `${frameCount} frames at h = ${h.toExponential(1)}`;
    });
    object.add(segments(tangentLines, palette.line));
    object.add(segments(normalLines, palette.surfaceAlt));
    object.add(segments(binormalLines, palette.accent));

    return {
      object: fitSpatial(object),
      readings: [
        note('curve', entry.label),
        evaluated,
        triads,
        note('speed range', Number.isFinite(slowest) ? `${n4(slowest)} .. ${n4(fastest)}` : '—'),
        reading('Chord', () => p3(curve.Eval(0).Between(curve.Eval(1)).ToPoint())),
        reading('ChordLength', () => n4(curve.Eval(0).Between(curve.Eval(1)).Magnitude())),
        missing('FrenetFrame3D', 'record type; no library member produces one'),
        missing('RotationMinimizingFrame3D', 'record type; no library member produces one'),
        missing('DarbouxFrame3D', 'record type; no library member produces one'),
        missing('CurveJet2D / CurveJet3D', 'record types; no library member produces one'),
        missing('OsculatingCircle2D / CurvatureComb2D', 'record types; no library member produces one'),
        missing(
          'IDifferentiableCurve3D.CurvatureAt / TorsionAt',
          'no concrete curve implements the obligation',
        ),
        missing('IFramedCurve3D.FrameAt', 'no concrete curve implements the obligation'),
      ],
    };
  },
});

function push(out: number[], p: Point3D, v: Vector3D, length: number): void {
  out.push(p.X, p.Y, p.Z, p.X + v.X * length, p.Y + v.Y * length, p.Z + v.Z * length);
}

// ---------------------------------------------------------------------------
// Scene 7 — measures

interface MeasureEntry {
  label: string;
  member: string;
  make(s: Shape): Curve2D;
  /** Implements IClosedCurve2D, so ClosureGap and IsProperlyClosed are derived. */
  closedCurve?: boolean;
  /** Geometrically returns to its start, so the samples make a sensible ring. */
  ring?: boolean;
  /** Implements IArcLengthParameterized<Point2D>. */
  arcLength?: boolean;
  cap?: number;
}

const MEASURE_ENTRIES: MeasureEntry[] = [
  {
    label: 'Butterfly',
    member: 'ButterflyCurve2D.Eval',
    make: s => new ButterflyCurve2D(s.scale * 0.4),
    closedCurve: true,
    ring: true,
  },
  {
    label: 'Cardioid',
    member: 'Cardioid2D.Eval',
    make: s => new Cardioid2D(s.scale * 0.5),
    closedCurve: true,
    ring: true,
  },
  {
    label: 'Limacon',
    member: 'Limacon2D.Eval',
    make: s => new Limacon2D(s.scale * 0.5, s.shape * 2),
    closedCurve: true,
    ring: true,
  },
  {
    label: 'Rose',
    member: 'RoseCurve2D.RadiusAt',
    make: s => new RoseCurve2D(s.scale, Math.max(1, Math.round(s.freq))),
    ring: true,
  },
  {
    label: 'Superformula',
    member: 'Superformula2D.RadiusAt',
    make: s =>
      new Superformula2D(Math.max(1, Math.round(s.freq)), 1, 1, 1, s.scale, s.scale),
    ring: true,
  },
  {
    label: 'Clothoid',
    member: 'Clothoid2D.Eval',
    make: s => new Clothoid2D(s.shape * 1.6 + 0.4, span(-s.scale * 4, s.scale * 4)),
    arcLength: true,
  },
  {
    label: 'Involute',
    member: 'CircleInvolute2D.Eval',
    make: s => new CircleInvolute2D(s.scale * 0.25, turns(0, Math.max(1, s.freq))),
  },
];

const measures = sceneOf({
  id: 'measures',
  title: 'Measures',
  description:
    'What can be measured about a curve once it has been sampled. ToPolyline2D turns the curve into the ' +
    'chain Polyline2D.Length measures, and doubling the sample count shows the length converging from ' +
    'below, which is what a chord approximation does. For the curves that close, the samples make a ' +
    'Polygon2D and the ring measures of polygons.library.plato apply — Area, Perimeter, Centroid, and ' +
    'the ClosestPoint the orange spokes run to. The clothoid is the one type in the whole curve catalog ' +
    'that implements IArcLengthParameterized: its parameter IS signed arc length, so ArcLength, ' +
    'PointAtLength and ParameterAtLength are exact rather than tabulated.',
  plato: [
    'ICurve2D.Sample',
    'ICurve2D.ToPolyline2D',
    'Polyline2D.Length',
    'Polyline2D.Bounds',
    'Polyline2D.VertexCentroid',
    'ICurve2D.ChordLength',
    'IClosedCurve2D.ClosureGap',
    'IClosedCurve2D.IsProperlyClosed',
    'Polygon2D.Area',
    'Polygon2D.Perimeter',
    'Polygon2D.Centroid',
    'Polygon2D.ClosestPoint',
    'Polygon2D.DistanceTo',
    'Polygon2D.Contains',
    'Clothoid2D.ArcLength',
    'Clothoid2D.PointAtLength',
    'Clothoid2D.ParameterAtLength',
    'IArcLengthParameterized.PointAtLengthFraction',
    'IArcLengthParameterized.LengthFraction',
    'IArcLengthParameterized.IsValidLength',
  ],
  viewer: PLANAR,
  controls: [
    {
      key: 'curve',
      label: 'Curve',
      kind: 'select',
      options: MEASURE_ENTRIES.map(e => e.label),
      def: 0,
    },
    { key: 'scale', label: 'Scale', kind: 'slider', min: 0.2, max: 1.6, step: 0.01, def: 1 },
    { key: 'freq', label: 'Frequency', kind: 'slider', min: 1, max: 9, step: 1, def: 5 },
    { key: 'shape', label: 'Shape', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0.5 },
    { key: 'samples', label: 'Samples', kind: 'slider', min: 24, max: 192, step: 8, def: 96 },
    { key: 'probes', label: 'Probes', kind: 'slider', min: 0, max: 16, step: 1, def: 8 },
  ],
  build(params: Params): Built {
    const entry = MEASURE_ENTRIES[clampIndex(params.curve, MEASURE_ENTRIES.length)];
    const shape: Shape = { scale: params.scale, freq: params.freq, shape: params.shape };
    const samples = Math.round(params.samples);
    const cap = entry.cap ?? 40;
    const curve = entry.make(shape);

    const object = new THREE.Group();
    object.add(originCross(0.25));

    let traced: Point2D[] = [];
    const evaluated = reading(entry.member, () => {
      traced = sample2D(curve, samples);
      object.add(lines(clippedPolyline2D(traced, cap), palette.line));
      return `${traced.length} samples`;
    });

    const readings: Reading[] = [
      note('curve', entry.label),
      evaluated,
      reading('Polyline2D.Length', () =>
        n4((curve as unknown as { ToPolyline2D(n: number): { Length(): number } })
          .ToPolyline2D(samples)
          .Length()),
      ),
      reading('… at twice the samples', () =>
        n4((curve as unknown as { ToPolyline2D(n: number): { Length(): number } })
          .ToPolyline2D(samples * 2)
          .Length()),
      ),
      reading('ChordLength', () =>
        n4((curve as unknown as { ChordLength(): number }).ChordLength()),
      ),
    ];

    // Bounds and the vertex centroid come from the polyline the curve produces.
    const polyline = reading('Polyline2D.Bounds', () => {
      const line = (curve as unknown as {
        ToPolyline2D(n: number): { Bounds(): { Min: Point2D; Max: Point2D }; VertexCentroid(): Point2D };
      }).ToPolyline2D(samples);
      const bounds = line.Bounds();
      object.add(boundsOutline(bounds.Min, bounds.Max, 0x2f3a4c));
      const centre = line.VertexCentroid();
      object.add(marker2D(centre, palette.surfaceAlt, 0.05 * shape.scale + 0.02));
      return `${p2(bounds.Min)}..${p2(bounds.Max)}`;
    });
    readings.push(polyline);

    if (entry.closedCurve) {
      readings.push(
        reading('ClosureGap', () =>
          (curve as unknown as { ClosureGap(): number }).ClosureGap().toExponential(2),
        ),
        reading('IsProperlyClosed(1e-9)', () =>
          String((curve as unknown as { IsProperlyClosed(t: number): boolean }).IsProperlyClosed(1e-9)),
        ),
      );
    } else {
      readings.push(note('ClosureGap', 'not an IClosedCurve2D'));
    }

    if (entry.ring && traced.length >= 3) {
      const usable = traced.filter(
        p => Number.isFinite(p.X) && Number.isFinite(p.Y) && Math.abs(p.X) <= cap,
      );
      if (usable.length >= 3) {
        const ring = new Polygon2D(fromArray(usable));
        readings.push(
          reading('Polygon2D.Area', () => n4(ring.Area())),
          reading('Polygon2D.Perimeter', () => n4(ring.Perimeter())),
          reading('Polygon2D.Centroid', () => p2(ring.Centroid())),
          reading('Contains origin', () => String(ring.Contains(origin2D))),
        );
        // Probes on a circle, each joined to its nearest point on the ring.
        const probeCount = Math.round(params.probes);
        if (probeCount > 0) {
          const radius = Math.max(0.2, extentRadius(usable) * 1.25);
          const spokes: number[] = [];
          let far = new Point2D(radius, 0);
          for (let i = 0; i < probeCount; i++) {
            const a = (i / probeCount) * Math.PI * 2;
            const probe = new Point2D(Math.cos(a) * radius, Math.sin(a) * radius);
            const near = ring.ClosestPoint(probe);
            spokes.push(probe.X, probe.Y, 0.01, near.X, near.Y, 0.01);
            if (ring.DistanceTo(probe) > ring.DistanceTo(far)) far = probe;
          }
          object.add(segments(spokes, palette.surfaceAlt));
          readings.push(
            reading('farthest probe DistanceTo', () => n4(ring.DistanceTo(far))),
            reading('its ClosestPoint', () => p2(ring.ClosestPoint(far))),
          );
        }
      }
    }

    if (entry.arcLength) {
      const arc = curve as unknown as {
        ArcLength(): number;
        PointAtLength(l: number): Point2D;
        ParameterAtLength(l: number): number;
        PointAtLengthFraction(f: number): Point2D;
        LengthFraction(l: number): number;
        IsValidLength(l: number): boolean;
        ArcMidPoint(): Point2D;
      };
      readings.push(
        reading('ArcLength', () => n4(arc.ArcLength())),
        reading('ArcMidPoint', () => p2(arc.ArcMidPoint())),
        reading('PointAtLengthFraction(0.25)', () => p2(arc.PointAtLengthFraction(0.25))),
        reading('ParameterAtLength(half)', () => n4(arc.ParameterAtLength(arc.ArcLength() / 2))),
        reading('LengthFraction(half)', () => n4(arc.LengthFraction(arc.ArcLength() / 2))),
        reading('IsValidLength(2x)', () => String(arc.IsValidLength(arc.ArcLength() * 2))),
      );
      // The quarter, half and three-quarter marks along the arc, by length.
      const marks = reading('length marks', () => {
        const points = [0.25, 0.5, 0.75].map(f => arc.PointAtLengthFraction(f));
        object.add(dots2D(points, palette.accent, 8));
        return `${points.length} drawn`;
      });
      readings.push(marks);
    }

    return { object: fitPlanar(object), readings };
  },
});

function extentRadius(points: readonly Point2D[]): number {
  let r = 0;
  for (const p of points) r = Math.max(r, Math.hypot(p.X, p.Y));
  return r;
}

// ---------------------------------------------------------------------------

const demo: Demo = {
  title: 'Parametric curves',
  subtitle:
    'curves.{concepts,library}.plato · curves-shapes.{types,library}.plato · ' +
    'splines.{types,library}.plato · differential-geometry.types.plato',
  scenes: [splines, polar, spirals, arcs, space, frames, measures],
};

mountDemo(demo, SPATIAL);

// The page never imports this; it exists so an offline script can call every
// scene's `build` without a WebGL context.
export { demo };
