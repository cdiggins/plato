// Polygons — a scene catalog over stdlib/geometry/polygons.{types,library}.plato,
// the ring kernels in geometry.library.plato, and triangulation.library.plato.
//
// Every ring, every measure and every predicate below comes from a member of
// `src/plato/plato.g.ts`. This file builds inputs (point lists, stars, regions
// with holes), calls the generated members, and repacks the answers into
// Three.js lines and point clouds. No formula the stdlib defines is re-derived
// here — where a generated member still throws, the status line says so by name
// rather than substituting a hand-rolled answer.
//
// The planar scenes take the page's orthographic, grid-free, non-spinning
// camera; the ring in space overrides it with an orbitable perspective one.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { fromArray, polygon2DLines, polygon3DLines, toArray } from '../shared/mesh.js';
import { palette, type ViewerOptions } from '../shared/viewer.js';
import {
  Angle,
  Plane,
  Point2D,
  Point3D,
  Polygon2D,
  Polygon3D,
  PolygonSet2D,
  PolygonWithHoles2D,
  RegularPolygon,
  RegularStar2D,
  TriangleMesh2D,
  Vector2D,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// A member that throws is a gap in the emitted library, not a fact about the
// geometry, so the status line keeps the member's name and the failure instead
// of quietly computing the number some other way. One family is still in that
// state: the `Triangulate` / `ToTriangleMesh` entry points. Their kernel
// (`TriangulateRings` and the ear-clipping passes under it) takes an `Array`
// first, and the TypeScript writer emits no `Array` libraries.

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

const containment = sceneOf({
  id: 'containment',
  title: 'Containment and nearest point',
  description:
    'Polygon2D.Contains over a grid of samples. The rule is even-odd, so on a non-convex star ring ' +
    'the notches read as outside; the sampled area fraction converges on the shoelace Area. The spokes ' +
    'join probe points to Polygon2D.ClosestPoint, which returns the query itself when it is inside.',
  plato: [
    'RegularStar2D.RegularStarVertices',
    'RegularStar2D.Contains',
    'Polygon2D.Contains',
    'Polygon2D.Excludes',
    'Polygon2D.ClosestPoint',
    'Polygon2D.DistanceTo',
    'Polygon2D.Area',
  ],
  controls: [
    { key: 'points', label: 'Star points', kind: 'slider', min: 3, max: 12, step: 1, def: 5 },
    { key: 'inset', label: 'Inner radius', kind: 'slider', min: 0.15, max: 1, step: 0.01, def: 0.38 },
    { key: 'resolution', label: 'Grid', kind: 'slider', min: 8, max: 40, step: 1, def: 26 },
    { key: 'probes', label: 'Probes', kind: 'slider', min: 0, max: 24, step: 1, def: 12 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.points);
    const half = RADIUS * 1.12;
    const star = new RegularStar2D(
      new Point2D(0, 0),
      RADIUS,
      RADIUS * params.inset,
      count,
      new Angle(0),
    );
    const ring = new Polygon2D(star.RegularStarVertices());
    const grid = sampleGrid(half, Math.round(params.resolution), p => ring.Contains(p));
    const boxArea = (2 * half) ** 2;
    const estimate = (grid.inside / grid.samples.length) * boxArea;

    const object = new THREE.Group();
    object.add(dots(grid.samples));

    // Spokes from probes on a circle to the nearest boundary point.
    const probeCount = Math.round(params.probes);
    const spokes: number[] = [];
    let farthest = new Point2D(RADIUS * 1.3, 0);
    for (let i = 0; i < probeCount; i++) {
      const a = (i / probeCount) * Math.PI * 2;
      const probe = new Point2D(Math.cos(a) * RADIUS * 1.3, Math.sin(a) * RADIUS * 1.3);
      const near = ring.ClosestPoint(probe);
      spokes.push(probe.X, probe.Y, 0.01, near.X, near.Y, 0.01);
      if (ring.DistanceTo(probe) > ring.DistanceTo(farthest)) farthest = probe;
    }
    if (probeCount > 0) object.add(segments(spokes, palette.surfaceAlt));
    object.add(lines(polygon2DLines(ring), palette.line));

    return {
      object,
      readings: [
        note('samples', String(grid.samples.length)),
        note('inside', String(grid.inside)),
        reading('Area', () => n4(ring.Area())),
        note('sampled area', n4(estimate)),
        reading('Contains(0,0)', () => String(ring.Contains(new Point2D(0, 0)))),
        reading('Excludes(0,0)', () => String(ring.Excludes(new Point2D(0, 0)))),
        reading('RegularStar2D.Contains(0,0)', () => String(star.Contains(new Point2D(0, 0)))),
        reading('farthest probe DistanceTo', () => n4(ring.DistanceTo(farthest))),
        reading('its ClosestPoint', () => p2(ring.ClosestPoint(farthest))),
      ],
    };
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
    'clips in the polygon\'s own plane and keeps the face indices. The kernel is Array-first and the ' +
    'TypeScript writer emits no Array libraries, so these are the members still unavailable here; the ' +
    'outlines and their measures come from members that are.',
  plato: [
    'Polygon2D.Triangulate',
    'PolygonWithHoles2D.Triangulate',
    'PolygonSet2D.Triangulate',
    'Polygon3D.PlanarProjection',
    'Polygon3D.ToTriangleMesh',
    'TriangleMesh2D.FaceCount',
  ],
  controls: [
    {
      key: 'shape',
      label: 'Input',
      kind: 'select',
      options: ['Polygon2D (star)', 'PolygonSet2D (holes)', 'Polygon3D (projected)'],
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
  subtitle: 'polygons.types.plato · polygons.library.plato · triangulation.library.plato',
  scenes: [measures, containment, winding, repair, holes, sets, triangulation, inSpace],
};

mountDemo(demo, { orthographic: true, grid: false, spin: false });

// The page never imports this; it exists so an offline script can call every
// scene's `build` without a WebGL context.
export { demo };
