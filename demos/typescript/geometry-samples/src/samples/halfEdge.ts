// Half-edge mesh data structure + Laplacian smoothing.
//
// Every triangle contributes three directed half-edges. Each half-edge knows
// its origin vertex, the next half-edge around its face, and its twin (the
// opposite direction across the shared edge). This makes one-ring vertex
// traversal O(degree) with no searching: twin(next(next(e))) is the next
// outgoing edge around the origin of e.

import type { MeshData, Sample } from '../core/types.js';
import { Vector3D } from '../plato/plato.g.js';
import { computeVertexNormals, translateMesh, vertexAt } from '../core/meshBuilder.js';
import { buildIcosphere } from './icosphere.js';
import { valueNoise } from './terrain.js';

export interface HalfEdge {
    origin: number; // vertex index at the start of the edge
    next: number;   // next half-edge counter-clockwise around the face
    twin: number;   // opposite half-edge, or -1 on a boundary
}

export interface HalfEdgeMesh {
    halfEdges: HalfEdge[];
    /** One outgoing half-edge per vertex. */
    vertexEdge: number[];
    vertexCount: number;
}

export function buildHalfEdgeMesh(indices: number[], vertexCount: number): HalfEdgeMesh {
    const halfEdges: HalfEdge[] = [];
    const vertexEdge = new Array<number>(vertexCount).fill(-1);
    const edgeMap = new Map<string, number>(); // "from_to" -> half-edge index

    for (let f = 0; f < indices.length; f += 3) {
        const base = halfEdges.length;
        for (let k = 0; k < 3; k++) {
            const from = indices[f + k];
            const to = indices[f + ((k + 1) % 3)];
            halfEdges.push({ origin: from, next: base + ((k + 1) % 3), twin: -1 });
            edgeMap.set(`${from}_${to}`, base + k);
            vertexEdge[from] = base + k;
        }
    }
    for (const [key, e] of edgeMap) {
        const [from, to] = key.split('_');
        const twin = edgeMap.get(`${to}_${from}`);
        if (twin !== undefined)
            halfEdges[e].twin = twin;
    }
    return { halfEdges, vertexEdge, vertexCount };
}

/** Visits the one-ring neighbor vertices of v (closed meshes). */
export function oneRing(mesh: HalfEdgeMesh, v: number): number[] {
    const result: number[] = [];
    const start = mesh.vertexEdge[v];
    let e = start;
    do {
        const he = mesh.halfEdges[e];
        result.push(mesh.halfEdges[he.next].origin);
        e = mesh.halfEdges[mesh.halfEdges[he.next].next].twin; // next outgoing edge
    } while (e !== start && e !== -1);
    return result;
}

/** Uniform Laplacian smoothing: move each vertex toward its ring average. */
export function laplacianSmooth(positions: number[], mesh: HalfEdgeMesh, lambda: number, iterations: number): number[] {
    let current = positions.slice();
    for (let iter = 0; iter < iterations; iter++) {
        const next = current.slice();
        for (let v = 0; v < mesh.vertexCount; v++) {
            const ring = oneRing(mesh, v);
            let avg = new Vector3D(0, 0, 0);
            for (const n of ring)
                avg = avg.Add(vertexAt(current, n));
            avg = avg.Scale(1 / ring.length);
            const moved = vertexAt(current, v).Lerp(avg, lambda);
            next[v * 3] = moved.X;
            next[v * 3 + 1] = moved.Y;
            next[v * 3 + 2] = moved.Z;
        }
        current = next;
    }
    return current;
}

/** An icosphere with noisy radius: the smoothing test subject. */
export function noisySphere(): MeshData {
    const mesh = buildIcosphere(3);
    for (let i = 0; i < mesh.positions.length; i += 3) {
        const dir = vertexAt(mesh.positions, i / 3).Normalize();
        const bump = 1 + 0.28 * (valueNoise(dir.X * 5 + 5, dir.Y * 5 + dir.Z * 4 + 5) - 0.5) * 2;
        const p = dir.Scale(bump);
        mesh.positions[i] = p.X;
        mesh.positions[i + 1] = p.Y;
        mesh.positions[i + 2] = p.Z;
    }
    return mesh;
}

export const halfEdgeSample: Sample = {
    id: 'half-edge',
    title: 'Half-Edge + Smoothing',
    description: 'Half-edge connectivity drives one-ring Laplacian smoothing (before / after).',
    build() {
        const noisy = noisySphere();
        const he = buildHalfEdgeMesh(noisy.indices, noisy.positions.length / 3);
        const smoothedPositions = laplacianSmooth(noisy.positions, he, 0.6, 12);

        const smoothed: MeshData = {
            kind: 'mesh',
            positions: smoothedPositions,
            indices: noisy.indices.slice(),
            normals: computeVertexNormals(smoothedPositions, noisy.indices),
            color: 0x6fbf73,
        };
        noisy.color = 0xff8c66;
        noisy.flatShading = true;
        translateMesh(noisy, new Vector3D(-1.4, 0, 0));
        translateMesh(smoothed, new Vector3D(1.4, 0, 0));
        return [noisy, smoothed];
    },
};
