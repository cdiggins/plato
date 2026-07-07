// Source text for the code panel. Vite's `?raw` suffix imports the file
// contents as a string; this is browser-only, which is why it lives in app/
// and not in the sample registry.

import platoSource from '../../../../plato-src/geometry.plato?raw';
import platoGenerated from '../plato/plato.g.ts?raw';

/** Extra code-panel tabs shown for every sample. */
export const sharedTabs: { label: string; source: string }[] = [
    { label: 'geometry.plato', source: platoSource },
    { label: 'plato.g.ts', source: platoGenerated },
];

import parametricSurface from '../samples/parametricSurface.ts?raw';
import icosphere from '../samples/icosphere.ts?raw';
import terrain from '../samples/terrain.ts?raw';
import delaunay from '../samples/delaunay.ts?raw';
import convexHull from '../samples/convexHull.ts?raw';
import splineTube from '../samples/splineTube.ts?raw';
import octree from '../samples/octree.ts?raw';
import bvh from '../samples/bvh.ts?raw';
import halfEdge from '../samples/halfEdge.ts?raw';
import raycast from '../samples/raycast.ts?raw';
import poissonDisk from '../samples/poissonDisk.ts?raw';
import marchingSquares from '../samples/marchingSquares.ts?raw';

export const sampleSources: Record<string, string> = {
    'parametric-surface': parametricSurface,
    'icosphere': icosphere,
    'terrain': terrain,
    'delaunay': delaunay,
    'convex-hull': convexHull,
    'spline-tube': splineTube,
    'octree': octree,
    'bvh': bvh,
    'half-edge': halfEdge,
    'raycast': raycast,
    'poisson-disk': poissonDisk,
    'marching-squares': marchingSquares,
};
