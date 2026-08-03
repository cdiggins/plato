// Types for the vendored Gratify build (`vendor/gratify/**`), which ships as
// plain ESM with no declarations. This shim covers exactly the surface the demo
// control panel uses — the value types, the three parts we compose with, the
// part builder, the interactors, and `mount`. It is deliberately not a full
// description of the framework; widen it when a panel needs more.
//
// It sits beside the vendored `index.js` so TypeScript picks it up as that
// module's types: Vite resolves the bare specifier `gratify` through the alias in
// `vite.config.ts`, tsc and tsx through the `paths` entry in `tsconfig.json`,
// and both land on `vendor/gratify/index.js`.

export interface Vec {
  x: number;
  y: number;
}
export function v(x?: number, y?: number): Vec;

export class Rect {
  constructor(x: number, y: number, w: number, h: number);
  readonly x: number;
  readonly y: number;
  readonly w: number;
  readonly h: number;
  readonly right: number;
  readonly bottom: number;
  readonly center: Vec;
  inset(n: number): Rect;
  raise(n: number): Rect;
}
export function rect(x?: number, y?: number, w?: number, h?: number): Rect;

/** Gratify colours are plain 0..255 RGB with a 0..1 alpha. */
export interface Paint {
  r: number;
  g: number;
  b: number;
  a: number;
}
export function rgb(r: number, g: number, b: number, a?: number): Paint;
export function calpha(c: Paint, a: number): Paint;
export function cmix(a: Paint, b: Paint, t: number): Paint;
export function hsl(h: number, s: number, l: number, a?: number): Paint;
export function clamp(x: number, lo: number, hi: number): number;
export function lerp(a: number, b: number, t: number): number;

/** The live theme tokens, plus the mixer every style facet reaches for. */
export interface Theme {
  bg: Paint;
  surface: Paint;
  surfaceHi: Paint;
  muted: Paint;
  text: Paint;
  textDim: Paint;
  textBright: Paint;
  accent: Paint;
  accent2: Paint;
  danger: Paint;
  mix(a: Paint, b: Paint, t: number): Paint;
}
export const tokens: Theme;
export function setTheme(name: 'dark' | 'light'): void;

export interface TextOptions {
  size?: number;
  weight?: number | string;
  align?: 'left' | 'center' | 'right';
}

/** The drawing contract: rounded boxes, text, lines, dots, and a glow scope. */
export interface Painter {
  box(r: Rect, corner: number, fill: Paint, stroke?: Paint, lw?: number): void;
  label(text: string, at: Vec, color: Paint, o?: TextOptions): void;
  line(a: Vec, b: Vec, color: Paint, lw?: number): void;
  dot(p: Vec, r: number, color: Paint): void;
  glow(color: Paint, blur: number, draw: () => void): void;
}

/** What a `size` facet is handed to measure its own content. */
export interface Measurer {
  text(s: string, size?: number): Vec;
  children(avail: Vec): Vec[];
}

export interface Node<P = Record<string, unknown>> {
  readonly rect: Rect;
  readonly props: P;
  /** Animated channels, eased toward their targets each frame. */
  readonly ch: Record<string, number>;
}

export interface Element {
  readonly key: string;
}
export type Intent = unknown;

/** A part's element constructor. */
export type Ctor<P = Record<string, unknown>> = (
  key: string,
  props?: P,
  children?: Element[],
) => Element;

export interface Interactor {
  readonly kind: string;
}
export function Press<P>(to: (node: Node<P>) => Intent): Interactor;
export function Hover(): Interactor;
export function Drag1D<P>(o: {
  axis: 'x' | 'y';
  pad?: number;
  to: (node: Node<P>, fraction: number) => Intent;
}): Interactor;
export function Gesture<P, S>(spec: {
  begin(node: Node<P>, p: Vec): S | null;
  move?(state: S, node: Node<P>, p: Vec): S;
  during?(state: S, node: Node<P>, p: Vec): Intent;
  up?(state: S, node: Node<P>, p: Vec): Intent;
}): Interactor;
export function axisFraction(
  r: Rect,
  axis: 'x' | 'y',
  pad: number,
  px: number,
  py: number,
): number;

export interface ChannelSpec<P> {
  target(node: Node<P>): number;
  spring?: { stiffness: number; damping: number };
}

/** The facets a part declares. `S` is whatever its `style` facet returns. */
export interface PartSpec<P, S> {
  size?(props: P, m: Measurer): Vec;
  channels?: Record<string, ChannelSpec<P>>;
  style?(t: Theme, ch: Record<string, number>, props: P): S;
  render?(node: Node<P>, p: Painter, s: S): void;
  body?(props: P, children: Element[]): Element[];
  on?: Interactor[];
}

/** `part()` curried by a props type, then named: `part<P>()('slider', spec)`. */
export function part<P = Record<string, unknown>>(): <S>(
  name: string,
  spec: PartSpec<P, S>,
) => Ctor<P>;

export const Stack: Ctor<{ gap?: number; pad?: number; align?: 'start' | 'center' | 'end' | 'stretch' }>;
export const Row: Ctor<{
  gap?: number;
  pad?: number;
  align?: 'start' | 'center' | 'end' | 'stretch';
  justify?: 'start' | 'between';
}>;
export const Flow: Ctor<{ gap?: number; pad?: number }>;
export const Layers: Ctor<Record<string, never>>;
export const Label: Ctor<{
  text: string;
  size?: number;
  weight?: number;
  dim?: boolean;
  bright?: boolean;
}>;

export interface Runtime<Doc> {
  readonly doc: Doc;
  dispatch(intent: Intent): void;
  stop(): void;
}
export function mount<Doc>(
  canvas: HTMLCanvasElement,
  app: {
    init: Doc;
    update(doc: Doc, intent: Intent): Doc;
    view(doc: Doc): Element;
  },
): Runtime<Doc>;
