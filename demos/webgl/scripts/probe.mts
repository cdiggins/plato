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
//
// Set PROBE_STACK=1 to print the stack of each failure as well as its message.

import '../src/plato/array-ext.ts';
import {
  Angle,
  AngleInterval,
  BlueNoisePattern2D,
  Cloth3D,
  ClothGrid3D,
  ClothSolverSettings,
  ContactSolverSettings,
  Density,
  DofIndex,
  Duration,
  ElasticModel3D,
  EngineeringMaterial,
  FaceTraction3D,
  HaltonPattern2D,
  HaltonPattern3D,
  HammersleyPattern2D,
  IntegerVector2,
  JitteredGridPattern2D,
  JitteredGridPattern3D,
  LatticeUnitCell,
  Length,
  LinearSolveSettings,
  MassSpringSettings,
  Plane,
  PlasticPattern2D,
  PlasticPattern3D,
  PoissonDiskPattern2D,
  PrescribedDisplacement,
  Pressure,
  Proportion,
  RigidWorld3D,
  SobolPattern2D,
  SolverBody3D,
  SpatialVelocity3D,
  Speed,
  StratifiedPattern2D,
  StratifiedPattern3D,
  StrutLattice3D,
  TetrahedralMesh3D,
  TetrahedronCell,
  ThermalConductivity,
  TimeStepSettings,
  TpmsField3D,
  TpmsNetwork3D,
  TpmsSheet3D,
  TriangleFace,
  BodyIndex,
  Area,
  Beam,
  BeamSupport,
  DampingCoefficient,
  Mass,
  SectionProperties,
  ElasticModel2D,
  NodalForce2D,
  TriangleMesh2D,
  Vector2D,
  ParticleForces3D,
  ParticleGravity,
  PolygonMesh3D,
  Quaternion,
  Stiffness,
  VertexIndex,
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
    // `PROBE_STACK=1 npx tsx scripts/probe.mts` prints the stack too, which is
    // the fastest way to find WHICH emitted body a missing member is called
    // from — the message alone rarely names it.
    if (process.env.PROBE_STACK) console.log((error as Error).stack);
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

// ---------------------------------------------------------------------------
// The 2026-08-03 tracks. `stdlib/future` is neither linted nor converted to C#,
// so treat a FAIL below as less surprising than one above — but no less real.
// ---------------------------------------------------------------------------

const b2 = (lo: number, hi: number): Bounds2D => new Bounds2D(p2(lo, lo), p2(hi, hi));
const b3 = (lo: number, hi: number): Bounds3D => new Bounds3D(p3(lo, lo, lo), p3(hi, hi, hi));

// The rest of the sum types the prelude supplies; see array-ext.ts.
const TpmsFamily = (globalThis as any).TpmsFamily;
const MaterialCombine = (globalThis as any).MaterialCombine;
const BodyMotion = (globalThis as any).BodyMotion;
const PlaneCondition = (globalThis as any).PlaneCondition;
const BeamRestraint = (globalThis as any).BeamRestraint;
const BeamLoad = (globalThis as any).BeamLoad;
const LaplacianWeighting = (globalThis as any).LaplacianWeighting;
const SubdivisionScheme = (globalThis as any).SubdivisionScheme;

section('lattices (lattices.library.plato)');
const cellNames = [
  'SimpleCubic',
  'BodyCenteredCubic',
  'FaceCenteredCubic',
  'OctetTruss',
  'DiamondCubic',
  'TruncatedOctahedron',
  'ReentrantAuxetic',
];
for (const name of cellNames) {
  probe(`LatticeUnitCell.${name}`, () => {
    const cell = (LatticeUnitCell as any)[name]();
    const valences = cell.NodeValences();
    const list: number[] = [];
    for (let i = 0; i < valences.Count(); i++) list.push(valences.At(i));
    return `${cell.Nodes.Count()} nodes, ${cell.Struts.Count()} struts, valences ${list.join('/')}`;
  });
}
const octet: any = (LatticeUnitCell as any).OctetTruss().UniformLattice(b3(-1, 1), 3, 0.08);
probe('StrutLattice3D.Cells', () => `${octet.Cells().Count()} cells`);
probe('StrutLattice3D.StrutCount', () => octet.StrutCount());
probe('StrutLattice3D.NodeCount', () => octet.NodeCount());
probe('StrutLattice3D.TotalStrutLength', () => octet.TotalStrutLength());
probe('StrutLattice3D.RelativeDensity', () => octet.RelativeDensity());
probe('StrutLattice3D.ToSdf().Eval', () => octet.ToSdf().Eval(p3(0, 0, 0)));
probe('Array<Line3D>.Trimmed(Bounds3D)', () => octet.Struts().Trimmed(b3(-0.5, 0.5)).Count());
probe('Array<Line3D>.StrutRadii(Number)', () => octet.Struts().StrutRadii(0.05).At(0));
probe('Array<Line3D>.Deformed', () => octet.Struts().Deformed((q: Point3D) => q).Count());
probe('Array<Line3D>.ToSdf(radii).Eval', () =>
  octet.Struts().ToSdf(octet.Struts().StrutRadii(0.05)).Eval(p3(0, 0, 0)),
);
for (const family of ['Gyroid', 'SchwarzPrimitive', 'SchwarzDiamond', 'Neovius', 'IwpSurface']) {
  probe(`TpmsField3D.Eval (${family})`, () =>
    new TpmsField3D(TpmsFamily[family](), 1, 0).Eval(p3(0.1, 0.2, 0.3)),
  );
}
probe('TpmsNetwork3D.Eval', () =>
  new TpmsNetwork3D(TpmsFamily.Gyroid(), 1, 0).Eval(p3(0.1, 0.2, 0.3)),
);
probe('TpmsSheet3D.Eval', () =>
  new TpmsSheet3D(TpmsFamily.Gyroid(), 1, 0, 0.2).Eval(p3(0.1, 0.2, 0.3)),
);

section('sampling (sampling.library.plato)');
probe('Number.RadicalInverse (base 2)', () => (11 as any).RadicalInverse(2));
probe('Number.HaltonPoint2D', () => (11 as any).HaltonPoint2D(2, 3));
probe('Number.SobolPoint2D', () => (11 as any).SobolPoint2D());
probe('HaltonPattern2D.Points', () => new HaltonPattern2D(b2(0, 1), 32, 2, 3).Points().At(5));
probe('HammersleyPattern2D.Points', () => new HammersleyPattern2D(b2(0, 1), 32, 2).Points().At(5));
probe('SobolPattern2D.Points', () => new SobolPattern2D(b2(0, 1), 32, 0).Points().At(5));
probe('PlasticPattern2D.Points', () => new PlasticPattern2D(b2(0, 1), 32, 0.5).Points().At(5));
probe('JitteredGridPattern2D.Points', () =>
  new JitteredGridPattern2D(b2(0, 1), new IntegerVector2(5, 4), 1, 7).Points().At(7),
);
probe('StratifiedPattern2D.Points', () =>
  new StratifiedPattern2D(b2(0, 1), new IntegerVector2(5, 4), 2, 7).Points().At(7),
);
probe('PoissonDiskPattern2D.Points', () =>
  `${new PoissonDiskPattern2D(b2(0, 1), 0.12, 30, 7).Points().Count()} points`,
);
probe('BlueNoisePattern2D.Points (thinned to Count)', () =>
  `${new BlueNoisePattern2D(b2(0, 1), 48, 7).Points().Count()} points`,
);
probe('Bounds2D.PatternGrid', () => b2(0, 1).PatternGrid(17));
probe('Bounds2D.RelativeRadius', () =>
  b2(0, 1).RelativeRadius(new JitteredGridPattern2D(b2(0, 1), new IntegerVector2(6, 6), 0, 7).Points()),
);
probe('JitteredGridPattern3D.Points', () =>
  new JitteredGridPattern3D(b3(0, 1), new IntegerVector3(4, 3, 2), 1, 7).Points().At(9),
);
probe('StratifiedPattern3D.Points', () =>
  new StratifiedPattern3D(b3(0, 1), new IntegerVector3(4, 3, 2), 2, 7).Points().At(9),
);
probe('HaltonPattern3D.Points', () => new HaltonPattern3D(b3(0, 1), 32, 2, 3, 5).Points().At(5));
probe('PlasticPattern3D.Points', () => new PlasticPattern3D(b3(0, 1), 32, 0.5).Points().At(5));

section('finite elements (finite-elements.library.plato)');

// A unit cube split into six tetrahedra (Kuhn's triangulation: every tet is a
// monotone path from corner 0 to corner 7), pulled along +X by 1 MPa with
// symmetry planes on the three faces through the origin. This is the closed-form
// patch test plato-424 records.
const cubeCorners = Intrinsics.MakeArray(
  p3(0, 0, 0), p3(1, 0, 0), p3(0, 1, 0), p3(1, 1, 0),
  p3(0, 0, 1), p3(1, 0, 1), p3(0, 1, 1), p3(1, 1, 1),
);
const tet = (a: number, b: number, c: number, d: number): TetrahedronCell =>
  new TetrahedronCell(new VertexIndex(a), new VertexIndex(b), new VertexIndex(c), new VertexIndex(d));
const cubeTets = new TetrahedralMesh3D(
  cubeCorners,
  Intrinsics.MakeArray(
    tet(0, 1, 3, 7), tet(0, 1, 5, 7), tet(0, 2, 3, 7),
    tet(0, 2, 6, 7), tet(0, 4, 5, 7), tet(0, 4, 6, 7),
  ),
);
const steel = new EngineeringMaterial(
  'Steel',
  new Density(7850),
  new Pressure(200e9),
  0.3,
  new Pressure(250e6),
  new Pressure(400e6),
  1.2e-5,
  new ThermalConductivity(50),
  490,
);
// Symmetry: the x = 0 face holds x, the y = 0 face holds y, the z = 0 face holds z.
const held: PrescribedDisplacement[] = [];
for (let i = 0; i < 8; i++) {
  const corner = cubeCorners.At(i);
  if (corner.X === 0) held.push(new PrescribedDisplacement(new DofIndex(i * 3), 0));
  if (corner.Y === 0) held.push(new PrescribedDisplacement(new DofIndex(i * 3 + 1), 0));
  if (corner.Z === 0) held.push(new PrescribedDisplacement(new DofIndex(i * 3 + 2), 0));
}
const face = (a: number, b: number, c: number): TriangleFace =>
  new TriangleFace(new VertexIndex(a), new VertexIndex(b), new VertexIndex(c));
const pulled = new ElasticModel3D(
  cubeTets,
  steel,
  Intrinsics.MakeArray(...held),
  Intrinsics.MakeArray(),
  Intrinsics.MakeArray(
    new FaceTraction3D(face(1, 3, 7), new Vector3D(1e6, 0, 0)),
    new FaceTraction3D(face(1, 7, 5), new Vector3D(1e6, 0, 0)),
  ),
  new Vector3D(0, 0, 0),
);
probe('ElasticModel3D.DofCount', () => pulled.DofCount());
probe('ElasticModel3D.StiffnessMatrix entries', () =>
  `${pulled.StiffnessMatrix().Entries.Count()} entries`,
);
probe('ElasticModel3D.LoadVector (ScatterLoads)', () => {
  const loads = pulled.LoadVector();
  let total = 0;
  for (let i = 0; i < loads.Count(); i++) total += loads.At(i);
  return `${loads.Count()} dofs, sum ${total}`;
});
probe('SparseMatrix.SolveConstrained (conjugate gradient)', () => {
  const solved = pulled.SolveElastic(new LinearSolveSettings(500, 1e-10));
  return `converged ${solved.Converged} in ${solved.Iterations}, u(7).X = ${solved.Displacements.At(7).X}`;
});
probe('ElasticModel3D.VonMisesStresses', () => {
  const solved = pulled.SolveElastic(new LinearSolveSettings(500, 1e-10));
  return `${pulled.VonMisesStresses(solved).At(0).Pascals} Pa in cell 0`;
});
probe('ElasticModel3D gravity load total', () => {
  const heavy = pulled.WithTractions(Intrinsics.MakeArray()).WithGravity(new Vector3D(0, -9.81, 0));
  const loads = heavy.LoadVector();
  let total = 0;
  for (let i = 0; i < loads.Count(); i++) total += loads.At(i);
  return total;
});
probe('PlaneCondition sum type', () => String(PlaneCondition.PlaneStress()));
// The Euler-Bernoulli path: a cantilever with a tip load, whose textbook
// deflection is P L^3 / (3 E I). `BeamRestraint` and `BeamLoad` are both sum
// types, so both are prelude-supplied.
const cantilever = new Beam(
  new Length(2),
  new SectionProperties(new Area(1e-4), p2(0, 0), 1e-8, 1e-8, 0, 0, 0, 0),
  steel,
  Intrinsics.MakeArray(new BeamSupport(BeamRestraint.Fixed(), new Length(0))),
  Intrinsics.MakeArray(BeamLoad.PointForce(new Length(2), 1000)),
);
probe('Beam.SolveBeam (cantilever tip load)', () => {
  const solved = (cantilever as any).SolveBeam(8, new LinearSolveSettings(2000, 1e-14));
  const tip = solved.Deflections.At(solved.Deflections.Count() - 1);
  return `tip ${tip} m against P L^3 / 3EI = ${(1000 * 8) / (3 * 200e9 * 1e-8)}`;
});
// The plane path: a unit square as two plane-stress triangles, pulled the same
// way. plato-424 records that it reproduces the same closed forms.
probe('ElasticModel2D.SolveElastic (plane stress)', () => {
  const corners = Intrinsics.MakeArray(p2(0, 0), p2(1, 0), p2(0, 1), p2(1, 1));
  const square = new TriangleMesh2D(
    corners,
    Intrinsics.MakeArray(
      new TriangleFace(new VertexIndex(0), new VertexIndex(1), new VertexIndex(3)),
      new TriangleFace(new VertexIndex(0), new VertexIndex(3), new VertexIndex(2)),
    ),
  );
  const holdPlane: PrescribedDisplacement[] = [];
  for (let i = 0; i < 4; i++) {
    if (corners.At(i).X === 0) holdPlane.push(new PrescribedDisplacement(new DofIndex(i * 2), 0));
    if (corners.At(i).Y === 0) holdPlane.push(new PrescribedDisplacement(new DofIndex(i * 2 + 1), 0));
  }
  const plane = new ElasticModel2D(
    square,
    steel,
    new Length(1),
    PlaneCondition.PlaneStress(),
    Intrinsics.MakeArray(...holdPlane),
    Intrinsics.MakeArray(
      new NodalForce2D(new VertexIndex(1), new Vector2D(5e5, 0)),
      new NodalForce2D(new VertexIndex(3), new Vector2D(5e5, 0)),
    ),
    new Vector2D(0, 0),
  );
  const solved = plane.SolveElastic(new LinearSolveSettings(500, 1e-12));
  return `u(3).X = ${solved.Displacements.At(3).X} against sigma L / E = 5e-6`;
});
probe('EngineeringMaterial.PlaneLameParameters', () =>
  (steel as any).PlaneLameParameters(PlaneCondition.PlaneStress()).FirstParameter.Pascals,
);

section('rigid bodies (rigid-dynamics / collision.library.plato)');
const still = (x: number, y: number, z: number, inverseMass: number): SolverBody3D =>
  new SolverBody3D(
    p3(x, y, z),
    Quaternion.Identity(),
    new SpatialVelocity3D(new Vector3D(0, 0, 0), new Vector3D(0, 0, 0)),
    inverseMass,
    new Vector3D(inverseMass, inverseMass, inverseMass),
    0,
    0,
    1,
  );
const bodies = Intrinsics.MakeArray(
  still(0, 2, 0, 1),
  still(0.9, 2, 0, 1),
  still(0, -1000, 0, 0),
);
const radii = Intrinsics.MakeArray(0.5, 0.5, 0);
const ground = new Plane(new Direction3D(new Vector3D(0, 1, 0)), 0);
const world = new RigidWorld3D(
  bodies,
  Intrinsics.MakeArray(),
  new Vector3D(0, -9.81, 0),
  new TimeStepSettings(new Duration(1 / 60), 1, 8, 2, 4),
  new ContactSolverSettings(new Length(0.005), 0.2, new Speed(2), new Speed(1)),
);
probe('Array<T>.ReplacedAt', () => {
  const replaced = (bodies as any).ReplacedAt(1, still(5, 5, 5, 1));
  return `${replaced.At(1).Center.X} (was ${bodies.At(1).Center.X})`;
});
probe('Sphere.Collide(Sphere)', () =>
  `${(bodies as any).BallOf(radii, 0).Collide((bodies as any).BallOf(radii, 1)).Count()} contacts`,
);
probe('Sphere.Collide(Plane)', () =>
  `${new Sphere(p3(0, 0.4, 0), 0.5).Collide(ground).Count()} contacts`,
);
probe('Array<SolverBody3D>.BallSceneManifolds', () =>
  `${(bodies as any).BallSceneManifolds(radii, ground, new BodyIndex(2), 0.5, new Proportion(0.2)).Count()} manifolds`,
);
probe('RigidWorld3D.BallSceneConstraints', () =>
  `${(world as any)
    .BallSceneConstraints(radii, ground, new BodyIndex(2), 0.5, new Proportion(0.2), new Length(0.01))
    .Count()} rows`,
);
probe('RigidWorld3D.Step', () => {
  const rows = (world as any).BallSceneConstraints(
    radii, ground, new BodyIndex(2), 0.5, new Proportion(0.2), new Length(0.01),
  );
  const stepped = (world.WithConstraints(rows) as any).Step();
  return `body 0 at y = ${stepped.Bodies.At(0).Center.Y}`;
});
probe('MaterialCombine dispatch', () => MaterialCombine.Average().Combine(0.4, 0.6));
probe('BodyMotion dispatch', () => BodyMotion.Static().MobilityScale());

section('remeshing (remeshing.library.plato)');
const tetraMesh: any = (PolygonMesh3D.Tetrahedron() as any).ToTriangleMesh();
probe('TriangleMesh3D.TopologyOf', () => `${tetraMesh.TopologyOf().UndirectedEdges.Count()} edges`);
probe('TriangleMesh3D.UndirectedEdgeCount', () => tetraMesh.UndirectedEdgeCount());
probe('TriangleMesh3D.VertexValences', () => {
  const v = tetraMesh.VertexValences(tetraMesh.TopologyOf());
  const list: number[] = [];
  for (let i = 0; i < v.Count(); i++) list.push(v.At(i));
  return list.join('/');
});
probe('TriangleMesh3D.LoopSubdivided', () => `${tetraMesh.LoopSubdivided().Faces.Count()} faces`);
probe('TriangleMesh3D.ButterflySubdivided', () =>
  `${tetraMesh.ButterflySubdivided().Faces.Count()} faces`,
);
probe('TriangleMesh3D.LaplacianSmoothed (uniform)', () =>
  `${tetraMesh.LaplacianSmoothed(LaplacianWeighting.UniformWeights(), 0.5, 1).Positions.Count()} vertices`,
);
probe('TriangleMesh3D.LaplacianSmoothed (cotangent)', () =>
  `${tetraMesh.LaplacianSmoothed(LaplacianWeighting.CotangentWeights(), 0.5, 1).Positions.Count()} vertices`,
);
probe('TriangleMesh3D.Welded', () => `${tetraMesh.Welded(1e-6).Positions.Count()} vertices`);
probe('TriangleMesh3D.SplitLongEdges', () => `${tetraMesh.SplitLongEdges(1).Faces.Count()} faces`);
probe('TriangleMesh3D.CollapseShortEdges', () =>
  `${tetraMesh.CollapseShortEdges(0.1).Faces.Count()} faces`,
);
probe('PolygonMesh3D.CatmullClarkSubdivided', () =>
  `${(PolygonMesh3D.Cube() as any).CatmullClarkSubdivided().FaceCount()} faces`,
);
probe('PolygonMesh3D.DooSabinSubdivided', () =>
  `${(PolygonMesh3D.Cube() as any).DooSabinSubdivided().FaceCount()} faces`,
);
probe('PolygonMesh3D.Subdivided(scheme, levels)', () =>
  `${(PolygonMesh3D.Cube() as any).Subdivided(SubdivisionScheme.CatmullClark(), 1).FaceCount()} faces`,
);

section('cloth (cloth.library.plato)');
const clothGrid: any = new ClothGrid3D(
  p3(-0.5, 1, -0.5),
  new Vector3D(0.125, 0, 0),
  new Vector3D(0, 0, 0.125),
  9,
  9,
);
const clothForces = new ParticleForces3D(
  Intrinsics.MakeArray(new ParticleGravity(new Vector3D(0, -9.81, 0))),
  Intrinsics.MakeArray(),
  Intrinsics.MakeArray(),
  Intrinsics.MakeArray(),
  Intrinsics.MakeArray(),
  Intrinsics.MakeArray(),
);
probe('ClothGrid3D.ClothFromGrid', () =>
  `${clothGrid.ClothFromGrid(new Mass(0.01), 0, new Length(0.01)).Cloth.Vertices.Count()} vertices`,
);
probe('Cloth3D.Step (position-based)', () => {
  const sheet = clothGrid.ClothFromGrid(new Mass(0.01), 0, new Length(0.01));
  const settings = new ClothSolverSettings(new Duration(1 / 60), 1, 8, 1, 0.01);
  return String(sheet.Step(clothForces, settings, 0).Cloth.Vertices.At(40).Position);
});
probe('Cloth3D.StepMassSpring', () => {
  const sheet = clothGrid.ClothFromGrid(new Mass(0.01), 0, new Length(0.01));
  const settings = new MassSpringSettings(
    new Duration(1 / 240),
    1,
    new Stiffness(200),
    new DampingCoefficient(0.5),
    0.01,
  );
  return String(sheet.StepMassSpring(clothForces, settings, 0).Cloth.Vertices.At(40).Position);
});
probe('Cloth3D.CollideWith(Sphere)', () => {
  const sheet = clothGrid.ClothFromGrid(new Mass(0.01), 0, new Length(0.01));
  return `${sheet.CollideWith(new Sphere(p3(0, 0.9, 0), 0.4)).Cloth.Vertices.Count()} vertices`;
});

console.log(`\n${ok} ok, ${nan} returned NaN, ${bad} failed`);
