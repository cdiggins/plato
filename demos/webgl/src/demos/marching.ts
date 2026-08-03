// Marching cubes — a scene catalog over the isosurface path of the stdlib:
// `stdlib/geometry/voxels.library.plato` (the kernel), `implicit-sdf.{types,library}.plato`
// (the fields the kernel is pointed at), `fields-implicits.library.plato` (the
// `ToSdf` bridge and the metaball falloff) and `meshes.types.plato` (`TriangleArray3D`,
// the unwelded soup the kernel returns).
//
// The mesher is the stdlib's throughout. No case table, no edge interpolation and
// no lattice walk is written here: `TriangleArray3D.MarchingCubesCaseTable`,
// `Number.MarchingCubesEdgePoint`, `IntegerVector3.MarchingCubesCase` and
// `IntegerVector3.MarchingCubesLattice` do that work, and the first two scenes exist
// to show them doing it. What this file does is build fields, choose bounds and node
// counts, and repack the returned triangles into Three.js buffers.
//
// Where a generated member is missing or returns something the source does not
// promise, the status line says so by name rather than substituting a hand-rolled
// answer. Two such findings are on this page:
//
//  - `SdfNode3D` and `SdfCombine` are sum types, which the TypeScript writer reports
//    as CHK320 and does not emit, so `SdfTree3D` exists but cannot be populated. The
//    combinator scene therefore composes `FunctionSdf3D.Union` / `SmoothUnion` / …
//    directly, which is the same set of operations the tree's `SdfCombine` cases name.
//  - `ToSdf(Triangle3D, thickness)` and `ToSdf(Quad3D, thickness)` are the second
//    overload of their pair and were dropped ("Skipped: overload or duplicate
//    member"), so `triangle.ToSdf(0.3)` silently reaches the ZERO-THICKNESS body and
//    marches to an empty surface. The plate entry in the primitive gallery reaches
//    the same field through `FunctionSdf3D.Offset` and says so.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { toArray, triangleArrayGeometry } from '../shared/mesh.js';
import { palette, surfaceMaterial, type ViewerOptions } from '../shared/viewer.js';
import {
  Angle,
  Bounds3D,
  Capsule3D,
  Direction3D,
  FunctionSdf3D,
  IntegerVector3,
  Intrinsics,
  ItemIndex,
  MetaBall3D,
  MetaBallSystem3D,
  PerlinNoise3D,
  Plane,
  Point3D,
  SdfBendModifier3D,
  SdfDisplacementModifier3D,
  SdfElongationModifier3D,
  SdfOnionModifier,
  SdfRepetitionModifier3D,
  SdfRoundingModifier,
  SdfShellModifier,
  SdfTwistModifier3D,
  Sphere,
  Triangle3D,
  TriangleArray3D,
  ValueNoise3D,
  Vector3D,
  type IArray,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// Same contract as `src/demos/polygons.ts`: a member that throws is a gap in the
// emitted library, not a fact about the geometry, so the status line keeps the
// member's name and the failure.

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

const n3 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(3);
const pt = (p: Point3D): string => `(${n3(p.X)}, ${n3(p.Y)}, ${n3(p.Z)})`;
const p3 = (x: number, y: number, z: number): Point3D => new Point3D(x, y, z);
const cube = (n: number): IntegerVector3 => new IntegerVector3(n, n, n);
const box = (half: number): Bounds3D => new Bounds3D(p3(-half, -half, -half), p3(half, half, half));

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
// The kernel's own vocabulary
//
// `voxels.library.plato` writes the cube kernel against two sampling functions
// rather than against a grid type, so a scene supplies exactly these two and
// nothing else. `valueAt` reports the sample at a lattice node, `pointAt` its
// world position; both are called only with in-range indices.

type ValueAt = (i: number, j: number, k: number) => number;
type PointAt = (i: number, j: number, k: number) => Point3D;

/**
 * The members whose RECEIVER is the case table itself. The emitted library puts
 * them on `Array<Integer>`, which TypeScript sees as a bare `IArray<number>`, so
 * they are reached through this shape rather than through the class typings.
 */
interface CaseTableOps {
  Count(): number;
  At(index: number): number;
  MarchingCubesTriangleCount(configuration: number): number;
  MarchingCubesCell(
    cell: IntegerVector3,
    valueAt: ValueAt,
    pointAt: PointAt,
    isoLevel: number,
  ): IArray<Triangle3D>;
}

/**
 * Read ONCE, as the source insists: the table is an array literal, so calling
 * the member allocates four thousand entries per call. `MarchingCubesLattice`
 * reads its own copy per extraction; the two mechanism scenes below share this
 * one across every rebuild.
 */
const CASE_TABLE = TriangleArray3D.MarchingCubesCaseTable() as unknown as CaseTableOps;

/**
 * Bourke's corner numbering, taken from the generated offset members rather than
 * restated: corner c sits at (X, Y, Z) offset from the cube's minimum corner.
 */
const CORNER_OFFSETS: readonly (readonly [number, number, number])[] = Array.from(
  { length: 8 },
  (_unused, c: number) =>
    [
      c.MarchingCubesCornerOffsetX(),
      c.MarchingCubesCornerOffsetY(),
      c.MarchingCubesCornerOffsetZ(),
    ] as const,
);

/** The twelve cube edges as corner pairs, from `Number.MarchingCubesEdgeCornerA/B`. */
const EDGE_CORNERS: readonly (readonly [number, number])[] = Array.from(
  { length: 12 },
  (_unused, e: number) => [e.MarchingCubesEdgeCornerA(), e.MarchingCubesEdgeCornerB()] as const,
);

const ORIGIN_CELL = new IntegerVector3(0, 0, 0);

// ---------------------------------------------------------------------------
// Presentation helpers

function segments(coordinates: number[], color: number, opacity = 1): THREE.LineSegments {
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(coordinates, 3));
  return new THREE.LineSegments(
    geometry,
    new THREE.LineBasicMaterial({ color, transparent: opacity < 1, opacity }),
  );
}

function dots(points: readonly Point3D[], color: number, size: number): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.X, p.Y, p.Z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(
    geometry,
    new THREE.PointsMaterial({ color, size, sizeAttenuation: false }),
  );
}

/** The soup the kernel returns, as flat-shaded triangles. */
function surfaceOf(triangles: TriangleArray3D, color = palette.surface): THREE.Mesh {
  return new THREE.Mesh(triangleArrayGeometry(triangles), surfaceMaterial(color));
}

/** Every emitted triangle's own three edges — the cut polygon, cell by cell. */
function triangleEdges(triangles: TriangleArray3D, color: number): THREE.LineSegments {
  const coordinates: number[] = [];
  for (const t of toArray(triangles.Triangles)) {
    const corners = [t.A, t.B, t.C];
    for (let i = 0; i < 3; i++) {
      const a = corners[i];
      const b = corners[(i + 1) % 3];
      coordinates.push(a.X, a.Y, a.Z, b.X, b.Y, b.Z);
    }
  }
  return segments(coordinates, color);
}

/** The twelve edges of one cube of a lattice, in the kernel's own numbering. */
function cellEdgeCoordinates(cell: IntegerVector3, pointAt: PointAt, into: number[]): void {
  for (const [a, b] of EDGE_CORNERS) {
    const pa = cell.MarchingCubesCornerPoint(a, pointAt);
    const pb = cell.MarchingCubesCornerPoint(b, pointAt);
    into.push(pa.X, pa.Y, pa.Z, pb.X, pb.Y, pb.Z);
  }
}

function boundsEdges(bounds: Bounds3D, color: number, opacity = 1): THREE.LineSegments {
  const { Min: a, Max: b } = bounds;
  const corner = (i: number): [number, number, number] => [
    (i & 1) === 0 ? a.X : b.X,
    (i & 2) === 0 ? a.Y : b.Y,
    (i & 4) === 0 ? a.Z : b.Z,
  ];
  const coordinates: number[] = [];
  for (let i = 0; i < 8; i++) {
    for (const bit of [1, 2, 4]) {
      if ((i & bit) !== 0) continue;
      coordinates.push(...corner(i), ...corner(i | bit));
    }
  }
  return segments(coordinates, color, opacity);
}

const OUTSIDE_DOT = 0x51617d;

// ---------------------------------------------------------------------------
// Field construction
//
// Building the field is demo work; every distance in it comes from a generated
// member. The closed-form primitives of `implicit-sdf.library.plato` dispatch on
// `Point3D` — `DistanceToBox`, `DistanceToTorus`, … — and `FunctionSdf3D` is the
// type that stores such a lambda, so a primitive with no `ToSdf` of its own is
// packaged here and combined by the same generated operators as one that has.

const sdfOf = (f: (p: Point3D) => number): FunctionSdf3D => new FunctionSdf3D(f);

interface Primitive {
  label: string;
  /** The generated member the distance comes from. */
  member: string;
  field(): FunctionSdf3D;
  /** An extra offset the entry needs before the scene's level slider applies. */
  base?: number;
  /** A caveat the status line should carry. */
  caveat?: string;
}

const PRIMITIVES: readonly Primitive[] = [
  {
    label: 'Sphere',
    member: 'Sphere.ToSdf',
    field: () => new Sphere(p3(0, 0, 0), 0.9).ToSdf(),
  },
  {
    label: 'Capsule',
    member: 'Capsule3D.ToSdf',
    field: () => new Capsule3D(p3(-0.55, -0.3, 0), p3(0.55, 0.35, 0.15), 0.38).ToSdf(),
  },
  {
    label: 'Box',
    member: 'Point3D.DistanceToBox',
    field: () => sdfOf(p => p.DistanceToBox(new Vector3D(0.7, 0.5, 0.6))),
  },
  {
    label: 'RoundBox',
    member: 'Point3D.DistanceToRoundedBox',
    field: () => sdfOf(p => p.DistanceToRoundedBox(new Vector3D(0.55, 0.4, 0.5), 0.22)),
  },
  {
    label: 'Torus',
    member: 'Point3D.DistanceToTorus',
    field: () => sdfOf(p => p.DistanceToTorus(0.75, 0.3)),
  },
  {
    label: 'Cylinder',
    member: 'Point3D.DistanceToCappedCylinder',
    field: () => sdfOf(p => p.DistanceToCappedCylinder(0.7, 0.55)),
  },
  {
    label: 'Cone',
    member: 'Point3D.DistanceToCone',
    field: () => sdfOf(p => p.DistanceToCone(new Angle(0.5), 1.1)),
  },
  {
    label: 'CapCone',
    member: 'Point3D.DistanceToCappedCone',
    field: () => sdfOf(p => p.DistanceToCappedCone(0.7, 0.7, 0.28)),
  },
  {
    label: 'Ellipsoid',
    member: 'Point3D.DistanceToEllipsoid',
    field: () => sdfOf(p => p.DistanceToEllipsoid(new Vector3D(1, 0.62, 0.45))),
  },
  {
    label: 'VCapsule',
    member: 'Point3D.DistanceToVerticalCapsule',
    field: () => sdfOf(p => p.DistanceToVerticalCapsule(0.9, 0.42)),
  },
  {
    label: 'Plane',
    member: 'Plane.ToSdf',
    field: () => new Plane(new Direction3D(new Vector3D(0.25, 1, 0.15)), 0.1).ToSdf(),
    caveat: 'a half-space: the surface is the lattice boundary’s cross-section, not a closed solid',
  },
  {
    label: 'TriPlate',
    member: 'Triangle3D.ToSdf + FunctionSdf3D.Offset',
    field: () =>
      new Triangle3D(p3(-0.95, -0.5, -0.25), p3(0.95, -0.5, 0.25), p3(0, 0.95, 0)).ToSdf(),
    base: 0.16,
    caveat:
      'ToSdf(Triangle3D, thickness) was dropped as a duplicate overload, so the plate is the '
      + 'zero-thickness patch offset by FunctionSdf3D.Offset instead',
  },
];

/** The pair of operands a boolean scene combines. */
interface OperandPair {
  label: string;
  member: string;
  left(shift: number): FunctionSdf3D;
  right(shift: number): FunctionSdf3D;
}

const OPERANDS: readonly OperandPair[] = [
  {
    label: 'Spheres',
    member: 'Sphere.ToSdf, twice',
    left: shift => new Sphere(p3(-shift, 0, 0), 0.75).ToSdf(),
    right: shift => new Sphere(p3(shift, 0, 0), 0.75).ToSdf(),
  },
  {
    label: 'Box+Ball',
    member: 'Point3D.DistanceToBox, Sphere.ToSdf',
    left: () => sdfOf(p => p.DistanceToBox(new Vector3D(0.62, 0.62, 0.62))),
    right: shift => new Sphere(p3(shift, shift * 0.6, shift * 0.4), 0.68).ToSdf(),
  },
  {
    label: 'Torus+Rod',
    member: 'Point3D.DistanceToTorus, Point3D.DistanceToVerticalCapsule',
    left: () => sdfOf(p => p.DistanceToTorus(0.7, 0.28)),
    right: shift => sdfOf(p => p3(p.X - shift, p.Y, p.Z).DistanceToVerticalCapsule(0.85, 0.3)),
  },
];

/** The distance operators `implicit-sdf.library.plato` defines on a pair of fields. */
interface Combinator {
  label: string;
  member: string;
  /** `blend` is the blend radius, the chamfer width or the morph weight. */
  apply(a: FunctionSdf3D, b: FunctionSdf3D, blend: number): FunctionSdf3D;
  usesBlend: boolean;
}

const COMBINATORS: readonly Combinator[] = [
  { label: 'Union', member: 'FunctionSdf3D.Union', usesBlend: false, apply: (a, b) => a.Union(b) },
  {
    label: 'Intersect',
    member: 'FunctionSdf3D.Intersection',
    usesBlend: false,
    apply: (a, b) => a.Intersection(b),
  },
  {
    label: 'Difference',
    member: 'FunctionSdf3D.Difference',
    usesBlend: false,
    apply: (a, b) => a.Difference(b),
  },
  {
    label: 'XOR',
    member: 'FunctionSdf3D.ExclusiveOr',
    usesBlend: false,
    apply: (a, b) => a.ExclusiveOr(b),
  },
  {
    label: 'SmoothU',
    member: 'FunctionSdf3D.SmoothUnion',
    usesBlend: true,
    apply: (a, b, r) => a.SmoothUnion(b, r),
  },
  {
    label: 'SmoothI',
    member: 'FunctionSdf3D.SmoothIntersection',
    usesBlend: true,
    apply: (a, b, r) => a.SmoothIntersection(b, r),
  },
  {
    label: 'SmoothD',
    member: 'FunctionSdf3D.SmoothDifference',
    usesBlend: true,
    apply: (a, b, r) => a.SmoothDifference(b, r),
  },
  {
    label: 'ChamferU',
    member: 'FunctionSdf3D.ChamferUnion',
    usesBlend: true,
    apply: (a, b, w) => a.ChamferUnion(b, w),
  },
  {
    label: 'ChamferI',
    member: 'FunctionSdf3D.ChamferIntersection',
    usesBlend: true,
    apply: (a, b, w) => a.ChamferIntersection(b, w),
  },
  {
    label: 'ChamferD',
    member: 'FunctionSdf3D.ChamferDifference',
    usesBlend: true,
    apply: (a, b, w) => a.ChamferDifference(b, w),
  },
  {
    label: 'Morph',
    member: 'FunctionSdf3D.Morph',
    usesBlend: true,
    apply: (a, b, t) => a.Morph(b, Math.min(1, t / 0.6)),
  },
];

/**
 * The modifier records of `implicit-sdf.types.plato`. They are parameter records,
 * not fields: the type carries the parameters and one member — `ApplyToDistance`
 * or `ApplyToDomain` — and the SOURCE field is supplied by the caller. Composing
 * the two is what this table does, through `FunctionSdf3D.MapDomain` for the
 * domain modifiers so that even the composition is a generated member.
 */
interface Modifier {
  label: string;
  member: string;
  /** `amount` is the scene's single slider, 0..1. */
  apply(amount: number): FunctionSdf3D;
  detail(amount: number): Reading[];
}

const MODIFIER_BOX = (): FunctionSdf3D => sdfOf(p => p.DistanceToBox(new Vector3D(0.5, 0.5, 0.5)));
const MODIFIER_TALL = (): FunctionSdf3D =>
  sdfOf(p => p.DistanceToBox(new Vector3D(0.42, 0.42, 0.95)));
const MODIFIER_WIDE = (): FunctionSdf3D =>
  sdfOf(p => p.DistanceToBox(new Vector3D(1.05, 0.28, 0.28)));

const MODIFIERS: readonly Modifier[] = [
  {
    label: 'Rounding',
    member: 'SdfRoundingModifier.ApplyToDistance',
    apply: amount => {
      const modifier = new SdfRoundingModifier(0.05 + 0.3 * amount);
      const source = MODIFIER_BOX();
      return sdfOf(p => modifier.ApplyToDistance(source.Eval(p)));
    },
    detail: amount => [note('SdfRoundingModifier.Radius', n3(0.05 + 0.3 * amount))],
  },
  {
    label: 'Shell',
    member: 'SdfShellModifier.ApplyToDistance',
    apply: amount => {
      const modifier = new SdfShellModifier(0.06 + 0.34 * amount);
      const source = MODIFIER_BOX();
      return sdfOf(p => modifier.ApplyToDistance(source.Eval(p)));
    },
    detail: amount => [note('SdfShellModifier.Thickness', n3(0.06 + 0.34 * amount))],
  },
  {
    label: 'Onion',
    member: 'SdfOnionModifier.ApplyToDistance',
    apply: amount => {
      const modifier = new SdfOnionModifier(0.08 + 0.22 * amount, 3);
      const source = new Sphere(p3(0, 0, 0), 0.95).ToSdf();
      return sdfOf(p => modifier.ApplyToDistance(source.Eval(p)));
    },
    detail: amount => {
      const modifier = new SdfOnionModifier(0.08 + 0.22 * amount, 3);
      return [
        note('SdfOnionModifier.Thickness', n3(modifier.Thickness)),
        note('SdfOnionModifier.Count', String(modifier.Count)),
        reading('SdfOnionModifier.TotalDepth', () => n3(modifier.TotalDepth())),
      ];
    },
  },
  {
    label: 'Elongate',
    member: 'SdfElongationModifier3D.ApplyToDomain',
    apply: amount => {
      const modifier = new SdfElongationModifier3D(new Vector3D(0.75 * amount, 0, 0.35 * amount));
      return new Sphere(p3(0, 0, 0), 0.55).ToSdf().MapDomain(p => modifier.ApplyToDomain(p));
    },
    detail: amount => [
      note('SdfElongationModifier3D.Amount', `(${n3(0.75 * amount)}, 0.000, ${n3(0.35 * amount)})`),
    ],
  },
  {
    label: 'Repeat',
    member: 'SdfRepetitionModifier3D.ApplyToDomain',
    apply: amount => {
      const period = 0.55 + 0.5 * amount;
      const modifier = new SdfRepetitionModifier3D(
        new Vector3D(period, period, period),
        new IntegerVector3(1, 1, 1),
      );
      return sdfOf(p => p.DistanceToBox(new Vector3D(0.16, 0.16, 0.16))).MapDomain(p =>
        modifier.ApplyToDomain(p),
      );
    },
    detail: amount => {
      const period = 0.55 + 0.5 * amount;
      const modifier = new SdfRepetitionModifier3D(
        new Vector3D(period, period, period),
        new IntegerVector3(1, 1, 1),
      );
      return [
        note('SdfRepetitionModifier3D.Period', n3(period)),
        reading('SdfRepetitionModifier3D.RepetitionExtent', () => {
          const extent = modifier.RepetitionExtent();
          return `(${n3(extent.X)}, ${n3(extent.Y)}, ${n3(extent.Z)})`;
        }),
        reading('Number.RepeatedCoordinate(1.4)', () => n3((1.4).RepeatedCoordinate(period, 1))),
      ];
    },
  },
  {
    label: 'Twist',
    member: 'SdfTwistModifier3D.ApplyToDomain',
    apply: amount => {
      const modifier = new SdfTwistModifier3D(new Angle(2.6 * amount));
      return MODIFIER_TALL().MapDomain(p => modifier.ApplyToDomain(p));
    },
    detail: amount => {
      const modifier = new SdfTwistModifier3D(new Angle(2.6 * amount));
      return [
        note('SdfTwistModifier3D.AnglePerUnit', `${n3(modifier.AnglePerUnit.Radians)} rad/unit`),
        reading('ApplyToDomain(0.4, 0, 0.8)', () => pt(modifier.ApplyToDomain(p3(0.4, 0, 0.8)))),
      ];
    },
  },
  {
    label: 'Bend',
    member: 'SdfBendModifier3D.ApplyToDomain',
    apply: amount => {
      const modifier = new SdfBendModifier3D(1.7 * amount);
      return MODIFIER_WIDE().MapDomain(p => modifier.ApplyToDomain(p));
    },
    detail: amount => {
      const modifier = new SdfBendModifier3D(1.7 * amount);
      return [
        note('SdfBendModifier3D.Curvature', n3(modifier.Curvature)),
        reading('ApplyToDomain(0.9, 0.2, 0)', () => pt(modifier.ApplyToDomain(p3(0.9, 0.2, 0)))),
      ];
    },
  },
  {
    label: 'Displace',
    member: 'SdfDisplacementModifier3D.ApplyToDistance',
    apply: amount => {
      const modifier = new SdfDisplacementModifier3D(new ItemIndex(0), 0.42 * amount);
      const source = new Sphere(p3(0, 0, 0), 0.85).ToSdf();
      const field = new PerlinNoise3D(7, 1.9);
      return sdfOf(p => modifier.ApplyToDistance(source.Eval(p), field.Eval(p)));
    },
    detail: amount => {
      const modifier = new SdfDisplacementModifier3D(new ItemIndex(0), 0.42 * amount);
      const field = new PerlinNoise3D(7, 1.9);
      return [
        note('SdfDisplacementModifier3D.Amplitude', n3(modifier.Amplitude)),
        reading('SourceFieldIndex', () => String(modifier.SourceFieldIndex())),
        reading('PerlinNoise3D.Eval(0.3, 0.2, 0.1)', () => n3(field.Eval(p3(0.3, 0.2, 0.1)))),
      ];
    },
  },
];

// ---------------------------------------------------------------------------
// Scene 1 — one cube, all 256 configurations
//
// The kernel with nothing else around it. The eight corner samples are set to
// -1 or +1 straight from the bits of the configuration index, which is the
// definition `MarchingCubesCase` states, and then the index is read BACK out of
// those samples by the generated member as a live self-check.

const cellValues =
  (configuration: number): ValueAt =>
  (i, j, k) => {
    const corner = CORNER_OFFSETS.findIndex(([x, y, z]) => x === i && y === j && z === k);
    if (corner < 0) return 1;
    // Bit c is set when corner c is BELOW the iso level — Bourke's convention,
    // which is what makes the table's winding come out right.
    return ((configuration >> corner) & 1) === 1 ? -1 : 1;
  };

const cellPoints: PointAt = (i, j, k) => p3(i - 0.5, j - 0.5, k - 0.5);

const caseScene = sceneOf({
  id: 'case-table',
  title: 'One cube, 256 cases',
  description:
    'The marching-cubes kernel on a single cube. The eight corner samples are set to -1 or +1 from the '
    + 'bits of the configuration index, and IntegerVector3.MarchingCubesCase reads the index back out of '
    + 'them — the two agree for all 256 cases, which is the self-check in the status line. Green corners '
    + 'are at or above the iso level (inside the extracted region), grey ones below it; the orange cube '
    + 'edges are the cut ones, and every emitted triangle has its corners on exactly those. The triangles '
    + 'come from TriangleArray3D.MarchingCubesCaseTable, read once for the whole page.',
  plato: [
    'TriangleArray3D.MarchingCubesCaseTable',
    'Array.MarchingCubesTriangleCount',
    'Array.MarchingCubesCell',
    'IntegerVector3.MarchingCubesCase',
    'IntegerVector3.MarchingCubesCornerBit',
    'IntegerVector3.MarchingCubesCornerValue',
    'IntegerVector3.MarchingCubesCornerPoint',
    'Number.MarchingCubesCornerOffsetX',
    'Number.MarchingCubesCornerOffsetY',
    'Number.MarchingCubesCornerOffsetZ',
    'Number.MarchingCubesEdgeCornerA',
    'Number.MarchingCubesEdgeCornerB',
    'Number.MarchingCubesEdgePoint',
  ],
  viewer: { distance: 3, grid: false, spin: false },
  controls: [
    { key: 'case', label: 'Configuration', kind: 'slider', min: 0, max: 255, step: 1, def: 105 },
    { key: 'edges', label: 'Triangle edges', kind: 'toggle', def: 1 },
    { key: 'points', label: 'Edge crossings', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const configuration = Math.round(params.case);
    const valueAt = cellValues(configuration);

    const bits = Array.from({ length: 8 }, (_unused, c: number) =>
      ORIGIN_CELL.MarchingCubesCornerBit(c, valueAt, 0),
    );
    const inside: Point3D[] = [];
    const outside: Point3D[] = [];
    for (let c = 0; c < 8; c++) {
      const point = ORIGIN_CELL.MarchingCubesCornerPoint(c, cellPoints);
      (bits[c] === 1 ? outside : inside).push(point);
    }

    const cut: number[] = [];
    const whole: number[] = [];
    const cutEdges: number[] = [];
    for (let e = 0; e < 12; e++) {
      const [a, b] = EDGE_CORNERS[e];
      const pa = ORIGIN_CELL.MarchingCubesCornerPoint(a, cellPoints);
      const pb = ORIGIN_CELL.MarchingCubesCornerPoint(b, cellPoints);
      const target = bits[a] === bits[b] ? whole : cut;
      target.push(pa.X, pa.Y, pa.Z, pb.X, pb.Y, pb.Z);
      if (bits[a] !== bits[b]) cutEdges.push(e);
    }

    const cellTriangles = CASE_TABLE.MarchingCubesCell(ORIGIN_CELL, valueAt, cellPoints, 0);
    const triangles = new TriangleArray3D(cellTriangles);
    const count = triangles.Triangles.Count();

    const object = new THREE.Group();
    object.add(segments(whole, 0x2f3a4c));
    object.add(segments(cut, palette.surfaceAlt));
    if (count > 0) {
      object.add(surfaceOf(triangles));
      if (params.edges) object.add(triangleEdges(triangles, palette.line));
    }
    object.add(dots(inside, palette.accent, 11));
    object.add(dots(outside, OUTSIDE_DOT, 9));
    if (params.points) {
      // Where the surface meets each cut edge, from the generated interpolator.
      // At +/-1 corner values the crossing is the midpoint; the same member does
      // the real interpolation everywhere else on this page.
      const crossings = cutEdges.map(e =>
        e.MarchingCubesEdgePoint(ORIGIN_CELL, valueAt, cellPoints, 0),
      );
      object.add(dots(crossings, palette.line, 7));
    }

    const row = configuration * 16;
    const slots: number[] = [];
    for (let i = 0; i < count * 3; i++) slots.push(CASE_TABLE.At(row + i));

    return {
      object,
      readings: [
        note('configuration', `${configuration} (0b${configuration.toString(2).padStart(8, '0')})`),
        reading('IntegerVector3.MarchingCubesCase', () => {
          const read = ORIGIN_CELL.MarchingCubesCase(valueAt, 0);
          return read === configuration ? `${read} (agrees)` : `${read} (DISAGREES)`;
        }),
        note('corner bits (7..0)', [...bits].reverse().join('')),
        note('corners at or above iso', `${inside.length} of 8`),
        note('cut edges', cutEdges.length === 0 ? 'none' : cutEdges.join(', ')),
        reading('MarchingCubesTriangleCount', () =>
          String(CASE_TABLE.MarchingCubesTriangleCount(configuration)),
        ),
        note('triangles', String(count)),
        note('case table row', slots.length === 0 ? '(empty)' : slots.join(' ')),
        reading('table entries', () => String(CASE_TABLE.Count())),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 2 — the lattice driver
//
// The same kernel over a whole lattice, small enough to see every cube. This
// scene calls `IntegerVector3.MarchingCubesLattice` itself rather than an entry
// point, because it needs the very `valueAt` the driver is given in order to
// colour the nodes and pick out the cut cells with `MarchingCubesCase`.

const LATTICE_BOUNDS = box(1.3);

function twoBallField(separation: number, blend: number): FunctionSdf3D {
  const a = new Sphere(p3(-separation, 0, 0), 0.68).ToSdf();
  const b = new Sphere(p3(separation, separation * 0.5, 0), 0.58).ToSdf();
  return blend > 0.001 ? a.SmoothUnion(b, blend) : a.Union(b);
}

const latticeScene = sceneOf({
  id: 'lattice',
  title: 'The lattice walk',
  description:
    'IntegerVector3.MarchingCubesLattice over a coarse lattice of a smooth union of two spheres, with '
    + 'the sampling shown. Every node is drawn and coloured by IntegerVector3.MarchingCubesCornerBit — '
    + 'grey below the iso level, green at or above it — and every cube whose MarchingCubesCase is neither '
    + '0 nor 255 has its twelve edges outlined. Those outlined cubes are the only ones that emit anything: '
    + 'they are a surface through a volume, so their number grows as the square of the node count while '
    + 'the lattice grows as its cube. The SDF entry points march on the NEGATED field, and so does this '
    + 'scene, which is what puts the solid on the at-or-above side.',
  plato: [
    'IntegerVector3.MarchingCubesLattice',
    'IntegerVector3.MarchingCubesCase',
    'IntegerVector3.MarchingCubesCornerBit',
    'IntegerVector3.MarchingCubesCornerPoint',
    'Bounds3D.LatticeNodePosition',
    'Number.LatticeNodeCoordinate',
    'Sphere.ToSdf',
    'FunctionSdf3D.SmoothUnion',
    'FunctionSdf3D.Offset',
  ],
  viewer: { distance: 4.4, grid: false },
  controls: [
    { key: 'nodes', label: 'Nodes per axis', kind: 'slider', min: 4, max: 16, step: 1, def: 9 },
    { key: 'level', label: 'Iso level', kind: 'slider', min: -0.3, max: 0.5, step: 0.02, def: 0 },
    { key: 'blend', label: 'Blend radius', kind: 'slider', min: 0, max: 0.5, step: 0.02, def: 0.25 },
    { key: 'showNodes', label: 'Lattice nodes', kind: 'toggle', def: 1 },
    { key: 'showCells', label: 'Cut cubes', kind: 'toggle', def: 1 },
    { key: 'showSurface', label: 'Surface', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const n = Math.round(params.nodes);
    const nodeCounts = cube(n);
    const field = twoBallField(0.45, params.blend).Offset(params.level);

    const pointAt: PointAt = (i, j, k) => LATTICE_BOUNDS.LatticeNodePosition(nodeCounts, i, j, k);
    const valueAt: ValueAt = (i, j, k) => field.Eval(pointAt(i, j, k)).Negative();

    const started = performance.now();
    const triangles = nodeCounts.MarchingCubesLattice(valueAt, pointAt, 0);
    const elapsed = performance.now() - started;

    const object = new THREE.Group();
    object.add(boundsEdges(LATTICE_BOUNDS, 0x2a3140));

    let insideNodes = 0;
    if (params.showNodes) {
      const inside: Point3D[] = [];
      const outside: Point3D[] = [];
      for (let k = 0; k < n; k++) {
        for (let j = 0; j < n; j++) {
          for (let i = 0; i < n; i++) {
            // Corner 0 of the cube at (i, j, k) IS node (i, j, k), so the
            // generated corner test reports the node's own side.
            const below = new IntegerVector3(i, j, k).MarchingCubesCornerBit(0, valueAt, 0);
            (below === 1 ? outside : inside).push(pointAt(i, j, k));
          }
        }
      }
      insideNodes = inside.length;
      object.add(dots(outside, OUTSIDE_DOT, 3));
      object.add(dots(inside, palette.accent, 6));
    }

    let cutCells = 0;
    const cellLines: number[] = [];
    const cells = Math.max(n - 1, 0);
    for (let k = 0; k < cells; k++) {
      for (let j = 0; j < cells; j++) {
        for (let i = 0; i < cells; i++) {
          const cell = new IntegerVector3(i, j, k);
          const configuration = cell.MarchingCubesCase(valueAt, 0);
          if (configuration === 0 || configuration === 255) continue;
          cutCells++;
          if (params.showCells) cellEdgeCoordinates(cell, pointAt, cellLines);
        }
      }
    }
    if (cellLines.length > 0) object.add(segments(cellLines, palette.surfaceAlt, 0.4));

    const count = triangles.Triangles.Count();
    if (params.showSurface && count > 0) object.add(surfaceOf(triangles));

    return {
      object,
      readings: [
        note('lattice', `${n}³ nodes = ${n ** 3}, ${cells}³ cubes = ${cells ** 3}`),
        note('cut cubes', `${cutCells} (${((100 * cutCells) / Math.max(1, cells ** 3)).toFixed(1)}% of the lattice)`),
        note('triangles', String(count)),
        note('nodes at or above iso', params.showNodes ? String(insideNodes) : 'not sampled'),
        note('iso level', n3(params.level)),
        reading('Bounds3D.LatticeNodePosition(0,0,0)', () =>
          pt(LATTICE_BOUNDS.LatticeNodePosition(nodeCounts, 0, 0, 0)),
        ),
        note('MarchingCubesLattice', `${elapsed.toFixed(0)} ms`),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 3 — isosurfaces of the closed-form primitives

const primitiveScene = sceneOf({
  id: 'primitives',
  title: 'Primitive isosurfaces',
  description:
    'ISignedDistanceField3D.MarchingCubes over the closed-form distance primitives of '
    + 'implicit-sdf.library.plato. Sphere and Capsule3D have a ToSdf of their own in '
    + 'fields-implicits.library.plato; the rest are Point3D distance members packaged into a '
    + 'FunctionSdf3D, which is exactly what those ToSdf bodies do. The iso level runs the surface off '
    + 'zero through FunctionSdf3D.Offset — that is what a signed distance buys over a membership '
    + 'predicate: every level set of the field is another surface, offset by that distance. '
    + 'EikonalResidualAt reports how far the field is from a true distance (zero means exact).',
  plato: [
    'Sphere.ToSdf',
    'Capsule3D.ToSdf',
    'Plane.ToSdf',
    'Triangle3D.ToSdf',
    'Point3D.DistanceToBox',
    'Point3D.DistanceToRoundedBox',
    'Point3D.DistanceToTorus',
    'Point3D.DistanceToCappedCylinder',
    'Point3D.DistanceToCone',
    'Point3D.DistanceToCappedCone',
    'Point3D.DistanceToEllipsoid',
    'Point3D.DistanceToVerticalCapsule',
    'FunctionSdf3D.Offset',
    'FunctionSdf3D.Eval',
    'FunctionSdf3D.EikonalResidualAt',
    'ISignedDistanceField3D.MarchingCubes',
    'TriangleArray3D.Bounds',
  ],
  controls: [
    {
      key: 'shape',
      label: 'Primitive',
      kind: 'select',
      options: PRIMITIVES.map(p => p.label),
      def: 4,
    },
    { key: 'nodes', label: 'Nodes per axis', kind: 'slider', min: 12, max: 48, step: 2, def: 32 },
    { key: 'level', label: 'Iso level', kind: 'slider', min: -0.35, max: 0.6, step: 0.01, def: 0 },
    { key: 'edges', label: 'Triangle edges', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const primitive = PRIMITIVES[Math.min(PRIMITIVES.length - 1, Math.max(0, Math.round(params.shape)))];
    const n = Math.round(params.nodes);
    const nodeCounts = cube(n);
    const bounds = box(1.6);
    const offset = (primitive.base ?? 0) + params.level;
    const source = primitive.field();
    const field = offset === 0 ? source : source.Offset(offset);

    const started = performance.now();
    const triangles = field.MarchingCubes(bounds, nodeCounts);
    const elapsed = performance.now() - started;
    const count = triangles.Triangles.Count();

    const object = new THREE.Group();
    object.add(boundsEdges(bounds, 0x232b38, 0.6));
    if (count > 0) {
      object.add(surfaceOf(triangles));
      if (params.edges) object.add(triangleEdges(triangles, 0x18202b));
    }

    const readings: Reading[] = [
      note('primitive', `${primitive.label} via ${primitive.member}`),
      note('lattice', `${n}³ nodes, ${(n - 1) ** 3} cubes`),
      note('triangles', String(count)),
      note('iso level', n3(offset)),
      reading('Eval at origin', () => n3(field.Eval(p3(0, 0, 0)))),
      reading('EikonalResidualAt (0.8, 0.3, 0.2)', () => n3(field.EikonalResidualAt(p3(0.8, 0.3, 0.2)))),
      reading('TriangleArray3D.Bounds', () => {
        if (count === 0) return 'empty';
        const b = triangles.Bounds();
        return `${n3(b.Max.X - b.Min.X)} x ${n3(b.Max.Y - b.Min.Y)} x ${n3(b.Max.Z - b.Min.Z)}`;
      }),
      note('MarchingCubes', `${elapsed.toFixed(0)} ms`),
    ];
    if (primitive.caveat) readings.push(note('note', primitive.caveat));
    return { object, readings };
  },
});

// ---------------------------------------------------------------------------
// Scene 4 — the distance combinators
//
// The contrast with `csg.html`: there the booleans clip polygon loops against
// planes, here they are pointwise arithmetic on two distances and the surface is
// whatever the lattice finds afterwards. That is why the smooth and chamfered
// variants exist at all — there is no polygon-clipping spelling of them.

const combinatorScene = sceneOf({
  id: 'combinators',
  title: 'Boolean and blended fields',
  description:
    'The distance combinators of implicit-sdf.library.plato applied to two primitives, then marched. A '
    + 'union is the pointwise minimum of two distances, an intersection the maximum, a difference the '
    + 'left against the negated right — no geometry is clipped, unlike the polygon CSG on the csg page, '
    + 'and that is what makes the smooth and chamfered variants expressible: they are the same arithmetic '
    + 'with a blend. The blend slider is the blend radius for the smooth operators, the bevel width for '
    + 'the chamfered ones and the weight for Morph. SdfTree3D would express these as a stored tree, but '
    + 'its SdfNode3D and SdfCombine are sum types the TypeScript writer reports as CHK320 and does not '
    + 'emit, so the operators are composed directly.',
  plato: [
    'FunctionSdf3D.Union',
    'FunctionSdf3D.Intersection',
    'FunctionSdf3D.Difference',
    'FunctionSdf3D.ExclusiveOr',
    'FunctionSdf3D.SmoothUnion',
    'FunctionSdf3D.SmoothIntersection',
    'FunctionSdf3D.SmoothDifference',
    'FunctionSdf3D.ChamferUnion',
    'FunctionSdf3D.ChamferIntersection',
    'FunctionSdf3D.ChamferDifference',
    'FunctionSdf3D.Morph',
    'Number.UnionDistance',
    'Number.SmoothUnionDistance',
    'ISignedDistanceField3D.MarchingCubes',
  ],
  controls: [
    {
      key: 'operands',
      label: 'Operands',
      kind: 'select',
      options: OPERANDS.map(o => o.label),
      def: 0,
    },
    {
      key: 'op',
      label: 'Operator',
      kind: 'select',
      options: COMBINATORS.map(c => c.label),
      def: 4,
    },
    { key: 'shift', label: 'Separation', kind: 'slider', min: 0, max: 0.8, step: 0.01, def: 0.42 },
    { key: 'blend', label: 'Blend / width', kind: 'slider', min: 0.02, max: 0.6, step: 0.01, def: 0.3 },
    { key: 'nodes', label: 'Nodes per axis', kind: 'slider', min: 14, max: 44, step: 2, def: 32 },
  ],
  build(params: Params): Built {
    const pair = OPERANDS[Math.min(OPERANDS.length - 1, Math.max(0, Math.round(params.operands)))];
    const op = COMBINATORS[Math.min(COMBINATORS.length - 1, Math.max(0, Math.round(params.op)))];
    const n = Math.round(params.nodes);
    const nodeCounts = cube(n);
    const bounds = box(1.7);

    const left = pair.left(params.shift);
    const right = pair.right(params.shift);
    const field = op.apply(left, right, params.blend);

    const started = performance.now();
    const triangles = field.MarchingCubes(bounds, nodeCounts);
    const elapsed = performance.now() - started;
    const count = triangles.Triangles.Count();

    const object = new THREE.Group();
    object.add(boundsEdges(bounds, 0x232b38, 0.6));
    if (count > 0) object.add(surfaceOf(triangles));

    const probe = p3(0.35, 0.2, 0.1);
    return {
      object,
      readings: [
        note('operator', `${op.label} via ${op.member}`),
        note('operands', `${pair.label} — ${pair.member}`),
        note('lattice', `${n}³ nodes, ${(n - 1) ** 3} cubes`),
        note('triangles', String(count)),
        note('blend', op.usesBlend ? n3(params.blend) : 'not used by this operator'),
        reading('left.Eval(probe)', () => n3(left.Eval(probe))),
        reading('right.Eval(probe)', () => n3(right.Eval(probe))),
        reading('combined.Eval(probe)', () => n3(field.Eval(probe))),
        reading('Number.UnionDistance of the two', () =>
          n3(left.Eval(probe).UnionDistance(right.Eval(probe))),
        ),
        note('MarchingCubes', `${elapsed.toFixed(0)} ms`),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 5 — the modifier records

const modifierScene = sceneOf({
  id: 'modifiers',
  title: 'SDF modifiers',
  description:
    'The modifier records of implicit-sdf.types.plato. Each one stores parameters and nothing else: the '
    + 'source field is the caller’s, and the modifier contributes either ApplyToDistance — rounding, '
    + 'shell, onion, displacement, all of which reshape the returned distance — or ApplyToDomain, which '
    + 'moves the query point before the source ever sees it. The domain modifiers are composed here with '
    + 'FunctionSdf3D.MapDomain, so both halves of the composition are generated members. Twist and bend '
    + 'are the same trick the deformers page applies to vertices, applied instead to the sample point, '
    + 'which is why they need no mesh to act on.',
  plato: [
    'SdfRoundingModifier.ApplyToDistance',
    'SdfShellModifier.ApplyToDistance',
    'SdfOnionModifier.ApplyToDistance',
    'SdfOnionModifier.TotalDepth',
    'SdfElongationModifier3D.ApplyToDomain',
    'SdfRepetitionModifier3D.ApplyToDomain',
    'SdfRepetitionModifier3D.RepetitionExtent',
    'Number.RepeatedCoordinate',
    'SdfTwistModifier3D.ApplyToDomain',
    'SdfBendModifier3D.ApplyToDomain',
    'Point3D.RotatedAboutZ',
    'SdfDisplacementModifier3D.ApplyToDistance',
    'SdfDisplacementModifier3D.SourceFieldIndex',
    'FunctionSdf3D.MapDomain',
    'ISignedDistanceField3D.MarchingCubes',
  ],
  controls: [
    {
      key: 'modifier',
      label: 'Modifier',
      kind: 'select',
      options: MODIFIERS.map(m => m.label),
      def: 5,
    },
    { key: 'amount', label: 'Amount', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0.6 },
    { key: 'nodes', label: 'Nodes per axis', kind: 'slider', min: 14, max: 40, step: 2, def: 28 },
    { key: 'edges', label: 'Triangle edges', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const modifier = MODIFIERS[Math.min(MODIFIERS.length - 1, Math.max(0, Math.round(params.modifier)))];
    const n = Math.round(params.nodes);
    const nodeCounts = cube(n);
    const bounds = box(1.6);
    const field = modifier.apply(params.amount);

    const started = performance.now();
    const triangles = field.MarchingCubes(bounds, nodeCounts);
    const elapsed = performance.now() - started;
    const count = triangles.Triangles.Count();

    const object = new THREE.Group();
    object.add(boundsEdges(bounds, 0x232b38, 0.6));
    if (count > 0) {
      object.add(surfaceOf(triangles));
      if (params.edges) object.add(triangleEdges(triangles, 0x18202b));
    }

    return {
      object,
      readings: [
        note('modifier', `${modifier.label} via ${modifier.member}`),
        note('lattice', `${n}³ nodes, ${(n - 1) ** 3} cubes`),
        note('triangles', String(count)),
        ...modifier.detail(params.amount),
        reading('Eval at origin', () => n3(field.Eval(p3(0, 0, 0)))),
        note('MarchingCubes', `${elapsed.toFixed(0)} ms`),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 6 — metaballs
//
// The friendliest implicit surface there is: a sum of radial falloff kernels,
// with the surface wherever the sum reaches a threshold. `MetaBallSystem3D` is an
// IScalarField3D rather than an SDF, so it reaches the ISO-LEVEL entry point —
// which is the overload the SDF types do not get, because their own entry point
// negates the field and fixes the level at zero.

function ballRing(count: number, spread: number, radius: number, carve: boolean): IArray<MetaBall3D> {
  const balls: MetaBall3D[] = [];
  for (let i = 0; i < count; i++) {
    const a = (i / count) * Math.PI * 2;
    const lift = ((i % 3) - 1) * spread * 0.55;
    balls.push(
      new MetaBall3D(p3(Math.cos(a) * spread, Math.sin(a) * spread, lift), radius, 1),
    );
  }
  if (carve) balls.push(new MetaBall3D(p3(0, 0, 0), radius * 1.15, -1.4));
  return Intrinsics.MakeArray(...balls);
}

const metaballScene = sceneOf({
  id: 'metaballs',
  title: 'Metaballs',
  description:
    'MetaBallSystem3D.MarchingCubes. Each MetaBall3D contributes Strength times MetaBallFalloff of the '
    + 'distance to it over its radius — a cubic that is 1 at the centre and 0 at the rim — and the '
    + 'system’s Eval sums them and subtracts Threshold, so the zero level set is the blob surface. '
    + 'Because the sum is a scalar field and not a distance field it takes the ISO-LEVEL overload of '
    + 'MarchingCubes, the one the SDF types do not get. A negative strength carves rather than adds.',
  plato: [
    'MetaBall3D.InfluenceAt',
    'Number.MetaBallFalloff',
    'MetaBallSystem3D.Eval',
    'IScalarField3D.MarchingCubes',
    'IScalarField3D.IsAtOrAboveLevel',
  ],
  controls: [
    { key: 'balls', label: 'Balls', kind: 'slider', min: 2, max: 9, step: 1, def: 5 },
    { key: 'spread', label: 'Ring radius', kind: 'slider', min: 0.15, max: 0.9, step: 0.01, def: 0.5 },
    { key: 'radius', label: 'Ball radius', kind: 'slider', min: 0.5, max: 1.5, step: 0.02, def: 0.95 },
    { key: 'threshold', label: 'Threshold', kind: 'slider', min: 0.1, max: 1.4, step: 0.02, def: 0.5 },
    { key: 'carve', label: 'Carve with a negative ball', kind: 'toggle', def: 0 },
    { key: 'nodes', label: 'Nodes per axis', kind: 'slider', min: 14, max: 40, step: 2, def: 28 },
  ],
  build(params: Params): Built {
    const n = Math.round(params.nodes);
    const nodeCounts = cube(n);
    const bounds = box(1.8);
    const balls = ballRing(
      Math.round(params.balls),
      params.spread,
      params.radius,
      params.carve === 1,
    );
    const system = new MetaBallSystem3D(balls, params.threshold);

    const started = performance.now();
    const triangles = system.MarchingCubes(bounds, nodeCounts, 0);
    const elapsed = performance.now() - started;
    const count = triangles.Triangles.Count();

    const object = new THREE.Group();
    object.add(boundsEdges(bounds, 0x232b38, 0.6));
    if (count > 0) object.add(surfaceOf(triangles, palette.accent));
    object.add(dots(toArray(balls).map(b => b.Center), palette.surfaceAlt, 7));

    const origin = p3(0, 0, 0);
    return {
      object,
      readings: [
        note('balls', `${balls.Count()}${params.carve === 1 ? ' (one negative)' : ''}`),
        note('threshold', n3(params.threshold)),
        note('lattice', `${n}³ nodes, ${(n - 1) ** 3} cubes`),
        note('triangles', String(count)),
        reading('MetaBallSystem3D.Eval at origin', () => n3(system.Eval(origin))),
        reading('MetaBall3D.InfluenceAt(origin) of ball 0', () =>
          n3(balls.At(0).InfluenceAt(origin)),
        ),
        reading('Number.MetaBallFalloff(0.5)', () => n3((0.5).MetaBallFalloff())),
        reading('IsAtOrAboveLevel(origin, 0)', () => String(system.IsAtOrAboveLevel(origin, 0))),
        note('MarchingCubes', `${elapsed.toFixed(0)} ms`),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 7 — noise volumes
//
// A noise field is the case marching cubes is least like a mesher for: there is
// no solid, only a level set of a function that has one everywhere. The lattice
// is deliberately small — a spatial noise Eval is the expensive kind, and the
// cut-cell count here is a large fraction of the lattice rather than a thin
// shell, so both terms of the cost grow at once.

interface NoiseVolume {
  label: string;
  member: string;
  field(frequency: number): { Eval(p: Point3D): number; MarchingCubes(b: Bounds3D, n: IntegerVector3, iso: number): TriangleArray3D };
}

const NOISE_VOLUMES: readonly NoiseVolume[] = [
  {
    label: 'Perlin',
    member: 'PerlinNoise3D.MarchingCubes',
    field: frequency => new PerlinNoise3D(7, frequency),
  },
  {
    label: 'Value',
    member: 'ValueNoise3D.MarchingCubes',
    field: frequency => new ValueNoise3D(7, frequency),
  },
];

const noiseScene = sceneOf({
  id: 'noise-volume',
  title: 'Noise volumes',
  description:
    'The iso level set of a spatial noise field, through the IScalarField3D overload of MarchingCubes. '
    + 'A noise volume has no inside to bound, so what comes back is the boundary between the region '
    + 'where the field is at or above the level and the region below it, and moving the level does not '
    + 'offset a surface — it picks a different one. The lattice is kept small on purpose: a spatial '
    + 'noise Eval is expensive and a noise field crosses the level nearly everywhere, so a large fraction '
    + 'of the cubes are cut cubes and the triangle count grows with the whole lattice instead of with the '
    + 'thin shell a solid gives. The noise page owns the basis gallery; this scene stays on the isosurface.',
  plato: [
    'PerlinNoise3D.Eval',
    'ValueNoise3D.Eval',
    'IScalarField3D.MarchingCubes',
    'IScalarField3D.IsAtOrAboveLevel',
    'TriangleArray3D.Triangles',
  ],
  viewer: { distance: 4.8 },
  controls: [
    {
      key: 'basis',
      label: 'Basis',
      kind: 'select',
      options: NOISE_VOLUMES.map(v => v.label),
      def: 0,
    },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 0.8, max: 3, step: 0.1, def: 1.6 },
    { key: 'level', label: 'Iso level', kind: 'slider', min: -0.5, max: 0.5, step: 0.02, def: 0 },
    { key: 'nodes', label: 'Nodes per axis', kind: 'slider', min: 10, max: 26, step: 2, def: 18 },
  ],
  build(params: Params): Built {
    const volume = NOISE_VOLUMES[Math.min(NOISE_VOLUMES.length - 1, Math.max(0, Math.round(params.basis)))];
    const n = Math.round(params.nodes);
    const nodeCounts = cube(n);
    const bounds = box(1.5);
    const field = volume.field(params.frequency);

    const started = performance.now();
    const triangles = field.MarchingCubes(bounds, nodeCounts, params.level);
    const elapsed = performance.now() - started;
    const count = triangles.Triangles.Count();

    const object = new THREE.Group();
    object.add(boundsEdges(bounds, 0x232b38, 0.6));
    if (count > 0) object.add(surfaceOf(triangles, palette.surfaceAlt));

    const cubes = (n - 1) ** 3;
    return {
      object,
      readings: [
        note('field', `${volume.label} via ${volume.member}`),
        note('frequency', n3(params.frequency)),
        note('iso level', n3(params.level)),
        note('lattice', `${n}³ nodes, ${cubes} cubes`),
        note('triangles', String(count)),
        note('triangles per cube', (count / Math.max(1, cubes)).toFixed(2)),
        reading('Eval at origin', () => n3(field.Eval(p3(0, 0, 0)))),
        note('MarchingCubes', `${elapsed.toFixed(0)} ms`),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 8 — what the node count costs

interface CostField {
  label: string;
  member: string;
  field(): FunctionSdf3D;
}

const COST_FIELDS: readonly CostField[] = [
  { label: 'Sphere', member: 'Sphere.ToSdf', field: () => new Sphere(p3(0, 0, 0), 0.85).ToSdf() },
  {
    label: 'SmoothU',
    member: 'FunctionSdf3D.SmoothUnion',
    field: () => twoBallField(0.4, 0.3),
  },
  {
    label: 'Torus',
    member: 'Point3D.DistanceToTorus',
    field: () => sdfOf(p => p.DistanceToTorus(0.62, 0.26)),
  },
];

const COST_SPACING = 2.3;

const resolutionScene = sceneOf({
  id: 'resolution',
  title: 'Resolution and cost',
  description:
    'One field, marched three times at increasing node counts and shown side by side. The lattice is '
    + 'cubic in the node count and the surface is a two-dimensional slice through it, so doubling the '
    + 'nodes roughly quadruples the triangle count and multiplies the work by eight — the status line '
    + 'reports both for each copy. Marching cubes returns unwelded triangles: neighbouring cubes emit '
    + 'their own copies of a shared corner, which is why the shared mesh helper takes face normals '
    + 'rather than averaging them.',
  plato: [
    'ISignedDistanceField3D.MarchingCubes',
    'IntegerVector3.MarchingCubesLattice',
    'Sphere.ToSdf',
    'FunctionSdf3D.SmoothUnion',
    'Point3D.DistanceToTorus',
    'TriangleArray3D.Triangles',
  ],
  viewer: { distance: 9, spin: false },
  controls: [
    {
      key: 'field',
      label: 'Field',
      kind: 'select',
      options: COST_FIELDS.map(f => f.label),
      def: 0,
    },
    { key: 'base', label: 'Coarsest nodes', kind: 'slider', min: 5, max: 14, step: 1, def: 9 },
    { key: 'edges', label: 'Triangle edges', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const choice = COST_FIELDS[Math.min(COST_FIELDS.length - 1, Math.max(0, Math.round(params.field)))];
    const base = Math.round(params.base);
    const bounds = box(1.2);
    const field = choice.field();

    const object = new THREE.Group();
    const readings: Reading[] = [
      note('field', `${choice.label} via ${choice.member}`),
    ];

    for (let step = 0; step < 3; step++) {
      const n = base * (step + 1);
      const started = performance.now();
      const triangles = field.MarchingCubes(bounds, cube(n));
      const elapsed = performance.now() - started;
      const count = triangles.Triangles.Count();

      const group = new THREE.Group();
      group.position.x = (step - 1) * COST_SPACING;
      group.add(boundsEdges(bounds, 0x232b38, 0.5));
      if (count > 0) {
        group.add(surfaceOf(triangles));
        if (params.edges) group.add(triangleEdges(triangles, 0x18202b));
      }
      object.add(group);

      readings.push(
        note(`${n}³ nodes`, `${count} triangles, ${(n - 1) ** 3} cubes, ${elapsed.toFixed(0)} ms`),
      );
    }
    return { object, readings };
  },
});

// ---------------------------------------------------------------------------

const demo: Demo = {
  title: 'Marching cubes',
  subtitle:
    'voxels.library.plato · implicit-sdf.{types,library}.plato · fields-implicits.library.plato · meshes.types.plato',
  scenes: [
    caseScene,
    latticeScene,
    primitiveScene,
    combinatorScene,
    modifierScene,
    metaballScene,
    noiseScene,
    resolutionScene,
  ],
};

mountDemo(demo, { distance: 4.6 });

export { demo };
