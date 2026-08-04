// Mesh connectivity + Laplacian smoothing, both from the stdlib.
//
// `TriangleMesh3D.TopologyOf()` builds the corner-twin structure Plato uses in
// place of a half-edge record: corner 3f+k of face f, its undirected edge, and
// the twin corner across that edge (-1 on a boundary). From it,
// `VertexNeighborTable` gives every vertex's one-ring, and
// `UniformLaplacianField` gives the vector each vertex would move along.
//
// The smoothing step is written out here rather than calling the stdlib's own,
// for two reasons, both writer bugs rather than library ones:
//   * `LaplacianSmoothed` takes the sum type `LaplacianWeighting`, and sum types
//     are C#-only in v1 (CHK320), so it has no TypeScript surface — plato-440.
//   * `UniformLaplacianField` resolves to the Vector2D overload of
//     `UniformLaplacian` and returns 2D vectors for a 3D mesh — plato-441.

import type { Drawable, Sample } from '../core/types.js';
import { Point3D, TriangleMesh3D, Vector3D } from '../plato/plato.g.js';
import { fromArray, meshVertices, toArray, translateMesh } from '../core/meshBuilder.js';
import { buildIcosphere } from './icosphere.js';
import { valueNoise } from './terrain.js';

/** The one-ring neighbours of every vertex, from stdlib topology. */
export const vertexRings = (mesh: TriangleMesh3D): number[][] =>
    toArray(mesh.VertexNeighborTable(mesh.TopologyOf()))
        .map(ring => toArray(ring).map(v => v.Value));

/**
 * One uniform-Laplacian step: every vertex moves `strength` of the way toward
 * the average of its one-ring, which the stdlib topology tables supply.
 */
export function laplacianStep(mesh: TriangleMesh3D, strength: number): TriangleMesh3D {
    const rings = vertexRings(mesh);
    const points = meshVertices(mesh);
    const moved = points.map((p, i) => {
        const ring = rings[i];
        if (ring.length === 0)
            return p;
        const sum = ring.reduce((acc, n) => acc.Add(points[n].PositionVector()), new Vector3D(0, 0, 0));
        const average = new Point3D(0, 0, 0).Add(sum.Multiply(1 / ring.length));
        return p.Lerp(average, strength);
    });
    return new TriangleMesh3D(fromArray(moved), mesh.Faces);
}

export function laplacianSmooth(mesh: TriangleMesh3D, strength: number, iterations: number): TriangleMesh3D {
    let current = mesh;
    for (let i = 0; i < iterations; i++)
        current = laplacianStep(current, strength);
    return current;
}

/** An icosphere with noisy radius: the smoothing test subject. */
export function noisySphere(): TriangleMesh3D {
    return buildIcosphere(3).Deform(p => {
        const dir = p.PositionVector().Normalize();
        const bump = 1 + 0.28 * valueNoise(dir.X * 5 + 5, dir.Y * 5 + dir.Z * 4 + 5);
        return new Point3D(0, 0, 0).Add(dir.Multiply(bump));
    });
}

export const halfEdgeSample: Sample = {
    id: 'half-edge',
    title: 'Connectivity + Smoothing',
    description: 'TriangleMesh3D.TopologyOf drives the one-ring tables behind ' +
        'UniformLaplacianField (before / after).',
    build(): Drawable[] {
        const noisy = noisySphere();
        return [
            {
                kind: 'mesh',
                mesh: translateMesh(noisy, new Vector3D(-1.4, 0, 0)),
                color: 0xff8c66,
                flatShading: true,
            },
            {
                kind: 'mesh',
                mesh: translateMesh(laplacianSmooth(noisy, 0.6, 12), new Vector3D(1.4, 0, 0)),
                color: 0x6fbf73,
            },
        ];
    },
};
