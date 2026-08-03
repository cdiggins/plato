// STUB — replaced by the csg demo agent.
import * as THREE from 'three';
import { mountDemo } from '../shared/ui.js';
import { polygonMeshGeometry } from '../shared/mesh.js';
import { surfaceMaterial } from '../shared/viewer.js';
import { PolygonMesh3D } from '../plato/plato.g.js';
import type { Demo } from '../shared/demo.js';

const demo: Demo = {
  title: 'csg',
  subtitle: 'scaffold only',
  scenes: [
    {
      id: 'placeholder',
      title: 'Placeholder',
      description: 'Scaffold scene: the csg catalog has not been written yet.',
      plato: ['PolygonMesh3D.Cube'],
      build: () => new THREE.Mesh(polygonMeshGeometry(PolygonMesh3D.Cube()), surfaceMaterial()),
    },
  ],
};

mountDemo(demo);
