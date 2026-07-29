**Law/witness libraries for the FORWARD stdlib (`stdlib/`).**

The v3 sibling of `stdlib-legacy-tests/`. `Law_*`/`Witness_*` Boolean functions live here in
`library` blocks and are **never merged into `stdlib/`** — `tools/regen-forward-conformance.ps1`
merges them with `stdlib/` into a temporary folder for type-checking and (once codegen is
unblocked) code generation + execution by `conformance/Plato.ForwardConformanceTests`.

See that project's README for the current codegen blocker. Every member a law references must be
verified against the forward library source (LIBRARIES.md rule 3) before it is added.
