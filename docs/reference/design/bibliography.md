# Bibliography

This page lists the outside works that the repository already cites or compares against, and
what Plato's documentation or design takes from each. It adds no work the repository does not
mention. A citation shows a comparison. It does not say where Plato came from, and it does not
say that Plato matches the work.

Links go to the web editions the repository itself links. A web edition can change after the
repository cites it.

## Language specifications and references

| Work | What Plato takes or contrasts |
|---|---|
| The Rust Reference. Rust project. <https://doc.rust-lang.org/reference/> | The [reference README](../README.md) takes its chapter layout from it. Chapters compare [structs](https://doc.rust-lang.org/reference/items/structs.html) ([products](../language/products.md)), [enums](https://doc.rust-lang.org/reference/items/enumerations.html) and [match](https://doc.rust-lang.org/stable/reference/expressions/match-expr.html) ([sums](../language/sums.md)), and [traits](https://doc.rust-lang.org/reference/items/traits.html) ([interfaces](../language/interfaces.md)). Rust's patterns are richer than Plato's, and Plato does not adopt its syntax or lifetime rules. |
| The Rust Programming Language, chapter 4, "Understanding Ownership". Klabnik and Nichols. <https://doc.rust-lang.org/book/ch04-00-understanding-ownership.html> | Compared with Plato's runtime-checked builders in [state and builders](../language/state-and-builders.md) and the [language improvement register](../review/language-improvements.md). Rust's compile-time borrowing rules are stronger. |
| Haskell 2010 Language Report, the [introduction](https://www.haskell.org/onlinereport/intro.html) and [declarations chapter](https://www.haskell.org/onlinereport/decls.html). Marlow (ed.). | A comparison for algebraic data types and type classes in [the model](../language/model.md), [interfaces](../language/interfaces.md), and [generics](../language/generics.md). Plato is eager, so the report's non-strict evaluation does not apply. |
| C# language specification, [expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions), [classes](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#extension-methods), and [operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/). Microsoft. | The generated target. [Evaluation](../language/evaluation.md), [syntax](../language/syntax.md), [expressions](../language/expressions.md), and [functions](../language/functions.md) contrast Plato's rules with C#'s, because the C# writer must not copy a C# spelling over a Plato rule. |
| .NET [floating-point types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types) and [Double.Equals](https://learn.microsoft.com/en-us/dotnet/api/system.double.equals). Microsoft. | The host behavior behind [numbers](../language/numbers.md) and [equality](../language/equality.md). Generated `Number` equality inherits the NaN behavior of `Double.Equals`. |

## Formal methods

| Work | What Plato takes or contrasts |
|---|---|
| Software Foundations, volume 2, [Types](https://softwarefoundations.cis.upenn.edu/plf-current/Types.html) and [Smallstep](https://softwarefoundations.cis.upenn.edu/plf-current/Smallstep.html). Pierce and others. | The [formal model](../formal/README.md) states that its proofs are not the small-step progress and preservation theorems, and that divergence and intermediate states stay open. The [documentation issue register](../review/documentation-issues.md) cites the book as an example of prose, rules, and proof artifacts explaining each other. |
| Theorem Proving in Lean 4. Avigad, de Moura, Kong, and Ullrich. <https://lean-lang.org/theorem_proving_in_lean4/> | The formal model's reading guide for propositions as types, induction, and the line between computation and axioms. The proofs are checked in [Core.lean](../formal/Core.lean) against a pinned Lean release. |
| elan, the Lean toolchain manager. <https://github.com/leanprover/elan> | Installs the pinned checker release the formal model is reproduced with. |

## Libraries Plato compares against

The [library guide](../library/README.md) cites these where a Plato helper shares a name with
theirs but differs in meaning.

| Work | What Plato takes or contrasts |
|---|---|
| Physically Based Rendering, fourth edition. Pharr, Jakob, and Humphreys. <https://pbr-book.org/4ed/contents> | Cited for geometry and transformations, vectors and points, radiometry, Monte Carlo sampling, and cameras and lights, for example in [points and vectors](../library/foundation/points-vectors.md) and [cameras and lights](../library/graphics/cameras-lights.md). Plato's camera helpers are not that book's optical or transport implementation. |
| NIST Digital Library of Mathematical Functions. <https://dlmf.nist.gov/> | The reference for identities behind [angles](../library/foundation/angles.md), [polynomials](../library/foundation/polynomials.md), and [probability](../library/foundation/probability.md). |
| CGAL manual. The CGAL Project. <https://doc.cgal.org/latest/Manual/packages.html> | Compared in [scalars and algebra](../library/foundation/scalars-algebra.md), [curves](../library/geometry/curves.md), and [triangulation](../library/geometry/triangulation.md). |
| NumPy [array routines](https://numpy.org/doc/stable/reference/routines.array-manipulation.html) and [polynomial package](https://numpy.org/doc/stable/reference/routines.polynomials.html). | Plato's records and immutable arrays do not imply NumPy's array model ([collections](../library/foundation/collections.md)). The polynomial page covers interchange ([polynomials](../library/foundation/polynomials.md)). |
| SciPy [statistics reference](https://docs.scipy.org/doc/scipy/reference/stats.html). | A familiar result name does not supply the estimator ([statistics](../library/foundation/statistics.md), [probability](../library/foundation/probability.md)). |
| System.Numerics [Matrix4x4](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4). Microsoft. | Compared for row and application order in [matrices and transforms](../library/foundation/matrices-transforms.md). |
| glTF 2.0 specification. Khronos Group. <https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html> | An interchange format, not Plato's scene model ([cameras and lights](../library/graphics/cameras-lights.md)). |
| JCGM [measurement guides](https://www.bipm.org/en/committees/jc/jcgm/publications). BIPM. | Cited in [engineering](../library/future/engineering.md) for measurement vocabulary in an aspirational library. |
| DOLFINx [elasticity demonstration](https://docs.fenicsproject.org/dolfinx/main/python/demos/demo_elasticity.html). FEniCS Project. | A finite-element framework contrasted with the different element formulas in [engineering](../library/future/engineering.md). |

## The comparative studies

The standard library was compared with a wider set of libraries in dated, frozen reports. Read
them for the analysis, not for current state.

- [Plato standard-library comparative study (2026-08-04, revision 2)](../../reports/plato-stdlib-comparative-study-2026-08-04-r2.md)
  compares the forward library with geometry3Sharp, libigl, geometry-central, nalgebra, Eigen,
  GLM, NumPy, SciPy, Shapely, CGAL, Open CASCADE, Wolfram's computational geometry, three.js,
  Unity, Unreal Engine, Godot, and Blender. The report links each project's documentation.
- [Plato standard-library comparative study (2026-08-04)](../../reports/plato-stdlib-comparative-study-2026-08-04.md)
  is the earlier expansion covering Open CASCADE, Wolfram's region system, and three.js.
- [Plato standard-library comparative study (2026-08-03)](../../reports/plato-stdlib-comparative-study-2026-08-03.md)
  is the initial comparison.

## How to cite this reference

The reference is the `docs/reference/` folder of the Plato repository, which is the `plato`
repository under the `ara3d` organisation on GitHub. It has no released version numbers. Cite a
commit: give the repository, the path of the page, and the full commit hash you read. A page
changes with the source tree, so a hash is the only stable citation. A branch name or a bare URL
is not.

For a result in the [formal model](../formal/README.md), also cite the theorem name in
[Core.lean](../formal/Core.lean) and the pinned Lean release in its `lean-toolchain` file at
that commit. For a language rule, cite [SEMANTICS.md](../../SEMANTICS.md) at the same commit.
For a design choice, cite the [decision record](rejected-alternatives.md) by its file name.
