// Raycasting a triangle mesh.
//
// `Triangle3D.Raycast(Ray3D)` is the stdlib's Moller-Trumbore intersection: it
// solves the ray/triangle system for the barycentric pair and the distance in
// one pass, with no precomputed plane, and returns the full RayHit3D record
// (distance, position, ray-facing normal, barycentric coordinate, UV).
//
// The sweep over the mesh's triangles is brute force. `TriangleMesh3D.Primitives`
// gathers the faces into concrete Triangle3D values; the BVH sample is the
// version that prunes.

import type { Drawable, Sample } from '../core/types.js';
import {
    Direction3D, Line3D, Point3D, Ray3D, RayHit3D, TriangleMesh3D, Vector3D,
} from '../plato/plato.g.js';
import { toArray } from '../core/meshBuilder.js';
import { buildIcosphere } from './icosphere.js';

export interface MeshHit {
    hit: RayHit3D;
    triangle: number;
}

/** Closest hit against every triangle of the mesh. */
export function raycastMesh(mesh: TriangleMesh3D, ray: Ray3D): MeshHit | null {
    let best: MeshHit | null = null;
    const triangles = toArray(mesh.Primitives());
    for (let i = 0; i < triangles.length; i++) {
        const hit = triangles[i].Raycast(ray);
        if (hit.Hit && (best === null || hit.Distance < best.hit.Distance))
            best = { hit, triangle: i };
    }
    return best;
}

export const raycastSample: Sample = {
    id: 'raycast',
    title: 'Raycasting',
    description: 'Triangle3D.Raycast (Moller-Trumbore) swept over TriangleMesh3D.Primitives.',
    build(): Drawable[] {
        const mesh = buildIcosphere(2);
        const direction = new Direction3D(new Vector3D(0, 0, -1));

        const rays: Line3D[] = [];
        const misses: Line3D[] = [];
        const hits: Point3D[] = [];
        for (let iy = 0; iy < 9; iy++) {
            for (let ix = 0; ix < 9; ix++) {
                const origin = new Point3D((ix / 8 - 0.5) * 2.6, (iy / 8 - 0.5) * 2.6, 2.2);
                const ray = new Ray3D(origin, direction);
                const found = raycastMesh(mesh, ray);
                if (found) {
                    rays.push(new Line3D(origin, found.hit.Position));
                    hits.push(found.hit.Position);
                } else {
                    misses.push(new Line3D(origin, ray.PointAt(4.4)));
                }
            }
        }

        return [
            { kind: 'mesh', mesh, color: 0x4da3ff, opacity: 0.45 },
            { kind: 'lines', segments: rays, color: 0xffd166, opacity: 0.9 },
            { kind: 'lines', segments: misses, color: 0x39414e, opacity: 0.5 },
            { kind: 'points', points: hits, color: 0xff6b6b, size: 0.07 },
        ];
    },
};
