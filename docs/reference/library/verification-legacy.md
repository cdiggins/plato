# Verification, source authority and the legacy boundary

This guide describes how to interpret evidence. It deliberately contains no pass counts or ratchet ceilings: those belong to live checks, the [verification manifest](../../../stdlib/verification.json), and recorded [gate results](../../gate-log.md).

## What a source page establishes

| Evidence | Establishes | Does not establish |
|---|---|---|
| Type declaration | Fields, sum cases, claimed interfaces | Invariants validated at construction |
| Interface declaration | Required signatures and constraints | Mathematical laws proved |
| Library body | The written computation | All domains numerically correct |
| Intrinsic signature | A required host operation | A correct matching runtime by itself |
| Parsing/resolution/type checking | Relevant static properties | Executed results |
| Generated C# builds | Host syntax/type compatibility | Every member has a real nonthrowing body |
| A concrete test or law run | The cases and configuration actually executed | Every possible input or configuration |
| Mathematical argument | Consequence under stated assumptions | A machine proof about the implementation |

The [verification policy](../../../stdlib/VERIFICATION.md) explains static gates, code generation, execution and wrong greens. Some prose there records obsolete states, including conflicting claims about law-runner status and references to retired tests. Use the actual gate/result authority before stating current support. This documentation task's authoring checks are not a new execution certification of the library.

## C# is a host boundary, not a synonym

The [library artifact map](../../plato-library-map.md) identifies what consumes each source/runtime artifact. [Intrinsics](../../../src/Plato.Intrinsics) implement the primitive host contract. The [C# writer](../../../writers/Plato.CSharpWriter) produces methods and wrapper scalars under its own lowering rules. Familiar C# or `System.Numerics` names do not automatically grant every host operation to Plato.

A source function can be expressible mathematically yet fail a particular lowering, emit a degraded throwing body, or lack a concrete interface obligation. Check source, compiler diagnostics, emitted body and runtime evidence separately. Experimental non-C# writers are not semantic authority for these guides.

## Forward and legacy libraries differ

`stdlib` is the forward vocabulary documented here. [Legacy source](../../../legacy/stdlib-legacy) is a separate shipping-generation library; names, overloads and implementation coverage can differ. A legacy operation must not be silently copied into the forward API reference. Generated output is cached build output, not the language specification or a byte-identity golden.

Known limitations found during this reference's creation are tracked in [stdlib-526](../../../tracker/issues/stdlib-526.md), the library reference audit item. Local guide callouts state the practical effect so a reader does not need the tracker to discover a precondition or conflict.

## Related work

[System.Numerics.Matrix4x4](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4) documents the intended row-vector matrix model. Compatibility with that model is narrower than language identity: Plato's pure values, typed quantities, conversions and generated wrapper surface remain its own contracts.
