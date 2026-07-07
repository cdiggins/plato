// 2D convex hull (Andrew's monotone chain), displayed as an extruded prism.
//
// Sort points lexicographically, then build the lower and upper chains,
// popping vertices while the turn is not counter-clockwise. The turn test is
// the 2D cross product: (a - o).Cross(b - o). Runs in O(n log n).

import type { Sample } from '../core/types.js';
import { Vector2D } from '../plato/plato.g.js';
import { makeRng, range } from '../core/random.js';
import { computeVertexNormals } from '../core/meshBuilder.js';

export const turn = (o: Vector2D, a: Vector2D, b: Vector2D): number =>
    a.Subtract(o).Cross(b.Subtract(o));

/** Returns the hull as point indices in counter-clockwise order. */
export function convexHull(points: Vector2D[]): number[] {
    const idx = points.map((_, i) => i).sort((i, j) =>
        points[i].X - points[j].X || points[i].Y - points[j].Y);

    const lower: number[] = [];
    for (const i of idx) {
        while (lower.length >= 2 &&
               turn(points[lower[lower.length - 2]], points[lower[lower.length - 1]], points[i]) <= 0)
            lower.pop();
        lower.push(i);
    }
    const upper: number[] = [];
    for (const i of idx.slice().reverse()) {
        while (upper.length >= 2 &&
               turn(points[upper[upper.length - 2]], points[upper[upper.length - 1]], points[i]) <= 0)
            upper.pop();
        upper.push(i);
    }
    return lower.slice(0, -1).concat(upper.slice(0, -1));
}

/** Extrudes a convex CCW polygon into a prism mesh (bottom y=0, top y=h). */
export function extrudePolygon(polygon: Vector2D[], h: number) {
    const n = polygon.length;
    const positions: number[] = [];
    for (const p of polygon) positions.push(p.X, 0, p.Y);
    for (const p of polygon) positions.push(p.X, h, p.Y);
    const indices: number[] = [];
    for (let i = 2; i < n; i++) indices.push(0, i, i - 1);             // bottom fan
    for (let i = 2; i < n; i++) indices.push(n, n + i - 1, n + i);     // top fan
    for (let i = 0; i < n; i++) {                                      // sides
        const j = (i + 1) % n;
        indices.push(i, j, n + i, j, n + j, n + i);
    }
    return { positions, indices };
}

export const convexHullSample: Sample = {
    id: 'convex-hull',
    title: 'Convex Hull',
    description: "Andrew's monotone chain in O(n log n), extruded into a prism.",
    build() {
        const rng = makeRng(11);
        const points: Vector2D[] = [];
        for (let i = 0; i < 90; i++) {
            const theta = rng().Turns();
            const r = 1.4 * rng().Sqrt();
            points.push(new Vector2D(r * theta.Cos() * 1.2, r * theta.Sin()));
        }
        const hull = convexHull(points).map(i => points[i]);
        const { positions, indices } = extrudePolygon(hull, 0.35);

        const outline: number[] = [];
        for (let i = 0; i < hull.length; i++) {
            const a = hull[i], b = hull[(i + 1) % hull.length];
            for (const y of [0, 0.35])
                outline.push(a.X, y, a.Y, b.X, y, b.Y);
        }
        const cloud: number[] = [];
        for (const p of points) cloud.push(p.X, 0.02, p.Y);

        return [
            { kind: 'mesh', positions, indices, normals: computeVertexNormals(positions, indices), color: 0x4da3ff, opacity: 0.35, flatShading: true },
            { kind: 'lines', positions: outline, color: 0x9fd0ff },
            { kind: 'points', positions: cloud, color: 0xffd166, size: 0.06 },
        ];
    },
};
