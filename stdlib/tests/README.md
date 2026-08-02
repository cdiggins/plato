**Law/witness libraries for the FORWARD stdlib (`stdlib/`).**

The v3 sibling of `stdlib-legacy-tests/`. `Law_*`/`Witness_*` Boolean functions live here in
`library` blocks and are **never merged into the library** — `tools/regen-forward-conformance.ps1`
merges them with the `stdlib` tiers into a temporary folder for type-checking and (once codegen is
unblocked) code generation + execution by `conformance/Plato.ForwardConformanceTests`.

This folder sits inside `stdlib/` but is **not a tier**. The tiers are `foundation`, `geometry`,
`graphics` and `future`; every gate over the library names them explicitly and therefore does not
see this packet (stdlib-398). A consumer that wants the laws asks for `stdlib/tests` by name — the
codegen step of `tools/record-gates.py` and the conformance regeneration script do; the lint gate,
the checker ratchet and the C# codegen recipe do not.

See that project's README for the current codegen blocker. Every member a law references must be
verified against the forward library source (LIBRARIES.md rule 3) before it is added.
