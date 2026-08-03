// The page shell every demo shares: sidebar with the scene list, parameter
// controls, and a panel naming the generated members the scene calls.

import './style.css';
import { Viewer, type ViewerOptions } from './viewer.js';
import type { Demo, Params, Scene } from './demo.js';

export function mountDemo(demo: Demo, viewerOptions: ViewerOptions = {}): void {
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

  const viewer = new Viewer(stage, viewerOptions);
  let current: Scene = demo.scenes[0];
  let params: Params = {};

  function rebuild(): void {
    try {
      viewer.show(current.build(params));
      stage.classList.remove('errored');
    } catch (error) {
      stage.classList.add('errored');
      console.error(error);
      description.textContent = `Build failed: ${(error as Error).message}`;
    }
  }

  function selectScene(scene: Scene): void {
    current = scene;
    params = Object.fromEntries((scene.controls ?? []).map(c => [c.key, c.def]));
    description.textContent = scene.description;
    members.innerHTML = scene.plato.map(m => `<li><code>${m}</code></li>`).join('');
    renderControls();
    for (const button of list.querySelectorAll('button')) {
      button.classList.toggle('active', button.dataset.id === scene.id);
    }
    rebuild();
  }

  function renderControls(): void {
    controlPanel.innerHTML = '';
    for (const control of current.controls ?? []) {
      const row = document.createElement('div');
      row.className = 'control';

      if (control.kind === 'toggle') {
        const id = `toggle-${control.key}`;
        row.innerHTML = `<label for="${id}">${control.label}</label>`;
        const box = document.createElement('input');
        box.type = 'checkbox';
        box.id = id;
        box.checked = params[control.key] !== 0;
        box.addEventListener('change', () => {
          params[control.key] = box.checked ? 1 : 0;
          rebuild();
        });
        row.appendChild(box);
      } else if (control.kind === 'select') {
        const id = `select-${control.key}`;
        row.innerHTML = `<label for="${id}">${control.label}</label>`;
        const select = document.createElement('select');
        select.id = id;
        (control.options ?? []).forEach((option, i) => {
          const element = document.createElement('option');
          element.value = String(i);
          element.textContent = option;
          select.appendChild(element);
        });
        select.value = String(params[control.key]);
        select.addEventListener('change', () => {
          params[control.key] = Number(select.value);
          rebuild();
        });
        row.appendChild(select);
      } else {
        const id = `slider-${control.key}`;
        const value = document.createElement('span');
        value.className = 'value';
        value.textContent = format(params[control.key]);
        row.innerHTML = `<label for="${id}">${control.label}</label>`;
        row.appendChild(value);
        const slider = document.createElement('input');
        slider.type = 'range';
        slider.id = id;
        slider.min = String(control.min ?? 0);
        slider.max = String(control.max ?? 1);
        slider.step = String(control.step ?? 0.01);
        slider.value = String(params[control.key]);
        slider.addEventListener('input', () => {
          params[control.key] = Number(slider.value);
          value.textContent = format(params[control.key]);
          rebuild();
        });
        row.appendChild(slider);
      }
      controlPanel.appendChild(row);
    }
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

function format(value: number): string {
  return Number.isInteger(value) ? String(value) : value.toFixed(2);
}
