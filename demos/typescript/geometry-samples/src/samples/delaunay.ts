// Delaunay triangulation (Bowyer-Watson).
//
// Insert points one at a time: find all triangles whose circumcircle contains
// the new point (the "bad" triangles), remove them, and re-triangulate the
// boundary of the resulting cavity by fanning to the new point. A triangle
// mesh built this way maximizes the minimum angle over all triangulations.

import type { Sample } from '../core/types.js';
import { Vector2D } from '../plato/plato.g.js';
import { makeRng, range } from '../core/random.js';

export interface Triangle2 { a: number; b: number; c: number; }

export interface Circumcircle { center: Vector2D; radiusSquared: number; }

export function circumcircle(pts: Vector2D[], t: Triangle2): Circumcircle {
    const [A, B, C] = [pts[t.a], pts[t.b], pts[t.c]];
    const d = 2 * (A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y));
    const a2 = A.LengthSquared(), b2 = B.LengthSquared(), c2 = C.LengthSquared();
    const center = new Vector2D(
        (a2 * (B.Y - C.Y) + b2 * (C.Y - A.Y) + c2 * (A.Y - B.Y)) / d,
        (a2 * (C.X - B.X) + b2 * (A.X - C.X) + c2 * (B.X - A.X)) / d);
    return { center, radiusSquared: A.DistanceSquared(center) };
}

export function delaunay(points: Vector2D[]): Triangle2[] {
    const n = points.length;
    // Vertices n .. n+2 form a "super triangle" enclosing every input point.
    const pts = points.concat([
        new Vector2D(0, -1000), new Vector2D(1000, 1000), new Vector2D(-1000, 1000)]);
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

export const delaunaySample: Sample = {
    id: 'delaunay',
    title: 'Delaunay Triangulation',
    description: 'Bowyer-Watson incremental insertion with the empty-circumcircle property.',
    build() {
        const rng = makeRng(7);
        const points: Vector2D[] = [];
        for (let i = 0; i < 120; i++)
            points.push(new Vector2D(range(rng, -1.6, 1.6), range(rng, -1.2, 1.2)));
        const triangles = delaunay(points);

        const positions: number[] = [];
        for (const p of points)
            positions.push(p.X, 0, p.Y);
        const indices: number[] = [];
        for (const t of triangles)
            indices.push(t.a, t.b, t.c);

        const wire: number[] = [];
        for (const t of triangles)
            for (const [a, b] of [[t.a, t.b], [t.b, t.c], [t.c, t.a]])
                wire.push(points[a].X, 0.001, points[a].Y, points[b].X, 0.001, points[b].Y);

        return [
            { kind: 'mesh', positions, indices, color: 0x27435f, flatShading: true },
            { kind: 'lines', positions: wire, color: 0x7fc4ff },
            { kind: 'points', positions, color: 0xffd166, size: 0.05 },
        ];
    },
};
