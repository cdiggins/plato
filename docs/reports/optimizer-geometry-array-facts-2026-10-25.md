# Geometry array facts — 2026-10-25

Work item compiler-508; geometry qualification contract G3. **The focused private probe built
and passed its actual-TIR checks in the supervisor-granted runtime window.** No production pass
or generated application was added. The frozen compiler revision was
`b612c392b8998ad719f70027eba1ac7c09d91779`; manifest hashes were checked before and after execution.

The private adapter in `tools/experiments/optimizer-geometry/array-facts/ArrayFacts.cs` observes
actual lowered Plato TIR without changing it. It admits completed canonical MapRange producers,
then an immediately adjacent local binding of their result, and exact-typed local aliases.
Canonical intrinsic identity comes from the production `TirIntrinsicIdentity` helper. Temporary
association requires exact emitted temporary-name linkage, a unique shared Origin object,
and exact type agreement. `TirTempRef` exposes no binding symbol: its Name links to the
producer's TempName in emitted C#. Both producer origins and temporary names must be unique;
ambiguous or absent origins and mismatched names refuse. This is binding linkage, not semantic
operation recognition from a name. Local alias keys use `VariableDef` reference identity.
This is a restricted adapter over well-formed lowerer output, not a validator for arbitrary
forged marker nodes. It creates no production flag or emitter change.

The representative caller is newly authored `SampleRowLength`: it materializes a parameter row
using `GridParameter`, takes a local alias, and observes Count. The helper is extracted byte for
byte from the frozen `stdlib/geometry/meshes.library.plato` source. This is an honest extracted
helper plus a new caller, not an unchanged whole-mesh workload. It preserves the helper's open
versus closed sampling formula and its documented precondition; no runtime geometry equivalence
or degenerate-input behavior is claimed from a read-only analysis.

Observed concrete enabling benefit: a surviving interface Count on a local alias can be tied
to the already evaluated producer's normalized checked collection count. A later backend could
reuse that saved count without executing the source count expression again or suppressing the
producer's eager callbacks. Facts describe successful completion only. This does not establish
runtime savings, bounds-check removal, or a dynamic neighboring-index proof.

Unknown calls, assignments, coercions and scope transitions clear facts. Lambda creation that
mentions a tracked alias invalidates all aliases, even before any invocation; unknown invokes
also clear facts, so captures hidden behind delegate locals cannot preserve stale knowledge.
The adapter refuses external arrays. It does not import outer facts into child blocks or join
facts back out; it does not attempt effect summaries, general control flow, or interval solving.

Passing checks cover both inlining recipes, external refusal, unchanged output emission,
capture invalidation, unknown invocation, assignment, wrapped aliases, distinct same-spelled
bindings, wrong origins, mismatched temporary bindings, duplicate producer origins or temporary
names, and child scopes. Adversarial checks reuse
actual lowered nodes but are deliberately labeled synthetic TIR edits. Every accepted Count
must retain its original materialization; there is no transformation to benchmark.

Run only in a granted build/execution window:

```powershell
python tools/experiments/optimizer-geometry/array-facts/run.py --execute
```

The runner uses only frozen DLLs and stdlib under `.temp/optimizer-geometry/compiler`, validates
the supervisor's manifest before and after, and stores build output, categorized errors, inputs
and emitted bodies in the private `.temp/optimizer-geometry/array-facts` directory. It uses the
existing build recorder with a private snapshot to avoid overwriting concurrent shared reports.
No working-tree assembly fallback, shared project build, timing run or speedup claim is planned.

## Results actually obtained

| Focused check | Result | Duration |
| --- | --- | --- |
| Private reference-only harness build | PASS, zero errors or warnings | 4.10 seconds, MSBuild |
| Geometry producer and alias recognition | One Count fact in each inline-off/on recipe | Not separately timed |
| External array and adversarial TIR cases | PASS in both recipes | Not separately timed |
| Body emission with and without analysis | Byte-identical with equal capture-name counter state | Not separately timed |
| Snapshot and experiment input hashes | Unchanged through execution | Not separately timed |
| Generated application execution, timing, broad gates | Not run; this analysis changes no output | — |

The emitted sample-row body still allocates and fills the Number array, assigns it through
`coordinates` and `row`, and returns `((Integer)row.Count)`. The report-only fact connects that
Count to `_n0` (inline off) or `_n1` (inline on), the saved checked-and-normalized collection
length. This is a concrete identity/provenance capability for a later backend, not evidence
that a replacement will improve the JIT's machine code. No callback or allocation was removed.

The first attempt built successfully but its unchanged-output assertion failed because repeated
emissions advanced the process-local capture naming counter. The harness now resets
`SymbolRewriter.NextId` equally before both emissions. The corrected focused rerun passed;
no compiler fix was made or needed. The earlier build reported 3.97 seconds and zero diagnostics.

The next step is review of this narrow fact contract with the concrete-storage owner before
any production consumer. The probe does not yet prove neighboring-index correlations, general
branch joins, executable output equivalence, or numeric-profile-dependent runtime behavior.
