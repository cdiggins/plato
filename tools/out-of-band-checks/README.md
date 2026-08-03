# Out-of-band algorithm checks

Python transcriptions of Plato geometry and analysis algorithms, run against
closed-form answers and structural invariants.

## Why these exist

No gate in this repo **executes** a `stdlib/geometry` or `stdlib/future` body.
The forward conformance law runner generates code that does not compile
(`tracker/issues/plato-308.md`), so the seven rungs in
[`../../stdlib/VERIFICATION.md`](../../stdlib/VERIFICATION.md) stop at codegen:
lint, the type checker and the ratchets all agree a body is well-formed, and
none of them can tell you it is right.

The convention that grew around that gap is the one
[`tracker/issues/plato-413.md`](../../tracker/issues/plato-413.md) set — check
the algorithm out of band against the same index arithmetic the Plato source
uses, and say in the issue that the evidence is a transcription rather than a
run. These scripts are those checks, kept rather than discarded.

## What they prove, and what they do not

**They do not test the Plato source.** They test a transcription of it. A
transcription can drift from the file it mirrors, and nothing here will notice.
Read them as recorded reasoning that can be re-run, not as a suite.

What they are good for is exactly what they were good for when they were
written: each one caught real bugs that lint and the type checker could not see,
because the failures were geometric rather than syntactic — a face wound
backwards, a batched edit that broke manifoldness, a table row that named the
wrong edge. That class of bug is invisible to every gate that currently runs.

**When `plato-308` is fixed and the law runner executes, these become
redundant** and should be deleted rather than maintained in parallel. Until
then they are the only executable evidence three tracks have.

## Running them

Each prints its own verdict and exits non-zero on failure. No dependencies
beyond the standard library.

```bash
python tools/out-of-band-checks/remeshing.py
python tools/out-of-band-checks/lattice-cells.py
python tools/out-of-band-checks/finite-elements.py
```

| Script | Mirrors | Checks |
|---|---|---|
| `remeshing.py` | `stdlib/geometry/remeshing.library.plato` (plato-423) | Split, collapse, flip, the four subdivision schemes, smoothing, quadric decimation and welding over a tetrahedron, an octahedron, an open triangulated grid and a twice-subdivided octahedron. Manifoldness and orientation are re-checked after every operation |
| `lattice-cells.py` | `stdlib/geometry/lattices.library.plato` (plato-421) | That the strut-ownership welding rule emits every distinct world-space strut exactly once, for all seven unit cells, tiled evenly and at a lopsided count — the lopsided case is the one that matters, since it is where a cell has a successor on one axis and not another. Also that each cell's node valences match the published coordination numbers |
| `finite-elements.py` | `stdlib/future/finite-elements.library.plato` (plato-424) | Beam deflections against textbook cantilever and simply-supported formulas, and the gravity body-force total against rho·V·g |

`finite-elements.py` is the transcription half of that track's evidence; the
other half was a real run of the **generated TypeScript** under Node, which is
the strongest verification any of this work has and is described in
`tracker/issues/plato-424.md`.
