// Marching squares: extracting iso-contours of a scalar field.
//
// Sample the field on a grid; each cell gets a 4-bit mask from which corners
// are above the iso value. A 16-entry table says which cell edges the contour
// crosses, and linear interpolation places the crossing precisely. This is
// the 2D sibling of marching cubes. The field here is a set of "metaballs".

use crate::core::types::Drawable;
use crate::plato::*;

/// Edge order: 0 = bottom, 1 = right, 2 = top, 3 = left.
const CASES: [&[usize]; 16] = [
    &[],
    &[3, 0],
    &[0, 1],
    &[3, 1],
    &[1, 2],
    &[3, 0, 1, 2],
    &[0, 2],
    &[3, 2],
    &[2, 3],
    &[0, 2],
    &[0, 1, 2, 3],
    &[1, 2],
    &[1, 3],
    &[0, 1],
    &[3, 0],
    &[],
];

pub fn marching_squares(
    f: impl Fn(f64, f64) -> f64,
    iso: f64,
    min: Vector2D,
    max: Vector2D,
    resolution: usize,
) -> Vec<Vector2D> {
    let mut segments: Vec<Vector2D> = Vec::new(); // consecutive pairs form line segments
    let dx = (max.X - min.X) / resolution as f64;
    let dy = (max.Y - min.Y) / resolution as f64;

    for j in 0..resolution {
        for i in 0..resolution {
            let x0 = min.X + i as f64 * dx;
            let y0 = min.Y + j as f64 * dy;
            let x1 = x0 + dx;
            let y1 = y0 + dy;
            let v = [f(x0, y0), f(x1, y0), f(x1, y1), f(x0, y1)];
            let mask = (if v[0] > iso { 1 } else { 0 })
                | (if v[1] > iso { 2 } else { 0 })
                | (if v[2] > iso { 4 } else { 0 })
                | (if v[3] > iso { 8 } else { 0 });

            // Where does the contour cross each edge? (linear interpolation)
            let lerp_t = |a: f64, b: f64| (iso - a) / (b - a);
            let on_edge = |e: usize| -> Vector2D {
                match e {
                    0 => Vector2D::new(x0 + dx * lerp_t(v[0], v[1]), y0),
                    1 => Vector2D::new(x1, y0 + dy * lerp_t(v[1], v[2])),
                    2 => Vector2D::new(x0 + dx * lerp_t(v[3], v[2]), y1),
                    _ => Vector2D::new(x0, y0 + dy * lerp_t(v[0], v[3])),
                }
            };
            let edges = CASES[mask];
            for e in (0..edges.len()).step_by(2) {
                segments.push(on_edge(edges[e]));
                segments.push(on_edge(edges[e + 1]));
            }
        }
    }
    segments
}

/// Three metaballs: smooth field with interesting topology changes.
pub fn metaballs(x: f64, y: f64) -> f64 {
    let point = Vector2D::new(x, y);
    let blobs = [
        (Vector2D::new(-0.5, 0.1), 0.28),
        (Vector2D::new(0.45, 0.25), 0.22),
        (Vector2D::new(0.1, -0.45), 0.18),
    ];
    let mut sum = 0.0;
    for (center, weight) in blobs {
        sum += weight / (point.DistanceSquared(center) + 0.03);
    }
    sum
}

pub fn build() -> Vec<Drawable> {
    let min = Vector2D::new(-1.6, -1.4);
    let max = Vector2D::new(1.6, 1.4);
    let iso_levels = [(1.0, 0.0), (1.8, 0.012), (3.2, 0.024)];
    iso_levels
        .iter()
        .map(|&(iso, y)| {
            let segs = marching_squares(metaballs, iso, min, max, 110);
            let mut positions: Vec<f64> = Vec::new();
            for p in segs {
                positions.extend_from_slice(&[p.X, y, p.Y]);
            }
            Drawable::Lines { positions }
        })
        .collect()
}
