// Colour spaces — a scene catalog over `color.{types,library}.plato · color-spaces.{types,library}.plato`.
//
// PLACEHOLDER. The page is wired into the shell (entry, landing card, Vite
// input) so the route exists; the scenes are the demo agent's work. Replace
// this whole file — see COORDINATION.md for the contract and the rules.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { palette } from '../shared/viewer.js';
import type { Demo } from '../shared/demo.js';

const demo: Demo = {
  title: 'Colour spaces',
  subtitle: 'color.{types,library}.plato · color-spaces.{types,library}.plato',
  scenes: [
    {
      id: 'placeholder',
      title: 'Not built yet',
      description:
        'This page will cover the colour space catalog — sRGB, HSL, HSV, Lab, LCh, OkLab, XYZ, and the conversions between them.',
      plato: [],
      controls: [
        { key: 'tint', label: 'Colour', kind: 'color', def: 0, colorDef: [0.55, 0.7, 0.9] },
        { key: 'gamma', label: 'Gamma', kind: 'slider', min: 0.5, max: 2.4, step: 0.01, def: 1 },
      ],
      build: (): THREE.Object3D =>
        new THREE.LineSegments(
          new THREE.BufferGeometry().setAttribute(
            'position',
            new THREE.Float32BufferAttribute([-1, 0, 0, 1, 0, 0, 0, -1, 0, 0, 1, 0], 3),
          ),
          new THREE.LineBasicMaterial({ color: palette.line }),
        ),
    },
  ],
};

mountDemo(demo);

export { demo };
