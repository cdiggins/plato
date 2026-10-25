# Record projection geometry qualification

Status: compiler-507 remains in progress. The private typed-IR experiment passes focused
generated-code correctness checks. No performance improvement or shipping-workload benefit has
been established; production integration remains deferred.

## Geometry relevance and refusals

`FrameOriginAt`, `TangentDirectionAt`, `NormalDirectionAt`, and `BinormalDirectionAt` in
`stdlib/geometry/curves.library.plato` read one field from `FrameAt`. The receiver is an interface
call in source, not an immediate constructor. Specialization and inlining must expose a concrete
constructor before a direct-projection pass can help. No such exposure has been verified here.

`Triangle` in `stdlib/geometry/meshes.library.plato` repeatedly reads the fields of `FaceAt`.
For `TriangleMesh3D`, that helper reads an existing array element. Eliminating a fresh temporary
constructor does not remove the array read or prove safe common-subexpression elimination.
These are useful negative candidates, not reasons to broaden the proposed rewrite.

The `ProjectionPoint` fixture is a newly authored coordinate-extraction kernel. It
represents selecting one coordinate while constructing a point, but is only a small geometric
operation, not a whole shipping geometry workload. Its explicit constructor makes the JIT baseline
easy to challenge. The experiment must be deprioritized if the JIT already removes the struct cost;
no heap allocation saving should be assumed for a value-type record.

## Verified experiment and decision gate

`tools/experiments/optimizer-geometry/record-elimination/Projection.cs` transforms only a direct
function-body property call whose receiver is a resolved generated constructor. Getter and
constructor identities must match their owning declaration metadata. The receiver must have the
exact record type, and each argument must already have its declared field type. Generic records,
primitive records, sum types, converted receivers and non-constructor receivers refuse. Every
argument is preserved in an ordered typed binding, including unused fields that can throw.
It adds no compiler option; leaving this experiment uninvoked disables it entirely.

The harness compiles baseline and transformed bodies emitted by Plato against frozen
runtime assemblies. It checks wrapper bit patterns, unused-field callback order and exceptions,
same-spelled fake getter/constructor identities, and converted receiver refusal. It tests both
existing inline recipes. Its optional timing mode warms both delegates, alternates order, consumes
results and records allocation bytes over point-coordinate arrays. Delegate dispatch is shared
overhead and can mask tiny struct-copy costs. Timing equality alone would not prove identical
machine code; inspect JIT disassembly before making that stronger claim.

After the supervisor grants the runtime window:

```powershell
python tools/experiments/optimizer-geometry/record-elimination/run.py --execute
python tools/experiments/optimizer-geometry/record-elimination/run.py --execute --mode benchmark
```

The first command passed against revision `b612c392b8998ad719f70027eba1ac7c09d91779` and the
supervisor's `GeometryRuntime.dll` cpu64 runtime. Both inline recipes retain the baseline
`new ProjectionPoint(x, y, z).X` shape; the private rewrite produces three ordered bindings and
returns the first. The faulting fixture still invokes both callbacks in declaration order when
neither throws, stops at the first exception, and never discards the unused fields. Tests reject
fake same-spelled getter and constructor definitions, converted receivers, existing-record receivers,
and escaping records. Frozen source and copied/loaded assembly fingerprints passed.

The private harness build passed with no warnings in 10.96 seconds as reported by MSBuild.
The initial attempt exposed harness error CS0104, `DiagnosticSeverity is an ambiguous reference`;
explicitly selecting Roslyn's diagnostic enum fixed it. Generated-code checks then passed; their
duration was not separately recorded. No broader gates, benchmarks or JIT disassembly ran.

Critical review identified a generated-local name collision with an unused lambda parameter.
The name reservation now includes every lambda parameter, even when its body never references it.
The focused source fixture `collision.plato` places such a lambda inside a constructor argument;
both emitted variants compile and return the same coordinate after the local is renamed.
Only that affected slice was rerun using `run.py --execute --mode collision`; its passing log is
`results-collision.txt`, with generated source in `collision-generated.cs.txt`.

The second command still needs its own serialized timing reservation. Both use the frozen manifest at
`.temp/optimizer-geometry/compiler/manifest.json`; build logs, categorized build errors, generated
sources and input hashes remain in the private evidence directory. `results-verify.txt` holds the
loaded assembly identities and correctness results. Production remains deferred until meaningful incremental
benefit or a concrete downstream optimization opportunity is demonstrated.
