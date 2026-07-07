// Catmull-Rom spline with a parallel-transport tube.
//
// The spline interpolates its control points. To wrap a tube around it
// without twisting, the normal frame is "parallel transported": at each step
// the previous normal is re-projected to stay perpendicular to the tangent,
// rather than being recomputed from scratch (which is what causes flips in
// the naive Frenet approach).

import type { Sample } from '../core/types.js';
import { Vector3D } from '../plato/plato.g.js';
import { computeVertexNormals, pushVectors } from '../core/meshBuilder.js';

/** Uniform Catmull-Rom: evaluates the segment p1..p2 at t in [0, 1]. */
export function catmullRom(p0: Vector3D, p1: Vector3D, p2: Vector3D, p3: Vector3D, t: number): Vector3D {
    const t2 = t * t, t3 = t2 * t;
    return p0.Scale(-0.5 * t3 + t2 - 0.5 * t)
        .Add(p1.Scale(1.5 * t3 - 2.5 * t2 + 1))
        .Add(p2.Scale(-1.5 * t3 + 2 * t2 + 0.5 * t))
        .Add(p3.Scale(0.5 * t3 - 0.5 * t2));
}

/** Samples an open Catmull-Rom curve through the control points. */
export function sampleSpline(control: Vector3D[], perSegment: number): Vector3D[] {
    const result: Vector3D[] = [];
    const clamped = (i: number) => control[i.Clamp(0, control.length - 1)];
    for (let seg = 0; seg < control.length - 1; seg++) {
        const last = seg === control.length - 2 ? perSegment : perSegment - 1;
        for (let s = 0; s <= last; s++)
            result.push(catmullRom(clamped(seg - 1), control[seg], control[seg + 1], clamped(seg + 2), s / perSegment));
    }
    return result;
}

/** Sweeps a circle along the curve using parallel-transport frames. */
export function tubeMesh(curve: Vector3D[], radius: number, radialSegments: number) {
    const tangents = curve.map((_, i) =>
        curve[(i + 1).Min(curve.length - 1)].Subtract(curve[(i - 1).Max(0)]).Normalize());

    // Initial normal: any direction perpendicular to the first tangent.
    let normal = tangents[0].Perpendicular();

    const positions: number[] = [];
    for (let i = 0; i < curve.length; i++) {
        // Parallel transport: remove the tangent component, keep the rest.
        normal = normal.Subtract(tangents[i].Scale(normal.Dot(tangents[i]))).Normalize();
        const binormal = tangents[i].Cross(normal);
        for (let j = 0; j < radialSegments; j++) {
            const a = (j / radialSegments).Turns();
            pushVectors(positions,
                curve[i].Add(normal.Scale(a.Cos() * radius)).Add(binormal.Scale(a.Sin() * radius)));
        }
    }
    const indices: number[] = [];
    for (let i = 0; i < curve.length - 1; i++) {
        for (let j = 0; j < radialSegments; j++) {
            const j1 = (j + 1) % radialSegments;
            const [a, b, c, d] = [i * radialSegments + j, i * radialSegments + j1,
                (i + 1) * radialSegments + j, (i + 1) * radialSegments + j1];
            indices.push(a, c, b, b, c, d);
        }
    }
    return { positions, indices };
}

export const controlPoints: Vector3D[] = [
    new Vector3D(-1.6, -0.4, 0.6), new Vector3D(-0.9, 0.5, -0.6), new Vector3D(0.0, -0.3, 0.7),
    new Vector3D(0.8, 0.6, -0.2), new Vector3D(1.5, -0.2, 0.5), new Vector3D(1.9, 0.7, -0.6),
];

export const splineTubeSample: Sample = {
    id: 'spline-tube',
    title: 'Spline + Tube Sweep',
    description: 'Catmull-Rom interpolation with twist-free parallel-transport frames.',
    build() {
        const curve = sampleSpline(controlPoints, 32);
        const { positions, indices } = tubeMesh(curve, 0.09, 10);

        const polyline: number[] = [];
        for (let i = 0; i + 1 < controlPoints.length; i++)
            pushVectors(polyline, controlPoints[i], controlPoints[i + 1]);
        const ctrl: number[] = [];
        pushVectors(ctrl, ...controlPoints);

        return [
            { kind: 'mesh', positions, indices, normals: computeVertexNormals(positions, indices), color: 0xc678dd },
            { kind: 'lines', positions: polyline, color: 0x556070 },
            { kind: 'points', positions: ctrl, color: 0xffd166, size: 0.09 },
        ];
    },
};
