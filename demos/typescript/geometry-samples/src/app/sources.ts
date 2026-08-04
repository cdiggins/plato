// Source text for the code panel. Vite's `?raw` suffix imports the file
// contents as a string; this is browser-only, which is why it lives in app/
// and not in the sample registry.

import meshesLibrary from '../../../../../stdlib/geometry/meshes.library.plato?raw';
import fieldsLibrary from '../../../../../stdlib/geometry/fields-implicits.library.plato?raw';
import platoGenerated from '../plato/plato.g.ts?raw';

/**
 * Extra code-panel tabs shown for every sample: the two stdlib library files
 * carrying the tessellation and contouring the samples lean on, plus the
 * generated TypeScript they are compiled into.
 */
export const sharedTabs: { label: string; source: string }[] = [
    { label: 'meshes.library.plato', source: meshesLibrary },
    { label: 'fields-implicits.library.plato', source: fieldsLibrary },
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
