---
id: plato-330
title: Lint intrinsic obligations against the intrinsics API snapshot
type: debt
status: done
priority: p3
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-30
links: [tracker/issues/plato-308.md, submodules/Plato/conformance/Ara3D.SDK.ConformanceTests/intrinsics-api-snapshot.txt, submodules/Plato/PlatoCompiler/Analysis/Linter.cs]
---

## Issue

plato-308 Root 2 (`Angle.Compare/Hash`, matrix `ColumnCount/ElementAt`, `Quaternion.Lerp`) and the
`Number.Pi/Tau/E/Epsilon` gap were all the same shape: the stdlib declares an obligation the
handwritten `Plato.Intrinsics.V2` runtime is expected to discharge, the member doesn't exist, and
nothing notices until the generated C# fails to build. The linter can't see C# — but
`conformance/Ara3D.SDK.ConformanceTests/intrinsics-api-snapshot.txt` is a committed, test-maintained
inventory of the runtime's actual surface.

## Fix approach

A lint rule (or PlatoTests gate, following the checker-behind-tests convention) that cross-checks
obligations expected from the handwritten runtime — `_`-receiver intrinsics and members of
primitive-backed types (`CSharpWriter.PrimitiveTypes`) — against the snapshot, flagging declared
members with no runtime counterpart. Needs a mapping story for scalar erasure (Number→float etc.)
since the snapshot records erased signatures.

## Bedrock

Closes the stdlib-to-runtime seam (the other one is [plato-329](plato-329.md)). New vocabulary
that leans on a runtime member that doesn't exist gets caught at lint/test time, not 1244 files
later. Verdict: **right**.

## Done means

- [x] Rule/gate that would have flagged all six plato-308 Root 2 members and the four Number constants.
- [x] Zero false positives on the current stdlib + stdlib-legacy (or an explicit allowlist with rationale).

## Resolution (2026-07-30)

Gate shipped as `PlatoTests/IntrinsicObligationTests.cs` — better than the snapshot plan: since
plato-331 the V2 shared sources compile into PlatoTests, so the check is direct REFLECTION over the
actual runtime, and the "mapping story for scalar erasure" reduces to a receiver-type table
(Number intrinsics live as extensions on float, etc.). An obligation = a bodyless library function
(`FunctionType.Intrinsic`) whose receiver is a V2 primitive struct. A counterpart = a
method/property/field on the struct, a V2 extension method on the struct or its erased C# type, or
an operator-name (writer synthesizes `Add(this int a, int b) => ((Integer)a) + b` wrappers from
V2's operator overloads; a missing operator fails ONE wrapper emission, not a thousand call sites).

- Detection capability pinned by a synthetic corpus (`Number.Frobnicate987` flagged, `Abs` not) —
  the plato-308 Root 2 members exist in V2 now so cannot be flagged live, but Hash/Compare below
  are literally the same shape caught on the current stdlib.
- stdlib-legacy: 347 in-scope obligations, ZERO missing (assert-empty gate).
- forward stdlib: 197 in-scope obligations, 22 undischarged, pinned as `ForwardKnownMissing` — an
  exact two-way worklist (new gap fails; fixed gap fails until deleted). Nine already fail the
  forward-conformance C# build as CS1061 (Number.IsNaN/IsInfinite/IsFinite/ToInteger/ToNumber,
  Plane.Hash, Quaternion.Hash, Matrix3x2/4x4.Hash); thirteen are LATENT — declared but not yet
  called by any generated body, invisible to the C# build until first use (Boolean/Integer/Number
  Compare+Hash, Integer BitwiseNot/BitwiseXor/Max/Min/Repeat/ShiftLeft/ShiftRight/ToNumber). Note
  ShiftLeft/ShiftRight/BitwiseNot/BitwiseXor have no operator-name mapping in `Operators`, so the
  writer cannot synthesize them either — V2 members are the only fix.

Discharging the 22 (a V2 content increment) is follow-up work, not this gate.
