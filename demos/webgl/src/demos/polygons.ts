// Polygons — a scene catalog over stdlib/geometry/polygons.{types,library}.plato,
// the ring kernels in geometry.library.plato, and triangulation.library.plato.
//
// Every ring, every measure and every predicate below comes from a member of
// `src/plato/plato.g.ts`. This file builds inputs (point lists, stars, regions
// with holes), calls the generated members, and repacks the answers into
// Three.js lines, point clouds and a canvas readout. No formula the stdlib
// defines is re-derived here — where a generated member currently throws, the
// readout says so by name rather than substituting a hand-rolled answer.
//
// The polygons live in the XY plane, so the page mounts an orthographic,
// grid-free, non-spinning viewer.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { fromArray, polygon2DLines, polygon3DLines, toArray } from '../shared/mesh.js';
import { palette } from '../shared/viewer.js';
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
import type { Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// A member that throws is a gap in the emitted library, not a fact about the
// geometry, so the readout keeps the member's name and the failure instead of
// quietly computing the number some other way.

interface Reading {
  label: string;
  value: string;
  blocked: boolean;
}

function reading(label: string, produce: () => string): Reading {
  try {
    return { label, value: produce(), blocked: false };
  } catch (error) {
    return { label, value: `unavailable (${(error as Error).message})`, blocked: true };
  }
}

function note(label: string, value: string): Reading {
  return { label, value, blocked: false };
}

const n4 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(4);
const p2 = (p: Point2D): string => `(${n4(p.X)}, ${n4(p.Y)})`;
const p3 = (p: Point3D): string => `(${n4(p.X)}, ${n4(p.Y)}, ${n4(p.Z)})`;

// ---------------------------------------------------------------------------
// Presentation helpers

const CONTENT_Y = 0.65;
const PANEL_WIDTH = 3.9;
const PANEL_PX = 520;
const LINE_PX = 20;
const PAD_PX = 10;

/** The measurements panel, as a canvas-textured quad pinned below the geometry. */
function panel(readings: Reading[]): { mesh: THREE.Mesh; worldHeight: number } {
  const height = PAD_PX * 2 + readings.length * LINE_PX;
  const dpr = 3;
  const canvas = document.createElement('canvas');
  canvas.width = PANEL_PX * dpr;
  canvas.height = height * dpr;
  const ctx = canvas.getContext('2d');
  if (ctx) {
    ctx.scale(dpr, dpr);
    ctx.fillStyle = 'rgba(13, 17, 23, 0.86)';
    ctx.fillRect(0, 0, PANEL_PX, height);
    ctx.font = '13px ui-monospace, Menlo, Consolas, monospace';
    ctx.textBaseline = 'middle';
    readings.forEach((r, i) => {
      const y = PAD_PX + LINE_PX * i + LINE_PX / 2;
      ctx.fillStyle = '#7d8ba1';
      ctx.fillText(r.label, PAD_PX, y);
      ctx.fillStyle = r.blocked ? '#e0894a' : '#d8e6f6';
      ctx.fillText(r.value, PAD_PX + 208, y);
    });
  }

  const texture = new THREE.CanvasTexture(canvas);
  texture.colorSpace = THREE.SRGBColorSpace;
  const worldHeight = (PANEL_WIDTH * height) / PANEL_PX;
  const mesh = new THREE.Mesh(
    new THREE.PlaneGeometry(PANEL_WIDTH, worldHeight),
    new THREE.MeshBasicMaterial({ map: texture, transparent: true, depthTest: false }),
  );
  mesh.position.set(0, -2.06 + worldHeight / 2, 0.5);
  mesh.renderOrder = 10;
  return { mesh, worldHeight };
}

/**
 * Geometry lifted above the readout, readout pinned to the bottom of whatever
 * the orthographic camera can currently see. The stage is resizable and only
 * the camera knows the visible width, so the fit is refreshed per frame.
 */
function frame(content: THREE.Object3D, readings: Reading[]): THREE.Object3D {
  const root = new THREE.Group();
  content.position.y += CONTENT_Y;
  const { mesh, worldHeight } = panel(readings);
  root.add(content, mesh);

  mesh.onBeforeRender = (_renderer, _scene, camera): void => {
    const ortho = camera as THREE.OrthographicCamera;
    if (ortho.top === undefined || ortho.right === undefined) return;
    const halfHeight = (ortho.top - ortho.bottom) / 2;
    const halfWidth = (ortho.right - ortho.left) / 2;
    const scale = Math.min(1, (halfWidth * 1.92) / PANEL_WIDTH);
    root.scale.setScalar(scale);
    mesh.position.y = -halfHeight / scale + worldHeight / 2 + 0.03;
  };
  return root;
}

function lineLoop(geometry: THREE.BufferGeometry, color: number): THREE.LineSegments {
  return new THREE.LineSegments(geometry, new THREE.LineBasicMaterial({ color }));
}

/** A small axis cross, for marking a computed centroid. */
function marker(x: number, y: number, color: number, size = 0.07): THREE.LineSegments {
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute(
    'position',
    new THREE.Float32BufferAttribute(
      [x - size, y, 0.02, x + size, y, 0.02, x, y - size, 0.02, x, y + size, 0.02],
      3,
    ),
  );
  return lineLoop(geometry, color);
}

function dots(points: { x: number; y: number; inside: boolean }[]): THREE.Points {
  const positions: number[] = [];
  const colors: number[] = [];
  const inside = new THREE.Color(palette.accent);
  const outside = new THREE.Color(0x35435c);
  for (const p of points) {
    positions.push(p.x, p.y, -0.01);
    const c = p.inside ? inside : outside;
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
  const edges = new THREE.BufferGeometry();
  edges.setAttribute('position', new THREE.Float32BufferAttribute(wire, 3));
  const group = new THREE.Group();
  group.add(
    new THREE.Mesh(
      solid,
      new THREE.MeshBasicMaterial({ color: palette.surface, side: THREE.DoubleSide }),
    ),
    lineLoop(edges, 0x0d1117),
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

function reversed(polygon: Polygon2D): Polygon2D {
  return ringOf(toArray(polygon.Points).reverse());
}

function starRing(pointCount: number, outer: number, inset: number): Polygon2D {
  const star = new RegularStar2D(new Point2D(0, 0), outer, outer * inset, pointCount, new Angle(0));
  return new Polygon2D(star.RegularStarVertices());
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
    rings.push(reversed(regularRing(6, radius, Math.cos(a) * orbit, Math.sin(a) * orbit)));
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

// ---------------------------------------------------------------------------
// Scenes

const measures: Scene = {
  id: 'measures',
  title: 'Ring measures',
  description:
    'Area, SignedArea, Perimeter and Centroid of a Polygon2D built from RegularStar2D.RegularStarVertices. ' +
    'The star\'s own closed-form Area and Perimeter are shown beside the shoelace answers: they agree, ' +
    'and at inset = 1 the ring is a regular 2n-gon.',
  plato: [
    'RegularStar2D.RegularStarVertices',
    'RegularStar2D.Area',
    'RegularStar2D.Perimeter',
    'Polygon2D.Area',
    'Polygon2D.SignedArea',
    'Polygon2D.Perimeter',
    'Polygon2D.Centroid',
    'Polygon2D.Translate',
  ],
  controls: [
    { key: 'points', label: 'Star points', kind: 'slider', min: 3, max: 12, step: 1, def: 5 },
    { key: 'inset', label: 'Inner radius', kind: 'slider', min: 0.15, max: 1, step: 0.01, def: 0.45 },
    { key: 'shift', label: 'Shift X', kind: 'slider', min: -0.7, max: 0.7, step: 0.01, def: 0 },
  ],
  build(params: Params): THREE.Object3D {
    const count = Math.round(params.points);
    const outer = 1.1;
    const star = new RegularStar2D(
      new Point2D(0, 0),
      outer,
      outer * params.inset,
      count,
      new Angle(0),
    );
    const ring = new Polygon2D(star.RegularStarVertices()).Translate(new Vector2D(params.shift, 0));
    const centroid = ring.Centroid();

    const content = new THREE.Group();
    content.add(lineLoop(polygon2DLines(ring), palette.line));
    content.add(vertexDots(toArray(ring.Points), palette.line, 5));
    content.add(marker(centroid.X, centroid.Y, palette.surfaceAlt));

    return frame(content, [
      note('vertices', String(ring.Points.Count())),
      reading('Polygon2D.Area', () => n4(ring.Area())),
      reading('Polygon2D.SignedArea', () => n4(ring.SignedArea())),
      reading('Polygon2D.Perimeter', () => n4(ring.Perimeter())),
      reading('Polygon2D.Centroid', () => p2(centroid)),
      reading('RegularStar2D.Area', () => n4(star.Area())),
      reading('RegularStar2D.Perimeter', () => n4(star.Perimeter())),
    ]);
  },
};

const containment: Scene = {
  id: 'containment',
  title: 'Even-odd containment',
  description:
    'Polygon2D.Contains over a grid of samples. The rule is even-odd, so on a non-convex star ring ' +
    'the notches read as outside; the sampled area fraction converges on the shoelace Area.',
  plato: ['RegularStar2D.RegularStarVertices', 'RegularStar2D.Contains', 'Polygon2D.Contains', 'Polygon2D.Area'],
  controls: [
    { key: 'points', label: 'Star points', kind: 'slider', min: 3, max: 12, step: 1, def: 5 },
    { key: 'inset', label: 'Inner radius', kind: 'slider', min: 0.15, max: 1, step: 0.01, def: 0.38 },
    { key: 'resolution', label: 'Grid', kind: 'slider', min: 8, max: 40, step: 1, def: 26 },
  ],
  build(params: Params): THREE.Object3D {
    const count = Math.round(params.points);
    const resolution = Math.round(params.resolution);
    const half = 1.25;
    const star = new RegularStar2D(new Point2D(0, 0), 1.1, 1.1 * params.inset, count, new Angle(0));
    const ring = new Polygon2D(star.RegularStarVertices());
    const grid = sampleGrid(half, resolution, p => ring.Contains(p));
    const boxArea = (2 * half) ** 2;
    const estimate = (grid.inside / grid.samples.length) * boxArea;

    const content = new THREE.Group();
    content.add(dots(grid.samples));
    content.add(lineLoop(polygon2DLines(ring), palette.line));

    return frame(content, [
      note('samples', `${grid.samples.length} over ${(2 * half).toFixed(1)} x ${(2 * half).toFixed(1)}`),
      note('inside', String(grid.inside)),
      reading('Polygon2D.Area', () => n4(ring.Area())),
      note('sampled area', n4(estimate)),
      reading('RegularStar2D.Contains(0,0)', () => String(star.Contains(new Point2D(0, 0)))),
      reading('Polygon2D.Contains(0,0)', () => String(ring.Contains(new Point2D(0, 0)))),
    ]);
  },
};

const winding: Scene = {
  id: 'winding',
  title: 'Winding and simplicity',
  description:
    'A quad whose last two vertices swap places as the slider runs: past the halfway point the ring ' +
    'crosses itself. SignedArea carries the winding; Winding, IsSimple and SelfIntersectionCount are ' +
    'the predicates polygons.library.plato adds on top of it.',
  plato: [
    'Polygon2D.SignedArea',
    'Polygon2D.Area',
    'Polygon2D.Perimeter',
    'Polygon2D.Winding',
    'Polygon2D.IsSimple',
    'Polygon2D.SelfIntersectionCount',
    'Polygon2D.EnsureCounterClockwise',
  ],
  controls: [
    { key: 'cross', label: 'Swap corners', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0 },
    { key: 'clockwise', label: 'Wind clockwise', kind: 'toggle', def: 0 },
  ],
  build(params: Params): THREE.Object3D {
    const t = params.cross;
    const r = 1.05;
    const lerp = (a: number, b: number): number => a + (b - a) * t;
    const corners: Point2D[] = [
      new Point2D(-r, -r),
      new Point2D(r, -r),
      new Point2D(lerp(r, -r), r),
      new Point2D(lerp(-r, r), r),
    ];
    const base = ringOf(corners);
    const ring = params.clockwise ? reversed(base) : base;

    const content = new THREE.Group();
    content.add(lineLoop(polygon2DLines(ring), palette.line));
    content.add(vertexDots(toArray(ring.Points), palette.surfaceAlt, 7));
    // A bow tie has zero signed area, so its area centroid is undefined; the
    // marker is drawn only when the generated Centroid returns a finite point.
    const centroid = ring.Centroid();
    if (Number.isFinite(centroid.X) && Number.isFinite(centroid.Y)) {
      content.add(marker(centroid.X, centroid.Y, palette.accent));
    }

    return frame(content, [
      reading('Polygon2D.SignedArea', () => n4(ring.SignedArea())),
      reading('Polygon2D.Area', () => n4(ring.Area())),
      reading('Polygon2D.Perimeter', () => n4(ring.Perimeter())),
      reading('Polygon2D.Winding', () => String(ring.Winding())),
      reading('Polygon2D.IsSimple', () => String(ring.IsSimple())),
      reading('Polygon2D.SelfIntersectionCount', () => String(ring.SelfIntersectionCount())),
      reading('EnsureCounterClockwise area', () => n4(ring.EnsureCounterClockwise().SignedArea())),
    ]);
  },
};

const repair: Scene = {
  id: 'repair',
  title: 'Ring repair',
  description:
    'A pentagon with vertices deliberately repeated and collinear midpoints inserted. ' +
    'RemoveDuplicateVertices, RemoveCollinearVertices and Canonical are the repairs ' +
    'polygons.library.plato defines; every one of them preserves the area.',
  plato: [
    'Polygon2D.RemoveDuplicateVertices',
    'Polygon2D.RemoveCollinearVertices',
    'Polygon2D.Canonical',
    'Polygon2D.Area',
    'Polygon2D.Perimeter',
  ],
  controls: [
    { key: 'duplicates', label: 'Repeated vertices', kind: 'slider', min: 0, max: 4, step: 1, def: 2 },
    { key: 'midpoints', label: 'Collinear midpoints', kind: 'slider', min: 0, max: 4, step: 1, def: 2 },
  ],
  build(params: Params): THREE.Object3D {
    const clean = toArray(regularRing(5, 1.05).Points);
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

    const content = new THREE.Group();
    content.add(lineLoop(polygon2DLines(ring), palette.line));
    content.add(vertexDots(dirty, palette.surfaceAlt, 8));
    const canonical = (() => {
      try {
        return ring.Canonical();
      } catch {
        return null;
      }
    })();
    if (canonical) content.add(vertexDots(toArray(canonical.Points), palette.accent, 4));

    return frame(content, [
      note('stored vertices', String(ring.Points.Count())),
      reading('RemoveDuplicateVertices', () => `${ring.RemoveDuplicateVertices().Points.Count()} vertices`),
      reading('RemoveCollinearVertices', () => `${ring.RemoveCollinearVertices().Points.Count()} vertices`),
      reading('Canonical', () => `${ring.Canonical().Points.Count()} vertices`),
      reading('Polygon2D.Area', () => n4(ring.Area())),
      reading('Canonical area', () => n4(ring.Canonical().Area())),
      reading('Polygon2D.Perimeter', () => n4(ring.Perimeter())),
    ]);
  },
};

const holes: Scene = {
  id: 'holes',
  title: 'Region with holes',
  description:
    'PolygonWithHoles2D over an octagonal boundary. Area subtracts every hole, Perimeter counts every ' +
    'boundary component, Centroid weights the holes negatively, and Contains is inside the boundary and ' +
    'outside every hole.',
  plato: [
    'RegularPolygon.RegularPolygonVertices',
    'RegularPolygon.Area',
    'PolygonWithHoles2D.Area',
    'PolygonWithHoles2D.Perimeter',
    'PolygonWithHoles2D.Centroid',
    'PolygonWithHoles2D.Contains',
    'PolygonWithHoles2D.HolesLieInside',
    'PolygonWithHoles2D.IsSimple',
  ],
  controls: [
    { key: 'holes', label: 'Holes', kind: 'slider', min: 1, max: 4, step: 1, def: 3 },
    { key: 'radius', label: 'Hole radius', kind: 'slider', min: 0.1, max: 0.36, step: 0.01, def: 0.24 },
    { key: 'resolution', label: 'Grid', kind: 'slider', min: 8, max: 40, step: 1, def: 26 },
  ],
  build(params: Params): THREE.Object3D {
    const count = Math.round(params.holes);
    const boundary = regularRing(8, 1.12, 0, 0, 0.125);
    const region = regionWithHoles(boundary, count, params.radius, 0.58);
    const grid = sampleGrid(1.25, Math.round(params.resolution), p => region.Contains(p));

    const content = new THREE.Group();
    content.add(dots(grid.samples));
    content.add(lineLoop(polygon2DLines(region.Boundary), palette.line));
    for (const hole of toArray(region.Holes)) {
      content.add(lineLoop(polygon2DLines(hole), palette.surfaceAlt));
    }
    const centroid = region.Centroid();
    content.add(marker(centroid.X, centroid.Y, palette.accent));

    return frame(content, [
      note('holes', String(region.Holes.Count())),
      reading('PolygonWithHoles2D.Area', () => n4(region.Area())),
      reading('boundary Area', () => n4(region.Boundary.Area())),
      reading('PolygonWithHoles2D.Perimeter', () => n4(region.Perimeter())),
      reading('PolygonWithHoles2D.Centroid', () => p2(centroid)),
      reading('Contains(0,0)', () => String(region.Contains(new Point2D(0, 0)))),
      reading('HolesLieInside', () => String(region.HolesLieInside())),
      reading('PolygonWithHoles2D.IsSimple', () => String(region.IsSimple())),
    ]);
  },
};

const sets: Scene = {
  id: 'polygon-set',
  title: 'PolygonSet2D',
  description:
    'The multi-component region Boolean operations return. Its components are declared disjoint, so ' +
    'Area and Perimeter are plain sums over the components and Contains is a disjunction.',
  plato: [
    'PolygonSet2D.Area',
    'PolygonSet2D.Perimeter',
    'PolygonSet2D.Contains',
    'PolygonSet2D.PointPool',
    'PolygonSet2D.Bounds',
    'PolygonWithHoles2D.Area',
    'RegularPolygon.Area',
  ],
  controls: [
    { key: 'components', label: 'Components', kind: 'slider', min: 1, max: 5, step: 1, def: 3 },
    { key: 'pierced', label: 'Holes', kind: 'toggle', def: 1 },
    { key: 'resolution', label: 'Grid', kind: 'slider', min: 8, max: 40, step: 1, def: 28 },
  ],
  build(params: Params): THREE.Object3D {
    const count = Math.round(params.components);
    const pierced = params.pierced !== 0;
    const components: PolygonWithHoles2D[] = [];
    for (let i = 0; i < count; i++) {
      const a = (i / count) * Math.PI * 2 + Math.PI / 2;
      const cx = count === 1 ? 0 : Math.cos(a) * 0.62;
      const cy = count === 1 ? 0 : Math.sin(a) * 0.62;
      const boundary = regularRing(6, count === 1 ? 1.05 : 0.46, cx, cy);
      components.push(
        pierced
          ? new PolygonWithHoles2D(
              boundary,
              fromArray([reversed(regularRing(6, count === 1 ? 0.45 : 0.2, cx, cy))]),
            )
          : new PolygonWithHoles2D(boundary, fromArray<Polygon2D>([])),
      );
    }
    const set = new PolygonSet2D(fromArray(components));
    const grid = sampleGrid(1.25, Math.round(params.resolution), p => set.Contains(p));

    const content = new THREE.Group();
    content.add(dots(grid.samples));
    for (const component of components) {
      content.add(lineLoop(polygon2DLines(component.Boundary), palette.line));
      for (const hole of toArray(component.Holes)) {
        content.add(lineLoop(polygon2DLines(hole), palette.surfaceAlt));
      }
    }

    return frame(content, [
      note('components', String(set.Polygons.Count())),
      reading('PolygonSet2D.Area', () => n4(set.Area())),
      reading('component Area', () => n4(components[0].Area())),
      reading('PolygonSet2D.Perimeter', () => n4(set.Perimeter())),
      reading('PolygonSet2D.Contains(0,0)', () => String(set.Contains(new Point2D(0, 0)))),
      reading('PolygonSet2D.PointPool', () => `${set.PointPool().Count()} points`),
      reading('PolygonSet2D.Bounds', () => `${p2(set.Bounds().Min)} .. ${p2(set.Bounds().Max)}`),
    ]);
  },
};

const triangulation: Scene = {
  id: 'triangulation',
  title: 'Triangulation',
  description:
    'Ear clipping from triangulation.library.plato: Polygon2D.Triangulate on a star ring, ' +
    'PolygonSet2D.Triangulate over a bridged region with holes, and Polygon3D.ToTriangleMesh, ' +
    'which clips in the polygon\'s own plane and keeps the face indices.',
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
  build(params: Params): THREE.Object3D {
    const detail = Math.round(params.points);
    const kind = Math.round(params.shape);

    let outline: Polygon2D[];
    let source: string;
    let triangulate: () => TriangleMesh2D;

    if (kind === 0) {
      const ring = starRing(detail, 1.08, 0.42);
      outline = [ring];
      source = `Polygon2D, ${ring.Points.Count()} vertices`;
      triangulate = () => ring.Triangulate();
    } else if (kind === 1) {
      const region = regionWithHoles(regularRing(8, 1.12, 0, 0, 0.125), Math.min(detail, 4), 0.24, 0.58);
      const set = new PolygonSet2D(fromArray([region]));
      outline = [region.Boundary, ...toArray(region.Holes)];
      source = `PolygonSet2D, ${set.PointPool().Count()} pooled points`;
      triangulate = () => set.Triangulate();
    } else {
      const flat = starRing(detail, 1.08, 0.42);
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

    const content = new THREE.Group();
    let faceCount: Reading;
    try {
      const mesh = triangulate();
      content.add(triangleMesh2D(mesh));
      faceCount = note('faces', `${mesh.FaceCount()} (expected ${outlineFanCount(outline)})`);
    } catch (error) {
      faceCount = { label: 'Triangulate', value: `unavailable (${(error as Error).message})`, blocked: true };
    }
    for (const ring of outline) content.add(lineLoop(polygon2DLines(ring, 0.02), palette.line));

    return frame(content, [
      note('input', source),
      faceCount,
      reading('outline vertices', () => String(outline.reduce((n, r) => n + r.Points.Count(), 0))),
      reading('outline Area', () =>
        n4(outline.reduce((a, r, i) => a + (i === 0 ? r.Area() : -r.Area()), 0)),
      ),
      reading('outline Perimeter', () => n4(outline.reduce((a, r) => a + r.Perimeter(), 0))),
    ]);
  },
};

/** n - 2 per ring for a simple polygon; a bridged hole adds two triangles per hole vertex pair. */
function outlineFanCount(rings: Polygon2D[]): number {
  const boundary = rings[0].Points.Count();
  const holeVertices = rings.slice(1).reduce((n, r) => n + r.Points.Count(), 0);
  return boundary + holeVertices + 2 * (rings.length - 1) - 2;
}

const inSpace: Scene = {
  id: 'polygon3d',
  title: 'Polygon in space',
  description:
    'A planar ring lifted out of the XY plane. Area is the magnitude of the Newell vector area, Normal ' +
    'its direction, Centroid the fan-weighted centroid, and PlanarProjection the ring re-expressed in ' +
    'the plane basis SupportingPlane names — its 2D area matches the 3D one exactly.',
  plato: [
    'Polygon3D.Area',
    'Polygon3D.Perimeter',
    'Polygon3D.Normal',
    'Polygon3D.Centroid',
    'Polygon3D.SupportingPlane',
    'Polygon3D.PlanarProjection',
    'Plane.SignedDistance',
    'Plane.ClosestPoint',
    'Polygon2D.Area',
  ],
  controls: [
    { key: 'points', label: 'Star points', kind: 'slider', min: 3, max: 10, step: 1, def: 5 },
    { key: 'inset', label: 'Inner radius', kind: 'slider', min: 0.2, max: 1, step: 0.01, def: 0.5 },
    { key: 'tilt', label: 'Tilt', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0.5 },
  ],
  build(params: Params): THREE.Object3D {
    const flat = starRing(Math.round(params.points), 0.85, params.inset);
    const ring = new Polygon3D(
      fromArray(
        toArray(flat.Points).map(p => new Point3D(p.X, p.Y, params.tilt * p.X + 0.25 * params.tilt * p.Y)),
      ),
    );
    const centroid = ring.Centroid();
    const normal = ring.Normal();
    const plane: Plane = ring.SupportingPlane();

    // Left: the ring in space, tipped towards the viewer so the lift reads.
    const space = new THREE.Group();
    space.add(lineLoop(polygon3DLines(ring), palette.line));
    space.add(
      new THREE.ArrowHelper(
        new THREE.Vector3(normal.Vector.X, normal.Vector.Y, normal.Vector.Z).normalize(),
        new THREE.Vector3(centroid.X, centroid.Y, centroid.Z),
        0.55,
        palette.accent,
        0.14,
        0.09,
      ),
    );
    // Probe points dropped onto the supporting plane, via Plane.ClosestPoint.
    const drops: number[] = [];
    for (const [dx, dy] of [
      [0, 0],
      [0.5, 0.35],
      [-0.5, 0.35],
      [0, -0.55],
    ]) {
      const probe = new Point3D(centroid.X + dx, centroid.Y + dy, centroid.Z + 0.8);
      const foot = plane.ClosestPoint(probe);
      drops.push(probe.X, probe.Y, probe.Z, foot.X, foot.Y, foot.Z);
    }
    const dropGeometry = new THREE.BufferGeometry();
    dropGeometry.setAttribute('position', new THREE.Float32BufferAttribute(drops, 3));
    space.add(lineLoop(dropGeometry, 0x51617d));
    space.rotation.set(-1.0, 0.45, 0);
    space.position.x = -0.75;

    // Right: the same ring in its own plane coordinates, drawn flat.
    const projected = new Polygon2D(ring.PlanarProjection());
    const recentred = projected.Translate(
      new Vector2D(-projected.Centroid().X, -projected.Centroid().Y),
    );
    const flatGroup = new THREE.Group();
    flatGroup.add(lineLoop(polygon2DLines(recentred), palette.surfaceAlt));
    flatGroup.add(vertexDots(toArray(recentred.Points), palette.surfaceAlt, 5));
    flatGroup.position.x = 0.85;

    const content = new THREE.Group();
    content.add(space, flatGroup);

    return frame(content, [
      note('vertices', String(ring.Points.Count())),
      reading('Polygon3D.Area', () => n4(ring.Area())),
      reading('PlanarProjection Area', () => n4(projected.Area())),
      reading('Polygon3D.Perimeter', () => n4(ring.Perimeter())),
      reading('Polygon3D.Normal', () => p3(normal.Vector.ToPoint())),
      reading('Polygon3D.Centroid', () => p3(centroid)),
      reading('SupportingPlane', () => `n ${p3(plane.Normal.Vector.ToPoint())} d ${n4(plane.Distance)}`),
      reading('plane distance to centroid', () => n4(plane.SignedDistance(centroid))),
    ]);
  },
};

const demo: Demo = {
  title: 'Polygons',
  subtitle: 'polygons.types.plato · polygons.library.plato · triangulation.library.plato',
  scenes: [measures, containment, winding, repair, holes, sets, triangulation, inSpace],
};

mountDemo(demo, { orthographic: true, grid: false, spin: false });

// The page never imports this; it exists so an offline script can call every
// scene's `build` without a WebGL context.
export { demo };
