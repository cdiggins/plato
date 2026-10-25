# Secondary optimizer TIR census — 2026-10-25

This report advances compiler-507 (temporary record elimination), compiler-508 (array facts),
compiler-510 (loop unswitching), and compiler-511 (grid sample reuse) from abstract models to
actual Plato parsing, ground typed IR, optimizer traversal, and C# body emission. It adds no
production rewrite and enables no option. The production fusion and exact-output work is independent.

## Reproduction and limits

Run `python tools/experiments/optimizer-exploration/secondary-census/probe.py` from the repo root.
The private probe references existing Release CLI assemblies; it never builds shared compiler projects.
It parses the foundation sources together with ten fixtures, obtains ground TIR, rejects unresolved
nodes, checks resolved node identities, and emits bodies with loops enabled and inline disabled/enabled.
It initializes the writer's extension plans through reflection because this isolated body-emission
probe bypasses `WriteAll`; that private harness dependency is not proposed production architecture.

Evidence is under `.temp/optimizer-exploration/secondary-census/tir-probe/`: `results.txt` contains raw
and lowered trees plus C# bodies, `build.log` and `build-status.json` record the private harness build,
`source-fingerprints.txt` covers every parsed source and probe input, and `fingerprints.txt` records
assembly SHA-256 values. The runner checks source stability and copied assembly equality after execution.
The compiler assembly fingerprint is `45050e9b4c54b6766e4c17dc3fc8d63c416fe14ea08cc5d3dd853eac7fb41e4f`;
the C# writer fingerprint is `65478ddf900cc929cac8391fd9a1bab90557eeef8486c857ca68b703f46813b3`.
These are the binaries tested, not a claim that concurrent working-source changes were built.

This is a fixture census, not a frequency measurement over all geometry. Generated C# bodies were
inspected, not compiled/executed in a standalone generated application. There are no runtime speedup
claims or new proofs of full semantic equivalence. The first harness attempt failed because extension
plans were uninitialized; the corrected harness explicitly performs that normal writer initialization.

## Findings and bounded next slices

| Idea | Actual observed shape | Admission/refusal and next bounded slice |
|---|---|---|
| Record elimination | `DirectProjection` is a property `TirCall` over a constructor `TirCall`, surviving both recipes; emitted C# is `new CensusPair(a, b).First`. | Match the actual getter through `Callee.OwnerType.Fields` and reference equality with `FieldDef.Function`, then confirm constructor identity, declaration order and exact types. Do not key on name or assume `TirNew`. A direct ordered-binding projection slice is reachable, but defer production until a surviving JIT cost or downstream enabling benefit is measured. |
| Array facts | `OwnedLength` lowers to a completed `TirLoweredLoop`, then a `TirLet` alias of `TirTempRef`, then an intrinsic Count of the local. C# still returns `xs.Count` through `IReadOnlyList<Integer>`. | A report-only producer/alias fact adapter is a concrete next slice. Reuse storage provenance rather than build a separate name registry. `ExternalLength` has only a parameter and no owned producer: refuse stable-storage claims. No count replacement may skip materialization. |
| Loop unswitch | `Invariant` retains a direct `TirConditional` in the MapRange lambda. Its raw condition references the exact outer Boolean parameter. Capture rewriting introduces a Boolean local before emission; the conditional remains inside the fill loop. | A narrow immutable-Boolean analyzer is reachable. Run before capture rewriting or follow resolved let aliases afterward. Keep setup/allocation and positive-count guard; split fill loops only. `Variant` has an executable comparison on the induction parameter and `FaultingCondition` invokes a delegate: both refuse. Production should follow a representative workload/JIT benefit check, not this synthetic survivor alone. |
| Grid reuse | `Grid` has two nested MapRange calls and four `TirInvoke` sample calls. Only the outer range becomes `TirLoweredLoop`; the inner MapRange remains a runtime call inside its callback under both recipes. | Refuse this candidate for production caching: there is no proved rectangular lowered schedule, callback totality or immutable host environment. Separate capture aliases also mean emitted local-name equality cannot identify the callback. Defer until those proofs exist; prefer the existing explicit library sampling work where applicable. |

`FaultingUnused` retains `b.Invoke(a)` in the second constructor argument even though only First is
read. Scalar replacement must evaluate that argument once in order; dropping it is a different,
unsafe dead-computation transformation. `EscapePair` returns the complete record and is outside the
initial direct-projection slice. Getter metadata is authoritative: `FunctionType.Field` alone is
insufficient because the symbol constructor classifies one-parameter concrete-type functions broadly.

`Neighbor` retains two lowered loops and `xs[i + 1]` in the second. Its independent interval facts
cannot prove the dynamic correlation between the first materialized length and the second loop bound.
Before rewriting, an adapter must preserve checked Integer-to-int narrowing before negative-count
normalization and prove subtraction/addition overflow boundaries. Do not substitute allocated capacity
for logical length or interpret this shape as an already-established bounds-elimination opportunity.

The Boolean wrapper's conversion to native bool reads its stored Value through ToSystem; no custom
callback executes for this specific parameter condition. That supports the leaf-condition rule, not
a rule that arbitrary coercions can be stripped. Existing generated setup still allocates one result
array before entering the loop; an unswitch implementation should keep that allocation shared.

Source inspection additionally confirms that `MarchingCubesLattice` in `stdlib/geometry/voxels.library.plato`
uses a flattened range with division/modulo and a FlatMap into cell processing, while
`MarchingCubesCornerValue` invokes a supplied callback. This is more complex than the planar fixture;
no claim is made that marching cubes was compiled by this probe or is admitted for automatic caching.

## Architectural decision

Keep these tracks behind analysis/proof boundaries until useful production candidates are verified.
The smallest reusable improvement is resolved producer and alias provenance shared by array facts,
concrete storage, and later invariant recognition. Avoid a general solver, duplicate registries, or
public switches for unmeasured no-op passes. Grid schedule and totality proofs are distinct work;
record replacement needs typed argument sequencing. These dependencies must not be hidden inside
C# text generation. No production flags are needed for this report-only probe; omitting its command
turns the whole experiment off.

## Checks actually run

| Check | Result | Duration |
|---|---|---|
| Private reference-only probe build | PASS, no compiler errors or warnings | 6.09 s reported by MSBuild |
| Real TIR identity/shape assertions, inline off/on | PASS for every fixture in both recipes; no unresolved nodes | Not separately timed |
| Body emission and input fingerprint stability | PASS | Not separately timed |
| Generated application runtime differential tests | Not run; no production transform exists on this track | — |
