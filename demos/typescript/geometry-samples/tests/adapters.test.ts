// Round-trip tests for the Three.js adapter layer. Three.js itself runs fine
// in Node, so the conversions are testable without a browser.

import { test } from 'node:test';
import assert from 'node:assert/strict';

import {
    bufferGeometryToMesh,
    meshDataToBufferGeometry,
    point3DToThree,
    threeVectorToVector3D,
    vector3DToThree,
} from '../src/adapters/three.js';
import { buildIcosphere } from '../src/samples/icosphere.js';
import { meshIndices, meshVertices } from '../src/core/meshBuilder.js';
import { Point3D, Vector3D } from '../src/plato/plato.g.js';

test('mesh -> BufferGeometry -> mesh round-trips positions and indices', () => {
    const mesh = buildIcosphere(2);
    const geometry = meshDataToBufferGeometry({ kind: 'mesh', mesh });
    const back = bufferGeometryToMesh(geometry);

    const before = meshVertices(mesh);
    const after = meshVertices(back);
    assert.equal(after.length, before.length);
    for (let i = 0; i < before.length; i++)
        assert.ok(before[i].Distance(after[i]) < 1e-6, `position ${i}`);

    assert.deepEqual(meshIndices(back), meshIndices(mesh));

    // Smooth shading takes its normals from the stdlib, one per vertex.
    const normals = geometry.getAttribute('normal');
    assert.equal(normals.count, before.length, 'one normal per vertex');
});

test('non-indexed BufferGeometry gets a trivial index', () => {
    const mesh = buildIcosphere(0);
    const geometry = meshDataToBufferGeometry({ kind: 'mesh', mesh }).toNonIndexed();
    const back = bufferGeometryToMesh(geometry);
    assert.equal(meshIndices(back).length, meshVertices(back).length);
    assert.deepEqual(meshIndices(back).slice(0, 3), [0, 1, 2]);
});

test('flat shading asks Three.js for face normals instead', () => {
    const mesh = buildIcosphere(1);
    const geometry = meshDataToBufferGeometry({ kind: 'mesh', mesh, flatShading: true });
    assert.ok(geometry.getAttribute('normal'), 'normals computed');
});

test('vector and point conversions round-trip', () => {
    const v = new Vector3D(1.5, -2.25, 3.75);
    assert.ok(threeVectorToVector3D(vector3DToThree(v)).Equals(v));

    const p = new Point3D(-0.5, 4, 2.125);
    const back = point3DToThree(p);
    assert.deepEqual([back.x, back.y, back.z], [p.X, p.Y, p.Z]);
});
