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
