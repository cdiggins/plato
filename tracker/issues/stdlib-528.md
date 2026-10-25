---
id: stdlib-528
title: Point-cloud deformation leaves normals unchanged under rotation
type: bug
status: idea
priority: p1
effort: M
risk: med
area: stdlib
sprint: 
created: 2026-10-25
closed:
links: [stdlib/geometry/pointclouds.library.plato]
---

Source audit for docs-523: Deform on OrientedPointCloud3D and AttributedPointCloud3D maps
positions while carrying normals through unchanged. A comment claims normals stay exact for
rigid mappings, but rotation is rigid and must rotate the normals too. For example, rotating
the +X normal by a quarter turn about +Z should yield +Y; these bodies retain +X.

This can silently produce incorrect lighting and geometric normal queries. A position-only
arbitrary mapping lacks enough information to transform normals in general: decide whether to
require a Jacobian/normal mapping, specialize rigid transforms, or recompute with documented
limits. Capture is by body inspection; a fresh executable reproduction is still needed.

## Done means

- [ ] Add a reproduction for a rigid rotation of a point cloud and its normals.
- [ ] Define and implement the normal contract for each deformation overload.
- [ ] Correct the source comment and reference guide, and run the relevant law/test gate.
