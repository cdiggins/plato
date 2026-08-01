---
id: repo-392
title: Repoint Navigation test corpus at tests/stdlib-tests; unshadow LINQ in test namespace
type: bug
status: done
priority: p1
effort: S
risk: low
area: repo
sprint: 
created: 2026-08-01
closed: 2026-08-01
links: [tests/Plato.Navigation.Tests/Corpus.cs, src/Plato.Intrinsics/bonepile/ArrayExtensions.cs]
---

## Issue
`dotnet test tests/Plato.Navigation.Tests` failed 32/38 (25 + 7) on a clean checkout. Two independent causes:
1. `Corpus.cs` built its corpus roots from `<repo>/stdlib-tests`, but that folder moved to `tests/stdlib-tests`; the law/witness half of the corpus silently vanished, so 25 shape/coverage tests failed.
2. The remaining 7 failed with the nonsense NUnit message `Expected: True  But was: True`: the test namespace `Ara3D.Geometry.Navigation.Tests` is nested inside `Ara3D.Geometry`, where `src/Plato.Intrinsics/bonepile/ArrayExtensions.cs:120` declares `All`/`Any` extensions on `IReadOnlyList<T>` returning Plato `Boolean`. Enclosing-namespace extensions beat `System.Linq` from usings, so `Assert.That(xs.All(...), Is.True)` received a Plato `Boolean` (prints "True", is not `bool`) and failed.

## Impact
Whole Navigation gate red; any agent running the suite gets 32 spurious failures and no signal from real regressions. The shadowing trap bites ANY test/code that sits in the `Ara3D.Geometry` namespace tree and calls LINQ `All`/`Any` on an `IReadOnlyList`.

## Affected code
- tests/Plato.Navigation.Tests/Corpus.cs:19 — corpus root pointed at the moved folder.
- src/Plato.Intrinsics/bonepile/ArrayExtensions.cs:120 — LINQ-shadowing `All`/`Any` in namespace `Ara3D.Geometry`.
- tests/Plato.Navigation.Tests/*.cs — lived in `Ara3D.Geometry.Navigation.Tests`, inside the shadowed namespace.

## Cause / analysis
Cause 1: folder move (`stdlib-tests` -> `tests/stdlib-tests`) landed without updating the one hard-coded consumer. Cause 2: pre-existing footgun; C# resolves extension methods from enclosing namespaces before using-directives, so the bonepile extensions win over `System.Linq` for anything under `Ara3D.Geometry`.

## Priority
p1: gate suite fully red, blocks trust in every Navigation change; fix is small.

## Fix approaches
1. (taken) Repoint corpus root to `tests/stdlib-tests`; move tests to namespace `Plato.Navigation.Tests` (outside `Ara3D.Geometry`) with explicit `using Ara3D.Geometry.Navigation;`.
2. Alternative for cause 2: delete/rename the bonepile `All`/`Any` — larger blast radius (generated + intrinsics callers), left as prevention work.

## Bedrock
The namespace move fixes the invariant, not the symptom: test code no longer lives inside the namespace tree whose vocabulary is generated and unstable, so future generated extensions cannot silently rebind test LINQ calls. Verdict: **right**. The remaining footgun (bonepile `All`/`Any` shadowing LINQ for in-tree geometry code) is real but separate.

## Done means
- [x] `Corpus.Roots` includes `tests/stdlib-tests` and the folder exists.
- [x] Test namespace is outside `Ara3D.Geometry`; `All`/`Any` bind to System.Linq.
- [x] `dotnet test tests/Plato.Navigation.Tests` green: 38/38 passed.

## Prevention
- A test asserting every `Corpus.Roots` folder exists and is non-empty would have turned 25 shape failures into one clear message.
- Consider retiring `bonepile/ArrayExtensions.cs` `All`/`Any` overloads (shadow LINQ for the whole `Ara3D.Geometry` tree) — candidate for its own retire issue.
