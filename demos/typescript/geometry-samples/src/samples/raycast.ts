// Ray-triangle intersection (Moller-Trumbore).
//
// Solves the ray/triangle equation directly with two cross products, giving
// the barycentric coordinates (u, v) and the distance t in one pass, with no
// precomputed plane. Written fluently against the Plato vector library -
// compare with the C# version, which reads identically.

import type { MeshData, Sample } from '../core/types.js';
import { Vector3D } from '../plato/plato.g.js';
import { pushVectors, vertexAt } from '../core/meshBuilder.js';
import { buildIcosphere } from './icosphere.js';

export interface RayHit {
    t: number;
    point: Vector3D;
    triangle: number;
}

export function intersectTriangle(
    origin: Vector3D, dir: Vector3D, a: Vector3D, b: Vector3D, c: Vector3D): number | null {
    const e1 = b.Subtract(a);
    const e2 = c.Subtract(a);
    const p = dir.Cross(e2);
    const det = e1.Dot(p);
    if (det.Abs() < 1e-10)
        return null; // parallel to the triangle plane
    const inv = 1 / det;
    const s = origin.Subtract(a);
    const u = s.Dot(p) * inv;
    if (u < 0 || u > 1)
        return null;
    const q = s.Cross(e1);
    const v = dir.Dot(q) * inv;
    if (v < 0 || u + v > 1)
        return null;
    const t = e2.Dot(q) * inv;
    return t > 1e-9 ? t : null;
}

/** Closest hit against every triangle (brute force; see the BVH sample). */
export function raycastMesh(mesh: MeshData, origin: Vector3D, dir: Vector3D): RayHit | null {
    let best: RayHit | null = null;
    for (let tri = 0; tri < mesh.indices.length / 3; tri++) {
        const a = vertexAt(mesh.positions, mesh.indices[tri * 3]);
        const b = vertexAt(mesh.positions, mesh.indices[tri * 3 + 1]);
        const c = vertexAt(mesh.positions, mesh.indices[tri * 3 + 2]);
        const t = intersectTriangle(origin, dir, a, b, c);
        if (t !== null && (!best || t < best.t))
            best = { t, point: origin.Add(dir.Scale(t)), triangle: tri };
    }
    return best;
}

export const raycastSample: Sample = {
    id: 'raycast',
    title: 'Raycasting',
    description: 'Moller-Trumbore ray-triangle intersection: a ray grid vs. an icosphere.',
    build() {
        const mesh = buildIcosphere(2);
        mesh.color = 0x4da3ff;
        mesh.opacity = 0.45;

        const rays: number[] = [];
        const hits: number[] = [];
        const misses: number[] = [];
        const dir = new Vector3D(0, 0, -1);
        for (let iy = 0; iy < 9; iy++) {
            for (let ix = 0; ix < 9; ix++) {
                const origin = new Vector3D((ix / 8 - 0.5) * 2.6, (iy / 8 - 0.5) * 2.6, 2.2);
                const hit = raycastMesh(mesh, origin, dir);
                if (hit) {
                    pushVectors(rays, origin, hit.point);
                    pushVectors(hits, hit.point);
                } else {
                    pushVectors(misses, origin, origin.Add(dir.Scale(4.4)));
                }
            }
        }

        return [
            mesh,
            { kind: 'lines', positions: rays, color: 0xffd166, opacity: 0.9 },
            { kind: 'lines', positions: misses, color: 0x39414e, opacity: 0.5 },
            { kind: 'points', positions: hits, color: 0xff6b6b, size: 0.07 },
        ];
    },
};
