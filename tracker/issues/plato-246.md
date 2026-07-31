---
id: plato-246
title: BigInt (arbitrary-precision integer) type
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

Arbitrary-precision integer type. Needed as a building block for exact rationals ([[plato-244]]),
combinatorics, and hashing/ID arithmetic that overflows 64 bits. Backend mapping: `System.Numerics.BigInteger`
in C#; consider whether Plato's value-type/no-allocation posture permits it.
