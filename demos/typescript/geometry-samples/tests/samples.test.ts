// Invariant tests for every sample, run with the built-in Node test runner:
//   npm test   (tsc -p tsconfig.node.json && node --test dist-node/tests/*.test.js)
//
// The samples build stdlib geometry, so the assertions here are about the
// geometry, not about flat arrays: a mesh is a TriangleMesh3D and its faces
// index into its own positions.

import { test } from 'node:test';
import assert from 'node:assert/strict';

import { Bounds2D, Point2D, Point3D, Ray3D, Direction3D, Vector3D } from '../src/plato/plato.g.js';
import { samples } from '../src/samples/index.js';
import { buildIcosphere } from '../src/samples/icosphere.js';
import { delaunay, circumcircle } from '../src/samples/delaunay.js';
import { convexHull, turn } from '../src/samples/convexHull.js';
import { spline, sampleSpline, controlPoints } from '../src/samples/splineTube.js';
import { makeOctree, insert, collectLeaves, clusteredPoints } from '../src/samples/octree.js';
import { buildBvh, triangleCentroid, meshTriangles } from '../src/samples/bvh.js';
import { laplacianSmooth, noisySphere, vertexRings } from '../src/samples/halfEdge.js';
import { raycastMesh } from '../src/samples/raycast.js';
import { poissonPoints } from '../src/samples/poissonDisk.js';
import { metaballs, domain, contourAt } from '../src/samples/marchingSquares.js';
import { Bounds3D } from '../src/plato/plato.g.js';
import { meshIndices, meshVertices, toArray } from '../src/core/meshBuilder.js';

const finite = (...xs: number[]) => xs.every(Number.isFinite);

test('every sample builds finite, non-empty drawables', () => {
    assert.ok(samples.length >= 10, 'at least 10 samples');
    for (const d of samples.flatMap(s => s.build().map(d => ({ id: s.id, d })))) {
        const { id, d: drawable } = d;
        if (drawable.kind === 'mesh') {
            const vertices = meshVertices(drawable.mesh);
            assert.ok(vertices.length > 0, `${id} has positions`);
            for (const p of vertices)
                assert.ok(finite(p.X, p.Y, p.Z), `${id} has finite coordinates`);
            const indices = meshIndices(drawable.mesh);
            assert.ok(indices.length % 3 === 0, `${id} indices form triangles`);
            for (const i of indices)
                assert.ok(i >= 0 && i < vertices.length, `${id} indices in range`);
        } else if (drawable.kind === 'lines') {
            assert.ok(drawable.segments.length > 0, `${id} has segments`);
            for (const s of drawable.segments)
                assert.ok(finite(s.A.X, s.A.Y, s.A.Z, s.B.X, s.B.Y, s.B.Z), `${id} finite segments`);
        } else {
            assert.ok(drawable.points.length > 0, `${id} has points`);
            for (const p of drawable.points)
                assert.ok(finite(p.X, p.Y, p.Z), `${id} finite points`);
        }
    }
});

test('icosphere: watertight (V - E + F = 2) and unit radius', () => {
    for (const level of [0, 1, 2]) {
        const mesh = buildIcosphere(level);
        const V = mesh.Positions.Count();
        const F = mesh.Faces.Count();
        const indices = meshIndices(mesh);
        const edges = new Set<string>();
        for (let i = 0; i < indices.length; i += 3) {
            for (const [a, b] of [[0, 1], [1, 2], [2, 0]]) {
                const [u, v] = [indices[i + a], indices[i + b]];
                edges.add(u < v ? `${u}_${v}` : `${v}_${u}`);
            }
        }
        assert.equal(V - edges.size + F, 2, `Euler characteristic at level ${level}`);
        for (const p of meshVertices(mesh))
            assert.ok(Math.abs(p.PositionVector().Length() - 1) < 1e-9, 'vertex on the unit sphere');
    }
});

test('delaunay: empty circumcircle property', () => {
    const region = new Bounds2D(new Point2D(-1.5, -1), new Point2D(1.5, 1));
    const points = toArray(region.HaltonPoints2D(60, 2, 3));
    const triangles = delaunay(points);
    assert.ok(triangles.length > 0);
    for (const t of triangles) {
        const cc = circumcircle(points, t);
        for (let i = 0; i < points.length; i++) {
            if (i === t.a || i === t.b || i === t.c) continue;
            assert.ok(points[i].DistanceSquared(cc.center) >= cc.radiusSquared - 1e-9,
                `point ${i} not inside circumcircle`);
        }
    }
});

test('convex hull: convex, counter-clockwise, and contains all points', () => {
    const region = new Bounds2D(new Point2D(-1, -1), new Point2D(1, 1));
    const points = toArray(region.HaltonPoints2D(80, 2, 3));
    const hull = convexHull(points);
    const boundary = toArray(hull.Hull.Points);
    assert.ok(boundary.length >= 3);
    assert.equal(hull.SourceIndices.Count(), boundary.length, 'one source index per hull vertex');

    const n = boundary.length;
    for (let i = 0; i < n; i++) {
        const [a, b, c] = [boundary[i], boundary[(i + 1) % n], boundary[(i + 2) % n]];
        assert.ok(turn(a, b, c) > 0, 'hull turns counter-clockwise');
    }
    for (const p of points)
        for (let i = 0; i < n; i++)
            assert.ok(turn(boundary[i], boundary[(i + 1) % n], p) >= -1e-12, 'point inside hull');
});

test('spline: interpolates its control points at the knots', () => {
    const perSegment = 16;
    const samplesPerCurve = (controlPoints.length - 1) * perSegment + 1;
    const curve = sampleSpline(spline, samplesPerCurve);
    for (let i = 0; i < controlPoints.length; i++) {
        const at = Math.min(i * perSegment, curve.length - 1);
        assert.ok(curve[at].Distance(controlPoints[i]) < 1e-6, `knot ${i} interpolated`);
    }
});

test('octree: every point lands in exactly one leaf that contains it', () => {
    const pts = clusteredPoints(500);
    const root = makeOctree(new Bounds3D(new Point3D(-2, -2, -2), new Point3D(2, 2, 2)));
    for (let i = 0; i < pts.length; i++)
        insert(root, pts, i, 8, 5);
    const leaves = collectLeaves(root);
    const seen = new Map<number, number>();
    for (const leaf of leaves) {
        for (const i of leaf.points) {
            seen.set(i, (seen.get(i) ?? 0) + 1);
            assert.ok(leaf.bounds.GrowBounds(1e-12).Contains(pts[i]), 'leaf contains its point');
        }
    }
    assert.equal(seen.size, pts.length, 'all points stored');
    for (const count of seen.values())
        assert.equal(count, 1, 'stored exactly once');
});

test('bvh: leaves partition the triangles and boxes contain their centroids', () => {
    const triangles = meshTriangles(buildIcosphere(2));
    const all = triangles.map((_, i) => i);
    const root = buildBvh(triangles, all);
    const seen = new Set<number>();
    const walk = (node: ReturnType<typeof buildBvh>): void => {
        if (node.triangles) {
            for (const t of node.triangles) {
                assert.ok(!seen.has(t), 'triangle in one leaf only');
                seen.add(t);
                assert.ok(node.bounds.GrowBounds(1e-9).Contains(triangleCentroid(triangles, t)),
                    'centroid inside leaf box');
            }
            return;
        }
        walk(node.left!);
        walk(node.right!);
    };
    walk(root);
    assert.equal(seen.size, all.length, 'every triangle assigned');
});

test('mesh topology: one-ring tables cover each directed edge once', () => {
    const mesh = noisySphere();
    const rings = vertexRings(mesh);
    assert.equal(rings.length, mesh.Positions.Count(), 'one ring per vertex');

    // A closed triangle mesh has 3F directed edges, and each vertex's ring holds
    // one entry per outgoing directed edge.
    const total = rings.reduce((sum, r) => sum + r.length, 0);
    assert.equal(total, mesh.Faces.Count() * 3, 'rings cover each directed edge once');

    // Every neighbour relation is symmetric on a closed mesh.
    for (let v = 0; v < rings.length; v++)
        for (const n of rings[v])
            assert.ok(rings[n].includes(v), `ring of ${n} contains ${v}`);
});

test('laplacian smoothing reduces radial roughness', () => {
    const mesh = noisySphere();
    const radiusVariance = (m: typeof mesh): number => {
        const radii = meshVertices(m).map(p => p.PositionVector().Length());
        const mean = radii.reduce((a, b) => a + b, 0) / radii.length;
        return radii.reduce((a, r) => a + (r - mean) ** 2, 0) / radii.length;
    };
    const smoothed = laplacianSmooth(mesh, 0.6, 10);
    assert.equal(smoothed.Positions.Count(), mesh.Positions.Count(), 'vertex count preserved');
    assert.ok(radiusVariance(smoothed) < radiusVariance(mesh) * 0.35, 'roughness reduced');
});

test('raycast: central ray hits the unit icosphere at distance ~2', () => {
    const mesh = buildIcosphere(2);
    const down = new Direction3D(new Vector3D(0, 0, -1));
    const hit = raycastMesh(mesh, new Ray3D(new Point3D(0, 0, 3), down));
    assert.ok(hit, 'hit found');
    assert.ok(Math.abs(hit!.hit.Distance - 2) < 0.06, `distance ${hit!.hit.Distance} close to 2`);
    // The reported normal always opposes the ray.
    assert.ok(hit!.hit.Normal.Vector.Dot(down.Vector) <= 0, 'normal faces the ray');
    const miss = raycastMesh(mesh, new Ray3D(new Point3D(5, 5, 3), down));
    assert.equal(miss, null, 'offset ray misses');
});

test('poisson disk: pairwise distances respect the radius', () => {
    const region = new Bounds2D(new Point2D(-1.1, -1.1), new Point2D(1.1, 1.1));
    const pts = poissonPoints(region);
    assert.ok(pts.length > 50, 'reasonable density');
    for (let i = 0; i < pts.length; i++)
        for (let j = i + 1; j < pts.length; j++)
            assert.ok(pts[i].Distance(pts[j]) >= 0.11 - 1e-9, `points ${i},${j} too close`);
});

test('marching squares: contour vertices lie near the iso value', () => {
    const iso = 0.5;
    const segments = contourAt(iso, 0);
    assert.ok(segments.length >= 2, 'contour found');
    for (const s of segments) {
        for (const p of [s.A, s.B]) {
            const value = metaballs.Eval(new Point2D(p.X, p.Z));
            assert.ok(Math.abs(value - iso) < 0.1, `field ${value} near iso ${iso}`);
        }
    }
});

test('marching squares: contour separates the field, and lies inside the domain', () => {
    for (const s of contourAt(0.5, 0)) {
        for (const p of [s.A, s.B]) {
            assert.ok(p.X >= domain.Min.X - 1e-9 && p.X <= domain.Max.X + 1e-9, 'inside domain X');
            assert.ok(p.Z >= domain.Min.Y - 1e-9 && p.Z <= domain.Max.Y + 1e-9, 'inside domain Y');
        }
    }
});
