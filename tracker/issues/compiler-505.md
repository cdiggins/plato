---
id: compiler-505
title: Create the output directory before writing C# docs
type: bug
status: idea
priority: "?"
effort: "?"
risk: "?"
area: compiler
sprint: 
created: 2026-10-24
closed:
links: []
---

## Problem

C# generation to a new --out directory throws DirectoryNotFoundException while
CSharpWriterExtensions.ToCSharp writes docs.html, before GeneratedFiles.Write creates
the output directory. Reproduced during compiler-504 CLI smoke testing; creating the
directory first makes the same command succeed. This affects ordinary generation too.

Reproduce with a valid input folder and --out pointing to a nonexistent folder.
The stack ends at CSharpWriterExtensions.ToCSharp's docs.html write. The small fix is
to establish the output directory before any artifact write and add a CLI regression.

## Done means

- [ ] A valid C# generation into a new directory succeeds and writes code and docs.html.
- [ ] An executable regression covers a previously nonexistent output directory.
