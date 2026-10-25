---
id: compiler-520
title: Report useful grammar failure locations
type: bug
status: in-progress
priority: p1
effort: S
risk: med
area: compiler
sprint:
created: 2026-10-25
closed:
links: [../plato-brep/tickets/TKT-345-plato-parse-error-line.md]
---

## Problem

Parser backtracking can report the beginning of a file instead of the failing grammar location. Missing semicolons at lines 3 and 4 reproduce this in TKT-345. The CLI also discards useful parser messages.

## Approach

Track the furthest failure within each parse, isolate lookahead and recovery scans, and retry diagnostics only after an unsuccessful parse. Preserve exceptions and forward parser diagnostics through the CLI. Carry the verified nested Parakeet change through its Git pin; do not ship private test-build dependency substitutions.

## Done means

- [x] Missing-semicolon fixtures report their expected line 3/4 locations through Navigation and the cold CLI; valid input still passes.
- [x] Alternatives, lookahead, recovery, shared-input reuse, concurrency, and exception-preservation regressions pass.
- [ ] The fixed Parakeet commit is recorded and the actual normal Release compiler passes the location checks with the fixed parser assembly.

## Evidence

The private Debug red/green gate is preserved in plato-brep staging/next-ticket-readiness/parser345/RESULT-345D11.md. Its result receipt SHA256 is 8c9617dc3d7b3c9805f6b3e4f5c4ea69b40c5d46807e865da4816762dda68076: baseline has two intended failures and one passing control; candidate has ten passing tests and both cold CLI location checks pass. Shipping Release verification remains outstanding.

The carrying parser commit is 84be3f22c4d04175929ed41e211b4617519c05e3. The work commit carries only the CLI error forwarding, the two regression classes, the Navigation test friend grant, and that nested pin. Normal Release verification remains open.
