---
id: plato-235
title: GLSL overload erasure picks the winner by emission order, not by intent
type: problem
status: idea
priority: p3
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-27
closed:
links: [submodules/Plato/Plato.GlslWriter/GlslWriter.cs, submodules/Plato/plato-src/curves.plato, submodules/Plato/plato-src/angles.plato, plato-234]
---

## Issue
GLSL has no newtypes, so distinct Plato types can erase to one GLSL type (`Angle` and
`Number` both become `float`, plato-234). Two Plato overloads then produce one GLSL
signature. `TryEmitFunction` resolves this first-wins by emission order — an arbitrary
order, not a decision. On the full stdlib this drops 75 functions; 3 of them
(`Angle.Turns`, `Angle.Degrees`, `Angle.Gradians`) change the meaning of a name that
survives. There is no diagnostic beyond the trailing `// Skipped` block, and callers of the
generated library have no way to tell which semantics they got.

Open question: what *should* the writer do when two Plato overloads erase to one GLSL
signature?

## Impact
Silent semantic substitution in generated shader code. Concretely:

- `Turns(x)` in the emitted GLSL means `Number -> Angle` (`x * 2π`). Plato code that meant
  `Angle -> Number` (`radians / 2π`) gets the inverse. Same for `Degrees`, `Gradians`.
- `Eval(Circle, float)` means the `Angle` (radians) parameterization. Plato code written
  against `Eval(curve: IAngularCurve2D, t: Number)` — t in *turns*, `plato-src/curves.plato:158`
  — is off by a factor of 2π.

Today the blast radius is contained: the stdlib GLSL output is not compiled into anything
shipping, and the demos do not use these overloads. It grows the moment someone targets the
full stdlib from a real shader.

## Affected code
- `submodules/Plato/Plato.GlslWriter/GlslWriter.cs` — `TryEmitFunction`, the
  `_claimedSignatures.Add(sigKey)` guard: records the drop (as of plato-234) but does not
  choose between candidates.
- `submodules/Plato/plato-src/curves.plato:152` / `:158` — `IAngularCurve2D` declares
  `Eval(Self, t: Angle)` and the library adds `Eval(IAngularCurve2D, t: Number) => curve.Eval(t.Turns)`.
- `submodules/Plato/plato-src/curves.plato:533` / `:538` — the 3D twin.
- `submodules/Plato/plato-src/angles.plato` — `Turns`/`Degrees`/`Gradians` in both directions.

## Cause / analysis
The writer's signature key is the *GLSL* signature (`Name(glslParamTypes)`), which is
correct for detecting the clash but throws away the information needed to resolve it: the
Plato parameter types are still available via `PlatoTypeName(arg.Type)` at both declaration
and call site. So a mangling or preference scheme is *possible*; none was designed, because
before plato-234 no collision existed (measured: zero duplicate drops with `Angle` unmapped).

Worth recording: for the 20 curve `Eval` collisions the arbitrary winner happened to be the
only legal one. The loser, `Eval(curve, Turns(t))`, would have emitted as self-recursive
after erasure — and GLSL forbids recursion, which this writer does not detect (its own class
doc says so). A different emission order would have produced 20 shaders that fail to compile.
Relying on luck here is the real defect.

## Priority
p3. No current consumer is harmed and plato-234 made the casualties visible in the output,
which converts a silent wrong answer into a documented one. It becomes p2 the moment the
full-stdlib GLSL is used as a real shader library, and p1 if the emission order ever shifts
such that a self-recursive `Eval` wins.

## Dependencies
- Blocked by: nothing — plato-234 landed the erasure and the skip record.
- Blocks: any "compile the full stdlib GLSL in CI" gate (a recursive winner would fail it
  for reasons unrelated to the gate's purpose).
- Touches: `Plato.GlslWriter` only.

## Candidate answers
1. **Deterministic preference, no name change.** Rank candidates (e.g. prefer the overload
   whose Plato parameter types are most specific / declared on the concrete type over the
   interface adapter) and emit that one; keep recording the loser. Cheap; keeps output
   names stable; still one semantics per name.
2. **Mangle on collision.** Emit the loser as `Eval_Angle(...)` / `Turns_Angle(...)`, keyed
   by the Plato parameter type names, and resolve call sites the same way (`TirCall` args
   carry their Plato types). No semantics lost. Costs a name-mangling scheme in both the
   declaration and call-site paths, and uglier output for a case that is rare.
3. **Refuse to erase.** Skip both members of a colliding pair rather than pick. Safest, but
   gives back a slice of what plato-234 won.
4. **Fix it upstream in `plato-src`.** Rename one direction (`ToTurns`/`FromTurns`) so no
   overload pair depends on the `Angle`/`Number` distinction. Helps every backend, not just
   GLSL — but changes the public stdlib surface.

Closing this should produce an ADR plus a follow-up issue for whichever answer wins.

## Bedrock
The invariant to establish: **a name in the emitted GLSL has exactly one meaning, and that
meaning is chosen, not raced.** Whatever the answer, the writer must stop treating
"signature already claimed" as a no-op — that is the seam where an unresolvable ambiguity
becomes an unnoticed substitution. Strengthening it makes every future erased type (`Time`,
`Ratio`, any fieldless measure) safe by construction rather than by re-auditing the skip
block. Verdict: **right** — the cheap fix (option 1) still leaves a name with two Plato
meanings, so it is a mitigation, not a resolution.

## Done means
- [ ] ADR recorded stating how colliding overloads are resolved
- [ ] No emitted GLSL function can be self-recursive as a result of erasure (detect and skip,
      or make it unreachable by construction)
- [ ] Full-stdlib output has either zero `overload erases` entries or every entry is a
      deliberate, documented choice
- [ ] Follow-up issue filed for the chosen implementation

## Prevention
- Recursion detection in the writer would have turned the latent 20-shader failure into a
  skip. Worth its own issue regardless of how this question is answered.
- A "compile the emitted stdlib GLSL with a validator in CI" gate would catch both this and
  the dangling-identifier class noted in plato-234.
