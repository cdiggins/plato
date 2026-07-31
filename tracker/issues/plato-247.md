---
id: plato-247
title: Float8 type and other literal SIMD-width vector types
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-27
closed:
links: []
---

User idea, captured 2026-07-27.

Literal SIMD-width numeric vector types — `Float8` (AVX 256-bit), and the family around it
(`Float4`, `Float16`, `Int8`, ...) — mapping directly to hardware vector registers via
`System.Runtime.Intrinsics`. These are *numeric lanes*, not geometry; naming them plainly
depends on freeing the `VectorN` names, which is [[plato-241]].
