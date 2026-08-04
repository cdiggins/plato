// Terrain as the graph of a scalar field.
//
// Two stdlib pieces meet here. `ValueNoise2D` is the stdlib's value noise:
// one hashed value per integer lattice corner, faded between them. Summing
// octaves at doubling frequency and halving amplitude is fractal Brownian
// motion; the sum is wrapped in `ScalarFunctionField2D`, the stdlib's
// function-backed scalar field, which makes it an IScalarField2D like any
// other. `ToTriangleMesh(domain, nCols, nRows)` then draws any such field as
// the surface z = f(x, y).
//
// fBm is composed here rather than using the stdlib's own `FbmNoise2D` because
// that type's Basis field is the sum type NoiseBasis, which has no TypeScript
// surface (CHK320) — see plato-440.

import type { Drawable, Sample } from '../core/types.js';
import {
    Bounds2D, Point2D, ScalarFunctionField2D, ValueNoise2D,
} from '../plato/plato.g.js';

const octave = ValueNoise2D.Create(1234, 1);

/** Stdlib value noise at (x, y), in [-1, 1]. */
export const valueNoise = (x: number, y: number): number =>
    octave.Eval(new Point2D(x, y));

/** Fractal Brownian motion: octaves at doubling frequency, halving amplitude. */
export function fbm(p: Point2D, octaves = 5): number {
    let sum = 0, amplitude = 0.5, frequency = 1, total = 0;
    for (let i = 0; i < octaves; i++) {
        sum += amplitude * valueNoise(p.X * frequency, p.Y * frequency);
        total += amplitude;
        amplitude *= 0.5;
        frequency *= 2;
    }
    return sum / total; // back to [-1, 1]
}

/** The fBm sum as a stdlib scalar field, scaled to the terrain's relief. */
export const terrainField = ScalarFunctionField2D.Create(p => fbm(p) * 0.45);

export const terrainSample: Sample = {
    id: 'terrain',
    title: 'Value-Noise Terrain',
    description: 'ScalarFunctionField2D over stdlib ValueNoise2D, drawn as its graph ' +
        'by IScalarField2D.ToTriangleMesh.',
    build(): Drawable[] {
        const domain = new Bounds2D(new Point2D(-1.6, -1.6), new Point2D(1.6, 1.6));
        return [{
            kind: 'mesh',
            mesh: terrainField.ToTriangleMesh(domain, 140, 140),
            color: 0x6fbf73,
            flatShading: true,
        }];
    },
};
