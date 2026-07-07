// Octree over a point cloud.
//
// Each node covers an axis-aligned box. Points accumulate in leaves; when a
// leaf exceeds its capacity (and the depth limit allows) it splits into eight
// children and pushes its points down. Query structures like this power
// neighbor searches, culling, and collision broad-phases.

import type { Sample } from '../core/types.js';
import { Vector3D } from '../plato/plato.g.js';
import { makeRng } from '../core/random.js';
import { appendBoxEdges, pushVectors } from '../core/meshBuilder.js';

export interface OctreeNode {
    min: Vector3D;
    max: Vector3D;
    depth: number;
    /** Indices into the point list (leaves only). */
    points: number[];
    children: OctreeNode[] | null;
}

export function makeOctree(min: Vector3D, max: Vector3D, depth = 0): OctreeNode {
    return { min, max, depth, points: [], children: null };
}

export function insert(node: OctreeNode, pts: Vector3D[], i: number, capacity: number, maxDepth: number): void {
    if (node.children) {
        insert(childFor(node, pts[i]), pts, i, capacity, maxDepth);
        return;
    }
    node.points.push(i);
    if (node.points.length > capacity && node.depth < maxDepth) {
        split(node);
        const pending = node.points;
        node.points = [];
        for (const p of pending)
            insert(childFor(node, pts[p]), pts, p, capacity, maxDepth);
    }
}

function split(node: OctreeNode): void {
    const { min, max } = node;
    const mid = min.MidPoint(max);
    node.children = [];
    for (let octant = 0; octant < 8; octant++) {
        node.children.push(makeOctree(
            new Vector3D(octant & 1 ? mid.X : min.X, octant & 2 ? mid.Y : min.Y, octant & 4 ? mid.Z : min.Z),
            new Vector3D(octant & 1 ? max.X : mid.X, octant & 2 ? max.Y : mid.Y, octant & 4 ? max.Z : mid.Z),
            node.depth + 1));
    }
}

function childFor(node: OctreeNode, p: Vector3D): OctreeNode {
    const mid = node.min.MidPoint(node.max);
    const octant = (p.X >= mid.X ? 1 : 0) | (p.Y >= mid.Y ? 2 : 0) | (p.Z >= mid.Z ? 4 : 0);
    return node.children![octant];
}

export function collectLeaves(node: OctreeNode, out: OctreeNode[] = []): OctreeNode[] {
    if (node.children)
        for (const c of node.children) collectLeaves(c, out);
    else out.push(node);
    return out;
}

/** Three gaussian-ish clusters of points, deterministic. */
export function clusteredPoints(count: number): Vector3D[] {
    const rng = makeRng(23);
    const centers = [new Vector3D(-0.7, 0.4, -0.5), new Vector3D(0.8, -0.3, 0.4), new Vector3D(0.1, 0.7, 0.8)];
    const pts: Vector3D[] = [];
    for (let i = 0; i < count; i++) {
        const gauss = () => (rng() + rng() + rng() - 1.5) * 0.55;
        pts.push(centers[i % centers.length].Add(new Vector3D(gauss(), gauss(), gauss())));
    }
    return pts;
}

export const octreeSample: Sample = {
    id: 'octree',
    title: 'Octree',
    description: 'Adaptive spatial subdivision: leaves split at 8 points, down to depth 5.',
    build() {
        const pts = clusteredPoints(600);
        const root = makeOctree(new Vector3D(-2, -2, -2), new Vector3D(2, 2, 2));
        for (let i = 0; i < pts.length; i++)
            insert(root, pts, i, 8, 5);

        const boxes: number[] = [];
        for (const leaf of collectLeaves(root))
            if (leaf.points.length > 0)
                appendBoxEdges(leaf.min, leaf.max, boxes);

        const cloud: number[] = [];
        pushVectors(cloud, ...pts);

        return [
            { kind: 'lines', positions: boxes, color: 0x3f76a8, opacity: 0.7 },
            { kind: 'points', positions: cloud, color: 0xffd166, size: 0.035 },
        ];
    },
};
