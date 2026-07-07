// Marching squares: extracting iso-contours of a scalar field.
//
// Sample the field on a grid; each cell gets a 4-bit mask from which corners
// are above the iso value. A 16-entry table says which cell edges the contour
// crosses, and linear interpolation places the crossing precisely. This is
// the 2D sibling of marching cubes. The field here is a set of "metaballs".

import type { Sample } from '../core/types.js';
import { Vector2D } from '../plato/plato.g.js';

export type Field2 = (x: number, y: number) => number;

/** Edge order: 0 = bottom, 1 = right, 2 = top, 3 = left. */
const CASES: number[][] = [
    [], [3, 0], [0, 1], [3, 1], [1, 2], [3, 0, 1, 2], [0, 2], [3, 2],
    [2, 3], [0, 2], [0, 1, 2, 3], [1, 2], [1, 3], [0, 1], [3, 0], [],
];

export function marchingSquares(
    f: Field2, iso: number, min: Vector2D, max: Vector2D, resolution: number): Vector2D[] {
    const segments: Vector2D[] = []; // consecutive pairs form line segments
    const dx = (max.X - min.X) / resolution;
    const dy = (max.Y - min.Y) / resolution;

    for (let j = 0; j < resolution; j++) {
        for (let i = 0; i < resolution; i++) {
            const x0 = min.X + i * dx, y0 = min.Y + j * dy;
            const x1 = x0 + dx, y1 = y0 + dy;
            const v = [f(x0, y0), f(x1, y0), f(x1, y1), f(x0, y1)];
            const mask = (v[0] > iso ? 1 : 0) | (v[1] > iso ? 2 : 0) |
                         (v[2] > iso ? 4 : 0) | (v[3] > iso ? 8 : 0);

            // Where does the contour cross each edge? (linear interpolation)
            const lerpT = (a: number, b: number) => (iso - a) / (b - a);
            const onEdge = (e: number): Vector2D => {
                switch (e) {
                    case 0: return new Vector2D(x0 + dx * lerpT(v[0], v[1]), y0);
                    case 1: return new Vector2D(x1, y0 + dy * lerpT(v[1], v[2]));
                    case 2: return new Vector2D(x0 + dx * lerpT(v[3], v[2]), y1);
                    default: return new Vector2D(x0, y0 + dy * lerpT(v[0], v[3]));
                }
            };
            const edges = CASES[mask];
            for (let e = 0; e < edges.length; e += 2)
                segments.push(onEdge(edges[e]), onEdge(edges[e + 1]));
        }
    }
    return segments;
}

/** Three metaballs: smooth field with interesting topology changes. */
export const metaballs: Field2 = (x, y) => {
    const point = new Vector2D(x, y);
    const blobs = [
        { center: new Vector2D(-0.5, 0.1), weight: 0.28 },
        { center: new Vector2D(0.45, 0.25), weight: 0.22 },
        { center: new Vector2D(0.1, -0.45), weight: 0.18 },
    ];
    let sum = 0;
    for (const b of blobs)
        sum += b.weight / (point.DistanceSquared(b.center) + 0.03);
    return sum;
};

export const marchingSquaresSample: Sample = {
    id: 'marching-squares',
    title: 'Marching Squares',
    description: 'Iso-contours of a metaball field via the 16-case cell lookup table.',
    build() {
        const min = new Vector2D(-1.6, -1.4), max = new Vector2D(1.6, 1.4);
        const isoLevels = [
            { iso: 1.0, color: 0x4da3ff, y: 0.0 },
            { iso: 1.8, color: 0xffd166, y: 0.012 },
            { iso: 3.2, color: 0xff6b6b, y: 0.024 },
        ];
        return isoLevels.map(({ iso, color, y }) => {
            const segs = marchingSquares(metaballs, iso, min, max, 110);
            const positions: number[] = [];
            for (const p of segs)
                positions.push(p.X, y, p.Y);
            return { kind: 'lines', positions, color } as const;
        });
    },
};
