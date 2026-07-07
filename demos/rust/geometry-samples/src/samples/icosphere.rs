// Icosphere by recursive subdivision.
//
// Start from a regular icosahedron and repeatedly split every triangle into
// four, projecting new vertices onto the unit sphere. A midpoint cache keyed
// on the edge guarantees that shared edges reuse the same vertex, keeping the
// mesh watertight (V - E + F = 2).

use std::collections::HashMap;

use crate::core::mesh_builder::mesh_from_vertices;
use crate::core::types::{Drawable, MeshData};
use crate::plato::*;

pub fn build_icosphere(subdivisions: usize) -> MeshData {
    let t = (1.0 + (5.0).Sqrt()) / 2.0;
    let mut vertices: Vec<Vector3D> = [
        Vector3D::new(-1.0, t, 0.0),
        Vector3D::new(1.0, t, 0.0),
        Vector3D::new(-1.0, -t, 0.0),
        Vector3D::new(1.0, -t, 0.0),
        Vector3D::new(0.0, -1.0, t),
        Vector3D::new(0.0, 1.0, t),
        Vector3D::new(0.0, -1.0, -t),
        Vector3D::new(0.0, 1.0, -t),
        Vector3D::new(t, 0.0, -1.0),
        Vector3D::new(t, 0.0, 1.0),
        Vector3D::new(-t, 0.0, -1.0),
        Vector3D::new(-t, 0.0, 1.0),
    ]
    .iter()
    .map(|v| v.Normalize())
    .collect();
    let mut faces: Vec<u32> = vec![
        0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, //
        1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8, //
        3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, //
        4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
    ];

    for _level in 0..subdivisions {
        let mut cache: HashMap<(u32, u32), u32> = HashMap::new();
        let mut next: Vec<u32> = Vec::new();
        for i in (0..faces.len()).step_by(3) {
            let (a, b, c) = (faces[i], faces[i + 1], faces[i + 2]);
            let mut midpoint_index = |a: u32, b: u32, vertices: &mut Vec<Vector3D>| -> u32 {
                let key = if a < b { (a, b) } else { (b, a) };
                *cache.entry(key).or_insert_with(|| {
                    let index = vertices.len() as u32;
                    let mid = vertices[a as usize].MidPoint(vertices[b as usize]).Normalize();
                    vertices.push(mid);
                    index
                })
            };
            let ab = midpoint_index(a, b, &mut vertices);
            let bc = midpoint_index(b, c, &mut vertices);
            let ca = midpoint_index(c, a, &mut vertices);
            next.extend_from_slice(&[a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca]);
        }
        faces = next;
    }

    mesh_from_vertices(&vertices, faces)
}

pub fn build() -> Vec<Drawable> {
    let levels = [0usize, 1, 2, 3];
    levels
        .iter()
        .enumerate()
        .map(|(i, &level)| {
            let mut mesh = build_icosphere(level);
            for j in (0..mesh.positions.len()).step_by(3) {
                mesh.positions[j] += (i as f64 - (levels.len() as f64 - 1.0) / 2.0) * 2.3;
            }
            mesh.into_drawable()
        })
        .collect()
}
