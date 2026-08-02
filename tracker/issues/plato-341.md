---
id: plato-341
title: Add first-class Byte type
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-284, submodules/Plato/stdlib/primitives.plato, submodules/Plato/stdlib/color.plato]
---

## Idea
Introduce a first-class `Byte` type in Plato (interpretation of truncated ask “Log Plato idea for a Byte,”). Today scalar primitives in `stdlib/primitives.plato` are only `Number` (64-bit float), `Integer` (signed whole, `Whole`+`Bitwise`), `Boolean`, `String`, `Character`, `Dynamic`, `Object` — no `Byte` / `UInt8` / octet exists (MCP + scoped search). `Color8` channels are plain `Integer` (`R,G,B,A`). This idea is to elevate/standardize an 8-bit unsigned value as a named primitive (or tightly constrained type) so color/image/interop APIs stop overloading unbounded `Integer` for 0–255 domains.

## Assumptions
- Call sites (color channels, packed buffers, interop) need a discoverable 0–255 vocabulary, not just convention on `Integer`.
- `Integer` stays the general signed whole; `Number` stays float; `Byte` does not replace either.
- Codegen/intrinsics can map `Byte` to host `byte`/`uint8` without breaking existing `Integer` emission (or the mapping is an explicit follow-on).
- plato-284’s opaque/RGBA split is orthogonal but would benefit if channels become `Byte`.

## Design decisions
- **Range / signedness** — unsigned 0–255 (`Byte` / `UInt8`) vs signed −128…127 (`SByte`). Unsigned matches color/octet use; signed needs a separate name if ever wanted.
- **Distinct type vs constrained Integer** — new intrinsic primitive (like `Integer`) vs `Integer` + library predicates/newtypes. Distinct type gives affordances and codegen mapping; constrained `Integer` is cheaper but weak at the type boundary.
- **Relation to Integer / Number** — `Byte` implements `Whole`/`Bitwise`? widening only to `Integer`? any path to `Number`? Prefer explicit conversions over silent promotion that blurs overflow.
- **Overflow** — wrap (mod 256), saturate/clamp, or trap/error on out-of-range construct and arithmetic. Color pipelines often want clamp; buffer/interop often want wrap; language default must pick one and document.
- **Color / channel use** — retarget `Color8` fields to `Byte` vs keep `Integer` and only use `Byte` at new APIs. Retarget is the dogfood win; keep-`Integer` delays payoff.
- **Naming** — `Byte` (familiar, C#-aligned) vs `UInt8` / `Octet` (width-explicit). Prefer `Byte` unless multi-width integers land soon.

## Related
- [plato-284](plato-284.md) — OpaqueColor8 / ColorWithAlpha8; still uses `Integer` channels; natural first consumer of `Byte`.
- [stdlib/primitives.plato](../../submodules/Plato/stdlib/primitives.plato) — compiler-assumed scalar primitives; likely home for `Byte`.
- [stdlib/color.plato](../../submodules/Plato/stdlib/color.plato) — `Color8 { R,G,B,A: Integer }` today.
- No tracker/DONE/decisions hit for a Plato `Byte`/`UInt8` type; “byte” elsewhere is golden byte-identity or GPU buffer bytes, not this type.

## Approaches
Short term: declare `type Byte implements Whole, Bitwise { }` (or equivalent) in `primitives.plato`; add explicit `ToInteger` / `FromInteger` (clamp or checked); optionally retarget `Color8` channels to `Byte`.
Long term: codegen maps to host `byte`; packed image/buffer APIs; possible siblings (`UInt16`, …) only if width vocabulary proves necessary.
Adjacent ideas worth their own issue: signed `SByte`; multi-width unsigned integers; clamp-vs-wrap policy as a reusable numeric interface.

## Bedrock
Strengthens the **primitive-scalar seam** in `stdlib/primitives.plato`: bounded octet values become a named intrinsic instead of an unenforced `Integer` convention, so `Color8` and future image/interop types can state channel width in the type. **Verdict: simplest-along-the-grain** — must NOT invent a full fixed-width integer lattice or change `Integer`/`Number` semantics; only add `Byte` + conversions (+ optional `Color8` field retarget).

## Done means
- [ ] `Byte` declared among compiler-known primitives (or documented non-intrinsic equivalent) with stated range 0–255
- [ ] Documented conversion policy to/from `Integer` (and overflow/out-of-range behavior)
- [ ] At least one stdlib consumer uses `Byte` (e.g. `Color8` channels) or an explicit decision not to yet
- [ ] `lint stdlib` still 0 parse / 0 symbol errors

## Simplest possible implementation
Add empty `type Byte implements Whole, Bitwise { }` next to `Integer` in `primitives.plato`; library helpers `ClampToByte(Integer)` / `ToInteger(Byte)`; leave `Color8` on `Integer` until a follow-up.
- Pros: tiny surface; establishes the name; no color/API churn yet.
- Cons: no dogfood until something uses it; overflow/arithmetic still undefined until helpers or intrinsics land.

## Case against
- **Integer is enough.** Channels already work as `Integer` with 0/255 literals; a new primitive is ceremony without runtime proof of bugs from out-of-range values.
- **Intrinsic tax.** Every new compiler-assumed name touches writers, intrinsics, docs, and teaching; a library newtype may be enough.
- **Overflow ambiguity.** Without a clear wrap vs clamp default, `Byte` arithmetic becomes a footgun worse than unbounded `Integer`.
- Verdict: **pursue** as a small primitive if color/image work (plato-284+) is imminent; otherwise **park** until a concrete consumer needs typed 0–255 at the API boundary.

## Note (2026-07-30)
User restated "Missing a byte type" in a batch of Plato ideas — already covered here; no duplicate filed.
