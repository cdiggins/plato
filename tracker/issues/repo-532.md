---
id: repo-532
title: Studio frozen runtime tripwire differs from recorded baseline
type: problem
status: idea
priority: p2
effort: S
risk: med
area: repo
sprint: 
created: 2026-10-25
closed:
links: [docs/reference/implementation/verification.md]
---

The required studio battery run for docs-523 failed its checksum gate while its library lints,
SDK build and geometry tests passed. A targeted read-only hash check identifies drift in the
SDK's vendored frozen runtime files:

- ara3d-sdk/src/Plato.Intrinsics/PlatoList.cs
- ara3d-sdk/src/Plato.Intrinsics/PlatoBuffer.cs
- ara3d-sdk/src/Plato.Intrinsics/Number.cs

These live in C:/Users/cdigg/git/studio and were not modified by the reference mission. The
raw check output is retained at `.temp/reference-frozen-detail.log`; the battery log is
`.temp/reference-check-all.log`. The gate reports changed bytes rather than an execution defect.
Review the actual diffs in that checkout to determine whether the baseline or the artifacts
need correction. Do not automatically revert someone else's edits or regenerate the checksum
manifest to hide the failure.

This is a cross-checkout process problem, not evidence that the new Plato reference or its
forward library codegen changed a frozen artifact.
