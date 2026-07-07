// Invariant tests for every sample: the Rust port of
// demos/typescript/geometry-samples/tests/samples.test.ts (same seeds via the mulberry32
// port, so results are comparable across languages).

#![allow(non_snake_case)]

use std::collections::{HashMap, HashSet};

use geometry_samples::core::mesh_builder::vertex_at;
use geometry_samples::core::random::make_rng;
use geometry_samples::core::types::Drawable;
use geometry_samples::plato::*;
use geometry_samples::samples::bvh::{build_bvh, triangle_centroid, BvhNode};
use geometry_samples::samples::convex_hull::{convex_hull, turn};
use geometry_samples::samples::delaunay::{circumcircle, delaunay};
use geometry_samples::samples::half_edge::{
    build_half_edge_mesh, laplacian_smooth, noisy_sphere, one_ring,
};
use geometry_samples::samples::icosphere::build_icosphere;
use geometry_samples::samples::marching_squares::{marching_squares, metaballs};
use geometry_samples::samples::octree::{clustered_points, collect_leaves, insert, make_octree};
use geometry_samples::samples::poisson_disk::poisson_disk;
use geometry_samples::samples::raycast::raycast_mesh;
use geometry_samples::samples::spline_tube::{control_points, sample_spline};
use geometry_samples::samples::samples;

#[test]
fn every_sample_builds_finite_non_empty_drawables() {
    let all = samples();
    assert!(all.len() >= 10, "at least 10 samples");
    for (id, build) in all {
        let drawables = build();
        assert!(!drawables.is_empty(), "{} produced drawables", id);
        for d in &drawables {
            let positions = d.positions();
            assert!(!positions.is_empty(), "{} has positions", id);
            assert!(positions.len() % 3 == 0, "{} positions are xyz triples", id);
            for v in positions {
                assert!(v.is_finite(), "{} has finite coordinates", id);
            }
            if let Drawable::Mesh { positions, indices, .. } = d {
                assert!(indices.len() % 3 == 0, "{} indices form triangles", id);
                let vertex_count = (positions.len() / 3) as u32;
                for &i in indices {
                    assert!(i < vertex_count, "{} indices in range", id);
                }
            }
            if let Drawable::Lines { positions } = d {
                assert!((positions.len() / 3) % 2 == 0, "{} lines come in pairs", id);
            }
        }
    }
}

#[test]
fn icosphere_watertight_euler_characteristic_and_unit_radius() {
    for level in [0usize, 1, 2] {
        let mesh = build_icosphere(level);
        let v = (mesh.positions.len() / 3) as i64;
        let f = (mesh.indices.len() / 3) as i64;
        let mut edges: HashSet<(u32, u32)> = HashSet::new();
        for tri in mesh.indices.chunks_exact(3) {
            for (a, b) in [(0, 1), (1, 2), (2, 0)] {
                let (u, w) = (tri[a], tri[b]);
                edges.insert(if u < w { (u, w) } else { (w, u) });
            }
        }
        assert_eq!(v - edges.len() as i64 + f, 2, "Euler characteristic at level {}", level);
        for i in 0..v as usize {
            assert!((vertex_at(&mesh.positions, i).Length() - 1.0).abs() < 1e-9);
        }
    }
}

#[test]
fn delaunay_empty_circumcircle_property() {
    let mut rng = make_rng(7);
    let mut points: Vec<Vector2D> = Vec::new();
    for _ in 0..60 {
        points.push(Vector2D::new(rng.range(-1.5, 1.5), rng.range(-1.0, 1.0)));
    }
    let triangles = delaunay(&points);
    assert!(!triangles.is_empty());
    for t in &triangles {
        let cc = circumcircle(&points, *t);
        for (i, p) in points.iter().enumerate() {
            if i == t.a || i == t.b || i == t.c {
                continue;
            }
            assert!(
                p.DistanceSquared(cc.center) >= cc.radius_squared - 1e-9,
                "point {} not inside circumcircle",
                i
            );
        }
    }
}

#[test]
fn convex_hull_is_convex_and_contains_all_points() {
    let mut rng = make_rng(11);
    let mut points: Vec<Vector2D> = Vec::new();
    for _ in 0..80 {
        points.push(Vector2D::new(rng.range(-1.0, 1.0), rng.range(-1.0, 1.0)));
    }
    let hull = convex_hull(&points);
    assert!(hull.len() >= 3);
    let n = hull.len();
    for i in 0..n {
        let a = points[hull[i]];
        let b = points[hull[(i + 1) % n]];
        let c = points[hull[(i + 2) % n]];
        assert!(turn(a, b, c) > 0.0, "hull turns counter-clockwise");
    }
    for p in &points {
        for i in 0..n {
            assert!(
                turn(points[hull[i]], points[hull[(i + 1) % n]], *p) >= -1e-12,
                "point inside hull"
            );
        }
    }
}

#[test]
fn spline_interpolates_its_control_points_at_the_knots() {
    let per_segment = 16usize;
    let control = control_points();
    let curve = sample_spline(&control, per_segment);
    for (i, p) in control.iter().enumerate() {
        let at = (i * per_segment).min(curve.len() - 1);
        assert!(curve[at].Distance(*p) < 1e-9, "knot {} interpolated", i);
    }
}

#[test]
fn octree_every_point_lands_in_exactly_one_leaf_that_contains_it() {
    let pts = clustered_points(500);
    let mut root = make_octree(Vector3D::new(-2.0, -2.0, -2.0), Vector3D::new(2.0, 2.0, 2.0));
    for i in 0..pts.len() {
        insert(&mut root, &pts, i, 8, 5);
    }
    let leaves = collect_leaves(&root);
    let mut seen: HashMap<usize, usize> = HashMap::new();
    for leaf in leaves {
        for &i in &leaf.points {
            *seen.entry(i).or_insert(0) += 1;
            let p = pts[i];
            assert!(
                p.X >= leaf.min.X - 1e-12
                    && p.X <= leaf.max.X + 1e-12
                    && p.Y >= leaf.min.Y - 1e-12
                    && p.Y <= leaf.max.Y + 1e-12
                    && p.Z >= leaf.min.Z - 1e-12
                    && p.Z <= leaf.max.Z + 1e-12,
                "leaf contains its point"
            );
        }
    }
    assert_eq!(seen.len(), pts.len(), "all points stored");
    for &count in seen.values() {
        assert_eq!(count, 1, "stored exactly once");
    }
}

#[test]
fn bvh_leaves_partition_the_triangles_and_boxes_contain_their_centroids() {
    let mesh = build_icosphere(2);
    let all_tris: Vec<usize> = (0..mesh.indices.len() / 3).collect();
    let tri_count = all_tris.len();
    let root = build_bvh(&mesh, all_tris, 0, 8);
    let mut seen: HashSet<usize> = HashSet::new();

    fn walk(
        node: &BvhNode,
        mesh: &geometry_samples::core::types::MeshData,
        seen: &mut HashSet<usize>,
    ) {
        if let Some(triangles) = &node.triangles {
            for &t in triangles {
                assert!(!seen.contains(&t), "triangle in one leaf only");
                seen.insert(t);
                let c = triangle_centroid(mesh, t);
                assert!(
                    c.X >= node.min.X - 1e-9
                        && c.X <= node.max.X + 1e-9
                        && c.Y >= node.min.Y - 1e-9
                        && c.Y <= node.max.Y + 1e-9
                        && c.Z >= node.min.Z - 1e-9
                        && c.Z <= node.max.Z + 1e-9,
                    "centroid inside leaf box"
                );
            }
            return;
        }
        walk(node.left.as_ref().unwrap(), mesh, seen);
        walk(node.right.as_ref().unwrap(), mesh, seen);
    }
    walk(&root, &mesh, &mut seen);
    assert_eq!(seen.len(), tri_count, "every triangle assigned");
}

#[test]
fn half_edge_twin_next_invariants_and_smoothing_reduces_roughness() {
    let mesh = noisy_sphere();
    let he = build_half_edge_mesh(&mesh.indices, mesh.positions.len() / 3);

    for e in 0..he.half_edges.len() {
        let edge = he.half_edges[e];
        assert_ne!(edge.twin, -1, "closed mesh: every edge has a twin");
        assert_eq!(he.half_edges[edge.twin as usize].twin, e as isize, "twin is symmetric");
        let n3 = he.half_edges[he.half_edges[edge.next].next].next;
        assert_eq!(n3, e, "next cycles with period 3");
    }

    // One-ring traversal terminates and sums to the half-edge count.
    let mut ring_total = 0;
    for v in 0..he.vertex_count {
        ring_total += one_ring(&he, v).len();
    }
    assert_eq!(ring_total, he.half_edges.len(), "rings cover each half-edge once");

    fn radius_variance(positions: &[f64]) -> f64 {
        let n = positions.len() / 3;
        let radii: Vec<f64> = (0..n).map(|i| vertex_at(positions, i).Length()).collect();
        let mean = radii.iter().sum::<f64>() / n as f64;
        radii.iter().map(|r| (r - mean) * (r - mean)).sum::<f64>() / n as f64
    }
    let smoothed = laplacian_smooth(&mesh.positions, &he, 0.6, 10);
    assert_eq!(smoothed.len(), mesh.positions.len(), "vertex count preserved");
    assert!(
        radius_variance(&smoothed) < radius_variance(&mesh.positions) * 0.35,
        "roughness reduced"
    );
}

#[test]
fn raycast_central_ray_hits_the_unit_icosphere_at_distance_2() {
    let mesh = build_icosphere(2);
    let hit = raycast_mesh(&mesh, Vector3D::new(0.0, 0.0, 3.0), Vector3D::new(0.0, 0.0, -1.0));
    let hit = hit.expect("hit found");
    assert!((hit.t - 2.0).abs() < 0.06, "distance {} close to 2", hit.t);
    let miss = raycast_mesh(&mesh, Vector3D::new(5.0, 5.0, 3.0), Vector3D::new(0.0, 0.0, -1.0));
    assert!(miss.is_none(), "offset ray misses");
}

#[test]
fn poisson_disk_pairwise_distances_respect_the_radius() {
    let r = 0.15;
    let mut rng = make_rng(3);
    let pts = poisson_disk(2.0, 2.0, r, &mut rng, 30);
    assert!(pts.len() > 50, "reasonable density");
    for i in 0..pts.len() {
        for j in i + 1..pts.len() {
            assert!(
                pts[i].DistanceSquared(pts[j]) >= r * r - 1e-12,
                "points {},{} too close",
                i,
                j
            );
        }
    }
}

#[test]
fn marching_squares_contour_vertices_lie_near_the_iso_value() {
    let iso = 1.5;
    let segments = marching_squares(
        metaballs,
        iso,
        Vector2D::new(-1.6, -1.4),
        Vector2D::new(1.6, 1.4),
        128,
    );
    assert!(segments.len() >= 2, "contour found");
    for p in &segments {
        let value = metaballs(p.X, p.Y);
        assert!((value - iso).abs() < 0.25, "field {} near iso {}", value, iso);
    }
}
