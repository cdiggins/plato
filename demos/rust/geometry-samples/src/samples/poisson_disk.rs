// Poisson disk sampling (Bridson's algorithm).
//
// Produces "blue noise": points at least r apart but tightly packed. A
// background grid with cell size r / sqrt(2) holds at most one sample per
// cell, so a distance check only needs to look at nearby cells - O(n) total.

use crate::core::random::{make_rng, Rng};
use crate::core::types::Drawable;
use crate::plato::*;

pub fn poisson_disk(width: f64, height: f64, r: f64, rng: &mut Rng, k: usize) -> Vec<Vector2D> {
    let cell = r / std::f64::consts::SQRT_2;
    let gw = (width / cell).Floor() as i64 + 1;
    let gh = (height / cell).Floor() as i64 + 1;
    let mut grid: Vec<i64> = vec![-1; (gw * gh) as usize];
    let mut samples: Vec<Vector2D> = Vec::new();
    let mut active: Vec<usize> = Vec::new();

    let grid_index = |p: Vector2D| -> usize {
        let gx = ((p.X / cell).Floor() as i64).min(gw - 1);
        let gy = ((p.Y / cell).Floor() as i64).min(gh - 1);
        (gy * gw + gx) as usize
    };

    let far_from_neighbors = |p: Vector2D, grid: &[i64], samples: &[Vector2D]| -> bool {
        let gx = (p.X / cell).Floor() as i64;
        let gy = (p.Y / cell).Floor() as i64;
        for y in (gy - 2).max(0)..=(gy + 2).min(gh - 1) {
            for x in (gx - 2).max(0)..=(gx + 2).min(gw - 1) {
                let s = grid[(y * gw + x) as usize];
                if s >= 0 && samples[s as usize].DistanceSquared(p) < r * r {
                    return false;
                }
            }
        }
        true
    };

    let emit = |p: Vector2D, grid: &mut Vec<i64>, samples: &mut Vec<Vector2D>, active: &mut Vec<usize>| {
        grid[grid_index(p)] = samples.len() as i64;
        active.push(samples.len());
        samples.push(p);
    };
    emit(Vector2D::new(width / 2.0, height / 2.0), &mut grid, &mut samples, &mut active);

    while !active.is_empty() {
        let pick = (rng.next() * active.len() as f64).Floor() as usize;
        let around = samples[active[pick]];
        let mut placed = false;
        for _attempt in 0..k {
            // Uniform in the annulus [r, 2r] around the active sample.
            let angle = rng.next().Turns();
            let dist = r * rng.range(1.0, 4.0).Sqrt();
            let p = around.Add(Vector2D::new(angle.Cos() * dist, angle.Sin() * dist));
            if p.X >= 0.0
                && p.X < width
                && p.Y >= 0.0
                && p.Y < height
                && far_from_neighbors(p, &grid, &samples)
            {
                emit(p, &mut grid, &mut samples, &mut active);
                placed = true;
                break;
            }
        }
        if !placed {
            active.remove(pick); // exhausted: retire this sample
        }
    }
    samples
}

pub fn build() -> Vec<Drawable> {
    let mut rng = make_rng(31);
    let blue = poisson_disk(2.2, 2.2, 0.11, &mut rng, 30);

    let mut blue_pts: Vec<f64> = Vec::new();
    for p in &blue {
        blue_pts.extend_from_slice(&[p.X - 2.4, 0.0, p.Y - 1.1]);
    }

    // The same number of uniformly random points, for comparison.
    let mut white_pts: Vec<f64> = Vec::new();
    for _ in 0..blue.len() {
        white_pts.extend_from_slice(&[rng.range(0.2, 2.4), 0.0, rng.range(-1.1, 1.1)]);
    }

    let mut frame: Vec<f64> = Vec::new();
    for x0 in [-2.4, 0.2] {
        frame.extend_from_slice(&[
            x0, 0.0, -1.1, x0 + 2.2, 0.0, -1.1, x0 + 2.2, 0.0, -1.1, x0 + 2.2, 0.0, 1.1, //
            x0 + 2.2, 0.0, 1.1, x0, 0.0, 1.1, x0, 0.0, 1.1, x0, 0.0, -1.1,
        ]);
    }

    vec![
        Drawable::Points { positions: blue_pts },
        Drawable::Points { positions: white_pts },
        Drawable::Lines { positions: frame },
    ]
}
