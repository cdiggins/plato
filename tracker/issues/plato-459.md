---
id: plato-459
title: C# writer stubs bodiless functions three different ways
type: bug
status: ready
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-10-17
closed:
links: []
---

## Problem

A function declared without a body is emitted three different ways depending on its receiver: a library type gets a stub that throws NotImplementedException; an `Array<T>` receiver gets nothing, so the call fails with CS1061 (`FlatRecord` in the STEP design library); a scalar receiver gets a forwarder that calls itself and overflows the stack (`ParseStep`, `NameOctetsPrecede`).

## Done means

- [x] Every bodiless non-intrinsic function is emitted as the same stub, which throws NotImplementedException naming the function, whatever its receiver.
- [x] Intrinsic obligations (the host contract in intrinsics.library.plato) keep forwarding to the handwritten runtime.

## Notes

2026-10-18 (TKT-315, paused before landing): a first attempt keyed the host contract on the owning library's name (`library Intrinsics`) and emitted one stub, `throw new System.NotImplementedException("plato: <Name> is declared without a body")`, for every other bodiless function, including array and scalar receivers. It fixed the STEP library's three errors (FlatRecord, ParseStep, NameOctetsPrecede), but PlatoTests showed the library-name test is too narrow: `EmitSnapshotTests.V2MemberBodiesMatchBaseline` turned the forwarder `Boolean.And(this Boolean a, Boolean b) => a.And(b)` into a stub, and `ArrayReceiverLibraryEmissionTests.DeclaredOnlyArrayReceiverIsNotEmitted` pins the old array behaviour and would need restating. The patch is kept outside the repository (bim-open-toolkit session scratchpad, `number-profile/fix6-bodiless-stubs.patch`). Next step: find how the snapshot fixture declares its scalar intrinsics, and decide the host contract from that, not from a library name.
