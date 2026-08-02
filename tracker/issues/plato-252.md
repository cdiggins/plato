---
id: plato-252
title: Automatic differentiation for Plato (forward-mode TIR pass, then reverse)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-27
closed:
links: []
---

Proposed 2026-07-27 (agent idea, accepted by user for capture). Untriaged.

## Why

Plato is pure, total-ish, and monomorphized — the conditions that make automatic differentiation
tractable are already met, and almost no other geometry library has AD through its whole stack.
Payoffs: exact SDF normals and curvature for free, inverse kinematics, curve/surface fitting,
shape optimization, physics constraint solving ([[plato-249]]), and training in the LLM demo
([[plato-251]]). Numerical integration ([[plato-250]]) supplies the other half of a simulation stack.

## Two routes

**Route A — dual numbers in the library.** Add `type Dual { Value: Number; Derivative: Number }`
implementing the `Real` interface, and code written generically over `Real` differentiates for free.
Blocker: the stdlib is not scalar-polymorphic — `plato-src-v3/08-vectors.plato` declares
`Vector2 { X: Number; Y: Number }`, hardcoding the scalar. Getting Route A to reach geometry means
generalizing the type family to something like `Vector3<$T: Real>`, which is a large, invasive
library change (and interacts with [[plato-241]]/[[plato-247]] naming). A cheap subset does still
stand alone: ship `Dual` plus hand-written duals of the scalar functions and let users write
differentiable code against `Dual` explicitly.

**Route B — a TIR differentiation pass (recommended).** The compiler already lowers every function
to a monomorphized Typed IR (Normalize -> Constrain -> Solve -> Elaborate -> Monomorphize,
`PlatoCompiler/Checking/`) and every C# body is rendered from it (`TirCSharpBodyWriter`). AD then
becomes a source-to-source transform over TIR nodes: given monomorphized `f`, synthesize `f'` by
structural rewrite. Forward mode first — each `Number`-typed value becomes a (value, tangent) pair,
one rule per primitive op, chain rule at call sites. Reverse mode (tape or adjoint/CPS transform)
is a later phase.

Route B is preferred because purity removes aliasing and side-effect analysis from the problem,
monomorphization removes dispatch to differentiate through, and one pass serves every backend
(C#/GLSL/C++/Rust) instead of just one.

## Open questions

- **Surface syntax.** An annotation on differentiable functions, or compiler-recognized intrinsics
  `Derivative(f, x)` / `Gradient(f)` that trigger synthesis at the call site?
- **Primitive derivative table.** Needs an authoritative rule for every op in the `intrinsics-*.library.plato` files,
  including the awkward ones: `Abs`/`Min`/`Max`/`Clamp` (subgradient convention at the kink),
  `Floor`/`Round`/comparisons (zero derivative, or a hard error?), integer ops.
- **Shapes.** Gradient of `Vector3 -> Number` should come back as a `Vector3`; `Vector3 -> Vector3`
  wants a Jacobian. Decide what types the synthesized functions have before writing the pass.
- **Pass ordering.** Must run after monomorphization but before the writers, and before
  `ComponentUnroller`/`--optimize` so the optimizer sees the derivative code too. Watch generated
  code size.
- **Backend reach.** GLSL/C++ skip lambda lowering today; check what that costs a reverse-mode tape.

## Validation (cheap, already built)

The conformance harness has seeded deterministic value generation (`ConformanceSupport.cs`,
`ValueGen`). Derivative laws are then free: for random inputs, compare the synthesized analytic
derivative against a central finite difference within tolerance. That gives a real gate from day
one rather than eyeballed spot checks.

## Suggested staging

1. `Dual` type + scalar derivative table, validated by finite-difference laws.
2. Forward-mode TIR pass for scalar -> scalar functions.
3. Extend to vector inputs/outputs (gradients, Jacobians).
4. Reverse mode for the many-inputs-one-output case (fitting, training).
