// Parametric surface tessellation.
//
// A surface is a function f(u, v) -> R3 over the unit square. gridMesh samples
// it on a regular grid and stitches the samples into an indexed triangle mesh
// with area-weighted vertex normals. Here: a torus with a radial ripple,
// written fluently against the Plato library (Turns/Cos/Sin on plain numbers).

import type { Sample } from '../core/types.js';
import { Vector3D } from '../plato/plato.g.js';
import { gridMesh } from '../core/meshBuilder.js';

export function rippledTorus(u: number, v: number): Vector3D {
    const theta = u.Turns();   // around the main ring
    const phi = v.Turns();     // around the tube
    const R = 1.1;             // ring radius
    const r = 0.42 + 0.1 * (u * 5).Turns().Sin() * (v * 3).Turns().Sin();
    return new Vector3D(
        (R + r * phi.Cos()) * theta.Cos(),
        r * phi.Sin(),
        (R + r * phi.Cos()) * theta.Sin());
}

export const parametricSurfaceSample: Sample = {
    id: 'parametric-surface',
    title: 'Parametric Surface',
    description: 'Tessellating f(u, v) into an indexed triangle mesh with vertex normals.',
    build() {
        const mesh = gridMesh(160, 48, rippledTorus);
        mesh.color = 0x4da3ff;
        return [mesh];
    },
};
