// Parametric surface tessellation.
//
// A parametric surface in Plato is any type implementing IParametricSurface:
// Eval(UvCoordinate) -> Point3D, plus ClosedU / ClosedV saying which directions
// seam shut. `ToQuadMesh(nCols, nRows)` samples one on a grid and stitches the
// samples into a quad mesh, wrapping the last ring of cells in each closed
// direction so a torus comes out watertight with no duplicated seam vertices.
//
// Nothing here computes geometry: the surfaces below are stdlib types, and the
// tessellation is a stdlib function.

import type { Drawable, Sample } from '../core/types.js';
import { Direction3D, Point3D, Supertoroid, Torus, Vector3D } from '../plato/plato.g.js';
import { translateMesh } from '../core/meshBuilder.js';

/** The plain torus: the reference against which the superquadric is read. */
export const torus = new Torus(
    new Point3D(0, 0, 0), new Direction3D(new Vector3D(0, 1, 0)), 1.1, 0.42);

/** A superquadric torus — same ring, but square-ish section and ring profiles. */
export const supertoroid = new Supertoroid(1.1, 0.42, 0.35, 0.6);

export const parametricSurfaceSample: Sample = {
    id: 'parametric-surface',
    title: 'Parametric Surface',
    description: 'IParametricSurface.ToQuadMesh: a stdlib surface sampled on a UV grid, ' +
        'seaming shut in both closed directions.',
    build(): Drawable[] {
        const shift = new Vector3D(1.7, 0, 0);
        return [
            {
                kind: 'mesh',
                mesh: translateMesh(torus.ToTriangleMesh(160, 48), shift.Negative()),
                color: 0x4da3ff,
            },
            {
                kind: 'mesh',
                mesh: translateMesh(supertoroid.ToTriangleMesh(160, 48), shift),
                color: 0xffd166,
            },
        ];
    },
};
