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

- [ ] Every bodiless non-intrinsic function is emitted as the same stub, which throws NotImplementedException naming the function, whatever its receiver.
- [ ] Intrinsic obligations (the host contract in intrinsics.library.plato) keep forwarding to the handwritten runtime.
