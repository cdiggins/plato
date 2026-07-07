// Helpers for constructing MeshData and LinesData from Plato geometry.

import type { MeshData } from './types.js';
import { Vector3D } from '../plato/plato.g.js';

/** Appends the components of vectors to a flat position array. */
export function pushVectors(out: number[], ...vs: Vector3D[]): void {
    for (const v of vs)
        out.push(v.X, v.Y, v.Z);
}

/** Extracts vertex i of a flat position array as a Vector3D. */
export function vertexAt(positions: number[], i: number): Vector3D {
    return new Vector3D(positions[i * 3], positions[i * 3 + 1], positions[i * 3 + 2]);
}

/** Area-weighted vertex normals (the cross product length weighs by area). */
export function computeVertexNormals(positions: number[], indices: number[]): number[] {
    const normals = new Array<number>(positions.length).fill(0);
    for (let i = 0; i < indices.length; i += 3) {
        const [ia, ib, ic] = [indices[i], indices[i + 1], indices[i + 2]];
        const a = vertexAt(positions, ia);
        const b = vertexAt(positions, ib);
        const c = vertexAt(positions, ic);
        const n = b.Subtract(a).Cross(c.Subtract(a));
        for (const j of [ia * 3, ib * 3, ic * 3]) {
            normals[j] += n.X;
            normals[j + 1] += n.Y;
            normals[j + 2] += n.Z;
        }
    }
    for (let i = 0; i < normals.length; i += 3) {
        const n = new Vector3D(normals[i], normals[i + 1], normals[i + 2]);
        const len = n.Length();
        const unit = len > 1e-12 ? n.Scale(1 / len) : n;
        normals[i] = unit.X;
        normals[i + 1] = unit.Y;
        normals[i + 2] = unit.Z;
    }
    return normals;
}

/**
 * Tessellates a parametric surface f(u, v) over the unit square into a quad
 * grid of nu x nv cells (each split into two triangles).
 */
export function gridMesh(nu: number, nv: number, f: (u: number, v: number) => Vector3D): MeshData {
    const positions: number[] = [];
    const indices: number[] = [];
    for (let iv = 0; iv <= nv; iv++)
        for (let iu = 0; iu <= nu; iu++)
            pushVectors(positions, f(iu / nu, iv / nv));
    const stride = nu + 1;
    for (let iv = 0; iv < nv; iv++) {
        for (let iu = 0; iu < nu; iu++) {
            const i00 = iv * stride + iu;
            const [i10, i01, i11] = [i00 + 1, i00 + stride, i00 + stride + 1];
            indices.push(i00, i01, i10, i10, i01, i11);
        }
    }
    return { kind: 'mesh', positions, indices, normals: computeVertexNormals(positions, indices) };
}

/** Builds a MeshData from separate vertex and triangle-index lists. */
export function meshFromVertices(vertices: Vector3D[], indices: number[]): MeshData {
    const positions: number[] = [];
    pushVectors(positions, ...vertices);
    return { kind: 'mesh', positions, indices, normals: computeVertexNormals(positions, indices) };
}

/** Appends the 12 edges of an axis-aligned box to a flat segment list. */
export function appendBoxEdges(min: Vector3D, max: Vector3D, out: number[]): void {
    const c = [
        new Vector3D(min.X, min.Y, min.Z), new Vector3D(max.X, min.Y, min.Z),
        new Vector3D(max.X, max.Y, min.Z), new Vector3D(min.X, max.Y, min.Z),
        new Vector3D(min.X, min.Y, max.Z), new Vector3D(max.X, min.Y, max.Z),
        new Vector3D(max.X, max.Y, max.Z), new Vector3D(min.X, max.Y, max.Z),
    ];
    const edges = [0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7];
    for (const i of edges)
        out.push(c[i].X, c[i].Y, c[i].Z);
}

/** Translates a mesh in place (useful for side-by-side comparisons). */
export function translateMesh(mesh: MeshData, offset: Vector3D): MeshData {
    for (let i = 0; i < mesh.positions.length; i += 3) {
        mesh.positions[i] += offset.X;
        mesh.positions[i + 1] += offset.Y;
        mesh.positions[i + 2] += offset.Z;
    }
    return mesh;
}
