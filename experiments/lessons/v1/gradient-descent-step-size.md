---
lesson: gradient-descent-step-size
title: Gradient Descent and Step Size
domain: Math, statistics & signals
v3-files: [62-optimization.plato]
audience: Calculus basics (derivative as slope) and general programming background
status: draft-v1
---

# Gradient Descent and Step Size

You want the lowest point of a smooth cost function — fitting a curve, resolving a
closest-point query, training a tiny model. **Gradient descent** walks downhill:
look at the slope, take a step opposite that slope, repeat. The entire art that
separates "converges nicely" from "explodes or crawls" is the **step size**, often
called the learning rate.

Too large and you leap over valleys and diverge. Too small and you burn your
iteration budget inching forward. Everything else in first-order optimization is a
refinement of that tension.

## The idea

For a scalar objective $f(\mathbf{x})$ with gradient $\nabla f$, the basic update is

$$
\mathbf{x}_{k+1} = \mathbf{x}_k - \alpha\, \nabla f(\mathbf{x}_k)
$$

where $\alpha > 0$ is the step size. In one dimension:

$$
x_{k+1} = x_k - \alpha\, f'(x_k)
$$

```
 f
  |     *
  |    / \
  |   /   \*
  |  /      \
  | /        \*
  |/__________\____> x
         --> steps opposite the slope
```

**Why the minus sign?** The gradient points to steepest *ascent*. Minimization walks
the other way. Maximization flips the sign (or negates $f$).

**Fixed step.** $\alpha$ constant. Simple. Works when $f$ is well-scaled and $\alpha$
is below ~$2/L$ for $L$-smooth functions (Lipschitz gradient). In practice you tune.

**Decaying step.** Multiply $\alpha$ by a factor $\rho \in (0,1]$ each iteration so
early steps explore and later steps settle.

**Momentum.** Blend in the previous step direction so the walk builds speed in
consistent valleys and dampens zig-zag in narrow ravines:

$$
\mathbf{v}_{k+1} = \mu\, \mathbf{v}_k + \nabla f(\mathbf{x}_k),
\quad
\mathbf{x}_{k+1} = \mathbf{x}_k - \alpha\, \mathbf{v}_{k+1}
$$

(with $\mu \in [0,1)$). Variants differ on whether gradient is evaluated before or
after the velocity update; the qualitative role of $\mu$ is the same.

**Line search.** Instead of committing to a global $\alpha$, search along the ray
$\mathbf{x} - t\,\mathbf{d}$ for a $t$ that sufficiently decreases $f$ (backtracking,
Wolfe conditions, golden section, or an "exact" 1D minimize). Step size becomes a
*policy*, not a single scalar.

Stopping usually watches parameter change, objective change, or budgets on
iterations and evaluations.

## In Plato

`62-optimization.plato` stores solver *parameters* and *results* as pure records —
no solver bodies yet. Gradient descent tuning is one record; line-search strategy is
a sum type used by other methods.

```plato
type OptimizationGoal = Minimize | Maximize;

type ConvergenceCriteria
{
    MaxIterations: Integer;
    AbsoluteTolerance: Number;
    RelativeTolerance: Number;
    MaxEvaluations: Integer;   // -1 = no limit
}

type TerminationReason = Converged | MaxIterations | Stalled | Failed | NumericalError;

type OptimizationResult
{
    Parameters: Array<Number>;
    ObjectiveValue: Number;
    Iterations: Integer;
    Converged: Boolean;
    Reason: TerminationReason;
}

type GradientDescentParameters
{
    LearningRate: Number;   // α
    Momentum: Number;       // μ in [0, 1)
    Decay: Number;          // multiply α each iteration; 1 = constant
}

type LineSearch = FixedStep | Backtracking | WolfeConditions | GoldenSection | Exact;
```

Usage-shaped setup (illustrative):

```plato
let gd = GradientDescentParameters(
    0.05,    // LearningRate
    0.9,     // Momentum
    0.99);   // Decay — mild shrink each step

let stop = ConvergenceCriteria(
    1000,    // MaxIterations
    1e-8,    // AbsoluteTolerance
    1e-8,    // RelativeTolerance
    -1);     // MaxEvaluations unlimited

// After a solve (hypothetical library call):
// result.Parameters, result.ObjectiveValue, result.Reason
```

Note the asymmetry: `LbfgsParameters` *carries* a `LineSearch` field, while
`GradientDescentParameters` encodes step policy only via `LearningRate` + `Decay` +
`Momentum` — effectively a fixed (possibly decaying) step with optional momentum,
not a Wolfe line search.

```plato
type LbfgsParameters
{
    MemorySize: Integer;
    LineSearch: LineSearch;
}
```

Goal and constraints live nearby for larger problems (`LinearProgram`,
`QuadraticProgram`) but the step-size lesson is local to first-order updates on an
unconstrained parameter vector `Array<Number>`.

## Pitfalls / fine print

**Scale of parameters.** If one coordinate is in meters and another in millimeters,
the same $\alpha$ is huge on one axis and tiny on the other. Normalize variables, or
use adaptive per-coordinate rates (not declared on `GradientDescentParameters`).

**LearningRate of zero.** A zero rate never moves; a negative rate ascends. The type
does not enforce $\alpha > 0$.

**Momentum in $[0,1)$.** Doc comment requires $\mu \in [0,1)$. $\mu = 1$ retains
velocity forever and usually diverges; $\mu < 0$ is undefined in the comment.

**Decay semantics.** `Decay` multiplies the learning rate *after every iteration*.
`Decay = 1` keeps $\alpha$ constant. `Decay = 0` would kill the step after one
iteration — legal as a Number, catastrophic as a schedule.

**Maximize vs minimize.** `OptimizationGoal` exists, but `GradientDescentParameters`
does not carry the goal; the solver wrapper must apply the sign. Forgetting to flip
for maximization walks to a maximum of the wrong sense of "error."

**Stalled vs Converged.** Tiny steps from an undersized $\alpha$ can look like
convergence when the true gradient is still large. Prefer checking gradient norm
(not declared on `OptimizationResult`) in addition to parameter delta.

**No gradient in the result.** `OptimizationResult` stores parameters and objective,
not the final gradient. Debugging step-size issues often needs that residual slope.

## Try it

1. $f(x) = x^2$, $x_0 = 3$, $\alpha = 0.1$, no momentum. What is $x_1$?
2. Same start, $\alpha = 2$. What goes wrong qualitatively?
3. If `Decay = 0.5` and `LearningRate = 0.8`, what rate is used on the third update
   (after two decay multiplications)?

<details>
<summary>Answers</summary>

1. $f'(x) = 2x$, so $x_1 = 3 - 0.1\cdot 6 = 2.4$.
2. $\alpha = 2$ overshoots: $x_1 = 3 - 2\cdot 6 = -9$, magnitude grows; the fixed step
   exceeds the stable range for this quadratic and diverges by oscillation.
3. $0.8 \times 0.5 \times 0.5 = 0.2$.

</details>

## Library recommendations

- **missing-function** — `62-optimization.plato`: no declared step operator such as
  `DescentStep(params: GradientDescentParameters, x, grad, velocity) -> (x', velocity', params')`.
  Teaching step size needs a single pure transition; the file only stores knobs.

- **wrong-shape** — `62-optimization.plato`: `GradientDescentParameters` omits
  `LineSearch` while `LbfgsParameters` includes it. Either add
  `LineSearch` to gradient descent (with `FixedStep` as default) or document that GD
  is intentionally fixed-step-only so callers do not hunt for Wolfe settings in vain.

- **missing-type** — `62-optimization.plato`: `OptimizationResult` lacks
  `GradientNorm: Number` (and optionally last gradient vector). Step-size debugging
  and "are we actually at a critical point?" checks need it.

- **doc-comment** — `62-optimization.plato`: clarify whether `Momentum` uses classical
  heavy-ball or Nesterov-style evaluation order; the update formula is not written,
  and different libraries disagree.
