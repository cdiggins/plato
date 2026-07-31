---
lesson: complex-numbers-rotate
title: Complex Numbers Rotate
domain: Foundations & vectors
v3-files: [05-numbers.plato, 10-rotations.plato]
audience: High-school trigonometry and general programming background
status: draft-v1
---

# Complex Numbers Rotate

Spin a stick in the plane and every tip position is some length times
$(\cos \theta, \sin \theta)$. That pair of numbers is also the real and imaginary
parts of a complex number. Multiplying by another complex number on the unit
circle turns out to be exactly a rotation — no matrix required, no special
"rotate" opcode, just the algebra of $\mathbb{C}$.

This is why 2D graphics, geometry, and geometric algebra keep rediscovering the
same two numbers under different names. Plato's vocabulary has all three
faces: `Complex`, `Rotation2D`, and `Rotor2D`.

## The idea

Write a plane vector $(x, y)$ as the complex number $z = x + yi$. A second
complex number $w = a + bi$ multiplies as

$$
zw = (ax - by) + (ay + bx)i.
$$

If $w$ has magnitude 1, then $a = \cos\theta$ and $b = \sin\theta$ for some
angle $\theta$, and the product becomes

$$
zw = (x\cos\theta - y\sin\theta) + (x\sin\theta + y\cos\theta)i,
$$

which is the familiar counter-clockwise rotation of $(x, y)$ by $\theta$.
Magnitudes multiply and arguments (angles) add:

$$
|zw| = |z|\,|w|, \qquad \arg(zw) = \arg(z) + \arg(w).
$$

So a unit complex number is a pure rotation; a non-unit one rotates and scales
in one multiply.

```
        y
        ^
        |     *  z rotated by θ
        |    /
        |   / θ
        |  /
        | /____*  z = x + yi
        +------------> x
```

Two unit complex numbers commute under multiplication — plane rotations
commute. That stops being true in 3D, which is why the story upgrades to
quaternions and rotors there. In 2D, complex multiplication is enough.

Half-angles appear when you want a geometric-algebra rotor: the rotor that
represents a rotation by $\theta$ stores $\cos(\theta/2)$ and
$\sin(\theta/2)$. Applying the rotor uses a sandwich product that doubles the
half-angle back to $\theta$. The stored pair still looks like a unit complex
number — Plato names that pair `Rotor2D`.

## In Plato

From `05-numbers.plato`, a complex value is just the two parts:

```plato
// A complex number with real and imaginary parts.
type Complex
    implements Numerical
{
    Real: Number;
    Imaginary: Number;
}
```

From `10-rotations.plato`, the angle form and the rotor form:

```plato
// A rotation in the plane, stored as a single angle.
type Rotation2D
    implements Value, Multiplicative, Interpolatable
{
    Angle: Angle;
}

// A 2D geometric-algebra rotor: scalar plus bivector part. Equivalent to a
// unit complex number.
type Rotor2D
    implements Value, Multiplicative
{
    Scalar: Number;
    XY: Number;
}
```

`Rotation2D` is the authoring form: one `Angle` (always radians under the
hood — never a raw `Number`). `Rotor2D` is the algebraic form: `Scalar` plays
the role of the real part, `XY` the imaginary / bivector part. The doc comment
states the equivalence to a unit complex number explicitly.

The `Transforms` library wires the bridge and the multiply:

```plato
// Composition of two plane rotations: angles add (2D rotations commute).
Multiply(a: Rotation2D, b: Rotation2D): Rotation2D
    => Rotation2D(a.Angle + b.Angle);

// The rotor of a plane rotation: R = cos(a/2) + sin(a/2) e12.
Rotor2D(r: Rotation2D): Rotor2D {
    var h = r.Angle * 0.5;
    return (h.Cos, h.Sin);
}

// The geometric product of two rotors (complex multiplication).
Multiply(a: Rotor2D, b: Rotor2D): Rotor2D
    => (a.Scalar * b.Scalar - a.XY * b.XY,
        a.Scalar * b.XY + a.XY * b.Scalar);

// Rotate a displacement counter-clockwise by the rotation's angle.
Transform(v: Vector2D, r: Rotation2D): Vector2D { ... }
```

Usage-shaped snippets:

```plato
let quarter = Rotation2D { Angle: Angle { Radians: 1.5707963 } };
let tip = Vector2D { X: 1.0, Y: 0.0 };

// Angle form: rotate the tip 90° CCW → (0, 1)
let spun = tip.Transform(quarter);

// Same rotation as a rotor (half-angle encoding)
let rotor = quarter.Rotor2D;
let spun2 = tip.Transform(rotor);

// Compose two 45° turns into 90°
let eighth = Rotation2D { Angle: Angle { Radians: 0.78539816 } };
let composed = eighth.Multiply(eighth);

// Rotor multiply is complex multiply on (Scalar, XY)
let rA = eighth.Rotor2D;
let rB = eighth.Rotor2D;
let rAB = rA.Multiply(rB);
```

`Complex` itself implements `Numerical` (add, scale, lerp) but not
`Multiplicative`. The rotation story in Plato therefore lives on
`Rotation2D` / `Rotor2D`, not on `Complex.Multiply` — that gap is real and
called out below.

Identities and inverses:

```plato
let id = Rotation2D.Identity;          // zero angle
let undo = quarter.Inverse;            // negate the angle
let undoR = rotor.Inverse;             // (Scalar, -XY) — conjugate
```

## Pitfalls / fine print

**Half-angle vs full angle.** `Rotation2D.Angle` is the full turn. `Rotor2D`
stores the half-angle trig pair. Mixing them without converting (`Rotor2D(r)` /
`Rotation2D(r)`) doubles or halves the rotation silently in your head even if
the types refuse to mix at compile time.

**Unit length.** A rotor (or unit complex) must stay normalized. Repeated
`Multiply` drifts off the unit circle in floating point; call `Normalize` on
`Rotor2D` after long product chains. `Rotation2D` sidesteps this by storing an
angle instead of a pair of floats.

**Clockwise vs counter-clockwise.** Plato's `Transform` for `Rotation2D` is
counter-clockwise: $(x\cos - y\sin,\; x\sin + y\cos)$. Screen coordinates with
$y$ down reverse the visual sense of "positive angle" without changing the math.

**Complex is not yet a rotator in the type system.** You can *think* of
`Complex` as $x + yi$, but v3 does not declare `Multiply(Complex, Complex)` or
conversions to `Rotor2D`. Do not invent those names in code; use `Rotor2D` or
`Rotation2D`.

**Interpolation.** `Rotation2D.Lerp` interpolates stored angles, not the
shortest arc. Crossing the $\pm\pi$ branch cut takes the long way. Rotors and
complex numbers on the circle want a spherical / angular lerp for animation.

**Scaling by accident.** Multiplying by a non-unit complex (or an unnormalized
rotor before normalize) rotates *and* scales. If you only wanted a turn, check
magnitude.

## Try it

1. Let $z = 1 + 0i$ and $w = 0 + 1i$. What is $zw$? What rotation of the
   positive X axis does $w$ represent?
2. A `Rotation2D` of $\pi/2$ becomes which `Rotor2D` `(Scalar, XY)` pair?
3. Why does `eighth.Multiply(eighth)` equal a quarter turn, and why do plane
   rotations commute?

<details>
<summary>Answers</summary>

1. $zw = i$, so $(0, 1)$. Multiplying by $i$ is a $+90^\circ$ (counter-clockwise)
   rotation.
2. Half angle is $\pi/4$: $(\cos(\pi/4),\;\sin(\pi/4)) = (\sqrt{2}/2,\;\sqrt{2}/2)$.
3. Angles add under `Multiply` for `Rotation2D`. Addition of scalars commutes,
   so composition of plane rotations commutes — unlike 3D rotations.

</details>

## Library recommendations

- **missing-function** — `05-numbers.plato`: `Complex` implements `Numerical`
  but not `Multiplicative`, so there is no declared `Multiply(Complex, Complex)`.
  Teaching complex-as-rotation needs that product (or an explicit
  `Rotate(Vector2D, Complex)`). Without it, authors must leave `Complex` and
  switch to `Rotor2D` mid-explanation.

- **missing-function** — `05-numbers.plato` / `10-rotations.plato`: no
  conversions `Rotor2D(c: Complex)` / `Complex(r: Rotor2D)` even though the
  `Rotor2D` doc comment says the types are equivalent. A total conversion on
  the unit circle (and a documented precondition for non-unit `Complex`) would
  close the teaching bridge.

- **missing-concept** — `05-numbers.plato`: `Complex` does not implement
  `Normed` or expose `Argument: Angle`. Magnitude and argument are the two
  polar coordinates of a complex number; without them, the
  "magnitudes multiply, angles add" slogan cannot be typed against `Complex`.

- **pedagogy** — `10-rotations.plato`: `Rotor2D` fields are `Scalar` and `XY`,
  while `Complex` uses `Real` and `Imaginary`. Parallel field names (or a doc
  comment table mapping Real↔Scalar, Imaginary↔XY) would make the isomorphism
  obvious without a prose lecture.
