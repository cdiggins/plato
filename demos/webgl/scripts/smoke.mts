// Gate: the generated stdlib TypeScript, plus the prelude in
// src/plato/array-ext.ts, must actually evaluate the members these demos build on.
//
//   npm run smoke
//
// Every check is a value the Plato source pins down (face counts of the Conway
// operators, the area of a unit square, the parity test inside a cube), so a
// regression in either the writer or the prelude shows up as a wrong number
// rather than a blank screen in the browser.

import '../src/plato/array-ext.ts';
import {
  Intrinsics,
  Angle,
  Bounds3D,
  Point2D,
  Point3D,
  Vector3D,
  Direction3D,
  Polygon2D,
  PolygonSet2D,
  PolygonWithHoles2D,
  Polygon3D,
  PolygonSoup3D,
  PolygonMesh3D,
  Twist3D,
  Twist2D,
  Bend3D,
  Taper3D,
  BezierCurve3D,
  CatmullRomCurve3D,
  DomainWarpNoise2D,
  Epicycloid2D,
  Epitrochoid2D,
  FbmNoise2D,
  Hypocycloid2D,
  Hypotrochoid2D,
  IntegerVector3,
  Matrix4x4,
  PerlinNoise2D,
  PerlinNoise3D,
  RidgedNoise2D,
  Sphere,
  Translation3D,
  TriangleArray3D,
  TurbulenceNoise2D,
  WhiteNoise3D,
  WorleyNoise2D,
} from '../src/plato/plato.g.ts';

// The sum types `plato.g.ts` reports as CHK320 and the prelude supplies.
const NoiseBasis = (globalThis as any).NoiseBasis;
const WorleyDistance = (globalThis as any).WorleyDistance;
const WorleyFeature = (globalThis as any).WorleyFeature;

let failures = 0;

function check(name: string, actual: unknown, expected: unknown): void {
  const ok = Object.is(actual, expected);
  if (!ok) failures++;
  console.log(`${ok ? 'ok  ' : 'FAIL'} ${name}: ${actual}${ok ? '' : ` (expected ${expected})`}`);
}

function close(name: string, actual: number, expected: number, tol = 1e-9): void {
  const ok = Math.abs(actual - expected) <= tol;
  if (!ok) failures++;
  console.log(`${ok ? 'ok  ' : 'FAIL'} ${name}: ${actual}${ok ? '' : ` (expected ${expected})`}`);
}

// --- Platonic seeds (polyhedra.library.plato) -------------------------------

check('Tetrahedron faces', PolygonMesh3D.Tetrahedron().FaceCount(), 4);
check('Cube faces', PolygonMesh3D.Cube().FaceCount(), 6);
check('Octahedron faces', PolygonMesh3D.Octahedron().FaceCount(), 8);
check('Dodecahedron faces', PolygonMesh3D.Dodecahedron().FaceCount(), 12);
check('Icosahedron faces', PolygonMesh3D.Icosahedron().FaceCount(), 20);
check('Cube vertices', PolygonMesh3D.Cube().VertexCount(), 8);

// --- Conway operators -------------------------------------------------------

const cube = PolygonMesh3D.Cube();
check('Cube.Ambo faces (cuboctahedron)', cube.Ambo().FaceCount(), 14);
check('Cube.Truncate faces', cube.Truncate().FaceCount(), 14);
check('Cube.Dual faces (octahedron)', cube.Dual().FaceCount(), 8);
check('Cube.Cuboctahedron faces', cube.Cuboctahedron().FaceCount(), 14);
check('Icosahedron.Truncate faces (football)', PolygonMesh3D.Icosahedron().Truncate().FaceCount(), 32);
check('Cube triangle count', cube.ToTriangleMesh().Faces.Count(), 12);

// --- Polygons (polygons.library.plato, geometry.library.plato) --------------

const square = new Polygon2D(
  Intrinsics.MakeArray(new Point2D(-1, -1), new Point2D(1, -1), new Point2D(1, 1), new Point2D(-1, 1)),
);
close('square Area', square.Area(), 4);
close('square SignedArea', square.SignedArea(), 4);
close('square Perimeter', square.Perimeter(), 8);
close('square Centroid.X', square.Centroid().X, 0);
check('square contains origin', square.Contains(new Point2D(0, 0)), true);
check('square excludes (5,0)', square.Contains(new Point2D(5, 0)), false);
check('square Bounds.Max.X', square.Bounds().Max.X, 1);
check('square Winding', String(square.Winding()), 'CounterClockwise');
check('square IsSimple', square.IsSimple(), true);
close('square IsoperimetricQuotient', square.IsoperimetricQuotient(), Math.PI / 4);
check('square ClosestPoint to (5,0)', square.ClosestPoint(new Point2D(5, 0)).X, 1);

const bowtie = new Polygon2D(
  Intrinsics.MakeArray(new Point2D(-1, -1), new Point2D(1, 1), new Point2D(1, -1), new Point2D(-1, 1)),
);
check('bowtie is not simple', bowtie.IsSimple(), false);
check('bowtie self-intersections', bowtie.SelfIntersectionCount(), 1);

// A duplicated vertex and a collinear midpoint, removed one at a time.
const messy = new Polygon2D(
  Intrinsics.MakeArray(
    new Point2D(-1, -1),
    new Point2D(0, -1),
    new Point2D(1, -1),
    new Point2D(1, 1),
    new Point2D(1, 1),
    new Point2D(-1, 1),
  ),
);
check('RemoveDuplicateVertices drops the repeat', messy.RemoveDuplicateVertices().Points.Count(), 5);
check('RemoveCollinearVertices reaches the 4 corners', messy.RemoveCollinearVertices().Points.Count(), 4);

// PolygonWithHoles2D.IsSimple goes through RingsAreDisjoint.
const ring = (points: [number, number][]) =>
  new Polygon2D(Intrinsics.MakeArray(...points.map(([x, y]) => new Point2D(x, y))));
const outer = ring([[-2, -2], [2, -2], [2, 2], [-2, 2]]);
check(
  'region with a contained hole is simple',
  new PolygonWithHoles2D(outer, Intrinsics.MakeArray(ring([[-1, -1], [1, -1], [1, 1], [-1, 1]]))).IsSimple(),
  true,
);
check(
  'region whose hole crosses the boundary is not',
  new PolygonWithHoles2D(outer, Intrinsics.MakeArray(ring([[-3, -1], [3, -1], [3, 1], [-3, 1]]))).IsSimple(),
  false,
);

// --- CSG (solids-csg.library.plato) -----------------------------------------

function toSoup(m: PolygonMesh3D): PolygonSoup3D {
  return new PolygonSoup3D(Intrinsics.Range(m.FaceCount()).Map(f => new Polygon3D(m.FacePositions(f))));
}

const soup = toSoup(cube);
check('soup polygons', soup.Polygons.Count(), 6);
check('soup planes', soup.Planes().Count(), 6);
check('soup contains origin', soup.Contains(new Point3D(0, 0, 0)), true);
check('soup excludes far point', soup.Contains(new Point3D(9, 9, 9)), false);

const shifted = toSoup(cube.Deform(p => new Point3D(p.X + 0.6, p.Y, p.Z)));
check('union is non-empty', soup.Union(shifted).Polygons.Count() > 0, true);
check('intersection is non-empty', soup.Intersection(shifted).Polygons.Count() > 0, true);
check('difference is non-empty', soup.Difference(shifted).Polygons.Count() > 0, true);

// --- Deformers (deformations.library.plato) ---------------------------------

const yAxis = new Direction3D(new Vector3D(0, 1, 0));
const xAxis = new Direction3D(new Vector3D(1, 0, 0));

const twist = new Twist3D(new Point3D(0, 0, 0), yAxis, 1.0);
const twisted = twist.Eval(new Point3D(1, 1, 0));
close('Twist3D at height 1 rotates by 1 rad (X)', twisted.X, Math.cos(1));
close('Twist3D at height 1 rotates by 1 rad (Z)', twisted.Z, -Math.sin(1));
close('Twist3D fixes the axis', twist.Eval(new Point3D(0, 2, 0)).Y, 2);

const bend = new Bend3D(new Point3D(0, 0, 0), yAxis, xAxis, 0.5);
check('Bend3D deforms a mesh', cube.Deform(p => bend.Eval(p)).VertexCount(), 8);

const taper = new Taper3D(new Point3D(0, 0, 0), yAxis, 0.5);
close('Taper3D scales off-axis by 1 + rate * t', taper.Eval(new Point3D(1, 1, 0)).X, 1.5);

// Twist2D goes through Vector2D.Transform(Rotation2D) — the 2D case of the
// dropped-overload defect.
const twist2D = new Twist2D(new Point2D(0, 0), 0.5);
close('Twist2D rotates by anglePerUnit * radius', twist2D.Eval(new Point2D(1, 0)).X, Math.cos(0.5));

// ScaleX goes through the non-uniform Scale overload the writer skipped.
const stretched = cube.ScaleX(2);
close('ScaleX doubles X', stretched.Positions.At(0).X, cube.Positions.At(0).X * 2);
close('ScaleX leaves Y', stretched.Positions.At(0).Y, cube.Positions.At(0).Y);

// --- Triangulation (triangulation.library.plato) ----------------------------
//
// Ear clipping turns a simple ring of n vertices into exactly n - 2 faces, and a
// ring bridged to h holes into n + 2h - 2. The faces index the pool they were
// built from, so their signed areas sum to the region's area — which also proves
// every face came out counter-clockwise.

const ring2 = (points: [number, number][]) =>
  new Polygon2D(Intrinsics.MakeArray(...points.map(([x, y]) => new Point2D(x, y))));

function triangulatedArea(mesh: { Positions: any; Faces: any }): number {
  let total = 0;
  for (let i = 0; i < mesh.Faces.Count(); i++) {
    const f = mesh.Faces.At(i);
    const a = mesh.Positions.At(f.A.Value);
    const b = mesh.Positions.At(f.B.Value);
    const c = mesh.Positions.At(f.C.Value);
    total += 0.5 * ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X));
  }
  return total;
}

const lShape = ring2([[0, 0], [2, 0], [2, 1], [1, 1], [1, 2], [0, 2]]);
check('L-shape triangulates to n - 2 faces', lShape.Triangulate().Faces.Count(), 4);
close('L-shape triangle areas sum to its area', triangulatedArea(lShape.Triangulate()), 3);

const holed = new PolygonWithHoles2D(
  ring2([[-2, -2], [2, -2], [2, 2], [-2, 2]]),
  Intrinsics.MakeArray(ring2([[-1, -1], [-1, 1], [1, 1], [1, -1]])),
);
check('one bridged hole gives n + 2h - 2 faces', holed.Triangulate().Faces.Count(), 8);
close('region areas sum to outer minus hole', triangulatedArea(holed.Triangulate()), 12);

// PolygonSet2D goes through ShiftedFaces, which rebases each component's faces
// onto the shared pool — so the areas still add up.
const twoSquares = new PolygonSet2D(
  Intrinsics.MakeArray(
    new PolygonWithHoles2D(ring2([[0, 0], [1, 0], [1, 1], [0, 1]]), Intrinsics.MakeArray()),
    new PolygonWithHoles2D(ring2([[3, 0], [5, 0], [5, 2], [3, 2]]), Intrinsics.MakeArray()),
  ),
);
check('two components give 2 + 2 faces', twoSquares.Triangulate().Faces.Count(), 4);
close('shifted faces keep both areas', triangulatedArea(twoSquares.Triangulate()), 5);

check(
  'Polygon3D.ToTriangleMesh triangulates in its own plane',
  new Polygon3D(
    Intrinsics.MakeArray(
      new Point3D(0, 0, 0),
      new Point3D(1, 0, 0),
      new Point3D(1, 1, 0),
      new Point3D(0, 1, 0),
    ),
  )
    .ToTriangleMesh()
    .Faces.Count(),
  2,
);

// --- Splines (splines.library.plato) -----------------------------------------
//
// De Casteljau at the ends of the interval is the first and last control point
// exactly, whatever the degree.

const bezier = new BezierCurve3D(
  Intrinsics.MakeArray(
    new Point3D(0, 0, 0),
    new Point3D(1, 2, 0),
    new Point3D(2, -1, 1),
    new Point3D(3, 0, 0),
  ),
);
close('DeCasteljau at t=0 is the first control point', bezier.Eval(0).X, 0);
close('DeCasteljau at t=1 is the last control point', bezier.Eval(1).X, 3);
// The X coordinates are equally spaced, so the cubic in X reduces to 3t.
close('DeCasteljau of an evenly spaced polygon is linear', bezier.Eval(0.4).X, 1.2);

// AtWrapped clamps on an open curve, so t = 0 lands on the first point.
const catmull = new CatmullRomCurve3D(
  Intrinsics.MakeArray(
    new Point3D(0, 0, 0),
    new Point3D(1, 1, 0),
    new Point3D(2, 0, 1),
    new Point3D(3, 1, 0),
  ),
  0.5,
  false,
);
close('AtWrapped clamps: Catmull-Rom starts on its first point', catmull.Eval(0).X, 0);

// --- Scalar on the left of Multiply (algebra.library.plato) -------------------
//
// `Multiply(scalar: Number, x: IScalable) => x * scalar`. The roulette curves
// are the callers that need it: each multiplies a turn count by an Angle.

close('a scalar times an Angle is an Angle', (2 as any).Multiply(new Angle(3)).Radians, 6);
// At t = 0 every roulette sits on the +X axis at the sum of its radii.
close('Epicycloid2D at t=0 is (R, 0)', new Epicycloid2D(3, 1).Eval(0).X, 3);
close('Hypocycloid2D at t=0 is (R, 0)', new Hypocycloid2D(3, 1).Eval(0).X, 3);
close('Epitrochoid2D at t=0 is (R + r - d, 0)', new Epitrochoid2D(3, 1, 0.5).Eval(0).X, 3.5);
close('Hypotrochoid2D at t=0 is (R - r + d, 0)', new Hypotrochoid2D(5, 3, 5).Eval(0).X, 7);

// --- Transforms (transforms.library.plato) -----------------------------------
//
// `Point3D.Transform(AffineTransform3D)` scales the matrix rows by a scalar,
// which is the NumberN scalar overload.

const identityMoved = new Point3D(1, 2, 3).Transform(Matrix4x4.Identity().AffineTransform3D());
close('identity affine fixes a point (X)', identityMoved.X, 1);
close('identity affine fixes a point (Z)', identityMoved.Z, 3);
const translated = new Point3D(1, 1, 1).Transform(
  new Translation3D(new Vector3D(1, 2, 3)).AffineTransform3D(),
);
close('translation affine adds the vector (Y)', translated.Y, 3);

// --- Noise (noise.library.plato) ---------------------------------------------
//
// Perlin noise is zero at every lattice corner by construction: each corner's
// contribution is a dot product with the offset to the query point.

close('PerlinNoise2D is zero on the lattice', new PerlinNoise2D(7, 4).Eval(new Point2D(0.25, 0.5)), 0);
close(
  'PerlinNoise3D is zero on the lattice',
  new PerlinNoise3D(7, 4).Eval(new Point3D(0.25, 0.5, 0.75)),
  0,
);

// The four-argument LatticeHash overload: without it every spatial noise drops
// its z index and the field is constant along z.
check(
  'WhiteNoise3D varies along z',
  new WhiteNoise3D(7).Eval(new Point3D(0.1, 0.2, 0.3)) !==
    new WhiteNoise3D(7).Eval(new Point3D(0.1, 0.2, 0.9)),
  true,
);

// A one-octave fractal sum with gain 1 has total weight 1, so it IS its basis.
const noisePoint = new Point2D(0.3, 0.7);
const basisValue = new PerlinNoise2D(7, 3).Eval(noisePoint);
close(
  'FbmNoise2D of one octave is its basis',
  new FbmNoise2D(NoiseBasis.Perlin(), 7, 3, 1, 2, 1).Eval(noisePoint),
  basisValue,
);
close(
  'TurbulenceNoise2D of one octave is |basis|',
  new TurbulenceNoise2D(NoiseBasis.Perlin(), 7, 3, 1, 2, 1).Eval(noisePoint),
  Math.abs(basisValue),
);
close(
  'RidgedNoise2D at offset 0 is basis squared',
  new RidgedNoise2D(NoiseBasis.Perlin(), 7, 3, 1, 2, 1, 0).Eval(noisePoint),
  basisValue * basisValue,
);
close(
  'DomainWarpNoise2D with no iterations is its basis',
  new DomainWarpNoise2D(NoiseBasis.Perlin(), 7, 3, 0.4, 2, 0).Eval(noisePoint),
  basisValue,
);

// With no jitter every feature point sits at its cell centre, so a query at a
// cell centre is exactly on one.
close(
  'WorleyNoise2D F1 is zero at an unjittered feature point',
  new WorleyNoise2D(7, 4, 0, WorleyDistance.Euclidean(), WorleyFeature.F1()).Eval(
    new Point2D(0.125, 0.125),
  ),
  0,
);

// --- Marching cubes (voxels.library.plato) -----------------------------------
//
// The case table's first entry per row terminates the empty configurations, and
// the corner offsets are Integer arithmetic: corner 1 is (1, 0, 0), corner 7 is
// (1, 1, 1).

const caseTable: any = TriangleArray3D.MarchingCubesCaseTable();
check('case table is 256 rows of 16', caseTable.Count(), 4096);
check('configuration 0 emits no triangles', caseTable.MarchingCubesTriangleCount(0), 0);
check('configuration 255 emits no triangles', caseTable.MarchingCubesTriangleCount(255), 0);
check('configuration 1 emits one triangle', caseTable.MarchingCubesTriangleCount(1), 1);
const cornerOffset = (c: number): string =>
  [
    (c as any).MarchingCubesCornerOffsetX(),
    (c as any).MarchingCubesCornerOffsetY(),
    (c as any).MarchingCubesCornerOffsetZ(),
  ].join(',');
check('cube corner 0 is (0, 0, 0)', cornerOffset(0), '0,0,0');
check('cube corner 1 is (1, 0, 0)', cornerOffset(1), '1,0,0');
check('cube corner 2 is (1, 1, 0)', cornerOffset(2), '1,1,0');
check('cube corner 3 is (0, 1, 0)', cornerOffset(3), '0,1,0');
check('cube corner 7 is (0, 1, 1)', cornerOffset(7), '0,1,1');

// The isosurface of a unit sphere's SDF must land on the unit sphere, to within
// the linear interpolation error of the lattice it was sampled on.
const marched = new Sphere(new Point3D(0, 0, 0), 1)
  .ToSdf()
  .MarchingCubes(
    new Bounds3D(new Point3D(-1.5, -1.5, -1.5), new Point3D(1.5, 1.5, 1.5)),
    new IntegerVector3(24, 24, 24),
  );
check('marching a sphere produces triangles', marched.Triangles.Count() > 0, true);
let worstRadius = 0;
for (let i = 0; i < marched.Triangles.Count(); i++) {
  const t: any = marched.Triangles.At(i);
  for (const p of [t.A, t.B, t.C]) {
    worstRadius = Math.max(worstRadius, Math.abs(Math.hypot(p.X, p.Y, p.Z) - 1));
  }
}
check('every marched vertex lands on the unit sphere', worstRadius < 0.01, true);

// --- Array3D (primitives.library.plato) --------------------------------------
//
// `MakeArray3D` builds a row-major volume, so cell (c, r, l) is element
// (l * rows + r) * columns + c.

const volume: any = (4 as any).MakeArray3D(3, 2, (i: number, j: number, k: number) => i + 10 * j + 100 * k);
check('MakeArray3D columns', volume.ColumnCount, 4);
check('MakeArray3D rows', volume.RowCount, 3);
check('MakeArray3D layers', volume.LayerCount, 2);
check('MakeArray3D indexes row-major', volume.At(2, 1, 1), 2 + 10 + 100);
check('MakeArray3D cell count', volume.Count(), 24);

if (failures > 0) {
  console.error(`${failures} smoke check(s) failed`);
  process.exit(1);
}
console.log('smoke: all checks passed');
