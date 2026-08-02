---
id: stdlib-400
title: Corpus-floor assertion stale after the stdlib file consolidation
type: bug
status: ready
priority: p3
effort: S
risk: low
area: stdlib
sprint: 
created: 2026-08-02
closed:
links: []
---

## Symptom

`ForwardStdLibParsesAndCompiles` (`tests/PlatoTests/ForwardStdLibCheckerTests.cs`) fails:

```
Expected: greater than 300
But was:  172
```

## Cause

That assertion is a **corpus floor**: it fails when the file enumeration collapses, so that an
empty or top-directory-only scan cannot pass the suite by finding nothing to check. The floor was
chosen against a corpus of roughly four hundred files.

`50b0134 chore(stdlib): consolidate split files into stem files` merged the split declaration
files into stem files and took `stdlib` from 425 tracked `.plato` files to 168. The corpus did not
collapse — it was deliberately consolidated — but the floor was not moved with it, so the guard has
been failing on every run since.

## Fix

Lower the floor in the same spirit as a ratchet: pick a value comfortably below the real corpus
but far above what a broken enumeration would produce, and say in the assertion message that it
moved because of the consolidation. Do not delete the assertion — a collapsed enumeration passing
silently is the failure mode it exists to catch.

Note the count is also affected by `stdlib-398`: the forward law packet now sits at `stdlib/tests`,
inside the recursive scan, so it contributes files that were previously outside it. Settle that
issue's location question first, or the floor will need moving twice.

## Done means

- [ ] `ForwardStdLibParsesAndCompiles` passes, with a floor that still fails on an empty scan
- [ ] The assertion's message explains which change moved it
