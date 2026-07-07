// Half-edge mesh data structure + Laplacian smoothing.
//
// Every triangle contributes three directed half-edges. Each half-edge knows
// its origin vertex, the next half-edge around its face, and its twin (the
// opposite direction across the shared edge). This makes one-ring vertex
// traversal O(degree) with no searching: twin(next(next(e))) is the next
// outgoing edge around the origin of e.

use std::collections::HashMap;

use crate::core::mesh_builder::{translate_mesh, vertex_at};
use crate::core::types::{Drawable, MeshData};
use crate::plato::*;

use super::icosphere::build_icosphere;
use super::terrain::value_noise;

#[derive(Clone, Copy, Debug)]
pub struct HalfEdge {
    /// Vertex index at the start of the edge.
    pub origin: usize,
    /// Next half-edge counter-clockwise around the face.
    pub next: usize,
    /// Opposite half-edge, or -1 on a boundary.
    pub twin: isize,
}

pub struct HalfEdgeMesh {
    pub half_edges: Vec<HalfEdge>,
    /// One outgoing half-edge per vertex.
    pub vertex_edge: Vec<isize>,
    pub vertex_count: usize,
}

pub fn build_half_edge_mesh(indices: &[u32], vertex_count: usize) -> HalfEdgeMesh {
    let mut half_edges: Vec<HalfEdge> = Vec::new();
    let mut vertex_edge: Vec<isize> = vec![-1; vertex_count];
    let mut edge_map: HashMap<(usize, usize), usize> = HashMap::new(); // (from, to) -> half-edge index

    for f in (0..indices.len()).step_by(3) {
        let base = half_edges.len();
        for k in 0..3 {
            let from = indices[f + k] as usize;
            let to = indices[f + ((k + 1) % 3)] as usize;
            half_edges.push(HalfEdge { origin: from, next: base + ((k + 1) % 3), twin: -1 });
            edge_map.insert((from, to), base + k);
            vertex_edge[from] = (base + k) as isize;
        }
    }
    for (&(from, to), &e) in &edge_map {
        if let Some(&twin) = edge_map.get(&(to, from)) {
            half_edges[e].twin = twin as isize;
        }
    }
    HalfEdgeMesh { half_edges, vertex_edge, vertex_count }
}

/// Visits the one-ring neighbor vertices of v (closed meshes).
pub fn one_ring(mesh: &HalfEdgeMesh, v: usize) -> Vec<usize> {
    let mut result: Vec<usize> = Vec::new();
    let start = mesh.vertex_edge[v];
    let mut e = start;
    loop {
        let he = mesh.half_edges[e as usize];
        result.push(mesh.half_edges[he.next].origin);
        e = mesh.half_edges[mesh.half_edges[he.next].next].twin; // next outgoing edge
        if e == start || e == -1 {
            break;
        }
    }
    result
}

/// Uniform Laplacian smoothing: move each vertex toward its ring average.
pub fn laplacian_smooth(
    positions: &[f64],
    mesh: &HalfEdgeMesh,
    lambda: f64,
    iterations: usize,
) -> Vec<f64> {
    let mut current = positions.to_vec();
    for _ in 0..iterations {
        let mut next = current.clone();
        for v in 0..mesh.vertex_count {
            let ring = one_ring(mesh, v);
            let mut avg = Vector3D::new(0.0, 0.0, 0.0);
            for &n in &ring {
                avg = avg.Add(vertex_at(&current, n));
            }
            avg = avg.Scale(1.0 / ring.len() as f64);
            let moved = vertex_at(&current, v).Lerp(avg, lambda);
            next[v * 3] = moved.X;
            next[v * 3 + 1] = moved.Y;
            next[v * 3 + 2] = moved.Z;
        }
        current = next;
    }
    current
}

/// An icosphere with noisy radius: the smoothing test subject.
pub fn noisy_sphere() -> MeshData {
    let mut mesh = build_icosphere(3);
    for i in (0..mesh.positions.len()).step_by(3) {
        let dir = vertex_at(&mesh.positions, i / 3).Normalize();
        let bump = 1.0 + 0.28 * (value_noise(dir.X * 5.0 + 5.0, dir.Y * 5.0 + dir.Z * 4.0 + 5.0) - 0.5) * 2.0;
        let p = dir.Scale(bump);
        mesh.positions[i] = p.X;
        mesh.positions[i + 1] = p.Y;
        mesh.positions[i + 2] = p.Z;
    }
    mesh
}

pub fn build() -> Vec<Drawable> {
    let mut noisy = noisy_sphere();
    let he = build_half_edge_mesh(&noisy.indices, noisy.positions.len() / 3);
    let smoothed_positions = laplacian_smooth(&noisy.positions, &he, 0.6, 12);

    let mut smoothed = MeshData { positions: smoothed_positions, indices: noisy.indices.clone() };
    translate_mesh(&mut noisy, Vector3D::new(-1.4, 0.0, 0.0));
    translate_mesh(&mut smoothed, Vector3D::new(1.4, 0.0, 0.0));
    vec![noisy.into_drawable(), smoothed.into_drawable()]
}
