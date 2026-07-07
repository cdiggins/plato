// Invariant tests for every sample, run with the built-in Node test runner:
//   npm test   (tsc -p tsconfig.node.json && node --test dist-node/tests/*.test.js)

import { test } from 'node:test';
import assert from 'node:assert/strict';

import { Vector2D, Vector3D } from '../src/plato/plato.g.js';
import { samples } from '../src/samples/index.js';
import { buildIcosphere } from '../src/samples/icosphere.js';
import { delaunay, circumcircle } from '../src/samples/delaunay.js';
import { convexHull, turn } from '../src/samples/convexHull.js';
import { sampleSpline, controlPoints } from '../src/samples/splineTube.js';
import { makeOctree, insert, collectLeaves, clusteredPoints } from '../src/samples/octree.js';
import { buildBvh, triangleCentroid } from '../src/samples/bvh.js';
import { buildHalfEdgeMesh, laplacianSmooth, noisySphere, oneRing } from '../src/samples/halfEdge.js';
import { raycastMesh } from '../src/samples/raycast.js';
import { poissonDisk } from '../src/samples/poissonDisk.js';
import { marchingSquares, metaballs } from '../src/samples/marchingSquares.js';
import { makeRng, range } from '../src/core/random.js';
import { vertexAt } from '../src/core/meshBuilder.js';

test('every sample builds finite, non-empty drawables', () => {
    assert.ok(samples.length >= 10, 'at least 10 samples');
    for (const sample of samples) {
        const drawables = sample.build();
        assert.ok(drawables.length > 0, `${sample.id} produced drawables`);
        for (const d of drawables) {
            assert.ok(d.positions.length > 0, `${sample.id} has positions`);
            assert.ok(d.positions.length % 3 === 0, `${sample.id} positions are xyz triples`);
            for (const v of d.positions)
                assert.ok(Number.isFinite(v), `${sample.id} has finite coordinates`);
            if (d.kind === 'mesh') {
                assert.ok(d.indices.length % 3 === 0, `${sample.id} indices form triangles`);
                const vertexCount = d.positions.length / 3;
                for (const i of d.indices)
                    assert.ok(i >= 0 && i < vertexCount, `${sample.id} indices in range`);
            }
            if (d.kind === 'lines')
                assert.ok((d.positions.length / 3) % 2 === 0, `${sample.id} lines come in pairs`);
        }
    }
});

test('icosphere: watertight (V - E + F = 2) and unit radius', () => {
    for (const level of [0, 1, 2]) {
        const mesh = buildIcosphere(level);
        const V = mesh.positions.length / 3;
        const F = mesh.indices.length / 3;
        const edges = new Set<string>();
        for (let i = 0; i < mesh.indices.length; i += 3) {
            for (const [a, b] of [[0, 1], [1, 2], [2, 0]]) {
                const [u, v] = [mesh.indices[i + a], mesh.indices[i + b]];
                edges.add(u < v ? `${u}_${v}` : `${v}_${u}`);
            }
        }
        assert.equal(V - edges.size + F, 2, `Euler characteristic at level ${level}`);
        for (let i = 0; i < V; i++)
            assert.ok(Math.abs(vertexAt(mesh.positions, i).Length() - 1) < 1e-9);
    }
});

test('delaunay: empty circumcircle property', () => {
    const rng = makeRng(7);
    const points: Vector2D[] = [];
    for (let i = 0; i < 60; i++)
        points.push(new Vector2D(range(rng, -1.5, 1.5), range(rng, -1, 1)));
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

test('convex hull: convex and contains all points', () => {
    const rng = makeRng(11);
    const points: Vector2D[] = [];
    for (let i = 0; i < 80; i++)
        points.push(new Vector2D(range(rng, -1, 1), range(rng, -1, 1)));
    const hull = convexHull(points);
    assert.ok(hull.length >= 3);
    const n = hull.length;
    for (let i = 0; i < n; i++) {
        const [a, b, c] = [points[hull[i]], points[hull[(i + 1) % n]], points[hull[(i + 2) % n]]];
        assert.ok(turn(a, b, c) > 0, 'hull turns counter-clockwise');
    }
    for (const p of points)
        for (let i = 0; i < n; i++)
            assert.ok(turn(points[hull[i]], points[hull[(i + 1) % n]], p) >= -1e-12, 'point inside hull');
});

test('spline: interpolates its control points at the knots', () => {
    const perSegment = 16;
    const curve = sampleSpline(controlPoints, perSegment);
    for (let i = 0; i < controlPoints.length; i++) {
        const at = Math.min(i * perSegment, curve.length - 1);
        assert.ok(curve[at].Distance(controlPoints[i]) < 1e-9, `knot ${i} interpolated`);
    }
});

test('octree: every point lands in exactly one leaf that contains it', () => {
    const pts = clusteredPoints(500);
    const root = makeOctree(new Vector3D(-2, -2, -2), new Vector3D(2, 2, 2));
    for (let i = 0; i < pts.length; i++)
        insert(root, pts, i, 8, 5);
    const leaves = collectLeaves(root);
    const seen = new Map<number, number>();
    for (const leaf of leaves) {
        for (const i of leaf.points) {
            seen.set(i, (seen.get(i) ?? 0) + 1);
            const p = pts[i];
            assert.ok(
                p.X >= leaf.min.X - 1e-12 && p.X <= leaf.max.X + 1e-12 &&
                p.Y >= leaf.min.Y - 1e-12 && p.Y <= leaf.max.Y + 1e-12 &&
                p.Z >= leaf.min.Z - 1e-12 && p.Z <= leaf.max.Z + 1e-12,
                'leaf contains its point');
        }
    }
    assert.equal(seen.size, pts.length, 'all points stored');
    for (const count of seen.values())
        assert.equal(count, 1, 'stored exactly once');
});

test('bvh: leaves partition the triangles and boxes contain their centroids', () => {
    const mesh = buildIcosphere(2);
    const allTris = Array.from({ length: mesh.indices.length / 3 }, (_, i) => i);
    const root = buildBvh(mesh, allTris);
    const seen = new Set<number>();
    const walk = (node: ReturnType<typeof buildBvh>): void => {
        if (node.triangles) {
            for (const t of node.triangles) {
                assert.ok(!seen.has(t), 'triangle in one leaf only');
                seen.add(t);
                const c = triangleCentroid(mesh, t);
                assert.ok(
                    c.X >= node.min.X - 1e-9 && c.X <= node.max.X + 1e-9 &&
                    c.Y >= node.min.Y - 1e-9 && c.Y <= node.max.Y + 1e-9 &&
                    c.Z >= node.min.Z - 1e-9 && c.Z <= node.max.Z + 1e-9,
                    'centroid inside leaf box');
            }
            return;
        }
        walk(node.left!);
        walk(node.right!);
    };
    walk(root);
    assert.equal(seen.size, allTris.length, 'every triangle assigned');
});

test('half-edge: twin/next invariants and smoothing reduces roughness', () => {
    const mesh = noisySphere();
    const he = buildHalfEdgeMesh(mesh.indices, mesh.positions.length / 3);

    for (let e = 0; e < he.halfEdges.length; e++) {
        const edge = he.halfEdges[e];
        assert.notEqual(edge.twin, -1, 'closed mesh: every edge has a twin');
        assert.equal(he.halfEdges[edge.twin].twin, e, 'twin is symmetric');
        const n3 = he.halfEdges[he.halfEdges[edge.next].next].next;
        assert.equal(n3, e, 'next cycles with period 3');
    }

    // One-ring traversal terminates and sums to the half-edge count.
    let ringTotal = 0;
    for (let v = 0; v < he.vertexCount; v++)
        ringTotal += oneRing(he, v).length;
    assert.equal(ringTotal, he.halfEdges.length, 'rings cover each half-edge once');

    const radiusVariance = (positions: number[]): number => {
        const n = positions.length / 3;
        const radii: number[] = [];
        for (let i = 0; i < n; i++)
            radii.push(vertexAt(positions, i).Length());
        const mean = radii.reduce((a, b) => a + b, 0) / n;
        return radii.reduce((a, r) => a + (r - mean) ** 2, 0) / n;
    };
    const smoothed = laplacianSmooth(mesh.positions, he, 0.6, 10);
    assert.equal(smoothed.length, mesh.positions.length, 'vertex count preserved');
    assert.ok(radiusVariance(smoothed) < radiusVariance(mesh.positions) * 0.35, 'roughness reduced');
});

test('raycast: central ray hits the unit icosphere at distance ~2', () => {
    const mesh = buildIcosphere(2);
    const hit = raycastMesh(mesh, new Vector3D(0, 0, 3), new Vector3D(0, 0, -1));
    assert.ok(hit, 'hit found');
    assert.ok(Math.abs(hit.t - 2) < 0.06, `distance ${hit.t} close to 2`);
    const miss = raycastMesh(mesh, new Vector3D(5, 5, 3), new Vector3D(0, 0, -1));
    assert.equal(miss, null, 'offset ray misses');
});

test('poisson disk: pairwise distances respect the radius', () => {
    const r = 0.15;
    const pts = poissonDisk(2, 2, r, makeRng(3));
    assert.ok(pts.length > 50, 'reasonable density');
    for (let i = 0; i < pts.length; i++)
        for (let j = i + 1; j < pts.length; j++)
            assert.ok(pts[i].DistanceSquared(pts[j]) >= r * r - 1e-12, `points ${i},${j} too close`);
});

test('marching squares: contour vertices lie near the iso value', () => {
    const iso = 1.5;
    const segments = marchingSquares(
        metaballs, iso, new Vector2D(-1.6, -1.4), new Vector2D(1.6, 1.4), 128);
    assert.ok(segments.length >= 2, 'contour found');
    for (const p of segments) {
        const value = metaballs(p.X, p.Y);
        assert.ok(Math.abs(value - iso) < 0.25, `field ${value} near iso ${iso}`);
    }
});
