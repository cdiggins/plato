---
lesson: skeletal-animation
title: Skeletal Animation
domain: Animation & motion
v3-files: [38-skeletal-animation.plato]
audience: Comfortable with 3D poses (position + orientation) and hierarchical transforms.
status: draft-v1
---

# Skeletal Animation

A character mesh has thousands of vertices. Animating each vertex independently is
impossible to author and wasteful to store. Instead we animate a compact **skeleton** —
a hierarchy of bones — and let each vertex follow a weighted blend of nearby bones.
That pipeline is **skeletal animation**: bind a mesh to bones once, then drive poses
forever after.

## The idea

### Hierarchy and local pose

Each bone stores a **local** rigid pose relative to its parent (roots relative to the
model). Walking the parent chain composes those locals into **model-space** poses:

$$
M_i = M_{\mathrm{parent}(i)}\, L_i
$$

with $L_i$ the local pose and $M_i$ the model-space transform. Bones are stored in
topological order so one forward pass suffices.

```
        [hips]
        /    \
    [spine]  [leg_L]
       |        |
    [chest]  [shin_L]
       |
    [head]
```

### Bind pose and skinning

The **bind pose** is the rest articulation used when the mesh was skinned. For each bone
$i$ one stores the inverse of its model-space bind transform, $B_i^{-1}$. At runtime,
with current model-space bone matrix $M_i$, the skinning matrix is

$$
S_i = M_i\, B_i^{-1}.
$$

A vertex with bind position $p$ and weights $(w_j, i_j)$ deforms as

$$
p' = \sum_j w_j\, S_{i_j}\, p, \qquad \sum_j w_j = 1.
$$

This is **linear blend skinning** (LBS). It is simple and ubiquitous; it also collapses
volume under large twists — the classic candy-wrapper artifact on wrists and elbows.

### Dual-quaternion skinning (why it comes back)

Replacing the affine blend with a blend of **dual quaternions** (rigid motors) preserves
rigidity better under twist. Plato's transform vocabulary already has `Motor3D` for
rotation+translation as one algebraic object; skinning is one of the places that
representation earns its keep. The skeletal file today declares LBS-shaped data
(`AffineTransform3D` inverse binds) — dual-quaternion skinning is a semantic cousin,
not a second mesh format.

## In Plato

### Bones and skeleton

```
type BoneIndex { Value: Integer; }   // -1 = no bone

type Bone
{
    Name: String;
    Parent: BoneIndex;
    BindPose: Pose3D;                // local rest pose
}

type Skeleton
{
    Bones: Array<Bone>;              // Parent index < own index
}
```

`Pose3D` is position plus `Quaternion` orientation — rigid, no scale. Scale, when
needed for cartoon squash, lives on animation tracks (`TransformTrack3D.Scale`), not on
the bind bone itself in this vocabulary.

### Poses

```
type SkeletonPose
{
    LocalPoses: Array<Pose3D>;       // parallel to Skeleton.Bones
}
```

Sampling animation yields a `SkeletonPose`. Composing along parents yields model-space
poses for skinning and for attaching props.

### Skin binding

```
type SkinWeight { Bone: BoneIndex; Weight: Proportion; }

type VertexSkinWeights { Weights: Array<SkinWeight>; }

type SkinBinding
{
    Skeleton: Skeleton;
    InverseBindTransforms: Array<AffineTransform3D>;
    VertexWeights: Array<VertexSkinWeights>;
}
```

`InverseBindTransforms` is parallel to bones; `VertexWeights` is parallel to mesh
vertices. Renderers commonly keep the four largest weights per vertex.

Usage-shaped sketch:

```
binding = SkinBinding(skeleton, inverseBinds, vertexWeights)
pose    = SkeletonPose(localPoses)
// player forward-kinematics:
//   modelPose[i] = Compose(modelPose[parent], pose.LocalPoses[i])   // Pose3D.Compose exists
//   then convert each model pose to AffineTransform3D and Compose with InverseBindTransforms[i]
// v3 does not yet declare the pose→affine helper or SkinMatrices — see recommendations.
```

v3 declares the *data*; compose/skin functions are a later library pass — call out the
gap rather than inventing names.

### Morph targets

Facial and detail shapes often ride beside bones:

```
type MorphTarget
{
    Name: String;
    PositionDeltas: Array<Vector3D>;
    NormalDeltas: Array<Vector3D>;     // empty = positions only
}

type MorphWeights { Values: Array<Proportion>; }
```

### IK and constraints

```
type IkSolver = TwoBone | Ccd | Fabrik;

type IkChain
{
    BoneIndices: Array<BoneIndex>;     // root → tip
    Target: IkTarget;
    PoleTarget: Point3D;               // elbow/knee plane
    Solver: IkSolver;
    Iterations: Integer;
    Tolerance: Number;
}
```

`LookAtConstraint` aims a bone; `TwistConstraint` distributes twist (forearm/neck);
`BoneMask` scales layer influence per bone; `RetargetMap` maps source bones onto a
target skeleton; `BoneSocket` attaches props with a local `Pose3D` offset.

### Skeletal clips

```
type SkeletalClip
{
    Name: String;
    Duration: Duration;
    BoneTracks: Array<TransformTrack3D>;   // parallel to bones, local TRS
    Events: Array<AnimationEvent>;
}
```

Track values are parent-relative — the same space as `SkeletonPose.LocalPoses`.

## Pitfalls / fine print

**Local vs model space.** Painting weights or debugging skinning in the wrong space is
the number-one skeletal bug. Bind inverses are model-space; animation keys are local.

**-1 is "none".** `BoneIndex`, unmapped retarget entries, and missing parents use the
sentinel `-1`. Do not use nullable wrappers inconsistently with the rest of v3.

**Weight normalization.** Weights must sum to 1. Import pipelines that drop small
weights must renormalize or the mesh shrinks toward the origin.

**LBS candy wrapper.** Extreme bone twists with LBS lose volume. Dual-quaternion
skinning or twist bones (`TwistConstraint`) mitigate it; the vocabulary currently
expresses the constraint path more clearly than the DQS path.

**Topology order.** If a child's index precedes its parent, a single forward compose
pass fails. Enforce the invariant at skeleton build time.

**Scale in skinning.** Non-uniform scale in bone chains complicates inverse-transpose
rules for normals. Prefer uniform scale on characters, or skin normals with the
inverse-transpose of the 3×3 part of $S_i$.

## Try it

<details>
<summary>Exercise 1 — Parent index</summary>

Bone 0 is a root. Bone 1's parent is the root. What is `Bones[1].Parent.Value`?

**Answer.** `0`. Roots use `-1`; children store the parent's index.
</details>

<details>
<summary>Exercise 2 — Skinning matrix</summary>

In bind pose, current model matrix $M_i$ equals bind model matrix $B_i$. What is $S_i$?

**Answer.** $S_i = B_i B_i^{-1} = I$ — vertices stay put in bind pose, as required.
</details>

<details>
<summary>Exercise 3 — Morph exaggeration</summary>

`MorphWeights` docs allow values outside $[0,1]$. What does weight $2$ mean?

**Answer.** It exaggerates the target (double the authored delta). Negative weights invert
the delta.
</details>

## Library recommendations

- **missing-function** — `38-skeletal-animation.plato`: no declared
  `ModelPoses(skeleton, localPose) → Array<Pose3D>` or
  `SkinMatrices(binding, modelPoses) → Array<AffineTransform3D>`. The lesson's core
  formulas are universal; without named operations every consumer reimplements the
  forward kinematics loop.

- **missing-type** — `38-skeletal-animation.plato`: `SkinBinding` hard-wires LBS via
  `AffineTransform3D` inverse binds. A `SkinningMethod = LinearBlend | DualQuaternion`
  (and optional `Array<Motor3D>` inverse binds) would let the vocabulary express the
  rigidity-preserving path the dual-quaternion/`Motor3D` story prepares.

- **missing-function** — `38-skeletal-animation.plato`: `SkeletonPose` has no
  `Blend(a, b, t)` / `ApplyMask(pose, BoneMask)` declarations. Layered animation and
  upper-body masks are described by `BoneMask` but not operable as typed functions.

- **doc-comment** — `38-skeletal-animation.plato`: `Bone.BindPose` should state
  explicitly that it is *local* (parent-relative), matching `SkeletonPose`, and that
  model-space bind lives only as the inverse cache on `SkinBinding`. New readers
  routinely assume `BindPose` is already model-space.
