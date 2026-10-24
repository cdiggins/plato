---
id: compiler-490
title: Reject ignored generation flags before compiling
type: bug
status: in-progress
priority: p2
effort: S
risk: low
area: compiler
sprint: 
created: 2026-10-24
closed:
links: []
---

## Problem

Generation ignores any unrecognized double-dash argument. The documented retired --methods flag still returns exit 0 and generates files; misspelled optimization flags likewise silently select a different recipe. Reproduced with the small Boolean fixture used by tools/check-cli-behavior.py. This violates the agent guide's hard-error contract for retired flags.

## Done means

- [x] Generation rejects unknown and retired flags before parsing input or writing output, preserving existing specific scalar/style diagnostics.
- [x] The executable CLI behavior check covers misspelled flags, retired flags and accepted generation options.

The fix is a small command-boundary validation change; language semantics and run/lint option policies are separate.

Verified by the recorded CLI build and tools/check-cli-behavior.py: generation succeeds with the supported options, rejects misspellings and retired switches before opening inputs, and retains the existing scalar/style error paths.
