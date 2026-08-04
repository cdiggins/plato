---
id: plato-445
title: Extend the stdlib comparison with OCCT, Wolfram, and Three.js
type: debt
status: in-progress
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-04
closed:
links: []
---

## Problem

The 2026-08-03 standard-library comparison omits three useful perspectives:
Open CASCADE Technology for industrial CAD/B-rep workflows, Wolfram Language for
symbolic-numeric regions, and Three.js for browser scene graphs and rendering.

## Scope

Publish a new dated revision, preserving the prior report as the frozen snapshot
required by the documentation conventions. Add the three comparators to the method,
comparison map, findings, and recommendations where they change the conclusions.

## Done means

- [ ] The revised report compares OCCT's CAD/B-rep, healing, and interchange model.
- [ ] It treats Wolfram Language as an adjacent integrated system rather than a
      like-for-like library.
- [ ] It compares Three.js with Plato's graphics and TypeScript/browser ambitions.
- [ ] Claims use current official primary sources and distinguish reported capability
      from measured behavior.
- [ ] The documentation index points readers to the new dated revision while retaining
      the earlier report.

## Verification

- Check every repository-local link from the revised report's directory.
- Open every new external citation from an official project source.
- Run `git diff --cached --check` before committing.
