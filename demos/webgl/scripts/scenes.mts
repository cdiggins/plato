// Gate: every scene on every page builds, off the page.
//
//   npm run scenes
//
// A demo module calls `mountDemo` at import time and exports its catalog;
// `mountDemo` no-ops when there is no `document`, so this script can import
// every page, call each scene's `build` at its default parameters, and report
// what came back. Three.js builds buffer geometry happily without a WebGL
// context — only the renderer needs one, and nothing here makes a renderer.
//
// It catches what `npm run typecheck` cannot: a member that throws at runtime, a
// scene that returns an empty object, a build that takes seconds per tick.
//
// A scene that declares `tick` is a simulation, and gets stepped as well as
// built: the failure it is guarding against is a solver that diverges, which
// throws nothing and draws nothing — it just fills the position buffer with
// NaN. That is checked explicitly after the run.

import type { Object3D } from 'three';
import type { Demo, Params } from '../src/shared/demo.ts';

// A demo may reach for the DOM for presentation — a 2D canvas for billboard
// text, the description element for a live count. Enough of a stub to let those
// run, and deliberately without a `body`, which is how `mountDemo` tells this
// harness from a real page. Nothing here draws: text measurement returns a
// plausible width so a label sizes itself, and the paint calls are sinks.
const context2d = {
  font: '',
  textBaseline: '',
  textAlign: '',
  lineJoin: '',
  lineWidth: 0,
  strokeStyle: '',
  fillStyle: '',
  measureText: (text: string) => ({ width: text.length * 22 }),
  strokeText: () => {},
  fillText: () => {},
  clearRect: () => {},
  fillRect: () => {},
  beginPath: () => {},
  closePath: () => {},
  moveTo: () => {},
  lineTo: () => {},
  arc: () => {},
  fill: () => {},
  stroke: () => {},
};

Object.defineProperty(globalThis, 'document', {
  value: {
    createElement: () => ({ width: 0, height: 0, style: {}, getContext: () => context2d }),
    querySelector: () => null,
  },
  configurable: true,
});

const pages = [
  'polyhedra',
  'polygons',
  'csg',
  'deformers',
  'curves',
  'surfaces',
  'noise',
  'colors',
  'transforms',
  'marching',
  'voxels',
  'lattices',
  'sampling',
  'remeshing',
  'fea',
  'rigidbody',
  'cloth',
];

// `npm run scenes -- lattices cloth` runs only those pages. The full run is the
// gate; the filter exists because several agents build different pages in one
// shared tree at once, and a page whose module nobody has written yet fails to
// import — which would otherwise leave everyone unable to gate their own work.
// A name that matches nothing is an error rather than an empty run, so a typo
// does not read as success.
const requested = process.argv.slice(2);
const unknown = requested.filter(name => !pages.includes(name));
if (unknown.length > 0) {
  console.log(`No such page: ${unknown.join(', ')}. Known pages: ${pages.join(', ')}`);
  process.exit(2);
}
const selected = requested.length > 0 ? requested : pages;

/**
 * How many frames a simulation scene is stepped before the gate is satisfied.
 * One frame proves `tick` runs; a short run proves it survives its own output,
 * which is where a solver that diverges or a state bag that grows shows up.
 */
const TICK_FRAMES = 30;
const TICK_SECONDS = 1 / 60;

/** The parameter bag the shell would start a scene with. */
function defaults(demo: Demo, index: number): Params {
  const out: Params = {};
  for (const control of demo.scenes[index].controls ?? []) {
    if (control.kind === 'color') {
      const [h, s, v] = control.colorDef ?? [0.58, 0.7, 0.9];
      out[`${control.key}H`] = h;
      out[`${control.key}S`] = s;
      out[`${control.key}V`] = v;
    } else {
      out[control.key] = control.def;
    }
  }
  return out;
}

function vertexCount(object: Object3D): number {
  let total = 0;
  object.traverse(node => {
    const geometry = (node as { geometry?: { attributes?: { position?: { count: number } } } }).geometry;
    total += geometry?.attributes?.position?.count ?? 0;
  });
  return total;
}

/**
 * A diverged solver is silent: no throw, no empty result, just NaN positions.
 * Reports the first offending attribute so the message names something.
 */
function nonFinitePositions(object: Object3D): string | null {
  let found: string | null = null;
  object.traverse(node => {
    if (found) return;
    const position = (node as { geometry?: { attributes?: { position?: { array: ArrayLike<number> } } } })
      .geometry?.attributes?.position;
    if (!position) return;
    const { array } = position;
    for (let i = 0; i < array.length; i++) {
      if (!Number.isFinite(array[i])) {
        found = `${node.type}${node.name ? ` "${node.name}"` : ''} position[${i}]`;
        return;
      }
    }
  });
  return found;
}

let failures = 0;
let scenes = 0;

for (const page of selected) {
  const module = (await import(`../src/demos/${page}.ts`)) as { demo?: Demo };
  const demo = module.demo;
  if (!demo) {
    failures++;
    console.log(`FAIL ${page}: the module exports no \`demo\` — the scenes cannot be exercised`);
    continue;
  }
  console.log(`\n${page} — ${demo.scenes.length} scene(s)`);
  for (let i = 0; i < demo.scenes.length; i++) {
    const scene = demo.scenes[i];
    const params = defaults(demo, i);
    scenes++;
    const started = performance.now();
    try {
      const built = scene.build(params);
      const ms = performance.now() - started;
      const points = vertexCount(built);
      let status = scene.status?.(params) ?? '';
      const slow = ms > 400 ? '  SLOW' : '';
      if (points === 0) {
        failures++;
        console.log(`  FAIL ${scene.id}: built nothing (0 vertices) in ${ms.toFixed(0)} ms`);
      } else {
        console.log(`  ok   ${scene.id}: ${points} vertices in ${ms.toFixed(0)} ms${slow}`);
      }
      if (scene.tick) {
        const startedTick = performance.now();
        for (let f = 0; f < TICK_FRAMES; f++) {
          const line = scene.tick(TICK_SECONDS, params, built);
          if (typeof line === 'string') status = line;
        }
        const perFrame = (performance.now() - startedTick) / TICK_FRAMES;
        const budget = perFrame > 16 ? '  OVER BUDGET' : '';
        console.log(`       ${TICK_FRAMES} frames, ${perFrame.toFixed(1)} ms/frame${budget}`);
        const diverged = nonFinitePositions(built);
        if (diverged) {
          failures++;
          console.log(`  FAIL ${scene.id}: diverged after ${TICK_FRAMES} frames — ${diverged} is not finite`);
        }
      }
      if (status) console.log(`       ${status.slice(0, 150)}`);
    } catch (error) {
      failures++;
      console.log(`  FAIL ${scene.id}: ${(error as Error).message.slice(0, 160)}`);
    }
  }
}

console.log(`\n${scenes} scenes, ${failures} failure(s)`);
if (failures > 0) process.exit(1);
