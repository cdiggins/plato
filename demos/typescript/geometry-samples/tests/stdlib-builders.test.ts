// Executable invariants for the stdlib's spatial-structure builders
// (stdlib/geometry/spatial-structures.library.plato BuildBvh / BuildOctree /
// BuildLooseOctree, generated to TypeScript). The same properties are stated
// as laws in stdlib/tests/spatial-structures.laws.plato; until the forward law
// runner unblocks (plato-308), this file and SortingTests.cs are where they
// actually run.

import { test } from 'node:test';
import assert from 'node:assert/strict';

import {
    Bounds3D, BuildBvh, BuildLooseOctree, BuildOctree, Point3D,
} from '../src/plato/plato.g.js';
import { fromArray } from '../src/core/meshBuilder.js';

const rng = (seed: number) => () => {
    seed = (seed * 1664525 + 1013904223) >>> 0;
    return seed / 0x100000000;
};

const randomPoints = (n: number, seed: number): Point3D[] => {
    const r = rng(seed);
    return Array.from({ length: n }, () => new Point3D(r() * 20 - 10, r() * 20 - 10, r() * 20 - 10));
};

const randomBounds = (n: number, seed: number): Bounds3D[] =>
    randomPoints(n, seed).map(p =>
        new Bounds3D(p, new Point3D(p.X + 0.5, p.Y + 0.5, p.Z + 0.5)));

/** Every index in [0, n) appears exactly once. */
const assertPermutation = (perm: number[], n: number, label: string) => {
    assert.equal(perm.length, n, `${label}: permutation length`);
    assert.deepEqual([...perm].sort((a, b) => a - b), Array.from({ length: n }, (_, i) => i),
        `${label}: permutation completeness`);
};

type Range = { Start: number; End: number };
/** Leaf ranges tile [0, n); internal ranges are empty. */
const assertLeavesPartition = (
    nodes: { isLeaf: boolean; range: Range }[], n: number, label: string) => {
    const covered = new Array<number>(n).fill(0);
    for (const node of nodes) {
        if (!node.isLeaf) {
            assert.equal(node.range.Start, node.range.End, `${label}: internal range empty`);
            continue;
        }
        for (let i = node.range.Start; i < node.range.End; i++) covered[i]++;
    }
    for (let i = 0; i < n; i++)
        assert.equal(covered[i], 1, `${label}: position ${i} in exactly one leaf`);
};

test('stdlib BuildBvh: permutation, leaf partition, full binary tree', () => {
    for (const n of [0, 1, 2, 3, 5, 17, 200]) {
        const tree = BuildBvh(fromArray(randomBounds(n, n + 3)), 4);
        const perm: number[] = [];
        for (let i = 0; i < tree.PrimitiveIndices.Count(); i++)
            perm.push(tree.PrimitiveIndices.At(i).Value);
        assertPermutation(perm, n, `bvh n=${n}`);

        const nodes: { isLeaf: boolean; range: Range }[] = [];
        let leaves = 0;
        for (let i = 0; i < tree.Nodes.Count(); i++) {
            const node = tree.Nodes.At(i);
            const isLeaf = node.LeftChild.Value < 0 && node.RightChild.Value < 0;
            if (isLeaf) leaves++;
            nodes.push({ isLeaf, range: node.Primitives });
        }
        assertLeavesPartition(nodes, n, `bvh n=${n}`);
        assert.equal(tree.Nodes.Count(), n === 0 ? 0 : 2 * leaves - 1, `bvh n=${n}: full binary tree`);
    }
});

test('stdlib BuildOctree: permutation, leaf partition, eight consecutive children', () => {
    for (const n of [0, 1, 2, 9, 100, 500]) {
        const tree = BuildOctree(fromArray(randomPoints(n, n + 11)), 8, 4);
        const perm: number[] = [];
        for (let i = 0; i < tree.ItemIndices.Count(); i++)
            perm.push(tree.ItemIndices.At(i).Value);
        assertPermutation(perm, n, `octree n=${n}`);

        const nodes: { isLeaf: boolean; range: Range }[] = [];
        for (let i = 0; i < tree.Nodes.Count(); i++) {
            const node = tree.Nodes.At(i);
            const isLeaf = node.FirstChild.Value < 0;
            if (!isLeaf)
                assert.ok(node.FirstChild.Value > 0 && node.FirstChild.Value + 8 <= tree.Nodes.Count(),
                    `octree n=${n}: children present`);
            nodes.push({ isLeaf, range: node.Items });
        }
        assertLeavesPartition(nodes, n, `octree n=${n}`);
    }
});

test('stdlib BuildOctree: coincident points terminate at the depth cap', () => {
    const same = Array.from({ length: 40 }, () => new Point3D(1, 2, 3));
    const tree = BuildOctree(fromArray(same), 3, 4);
    const perm: number[] = [];
    for (let i = 0; i < tree.ItemIndices.Count(); i++)
        perm.push(tree.ItemIndices.At(i).Value);
    assertPermutation(perm, 40, 'coincident octree');
});

test('stdlib BuildLooseOctree: loosened bounds contain the exact ones', () => {
    const points = randomPoints(120, 7);
    const exact = BuildOctree(fromArray(points), 6, 4);
    const loose = BuildLooseOctree(fromArray(points), 6, 4, 2);
    assert.equal(loose.Octree.Nodes.Count(), exact.Nodes.Count());
    for (let i = 0; i < exact.Nodes.Count(); i++) {
        const e = exact.Nodes.At(i).Bounds;
        const l = loose.Octree.Nodes.At(i).Bounds;
        assert.ok(l.Min.X <= e.Min.X + 1e-9 && l.Max.X >= e.Max.X - 1e-9, 'X span grows');
        assert.ok(l.Min.Y <= e.Min.Y + 1e-9 && l.Max.Y >= e.Max.Y - 1e-9, 'Y span grows');
        assert.ok(l.Min.Z <= e.Min.Z + 1e-9 && l.Max.Z >= e.Max.Z - 1e-9, 'Z span grows');
    }
});
