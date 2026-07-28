---
lesson: scene-graph-hierarchy
title: Scene Graphs as Parent-Indexed Hierarchies
domain: Scenes & rendering
v3-files: [51-scene3d.plato, 43-scene2d.plato]
audience: Comfortable with transforms and arrays; may have used nested scene nodes in a game engine.
status: draft-v1
---

# Scene Graphs as Parent-Indexed Hierarchies

A scene is not only a pile of meshes. It is a **tree** (or forest) of nodes: a wheel
turns relative to a car; the car moves relative to the world. Plato stores that tree as a
**flat array** plus parent indices — the same pattern in 2D and 3D — and keeps meshes,
materials, and lights in side pools addressed by typed indices.

This lesson walks the hierarchy model, the $-1$ sentinel for roots, and how local
`Transform3D` / `Transform2D` compose toward world space.

## Flat array, parent pointer

Instead of nested objects with child lists, Plato uses:

```
Nodes: Array<SceneNode3D>
```

Each node stores `Parent: SceneNodeIndex3D`. Roots use parent value $-1`. Children are
found by scanning for matching parent indices (or by an auxiliary child list the host
builds).

```
 index:   0        1        2        3
        World     Car     WheelL   WheelR
        Parent=-1  Parent=0 Parent=1 Parent=1

        World
          └── Car
                ├── WheelL
                └── WheelR
```

| Benefit | Why flat + parent index |
|---------|-------------------------|
| Stable references | Indices survive as integers in files and GPU buffers |
| Simple serialization | One array, no pointer chasing |
| Pool-friendly | Meshes/materials live elsewhere; nodes stay small |

### Typed indices and sentinels

```plato
type SceneNodeIndex3D
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}
```

`-1` means "none" for a root's parent. The same sentinel pattern applies to optional
attachments (`Mesh`, `Material`, `Light`, `Camera`).

2D mirrors the idea:

```plato
type SceneNodeIndex2D
    implements Value, Hashable, Comparable, Index
{
    Value: Integer;
}
```

Never confuse a node index with a mesh index — different types, different arrays.

## In Plato — 3D nodes

```plato
type SceneNode3D
    implements Value
{
    Name: String;
    Parent: SceneNodeIndex3D;
    Transform: Transform3D;
    Visible: Boolean;
    Mesh: MeshIndex;
    Material: MaterialIndex;
    Light: LightBindingIndex;
    Camera: CameraBindingIndex;
    LayerMask: Integer;
}
```

`Transform` is **relative to the parent**. World transform is the composition of
ancestors' transforms from root down to the node.

```
world(Car)    = Transform(World) ∘ ... wait: World is root
world(WheelL) = world(Car) composed with WheelL.Transform
```

With TRS stored on `Transform3D`, composition follows the scene-graph convention used by
the transforms library: apply the child's local transform in the parent's space.

Attachments are optional indices into `SceneResources`:

```plato
type SceneResources
    implements Value
{
    Meshes: Array<TriangleMesh3D>;
    Materials: Array<Material>;
    Lights: SceneLightSet;
    LightBindings: Array<SceneLightBinding>;
    Cameras: SceneCameraSet;
    CameraBindings: Array<SceneCameraBinding>;
}

type Scene3D
    implements Value
{
    Name: String;
    Nodes: Array<SceneNode3D>;
    Resources: SceneResources;
    Layers: Array<RenderLayer>;
    Instances: Array<InstanceSet>;
    LodGroups: Array<LodGroup>;
}
```

A node may carry a mesh, a light, a camera, several of these, or none (pure grouping /
transform joint). `Material` on the node overrides the mesh default when not $-1$.

### Visibility

`Visible: Boolean` is the simple flag. Pipelines that need inheritance use:

```plato
type Visibility = Inherited | Visible | Hidden;
```

(`Visibility` is declared beside the 3D scene types; `SceneNode3D` itself exposes the
boolean — hosts may layer the tri-state in an extension record.)

`LayerMask` bits select `RenderLayer` membership for selective rendering and lighting.

### Instancing beside the graph

Not everything must be a node:

```plato
type InstanceSet
    implements Value
{
    Mesh: MeshIndex;
    Material: MaterialIndex;
    Transforms: Array<Transform3D>;
    Colors: Array<Color>;
}
```

`InstanceSet` stores **world-space** transforms for many copies — a side channel for
hardware instancing, not parented leaves. Use the hierarchy for articulated structure;
use instances for forests of identical props.

## In Plato — 2D parallel

```plato
type SceneNode2D
    implements Value
{
    Name: String;
    Parent: SceneNodeIndex2D;
    Transform: Transform2D;
    Opacity: Proportion;
    Blend: BlendMode;
    Visible: Boolean;
    Content: NodeContent2D;
    ClipIndex: ItemIndex;
}

type NodeContent2D
    = None
    | Shape(Index: ItemIndex)
    | Text(Index: ItemIndex)
    | Image(Index: ItemIndex)
    | Group;

type Scene2D
    implements Value
{
    Nodes: Array<SceneNode2D>;
    Shapes: Array<StyledPath2D>;
    Texts: Array<TextLayout>;
    ImageReferences: Array<Integer>;
    Clips: Array<ClipMask2D>;
}
```

Same hierarchy mechanics; content is a sum type into parallel pools instead of mesh/
material/light slots. `Group` isolates compositing; `ClipIndex` references `Clips`.

Hit-testing returns a node index and local point:

```plato
type SceneHitResult2D
    implements Value
{
    Node: SceneNodeIndex2D;
    LocalPosition: Point2D;
}
```

### Worked sketch — build a tiny 3D forest

```
// Pseudocode-shaped construction
var nodes = Array(
    SceneNode3D("root", SceneNodeIndex3D(-1), Transform3D.Identity, true,
                MeshIndex(-1), MaterialIndex(-1), ...),
    SceneNode3D("body", SceneNodeIndex3D(0), bodyLocal, true,
                MeshIndex(0), MaterialIndex(0), ...),
    SceneNode3D("door", SceneNodeIndex3D(1), doorLocal, true,
                MeshIndex(1), MaterialIndex(0), ...)
);
var scene = Scene3D("car", nodes, resources, layers, instances, lods);
```

Animating the door rotates `nodes[2].Transform` only; the body and root stay put. Moving
the body carries the door automatically when world matrices are rebuilt from parents.

## Pitfalls and fine print

**Dangling parents.** Parent index must be in range or $-1$. Cycles (A parent of B parent
of A) break world-transform walks — validate if data is user-edited.

**Index mixups.** `MeshIndex(3)` is not node 3. Typed wrappers exist so you do not pass
the wrong integer into the wrong pool — use them.

**World vs local instances.** `MeshInstance` / `InstanceSet` transforms are world space;
`SceneNode3D.Transform` is parent-relative. Do not parent an instance array.

**Opacity vs visibility (2D).** A node can be `Visible` yet `Opacity = 0`. Hit-test and
render policies may disagree — define both.

**Resource lifetime.** Deleting mesh 0 without fixing node references leaves stale
indices. Compacting pools requires remapping.

## Try it

<details>
<summary>Exercise 1 — Root sentinel</summary>

A node's `Parent.Value` is `-1`. Is it a root? Can it still have a mesh?

**Answer.** Yes, it is a root. Yes — attachments are independent of parenting.
</details>

<details>
<summary>Exercise 2 — Move a subtree</summary>

Wheel nodes parent to Car. You translate only Car's local transform. Do wheels move in
world space?

**Answer.** Yes — their world transforms recompose through Car.
</details>

<details>
<summary>Exercise 3 — 2D content</summary>

A `SceneNode2D` has `Content = Group` and `Parent = -1`. What does it draw by itself?

**Answer.** Nothing of its own — it groups children and may isolate compositing; shapes
live on descendant nodes with `Shape(...)` content.
</details>

## Library recommendations

- **missing-function** — `51-scene3d.plato`: no declared `WorldTransform(scene, node)` or
  `Children(scene, parent)`. Every host reimplements the walk; naming it locks
  composition order against `Transform3D` conventions.

- **missing-function** — no cycle check or `ValidateHierarchy(scene)`. Flat arrays make
  cycles easy to author by mistake; a pure validator belongs beside the types.

- **doc-comment** — `SceneNode3D` should restate that `Transform` is parent-relative and
  that `InstanceSet` transforms are world-space, in one place — the distinction is easy
  to miss across sections.

- **pedagogy** — 2D and 3D share the parent-index pattern but diverge on content
  (`NodeContent2D` sum vs parallel resource slots). A banner cross-note in both files
  ("same hierarchy mechanics") would help authors port mental models without implying
  type compatibility.
