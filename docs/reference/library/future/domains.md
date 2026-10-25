# Aspirational library domains

The `future` tier is excluded from the default shipping lint/codegen scope and must not be referenced by shipping tiers. It still participates in the repository's parse/type-check policy. [Verification policy](../../../../stdlib/VERIFICATION.md) and its [manifest](../../../../stdlib/verification.json) specify the actual scopes; a declaration's location is not evidence of executable support.

“Aspirational” does not mean “all empty.” Some files contain substantial reference algorithms. Their numerical correctness, performance, code generation and law execution must be checked independently. These guides explain their intended data contracts and visible limitations, without claiming that the whole tier has passed execution gates.

## Domain guides

- [Dynamics, collision, particles and cloth](dynamics.md): time stepping, contacts and constraint solvers.
- [Engineering and finite elements](engineering.md): material models, structural equations and iterative solves.
- [Animation and signals](animation-signals.md): tracks, skeletons, sampled signals and optimization descriptions.
- [Scientific data and uncertainty](scientific-data.md): time-indexed measurements, provenance and uncertainty.
- [Geospatial and higher dimensions](geospatial.md): planetary coordinates and exploratory higher-dimensional shapes.

These are independent topics, not a promise that they form a complete simulation suite. Exact members and source comments are retained in the generated `api/future` pages linked from each guide. For runtime expectations at a host boundary, follow [legacy and verification](../verification-legacy.md).

## Related work

[Box2D's simulation documentation](https://box2d.org/documentation/md_simulation.html), [JCGM's measurement guides](https://www.bipm.org/en/committees/jc/jcgm/publications) and [glTF's scene/animation model](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) are useful comparison points for different future domains. They are not imported specifications: Plato must state which algorithms, units and representations it actually implements.
