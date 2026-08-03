// The page shell every demo shares: sidebar with the scene list, parameter
// controls, and a panel naming the generated members the scene calls.
//
// The controls are a Gratify app on a canvas rather than DOM inputs. Gratify's
// loop is its own — an Elm-shaped (init, update, view) triple whose `update`
// hands us a new parameter bag — so the seam is small: the panel owns the
// numbers, and a change schedules exactly one rebuild per animation frame,
// however fast the pointer moves.

import type * as THREE from 'three';
import { mount, type Element, type Intent, type Runtime } from '../../vendor/gratify/index.js';
import { Viewer, type ViewerOptions } from './viewer.js';
import { colorKeys, type Control, type Demo, type Params, type Scene } from './demo.js';
import {
  BUTTON_HEIGHT,
  ButtonRow,
  COLOR_HEIGHT,
  ColorRow,
  Panel,
  ROW_GAP,
  SLIDER_HEIGHT,
  SegmentRow,
  SliderRow,
  TOGGLE_HEIGHT,
  ToggleRow,
  segmentHeight,
  type SetParams,
} from './widgets.js';

export function mountDemo(demo: Demo, viewerOptions: ViewerOptions = {}): void {
  // Every demo module calls this at import time, which is also how
  // `scripts/scenes.mts` gets at the catalog: off a page there is nothing to
  // mount, and the script drives `build` itself. The script's headless stub has
  // no `body`, which is the difference between "no page" and "a real one" —
  // a module script is deferred, so a real document always has one by now.
  if (typeof document === 'undefined' || !document.body) return;
  document.title = `${demo.title} — Plato`;

  const app = document.createElement('div');
  app.id = 'app';
  app.innerHTML = `
    <aside id="sidebar">
      <header>
        <a class="back" href="./index.html">&larr; Plato demos</a>
        <h1>${demo.title}</h1>
        <p class="subtitle">${demo.subtitle}</p>
      </header>
      <nav id="scene-list"></nav>
      <section id="controls"></section>
      <section id="about">
        <h2>Generated members</h2>
        <ul id="plato-members"></ul>
        <p id="scene-description"></p>
        <p id="scene-status"></p>
      </section>
    </aside>
    <main id="stage"></main>
  `;
  document.body.appendChild(app);

  const stage = app.querySelector('#stage') as HTMLElement;
  const list = app.querySelector('#scene-list') as HTMLElement;
  const controlPanel = app.querySelector('#controls') as HTMLElement;
  const members = app.querySelector('#plato-members') as HTMLElement;
  const description = app.querySelector('#scene-description') as HTMLElement;
  const status = app.querySelector('#scene-status') as HTMLElement;

  let viewer = new Viewer(stage, viewerOptions);
  let viewerKey = JSON.stringify(viewerOptions);
  let current: Scene = demo.scenes[0];
  let params: Params = {};
  let panel: Runtime<Params> | null = null;
  let pending = 0;
  // The simulation seam. `live` is what the last successful build returned, and
  // the driver below only runs while the selected scene has a `tick`, so the
  // eleven static pages cost nothing and see no behaviour change.
  let live: THREE.Object3D | null = null;
  let ticking = 0;
  let lastFrame = 0;

  /** A scene's viewer settings win over the page's; a change rebuilds the stage. */
  function useViewerFor(scene: Scene): void {
    const merged = { ...viewerOptions, ...(scene.viewer ?? {}) };
    const key = JSON.stringify(merged);
    if (key === viewerKey) return;
    viewer.dispose();
    stage.replaceChildren();
    viewer = new Viewer(stage, merged);
    viewerKey = key;
  }

  function rebuild(): void {
    try {
      const built = current.build(params);
      viewer.show(built);
      live = built;
      stage.classList.remove('errored');
      status.textContent = current.status?.(params) ?? '';
    } catch (error) {
      live = null;
      stage.classList.add('errored');
      console.error(error);
      status.textContent = `Build failed: ${(error as Error).message}`;
    }
  }

  /**
   * One frame of a simulation scene. A throw here would otherwise repeat sixty
   * times a second, so the first one stops the driver and leaves the message on
   * the status line — the same bargain `rebuild` makes.
   */
  const advance = (now: number): void => {
    ticking = requestAnimationFrame(advance);
    const seconds = lastFrame ? Math.min((now - lastFrame) / 1000, 1 / 20) : 0;
    lastFrame = now;
    if (!live || seconds === 0) return;
    try {
      const line = current.tick?.(seconds, params, live);
      if (typeof line === 'string') status.textContent = line;
    } catch (error) {
      stopTicking();
      stage.classList.add('errored');
      console.error(error);
      status.textContent = `Step failed: ${(error as Error).message}`;
    }
  };

  function stopTicking(): void {
    if (!ticking) return;
    cancelAnimationFrame(ticking);
    ticking = 0;
    lastFrame = 0;
  }

  /**
   * A drag emits an intent per pointer event; the scene only needs the value the
   * frame ends on. Coalescing here is what lets a build that costs a few
   * milliseconds sit behind a slider without the panel going sticky.
   */
  function rebuildSoon(): void {
    if (pending) return;
    pending = requestAnimationFrame(() => {
      pending = 0;
      rebuild();
    });
  }

  function selectScene(scene: Scene): void {
    stopTicking();
    current = scene;
    useViewerFor(scene);
    params = defaultParams(scene.controls ?? []);
    description.textContent = scene.description;
    members.innerHTML = scene.plato.map(m => `<li><code>${m}</code></li>`).join('');
    mountPanel(scene);
    for (const button of list.querySelectorAll('button')) {
      button.classList.toggle('active', button.dataset.id === scene.id);
    }
    rebuild();
    // A rebuild re-seeds `live`, so a parameter change during a simulation
    // restarts it rather than needing the driver restarted with it.
    if (scene.tick) advance(performance.now());
  }

  /**
   * One Gratify runtime per scene: the control set is the app's shape, so a new
   * scene gets a new canvas rather than a reconciled one.
   */
  function mountPanel(scene: Scene): void {
    panel?.stop();
    panel = null;
    controlPanel.replaceChildren();
    const controls = scene.controls ?? [];
    if (controls.length === 0) return;

    const canvas = document.createElement('canvas');
    canvas.className = 'control-canvas';
    canvas.style.height = `${panelHeight(controls)}px`;
    controlPanel.appendChild(canvas);
    // Gratify measures from the canvas's CSS box; the sidebar's padding is what
    // sets it, so read it back rather than hard-coding a width.
    const width = (): number => canvas.clientWidth || 268;

    const initial = defaultParams(controls);
    const byKey = new Map(controls.map(c => [c.key, c]));
    panel = mount<Params>(canvas, {
      init: initial,
      update(doc: Params, intent: Intent): Params {
        const next = applyIntent(doc, initial, byKey, intent);
        if (next !== doc && !isResize(intent)) {
          params = next;
          rebuildSoon();
        }
        return next;
      },
      view: (doc: Params): Element => Panel(rows(controls, doc, width())),
    });

    // A widget's width is a prop, read when the view is built, and the view is
    // only rebuilt when the document changes. The sidebar's scrollbar coming and
    // going is what moves that width, so a resize asks for a fresh view.
    let observed = width();
    new ResizeObserver(() => {
      if (canvas.clientWidth === observed || canvas.clientWidth === 0) return;
      observed = canvas.clientWidth;
      panel?.dispatch({ kind: 'resize' });
    }).observe(canvas);
  }

  for (const scene of demo.scenes) {
    const button = document.createElement('button');
    button.dataset.id = scene.id;
    button.textContent = scene.title;
    button.addEventListener('click', () => selectScene(scene));
    list.appendChild(button);
  }

  selectScene(demo.scenes[0]);
}

// ---------------------------------------------------------------------------
// Controls → parameters

function defaultParams(controls: Control[]): Params {
  const out: Params = {};
  for (const control of controls) {
    if (control.kind === 'color') {
      const [h, s, v] = control.colorDef ?? [0.58, 0.7, 0.9];
      const [hk, sk, vk] = colorKeys(control.key);
      out[hk] = h;
      out[sk] = s;
      out[vk] = v;
    } else {
      out[control.key] = control.def;
    }
  }
  return out;
}

const isResize = (intent: Intent): boolean => (intent as { kind?: string } | null)?.kind === 'resize';

/**
 * A slider reports where its knob landed, 0..1; every other widget reports the
 * value itself. The control is what knows the range and the step, so the
 * conversion happens here, on the way into the parameter bag.
 */
function applyIntent(
  doc: Params,
  initial: Params,
  byKey: Map<string, Control>,
  intent: Intent,
): Params {
  const tagged = intent as { kind?: string } | null;
  if (!tagged) return doc;
  if (tagged.kind === 'reset') return { ...initial };
  // A new object with the same numbers: the view runs again at the new width,
  // and the caller's identity check sees no parameter change to rebuild for.
  if (tagged.kind === 'resize') return { ...doc };
  if (tagged.kind !== 'set') return doc;
  const { values } = intent as SetParams;
  let changed = false;
  const next = { ...doc };
  for (const key in values) {
    const control = byKey.get(key);
    const value = control?.kind === 'slider' ? fromFraction(control, values[key]) : values[key];
    if (next[key] !== value) {
      next[key] = value;
      changed = true;
    }
  }
  return changed ? next : doc;
}

// ---------------------------------------------------------------------------
// Controls → widgets

function rowHeight(control: Control): number {
  switch (control.kind) {
    case 'toggle':
      return TOGGLE_HEIGHT;
    case 'select':
      return segmentHeight((control.options ?? []).length);
    case 'color':
      return COLOR_HEIGHT;
    default:
      return SLIDER_HEIGHT;
  }
}

function panelHeight(controls: Control[]): number {
  const rows = controls.reduce((total, c) => total + rowHeight(c), 0);
  return rows + ROW_GAP * controls.length + BUTTON_HEIGHT;
}

function rows(controls: Control[], doc: Params, width: number): Element[] {
  const out = controls.map(control => row(control, doc, width));
  out.push(ButtonRow('reset', { label: 'Reset parameters', width, intent: { kind: 'reset' } }));
  return out;
}

function row(control: Control, doc: Params, width: number): Element {
  const { key, label } = control;
  if (control.kind === 'toggle') {
    return ToggleRow(key, { label, on: doc[key] !== 0, width, paramKey: key });
  }
  if (control.kind === 'select') {
    const options = control.options ?? [];
    return SegmentRow(key, { label, options, index: Math.round(doc[key]), width, paramKey: key });
  }
  if (control.kind === 'color') {
    const [hk, sk, vk] = colorKeys(key);
    return ColorRow(key, { label, h: doc[hk], s: doc[sk], v: doc[vk], width, paramKey: key });
  }
  const min = control.min ?? 0;
  const max = control.max ?? 1;
  return SliderRow(key, {
    label,
    readout: format(doc[key]),
    fraction: max === min ? 0 : (doc[key] - min) / (max - min),
    width,
    paramKey: key,
  });
}

/**
 * The panel speaks in 0..1 fractions; a slider's real range and step live in its
 * `Control`, so the conversion happens where both are in scope. Snapping to the
 * declared step is what keeps an integer control (sides, octaves) integral.
 */
export function fromFraction(control: Control, fraction: number): number {
  const min = control.min ?? 0;
  const max = control.max ?? 1;
  const step = control.step ?? 0.01;
  const raw = min + (max - min) * fraction;
  const snapped = Math.round((raw - min) / step) * step + min;
  return Math.min(max, Math.max(min, Number(snapped.toFixed(6))));
}

function format(value: number): string {
  return Number.isInteger(value) ? String(value) : value.toFixed(2);
}
