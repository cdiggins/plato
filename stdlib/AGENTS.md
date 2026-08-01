# Agent rules for `stdlib/`

Folder-scoped rules for editing the forward standard library. The repo-wide guide is
[`../AGENTS.md`](../AGENTS.md); read this first when your change lands in this folder.

## Read before editing

- [`README.md`](README.md) — what the folder is, the tier partition, what `future` means
- [`CONVENTIONS.md`](CONVENTIONS.md) — what the vocabulary means (frames, winding, units)
- [`STYLE_GUIDE.md`](STYLE_GUIDE.md) — how to write declarations and bodies
- [`LIBRARIES.md`](LIBRARIES.md) — how `*.library.plato` files relate to declaration files
- [`VERIFICATION.md`](VERIFICATION.md) — which gate proves what, and the command that runs it

## Regenerate `types-and-concepts.txt` with every change

[`types-and-concepts.txt`](types-and-concepts.txt) is **generated** — one compressed declaration
per line for every type and concept in the three shipping tiers, sorted by name within a tier.
It exists so an agent can see what the library actually contains without opening every file, and
so a name it does not find there is known not to exist. That guarantee holds only while the file
matches the source.

**If your change adds, removes, or renames a type or a concept, or changes a member signature,
regenerate the file in the same commit:**

```bat
.\tools\export-types-context.bat
```

From the studio root: `.\submodules\Plato\tools\export-types-context.bat`.

Rules:

- **Never hand-edit it.** Edit the `.plato` source and re-run the tool.
- **Regenerating is not optional.** A stale index is worse than no index: a reader cannot tell a
  stale line from a fresh one, so one wrong entry discredits the file.
- The file is tracked, so an unregenerated change shows up as an uncommitted diff — but nothing
  fails the build. Checking is on you.
- `future/` is excluded on purpose: it is declared, not shipped. Do not add it.
