---
id: plato-263
title: Rename plato-src and plato-src-v3 to clearer stdlib names
type: idea
status: done
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-28
closed: 2026-07-28
links: [submodules/Plato/plato-src, submodules/Plato/plato-src-v3, plato-230, plato-228, plato-261]
---

> **Plan (2026-07-28):** approved execution plan at
> [submodules/Plato/docs/archive/plato-263-stdlib-rename-plan-2026-07-28.md](../../submodules/Plato/docs/archive/plato-263-stdlib-rename-plan-2026-07-28.md)
> — names locked: `stdlib` ← plato-src-v3, `stdlib-legacy` ← plato-src,
> `stdlib-legacy-tests` ← plato-test-src, `stdlib-snapshot-2026-07-09` ← plato-src-legacy;
> includes v3 number-prefix removal (`domain.plato` / `domain.concepts.plato` / `domain.library.plato`).

## Idea

`plato-src` and `plato-src-v3` are two generations of the Plato standard library that share one language. The folder names obscure that: "plato-src" reads as *the* Plato source tree, and "v3" reads as a version bump of the same library rather than a parallel vocabulary (concept-based, declaration-heavy) that is the forward direction. Rename so the role (stdlib) and status (current vs legacy) are obvious at a glance — e.g. `stdlib` / `stdlib-legacy`, or a clearer pair if brainstorming finds one.

## Assumptions

- `plato-src-v3` remains the forward library; `plato-src` remains the shipping/executable generation for the foreseeable future (bodies, codegen consumers, nav gates).
- Agents and humans regularly confuse which tree is authoritative for new vocabulary vs. what still drives `Plato.Generated` / Studio.
- A rename is mostly mechanical (paths, defaults, docs, discovery roots) but touches many references across Plato submodule + studio tracker/docs.
- No ADR currently locks the folder names.

## Design decisions

- **Name pair** — status-based (`stdlib` / `stdlib-legacy`) vs recipe-based (`stdlib-concepts` / `stdlib-interfaces`) vs keep-`plato-` prefix (`plato-stdlib` / `plato-stdlib-legacy`). Status-based matches "forward direction"; recipe-based stays accurate if both remain actively maintained; prefix helps monorepo grep.
- **Which folder gets the short name** — give `stdlib` to v3 (forward vocabulary) vs to current `plato-src` (what still ships). Wrong choice amplifies confusion during the dual-library era.
- **Transition** — hard rename in one commit vs temporary aliases/symlinks vs config-only display names with folders unchanged. Hard rename is clearest; aliases reduce breakage for external clones/scripts.
- **Test / nav roots** — whether `plato-test-src` becomes `stdlib-tests` (or stays) so the trio reads as one family.

## Related

- [plato-230](plato-230.md) — created `plato-src-v3`; naming debt left in place.
- [plato-228](plato-228.md) — `plato-src-v2` prototype (same versioning-as-folder smell).
- [plato-261](plato-261.md) — language reference notes two stdlib generations share one language; clearer folder names would help that doc.
- [plato-256](plato-256.md) — nav index over both roots; rename doesn't fix collisions but makes dual-root configs self-explanatory.
- [docs/archive/plato-roadmap.md](../../docs/archive/plato-roadmap.md) — earlier (superseded) plan already floated `plato-src-legacy`.
- [submodules/Plato/Plato.Navigation/README.md](../../submodules/Plato/Plato.Navigation/README.md) — defaults/docs hardcode `plato-src` / `plato-src-v3`.

## Approaches

Short term:
1. Pick the name pair in an ADR-scale note on this issue, then one mechanical rename commit in the Plato submodule + a studio-side path sweep (tracker links, docs, vscode defaults).
2. Until rename: document the mapping once in `submodules/Plato/CLAUDE.md` / `docs/plato-for-agents.md` ("v3 = forward stdlib vocabulary; plato-src = shipping stdlib").

Long term: single `stdlib` folder once the legacy generation is retired or folded; drop the `-legacy` suffix then.

Adjacent ideas:
- Retire / freeze policy for `plato-src` once v3 gains bodies (separate from rename).
- Align generated output project names (`Plato.Generated`) with the same vocabulary.

## Case against

- **Churn vs clarity.** Dozens of scripts, READMEs, tracker links, nav defaults, and muscle memory break for a cosmetic win while both libraries still coexist under *some* pair of names.
- **Wrong short name is worse.** Calling v3 `stdlib` while codegen still consumes `plato-src` may convince agents that v3 is what ships — already a live confusion mode.
- **Recipe names age better than "legacy".** If both trees stay live for a long dual period, `legacy` becomes a lie; `concepts` / `interfaces` (or similar) describe the real difference.
- **Docs-only might be enough.** A one-paragraph agent guide + README banners could remove most misfires without a rename.

**Verdict: pursue** — folder names are the highest-leverage signal for agents and new contributors, and the current `*-v3` scheme actively misleads. Park only if a rename lands in the same window as retiring one tree (then name once). Prefer a pair that survives the dual-library era (`stdlib` + `stdlib-legacy` *or* recipe-based), with an explicit decision on which tree owns the bare `stdlib` name.

## Bedrock

Strengthens the **stdlib identity seam** at `submodules/Plato/` directory roots and discovery defaults (`plato.navigation.roots`, Navigation README/CLI examples): one obvious "current vocabulary" root and one obvious "shipping/legacy" root. Makes future dual-root tooling and "which library do I edit?" cheaper. **Verdict: simplest-along-the-grain** — rename folders + update references; must NOT merge the libraries, change recipes, or retarget codegen consumers in the same change.

## Names shipped

stdlib ← plato-src-v3; stdlib-legacy ← plato-src; stdlib-legacy-tests ← plato-test-src; stdlib-snapshot-2026-07-09 ← plato-src-legacy. V3 de-numbering shipped in the same change.

## Done means

- [x] Chosen name pair recorded (and rationale) on this issue or a short ADR
- [x] Folders renamed under `submodules/Plato/`; all in-repo path references updated
- [x] Navigation / vscode / CLI defaults discover the new roots
- [x] Agent guides (`CLAUDE.md`, `plato-for-agents.md`) state the mapping in one place
- [x] `lint` / nav gates still pass against the renamed trees

## Simplest possible implementation

Pick `stdlib` ← `plato-src-v3` and `stdlib-legacy` ← `plato-src` (or the reverse if shipping clarity wins), `git mv`, fix references, update docs.

Pros:
- Instant orientation for humans and agents
- Matches prior roadmap instinct (`plato-src-legacy`)
- No semantic/compiler change

Cons:
- Wide mechanical churn; external scripts/bookmarks break
- Risk of assigning the short name to the wrong generation
- "Legacy" may overstate how dead `plato-src` still is
