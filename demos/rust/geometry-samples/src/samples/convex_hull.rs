// 2D convex hull (Andrew's monotone chain), displayed as an extruded prism.
//
// Sort points lexicographically, then build the lower and upper chains,
// popping vertices while the turn is not counter-clockwise. The turn test is
// the 2D cross product: (a - o).Cross(b - o). Runs in O(n log n).

use crate::core::mesh_builder::compute_vertex_normals;
use crate::core::random::make_rng;
use crate::core::types::Drawable;
use crate::plato::*;

pub fn turn(o: Vector2D, a: Vector2D, b: Vector2D) -> f64 {
    a.Subtract(o).Cross(b.Subtract(o))
}

/// Returns the hull as point indices in counter-clockwise order.
pub fn convex_hull(points: &[Vector2D]) -> Vec<usize> {
    let mut idx: Vec<usize> = (0..points.len()).collect();
    idx.sort_by(|&i, &j| {
        points[i]
            .X
            .partial_cmp(&points[j].X)
            .unwrap()
            .then(points[i].Y.partial_cmp(&points[j].Y).unwrap())
    });

    let mut lower: Vec<usize> = Vec::new();
    for &i in &idx {
        while lower.len() >= 2
            && turn(points[lower[lower.len() - 2]], points[lower[lower.len() - 1]], points[i]) <= 0.0
        {
            lower.pop();
        }
        lower.push(i);
    }
    let mut upper: Vec<usize> = Vec::new();
    for &i in idx.iter().rev() {
        while upper.len() >= 2
            && turn(points[upper[upper.len() - 2]], points[upper[upper.len() - 1]], points[i]) <= 0.0
        {
            upper.pop();
        }
        upper.push(i);
    }
    lower.pop();
    upper.pop();
    lower.extend(upper);
    lower
}

/// Extrudes a convex CCW polygon into a prism mesh (bottom y=0, top y=h).
pub fn extrude_polygon(polygon: &[Vector2D], h: f64) -> (Vec<f64>, Vec<u32>) {
    let n = polygon.len();
    let mut positions: Vec<f64> = Vec::new();
    for p in polygon {
        positions.extend_from_slice(&[p.X, 0.0, p.Y]);
    }
    for p in polygon {
        positions.extend_from_slice(&[p.X, h, p.Y]);
    }
    let nu = n as u32;
    let mut indices: Vec<u32> = Vec::new();
    for i in 2..nu {
        indices.extend_from_slice(&[0, i, i - 1]); // bottom fan
    }
    for i in 2..nu {
        indices.extend_from_slice(&[nu, nu + i - 1, nu + i]); // top fan
    }
    for i in 0..nu {
        // sides
        let j = (i + 1) % nu;
        indices.extend_from_slice(&[i, j, nu + i, j, nu + j, nu + i]);
    }
    (positions, indices)
}

pub fn build() -> Vec<Drawable> {
    let mut rng = make_rng(11);
    let mut points: Vec<Vector2D> = Vec::new();
    for _ in 0..90 {
        let theta = rng.next().Turns();
        let r = 1.4 * rng.next().Sqrt();
        points.push(Vector2D::new(r * theta.Cos() * 1.2, r * theta.Sin()));
    }
    let hull: Vec<Vector2D> = convex_hull(&points).iter().map(|&i| points[i]).collect();
    let (positions, indices) = extrude_polygon(&hull, 0.35);

    let mut outline: Vec<f64> = Vec::new();
    for i in 0..hull.len() {
        let a = hull[i];
        let b = hull[(i + 1) % hull.len()];
        for y in [0.0, 0.35] {
            outline.extend_from_slice(&[a.X, y, a.Y, b.X, y, b.Y]);
        }
    }
    let mut cloud: Vec<f64> = Vec::new();
    for p in &points {
        cloud.extend_from_slice(&[p.X, 0.02, p.Y]);
    }

    let normals = compute_vertex_normals(&positions, &indices);
    vec![
        Drawable::Mesh { positions, indices, normals },
        Drawable::Lines { positions: outline },
        Drawable::Points { positions: cloud },
    ]
}
