# Ara3D.SDK.ConformanceTests.V2

Differential gate for the extension-style ("V2") Plato C# emitter (docs/plato-roadmap.md Phase 2.4): the exact test sources and KnownFailures.json of `..\Ara3D.SDK.ConformanceTests` (linked, never copied) run against `Generated\` output produced by `tools\regen-conformance-v2.ps1` with `--csharp-style=extensions` (C# 14 extension blocks, LangVersion 14). Expected result is identical to V1: 178 tests, 142 pass / 36 ignored-known / 0 fail — any delta means the V2 emitter changed semantics.
