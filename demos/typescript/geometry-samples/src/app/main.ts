// App shell: sample list on the left, viewer + syntax-colored source on the
// right. Samples produce plain Drawables from the Plato geometry library; the
// adapter layer turns them into Three.js objects. The code panel has three
// tabs: the sample driver (TS), the Plato source, and the generated library.

import hljs from 'highlight.js/lib/core';
import typescript from 'highlight.js/lib/languages/typescript';
import 'highlight.js/styles/github-dark.css';

import { samples } from '../samples/index.js';
import { sampleSources, sharedTabs } from './sources.js';
import { drawablesToGroup } from '../adapters/three.js';
import { Viewer } from './viewer.js';

hljs.registerLanguage('typescript', typescript);

const viewer = new Viewer(document.getElementById('viewer')!);
const list = document.getElementById('sample-list')!;
const codeElement = document.getElementById('code')!;
const codeHeader = document.getElementById('code-header')!;
const tabBar = document.getElementById('code-tabs')!;

const buttons = new Map<string, HTMLButtonElement>();
let currentSampleId = '';
let currentTab = 0; // 0 = sample source, 1.. = shared tabs

function tabsFor(sampleId: string): { label: string; source: string }[] {
    return [
        { label: `${sampleId}.ts`, source: sampleSources[sampleId] ?? '// source not found' },
        ...sharedTabs,
    ];
}

function renderCode(): void {
    const tabs = tabsFor(currentSampleId);
    const active = tabs[Math.min(currentTab, tabs.length - 1)];

    tabBar.innerHTML = '';
    tabs.forEach((tab, i) => {
        const b = document.createElement('button');
        b.textContent = tab.label;
        b.classList.toggle('active', i === currentTab);
        b.addEventListener('click', () => { currentTab = i; renderCode(); });
        tabBar.appendChild(b);
    });

    codeElement.textContent = active.source;
    codeElement.removeAttribute('data-highlighted');
    hljs.highlightElement(codeElement as HTMLElement);
    codeElement.parentElement!.parentElement!.scrollTop = 0;
}

function select(id: string): void {
    const sample = samples.find(s => s.id === id) ?? samples[0];
    currentSampleId = sample.id;
    for (const [key, button] of buttons)
        button.classList.toggle('active', key === sample.id);

    viewer.setContent(drawablesToGroup(sample.build()));

    codeHeader.innerHTML = `<b>${sample.title}</b> &mdash; ${sample.description}`;
    renderCode();

    history.replaceState(null, '', `#${sample.id}`);
}

for (const sample of samples) {
    const button = document.createElement('button');
    button.innerHTML = `${sample.title}<span class="sample-desc">${sample.description}</span>`;
    button.addEventListener('click', () => select(sample.id));
    list.appendChild(button);
    buttons.set(sample.id, button);
}

select(location.hash.slice(1) || samples[0].id);

// Debug handle for the console / tooling.
(window as unknown as { viewer: Viewer }).viewer = viewer;
