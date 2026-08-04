// Scene-description types shared by all samples.
//
// Geometry itself is the generated Plato stdlib (src/plato/plato.g.ts): a mesh
// IS a TriangleMesh3D, a point cloud IS an array of Point3D. These types only
// say how to draw it. Flattening to renderer buffers happens once, in the
// adapter layer (src/adapters/three.ts).

import type { Line3D, Point3D, TriangleMesh3D } from '../plato/plato.g.js';

/** A Plato triangle mesh, drawn as a surface. */
export interface MeshData {
    kind: 'mesh';
    mesh: TriangleMesh3D;
    color?: number;
    opacity?: number;
    /** Faceted shading; when false the adapter uses stdlib vertex normals. */
    flatShading?: boolean;
    wireframe?: boolean;
}

/** Disjoint line segments. */
export interface LinesData {
    kind: 'lines';
    segments: Line3D[];
    color?: number;
    opacity?: number;
}

/** A point cloud. */
export interface PointsData {
    kind: 'points';
    points: Point3D[];
    color?: number;
    size?: number;
}

export type Drawable = MeshData | LinesData | PointsData;

/** A sample is a pure function from nothing to drawables: no DOM, no Three.js. */
export interface Sample {
    id: string;
    title: string;
    description: string;
    build(): Drawable[];
}
