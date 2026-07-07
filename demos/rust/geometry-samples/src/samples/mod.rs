// The sample registry: pure modules, usable from tests and future front-ends.
// Port of demos/typescript/geometry-samples/src/samples/index.ts.

pub mod bvh;
pub mod convex_hull;
pub mod delaunay;
pub mod half_edge;
pub mod icosphere;
pub mod marching_squares;
pub mod octree;
pub mod parametric_surface;
pub mod poisson_disk;
pub mod raycast;
pub mod spline_tube;
pub mod terrain;

use crate::core::types::Drawable;

/// A registered sample: its id and its build function.
pub type SampleEntry = (&'static str, fn() -> Vec<Drawable>);

pub fn samples() -> Vec<SampleEntry> {
    vec![
        ("parametric-surface", parametric_surface::build),
        ("icosphere", icosphere::build),
        ("terrain", terrain::build),
        ("delaunay", delaunay::build),
        ("convex-hull", convex_hull::build),
        ("spline-tube", spline_tube::build),
        ("octree", octree::build),
        ("bvh", bvh::build),
        ("half-edge", half_edge::build),
        ("raycast", raycast::build),
        ("poisson-disk", poisson_disk::build),
        ("marching-squares", marching_squares::build),
    ]
}
