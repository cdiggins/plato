// Deterministic pseudo-random numbers (mulberry32) so that samples and tests
// always produce the same output.

export type Rng = () => number;

export function makeRng(seed: number): Rng {
    let a = seed >>> 0;
    return () => {
        a |= 0;
        a = (a + 0x6d2b79f5) | 0;
        let t = Math.imul(a ^ (a >>> 15), 1 | a);
        t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
        return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
}

/** Random number in [min, max). */
export const range = (rng: Rng, min: number, max: number): number => min + rng() * (max - min);
