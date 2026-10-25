---
id: docs-523
title: Hierarchical language and standard-library reference with React reader
type: feature
status: in-progress
priority: p1
effort: L
risk: med
area: docs
sprint: 
created: 2026-10-25
closed:
links: []
---

Build the requested repository-owned reference for junior developers and scientific programmers.
Markdown is the durable source; a React reader assembles it with a dynamic tree, search, and deep links.
Cover the language, every forward stdlib source, the legacy boundary, C# mapping, related work,
formal notation, and explicit contradictions. Other writers are not semantic evidence.

## Done means

- [ ] Small hierarchical language chapters explain syntax, static and dynamic semantics, with source evidence, formal rules, and related work.
- [ ] Every forward stdlib source has a generated reference entry retaining its declarations, comments, bodies, tier, and unresolved obligations; domain guides teach conventions and use.
- [ ] A reproducible generator validates coverage, provenance, local links, and freshness and builds the React reader with navigation, search, and deep links.
- [ ] Reader production build, generator checks, and appropriate repository gates are run and limitations recorded; discrepancies remain explicit.

## Scope decisions

The reader is a local repository tool. Sandboxes and 2D/3D renderers are future extensions, not
implemented language backends. Source-derived appendices are evidence, not assertions that all
library algorithms are correct. Existing normative docs remain the authority; conflicting claims
are recorded with both sides rather than silently resolved.
