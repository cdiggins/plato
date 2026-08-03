// Noise — a scene catalog over stdlib/geometry/noise.{types,library}.plato.
//
// Every field value on this page comes out of a generated `Eval` or one of the
// members declared beside it — `IsPositiveAt`, `IsAtOrAboveLevel`,
// `IsNearLevelAt`, `MagnitudeAt`, `FlowDirectionAt`, `ScalarFunctionField2D`,
// `MarchingCubes`. Walking a lattice, mapping a value to a colour and packing
// bytes is demo work; a hash, a fade curve, a gradient table and an octave sum
// are not, and none of them is written here.
//
// The brief for this page is *different visualizations* of the same algorithms,
// so the readings get as much room as the fields: a greyscale raster, a colour
// ramp driven by the generated ColorGradient, quantized bands, iso-contours from
// IsNearLevelAt, a binary threshold from IsPositiveAt, a displaced height field,
// a slice through a 3D field, a marching-cubes isosurface, and curl noise drawn
// as arrows and streamlines.
//
// Cost is the risk here — a raster is quadratic and a fractal sum multiplies it
// by the octave count, so every scene puts its resolution on a slider and starts
// modest. The status line reports the sampled min, max and mean, because the
// fields do NOT share a range: WhiteNoise2D returns HashToUnit in 0..1, the
// gradient bases return roughly -1..1, Worley returns a distance, and Gabor
// returns an unbounded sum of kernels. Every raster therefore maps the window's
// own measured range by default, and says what that range was.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import {
  fromArray,
  parametricGeometry,
  polylineGeometry,
  triangleArrayGeometry,
} from '../shared/mesh.js';
import { rasterPlane, type Rgb } from '../shared/raster.js';
import { palette, type ViewerOptions } from '../shared/viewer.js';
import {
  Angle,
  Bounds3D,
  Color,
  ColorGradient,
  ColorLookupTable,
  ColorStop,
  CurlNoise2D,
  CurlNoise3D,
  DomainWarpNoise2D,
  FbmNoise2D,
  FbmNoise3D,
  GaborNoise2D,
  IntegerVector3,
  PerlinNoise2D,
  PerlinNoise3D,
  Point2D,
  Point3D,
  RidgedNoise2D,
  ScalarFunctionField2D,
  SimplexNoise2D,
  SimplexNoise3D,
  TurbulenceNoise2D,
  ValueNoise2D,
  ValueNoise3D,
  Vector2D,
  Vector3D,
  WhiteNoise2D,
  WhiteNoise3D,
  WorleyNoise2D,
  WorleyNoise3D,
  type TriangleArray3D,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// Same pattern as `src/demos/polygons.ts`: a member that throws or returns NaN
// keeps its name in the status line and says so, instead of being quietly
// replaced by an answer this file computed some other way.

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
const n2 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(2);
const pct = (x: number): string => `${(x * 100).toFixed(1)}%`;

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
 * would clip a raster sideways. Shrink to whichever half-extent is smaller,
 * refreshed per frame because the stage is resizable. A no-op under the
 * perspective camera the height field, the isosurface and the 3D flow ask for.
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
// The sum types the prelude supplies
//
// `NoiseBasis`, `WorleyDistance` and `WorleyFeature` are the three sum types
// `plato.g.ts` reports as CHK320 — sum types are C#-only in v1 — so they arrive
// on `globalThis` from `src/plato/array-ext.ts` rather than from the generated
// module. Everything that dispatches on a NoiseBasis (BasisValue2D/3D, the three
// octave contributions, WarpPoint) lives there with them; the fractal types
// themselves are generated, and it is their `Eval` that drives every fractal
// scene below.

interface SumCase {
  readonly Tag: string;
}
type SumFactory = Record<string, () => SumCase>;

const prelude = globalThis as unknown as {
  NoiseBasis: SumFactory;
  WorleyDistance: SumFactory;
  WorleyFeature: SumFactory;
};

const BASIS_LABELS = ['White', 'Value', 'Perlin', 'Simplex', 'Worley', 'Gabor'];
const basisCase = (index: number): SumCase =>
  prelude.NoiseBasis[BASIS_LABELS[clampIndex(index, BASIS_LABELS.length)]]();

const DISTANCE_LABELS = ['Euclid', 'Manhattan', 'Chebyshev', 'Minkowski'];
const DISTANCE_CASES = ['Euclidean', 'Manhattan', 'Chebyshev', 'Minkowski'];
const FEATURE_LABELS = ['F1', 'F2', 'F2-F1', 'F1+F2'];
const FEATURE_CASES = ['F1', 'F2', 'F2MinusF1', 'F1PlusF2'];

function clampIndex(value: number, count: number): number {
  const i = Math.round(value);
  return i < 0 ? 0 : i >= count ? count - 1 : i;
}

// ---------------------------------------------------------------------------
// The shapes every 2D and 3D noise type shares
//
// `noise.types.plato` gives all of them the same interface, so one structural
// type reaches every field on the page and no scene needs to know which one it
// is holding.

interface Noise2D {
  Eval(p: Point2D): number;
  IsPositiveAt(p: Point2D): boolean;
  IsAtOrAboveLevel(p: Point2D, level: number): boolean;
  IsNearLevelAt(p: Point2D, level: number, tolerance: number): boolean;
  ScalarFunctionField2D(): ScalarFunctionField2D;
}

interface Noise3D {
  Eval(p: Point3D): number;
  IsPositiveAt(p: Point3D): boolean;
  MarchingCubes(
    bounds: Bounds3D,
    nodeCounts: IntegerVector3,
    isoLevel: number,
  ): TriangleArray3D;
}

/** One entry of a field select: the generated type, its arguments, and it. */
interface Named2D {
  name: string;
  detail: string;
  members: string[];
  noise: Noise2D;
}

interface Named3D {
  name: string;
  detail: string;
  noise: Noise3D;
}

/** The six basis fields, in the order `NoiseBasis` lists its cases. */
function basisField(index: number, seed: number, frequency: number): Named2D {
  switch (clampIndex(index, 6)) {
    case 0:
      return {
        name: 'WhiteNoise2D',
        detail: `Seed ${seed} — no frequency; the hash is of the coordinates themselves`,
        members: ['WhiteNoise2D.Eval'],
        noise: new WhiteNoise2D(seed),
      };
    case 1:
      return {
        name: 'ValueNoise2D',
        detail: `Seed ${seed} Frequency ${n2(frequency)}`,
        members: ['ValueNoise2D.Eval'],
        noise: new ValueNoise2D(seed, frequency),
      };
    case 2:
      return {
        name: 'PerlinNoise2D',
        detail: `Seed ${seed} Frequency ${n2(frequency)}`,
        members: ['PerlinNoise2D.Eval'],
        noise: new PerlinNoise2D(seed, frequency),
      };
    case 3:
      return {
        name: 'SimplexNoise2D',
        detail: `Seed ${seed} Frequency ${n2(frequency)}`,
        members: ['SimplexNoise2D.Eval'],
        noise: new SimplexNoise2D(seed, frequency),
      };
    case 4:
      return {
        name: 'WorleyNoise2D',
        detail: `Seed ${seed} Frequency ${n2(frequency)} Jitter 1 Euclidean F1`,
        members: ['WorleyNoise2D.Eval'],
        noise: new WorleyNoise2D(
          seed,
          frequency,
          1,
          prelude.WorleyDistance.Euclidean(),
          prelude.WorleyFeature.F1(),
        ),
      };
    default:
      return {
        name: 'GaborNoise2D',
        detail: `Seed ${seed} Frequency ${n2(frequency)} Orientation 0 Bandwidth 1 Anisotropy 0`,
        members: ['GaborNoise2D.Eval'],
        noise: new GaborNoise2D(seed, frequency, new Angle(0), 1, 0),
      };
  }
}

const FRACTAL_LABELS = ['fBm', 'Turbulence', 'Ridged', 'Warp'];

/** The four fractal sums, each over a selectable basis. */
function fractalField(
  kind: number,
  basis: number,
  seed: number,
  frequency: number,
  octaves: number,
  lacunarity: number,
  gain: number,
  offset: number,
): Named2D {
  const b = basisCase(basis);
  const label = BASIS_LABELS[clampIndex(basis, BASIS_LABELS.length)];
  const shared = `basis ${label} Seed ${seed} Frequency ${n2(frequency)}`;
  const sum = `Octaves ${octaves} Lacunarity ${n2(lacunarity)} Gain ${n2(gain)}`;
  switch (clampIndex(kind, 4)) {
    case 0:
      return {
        name: 'FbmNoise2D',
        detail: `${shared} ${sum}`,
        members: ['FbmNoise2D.Eval'],
        noise: new FbmNoise2D(b, seed, frequency, octaves, lacunarity, gain),
      };
    case 1:
      return {
        name: 'TurbulenceNoise2D',
        detail: `${shared} ${sum}`,
        members: ['TurbulenceNoise2D.Eval'],
        noise: new TurbulenceNoise2D(b, seed, frequency, octaves, lacunarity, gain),
      };
    case 2:
      return {
        name: 'RidgedNoise2D',
        detail: `${shared} ${sum} Offset ${n2(offset)}`,
        members: ['RidgedNoise2D.Eval'],
        noise: new RidgedNoise2D(b, seed, frequency, octaves, lacunarity, gain, offset),
      };
    default:
      return {
        name: 'DomainWarpNoise2D',
        detail: `${shared} WarpStrength ${n2(gain)} WarpFrequency ${n2(lacunarity)} Iterations ${octaves}`,
        members: ['DomainWarpNoise2D.Eval'],
        noise: new DomainWarpNoise2D(b, seed, frequency, gain, lacunarity, octaves),
      };
  }
}

/** The catalog the readings, height-field and contour scenes share. */
const FIELD_LABELS = ['Perlin', 'Simplex', 'Value', 'Worley', 'fBm', 'Ridged', 'Warp'];

function catalogField(
  index: number,
  seed: number,
  frequency: number,
  octaves: number,
): Named2D {
  switch (clampIndex(index, FIELD_LABELS.length)) {
    case 0:
      return basisField(2, seed, frequency);
    case 1:
      return basisField(3, seed, frequency);
    case 2:
      return basisField(1, seed, frequency);
    case 3:
      return basisField(4, seed, frequency);
    case 4:
      return fractalField(0, 2, seed, frequency, octaves, 2, 0.5, 1);
    case 5:
      return fractalField(2, 2, seed, frequency, octaves, 2, 0.5, 1);
    default:
      return fractalField(3, 2, seed, frequency, octaves, 2, 0.45, 1);
  }
}

const FIELD_3D_LABELS = ['Perlin', 'Simplex', 'Value', 'White', 'Worley', 'fBm'];

function field3D(index: number, seed: number, frequency: number, octaves: number): Named3D {
  switch (clampIndex(index, FIELD_3D_LABELS.length)) {
    case 0:
      return {
        name: 'PerlinNoise3D',
        detail: `Seed ${seed} Frequency ${n2(frequency)}`,
        noise: new PerlinNoise3D(seed, frequency),
      };
    case 1:
      return {
        name: 'SimplexNoise3D',
        detail: `Seed ${seed} Frequency ${n2(frequency)}`,
        noise: new SimplexNoise3D(seed, frequency),
      };
    case 2:
      return {
        name: 'ValueNoise3D',
        detail: `Seed ${seed} Frequency ${n2(frequency)}`,
        noise: new ValueNoise3D(seed, frequency),
      };
    case 3:
      return {
        name: 'WhiteNoise3D',
        detail: `Seed ${seed} — unfiltered, so a slice is uncorrelated pixel to pixel`,
        noise: new WhiteNoise3D(seed),
      };
    case 4:
      return {
        name: 'WorleyNoise3D',
        detail: `Seed ${seed} Frequency ${n2(frequency)} Jitter 1 Euclidean F1`,
        noise: new WorleyNoise3D(
          seed,
          frequency,
          1,
          prelude.WorleyDistance.Euclidean(),
          prelude.WorleyFeature.F1(),
        ),
      };
    default:
      return {
        name: 'FbmNoise3D',
        detail: `basis Perlin Seed ${seed} Frequency ${n2(frequency)} Octaves ${octaves}`,
        noise: new FbmNoise3D(prelude.NoiseBasis.Perlin(), seed, frequency, octaves, 2, 0.5),
      };
  }
}

// ---------------------------------------------------------------------------
// Sampling a field
//
// One pass of `Eval` over the same lattice `rasterPlane` walks, kept so the
// readings below can map it several ways without evaluating the field again.
// The measured range is the honest thing to normalise by: the six bases do not
// share one, and the fractal sums narrow theirs as octaves accumulate.

interface FieldSample {
  values: Float64Array;
  resolution: number;
  min: number;
  max: number;
  mean: number;
  /** Samples that came back NaN — a finding, never silently dropped. */
  nan: number;
  ms: number;
}

function sampleField(resolution: number, evaluate: (u: number, v: number) => number): FieldSample {
  const values = new Float64Array(resolution * resolution);
  const started = performance.now();
  let min = Infinity;
  let max = -Infinity;
  let sum = 0;
  let nan = 0;
  for (let j = 0; j < resolution; j++) {
    const v = (j + 0.5) / resolution;
    for (let i = 0; i < resolution; i++) {
      const value = evaluate((i + 0.5) / resolution, v);
      values[j * resolution + i] = value;
      if (Number.isFinite(value)) {
        if (value < min) min = value;
        if (value > max) max = value;
        sum += value;
      } else {
        nan++;
      }
    }
  }
  const finite = values.length - nan;
  return {
    values,
    resolution,
    min: finite > 0 ? min : NaN,
    max: finite > 0 ? max : NaN,
    mean: finite > 0 ? sum / finite : NaN,
    nan,
    ms: performance.now() - started,
  };
}

/** The stored value under a `rasterPlane` (u, v), which walks the same lattice. */
function valueAt(sample: FieldSample, u: number, v: number): number {
  const i = Math.min(sample.resolution - 1, Math.max(0, Math.floor(u * sample.resolution)));
  const j = Math.min(sample.resolution - 1, Math.max(0, Math.floor(v * sample.resolution)));
  return sample.values[j * sample.resolution + i];
}

/** Where a value sits in the window it was measured over, 0..1. */
function normalizer(sample: FieldSample, fixed: boolean): (value: number) => number {
  if (fixed) return value => (value + 1) / 2;
  const span = sample.max - sample.min;
  if (!Number.isFinite(span) || span <= 0) return () => 0.5;
  return value => (value - sample.min) / span;
}

function statReadings(sample: FieldSample): Reading[] {
  const readings = [
    note('samples', `${sample.resolution}x${sample.resolution}`),
    note('min', n4(sample.min)),
    note('max', n4(sample.max)),
    note('mean', n4(sample.mean)),
    note('Eval', `${sample.ms.toFixed(1)} ms`),
  ];
  if (sample.nan > 0) {
    readings.push(note('Eval NaN', `${sample.nan} of ${sample.values.length} samples`));
  }
  return readings;
}

// ---------------------------------------------------------------------------
// Reading a value as a colour
//
// The ramps are `ColorGradient`s and every colour comes out of
// `ColorGradient.ColorAtParameter`. Calling it per pixel would re-reduce the
// stop list a hundred thousand times a rebuild, so each build fills a
// `ColorLookupTable` — the generated type from `image.types.plato` — with 256
// samples of the gradient and the raster indexes that. Indexing is demo work;
// the interpolation is not.

const RAMP_LABELS = ['Grey', 'Signed', 'Terrain', 'Heat'];

const opaque = (r: number, g: number, b: number): Color => new Color(r, g, b, 1);

const RAMPS: ColorGradient[] = [
  new ColorGradient(
    fromArray([new ColorStop(0, opaque(0, 0, 0)), new ColorStop(1, opaque(1, 1, 1))]),
  ),
  new ColorGradient(
    fromArray([
      new ColorStop(0, opaque(0.09, 0.29, 0.55)),
      new ColorStop(0.5, opaque(0.96, 0.96, 0.86)),
      new ColorStop(1, opaque(0.72, 0.18, 0.13)),
    ]),
  ),
  new ColorGradient(
    fromArray([
      new ColorStop(0, opaque(0.04, 0.11, 0.29)),
      new ColorStop(0.34, opaque(0.13, 0.35, 0.6)),
      new ColorStop(0.42, opaque(0.83, 0.77, 0.5)),
      new ColorStop(0.55, opaque(0.26, 0.47, 0.24)),
      new ColorStop(0.76, opaque(0.42, 0.34, 0.26)),
      new ColorStop(1, opaque(0.97, 0.97, 0.99)),
    ]),
  ),
  new ColorGradient(
    fromArray([
      new ColorStop(0, opaque(0.02, 0.02, 0.06)),
      new ColorStop(0.35, opaque(0.55, 0.08, 0.12)),
      new ColorStop(0.62, opaque(0.9, 0.42, 0.06)),
      new ColorStop(0.84, opaque(0.98, 0.85, 0.29)),
      new ColorStop(1, opaque(1, 1, 0.95)),
    ]),
  ),
];

const RAMP_STEPS = 256;

/** 256 samples of a generated gradient, held in the generated table type. */
function rampTable(index: number): ColorLookupTable {
  const gradient = RAMPS[clampIndex(index, RAMPS.length)];
  const colors: Color[] = [];
  for (let i = 0; i < RAMP_STEPS; i++) {
    colors.push(gradient.ColorAtParameter(i / (RAMP_STEPS - 1)));
  }
  return new ColorLookupTable(RAMP_STEPS, fromArray(colors));
}

function rampLookup(table: ColorLookupTable): (t: number) => Rgb {
  const cache: Rgb[] = [];
  for (let i = 0; i < RAMP_STEPS; i++) {
    const c = table.Colors.At(i);
    cache.push({ r: c.R, g: c.G, b: c.B });
  }
  return (t: number): Rgb => {
    if (!Number.isFinite(t)) return MISSING;
    const i = Math.round(t * (RAMP_STEPS - 1));
    return cache[i < 0 ? 0 : i >= RAMP_STEPS ? RAMP_STEPS - 1 : i];
  };
}

/** What a NaN sample looks like: magenta, so it cannot be mistaken for a value. */
const MISSING: Rgb = { r: 0.85, g: 0.1, b: 0.65 };
const BACKDROP: Rgb = { r: 0.05, g: 0.06, b: 0.08 };

// ---------------------------------------------------------------------------
// Presentation helpers

function segments(coordinates: number[], color: number): THREE.LineSegments {
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(coordinates, 3));
  return new THREE.LineSegments(geometry, new THREE.LineBasicMaterial({ color }));
}

/** A rectangle outline in the raster plane's own coordinates. */
function frame(x0: number, y0: number, x1: number, y1: number, color: number): THREE.LineSegments {
  const z = 0.02;
  return segments(
    [x0, y0, z, x1, y0, z, x1, y0, z, x1, y1, z, x1, y1, z, x0, y1, z, x0, y1, z, x0, y0, z],
    color,
  );
}

function line3D(points: readonly Point3D[], color: number): THREE.Line {
  return new THREE.Line(polylineGeometry(points), new THREE.LineBasicMaterial({ color }));
}

// ---------------------------------------------------------------------------
// Scenes

const PLANAR: ViewerOptions = { orthographic: true, grid: false, spin: false };
const SPATIAL: ViewerOptions = { orthographic: false, grid: true, spin: false, distance: 4.2 };

const basisGallery = sceneOf({
  id: 'basis',
  title: 'The basis gallery',
  description:
    'Every basis field noise.types.plato declares, rendered as a raster over the unit square: ' +
    'WhiteNoise2D hashes the coordinates straight to a unit value, ValueNoise2D interpolates hashed ' +
    'lattice corners with SmootherStep, PerlinNoise2D and SimplexNoise2D dot a hashed gradient against ' +
    'the offset, WorleyNoise2D takes the nearest of nine jittered feature points, and GaborNoise2D sums ' +
    'nine windowed cosine kernels. They do not share a range — the status line reports the measured min, ' +
    'max and mean of exactly the window on screen, and the raster maps that range unless the toggle ' +
    'pins it to [-1, 1] so absolute levels can be compared across fields.',
  plato: [
    'WhiteNoise2D.Eval',
    'ValueNoise2D.Eval',
    'PerlinNoise2D.Eval',
    'SimplexNoise2D.Eval',
    'WorleyNoise2D.Eval',
    'GaborNoise2D.Eval',
    'ColorGradient.ColorAtParameter',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'basis', label: 'Basis', kind: 'select', options: BASIS_LABELS, def: 2 },
    { key: 'ramp', label: 'Ramp', kind: 'select', options: RAMP_LABELS, def: 0 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 1, max: 24, step: 1, def: 6 },
    { key: 'resolution', label: 'Resolution', kind: 'slider', min: 32, max: 192, step: 16, def: 128 },
    { key: 'fixed', label: 'Fixed [-1, 1]', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const field = basisField(params.basis, Math.round(params.seed), params.frequency);
    const resolution = Math.round(params.resolution);
    const sample = sampleField(resolution, (u, v) => field.noise.Eval(new Point2D(u, v)));
    const to01 = normalizer(sample, params.fixed > 0.5);
    const ramp = rampLookup(rampTable(params.ramp));

    const object = new THREE.Group();
    object.add(
      rasterPlane((u, v) => ramp(to01(valueAt(sample, u, v))), { resolution, size: 1.9 }),
    );
    object.add(frame(-0.95, -0.95, 0.95, 0.95, 0x27303d));

    const centre = new Point2D(0.5, 0.5);
    return {
      object,
      readings: [
        note('field', `${field.name} · ${field.detail}`),
        ...statReadings(sample),
        reading('Eval(0.5, 0.5)', () => n4(field.noise.Eval(centre))),
        reading('IsPositiveAt(0.5, 0.5)', () => String(field.noise.IsPositiveAt(centre))),
        reading('ScalarFunctionField2D().Eval', () =>
          n4(field.noise.ScalarFunctionField2D().Eval(centre)),
        ),
        note('mapped over', params.fixed > 0.5 ? '[-1, 1]' : 'the measured range'),
      ],
    };
  },
});

const fractalSums = sceneOf({
  id: 'fractals',
  title: 'Fractal sums',
  description:
    'FbmNoise2D, TurbulenceNoise2D and RidgedNoise2D over any of the six bases. All three sum Octaves ' +
    'copies of the basis, each at Lacunarity times the last frequency and Gain times the last weight, ' +
    'and each divides by the total weight so the sum stays in range: fBm sums the signed octave, ' +
    'turbulence sums its absolute value, and ridged inverts each octave about Offset and squares it. ' +
    'Cost is the octave count times the raster, so both are on sliders and the status line reports how ' +
    'long the pass took.',
  plato: [
    'FbmNoise2D.Eval',
    'TurbulenceNoise2D.Eval',
    'RidgedNoise2D.Eval',
    'ColorGradient.ColorAtParameter',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'kind', label: 'Sum', kind: 'select', options: ['fBm', 'Turbulence', 'Ridged'], def: 0 },
    { key: 'basis', label: 'Basis', kind: 'select', options: BASIS_LABELS, def: 2 },
    { key: 'ramp', label: 'Ramp', kind: 'select', options: RAMP_LABELS, def: 0 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 1, max: 12, step: 1, def: 3 },
    { key: 'octaves', label: 'Octaves', kind: 'slider', min: 1, max: 8, step: 1, def: 4 },
    { key: 'lacunarity', label: 'Lacunarity', kind: 'slider', min: 1.2, max: 3.2, step: 0.05, def: 2 },
    { key: 'gain', label: 'Gain', kind: 'slider', min: 0.2, max: 0.9, step: 0.01, def: 0.5 },
    { key: 'offset', label: 'Ridge offset', kind: 'slider', min: 0.4, max: 1.6, step: 0.05, def: 1 },
    // Capped below the other rasters: a fractal sum costs its octave count times
    // a basis, and the Gabor basis is nine kernels per sample on its own.
    { key: 'resolution', label: 'Resolution', kind: 'slider', min: 32, max: 128, step: 16, def: 96 },
  ],
  build(params: Params): Built {
    const field = fractalField(
      params.kind,
      params.basis,
      Math.round(params.seed),
      params.frequency,
      Math.round(params.octaves),
      params.lacunarity,
      params.gain,
      params.offset,
    );
    const resolution = Math.round(params.resolution);
    const sample = sampleField(resolution, (u, v) => field.noise.Eval(new Point2D(u, v)));
    const to01 = normalizer(sample, false);
    const ramp = rampLookup(rampTable(params.ramp));

    const object = new THREE.Group();
    object.add(
      rasterPlane((u, v) => ramp(to01(valueAt(sample, u, v))), { resolution, size: 1.9 }),
    );
    object.add(frame(-0.95, -0.95, 0.95, 0.95, 0x27303d));

    return {
      object,
      readings: [
        note('field', `${field.name} · ${field.detail}`),
        ...statReadings(sample),
        note('basis evaluations', String(sample.values.length * Math.round(params.octaves))),
        reading('Eval(0.5, 0.5)', () => n4(field.noise.Eval(new Point2D(0.5, 0.5)))),
      ],
    };
  },
});

const OCTAVE_TILES = [1, 2, 4, 8];

const octaveBuildup = sceneOf({
  id: 'octaves',
  title: 'Octave build-up',
  description:
    'The same FbmNoise2D at one, two, four and eight octaves — clockwise from the top left — with every ' +
    'other parameter held. Each tile is its own FbmNoise2D built through WithOctaves, so the four ' +
    'pictures differ only in how many copies of the basis the generated Eval sums. The status line ' +
    'reports each tile\'s measured range: the sum divides by the total weight, so adding octaves adds ' +
    'detail without widening the range much.',
  plato: ['FbmNoise2D.Eval', 'FbmNoise2D.WithOctaves', 'ColorGradient.ColorAtParameter'],
  viewer: PLANAR,
  controls: [
    { key: 'basis', label: 'Basis', kind: 'select', options: BASIS_LABELS, def: 2 },
    { key: 'ramp', label: 'Ramp', kind: 'select', options: RAMP_LABELS, def: 0 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 1, max: 12, step: 1, def: 3 },
    { key: 'lacunarity', label: 'Lacunarity', kind: 'slider', min: 1.2, max: 3.2, step: 0.05, def: 2 },
    { key: 'gain', label: 'Gain', kind: 'slider', min: 0.2, max: 0.9, step: 0.01, def: 0.5 },
    { key: 'resolution', label: 'Tile resolution', kind: 'slider', min: 24, max: 96, step: 8, def: 56 },
  ],
  build(params: Params): Built {
    const seed = Math.round(params.seed);
    const base = new FbmNoise2D(
      basisCase(params.basis),
      seed,
      params.frequency,
      1,
      params.lacunarity,
      params.gain,
    );
    const resolution = Math.round(params.resolution);
    const ramp = rampLookup(rampTable(params.ramp));

    const object = new THREE.Group();
    const readings: Reading[] = [
      note('field', `FbmNoise2D · basis ${BASIS_LABELS[clampIndex(params.basis, 6)]} Seed ${seed}`),
    ];
    // Clockwise from the top left, so the reading order matches the picture.
    const corners: [number, number][] = [
      [-0.48, 0.48],
      [0.48, 0.48],
      [0.48, -0.48],
      [-0.48, -0.48],
    ];
    let total = 0;
    for (let t = 0; t < OCTAVE_TILES.length; t++) {
      const octaves = OCTAVE_TILES[t];
      const field = base.WithOctaves(octaves);
      const sample = sampleField(resolution, (u, v) => field.Eval(new Point2D(u, v)));
      const to01 = normalizer(sample, false);
      const tile = rasterPlane((u, v) => ramp(to01(valueAt(sample, u, v))), {
        resolution,
        size: 0.92,
      });
      tile.position.set(corners[t][0], corners[t][1], 0);
      object.add(tile);
      object.add(
        frame(
          corners[t][0] - 0.46,
          corners[t][1] - 0.46,
          corners[t][0] + 0.46,
          corners[t][1] + 0.46,
          0x27303d,
        ),
      );
      total += sample.ms;
      readings.push(
        note(
          `${octaves} octave${octaves === 1 ? '' : 's'}`,
          `min ${n2(sample.min)} max ${n2(sample.max)} mean ${n2(sample.mean)}`,
        ),
      );
    }
    readings.push(
      note('tiles', `4 x ${resolution}x${resolution}`),
      note('Eval', `${total.toFixed(1)} ms`),
    );
    return { object, readings };
  },
});

const domainWarp = sceneOf({
  id: 'warp',
  title: 'Domain warp, before and after',
  description:
    'DomainWarpNoise2D displaces the sample point by the basis field itself — read at two decorrelated ' +
    'seeds for the two components — and then reads the basis at the moved point, Iterations times over. ' +
    'The left half is the same generated type through WithWarpStrength(0), which is exactly the ' +
    'unwarped basis, so the two halves differ only by the warp. Both halves are mapped over the same ' +
    'range, so the comparison is of the fields and not of two normalisations.',
  plato: [
    'DomainWarpNoise2D.Eval',
    'DomainWarpNoise2D.WithWarpStrength',
    'ColorGradient.ColorAtParameter',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'basis', label: 'Basis', kind: 'select', options: BASIS_LABELS, def: 2 },
    { key: 'ramp', label: 'Ramp', kind: 'select', options: RAMP_LABELS, def: 0 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 1, max: 12, step: 1, def: 4 },
    { key: 'strength', label: 'Warp strength', kind: 'slider', min: 0, max: 1.2, step: 0.02, def: 0.4 },
    { key: 'warpFreq', label: 'Warp frequency', kind: 'slider', min: 0.5, max: 8, step: 0.25, def: 2 },
    { key: 'iterations', label: 'Iterations', kind: 'slider', min: 1, max: 5, step: 1, def: 3 },
    { key: 'resolution', label: 'Resolution', kind: 'slider', min: 24, max: 128, step: 8, def: 72 },
  ],
  build(params: Params): Built {
    const seed = Math.round(params.seed);
    const iterations = Math.round(params.iterations);
    const warped = new DomainWarpNoise2D(
      basisCase(params.basis),
      seed,
      params.frequency,
      params.strength,
      params.warpFreq,
      iterations,
    );
    const plain = warped.WithWarpStrength(0);
    const resolution = Math.round(params.resolution);

    const left = sampleField(resolution, (u, v) => plain.Eval(new Point2D(u, v)));
    const right = sampleField(resolution, (u, v) => warped.Eval(new Point2D(u, v)));
    // One normalisation for both halves, so the pictures are comparable.
    const min = Math.min(left.min, right.min);
    const max = Math.max(left.max, right.max);
    const span = max - min;
    const to01 = (value: number): number => (span > 0 ? (value - min) / span : 0.5);
    const ramp = rampLookup(rampTable(params.ramp));

    const object = new THREE.Group();
    const tileA = rasterPlane((u, v) => ramp(to01(valueAt(left, u, v))), {
      resolution,
      size: 0.94,
    });
    tileA.position.set(-0.48, 0, 0);
    const tileB = rasterPlane((u, v) => ramp(to01(valueAt(right, u, v))), {
      resolution,
      size: 0.94,
    });
    tileB.position.set(0.48, 0, 0);
    object.add(tileA, tileB);
    object.add(frame(-0.95, -0.47, -0.01, 0.47, 0x27303d));
    object.add(frame(0.01, -0.47, 0.95, 0.47, palette.accent));

    return {
      object,
      readings: [
        note('field', `DomainWarpNoise2D · basis ${BASIS_LABELS[clampIndex(params.basis, 6)]}`),
        note('left', `WithWarpStrength(0) — min ${n2(left.min)} max ${n2(left.max)}`),
        note(
          'right',
          `WarpStrength ${n2(params.strength)} Iterations ${iterations} — min ${n2(right.min)} max ${n2(right.max)}`,
        ),
        note('samples', `2 x ${resolution}x${resolution}`),
        note('Eval', `${(left.ms + right.ms).toFixed(1)} ms`),
        reading('warped Eval(0.5, 0.5)', () => n4(warped.Eval(new Point2D(0.5, 0.5)))),
        reading('unwarped Eval(0.5, 0.5)', () => n4(plain.Eval(new Point2D(0.5, 0.5)))),
      ],
    };
  },
});

const READING_LABELS = ['Grey', 'Ramp', 'Bands', 'Contour', 'Level', 'Sign'];

const readings = sceneOf({
  id: 'readings',
  title: 'Six readings of one field',
  description:
    'One scalar field, read six ways. Grey and Ramp map the Eval value continuously; Bands quantises it; ' +
    'Contour asks the generated IsNearLevelAt whether the point is within Tolerance of the nearest of N ' +
    'evenly spaced levels; Level asks IsAtOrAboveLevel; Sign asks IsPositiveAt. The last three are ' +
    'predicates every type in noise.types.plato carries, and none of them is re-derived here from the ' +
    'sampled value — they are called per pixel and their true-count is reported as an area fraction.',
  plato: [
    'PerlinNoise2D.Eval',
    'SimplexNoise2D.Eval',
    'ValueNoise2D.Eval',
    'WorleyNoise2D.Eval',
    'FbmNoise2D.Eval',
    'RidgedNoise2D.Eval',
    'DomainWarpNoise2D.Eval',
    'IScalarField2D.IsNearLevelAt',
    'IScalarField2D.IsAtOrAboveLevel',
    'IScalarField2D.IsPositiveAt',
    'IScalarField2D.ScalarFunctionField2D',
    'ColorGradient.ColorAtParameter',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'field', label: 'Field', kind: 'select', options: FIELD_LABELS, def: 4 },
    { key: 'mode', label: 'Reading', kind: 'select', options: READING_LABELS, def: 1 },
    { key: 'ramp', label: 'Ramp', kind: 'select', options: RAMP_LABELS, def: 2 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 1, max: 12, step: 1, def: 4 },
    { key: 'octaves', label: 'Octaves', kind: 'slider', min: 1, max: 6, step: 1, def: 4 },
    { key: 'level', label: 'Level', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0 },
    { key: 'tolerance', label: 'Tolerance', kind: 'slider', min: 0.002, max: 0.12, step: 0.002, def: 0.02 },
    { key: 'bands', label: 'Bands', kind: 'slider', min: 2, max: 16, step: 1, def: 8 },
    { key: 'resolution', label: 'Resolution', kind: 'slider', min: 32, max: 160, step: 16, def: 96 },
  ],
  build(params: Params): Built {
    const field = catalogField(
      params.field,
      Math.round(params.seed),
      params.frequency,
      Math.round(params.octaves),
    );
    const resolution = Math.round(params.resolution);
    const mode = clampIndex(params.mode, READING_LABELS.length);
    const bands = Math.round(params.bands);
    const level = params.level;
    const tolerance = params.tolerance;

    const sample = sampleField(resolution, (u, v) => field.noise.Eval(new Point2D(u, v)));
    const to01 = normalizer(sample, false);
    const ramp = rampLookup(rampTable(mode === 0 ? 0 : params.ramp));

    // Levels the contour reading asks about, spread over the measured range.
    const levels: number[] = [];
    for (let i = 1; i <= bands; i++) {
      levels.push(sample.min + ((sample.max - sample.min) * i) / (bands + 1));
    }

    let hits = 0;
    const shade = (u: number, v: number): Rgb => {
      const value = valueAt(sample, u, v);
      if (!Number.isFinite(value)) return MISSING;
      const p = new Point2D(u, v);
      switch (mode) {
        case 0:
        case 1:
          return ramp(to01(value));
        case 2: {
          const band = Math.min(bands - 1, Math.floor(to01(value) * bands));
          return ramp(band / Math.max(1, bands - 1));
        }
        case 3: {
          // The nearest level is arithmetic on the sampled value; whether the
          // point is ON it is the generated predicate's answer.
          let nearest = levels[0];
          for (const l of levels) {
            if (Math.abs(value - l) < Math.abs(value - nearest)) nearest = l;
          }
          if (field.noise.IsNearLevelAt(p, nearest, tolerance)) {
            hits++;
            return { r: 0.95, g: 0.97, b: 1 };
          }
          const band = Math.min(bands - 1, Math.floor(to01(value) * bands));
          const shadeOf = ramp((band / Math.max(1, bands - 1)) * 0.72 + 0.05);
          return { r: shadeOf.r * 0.55, g: shadeOf.g * 0.55, b: shadeOf.b * 0.55 };
        }
        case 4: {
          const above = field.noise.IsAtOrAboveLevel(p, level);
          if (above) hits++;
          return above ? ramp(0.82) : BACKDROP;
        }
        default: {
          const positive = field.noise.IsPositiveAt(p);
          if (positive) hits++;
          return positive ? ramp(0.82) : BACKDROP;
        }
      }
    };

    const object = new THREE.Group();
    object.add(rasterPlane(shade, { resolution, size: 1.9 }));
    object.add(frame(-0.95, -0.95, 0.95, 0.95, 0x27303d));

    const centre = new Point2D(0.5, 0.5);
    const out: Reading[] = [
      note('field', `${field.name} · ${field.detail}`),
      note('reading', READING_LABELS[mode]),
      ...statReadings(sample),
    ];
    if (mode === 3) {
      out.push(
        note('IsNearLevelAt levels', String(bands)),
        note('IsNearLevelAt true', `${pct(hits / sample.values.length)} of the window`),
        reading('IsNearLevelAt(0.5, 0.5)', () =>
          String(field.noise.IsNearLevelAt(centre, level, tolerance)),
        ),
      );
    } else if (mode === 4) {
      out.push(
        note('IsAtOrAboveLevel true', `${pct(hits / sample.values.length)} of the window`),
        reading('IsAtOrAboveLevel(0.5, 0.5)', () =>
          String(field.noise.IsAtOrAboveLevel(centre, level)),
        ),
      );
    } else if (mode === 5) {
      out.push(
        note('IsPositiveAt true', `${pct(hits / sample.values.length)} of the window`),
        reading('IsPositiveAt(0.5, 0.5)', () => String(field.noise.IsPositiveAt(centre))),
      );
    } else {
      out.push(reading('Eval(0.5, 0.5)', () => n4(field.noise.Eval(centre))));
    }
    out.push(
      reading('ScalarFunctionField2D().IsPositiveAt', () =>
        String(field.noise.ScalarFunctionField2D().IsPositiveAt(centre)),
      ),
    );
    return { object, readings: out };
  },
});

const worley = sceneOf({
  id: 'worley',
  title: 'Worley in detail',
  description:
    'WorleyNoise2D scores the nine neighbouring cells of the lattice, offsets each cell\'s feature point ' +
    'by Jitter, measures with Vector2D.WorleyLength under the chosen WorleyDistance, and combines the ' +
    'nearest two with Number.WorleyCombine under the chosen WorleyFeature. Both are sum types the ' +
    'TypeScript target drops (CHK320) and the prelude supplies as tag objects, and both change the ' +
    'picture completely: Chebyshev squares the cells, Manhattan turns them to diamonds, F2-F1 draws the ' +
    'cell walls rather than the cell interiors, and jitter at zero collapses the whole thing to a grid.',
  plato: [
    'WorleyNoise2D.Eval',
    'WorleyNoise2D.WorleyNeighbour2D',
    'Vector2D.WorleyLength',
    'Number.WorleyCombine',
    'Array.NearestDistance',
    'Array.SecondNearestDistance',
    'ColorGradient.ColorAtParameter',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'distance', label: 'Metric', kind: 'select', options: DISTANCE_LABELS, def: 0 },
    { key: 'feature', label: 'Feature', kind: 'select', options: FEATURE_LABELS, def: 0 },
    { key: 'ramp', label: 'Ramp', kind: 'select', options: RAMP_LABELS, def: 0 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 1, max: 16, step: 1, def: 6 },
    { key: 'jitter', label: 'Jitter', kind: 'slider', min: 0, max: 1, step: 0.02, def: 1 },
    { key: 'resolution', label: 'Resolution', kind: 'slider', min: 32, max: 160, step: 16, def: 96 },
  ],
  build(params: Params): Built {
    const distance = clampIndex(params.distance, DISTANCE_CASES.length);
    const feature = clampIndex(params.feature, FEATURE_CASES.length);
    const seed = Math.round(params.seed);
    const noise = new WorleyNoise2D(
      seed,
      params.frequency,
      params.jitter,
      prelude.WorleyDistance[DISTANCE_CASES[distance]](),
      prelude.WorleyFeature[FEATURE_CASES[feature]](),
    );
    const resolution = Math.round(params.resolution);
    const sample = sampleField(resolution, (u, v) => noise.Eval(new Point2D(u, v)));
    const to01 = normalizer(sample, false);
    const ramp = rampLookup(rampTable(params.ramp));

    const object = new THREE.Group();
    object.add(
      rasterPlane((u, v) => ramp(to01(valueAt(sample, u, v))), { resolution, size: 1.9 }),
    );
    object.add(frame(-0.95, -0.95, 0.95, 0.95, 0x27303d));

    return {
      object,
      readings: [
        note(
          'field',
          `WorleyNoise2D · Seed ${seed} Frequency ${n2(params.frequency)} Jitter ${n2(params.jitter)}`,
        ),
        note('WorleyDistance', DISTANCE_CASES[distance]),
        note('WorleyFeature', FEATURE_CASES[feature]),
        ...statReadings(sample),
        reading('Eval(0.5, 0.5)', () => n4(noise.Eval(new Point2D(0.5, 0.5)))),
        reading('WorleyNeighbour2D centre cell', () =>
          n4(noise.WorleyNeighbour2D(0, 0, 0.5, 0.5, 4)),
        ),
      ],
    };
  },
});

const heightField = sceneOf({
  id: 'heightfield',
  title: 'The field as a height field',
  description:
    'The same scalar field again, this time displacing a lattice: every vertex sits at (u, Eval(u, v), v) ' +
    'and is coloured by its own height through the generated ColorGradient. Nothing about the field ' +
    'changes — only the reading — and the shading makes visible what a raster flattens: fBm\'s ridges ' +
    'against turbulence\'s creases, and the sharp valley floors RidgedNoise2D squares into place. ' +
    'Lattice cost is quadratic, so the step count is on a slider.',
  plato: [
    'PerlinNoise2D.Eval',
    'SimplexNoise2D.Eval',
    'ValueNoise2D.Eval',
    'WorleyNoise2D.Eval',
    'FbmNoise2D.Eval',
    'RidgedNoise2D.Eval',
    'DomainWarpNoise2D.Eval',
    'ColorGradient.ColorAtParameter',
  ],
  viewer: SPATIAL,
  controls: [
    { key: 'field', label: 'Field', kind: 'select', options: FIELD_LABELS, def: 4 },
    { key: 'ramp', label: 'Ramp', kind: 'select', options: RAMP_LABELS, def: 2 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 1, max: 12, step: 1, def: 4 },
    { key: 'octaves', label: 'Octaves', kind: 'slider', min: 1, max: 8, step: 1, def: 5 },
    { key: 'amplitude', label: 'Amplitude', kind: 'slider', min: 0.05, max: 1.2, step: 0.05, def: 0.5 },
    { key: 'steps', label: 'Lattice steps', kind: 'slider', min: 16, max: 128, step: 8, def: 64 },
    { key: 'wire', label: 'Show lattice', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const field = catalogField(
      params.field,
      Math.round(params.seed),
      params.frequency,
      Math.round(params.octaves),
    );
    const steps = Math.round(params.steps);
    const amplitude = params.amplitude;

    let min = Infinity;
    let max = -Infinity;
    let sum = 0;
    let count = 0;
    let nan = 0;
    const started = performance.now();
    const geometry = parametricGeometry(
      (u, v) => {
        const value = field.noise.Eval(new Point2D(u, v));
        if (Number.isFinite(value)) {
          if (value < min) min = value;
          if (value > max) max = value;
          sum += value;
          count++;
        } else {
          nan++;
        }
        return new Point3D(u * 2 - 1, Number.isFinite(value) ? value * amplitude : 0, v * 2 - 1);
      },
      steps,
      steps,
    );
    const ms = performance.now() - started;

    // Colour by height, which is the field value again — repacking, not maths.
    const ramp = rampLookup(rampTable(params.ramp));
    const position = geometry.getAttribute('position');
    const colors = new Float32Array(position.count * 3);
    const span = max - min;
    for (let i = 0; i < position.count; i++) {
      const value = amplitude > 0 ? position.getY(i) / amplitude : 0;
      const c = ramp(span > 0 ? (value - min) / span : 0.5);
      colors[i * 3] = c.r;
      colors[i * 3 + 1] = c.g;
      colors[i * 3 + 2] = c.b;
    }
    geometry.setAttribute('color', new THREE.BufferAttribute(colors, 3));

    const object = new THREE.Group();
    object.add(
      new THREE.Mesh(
        geometry,
        new THREE.MeshStandardMaterial({
          vertexColors: true,
          roughness: 0.72,
          metalness: 0.04,
          side: THREE.DoubleSide,
        }),
      ),
    );
    if (params.wire > 0.5) {
      object.add(
        new THREE.LineSegments(
          new THREE.WireframeGeometry(geometry),
          new THREE.LineBasicMaterial({ color: 0x1b2430, transparent: true, opacity: 0.5 }),
        ),
      );
    }

    return {
      object,
      readings: [
        note('field', `${field.name} · ${field.detail}`),
        note('lattice', `${steps + 1}x${steps + 1} vertices`),
        note('min', n4(count > 0 ? min : NaN)),
        note('max', n4(count > 0 ? max : NaN)),
        note('mean', n4(count > 0 ? sum / count : NaN)),
        ...(nan > 0 ? [note('Eval NaN', `${nan} vertices`)] : []),
        note('Eval', `${ms.toFixed(1)} ms`),
        note('amplitude', n2(amplitude)),
      ],
    };
  },
});

const slice = sceneOf({
  id: 'slice',
  title: 'A slice through a 3D field',
  description:
    'The 3D fields of noise.types.plato read on one plane: Eval takes a Point3D and the slider is its Z, ' +
    'so dragging it walks a solid field one plane at a time. The gradient bases drift smoothly because ' +
    'they interpolate in three dimensions; WhiteNoise3D does not, because its hash takes Z as an ' +
    'independent coordinate and every slice is uncorrelated with the last.',
  plato: [
    'PerlinNoise3D.Eval',
    'SimplexNoise3D.Eval',
    'ValueNoise3D.Eval',
    'WhiteNoise3D.Eval',
    'WorleyNoise3D.Eval',
    'FbmNoise3D.Eval',
    'ColorGradient.ColorAtParameter',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'field', label: 'Field', kind: 'select', options: FIELD_3D_LABELS, def: 0 },
    { key: 'ramp', label: 'Ramp', kind: 'select', options: RAMP_LABELS, def: 0 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 1, max: 16, step: 1, def: 5 },
    { key: 'octaves', label: 'Octaves', kind: 'slider', min: 1, max: 6, step: 1, def: 3 },
    { key: 'z', label: 'Slice Z', kind: 'slider', min: -2, max: 2, step: 0.01, def: 0 },
    { key: 'resolution', label: 'Resolution', kind: 'slider', min: 32, max: 160, step: 16, def: 96 },
  ],
  build(params: Params): Built {
    const field = field3D(
      params.field,
      Math.round(params.seed),
      params.frequency,
      Math.round(params.octaves),
    );
    const z = params.z;
    const resolution = Math.round(params.resolution);
    const sample = sampleField(resolution, (u, v) => field.noise.Eval(new Point3D(u, v, z)));
    const to01 = normalizer(sample, false);
    const ramp = rampLookup(rampTable(params.ramp));

    const object = new THREE.Group();
    object.add(
      rasterPlane((u, v) => ramp(to01(valueAt(sample, u, v))), { resolution, size: 1.9 }),
    );
    object.add(frame(-0.95, -0.95, 0.95, 0.95, 0x27303d));

    const centre = new Point3D(0.5, 0.5, z);
    return {
      object,
      readings: [
        note('field', `${field.name} · ${field.detail}`),
        note('slice Z', n2(z)),
        ...statReadings(sample),
        reading('Eval(0.5, 0.5, z)', () => n4(field.noise.Eval(centre))),
        reading('IsPositiveAt(0.5, 0.5, z)', () => String(field.noise.IsPositiveAt(centre))),
      ],
    };
  },
});

const isosurface = sceneOf({
  id: 'isosurface',
  title: 'An isosurface of a 3D field',
  description:
    'IScalarField3D.MarchingCubes samples the field on a lattice over a Bounds3D and hands back the ' +
    'TriangleArray3D of the surface where Eval equals IsoLevel — the same generated marching-cubes ' +
    'kernel the SDF and voxel pages use, given a noise field instead of a distance function. Cost is ' +
    'cubic in the node count and every cell corner is an Eval, so the lattice starts small and is on a ' +
    'slider. An iso level outside the field\'s range is crossed nowhere and the surface is legitimately ' +
    'empty: WhiteNoise3D and WorleyNoise3D return non-negative values, so level 0 finds nothing in ' +
    'them where the gradient bases straddle it. The status line probes the box first and reports the ' +
    'range it found, so an empty result reads as the level being outside it rather than as a failure.',
  plato: [
    'PerlinNoise3D.MarchingCubes',
    'SimplexNoise3D.MarchingCubes',
    'ValueNoise3D.MarchingCubes',
    'WorleyNoise3D.MarchingCubes',
    'FbmNoise3D.MarchingCubes',
    'Bounds3D.LatticeNodePosition',
    'IntegerVector3.MarchingCubesLattice',
  ],
  viewer: SPATIAL,
  controls: [
    { key: 'field', label: 'Field', kind: 'select', options: FIELD_3D_LABELS, def: 0 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 0.5, max: 6, step: 0.25, def: 2 },
    { key: 'octaves', label: 'Octaves', kind: 'slider', min: 1, max: 3, step: 1, def: 2 },
    { key: 'level', label: 'Iso level', kind: 'slider', min: -0.8, max: 0.8, step: 0.02, def: 0 },
    { key: 'nodes', label: 'Lattice nodes', kind: 'slider', min: 8, max: 32, step: 2, def: 20 },
  ],
  build(params: Params): Built {
    const field = field3D(
      params.field,
      Math.round(params.seed),
      params.frequency,
      Math.round(params.octaves),
    );
    const nodes = Math.round(params.nodes);
    const bounds = new Bounds3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
    const counts = new IntegerVector3(nodes, nodes, nodes);

    // A coarse pass over the same box, so an empty surface can say whether the
    // iso level was inside the field's range at all. 10³ Eval calls, against the
    // lattice's own node count cubed times its cell corners.
    const probeSteps = 10;
    let probeMin = Infinity;
    let probeMax = -Infinity;
    for (let k = 0; k < probeSteps; k++) {
      for (let j = 0; j < probeSteps; j++) {
        for (let i = 0; i < probeSteps; i++) {
          const at = (n: number): number => -1 + (2 * n) / (probeSteps - 1);
          const value = field.noise.Eval(new Point3D(at(i), at(j), at(k)));
          if (!Number.isFinite(value)) continue;
          if (value < probeMin) probeMin = value;
          if (value > probeMax) probeMax = value;
        }
      }
    }
    const inRange = params.level > probeMin && params.level < probeMax;

    const object = new THREE.Group();
    let triangles = 0;
    let ms = 0;
    const marched = reading(`${field.name}.MarchingCubes`, () => {
      const started = performance.now();
      const result = field.noise.MarchingCubes(bounds, counts, params.level);
      triangles = result.Triangles.Count();
      ms = performance.now() - started;
      if (triangles > 0) {
        object.add(
          new THREE.Mesh(
            triangleArrayGeometry(result),
            new THREE.MeshStandardMaterial({
              color: palette.surface,
              roughness: 0.44,
              metalness: 0.06,
              flatShading: true,
              side: THREE.DoubleSide,
            }),
          ),
        );
      }
      return `${triangles} triangles in ${ms.toFixed(1)} ms`;
    });

    // The sampled box, so an empty result still reads as a scene.
    object.add(
      segments(
        [
          -1, -1, -1, 1, -1, -1, 1, -1, -1, 1, -1, 1, 1, -1, 1, -1, -1, 1, -1, -1, 1, -1, -1, -1,
          -1, 1, -1, 1, 1, -1, 1, 1, -1, 1, 1, 1, 1, 1, 1, -1, 1, 1, -1, 1, 1, -1, 1, -1,
          -1, -1, -1, -1, 1, -1, 1, -1, -1, 1, 1, -1, 1, -1, 1, 1, 1, 1, -1, -1, 1, -1, 1, 1,
        ],
        0x27303d,
      ),
    );

    const out: Reading[] = [
      note('field', `${field.name} · ${field.detail}`),
      note('lattice', `${nodes}x${nodes}x${nodes} nodes`),
      note('iso level', n2(params.level)),
      note('Eval over the box', `${n2(probeMin)} .. ${n2(probeMax)} (${probeSteps ** 3} samples)`),
      marched,
      note('triangles', String(triangles)),
    ];
    if (triangles === 0) {
      out.push(
        note(
          'empty surface',
          inRange
            ? 'the level is inside the range but the lattice is too coarse to catch a crossing'
            : `no crossing — the level is outside ${n2(probeMin)} .. ${n2(probeMax)}`,
        ),
      );
    }
    out.push(reading('Eval at the centre', () => n4(field.noise.Eval(new Point3D(0, 0, 0)))));
    return { object, readings: out };
  },
});

const FLOW_LABELS = ['Arrows', 'Streamlines', 'Both'];

const curlFlow = sceneOf({
  id: 'curl',
  title: 'Curl noise as a flow field',
  description:
    'CurlNoise2D.Eval returns a Vector2D, not a scalar: it takes the perpendicular gradient of a Perlin ' +
    'potential by central differences, which makes the field divergence-free — the arrows never converge ' +
    'on a source or a sink, and a streamline that follows FlowDirectionAt closes or leaves rather than ' +
    'piling up. Arrow length and colour come from MagnitudeAt; the streamlines step along ' +
    'FlowDirectionAt, which is the generated Normalize of the same vector.',
  plato: [
    'CurlNoise2D.Eval',
    'CurlNoise2D.MagnitudeAt',
    'CurlNoise2D.FlowDirectionAt',
    'CurlNoise2D.CurlPotential',
    'PerlinNoise2D.Eval',
    'ColorGradient.ColorAtParameter',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'mode', label: 'Draw', kind: 'select', options: FLOW_LABELS, def: 2 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 1, max: 12, step: 1, def: 4 },
    { key: 'grid', label: 'Arrow grid', kind: 'slider', min: 6, max: 32, step: 1, def: 18 },
    { key: 'seeds', label: 'Streamlines', kind: 'slider', min: 8, max: 160, step: 8, def: 72 },
    { key: 'steps', label: 'Steps', kind: 'slider', min: 8, max: 96, step: 4, def: 40 },
    { key: 'step', label: 'Step size', kind: 'slider', min: 0.002, max: 0.03, step: 0.001, def: 0.012 },
    { key: 'potential', label: 'Show potential', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const seed = Math.round(params.seed);
    const noise = new CurlNoise2D(seed, params.frequency);
    const mode = clampIndex(params.mode, FLOW_LABELS.length);
    const object = new THREE.Group();

    // The potential the curl is taken of, as a backdrop. Same member the
    // generated CurlPotential reads.
    let potential: FieldSample | null = null;
    if (params.potential > 0.5) {
      const resolution = 96;
      const sample = sampleField(resolution, (u, v) => noise.CurlPotential(new Point2D(u, v)));
      potential = sample;
      const to01 = normalizer(sample, false);
      const ramp = rampLookup(rampTable(0));
      const plate = rasterPlane(
        (u, v) => {
          const c = ramp(to01(valueAt(sample, u, v)));
          return { r: c.r * 0.4, g: c.g * 0.42, b: c.b * 0.5 };
        },
        { resolution, size: 1.9 },
      );
      plate.position.z = -0.05;
      object.add(plate);
    }

    const toWorld = (u: number, v: number): [number, number] => [u * 1.9 - 0.95, v * 1.9 - 0.95];

    let minMag = Infinity;
    let maxMag = -Infinity;
    let sumMag = 0;
    let magCount = 0;
    let nan = 0;

    if (mode === 0 || mode === 2) {
      const grid = Math.round(params.grid);
      const scale = 1.9 / grid;
      // Two passes: MagnitudeAt has no fixed range — it scales with Frequency,
      // because the potential's gradient does — so the arrows are drawn against
      // the largest magnitude actually measured rather than a guessed ceiling.
      const cells: { u: number; v: number; magnitude: number }[] = [];
      for (let j = 0; j < grid; j++) {
        for (let i = 0; i < grid; i++) {
          const u = (i + 0.5) / grid;
          const v = (j + 0.5) / grid;
          const p = new Point2D(u, v);
          const magnitude = noise.MagnitudeAt(p);
          if (!Number.isFinite(magnitude)) {
            nan++;
            continue;
          }
          if (magnitude < minMag) minMag = magnitude;
          if (magnitude > maxMag) maxMag = magnitude;
          sumMag += magnitude;
          magCount++;
          cells.push({ u, v, magnitude });
        }
      }
      const arrows: number[] = [];
      for (const cell of cells) {
        const p = new Point2D(cell.u, cell.v);
        const direction: Vector2D = noise.FlowDirectionAt(p);
        if (!Number.isFinite(direction.X) || !Number.isFinite(direction.Y)) {
          nan++;
          continue;
        }
        const [x, y] = toWorld(cell.u, cell.v);
        const len = (maxMag > 0 ? cell.magnitude / maxMag : 0) * scale * 0.95;
        const dx = direction.X * len;
        const dy = direction.Y * len;
        arrows.push(x - dx * 0.5, y - dy * 0.5, 0.01, x + dx * 0.5, y + dy * 0.5, 0.01);
        // A short barb at the head, so direction reads at a glance.
        arrows.push(
          x + dx * 0.5,
          y + dy * 0.5,
          0.01,
          x + dx * 0.5 - dx * 0.32 + dy * 0.2,
          y + dy * 0.5 - dy * 0.32 - dx * 0.2,
          0.01,
        );
      }
      object.add(segments(arrows, palette.line));
    }

    if (mode === 1 || mode === 2) {
      const count = Math.round(params.seeds);
      const steps = Math.round(params.steps);
      const step = params.step;
      // Seed positions are jittered by a generated WhiteNoise2D rather than by
      // Math.random, so a rebuild at the same parameters draws the same picture.
      const jitter = new WhiteNoise2D(seed + 991);
      const side = Math.max(1, Math.round(Math.sqrt(count)));
      const paths: number[] = [];
      for (let k = 0; k < count; k++) {
        const gx = (k % side) + 0.5;
        const gy = Math.floor(k / side) + 0.5;
        const ju = jitter.Eval(new Point2D(gx * 0.31, gy * 0.57)) - 0.5;
        const jv = jitter.Eval(new Point2D(gy * 0.19, gx * 0.83)) - 0.5;
        let u = (gx + ju * 0.9) / side;
        let v = (gy + jv * 0.9) / Math.ceil(count / side);
        for (let s = 0; s < steps; s++) {
          const p = new Point2D(u, v);
          const direction = noise.FlowDirectionAt(p);
          if (!Number.isFinite(direction.X) || !Number.isFinite(direction.Y)) {
            nan++;
            break;
          }
          const nu = u + direction.X * step;
          const nv = v + direction.Y * step;
          if (nu < 0 || nu > 1 || nv < 0 || nv > 1) break;
          const [x0, y0] = toWorld(u, v);
          const [x1, y1] = toWorld(nu, nv);
          paths.push(x0, y0, 0.02, x1, y1, 0.02);
          u = nu;
          v = nv;
        }
      }
      object.add(segments(paths, palette.accent));
    }

    object.add(frame(-0.95, -0.95, 0.95, 0.95, 0x27303d));

    const centre = new Point2D(0.5, 0.5);
    const out: Reading[] = [
      note('field', `CurlNoise2D · Seed ${seed} Frequency ${n2(params.frequency)}`),
      note('draw', FLOW_LABELS[mode]),
      reading('Eval(0.5, 0.5)', () => {
        const vector: Vector2D = noise.Eval(centre);
        return `(${n4(vector.X)}, ${n4(vector.Y)})`;
      }),
      reading('MagnitudeAt(0.5, 0.5)', () => n4(noise.MagnitudeAt(centre))),
      reading('FlowDirectionAt(0.5, 0.5)', () => {
        const d = noise.FlowDirectionAt(centre);
        return `(${n4(d.X)}, ${n4(d.Y)})`;
      }),
      reading('CurlStep', () => n4(noise.CurlStep())),
    ];
    if (magCount > 0) {
      out.push(
        note('MagnitudeAt min', n4(minMag)),
        note('MagnitudeAt max', n4(maxMag)),
        note('MagnitudeAt mean', n4(sumMag / magCount)),
      );
    }
    if (potential) {
      out.push(note('CurlPotential range', `${n2(potential.min)} .. ${n2(potential.max)}`));
    }
    if (nan > 0) out.push(note('non-finite samples', String(nan)));
    return { object, readings: out };
  },
});

const curlFlow3D = sceneOf({
  id: 'curl3d',
  title: 'Curl noise in space',
  description:
    'CurlNoise3D.Eval is the curl of a three-component Perlin potential — three PerlinNoise3D fields at ' +
    'decorrelated seeds, differenced along each axis — so the result is a divergence-free Vector3D and ' +
    'the streamlines below never end inside the box. Each is integrated by stepping along ' +
    'FlowDirectionAt; the colour is MagnitudeAt at the start of the line. Every step costs twelve ' +
    'PerlinNoise3D evaluations, so the line and step counts are both on sliders.',
  plato: [
    'CurlNoise3D.Eval',
    'CurlNoise3D.MagnitudeAt',
    'CurlNoise3D.FlowDirectionAt',
    'CurlNoise3D.CurlDerivative',
    'CurlNoise3D.CurlPotential',
    'PerlinNoise3D.Eval',
  ],
  viewer: SPATIAL,
  controls: [
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'frequency', label: 'Frequency', kind: 'slider', min: 0.5, max: 6, step: 0.25, def: 1.5 },
    { key: 'lines', label: 'Streamlines', kind: 'slider', min: 8, max: 96, step: 4, def: 40 },
    { key: 'steps', label: 'Steps', kind: 'slider', min: 8, max: 80, step: 4, def: 44 },
    { key: 'step', label: 'Step size', kind: 'slider', min: 0.005, max: 0.06, step: 0.005, def: 0.03 },
  ],
  build(params: Params): Built {
    const seed = Math.round(params.seed);
    const noise = new CurlNoise3D(seed, params.frequency);
    const lines = Math.round(params.lines);
    const steps = Math.round(params.steps);
    const step = params.step;

    // Seeds scattered by a generated WhiteNoise3D, so `build` stays pure.
    const scatter = new WhiteNoise3D(seed + 619);
    const object = new THREE.Group();
    const ramp = rampLookup(rampTable(3));

    let minMag = Infinity;
    let maxMag = -Infinity;
    let sumMag = 0;
    let magCount = 0;
    let nan = 0;
    let segmentCount = 0;
    const started = performance.now();

    const drawn: { path: Point3D[]; magnitude: number }[] = [];
    for (let k = 0; k < lines; k++) {
      const sx = scatter.Eval(new Point3D(k * 0.37, 0.11, 0.53)) * 2 - 1;
      const sy = scatter.Eval(new Point3D(0.29, k * 0.41, 0.77)) * 2 - 1;
      const sz = scatter.Eval(new Point3D(0.61, 0.83, k * 0.47)) * 2 - 1;
      let p = new Point3D(sx * 0.9, sy * 0.9, sz * 0.9);

      const magnitude = noise.MagnitudeAt(p);
      if (Number.isFinite(magnitude)) {
        if (magnitude < minMag) minMag = magnitude;
        if (magnitude > maxMag) maxMag = magnitude;
        sumMag += magnitude;
        magCount++;
      } else {
        nan++;
      }

      const path: Point3D[] = [p];
      for (let s = 0; s < steps; s++) {
        const direction: Vector3D = noise.FlowDirectionAt(p);
        if (
          !Number.isFinite(direction.X) ||
          !Number.isFinite(direction.Y) ||
          !Number.isFinite(direction.Z)
        ) {
          nan++;
          break;
        }
        const next = new Point3D(
          p.X + direction.X * step,
          p.Y + direction.Y * step,
          p.Z + direction.Z * step,
        );
        if (Math.abs(next.X) > 1 || Math.abs(next.Y) > 1 || Math.abs(next.Z) > 1) break;
        path.push(next);
        p = next;
      }
      if (path.length > 1) {
        segmentCount += path.length - 1;
        drawn.push({ path, magnitude });
      }
    }
    const ms = performance.now() - started;

    // MagnitudeAt scales with Frequency, so the ramp is spread over the
    // magnitudes this build actually measured rather than a guessed ceiling.
    for (const { path, magnitude } of drawn) {
      const t =
        Number.isFinite(magnitude) && maxMag > minMag
          ? (magnitude - minMag) / (maxMag - minMag)
          : 0.5;
      const c = ramp(0.25 + t * 0.7);
      object.add(line3D(path, new THREE.Color(c.r, c.g, c.b).getHex()));
    }

    object.add(
      segments(
        [
          -1, -1, -1, 1, -1, -1, 1, -1, -1, 1, -1, 1, 1, -1, 1, -1, -1, 1, -1, -1, 1, -1, -1, -1,
          -1, 1, -1, 1, 1, -1, 1, 1, -1, 1, 1, 1, 1, 1, 1, -1, 1, 1, -1, 1, 1, -1, 1, -1,
          -1, -1, -1, -1, 1, -1, 1, -1, -1, 1, 1, -1, 1, -1, 1, 1, 1, 1, -1, -1, 1, -1, 1, 1,
        ],
        0x27303d,
      ),
    );

    const centre = new Point3D(0, 0, 0);
    const out: Reading[] = [
      note('field', `CurlNoise3D · Seed ${seed} Frequency ${n2(params.frequency)}`),
      note('streamlines', `${lines} seeds, ${segmentCount} segments`),
      note('integration', `${ms.toFixed(1)} ms`),
      reading('Eval(0, 0, 0)', () => {
        const vector = noise.Eval(centre);
        return `(${n4(vector.X)}, ${n4(vector.Y)}, ${n4(vector.Z)})`;
      }),
      reading('MagnitudeAt(0, 0, 0)', () => n4(noise.MagnitudeAt(centre))),
      reading('CurlDerivative(0, 0, 0; c 0, axis 1)', () => n4(noise.CurlDerivative(centre, 0, 1))),
      reading('CurlPotential(0, 0, 0; c 0)', () => n4(noise.CurlPotential(centre, 0))),
    ];
    if (magCount > 0) {
      out.push(
        note('MagnitudeAt min', n4(minMag)),
        note('MagnitudeAt max', n4(maxMag)),
        note('MagnitudeAt mean', n4(sumMag / magCount)),
      );
    }
    if (nan > 0) out.push(note('non-finite samples', String(nan)));
    return { object, readings: out };
  },
});

const demo: Demo = {
  title: 'Noise',
  subtitle: 'noise.{types,library}.plato',
  scenes: [
    basisGallery,
    fractalSums,
    octaveBuildup,
    domainWarp,
    readings,
    worley,
    heightField,
    slice,
    isosurface,
    curlFlow,
    curlFlow3D,
  ],
};

// The raster scenes want the flat, grid-free camera; the height field, the
// isosurface and the 3D flow override it with the orbit camera.
mountDemo(demo, PLANAR);

export { demo };
