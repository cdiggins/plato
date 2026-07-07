// Bounding volume hierarchy (BVH) over mesh triangles.
//
// Recursively partition triangles by sorting their centroids along the
// longest axis of the current bounds and splitting at the median. The result
// is a balanced tree of axis-aligned boxes used to accelerate raycasts and
// intersection queries from O(n) to O(log n).

use crate::core::mesh_builder::{append_box_edges, vertex_at};
use crate::core::types::{Drawable, MeshData};
use crate::plato::*;

use super::icosphere::build_icosphere;

pub struct BvhNode {
    pub min: Vector3D,
    pub max: Vector3D,
    pub depth: usize,
    pub left: Option<Box<BvhNode>>,
    pub right: Option<Box<BvhNode>>,
    /// Triangle indices (leaves only).
    pub triangles: Option<Vec<usize>>,
}

pub fn triangle_centroid(mesh: &MeshData, t: usize) -> Vector3D {
    let a = vertex_at(&mesh.positions, mesh.indices[t * 3] as usize);
    let b = vertex_at(&mesh.positions, mesh.indices[t * 3 + 1] as usize);
    let c = vertex_at(&mesh.positions, mesh.indices[t * 3 + 2] as usize);
    a.Add(b).Add(c).Scale(1.0 / 3.0)
}

fn triangle_bounds(mesh: &MeshData, t: usize) -> (Vector3D, Vector3D) {
    let mut min = Vector3D::new(f64::INFINITY, f64::INFINITY, f64::INFINITY);
    let mut max = Vector3D::new(f64::NEG_INFINITY, f64::NEG_INFINITY, f64::NEG_INFINITY);
    for k in 0..3 {
        let v = vertex_at(&mesh.positions, mesh.indices[t * 3 + k] as usize);
        min = min.Min(v);
        max = max.Max(v);
    }
    (min, max)
}

pub fn build_bvh(mesh: &MeshData, triangles: Vec<usize>, depth: usize, leaf_size: usize) -> BvhNode {
    let mut min = Vector3D::new(f64::INFINITY, f64::INFINITY, f64::INFINITY);
    let mut max = Vector3D::new(f64::NEG_INFINITY, f64::NEG_INFINITY, f64::NEG_INFINITY);
    for &t in &triangles {
        let (bmin, bmax) = triangle_bounds(mesh, t);
        min = min.Min(bmin);
        max = max.Max(bmax);
    }

    if triangles.len() <= leaf_size || depth >= 16 {
        return BvhNode { min, max, depth, left: None, right: None, triangles: Some(triangles) };
    }

    // Median split along the longest axis of the bounds.
    let extent = max.Subtract(min);
    let axis = if extent.X >= extent.Y && extent.X >= extent.Z {
        0
    } else if extent.Y >= extent.Z {
        1
    } else {
        2
    };
    let component = |v: Vector3D| match axis {
        0 => v.X,
        1 => v.Y,
        _ => v.Z,
    };
    let mut sorted = triangles;
    sorted.sort_by(|&a, &b| {
        component(triangle_centroid(mesh, a))
            .partial_cmp(&component(triangle_centroid(mesh, b)))
            .unwrap()
    });
    let half = sorted.len() / 2;
    let right_tris = sorted.split_off(half);

    BvhNode {
        min,
        max,
        depth,
        left: Some(Box::new(build_bvh(mesh, sorted, depth + 1, leaf_size))),
        right: Some(Box::new(build_bvh(mesh, right_tris, depth + 1, leaf_size))),
        triangles: None,
    }
}

pub fn collect_boxes_at_depth(node: &BvhNode, depth: usize, out: &mut Vec<f64>) {
    if node.depth == depth || (node.triangles.is_some() && node.depth < depth) {
        append_box_edges(node.min, node.max, out);
        return;
    }
    if let Some(left) = &node.left {
        collect_boxes_at_depth(left, depth, out);
    }
    if let Some(right) = &node.right {
        collect_boxes_at_depth(right, depth, out);
    }
}

pub fn build() -> Vec<Drawable> {
    let mesh = build_icosphere(3);
    let all_tris: Vec<usize> = (0..mesh.indices.len() / 3).collect();
    let root = build_bvh(&mesh, all_tris, 0, 8);

    let mut drawables: Vec<Drawable> = Vec::new();

    for k in [-1.0f64, 0.0, 1.0] {
        let mut m = build_icosphere(3);
        for j in (0..m.positions.len()).step_by(3) {
            m.positions[j] += k * 2.6;
        }
        drawables.push(m.into_drawable());
    }

    for (i, depth) in [2usize, 4, 6].iter().enumerate() {
        let mut positions: Vec<f64> = Vec::new();
        collect_boxes_at_depth(&root, *depth, &mut positions);
        for j in (0..positions.len()).step_by(3) {
            positions[j] += (i as f64 - 1.0) * 2.6; // side by side
        }
        drawables.push(Drawable::Lines { positions });
    }

    drawables
}
