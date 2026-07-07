// Parametric surface tessellation.
//
// A surface is a function f(u, v) -> R3 over the unit square. grid_mesh samples
// it on a regular grid and stitches the samples into an indexed triangle mesh
// with area-weighted vertex normals. Here: a torus with a radial ripple,
// written fluently against the Plato library (Turns/Cos/Sin on plain numbers).

use crate::core::mesh_builder::grid_mesh;
use crate::core::types::Drawable;
use crate::plato::*;

pub fn rippled_torus(u: f64, v: f64) -> Vector3D {
    let theta = u.Turns(); // around the main ring
    let phi = v.Turns(); // around the tube
    let big_r = 1.1; // ring radius
    let r = 0.42 + 0.1 * (u * 5.0).Turns().Sin() * (v * 3.0).Turns().Sin();
    Vector3D::new(
        (big_r + r * phi.Cos()) * theta.Cos(),
        r * phi.Sin(),
        (big_r + r * phi.Cos()) * theta.Sin(),
    )
}

pub fn build() -> Vec<Drawable> {
    vec![grid_mesh(160, 48, rippled_torus).into_drawable()]
}
