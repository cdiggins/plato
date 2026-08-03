// The CSG page: `stdlib/geometry/solids-csg.library.plato` from the outside in.
//
// Three scenes show the booleans themselves, three show the machinery underneath
// them (fragmenting, polygon/plane classification, parity containment) and one
// shows the case the library says it does not handle. Every polygon, plane,
// fragment and containment answer comes from a generated member; this file only
// builds operands, repacks results into Three.js buffers, and picks colours.
//
// Operand sizes are deliberately small. A boolean costs O(|A|^2 |B| + |B|^2 |A|),
// so the largest cutter here has 14 faces (a hexagonal antiprism) against a
// 6-face cube: a single boolean measures in the tens of milliseconds, which a
// slider can drive at interactive rates. A 32-face solid would not.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import {
  fromArray,
  meshToSoup,
  polygon3DLines,
  polygonSoupGeometry,
  toArray,
} from '../shared/mesh.js';
import { edgeMaterial, palette, surfaceMaterial } from '../shared/viewer.js';
import {
  Antiprism,
  Direction3D,
  Plane,
  Point3D,
  Polygon3D,
  PolygonMesh3D,
  PolygonSoup3D,
  RegularPrism,
  Vector3D,
} from '../plato/plato.g.js';
import type { Demo, Params, Scene } from '../shared/demo.js';

// The library's own on-plane thickness — not a number invented here.
const TOLERANCE = Number.CsgPlaneTolerance();

// ---------------------------------------------------------------- operands ---

const Z_UP = new Direction3D(Vector3D.UnitZ());
const originFrame = () => new Point3D(0, 0, 0).AxisFrame(Z_UP);

/** Solid A, shared by every scene: the unit-circumradius cube, six quad faces. */
const A = (meshToSoup(PolygonMesh3D.Cube()));

/** Solid B, the operand the select control chooses. All small on purpose. */
const CUTTERS: ReadonlyArray<{ label: string; soup: PolygonSoup3D }> = [
  {
    label: 'Cube — 6 faces',
    soup: (meshToSoup(PolygonMesh3D.Cube().ScaledAboutOrigin(0.95))),
  },
  {
    label: 'Octagonal prism — 10 faces',
    soup: (meshToSoup(new RegularPrism(originFrame(), 8, 0.62, 1.5).ToPolygonMesh())),
  },
  {
    label: 'Hexagonal antiprism — 14 faces',
    soup: (meshToSoup(new Antiprism(originFrame(), 6, 0.7, 1.3).ToPolygonMesh())),
  },
  {
    label: 'Tetrahedron — 4 faces',
    soup: (meshToSoup(PolygonMesh3D.Tetrahedron())),
  },
  {
    label: 'Octahedron — 8 faces',
    soup: (meshToSoup(PolygonMesh3D.Octahedron().ScaledAboutOrigin(1.05))),
  },
];

function index(value: number, count: number): number {
  const i = Math.round(value);
  return ((i % count) + count) % count;
}

/** Operand B, slid along X by `slide`, through `PolygonSoup3D.Deform`. */
function cutter(pair: number, slide: number): PolygonSoup3D {
  const offset = new Vector3D(slide, 0, 0);
  return CUTTERS[index(pair, CUTTERS.length)].soup.Deform(p => p.Add(offset));
}

// ------------------------------------------------------------------ visuals ---

/** Every polygon as a closed line loop, merged into one LineSegments. */
function lineLoops(polygons: readonly Polygon3D[], color: number): THREE.LineSegments {
  const positions: number[] = [];
  for (const polygon of polygons) {
    const loop = polygon3DLines(polygon);
    const attribute = loop.getAttribute('position');
    const values = attribute.array as ArrayLike<number>;
    for (let i = 0; i < values.length; i++) positions.push(values[i]);
    loop.dispose();
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.LineSegments(geometry, edgeMaterial(color));
}

/** The filled surface of an arbitrary polygon list, via the soup packer. */
function surfaceOf(polygons: readonly Polygon3D[], color: number, opacity = 1): THREE.Mesh {
  const material = surfaceMaterial(color);
  if (opacity < 1) {
    material.transparent = true;
    material.opacity = opacity;
    material.depthWrite = false;
  }
  return new THREE.Mesh(polygonSoupGeometry(new PolygonSoup3D(fromArray(polygons))), material);
}

/** Counts alongside the scene text, so the O(n^2) blow-up is visible. */
function note(description: string, counts: string): void {
  const element = document.querySelector('#scene-description');
  if (element) element.textContent = `${description} ${counts}`;
}

const OP_COLOR = {
  Union: palette.surface,
  Intersection: palette.accent,
  Difference: palette.surfaceAlt,
} as const;

// ------------------------------------------------------- the three booleans ---

const PRECONDITION =
  'Both operands are closed with convex CCW-outward faces, which the boolean bodies require;' +
  ' exactly coplanar shared faces are the library\'s known unreliable case (see the last scene).';

function booleanScene(
  op: 'Union' | 'Intersection' | 'Difference',
  id: string,
  title: string,
  sentence: string,
): Scene {
  const description = `${sentence} ${PRECONDITION}`;
  return {
    id,
    title,
    description,
    plato: [
      `PolygonSoup3D.${op}`,
      'PolygonSoup3D.KeptFragments',
      'PolygonSoup3D.Fragments',
      'PolygonSoup3D.Planes',
      'PolygonSoup3D.Contains',
      'PolygonSoup3D.Deform',
      'Polygon3D.Centroid',
      'PolygonMesh3D.Cube',
      'Number.CsgPlaneTolerance',
    ],
    controls: [
      { key: 'pair', label: 'Operand B', kind: 'select', options: CUTTERS.map(c => c.label), def: 0 },
      { key: 'slide', label: 'Slide B along X', kind: 'slider', min: -1.4, max: 1.4, step: 0.05, def: 0.55 },
      { key: 'operands', label: 'Outline operands', kind: 'toggle', def: 1 },
      { key: 'edges', label: 'Outline result polygons', kind: 'toggle', def: 1 },
    ],
    build(params: Params): THREE.Object3D {
      const b = cutter(params.pair, params.slide);
      const result =
        op === 'Union' ? A.Union(b) : op === 'Intersection' ? A.Intersection(b) : A.Difference(b);
      const polygons = toArray(result.Polygons);

      const group = new THREE.Group();
      group.add(new THREE.Mesh(polygonSoupGeometry(result), surfaceMaterial(OP_COLOR[op])));
      if (params.edges) group.add(lineLoops(polygons, palette.edge));
      if (params.operands) {
        group.add(lineLoops(toArray(A.Polygons), palette.line));
        group.add(lineLoops(toArray(b.Polygons), palette.surfaceAlt));
      }

      note(
        description,
        `A has ${A.Polygons.Count()} faces, B has ${b.Polygons.Count()}; the result is` +
          ` ${polygons.length} polygons.`,
      );
      return group;
    },
  };
}

// --------------------------------------------------- how a boolean is built ---

const FRAGMENTS_DESCRIPTION =
  'The machinery under every boolean: `Fragments` cuts each face of A by every supporting plane of B' +
  ' (`Planes`), so no fragment straddles B\'s boundary, and `KeptFragments` then keeps a fragment when' +
  ' `Contains` puts its centroid on the wanted side. Union and difference keep A\'s outside fragments,' +
  ' intersection keeps the inside ones.';

const fragmentsScene: Scene = {
  id: 'fragments',
  title: 'Fragments and keeps',
  description: FRAGMENTS_DESCRIPTION,
  plato: [
    'PolygonSoup3D.Planes',
    'PolygonSoup3D.Fragments',
    'PolygonSoup3D.KeptFragments',
    'PolygonSoup3D.Contains',
    'Polygon3D.FragmentAgainst',
    'Polygon3D.SplitByPlane',
    'Polygon3D.Centroid',
    'Polygon3D.SupportingPlane',
    'Number.CsgPlaneTolerance',
  ],
  controls: [
    { key: 'pair', label: 'Operand B', kind: 'select', options: CUTTERS.map(c => c.label), def: 1 },
    { key: 'slide', label: 'Slide B along X', kind: 'slider', min: -1.4, max: 1.4, step: 0.05, def: 0.45 },
    {
      key: 'keep',
      label: 'Keep fragments',
      kind: 'select',
      options: ['Outside B (union, A−B)', 'Inside B (intersection)'],
      def: 0,
    },
    { key: 'discarded', label: 'Show discarded fragments', kind: 'toggle', def: 1 },
    { key: 'operand', label: 'Outline B', kind: 'toggle', def: 1 },
  ],
  build(params: Params): THREE.Object3D {
    const b = cutter(params.pair, params.slide);
    const keepInside = params.keep === 1;
    const planes = b.Planes();
    const fragments = toArray(A.Fragments(planes, TOLERANCE));
    const kept: Polygon3D[] = [];
    const dropped: Polygon3D[] = [];
    for (const fragment of fragments) {
      (b.Contains(fragment.Centroid()) === keepInside ? kept : dropped).push(fragment);
    }

    const group = new THREE.Group();
    if (kept.length > 0) group.add(surfaceOf(kept, palette.accent));
    if (params.discarded && dropped.length > 0) {
      group.add(surfaceOf(dropped, palette.surfaceAlt, 0.55));
    }
    group.add(lineLoops(fragments, palette.edge));
    if (params.operand) group.add(lineLoops(toArray(b.Polygons), palette.line));

    // The same count the boolean itself pays for.
    const viaLibrary = A.KeptFragments(b, keepInside).Count();
    note(
      FRAGMENTS_DESCRIPTION,
      `${A.Polygons.Count()} faces of A cut by ${planes.Count()} planes of B give` +
        ` ${fragments.length} fragments; ${kept.length} kept (KeptFragments agrees: ${viaLibrary}),` +
        ` ${dropped.length} discarded.`,
    );
    return group;
  },
};

// ------------------------------------------------ one polygon, one plane -----

const SPLIT_DESCRIPTION =
  'The primitive the fragmenting rests on: `RelationTo` classifies a convex polygon against a plane as' +
  ' Front, Back, Coplanar or Spanning within the tolerance, and `SplitByPlane` returns the polygon' +
  ' untouched unless it spans, in which case it returns the two `ClipToHalfSpace` pieces' +
  ' (Sutherland-Hodgman against the plane and its flip), each dropped by `NonDegenerate` if its ring' +
  ' has fewer than three vertices.';

const RELATION_COLOR: Record<string, number> = {
  Front: palette.surface,
  Back: palette.surfaceAlt,
  Coplanar: palette.accent,
  Spanning: 0xc57ce0,
};

function relationName(polygon: Polygon3D, plane: Plane): string {
  const relation = polygon.RelationTo(plane, TOLERANCE);
  if (relation.IsFront()) return 'Front';
  if (relation.IsBack()) return 'Back';
  if (relation.IsCoplanar()) return 'Coplanar';
  return 'Spanning';
}

/** The polygon scaled about its own centroid — a coplanar stand-in for the
 *  infinite plane, built from the polygon the plane supports. */
function enlarged(polygon: Polygon3D, factor: number): Polygon3D {
  const centre = polygon.Centroid();
  return polygon.Deform(p => centre.Add(centre.Between(p).Multiply(factor)));
}

function shifted(polygon: Polygon3D, direction: Vector3D, distance: number): Polygon3D {
  const offset = direction.Multiply(distance);
  return polygon.Deform(p => p.Add(offset));
}

const planeSplitScene: Scene = {
  id: 'plane-split',
  title: 'Polygon vs plane',
  description: SPLIT_DESCRIPTION,
  plato: [
    'Polygon3D.RelationTo',
    'Polygon3D.SplitByPlane',
    'Polygon3D.ClipToHalfSpace',
    'Polygon3D.NonDegenerate',
    'Polygon3D.SupportingPlane',
    'Polygon3D.Centroid',
    'Polygon3D.Deform',
    'Plane.Flip',
    'Plane.SignedDistance',
    'PolygonSoup3D.Planes',
    'Number.CsgPlaneTolerance',
  ],
  controls: [
    { key: 'pair', label: 'Plane from operand', kind: 'select', options: CUTTERS.map(c => c.label), def: 1 },
    { key: 'face', label: 'Face of A (wraps)', kind: 'slider', min: 0, max: 5, step: 1, def: 5 },
    { key: 'plane', label: 'Plane of B (wraps)', kind: 'slider', min: 0, max: 13, step: 1, def: 3 },
    { key: 'slide', label: 'Slide B along X', kind: 'slider', min: -1.4, max: 1.4, step: 0.05, def: 0.2 },
    { key: 'separate', label: 'Pull pieces apart', kind: 'slider', min: 0, max: 0.5, step: 0.01, def: 0.18 },
    { key: 'halfspace', label: 'Front half-space only', kind: 'toggle', def: 0 },
  ],
  build(params: Params): THREE.Object3D {
    const b = cutter(params.pair, params.slide);
    const face = A.Polygons.At(index(params.face, A.Polygons.Count()));
    const planePolygon = b.Polygons.At(index(params.plane, b.Polygons.Count()));
    const plane = planePolygon.SupportingPlane();
    const normal = plane.Normal.Vector;

    const group = new THREE.Group();
    // The cutting plane, drawn as the face that supports it, blown up in place.
    const witness = enlarged(planePolygon, 1.9);
    group.add(surfaceOf([witness], palette.line, 0.2));
    group.add(lineLoops([witness, planePolygon], palette.line));
    // The uncut polygon, for reference.
    group.add(lineLoops([face], 0xffffff));

    let counts: string;
    if (params.halfspace === 1) {
      const clipped = face.ClipToHalfSpace(plane);
      const surviving = toArray(clipped.NonDegenerate());
      if (surviving.length > 0) group.add(surfaceOf(surviving, palette.surface));
      group.add(lineLoops([clipped], palette.edge));
      counts =
        `ClipToHalfSpace returned a ring of ${clipped.Points.Count()} vertices from` +
        ` ${face.Points.Count()}; NonDegenerate kept ${surviving.length} polygon(s).`;
    } else {
      const pieces = toArray(face.SplitByPlane(plane, TOLERANCE));
      const separation = params.separate;
      const tally: string[] = [];
      for (const piece of pieces) {
        const name = relationName(piece, plane);
        const sign = name === 'Back' ? -1 : 1;
        const moved = shifted(piece, normal, sign * separation);
        group.add(surfaceOf([moved], RELATION_COLOR[name] ?? palette.surface));
        group.add(lineLoops([moved], palette.edge));
        tally.push(`${name} (${piece.Points.Count()} vertices)`);
      }
      counts =
        `RelationTo says ${relationName(face, plane)}; SplitByPlane returned` +
        ` ${pieces.length} piece(s): ${tally.join(', ')}.`;
    }

    note(SPLIT_DESCRIPTION, counts);
    return group;
  },
};

// -------------------------------------------------- point-in-solid parity ----

const CONTAINS_DESCRIPTION =
  '`PolygonSoup3D.Contains` is an even-odd parity test: it fires one ray from the query point along the' +
  ' fixed skew probe direction `CsgProbeDirection` (normalized (pi/6, ln 2, pi/4), so it parallels no' +
  ' axis-aligned face), asks every face `RayHits`, and calls the point inside when the crossing count' +
  ' is odd. This is what classifies each fragment centroid in a boolean. Points exactly on the boundary' +
  ' are not classified reliably.';

const containsScene: Scene = {
  id: 'contains',
  title: 'Point-in-solid parity',
  description: CONTAINS_DESCRIPTION,
  plato: [
    'PolygonSoup3D.Contains',
    'Polygon3D.RayHits',
    'Polygon3D.SupportingPlane',
    'Direction3D.CsgProbeDirection',
    'Ray3D.Intersect',
  ],
  controls: [
    { key: 'solid', label: 'Solid', kind: 'select', options: ['Cube (A)', ...CUTTERS.map(c => c.label)], def: 0 },
    { key: 'grid', label: 'Lattice size (n^3)', kind: 'slider', min: 0, max: 11, step: 1, def: 7 },
    { key: 'probeX', label: 'Probe X', kind: 'slider', min: -1.2, max: 1.2, step: 0.02, def: 0.3 },
    { key: 'probeY', label: 'Probe Y', kind: 'slider', min: -1.2, max: 1.2, step: 0.02, def: 0.15 },
    { key: 'probeZ', label: 'Probe Z', kind: 'slider', min: -1.2, max: 1.2, step: 0.02, def: -0.1 },
    { key: 'ray', label: 'Show probe ray', kind: 'toggle', def: 1 },
  ],
  build(params: Params): THREE.Object3D {
    const solid = params.solid === 0 ? A : CUTTERS[index(params.solid - 1, CUTTERS.length)].soup;
    const faces = toArray(solid.Polygons);
    const group = new THREE.Group();
    group.add(lineLoops(faces, palette.line));

    // The lattice, offset by half a cell so no sample lands exactly on a face.
    const n = Math.max(0, Math.round(params.grid));
    let insideCount = 0;
    if (n > 0) {
      const extent = 1.15;
      const step = (2 * extent) / n;
      const positions: number[] = [];
      const colors: number[] = [];
      const insideColor = new THREE.Color(palette.accent);
      const outsideColor = new THREE.Color(0x39435a);
      for (let i = 0; i < n; i++) {
        for (let j = 0; j < n; j++) {
          for (let k = 0; k < n; k++) {
            const x = -extent + (i + 0.5) * step;
            const y = -extent + (j + 0.5) * step;
            const z = -extent + (k + 0.5) * step;
            const inside = solid.Contains(new Point3D(x, y, z));
            if (inside) insideCount++;
            const color = inside ? insideColor : outsideColor;
            positions.push(x, y, z);
            colors.push(color.r, color.g, color.b);
          }
        }
      }
      const cloud = new THREE.BufferGeometry();
      cloud.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
      cloud.setAttribute('color', new THREE.Float32BufferAttribute(colors, 3));
      group.add(
        new THREE.Points(cloud, new THREE.PointsMaterial({ size: 0.055, vertexColors: true })),
      );
    }

    // The single probe point, its ray, and the faces that ray strikes.
    const probe = new Point3D(params.probeX, params.probeY, params.probeZ);
    const direction = Direction3D.CsgProbeDirection();
    const probeInside = solid.Contains(probe);
    const hits = faces.filter(f => f.RayHits(probe, direction));
    if (hits.length > 0) group.add(surfaceOf(hits, palette.surfaceAlt, 0.5));
    const marker = new THREE.Mesh(
      new THREE.SphereGeometry(0.06, 16, 12),
      surfaceMaterial(probeInside ? palette.accent : 0xffffff),
    );
    marker.position.set(probe.X, probe.Y, probe.Z);
    group.add(marker);
    if (params.ray) {
      const v = direction.Vector;
      const ray = new THREE.BufferGeometry();
      ray.setAttribute(
        'position',
        new THREE.Float32BufferAttribute(
          [probe.X, probe.Y, probe.Z, probe.X + v.X * 4, probe.Y + v.Y * 4, probe.Z + v.Z * 4],
          3,
        ),
      );
      group.add(new THREE.LineSegments(ray, edgeMaterial(0xffffff)));
    }

    note(
      CONTAINS_DESCRIPTION,
      `${faces.length} faces; the probe ray strikes ${hits.length} of them, so parity says` +
        ` ${probeInside ? 'inside' : 'outside'}. Lattice: ${n ** 3} samples, ${insideCount} inside.`,
    );
    return group;
  },
};

// ----------------------------------------------- the case that does not work --

const COPLANAR_DESCRIPTION =
  'The failure the library documents, reproduced rather than hidden: two identical cubes. At offset 0' +
  ' every face of A is exactly coplanar with a face of B, so each fragment centroid sits ON the other' +
  ' solid\'s boundary, where `Contains` — an exact-float parity test — has no defined answer. The' +
  ' difference should be empty and the union should be one cube; neither comes out. At ±1 the cubes' +
  ' share exactly one face and the same problem hits that face alone. At ±0.5 no faces are coplanar and' +
  ' the results are correct.';

const HALF_EXTENT = 0.5773502691896258; // the unit-circumradius cube's half-extent

const coplanarScene: Scene = {
  id: 'coplanar',
  title: 'Coplanar faces: the unreliable case',
  description: COPLANAR_DESCRIPTION,
  plato: [
    'PolygonSoup3D.Union',
    'PolygonSoup3D.Intersection',
    'PolygonSoup3D.Difference',
    'PolygonSoup3D.Contains',
    'PolygonSoup3D.Deform',
    'PolygonMesh3D.Cube',
  ],
  controls: [
    {
      key: 'op',
      label: 'Operation',
      kind: 'select',
      options: ['Union — expect 6', 'Intersection — expect 6', 'Difference — expect 0'],
      def: 2,
    },
    { key: 'steps', label: 'Offset in face widths', kind: 'slider', min: -1, max: 1, step: 0.5, def: 0 },
    { key: 'operands', label: 'Outline operands', kind: 'toggle', def: 1 },
  ],
  build(params: Params): THREE.Object3D {
    const offset = new Vector3D(params.steps * 2 * HALF_EXTENT, 0, 0);
    const b = A.Deform(p => p.Add(offset));
    const op = Math.round(params.op);
    const result = op === 0 ? A.Union(b) : op === 1 ? A.Intersection(b) : A.Difference(b);
    const polygons = toArray(result.Polygons);

    const group = new THREE.Group();
    group.add(new THREE.Mesh(polygonSoupGeometry(result), surfaceMaterial(palette.surface)));
    group.add(lineLoops(polygons, palette.edge));
    if (params.operands) {
      group.add(lineLoops(toArray(A.Polygons), palette.line));
      group.add(lineLoops(toArray(b.Polygons), palette.surfaceAlt));
    }

    const expected = op === 2 ? 0 : 6;
    const coplanar = params.steps === 0 || Math.abs(params.steps) === 1;
    note(
      COPLANAR_DESCRIPTION,
      `Offset ${params.steps}: ${polygons.length} result polygons` +
        (params.steps === 0 ? ` where the exact answer is ${expected}` : '') +
        `. ${coplanar ? 'Faces are coplanar here — the answer is not trustworthy.' : 'No coplanar faces here.'}`,
    );
    return group;
  },
};

// ------------------------------------------------------------------ catalog ---

const demo: Demo = {
  title: 'CSG',
  subtitle: 'solids-csg.library.plato · solids.types.plato · solids.library.plato',
  scenes: [
    booleanScene(
      'Union',
      'union',
      'Union',
      'A | B, assembled from the fragments of A that lie outside B plus the fragments of B that lie' +
        ' outside A — no BSP tree, just plane clipping and a centroid test per fragment.',
    ),
    booleanScene(
      'Intersection',
      'intersection',
      'Intersection',
      'A & B: the same fragmenting, keeping the fragments of each solid that lie inside the other.',
    ),
    booleanScene(
      'Difference',
      'difference',
      'Difference',
      'A − B: A\'s fragments outside B, plus B\'s fragments inside A turned inside out by' +
        ' `ReversedWinding` so they face into the carved cavity.',
    ),
    fragmentsScene,
    planeSplitScene,
    containsScene,
    coplanarScene,
  ],
};

mountDemo(demo, { distance: 4.6 });

// The page never imports this; it exists so `npm run scenes` can call every
// scene's `build` without a WebGL context.
export { demo };
