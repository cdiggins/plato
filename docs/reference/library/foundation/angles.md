# Angles and periodic values

An angle is an `Angle`, stored in radians. Read [angle operations](../../api/foundation/angles.library.md), its [quantity declaration](../../api/foundation/quantities.types.md), and the [canonical convention](../../../../stdlib/CONVENTIONS.md#angles--angle-typed-radians-canonical).

## Construct in the unit you know

Plato examples such as `90.Degrees` and `0.25.Turns` construct the same quarter turn. The unit methods return an `Angle`; the matching accessor reads a numeric unit count. `Angle(x)` is the raw radians construction. Degree, turn, gradian, arc-minute and arc-second constructors make call-site units visible. Do not pass a bare number of degrees to a radians-based formula.

Accessors do not normalize: two turns remain two turns when read back. This matters for accumulated rotations and animation, where collapsing a complete revolution to zero would lose history.

## Equality on the circle differs from equality of amounts

`Normalize` selects the representative in `(-pi,pi]`. Both -180 degrees and +180 degrees select +180 degrees. `Wrap(a,period)` selects `[0,period)` for a meaningful positive period. The left and right endpoints have intentionally different membership.

`LerpShortest` uses the normalized signed delta, so interpolating from 350 degrees to 10 degrees crosses zero by the short arc. Ordinary interpolation of their raw amounts would pass through 180 degrees. An exact half-turn difference has two equally short geometric paths; the canonical sign chooses the representative, rather than discovering a unique physical rotation.

`EquivalentAngle` compares normalized radians with exact equality; it does not accept a numerical tolerance. Floating-point normalization can accumulate error, so mathematical periodicity is not a promise of bitwise identity after arbitrary arithmetic, especially near a wrapping boundary.

## Trigonometric domains

`Sin` and `Cos` consume angles and return unitless values. Inverse trigonometric operations select branches; read their return type and range. Reciprocal functions have poles: `Csc` divides by sine, `Sec` by cosine, and `Cot` by tangent. A value close to a pole can be numerically unstable even if it is not exactly singular.

## Related work

[DLMF's elementary-function chapters](https://dlmf.nist.gov/) describe trigonometric identities and branch conventions mathematically. Plato adds a typed angle boundary and a chosen periodic representative; familiar identities alone do not settle API units, endpoints or floating-point error.
