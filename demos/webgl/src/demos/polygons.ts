// Polygons — a scene catalog over stdlib/geometry/polygons.{types,library}.plato,
// the named planar regions of planar.{types,library}.plato, the closed plane
// curves of curves-shapes.{types,library}.plato, the ring kernels in
// geometry.library.plato, and triangulation.library.plato.
//
// Every ring, every measure and every predicate below comes from a member of
// `src/plato/plato.g.ts`. This file builds inputs (point lists, stars, named
// shapes, regions with holes), calls the generated members, and repacks the
// answers into Three.js lines and point clouds. No formula the stdlib defines is
// re-derived here — where a generated member still throws or returns NaN, the
// status line says so by name rather than substituting a hand-rolled answer.
//
// The planar scenes take the page's orthographic, grid-free, non-spinning
// camera; the ring in space overrides it with an orbitable perspective one.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { fromArray, polygon2DLines, polygon3DLines, toArray } from '../shared/mesh.js';
import { palette, type ViewerOptions } from '../shared/viewer.js';
import {
  Angle,
  AngleInterval,
  Annulus,
  Bounds2D,
  ButterflyCurve2D,
  Capsule2D,
  Cardioid2D,
  Circle,
  CircularArc2D,
  CircularSector,
  CircularSegment,
  ConvexPolygon2D,
  Direction2D,
  Ellipse,
  EllipticalArc2D,
  Epicycloid2D,
  Epitrochoid2D,
  Hypocycloid2D,
  Hypotrochoid2D,
  Lemniscate2D,
  Limacon2D,
  Lissajous2D,
  Number2,
  OrientedBox2D,
  Parallelogram2D,
  Plane,
  Point2D,
  Point3D,
  Polygon2D,
  Polygon3D,
  PolygonSet2D,
  PolygonWithHoles2D,
  Quad2D,
  Rect2D,
  RegularPolygon,
  RegularStar2D,
  RoseCurve2D,
  RoundedRect2D,
  Size2D,
  SuperEllipse,
  Superformula2D,
  Triangle2D,
  TriangleMesh2D,
  Vector2D,
  type IArray,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// A member that throws is a gap in the emitted library, not a fact about the
// geometry, so the status line keeps the member's name and the failure instead
// of quietly computing the number some other way. A member that returns NaN is
// the same kind of finding, and the closed-curve scene reports it the same way:
// the four rolling-circle curves in `curves-shapes.library.plato` multiply a
// Number by an Angle, which the emitted prelude has no overload for.

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

const n4 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(4);
const p2 = (p: Point2D): string => `(${n4(p.X)}, ${n4(p.Y)})`;
const p3 = (p: Point3D): string => `(${n4(p.X)}, ${n4(p.Y)}, ${n4(p.Z)})`;

/**
 * A scene whose `build` also reports what it measured. The shell calls `status`
 * right after `build` on every rebuild, so the readings the latest build took
 * are the ones the sidebar shows.
 */
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
      return fitToView(built.object);
    },
    status(): string {
      return latest.map(r => `${r.label} ${r.value}`).join('  ·  ');
    },
  };
}

/**
 * The shell's orthographic camera is sized by height alone, so a narrow stage
 * would clip a planar scene sideways. Scale the scene to whichever half-extent
 * is smaller, refreshed per frame because the stage is resizable. A no-op under
 * the perspective camera the scene in space asks for.
 */
function fitToView(object: THREE.Object3D): THREE.Object3D {
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
    object.scale.setScalar(Math.min(1, (half * 0.96) / extent));
  };
  object.add(probe);
  return object;
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

/** A small axis cross, for marking a computed centroid. */
function marker(x: number, y: number, color: number, size = 0.08): THREE.LineSegments {
  return segments([x - size, y, 0.02, x + size, y, 0.02, x, y - size, 0.02, x, y + size, 0.02], color);
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

function dots(samples: { x: number; y: number; inside: boolean }[]): THREE.Points {
  const positions: number[] = [];
  const colors: number[] = [];
  const inside = new THREE.Color(palette.accent);
  const outside = new THREE.Color(0x35435c);
  for (const s of samples) {
    positions.push(s.x, s.y, -0.01);
    const c = s.inside ? inside : outside;
    colors.push(c.r, c.g, c.b);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  geometry.setAttribute('color', new THREE.Float32BufferAttribute(colors, 3));
  return new THREE.Points(
    geometry,
    new THREE.PointsMaterial({ size: 4, sizeAttenuation: false, vertexColors: true }),
  );
}

function vertexDots(points: Point2D[], color: number, size = 6): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.X, p.Y, 0.01);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(geometry, new THREE.PointsMaterial({ color, size, sizeAttenuation: false }));
}

/** A generated TriangleMesh2D as filled triangles plus its edges. */
function triangleMesh2D(mesh: TriangleMesh2D, z = -0.02): THREE.Object3D {
  const positions = toArray(mesh.Positions);
  const faces = toArray(mesh.Faces);
  const filled: number[] = [];
  const wire: number[] = [];
  for (const f of faces) {
    const corners = [positions[f.A.Value], positions[f.B.Value], positions[f.C.Value]];
    for (const c of corners) filled.push(c.X, c.Y, z);
    for (let i = 0; i < 3; i++) {
      const a = corners[i];
      const b = corners[(i + 1) % 3];
      wire.push(a.X, a.Y, z + 0.01, b.X, b.Y, z + 0.01);
    }
  }
  const solid = new THREE.BufferGeometry();
  solid.setAttribute('position', new THREE.Float32BufferAttribute(filled, 3));
  const group = new THREE.Group();
  group.add(
    new THREE.Mesh(
      solid,
      new THREE.MeshBasicMaterial({ color: palette.surface, side: THREE.DoubleSide }),
    ),
    segments(wire, 0x0d1117),
  );
  return group;
}

// ---------------------------------------------------------------------------
// Input construction
//
// Building the point lists a generated member consumes is demo work; the
// measures taken from them are not.

function ringOf(points: Point2D[]): Polygon2D {
  return new Polygon2D(fromArray(points));
}

/** Stored the other way round, so the generated Winding and repair have work to do. */
function storedBackwards(polygon: Polygon2D): Polygon2D {
  return ringOf(toArray(polygon.Points).reverse());
}

function starRing(pointCount: number, outer: number, inset: number): Polygon2D {
  const star = new RegularStar2D(new Point2D(0, 0), outer, outer * inset, pointCount, new Angle(0));
  return new Polygon2D(star.RegularStarVertices());
}

/**
 * A hexagon whose vertices — and therefore whose edge midpoints — are exact in
 * binary floating point, so the exact collinearity test in the repair bodies
 * sees a genuinely flat corner.
 */
function dyadicHexagon(): Polygon2D {
  return ringOf(
    ([
      [-1.25, -0.75],
      [0, -1.25],
      [1.25, -0.75],
      [1.25, 0.75],
      [0, 1.25],
      [-1.25, 0.75],
    ] as [number, number][]).map(([x, y]) => new Point2D(x, y)),
  );
}

function regularRing(sides: number, radius: number, cx = 0, cy = 0, turn = 0): Polygon2D {
  const shape = new RegularPolygon(new Point2D(cx, cy), radius, sides, new Angle(turn));
  return new Polygon2D(shape.RegularPolygonVertices());
}

/** `holes` regular rings on a circle inside `boundary`, wound clockwise as the type asks. */
function regionWithHoles(
  boundary: Polygon2D,
  holes: number,
  radius: number,
  orbit: number,
): PolygonWithHoles2D {
  const rings: Polygon2D[] = [];
  for (let i = 0; i < holes; i++) {
    const a = (i / holes) * Math.PI * 2 + Math.PI / 2;
    rings.push(storedBackwards(regularRing(6, radius, Math.cos(a) * orbit, Math.sin(a) * orbit)));
  }
  return new PolygonWithHoles2D(boundary, fromArray(rings));
}

/** Sample an axis-aligned grid and ask the generated predicate about each point. */
function sampleGrid(
  half: number,
  resolution: number,
  contains: (p: Point2D) => boolean,
): { samples: { x: number; y: number; inside: boolean }[]; inside: number } {
  const samples: { x: number; y: number; inside: boolean }[] = [];
  let count = 0;
  for (let j = 0; j < resolution; j++) {
    for (let i = 0; i < resolution; i++) {
      const x = -half + (2 * half * (i + 0.5)) / resolution;
      const y = -half + (2 * half * (j + 0.5)) / resolution;
      const inside = contains(new Point2D(x, y));
      if (inside) count++;
      samples.push({ x, y, inside });
    }
  }
  return { samples, inside: count };
}

const RADIUS = 1.35;

// ---------------------------------------------------------------------------
// The named-shape catalog
//
// `planar.types.plato` declares a dozen parametric planar regions and
// `curves-shapes.types.plato` a shelf of named plane curves. Almost none of them
// stores a vertex list: a `Circle` is a centre and a radius, an `Ellipse` a
// centre and a semi-axis pair. So a ring for one of them is built the way the
// stdlib builds rings — out of another generated member. `RegularPolygon`,
// `Triangle2D`, `Quad2D` and `Parallelogram2D` hand over their corners directly;
// the curved regions are sampled with the arc and curve evaluators
// (`CircularArc2D`, `EllipticalArc2D`, `Superformula2D`) that describe their own
// boundaries; `OrientedBox2D` gives up its corners through the `ISupport2D`
// member `Support`.
//
// The point of the exercise is the pairing: every one of these types carries its
// own closed-form `Area`, `Perimeter`, `Centroid` and `IsoperimetricQuotient`,
// and the ring built from it carries the shoelace ones. Both are reported, and
// the gap between them is the sampling error of the ring — except where it is
// not, which is the interesting case.

const TURN = Math.PI * 2;

/** An Angle from a fraction of a turn, the unit `planar.library.plato` works in. */
function turns(fraction: number): Angle {
  return new Angle(fraction * TURN);
}

function sweepOf(from: number, to: number): AngleInterval {
  return new AngleInterval(turns(from), turns(to));
}

/**
 * Points along a `CircularArc2D`. `ICurve2D.Sample` divides the parameter into
 * `count - 1` steps, so a single sample is a division by zero — never ask for
 * fewer than two.
 */
function arcPoints(
  center: Point2D,
  radius: number,
  from: number,
  to: number,
  count: number,
): Point2D[] {
  const arc = new CircularArc2D(center, radius, sweepOf(from, to));
  return toArray(arc.Sample(Math.max(2, Math.round(count))));
}

/** Everything `ICurve2D` reaches in these scenes: the periodic sampler over [0, 1). */
interface PeriodicCurve {
  SamplePeriodic(count: number): IArray<Point2D>;
}

/** A closed ring from a periodic sampler: `count` distinct points, no repeated seam. */
function periodicRing(curve: PeriodicCurve, count: number): Polygon2D {
  return new Polygon2D(curve.SamplePeriodic(Math.max(3, Math.round(count))));
}

interface Measured {
  Area(): number;
  Perimeter(): number;
  Centroid(): Point2D;
  IsoperimetricQuotient(): number;
}

/** The four measures every `IArea` planar region in `planar.types.plato` carries. */
function closedForm(name: string, shape: Measured): Reading[] {
  return [
    reading(`${name}.Area`, () => n4(shape.Area())),
    reading(`${name}.Perimeter`, () => n4(shape.Perimeter())),
    reading(`${name}.Centroid`, () => p2(shape.Centroid())),
    reading(`${name}.IsoperimetricQuotient`, () => n4(shape.IsoperimetricQuotient())),
  ];
}

interface NamedShape {
  /** The generated type, as the readings name it. */
  name: string;
  /** The generated member the ring's vertices came out of. */
  source: string;
  /** Boundary first, then holes, stored the other way round as the types ask. */
  rings: Polygon2D[];
  /** The measures the type computes for itself. */
  closed: Reading[];
  /** The type's own `Bounds`, where it declares one. */
  bounds?: Bounds2D;
  /** The type's own containment predicate. */
  contains?: (point: Point2D) => boolean;
  /** The type's own nearest-point member, where `INearestPoint2D` gave it one. */
  closest?: (point: Point2D) => Point2D;
  /** Whether `ConvexPolygon2D`'s convexity precondition holds for the ring. */
  convex: boolean;
}

/**
 * The `planar.types.plato` regions, in the order the `Shape` select lists them.
 * `u` is the scene's single shape parameter, 0..1; what it means per shape is in
 * each scene's description. `samples` is the arc sample count, ignored by the
 * four straight-edged types.
 */
const REGION_LABELS = [
  'Circle',
  'Ellipse',
  'Annulus',
  'Sector',
  'Segment',
  'Capsule',
  'Superellipse',
  'RoundedRect',
  'OrientedBox',
  'RegularPolygon',
  'Triangle',
  'Quad',
  'Parallelogram',
];

function regionShape(index: number, u: number, samples: number): NamedShape {
  const r = RADIUS;
  const origin = new Point2D(0, 0);
  const n = Math.max(8, Math.round(samples));

  switch (index) {
    case 0: {
      const shape = new Circle(origin, r * (0.45 + 0.55 * u));
      return {
        name: 'Circle',
        source: 'CircularArc2D.SamplePeriodic',
        rings: [periodicRing(new CircularArc2D(shape.Center, shape.Radius, sweepOf(0, 1)), n)],
        closed: closedForm('Circle', shape),
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        closest: p => shape.ClosestPoint(p),
        convex: true,
      };
    }
    case 1: {
      const axes = new Number2(r, r * (0.15 + 0.85 * u));
      const shape = new Ellipse(origin, axes, turns(0.06));
      const arc = new EllipticalArc2D(shape.Center, axes, shape.Rotation, sweepOf(0, 1));
      return {
        name: 'Ellipse',
        source: 'EllipticalArc2D.SamplePeriodic',
        rings: [periodicRing(arc, n)],
        closed: closedForm('Ellipse', shape),
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        convex: true,
      };
    }
    case 2: {
      const shape = new Annulus(origin, r * (0.05 + 0.8 * u), r);
      const outer = periodicRing(new CircularArc2D(shape.Center, shape.OuterRadius, sweepOf(0, 1)), n);
      const inner = periodicRing(new CircularArc2D(shape.Center, shape.InnerRadius, sweepOf(0, 1)), n);
      return {
        name: 'Annulus',
        source: 'CircularArc2D.SamplePeriodic, twice',
        rings: [outer, storedBackwards(inner)],
        closed: closedForm('Annulus', shape),
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        convex: false,
      };
    }
    case 3: {
      const span = 0.05 + 0.9 * u;
      const shape = new CircularSector(new Circle(origin, r), sweepOf(0.25 - span / 2, 0.25 + span / 2));
      const arc = arcPoints(origin, r, 0.25 - span / 2, 0.25 + span / 2, n);
      return {
        name: 'CircularSector',
        source: 'CircularArc2D.Sample, closed on the centre',
        rings: [ringOf([shape.Circle.Center, ...arc])],
        closed: closedForm('CircularSector', shape),
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        convex: false,
      };
    }
    case 4: {
      const span = 0.05 + 0.9 * u;
      const shape = new CircularSegment(new Circle(origin, r), sweepOf(0.25 - span / 2, 0.25 + span / 2));
      return {
        name: 'CircularSegment',
        source: 'CircularArc2D.Sample, closed on the chord',
        rings: [ringOf(arcPoints(origin, r, 0.25 - span / 2, 0.25 + span / 2, n))],
        closed: [
          ...closedForm('CircularSegment', shape),
          reading('CircularSegment.ChordStart', () => p2(shape.ChordStart())),
          reading('CircularSegment.ChordEnd', () => p2(shape.ChordEnd())),
        ],
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        convex: true,
      };
    }
    case 5: {
      const half = r * (0.05 + 0.6 * u);
      const radius = r * 0.35;
      const shape = new Capsule2D(new Point2D(-half, 0), new Point2D(half, 0), radius);
      const cap = Math.max(2, Math.round(n / 2));
      return {
        name: 'Capsule2D',
        source: 'two CircularArc2D.Sample half-turns',
        rings: [
          ringOf([
            ...arcPoints(shape.B, radius, -0.25, 0.25, cap),
            ...arcPoints(shape.A, radius, 0.25, 0.75, cap),
          ]),
        ],
        closed: [
          ...closedForm('Capsule2D', shape),
          reading('Capsule2D.SpineLength', () => n4(shape.SpineLength())),
        ],
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        closest: p => shape.ClosestPoint(p),
        convex: true,
      };
    }
    case 6: {
      // The Gielis superformula at m = 4 with all three exponents equal IS the
      // Lame curve the SuperEllipse region is bounded by, so one type's sampler
      // draws the other type's boundary and their measures can be compared.
      const exponent = 1 + 7 * u;
      const axes = new Number2(r, r * 0.7);
      const shape = new SuperEllipse(origin, axes, turns(0), exponent);
      const formula = new Superformula2D(4, exponent, exponent, exponent, axes.X, axes.Y);
      return {
        name: 'SuperEllipse',
        source: 'Superformula2D.SamplePeriodic at Symmetry 4',
        rings: [periodicRing(formula, n)],
        closed: [
          ...closedForm('SuperEllipse', shape),
          reading('Superformula2D.RadiusAt(0)', () => n4(formula.RadiusAt(turns(0)))),
        ],
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        convex: true,
      };
    }
    case 7: {
      const size = new Size2D(2 * r, 1.3 * r);
      const shape = new RoundedRect2D(new Rect2D(origin, size), u * 0.65 * r);
      const radius = shape.EffectiveCornerRadius();
      const hx = size.Width / 2 - radius;
      const hy = size.Height / 2 - radius;
      const quarter = Math.max(2, Math.round(n / 4));
      const corners: [number, number, number][] = [
        [hx, hy, 0],
        [-hx, hy, 0.25],
        [-hx, -hy, 0.5],
        [hx, -hy, 0.75],
      ];
      const points: Point2D[] = [];
      for (const [cx, cy, start] of corners) {
        points.push(...arcPoints(new Point2D(cx, cy), radius, start, start + 0.25, quarter));
      }
      // At zero corner radius every quarter arc collapses onto the corner point;
      // the generated repair member drops the repeats rather than a hand-rolled
      // special case.
      return {
        name: 'RoundedRect2D',
        source: 'four CircularArc2D.Sample quarters, RemoveDuplicateVertices',
        rings: [ringOf(points).RemoveDuplicateVertices()],
        closed: [
          ...closedForm('RoundedRect2D', shape),
          reading('RoundedRect2D.EffectiveCornerRadius', () => n4(shape.EffectiveCornerRadius())),
        ],
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        convex: true,
      };
    }
    case 8: {
      const rotation = turns(0.06);
      const size = new Size2D(2 * r * (0.35 + 0.65 * u), 1.2 * r);
      const shape = new OrientedBox2D(origin, size, rotation);
      // Support in a direction returns the extreme point of the region, and for
      // a box that is a corner: the four local-frame diagonals name all four.
      const cos = Math.cos(rotation.Radians);
      const sin = Math.sin(rotation.Radians);
      const hw = size.Width / 2;
      const hh = size.Height / 2;
      const corners = ([[1, 1], [-1, 1], [-1, -1], [1, -1]] as [number, number][]).map(([sx, sy]) =>
        shape.Support(
          new Direction2D(new Vector2D(sx * hw * cos - sy * hh * sin, sx * hw * sin + sy * hh * cos)),
        ),
      );
      return {
        name: 'OrientedBox2D',
        source: 'OrientedBox2D.Support on the four local diagonals',
        rings: [ringOf(corners)],
        closed: closedForm('OrientedBox2D', shape),
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        closest: p => shape.ClosestPoint(p),
        convex: true,
      };
    }
    case 9: {
      const shape = new RegularPolygon(origin, r, 3 + Math.round(u * 9), turns(0));
      return {
        name: 'RegularPolygon',
        source: 'RegularPolygon.RegularPolygonVertices',
        rings: [new Polygon2D(shape.RegularPolygonVertices())],
        closed: closedForm('RegularPolygon', shape),
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        convex: true,
      };
    }
    case 10: {
      const shape = new Triangle2D(
        new Point2D(-r, -0.72 * r),
        new Point2D(r, -0.58 * r),
        new Point2D((2 * u - 1) * r, r),
      );
      return {
        name: 'Triangle2D',
        source: 'Triangle2D.Points',
        rings: [new Polygon2D(shape.Points())],
        closed: [
          ...closedForm('Triangle2D', shape),
          reading('Triangle2D.Circumcenter', () => p2(shape.Circumcenter())),
          reading('Triangle2D.Incenter', () => p2(shape.Incenter())),
        ],
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        closest: p => shape.ClosestPoint(p),
        convex: true,
      };
    }
    case 11: {
      const shape = new Quad2D(
        new Point2D(-r, -0.8 * r),
        new Point2D(r, -0.6 * r),
        new Point2D(r * (0.35 + 0.6 * u), 0.85 * r),
        new Point2D(-0.8 * r, r),
      );
      return {
        name: 'Quad2D',
        source: 'Quad2D.Points',
        rings: [new Polygon2D(shape.Points())],
        closed: closedForm('Quad2D', shape),
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        convex: false,
      };
    }
    default: {
      const shape = new Parallelogram2D(
        new Point2D(-r, -0.75 * r),
        new Vector2D(1.9 * r, 0.15 * r),
        new Vector2D((2 * u - 1) * r, 1.4 * r),
      );
      return {
        name: 'Parallelogram2D',
        source: 'Parallelogram2D.Corners',
        rings: [new Polygon2D(shape.Corners())],
        closed: closedForm('Parallelogram2D', shape),
        bounds: shape.Bounds(),
        contains: p => shape.Contains(p),
        convex: true,
      };
    }
  }
}

/** The half-extent a sampling grid needs to cover a shape, from the ring's own Bounds. */
function ringHalfExtent(ring: Polygon2D): number {
  const bounds = ring.Bounds();
  return (
    Math.max(
      Math.abs(bounds.Min.X),
      Math.abs(bounds.Max.X),
      Math.abs(bounds.Min.Y),
      Math.abs(bounds.Max.Y),
    ) * 1.12
  );
}

// ---------------------------------------------------------------------------
// The closed plane curves
//
// `curves-shapes.types.plato` declares the named plane curves as evaluators, not
// as regions: a `Cardioid2D` is one number and an `Eval`. `SamplePeriodic` turns
// one into a ring, and from there the polygon measures apply — including the
// ones a self-crossing curve makes nonsense of, which is the point of reporting
// `IsSimple` and `SelfIntersectionCount` beside them.
//
// Five entries below do not evaluate, and they are listed anyway so the page
// names them. Four share one cause: `Number.Multiply(Angle)` has no overload in
// the emitted prelude, so the rolling-circle bodies multiply a number by an
// Angle object and get NaN — which `Epicycloid2D` and `Epitrochoid2D` then feed
// to `Angle.CirclePoint`, turning the NaN into a missing-member throw. The
// fifth, `Lemniscate2D`, is a fact about the curve rather than the writer: its
// polar radius is the square root of cos(2 theta), which is negative over half
// the canonical parameter domain.

const CURVE_LABELS = [
  'Cardioid2D',
  'Limacon2D',
  'RoseCurve2D',
  'Superformula2D',
  'ButterflyCurve2D',
  'Lissajous2D',
  'Lemniscate2D',
  'Epicycloid2D',
  'Hypocycloid2D',
  'Epitrochoid2D',
  'Hypotrochoid2D',
];

interface NamedCurve {
  name: string;
  /** The stored parameters, for the status line. */
  detail: string;
  sample(count: number): Point2D[];
  /** `IClosedCurve2D` gives some of them a self-check; the open ones have none. */
  closure?: () => string;
}

function namedCurve(index: number, u: number): NamedCurve {
  const r = RADIUS;
  const sampler =
    (curve: PeriodicCurve) =>
    (count: number): Point2D[] =>
      toArray(curve.SamplePeriodic(Math.max(3, Math.round(count))));

  switch (index) {
    case 0: {
      const curve = new Cardioid2D(r * (0.15 + 0.35 * u));
      return {
        name: 'Cardioid2D',
        detail: `Radius ${n4(curve.Radius)}`,
        sample: sampler(curve),
        closure: () => n4(curve.ClosureGap()),
      };
    }
    case 1: {
      const amplitude = r * 0.6;
      const curve = new Limacon2D(u * 2 * amplitude, amplitude);
      return {
        name: 'Limacon2D',
        detail: `Offset ${n4(curve.Offset)} Amplitude ${n4(curve.Amplitude)}`,
        sample: sampler(curve),
        closure: () => n4(curve.ClosureGap()),
      };
    }
    case 2: {
      const curve = new RoseCurve2D(r, 2 + Math.round(u * 8));
      return {
        name: 'RoseCurve2D',
        detail: `PetalFrequency ${curve.PetalFrequency}`,
        sample: sampler(curve),
      };
    }
    case 3: {
      // XRadius and YRadius are DIVISORS inside the radial formula, not a size:
      // shrinking them inflates the bracket and collapses the curve to a point.
      // Left at one, and the scene's fit-to-view handles the resulting scale.
      const curve = new Superformula2D(3 + Math.round(u * 9), 1, 7, 8, 1, 1);
      return {
        name: 'Superformula2D',
        detail: `Symmetry ${curve.Symmetry}, exponents 1/7/8`,
        sample: sampler(curve),
      };
    }
    case 4: {
      const curve = new ButterflyCurve2D(r * (0.15 + 0.25 * u));
      return {
        name: 'ButterflyCurve2D',
        detail: `Scale ${n4(curve.Scale)}`,
        sample: sampler(curve),
        closure: () => n4(curve.ClosureGap()),
      };
    }
    case 5: {
      const curve = new Lissajous2D(r, r, 3, 2 + Math.round(u * 5), turns(0.25));
      return {
        name: 'Lissajous2D',
        detail: `frequencies ${curve.XFrequency}:${curve.YFrequency}`,
        sample: sampler(curve),
      };
    }
    case 6: {
      const curve = new Lemniscate2D(r * (0.5 + 0.5 * u));
      return {
        name: 'Lemniscate2D',
        detail: `Scale ${n4(curve.Scale)}`,
        sample: sampler(curve),
        closure: () => n4(curve.ClosureGap()),
      };
    }
    case 7: {
      const curve = new Epicycloid2D(r * 0.7, r * 0.7 * (0.1 + 0.4 * u));
      return {
        name: 'Epicycloid2D',
        detail: `FixedRadius ${n4(curve.FixedRadius)} RollingRadius ${n4(curve.RollingRadius)}`,
        sample: sampler(curve),
      };
    }
    case 8: {
      const curve = new Hypocycloid2D(r, r * (0.1 + 0.4 * u));
      return {
        name: 'Hypocycloid2D',
        detail: `FixedRadius ${n4(curve.FixedRadius)} RollingRadius ${n4(curve.RollingRadius)}`,
        sample: sampler(curve),
      };
    }
    case 9: {
      const curve = new Epitrochoid2D(r * 0.7, r * 0.7 * (0.1 + 0.4 * u), r * 0.3);
      return {
        name: 'Epitrochoid2D',
        detail: `PoleDistance ${n4(curve.PoleDistance)}`,
        sample: sampler(curve),
      };
    }
    default: {
      const curve = new Hypotrochoid2D(r, r * (0.1 + 0.4 * u), r * 0.3);
      return {
        name: 'Hypotrochoid2D',
        detail: `PoleDistance ${n4(curve.PoleDistance)}`,
        sample: sampler(curve),
      };
    }
  }
}

const isFinitePoint = (p: Point2D): boolean => Number.isFinite(p.X) && Number.isFinite(p.Y);

// ---------------------------------------------------------------------------
// Scenes

const measures = sceneOf({
  id: 'measures',
  title: 'Ring measures',
  description:
    'Area, SignedArea, Perimeter, Centroid and Bounds of a Polygon2D built from ' +
    'RegularStar2D.RegularStarVertices. The star\'s own closed-form Area and Perimeter are shown ' +
    'beside the shoelace answers: they agree, and at inset = 1 the ring is a regular 2n-gon.',
  plato: [
    'RegularStar2D.RegularStarVertices',
    'RegularStar2D.Area',
    'RegularStar2D.Perimeter',
    'Polygon2D.Area',
    'Polygon2D.SignedArea',
    'Polygon2D.Perimeter',
    'Polygon2D.Centroid',
    'Polygon2D.VertexCentroid',
    'Polygon2D.Bounds',
    'Polygon2D.IsoperimetricQuotient',
    'Polygon2D.Translate',
  ],
  controls: [
    { key: 'points', label: 'Star points', kind: 'slider', min: 3, max: 12, step: 1, def: 5 },
    { key: 'inset', label: 'Inner radius', kind: 'slider', min: 0.15, max: 1, step: 0.01, def: 0.45 },
    { key: 'shift', label: 'Shift X', kind: 'slider', min: -0.8, max: 0.8, step: 0.01, def: 0 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.points);
    const star = new RegularStar2D(
      new Point2D(0, 0),
      RADIUS,
      RADIUS * params.inset,
      count,
      new Angle(0),
    );
    const ring = new Polygon2D(star.RegularStarVertices()).Translate(new Vector2D(params.shift, 0));
    const centroid = ring.Centroid();
    const bounds = ring.Bounds();

    const object = new THREE.Group();
    object.add(boundsOutline(bounds.Min, bounds.Max, 0x2f3a4c));
    object.add(lines(polygon2DLines(ring), palette.line));
    object.add(vertexDots(toArray(ring.Points), palette.line, 5));
    object.add(marker(centroid.X, centroid.Y, palette.surfaceAlt));

    return {
      object,
      readings: [
        note('vertices', String(ring.Points.Count())),
        reading('Area', () => n4(ring.Area())),
        reading('SignedArea', () => n4(ring.SignedArea())),
        reading('Perimeter', () => n4(ring.Perimeter())),
        reading('Centroid', () => p2(centroid)),
        reading('VertexCentroid', () => p2(ring.VertexCentroid())),
        reading('Bounds', () => `${n4(ring.BoundsWidth())} x ${n4(ring.BoundsHeight())}`),
        reading('IsoperimetricQuotient', () => n4(ring.IsoperimetricQuotient())),
        reading('RegularStar2D.Area', () => n4(star.Area())),
        reading('RegularStar2D.Perimeter', () => n4(star.Perimeter())),
      ],
    };
  },
});

const gallery = sceneOf({
  id: 'named-shapes',
  title: 'Named shape gallery',
  description:
    'The parametric planar regions of planar.types.plato, each turned into a Polygon2D ring by a ' +
    'generated member of its own — RegularPolygonVertices, Points, Corners, Support on the four local ' +
    'diagonals, or the arc and superformula samplers that describe its boundary. Every type computes its ' +
    'own Area, Perimeter, Centroid and IsoperimetricQuotient in closed form; those are shown beside the ' +
    'shoelace answers the ring gives, and the gap between them is the ring\'s sampling error. The shape ' +
    'parameter means, in the order of the list: radius, axis ratio, inner radius, sweep, sweep, spine ' +
    'length, Lame exponent, corner radius, aspect ratio, side count, apex, corner, shear. The grey box ' +
    'is the type\'s own Bounds — CircularSector declares its conservative, and reports the whole circle.',
  plato: [
    'CircularArc2D.Sample',
    'CircularArc2D.SamplePeriodic',
    'EllipticalArc2D.SamplePeriodic',
    'Superformula2D.SamplePeriodic',
    'RegularPolygon.RegularPolygonVertices',
    'OrientedBox2D.Support',
    'Triangle2D.Points',
    'Quad2D.Points',
    'Parallelogram2D.Corners',
    'Circle.Area',
    'Ellipse.Perimeter',
    'Annulus.Area',
    'CircularSector.Centroid',
    'CircularSegment.ChordStart',
    'Capsule2D.SpineLength',
    'SuperEllipse.Area',
    'RoundedRect2D.EffectiveCornerRadius',
    'Triangle2D.Circumcenter',
    'Polygon2D.Area',
    'Polygon2D.Perimeter',
    'Polygon2D.Centroid',
    'Polygon2D.Bounds',
    'Polygon2D.IsoperimetricQuotient',
    'PolygonWithHoles2D.Area',
    'ConvexPolygon2D.Area',
  ],
  controls: [
    { key: 'shape', label: 'Shape', kind: 'select', options: REGION_LABELS, def: 1 },
    { key: 'param', label: 'Shape parameter', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0.55 },
    { key: 'samples', label: 'Arc samples', kind: 'slider', min: 12, max: 96, step: 4, def: 48 },
  ],
  build(params: Params): Built {
    const shape = regionShape(Math.round(params.shape), params.param, params.samples);
    const ring = shape.rings[0];
    const holes = shape.rings.slice(1);
    const centroid = ring.Centroid();

    const object = new THREE.Group();
    if (shape.bounds) object.add(boundsOutline(shape.bounds.Min, shape.bounds.Max, 0x2f3a4c));
    object.add(lines(polygon2DLines(ring), palette.line));
    object.add(vertexDots(toArray(ring.Points), palette.line, 5));
    for (const hole of holes) {
      object.add(lines(polygon2DLines(hole), palette.surfaceAlt));
      object.add(vertexDots(toArray(hole.Points), palette.surfaceAlt, 4));
    }
    if (Number.isFinite(centroid.X) && Number.isFinite(centroid.Y)) {
      object.add(marker(centroid.X, centroid.Y, palette.surfaceAlt, 0.06));
    }

    const readings: Reading[] = [
      note('shape', `${shape.name} from ${shape.source}`),
      note('ring vertices', String(ring.Points.Count())),
      reading('Polygon2D.Area', () => n4(ring.Area())),
      reading('Polygon2D.Perimeter', () => n4(ring.Perimeter())),
      reading('Polygon2D.Centroid', () => p2(centroid)),
      reading('Polygon2D.Bounds', () => `${n4(ring.BoundsWidth())} x ${n4(ring.BoundsHeight())}`),
      reading('Polygon2D.IsoperimetricQuotient', () => n4(ring.IsoperimetricQuotient())),
      ...shape.closed,
    ];
    if (shape.bounds) {
      const box = shape.bounds;
      readings.push(
        reading(
          `${shape.name}.Bounds`,
          () => `${n4(box.Max.X - box.Min.X)} x ${n4(box.Max.Y - box.Min.Y)}`,
        ),
      );
    }
    if (holes.length > 0) {
      const region = new PolygonWithHoles2D(ring, fromArray(holes));
      readings.push(
        reading('PolygonWithHoles2D.Area', () => n4(region.Area())),
        reading('PolygonWithHoles2D.Perimeter', () => n4(region.Perimeter())),
      );
    }
    if (shape.convex) {
      // The type declares itself convex, so the ring satisfies ConvexPolygon2D's
      // precondition and its fan triangulation applies without an ear search.
      const convex = new ConvexPolygon2D(ring.Points);
      readings.push(
        reading('ConvexPolygon2D.Area', () => n4(convex.Area())),
        reading('ConvexPolygon2D.TriangulationFaceCount', () =>
          String(convex.TriangulationFaceCount()),
        ),
      );
    }
    return { object, readings };
  },
});

const closedCurves = sceneOf({
  id: 'closed-curves',
  title: 'Closed plane curves',
  description:
    'The named plane curves of curves-shapes.types.plato sampled into rings with ICurve2D.SamplePeriodic ' +
    'and measured as polygons. Several of them cross themselves, so IsSimple and SelfIntersectionCount ' +
    'are reported beside Area: a rose\'s petals overlap, a Lissajous figure encloses zero signed area, and ' +
    'the shoelace number stops meaning enclosed region. Five entries do not evaluate and are listed so the ' +
    'page names them: the four rolling-circle curves hit an Number-times-Angle multiplication the emitted ' +
    'prelude has no overload for, and Lemniscate2D is undefined wherever cos(2 theta) is negative.',
  plato: [
    'Cardioid2D.Eval',
    'Limacon2D.Eval',
    'RoseCurve2D.RadiusAt',
    'Superformula2D.RadiusAt',
    'ButterflyCurve2D.Eval',
    'Lissajous2D.Eval',
    'Lemniscate2D.Eval',
    'Epicycloid2D.Eval',
    'Hypocycloid2D.Eval',
    'Epitrochoid2D.Eval',
    'Hypotrochoid2D.Eval',
    'IClosedCurve2D.ClosureGap',
    'ICurve2D.SamplePeriodic',
    'Polygon2D.Area',
    'Polygon2D.SignedArea',
    'Polygon2D.Perimeter',
    'Polygon2D.IsSimple',
    'Polygon2D.SelfIntersectionCount',
  ],
  controls: [
    { key: 'curve', label: 'Curve', kind: 'select', options: CURVE_LABELS, def: 0 },
    { key: 'param', label: 'Curve parameter', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0.5 },
    { key: 'samples', label: 'Samples', kind: 'slider', min: 24, max: 192, step: 8, def: 96 },
  ],
  build(params: Params): Built {
    const curve = namedCurve(Math.round(params.curve), params.param);
    const count = Math.round(params.samples);

    const object = new THREE.Group();
    let points: Point2D[] = [];
    const evaluated = reading(`${curve.name}.SamplePeriodic`, () => {
      points = curve.sample(count);
      const bad = points.filter(p => !isFinitePoint(p)).length;
      return bad > 0 ? `${count} samples, ${bad} NaN` : `${count} samples`;
    });

    const finite = points.filter(isFinitePoint);
    const whole = points.length > 0 && finite.length === points.length;
    const ring = whole ? ringOf(points) : null;

    if (ring) {
      object.add(lines(polygon2DLines(ring), ring.IsSimple() ? palette.line : palette.surfaceAlt));
      object.add(vertexDots(points, palette.line, 4));
      const bounds = ring.Bounds();
      object.add(boundsOutline(bounds.Min, bounds.Max, 0x2f3a4c));
      const centroid = ring.Centroid();
      if (Number.isFinite(centroid.X) && Number.isFinite(centroid.Y)) {
        object.add(marker(centroid.X, centroid.Y, palette.accent, 0.06));
      }
    } else if (finite.length > 0) {
      // Part of the parameter domain evaluated; the rest is NaN. The samples that
      // came back are shown, but no ring is closed over them and no measure is
      // taken from a point set the curve did not actually produce.
      object.add(vertexDots(finite, palette.surfaceAlt, 5));
    } else {
      object.add(marker(0, 0, 0x51617d, 0.2));
    }

    /** A polygon measure, or the reason it was not taken. */
    const measure = (label: string, produce: (r: Polygon2D) => string): Reading =>
      ring
        ? reading(label, () => produce(ring))
        : note(label, points.length > 0 ? 'NOT TAKEN (the sample set contains NaN)' : 'NOT TAKEN (no samples)');

    const readings: Reading[] = [
      note('curve', `${curve.name} · ${curve.detail}`),
      evaluated,
      note('finite samples', `${finite.length} of ${points.length}`),
      measure('Area', r => n4(r.Area())),
      measure('SignedArea', r => n4(r.SignedArea())),
      measure('Perimeter', r => n4(r.Perimeter())),
      measure('Centroid', r => p2(r.Centroid())),
      measure('Bounds', r => `${n4(r.BoundsWidth())} x ${n4(r.BoundsHeight())}`),
      measure('IsoperimetricQuotient', r => n4(r.IsoperimetricQuotient())),
      measure('IsSimple', r => String(r.IsSimple())),
      measure('SelfIntersectionCount', r => String(r.SelfIntersectionCount())),
    ];
    if (curve.closure) {
      readings.push(reading(`${curve.name}.ClosureGap`, curve.closure));
    }
    return { object, readings };
  },
});

/** The subset of the region catalog the containment grid offers, after the star. */
const CONTAINMENT_LABELS = [
  'Star',
  'Circle',
  'Ellipse',
  'Annulus',
  'Sector',
  'Capsule',
  'Superellipse',
  'RegularPolygon',
];
const CONTAINMENT_REGIONS = [0, 1, 2, 3, 5, 6, 9];

const containment = sceneOf({
  id: 'containment',
  title: 'Containment and nearest point',
  description:
    'Polygon2D.Contains over a grid of samples. The rule is even-odd, so on a non-convex star ring ' +
    'the notches read as outside; the sampled area fraction converges on the shoelace Area. The spokes ' +
    'join probe points to Polygon2D.ClosestPoint, which returns the query itself when it is inside. ' +
    'Every named region carries its own analytic predicate too, so the ring\'s answer and the type\'s ' +
    'answer are both taken over the same grid and the disagreements — samples the sampled ring gets ' +
    'wrong near the boundary, and the seams the even-odd rule cannot see — are counted.',
  plato: [
    'RegularStar2D.RegularStarVertices',
    'RegularStar2D.Contains',
    'Circle.Contains',
    'Ellipse.Contains',
    'Annulus.Contains',
    'CircularSector.Contains',
    'Capsule2D.Contains',
    'SuperEllipse.Contains',
    'RegularPolygon.Contains',
    'Capsule2D.ClosestPoint',
    'Polygon2D.Contains',
    'Polygon2D.Excludes',
    'Polygon2D.ClosestPoint',
    'Polygon2D.DistanceTo',
    'Polygon2D.Area',
    'PolygonWithHoles2D.Contains',
  ],
  controls: [
    { key: 'shape', label: 'Shape', kind: 'select', options: CONTAINMENT_LABELS, def: 0 },
    { key: 'points', label: 'Star points', kind: 'slider', min: 3, max: 12, step: 1, def: 5 },
    { key: 'param', label: 'Shape parameter', kind: 'slider', min: 0.15, max: 1, step: 0.01, def: 0.38 },
    { key: 'samples', label: 'Arc samples', kind: 'slider', min: 12, max: 96, step: 4, def: 40 },
    { key: 'resolution', label: 'Grid', kind: 'slider', min: 8, max: 40, step: 1, def: 26 },
    { key: 'probes', label: 'Probes', kind: 'slider', min: 0, max: 24, step: 1, def: 12 },
  ],
  build(params: Params): Built {
    const kind = Math.round(params.shape);

    // The star keeps its own construction; everything else comes out of the
    // named-shape catalog, which is the same catalog the gallery scene draws.
    let name: string;
    let ring: Polygon2D;
    let holes: Polygon2D[] = [];
    let ownContains: ((p: Point2D) => boolean) | undefined;
    let ownClosest: ((p: Point2D) => Point2D) | undefined;
    let ownArea: Reading | undefined;

    if (kind === 0) {
      const star = new RegularStar2D(
        new Point2D(0, 0),
        RADIUS,
        RADIUS * params.param,
        Math.round(params.points),
        new Angle(0),
      );
      name = 'RegularStar2D';
      ring = new Polygon2D(star.RegularStarVertices());
      ownContains = p => star.Contains(p);
      ownArea = reading('RegularStar2D.Area', () => n4(star.Area()));
    } else {
      const shape = regionShape(CONTAINMENT_REGIONS[kind - 1], params.param, params.samples);
      name = shape.name;
      ring = shape.rings[0];
      holes = shape.rings.slice(1);
      ownContains = shape.contains;
      ownClosest = shape.closest;
      ownArea = shape.closed.find(r => r.label === `${shape.name}.Area`);
    }

    // A region with holes answers for the whole region, not just its boundary.
    const region = holes.length > 0 ? new PolygonWithHoles2D(ring, fromArray(holes)) : null;
    const ringContains = region ? (p: Point2D) => region.Contains(p) : (p: Point2D) => ring.Contains(p);

    const half = ringHalfExtent(ring);
    const grid = sampleGrid(half, Math.round(params.resolution), ringContains);
    const boxArea = (2 * half) ** 2;
    const estimate = (grid.inside / grid.samples.length) * boxArea;
    const disagreements = ownContains
      ? grid.samples.filter(s => ownContains(new Point2D(s.x, s.y)) !== s.inside).length
      : 0;

    const object = new THREE.Group();
    object.add(dots(grid.samples));

    // Spokes from probes on a circle to the nearest boundary point.
    const probeCount = Math.round(params.probes);
    const spokes: number[] = [];
    const reach = half * 1.16;
    let farthest = new Point2D(reach, 0);
    for (let i = 0; i < probeCount; i++) {
      const a = (i / probeCount) * Math.PI * 2;
      const probe = new Point2D(Math.cos(a) * reach, Math.sin(a) * reach);
      const near = ring.ClosestPoint(probe);
      spokes.push(probe.X, probe.Y, 0.01, near.X, near.Y, 0.01);
      if (ring.DistanceTo(probe) > ring.DistanceTo(farthest)) farthest = probe;
    }
    if (probeCount > 0) object.add(segments(spokes, palette.surfaceAlt));
    object.add(lines(polygon2DLines(ring), palette.line));
    for (const hole of holes) object.add(lines(polygon2DLines(hole), palette.surfaceAlt));

    const origin = new Point2D(0, 0);
    const readings: Reading[] = [
      note('shape', name),
      note('samples', String(grid.samples.length)),
      note('inside', String(grid.inside)),
      reading('Area', () => n4(region ? region.Area() : ring.Area())),
      note('sampled area', n4(estimate)),
      reading('Contains(0,0)', () => String(ringContains(origin))),
      reading('Excludes(0,0)', () => String(ring.Excludes(origin))),
    ];
    if (ownArea) readings.push(ownArea);
    if (ownContains) {
      const predicate = ownContains;
      readings.push(
        reading(`${name}.Contains(0,0)`, () => String(predicate(origin))),
        note(`${name}.Contains disagreements`, `${disagreements} of ${grid.samples.length}`),
      );
    }
    readings.push(
      reading('farthest probe DistanceTo', () => n4(ring.DistanceTo(farthest))),
      reading('its ClosestPoint', () => p2(ring.ClosestPoint(farthest))),
    );
    if (ownClosest) {
      const nearest = ownClosest;
      readings.push(reading(`${name}.ClosestPoint`, () => p2(nearest(farthest))));
    }
    return { object, readings };
  },
});

const winding = sceneOf({
  id: 'winding',
  title: 'Winding and simplicity',
  description:
    'A quad whose last two vertices swap places as the slider runs: past the halfway point the ring ' +
    'crosses itself and IsSimple turns false. SignedArea carries the winding, Winding names it, and ' +
    'EnsureCounterClockwise rewinds the ring so that it encloses positive area.',
  plato: [
    'Polygon2D.SignedArea',
    'Polygon2D.Area',
    'Polygon2D.Perimeter',
    'Polygon2D.Winding',
    'Polygon2D.IsSimple',
    'Polygon2D.SelfIntersectionCount',
    'Polygon2D.EnsureCounterClockwise',
    'Polygon2D.Centroid',
  ],
  controls: [
    { key: 'cross', label: 'Swap corners', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0 },
    { key: 'clockwise', label: 'Store clockwise', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const t = params.cross;
    const r = RADIUS;
    const lerp = (a: number, b: number): number => a + (b - a) * t;
    const base = ringOf([
      new Point2D(-r, -r),
      new Point2D(r, -r),
      new Point2D(lerp(r, -r), r),
      new Point2D(lerp(-r, r), r),
    ]);
    const ring = params.clockwise ? storedBackwards(base) : base;
    const simple = ring.IsSimple();

    const object = new THREE.Group();
    object.add(lines(polygon2DLines(ring), simple ? palette.line : palette.surfaceAlt));
    object.add(vertexDots(toArray(ring.Points), palette.surfaceAlt, 7));
    // A bow tie has zero signed area, so its area centroid is undefined.
    const centroid = ring.Centroid();
    if (Number.isFinite(centroid.X) && Number.isFinite(centroid.Y)) {
      object.add(marker(centroid.X, centroid.Y, palette.accent));
    }

    return {
      object,
      readings: [
        reading('SignedArea', () => n4(ring.SignedArea())),
        reading('Area', () => n4(ring.Area())),
        reading('Perimeter', () => n4(ring.Perimeter())),
        reading('Winding', () => String(ring.Winding())),
        reading('IsSimple', () => String(ring.IsSimple())),
        reading('SelfIntersectionCount', () => String(ring.SelfIntersectionCount())),
        reading('EnsureCounterClockwise SignedArea', () =>
          n4(ring.EnsureCounterClockwise().SignedArea()),
        ),
        reading('EnsureCounterClockwise Winding', () =>
          String(ring.EnsureCounterClockwise().Winding()),
        ),
      ],
    };
  },
});

const repair = sceneOf({
  id: 'repair',
  title: 'Ring repair',
  description:
    'A hexagon stored clockwise with vertices deliberately repeated and collinear midpoints inserted. ' +
    'RemoveDuplicateVertices drops the repeats, RemoveCollinearVertices drops the flat corners one at a ' +
    'time, and Canonical does both and rewinds. All three preserve the area. Collinearity is tested ' +
    'exactly, with no tolerance, so the ring here has dyadic coordinates.',
  plato: [
    'Polygon2D.RemoveDuplicateVertices',
    'Polygon2D.RemoveCollinearVertices',
    'Polygon2D.Canonical',
    'Polygon2D.Winding',
    'Polygon2D.SignedArea',
    'Polygon2D.Area',
    'Polygon2D.Perimeter',
  ],
  controls: [
    { key: 'duplicates', label: 'Repeated vertices', kind: 'slider', min: 0, max: 5, step: 1, def: 2 },
    { key: 'midpoints', label: 'Collinear midpoints', kind: 'slider', min: 0, max: 5, step: 1, def: 2 },
  ],
  build(params: Params): Built {
    // Dyadic coordinates on purpose: polygons.library.plato tests collinearity
    // EXACTLY, and the midpoint of two vertices at irrational coordinates (a
    // regular pentagon's, say) does not land exactly on the edge, so the flat
    // corner it should create is not flat and nothing is removed. Stored
    // clockwise, so Canonical has a winding to correct as well.
    const clean = toArray(storedBackwards(dyadicHexagon()).Points);
    const dirty: Point2D[] = [];
    const duplicates = Math.round(params.duplicates);
    const midpoints = Math.round(params.midpoints);
    for (let i = 0; i < clean.length; i++) {
      const a = clean[i];
      const b = clean[(i + 1) % clean.length];
      dirty.push(a);
      if (i < duplicates) dirty.push(new Point2D(a.X, a.Y));
      if (i < midpoints) dirty.push(new Point2D((a.X + b.X) / 2, (a.Y + b.Y) / 2));
    }
    const ring = ringOf(dirty);
    const canonical = ring.Canonical();

    const object = new THREE.Group();
    object.add(lines(polygon2DLines(ring), 0x3f4c63));
    object.add(vertexDots(dirty, palette.surfaceAlt, 9));
    object.add(lines(polygon2DLines(canonical, 0.02), palette.line));
    object.add(vertexDots(toArray(canonical.Points), palette.accent, 4));

    return {
      object,
      readings: [
        note('stored vertices', String(ring.Points.Count())),
        reading('RemoveDuplicateVertices', () => `${ring.RemoveDuplicateVertices().Points.Count()} left`),
        reading('RemoveCollinearVertices', () => `${ring.RemoveCollinearVertices().Points.Count()} left`),
        reading('Canonical', () => `${canonical.Points.Count()} left`),
        reading('stored Winding', () => String(ring.Winding())),
        reading('Canonical Winding', () => String(canonical.Winding())),
        reading('Area', () => n4(ring.Area())),
        reading('Canonical Area', () => n4(canonical.Area())),
        reading('Perimeter', () => n4(ring.Perimeter())),
      ],
    };
  },
});

const holes = sceneOf({
  id: 'holes',
  title: 'Region with holes',
  description:
    'PolygonWithHoles2D over an octagonal boundary. Area subtracts every hole, Perimeter counts every ' +
    'boundary component, Centroid weights the holes negatively, and Contains is inside the boundary and ' +
    'outside every hole. Canonical winds the boundary counter-clockwise and every hole the other way. ' +
    'Widen the holes past the point where they meet each other or the boundary and IsSimple turns false: ' +
    'the declared invariants are broken, and the measures above it stop meaning anything.',
  plato: [
    'RegularPolygon.RegularPolygonVertices',
    'PolygonWithHoles2D.Area',
    'PolygonWithHoles2D.Perimeter',
    'PolygonWithHoles2D.Centroid',
    'PolygonWithHoles2D.Contains',
    'PolygonWithHoles2D.Bounds',
    'PolygonWithHoles2D.HolesLieInside',
    'PolygonWithHoles2D.Canonical',
    'PolygonWithHoles2D.IsSimple',
  ],
  controls: [
    { key: 'holes', label: 'Holes', kind: 'slider', min: 1, max: 4, step: 1, def: 3 },
    { key: 'radius', label: 'Hole radius', kind: 'slider', min: 0.12, max: 0.8, step: 0.01, def: 0.28 },
    { key: 'resolution', label: 'Grid', kind: 'slider', min: 8, max: 40, step: 1, def: 26 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.holes);
    const boundary = regularRing(8, RADIUS, 0, 0, 0.125);
    const region = regionWithHoles(boundary, count, params.radius, RADIUS * 0.52);
    const grid = sampleGrid(RADIUS * 1.12, Math.round(params.resolution), p => region.Contains(p));
    const bounds = region.Bounds();
    const canonical = region.Canonical();

    const object = new THREE.Group();
    object.add(boundsOutline(bounds.Min, bounds.Max, 0x2f3a4c));
    object.add(dots(grid.samples));
    object.add(lines(polygon2DLines(region.Boundary), palette.line));
    for (const hole of toArray(region.Holes)) {
      object.add(lines(polygon2DLines(hole), palette.surfaceAlt));
    }
    // Once the holes eat the boundary the net area passes through zero, and the
    // area-weighted centroid the body divides by it is undefined.
    const centroid = region.Centroid();
    if (Number.isFinite(centroid.X) && Number.isFinite(centroid.Y)) {
      object.add(marker(centroid.X, centroid.Y, palette.accent));
    }

    return {
      object,
      readings: [
        note('holes', String(region.Holes.Count())),
        reading('Area', () => n4(region.Area())),
        reading('boundary Area', () => n4(region.Boundary.Area())),
        reading('Perimeter', () => n4(region.Perimeter())),
        reading('Centroid', () => p2(centroid)),
        reading('Contains(0,0)', () => String(region.Contains(new Point2D(0, 0)))),
        reading('HolesLieInside', () => String(region.HolesLieInside())),
        reading('Bounds', () => `${p2(bounds.Min)}..${p2(bounds.Max)}`),
        reading(
          'Canonical windings',
          () => `${canonical.Boundary.Winding()} / ${canonical.Holes.At(0).Winding()}`,
        ),
        reading('IsSimple', () => String(region.IsSimple())),
      ],
    };
  },
});

const sets = sceneOf({
  id: 'polygon-set',
  title: 'PolygonSet2D',
  description:
    'The multi-component region Boolean operations return. Its components are declared disjoint, so ' +
    'Area and Perimeter are plain sums over the components, Bounds is the union of theirs and Contains ' +
    'is a disjunction.',
  plato: [
    'PolygonSet2D.Area',
    'PolygonSet2D.Perimeter',
    'PolygonSet2D.Contains',
    'PolygonSet2D.PointPool',
    'PolygonSet2D.Bounds',
    'PolygonWithHoles2D.Area',
    'RegularPolygon.RegularPolygonVertices',
  ],
  controls: [
    { key: 'components', label: 'Components', kind: 'slider', min: 1, max: 5, step: 1, def: 3 },
    { key: 'pierced', label: 'Holes', kind: 'toggle', def: 1 },
    { key: 'resolution', label: 'Grid', kind: 'slider', min: 8, max: 40, step: 1, def: 28 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.components);
    const pierced = params.pierced !== 0;
    const outer = count === 1 ? RADIUS : RADIUS * 0.42;
    const components: PolygonWithHoles2D[] = [];
    for (let i = 0; i < count; i++) {
      const a = (i / count) * Math.PI * 2 + Math.PI / 2;
      const cx = count === 1 ? 0 : Math.cos(a) * RADIUS * 0.56;
      const cy = count === 1 ? 0 : Math.sin(a) * RADIUS * 0.56;
      const boundary = regularRing(6, outer, cx, cy);
      components.push(
        new PolygonWithHoles2D(
          boundary,
          fromArray(pierced ? [storedBackwards(regularRing(6, outer * 0.45, cx, cy))] : []),
        ),
      );
    }
    const set = new PolygonSet2D(fromArray(components));
    const grid = sampleGrid(RADIUS * 1.12, Math.round(params.resolution), p => set.Contains(p));
    const bounds = set.Bounds();

    const object = new THREE.Group();
    object.add(boundsOutline(bounds.Min, bounds.Max, 0x2f3a4c));
    object.add(dots(grid.samples));
    for (const component of components) {
      object.add(lines(polygon2DLines(component.Boundary), palette.line));
      for (const hole of toArray(component.Holes)) {
        object.add(lines(polygon2DLines(hole), palette.surfaceAlt));
      }
    }

    return {
      object,
      readings: [
        note('components', String(set.Polygons.Count())),
        reading('Area', () => n4(set.Area())),
        reading('component Area', () => n4(components[0].Area())),
        reading('Perimeter', () => n4(set.Perimeter())),
        reading('Contains(0,0)', () => String(set.Contains(new Point2D(0, 0)))),
        reading('PointPool', () => `${set.PointPool().Count()} points`),
        reading('Bounds', () => `${p2(bounds.Min)}..${p2(bounds.Max)}`),
      ],
    };
  },
});

const triangulation = sceneOf({
  id: 'triangulation',
  title: 'Triangulation',
  description:
    'Ear clipping from triangulation.library.plato: Polygon2D.Triangulate on a star ring, ' +
    'PolygonSet2D.Triangulate over a bridged region with holes, and Polygon3D.ToTriangleMesh, which ' +
    'clips in the polygon\'s own plane and keeps the face indices. A simple ring of n vertices clips to ' +
    'n - 2 ears, and bridging a hole into the boundary adds its vertices plus the two bridge copies, so ' +
    'the face count the status line reports is checked against that. The fourth input takes the other ' +
    'road: ConvexPolygon2D declares its ring convex, so its Triangulate is a fan over MapRange with no ' +
    'ear search at all.',
  plato: [
    'Polygon2D.Triangulate',
    'PolygonWithHoles2D.Triangulate',
    'PolygonSet2D.Triangulate',
    'ConvexPolygon2D.Triangulate',
    'ConvexPolygon2D.TriangulationFaceCount',
    'Polygon3D.PlanarProjection',
    'Polygon3D.ToTriangleMesh',
    'TriangleMesh2D.FaceCount',
  ],
  controls: [
    {
      key: 'shape',
      label: 'Input',
      kind: 'select',
      options: [
        'Polygon2D (star)',
        'PolygonSet2D (holes)',
        'Polygon3D (projected)',
        'ConvexPolygon2D (fan)',
      ],
      def: 0,
    },
    { key: 'points', label: 'Detail', kind: 'slider', min: 3, max: 10, step: 1, def: 5 },
  ],
  build(params: Params): Built {
    const detail = Math.round(params.points);
    const kind = Math.round(params.shape);

    let outline: Polygon2D[];
    let source: string;
    let triangulate: () => TriangleMesh2D;

    if (kind === 0) {
      const ring = starRing(detail, RADIUS, 0.42);
      outline = [ring];
      source = `Polygon2D, ${ring.Points.Count()} vertices`;
      triangulate = () => ring.Triangulate();
    } else if (kind === 1) {
      const region = regionWithHoles(
        regularRing(8, RADIUS, 0, 0, 0.125),
        Math.min(detail, 4),
        0.28,
        RADIUS * 0.52,
      );
      const set = new PolygonSet2D(fromArray([region]));
      outline = [region.Boundary, ...toArray(region.Holes)];
      source = `PolygonSet2D, ${set.PointPool().Count()} pooled points`;
      triangulate = () => set.Triangulate();
    } else if (kind === 3) {
      // A convex ring: the regular polygon the catalog builds, handed to the
      // type whose triangulation is a fan rather than an ear search.
      const shape = new RegularPolygon(new Point2D(0, 0), RADIUS, Math.max(3, detail), turns(0));
      const vertices = shape.RegularPolygonVertices();
      const convex = new ConvexPolygon2D(vertices);
      outline = [new Polygon2D(vertices)];
      source = `ConvexPolygon2D, ${convex.Points.Count()} vertices`;
      triangulate = () => convex.Triangulate();
    } else {
      const flat = starRing(detail, RADIUS, 0.42);
      const space = new Polygon3D(
        fromArray(toArray(flat.Points).map(p => new Point3D(p.X, p.Y, 0.35 * p.X))),
      );
      const projected = new Polygon2D(space.PlanarProjection());
      const recentred = projected.Translate(
        new Vector2D(-projected.Centroid().X, -projected.Centroid().Y),
      );
      outline = [recentred];
      source = `Polygon3D projected, ${space.Points.Count()} vertices`;
      triangulate = () => new TriangleMesh2D(recentred.Points, space.ToTriangleMesh().Faces);
    }

    const object = new THREE.Group();
    const faces = reading('Triangulate', () => {
      const mesh = triangulate();
      object.add(triangleMesh2D(mesh));
      return `${mesh.FaceCount()} faces (expected ${earCount(outline)})`;
    });
    const bounds = outline[0].Bounds();
    object.add(boundsOutline(bounds.Min, bounds.Max, 0x2f3a4c));
    for (const ring of outline) {
      object.add(lines(polygon2DLines(ring, 0.02), palette.line));
      object.add(vertexDots(toArray(ring.Points), palette.surfaceAlt, 6));
    }

    return {
      object,
      readings: [
        note('input', source),
        faces,
        reading('outline vertices', () => String(outline.reduce((n, r) => n + r.Points.Count(), 0))),
        reading('outline Area', () =>
          n4(outline.reduce((a, r, i) => a + (i === 0 ? r.Area() : -r.Area()), 0)),
        ),
        reading('outline Perimeter', () => n4(outline.reduce((a, r) => a + r.Perimeter(), 0))),
        reading('outline IsSimple', () => String(outline.every(r => r.IsSimple()))),
      ],
    };
  },
});

/** n - 2 for a simple ring; bridging a hole in adds its vertices plus the two bridge copies. */
function earCount(rings: Polygon2D[]): number {
  const total = rings.reduce((n, r) => n + r.Points.Count(), 0);
  return total + 2 * (rings.length - 1) - 2;
}

const inSpace = sceneOf({
  id: 'polygon3d',
  title: 'Polygon in space',
  description:
    'A planar ring lifted out of the XY plane, drawn beside its own planar projection. Area is the ' +
    'magnitude of the Newell vector area, Normal its direction, Centroid the fan-weighted centroid, and ' +
    'PlanarProjection the ring re-expressed in the basis SupportingPlane names — its 2D area matches the ' +
    '3D one exactly. The grey spokes drop probes onto the plane; the orange ones clamp them to the ring.',
  plato: [
    'Polygon3D.Area',
    'Polygon3D.Perimeter',
    'Polygon3D.Normal',
    'Polygon3D.Centroid',
    'Polygon3D.Bounds',
    'Polygon3D.ClosestPoint',
    'Polygon3D.SupportingPlane',
    'Polygon3D.PlanarProjection',
    'Plane.SignedDistance',
    'Plane.ClosestPoint',
    'Polygon2D.Area',
  ],
  // A ring in space wants a camera you can orbit, not the page's flat one.
  viewer: { orthographic: false, grid: false, spin: true, distance: 4.8 },
  controls: [
    { key: 'points', label: 'Star points', kind: 'slider', min: 3, max: 10, step: 1, def: 5 },
    { key: 'inset', label: 'Inner radius', kind: 'slider', min: 0.2, max: 1, step: 0.01, def: 0.5 },
    { key: 'tilt', label: 'Tilt', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0.5 },
  ],
  build(params: Params): Built {
    const flat = starRing(Math.round(params.points), 0.95, params.inset);
    const ring = new Polygon3D(
      fromArray(
        toArray(flat.Points).map(
          p => new Point3D(p.X, p.Y, params.tilt * p.X + 0.25 * params.tilt * p.Y),
        ),
      ),
    );
    const centroid = ring.Centroid();
    const normal = ring.Normal();
    const plane: Plane = ring.SupportingPlane();

    const space = new THREE.Group();
    space.add(lines(polygon3DLines(ring), palette.line));
    space.add(
      new THREE.ArrowHelper(
        new THREE.Vector3(normal.Vector.X, normal.Vector.Y, normal.Vector.Z).normalize(),
        new THREE.Vector3(centroid.X, centroid.Y, centroid.Z),
        0.6,
        palette.accent,
        0.15,
        0.1,
      ),
    );

    // Two nearest-point answers per probe: the plane's, and the ring's.
    const toPlane: number[] = [];
    const toRing: number[] = [];
    let sample = new Point3D(centroid.X, centroid.Y, centroid.Z + 0.9);
    for (const [dx, dy] of [
      [0, 0],
      [0.75, 0.5],
      [-0.75, 0.5],
      [0.75, -0.5],
      [-0.75, -0.5],
    ]) {
      const probe = new Point3D(centroid.X + dx, centroid.Y + dy, centroid.Z + 0.9);
      const onPlane = plane.ClosestPoint(probe);
      const onRing = ring.ClosestPoint(probe);
      toPlane.push(probe.X, probe.Y, probe.Z, onPlane.X, onPlane.Y, onPlane.Z);
      toRing.push(probe.X, probe.Y, probe.Z, onRing.X, onRing.Y, onRing.Z);
      if (dx > 0 && dy > 0) sample = probe;
    }
    space.add(segments(toPlane, 0x51617d));
    space.add(segments(toRing, palette.surfaceAlt));
    space.position.x = -1.15;

    // The same ring in its own plane coordinates, lying flat in the world.
    const projected = new Polygon2D(ring.PlanarProjection());
    const recentred = projected.Translate(
      new Vector2D(-projected.Centroid().X, -projected.Centroid().Y),
    );
    const flatGroup = new THREE.Group();
    flatGroup.add(lines(polygon2DLines(recentred), palette.surfaceAlt));
    flatGroup.add(vertexDots(toArray(recentred.Points), palette.surfaceAlt, 5));
    flatGroup.position.x = 1.15;

    const object = new THREE.Group();
    object.add(space, flatGroup);

    return {
      object,
      readings: [
        note('vertices', String(ring.Points.Count())),
        reading('Area', () => n4(ring.Area())),
        reading('PlanarProjection Area', () => n4(projected.Area())),
        reading('Perimeter', () => n4(ring.Perimeter())),
        reading('Normal', () => p3(normal.Vector.ToPoint())),
        reading('Centroid', () => p3(centroid)),
        reading(
          'SupportingPlane',
          () => `n ${p3(plane.Normal.Vector.ToPoint())} d ${n4(plane.Distance)}`,
        ),
        reading('plane distance to Centroid', () => n4(plane.SignedDistance(centroid))),
        reading('probe Plane.ClosestPoint', () => p3(plane.ClosestPoint(sample))),
        reading('probe Polygon3D.ClosestPoint', () => p3(ring.ClosestPoint(sample))),
        reading('Bounds', () => `${p3(ring.Bounds().Min)}..${p3(ring.Bounds().Max)}`),
      ],
    };
  },
});

const demo: Demo = {
  title: 'Polygons',
  subtitle:
    'polygons.{types,library}.plato · planar.{types,library}.plato · ' +
    'curves-shapes.{types,library}.plato · triangulation.library.plato',
  scenes: [
    measures,
    gallery,
    closedCurves,
    containment,
    winding,
    repair,
    holes,
    sets,
    triangulation,
    inSpace,
  ],
};

mountDemo(demo, { orthographic: true, grid: false, spin: false });

// The page never imports this; it exists so an offline script can call every
// scene's `build` without a WebGL context.
export { demo };
