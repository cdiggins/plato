// Bounding volume hierarchy (BVH) over mesh triangles.
//
// Recursively partition triangles by sorting their centroids along the longest
// axis of the current bounds and splitting at the median. The result is a
// balanced tree of axis-aligned boxes that takes a raycast or an intersection
// query from O(n) to O(log n).
//
// The box algebra is the stdlib's: `Bounds3D.Empty` as the identity for
// `UnionOfBounds`, `Bounds3D.Extent` for the split axis, `Triangle3D.Centroid`
// for the sort key. The stdlib also ships the QUERY half of a BVH
// (`Bvh3D.Raycast`, `CandidatesInBounds`), but nothing that builds one — see
// plato-442.
//
// `Triangle3D.Bounds()` is deliberately not used: it resolves to the Bounds2D
// overload of BoundsOfPoints and silently returns a 2D box (plato-441).

import type { Drawable, Sample } from '../core/types.js';
import { Bounds3D, Line3D, Point3D, Triangle3D, TriangleMesh3D, Vector3D } from '../plato/plato.g.js';
import { boxEdges, toArray, translateMesh } from '../core/meshBuilder.js';
import { buildIcosphere } from './icosphere.js';

export interface BvhNode {
    bounds: Bounds3D;
    depth: number;
    left?: BvhNode;
    right?: BvhNode;
    /** Triangle indices (leaves only). */
    triangles?: number[];
}

export const triangleCentroid = (triangles: Triangle3D[], t: number): Point3D =>
    triangles[t].Centroid();

/** The axis-aligned box of one triangle. */
const triangleBounds = (t: Triangle3D): Bounds3D =>
    [t.A, t.B, t.C].reduce(
        (b, p) => new Bounds3D(
            new Point3D(b.Min.X.Min(p.X), b.Min.Y.Min(p.Y), b.Min.Z.Min(p.Z)),
            new Point3D(b.Max.X.Max(p.X), b.Max.Y.Max(p.Y), b.Max.Z.Max(p.Z))),
        Bounds3D.Empty());

/** The union of a set of triangle boxes; Bounds3D.Empty is the identity. */
const boundsOf = (triangles: Triangle3D[], indices: number[]): Bounds3D =>
    indices.reduce((acc, t) => acc.UnionOfBounds(triangleBounds(triangles[t])), Bounds3D.Empty());

export function buildBvh(triangles: Triangle3D[], indices: number[], depth = 0, leafSize = 8): BvhNode {
    const bounds = boundsOf(triangles, indices);
    if (indices.length <= leafSize || depth >= 16)
        return { bounds, depth, triangles: indices };

    // Median split along the longest axis of the bounds.
    const extent = bounds.Extent();
    const axis: 'X' | 'Y' | 'Z' =
        extent.X >= extent.Y && extent.X >= extent.Z ? 'X' : extent.Y >= extent.Z ? 'Y' : 'Z';
    const sorted = indices.slice().sort((a, b) =>
        triangleCentroid(triangles, a)[axis] - triangleCentroid(triangles, b)[axis]);
    const half = sorted.length >> 1;

    return {
        bounds, depth,
        left: buildBvh(triangles, sorted.slice(0, half), depth + 1, leafSize),
        right: buildBvh(triangles, sorted.slice(half), depth + 1, leafSize),
    };
}

/** The boxes of every node at a given depth (leaves shallower than it included). */
export function boxesAtDepth(node: BvhNode, depth: number): Line3D[] {
    if (node.depth === depth || (node.triangles && node.depth < depth))
        return boxEdges(node.bounds.Min, node.bounds.Max);
    return [
        ...(node.left ? boxesAtDepth(node.left, depth) : []),
        ...(node.right ? boxesAtDepth(node.right, depth) : []),
    ];
}

/** Every triangle of a mesh, gathered by the stdlib. */
export const meshTriangles = (mesh: TriangleMesh3D): Triangle3D[] => toArray(mesh.Primitives());

export const bvhSample: Sample = {
    id: 'bvh',
    title: 'BVH (AABB Tree)',
    description: 'Median-split Bounds3D hierarchy over TriangleMesh3D.Primitives, at three depths.',
    build(): Drawable[] {
        const mesh = buildIcosphere(3);
        const triangles = meshTriangles(mesh);
        const root = buildBvh(triangles, triangles.map((_, i) => i));

        const colors = [0xff6b6b, 0xffd166, 0x6fbf73];
        const panels = [2, 4, 6].map((depth, i): Drawable[] => {
            const shift = new Vector3D((i - 1) * 2.6, 0, 0);
            const move = (p: Point3D) => p.Add(shift);
            return [
                {
                    kind: 'mesh',
                    mesh: translateMesh(mesh, shift),
                    color: 0x4da3ff,
                    opacity: 0.25,
                },
                {
                    kind: 'lines',
                    segments: boxesAtDepth(root, depth)
                        .map(s => new Line3D(move(s.A), move(s.B))),
                    color: colors[i],
                    opacity: 0.85,
                },
            ];
        });

        return panels.flat();
    },
};
