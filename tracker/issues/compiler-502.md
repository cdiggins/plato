---
id: compiler-502
title: Include inter-stage work in B-rep meshing timing
type: problem
status: idea
priority: p2
effort: S
risk: low
area: compiler
sprint: 
created: 2026-10-24
closed:
links: [compiler-499]
---

The plato-brep harness at 99c8fdee records Tables/Edges/Faces/Refine/Assemble clocks,
but their sum omits inter-stage work such as EdgesByFace, AdjacentSurfaces, changed-edge
and remeshed-face bookkeeping. TotalMs also sums selected stages rather than elapsed
end-to-end time. AllocatedBytes does include that work. This can hide optimization
benefits or costs. compiler-499 adds a whole-meshing stopwatch in an isolated experiment
host, while retaining the existing stage sum; the upstream harness remains unchanged.

## Done means

- [ ] Add a whole-meshing elapsed metric to the upstream B-rep harness with clear boundaries.
- [ ] Distinguish elapsed wall time from stage sums in its summary and reporting consumers.
- [ ] Verify equivalent meshes and complete timing coverage on representative STEP models.
