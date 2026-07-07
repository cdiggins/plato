// Conformance tests for the generated Plato library: the Rust port of
// demos/typescript/geometry-samples/tests/plato.test.ts. The same assertions run against
// the TypeScript code generator — they test the algebra, not the
// implementation.

#![allow(non_snake_case)]

use geometry_samples::plato::*;

fn approx_eps(a: f64, b: f64, eps: f64) {
    assert!((a - b).abs() < eps, "{} ~ {}", a, b);
}

fn approx(a: f64, b: f64) {
    approx_eps(a, b, 1e-12);
}

#[test]
fn fluent_syntax_works_on_native_numbers() {
    assert_eq!((3.0).Add(4.0).Multiply(2.0), 14.0);
    assert_eq!((10.0).Subtract(4.0).Divide(2.0), 3.0);
    assert_eq!((5.0).Negative(), -5.0);
    assert_eq!((2.0).Pow(10.0), 1024.0);
    approx((2.0).Sqrt().Square(), 2.0);
    assert_eq!((-3.5).Abs(), 3.5);
    assert_eq!((7.9).Floor(), 7.0);
    assert_eq!((5.0).Min(3.0), 3.0);
    assert_eq!((5.0).Max(3.0), 5.0);
    assert_eq!((12.0).Clamp(0.0, 10.0), 10.0);
    approx((0.0).Lerp(10.0, 0.25), 2.5);
    assert!((1.0).LessThan(2.0));
    assert!(!(1.0).GreaterThan(2.0));
    assert_eq!((1i64).Compare(2), -1);
    assert!(!true.And(false));
    assert!(true.Or(false));
    assert!(false.Not());
}

#[test]
fn angles_turns_degrees_constructors_and_trig() {
    approx((0.5).Turns().Cos(), -1.0);
    approx((0.25).Turns().Sin(), 1.0);
    approx((180.0).Degrees().Cos(), -1.0);
    approx_eps((1.0).Turns().Sin(), 0.0, 1e-9);
    approx_eps(Constants::Pi(), std::f64::consts::PI, 1e-9);
}

#[test]
fn vector_algebra_identities() {
    let a = Vector3D::new(1.0, 2.0, 3.0);
    let b = Vector3D::new(-2.0, 0.5, 4.0);

    assert!(a.Add(b).Subtract(b).Equals(a), "add/subtract round-trip");
    approx(a.Dot(b), 1.0 * -2.0 + 2.0 * 0.5 + 3.0 * 4.0);
    approx(a.Cross(b).Dot(a), 0.0); // cross product is orthogonal
    approx(a.Cross(b).Dot(b), 0.0);
    approx(a.Normalize().Length(), 1.0);
    approx(a.Scale(2.0).Length(), a.Length() * 2.0);
    approx(a.Distance(b), b.Subtract(a).Length());
    assert!(
        a.Lerp(b, 0.0).Equals(a) && a.Lerp(b, 1.0).Equals(b),
        "lerp endpoints"
    );
    assert!(a.MidPoint(b).Equals(a.Lerp(b, 0.5)), "midpoint = lerp 0.5");
    approx(a.Perpendicular().Dot(a), 0.0);
    approx(a.Perpendicular().Length(), 1.0);

    let v = Vector2D::new(3.0, 4.0);
    approx(v.Length(), 5.0);
    approx(v.Cross(Vector2D::new(-4.0, 3.0)), 25.0); // perpendicular: full magnitude
    approx_eps(
        (0.25).Turns().UnitCircle().Distance(Vector2D::new(0.0, 1.0)),
        0.0,
        1e-9,
    );
}

#[test]
fn reflection_preserves_length_and_reverses_the_normal_component() {
    let v = Vector3D::new(1.0, -2.0, 0.5);
    let n = Vector3D::new(0.0, 1.0, 0.0);
    let r = v.Reflect(n);
    approx(r.Length(), v.Length());
    approx(r.Dot(n), -v.Dot(n));
}

#[test]
fn with_functions_and_default() {
    let v = Vector3D::new(1.0, 2.0, 3.0).WithY(9.0);
    assert!(v.Equals(Vector3D::new(1.0, 9.0, 3.0)));
    assert!(Vector3D::default().Equals(Vector3D::new(0.0, 0.0, 0.0)));
}
