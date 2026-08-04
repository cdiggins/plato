// Icosphere by recursive 4:1 subdivision.
//
// The seed is the stdlib's regular icosahedron (one of the five Platonic solids
// in `library Polyhedra`). Each level is `TriangleMesh3D.SplitEdges` with every
// edge marked: each edge gains its midpoint and each triangle becomes four.
// Projecting onto the unit sphere is what turns the refinement into a sphere
// rather than a subdivided icosahedron.
//
// The edge-midpoint bookkeeping that keeps the mesh watertight is stdlib
// topology, so V - E + F = 2 holds at every level (asserted in tests).
//
// `LoopSubdivided` would be the smoother alternative, but it currently returns
// NaN for every original vertex — see plato-444.

import type { Drawable, Sample } from '../core/types.js';
import { PolygonMesh3D, Point3D, TriangleMesh3D, Vector3D } from '../plato/plato.g.js';
import { fromArray, translateMesh } from '../core/meshBuilder.js';

/** Pushes every vertex out to the unit sphere. */
const projectToSphere = (mesh: TriangleMesh3D): TriangleMesh3D =>
    mesh.Deform(p => new Point3D(0, 0, 0).Add(p.PositionVector().Normalize()));

/** One 4:1 split: every edge of the mesh gains a midpoint. */
function splitEveryEdge(mesh: TriangleMesh3D): TriangleMesh3D {
    const topology = mesh.TopologyOf();
    const everyEdge = fromArray(new Array<boolean>(topology.UndirectedEdges.Count()).fill(true));
    return mesh.SplitEdges(topology, everyEdge);
}

export function buildIcosphere(subdivisions: number): TriangleMesh3D {
    let mesh = projectToSphere(PolygonMesh3D.Icosahedron().ToTriangleMesh());
    for (let level = 0; level < subdivisions; level++)
        mesh = projectToSphere(splitEveryEdge(mesh));
    return mesh;
}

export const icosphereSample: Sample = {
    id: 'icosphere',
    title: 'Icosphere Subdivision',
    description: 'Polyhedra.Icosahedron seeded, then TriangleMesh3D.SplitEdges 4:1, ' +
        'reprojected to the sphere at each level.',
    build(): Drawable[] {
        const levels = [0, 1, 2, 3];
        return levels.map((level, i): Drawable => ({
            kind: 'mesh',
            mesh: translateMesh(buildIcosphere(level),
                new Vector3D((i - (levels.length - 1) / 2) * 2.3, 0, 0)),
            color: 0x4da3ff,
            flatShading: true,
        }));
    },
};
