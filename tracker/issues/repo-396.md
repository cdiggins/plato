---
id: repo-396
title: Retire bonepile All/Any LINQ shadows; guard Corpus roots
type: retire
status: done
priority: p2
effort: S
risk: low
area: repo
sprint: 
created: 2026-08-01
closed: 2026-08-01
links: [src/Plato.Intrinsics/bonepile/ArrayExtensions.cs, tests/Plato.Navigation.Tests/Corpus.cs, repo-392]
---

## Issue
Follow-up prevention from [repo-392]. Two leftover liabilities: (1) `bonepile/ArrayExtensions.cs` declared `All`/`Any` extensions on `IReadOnlyList<T>` returning Plato `Boolean`, in namespace `Ara3D.Geometry` — enclosing-namespace resolution made them shadow System.Linq for ALL code under `Ara3D.Geometry`, producing the nonsense NUnit failure `Expected: True  But was: True`. (2) Nothing asserted the Navigation corpus roots exist, so a moved folder surfaced as 25 cryptic shape failures instead of one clear message.

## Impact
Latent: any future code placed under `Ara3D.Geometry` calling LINQ `All`/`Any` on an `IReadOnlyList` silently binds to the Plato-Boolean versions. Corpus guard turns the next folder move into a single self-explaining failure.

## Affected code
- src/Plato.Intrinsics/bonepile/ArrayExtensions.cs:119-125 — the removed `All`/`Any` overloads.
- tests/Plato.Navigation.Tests/InvariantTests.cs — new `EveryCorpusRootExistsAndHasPlatoFiles` guard.

## Cause / analysis
Bonepile is a legacy dump; generated code carries its own `All`/`Any` (`Extensions.g.cs`), so the bonepile copies had no remaining in-solution callers — full-solution build stayed green after deletion.

## Priority
p2: latent footgun, no active breakage after repo-392; cheap to remove now before new callers appear.

## Bedrock
Deleting the shadows restores the invariant that LINQ names mean LINQ everywhere in the solution; the guard test moves corpus-shape failures to their root cause. Verdict: **right**.

## Done means
- [x] Bonepile `All`/`Any` removed; full solution builds.
- [x] Guard test asserts every `Corpus.Roots` folder exists and contains `.plato` files.
- [x] Navigation suite green (39/39) and Intrinsics suite green (174/174).
