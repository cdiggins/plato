// Heightfield terrain from fractal value noise.
//
// Value noise: hash the integer lattice, then interpolate with a smoothstep.
// Fractal Brownian motion (fBm) sums several octaves at doubling frequency
// and halving amplitude. The result displaces a grid mesh vertically.

use crate::core::mesh_builder::grid_mesh;
use crate::core::types::Drawable;
use crate::plato::*;

/// Deterministic pseudo-random value in [0, 1) for an integer lattice point.
fn lattice_hash(ix: f64, iz: f64) -> f64 {
    let h = (ix * 127.1 + iz * 311.7).sin() * 43758.5453;
    h - h.Floor()
}

fn smoothstep(t: f64) -> f64 {
    t * t * (3.0 - 2.0 * t)
}

pub fn value_noise(x: f64, z: f64) -> f64 {
    let (ix, iz) = (x.Floor(), z.Floor());
    let (fx, fz) = (smoothstep(x - ix), smoothstep(z - iz));
    let v00 = lattice_hash(ix, iz);
    let v10 = lattice_hash(ix + 1.0, iz);
    let v01 = lattice_hash(ix, iz + 1.0);
    let v11 = lattice_hash(ix + 1.0, iz + 1.0);
    v00.Lerp(v10, fx).Lerp(v01.Lerp(v11, fx), fz) // bilinear, in [0, 1)
}

pub fn fbm(x: f64, z: f64, octaves: usize) -> f64 {
    let (mut sum, mut amplitude, mut frequency, mut total) = (0.0, 0.5, 1.0, 0.0);
    for _ in 0..octaves {
        sum += amplitude * value_noise(x * frequency, z * frequency);
        total += amplitude;
        amplitude *= 0.5;
        frequency *= 2.0;
    }
    sum / total // normalized back to [0, 1)
}

pub fn build() -> Vec<Drawable> {
    let (size, height) = (3.2, 0.9);
    let mesh = grid_mesh(120, 120, |u, v| {
        Vector3D::new(
            (u - 0.5) * size,
            fbm(u * 5.0, v * 5.0, 4) * height - height * 0.5,
            (v - 0.5) * size,
        )
    });
    vec![mesh.into_drawable()]
}
