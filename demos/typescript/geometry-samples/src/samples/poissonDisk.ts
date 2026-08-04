// Blue noise vs. white noise.
//
// `Bounds2D.PoissonDiskPoints2D(radius, attempts, seed)` is the stdlib's
// Poisson-disk sampler: no two points closer than `radius`, but otherwise as
// tightly packed as that constraint allows. Beside it, the same number of
// independent uniform samples drawn with `SampleUnit` — same density, visibly
// worse distribution: clumps and voids, which is what "white noise" looks like
// and why blue noise is preferred for sampling patterns.
//
// The stdlib sampler is stateless: it takes a seed rather than carrying a
// generator, so the same arguments always give the same point set.

import type { Drawable, Sample } from '../core/types.js';
import { Bounds2D, Line3D, Point2D, Point3D } from '../plato/plato.g.js';
import { toArray } from '../core/meshBuilder.js';
import { makeRng, range } from '../core/random.js';

const RADIUS = 0.11;
const ATTEMPTS = 30;

/** Lifts a planar sample into the drawing plane, offset to its panel. */
const lift = (p: Point2D, dx: number): Point3D => new Point3D(p.X + dx, 0, p.Y);

export const poissonPoints = (region: Bounds2D): Point2D[] =>
    toArray(region.PoissonDiskPoints2D(RADIUS, ATTEMPTS, 31));

export const poissonDiskSample: Sample = {
    id: 'poisson-disk',
    title: 'Poisson Disk Sampling',
    description: 'Bounds2D.PoissonDiskPoints2D beside plain white noise at the same count.',
    build(): Drawable[] {
        const region = new Bounds2D(new Point2D(-1.1, -1.1), new Point2D(1.1, 1.1));
        const blue = poissonPoints(region);

        // The same number of uniformly random points, for comparison.
        const rng = makeRng(31);
        const white = blue.map(() => new Point2D(range(rng, -1.1, 1.1), range(rng, -1.1, 1.1)));

        const panel = (dx: number): Line3D[] => {
            const corners = [
                new Point3D(dx - 1.1, 0, -1.1), new Point3D(dx + 1.1, 0, -1.1),
                new Point3D(dx + 1.1, 0, 1.1), new Point3D(dx - 1.1, 0, 1.1),
            ];
            return corners.map((c, i) => new Line3D(c, corners[(i + 1) % 4]));
        };

        return [
            { kind: 'points', points: blue.map(p => lift(p, -1.3)), color: 0x7fc4ff, size: 0.06 },
            { kind: 'points', points: white.map(p => lift(p, 1.3)), color: 0xff8c66, size: 0.06 },
            { kind: 'lines', segments: [...panel(-1.3), ...panel(1.3)], color: 0x39414e },
        ];
    },
};
