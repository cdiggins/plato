// Sampling — a scene catalog over stdlib/geometry/sampling.{types,library}.plato.
//
// The subject is blue noise, and the point of the page is that "blue" is a
// MEASUREMENT rather than an adjective. Every point on every scene comes out of a
// generated pattern generator, and every number in every status line comes out of
// one of the library's own quality readings — `RelativeRadius`,
// `RelativeMeanSpacing`, `StarDiscrepancyEstimate`, `NearestNeighborDistance`,
// `BoxDiscrepancy`, `WavePower`, `RadialPower` and `RadialPowerSpectrum`. Laying
// tiles out, binning distances into a histogram and packing dots into a buffer is
// demo work; a radical inverse, a dart, a repulsion pass and a periodogram are
// not, and none of them is written here.
//
// Three readings, because each is fooled by something the others catch, and the
// page is arranged so the disagreements are visible:
//
//   * `RelativeRadius` is the single "how blue" number, and the regular grid wins
//     it (0.93) with maximal Poisson-disk just behind.
//   * `StarDiscrepancyEstimate` is the honest twist: LOWER is better and the
//     low-discrepancy families win it, so Halton and Hammersley beat blue noise
//     on the reading while looking obviously worse on the page.
//   * `RadialPowerSpectrum` is the only one that separates "even" from "regular",
//     and it is the reading the word blue actually names. It is the money plot.
//
// Almost every scene is planar and takes the page's orthographic, grid-free,
// non-spinning camera; the scatter through a volume overrides it with the orbit
// camera.
//
// COST. The readings are O(n^2), and Poisson-disk and blue-noise output arrives
// as an `Append` chain whose element reads are O(depth), so every point set is
// materialised through `fromArray(toArray(...))` before a reading touches it —
// repacking, not recomputing — and the counts stay in the low hundreds. The
// closed-form families are O(1) per point and would be happy at thousands; they
// are held to the same count so the comparisons are of the patterns and not of
// their sizes.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { fromArray, toArray } from '../shared/mesh.js';
import { rasterPlane, type Rgb } from '../shared/raster.js';
import { palette, type ViewerOptions } from '../shared/viewer.js';
import {
  Bounds2D,
  Bounds3D,
  Color,
  ColorGradient,
  ColorStop,
  IntegerVector2,
  IntegerVector3,
  Point2D,
  Point3D,
  type IArray,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// Same pattern as `src/demos/polygons.ts` and `src/demos/noise.ts`: a member that
// throws or returns NaN keeps its name in the status line and says so, instead of
// being quietly replaced by an answer this file computed some other way.

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
const n3 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(3);
const n2 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(2);

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
 * The shell's orthographic camera is sized by height alone, so a wide chart would
 * be clipped sideways. Shrink to whichever half-extent is smaller, refreshed per
 * frame because the stage is resizable. A no-op under the perspective camera the
 * scatter through a volume asks for.
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

function clampIndex(value: number, count: number): number {
  const i = Math.round(value);
  return i < 0 ? 0 : i >= count ? count - 1 : i;
}

// ---------------------------------------------------------------------------
// The region every planar pattern is generated over
//
// One unit square, so `ReferenceSpacing`, `BlueNoiseRadius` and the spectrum's
// band frequencies are all in the same units and the numbers on screen can be
// compared with each other without a scale factor in the way. Points are carried
// to their drawing positions by `place`, which is the only mapping this file
// performs — the pattern generators themselves put the points in the region
// through `FromUnitSquare`, which is the library's own bridge.

const UNIT = new Bounds2D(new Point2D(0, 0), new Point2D(1, 1));

/** An `IArray` materialised into a plain-array one: repacking, not recomputing.
 *  The Poisson and blue-noise generators build with `Append`, whose chain costs
 *  O(depth) per element read, and every quality reading is O(n^2) reads. */
function solid(points: IArray<Point2D>): IArray<Point2D> {
  return fromArray(toArray(points));
}

// ---------------------------------------------------------------------------
// The families
//
// Ten strategies over one region, so every scene that compares them is comparing
// the same list. `PatternPoints2D` is the library's own dispatcher over the
// `SamplePattern` sum type, and it is C#-only in v1 (CHK320) — no sum type
// reaches the TypeScript target — so the switch lives here over the concrete
// generators instead. That is a writer gap and not a design choice; see the
// status line of the gallery scene, which reports it by name.

interface Family {
  /** Short enough for a segmented control cell. */
  name: string;
  /** The generated call, written the way a caller would write it. */
  member: string;
  /** One line on what the strategy is, for the status line. */
  detail: string;
  /** True when `count` is an input the family honours exactly. */
  exact: boolean;
  points(count: number, seed: number): IArray<Point2D>;
}

const FAMILIES: Family[] = [
  {
    name: 'Uniform',
    member: 'Bounds2D.StratifiedPoints2D(1x1 strata, count, seed)',
    detail:
      'one stratum holding every sample, so each point is an independent uniform draw — ' +
      'the white-noise baseline the others are judged against',
    exact: true,
    points: (count, seed) => UNIT.StratifiedPoints2D(new IntegerVector2(1, 1), count, seed),
  },
  {
    name: 'Grid',
    member: 'Bounds2D.JitteredGridPoints2D(PatternGrid(count), 0, seed)',
    detail: 'jitter zero, which is exactly the regular lattice',
    exact: false,
    points: (count, seed) => UNIT.JitteredGridPoints2D(UNIT.PatternGrid(count), 0, seed),
  },
  {
    name: 'Jittered',
    member: 'Bounds2D.JitteredGridPoints2D(PatternGrid(count), 1, seed)',
    detail: 'jitter one, so the point is uniform anywhere in its own cell',
    exact: false,
    points: (count, seed) => UNIT.JitteredGridPoints2D(UNIT.PatternGrid(count), 1, seed),
  },
  {
    name: 'Strata',
    member: 'Bounds2D.StratifiedPoints2D(PatternGrid(count), 1, seed)',
    detail: 'one uniform sample per stratum, drawn from two independent hash streams',
    exact: false,
    points: (count, seed) => UNIT.StratifiedPoints2D(UNIT.PatternGrid(count), 1, seed),
  },
  {
    name: 'Halton',
    member: 'Bounds2D.HaltonPoints2D(count, 2, 3)',
    detail: 'radical inverses in the coprime bases 2 and 3, from index one',
    exact: true,
    points: count => UNIT.HaltonPoints2D(count, 2, 3),
  },
  {
    name: 'Hammer',
    member: 'Bounds2D.HammersleyPoints2D(count, 2)',
    detail: 'i / count along X and the base-2 radical inverse along Y',
    exact: true,
    points: count => UNIT.HammersleyPoints2D(count, 2),
  },
  {
    name: 'Sobol',
    member: 'Bounds2D.SobolPoints2D(count, 1)',
    detail: 'the base-2 radical inverse against a Gray-code direction sum, skipping the origin',
    exact: true,
    points: count => UNIT.SobolPoints2D(count, 1),
  },
  {
    name: 'Plastic',
    member: 'Bounds2D.PlasticPoints2D(count, 0.5)',
    detail: 'the R2 additive recurrence on the plastic number, offset off the corner',
    exact: true,
    points: count => UNIT.PlasticPoints2D(count, 0.5),
  },
  {
    name: 'Poisson',
    member: 'Bounds2D.PoissonDiskPoints2D(BlueNoiseRadius(count), 30, seed)',
    detail:
      'maximal dart throwing over the background grid — the count is an OUTCOME of the radius, ' +
      'so it overshoots the asked-for count by design',
    exact: false,
    points: (count, seed) => UNIT.PoissonDiskPoints2D(UNIT.BlueNoiseRadius(count), 30, seed),
  },
  {
    name: 'Blue',
    member: 'Bounds2D.BlueNoisePoints2D(count, seed)',
    detail:
      'the same Poisson-disk set at the radius that saturates a fifth above the count, ' +
      'thinned evenly back to exactly the count',
    exact: true,
    points: (count, seed) => UNIT.BlueNoisePoints2D(count, seed),
  },
];

const FAMILY_LABELS = FAMILIES.map(f => f.name);

/** The family at a select's index, with its points already materialised. */
function familyPoints(index: number, count: number, seed: number): { family: Family; points: IArray<Point2D>; ms: number } {
  const family = FAMILIES[clampIndex(index, FAMILIES.length)];
  const started = performance.now();
  const points = solid(family.points(count, seed));
  return { family, points, ms: performance.now() - started };
}

// ---------------------------------------------------------------------------
// Colour
//
// Presentation only on this page, so the ramps are thin — but they are still
// `ColorGradient`s read through the generated `ColorAtParameter`, sampled once
// per build into a small table rather than per point.

const opaque = (r: number, g: number, b: number): Color => new Color(r, g, b, 1);

const RAMPS: Record<string, ColorGradient> = {
  // Index order, for the progressive-fill scene.
  sequence: new ColorGradient(
    fromArray([
      new ColorStop(0, opaque(0.16, 0.24, 0.5)),
      new ColorStop(0.45, opaque(0.24, 0.72, 0.78)),
      new ColorStop(0.75, opaque(0.86, 0.79, 0.32)),
      new ColorStop(1, opaque(0.95, 0.42, 0.32)),
    ]),
  ),
  // Power, for the periodogram: dark below one, bright above it.
  power: new ColorGradient(
    fromArray([
      new ColorStop(0, opaque(0.03, 0.04, 0.08)),
      new ColorStop(0.28, opaque(0.09, 0.22, 0.42)),
      new ColorStop(0.55, opaque(0.2, 0.58, 0.62)),
      new ColorStop(0.78, opaque(0.87, 0.78, 0.36)),
      new ColorStop(1, opaque(1, 0.96, 0.9)),
    ]),
  ),
  // Nearest-neighbour distance: close pairs read hot.
  spacing: new ColorGradient(
    fromArray([
      new ColorStop(0, opaque(0.94, 0.28, 0.24)),
      new ColorStop(0.5, opaque(0.88, 0.74, 0.3)),
      new ColorStop(1, opaque(0.36, 0.78, 0.62)),
    ]),
  ),
};

const RAMP_STEPS = 128;

function rampLookup(name: keyof typeof RAMPS): (t: number) => Rgb {
  const gradient = RAMPS[name];
  const table: Rgb[] = [];
  for (let i = 0; i < RAMP_STEPS; i++) {
    const c = gradient.ColorAtParameter(i / (RAMP_STEPS - 1));
    table.push({ r: c.R, g: c.G, b: c.B });
  }
  return (t: number): Rgb => {
    if (!Number.isFinite(t)) return { r: 0.85, g: 0.1, b: 0.65 };
    const i = Math.round(t * (RAMP_STEPS - 1));
    return table[i < 0 ? 0 : i >= RAMP_STEPS ? RAMP_STEPS - 1 : i];
  };
}

const INK = 0x27303d;
const INK_DIM = 0x1b2430;
const SERIES = [palette.line, palette.surfaceAlt, palette.accent];

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

function frame(x0: number, y0: number, x1: number, y1: number, color: number, z = 0.02): THREE.LineSegments {
  return segments(
    [x0, y0, z, x1, y0, z, x1, y0, z, x1, y1, z, x1, y1, z, x0, y1, z, x0, y1, z, x0, y0, z],
    color,
  );
}

/** A run of (x, y) samples as one open polyline. */
function polyline(xs: number[], ys: number[], color: number, z = 0.03): THREE.LineSegments {
  const out: number[] = [];
  for (let i = 0; i + 1 < xs.length; i++) out.push(xs[i], ys[i], z, xs[i + 1], ys[i + 1], z);
  return segments(out, color);
}

/** Axis-aligned rectangles as one flat mesh — histogram bars, cell shading. */
function quads(
  rects: { x0: number; y0: number; x1: number; y1: number }[],
  color: number,
  opacity: number,
  z = 0,
): THREE.Mesh {
  const positions: number[] = [];
  for (const r of rects) {
    positions.push(r.x0, r.y0, z, r.x1, r.y0, z, r.x1, r.y1, z);
    positions.push(r.x0, r.y0, z, r.x1, r.y1, z, r.x0, r.y1, z);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Mesh(
    geometry,
    new THREE.MeshBasicMaterial({
      color,
      transparent: opacity < 1,
      opacity,
      side: THREE.DoubleSide,
      depthWrite: false,
    }),
  );
}

function dots2D(points: readonly { x: number; y: number }[], color: number, size: number, z = 0.04): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.x, p.y, z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(geometry, new THREE.PointsMaterial({ color, size, sizeAttenuation: false }));
}

function coloredDots(
  points: readonly { x: number; y: number; c: Rgb }[],
  size: number,
  z = 0.04,
): THREE.Points {
  const positions: number[] = [];
  const colors: number[] = [];
  for (const p of points) {
    positions.push(p.x, p.y, z);
    colors.push(p.c.r, p.c.g, p.c.b);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  geometry.setAttribute('color', new THREE.Float32BufferAttribute(colors, 3));
  return new THREE.Points(
    geometry,
    new THREE.PointsMaterial({ size, sizeAttenuation: false, vertexColors: true }),
  );
}

function dots3D(points: readonly Point3D[], color: number, size: number): THREE.Points {
  const positions: number[] = [];
  for (const p of points) positions.push(p.X, p.Y, p.Z);
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  return new THREE.Points(geometry, new THREE.PointsMaterial({ color, size, sizeAttenuation: false }));
}

/** A circle outline's coordinates, appended to `into` so many circles can share
 *  one geometry — an exclusion radius per point would otherwise be one draw call
 *  per point. */
function circleInto(into: number[], cx: number, cy: number, r: number, steps: number, z: number): number[] {
  for (let i = 0; i < steps; i++) {
    const a = (i / steps) * Math.PI * 2;
    const b = ((i + 1) / steps) * Math.PI * 2;
    into.push(cx + Math.cos(a) * r, cy + Math.sin(a) * r, z, cx + Math.cos(b) * r, cy + Math.sin(b) * r, z);
  }
  return into;
}

/** A circle outline, for an exclusion radius. */
function circle(cx: number, cy: number, r: number, color: number, steps = 40, z = 0.03, opacity = 1): THREE.LineSegments {
  return segments(circleInto([], cx, cy, r, steps, z), color, opacity);
}

interface LabelArt {
  texture: THREE.Texture;
  aspect: number;
  lines: number;
}

const labelArt = new Map<string, LabelArt>();

/** Billboard text, cached because the viewer disposes a sprite's material on
 *  every rebuild and a material never owns its map. Same helper as
 *  `src/demos/polyhedra.ts`. */
function label(text: string, lineHeight = 0.06, maxWidth = 1.9, tint = '#c8d4e6'): THREE.Sprite {
  const cacheKey = `${tint}|${text}`;
  let art = labelArt.get(cacheKey);
  if (!art) {
    const lines = text.split('\n');
    const size = 44;
    const pad = 14;
    const face = `600 ${size}px ui-sans-serif, system-ui, -apple-system, sans-serif`;
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d')!;
    ctx.font = face;
    const width = Math.ceil(Math.max(...lines.map(l => ctx.measureText(l).width))) + pad * 2;
    const height = Math.ceil(lines.length * size * 1.3) + pad * 2;
    canvas.width = width;
    canvas.height = height;
    ctx.font = face;
    ctx.textBaseline = 'top';
    ctx.lineJoin = 'round';
    ctx.lineWidth = 7;
    ctx.strokeStyle = 'rgba(8, 10, 14, 0.92)';
    ctx.fillStyle = tint;
    lines.forEach((line, i) => {
      const y = pad + i * size * 1.3;
      ctx.strokeText(line, pad, y);
      ctx.fillText(line, pad, y);
    });
    const texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    art = { texture, aspect: width / Math.max(1, height), lines: lines.length };
    labelArt.set(cacheKey, art);
  }
  const sprite = new THREE.Sprite(
    new THREE.SpriteMaterial({ map: art.texture, transparent: true, depthTest: false, depthWrite: false }),
  );
  let h = lineHeight * art.lines;
  let w = h * art.aspect;
  if (w > maxWidth) {
    h *= maxWidth / w;
    w = maxWidth;
  }
  sprite.scale.set(w, h, 1);
  return sprite;
}

function placedLabel(text: string, x: number, y: number, lineHeight = 0.06, maxWidth = 1.9, tint?: string): THREE.Sprite {
  const sprite = label(text, lineHeight, maxWidth, tint);
  sprite.position.set(x, y, 0.06);
  return sprite;
}

/** A unit-region point at its drawing position inside a tile. */
function place(p: Point2D, cx: number, cy: number, size: number): { x: number; y: number } {
  return { x: cx + (p.X - 0.5) * size, y: cy + (p.Y - 0.5) * size };
}

function placeAll(points: IArray<Point2D>, cx: number, cy: number, size: number): { x: number; y: number }[] {
  const out: { x: number; y: number }[] = [];
  for (let i = 0; i < points.Count(); i++) out.push(place(points.At(i), cx, cy, size));
  return out;
}

/** Every point's `NearestNeighborDistance`, the per-point reading behind the
 *  histogram, the spacing colouring and (through the library) `RelativeRadius`. */
function nearestDistances(points: IArray<Point2D>): number[] {
  const out: number[] = [];
  for (let i = 0; i < points.Count(); i++) out.push(i.NearestNeighborDistance(points));
  return out;
}

/** `RelativeRadius` for a ranking, where a throw has to become a sortable value
 *  rather than a status line. A failure would still show up in the per-family
 *  `reading()` beside it. */
function safeRelativeRadius(points: IArray<Point2D>): number {
  try {
    return UNIT.RelativeRadius(points);
  } catch {
    return NaN;
  }
}

const PLANAR: ViewerOptions = { orthographic: true, grid: false, spin: false };
const SPATIAL: ViewerOptions = { orthographic: false, grid: false, spin: false, distance: 5.8 };

// ---------------------------------------------------------------------------
// Scene 1 — every family at one count

const GALLERY_ROWS = [4, 3, 3];

const gallery = sceneOf({
  id: 'families',
  title: 'Ten families at one count',
  description:
    'Every scattering strategy sampling.library.plato implements, over the same unit square at the same ' +
    'count and the same seed. The number under each tile is RelativeRadius — the smallest distance ' +
    'between any two of its points as a fraction of the spacing a perfect hexagonal packing would have ' +
    '— which is the library\'s single "how blue is it" reading. A regular grid reads about 0.93 and ' +
    'independent uniform draws read near zero, because two independent draws can land arbitrarily close ' +
    'and one pair doing so is enough. Poisson-disk takes a RADIUS and reports whatever count saturates, ' +
    'so its tile carries more points than the others by design; BlueNoisePoints2D is that same set at ' +
    'the radius which saturates a fifth above the count, thinned evenly back to exactly the count.',
  plato: [
    'Bounds2D.StratifiedPoints2D',
    'Bounds2D.JitteredGridPoints2D',
    'Bounds2D.HaltonPoints2D',
    'Bounds2D.HammersleyPoints2D',
    'Bounds2D.SobolPoints2D',
    'Bounds2D.PlasticPoints2D',
    'Bounds2D.PoissonDiskPoints2D',
    'Bounds2D.BlueNoisePoints2D',
    'Bounds2D.BlueNoiseRadius',
    'Bounds2D.PatternGrid',
    'Bounds2D.RelativeRadius',
    'Bounds2D.ReferenceSpacing',
    'Number.NearestNeighborDistance',
    'Number.SampleUnit',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'count', label: 'Count', kind: 'slider', min: 40, max: 320, step: 20, def: 160 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'size', label: 'Dot size', kind: 'slider', min: 1, max: 6, step: 0.5, def: 2.5 },
    { key: 'spacing', label: 'Colour by spacing', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.count);
    const seed = Math.round(params.seed);
    const bySpacing = params.spacing > 0.5;
    const ramp = rampLookup('spacing');

    const object = new THREE.Group();
    const readings: Reading[] = [
      note('count asked for', String(count)),
      note('seed', String(seed)),
      note('region', 'the unit square'),
    ];

    const tile = 0.5;
    const gap = 0.075;
    const rowHeight = tile + gap + 0.075;
    let index = 0;
    let ms = 0;
    for (let r = 0; r < GALLERY_ROWS.length; r++) {
      const columns = GALLERY_ROWS[r];
      const y = ((GALLERY_ROWS.length - 1) / 2 - r) * rowHeight + 0.04;
      for (let c = 0; c < columns; c++) {
        const family = FAMILIES[index];
        const x = (c - (columns - 1) / 2) * (tile + gap);
        const built = familyPoints(index, count, seed);
        ms += built.ms;
        const world = placeAll(built.points, x, y, tile);

        if (bySpacing && built.points.Count() > 1) {
          // The reference the library divides by, so the colour means the same
          // thing on every tile whatever its count came out at.
          const reference = UNIT.ReferenceSpacing(built.points.Count());
          const distances = nearestDistances(built.points);
          object.add(
            coloredDots(
              world.map((p, i) => ({
                ...p,
                c: ramp(reference > 0 ? Math.min(1, distances[i] / reference) : 0.5),
              })),
              params.size,
            ),
          );
        } else {
          object.add(dots2D(world, palette.line, params.size));
        }
        object.add(frame(x - tile / 2, y - tile / 2, x + tile / 2, y + tile / 2, INK));

        const relative = reading(`${family.name} RelativeRadius`, () =>
          n3(UNIT.RelativeRadius(built.points)),
        );
        object.add(
          placedLabel(
            `${family.name}  ${built.points.Count()} pts  r ${relative.value.startsWith('UNAVAILABLE') ? '—' : relative.value}`,
            x,
            y - tile / 2 - 0.045,
            0.042,
            tile + gap,
          ),
        );
        readings.push(
          note(
            family.name,
            `${built.points.Count()} pts${family.exact ? '' : ' (count is an outcome)'} · RelativeRadius ${relative.value}`,
          ),
        );
        index++;
      }
    }

    readings.push(
      note('generation', `${ms.toFixed(1)} ms for all ten`),
      // The whole random source behind every seeded family above: no state, no
      // stream to advance, just a hash of (seed, what for, which draw).
      reading('SampleUnit(seed, 0, 0..2)', () =>
        [0, 1, 2].map(stream => n4(seed.SampleUnit(0, stream))).join(', '),
      ),
      note(
        'PatternPoints2D',
        'UNAVAILABLE (SamplePattern is a sum type, and sum types are C#-only in v1 — CHK320) — ' +
          'the strategy switch above is over the concrete generators instead',
      ),
    );
    return { object, readings };
  },
});

// ---------------------------------------------------------------------------
// Scene 2 — the nearest-neighbour histogram

const spacingHistogram = sceneOf({
  id: 'spacing',
  title: 'The nearest-neighbour histogram',
  description:
    'NearestNeighborDistance mapped over every index of two point sets and binned, with the horizontal ' +
    'axis in units of ReferenceSpacing so the two are comparable whatever their counts came out at. ' +
    'This is the shape the word "blue" describes: independent uniform draws pile mass up against zero, ' +
    'because nothing stops two draws landing on top of each other, while a Poisson-disk set has NOTHING ' +
    'below its radius and a narrow bump just above it. The two dashed markers are ReferenceSpacing ' +
    'itself and BlueNoiseRadius — the radius the library picks to saturate at the asked-for count — and ' +
    'no blue-noise sample should fall left of the second one.',
  plato: [
    'Number.NearestNeighborDistance',
    'Bounds2D.ReferenceSpacing',
    'Bounds2D.RelativeRadius',
    'Bounds2D.RelativeMeanSpacing',
    'Bounds2D.BlueNoiseRadius',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'a', label: 'Set A', kind: 'select', options: FAMILY_LABELS, def: 0 },
    { key: 'b', label: 'Set B', kind: 'select', options: FAMILY_LABELS, def: 9 },
    { key: 'count', label: 'Count', kind: 'slider', min: 60, max: 400, step: 20, def: 200 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'bins', label: 'Bins', kind: 'slider', min: 8, max: 48, step: 2, def: 26 },
    { key: 'span', label: 'Axis span', kind: 'slider', min: 1, max: 3, step: 0.25, def: 2 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.count);
    const seed = Math.round(params.seed);
    const bins = Math.round(params.bins);
    const span = params.span;

    const x0 = -0.92;
    const x1 = 0.92;
    const y0 = -0.72;
    const y1 = 0.74;

    const object = new THREE.Group();
    const readings: Reading[] = [note('count asked for', String(count)), note('seed', String(seed))];

    interface Series {
      family: Family;
      counts: number[];
      distances: number[];
      points: IArray<Point2D>;
      reference: number;
    }
    const series: Series[] = [];
    let tallest = 1;
    for (const key of ['a', 'b'] as const) {
      const built = familyPoints(params[key], count, seed);
      const reference = UNIT.ReferenceSpacing(built.points.Count());
      const distances = nearestDistances(built.points);
      const counts = new Array<number>(bins).fill(0);
      for (const d of distances) {
        if (!Number.isFinite(d) || reference <= 0) continue;
        const t = d / reference / span;
        const bin = Math.min(bins - 1, Math.max(0, Math.floor(t * bins)));
        counts[bin]++;
      }
      for (const c of counts) if (c > tallest) tallest = c;
      series.push({ family: built.family, counts, distances, points: built.points, reference });
    }

    // The bars, drawn back to front so the second set reads over the first.
    series.forEach((s, k) => {
      const rects = s.counts.map((c, b) => ({
        x0: x0 + ((b + 0.06) / bins) * (x1 - x0),
        x1: x0 + ((b + 0.94) / bins) * (x1 - x0),
        y0,
        y1: y0 + (c / tallest) * (y1 - y0),
      }));
      object.add(quads(rects, SERIES[k], k === 0 ? 0.62 : 0.45, 0.01 * k));
      const outline: number[] = [];
      for (const r of rects) {
        outline.push(r.x0, r.y0, 0.03, r.x0, r.y1, 0.03);
        outline.push(r.x0, r.y1, 0.03, r.x1, r.y1, 0.03);
        outline.push(r.x1, r.y1, 0.03, r.x1, r.y0, 0.03);
      }
      object.add(segments(outline, SERIES[k]));
    });

    // The axis, and the two markers the library gives a meaning to.
    object.add(segments([x0, y0, 0.02, x1, y0, 0.02], INK));
    const atUnits = (u: number): number => x0 + (u / span) * (x1 - x0);
    for (const [units, colour, text] of [
      [1, 0x6f7c92, 'ReferenceSpacing'],
      [
        UNIT.ReferenceSpacing(count) > 0 ? UNIT.BlueNoiseRadius(count) / UNIT.ReferenceSpacing(count) : 0,
        palette.accent,
        'BlueNoiseRadius',
      ],
    ] as [number, number, string][]) {
      if (units <= 0 || units > span) continue;
      const x = atUnits(units);
      const dashes: number[] = [];
      for (let t = 0; t < 24; t++) {
        const ya = y0 + ((t + 0.15) / 24) * (y1 - y0);
        const yb = y0 + ((t + 0.7) / 24) * (y1 - y0);
        dashes.push(x, ya, 0.04, x, yb, 0.04);
      }
      object.add(segments(dashes, colour));
      object.add(placedLabel(text, x, y1 + 0.06, 0.05, 0.7));
    }

    for (let u = 0; u <= span + 1e-6; u += 0.5) {
      object.add(placedLabel(u.toFixed(1), atUnits(u), y0 - 0.06, 0.045, 0.2));
    }
    object.add(placedLabel('nearest-neighbour distance / ReferenceSpacing', 0, y0 - 0.15, 0.05, 1.5));
    series.forEach((s, k) => {
      object.add(
        placedLabel(
          `${s.family.name} — ${s.points.Count()} points`,
          x0 + 0.34,
          y1 - 0.07 - k * 0.09,
          0.055,
          0.9,
          k === 0 ? '#8fd0ff' : '#e0894a',
        ),
      );
    });

    for (const s of series) {
      const finite = s.distances.filter(d => Number.isFinite(d));
      const min = finite.length > 0 ? Math.min(...finite) : NaN;
      const max = finite.length > 0 ? Math.max(...finite) : NaN;
      const mean = finite.length > 0 ? finite.reduce((a, b) => a + b, 0) / finite.length : NaN;
      readings.push(
        note(
          s.family.name,
          `${s.points.Count()} pts · nearest ${n4(min)} mean ${n4(mean)} widest ${n4(max)}`,
        ),
        reading(`${s.family.name} RelativeRadius`, () => n3(UNIT.RelativeRadius(s.points))),
        reading(`${s.family.name} RelativeMeanSpacing`, () => n3(UNIT.RelativeMeanSpacing(s.points))),
      );
    }
    readings.push(
      reading('ReferenceSpacing(count)', () => n4(UNIT.ReferenceSpacing(count))),
      reading('BlueNoiseRadius(count)', () => n4(UNIT.BlueNoiseRadius(count))),
      note('bins', `${bins} over 0 .. ${n2(span)} reference spacings`),
    );
    return { object, readings };
  },
});

// ---------------------------------------------------------------------------
// Scene 3 — the radial power spectrum

const spectrum = sceneOf({
  id: 'spectrum',
  title: 'The radial power spectrum',
  description:
    'RadialPowerSpectrum is the reading the word "blue" actually names, and the only one of the three ' +
    'that separates EVEN from REGULAR. Band b is the mean periodogram power at b + 1 cycles across the ' +
    'region, averaged over Directions evenly spaced directions of that magnitude — so the average is a ' +
    'statement about isotropy as much as about frequency. A Poisson process reads one everywhere. Blue ' +
    'noise reads near zero through the low bands, rises through one around the mean spacing and settles ' +
    'back; a regular grid is near zero between spikes and enormous at its own period. The dashed line ' +
    'marks the reciprocal of ReferenceSpacing, which is where the rise should happen. Read band 0 with ' +
    'suspicion whatever the pattern: one cycle across a bounded square is a diagonal fraction of a ' +
    'period in most of the directions averaged over, so the first band measures the window as much as ' +
    'the points and reads near one for everything. The comparison starts at band 1.',
  plato: [
    'Bounds2D.RadialPowerSpectrum',
    'Number.RadialPower',
    'Number.WavePower',
    'Bounds2D.ReferenceSpacing',
    'Bounds2D.RelativeRadius',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'a', label: 'Set A', kind: 'select', options: FAMILY_LABELS, def: 9 },
    { key: 'b', label: 'Set B', kind: 'select', options: FAMILY_LABELS, def: 0 },
    { key: 'count', label: 'Count', kind: 'slider', min: 60, max: 320, step: 20, def: 200 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'bands', label: 'Bands', kind: 'slider', min: 12, max: 64, step: 4, def: 48 },
    { key: 'directions', label: 'Directions', kind: 'slider', min: 4, max: 48, step: 4, def: 32 },
    { key: 'ceiling', label: 'Power ceiling', kind: 'slider', min: 1.5, max: 8, step: 0.5, def: 3 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.count);
    const seed = Math.round(params.seed);
    const bands = Math.round(params.bands);
    const directions = Math.round(params.directions);
    const ceiling = params.ceiling;

    const x0 = -0.92;
    const x1 = 0.92;
    const y0 = -0.7;
    const y1 = 0.76;

    const object = new THREE.Group();
    const readings: Reading[] = [
      note('count asked for', String(count)),
      note('bands', `${bands} · directions ${directions}`),
    ];

    // Power 0 sits on the baseline and `ceiling` at the top; anything above is
    // clipped to the top and its true value goes in the status line.
    const atPower = (p: number): number => y0 + Math.min(1, Math.max(0, p / ceiling)) * (y1 - y0);
    const atBand = (b: number): number => x0 + ((b + 0.5) / bands) * (x1 - x0);

    object.add(segments([x0, y0, 0.02, x1, y0, 0.02], INK));
    object.add(segments([x0, y0, 0.02, x0, y1, 0.02], INK));
    // Power one: the level a Poisson process sits at, so it is the line to read
    // "suppressed" and "carried" against.
    object.add(segments([x0, atPower(1), 0.02, x1, atPower(1), 0.02], 0x55627a));
    object.add(placedLabel('power 1', x1 - 0.1, atPower(1) + 0.05, 0.05, 0.4));
    object.add(placedLabel(n2(ceiling), x0 - 0.06, y1, 0.045, 0.2));
    object.add(placedLabel('0', x0 - 0.06, y0, 0.045, 0.12));

    // Where the mean spacing puts its cycle count.
    const reference = UNIT.ReferenceSpacing(count);
    if (reference > 0) {
      const band = 1 / reference - 1;
      if (band >= 0 && band < bands) {
        const x = atBand(band);
        const dashes: number[] = [];
        for (let t = 0; t < 20; t++) {
          dashes.push(x, y0 + ((t + 0.15) / 20) * (y1 - y0), 0.03, x, y0 + ((t + 0.7) / 20) * (y1 - y0), 0.03);
        }
        object.add(segments(dashes, 0x6f7c92));
        object.add(placedLabel('1 / ReferenceSpacing', x, y1 + 0.06, 0.05, 0.85));
      }
    }

    const keys = ['a', 'b'] as const;
    keys.forEach((key, k) => {
      const built = familyPoints(params[key], count, seed);
      const values: number[] = [];
      const spec = reading(`${built.family.name} RadialPowerSpectrum`, () => {
        const started = performance.now();
        const result = UNIT.RadialPowerSpectrum(built.points, bands, directions);
        for (let b = 0; b < bands; b++) values.push(result.At(b));
        return `${bands} bands in ${(performance.now() - started).toFixed(1)} ms`;
      });

      if (values.length > 0) {
        const xs = values.map((_, b) => atBand(b));
        const ys = values.map(v => atPower(v));
        object.add(polyline(xs, ys, SERIES[k], 0.03 + 0.005 * k));
        object.add(dots2D(xs.map((x, i) => ({ x, y: ys[i] })), SERIES[k], 3.5, 0.05));

        const finite = values.filter(v => Number.isFinite(v));
        const max = finite.length > 0 ? Math.max(...finite) : NaN;
        const maxBand = values.indexOf(max);
        let crossing = -1;
        for (let b = 0; b < values.length; b++) {
          if (values[b] >= 1) {
            crossing = b;
            break;
          }
        }
        const low = values.slice(0, Math.min(6, values.length));
        readings.push(
          note(
            built.family.name,
            `${built.points.Count()} pts · bands 0-5 ${low.map(v => n2(v)).join(' ')}`,
          ),
          note(
            `${built.family.name} peak`,
            `${n2(max)} at band ${maxBand}${crossing >= 0 ? `, first reaches 1 at band ${crossing}` : ', never reaches 1'}`,
          ),
          spec,
        );
      } else {
        readings.push(spec);
      }
      object.add(
        placedLabel(
          `${built.family.name} — ${built.points.Count()} points`,
          x0 + 0.36,
          y1 - 0.07 - k * 0.09,
          0.055,
          0.95,
          k === 0 ? '#8fd0ff' : '#e0894a',
        ),
      );
    });

    object.add(placedLabel(`band 0 .. ${bands - 1}  (cycles across the region)`, 0, y0 - 0.09, 0.05, 1.5));
    readings.push(reading('ReferenceSpacing(count)', () => n4(reference)));
    return { object, readings };
  },
});

// ---------------------------------------------------------------------------
// Scene 4 — the full 2D periodogram

const periodogram = sceneOf({
  id: 'periodogram',
  title: 'The periodogram, as an image',
  description:
    'The same power the spectrum above averages, drawn without averaging: one WavePower call per pixel ' +
    'over a window of the frequency plane, so the horizontal axis is cycles along X and the vertical ' +
    'cycles along Y. This is where isotropy is visible rather than inferred. Blue noise is a dark hole ' +
    'at the origin inside a bright ring at the mean spacing, the same in every direction; a regular grid ' +
    'is a lattice of isolated spikes; and the low-discrepancy families carry hard crosses along the axes ' +
    '— which is exactly why they integrate better than blue noise and look worse than it. The centre ' +
    'pixel is the zero frequency, where WavePower is the point count by construction. It also shows the ' +
    'caveat the library states about its own Poisson-disk sweep: the background grid is walked in scan ' +
    'order, and the pattern keeps a trace of that grid, which appears here as bright points on the axes ' +
    'at the grid\'s own period — the reciprocal of PoissonCellSize, marked when the window reaches it. ' +
    'It is not symmetric: the trace is far stronger across the sweep than along it, which is the ' +
    'directional bias the library warns of rather than an isotropic ring. The status line reads the ' +
    'power on and off that frequency in both directions, and no spacing reading can see any of it.',
  plato: [
    'Number.WavePower',
    'ColorGradient.ColorAtParameter',
    'Bounds2D.ReferenceSpacing',
    'Bounds2D.BlueNoiseRadius',
    'Number.PoissonCellSize',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'family', label: 'Pattern', kind: 'select', options: FAMILY_LABELS, def: 9 },
    { key: 'count', label: 'Count', kind: 'slider', min: 60, max: 260, step: 20, def: 160 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'extent', label: 'Cycles shown', kind: 'slider', min: 8, max: 40, step: 4, def: 24 },
    { key: 'resolution', label: 'Resolution', kind: 'slider', min: 33, max: 89, step: 8, def: 57 },
    { key: 'ceiling', label: 'Power ceiling', kind: 'slider', min: 1, max: 8, step: 0.5, def: 3 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.count);
    const seed = Math.round(params.seed);
    const extent = Math.round(params.extent);
    const resolution = Math.round(params.resolution);
    const ceiling = params.ceiling;
    const built = familyPoints(params.family, count, seed);
    const ramp = rampLookup('power');

    let max = -Infinity;
    let maxAt = '';
    let nan = 0;
    let dc = NaN;
    const started = performance.now();
    const shade = (u: number, v: number): Rgb => {
      const fx = (u - 0.5) * 2 * extent;
      const fy = (v - 0.5) * 2 * extent;
      const power = fx.WavePower(fy, built.points);
      if (!Number.isFinite(power)) {
        nan++;
        return { r: 0.85, g: 0.1, b: 0.65 };
      }
      if (fx === 0 && fy === 0) dc = power;
      else if (power > max) {
        max = power;
        maxAt = `(${n2(fx)}, ${n2(fy)})`;
      }
      return ramp(Math.min(1, power / ceiling));
    };

    const object = new THREE.Group();
    object.add(rasterPlane(shade, { resolution, size: 1.7 }));
    const ms = performance.now() - started;
    object.add(frame(-0.85, -0.85, 0.85, 0.85, INK));

    // The mean-spacing ring, which is where the bright band should sit.
    const reference = UNIT.ReferenceSpacing(built.points.Count());
    if (reference > 0 && 1 / reference <= extent) {
      object.add(circle(0, 0, (1 / reference / extent) * 0.85, 0x9fb3d0, 72, 0.05, 0.55));
      object.add(placedLabel('1 / ReferenceSpacing', 0, -0.93, 0.05, 0.9));
    }
    object.add(placedLabel(`${built.family.name} — ${built.points.Count()} points`, 0, 0.94, 0.06, 1.2));
    object.add(placedLabel(`${extent} cycles`, 0.72, -0.93, 0.045, 0.35));

    // The two families that go through the Poisson sweep carry a trace of the
    // background grid they were built over. Its period is PoissonCellSize, so
    // its frequency is where the trace should be, and marking it is the
    // difference between "there is a bright dot there" and an explanation.
    const sweep = built.family.name === 'Poisson' || built.family.name === 'Blue';
    const cellRadius = sweep ? UNIT.BlueNoiseRadius(count) : 0;
    const gridFrequency = cellRadius > 0 ? 1 / cellRadius.PoissonCellSize() : 0;
    const gridReadings: Reading[] = [];
    if (sweep && gridFrequency > 0) {
      if (gridFrequency <= extent) {
        for (const s of [-1, 1]) {
          const at = ((gridFrequency * s) / extent) * 0.85;
          object.add(circle(at, 0, 0.035, palette.surfaceAlt, 20, 0.06, 0.9));
          object.add(circle(0, at, 0.035, palette.surfaceAlt, 20, 0.06, 0.9));
        }
        object.add(placedLabel('1 / PoissonCellSize', -0.55, 0.86, 0.045, 0.75, '#e0894a'));
      }
      gridReadings.push(
        note('background grid', `${n2(gridFrequency)} cycles = 1 / PoissonCellSize(${n4(cellRadius)})`),
        reading('WavePower on the grid frequency', () => {
          const alongX = gridFrequency.WavePower(0, built.points);
          const alongY = (0).WavePower(gridFrequency, built.points);
          const off = (gridFrequency * 0.6).WavePower(0, built.points);
          return `${n2(alongX)} along X, ${n2(alongY)} along Y, against ${n2(off)} off it — the sweep's own lattice`;
        }),
      );
    }

    return {
      object,
      readings: [
        note('pattern', `${built.family.member}`),
        note('detail', built.family.detail),
        note('points', String(built.points.Count())),
        note('window', `${resolution}x${resolution} over +/-${extent} cycles`),
        note('WavePower', `${resolution * resolution} calls in ${ms.toFixed(1)} ms`),
        reading('WavePower(0, 0) = the count', () => n2(dc)),
        note('largest non-zero power', `${n2(max)} at ${maxAt}`),
        note('ceiling drawn at', n2(ceiling)),
        reading('ReferenceSpacing', () => n4(reference)),
        ...gridReadings,
        ...(nan > 0 ? [note('WavePower NaN', `${nan} of ${resolution * resolution} pixels`)] : []),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 5 — white noise becomes blue

const relaxation = sceneOf({
  id: 'relaxation',
  title: 'White noise becomes blue',
  description:
    'A jittered grid at jitter one — one uniform draw per cell, which is as close to white noise as the ' +
    'library gets while keeping the count fixed — pushed apart by RepelledPoints2D, one pass per step of ' +
    'the slider. Each pass moves every point away from every neighbour inside the radius, the push ' +
    'falling linearly to zero there, and every point moves from the SAME starting configuration, so a ' +
    'pass is a simultaneous step rather than a sweep and the answer does not depend on the storage ' +
    'order. Watch RelativeRadius climb: the library says Lloyd relaxation is not here and why, and this ' +
    'is the cheap half of it that needs no assignment table. Each pass is O(n^2) through an Append ' +
    'chain, so the demo materialises between passes — the passes are eager here for the same reason a ' +
    'simulation fold has to be.',
  plato: [
    'Bounds2D.RepelledPoints2D',
    'Bounds2D.RepelledPoint2D',
    'Bounds2D.RelaxedPoints2D',
    'Bounds2D.JitteredGridPoints2D',
    'Bounds2D.ReferenceSpacing',
    'Bounds2D.RelativeRadius',
    'Bounds2D.RelativeMeanSpacing',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'side', label: 'Grid side', kind: 'slider', min: 6, max: 18, step: 1, def: 12 },
    { key: 'passes', label: 'Passes', kind: 'slider', min: 0, max: 10, step: 1, def: 4 },
    { key: 'strength', label: 'Strength', kind: 'slider', min: 0.05, max: 0.6, step: 0.05, def: 0.2 },
    { key: 'radius', label: 'Radius x spacing', kind: 'slider', min: 0.4, max: 2, step: 0.1, def: 1 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'trails', label: 'Show trails', kind: 'toggle', def: 1 },
  ],
  build(params: Params): Built {
    const side = Math.round(params.side);
    const passes = Math.round(params.passes);
    const seed = Math.round(params.seed);
    const strength = params.strength;
    const count = side * side;
    const reference = UNIT.ReferenceSpacing(count);
    const radius = reference * params.radius;

    const start = solid(UNIT.JitteredGridPoints2D(new IntegerVector2(side, side), 1, seed));

    // One `RepelledPoints2D` per pass, each materialised. `RelaxedPoints2D` is
    // the member that folds them, and the reading below checks that this loop is
    // exactly it — but its intermediate arrays stay lazy Append chains, so the
    // fold costs O(n^3) where the eager loop costs O(n^2) per pass.
    const started = performance.now();
    let current = start;
    let threw: string | null = null;
    try {
      for (let k = 0; k < passes; k++) current = solid(UNIT.RepelledPoints2D(current, radius, strength));
    } catch (error) {
      threw = (error as Error).message;
    }
    const ms = performance.now() - started;

    const tile = 1.75;
    const object = new THREE.Group();
    const before = placeAll(start, 0, 0, tile);
    const after = placeAll(current, 0, 0, tile);

    if (params.trails > 0.5 && passes > 0) {
      const trail: number[] = [];
      for (let i = 0; i < Math.min(before.length, after.length); i++) {
        trail.push(before[i].x, before[i].y, 0.01, after[i].x, after[i].y, 0.01);
      }
      object.add(segments(trail, 0x3d4f6b));
    }
    object.add(dots2D(before, INK_DIM, 4));
    object.add(dots2D(after, palette.accent, 5));
    object.add(frame(-tile / 2, -tile / 2, tile / 2, tile / 2, INK));
    object.add(
      placedLabel(
        `${count} points · ${passes} pass${passes === 1 ? '' : 'es'} · radius ${n3(radius)} strength ${n2(strength)}`,
        0,
        tile / 2 + 0.07,
        0.055,
        1.8,
      ),
    );

    const readings: Reading[] = [
      note('start', `JitteredGridPoints2D(${side}x${side}, jitter 1) — ${count} points`),
      note('passes', `${passes} · ${ms.toFixed(1)} ms`),
      note('radius', `${n4(radius)} = ${n2(params.radius)} x ReferenceSpacing(${count})`),
    ];
    if (threw) {
      readings.push(note('RepelledPoints2D', `UNAVAILABLE (${threw})`));
    } else {
      readings.push(
        reading('RelativeRadius at pass 0', () => n3(UNIT.RelativeRadius(start))),
        reading(`RelativeRadius at pass ${passes}`, () => n3(UNIT.RelativeRadius(current))),
        reading('RelativeMeanSpacing at pass 0', () => n3(UNIT.RelativeMeanSpacing(start))),
        reading(`RelativeMeanSpacing at pass ${passes}`, () => n3(UNIT.RelativeMeanSpacing(current))),
      );
      // Proof that the eager loop is the library's own fold. Capped, because
      // RelaxedPoints2D reads its own Append chains.
      if (count <= 150 && passes <= 6) {
        readings.push(
          reading('RelaxedPoints2D agrees', () => {
            const folded = solid(UNIT.RelaxedPoints2D(start, radius, strength, passes));
            let worst = 0;
            for (let i = 0; i < folded.Count(); i++) {
              worst = Math.max(
                worst,
                Math.abs(folded.At(i).X - current.At(i).X),
                Math.abs(folded.At(i).Y - current.At(i).Y),
              );
            }
            return `largest coordinate difference ${worst.toExponential(1)}`;
          }),
        );
      } else {
        readings.push(
          note(
            'RelaxedPoints2D',
            `not folded at this size — the member reads its own Append chains, which is O(n^3); ` +
              `drop to 150 points and 6 passes to see the check`,
          ),
        );
      }
    }
    return { object, readings };
  },
});

// ---------------------------------------------------------------------------
// Scene 6 — the algorithm, not the result

const poissonGrid = sceneOf({
  id: 'poisson',
  title: 'Poisson-disk: the background grid',
  description:
    'The mechanism rather than the picture. Cells are PoissonCellSize across — radius over root two, the ' +
    'largest cell whose diagonal is still shorter than the radius — so a cell holds at most one sample ' +
    'and its accepted point IS its occupancy record. That is what turns dart throwing from quadratic ' +
    'into linear: a candidate is only ever measured against the 5x5 block of cells around it, and only ' +
    'the earlier half of that block, because the later cells are still empty and will test against this ' +
    'one when their turn comes. The inspected cell shows all MaxAttempts darts PoissonCandidate throws ' +
    'at it — each a pure function of the seed, the cell and the attempt number — and which one, if any, ' +
    'cleared its neighbourhood.',
  plato: [
    'Bounds2D.PoissonDiskPoints2D',
    'Bounds2D.PoissonCandidate',
    'Bounds2D.PoissonColumns',
    'Bounds2D.PoissonRows',
    'Bounds2D.EmptyCellPoint',
    'Point2D.IsEmptyCell',
    'Number.PoissonCellSize',
    'Bounds2D.Contains',
    'Bounds2D.RelativeRadius',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'radius', label: 'Radius', kind: 'slider', min: 0.05, max: 0.2, step: 0.005, def: 0.09 },
    { key: 'attempts', label: 'Max attempts', kind: 'slider', min: 1, max: 30, step: 1, def: 30 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'cellX', label: 'Inspect column', kind: 'slider', min: 0, max: 24, step: 1, def: 7 },
    { key: 'cellY', label: 'Inspect row', kind: 'slider', min: 0, max: 24, step: 1, def: 7 },
    { key: 'circles', label: 'Show every radius', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const radius = params.radius;
    const attempts = Math.round(params.attempts);
    const seed = Math.round(params.seed);
    const cellSize = radius.PoissonCellSize();
    const columns = UNIT.PoissonColumns(radius);
    const rows = UNIT.PoissonRows(radius);

    const started = performance.now();
    const points = solid(UNIT.PoissonDiskPoints2D(radius, attempts, seed));
    const ms = performance.now() - started;

    const tile = 1.7;
    const object = new THREE.Group();
    const toWorld = (p: Point2D): { x: number; y: number } => place(p, 0, 0, tile);

    // Which cell each accepted point fell in. Its own cell index is arithmetic
    // on the position, which is repacking; the acceptance was the library's.
    const occupied = new Map<string, Point2D>();
    for (let i = 0; i < points.Count(); i++) {
      const p = points.At(i);
      const ci = Math.floor((p.X - UNIT.Min.X) / cellSize);
      const cj = Math.floor((p.Y - UNIT.Min.Y) / cellSize);
      occupied.set(`${ci},${cj}`, p);
    }

    const shaded: { x0: number; y0: number; x1: number; y1: number }[] = [];
    for (const key of occupied.keys()) {
      const [ci, cj] = key.split(',').map(Number);
      const a = place(new Point2D(ci * cellSize, cj * cellSize), 0, 0, tile);
      const b = place(new Point2D((ci + 1) * cellSize, (cj + 1) * cellSize), 0, 0, tile);
      shaded.push({ x0: a.x, y0: a.y, x1: b.x, y1: b.y });
    }
    object.add(quads(shaded, 0x2c3d55, 0.85, -0.02));

    const grid: number[] = [];
    for (let i = 0; i <= columns; i++) {
      const a = place(new Point2D(i * cellSize, 0), 0, 0, tile);
      const b = place(new Point2D(i * cellSize, rows * cellSize), 0, 0, tile);
      grid.push(a.x, a.y, 0, b.x, b.y, 0);
    }
    for (let j = 0; j <= rows; j++) {
      const a = place(new Point2D(0, j * cellSize), 0, 0, tile);
      const b = place(new Point2D(columns * cellSize, j * cellSize), 0, 0, tile);
      grid.push(a.x, a.y, 0, b.x, b.y, 0);
    }
    object.add(segments(grid, INK));
    object.add(frame(-tile / 2, -tile / 2, tile / 2, tile / 2, 0x3c4a60));

    const world: { x: number; y: number }[] = [];
    for (let i = 0; i < points.Count(); i++) world.push(toWorld(points.At(i)));
    if (params.circles > 0.5) {
      const rings: number[] = [];
      for (const p of world) circleInto(rings, p.x, p.y, radius * tile, 24, 0.02);
      object.add(segments(rings, 0x4c6d92, 0.5));
    }
    object.add(dots2D(world, palette.line, 5));

    // The inspected cell: the block it consults, the darts it throws, the result.
    const ci = Math.min(Math.max(0, Math.round(params.cellX)), Math.max(0, columns - 1));
    const cj = Math.min(Math.max(0, Math.round(params.cellY)), Math.max(0, rows - 1));
    const blockA = place(new Point2D((ci - 2) * cellSize, (cj - 2) * cellSize), 0, 0, tile);
    const blockB = place(new Point2D((ci + 3) * cellSize, (cj + 3) * cellSize), 0, 0, tile);
    object.add(frame(blockA.x, blockA.y, blockB.x, blockB.y, palette.surfaceAlt, 0.04));

    const accepted = occupied.get(`${ci},${cj}`) ?? null;
    // The library's attempt loop stops considering candidates once one has been
    // accepted — the `IsEmptyCell(accepted)` guard in PoissonCellPoint — so the
    // darts after the winning one were generated by the seed but never thrown.
    // Drawing them all as rejections would misdescribe the algorithm.
    let landedOutside = 0;
    let blocked = 0;
    let unthrown = 0;
    let acceptedAttempt = -1;
    const darts: { x: number; y: number; c: Rgb }[] = [];
    const outsideColour: Rgb = { r: 0.5, g: 0.36, b: 0.42 };
    const blockedColour: Rgb = { r: 0.88, g: 0.4, b: 0.32 };
    const takenColour: Rgb = { r: 0.4, g: 0.92, b: 0.68 };
    const unthrownColour: Rgb = { r: 0.22, g: 0.26, b: 0.33 };
    const candidateFailure = reading('Bounds2D.PoissonCandidate', () => {
      for (let a = 0; a < attempts; a++) {
        const candidate = UNIT.PoissonCandidate(radius, seed, ci, cj, a);
        const w = toWorld(candidate);
        const isAccepted =
          acceptedAttempt < 0 &&
          accepted !== null &&
          candidate.X === accepted.X &&
          candidate.Y === accepted.Y;
        if (isAccepted) {
          acceptedAttempt = a;
          darts.push({ ...w, c: takenColour });
          continue;
        }
        if (acceptedAttempt >= 0) {
          unthrown++;
          darts.push({ ...w, c: unthrownColour });
          continue;
        }
        if (UNIT.Contains(candidate)) blocked++;
        else landedOutside++;
        darts.push({ ...w, c: UNIT.Contains(candidate) ? blockedColour : outsideColour });
      }
      return `${attempts} candidates at cell (${ci}, ${cj}); ${attempts - unthrown} of them actually thrown`;
    });
    if (darts.length > 0) object.add(coloredDots(darts, 5.5, 0.05));
    if (accepted) {
      const w = toWorld(accepted);
      object.add(circle(w.x, w.y, radius * tile, palette.accent, 48, 0.05));
    }

    const emptyPoint = UNIT.EmptyCellPoint(radius);
    return {
      object,
      readings: [
        note('radius', n4(radius)),
        reading('PoissonCellSize', () => `${n4(cellSize)} = radius / sqrt(2)`),
        note('background grid', `${columns} columns x ${rows} rows = ${columns * rows} cells`),
        note('points', `${points.Count()} in ${ms.toFixed(1)} ms`),
        note(
          'cells occupied',
          `${occupied.size} of ${columns * rows} (${((100 * occupied.size) / Math.max(1, columns * rows)).toFixed(1)}%)`,
        ),
        candidateFailure,
        note(
          `cell (${ci}, ${cj})`,
          accepted
            ? `dart ${acceptedAttempt} won — ${blocked} earlier dart(s) were blocked by a neighbour, ` +
              `${landedOutside} fell outside the region, and ${unthrown} later candidate(s) were never thrown ` +
              `because the cell was already full`
            : `empty — all ${attempts} darts thrown, ${blocked} blocked by a neighbour and ${landedOutside} outside the region`,
        ),
        reading('EmptyCellPoint', () => `(${n3(emptyPoint.X)}, ${n3(emptyPoint.Y)})`),
        reading('EmptyCellPoint.IsEmptyCell', () => String(emptyPoint.IsEmptyCell(UNIT, radius))),
        reading('Bounds2D.RelativeRadius', () => n3(UNIT.RelativeRadius(points))),
        reading('Bounds2D.RelativeMeanSpacing', () => n3(UNIT.RelativeMeanSpacing(points))),
      ],
    };
  },
});

// ---------------------------------------------------------------------------
// Scene 7 — the honest twist

const discrepancy = sceneOf({
  id: 'discrepancy',
  title: 'Discrepancy: where blue noise loses',
  description:
    'StarDiscrepancyEstimate across every family, LOWER being better: the largest gap, over the boxes ' +
    'anchored at the region\'s minimum corner, between the fraction of points inside a box and the ' +
    'fraction of area it covers. This is the reading the low-discrepancy families were designed to win, ' +
    'and they do — Hammersley, Sobol, Halton and the R2 sequence all beat blue noise here while looking ' +
    'obviously worse in the gallery, and a regular grid is the worst of the lot despite having the ' +
    'highest RelativeRadius of anything. Both numbers are true, they measure different things, and a ' +
    'page that reported only one of them would be lying by omission. On the right, the selected family ' +
    'with its worst box drawn, found by BoxDiscrepancy at every point.',
  plato: [
    'Bounds2D.StarDiscrepancyEstimate',
    'Point2D.BoxDiscrepancy',
    'Bounds2D.RelativeRadius',
    'Bounds2D.RegionArea',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'family', label: 'Draw', kind: 'select', options: FAMILY_LABELS, def: 4 },
    { key: 'count', label: 'Count', kind: 'slider', min: 40, max: 260, step: 20, def: 140 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.count);
    const seed = Math.round(params.seed);
    const selected = clampIndex(params.family, FAMILIES.length);

    const object = new THREE.Group();
    const readings: Reading[] = [note('count asked for', String(count))];

    interface Row {
      family: Family;
      value: number;
      points: IArray<Point2D>;
      failure: string | null;
    }
    const rows: Row[] = [];
    for (let i = 0; i < FAMILIES.length; i++) {
      const built = familyPoints(i, count, seed);
      let value = NaN;
      let failure: string | null = null;
      try {
        value = UNIT.StarDiscrepancyEstimate(built.points);
      } catch (error) {
        failure = (error as Error).message;
      }
      rows.push({ family: built.family, value, points: built.points, failure });
    }
    const worstValue = Math.max(...rows.map(r => (Number.isFinite(r.value) ? r.value : 0)), 1e-6);

    // The bars, in the family order the gallery uses.
    const barX0 = -0.96;
    const barX1 = 0.06;
    const barTop = 0.88;
    const step = 0.175;
    rows.forEach((row, i) => {
      const y = barTop - i * step;
      const width = Number.isFinite(row.value) ? (row.value / worstValue) * (barX1 - barX0) : 0;
      object.add(
        quads(
          [{ x0: barX0, y0: y - 0.052, x1: barX0 + width, y1: y + 0.052 }],
          i === selected ? palette.accent : palette.line,
          i === selected ? 0.95 : 0.45,
          0.01,
        ),
      );
      object.add(placedLabel(row.family.name, barX0 - 0.005, y + 0.055, 0.048, 0.4));
      object.add(
        placedLabel(
          Number.isFinite(row.value) ? n4(row.value) : 'UNAVAILABLE',
          barX0 + width + 0.09,
          y,
          0.05,
          0.42,
        ),
      );
    });
    object.add(segments([barX0, barTop - rows.length * step + 0.06, 0.02, barX0, barTop + 0.06, 0.02], INK));
    object.add(placedLabel('StarDiscrepancyEstimate — lower is better', -0.45, barTop - rows.length * step - 0.02, 0.055, 1.05));

    // The selected family with its worst box.
    const chosen = rows[selected];
    const tile = 0.82;
    const cx = 0.56;
    const cy = 0.12;
    object.add(dots2D(placeAll(chosen.points, cx, cy, tile), palette.line, 3.5));
    object.add(frame(cx - tile / 2, cy - tile / 2, cx + tile / 2, cy + tile / 2, INK));

    let worstCorner: Point2D | null = null;
    let worstLocal = 0;
    const boxReading = reading('Point2D.BoxDiscrepancy', () => {
      for (let i = 0; i < chosen.points.Count(); i++) {
        const p = chosen.points.At(i);
        const local = p.BoxDiscrepancy(UNIT, chosen.points);
        if (local > worstLocal) {
          worstLocal = local;
          worstCorner = p;
        }
      }
      return `${chosen.points.Count()} boxes tested`;
    });
    if (worstCorner) {
      const corner: Point2D = worstCorner;
      const a = place(new Point2D(0, 0), cx, cy, tile);
      const b = place(corner, cx, cy, tile);
      object.add(quads([{ x0: a.x, y0: a.y, x1: b.x, y1: b.y }], palette.surfaceAlt, 0.22, 0.02));
      object.add(frame(a.x, a.y, b.x, b.y, palette.surfaceAlt, 0.03));
      let insideBox = 0;
      for (let i = 0; i < chosen.points.Count(); i++) {
        const p = chosen.points.At(i);
        if (p.X <= corner.X && p.Y <= corner.Y) insideBox++;
      }
      readings.push(
        note(
          'worst box',
          `corner (${n3(corner.X)}, ${n3(corner.Y)}) — ${insideBox} of ${chosen.points.Count()} points ` +
            `(${((100 * insideBox) / Math.max(1, chosen.points.Count())).toFixed(1)}%) inside ` +
            `${(100 * corner.X * corner.Y).toFixed(1)}% of the area, discrepancy ${n4(worstLocal)}`,
        ),
      );
    }
    object.add(placedLabel(chosen.family.name, cx, cy + tile / 2 + 0.06, 0.055, 0.8));

    const ranked = rows
      .filter(r => Number.isFinite(r.value))
      .slice()
      .sort((a, b) => a.value - b.value);
    // The two rankings side by side, because the disagreement is the point.
    const byRadius = rows
      .map(r => ({ name: r.family.name, radius: safeRelativeRadius(r.points) }))
      .filter(r => Number.isFinite(r.radius))
      .sort((a, b) => b.radius - a.radius);
    readings.push(
      note(
        'by discrepancy, best first',
        ranked.map(r => `${r.family.name} ${n4(r.value)}`).join('  '),
      ),
      note(
        'by RelativeRadius, bluest first',
        byRadius.map(r => `${r.name} ${n3(r.radius)}`).join('  '),
      ),
      boxReading,
      reading(`${chosen.family.name} RelativeRadius`, () => n3(UNIT.RelativeRadius(chosen.points))),
      note(
        'the twist',
        'the family with the highest RelativeRadius is not the one with the lowest discrepancy — ' +
          'even and uniform are different properties',
      ),
      ...rows.filter(r => r.failure).map(r => note(`${r.family.name} StarDiscrepancyEstimate`, `UNAVAILABLE (${r.failure})`)),
    );
    return { object, readings };
  },
});

// ---------------------------------------------------------------------------
// Scene 8 — the sequence itself, one point at a time

const SEQUENCES = ['Halton', 'Hammer', 'Sobol', 'Plastic'];

const sequence = sceneOf({
  id: 'sequence',
  title: 'The sequence, one index at a time',
  description:
    'The low-discrepancy families have a property none of the seeded ones do: the k-th point is a ' +
    'function of k alone, so a PREFIX of the sequence is already well distributed and a caller may ' +
    'evaluate one point without evaluating the others. This scene calls the per-index members directly ' +
    '— RadicalInverse, HaltonPoint2D, HammersleyPoint2D, SobolPoint2D, PlasticPoint2D — and carries each ' +
    'unit-square result onto the region through FromUnitSquare, which is the library\'s only bridge from ' +
    'unit coordinates to world ones. Colour runs with the index, so dragging Count shows the square ' +
    'filling in rather than being filled. The ladder underneath is the one-dimensional building block: ' +
    'RadicalInverse mirrors the index\'s digits about the point, which is why consecutive terms land far ' +
    'apart. Hammersley is the exception that proves the rule — its first coordinate is i / Count, so ' +
    'changing Count moves every point.',
  plato: [
    'Number.RadicalInverse',
    'Number.HaltonPoint2D',
    'Number.HammersleyPoint2D',
    'Number.SobolPoint2D',
    'Number.PlasticPoint2D',
    'Number.GeneralizedGoldenRatio',
    'Number.GoldenRatioRefine',
    'Bounds2D.FromUnitSquare',
    'Bounds2D.StarDiscrepancyEstimate',
    'Bounds2D.RelativeRadius',
  ],
  viewer: PLANAR,
  controls: [
    { key: 'kind', label: 'Sequence', kind: 'select', options: SEQUENCES, def: 0 },
    { key: 'count', label: 'Count', kind: 'slider', min: 8, max: 400, step: 4, def: 140 },
    { key: 'baseX', label: 'Base X', kind: 'slider', min: 2, max: 13, step: 1, def: 2 },
    { key: 'baseY', label: 'Base Y', kind: 'slider', min: 2, max: 13, step: 1, def: 3 },
    { key: 'offset', label: 'Plastic offset', kind: 'slider', min: 0, max: 1, step: 0.05, def: 0.5 },
    { key: 'skip', label: 'Sobol skip', kind: 'slider', min: 0, max: 64, step: 1, def: 1 },
    { key: 'join', label: 'Join in index order', kind: 'toggle', def: 0 },
  ],
  build(params: Params): Built {
    const kind = clampIndex(params.kind, SEQUENCES.length);
    const count = Math.round(params.count);
    const baseX = Math.round(params.baseX);
    const baseY = Math.round(params.baseY);
    const skip = Math.round(params.skip);
    const offset = params.offset;
    const ramp = rampLookup('sequence');

    // The per-index member, called one index at a time — no pattern record and
    // no array generator in sight.
    const unitPoint = (i: number): Point2D => {
      switch (kind) {
        case 0:
          return (i + 1).HaltonPoint2D(baseX, baseY);
        case 1:
          return i.HammersleyPoint2D(count, baseX);
        case 2:
          return (skip + i).SobolPoint2D();
        default:
          return i.PlasticPoint2D(offset);
      }
    };

    const tile = 1.44;
    const object = new THREE.Group();
    const world: { x: number; y: number; c: Rgb }[] = [];
    const placed: Point2D[] = [];
    let nan = 0;
    let threw: string | null = null;
    const started = performance.now();
    try {
      for (let i = 0; i < count; i++) {
        // FromUnitSquare is the library's mapping; `place` is only the camera.
        const p = UNIT.FromUnitSquare(unitPoint(i));
        if (!Number.isFinite(p.X) || !Number.isFinite(p.Y)) {
          nan++;
          continue;
        }
        placed.push(p);
        world.push({ ...place(p, 0, 0.16, tile), c: ramp(count > 1 ? i / (count - 1) : 0) });
      }
    } catch (error) {
      threw = (error as Error).message;
    }
    const ms = performance.now() - started;

    if (params.join > 0.5 && world.length > 1) {
      const path: number[] = [];
      for (let i = 0; i + 1 < world.length; i++) {
        path.push(world[i].x, world[i].y, 0.02, world[i + 1].x, world[i + 1].y, 0.02);
      }
      object.add(segments(path, 0x39506d));
    }
    object.add(coloredDots(world, 5));
    object.add(frame(-tile / 2, 0.16 - tile / 2, tile / 2, 0.16 + tile / 2, INK));

    // The van der Corput ladder underneath: RadicalInverse of each index, drawn
    // as a tick at its own value and coloured by the same index ramp.
    const ladderY = -0.78;
    const ladder: { x: number; y: number; c: Rgb }[] = [];
    const ladderReading = reading('Number.RadicalInverse', () => {
      for (let i = 0; i < count; i++) {
        const v = (i + 1).RadicalInverse(baseX);
        ladder.push({
          x: (v - 0.5) * tile,
          y: ladderY + ((i % 9) / 9 - 0.5) * 0.12,
          c: ramp(count > 1 ? i / (count - 1) : 0),
        });
      }
      return `${count} terms in base ${baseX}`;
    });
    if (ladder.length > 0) object.add(coloredDots(ladder, 4, 0.03));
    object.add(segments([-tile / 2, ladderY - 0.09, 0.01, tile / 2, ladderY - 0.09, 0.01], INK));
    object.add(placedLabel(`RadicalInverse(i + 1, ${baseX})`, 0, ladderY - 0.17, 0.05, 1));

    const detail =
      kind === 0
        ? `HaltonPoint2D(i + 1, ${baseX}, ${baseY})`
        : kind === 1
          ? `HammersleyPoint2D(i, ${count}, ${baseX})`
          : kind === 2
            ? `SobolPoint2D(${skip} + i)`
            : `PlasticPoint2D(i, ${n2(offset)})`;
    object.add(placedLabel(detail, 0, 0.16 + tile / 2 + 0.07, 0.058, 1.4));

    const solidPrefix = fromArray(placed);
    const readings: Reading[] = [
      note('sequence', detail),
      note('points', `${placed.length} in ${ms.toFixed(1)} ms`),
      ladderReading,
      reading('RadicalInverse(11, 2)', () => `${n4((11).RadicalInverse(2))} (the smoke gate pins 0.8125)`),
      reading('GeneralizedGoldenRatio(2)', () => `${n4((2).GeneralizedGoldenRatio())} — the plastic number`),
      reading('GeneralizedGoldenRatio(3)', () => `${n4((3).GeneralizedGoldenRatio())} — the R3 constant`),
      reading('last point', () =>
        placed.length > 0
          ? `(${n4(placed[placed.length - 1].X)}, ${n4(placed[placed.length - 1].Y)})`
          : 'none',
      ),
      reading('StarDiscrepancyEstimate of the prefix', () => n4(UNIT.StarDiscrepancyEstimate(solidPrefix))),
      reading('RelativeRadius of the prefix', () => n3(UNIT.RelativeRadius(solidPrefix))),
    ];
    if (kind === 0 && baseX === baseY) {
      readings.push(
        note('bases', `${baseX} and ${baseY} are not coprime — the two coordinates correlate, which is the diagonal on screen`),
      );
    }
    if (nan > 0) readings.push(note('non-finite points', String(nan)));
    if (threw) readings.push(note('per-index member', `UNAVAILABLE (${threw})`));
    return { object, readings };
  },
});

// ---------------------------------------------------------------------------
// Scene 9 — through a volume

interface Family3D {
  name: string;
  member: string;
  points(count: number, seed: number): IArray<Point3D>;
}

const VOLUME = new Bounds3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));

/** The grid closest to a cube of `count` cells, so the grid families can be
 *  asked for a count like the closed-form ones. */
function cubeCounts(count: number): IntegerVector3 {
  const side = Math.max(1, Math.round(Math.cbrt(Math.max(1, count))));
  return new IntegerVector3(side, side, side);
}

const FAMILIES_3D: Family3D[] = [
  {
    name: 'R3',
    member: 'Bounds3D.PlasticPoints3D(count, 0.5)',
    points: count => VOLUME.PlasticPoints3D(count, 0.5),
  },
  {
    name: 'Halton',
    member: 'Bounds3D.HaltonPoints3D(count, 2, 3, 5)',
    points: count => VOLUME.HaltonPoints3D(count, 2, 3, 5),
  },
  {
    name: 'Jittered',
    member: 'Bounds3D.JitteredGridPoints3D(cube, 1, seed)',
    points: (count, seed) => VOLUME.JitteredGridPoints3D(cubeCounts(count), 1, seed),
  },
  {
    name: 'Strata',
    member: 'Bounds3D.StratifiedPoints3D(cube, 1, seed)',
    points: (count, seed) => VOLUME.StratifiedPoints3D(cubeCounts(count), 1, seed),
  },
  {
    name: 'Uniform',
    member: 'Bounds3D.StratifiedPoints3D(1x1x1 strata, count, seed)',
    points: (count, seed) => VOLUME.StratifiedPoints3D(new IntegerVector3(1, 1, 1), count, seed),
  },
];

const FAMILY_3D_LABELS = FAMILIES_3D.map(f => f.name);

function cubeWires(offsetX: number, color: number): THREE.LineSegments {
  const e = 1;
  const p = (x: number, y: number, z: number): number[] => [x + offsetX, y, z];
  const signs = [-e, e];
  const edges: [number[], number[]][] = [];
  for (const y of signs) for (const z of signs) edges.push([p(-e, y, z), p(e, y, z)]);
  for (const x of signs) for (const z of signs) edges.push([p(x, -e, z), p(x, e, z)]);
  for (const x of signs) for (const y of signs) edges.push([p(x, y, -e), p(x, y, e)]);
  const flat: number[] = [];
  for (const [a, b] of edges) flat.push(...a, ...b);
  return segments(flat, color);
}

const spatial = sceneOf({
  id: 'spatial',
  title: 'Scattering through a volume',
  description:
    'The closed-form families one dimension up, two at a time in their own cubes. There is deliberately ' +
    'NO 3D Poisson-disk and no 3D blue noise: the planar sweep gets away with a two-row sliding window ' +
    'of occupancy, and in three dimensions that window becomes two whole planes which would have to be ' +
    'carried and randomly indexed — the affine-builder problem the library states in both files. R3, the ' +
    'plastic sequence one dimension up, is the even spatial scatter to reach for meanwhile, and its ' +
    'increments come from GeneralizedGoldenRatio(3) rather than from a table. The quality readings are ' +
    'planar only, so this scene reports counts and construction rather than a blueness number it has no ' +
    'right to.',
  plato: [
    'Bounds3D.PlasticPoints3D',
    'Bounds3D.HaltonPoints3D',
    'Bounds3D.JitteredGridPoints3D',
    'Bounds3D.StratifiedPoints3D',
    'Number.PlasticPoint3D',
    'Number.HaltonPoint3D',
    'Number.GeneralizedGoldenRatio',
    'Bounds3D.FromUnitCube',
  ],
  viewer: SPATIAL,
  controls: [
    { key: 'a', label: 'Left', kind: 'select', options: FAMILY_3D_LABELS, def: 0 },
    { key: 'b', label: 'Right', kind: 'select', options: FAMILY_3D_LABELS, def: 4 },
    { key: 'count', label: 'Count', kind: 'slider', min: 40, max: 600, step: 20, def: 240 },
    { key: 'seed', label: 'Seed', kind: 'slider', min: 0, max: 32, step: 1, def: 7 },
    { key: 'size', label: 'Dot size', kind: 'slider', min: 2, max: 9, step: 0.5, def: 4 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.count);
    const seed = Math.round(params.seed);
    const object = new THREE.Group();
    const readings: Reading[] = [note('count asked for', String(count)), note('seed', String(seed))];

    (['a', 'b'] as const).forEach((key, k) => {
      const family = FAMILIES_3D[clampIndex(params[key], FAMILIES_3D.length)];
      const offsetX = k === 0 ? -1.25 : 1.25;
      const colour = k === 0 ? palette.line : palette.surfaceAlt;
      object.add(cubeWires(offsetX, INK));
      const built = reading(family.member, () => {
        const started = performance.now();
        const points = toArray(family.points(count, seed));
        const ms = performance.now() - started;
        object.add(
          dots3D(
            points.map(p => new Point3D(p.X + offsetX, p.Y, p.Z)),
            colour,
            params.size,
          ),
        );
        return `${points.length} points in ${ms.toFixed(1)} ms`;
      });
      readings.push(note(k === 0 ? 'left' : 'right', family.name), built);
    });

    readings.push(
      reading('GeneralizedGoldenRatio(3)', () => `${n4((3).GeneralizedGoldenRatio())} — the R3 constant, Newton-solved from x^4 = x + 1`),
      reading('PlasticPoint3D(11, 0.5)', () => {
        const p = (11).PlasticPoint3D(0.5);
        return `(${n4(p.X)}, ${n4(p.Y)}, ${n4(p.Z)})`;
      }),
      reading('HaltonPoint3D(11, 2, 3, 5)', () => {
        const p = (11).HaltonPoint3D(2, 3, 5);
        return `(${n4(p.X)}, ${n4(p.Y)}, ${n4(p.Z)})`;
      }),
      note(
        'no 3D Poisson-disk',
        'the library declares none and says why — the occupancy window becomes two whole planes, ' +
          'which needs an affine builder several backends do not have',
      ),
      note(
        'no 3D quality reading',
        'NearestNeighborDistance, RelativeRadius, StarDiscrepancyEstimate and RadialPowerSpectrum are ' +
          'all Bounds2D members, so nothing here is scored',
      ),
    );
    return { object, readings };
  },
});

// ---------------------------------------------------------------------------

const demo: Demo = {
  title: 'Sampling',
  subtitle: 'sampling.{types,library}.plato',
  scenes: [
    gallery,
    spacingHistogram,
    spectrum,
    periodogram,
    relaxation,
    poissonGrid,
    discrepancy,
    sequence,
    spatial,
  ],
};

// Eight of the nine scenes are planar and take the flat, grid-free camera; the
// scatter through a volume overrides it with the orbit camera.
mountDemo(demo, PLANAR);

export { demo };
