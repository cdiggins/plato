// Round-trip tests for the Three.js adapter layer. Three.js itself runs fine
// in Node, so the conversions are testable without a browser.

import { test } from 'node:test';
import assert from 'node:assert/strict';

import {
    bufferGeometryToMeshData,
    meshDataToBufferGeometry,
    threeVectorToVector3D,
    vector3DToThree,
} from '../src/adapters/three.js';
import { buildIcosphere } from '../src/samples/icosphere.js';
import { Vector3D } from '../src/plato/plato.g.js';

test('mesh -> BufferGeometry -> mesh round-trips positions and indices', () => {
    const mesh = buildIcosphere(2);
    const geometry = meshDataToBufferGeometry(mesh);
    const back = bufferGeometryToMeshData(geometry);

    assert.equal(back.positions.length, mesh.positions.length);
    for (let i = 0; i < mesh.positions.length; i++)
        assert.ok(Math.abs(back.positions[i] - mesh.positions[i]) < 1e-6, `position ${i}`);

    assert.deepEqual(back.indices, mesh.indices);
    assert.ok(back.normals && back.normals.length === mesh.positions.length, 'normals present');
});

test('non-indexed BufferGeometry gets a trivial index', () => {
    const mesh = buildIcosphere(0);
    const geometry = meshDataToBufferGeometry(mesh).toNonIndexed();
    const back = bufferGeometryToMeshData(geometry);
    assert.equal(back.indices.length, back.positions.length / 3);
    assert.deepEqual(back.indices.slice(0, 3), [0, 1, 2]);
});

test('Vector3D conversions round-trip', () => {
    const v = new Vector3D(1.5, -2.25, 3.75);
    const roundTripped = threeVectorToVector3D(vector3DToThree(v));
    assert.ok(roundTripped.Equals(v));
});
