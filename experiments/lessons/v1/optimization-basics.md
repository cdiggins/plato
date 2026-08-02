---
lesson: optimization-basics
title: Optimization Basics
domain: Math, statistics & signals
v3-files: [62-optimization.plato]
audience: High-school calculus intuition (slope, minimum of a parabola) and general programming background
status: draft-v1
---

# Optimization Basics

Closest point on a mesh to a ray. Best-fit plane through noisy scans. Camera pose that
reprojects markers with least error. Geometry is full of questions of the form: **find
parameters $x$ that make a cost $f(x)$ as small as possible** (or as large). Sometimes
calculus hands you a closed form. Often you iterate: start somewhere, walk downhill, stop
when the steps get tiny.

## The idea

### Goals and landscapes

An **objective** $f$ maps a parameter vector to a score. **Minimize** or **maximize** is
an explicit goal (maximizing $f$ is minimizing $-f$). The graph of $f$ is a landscape:
valleys, ridges, saddle points, flat plateaus.

```
  f(x)
    |     /\        local max
    |    /  \/\
    |   /      \     global min →  ★
    |__/________\____
         x
```

Local minima can trap greedy methods. Smooth convex bowls are the friendly case: any local
minimum is global, and gradient methods behave.

### Gradient descent (pictures)

The **gradient** $\nabla f$ points uphill. To minimize, step the other way:

$$
x_{k+1} = x_k - \eta\,\nabla f(x_k)
$$

Learning rate $\eta$ scales the step. Too large → overshoot / diverge. Too small → snail.
**Momentum** blends in the previous step to smooth zigzags. **Decay** shrinks $\eta` over
iterations.

In 1D, $\nabla f$ is just $f'(x)$ — follow the slope toward zero derivative. In 2D, the
negative gradient is the steepest-descent arrow on a contour map.

```
  contours of f          step
     ○ ○ ○               ●──►●──►★
    ○  ★  ○                 downhill
     ○ ○ ○
```

### Line search

The gradient gives a direction $d$; **line search** picks a step length along that ray:
fixed step, backtracking, Wolfe conditions, golden section, or "exact" 1D minimization.
Good line search is why quasi-Newton methods converge fast without hand-tuning $\eta$ every
time.

### Derivative-free and stochastic cousins

When gradients are unavailable or noisy: **Nelder–Mead** (simplex reflection/expansion),
**simulated annealing**, **genetic algorithms**, **differential evolution**, **particle
swarms**. They explore more globally at higher evaluation cost — useful for nasty
non-smooth geometry objectives.

### Root finding and brackets

Solving $g(x) = 0$ is optimization's sibling. A **root bracket** $[a,b]$ with opposite
signs guarantees a root for continuous $g$ (intermediate value theorem). A **bracketed
minimum** triples $a < m < b$ with $f(m)$ below both ends — the setup for golden-section
search.

### Constrained problems

Variables often have bounds and linear equalities/inequalities. **Linear programs** and
**quadratic programs** encode those constraints explicitly — the bread and butter of
resource allocation and of many geometry QP subproblems (e.g. projected constraints).

### Stopping

Iterate until the step or objective change is below absolute/relative tolerance, or until
iteration / evaluation budgets expire. Record *why* you stopped: converged, max
iterations, stalled, failed, numerical error.

## In Plato

File `62-optimization.plato` is vocabulary for goals, stopping, results, and algorithm
tuning. Solvers themselves are later; parameters are pure records. Budgets use `-1` for
"no limit."

### Goals, criteria, results

```plato
type OptimizationGoal = Minimize | Maximize;

type ConvergenceCriteria
    implements Value
{
    MaxIterations: Integer;
    AbsoluteTolerance: Number;
    RelativeTolerance: Number;
    MaxEvaluations: Integer;
}

type TerminationReason = Converged | MaxIterations | Stalled | Failed | NumericalError;

type OptimizationResult
    implements Value
{
    Parameters: Array<Number>;
    ObjectiveValue: Number;
    Iterations: Integer;
    Converged: Boolean;
    Reason: TerminationReason;
}
```

### Roots, brackets, least squares

```plato
type RootBracket
    implements Value
{
    Lower: Number;
    Upper: Number;
}

type BracketedMinimum
    implements Value
{
    Lower: Number;
    Middle: Number;
    Upper: Number;
}

type RootFindResult
    implements Value
{
    Root: Number;
    Iterations: Integer;
    Residual: Number;
    Converged: Boolean;
}

type LeastSquaresResult
    implements Value
{
    Coefficients: Array<Number>;
    ResidualNorm: Number;
    Iterations: Integer;
    Converged: Boolean;
}

type LineSearch = FixedStep | Backtracking | WolfeConditions | GoldenSection | Exact;
```

### Algorithm parameters

```plato
type GradientDescentParameters
    implements Value
{
    LearningRate: Number;
    Momentum: Number;   // [0, 1)
    Decay: Number;      // 1 = constant rate
}

type LbfgsParameters
    implements Value
{
    MemorySize: Integer;
    LineSearch: LineSearch;
}

type NelderMeadParameters
    implements Value
{
    Reflection: Number;
    Expansion: Number;
    Contraction: Number;
    Shrink: Number;
}

type SimulatedAnnealingParameters
    implements Value
{
    InitialTemperature: Number;
    CoolingRate: Number;
    Seed: Integer;
}

type GeneticAlgorithmParameters
    implements Value
{
    PopulationSize: Integer;
    MutationProbability: Probability;
    CrossoverProbability: Probability;
    EliteCount: Integer;
    Seed: Integer;
}

type DifferentialEvolutionParameters
    implements Value
{
    PopulationSize: Integer;
    DifferentialWeight: Number;
    CrossoverProbability: Probability;
    Seed: Integer;
}

type ParticleSwarmParameters
    implements Value
{
    SwarmSize: Integer;
    Inertia: Number;
    CognitiveWeight: Number;
    SocialWeight: Number;
    Seed: Integer;
}
```

### Linear and quadratic programs

```plato
type ConstraintRelation = Equality | LessOrEqual | GreaterOrEqual;

type LinearProgram
    implements Value
{
    ObjectiveCoefficients: Array<Number>;
    ConstraintMatrix: MatrixN;
    ConstraintRelations: Array<ConstraintRelation>;
    RightHandSide: Array<Number>;
    LowerBounds: Array<Number>;
    UpperBounds: Array<Number>;
}

type QuadraticProgram
    implements Value
{
    Quadratic: MatrixN;
    Linear: Array<Number>;
    ConstraintMatrix: MatrixN;
    ConstraintRelations: Array<ConstraintRelation>;
    RightHandSide: Array<Number>;
    LowerBounds: Array<Number>;
    UpperBounds: Array<Number>;
}
```

Usage-shaped sketches — closest point on a line segment as 1D search over $t \in [0,1]$:

```plato
// f(t) = | (a + t*(b-a)) - p |^2
let criteria = ConvergenceCriteria {
    MaxIterations: 50,
    AbsoluteTolerance: 1e-8,
    RelativeTolerance: 1e-8,
    MaxEvaluations: -1
};

let gd = GradientDescentParameters {
    LearningRate: 0.1,
    Momentum: 0,
    Decay: 1
};

// After solving:
let result = OptimizationResult {
    Parameters: [tStar],
    ObjectiveValue: fAtTStar,
    Iterations: 12,
    Converged: true,
    Reason: Converged
};
```

Fitting example:

```plato
let ls = LeastSquaresResult {
    Coefficients: [intercept, slope],
    ResidualNorm: ...,
    Iterations: 1,
    Converged: true
};
```

## Pitfalls / fine print

**Local vs global.** Gradient descent on a non-convex closest-feature energy can snap to the
wrong basin (wrong triangle, wrong local dent). Multi-start or geometry-aware initialization
matters more than tweaking $\eta$.

**Scaling parameters.** Mixing meters and radians in one `Array<Number>` without scaling
makes gradients anisotropic — descent crawls in one coordinate and leaps in another.

**Tolerance units.** Absolute tolerance on an objective in $\mathrm{m}^2$ is not comparable
to tolerance on radians. Relative tolerance needs a sensible magnitude floor near zero.

**`-1` budgets.** Forgetting the sentinel and writing `0` means "no evaluations allowed,"
not "unlimited."

**Momentum in [0, 1).** Momentum `1` is excluded for a reason — it can fail to damp.

**QP/LP matrix shapes.** Row $i$ of `ConstraintMatrix` must align with
`ConstraintRelations[i]` and `RightHandSide[i]`; silent length mismatches are pure data
until a solver exists to check them.

**Converged flag vs Reason.** Always read `Reason`; `Converged: false` with
`MaxIterations` is a different story from `NumericalError`.

## Try it

1. $f(x) = (x-3)^2$. Starting at $x=0$, does one gradient step with small $\eta > 0$ move
   left or right?
2. Why bracket a root before polishing with Newton-like methods?
3. Name a geometry task that is naturally a least-squares problem.

<details>
<summary>Answers</summary>

1. Right — $f'(x) = 2(x-3)$, so at $0$ the derivative is negative, and minimizing steps
   opposite the gradient means increasing $x$ toward $3$.
2. Brackets guarantee a root exists in the interval (for continuous sign-changing $g$) and
   give a safe region when local methods diverge or jump to another root.
3. Plane or transform fitting to point correspondences; polyline simplification errors;
   camera resection from reprojection residuals — all minimize summed squared errors.

</details>

## Library recommendations

- **missing-function** — `62-optimization.plato`: abundant parameter and result types, but
  no `Minimize`, `FindRoot`, or `Solve(LinearProgram)` declarations. The lesson can teach
  gradient descent only as prose around `GradientDescentParameters`.

- **missing-type** — `62-optimization.plato`: no `Objective` / `DifferentiableObjective`
  interface with `Value(Self, Array<Number>)` and optional `Gradient`. Without that, solver
  parameters float free of any function they could optimize.

- **missing-type** — `62-optimization.plato`: no `BoxConstraints` separate from full LP —
  many geometry problems only need per-variable bounds. Teaching projected gradient needs a
  lighter type than `LinearProgram`.

- **doc-comment** — `62-optimization.plato`: `OptimizationResult` has both `Converged: Boolean`
  and `Reason: TerminationReason`. State the invariant (`Converged` iff `Reason == Converged`)
  so implementors and callers do not disagree.
