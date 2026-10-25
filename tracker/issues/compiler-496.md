---
id: compiler-496
title: FunctionInstance never assigns Interface so selection ignores interface distance
type: bug
status: in-progress
priority: p2
effort: S
risk: med
area: compiler
sprint: 
created: 2026-10-24
closed:
links: []
---

## Problem

FunctionInstance.cs declares a get-only Interface property but never assigns it. FunctionSelection.ScoreFunction (formerly copied into each writer's PlatoAnalyzer) treats Interface == null as concrete, returning -100 before examining OwnerType.DepthTo. A parsed Item implementing IChild inheriting IBase therefore gives both interface candidates -100, instead of using distance. Intrinsic precedence still works. This can make implementation selection depend on enumeration order rather than interface specificity.

Found while preserving behavior for repo-493; deliberately not fixed in that refactor. Constructor and selector source inspection plus the real-source FunctionSelectionTests fixture reproduce it. Correcting the constructor is small, but it changes selection across all writers and needs output review.

## Done means

- [x] Establish which FunctionInstance kinds should carry Interface and assign it from the correct declaration/implementation context.
- [x] Cover nearer and inherited interface candidates through normal construction, including ties and intrinsic/concrete precedence.
- [ ] Review changed output and run backend tests before accepting the semantic change; update the repo-493 preservation test accordingly.

## Implementation and focused verification

Bound interface declarations and implementations receive Interface from their existing InterfaceImplementation context. Concrete/field functions and standalone interface declarations/extensions have no binding and keep null. The old reflection-based test and preservation assertion are replaced by normal parsed-source construction.

Output review exposed two necessary companion changes. ReifiedType previously retained the first same-signature library body even when the writer selected a more-specific one, producing a C# throwing stub; it now retains the lower-scored body using FunctionSelection, keeping first arrival on equal scores. Dormant InterfaceName-only array exclusions were removed from every writer so restoring metadata cannot silently remove public members. These are not new allowlists: existing field/name/backend representability filters remain in place.

The red run reproduced three failures through normal construction. Final focused selection/monomorphization tests pass (20 tests, 22 s), including compiling and executing emitted C# to observe the interface result 22 and concrete override 33. Native tests pass (8 tests, 29 s, MSVC and nvcc). The comparison across C#, TypeScript, Rust, GLSL, C++ and CUDA changes exactly the fixture's selected result from 11 to 22 in each dialect; all 509 demo/legacy/foundation files are unchanged after normalizing only creation timestamps. No additional degraded bodies were introduced. Logs, output snapshots and comparison.json are under .temp/compiler-496.

The full PlatoTests gate is still running at this milestone. Studio integration passed both lints, the SDK build and geometry tests, but its frozen-artifact gate remains red for the same external PlatoList.cs, PlatoBuffer.cs and Number.cs files tracked in repo-473; those files and the manifest were not modified.

Root PROGRESS.md belongs to another mission. This issue and the explicit work commit serve as the checkpoint. Pre-existing dirty status JSON/HTML were backed up before running the recorders and will be restored after final verification.
