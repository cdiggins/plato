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
  Area,
  Beam,
  BeamSupport,
  BlueNoisePattern2D,
  Bounds2D,
  ElasticModel2D,
  NodalForce2D,
  SectionProperties,
  TriangleMesh2D,
  Vector2D,
  ClothGrid3D,
  ClothSolverSettings,
  ContactSolverSettings,
  Density,
  DofIndex,
  Duration,
  ElasticModel3D,
  EngineeringMaterial,
  FaceTraction3D,
  IntegerVector2,
  JitteredGridPattern2D,
  LatticeUnitCell,
  Length,
  LinearSolveSettings,
  Mass,
  ParticleForces3D,
  ParticleGravity,
  PrescribedDisplacement,
  Pressure,
  Quaternion,
  RigidWorld3D,
  SolverBody3D,
  SpatialVelocity3D,
  Speed,
  TetrahedralMesh3D,
  TetrahedronCell,
  ThermalConductivity,
  TimeStepSettings,
  TpmsField3D,
  TriangleFace,
  VertexIndex,
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
const TpmsFamily = (globalThis as any).TpmsFamily;
const PlaneCondition = (globalThis as any).PlaneCondition;
const BeamRestraint = (globalThis as any).BeamRestraint;
const BeamLoad = (globalThis as any).BeamLoad;

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

// --- Lattices (lattices.library.plato) ---------------------------------------
//
// A unit cell's node valences are fixed by crystallography, not by this code:
// simple cubic is 6-connected, body-centred 8, face-centred 12 at a cube corner
// and 4 at a face node, the octet truss 12 everywhere, diamond 4 and the Kelvin
// cell (truncated octahedron) 4. Welding is what makes those numbers come out —
// `NodeValence` counts a strut only when this cell owns it — so a break here
// says the ownership rule moved.

function valences(cell: any): number[] {
  const list = cell.NodeValences();
  const out: number[] = [];
  for (let i = 0; i < list.Count(); i++) out.push(list.At(i));
  return out;
}
const distinct = (xs: number[]): string => [...new Set(xs)].sort((a, b) => a - b).join(',');

check('simple cubic is 6-connected', distinct(valences(LatticeUnitCell.SimpleCubic())), '6');
check('body-centred cubic is 8-connected', distinct(valences(LatticeUnitCell.BodyCenteredCubic())), '8');
check('octet truss is 12-connected', distinct(valences(LatticeUnitCell.OctetTruss())), '12');
check('diamond cubic is 4-connected', distinct(valences(LatticeUnitCell.DiamondCubic())), '4');
check('Kelvin cell is 4-connected', distinct(valences(LatticeUnitCell.TruncatedOctahedron())), '4');
// Face-centred cubic is the one with two node kinds: the eight cube corners are
// 12-connected and the six face centres 4-connected.
const fcc = valences(LatticeUnitCell.FaceCenteredCubic());
check('face-centred cubic corner valence', fcc[0], 12);
check('face-centred cubic face-node valence', fcc[8], 4);

// `StrutLattice3D.Cells` enumerates the cell grid by Integer division, so a
// 3x3x3 lattice must produce 27 distinct cells rather than a fractional sweep.
const octet = LatticeUnitCell.OctetTruss().UniformLattice(
  new Bounds3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1)),
  3,
  0.08,
);
check('a 3x3x3 lattice has 27 cells', octet.Cells().Count(), 27);
check('every cell contributes struts', octet.StrutCount() > 0, true);

// The TPMS nodal implicits at the origin are the sums of their constant terms:
// the gyroid's three sine-cosine products all vanish, and Schwarz P is 1+1+1.
close('gyroid is zero at the origin', new TpmsField3D(TpmsFamily.Gyroid(), 1, 0).Eval((Point3D as any).Origin()), 0);
close(
  'Schwarz P is 3 at the origin',
  new TpmsField3D(TpmsFamily.SchwarzPrimitive(), 1, 0).Eval((Point3D as any).Origin()),
  3,
);

// --- Sampling (sampling.library.plato) ---------------------------------------
//
// `RadicalInverse` recurses on an Integer division, so float division there
// silently corrupts every Halton, Hammersley and Sobol point without throwing.
// The base-2 radical inverse of 11 (1011 in binary) is 0.1101 in binary.
close('RadicalInverse(11, 2) reverses the bits', (11 as any).RadicalInverse(2), 0.8125);
close('RadicalInverse(1, 2)', (1 as any).RadicalInverse(2), 0.5);
close('RadicalInverse(2, 2)', (2 as any).RadicalInverse(2), 0.25);
close('RadicalInverse(3, 2)', (3 as any).RadicalInverse(2), 0.75);

// The relative radius of a point set is the smallest distance between any two
// points over the spacing a hexagonal packing of that many points would have,
// so a hexagonal lattice reads 1 by definition and a square grid about 0.93.
const unitSquare = new Bounds2D(new Point2D(0, 0), new Point2D(1, 1));
const squareGrid = new JitteredGridPattern2D(unitSquare, new IntegerVector2(6, 6), 0, 1).Points();
check('an unjittered grid is one point per cell', squareGrid.Count(), 36);
close('a square grid reads about 0.93', unitSquare.RelativeRadius(squareGrid), Math.sqrt(Math.sqrt(3) / 2), 1e-12);

// A hexagonal lattice of 36 points: rows a unit apart along X, alternate rows
// offset by half a unit and sqrt(3)/2 apart along Y, over the box those exactly
// fill. Every neighbour is one unit away, in row and across rows alike.
const hexPoints: Point2D[] = [];
for (let row = 0; row < 6; row++) {
  for (let column = 0; column < 6; column++) {
    hexPoints.push(new Point2D(column + (row % 2) * 0.5, (row * Math.sqrt(3)) / 2));
  }
}
const hexRegion = new Bounds2D(new Point2D(0, 0), new Point2D(6, (6 * Math.sqrt(3)) / 2));
close(
  'a hexagonal lattice reads 1',
  hexRegion.RelativeRadius(Intrinsics.MakeArray(...hexPoints)),
  1,
  1e-12,
);

// Blue noise is thinned to exactly the count asked for, which is an Integer
// division per candidate.
check(
  'BlueNoisePattern2D returns the count it was asked for',
  new BlueNoisePattern2D(unitSquare, 48, 7).Points().Count(),
  48,
);

// --- Remeshing (remeshing.library.plato) -------------------------------------
//
// Loop subdivision splits every triangle into four. Butterfly is INTERPOLATING:
// it introduces the same four-fold split but leaves every original vertex where
// it was, which is the property that distinguishes it from Loop and the one a
// wrong topology pass would break.

const tetra: any = PolygonMesh3D.Tetrahedron().ToTriangleMesh();
check('a tetrahedron has 6 undirected edges', tetra.TopologyOf().UndirectedEdges.Count(), 6);
check('Loop subdivision multiplies faces by 4', tetra.LoopSubdivided().Faces.Count(), 16);
check('Butterfly subdivision multiplies faces by 4', tetra.ButterflySubdivided().Faces.Count(), 16);

const butterflied: any = tetra.ButterflySubdivided();
let worstDrift = 0;
for (let i = 0; i < tetra.Positions.Count(); i++) {
  const before = tetra.Positions.At(i);
  const after = butterflied.Positions.At(i);
  worstDrift = Math.max(worstDrift, Math.hypot(after.X - before.X, after.Y - before.Y, after.Z - before.Z));
}
check('Butterfly interpolates: every input vertex is unmoved', worstDrift, 0);

// Loop is approximating, so it must NOT leave them where they were.
const looped: any = tetra.LoopSubdivided();
check(
  'Loop approximates: the input vertices move',
  looped.Positions.At(0).Distance(tetra.Positions.At(0)) > 1e-6,
  true,
);

// --- Finite elements (finite-elements.library.plato) -------------------------
//
// A unit cube of six tetrahedra, steel (E = 200 GPa, nu = 0.3), pulled along +X
// by 1 MPa with a symmetry plane on each face through the origin. Every number
// below is a closed form, not a recorded output: the axial displacement is
// sigma L / E, the transverse contraction is -nu sigma / E, the von Mises
// stress is the applied stress in every cell, and the gravity load is rho V g.
// This is the check that says the conjugate gradient, the sparse assembly, the
// Buffer runtime and the whole `Array<Number>` system-vector surface all work.

const cubeNodes = Intrinsics.MakeArray(
  new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 1, 0),
  new Point3D(0, 0, 1), new Point3D(1, 0, 1), new Point3D(0, 1, 1), new Point3D(1, 1, 1),
);
const tet = (a: number, b: number, c: number, d: number) =>
  new TetrahedronCell(new VertexIndex(a), new VertexIndex(b), new VertexIndex(c), new VertexIndex(d));
const cubeMesh = new TetrahedralMesh3D(
  cubeNodes,
  Intrinsics.MakeArray(
    tet(0, 1, 3, 7), tet(0, 1, 5, 7), tet(0, 2, 3, 7),
    tet(0, 2, 6, 7), tet(0, 4, 5, 7), tet(0, 4, 6, 7),
  ),
);
const steel = new EngineeringMaterial(
  'Steel', new Density(7850), new Pressure(200e9), 0.3,
  new Pressure(250e6), new Pressure(400e6), 1.2e-5, new ThermalConductivity(50), 490,
);
const symmetry: PrescribedDisplacement[] = [];
for (let i = 0; i < 8; i++) {
  const node = cubeNodes.At(i);
  if (node.X === 0) symmetry.push(new PrescribedDisplacement(new DofIndex(i * 3), 0));
  if (node.Y === 0) symmetry.push(new PrescribedDisplacement(new DofIndex(i * 3 + 1), 0));
  if (node.Z === 0) symmetry.push(new PrescribedDisplacement(new DofIndex(i * 3 + 2), 0));
}
const triangleFace = (a: number, b: number, c: number) =>
  new TriangleFace(new VertexIndex(a), new VertexIndex(b), new VertexIndex(c));
const pulledCube = new ElasticModel3D(
  cubeMesh,
  steel,
  Intrinsics.MakeArray(...symmetry),
  Intrinsics.MakeArray(),
  Intrinsics.MakeArray(
    new FaceTraction3D(triangleFace(1, 3, 7), new Vector3D(1e6, 0, 0)),
    new FaceTraction3D(triangleFace(1, 7, 5), new Vector3D(1e6, 0, 0)),
  ),
  new Vector3D(0, 0, 0),
);
check('the cube has 24 degrees of freedom', pulledCube.DofCount(), 24);

const elastic = pulledCube.SolveElastic(new LinearSolveSettings(500, 1e-12));
check('the conjugate gradient converges', elastic.Converged, true);
close('axial displacement is sigma L / E', elastic.Displacements.At(7).X, 5e-6, 1e-12);
close('transverse contraction is -nu sigma / E', elastic.Displacements.At(7).Y, -1.5e-6, 1e-12);
close('the held face does not move', elastic.Displacements.At(0).X, 0, 1e-18);

const vonMises = pulledCube.VonMisesStresses(elastic);
let worstStress = 0;
for (let i = 0; i < vonMises.Count(); i++) {
  worstStress = Math.max(worstStress, Math.abs(vonMises.At(i).Pascals - 1e6));
}
check('all six cells are at the applied stress', vonMises.Count(), 6);
close('von Mises is 1 MPa in every cell', worstStress, 0, 1e-6);

// Gravity is shared equally among each tetrahedron's four nodes, so the load
// vector sums to rho V g over the whole cube however it was cut up.
const heavyCube = pulledCube
  .WithTractions(Intrinsics.MakeArray())
  .WithGravity(new Vector3D(0, -9.81, 0));
const gravityLoads = heavyCube.LoadVector();
let gravityTotal = 0;
for (let i = 0; i < gravityLoads.Count(); i++) gravityTotal += gravityLoads.At(i);
close('gravity load totals rho V g', gravityTotal, -7850 * 1 * 9.81, 1e-6);

// The plane path is the same variational form with the third gradient
// component zero, so the same unit test in 2D must give the same numbers. It
// goes through `PlaneCondition`, which is a sum type and therefore
// prelude-supplied.
const squareCorners = Intrinsics.MakeArray(
  new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1), new Point2D(1, 1),
);
const planeHeld: PrescribedDisplacement[] = [];
for (let i = 0; i < 4; i++) {
  if (squareCorners.At(i).X === 0) planeHeld.push(new PrescribedDisplacement(new DofIndex(i * 2), 0));
  if (squareCorners.At(i).Y === 0) planeHeld.push(new PrescribedDisplacement(new DofIndex(i * 2 + 1), 0));
}
const planeSolution = new ElasticModel2D(
  new TriangleMesh2D(
    squareCorners,
    Intrinsics.MakeArray(
      new TriangleFace(new VertexIndex(0), new VertexIndex(1), new VertexIndex(3)),
      new TriangleFace(new VertexIndex(0), new VertexIndex(3), new VertexIndex(2)),
    ),
  ),
  steel,
  new Length(1),
  PlaneCondition.PlaneStress(),
  Intrinsics.MakeArray(...planeHeld),
  Intrinsics.MakeArray(
    new NodalForce2D(new VertexIndex(1), new Vector2D(5e5, 0)),
    new NodalForce2D(new VertexIndex(3), new Vector2D(5e5, 0)),
  ),
  new Vector2D(0, 0),
).SolveElastic(new LinearSolveSettings(500, 1e-12));
close('plane stress gives the same sigma L / E', planeSolution.Displacements.At(3).X, 5e-6, 1e-12);
close(
  'plane stress gives the same -nu sigma / E',
  planeSolution.Displacements.At(3).Y,
  -1.5e-6,
  1e-12,
);

// The Euler-Bernoulli beam against the textbook cantilever: a tip load deflects
// the free end by P L^3 / (3 E I). `BeamRestraint` and `BeamLoad` are both sum
// types (the second one carries fields), so both come from the prelude.
const cantilever = new Beam(
  new Length(2),
  new SectionProperties(new Area(1e-4), new Point2D(0, 0), 1e-8, 1e-8, 0, 0, 0, 0),
  steel,
  Intrinsics.MakeArray(new BeamSupport(BeamRestraint.Fixed(), new Length(0))),
  Intrinsics.MakeArray(BeamLoad.PointForce(new Length(2), 1000)),
);
const beamSolution = (cantilever as any).SolveBeam(8, new LinearSolveSettings(2000, 1e-14));
check('the beam solve converges', beamSolution.Converged, true);
close(
  'cantilever tip deflection is P L^3 / 3EI',
  beamSolution.Deflections.At(beamSolution.Deflections.Count() - 1),
  (1000 * 8) / (3 * 200e9 * 1e-8),
  1e-9,
);

// --- Rigid bodies (rigid-dynamics.library.plato) ------------------------------
//
// One free ball, no contacts, one step of a 60 Hz semi-implicit Euler: the
// velocity takes the whole gravity increment and the position takes the new
// velocity, so the drop is g dt^2 exactly. `RigidWorld3D.Step` folds
// `ReplacedAt` over its bodies and integrates the orientation through the
// quaternion product, so this one number covers both.

const freeBall = new SolverBody3D(
  new Point3D(0, 2, 0),
  Quaternion.Identity(),
  new SpatialVelocity3D(new Vector3D(0, 0, 0), new Vector3D(0, 0, 0)),
  1,
  new Vector3D(1, 1, 1),
  0,
  0,
  1,
);
const freeWorld = new RigidWorld3D(
  Intrinsics.MakeArray(freeBall),
  Intrinsics.MakeArray(),
  new Vector3D(0, -9.81, 0),
  new TimeStepSettings(new Duration(1 / 60), 1, 8, 2, 4),
  new ContactSolverSettings(new Length(0.005), 0.2, new Speed(2), new Speed(1)),
);
close(
  'one step of free fall drops g dt^2',
  (freeWorld as any).Step().Bodies.At(0).Center.Y,
  2 - 9.81 / 3600,
  1e-12,
);
// ReplacedAt is the primitive that fold is built on.
check(
  'ReplacedAt changes one element and no other',
  String(
    (Intrinsics.MakeArray(1, 2, 3) as any)
      .ReplacedAt(1, 9)
      .Reduce('', (acc: string, x: number) => `${acc}${x}`),
  ),
  '193',
);

// --- Cloth (cloth.library.plato) ---------------------------------------------
//
// One unconstrained Verlet step of a free sheet is the same free fall: with
// equal previous and current positions the step is a dt^2 g displacement. The
// grid layout underneath it splits a flat vertex number across the sheet by
// Integer division, so a wrong division shows up as a vertex off its row.

const sheetGrid: any = new ClothGrid3D(
  new Point3D(-0.5, 1, -0.5),
  new Vector3D(0.125, 0, 0),
  new Vector3D(0, 0, 0.125),
  9,
  9,
);
const sheet = sheetGrid.ClothFromGrid(new Mass(0.01), 0, new Length(0.01));
check('a 9x9 sheet has 81 vertices', sheet.Cloth.Vertices.Count(), 81);
// Vertex 40 is row 4, column 4 — the centre of the sheet, which the grid places
// at the origin in x and z.
close('the grid lays vertex 40 at the sheet centre (X)', sheet.Cloth.Vertices.At(40).Position.X, 0);
close('the grid lays vertex 40 at the sheet centre (Z)', sheet.Cloth.Vertices.At(40).Position.Z, 0);
const clothGravity = new ParticleForces3D(
  Intrinsics.MakeArray(new ParticleGravity(new Vector3D(0, -9.81, 0))),
  Intrinsics.MakeArray(),
  Intrinsics.MakeArray(),
  Intrinsics.MakeArray(),
  Intrinsics.MakeArray(),
  Intrinsics.MakeArray(),
);
close(
  'one Verlet step of a free sheet drops g dt^2',
  sheet
    .Step(clothGravity, new ClothSolverSettings(new Duration(1 / 60), 1, 8, 1, 0), 0)
    .Cloth.Vertices.At(40).Position.Y,
  1 - 9.81 / 3600,
  1e-9,
);

if (failures > 0) {
  console.error(`${failures} smoke check(s) failed`);
  process.exit(1);
}
console.log('smoke: all checks passed');
