// Bounding volume hierarchy (BVH) over mesh triangles.
//
// Recursively partition triangles by sorting their centroids along the
// longest axis of the current bounds and splitting at the median. The result
// is a balanced tree of axis-aligned boxes used to accelerate raycasts and
// intersection queries from O(n) to O(log n).

import type { MeshData, Sample } from '../core/types.js';
import { Vector3D } from '../plato/plato.g.js';
import { appendBoxEdges, vertexAt } from '../core/meshBuilder.js';
import { buildIcosphere } from './icosphere.js';

export interface BvhNode {
    min: Vector3D;
    max: Vector3D;
    depth: number;
    left?: BvhNode;
    right?: BvhNode;
    /** Triangle indices (leaves only). */
    triangles?: number[];
}

export function triangleCentroid(mesh: MeshData, t: number): Vector3D {
    const a = vertexAt(mesh.positions, mesh.indices[t * 3]);
    const b = vertexAt(mesh.positions, mesh.indices[t * 3 + 1]);
    const c = vertexAt(mesh.positions, mesh.indices[t * 3 + 2]);
    return a.Add(b).Add(c).Scale(1 / 3);
}

function triangleBounds(mesh: MeshData, t: number): { min: Vector3D; max: Vector3D } {
    let min = new Vector3D(Infinity, Infinity, Infinity);
    let max = new Vector3D(-Infinity, -Infinity, -Infinity);
    for (let k = 0; k < 3; k++) {
        const v = vertexAt(mesh.positions, mesh.indices[t * 3 + k]);
        min = min.Min(v);
        max = max.Max(v);
    }
    return { min, max };
}

export function buildBvh(mesh: MeshData, triangles: number[], depth = 0, leafSize = 8): BvhNode {
    let min = new Vector3D(Infinity, Infinity, Infinity);
    let max = new Vector3D(-Infinity, -Infinity, -Infinity);
    for (const t of triangles) {
        const b = triangleBounds(mesh, t);
        min = min.Min(b.min);
        max = max.Max(b.max);
    }

    if (triangles.length <= leafSize || depth >= 16)
        return { min, max, depth, triangles };

    // Median split along the longest axis of the bounds.
    const extent = max.Subtract(min);
    const axis: 'X' | 'Y' | 'Z' =
        extent.X >= extent.Y && extent.X >= extent.Z ? 'X' : extent.Y >= extent.Z ? 'Y' : 'Z';
    const sorted = triangles.slice().sort((a, b) =>
        triangleCentroid(mesh, a)[axis] - triangleCentroid(mesh, b)[axis]);
    const half = sorted.length >> 1;

    return {
        min, max, depth,
        left: buildBvh(mesh, sorted.slice(0, half), depth + 1, leafSize),
        right: buildBvh(mesh, sorted.slice(half), depth + 1, leafSize),
    };
}

export function collectBoxesAtDepth(node: BvhNode, depth: number, out: number[]): void {
    if (node.depth === depth || (node.triangles && node.depth < depth)) {
        appendBoxEdges(node.min, node.max, out);
        return;
    }
    if (node.left) collectBoxesAtDepth(node.left, depth, out);
    if (node.right) collectBoxesAtDepth(node.right, depth, out);
}

export const bvhSample: Sample = {
    id: 'bvh',
    title: 'BVH (AABB Tree)',
    description: 'Median-split bounding box hierarchy over icosphere triangles, shown at three depths.',
    build() {
        const mesh = buildIcosphere(3);
        const allTris = Array.from({ length: mesh.indices.length / 3 }, (_, i) => i);
        const root = buildBvh(mesh, allTris);

        const colors = [0xff6b6b, 0xffd166, 0x6fbf73];
        const boxes = [2, 4, 6].map((depth, i) => {
            const positions: number[] = [];
            collectBoxesAtDepth(root, depth, positions);
            for (let j = 0; j < positions.length; j += 3)
                positions[j] += (i - 1) * 2.6; // side by side
            return { kind: 'lines', positions, color: colors[i], opacity: 0.85 } as const;
        });

        const meshes = [-1, 0, 1].map(k => {
            const m = buildIcosphere(3);
            for (let j = 0; j < m.positions.length; j += 3)
                m.positions[j] += k * 2.6;
            m.color = 0x4da3ff;
            m.opacity = 0.25;
            return m;
        });

        return [...meshes, ...boxes];
    },
};
