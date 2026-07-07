// Octree over a point cloud.
//
// Each node covers an axis-aligned box. Points accumulate in leaves; when a
// leaf exceeds its capacity (and the depth limit allows) it splits into eight
// children and pushes its points down. Query structures like this power
// neighbor searches, culling, and collision broad-phases.

use crate::core::mesh_builder::{append_box_edges, push_vector};
use crate::core::random::make_rng;
use crate::core::types::Drawable;
use crate::plato::*;

pub struct OctreeNode {
    pub min: Vector3D,
    pub max: Vector3D,
    pub depth: usize,
    /// Indices into the point list (leaves only).
    pub points: Vec<usize>,
    pub children: Option<Vec<OctreeNode>>,
}

pub fn make_octree(min: Vector3D, max: Vector3D) -> OctreeNode {
    make_octree_at(min, max, 0)
}

fn make_octree_at(min: Vector3D, max: Vector3D, depth: usize) -> OctreeNode {
    OctreeNode { min, max, depth, points: Vec::new(), children: None }
}

pub fn insert(node: &mut OctreeNode, pts: &[Vector3D], i: usize, capacity: usize, max_depth: usize) {
    if node.children.is_some() {
        let child = child_for(node, pts[i]);
        if let Some(children) = &mut node.children {
            insert(&mut children[child], pts, i, capacity, max_depth);
        }
        return;
    }
    node.points.push(i);
    if node.points.len() > capacity && node.depth < max_depth {
        split(node);
        let pending = std::mem::take(&mut node.points);
        for p in pending {
            let child = child_for(node, pts[p]);
            if let Some(children) = &mut node.children {
                insert(&mut children[child], pts, p, capacity, max_depth);
            }
        }
    }
}

fn split(node: &mut OctreeNode) {
    let (min, max) = (node.min, node.max);
    let mid = min.MidPoint(max);
    let mut children = Vec::with_capacity(8);
    for octant in 0..8usize {
        children.push(make_octree_at(
            Vector3D::new(
                if octant & 1 != 0 { mid.X } else { min.X },
                if octant & 2 != 0 { mid.Y } else { min.Y },
                if octant & 4 != 0 { mid.Z } else { min.Z },
            ),
            Vector3D::new(
                if octant & 1 != 0 { max.X } else { mid.X },
                if octant & 2 != 0 { max.Y } else { mid.Y },
                if octant & 4 != 0 { max.Z } else { mid.Z },
            ),
            node.depth + 1,
        ));
    }
    node.children = Some(children);
}

fn child_for(node: &OctreeNode, p: Vector3D) -> usize {
    let mid = node.min.MidPoint(node.max);
    (if p.X >= mid.X { 1 } else { 0 })
        | (if p.Y >= mid.Y { 2 } else { 0 })
        | (if p.Z >= mid.Z { 4 } else { 0 })
}

pub fn collect_leaves(node: &OctreeNode) -> Vec<&OctreeNode> {
    let mut out = Vec::new();
    collect_leaves_into(node, &mut out);
    out
}

fn collect_leaves_into<'a>(node: &'a OctreeNode, out: &mut Vec<&'a OctreeNode>) {
    match &node.children {
        Some(children) => {
            for c in children {
                collect_leaves_into(c, out);
            }
        }
        None => out.push(node),
    }
}

/// Three gaussian-ish clusters of points, deterministic.
pub fn clustered_points(count: usize) -> Vec<Vector3D> {
    let mut rng = make_rng(23);
    let centers = [
        Vector3D::new(-0.7, 0.4, -0.5),
        Vector3D::new(0.8, -0.3, 0.4),
        Vector3D::new(0.1, 0.7, 0.8),
    ];
    let mut pts: Vec<Vector3D> = Vec::new();
    for i in 0..count {
        let mut gauss = || (rng.next() + rng.next() + rng.next() - 1.5) * 0.55;
        let offset = Vector3D::new(gauss(), gauss(), gauss());
        pts.push(centers[i % centers.len()].Add(offset));
    }
    pts
}

pub fn build() -> Vec<Drawable> {
    let pts = clustered_points(600);
    let mut root = make_octree(Vector3D::new(-2.0, -2.0, -2.0), Vector3D::new(2.0, 2.0, 2.0));
    for i in 0..pts.len() {
        insert(&mut root, &pts, i, 8, 5);
    }

    let mut boxes: Vec<f64> = Vec::new();
    for leaf in collect_leaves(&root) {
        if !leaf.points.is_empty() {
            append_box_edges(leaf.min, leaf.max, &mut boxes);
        }
    }

    let mut cloud: Vec<f64> = Vec::new();
    for &p in &pts {
        push_vector(&mut cloud, p);
    }

    vec![Drawable::Lines { positions: boxes }, Drawable::Points { positions: cloud }]
}
