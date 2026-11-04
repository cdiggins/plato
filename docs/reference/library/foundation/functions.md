# Functions as mathematical objects

`IProcedural<D,R>` requires `Eval(x,input): R`, where `D` is the input domain and `R` is the result type. `Function1<D,R>` is a function value. A procedural object can instead be a record that carries parameters and an evaluation implementation. Several library families share that shape:

| Mapping | Interpretation |
|---|---|
| `Number -> Point3D` | Parameterized curve |
| `UvCoordinate -> Point3D` | Parameterized surface |
| `Point3D -> Number` | Scalar field |
| `Point3D -> Boolean` | Membership in an implicit volume |
| `Point3D -> Point3D` | Deformation of space |
| `Number -> Number` | Scalar curve or easing |

The table describes mathematical shapes. It is not a set of automatic conversions between every pair of interfaces. Distinct interfaces provide additional obligations and domain promises. `IProcedural` makes the domain and range pattern reusable across geometry, fields, and animation, and the interface alone supplies no rendering, automatic differentiation, or numerical integration engine. [PBRT's geometry and transformations](https://pbr-book.org/4ed/contents) distinguish geometric objects and their parameterizations in a renderer, and they do not define this interface.

## Composition, inversion and period

For `f: A -> B` and `g: B -> C`, mathematical composition maps `a` to `g(f(a))`. An adapter that stores a closure is still a value. It is not necessarily serializable, and it is not necessarily inspectable as a graph. A field-expression graph is a separate, explicit representation.

`IBijective<D,R>` additionally supplies `EvalInverse`. The intended laws are `inverse(eval(x))=x` and `eval(inverse(y))=y` on the stated domains. The interface does not prove them, and it does not establish behavior outside those domains.

`IPeriodic<P>` supplies a period. Wrapping requires a meaningful positive period and must correct remainder behavior for negative inputs. `IParameterDomain` supplies a preferred `NumberInterval`. That interval does not automatically clamp an arbitrary `Eval` call. A curve's conventional unit interval is a parameter choice, not a restriction inserted around every function evaluation.

## Example and pitfalls

If a field computes `f(p)=p.X*p.X`, then sampling at points with X coordinates 2 and 3 gives 4 and 9, regardless of their Y and Z coordinates. A grid built from these samples adds an interpolation policy. The grid is no longer the same representation as the analytic formula.

A pure function value can still be partial. Division by zero, invalid indexing, and recursion beyond a runtime budget do not disappear because a function has no side effects. Read preconditions independently of purity.

## Where to look

- [Functional interfaces](../../api/foundation/functional.concepts.md)
- [Procedural helpers](../../api/foundation/functional-procedural.library.md)
