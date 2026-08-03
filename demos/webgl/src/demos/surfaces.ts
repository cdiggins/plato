// Parametric surfaces — a scene catalog over `surfaces.{types,library}.plato · surfaces-shapes.{types,library}.plato`.
//
// PLACEHOLDER. The page is wired into the shell (entry, landing card, Vite
// input) so the route exists; the scenes are the demo agent's work. Replace
// this whole file — see COORDINATION.md for the contract and the rules.

import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { palette } from '../shared/viewer.js';
import type { Demo } from '../shared/demo.js';

const demo: Demo = {
  title: 'Parametric surfaces',
  subtitle: 'surfaces.{types,library}.plato · surfaces-shapes.{types,library}.plato',
  scenes: [
    {
      id: 'placeholder',
      title: 'Not built yet',
      description:
        'This page will cover the parametric surface catalog — torus, Mobius strip, Klein bottle, minimal surfaces, and the patch types.',
      plato: [],
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
