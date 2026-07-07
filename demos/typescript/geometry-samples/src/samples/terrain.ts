// Heightfield terrain from fractal value noise.
//
// Value noise: hash the integer lattice, then interpolate with a smoothstep.
// Fractal Brownian motion (fBm) sums several octaves at doubling frequency
// and halving amplitude. The result displaces a grid mesh vertically.

import type { Sample } from '../core/types.js';
import { Vector3D } from '../plato/plato.g.js';
import { gridMesh } from '../core/meshBuilder.js';

/** Deterministic pseudo-random value in [0, 1) for an integer lattice point. */
function latticeHash(ix: number, iz: number): number {
    const h = Math.sin(ix * 127.1 + iz * 311.7) * 43758.5453;
    return h - h.Floor();
}

const smoothstep = (t: number): number => t * t * (3 - 2 * t);

export function valueNoise(x: number, z: number): number {
    const ix = x.Floor(), iz = z.Floor();
    const fx = smoothstep(x - ix), fz = smoothstep(z - iz);
    const v00 = latticeHash(ix, iz), v10 = latticeHash(ix + 1, iz);
    const v01 = latticeHash(ix, iz + 1), v11 = latticeHash(ix + 1, iz + 1);
    return v00.Lerp(v10, fx).Lerp(v01.Lerp(v11, fx), fz); // bilinear, in [0, 1)
}

export function fbm(x: number, z: number, octaves = 4): number {
    let sum = 0, amplitude = 0.5, frequency = 1, total = 0;
    for (let i = 0; i < octaves; i++) {
        sum += amplitude * valueNoise(x * frequency, z * frequency);
        total += amplitude;
        amplitude *= 0.5;
        frequency *= 2;
    }
    return sum / total; // normalized back to [0, 1)
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
                fbm(u * 5, v * 5) * height - height * 0.5,
                (v - 0.5) * size));
        mesh.color = 0x6fbf73;
        mesh.flatShading = true;
        return [mesh];
    },
};
