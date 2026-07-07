// Helpers for constructing meshes and line lists from Plato geometry.
// Port of demos/typescript/geometry-samples/src/core/meshBuilder.ts.

use crate::plato::*;

use super::types::MeshData;

/// Appends the components of a vector to a flat position array.
pub fn push_vector(out: &mut Vec<f64>, v: Vector3D) {
    out.push(v.X);
    out.push(v.Y);
    out.push(v.Z);
}

/// Extracts vertex i of a flat position array as a Vector3D.
pub fn vertex_at(positions: &[f64], i: usize) -> Vector3D {
    Vector3D::new(positions[i * 3], positions[i * 3 + 1], positions[i * 3 + 2])
}

/// Area-weighted vertex normals (the cross product length weighs by area).
pub fn compute_vertex_normals(positions: &[f64], indices: &[u32]) -> Vec<f64> {
    let mut normals = vec![0.0; positions.len()];
    for tri in indices.chunks_exact(3) {
        let (ia, ib, ic) = (tri[0] as usize, tri[1] as usize, tri[2] as usize);
        let a = vertex_at(positions, ia);
        let b = vertex_at(positions, ib);
        let c = vertex_at(positions, ic);
        let n = b.Subtract(a).Cross(c.Subtract(a));
        for j in [ia * 3, ib * 3, ic * 3] {
            normals[j] += n.X;
            normals[j + 1] += n.Y;
            normals[j + 2] += n.Z;
        }
    }
    for i in (0..normals.len()).step_by(3) {
        let n = Vector3D::new(normals[i], normals[i + 1], normals[i + 2]);
        let len = n.Length();
        let unit = if len > 1e-12 { n.Scale(1.0 / len) } else { n };
        normals[i] = unit.X;
        normals[i + 1] = unit.Y;
        normals[i + 2] = unit.Z;
    }
    normals
}

/// Tessellates a parametric surface f(u, v) over the unit square into a quad
/// grid of nu x nv cells (each split into two triangles).
pub fn grid_mesh(nu: usize, nv: usize, f: impl Fn(f64, f64) -> Vector3D) -> MeshData {
    let mut positions: Vec<f64> = Vec::new();
    let mut indices: Vec<u32> = Vec::new();
    for iv in 0..=nv {
        for iu in 0..=nu {
            push_vector(&mut positions, f(iu as f64 / nu as f64, iv as f64 / nv as f64));
        }
    }
    let stride = nu + 1;
    for iv in 0..nv {
        for iu in 0..nu {
            let i00 = (iv * stride + iu) as u32;
            let (i10, i01, i11) = (i00 + 1, i00 + stride as u32, i00 + stride as u32 + 1);
            indices.extend_from_slice(&[i00, i01, i10, i10, i01, i11]);
        }
    }
    MeshData { positions, indices }
}

/// Builds a MeshData from separate vertex and triangle-index lists.
pub fn mesh_from_vertices(vertices: &[Vector3D], indices: Vec<u32>) -> MeshData {
    let mut positions: Vec<f64> = Vec::new();
    for v in vertices {
        push_vector(&mut positions, *v);
    }
    MeshData { positions, indices }
}

/// Appends the 12 edges of an axis-aligned box to a flat segment list.
pub fn append_box_edges(min: Vector3D, max: Vector3D, out: &mut Vec<f64>) {
    let c = [
        Vector3D::new(min.X, min.Y, min.Z),
        Vector3D::new(max.X, min.Y, min.Z),
        Vector3D::new(max.X, max.Y, min.Z),
        Vector3D::new(min.X, max.Y, min.Z),
        Vector3D::new(min.X, min.Y, max.Z),
        Vector3D::new(max.X, min.Y, max.Z),
        Vector3D::new(max.X, max.Y, max.Z),
        Vector3D::new(min.X, max.Y, max.Z),
    ];
    let edges = [0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7];
    for i in edges {
        push_vector(out, c[i]);
    }
}

/// Translates a mesh in place (useful for side-by-side comparisons).
pub fn translate_mesh(mesh: &mut MeshData, offset: Vector3D) {
    for i in (0..mesh.positions.len()).step_by(3) {
        mesh.positions[i] += offset.X;
        mesh.positions[i + 1] += offset.Y;
        mesh.positions[i + 2] += offset.Z;
    }
}
