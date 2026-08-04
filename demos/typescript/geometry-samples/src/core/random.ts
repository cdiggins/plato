// Deterministic pseudo-random numbers, backed by the Plato stdlib sampling
// vocabulary: SampleUnit(index, stream) on a seed is a stateless, hash-based
// uniform draw in [0, 1), so samples and tests always produce the same output.

import '../plato/plato.g.js';

export type Rng = () => number;

export function makeRng(seed: number): Rng {
    let i = 0;
    return () => seed.SampleUnit(i++, 0);
}

/** Random number in [min, max). */
export const range = (rng: Rng, min: number, max: number): number => min + rng() * (max - min);
