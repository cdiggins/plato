// Scene-description types shared by all samples.
//
// Geometry itself comes from the generated Plato library (src/plato/plato.g.ts);
// these types only describe what to draw. The adapter layer
// (src/adapters/three.ts) converts Drawables into Three.js objects.

/** An indexed triangle mesh. Positions are flat [x0,y0,z0, x1,y1,z1, ...]. */
export interface MeshData {
    kind: 'mesh';
    positions: number[];
    indices: number[];
    /** Optional; computed from faces by the adapter when absent. */
    normals?: number[];
    color?: number;
    opacity?: number;
    flatShading?: boolean;
    wireframe?: boolean;
}

/** Disjoint line segments: every consecutive pair of points is one segment. */
export interface LinesData {
    kind: 'lines';
    positions: number[];
    color?: number;
    opacity?: number;
}

/** A point cloud. */
export interface PointsData {
    kind: 'points';
    positions: number[];
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
