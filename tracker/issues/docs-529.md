---
id: docs-529
title: Interactive reference sandboxes and 2D/3D views
type: idea
status: idea
priority: p2
effort: L
risk: med
area: docs
sprint: 
created: 2026-10-25
closed:
links: [tools/reference-reader/README.md]
---

User direction captured during docs-523: keep the reference reader in React because it will
eventually contain execution sandboxes, 2D and 3D rendering, and interactive scientific examples.
The present reader displays Markdown and diagrams; it does not execute Plato or provide these
renderers. Design the execution boundary, embedding contract and numeric-profile reporting
before implementing sandboxes. Reuse source Markdown and page identities rather than copying
the reference into a separate application.
