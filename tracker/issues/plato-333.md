---
id: plato-333
title: IntegerInterval is half-open but inherits closed IntervalLike bodies
type: bug
status: idea
priority: p2
effort: S
risk: med
area: plato
sprint: 
created: 2026-07-30
closed:
links: [submodules/Plato/stdlib/intervals.plato, submodules/Plato/stdlib/intervals-transforms-interval.library.plato, submodules/Plato/stdlib/spatial-trees.plato, submodules/Plato/stdlib/CONVENTIONS.md]
---

## Issue
`IntegerInterval` documents itself as half-open — "Contains Start, excludes End"
(`stdlib/intervals.plato:42`) — but it `implements IntervalLike<Integer>`, and every
derived body on that concept is written for a **closed** interval. `Contains` is
`self.Start <= value && value <= self.End`
(`stdlib/intervals-transforms-interval.library.plato:63-64`), so
`IntegerInterval(0, 4).Contains(4)` returns `true` while the type's own doc comment
says index 4 is excluded. `ContainsInterval`, `Overlaps`, and `Extent` inherit the
same closed reading. Unverified by execution — read from source; the bodies are
one-line expressions, so the semantics are unambiguous on inspection.

## Impact
Every consumer that stores an index range as `IntegerInterval` is exposed to a
silent off-by-one. `stdlib/spatial-trees.plato:12` states outright that BVH/KdTree
leaves "reference half-open IntegerInterval ranges", and four node types
(`spatial-trees.plato:36,58,82,105`) plus `spatial-kdtrees.plato:19,42` carry
`Items`/`Primitives` as `IntegerInterval`. Any traversal written against the
inherited `Contains`/`Overlaps` reads one primitive past the leaf. Also affects
`meshes-sections.plato:21` (`FaceRange`). No symptom reported yet — this is a
latent trap that will surface the first time someone writes generic traversal code
against the concept instead of hand-rolling the loop bounds.

## Affected code
- `submodules/Plato/stdlib/intervals.plato:42-48` — `IntegerInterval` declares
  half-open in its doc comment, implements `IntervalLike<Integer>`.
- `submodules/Plato/stdlib/intervals-transforms-interval.library.plato:63-75` —
  `Contains`/`ContainsInterval`/`Overlaps`, all closed-interval expressions.
- `submodules/Plato/stdlib/intervals-transforms-interval.library.plato:40-41` —
  `Extent` (raw `End - Start`); correct for half-open counts, off-by-one for closed.
  The two conventions disagree about which one is the bug.
- `submodules/Plato/stdlib/CONVENTIONS.md:108-117` — "Bounds — inclusive, with an
  empty encoding", with the escape hatch "inclusive on `Max` unless a doc comment
  says otherwise", and `IntegerInterval` named as an owner.
- `submodules/Plato/stdlib/spatial-trees.plato:12,36,58,82,105`,
  `spatial-kdtrees.plato:19,42`, `meshes-sections.plato:21` — consumers.

## Cause / analysis
The convention's own escape clause is the root cause. "Inclusive unless a doc
comment says otherwise" makes endpoint semantics a *per-type prose fact* while the
bodies that implement those semantics are shared *per-concept code*. A doc comment
cannot override an inherited expression, so the moment one type used the escape
hatch the concept and the type diverged with nothing to catch it. `AngleInterval`
hit the same seam and was handled correctly — it overrides `Contains` and `Span`
explicitly (`intervals-transforms-interval.library.plato:195-203`) with a comment
saying the generic linear versions are wrong for it. `IntegerInterval` took the
escape hatch without the matching overrides. Speculation: the half-open comment was
added later, to describe how callers were already using it, without checking what
the concept supplied.

## Priority
Recommend **P2**. Severity is high where it lands (a wrong `Contains` in a spatial
index is a wrong query result, not a crash), but current frequency is near zero:
verified by grep (2026-07-30) — no stdlib call site invokes `Contains`, `Overlaps`,
`ContainsInterval`, or `Extent` on any of the `Items`/`Primitives`/`FaceRange`
fields, so the wrong bodies are currently unreachable through the consumers. Cost of deferral grows
though — this is a foot-gun armed for whoever next writes generic code over
`IntervalLike`, and it gets more expensive to change as more types adopt the
concept. Not urgent; should not sit indefinitely.

## Dependencies
- Blocked by: nothing.
- Blocks: nothing filed. Would gate any work that adds generic traversal helpers
  over `IntervalLike` (none currently in the backlog).
- Touches: `intervals*.plato` and `intervals-transforms-*.library.plato` are broadly
  depended on; a semantics change ripples to spatial, mesh, and statistics code.
  Concurrent stdlib waves editing those files would collide.

## Fix approaches
1. **Make inclusion explicit in the type** — add a boundary-inclusion field or a
   distinct `IntegerRange` (half-open) alongside `IntegerInterval` (closed), and let
   the bodies branch or split. Most correct; largest blast radius; adds a field to a
   hot value type.
2. **Override on `IntegerInterval`** — supply half-open `Contains`/`Overlaps`/
   `ContainsInterval` the way `AngleInterval` already does. Small, local, follows an
   existing in-repo pattern (note the precedent is partial: `AngleInterval` overrides
   only `Contains` and `Span`, so the three-body override here goes further). Leaves
   the convention's escape hatch armed for the next type, and leaves `Clamp` and
   `Lerp(1)` still reaching the excluded `End` — acceptable for index ranges only if
   documented.
3. **Delete the escape hatch** — make `IntegerInterval` closed like everything else,
   fix the doc comment, and audit the seven consumer sites for the off-by-one that
   flips. Simplest to state, but it fights how index ranges actually want to be
   spelled (`Extent` = count only when half-open).

## Bedrock
The invariant that broke is: *a concept's derived bodies define the semantics of
every type that implements it; a doc comment cannot opt out.* The seam is
`intervals-bounds.concepts.plato` / `intervals-transforms-interval.library.plato` —
the boundary between what `IntervalLike` promises and what each type means. Option 2
patches the one type that violated it; option 1 strengthens the seam by making
endpoint inclusion something the type *states in data* rather than in prose, so the
generic bodies can be correct for every implementer instead of correct-by-default
and overridden-by-exception. Option 1 also removes the need for CONVENTIONS.md:111's
escape clause, which is the actual defect generator.

Verdict: **simplest-along-the-grain** — take option 2 now, but the override must NOT
be written as a bare special case. It must land with (a) the half-open contract
stated on `IntegerInterval` in the same voice `AngleInterval` uses, and (b) a note in
CONVENTIONS.md:108-117 that a type taking the "unless a doc comment says otherwise"
escape hatch is *required* to override the affected bodies. Without (b) the next type
repeats this, and option 1 stays reachable.

## Done means
- [ ] `IntegerInterval.Contains(End)` is `false`, matching its doc comment
- [ ] `ContainsInterval`, `Overlaps`, and `Extent` agree with the same half-open reading
- [ ] the seven consumer sites (`spatial-trees.plato`, `spatial-kdtrees.plato`,
      `meshes-sections.plato`) are checked against the corrected semantics
- [ ] CONVENTIONS.md states the override obligation for types using the escape hatch
- [ ] ForwardStdLib test green

## Simplest fix
Option 2: three override bodies on `IntegerInterval` in
`intervals-transforms-interval.library.plato`, mirroring the `AngleInterval` block at
lines 195-203. Gain: closes the contradiction in ~15 lines, follows a pattern already
in the file, no field added to a hot type, no consumer churn. Give up: the escape
hatch stays open, so the class of bug is not prevented — only this instance is
fixed. That is what the CONVENTIONS.md note in Bedrock (b) is for.

## Prevention
- **Test**: no test currently pins endpoint semantics for any `IntervalLike`
  implementer. A table test asserting `Contains` at both endpoints for
  `NumberInterval`/`AngleInterval`/`LengthInterval`/`IntegerInterval` would have
  caught this at the moment the half-open comment was written. The
  `IntegerInterval` case is the regression test and belongs in the fix; the
  all-implementers table is broader and worth its own issue.
- **Check**: a CHK-style rule — a type whose doc comment contradicts its concept's
  documented default must override the affected bodies — is enforceable only if the
  convention is machine-readable, which argues for option 1's data field.
- Related: same class as the existing CHK-rule work in the stdlib conformance suite;
  worth folding in there rather than inventing a new mechanism.
