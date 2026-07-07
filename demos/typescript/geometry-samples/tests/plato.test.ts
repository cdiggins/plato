// Conformance tests for the generated Plato library. The same assertions can
// later run against the other code generators (C#, Rust, Python, ...): they
// test the algebra, not the implementation.

import { test } from 'node:test';
import assert from 'node:assert/strict';

import { Vector2D, Vector3D, Constants } from '../src/plato/plato.g.js';

const approx = (a: number, b: number, eps = 1e-12) =>
    assert.ok(Math.abs(a - b) < eps, `${a} ~ ${b}`);

test('fluent syntax works on native numbers', () => {
    assert.equal((3).Add(4).Multiply(2), 14);
    assert.equal((10).Subtract(4).Divide(2), 3);
    assert.equal((5).Negative(), -5);
    assert.equal((2).Pow(10), 1024);
    approx((2).Sqrt().Square(), 2);
    assert.equal((-3.5).Abs(), 3.5);
    assert.equal((7.9).Floor(), 7);
    assert.equal((5).Min(3), 3);
    assert.equal((5).Max(3), 5);
    assert.equal((12).Clamp(0, 10), 10);
    approx((0).Lerp(10, 0.25), 2.5);
    assert.equal((1).LessThan(2), true);
    assert.equal((1).GreaterThan(2), false);
    assert.equal((true).And(false), false);
    assert.equal((true).Or(false), true);
    assert.equal((false).Not(), true);
});

test('angles: Turns/Degrees constructors and trig', () => {
    approx((0.5).Turns().Cos(), -1);
    approx((0.25).Turns().Sin(), 1);
    approx((180).Degrees().Cos(), -1);
    approx((1).Turns().Sin(), 0, 1e-9);
    approx(Constants.Pi, Math.PI, 1e-9);
});

test('vector algebra identities', () => {
    const a = new Vector3D(1, 2, 3);
    const b = new Vector3D(-2, 0.5, 4);

    assert.ok(a.Add(b).Subtract(b).Equals(a), 'add/subtract round-trip');
    approx(a.Dot(b), 1 * -2 + 2 * 0.5 + 3 * 4);
    approx(a.Cross(b).Dot(a), 0, 1e-12); // cross product is orthogonal
    approx(a.Cross(b).Dot(b), 0, 1e-12);
    approx(a.Normalize().Length(), 1);
    approx(a.Scale(2).Length(), a.Length() * 2);
    approx(a.Distance(b), b.Subtract(a).Length());
    assert.ok(a.Lerp(b, 0).Equals(a) && a.Lerp(b, 1).Equals(b), 'lerp endpoints');
    assert.ok(a.MidPoint(b).Equals(a.Lerp(b, 0.5)), 'midpoint = lerp 0.5');
    approx(a.Perpendicular().Dot(a), 0, 1e-12);
    approx(a.Perpendicular().Length(), 1);

    const v = new Vector2D(3, 4);
    approx(v.Length(), 5);
    approx(v.Cross(new Vector2D(-4, 3)), 25); // perpendicular: full magnitude
    approx((0.25).Turns().UnitCircle().Distance(new Vector2D(0, 1)), 0, 1e-9);
});

test('reflection preserves length and reverses the normal component', () => {
    const v = new Vector3D(1, -2, 0.5);
    const n = new Vector3D(0, 1, 0);
    const r = v.Reflect(n);
    approx(r.Length(), v.Length());
    approx(r.Dot(n), -v.Dot(n));
});

test('With functions and Default', () => {
    const v = new Vector3D(1, 2, 3).WithY(9);
    assert.ok(v.Equals(new Vector3D(1, 9, 3)));
    assert.ok(Vector3D.Default.Equals(new Vector3D(0, 0, 0)));
});
