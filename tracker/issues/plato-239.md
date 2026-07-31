---
id: plato-239
title: First-class Plato C++/CUDA writer
type: feature
status: done
priority: p2
effort: L
risk: med
area: plato
sprint: 
created: 2026-07-27
closed: 2026-07-27
links: [submodules/Plato/Plato.CppWriter, submodules/Plato/Plato.CppWriter/README.md, submodules/Plato/Plato.GlslWriter, submodules/Plato/Plato.CSharpWriter/TirInliner.cs, submodules/Plato/docs/plato-inlining-beta-reduction-plan-2026-07-12.md, plato-078, plato-024, plato-235]
---

## Issue
`Plato.CppWriter` is a TIR-only POC (`--cpp` / `--cuda`) moving toward first-class coverage.
M1–M5, residual functors, and M3/M4 (Character/String + Tier-1 fixed arrays) landed.
Dynamic `IArray` remains out of scope (documented).

## Decisions (binding, 2026-07-27)
1. **Keep identical C++/CUDA bodies** for now (`Dialects_Differ_Only_In_The_Preamble` stays).
2. **String and array = simple Plato data types**, built from first principles; only the most
   basic surface as intrinsics — **not** `std::string` / `std::vector` (M3/M4).
3. **Order:** M1+M2 ? M5 (`--inline`) ? residual closures as C++ functors ? M3/M4.

## Impact
Blocks using Plato-generated geometry as a C++/CUDA library. After M3/M4: demos **147/0**;
stdlib with `--inline` **1638 emitted / 1367 skipped** (was 1090 / 1507 post-functor).

## Affected code
- `submodules/Plato/Plato.CppWriter/CppPrelude.cs` — fixed `String` POD, `HashString`, `plato::Id`
- `submodules/Plato/Plato.CppWriter/CppWriter.cs` — Character/String/Angle; MapComponents templates
- `submodules/Plato/Plato.CppWriter/TirCppBodyWriter.cs` — string literals, Angle?float coerce

## Priority
p2: multi-target value is real (CUDA especially), but C# remains production.

## Dependencies
- Related: plato-078 (TS), plato-024 (Rust), plato-235 (GLSL Angle)
- Follow-on (optional): dynamic IArray without std::vector if a device-friendly design appears

## Bedrock
Strengthen the TIR-lowering seam (`TirInliner` / `TirRewrite`) so every non-closure backend
consumes the same specialized bodies. Do not bake `std::vector`/`std::string` into shared bodies.

## Done means
- [x] Docs mention C++/CUDA alongside TS/Rust/GLSL (POC-accurate)
- [x] String + Character as simple Plato data types (not std::string) — M3
- [x] Array story: simple Plato data type (not std::vector); Tier-1 fixed-size via Components + MapComponents — M4 (dynamic IArray deferred/documented)
- [x] Static `_` members emitted as free functions (type-tag param kept for overloads)
- [x] Straightforward reflection helpers emitted (Equals/NotEquals/GetHashCode; IArrayLike Components)
- [x] Intrinsics over user structs lowered (componentwise / convert-to-floatN)
- [x] Lambda plan executed (M5 `--inline`); `Plato.CppWriter.Tests` compile gates green
- [x] Residual closures as C++ functors (fixed-size Components / floatN); compile gates green

## Simplest next slice
Optional: device-friendly dynamic IArray (still not `std::vector`) if a design fits identical bodies.

## Prevention
- Keep `Dialects_Differ_Only_In_The_Preamble` as the tripwire
- Prefer sharing TirInliner across writers over per-backend lambda hacks
- nvcc test gate must load vcvars (cl.exe on PATH)
- Functors must stay device-friendly (no std::function / heap)
- String stays fixed-capacity POD; do not silently switch to heap strings
