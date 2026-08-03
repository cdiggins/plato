import { scenes, scenesByDim, type Dim, type Scene } from './scenes.js';
import { Field2DViewer } from './render/field2d.js';
import { Volume3DViewer } from './render/volume3d.js';
import { FunctionSdf2D } from './plato/plato.g.js';

const viewerEl = document.getElementById('viewer')!;
const listEl = document.getElementById('scene-list')!;
const controlsEl = document.getElementById('controls')!;
const hintEl = document.getElementById('hint')!;
const dimButtons = [...document.querySelectorAll<HTMLButtonElement>('#dim-tabs button')];

let dim: Dim = '2d';
let current: Scene = scenesByDim('2d')[0];
let paramValues: number[] = [];
let field2d: Field2DViewer | null = null;
let vol3d: Volume3DViewer | null = null;

function ensureViewer(): void {
  if (dim === '2d') {
    if (vol3d) {
      vol3d.dispose();
      vol3d = null;
    }
    if (!field2d) {
      field2d = new Field2DViewer(viewerEl);
      // Debug/automation handle (harness screenshots need a forced render).
      (window as unknown as { __field2d?: Field2DViewer }).__field2d = field2d;
    }
    hintEl.textContent =
      'CPU field from generated FunctionSdf2D.Eval · white = zero level set';
  } else {
    if (field2d) {
      field2d.dispose();
      field2d = null;
    }
    if (!vol3d) {
      vol3d = new Volume3DViewer(viewerEl);
      // Debug/automation handle (harness screenshots need a forced render).
      (window as unknown as { __vol3d?: Volume3DViewer }).__vol3d = vol3d;
      vol3d.onResolution = (res) => {
        hintEl.textContent =
          `GPU raymarch over a ${res}³ field baked by FunctionSdf3D.Eval in workers · drag to orbit`;
      };
    }
    hintEl.textContent = 'Baking field via FunctionSdf3D.Eval in workers…';
  }
}

function defaultParams(scene: Scene): number[] {
  return scene.params.map((p) => p.default);
}

function applySdf(): void {
  if (dim === '2d') {
    const sdf = current.build(paramValues);
    if (!(sdf instanceof FunctionSdf2D)) throw new Error('expected FunctionSdf2D');
    field2d!.setSdf(sdf);
  } else {
    // The bake workers build the FunctionSdf3D themselves from the scene id.
    vol3d!.setScene(current.id, paramValues);
  }
}

function renderList(): void {
  listEl.innerHTML = '';
  for (const scene of scenesByDim(dim)) {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.classList.toggle('active', scene.id === current.id);
    btn.innerHTML = `${scene.title}<span class="desc">${scene.description}</span>`;
    btn.addEventListener('click', () => selectScene(scene.id));
    listEl.appendChild(btn);
  }
}

function renderControls(): void {
  controlsEl.innerHTML = '';

  const plato = document.createElement('pre');
  plato.className = 'plato-ref';
  plato.textContent = current.plato;
  controlsEl.appendChild(plato);

  const meta = document.createElement('p');
  meta.className = 'meta';
  meta.innerHTML =
    current.dim === '2d'
      ? `<span class="legend"><span><span class="swatch" style="background:var(--inside)"></span>inside (&lt; 0)</span>
         <span><span class="swatch" style="background:var(--outside)"></span>outside (&gt; 0)</span></span>
         · ${current.description}`
      : current.description;
  controlsEl.appendChild(meta);

  current.params.forEach((param, i) => {
    const label = document.createElement('label');
    const row = document.createElement('span');
    row.className = 'row';
    const name = document.createElement('span');
    name.textContent = param.label;
    const value = document.createElement('span');
    value.textContent = paramValues[i].toFixed(2);
    row.append(name, value);

    const input = document.createElement('input');
    input.type = 'range';
    input.min = String(param.min);
    input.max = String(param.max);
    input.step = String(param.step ?? 0.01);
    input.value = String(paramValues[i]);
    input.addEventListener('input', () => {
      paramValues[i] = Number(input.value);
      value.textContent = paramValues[i].toFixed(2);
      applySdf();
    });
    label.append(row, input);
    controlsEl.appendChild(label);
  });

  if (dim === '2d') {
    const zoom = document.createElement('label');
    zoom.innerHTML = `<span class="row"><span>Zoom</span><span id="zoom-val">1.35</span></span>`;
    const input = document.createElement('input');
    input.type = 'range';
    input.min = '0.5';
    input.max = '2.5';
    input.step = '0.01';
    input.value = '1.35';
    input.addEventListener('input', () => {
      const z = Number(input.value);
      zoom.querySelector('#zoom-val')!.textContent = z.toFixed(2);
      field2d?.setZoom(z);
    });
    zoom.appendChild(input);
    controlsEl.appendChild(zoom);
  } else {
    const hue = document.createElement('label');
    hue.innerHTML = `<span class="row"><span>Hue</span><span id="hue-val">0.08</span></span>`;
    const hueIn = document.createElement('input');
    hueIn.type = 'range';
    hueIn.min = '0';
    hueIn.max = '1';
    hueIn.step = '0.01';
    hueIn.value = '0.08';
    hueIn.addEventListener('input', () => {
      const h = Number(hueIn.value);
      hue.querySelector('#hue-val')!.textContent = h.toFixed(2);
      vol3d?.setHue(h);
    });
    hue.appendChild(hueIn);
    controlsEl.appendChild(hue);

    const amb = document.createElement('label');
    amb.innerHTML = `<span class="row"><span>Ambient</span><span id="amb-val">0.28</span></span>`;
    const ambIn = document.createElement('input');
    ambIn.type = 'range';
    ambIn.min = '0';
    ambIn.max = '0.8';
    ambIn.step = '0.01';
    ambIn.value = '0.28';
    ambIn.addEventListener('input', () => {
      const a = Number(ambIn.value);
      amb.querySelector('#amb-val')!.textContent = a.toFixed(2);
      vol3d?.setAmbient(a);
    });
    amb.appendChild(ambIn);
    controlsEl.appendChild(amb);
  }
}

function applyScene(): void {
  ensureViewer();
  applySdf();
  renderList();
  renderControls();
  history.replaceState(null, '', `#${current.id}`);
}

function selectScene(id: string): void {
  const next = scenes.find((s) => s.id === id);
  if (!next) return;
  dim = next.dim;
  current = next;
  paramValues = defaultParams(current);
  for (const b of dimButtons) b.classList.toggle('active', b.dataset.dim === dim);
  applyScene();
}

function setDim(next: Dim): void {
  if (dim === next) return;
  dim = next;
  current = scenesByDim(dim)[0];
  paramValues = defaultParams(current);
  for (const b of dimButtons) b.classList.toggle('active', b.dataset.dim === dim);
  applyScene();
}

for (const b of dimButtons) {
  b.addEventListener('click', () => setDim(b.dataset.dim as Dim));
}

const hash = location.hash.slice(1);
if (hash && scenes.some((s) => s.id === hash)) selectScene(hash);
else {
  paramValues = defaultParams(current);
  applyScene();
}
