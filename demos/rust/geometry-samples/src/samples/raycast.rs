// Ray-triangle intersection (Moller-Trumbore).
//
// Solves the ray/triangle equation directly with two cross products, giving
// the barycentric coordinates (u, v) and the distance t in one pass, with no
// precomputed plane. Written fluently against the Plato vector library -
// compare with the C# and TypeScript versions, which read identically.

use crate::core::mesh_builder::{push_vector, vertex_at};
use crate::core::types::{Drawable, MeshData};
use crate::plato::*;

use super::icosphere::build_icosphere;

pub struct RayHit {
    pub t: f64,
    pub point: Vector3D,
    pub triangle: usize,
}

pub fn intersect_triangle(
    origin: Vector3D,
    dir: Vector3D,
    a: Vector3D,
    b: Vector3D,
    c: Vector3D,
) -> Option<f64> {
    let e1 = b.Subtract(a);
    let e2 = c.Subtract(a);
    let p = dir.Cross(e2);
    let det = e1.Dot(p);
    if det.Abs() < 1e-10 {
        return None; // parallel to the triangle plane
    }
    let inv = 1.0 / det;
    let s = origin.Subtract(a);
    let u = s.Dot(p) * inv;
    if !(0.0..=1.0).contains(&u) {
        return None;
    }
    let q = s.Cross(e1);
    let v = dir.Dot(q) * inv;
    if v < 0.0 || u + v > 1.0 {
        return None;
    }
    let t = e2.Dot(q) * inv;
    if t > 1e-9 {
        Some(t)
    } else {
        None
    }
}

/// Closest hit against every triangle (brute force; see the BVH sample).
pub fn raycast_mesh(mesh: &MeshData, origin: Vector3D, dir: Vector3D) -> Option<RayHit> {
    let mut best: Option<RayHit> = None;
    for tri in 0..mesh.indices.len() / 3 {
        let a = vertex_at(&mesh.positions, mesh.indices[tri * 3] as usize);
        let b = vertex_at(&mesh.positions, mesh.indices[tri * 3 + 1] as usize);
        let c = vertex_at(&mesh.positions, mesh.indices[tri * 3 + 2] as usize);
        if let Some(t) = intersect_triangle(origin, dir, a, b, c) {
            if best.as_ref().is_none_or(|h| t < h.t) {
                best = Some(RayHit { t, point: origin.Add(dir.Scale(t)), triangle: tri });
            }
        }
    }
    best
}

pub fn build() -> Vec<Drawable> {
    let mesh = build_icosphere(2);

    let mut rays: Vec<f64> = Vec::new();
    let mut hits: Vec<f64> = Vec::new();
    let mut misses: Vec<f64> = Vec::new();
    let dir = Vector3D::new(0.0, 0.0, -1.0);
    for iy in 0..9 {
        for ix in 0..9 {
            let origin = Vector3D::new(
                (ix as f64 / 8.0 - 0.5) * 2.6,
                (iy as f64 / 8.0 - 0.5) * 2.6,
                2.2,
            );
            match raycast_mesh(&mesh, origin, dir) {
                Some(hit) => {
                    push_vector(&mut rays, origin);
                    push_vector(&mut rays, hit.point);
                    push_vector(&mut hits, hit.point);
                }
                None => {
                    push_vector(&mut misses, origin);
                    push_vector(&mut misses, origin.Add(dir.Scale(4.4)));
                }
            }
        }
    }

    vec![
        mesh.into_drawable(),
        Drawable::Lines { positions: rays },
        Drawable::Lines { positions: misses },
        Drawable::Points { positions: hits },
    ]
}
