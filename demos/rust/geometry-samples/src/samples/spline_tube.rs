// Catmull-Rom spline with a parallel-transport tube.
//
// The spline interpolates its control points. To wrap a tube around it
// without twisting, the normal frame is "parallel transported": at each step
// the previous normal is re-projected to stay perpendicular to the tangent,
// rather than being recomputed from scratch (which is what causes flips in
// the naive Frenet approach).

use crate::core::mesh_builder::push_vector;
use crate::core::types::{Drawable, MeshData};
use crate::plato::*;

/// Uniform Catmull-Rom: evaluates the segment p1..p2 at t in [0, 1].
pub fn catmull_rom(p0: Vector3D, p1: Vector3D, p2: Vector3D, p3: Vector3D, t: f64) -> Vector3D {
    let t2 = t * t;
    let t3 = t2 * t;
    p0.Scale(-0.5 * t3 + t2 - 0.5 * t)
        .Add(p1.Scale(1.5 * t3 - 2.5 * t2 + 1.0))
        .Add(p2.Scale(-1.5 * t3 + 2.0 * t2 + 0.5 * t))
        .Add(p3.Scale(0.5 * t3 - 0.5 * t2))
}

/// Samples an open Catmull-Rom curve through the control points.
pub fn sample_spline(control: &[Vector3D], per_segment: usize) -> Vec<Vector3D> {
    let mut result: Vec<Vector3D> = Vec::new();
    let clamped = |i: isize| control[i.clamp(0, control.len() as isize - 1) as usize];
    for seg in 0..control.len() - 1 {
        let last = if seg == control.len() - 2 { per_segment } else { per_segment - 1 };
        for s in 0..=last {
            result.push(catmull_rom(
                clamped(seg as isize - 1),
                control[seg],
                control[seg + 1],
                clamped(seg as isize + 2),
                s as f64 / per_segment as f64,
            ));
        }
    }
    result
}

/// Sweeps a circle along the curve using parallel-transport frames.
pub fn tube_mesh(curve: &[Vector3D], radius: f64, radial_segments: usize) -> MeshData {
    let tangents: Vec<Vector3D> = (0..curve.len())
        .map(|i| {
            curve[(i + 1).min(curve.len() - 1)]
                .Subtract(curve[i.saturating_sub(1)])
                .Normalize()
        })
        .collect();

    // Initial normal: any direction perpendicular to the first tangent.
    let mut normal = tangents[0].Perpendicular();

    let mut positions: Vec<f64> = Vec::new();
    for i in 0..curve.len() {
        // Parallel transport: remove the tangent component, keep the rest.
        normal = normal.Subtract(tangents[i].Scale(normal.Dot(tangents[i]))).Normalize();
        let binormal = tangents[i].Cross(normal);
        for j in 0..radial_segments {
            let a = (j as f64 / radial_segments as f64).Turns();
            push_vector(
                &mut positions,
                curve[i]
                    .Add(normal.Scale(a.Cos() * radius))
                    .Add(binormal.Scale(a.Sin() * radius)),
            );
        }
    }
    let mut indices: Vec<u32> = Vec::new();
    for i in 0..curve.len() - 1 {
        for j in 0..radial_segments {
            let j1 = (j + 1) % radial_segments;
            let a = (i * radial_segments + j) as u32;
            let b = (i * radial_segments + j1) as u32;
            let c = ((i + 1) * radial_segments + j) as u32;
            let d = ((i + 1) * radial_segments + j1) as u32;
            indices.extend_from_slice(&[a, c, b, b, c, d]);
        }
    }
    MeshData { positions, indices }
}

pub fn control_points() -> Vec<Vector3D> {
    vec![
        Vector3D::new(-1.6, -0.4, 0.6),
        Vector3D::new(-0.9, 0.5, -0.6),
        Vector3D::new(0.0, -0.3, 0.7),
        Vector3D::new(0.8, 0.6, -0.2),
        Vector3D::new(1.5, -0.2, 0.5),
        Vector3D::new(1.9, 0.7, -0.6),
    ]
}

pub fn build() -> Vec<Drawable> {
    let control = control_points();
    let curve = sample_spline(&control, 32);
    let mesh = tube_mesh(&curve, 0.09, 10);

    let mut polyline: Vec<f64> = Vec::new();
    for i in 0..control.len() - 1 {
        push_vector(&mut polyline, control[i]);
        push_vector(&mut polyline, control[i + 1]);
    }
    let mut ctrl: Vec<f64> = Vec::new();
    for &p in &control {
        push_vector(&mut ctrl, p);
    }

    vec![
        mesh.into_drawable(),
        Drawable::Lines { positions: polyline },
        Drawable::Points { positions: ctrl },
    ]
}
