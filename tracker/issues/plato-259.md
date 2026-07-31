---
id: plato-259
title: Motor (PGA rigid motion) as canonical for Pose3D, for correct screw interpolation
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: []
---

Proposed 2026-07-28 (agent idea; user asked for it to be captured after a discussion of geometric
algebra). Untriaged. Depends on the PGA machinery in [[plato-258]].

## The concrete defect

`plato-src-v3/13-transforms.plato` declares `Pose3D` as `Position: Point3D` plus
`Orientation: Quaternion`, implementing `Interpolatable`. On that shape, `Interpolatable` almost
certainly means "lerp the Position, slerp the Orientation, independently".

That is **not** the natural interpolation of a rigid motion. Blending rotation and translation
separately traces a different path than blending them together: the real rigid motion between two
poses is a *screw* — simultaneous rotation about an axis and translation along that same axis,
sweeping a helix. Separate interpolation sweeps something else. The difference shows up in
animation, camera paths, and IK.

A PGA **motor** (the rigid-motion element — rotation and translation as one object, 8 numbers)
interpolates correctly by construction: the natural interpolation of a motor *is* the screw motion.
Composition is one multiply, and the invariant survives renormalization better than a matrix does.

## Scope — deliberately narrow

This is **not** a proposal to change `Transform3D`. That type carries `Scale`, and its cousins
carry projective maps; motors can represent neither, so TRS/matrix stays canonical there. The
claim is only that `Pose3D` — rigid, no scale, already `Interpolatable` — is the one type where a
motor is the better canonical representation.

## Done means (draft, to firm up at triage)

- A demonstration that current `Pose3D` interpolation differs from screw interpolation, with
  numbers, **before** any code changes. If the difference turns out not to matter for realistic
  inputs, close this as dropped — that is a legitimate outcome.
- Motor type plus compose, apply-to-point, and interpolate, validated against quaternion+position
  composition at the endpoints (they must agree exactly at t=0 and t=1).
- A recorded decision: motor as the stored field, or motor as an interpolation-only intermediate
  that `Pose3D` converts through. The second is far cheaper and may be sufficient.

## Open questions

- Cost of apply-to-point via the motor sandwich vs. quaternion-rotate-plus-add, for bulk skinning.
- Interop: engines and file formats speak position+quaternion, so conversions are mandatory either
  way. Does the stored form actually matter, or only the interpolation path?
