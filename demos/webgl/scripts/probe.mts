// Does a generated member actually evaluate? Run before building a scene on it:
//
//   npm run probe
//
// Unlike `npm run smoke`, this asserts nothing — it calls each candidate and
// prints `ok`, `FAIL <message>` or `NaN`, so a demo author can tell a member
// that works from one the TypeScript writer left as a throwing stub and one that
// silently returns nothing useful. A FAIL here is a finding to report, not a
// licence to hand-roll the formula in a demo.
//
// Construct with the FULL field list of the generated class. A short
// constructor call leaves a field `undefined`, and the failure that produces —
// `Cannot read properties of undefined (reading 'Lerp')`, or a quiet NaN — looks
// exactly like a writer defect but is not one.
//
// Add the members your page needs; keep them grouped by the page that wants them.

import '../src/plato/array-ext.ts';
import {
  Angle,
  AngleInterval,
  BezierCurve3D,
  BilinearPatch,
  Bounds2D,
  Bounds3D,
  BoySurface,
  BreatherSurface,
  BSplineCurve3D,
  ButterflyCurve2D,
  Cardioid2D,
  Catenoid,
  CatmullRomCurve3D,
  Color,
  ColorHSL,
  CubicBezier3D,
  Cylinder,
  DensityGrid3D,
  DiniSurface,
  Direction3D,
  DomainWarpNoise2D,
  EnneperSurface,
  Epicycloid2D,
  Epitrochoid2D,
  FbmNoise2D,
  FbmNoise3D,
  FigureEightKnot,
  Frame3D,
  GaborNoise2D,
  Helicoid,
  Helix,
  HermiteCurve3D,
  Hypocycloid2D,
  Hypotrochoid2D,
  IntegerVector3,
  Intrinsics,
  KnotVector,
  Lemniscate2D,
  Matrix4x4,
  NumberInterval,
  PerlinNoise2D,
  PerlinNoise3D,
  Point2D,
  Point3D,
  Polygon2D,
  Polygon3D,
  PolygonSet2D,
  PolygonWithHoles2D,
  RidgedNoise2D,
  SampledSdf3D,
  SimplexNoise2D,
  Sphere,
  Torus,
  Translation3D,
  TrefoilKnot,
  TurbulenceNoise2D,
  UvCoordinate,
  ValueNoise2D,
  Vector3D,
  WhiteNoise2D,
  WhiteNoise3D,
  WorleyNoise2D,
} from '../src/plato/plato.g.ts';

let ok = 0;
let bad = 0;
let nan = 0;

function probe(name: string, produce: () => unknown): void {
  try {
    const value = produce();
    const text = String(value);
    if (/NaN/.test(text)) {
      nan++;
      console.log(`NaN  ${name} = ${text.slice(0, 80)}`);
      return;
    }
    ok++;
    console.log(`ok   ${name} = ${text.slice(0, 80)}`);
  } catch (error) {
    bad++;
    console.log(`FAIL ${name}: ${(error as Error).message.slice(0, 110)}`);
  }
}

function section(name: string): void {
  console.log(`\n--- ${name} ---`);
}

const p2 = (x: number, y: number): Point2D => new Point2D(x, y);
const p3 = (x: number, y: number, z: number): Point3D => new Point3D(x, y, z);
const d3 = (x: number, y: number, z: number): Direction3D => new Direction3D(new Vector3D(x, y, z));
const uv = (u: number, v: number): UvCoordinate => new UvCoordinate(u, v);
const angles = (a: number, b: number): AngleInterval =>
  new AngleInterval(new Angle(a), new Angle(b));
const worldFrame = new Frame3D(p3(0, 0, 0), d3(1, 0, 0), d3(0, 1, 0), d3(0, 0, 1));

// The sum types `plato.g.ts` reports as CHK320 and the prelude supplies.
const NoiseBasis = (globalThis as any).NoiseBasis;
const WorleyDistance = (globalThis as any).WorleyDistance;
const WorleyFeature = (globalThis as any).WorleyFeature;

section('parametric curves');
probe('BezierCurve3D.Eval', () =>
  new BezierCurve3D(
    Intrinsics.MakeArray(p3(0, 0, 0), p3(1, 2, 0), p3(2, -1, 1), p3(3, 0, 0)),
  ).Eval(0.4),
);
probe('CubicBezier3D.Eval', () =>
  new CubicBezier3D(p3(0, 0, 0), p3(1, 1, 0), p3(2, -1, 0), p3(3, 0, 0)).Eval(0.5),
);
probe('HermiteCurve3D.Eval', () =>
  new HermiteCurve3D(p3(0, 0, 0), new Vector3D(1, 0, 0), p3(1, 1, 0), new Vector3D(0, 1, 0)).Eval(0.5),
);
probe('CatmullRomCurve3D.Eval', () =>
  new CatmullRomCurve3D(
    Intrinsics.MakeArray(p3(0, 0, 0), p3(1, 1, 0), p3(2, 0, 1), p3(3, 1, 0)),
    0.5,
    false,
  ).Eval(0.5),
);
probe('BSplineCurve3D.Eval', () =>
  new BSplineCurve3D(
    Intrinsics.MakeArray(p3(0, 0, 0), p3(1, 2, 0), p3(2, -1, 1), p3(3, 0, 0)),
    2,
    new KnotVector(Intrinsics.MakeArray(0, 0, 0, 1, 2, 2, 2)),
  ).Eval(0.5),
);
probe('Helix.Eval', () => new Helix(worldFrame, 1, 0.5, angles(0, 6 * Math.PI)).Eval(0.5));
probe('TrefoilKnot.Eval', () => new TrefoilKnot(worldFrame, 1).Eval(0.25));
probe('FigureEightKnot.Eval', () => new FigureEightKnot(worldFrame, 1).Eval(0.25));
probe('ButterflyCurve2D.Eval', () => new ButterflyCurve2D(1).Eval(0.3));
probe('AngleInterval.Lerp', () => angles(0, 6).Lerp(0.5));

// The roulettes multiply a scalar by an Angle, which is the commuted overload.
// Probe `Eval` and not `SamplePeriodic`: the sampler is a lazy IArray, so it
// constructs happily and only produces NaN once a caller indexes it.
probe('Epicycloid2D.Eval', () => new Epicycloid2D(3, 1).Eval(0.2));
probe('Hypocycloid2D.Eval', () => new Hypocycloid2D(3, 1).Eval(0.2));
probe('Epitrochoid2D.Eval', () => new Epitrochoid2D(3, 1, 0.5).Eval(0.2));
probe('Hypotrochoid2D.Eval', () => new Hypotrochoid2D(5, 3, 5).Eval(0.2));
probe('Cardioid2D.Eval', () => new Cardioid2D(1).Eval(0.2));
// Not a defect: r = sqrt(Scale^2 cos 2t) has no real value where cos 2t < 0, so
// the lemniscate is NaN over half its parameter range by construction.
probe('Lemniscate2D.Eval (in the domain)', () => new Lemniscate2D(1).Eval(0.05));
probe('Lemniscate2D.Eval (out of the domain)', () => new Lemniscate2D(1).Eval(0.2));
// Also not a defect: `Sample(count)` divides by count - 1, so one sample has no
// parameter to evaluate at. Ask for two or more.
probe('ICurve2D.Sample(1)', () => new Cardioid2D(1).Sample(1).At(0));
probe('ICurve2D.Sample(2)', () => new Cardioid2D(1).Sample(2).At(0));

section('parametric surfaces');
probe('Torus.Eval', () => new Torus(p3(0, 0, 0), d3(0, 1, 0), 1, 0.35).Eval(uv(0.25, 0.5)));
probe('Sphere.Eval', () => new Sphere(p3(0, 0, 0), 1).Eval(uv(0.3, 0.4)));
probe('Cylinder.Eval', () => new Cylinder(p3(0, 0, 0), d3(0, 1, 0), 1, 2).Eval(uv(0.25, 0.5)));
probe('BilinearPatch.Eval', () =>
  new BilinearPatch(p3(0, 0, 0), p3(1, 0, 0), p3(0, 0, 1), p3(1, 1, 1)).Eval(uv(0.5, 0.5)),
);
probe('BoySurface.Eval', () => new BoySurface(1).Eval(uv(0.6, 0.4)));
probe('Catenoid.Eval', () => new Catenoid(1, new NumberInterval(-1, 1)).Eval(uv(0.6, 0.4)));
probe('Helicoid.Eval', () => new Helicoid(new NumberInterval(0, 1), 2, 0.5).Eval(uv(0.6, 0.4)));
probe('DiniSurface.Eval', () =>
  new DiniSurface(1, 0.2, 2, angles(0.1, 1.5)).Eval(uv(0.6, 0.4)),
);
probe('EnneperSurface.Eval', () =>
  new EnneperSurface(1, new Bounds2D(p2(-2, -2), p2(2, 2))).Eval(uv(0.6, 0.4)),
);
probe('BreatherSurface.Eval', () =>
  new BreatherSurface(0.4, new Bounds2D(p2(-13, -37), p2(13, 37))).Eval(uv(0.6, 0.4)),
);

section('noise — basis fields');
probe('ValueNoise2D.Eval', () => new ValueNoise2D(7, 4).Eval(p2(0.3, 0.7)));
probe('WhiteNoise2D.Eval', () => new WhiteNoise2D(7).Eval(p2(0.3, 0.7)));
probe('WhiteNoise3D.Eval varies in z', () =>
  new WhiteNoise3D(7).Eval(p3(0.1, 0.2, 0.3)) - new WhiteNoise3D(7).Eval(p3(0.1, 0.2, 0.9)),
);
probe('PerlinNoise2D.Eval', () => new PerlinNoise2D(7, 4).Eval(p2(0.3, 0.7)));
probe('PerlinNoise3D.Eval', () => new PerlinNoise3D(7, 3).Eval(p3(0.2, 0.3, 0.4)));
probe('SimplexNoise2D.Eval', () => new SimplexNoise2D(7, 4).Eval(p2(0.3, 0.7)));
probe('GaborNoise2D.Eval', () =>
  new GaborNoise2D(7, 4, new Angle(0), 1, 0).Eval(p2(0.3, 0.7)),
);
probe('WorleyNoise2D.Eval (F1)', () =>
  new WorleyNoise2D(7, 4, 1, WorleyDistance.Euclidean(), WorleyFeature.F1()).Eval(p2(0.3, 0.7)),
);
probe('WorleyNoise2D.Eval (F2-F1)', () =>
  new WorleyNoise2D(7, 4, 1, WorleyDistance.Chebyshev(), WorleyFeature.F2MinusF1()).Eval(
    p2(0.3, 0.7),
  ),
);

section('noise — fractal sums');
for (const basis of ['White', 'Value', 'Perlin', 'Simplex', 'Worley', 'Gabor']) {
  probe(`FbmNoise2D.Eval (${basis})`, () =>
    new FbmNoise2D(NoiseBasis[basis](), 7, 2, 4, 2, 0.5).Eval(p2(0.3, 0.7)),
  );
}
probe('FbmNoise3D.Eval', () =>
  new FbmNoise3D(NoiseBasis.Perlin(), 7, 2, 4, 2, 0.5).Eval(p3(0.2, 0.3, 0.4)),
);
probe('TurbulenceNoise2D.Eval', () =>
  new TurbulenceNoise2D(NoiseBasis.Perlin(), 7, 2, 4, 2, 0.5).Eval(p2(0.3, 0.7)),
);
probe('RidgedNoise2D.Eval', () =>
  new RidgedNoise2D(NoiseBasis.Perlin(), 7, 2, 4, 2, 0.5, 1).Eval(p2(0.3, 0.7)),
);
probe('DomainWarpNoise2D.Eval', () =>
  new DomainWarpNoise2D(NoiseBasis.Perlin(), 7, 2, 0.4, 2, 3).Eval(p2(0.3, 0.7)),
);

section('colours');
probe('Color.Default', () => Color.Default);
probe('ColorHSL value', () => new ColorHSL(new Angle(0.5), 0.7, 0.5));

section('transforms');
probe('Matrix4x4.Identity', () => Matrix4x4.Identity());
probe('Point3D.Transform(AffineTransform3D)', () =>
  p3(1, 2, 3).Transform(Matrix4x4.Identity().AffineTransform3D()),
);
probe('Point3D.Transform(Translation3D)', () =>
  p3(1, 1, 1).Transform(new Translation3D(new Vector3D(1, 2, 3)).AffineTransform3D()),
);

section('triangulation');
const ring = (points: [number, number][]) =>
  new Polygon2D(Intrinsics.MakeArray(...points.map(([x, y]) => p2(x, y))));
const lShape = ring([[0, 0], [2, 0], [2, 1], [1, 1], [1, 2], [0, 2]]);
const withHole = new PolygonWithHoles2D(
  ring([[-2, -2], [2, -2], [2, 2], [-2, 2]]),
  Intrinsics.MakeArray(ring([[-1, -1], [-1, 1], [1, 1], [1, -1]])),
);
probe('Polygon2D.Triangulate', () => `${lShape.Triangulate().Faces.Count()} faces`);
probe('PolygonWithHoles2D.Triangulate', () => `${withHole.Triangulate().Faces.Count()} faces`);
probe('PolygonSet2D.Triangulate', () =>
  `${new PolygonSet2D(Intrinsics.MakeArray(withHole)).Triangulate().Faces.Count()} faces`,
);
probe('Polygon3D.ToTriangleMesh', () =>
  `${new Polygon3D(Intrinsics.MakeArray(p3(0, 0, 0), p3(1, 0, 0), p3(1, 1, 0), p3(0, 1, 0)))
    .ToTriangleMesh()
    .Faces.Count()} faces`,
);

section('marching cubes');
probe('Sphere.ToSdf().MarchingCubes', () =>
  `${new Sphere(p3(0, 0, 0), 1)
    .ToSdf()
    .MarchingCubes(
      new Bounds3D(p3(-1.5, -1.5, -1.5), p3(1.5, 1.5, 1.5)),
      new IntegerVector3(16, 16, 16),
    )
    .Triangles.Count()} triangles`,
);
probe('PerlinNoise3D.MarchingCubes', () =>
  `${new PerlinNoise3D(7, 2)
    .MarchingCubes(new Bounds3D(p3(-1, -1, -1), p3(1, 1, 1)), new IntegerVector3(12, 12, 12), 0)
    .Triangles.Count()} triangles`,
);

section('voxels');
const lattice = 16;
const ball = (lattice as any).MakeArray3D(
  lattice,
  lattice,
  (i: number, j: number, k: number) => {
    const t = (n: number) => (n / (lattice - 1)) * 2 - 1;
    return Math.hypot(t(i), t(j), t(k)) - 0.8;
  },
);
probe('Number.MakeArray3D', () => `${ball.ColumnCount}x${ball.RowCount}x${ball.LayerCount}`);
probe('SampledSdf3D.MarchingCubes', () =>
  `${new SampledSdf3D(ball, new Bounds3D(p3(-1, -1, -1), p3(1, 1, 1)))
    .MarchingCubes()
    .Triangles.Count()} triangles`,
);
probe('DensityGrid3D.MarchingCubes', () => {
  const density = (lattice as any).MakeArray3D(
    lattice,
    lattice,
    (i: number, j: number, k: number) => {
      const t = (n: number) => (n / (lattice - 1)) * 2 - 1;
      return 1 - Math.hypot(t(i), t(j), t(k));
    },
  );
  return `${new DensityGrid3D(p3(-1, -1, -1), 2 / (lattice - 1), density)
    .MarchingCubes(0.2)
    .Triangles.Count()} triangles`;
});

console.log(`\n${ok} ok, ${nan} returned NaN, ${bad} failed`);
