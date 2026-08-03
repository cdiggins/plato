// The Gratify parts the demo sidebar is built from.
//
// Each control is ONE part that draws its own label, its own readout and its own
// track: a row is a fixed-height box whose whole layout is arithmetic inside
// `render`, rather than a nested Row/Label tree. That keeps hit testing honest
// (the part that owns the pixels is the part that owns the interactor) and keeps
// the panel's measured height a simple sum the mount code can compute up front.
//
// Nothing here imports the generated Plato types. The panel is chrome: it deals
// in normalized 0..1 numbers, and the demo pages map those onto stdlib values.
// The colour arithmetic below is the swatch's own presentation — the colour
// SPACES demo converts through the generated members instead.

import {
  Drag1D,
  Gesture,
  Press,
  Stack,
  clamp,
  part,
  rect,
  rgb,
  v,
  type Ctor,
  type Element,
  type Intent,
  type Paint,
  type Vec,
} from '../../vendor/gratify/index.js';

/**
 * The one intent the panel emits. It carries a whole assignment rather than a
 * single key because an interactor may only return one intent per event, and
 * the colour pad moves two parameters at once.
 */
export interface SetParams {
  kind: 'set';
  values: Record<string, number>;
}

export const setParam = (key: string, value: number): SetParams => ({
  kind: 'set',
  values: { [key]: value },
});

export const setParams = (values: Record<string, number>): SetParams => ({ kind: 'set', values });

// ---------------------------------------------------------------------------
// Row metrics — the mount code sizes the canvas from these, so they live here.

export const ROW_GAP = 6;
export const SLIDER_HEIGHT = 40;
export const TOGGLE_HEIGHT = 26;
export const SEGMENT_HEIGHT = 24;
export const SEGMENT_LABEL = 18;
export const COLOR_PAD = 84;
export const COLOR_STRIP = 14;
export const COLOR_HEIGHT = SEGMENT_LABEL + COLOR_PAD + 6 + COLOR_STRIP + 4;

// ---------------------------------------------------------------------------
// Slider

interface SliderProps {
  label: string;
  /** Preformatted readout — the panel never guesses a number's precision. */
  readout: string;
  /** Position in 0..1; the demo's real range is the caller's business. */
  fraction: number;
  width: number;
  paramKey: string;
}

const TRACK_PAD = 2;

export const SliderRow: Ctor<SliderProps> = part<SliderProps>()('demo-slider', {
  size: p => v(p.width, SLIDER_HEIGHT),
  channels: {
    shown: { target: n => n.props.fraction, spring: { stiffness: 320, damping: 26 } },
  },
  style: (t, ch) => ({
    label: t.mix(t.textDim, t.text, ch.hover),
    readout: t.mix(t.text, t.textBright, ch.hover),
    track: t.mix(t.muted, t.surfaceHi, 0.4),
    fill: t.accent,
    knob: t.mix(t.textBright, t.accent, 0.25 * ch.hover),
    knobR: 6 + 1.5 * ch.hover + 1 * ch.press,
    glow: 9 * ch.hover,
  }),
  render(node, p, s) {
    const r = node.rect;
    p.label(node.props.label, v(r.x, r.y + 8), s.label, { size: 11, align: 'left' });
    p.label(node.props.readout, v(r.right, r.y + 8), s.readout, { size: 11, align: 'right' });

    const x = r.x + TRACK_PAD;
    const w = r.w - 2 * TRACK_PAD;
    const y = r.y + 27;
    const t = clamp(node.ch.shown, 0, 1);
    p.box(rect(x, y - 2.5, w, 5), 2.5, s.track);
    p.box(rect(x, y - 2.5, w * t, 5), 2.5, s.fill);
    p.glow(s.fill, s.glow, () => p.dot(v(x + w * t, y), s.knobR, s.knob));
  },
  on: [
    Drag1D<SliderProps>({
      axis: 'x',
      pad: TRACK_PAD,
      to: (node, f) => setParam(node.props.paramKey, f),
    }),
  ],
});

// ---------------------------------------------------------------------------
// Toggle

interface ToggleProps {
  label: string;
  on: boolean;
  width: number;
  paramKey: string;
}

export const ToggleRow: Ctor<ToggleProps> = part<ToggleProps>()('demo-toggle', {
  size: p => v(p.width, TOGGLE_HEIGHT),
  channels: {
    on: { target: n => (n.props.on ? 1 : 0), spring: { stiffness: 300, damping: 22 } },
  },
  style: (t, ch) => {
    const on = clamp(ch.on, 0, 1);
    return {
      label: t.mix(t.mix(t.textDim, t.text, ch.hover), t.textBright, on * 0.5),
      track: t.mix(t.mix(t.muted, t.surfaceHi, 0.3), t.accent, on),
      knob: t.textBright,
      travel: ch.on,
      glow: 8 * ch.hover,
    };
  },
  render(node, p, s) {
    const r = node.rect;
    p.label(node.props.label, v(r.x, r.center.y), s.label, { size: 11, align: 'left' });
    const track = rect(r.right - 34, r.center.y - 9, 34, 18);
    p.box(track, 9, s.track);
    const knobX = track.x + 9 + clamp(s.travel, 0, 1) * (track.w - 18);
    p.glow(s.track, s.glow, () => p.dot(v(knobX, track.center.y), 6.5, s.knob));
  },
  on: [Press<ToggleProps>(node => setParam(node.props.paramKey, node.props.on ? 0 : 1))],
});

// ---------------------------------------------------------------------------
// Segmented select
//
// One part, `options.length` cells, hit tested from the pointer: `Press` reports
// only which part was pressed, so the cell under the finger comes from a Gesture
// that resolves the index on `up`.

interface SegmentProps {
  label: string;
  options: string[];
  index: number;
  width: number;
  paramKey: string;
}

/** Cells wrap into rows of at most three, so long option names stay readable. */
function segmentRows(count: number): number {
  return Math.ceil(count / (count <= 4 ? 2 : 3));
}

export function segmentHeight(count: number): number {
  return SEGMENT_LABEL + segmentRows(count) * SEGMENT_HEIGHT + (segmentRows(count) - 1) * 3;
}

function cellRect(r: { x: number; y: number; w: number }, count: number, i: number) {
  const perRow = count <= 4 ? 2 : 3;
  const row = Math.floor(i / perRow);
  const column = i % perRow;
  const cellW = (r.w - 3 * (perRow - 1)) / perRow;
  return rect(
    r.x + column * (cellW + 3),
    r.y + SEGMENT_LABEL + row * (SEGMENT_HEIGHT + 3),
    cellW,
    SEGMENT_HEIGHT,
  );
}

function cellAt(node: { rect: { x: number; y: number; w: number } }, count: number, p: Vec): number {
  for (let i = 0; i < count; i++) {
    const c = cellRect(node.rect, count, i);
    if (p.x >= c.x && p.x <= c.right && p.y >= c.y && p.y <= c.bottom) return i;
  }
  return -1;
}

export const SegmentRow: Ctor<SegmentProps> = part<SegmentProps>()('demo-segment', {
  size: p => v(p.width, segmentHeight(p.options.length)),
  style: t => ({
    label: t.textDim,
    on: t.mix(t.surface, t.accent, 0.75),
    off: t.mix(t.surface, t.surfaceHi, 0.5),
    edge: t.muted,
    onText: t.textBright,
    offText: t.text,
  }),
  render(node, p, s) {
    const { label, options, index } = node.props;
    p.label(label, v(node.rect.x, node.rect.y + 7), s.label, { size: 11, align: 'left' });
    options.forEach((option, i) => {
      const c = cellRect(node.rect, options.length, i);
      const chosen = i === index;
      p.box(c, 6, chosen ? s.on : s.off, chosen ? undefined : s.edge, 1);
      p.label(fitted(option, c.w), c.center, chosen ? s.onText : s.offText, {
        size: 11,
        weight: chosen ? 600 : 400,
      });
    });
  },
  on: [
    Gesture<SegmentProps, number>({
      begin: (node, p) => cellAt(node, node.props.options.length, p),
      move: (_state, node, p) => cellAt(node, node.props.options.length, p),
      up: (state, node) =>
        state >= 0 && state !== node.props.index ? setParam(node.props.paramKey, state) : null,
    }),
  ],
});

/** Trim an option name to what its cell can hold — canvas text does not wrap. */
function fitted(text: string, width: number): string {
  const fits = Math.max(1, Math.floor((width - 8) / 5.6));
  return text.length <= fits ? text : `${text.slice(0, Math.max(1, fits - 1))}…`;
}

// ---------------------------------------------------------------------------
// Colour field: a saturation/value pad over a hue strip.
//
// Writes three parameters — `<key>H`, `<key>S`, `<key>V`, each 0..1 — because
// the panel's parameter bag is flat numbers. The demo page is what turns them
// into a Plato colour.

interface ColorProps {
  label: string;
  h: number;
  s: number;
  v: number;
  width: number;
  paramKey: string;
}

/** HSV → Gratify paint, for drawing the picker itself. */
function hsv(h: number, s: number, value: number): Paint {
  const i = Math.floor(h * 6) % 6;
  const f = h * 6 - Math.floor(h * 6);
  const p = value * (1 - s);
  const q = value * (1 - f * s);
  const t = value * (1 - (1 - f) * s);
  const [r, g, b] =
    i === 0 ? [value, t, p]
    : i === 1 ? [q, value, p]
    : i === 2 ? [p, value, t]
    : i === 3 ? [p, q, value]
    : i === 4 ? [t, p, value]
    : [value, p, q];
  return rgb(r * 255, g * 255, b * 255);
}

const PAD_COLUMNS = 18;
const PAD_ROWS = 12;
const STRIP_CELLS = 36;

type ColorZone = 'pad' | 'strip' | 'none';

function padRect(r: { x: number; y: number; w: number }) {
  return rect(r.x, r.y + SEGMENT_LABEL, r.w, COLOR_PAD);
}

function stripRect(r: { x: number; y: number; w: number }) {
  return rect(r.x, r.y + SEGMENT_LABEL + COLOR_PAD + 6, r.w, COLOR_STRIP);
}

function zoneAt(node: { rect: { x: number; y: number; w: number } }, p: Vec): ColorZone {
  const pad = padRect(node.rect);
  if (p.y >= pad.y - 4 && p.y <= pad.bottom + 3) return 'pad';
  const strip = stripRect(node.rect);
  if (p.y >= strip.y - 3 && p.y <= strip.bottom + 4) return 'strip';
  return 'none';
}

export const ColorRow: Ctor<ColorProps> = part<ColorProps>()('demo-color', {
  size: p => v(p.width, COLOR_HEIGHT),
  style: t => ({
    label: t.textDim,
    ring: t.textBright,
    shadow: rgb(0, 0, 0, 0.55),
    edge: t.muted,
  }),
  render(node, p, s) {
    const props = node.props;
    p.label(props.label, v(node.rect.x, node.rect.y + 7), s.label, { size: 11, align: 'left' });

    // The painter draws boxes, not gradients, so the ramps are cell grids.
    const pad = padRect(node.rect);
    const cw = pad.w / PAD_COLUMNS;
    const chh = pad.h / PAD_ROWS;
    for (let j = 0; j < PAD_ROWS; j++) {
      for (let i = 0; i < PAD_COLUMNS; i++) {
        const sat = (i + 0.5) / PAD_COLUMNS;
        const val = 1 - (j + 0.5) / PAD_ROWS;
        p.box(rect(pad.x + i * cw, pad.y + j * chh, cw + 0.7, chh + 0.7), 0, hsv(props.h, sat, val));
      }
    }
    p.box(pad, 4, rgb(0, 0, 0, 0), s.edge, 1);

    const strip = stripRect(node.rect);
    const sw = strip.w / STRIP_CELLS;
    for (let i = 0; i < STRIP_CELLS; i++) {
      p.box(rect(strip.x + i * sw, strip.y, sw + 0.7, strip.h), 0, hsv((i + 0.5) / STRIP_CELLS, 1, 1));
    }
    p.box(strip, 4, rgb(0, 0, 0, 0), s.edge, 1);

    // Cursors: a dark ring under a bright one, so both ends of the ramp read.
    const cx = pad.x + clamp(props.s, 0, 1) * pad.w;
    const cy = pad.y + (1 - clamp(props.v, 0, 1)) * pad.h;
    p.dot(v(cx, cy), 5.5, s.shadow);
    p.dot(v(cx, cy), 3.5, s.ring);
    const hx = strip.x + clamp(props.h, 0, 1) * strip.w;
    p.line(v(hx, strip.y - 2), v(hx, strip.bottom + 2), s.shadow, 4);
    p.line(v(hx, strip.y - 2), v(hx, strip.bottom + 2), s.ring, 2);
  },
  on: [
    Gesture<ColorProps, ColorZone>({
      begin: (node, p) => {
        const zone = zoneAt(node, p);
        return zone === 'none' ? null : zone;
      },
      during: (zone, node, p) => {
        const key = node.props.paramKey;
        if (zone === 'strip') {
          const strip = stripRect(node.rect);
          return setParam(`${key}H`, clamp((p.x - strip.x) / strip.w, 0, 1));
        }
        const pad = padRect(node.rect);
        return setParams({
          [`${key}S`]: clamp((p.x - pad.x) / pad.w, 0, 1),
          [`${key}V`]: clamp(1 - (p.y - pad.y) / pad.h, 0, 1),
        });
      },
    }),
  ],
});

// ---------------------------------------------------------------------------
// Button

export const BUTTON_HEIGHT = 26;

interface ButtonProps {
  label: string;
  width: number;
  intent: Intent;
}

export const ButtonRow: Ctor<ButtonProps> = part<ButtonProps>()('demo-button', {
  size: p => v(p.width, BUTTON_HEIGHT),
  style: (t, ch) => ({
    fill: t.mix(t.surface, t.surfaceHi, 0.3 + 0.4 * ch.hover + 0.3 * ch.press),
    edge: t.mix(t.muted, t.accent, ch.hover),
    text: t.mix(t.textDim, t.textBright, ch.hover),
  }),
  render(node, p, s) {
    p.box(node.rect, 6, s.fill, s.edge, 1);
    p.label(node.props.label, node.rect.center, s.text, { size: 11 });
  },
  on: [Press<ButtonProps>(node => node.props.intent)],
});

// ---------------------------------------------------------------------------
// Panel

export const Panel = (children: Element[]): Element =>
  Stack('panel', { gap: ROW_GAP, pad: 0, align: 'stretch' }, children);

export type { Intent };
