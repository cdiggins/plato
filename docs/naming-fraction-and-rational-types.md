# Naming: Fraction, Rational, and NonZeroInteger

Design notes from a naming discussion (July 2026) about Plato types for
unit-interval parameters, exact rational numbers, and constrained integers.

## Context

Plato already uses `Number` for general floating-point values and `t:Number` in
`IInterpolatable.Lerp`. Several APIs need a clearer name for values that are
*usually* in the range [0, 1] — interpolation weights, blend factors, animation
progress — without implying they are always clamped (extrapolation with `t > 1`
or `t < 0` is valid for lerp).

Separately, some types represent exact rationals as an explicit integer
numerator and denominator (backed by `BigInteger`).

## Recommended names

| Concept | Recommended name | Notes |
|---------|------------------|-------|
| Unit-interval / lerp parameter | **`Fraction`** | Usually [0, 1]; not strictly bounded |
| Exact p/q rational number | **`Rational`** | Or **`BigRational`** if arbitrary precision is a first-class selling point |
| Integer guaranteed ≠ 0 | **`NonZeroInteger`** | Only if Plato has validated construction for refined types |

## Fraction — the lerp / blend parameter

**Recommendation: `Fraction`**

`Lerp(v1, v2, Fraction t)` reads naturally: "what fraction of the way from v1
to v2?"

### Why not `Amount`?

`Amount` is already used in Plato for scale factors (`UniformScale2D.Amount`,
`Scale2D.Amount`, `Scaling3D.Amount`), where values like `2.0` or `0.5` are
equally sensible. It carries no connotation of being bounded to [0, 1] and is
too generic for a semantic type.

### Alternatives considered

| Name | Pros | Cons |
|------|------|------|
| `Fraction` | Math and everyday language align; Android/Compose use `fraction` for lerp | Pedants note fractions can exceed 1 (e.g. 3/2) — acceptable for extrapolation |
| `Proportion` | Unambiguous [0, 1] in statistics | Rare as a type name in code |
| `Unit` / `UnitValue` | Precise math term (unit interval) | Collides with unit vectors and units of measure (`Angle`, `Time`, …) |
| `Progress` | Excellent in animation/tweening | Domain-specific; weak for general lerp |
| `Percent` | Familiar to non-coders | Dangerous: is `0.5` fifty percent or half a percent? |
| `Weight` / `Alpha` | Known in graphics | Vague; `Alpha` collides with opacity / `Color.A` |

### Integration with `IInterpolatable`

Current signature:

```plato
concept IInterpolatable { Lerp(a:Self, b:Self, t:Number):Self; }
```

If `Fraction` is introduced as a semantic type (likely implementing `IMeasure`
like `Angle` and `Time`), changing `t:Number` to `t:Fraction` would ripple
through every implementer. The benefit is self-documenting APIs at the primary
call site.

## Rational — exact numerator / denominator

**Recommendation: `Rational`** (or `BigRational` if the BigInteger backing
should be visible in the name).

Do **not** use `RationalFraction` — redundant; a rational number already *is*
the value p/q.

### Precedent

- Haskell: `Rational` (`Ratio Integer`)
- Ruby: `Rational`
- .NET ecosystem: `BigRational` (community / experimental numerics)
- Python's `fractions.Fraction` is the notable outlier and is exactly the name
  collision to avoid with Plato's `Fraction` type.

### Relationship to `Fraction`

With this split, the two names mean different things and do not overlap:

- **`Rational`** — an exact number (two integers, p/q).
- **`Fraction`** — how far along a blend or interpolation, from none to all.

## NonZeroInteger — constrained denominator?

**Name: `NonZeroInteger`** if the type is added. Rust's `NonZeroI32` /
`NonZeroU64` / `NonZero<T>` are the strongest precedent.

### When it is worth having

A refinement type makes invalid rationals unrepresentable at the type level:

```plato
type Rational { Numerator: Integer; Denominator: NonZeroInteger; }
```

Division APIs can also take `NonZeroInteger` and never need a zero check at
runtime.

### When to skip it

The type is only as good as its construction story. Somewhere, a plain
`Integer` must become `NonZeroInteger`, and the zero check moves to that
boundary rather than disappearing.

If `Rational`'s constructor already normalizes (reduce to lowest terms, keep
denominator positive) and rejects zero, that constructor is the natural
chokepoint — a separate `NonZeroInteger` may add friction without adding safety.

Also, if `NonZeroInteger` implemented `IWholeNumber`, it would inherit `Add`
and `Subtract`, under which non-zero is not closed (`3 + (-3) = 0`). It is
closed under multiplication only, so it fits awkwardly in the arithmetic
concept hierarchy unless constrained to a narrow API surface.

**Recommendation:** introduce `NonZeroInteger` only when Plato has (or plans) a
general mechanism for refined types with validated construction. Otherwise,
enforce the invariant in `Rational`'s constructor.

## Summary

- Use **`Fraction`** for the [0, 1]-ish lerp/blend parameter; avoid **`Amount`**
  for this role.
- Use **`Rational`** (or **`BigRational`**) for exact p/q values; avoid
  **`RationalFraction`**.
- Use **`NonZeroInteger`** only when refined-type construction is a first-class
  language feature; otherwise validate in `Rational`'s constructor.
