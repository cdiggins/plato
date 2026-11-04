# Cookbook

Problem-shaped pages for scientists and engineers. Each recipe states a problem, gives a complete Plato program, shows the result, and walks through the program. The program is a fixture group under `implementation/examples/cookbook-<recipe>/`, and the law runner checks the result shown on the page against it.

A recipe uses only the shipping tiers (foundation, geometry, graphics) and says which it needs. Where the library holds a closed-form body, the recipe calls it. Where the library has no estimator (the [statistics guide](../library/foundation/statistics.md) lists the gaps), the recipe writes the few lines itself and says so.

## Recipes

| Recipe | Problem | Tiers |
|---|---|---|
| [Fit a line by least squares](fit-line.md) | Slope and intercept of noisy measurements, with the fit quality | foundation |
| [Find a root with Newton's method](newton-root.md) | Evaluate a cubic and its derivative, then solve `p(x) = 0` | foundation |
| [Transform points by a rotation and a translation](transform-points.md) | Move a set of points with one rigid map | foundation |
| [Summarize a sample](sample-statistics.md) | Mean, sample variance, spread and standard error | foundation |
| [Intersect a ray with a triangle](ray-triangle.md) | Where a ray strikes a triangle, and whether it does | foundation, geometry |

Every recipe is checked in the interpreter. A result is checked within the tolerance its law states; the library's own comparison policy is in [stdlib/CONVENTIONS.md](../../../stdlib/CONVENTIONS.md).
