// 2D convex hull (Andrew's monotone chain), displayed as an extruded prism.
//
// The algorithm now lives in the stdlib: `ConvexHull`
// (stdlib/geometry/geometry.library.plato) sorts the points lexicographically
// with `SortedIndices` and grows the lower and upper chains, popping every
// corner that fails to turn strictly left. This driver only feeds it points
// and draws the result — the situation plato-442 existed to end.

import type { Drawable, Sample } from '../core/types.js';
import {
    Bounds2D, ConvexHull, ConvexHull2D, Line3D, Point2D, Point3D, TriangleMesh3D,
} from '../plato/plato.g.js';
import { fromArray, meshFromIndices, toArray } from '../core/meshBuilder.js';

/** Positive when o -> a -> b turns counter-clockwise. */
export const turn = (o: Point2D, a: Point2D, b: Point2D): number => o.TwiceSignedArea(a, b);

/** The hull as a stdlib ConvexHull2D, computed by the stdlib's own builder. */
export function convexHull(points: Point2D[]): ConvexHull2D {
    return ConvexHull(fromArray(points));
}

/** Extrudes a convex CCW polygon into a prism mesh (bottom y = 0, top y = h). */
export function extrudePolygon(polygon: Point2D[], h: number): TriangleMesh3D {
    const n = polygon.length;
    const positions = polygon.map(p => new Point3D(p.X, 0, p.Y))
        .concat(polygon.map(p => new Point3D(p.X, h, p.Y)));
    const indices: number[] = [];
    for (let i = 2; i < n; i++) indices.push(0, i, i - 1);             // bottom fan
    for (let i = 2; i < n; i++) indices.push(n, n + i - 1, n + i);     // top fan
    for (let i = 0; i < n; i++) {                                      // sides
        const j = (i + 1) % n;
        indices.push(i, j, n + i, j, n + j, n + i);
    }
    return meshFromIndices(positions, indices);
}

export const convexHullSample: Sample = {
    id: 'convex-hull',
    title: 'Convex Hull',
    description: "The stdlib's ConvexHull builder: monotone chain over SortedIndices.",
    build(): Drawable[] {
        // A low-discrepancy point set, squashed into an ellipse.
        const region = new Bounds2D(new Point2D(-1.7, -1.4), new Point2D(1.7, 1.4));
        const points = toArray(region.HaltonPoints2D(90, 2, 3));
        const hull = convexHull(points);
        const boundary = toArray(hull.Hull.Points);
        const height = 0.35;

        const outline: Line3D[] = [];
        for (let i = 0; i < boundary.length; i++) {
            const a = boundary[i], b = boundary[(i + 1) % boundary.length];
            for (const y of [0, height])
                outline.push(new Line3D(new Point3D(a.X, y, a.Y), new Point3D(b.X, y, b.Y)));
        }

        return [
            {
                kind: 'mesh',
                mesh: extrudePolygon(boundary, height),
                color: 0x4da3ff,
                opacity: 0.35,
                flatShading: true,
            },
            { kind: 'lines', segments: outline, color: 0x9fd0ff },
            {
                kind: 'points',
                points: points.map(p => new Point3D(p.X, 0.02, p.Y)),
                color: 0xffd166,
                size: 0.06,
            },
        ];
    },
};
