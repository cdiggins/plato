// Poisson disk sampling (Bridson's algorithm).
//
// Produces "blue noise": points at least r apart but tightly packed. A
// background grid with cell size r / sqrt(2) holds at most one sample per
// cell, so a distance check only needs to look at nearby cells - O(n) total.

import type { Sample } from '../core/types.js';
import { Vector2D } from '../plato/plato.g.js';
import { makeRng, range, type Rng } from '../core/random.js';

export function poissonDisk(width: number, height: number, r: number, rng: Rng, k = 30): Vector2D[] {
    const cell = r / Math.SQRT2;
    const gw = (width / cell).Floor() + 1;
    const gh = (height / cell).Floor() + 1;
    const grid = new Array<number>(gw * gh).fill(-1);
    const samples: Vector2D[] = [];
    const active: number[] = [];

    const gridIndex = (p: Vector2D) =>
        (p.Y / cell).Floor().Min(gh - 1) * gw + (p.X / cell).Floor().Min(gw - 1);

    const farFromNeighbors = (p: Vector2D): boolean => {
        const gx = (p.X / cell).Floor(), gy = (p.Y / cell).Floor();
        for (let y = (gy - 2).Max(0); y <= (gy + 2).Min(gh - 1); y++) {
            for (let x = (gx - 2).Max(0); x <= (gx + 2).Min(gw - 1); x++) {
                const s = grid[y * gw + x];
                if (s >= 0 && samples[s].DistanceSquared(p) < r * r)
                    return false;
            }
        }
        return true;
    };

    const emit = (p: Vector2D) => {
        grid[gridIndex(p)] = samples.length;
        active.push(samples.length);
        samples.push(p);
    };
    emit(new Vector2D(width / 2, height / 2));

    while (active.length > 0) {
        const pick = (rng() * active.length).Floor();
        const around = samples[active[pick]];
        let placed = false;
        for (let attempt = 0; attempt < k; attempt++) {
            // Uniform in the annulus [r, 2r] around the active sample.
            const angle = rng().Turns();
            const dist = r * range(rng, 1, 4).Sqrt();
            const p = around.Add(new Vector2D(angle.Cos() * dist, angle.Sin() * dist));
            if (p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height && farFromNeighbors(p)) {
                emit(p);
                placed = true;
                break;
            }
        }
        if (!placed)
            active.splice(pick, 1); // exhausted: retire this sample
    }
    return samples;
}

export const poissonDiskSample: Sample = {
    id: 'poisson-disk',
    title: 'Poisson Disk Sampling',
    description: "Bridson's blue-noise sampling next to plain white noise, same point count.",
    build() {
        const rng = makeRng(31);
        const blue = poissonDisk(2.2, 2.2, 0.11, rng);

        const bluePts: number[] = [];
        for (const p of blue)
            bluePts.push(p.X - 2.4, 0, p.Y - 1.1);

        // The same number of uniformly random points, for comparison.
        const whitePts: number[] = [];
        for (let i = 0; i < blue.length; i++)
            whitePts.push(range(rng, 0.2, 2.4), 0, range(rng, -1.1, 1.1));

        const frame: number[] = [];
        for (const x0 of [-2.4, 0.2])
            frame.push(
                x0, 0, -1.1, x0 + 2.2, 0, -1.1, x0 + 2.2, 0, -1.1, x0 + 2.2, 0, 1.1,
                x0 + 2.2, 0, 1.1, x0, 0, 1.1, x0, 0, 1.1, x0, 0, -1.1);

        return [
            { kind: 'points', positions: bluePts, color: 0x7fc4ff, size: 0.06 },
            { kind: 'points', positions: whitePts, color: 0xff8c66, size: 0.06 },
            { kind: 'lines', positions: frame, color: 0x39414e },
        ];
    },
};
