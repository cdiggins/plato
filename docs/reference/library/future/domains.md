# Aspirational library domains

The `future` tier is not part of the default library. Default lint and code generation skip it, and a shipping tier must not reference it. It still has to parse and type-check. The verification policy and its manifest name the actual scopes. A file's folder is not evidence that an operation runs.

Some files in this tier contain substantial reference algorithms. Numerical correctness, performance, code generation, and law execution still have to be checked on their own. These guides describe the data contracts and the limitations that are visible in the source. They do not claim that the tier has passed execution gates.

## Domain guides

- [Dynamics, collision, particles and cloth](dynamics.md): time stepping, contacts, and constraint solvers.
- [Engineering and finite elements](engineering.md): material models, structural equations, and iterative solves.
- [Animation and signals](animation-signals.md): tracks, skeletons, sampled signals, and optimization descriptions.
- [Scientific data and uncertainty](scientific-data.md): time-indexed measurements, provenance, and uncertainty.
- [Geospatial and higher dimensions](geospatial.md): planetary coordinates and exploratory higher-dimensional shapes.

These topics are independent. They are not a complete simulation suite. Exact members and source comments stay on the generated `api/future` pages linked from each guide. For what a host boundary can expect at runtime, read legacy and verification.

## Where to look

- [Verification policy](../../../../stdlib/VERIFICATION.md) and its [manifest](../../../../stdlib/verification.json)
- [Legacy and verification](../verification-legacy.md)

[Box2D's simulation documentation](https://box2d.org/documentation/md_simulation.html), [JCGM's measurement guides](https://www.bipm.org/en/committees/jc/jcgm/publications), and [glTF's scene and animation model](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) are comparisons for different domains in this tier. They are not imported specifications. Plato has to say which algorithms, units, and representations it implements.
