// Icosphere by recursive subdivision.
//
// Start from a regular icosahedron and repeatedly split every triangle into
// four, projecting new vertices onto the unit sphere. A midpoint cache keyed
// on the edge guarantees that shared edges reuse the same vertex, keeping the
// mesh watertight (V - E + F = 2).

import type { MeshData, Sample } from '../core/types.js';
import { Vector3D } from '../plato/plato.g.js';
import { meshFromVertices } from '../core/meshBuilder.js';

export function buildIcosphere(subdivisions: number): MeshData {
    const t = (1 + (5).Sqrt()) / 2;
    let vertices: Vector3D[] = [
        new Vector3D(-1, t, 0), new Vector3D(1, t, 0), new Vector3D(-1, -t, 0), new Vector3D(1, -t, 0),
        new Vector3D(0, -1, t), new Vector3D(0, 1, t), new Vector3D(0, -1, -t), new Vector3D(0, 1, -t),
        new Vector3D(t, 0, -1), new Vector3D(t, 0, 1), new Vector3D(-t, 0, -1), new Vector3D(-t, 0, 1),
    ].map(v => v.Normalize());
    let faces: number[] = [
        0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
        1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
        3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
        4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
    ];

    for (let level = 0; level < subdivisions; level++) {
        const cache = new Map<string, number>();
        const midpointIndex = (a: number, b: number): number => {
            const key = a < b ? `${a}_${b}` : `${b}_${a}`;
            let index = cache.get(key);
            if (index === undefined) {
                index = vertices.length;
                vertices.push(vertices[a].MidPoint(vertices[b]).Normalize());
                cache.set(key, index);
            }
            return index;
        };
        const next: number[] = [];
        for (let i = 0; i < faces.length; i += 3) {
            const [a, b, c] = [faces[i], faces[i + 1], faces[i + 2]];
            const [ab, bc, ca] = [midpointIndex(a, b), midpointIndex(b, c), midpointIndex(c, a)];
            next.push(a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca);
        }
        faces = next;
    }

    return meshFromVertices(vertices, faces);
}

export const icosphereSample: Sample = {
    id: 'icosphere',
    title: 'Icosphere Subdivision',
    description: 'Recursive triangle subdivision of an icosahedron with an edge-midpoint cache.',
    build() {
        const levels = [0, 1, 2, 3];
        return levels.map((level, i) => {
            const mesh = buildIcosphere(level);
            for (let j = 0; j < mesh.positions.length; j += 3)
                mesh.positions[j] += (i - (levels.length - 1) / 2) * 2.3;
            mesh.color = 0x4da3ff;
            mesh.flatShading = true;
            return mesh;
        });
    },
};
