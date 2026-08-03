// Colour spaces — a scene catalog over `color.{types,library}.plato` and
// `color-spaces.{types,library}.plato`.
//
// The headline finding of this page is a negative one, and the page is built
// around reporting it honestly rather than papering over it.
//
// `color-spaces.types.plato` declares the whole catalog of colour models —
// ColorSRGB, ColorHSL, ColorHSV, ColorHWB, ColorCMYK, ColorLab, ColorLCh,
// ColorOkLab, ColorOkLCh, ColorLuv, ColorXYZ, ColorXyY, ColorYCbCr, ColorYUV,
// Chromaticity, WhitePoint, RgbPrimaries, RgbColorSpace, ColorSpaceConversion
// and the adjustment types. Its own header says why nothing converts between
// them yet: "Conversions are functions in a later pass; these types name the
// endpoints and the knobs." Everything `color-spaces.library.plato` defines is
// ColorXYZ vector arithmetic plus Chromaticity.Hash, and `color.library.plato`
// is Color's vector space, ColorGradient's sampling, and the named Color8
// constants. There is no member anywhere that takes a Color and returns a
// ColorLab, or a ColorHSV and returns a Color. The status lines are the live
// authority on that: they call the conversion members by name every rebuild.
//
// So the scenes below show what the colour tier *is*: a vector space over RGBA,
// a gradient sampler, and a named-colour table. Every colour on screen comes
// from a generated member; the status lines probe the conversion members by
// name, live, so the absence is demonstrated rather than asserted. What that
// costs is written into each scene: no Lab/OkLab ramp comparison, no gamut
// slice of a perceptual space, no CIE xy diagram, and no HSV read-out of the
// chooser's own colour.
//
// Note also the convention `color.types.plato` states for Color: linear-light
// RGBA, and Color8 is "typically sRGB-encoded". The library emits no decode or
// encode member either, so this file never mixes the two — the Color scenes
// build their colours from Color arithmetic, and the Color8 scene shows bytes.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { toArray, fromArray, polylineGeometry } from '../shared/mesh.js';
import { rasterPlane, rasterStrip, type Rgb } from '../shared/raster.js';
import { palette, type ViewerOptions } from '../shared/viewer.js';
import {
  Color,
  Color8,
  ColorGradient,
  ColorLab,
  ColorLookupTable,
  ColorStop,
  ColorXYZ,
  Chromaticity,
  Point3D,
} from '../plato/plato.g.js';
import type { Control, Demo, Params, Scene } from '../shared/demo.js';

// ---------------------------------------------------------------------------
// Reading a generated member
//
// Same pattern as `src/demos/polygons.ts`: a member that fails keeps its name in
// the status line instead of being quietly replaced. Here it does double duty —
// `absent()` calls a conversion member *by name*, so a member the writer never
// emitted reports itself as UNAVAILABLE at run time. If the conversion pass ever
// lands, these readings start printing values without a line changing here.

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

/** Call a no-argument member by name, failing loudly when it was never emitted. */
function callByName(owner: object, name: string): unknown {
  const fn = (owner as unknown as Record<string, unknown>)[name];
  if (typeof fn !== 'function') {
    throw new Error(`not a member of ${owner.constructor.name}`);
  }
  return (fn as () => unknown).call(owner);
}

/** How many of `names` exist on `owner`. Zero, at the time of writing, always. */
function emittedCount(owner: object, names: readonly string[]): number {
  return names.filter(
    n => typeof (owner as unknown as Record<string, unknown>)[n] === 'function',
  ).length;
}

const n4 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(4);
const n2 = (x: number): string => (Object.is(x, -0) ? 0 : x).toFixed(2);
const rgba = (c: Color): string => `(${n4(c.R)}, ${n4(c.G)}, ${n4(c.B)}, ${n4(c.A)})`;
const xyz = (c: ColorXYZ): string => `(${n4(c.X)}, ${n4(c.Y)}, ${n4(c.Z)})`;
const bytes = (c: Color8): string => `(${c.R}, ${c.G}, ${c.B}, ${c.A})`;

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
 * refreshed per frame because the stage is resizable.
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
// Presentation helpers

/** Repacking a generated Color for the rasteriser is demo work, not colour maths. */
const toRgb = (c: Color): Rgb => ({ r: c.R, g: c.G, b: c.B });

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

function tick(x: number, y0: number, y1: number, color: number): THREE.LineSegments {
  return segments([x, y0, 0.02, x, y1, 0.02], color);
}

const outOfRange = (c: Color): boolean =>
  !(c.R >= 0 && c.R <= 1 && c.G >= 0 && c.G <= 1 && c.B >= 0 && c.B <= 1);

/** A checker stipple over out-of-range samples, so they read as marked, not dark. */
function markOutOfRange(c: Color, u: number, v: number, resolution: number): Rgb {
  const parity = (Math.floor(u * resolution) + Math.floor(v * resolution)) % 2;
  return parity === 0 ? { r: 0.06, g: 0.07, b: 0.09 } : toRgb(c);
}

// ---------------------------------------------------------------------------
// Building inputs
//
// A hue sweep is the one thing this page needs that the stdlib does not define:
// there is no ColorHSV.ToColor and no Color.ToColorHSV. Rather than hand-roll
// the sixth-of-a-turn piecewise formula, the sweep here is a seven-stop
// ColorGradient — the stops are demo input, and the generated
// ColorGradient.ColorAtParameter is what interpolates between them. HSV's hue
// sweep is piecewise-linear in RGB as well, so the six primaries and secondaries
// land on the same values a real conversion would give; between them the stops
// are a stand-in, and the page says so rather than calling it HSV.

const opaque = (r: number, g: number, b: number): Color => new Color(r, g, b, 1);

const HUE_WHEEL = new ColorGradient(
  fromArray([
    new ColorStop(0 / 6, opaque(1, 0, 0)),
    new ColorStop(1 / 6, opaque(1, 1, 0)),
    new ColorStop(2 / 6, opaque(0, 1, 0)),
    new ColorStop(3 / 6, opaque(0, 1, 1)),
    new ColorStop(4 / 6, opaque(0, 0, 1)),
    new ColorStop(5 / 6, opaque(1, 0, 1)),
    new ColorStop(6 / 6, opaque(1, 0, 0)),
  ]),
);

/**
 * The chooser's (h, s, v) as a Color, every step a generated member:
 * ColorGradient.ColorAtParameter for the hue, Color.One and Color.Lerp for the
 * saturation, Color.Multiply for the value, Color.WithA to keep alpha at 1
 * (Multiply scales all four components, alpha included).
 */
function tintOf(params: Params, key: string): Color {
  const h = params[`${key}H`] ?? 0;
  const s = params[`${key}S`] ?? 1;
  const v = params[`${key}V`] ?? 1;
  return Color.One().Lerp(HUE_WHEEL.ColorAtParameter(h), s).Multiply(v).WithA(1);
}

/** Every conversion member this page looked for and did not find. */
const CONVERSIONS: readonly string[] = [
  'ToColorSRGB',
  'ToColorHSL',
  'ToColorHSV',
  'ToColorHWB',
  'ToColorCMYK',
  'ToColorLab',
  'ToColorLCh',
  'ToColorOkLab',
  'ToColorOkLCh',
  'ToColorLuv',
  'ToColorXYZ',
  'ToColorXyY',
  'ToColorYCbCr',
  'ToColorYUV',
  'ToColor8',
];

const SPACE_LABELS = ['sRGB', 'HSL', 'HSV', 'HWB', 'CMYK', 'Lab', 'LCh', 'OkLab', 'XYZ'];
const SPACE_MEMBERS = [
  'ToColorSRGB',
  'ToColorHSL',
  'ToColorHSV',
  'ToColorHWB',
  'ToColorCMYK',
  'ToColorLab',
  'ToColorLCh',
  'ToColorOkLab',
  'ToColorXYZ',
];

// ---------------------------------------------------------------------------
// Scenes

const chooser = sceneOf({
  id: 'chooser',
  title: 'The chooser and the read-out',
  description:
    'The saturation/value pad over the hue strip drives one Color: the hue comes from ' +
    'ColorGradient.ColorAtParameter over a seven-stop sweep, the saturation from Color.One().Lerp, ' +
    'the value from Color.Multiply. Under the swatch, Color.LinearSpace ladders it to black and to ' +
    'white. The read-out this page was meant to headline — the same colour named in every space — is ' +
    'the finding instead: pick a target space and the status line calls the conversion member by name ' +
    'and reports that the library never emitted it.',
  plato: [
    'ColorGradient.ColorAtParameter',
    'Color.One',
    'Color.Zero',
    'Color.Lerp',
    'Color.Multiply',
    'Color.WithA',
    'Color.LinearSpace',
    'Color.MidPoint',
    'Color.IsOne',
  ],
  controls: [
    { key: 'tint', label: 'Colour', kind: 'color', def: 0, colorDef: [0.55, 0.72, 0.92] },
    { key: 'steps', label: 'Ladder steps', kind: 'slider', min: 3, max: 15, step: 1, def: 9 },
    { key: 'space', label: 'Read out in', kind: 'select', options: SPACE_LABELS, def: 5 },
  ],
  build(params: Params): Built {
    const tint = tintOf(params, 'tint');
    const steps = Math.round(params.steps);
    const black = Color.Zero().WithA(1);
    const white = Color.One();
    const shades = toArray(black.LinearSpace(tint, steps));
    const tints = toArray(tint.LinearSpace(white, steps));

    // One raster for the whole plate: a big swatch on top, two ladders below.
    const resolution = 160;
    const cell = (row: readonly Color[], u: number): Color => row[
      Math.min(row.length - 1, Math.max(0, Math.floor(u * row.length)))
    ];
    const plate = rasterPlane(
      (u, v) => {
        if (v > 0.42) return toRgb(tint);
        if (v > 0.235) return toRgb(cell(tints, u));
        if (v > 0.05) return toRgb(cell(shades, u));
        return { r: 0.06, g: 0.07, b: 0.09 };
      },
      { resolution },
    );

    const object = new THREE.Group();
    object.add(plate);
    object.add(frame(-1, -0.16, 1, 1, 0x1b222c));
    object.add(frame(-1, -0.53, 1, -0.16, 0x1b222c));
    for (let i = 1; i < steps; i++) {
      const x = -1 + (2 * i) / steps;
      object.add(tick(x, -0.16, 1, 0x1b222c));
      object.add(tick(x, -0.53, -0.16, 0x1b222c));
    }

    const index = Math.round(params.space);
    const memberName = SPACE_MEMBERS[index] ?? SPACE_MEMBERS[0];

    return {
      object,
      readings: [
        note('pad', `h ${n2(params.tintH)} s ${n2(params.tintS)} v ${n2(params.tintV)}`),
        reading('Color', () => rgba(tint)),
        reading('hue ColorAtParameter', () => rgba(HUE_WHEEL.ColorAtParameter(params.tintH))),
        reading('ladder MidPoint', () => rgba(black.MidPoint(tint))),
        note('LinearSpace', `${shades.length} shades + ${tints.length} tints`),
        reading(`Color.${memberName}`, () => String(callByName(tint, memberName))),
        // The absence is on both sides: the target types carry no way back either.
        reading('ColorLab.ToColor', () => String(callByName(new ColorLab(50, 20, -30), 'ToColor'))),
        note(
          'read-out',
          `${emittedCount(tint, CONVERSIONS)} of ${CONVERSIONS.length} conversion members emitted — ` +
            'color-spaces.library.plato is ColorXYZ arithmetic and Chromaticity.Hash only',
        ),
      ],
    };
  },
});

const BLEND_LABELS = ['Lerp', 'Smooth', 'Clamp', 'Bary', 'LinComb', 'QuadBez', 'CubBez', 'CatRom'];

const plane = sceneOf({
  id: 'plane',
  title: 'The plane the vector space draws',
  description:
    'This is the slot a Lab a*/b* slice or an OkLab plane would have taken; with no conversion member ' +
    'the only plane the library can raster is its own. Four corner Colors — the chooser tint, white, ' +
    'black, and One().Subtract(tint) — are combined per pixel by the chosen Color member: the two edge ' +
    'colours along u, then Lerp down v. Barycentric runs over the tint / white / complement triangle ' +
    'and LinearCombination over weights the spread slider pushes past 1, so both leave the cube; the ' +
    'Bezier and CatmullRom options overshoot through control colours pushed out the same way. Samples ' +
    'with a channel outside 0..1 are stippled, which is the nearest thing here to an out-of-gamut mark.',
  plato: [
    'Color.Lerp',
    'Color.SmoothLerp',
    'Color.LerpClamped',
    'Color.Barycentric',
    'Color.LinearCombination',
    'Color.QuadraticBezier',
    'Color.CubicBezier',
    'Color.CatmullRom',
    'Color.Subtract',
    'Color.Multiply',
    'Color.One',
    'Color.Zero',
  ],
  controls: [
    { key: 'tint', label: 'Corner colour', kind: 'color', def: 0, colorDef: [0.03, 0.85, 0.95] },
    { key: 'blend', label: 'Member', kind: 'select', options: BLEND_LABELS, def: 3 },
    { key: 'resolution', label: 'Resolution', kind: 'slider', min: 32, max: 256, step: 8, def: 128 },
    { key: 'spread', label: 'Control spread', kind: 'slider', min: 0, max: 1.5, step: 0.01, def: 0.6 },
  ],
  build(params: Params): Built {
    const resolution = Math.round(params.resolution);
    const spread = params.spread;
    const tint = tintOf(params, 'tint');
    const white = Color.One();
    const black = Color.Zero().WithA(1);
    const complement = white.Subtract(tint).WithA(1);
    const control = tint.Multiply(1 + spread).WithA(1);
    const control2 = complement.Multiply(1 + spread).WithA(1);
    const kind = Math.round(params.blend);

    // The same shape for every option: the top and bottom edges as a function of
    // u, then Lerp down v. Barycentric and LinearCombination are 2D in their own
    // right, so they take (u, v) whole.
    const edge = (a: Color, b: Color, u: number): Color => {
      switch (kind) {
        case 1:
          return a.SmoothLerp(b, u);
        case 2:
          // Deliberately over-ranged, so the clamp shows as flat plateaus.
          return a.LerpClamped(b, u * 1.6 - 0.3);
        case 5:
          return a.QuadraticBezier(control, b, u);
        case 6:
          return a.CubicBezier(control, control2, b, u);
        case 7:
          return a.CatmullRom(control, b, control2, u);
        default:
          return a.Lerp(b, u);
      }
    };

    let marked = 0;
    let lo = Number.POSITIVE_INFINITY;
    let hi = Number.NEGATIVE_INFINITY;
    let centre = tint;

    const shade = (u: number, v: number): Rgb => {
      const c =
        kind === 3
          ? // Over the triangle tint / white / complement: white + complement -
            // tint is 2 - 2·tint at the far corner, which leaves the cube on any
            // channel below a half. That corner is where the stipple starts.
            tint.Barycentric(white, complement, u, v)
          : kind === 4
            ? // Weights past 1, so the combination is an extrapolation.
              tint.LinearCombination(complement, u * (1 + spread), v * (1 + spread))
            : edge(tint, white, u).Lerp(edge(black, complement, u), v);
      lo = Math.min(lo, c.R, c.G, c.B);
      hi = Math.max(hi, c.R, c.G, c.B);
      if (Math.abs(u - 0.5) < 1 / resolution && Math.abs(v - 0.5) < 1 / resolution) centre = c;
      if (outOfRange(c)) {
        marked++;
        return markOutOfRange(c, u, v, resolution);
      }
      return toRgb(c);
    };

    const object = new THREE.Group();
    object.add(rasterPlane(shade, { resolution }));
    object.add(frame(-1, -1, 1, 1, 0x1b222c));

    const total = resolution * resolution;
    return {
      object,
      readings: [
        note('member', `Color.${BLEND_LABELS[kind]}`),
        note('samples', `${resolution}²`),
        reading('corner tint', () => rgba(tint)),
        reading('One().Subtract(tint)', () => rgba(complement)),
        reading('centre sample', () => rgba(centre)),
        note('channel range', `${n4(lo)} .. ${n4(hi)}`),
        note('outside 0..1', `${((marked / total) * 100).toFixed(1)}% of samples`),
        reading('Color.One().IsOne', () => String(Color.One().IsOne())),
        note('not shown', 'a Lab or OkLab slice — no member converts either way'),
      ],
    };
  },
});

const RAMP_LABELS = [
  'Lerp',
  'LerpClamped',
  'SmoothLerp',
  'QuadraticBezier',
  'CubicBezier',
  'CatmullRom',
  'Hermite',
  'LinearSpace',
  'ColorGradient',
];

const ramps = sceneOf({
  id: 'ramps',
  title: 'One pair of endpoints, every interpolator',
  description:
    'The page brief wanted the same ramp interpolated in sRGB, in Lab and in OkLab side by side. That ' +
    'comparison cannot be built: it needs a round trip through a space, and the library emits no ' +
    'conversion. What it can compare is every interpolator Color itself carries, all of them in ' +
    'linear-light RGBA. Top to bottom: Lerp, LerpClamped (fed an over-ranged t, so it plateaus), ' +
    'SmoothLerp, QuadraticBezier, CubicBezier, CatmullRom, Hermite, LinearSpace (quantised to the step ' +
    'count), and a two-stop ColorGradient sampled by ColorAtParameter — which agrees with Lerp exactly, ' +
    'because that is what GradientSample does between two stops.',
  plato: [
    'Color.Lerp',
    'Color.LerpClamped',
    'Color.SmoothLerp',
    'Color.MidPoint',
    'Color.QuadraticBezier',
    'Color.CubicBezier',
    'Color.CatmullRom',
    'Color.Hermite',
    'Color.LinearSpace',
    'ColorGradient.ColorAtParameter',
  ],
  controls: [
    { key: 'from', label: 'From', kind: 'color', def: 0, colorDef: [0.02, 0.9, 0.95] },
    { key: 'to', label: 'To', kind: 'color', def: 0, colorDef: [0.58, 0.85, 0.95] },
    { key: 'samples', label: 'Samples', kind: 'slider', min: 32, max: 512, step: 8, def: 256 },
    { key: 'steps', label: 'LinearSpace steps', kind: 'slider', min: 2, max: 24, step: 1, def: 8 },
    { key: 'bow', label: 'Control bow', kind: 'slider', min: -1, max: 1, step: 0.01, def: 0.55 },
  ],
  build(params: Params): Built {
    const a = tintOf(params, 'from');
    const b = tintOf(params, 'to');
    const samples = Math.round(params.samples);
    const steps = Math.round(params.steps);
    const bow = params.bow;
    // A control colour bowed off the straight line between the endpoints, so the
    // curve members have something to curve through.
    const control = a.MidPoint(b).Add(Color.One().Subtract(a.MidPoint(b)).Multiply(bow)).WithA(1);
    const ladder = toArray(a.LinearSpace(b, steps));
    const twoStop = new ColorGradient(fromArray([new ColorStop(0, a), new ColorStop(1, b)]));

    const curves: ((t: number) => Color)[] = [
      t => a.Lerp(b, t),
      t => a.LerpClamped(b, t * 1.6 - 0.3),
      t => a.SmoothLerp(b, t),
      t => a.QuadraticBezier(control, b, t),
      t => a.CubicBezier(control, control, b, t),
      t => a.CatmullRom(control, b, control, t),
      t => a.Hermite(b, control, control, t),
      t => ladder[Math.min(ladder.length - 1, Math.floor(t * ladder.length))],
      t => twoStop.ColorAtParameter(t),
    ];

    const object = new THREE.Group();
    const height = 0.16;
    const pitch = 0.2;
    const top = ((curves.length - 1) * pitch) / 2;
    curves.forEach((curve, i) => {
      const strip = rasterStrip(t => toRgb(curve(t)), samples, 2, height);
      strip.position.y = top - i * pitch;
      object.add(strip);
    });
    // A hairline at t = 0.5, where the readings below are taken.
    object.add(tick(0, -top - height, top + height, 0x2f3a4c));

    // Read at a quarter of the way along: the first three agree exactly at the
    // midpoint by construction, and the difference between them is the point.
    const quarter = curves.map(curve => curve(0.25));
    const half = curves.map(curve => curve(0.5));
    return {
      object,
      readings: [
        reading('from', () => rgba(a)),
        reading('to', () => rgba(b)),
        note('strips', `${curves.length} × ${samples} samples`),
        ...RAMP_LABELS.slice(0, 3).map((label, i) =>
          reading(`${label}(0.25)`, () => rgba(quarter[i])),
        ),
        reading('MidPoint == Lerp(0.5)', () => String(a.MidPoint(b).Equals(a.Lerp(b, 0.5)))),
        reading('ColorGradient(0.5) == Lerp(0.5)', () =>
          String(twoStop.ColorAtParameter(0.5).Equals(a.Lerp(b, 0.5))),
        ),
        reading('CubicBezier(0.5)', () => rgba(half[4])),
        note('not shown', 'the same ramp in Lab and OkLab — no conversion member to take it there'),
      ],
    };
  },
});

const gradients = sceneOf({
  id: 'gradient',
  title: 'ColorGradient and the lookup table',
  description:
    'The three members color.library.plato defines for gradients: StopIndexAt finds the stop at or ' +
    'below t by a Reduce over the stop range, GradientSample lerps between that stop and the next, and ' +
    'ColorAtParameter guards the empty case. The stops here are hues taken from the wheel gradient and ' +
    'placed at t^bias, so the spacing is uneven and StopIndexAt has work to do. The lower strip reads a ' +
    'ColorLookupTable built from the same gradient — the type is data-only, so sampling it is an index ' +
    'the demo computes over Colors the members produced.',
  plato: [
    'ColorGradient.StopIndexAt',
    'ColorGradient.GradientSample',
    'ColorGradient.ColorAtParameter',
    'ColorStop.Create',
    'ColorLookupTable.Create',
    'Color.Lerp',
  ],
  controls: [
    { key: 'stops', label: 'Stops', kind: 'slider', min: 2, max: 9, step: 1, def: 5 },
    { key: 'bias', label: 'Stop spacing', kind: 'slider', min: 0.4, max: 2.6, step: 0.01, def: 1.7 },
    { key: 'turn', label: 'Hue start', kind: 'slider', min: 0, max: 1, step: 0.01, def: 0 },
    { key: 'lut', label: 'LUT size', kind: 'slider', min: 2, max: 64, step: 1, def: 12 },
    { key: 'probe', label: 'Probe t', kind: 'slider', min: 0, max: 1, step: 0.001, def: 0.36 },
  ],
  build(params: Params): Built {
    const count = Math.round(params.stops);
    const bias = params.bias;
    const lutSize = Math.round(params.lut);
    const probe = params.probe;

    const positions: number[] = [];
    const stops: ColorStop[] = [];
    for (let i = 0; i < count; i++) {
      const even = i / (count - 1);
      const at = Math.pow(even, bias);
      positions.push(at);
      stops.push(new ColorStop(at, HUE_WHEEL.ColorAtParameter((params.turn + even * 0.8) % 1)));
    }
    const gradient = new ColorGradient(fromArray(stops));

    // The lookup table holds Colors the gradient produced; reading one back is an
    // index over ColorLookupTable.Colors, which is all the type offers.
    const lut = new ColorLookupTable(
      lutSize,
      fromArray(
        Array.from({ length: lutSize }, (_, i) => gradient.ColorAtParameter(i / (lutSize - 1))),
      ),
    );
    const lutAt = (t: number): Color =>
      lut.Colors.At(Math.min(lutSize - 1, Math.max(0, Math.floor(t * lutSize))));

    const object = new THREE.Group();
    const continuous = rasterStrip(t => toRgb(gradient.ColorAtParameter(t)), 512, 2, 0.72);
    continuous.position.y = 0.42;
    const quantised = rasterStrip(t => toRgb(lutAt(t)), Math.max(lutSize, 64), 2, 0.5);
    quantised.position.y = -0.34;
    object.add(continuous, quantised);
    object.add(frame(-1, 0.06, 1, 0.78, 0x1b222c));
    object.add(frame(-1, -0.59, 1, -0.09, 0x1b222c));
    for (const at of positions) {
      object.add(tick(-1 + 2 * at, 0.06, 0.86, palette.line));
    }
    object.add(tick(-1 + 2 * probe, -0.72, 0.94, palette.surfaceAlt));

    const empty = new ColorGradient(fromArray([]));
    return {
      object,
      readings: [
        note('stops', `${gradient.Stops.Count()} at ${positions.map(p => n2(p)).join(', ')}`),
        reading('StopIndexAt(t)', () => String(gradient.StopIndexAt(probe))),
        reading('GradientSample(t)', () => rgba(gradient.GradientSample(probe))),
        reading('ColorAtParameter(t)', () => rgba(gradient.ColorAtParameter(probe))),
        reading('empty ColorAtParameter', () => rgba(empty.ColorAtParameter(probe))),
        note('LUT', `${lut.Size} entries`),
        reading('LUT at t', () => rgba(lutAt(probe))),
        reading('last stop', () => rgba(gradient.Stops.At(count - 1).Color)),
      ],
    };
  },
});

/**
 * Every Color8 constant color.library.plato defines, in declaration order — the
 * order is not a choice: the library defines no luminance, no hue and no
 * ordering for Color8, so there is nothing generated to sort by.
 */
const NAMED: readonly string[] = [
  'AliceBlue', 'AntiqueWhite', 'Aqua', 'Aquamarine', 'Azure', 'Beige', 'Bisque', 'Black',
  'BlanchedAlmond', 'Blue', 'BlueViolet', 'Brown', 'BurlyWood', 'CadetBlue', 'Chartreuse',
  'Chocolate', 'Coral', 'CornflowerBlue', 'Cornsilk', 'Crimson', 'Cyan', 'DarkBlue', 'DarkCyan',
  'DarkGoldenRod', 'DarkGray', 'DarkGreen', 'DarkKhaki', 'DarkMagenta', 'DarkOliveGreen',
  'DarkOrange', 'DarkOrchid', 'DarkRed', 'DarkSalmon', 'DarkSeaGreen', 'DarkSlateBlue',
  'DarkSlateGray', 'DarkTurquoise', 'DarkViolet', 'DeepPink', 'DeepSkyBlue', 'DimGray',
  'DodgerBlue', 'FireBrick', 'FloralWhite', 'ForestGreen', 'Fuchsia', 'Gainsboro', 'GhostWhite',
  'Gold', 'GoldenRod', 'Gray', 'Green', 'GreenYellow', 'HoneyDew', 'HotPink', 'IndianRed',
  'Indigo', 'Ivory', 'Khaki', 'Lavender', 'LavenderBlush', 'LawnGreen', 'LemonChiffon',
  'LightBlue', 'LightCoral', 'LightCyan', 'LightGoldenRodYellow', 'LightGray', 'LightGreen',
  'LightPink', 'LightSalmon', 'LightSeaGreen', 'LightSkyBlue', 'LightSlateGray', 'LightSteelBlue',
  'LightYellow', 'Lime', 'LimeGreen', 'Linen', 'Magenta', 'Maroon', 'MediumAquaMarine',
  'MediumBlue', 'MediumOrchid', 'MediumPurple', 'MediumSeaGreen', 'MediumSlateBlue',
  'MediumSpringGreen', 'MediumTurquoise', 'MediumVioletRed', 'MidnightBlue', 'MintCream',
  'MistyRose', 'Moccasin', 'NavajoWhite', 'Navy', 'OldLace', 'Olive', 'OliveDrab', 'Orange',
  'OrangeRed', 'Orchid', 'PaleGoldenRod', 'PaleGreen', 'PaleTurquoise', 'PaleVioletRed',
  'PapayaWhip', 'PeachPuff', 'Peru', 'Pink', 'Plum', 'PowderBlue', 'Purple', 'RebeccaPurple',
  'Red', 'RosyBrown', 'RoyalBlue', 'SaddleBrown', 'Salmon', 'SandyBrown', 'SeaGreen', 'SeaShell',
  'Sienna', 'Silver', 'SkyBlue', 'SlateBlue', 'SlateGray', 'Snow', 'SpringGreen', 'SteelBlue',
  'Tan', 'Teal', 'Thistle', 'Tomato', 'Turquoise', 'Violet', 'Wheat', 'White', 'WhiteSmoke',
  'Yellow', 'YellowGreen',
];

/** Each name is a static on the generated class; a missing one throws by design. */
function named(name: string): Color8 {
  const fn = (Color8 as unknown as Record<string, unknown>)[name];
  if (typeof fn !== 'function') throw new Error(`Color8.${name} was not emitted`);
  return (fn as () => Color8)();
}

/** The table is fixed, so build it once rather than per parameter tick. */
const NAMED_TABLE: readonly Color8[] = NAMED.map(named);
const DISTINCT_HASHES = new Set(NAMED_TABLE.map(c => c.Hash())).size;
// Fewer than 141 because the table has synonyms — Aqua/Cyan and Fuchsia/Magenta
// are the same bytes. Comparing the two counts is what says whether the shortfall
// is duplicate colours or a hash collision.
const DISTINCT_BYTES = new Set(NAMED_TABLE.map(c => `${c.R},${c.G},${c.B},${c.A}`)).size;

const namedColors = sceneOf({
  id: 'named',
  title: 'The named colours',
  description:
    'Every Color8 constant color.library.plato defines, in declaration order — the library gives Color8 ' +
    'no luminance, no hue and no comparison, so there is no generated key to sort them by. Color8 is ' +
    'byte-valued and, by the type\'s own comment, typically sRGB-encoded, while Color is linear-light; ' +
    'no decode member exists to move between them, which is why these swatches never mix with the ' +
    'Color scenes. Hash and Equals are the only two members the library defines over Color8, and both ' +
    'are read out below.',
  plato: ['Color8.Crimson', 'Color8.RebeccaPurple', 'Color8.Hash', 'Color8.Equals'],
  controls: [
    { key: 'pick', label: 'Swatch', kind: 'slider', min: 0, max: NAMED.length - 1, step: 1, def: 19 },
    { key: 'columns', label: 'Columns', kind: 'slider', min: 6, max: 20, step: 1, def: 14 },
    { key: 'resolution', label: 'Resolution', kind: 'slider', min: 64, max: 256, step: 8, def: 192 },
  ],
  build(params: Params): Built {
    const columns = Math.round(params.columns);
    const rows = Math.ceil(NAMED_TABLE.length / columns);
    const pick = Math.min(NAMED_TABLE.length - 1, Math.round(params.pick));
    const resolution = Math.round(params.resolution);

    const object = new THREE.Group();
    object.add(
      rasterPlane(
        (u, v) => {
          const col = Math.min(columns - 1, Math.floor(u * columns));
          const row = Math.min(rows - 1, Math.floor((1 - v) * rows));
          const swatch = NAMED_TABLE[row * columns + col];
          // Bytes into the 0..1 the rasteriser packs back to bytes: repacking,
          // not a conversion — there is no encode member to consult.
          return swatch
            ? { r: swatch.R / 255, g: swatch.G / 255, b: swatch.B / 255 }
            : { r: 0.06, g: 0.07, b: 0.09 };
        },
        { resolution },
      ),
    );

    const col = pick % columns;
    const row = Math.floor(pick / columns);
    const x0 = -1 + (2 * col) / columns;
    const y0 = 1 - (2 * (row + 1)) / rows;
    object.add(frame(x0, y0, x0 + 2 / columns, y0 + 2 / rows, palette.line));

    const chosen = NAMED_TABLE[pick];
    const neighbour = NAMED_TABLE[(pick + 1) % NAMED_TABLE.length];
    return {
      object,
      readings: [
        note('constants', `${NAMED_TABLE.length} in ${columns} × ${rows}`),
        note('picked', `Color8.${NAMED[pick]}`),
        reading('bytes', () => bytes(chosen)),
        reading('Hash', () => String(chosen.Hash())),
        reading('Equals next', () => `${NAMED[(pick + 1) % NAMED.length]} ${chosen.Equals(neighbour)}`),
        note(
          'distinct',
          `${DISTINCT_HASHES} hashes over ${DISTINCT_BYTES} distinct byte values` +
            `${DISTINCT_HASHES === DISTINCT_BYTES ? ' — no collisions' : ' — Hash collides'}`,
        ),
        note('order', 'declaration order — no generated luminance or hue to sort by'),
        note('Color8 → Color', 'UNAVAILABLE (no decode member; the bytes above are shown raw)'),
      ],
    };
  },
});

const xyzSpace = sceneOf({
  id: 'xyz',
  title: 'XYZ, a vector space with no way home',
  description:
    'color-spaces.library.plato defines twelve members and they are all here: ColorXYZ.Add, Subtract, ' +
    'Negative, Lerp, Multiply, Divide, Modulo, Zero, One, MinValue, MaxValue, and Chromaticity.Hash. ' +
    'That is enough to interpolate in CIE XYZ — the polyline is CatmullRom through four tristimulus ' +
    'values, with LinearSpace marking a straight Lerp beside it — but not to show the result as a ' +
    'colour, because no member takes a ColorXYZ to a Color. The axes are X, Y and Z; the geometry is ' +
    'the tristimulus values plotted as coordinates, which is the only honest picture of them here. The ' +
    'CIE xy chromaticity diagram the page wanted belongs in this slot and is not buildable: Chromaticity ' +
    'carries Hash and nothing else, and ColorXyY has no members at all.',
  plato: [
    'ColorXYZ.Lerp',
    'ColorXYZ.CatmullRom',
    'ColorXYZ.LinearSpace',
    'ColorXYZ.Barycentric',
    'ColorXYZ.Add',
    'ColorXYZ.Multiply',
    'ColorXYZ.One',
    'ColorXYZ.Zero',
    'Chromaticity.Hash',
  ],
  viewer: { orthographic: false, grid: true, spin: true, distance: 3.4 },
  controls: [
    { key: 'samples', label: 'Samples', kind: 'slider', min: 8, max: 200, step: 1, def: 96 },
    { key: 'probe', label: 'Probe t', kind: 'slider', min: 0, max: 1, step: 0.001, def: 0.4 },
    { key: 'spread', label: 'Spread', kind: 'slider', min: 0.2, max: 1.4, step: 0.01, def: 0.85 },
  ],
  build(params: Params): Built {
    const samples = Math.round(params.samples);
    const spread = params.spread;
    const probe = params.probe;

    // Four tristimulus values to run a spline through; building the inputs is
    // demo work, every step between them is a generated member.
    const p0 = ColorXYZ.Zero();
    const p1 = ColorXYZ.One().Multiply(spread);
    const p2 = new ColorXYZ(0.95, 0.2, 0.25).Multiply(spread);
    const p3 = new ColorXYZ(0.18, 0.9, 1.05).Multiply(spread);

    const curve: Point3D[] = [];
    for (let i = 0; i < samples; i++) {
      const t = i / (samples - 1);
      const c = p0.CatmullRom(p1, p2, p3, t);
      curve.push(new Point3D(c.X - 0.5, c.Y - 0.5, c.Z - 0.5));
    }
    const straight = toArray(p1.LinearSpace(p3, Math.max(2, Math.round(samples / 6))));

    const object = new THREE.Group();
    object.add(
      new THREE.Line(polylineGeometry(curve), new THREE.LineBasicMaterial({ color: palette.line })),
    );
    object.add(
      new THREE.Line(
        polylineGeometry(straight.map(c => new Point3D(c.X - 0.5, c.Y - 0.5, c.Z - 0.5))),
        new THREE.LineBasicMaterial({ color: palette.surfaceAlt }),
      ),
    );

    const marks: number[] = [];
    for (const c of straight) marks.push(c.X - 0.5, c.Y - 0.5, c.Z - 0.5);
    const markGeometry = new THREE.BufferGeometry();
    markGeometry.setAttribute('position', new THREE.Float32BufferAttribute(marks, 3));
    object.add(
      new THREE.Points(
        markGeometry,
        new THREE.PointsMaterial({ color: palette.accent, size: 6, sizeAttenuation: false }),
      ),
    );

    // X, Y and Z axes from the origin of the plotted cube.
    object.add(
      segments(
        [
          -0.5, -0.5, -0.5, 0.9, -0.5, -0.5,
          -0.5, -0.5, -0.5, -0.5, 0.9, -0.5,
          -0.5, -0.5, -0.5, -0.5, -0.5, 0.9,
        ],
        0x3f4c63,
      ),
    );

    const at = p0.CatmullRom(p1, p2, p3, probe);
    const white = new Chromaticity(0.3127, 0.329);
    return {
      object,
      readings: [
        note('samples', String(samples)),
        reading('CatmullRom(t)', () => xyz(at)),
        reading('Lerp(t)', () => xyz(p1.Lerp(p3, probe))),
        reading('Barycentric', () => xyz(p1.Barycentric(p2, p3, probe, 1 - probe))),
        reading('LinearSpace', () => `${straight.length} values`),
        reading('One().Multiply', () => xyz(ColorXYZ.One().Multiply(spread))),
        reading('Chromaticity.Hash', () => String(white.Hash())),
        reading('ColorXYZ.ToColor', () => String(callByName(at, 'ToColor'))),
        note('not shown', 'the CIE xy diagram — ColorXyY and Chromaticity carry no conversion'),
      ],
    };
  },
});

const demo: Demo = {
  title: 'Colour spaces',
  subtitle: 'color.{types,library}.plato · color-spaces.{types,library}.plato',
  scenes: [chooser, plane, ramps, gradients, namedColors, xyzSpace],
};

// The planar scenes want the flat, grid-free camera; the XYZ scene overrides it.
mountDemo(demo, { orthographic: true, grid: false, spin: false });

export { demo };
