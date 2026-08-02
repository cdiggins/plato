---
id: stdlib-400
title: Corpus-floor assertion stale after the stdlib file consolidation
type: bug
status: done
priority: p3
effort: S
risk: low
area: stdlib
sprint: 
created: 2026-08-02
closed: 2026-08-02
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

## Resolution (c42dea5)

The literal became the named constant `MinCorpusFiles`, whose doc comment says it is a COLLAPSE
GUARD rather than a file count, records that it last moved at the stem-file consolidation, and tells
the next reader not to re-pin it to the current corpus. The assertion message points at the constant.

`stdlib-398` was settled first, in the same commit: the corpus is now the four tier folders, so the
law packet at `stdlib/tests` does not contribute to the count and the floor will not need moving a
second time.

## Done means

- [x] `ForwardStdLibParsesAndCompiles` passes, with a floor that still fails on an empty scan
- [x] The assertion's message explains which change moved it
