// Delaunay triangulation (Bowyer-Watson).
//
// Insert points one at a time: find all triangles whose circumcircle contains
// the new point (the "bad" triangles), remove them, and re-triangulate the
// boundary of the resulting cavity by fanning to the new point. The result
// maximizes the minimum angle over all triangulations of the point set.
//
// The circumcircle comes from the stdlib: `Triangle2D.Circumcenter`. The
// insertion loop stays here because it rewrites a triangle set as it goes,
// which the forward vocabulary has no container for — see plato-442.

import type { Drawable, Sample } from '../core/types.js';
import {
    Bounds2D, Line3D, Point2D, Point3D, Triangle2D, TriangleMesh3D,
} from '../plato/plato.g.js';
import { meshFromIndices, toArray } from '../core/meshBuilder.js';

export interface Triangle2 { a: number; b: number; c: number; }

/** The circumcircle of a triangle, as its center and squared radius. */
export function circumcircle(pts: Point2D[], t: Triangle2): { center: Point2D; radiusSquared: number } {
    const triangle = new Triangle2D(pts[t.a], pts[t.b], pts[t.c]);
    const center = triangle.Circumcenter();
    return { center, radiusSquared: pts[t.a].DistanceSquared(center) };
}

export function delaunay(points: Point2D[]): Triangle2[] {
    const n = points.length;
    // Vertices n .. n+2 form a "super triangle" enclosing every input point.
    const pts = points.concat([
        new Point2D(0, -1000), new Point2D(1000, 1000), new Point2D(-1000, 1000)]);
    let triangles: Triangle2[] = [{ a: n, b: n + 1, c: n + 2 }];

    for (let i = 0; i < n; i++) {
        const p = pts[i];
        const bad: Triangle2[] = [];
        const good: Triangle2[] = [];
        for (const t of triangles) {
            const cc = circumcircle(pts, t);
            (p.DistanceSquared(cc.center) < cc.radiusSquared ? bad : good).push(t);
        }
        // Edges appearing in exactly one bad triangle form the cavity boundary.
        const boundary = new Map<string, [number, number]>();
        for (const t of bad) {
            for (const [a, b] of [[t.a, t.b], [t.b, t.c], [t.c, t.a]] as const) {
                const key = a < b ? `${a}_${b}` : `${b}_${a}`;
                if (boundary.has(key)) boundary.delete(key);
                else boundary.set(key, [a, b]);
            }
        }
        triangles = good;
        for (const [a, b] of boundary.values())
            triangles.push({ a, b, c: i });
    }

    // Drop every triangle that touches the super triangle.
    return triangles.filter(t => t.a < n && t.b < n && t.c < n);
}

/** The triangulation as a mesh in the y = 0 plane. */
export const triangulationMesh = (points: Point2D[], triangles: Triangle2[]): TriangleMesh3D =>
    meshFromIndices(
        points.map(p => new Point3D(p.X, 0, p.Y)),
        triangles.flatMap(t => [t.a, t.b, t.c]));

export const delaunaySample: Sample = {
    id: 'delaunay',
    title: 'Delaunay Triangulation',
    description: 'Bowyer-Watson insertion over Triangle2D.Circumcenter, on a Halton point set.',
    build(): Drawable[] {
        const region = new Bounds2D(new Point2D(-1.6, -1.2), new Point2D(1.6, 1.2));
        const points = toArray(region.HaltonPoints2D(120, 2, 3));
        const triangles = delaunay(points);

        const wire: Line3D[] = [];
        const at = (i: number) => new Point3D(points[i].X, 0.001, points[i].Y);
        for (const t of triangles)
            for (const [a, b] of [[t.a, t.b], [t.b, t.c], [t.c, t.a]])
                wire.push(new Line3D(at(a), at(b)));

        return [
            {
                kind: 'mesh',
                mesh: triangulationMesh(points, triangles),
                color: 0x27435f,
                flatShading: true,
            },
            { kind: 'lines', segments: wire, color: 0x7fc4ff },
            {
                kind: 'points',
                points: points.map(p => new Point3D(p.X, 0, p.Y)),
                color: 0xffd166,
                size: 0.05,
            },
        ];
    },
};
