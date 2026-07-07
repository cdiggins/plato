// Deterministic pseudo-random numbers (mulberry32) so that samples and tests
// always produce the same output. Exact port of the TypeScript version
// (u32 wrapping arithmetic; ">>>" becomes ">>" on u32), so cross-language
// outputs stay comparable for the same seeds.

pub struct Rng {
    a: u32,
}

pub fn make_rng(seed: u32) -> Rng {
    Rng { a: seed }
}

impl Rng {
    #[allow(clippy::should_implement_trait)] // matches the TS `rng()` naming, not an Iterator
    pub fn next(&mut self) -> f64 {
        self.a = self.a.wrapping_add(0x6d2b79f5);
        let a = self.a;
        let mut t = (a ^ (a >> 15)).wrapping_mul(1 | a);
        t = (t.wrapping_add((t ^ (t >> 7)).wrapping_mul(61 | t))) ^ t;
        ((t ^ (t >> 14)) as f64) / 4294967296.0
    }

    /// Random number in [min, max).
    pub fn range(&mut self, min: f64, max: f64) -> f64 {
        min + self.next() * (max - min)
    }
}
