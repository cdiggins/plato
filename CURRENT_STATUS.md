# Current status — 2026-07-31

A snapshot of where this repository stands, written while preparing it to live as a
standalone repo rather than a submodule of `studio`. Everything below was measured, not
recalled; the commands are given so the numbers can be re-checked as they move.

## Summary

The compiler and all five backend writers build and their tests pass. The two checked-in
*generated* libraries do not compile, and one runtime obligation test fails — both are the
same underlying gap, at different scales. The blocker for standalone use is not any of
that: it is 18 project references that reach out of this repo into a sibling checkout.

## Build

```
dotnet build Plato.sln
```

1,868 errors, **all of them in two projects**:

| Project | Errors |
|---|---|
| `Plato.Generated.Optimized` | 1,370 |
| `Plato.Generated.Unoptimized` | 498 |

Everything else in the 33-project solution builds clean: `Plato.Compiler`, `Plato.AST`,
`Plato.Parser`, `PlatoTypeInference`, all of `Plato.CSharpWriter` / `CppWriter` /
`GlslWriter` / `RustWriter` / `TypeScriptWriter`, `Plato.Navigation`, `Plato.CLI`,
`Plato.ContextExport`, `PlatoTests`, and the vendored `Ara3D.Parakeet.*` projects.

Error mix is dominated by `CS1061` (1,046) — a member the forward stdlib declares that the
handwritten runtime does not implement — followed by `CS0103` (328) and `CS1729` (250).
This is the forward-conformance gap, not a structural problem: the two Generated projects
are checked-in output of the forward stdlib, and they compile only once the stdlib and the
`Plato.Intrinsics.V2` runtime agree on the full member surface.

## Tests

`PlatoTests`: **189 passing, 1 failing.**

The failure is `IntrinsicObligationTests.ForwardStdLibIntrinsicObligationsAreDischargedOrPinned`.
The forward stdlib declares 13 members on `Number` with no counterpart in the V2 runtime:

```
Sin  Cos  Tan  Sinh  Cosh  Tanh
AsinRadians  AcosRadians  AtanRadians  Atan2Radians  AsinhRadians  AcoshRadians  AtanhRadians
```

That test exists precisely to catch this before it becomes a thousand `CS1061`s downstream,
so it is the small, readable version of the Generated-project failure above. Adding those 13
members to `Plato.Intrinsics.V2` is the single highest-value next change: it turns the suite
green and removes the ambiguity from every gate run that follows.

## Standalone blocker: references that leave the repo

Seven projects reference four projects in a *sibling* repository, by relative path:

```
..\..\..\..\ara3d-sdk\src\Ara3D.Collections
..\..\..\..\ara3d-sdk\src\Ara3D.Memory
..\..\..\..\ara3d-sdk\src\Ara3D.Utils
..\..\..\..\ara3d-sdk\src\Ara3D.Logging
```

18 `ProjectReference` entries in total, from:

- `Generated/Plato.Generated.Optimized`
- `Generated/Plato.Generated.Unoptimized`
- `conformance/Plato.ForwardConformanceTests`
- `csg/Ara3D.Csg.Tests`
- `earcut/Ara3D.Earcut.Tests`
- `parakeet/Parakeet` and `parakeet/Parakeet.Parsers` — note that **parakeet is itself a
  submodule of this repo**, so the coupling is two levels deep

`Plato.sln` also lists those four SDK projects as though they belonged to it.

The path `..\..\..\..\ara3d-sdk` resolves only when this repo sits at
`studio/submodules/Plato` with `studio/ara3d-sdk` checked out beside it. Cloned on its own,
seven projects fail to load. This is why building from a detached worktree fails with
missing `CstNode` / `ILocation` types.

Three ways out, each with a different cost:

1. **Consume the four as NuGet packages.** Cleanest boundary, but requires publishing and
   versioning them, and makes local SDK edits a package round-trip.
2. **Vendor the sources** into this repo (as parakeet already is). No publishing step, but
   two copies to keep in sync.
3. **Make `ara3d-sdk` a submodule of Plato.** Smallest diff, keeps one source of truth, but
   deepens the submodule nesting that already makes parakeet awkward.

Whichever is chosen also has to cover parakeet, which has the same dependency.

## Other couplings to `studio`

Smaller than the above, but they must be resolved for a clean split:

- **The frozen-V1 tripwire lives in the parent.** `studio/tools/check-frozen-v1.ps1` and
  `studio/tools/frozen-v1.sha256` pin file hashes under `submodules/Plato/Plato.Intrinsics.Legacy`.
  Either that machinery moves here, or the freeze retires.
- **Docs link upward.** `CLAUDE.md` and `AGENTS.md` point at `../../docs/working-on-plato.md`.
- **A second copy of the frozen runtime** lives at `ara3d-sdk/src/Plato.Intrinsics`, kept in
  sync by a script in the parent.

### A latent bug found while renaming (2026-07-31)

`.gitattributes` marks the frozen V1 files `-text`, meaning git checks the stored bytes out
verbatim. Those blobs held **LF**, while the working copies on disk — and therefore the
hashes recorded in `frozen-v1.sha256` — were **CRLF**. A fresh clone would have produced LF
files and failed the tripwire; the freeze only ever held on machines where the files had
already been converted in place.

Renaming `Plato.Intrinsics` to `Plato.Intrinsics.Legacy` re-committed the files with their
on-disk CRLF bytes, so blob, disk and manifest now agree and a fresh clone passes. The fix
was accidental. The same LF/CRLF split may still exist in the `ara3d-sdk` copy, which was
deliberately left untouched.

## Recent changes (2026-07-31)

| Commit | Change |
|---|---|
| `f8f66da` | `Plato.Intrinsics` → `Plato.Intrinsics.Legacy` — the frozen runtime now says so in its name. Importers, `.gitattributes`, the parent's tripwire paths and the docs all moved with it. |
| `e2f595a` | Removed the `Object` type and the `Dynamic` primitive from the stdlib. Neither was used anywhere in the stdlib, the tests or the generated output. `foundation/primitives.types.plato` is gone; it held `Object` alone. |
| `c5d15e6` | Deleted the abandoned V3 experiment — `Plato.Intrinsics.V3`, `Plato.Generated.V3`, its smoke tests: 1,171 files, 103,657 lines. The only surviving result of that mission is the `CSharpTypeWriter` At/Count emitter fix, already in main. |
| `110e3d7` | Swept the now-dead `Dynamic` entries from the TypeScript, Rust, C++ and GLSL writers. No `"Dynamic"` string literal remains in any `.cs` in the repo. |

`Dynamic` was removed from `WriterPrimitiveNames.All` as part of this. It cost every new
backend an answer to "what is my dynamic?" for a name no declaration could produce. The
comment in that file records how to put it back if interop ever needs the escape hatch.

## Suggested order of work

1. **Add the 13 `Number` trig members to V2.** Turns the test suite green; small and
   mechanical; every later gate becomes unambiguous.
2. **Decide and execute the `ara3d-sdk` split** (the three options above). This is the real
   standalone work, and it decides what "standalone" means for parakeet too.
3. **Move or retire the frozen-V1 tripwire**, and check the `ara3d-sdk` copy for the same
   LF/CRLF split.
4. **Close the forward-conformance gap** so the two Generated projects compile. Existing
   workstream; does not block a standalone split.
