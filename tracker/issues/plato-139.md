---
id: plato-139
title: OKLab color space + perceptual mix in colors.plato
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-21
closed:
links: [tracker/issues/plato-134.md, docs/design/plato-kernel-libraries-sketch-2026-07-21.md, submodules/Plato/plato-src/colors.plato]
---

## Idea

Add OKLab (and OKLCH) conversion to colors.plato plus a perceptual
`Mix(a, b, t)` — better than RGB lerp for UI emphasis blends, gradients, and
heat-map ramps alike. colors.plato already carries LUV and other spaces, so
this is precedented content, not new architecture. Prerequisite for plato-134's
style.plato `ColorOps.Mix`/`Gradients.Eval` quality story. Capture only —
elaborate at promotion.
