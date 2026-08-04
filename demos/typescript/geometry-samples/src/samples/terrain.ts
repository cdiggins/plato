// Heightfield terrain from fractal value noise.
//
// The noise itself is the stdlib's ValueNoise2D: random values on the integer
// lattice, smoothly faded between lattice points. Fractal Brownian motion
// (fBm) sums several octaves at doubling frequency and halving amplitude.
// The result displaces a grid mesh vertically.

import type { Sample } from '../core/types.js';
import { Point2D, ValueNoise2D, Vector3D } from '../plato/plato.g.js';
import { gridMesh } from '../core/meshBuilder.js';

const noise = ValueNoise2D.Create(1234, 1);

/** Stdlib value noise at (x, z), in [-1, 1]. */
export const valueNoise = (x: number, z: number): number =>
    noise.Eval(new Point2D(x, z));

export function fbm(x: number, z: number, octaves = 4): number {
    let sum = 0, amplitude = 0.5, frequency = 1, total = 0;
    for (let i = 0; i < octaves; i++) {
        sum += amplitude * valueNoise(x * frequency, z * frequency);
        total += amplitude;
        amplitude *= 0.5;
        frequency *= 2;
    }
    return sum / total; // normalized back to [-1, 1]
}

export const terrainSample: Sample = {
    id: 'terrain',
    title: 'Value-Noise Terrain',
    description: 'Fractal Brownian motion over a hashed lattice displaces a heightfield grid.',
    build() {
        const size = 3.2, height = 0.9;
        const mesh = gridMesh(120, 120, (u, v) =>
            new Vector3D(
                (u - 0.5) * size,
                fbm(u * 5, v * 5) * height * 0.5,
                (v - 0.5) * size));
        mesh.color = 0x6fbf73;
        mesh.flatShading = true;
        return [mesh];
    },
};
