# v3 runtime mission (2026-07-27) — DONE
- Created Plato.Intrinsics.V3/ (V2-seeded runtime + ConceptObligations.cs), Plato.Generated.V3/ (csproj + regenerated v3 output), Plato.Generated.V3.Tests/ (7 NUnit smoke tests, all pass).
- Emitter fix: CSharpTypeWriter At/Count forward to single Array-typed field (VectorN); goldens byte-identical.
- Standalone build errors 293 -> 244 -> 128 -> 0. Gates: PlatoTests 142/142, conformance 205/205, regen-generated 184/184 identical, frozen V1 clean. Draft msg: COMMIT_MSG.v3-runtime.txt. Not committed, not in sln.

# PROGRESS — C++ / CUDA writer (worktree `cpp-opencl-writer`) — DONE

`Plato.CppWriter` (one emitter, `--cpp` / `--cuda` dialects) + `Plato.CppWriter.Tests`.

- Writer modelled on `Plato.GlslWriter`; TIR-only. Emitted code is identical across the two
  dialects, only the preamble differs (`CppPrelude`); a test enforces that.
- Wired into `Plato.CLI` as `--cpp` / `--cuda`.
- Gates: 8/8 tests green. demos/plato-src 66 emitted / 0 skipped; plato-src 461 / 1722.
  Both compile clean with MSVC `/std:c++17 /W3`.
- CUDA gate runs in MSVC-shim mode (no CUDA Toolkit on this machine); auto-upgrades to nvcc.
- NOT done: neither project is registered in `Ara3D.Studio.sln` (that file is in the studio repo).
- Worktree needs junctions for the external project refs: `.claude/ara3d-sdk` and
  `<worktree>/parakeet` (git worktrees do not populate submodules).
