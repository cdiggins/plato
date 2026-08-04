// Octree over a point cloud.
//
// Each node covers a `Bounds3D`. Points accumulate in leaves; when a leaf
// exceeds its capacity (and the depth limit allows) it splits into eight
// children and pushes its points down. Query structures like this power
// neighbour searches, culling and collision broad-phases.
//
// The stdlib ships the query half of this — `Octree3D`, `OctreeNode`, and
// `OctreeCandidates` in `library SpatialStructures` — but nothing that BUILDS
// one, because construction grows a tree of variable arity as it goes. See
// plato-442. The node bounds, containment tests and splits below are all
// stdlib Bounds3D operations.

import type { Drawable, Sample } from '../core/types.js';
import { Bounds2D, Bounds3D, Point2D, Point3D, Vector3D } from '../plato/plato.g.js';
import { boxEdges, toArray } from '../core/meshBuilder.js';

export interface OctreeNode {
    bounds: Bounds3D;
    depth: number;
    /** Indices into the point list (leaves only). */
    points: number[];
    children: OctreeNode[] | null;
}

export const makeOctree = (bounds: Bounds3D, depth = 0): OctreeNode =>
    ({ bounds, depth, points: [], children: null });

export function insert(node: OctreeNode, pts: Point3D[], i: number, capacity: number, maxDepth: number): void {
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

/** The eight octants of a node's bounds, split at its centre. */
function split(node: OctreeNode): void {
    const { Min: min, Max: max } = node.bounds;
    const mid = node.bounds.Center();
    node.children = [];
    for (let octant = 0; octant < 8; octant++) {
        node.children.push(makeOctree(new Bounds3D(
            new Point3D(octant & 1 ? mid.X : min.X, octant & 2 ? mid.Y : min.Y, octant & 4 ? mid.Z : min.Z),
            new Point3D(octant & 1 ? max.X : mid.X, octant & 2 ? max.Y : mid.Y, octant & 4 ? max.Z : mid.Z)),
            node.depth + 1));
    }
}

function childFor(node: OctreeNode, p: Point3D): OctreeNode {
    const mid = node.bounds.Center();
    const octant = (p.X >= mid.X ? 1 : 0) | (p.Y >= mid.Y ? 2 : 0) | (p.Z >= mid.Z ? 4 : 0);
    return node.children![octant];
}

export function collectLeaves(node: OctreeNode, out: OctreeNode[] = []): OctreeNode[] {
    if (node.children)
        for (const c of node.children) collectLeaves(c, out);
    else out.push(node);
    return out;
}

/** Three clusters of points, deterministic: stdlib Halton samples about each centre. */
export function clusteredPoints(count: number): Point3D[] {
    const centers = [
        new Vector3D(-0.7, 0.4, -0.5), new Vector3D(0.8, -0.3, 0.4), new Vector3D(0.1, 0.7, 0.8)];
    const spread = new Bounds2D(new Point2D(-0.55, -0.55), new Point2D(0.55, 0.55));
    const planar = toArray(spread.HaltonPoints2D(count, 2, 3));
    const depth = toArray(spread.HaltonPoints2D(count, 5, 7));
    return planar.map((p, i) => {
        const c = centers[i % centers.length];
        return new Point3D(0, 0, 0).Add(c.Add(new Vector3D(p.X, p.Y, depth[i].X)));
    });
}

export const octreeSample: Sample = {
    id: 'octree',
    title: 'Octree',
    description: 'Adaptive Bounds3D subdivision: leaves split at 8 points, down to depth 5.',
    build(): Drawable[] {
        const pts = clusteredPoints(600);
        const root = makeOctree(new Bounds3D(new Point3D(-2, -2, -2), new Point3D(2, 2, 2)));
        for (let i = 0; i < pts.length; i++)
            insert(root, pts, i, 8, 5);

        const boxes = collectLeaves(root)
            .filter(leaf => leaf.points.length > 0)
            .flatMap(leaf => boxEdges(leaf.bounds.Min, leaf.bounds.Max));

        return [
            { kind: 'lines', segments: boxes, color: 0x3f76a8, opacity: 0.7 },
            { kind: 'points', points: pts, color: 0xffd166, size: 0.035 },
        ];
    },
};
