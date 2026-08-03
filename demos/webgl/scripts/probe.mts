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
// Add the members your page needs; keep them grouped by the page that wants them.

import '../src/plato/array-ext.ts';
import {
  Angle,
  AngleInterval,
  BezierCurve3D,
  BilinearPatch,
  Bounds3D,
  BoySurface,
  ButterflyCurve2D,
  Catenoid,
  CatmullRomCurve3D,
  Color,
  ColorHSL,
  CubicBezier3D,
  DiniSurface,
  Direction3D,
  EnneperSurface,
  Helix,
  HermiteCurve3D,
  IntegerVector3,
  Intrinsics,
  Matrix4x3,
  Matrix4x4,
  PerlinNoise2D,
  PerlinNoise3D,
  Point2D,
  Point3D,
  Sphere,
  Torus,
  UvCoordinate,
  ValueNoise2D,
  Vector3D,
  WhiteNoise2D,
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
  ).Eval(0.5),
);
probe('Helix.Eval', () => new Helix(1, 0.5, 3).Eval(0.5));
probe('ButterflyCurve2D.Eval', () => new ButterflyCurve2D(1).Eval(0.3));
probe('AngleInterval.Lerp', () => new AngleInterval(new Angle(0), new Angle(6)).Lerp(0.5));

section('parametric surfaces');
probe('Torus.Eval', () => new Torus(p3(0, 0, 0), d3(0, 1, 0), 1, 0.35).Eval(uv(0.25, 0.5)));
probe('Sphere.Eval', () => new Sphere(p3(0, 0, 0), 1).Eval(uv(0.3, 0.4)));
probe('BilinearPatch.Eval', () =>
  new BilinearPatch(p3(0, 0, 0), p3(1, 0, 0), p3(0, 0, 1), p3(1, 1, 1)).Eval(uv(0.5, 0.5)),
);
probe('BoySurface.Eval', () => new BoySurface(1).Eval(uv(0.6, 0.4)));
probe('Catenoid.Eval', () => new Catenoid(1, 1).Eval(uv(0.6, 0.4)));
probe('DiniSurface.Eval', () => new DiniSurface(1, 0.2).Eval(uv(0.6, 0.4)));
probe('EnneperSurface.Eval', () => new EnneperSurface(1).Eval(uv(0.6, 0.4)));

section('noise');
probe('ValueNoise2D.Eval', () => new ValueNoise2D(7, 4).Eval(p2(0.3, 0.7)));
probe('WhiteNoise2D.Eval', () => new WhiteNoise2D(7).Eval(p2(0.3, 0.7)));
probe('PerlinNoise2D.Eval', () => new PerlinNoise2D(7, 4).Eval(p2(0.3, 0.7)));
probe('PerlinNoise3D.Eval', () => new PerlinNoise3D(7, 3).Eval(p3(0.2, 0.3, 0.4)));

section('colours');
probe('Color.Default', () => Color.Default);
probe('ColorHSL value', () => new ColorHSL(new Angle(0.5), 0.7, 0.5));

section('transforms');
probe('Matrix4x4.Identity', () => Matrix4x4.Identity());
probe('Point3D.Transform(AffineTransform3D)', () =>
  p3(1, 2, 3).Transform(Matrix4x3.Identity().AffineTransform3D()),
);

section('marching cubes');
probe('Sphere.ToSdf().MarchingCubes', () =>
  `${new Sphere(p3(0, 0, 0), 1)
    .ToSdf()
    .MarchingCubes(
      new Bounds3D(p3(-1.5, -1.5, -1.5), p3(1.5, 1.5, 1.5)),
      new IntegerVector3(16, 16, 16),
      0,
    )
    .Triangles.Count()} triangles`,
);
probe('PerlinNoise3D.MarchingCubes', () =>
  `${new PerlinNoise3D(7, 2)
    .MarchingCubes(new Bounds3D(p3(-1, -1, -1), p3(1, 1, 1)), new IntegerVector3(12, 12, 12), 0)
    .Triangles.Count()} triangles`,
);

console.log(`\n${ok} ok, ${nan} returned NaN, ${bad} failed`);
