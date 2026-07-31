> **RESOLVED, ARCHIVED 2026-07-16** — associativity fix landed (see plato-roadmap.md Phase 4 note). Historical diagnosis; do not execute.

# Diagnosis: additive-associativity codegen bug (review §8.1)

*2026-07-07. Diagnosis only — no parser/writer behavior has been changed. The proposed fix was
prototyped locally to confirm the mechanism and enumerate the blast radius, then reverted.*

## Symptom

`2.0 * t.Pow3 - 3.0 * t.Sqr + 1.0` (algebra.plato `Hermite`, source correct) is emitted as
`2t³ − (3t² + 1)`; `-x.Twice + 3.0` (`SmoothStep`) is emitted as `(2x + 3).Negative`
(shipped `ara3d-sdk\src\Plato.Generated\_Vector2.g.cs:97`). Five conformance witnesses fail
against shipped output (`Ara3D.SDK.ConformanceTests\KnownFailures.json`).

## Root cause

Two distinct defects, both in the CST→AST conversion in
**`submodules\Plato\Plato.AST\AstNodeFactory.cs`** — not in the C# writer, and not fixable in the
grammar alone.

### Grammar context (why the AST builder must rebalance at all)

`parakeet\Parakeet.Grammars\PlatoGrammar.cs` parses expressions with no precedence or
associativity structure:

```
line 35:  BinaryOperation  => Node(Not("=>") + BinaryOperator + AdvanceOnFail + Expression);
line 80:  InnerExpression  => Node(PrefixOperator.ZeroOrMore() + LeafExpression + PostfixOperator.ZeroOrMore());
```

A binary operation is a *postfix* whose right-hand side is a full `Expression`, so `a - b + c`
parses as `a` followed by one postfix `- (b + c)` — the entire tail right-nests. This is a
deliberate PEG simplification; the design intent is that `AstNodeFactory.CreateBinaryOp`
rebalances the right-recursive parse by precedence when building the AST. It does — but only
partially.

### Defect 1 — equal precedence is not rebalanced (`AstNodeFactory.cs:24`)

```csharp
// AstNodeFactory.cs:18-35 (CreateBinaryOp)
var precedence = Operators.BinaryOperatorPrecedence(op);
if (right is AstBinaryOp binRight && binRight.Precedence < precedence)   // line 24: '<' should be '<='
    return CreateBinaryOp(binRight.Location, binRight.Op,
        CreateBinaryOp(location, op, left, binRight.Left), binRight.Right);
```

The rebalance fires only when the right subtree binds *strictly looser* than the current
operator. `+` and `-` have equal precedence (9, `Operators.cs:67-69`), so
`a - (b + c)` from the right-recursive parse is left exactly as parsed: **all same-precedence
chains associate right**. For pure `+` chains (or pure `*` chains) that is value-preserving, so
the bug is invisible; the moment a chain mixes `-` (or `/`) with its equal-precedence partner,
the emitted math is wrong: `a − b + c` becomes `a − (b + c)`.

Trace for Hermite's coefficient `2.0 * t.Pow3 - 3.0 * t.Sqr + 1.0`:
the parse hands `CreateBinaryOp("*", 2.0, Sub(t³, Mul(3.0, Add(t², 1.0))))` etc.; the `*` vs `-`
(10 > 9) and `*` vs `+` steps rebalance correctly, but the final
`CreateBinaryOp("-", 2t³, Add(3t², 1))` compares 9 < 9 = false and keeps `2t³ − (3t² + 1)`.

### Defect 2 — prefix operators are applied after the binary fold (`AstNodeFactory.cs:118-122`)

```csharp
// AstNodeFactory.cs, end of ToAst(CstExpression) — runs AFTER the postfix loop
foreach (var prefix in expr.InnerExpression.Node.PrefixOperator.Nodes)
    r = ToIntrinsicInvocation(prefix, Operators.UnaryOperatorToName(prefix.Text.Trim()), r);
```

The postfix loop folds member accesses, calls, *and binary operations* into `r`, and only then
are prefix operators wrapped around the result. So in `-x.Twice + 3.0` the `-` is applied to the
whole folded expression `x.Twice + 3.0`, producing `(2x + 3).Negative` instead of
`(−2x) + 3`. Unary minus must bind tighter than any binary operator (it should scope over the
leaf plus its tight postfixes: member access, calls, indexers — not over binary/ternary tails).

## Minimal proposed fix (confirmed by prototype)

Both edits are in `Plato.AST\AstNodeFactory.cs`; grammar and writers untouched.

1. **Line 24:** `binRight.Precedence < precedence` → `binRight.Precedence <= precedence`.
   All Plato binary operators are left-associative (assignment `=` is intercepted before
   `CreateBinaryOp` is called, and there is no `**`/`??`-style right-associative operator), so
   equal precedence must also rebalance. The recursion already re-checks the rebuilt subtree.
2. **Prefix application order:** in `ToAst(CstExpression)`, apply the pending prefix operators to
   `r` immediately *before* folding the first `BinaryOperation` (or `TernaryOperation`) postfix,
   instead of after the loop (tight postfixes — member access, invocation, indexing — still fold
   first, which is correct: `-x.Twice` is `Negative(Twice(x))`).

Prototype verified: with both edits, `Hermite` emits
`this.Multiply(2·t³ − 3·t² + 1)` with correct left grouping, and `SmoothStep` emits
`this.Sqr.Multiply(this.Twice.Negative.Add(3))`. The fix was then reverted.

## Blast radius (measured, not estimated)

Regenerating plato-src with the prototype fix and diffing against checked-in
`ara3d-sdk\src\Plato.Generated` (timestamp lines ignored): **16 files change, 93 members total**.

Files: `_Angle`, `_AnglePair`, `_Bounds2D`, `_Bounds3D`, `_Line2D`, `_Line3D`, `_Number`,
`_NumberInterval`, `_Quad2D`, `_Quad3D`, `_Triangle2D`, `_Triangle3D`, `_Vector2`, `_Vector3`,
`_Vector4`, `_Vector8` (all `.g.cs`).

**Semantically wrong today → behavior changes when fixed (32 members = 8 × Vector2/3/4/8):**

| Member | Shipped (wrong) | Correct |
|---|---|---|
| `FromOne` | `−(x + 1)` | `1 − x` |
| `SmoothStep` | `x²·(−(2x + 3))` | `x²·(3 − 2x)` |
| `Hermite` | `2t³ − (3t² + 1)` groupings | `2t³ − 3t² + 1` |
| `HermiteDerivative` | same class | |
| `CatmullRom` | `2p₀ − (5p₁ + (4p₂ − p₃))` | `2p₀ − 5p₁ + 4p₂ − p₃` |
| `CatmullRomDerivative` | same class | |
| `QuadraticBezierSecondDerivative` | `2(a − (2b + c))` | `2(a − 2b + c)` |
| `CubicBezierSecondDerivative` | `c − (2b + a)` terms | `c − 2b + a` terms |

(`FromOne`, and therefore generic `Lerp` written as `a * t.FromOne + b * t`, is silently wrong on
vector types today — a consequence of Defect 2 that the review had not yet catalogued.)

**Regrouping-only → value-identical (float-rounding differences at most), 61 members:**
pure `+`/`·`/`Or` chains that merely re-associate left: `Pow3/4/5` (Number), `Overlaps`
(AnglePair, Bounds2D/3D, Line2D/3D, NumberInterval), `Center` (Quad2D/3D, Triangle2D/3D),
`Area` (Triangle2D), `Reflect` (Vector4/8), `Quadratic/Cubic/Quartic` and first derivatives,
Bezier evaluators and first derivatives (Vector2/3/4/8), and the Angle curve functions
(`Epicycloid`, `Hypocycloid`, `Epitrochoid`, `Hypotrochoid`, `ButterflyCurveSection` — whose
*source-level* frequency/amplitude bugs, review §1.5, remain and are out of scope here).

## Verification path via the conformance suite

`KnownFailures.json` carries exactly five witnesses attributed to this bug:

- `Witness_SmoothStepAtHalf`, `Witness_SmoothStepAtOne`
- `Witness_HermiteStartsAtP0`, `Witness_HermiteEndsAtP1`
- `Witness_CatmullRomEndsAtP2`

After applying the fix:

1. `tools\regen-conformance.ps1 -Test` — the regenerated law/witness assembly picks up the fixed
   emission; the five witnesses flip to passing. Since the harness treats a passing known-failure
   as an unexpected pass, remove their five entries from `KnownFailures.json` in the same change.
2. `tools\regen-plato.ps1` will report the 16 differing files; run with `-Apply` to recommit
   `Plato.Generated` (this is the deliberate, reviewed regeneration the fix exists to produce —
   the 32 behavior-changing members above are the expected functional delta).
3. Note `Witness_CatmullRomStartsAtP1` already passes (misgrouped terms vanish at t = 0) and must
   keep passing — it guards against overcorrection.

Per review §8.1, this fix should land **before** the §1 source-level bug-fix wave, since until
then "fix the source and regenerate" does not guarantee correct output for any formula mixing
`+`/`−`.
