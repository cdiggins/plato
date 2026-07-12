# CSG in Plato — findings

*2026-07-12. Companion to [`csg.plato`](csg.plato) and
[`Ara3D.Csg.Tests/`](Ara3D.Csg.Tests/). Sibling of the earcut port
([`../earcut/`](../earcut/)); same recipe, same spirit.*

## What this is

A functional re-expression of [evanw/csg.js](https://github.com/evanw/csg.js)
(constructive solid geometry — union, intersection, difference of polyhedra) as
a Plato library, compiled together with the full production standard library
(`plato-src`) into a standalone C# test library. NUnit tests assert the
one-dimension-up analogue of the earcut port's area invariant: the boolean of
two axis-aligned boxes is a closed solid whose enclosed volume is known
analytically and must match the volume the returned facet soup encloses
(divergence theorem over its fan-triangulated, outward faces).

```
csg/
  csg.plato            the library (~110 code lines incl. two types; 230 with comments)
  regen-csg.ps1        merge plato-src + csg.plato -> Generated\  (-Test runs suite)
  Ara3D.Csg.Tests/     NUnit project; Generated\ is emitted, not committed
    CsgTests.cs        13 volume tests + 1 explicit (documents a limitation)
    CsgSupport.cs      the same one-line Concatenate shim earcut needs
  FINDINGS.md          this file
```

Recipe: `--csharp-style=extensions --scalar=float --optimize --optimize-arrays
--methods` — identical to the earcut port. Result: **13/13 tests pass** — box
sanity checks, overlapping boxes (union/intersect/subtract, both operand
orders), fully disjoint boxes, a small box contained in a large one (including
the cubic-cavity subtraction), and a 45°-rotated intersection (an octagonal
prism, exercising oblique plane cuts). One further test is marked `Explicit`: it
documents the single geometric case this formulation does not resolve
(coplanar shared faces — below).

## The reformulation

csg.js is a mutable BSP tree. `union`/`subtract`/`intersect` each build two
trees and interleave four recursive traversals — `build`, `clipTo` (via
`clipPolygons`), `invert`, `allPolygons` — splitting polygons at node planes and
re-linking them in place. A BSP tree is a *recursive value* Plato cannot hold
(types are immutable and non-recursive), and those traversals are *recursive
functions* Plato does not have.

A BSP tree can be **emulated** without recursive types — flatten it to an
`IArray<Node>` where each node's front/back children are `Integer` indices
(−1 = null) — and each traversal rewritten as a bounded worklist fold over that
array, the shape the earcut port used for `ClipAll`. That is faithful but
reintroduces exactly the index bookkeeping earcut found it could *dissolve*.

So this port keeps only what the BSP *computes* and drops the tree. The insight:

> A boolean of A and B is assembled from pieces of A's faces and pieces of B's
> faces, and a piece belongs in the output only if it lies **wholly** inside or
> **wholly** outside the other solid — never straddling its surface.

csg.js guarantees the "wholly" by splitting polygons at every BSP plane. We
guarantee it directly, as two folds and a test:

1. **Fragment** — cut each face of A by *every* plane of B, one plane at a time
   (`FragmentFacet` = `planes.Reduce(f.Single, CutAll)`, `CutAll` a `FlatMap`
   of Sutherland-Hodgman half-space clips). After cutting by all of B's planes
   no fragment crosses a B plane, so none crosses B's boundary.
2. **Classify** — one even-odd ray-parity test per fragment centroid
   (`Inside`), deciding keep/drop.
3. **Assemble** — `Union`, `Intersect`, `Subtract` differ only in which side
   they keep and whether B's kept pieces are reversed:

   | op | A pieces kept | B pieces kept |
   |----|---------------|---------------|
   | `A \| B` | outside B | outside A |
   | `A & B` | inside B  | inside A  |
   | `A - B` | outside B | inside A, **reversed** |

Cost is O(k²) fragments per face cut by k planes (a plane-arrangement
subdivision of a convex region — *not* 2^k) and O(faces) per classification, so
a boolean is O(|A|²|B| + |B|²|A|). The explicit price of legibility, as with
earcut; a fast BSP version is a later pass.

### Simplifications relative to csg.js

| csg.js | this version |
|---|---|
| mutable BSP tree, four recursive traversals | two folds (`Fragment`, `Selected`) + one predicate (`Inside`) |
| `clipPolygons` recursion removes interior polygons | `Fragment` cuts blindly by all planes; `Inside` classifies |
| `invert()` flips solid/empty across the whole tree | `Reversed` on the ~half of facets that need it |
| `splitPolygon` with FRONT/BACK/COPLANAR/SPANNING bookkeeping | `SplitBy` = two half-space clips + a coplanar guard |
| vertex normals carried and interpolated | dropped — a facet's normal is its plane's (unused by the boolean) |

## The one genuine compromise: coplanar shared faces

When a face of A lies in the same plane as a face of B and the two overlap (two
boxes meeting exactly on `x = 1`), a fragment on that shared face has its
centroid **exactly on** the other solid's boundary plane, where the ray-parity
`Inside` test is ambiguous. `CoplanarSharedFaceUnion` records the symptom: the
union of two unit boxes sharing a face returns volume **14.667 instead of 16** —
the shared-face band (a 1⁄3-thick slab of the classification, 4⁄3 of volume) is
dropped. csg.js resolves this with its explicit COPLANAR case, orienting the
polygon by the dot product of the two normals; the fold formulation has no
comparable hook. Every green test deliberately offsets its operands so no faces
coincide. Fixing it cleanly would mean either a coplanar-aware classifier
(perturb the centroid off the boundary along the fragment normal and test which
side of B's coincident face it falls on) or the BSP's own normal-dot rule — both
are a natural increment for the performance pass.

A subtler instance of the same family *was* fixed during development: a facet
lying exactly **on a cutting plane** passed the `>= 0` half-space test on both
sides and was emitted twice, inflating disjoint-box volumes (union of two boxes
sharing y- and z-extents returned 26.667 instead of 16). The guard
`CoplanarWith` in `SplitBy` keeps such a facet once. The volume invariant caught
it in one line — the same verification payoff the earcut findings describe: a
pure function of the input makes a misclassification a value you can print,
not a mutation you must single-step.

## Toolchain findings

Filed because csg.plato is a fresh consumer alongside earcut.

1. **Heterogeneous `Reduce` now works.** `FragmentFacet` folds a `Plane` list
   into an `IArray<Facet>` accumulator — accumulator type ≠ element type,
   exactly the shape earcut's finding #1 reported the TIR body writer
   miscompiled (casting the accumulator to the element type). It emitted and
   ran correctly here. Either fixed since 2026-07-11 or narrower than described;
   worth a conformance law to pin down, and if solid it retires earcut's
   `ClipAll` dummy-fold wart.
2. **Generic library functions may not take a naked `$T` receiver.** `Single(x:
   $T): IArray<$T>` compiled fine until reification: *"First parameter type …
   not a concrete type or concept."* The first parameter must be a concrete
   type or an interface (earcut's `$T` helpers all had `IArray<$T>` receivers).
   Worked around with concrete `Single`/`KeepIf` overloads for `Facet` and
   `Point3D`; a generic `Single`/`Pure`/`KeepIf` in the stdlib (receiver
   `IAny`, or first-class `$T` receiver support) would serve every algorithm
   that builds 0-or-1-element lists inside a `FlatMap` to emulate `filter`.
3. **Zero-argument functions need `()` at declaration** (`RayDirection(): Vector3`,
   like `Epsilon()`), but are referenced bare (`RayDirection`, `Epsilon`). A
   property-style `Name: T => …` is a parse error.
4. **Same `Concatenate` gap as earcut** — declared in `intrinsics.plato`, absent
   from `Ara3D.Collections.LinqArray`; the identical one-line shim is duplicated
   in `CsgSupport.cs`. Adding `Concatenate` to `LinqArray` retires both.
5. **Worked without friction:** the two record types, `FlatMap`/`Reduce`/`All`/
   `Map` folds, `Point3D`↔`Vector3` conversions and operators (`Lerp`, `Cross`,
   `Dot`, `Normalize`, point−point, point+vector, vector·scalar), constructing
   `Triangle3D` to borrow its `Plane`, `Integer.Number` for the centroid divide,
   and multi-overload dispatch on a non-receiver parameter (`KeepIf`).

## Stdlib functions this algorithm wanted (a `Solids`/`Csg` library)

The predicates here are the 3D counterparts of the ones the earcut findings
asked to promote, and belong in the standard library the same way:

- `Facet`/`Solid` (or reuse of a mesh type) with `Plane`, `SignedDistance`,
  `Centroid`, convex `ContainsCoplanar`, and Sutherland-Hodgman
  `ClipToHalfSpace` — the foundation of *every* 3D solid-modeling algorithm.
- `Single`/`KeepIf` (or `filter`) — see toolchain #2.
- `Iterate(n, seed, f)` — still the missing primitive; `Fragment`'s inner fold
  is another "apply f once per element carrying data," which `Reduce` serves
  only because it now tolerates a heterogeneous accumulator.

## Follow-ups

- Resolve coplanar shared faces (centroid perturbation or the normal-dot rule),
  then flip `CoplanarSharedFaceUnion` from `Explicit` to a plain green test.
- Add the fast BSP pass: the flat-array node encoding + worklist-fold traversals
  described above reproduce csg.js's near-linear behavior without changing this
  file's role as the readable reference.
- Add `Concatenate` to `LinqArray`; delete `CsgSupport.cs` (shared with earcut).
- Add a conformance law exercising a heterogeneous `Reduce` (toolchain #1).
