---
id: plato-277
title: "stdlib interface-gap burn-down: the 17 markers that need a language or interface decision"
type: debt
status: ready
priority: p2
effort: M
risk: low
area: plato
sprint: 
links: [submodules/Plato/stdlib, submodules/Plato/stdlib/LIBRARIES.md]
created: 2026-07-28
closed:
---

Burn-down queue for the `TODO(interface-gap)` / `TODO(cross-package)` / `TODO(follow-up)`
markers in `submodules/Plato/stdlib`. Per `stdlib/LIBRARIES.md` rule 6 these markers are a
queue, not a permanent ban.

**Scope narrowed 2026-07-29.** This issue used to be a 74-marker inventory across all
categories. The two highest-leverage items in it are now done (see History), and the
`TODO(content)` category has grown into unrelated work of its own — pure stdlib authoring
(special functions, surface `Eval` bodies, statistics estimators, bounds tightening), which
needs no language or interface decision. That pile is **out of scope here**; see
"Out of scope: TODO(content)" below.

Current counts (grep, 2026-07-29, after the plato-293 re-partition):

| category | markers |
|---|---|
| `TODO(interface-gap)` | 11 |
| `TODO(cross-package)` | 4 |
| `TODO(follow-up)` | 1 |
| **in scope** | **16** |
| `TODO(content)` — out of scope | 36 |

Re-run before acting; the tree moves. The `grep -v` matters — resolved markers are retired in
place as `RESOLVED (was TODO(interface-gap): ...)`, so a bare grep over-counts by three:

```bash
cd submodules/Plato/stdlib && grep -rn "TODO(interface-gap)\|TODO(cross-package)\|TODO(follow-up)" --include=*.plato . | grep -v RESOLVED
```

Every marker carries its own full diagnosis in place — the reason it is blocked, what was
tried, and where the fix belongs. Read the marker, not a summary of it. The groupings below
exist only to show which markers share a single root cause.

## Type-parameter bounds: what exists, and what actually doesn't

An earlier revision of this issue claimed Plato has no type-parameter constraint syntax and
built a headline section on it. **That was false.** Interface `where` clauses are a first-class,
fully wired feature:

- Grammar: `Constraint` / `ConstraintList` at
  [PlatoGrammar.cs:172-173](../../submodules/Plato/parakeet/Parakeet.Grammars/PlatoGrammar.cs:172),
  wired into the `Interface` rule at
  [:191](../../submodules/Plato/parakeet/Parakeet.Grammars/PlatoGrammar.cs:191).
- AST: `AstConstraint` [Ast.cs:337](../../submodules/Plato/Plato.AST/Ast.cs:337),
  `AstTypeDeclaration.Constraints` [:374](../../submodules/Plato/Plato.AST/Ast.cs:374),
  populated at [AstNodeFactory.cs:499](../../submodules/Plato/Plato.AST/AstNodeFactory.cs:499).
- Symbols: resolved into `TypeParameterDef.Constraints` at
  [SymbolFactory.cs:509-512](../../submodules/Plato/PlatoCompiler/Symbols/SymbolFactory.cs:509).
- A dedicated lint rule, **LINT002**, exists for `where` clauses naming an undeclared type
  parameter: [Linter.cs:260](../../submodules/Plato/PlatoCompiler/Analysis/Linter.cs:260).
- Live in stdlib today:
  [intervals-bounds.concepts.plato:14](../../submodules/Plato/stdlib/intervals-bounds.concepts.plato:14)
  (`where T: Additive, T: Interpolatable, T: Comparable`),
  [:26](../../submodules/Plato/stdlib/intervals-bounds.concepts.plato:26)
  (`where TPoint: Difference<TDelta>`),
  [algebra-metric.concepts.plato:49](../../submodules/Plato/stdlib/algebra-metric.concepts.plato:49)
  (`where TDelta: Additive, TDelta: Scalable`). The `Additive` doc comment cites the feature
  as design intent
  ([algebra-operations.concepts.plato:12](../../submodules/Plato/stdlib/algebra-operations.concepts.plato:12)).

The "IntervalLike<T> bounded" entry in History below *was* a `where` clause, which is what
made the old claim self-contradicting.

**The real limit:** bounds sit only on `interface` / `interface` declarations. The `Type` rule
([:187](../../submodules/Plato/parakeet/Parakeet.Grammars/PlatoGrammar.cs:187)) carries no
`ConstraintList`, and `MethodDeclaration`
([:205](../../submodules/Plato/parakeet/Parakeet.Grammars/PlatoGrammar.cs:205)) takes no type
parameters at all — so a *library function's* type variables cannot be bounded. This is
correctly cited at
[numeric-structures-coordinate.library.plato:36](../../submodules/Plato/stdlib/numeric-structures-coordinate.library.plato:36)
("no `where` on library functions"). It blocks none of the 17 markers on its own.

The three items the old section grouped here were each misdiagnosed; their real status:

- **`WrapPhase`** — no language gap; **now shipped** (see History). `Periodic<TPeriod>` carries
  `where TPeriod: Additive, TPeriod: Modular` and `WrapPhase` is derived from the bound. The
  claimed second blocker (no `Eval` on `Periodic`) was not one: `WrapPhase` takes the domain
  value as a parameter rather than reading it off the surface.
- **Generic `Distance` over `Coordinate`** — never a syntax problem. Five implementors each
  carry a different delta type, so `Coordinate` would need a type parameter, a generic-arity
  change cascading to every bare `Coordinate` mention tree-wide; and `Distance` additionally
  needs `TDelta: Normed`, which `Difference` cannot bind (`Instant`'s delta is a `Duration`,
  not `Normed`). The marker documents concrete per-type bodies as the sanctioned resolution.
  [numeric-structures-coordinate.library.plato:20](../../submodules/Plato/stdlib/numeric-structures-coordinate.library.plato:20)
- **`Normalize`** — semantic, not syntactic. `Angle`/`Length`/`Integer` genuinely are not
  `Divisible` by Self; no bound can conjure the operation. The `NumberInterval`-specialized
  `ParameterOf` already fills the gap, so this is effectively resolved by specialization.
  [intervals-transforms-interval.library.plato:140](../../submodules/Plato/stdlib/intervals-transforms-interval.library.plato:140)

## Missing interface surfaces (cheap, but owned by `.interfaces` files)

- ~~`AngularCurve2D` / `AngularCurve3D`~~ — **DONE 2026-07-30** (commit `de223d6`, merged to main in `1f55a6c`): both interfaces + full-turn bridges landed in `curves.concepts.plato`. Bridges live in `curves-angular.library.plato` as `library CurvesAngular` (relocated out of the interfaces file in `558a945`, restoring LIBRARIES.md rule 1; interface bodies are declarations only, so bridges must be library functions). Two LINT013 warnings until the spirals/3D-curves passes adopt the interfaces.
  [curves-2d-spirals.library.plato:12](../../submodules/Plato/stdlib/curves-2d-spirals.library.plato:12),
  [curves-3d.library.plato:17](../../submodules/Plato/stdlib/curves-3d.library.plato:17)
- No analytic differentiation, so no true tangent or curvature: the parameter derivative of
  an evaluation is exposed by no interface. Trigonometry and `Sqrt` are available now, so this
  is the sole remaining blocker.
  [curves-capabilities.library.plato:9](../../submodules/Plato/stdlib/curves-capabilities.library.plato:9)
- No sanctioned `Point2D` offset in scope, blocking a finite-difference gradient
  (`Difference.Add` wants a `Vector2D` that `Coordinate` cannot supply).
  [fields-implicits-core.library.plato:32](../../submodules/Plato/stdlib/fields-implicits-core.library.plato:32)
- No sanctioned `Duration`-to-`Number` division, blocking an average rate of change.
  [fields-implicits-time-varying.library.plato:14](../../submodules/Plato/stdlib/fields-implicits-time-varying.library.plato:14)
- `MatrixLike` exposes no construction primitive, so a generic `Transpose` cannot be
  spelled; `MakeArray2D` yields `Array2D<Number>`, not a concrete `MatrixLike`. Each
  concrete matrix type supplies its own instead.
  [numeric-structures-matrix.library.plato:18](../../submodules/Plato/stdlib/numeric-structures-matrix.library.plato:18)
- Generic wrapped *evaluation* (`EvalWrapped` over any `Periodic` + `Procedural` receiver) is
  blocked by the real limit above: a library function cannot state a two-interface bound.
  `PeriodicCurve` is the interface that has both, and `EvalWrapped` lives there
  (`curves-capabilities.library.plato`). Generic `NormalizedPhase` is separately blocked —
  dividing a `TPeriod` by a `TPeriod` to get a `Number` is not something `Modular` supplies.
  Recorded at the retired marker,
  [functional-procedural.library.plato:41](../../submodules/Plato/stdlib/functional-procedural.library.plato:41)

## Algorithms `Reduce` did not unblock

`Reduce`/`Map`/`All`/`Any` on `Indexable` landed and closed the simple aggregate cases.
These three need shapes a single fold does not express, and each marker says so explicitly.
Treat them as "await a richer iteration primitive", not as pending work items.

- Cox-de Boor: a triangular DP over degree levels, each level reading two entries of the one
  below, with a zero-division guard at repeated knots.
  [splines-bspline.library.plato:14](../../submodules/Plato/stdlib/splines-bspline.library.plato:14)
- Natural cubic spline: the Thomas algorithm is a two-pass sweep (forward elimination
  building modified arrays, then backward substitution). Expressible as a `Reduce` carrying a
  tuple-of-arrays plus a reverse pass — correct but not clean, so no body shipped.
  [splines-interpolating.library.plato:214](../../submodules/Plato/stdlib/splines-interpolating.library.plato:214)
- Clothoid `Eval`: the term sum is now expressible, but a fixed-term Maclaurin series for
  the Fresnel integrals S/C degrades badly over the arc-length spans real road and rail
  clothoids use. Accuracy, not expressiveness, is the blocker.
  [curves-2d-spirals.library.plato:70](../../submodules/Plato/stdlib/curves-2d-spirals.library.plato:70)

## Cross-package obligations (4)

Each is a correct thing declared in the wrong package. Fix = move it to its owner.

- Boolean / String / Character orders (false < true, lexicographic, code-point).
  [core-comparison.library.plato:53](../../submodules/Plato/stdlib/core-comparison.library.plato:53)
- Concrete `Hash` / `LessThanOrEquals` for those same three types.
  [collections-indexable.library.plato:74](../../submodules/Plato/stdlib/collections-indexable.library.plato:74)
- `Sum(Indexable<Number>)` belongs to the P2 collections package; spelled inline as
  `Reduce(0.0, (total, x) => total + x)` throughout `random`.
  [random.library.plato:41](../../submodules/Plato/stdlib/random.library.plato:41)
- Non-uniform (centripetal / chordal) Catmull-Rom needs a non-uniform Hermite basis, which
  belongs beside the basis in `polynomials`. Until it lands, `Alpha` is documentation rather
  than behaviour.
  [splines-interpolating.library.plato:25](../../submodules/Plato/stdlib/splines-interpolating.library.plato:25)

## Follow-up (1)

Concrete per-type `Deform` bodies for ~18 types (Line2D/3D, Ray2D/3D, Triangle2D/3D,
Quad2D/3D, Circle, Ellipse, Polygon2D/3D, Polyline2D/3D, RegularPolygon,
PolygonWithHoles2D, Tetrahedron, mesh types). One-liners over each type's fields, but owned
by the packages that declare those types.
[intervals-transforms-transformable.library.plato:99](../../submodules/Plato/stdlib/intervals-transforms-transformable.library.plato:99)

## Out of scope: TODO(content) — 36 markers

Pure authoring work, no language or interface decision needed. Kept out of this issue so the
blocked-on-a-decision queue stays readable. Distribution:

- `surfaces.library.plato` (10) — missing `Eval` bodies: BezierPatch (de Casteljau),
  BSplineSurface (de Boor), NurbsSurface, CoonsPatch, SurfaceOfRevolution, LoftedSurface,
  SweptSurface, TubeSurface, OffsetSurface. Closure answers (`ClosedU`/`ClosedV`) ARE
  derived and are what tessellation reads; only evaluation is absent.
- `random-continuous-gamma.library.plato` (9) + `random-continuous-tails.library.plato` (6)
  — all 15 wait on one decision: whether the tree gets a special-function vocabulary
  (Gamma/LogGamma, regularized incomplete gamma and beta, erf, modified Bessel). Deliberately
  left unimplemented rather than approximated. **This is the leverage point in the content
  pile** — same shape `Reduce` had for interface-gap.
- Bounds tightening (8) — `planar-ellipses` (3, superellipse support/Area/Perimeter),
  `planar-circles` (2), `spatial-spheres` (1), `spatial-patches` (1), `polygons` (1, binary
  search over the convex vertex ring). These are NEW since the original inventory, landed
  with the port.
- `statistics.library.plato` (2) — no estimators at all. SummaryStatistics /
  FiveNumberSummary / Quantiles / BoxPlotStatistics need a sort (none declared) plus a choice
  among the nine standard quantile-interpolation conventions; Spearman and Kendall need
  ranking, i.e. the same sort.
- `random.library.plato` (1), `splines-bspline` and others (1 each).

If this pile is picked up, file it as its own issue and start with the special-function
decision.

## History

**2026-07-28 — fold primitive + constraint gaps.** Commits a4a4b7e, 9716e7b, 4488cf2,
9d95251 (4 agent missions). Markers 76 -> 57. Landed: `Reduce`/`Map`/`All`/`Any` on
`Indexable` (the highest-leverage item at the time); `Sliceable` inherits `Countable` +
Drop/TakeLast/DropLast; `EvalAll`; `IntervalLike<T>` bounded; `BoundsLike<TPoint,TDelta>` +
Extent/Diagonal; `Camera.AspectRatio` (6 implementors); `PeriodicCurve` inherits
`Procedural` + `EvalWrapped`; `Difference.Between(a,b) = b - a` pinned; geometry centroids;
Trace/FrobeniusNorm; voxel occupancy; the Horner `Eval` family
(Polynomial/PowerSeries/Sparse/Rational); single-fetch `SupportExtent`. Also resolved:
`RotateAbout` 2D and 3D (marked RESOLVED in place), and the `Vector` component gap
(`FromComponents` / `Broadcast` declared).

**2026-07-28 (later) — compiler gap closed.** `FunctionInstance.cs:162-165` rejected any
generic function of one or fewer parameters, so `Count`, `Freeze`, and `EmptyList` could not
be declared at all; a builder could be constructed and mutated but never consumed. Fixed in
Plato 925c03c: single-generic-param functions are allowed when the parameter determines all
type variables, with a named error for return-only type vars. `Count`/`Freeze`/`EmptyList`
now declared in `primitives.plato` (library `UniqueBuilders`); affine builders fully usable.
All gates green, goldens 0 diffs. Zero `TODO(compiler-gap)` / `TODO(writer-gap)` markers
remain in the tree.

**2026-07-28 (later) — testing scaffolding.** `stdlib-tests/` +
`Plato.ForwardConformanceTests` + `tools/regen-forward-conformance.ps1` (Plato aa43e19,
studio 5801928). Execution blocked by the writer ground-TIR gap — tracked as plato-291.

**2026-07-29 — re-partition + rescope.** plato-293 took `stdlib` from 85 files to 344, so
every `file.plato:line` reference in the old inventory died; markers themselves moved
verbatim. Doc rewritten against current paths, `TODO(content)` moved out of scope, effort
L -> M.

**2026-07-29 (later) — false "no constraint syntax" claim retracted.** The rescope above
promoted a claim from
[functional-procedural.library.plato](../../submodules/Plato/stdlib/functional-procedural.library.plato)
("no `where` clause and no `<T : Interface>` form anywhere in the tree — verified by grep") to
this issue's headline. It is false: interface `where` clauses are wired through grammar, AST,
symbol resolution, and their own lint rule, and are used in three places in current stdlib.
Verified against source; section rewritten and the three markers it grouped re-diagnosed
individually. The stale marker in the tree was corrected in place. Two other markers had
already retracted the same claim on their own
([algebra-metric.library.plato:61](../../submodules/Plato/stdlib/algebra-metric.library.plato:61),
[numeric-structures-coordinate.library.plato:33](../../submodules/Plato/stdlib/numeric-structures-coordinate.library.plato:33)).

**2026-07-29 (later) — `WrapPhase` landed.** With the premise corrected, the item turned out to
be two small edits, both applied: `Periodic<TPeriod>` gained
`where TPeriod: Additive, TPeriod: Modular`
([functional.concepts.plato:39](../../submodules/Plato/stdlib/functional.concepts.plato:39)) and
`WrapPhase(self: Periodic<$TPeriod>, t: $TPeriod)` is derived from that bound in
`functional-procedural.library.plato`, spelled
`((t % self.Period) + self.Period) % self.Period` to correct the truncated remainder for
negative inputs. `Additive` is in the bound as well as `Modular` because that correction needs
the addition. Marker retired in place as RESOLVED. In-scope markers 17 -> 16. Gates: stdlib
fast gate PASS, and the forward-stdlib checker worklist reports **0 diagnostics across 2286
functions** against a ceiling of 0 — so the new function type-checks clean rather than hiding
under ratchet slack.
