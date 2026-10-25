# Functions as mathematical objects

[Functional interfaces](../../api/foundation/functional.concepts.md) and [procedural helpers](../../api/foundation/functional-procedural.library.md) unify many library families. `IProcedural<D,R>` requires `Eval(x,input): R`, where `D` is the input domain and `R` the result type. `Function1<D,R>` is a function value; a procedural object can instead be a record carrying parameters and an evaluation implementation.

| Mapping | Interpretation |
|---|---|
| `Number -> Point3D` | Parameterized curve |
| `UvCoordinate -> Point3D` | Parameterized surface |
| `Point3D -> Number` | Scalar field |
| `Point3D -> Boolean` | Membership in an implicit volume |
| `Point3D -> Point3D` | Deformation of space |
| `Number -> Number` | Scalar curve or easing |

The table describes mathematical shapes, not automatic conversions between every pair of interfaces. Distinct interfaces provide additional obligations and domain promises.

## Composition, inversion and period

For `f: A -> B` and `g: B -> C`, mathematical composition maps `a` to `g(f(a))`. An adapter that stores a closure is still a value; it is not necessarily serializable or inspectable as a graph. A field-expression graph is a separate, explicit representation.

`IBijective<D,R>` additionally supplies `EvalInverse`; the intended laws are `inverse(eval(x))=x` and `eval(inverse(y))=y` on the stated domains. The interface does not prove them or establish behavior outside those domains.

`IPeriodic<P>` supplies a period. Wrapping requires a meaningful positive period and must correct remainder behavior for negative inputs. `IParameterDomain` supplies a preferred `NumberInterval`; this does not automatically clamp an arbitrary `Eval` call. For example, a curve's conventional unit interval is a parameter choice, not a restriction inserted around every function evaluation.

## Example and pitfalls

If a field computes `f(p)=p.X*p.X`, then sampling at points with X coordinates 2 and 3 gives 4 and 9 regardless of their Y/Z coordinates. A grid built from these samples adds an interpolation policy: it is no longer the same representation as the analytic formula.

Pure function values still can be partial: division by zero, invalid indexing and recursion beyond a runtime budget do not disappear because a function has no side effects. An API consumer must read preconditions independently of purity.

## Related work

[PBRT's geometry and transformations](https://pbr-book.org/4ed/contents) distinguish geometric objects and their parameterizations in a renderer. Plato's `IProcedural` makes the domain/range pattern reusable across geometry, fields and animation; this interface alone supplies no rendering, automatic differentiation or numerical integration engine.
