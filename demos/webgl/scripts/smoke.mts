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
  Point2D,
  Point3D,
  Vector3D,
  Direction3D,
  Polygon2D,
  Polygon3D,
  PolygonSoup3D,
  PolygonMesh3D,
  Twist3D,
  Twist2D,
  Bend3D,
  Taper3D,
} from '../src/plato/plato.g.ts';

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

if (failures > 0) {
  console.error(`${failures} smoke check(s) failed`);
  process.exit(1);
}
console.log('smoke: all checks passed');
