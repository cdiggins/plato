// Interop between Plato's IArray and native TypeScript arrays, plus the few
// mesh constructions the stdlib does not name.
//
// Geometry itself belongs to the stdlib: vertex normals are
// TriangleMesh3D.VertexNormalVectors, a tessellated parametric surface is
// IParametricSurface.ToQuadMesh, subdivision is TriangleMesh3D.LoopSubdivided.
// Nothing here re-implements any of them.

import {
    Intrinsics, Line3D, Point3D, TriangleFace, TriangleMesh3D, VertexIndex, Vector3D,
    type IArray,
} from '../plato/plato.g.js';

/** Reads a Plato IArray into a native array. */
export function toArray<T>(xs: IArray<T>): T[] {
    const out = new Array<T>(xs.Count());
    for (let i = 0; i < out.length; i++)
        out[i] = xs.At(i);
    return out;
}

/** Wraps a native array as a Plato IArray. */
export const fromArray = <T>(xs: T[]): IArray<T> => Intrinsics.MakeArray(...xs);

/** Builds a TriangleMesh3D from positions and flat triangle-corner indices. */
export function meshFromIndices(positions: Point3D[], indices: number[]): TriangleMesh3D {
    const faces: TriangleFace[] = [];
    for (let i = 0; i < indices.length; i += 3)
        faces.push(new TriangleFace(
            new VertexIndex(indices[i]), new VertexIndex(indices[i + 1]), new VertexIndex(indices[i + 2])));
    return new TriangleMesh3D(fromArray(positions), fromArray(faces));
}

/** The triangle-corner indices of a mesh, flattened for renderers and tests. */
export function meshIndices(mesh: TriangleMesh3D): number[] {
    const out: number[] = [];
    for (const f of toArray(mesh.Faces))
        out.push(f.A.Value, f.B.Value, f.C.Value);
    return out;
}

/** The vertex positions of a mesh as a native array. */
export const meshVertices = (mesh: TriangleMesh3D): Point3D[] => toArray(mesh.Positions);

/** The 12 edges of an axis-aligned box, as segments. */
export function boxEdges(min: Point3D, max: Point3D): Line3D[] {
    const c = [
        new Point3D(min.X, min.Y, min.Z), new Point3D(max.X, min.Y, min.Z),
        new Point3D(max.X, max.Y, min.Z), new Point3D(min.X, max.Y, min.Z),
        new Point3D(min.X, min.Y, max.Z), new Point3D(max.X, min.Y, max.Z),
        new Point3D(max.X, max.Y, max.Z), new Point3D(min.X, max.Y, max.Z),
    ];
    const pairs = [[0, 1], [1, 2], [2, 3], [3, 0], [4, 5], [5, 6], [6, 7], [7, 4],
                   [0, 4], [1, 5], [2, 6], [3, 7]];
    return pairs.map(([a, b]) => new Line3D(c[a], c[b]));
}

/** Moves every vertex of a mesh by an offset (for side-by-side comparisons). */
export const translateMesh = (mesh: TriangleMesh3D, offset: Vector3D): TriangleMesh3D =>
    mesh.Deform(p => p.Add(offset));
