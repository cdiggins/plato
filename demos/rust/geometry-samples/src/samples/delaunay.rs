// Delaunay triangulation (Bowyer-Watson).
//
// Insert points one at a time: find all triangles whose circumcircle contains
// the new point (the "bad" triangles), remove them, and re-triangulate the
// boundary of the resulting cavity by fanning to the new point. A triangle
// mesh built this way maximizes the minimum angle over all triangulations.

use std::collections::HashMap;

use crate::core::random::make_rng;
use crate::core::types::Drawable;
use crate::plato::*;

#[derive(Clone, Copy, Debug)]
pub struct Triangle2 {
    pub a: usize,
    pub b: usize,
    pub c: usize,
}

pub struct Circumcircle {
    pub center: Vector2D,
    pub radius_squared: f64,
}

pub fn circumcircle(pts: &[Vector2D], t: Triangle2) -> Circumcircle {
    let (a, b, c) = (pts[t.a], pts[t.b], pts[t.c]);
    let d = 2.0 * (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));
    let (a2, b2, c2) = (a.LengthSquared(), b.LengthSquared(), c.LengthSquared());
    let center = Vector2D::new(
        (a2 * (b.Y - c.Y) + b2 * (c.Y - a.Y) + c2 * (a.Y - b.Y)) / d,
        (a2 * (c.X - b.X) + b2 * (a.X - c.X) + c2 * (b.X - a.X)) / d,
    );
    Circumcircle { center, radius_squared: a.DistanceSquared(center) }
}

pub fn delaunay(points: &[Vector2D]) -> Vec<Triangle2> {
    let n = points.len();
    // Vertices n .. n+2 form a "super triangle" enclosing every input point.
    let mut pts = points.to_vec();
    pts.push(Vector2D::new(0.0, -1000.0));
    pts.push(Vector2D::new(1000.0, 1000.0));
    pts.push(Vector2D::new(-1000.0, 1000.0));
    let mut triangles: Vec<Triangle2> = vec![Triangle2 { a: n, b: n + 1, c: n + 2 }];

    for i in 0..n {
        let p = pts[i];
        let mut bad: Vec<Triangle2> = Vec::new();
        let mut good: Vec<Triangle2> = Vec::new();
        for &t in &triangles {
            let cc = circumcircle(&pts, t);
            if p.DistanceSquared(cc.center) < cc.radius_squared {
                bad.push(t);
            } else {
                good.push(t);
            }
        }
        // Edges appearing in exactly one bad triangle form the cavity boundary.
        let mut boundary: HashMap<(usize, usize), (usize, usize)> = HashMap::new();
        for t in &bad {
            for (a, b) in [(t.a, t.b), (t.b, t.c), (t.c, t.a)] {
                let key = if a < b { (a, b) } else { (b, a) };
                if boundary.remove(&key).is_none() {
                    boundary.insert(key, (a, b));
                }
            }
        }
        triangles = good;
        for &(a, b) in boundary.values() {
            triangles.push(Triangle2 { a, b, c: i });
        }
    }

    // Drop every triangle that touches the super triangle.
    triangles.retain(|t| t.a < n && t.b < n && t.c < n);
    triangles
}

pub fn build() -> Vec<Drawable> {
    let mut rng = make_rng(7);
    let mut points: Vec<Vector2D> = Vec::new();
    for _ in 0..120 {
        points.push(Vector2D::new(rng.range(-1.6, 1.6), rng.range(-1.2, 1.2)));
    }
    let triangles = delaunay(&points);

    let mut positions: Vec<f64> = Vec::new();
    for p in &points {
        positions.extend_from_slice(&[p.X, 0.0, p.Y]);
    }
    let mut indices: Vec<u32> = Vec::new();
    for t in &triangles {
        indices.extend_from_slice(&[t.a as u32, t.b as u32, t.c as u32]);
    }

    let mut wire: Vec<f64> = Vec::new();
    for t in &triangles {
        for (a, b) in [(t.a, t.b), (t.b, t.c), (t.c, t.a)] {
            wire.extend_from_slice(&[
                points[a].X, 0.001, points[a].Y, points[b].X, 0.001, points[b].Y,
            ]);
        }
    }

    let normals = crate::core::mesh_builder::compute_vertex_normals(&positions, &indices);
    vec![
        Drawable::Mesh { positions: positions.clone(), indices, normals },
        Drawable::Lines { positions: wire },
        Drawable::Points { positions },
    ]
}
