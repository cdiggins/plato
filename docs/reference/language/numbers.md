# Number profiles and scientific arithmetic

`Number` is the language's floating scalar and `Integer` its signed whole-number scalar. A compilation chooses one **profile**, fixing both machine representations. There is no independent source `Float32` or `Double` type and no per-function precision switch.

| Profile | Number representation | Integer representation |
|---|---|---|
| `cpu64` (default) | IEEE 754 binary64 | Signed 64-bit |
| `gpu32` | IEEE 754 binary32 | Signed 32-bit |

The CLI selects `--profile=cpu64` or `--profile=gpu32`. The C# runtime uses aliases selected by the `PlatoNumberProfile` build property; generated profile guards reject disagreement between generation and compilation. The interpreter also receives a profile. `gpu32` names an arithmetic model; selecting it does not execute the program on a GPU.

## Real-number intent versus machine values

The library describes geometric quantities as real-valued, but execution uses finite floating approximations, signed zero, infinities, and NaN. An equation over real numbers is not automatically an execution law. For example, associativity `(a + b) + c = a + (b + c)` can fail after rounding. Multiplication by zero is not a universal simplification when NaN and infinities are included.

Literal `1` starts as `Integer`; `1.0` starts as `Number`. Converting an integer to Number may round outside the profile's exact-integer region. In binary64, integers through magnitude `2^53` are representable exactly; beyond that, some adjacent integers share the same floating representation. Binary32 has the corresponding `2^24` threshold. These are arithmetic representation facts, not observed gate measurements.

The runtime's Number-to-Integer conversion truncates toward zero with host boundary behavior for non-finite and out-of-range values. Consult the exact implementation before relying on such inputs. CPU64 Integer-to-.NET-index conversion is checked when a 32-bit host index is required: a legal language integer can still exceed an array API's host range.

## Profiles apply to every operation

The interpreter stores Number values internally as doubles, rounding profile results through float under `gpu32`; elementary functions use `MathF` there and `Math` under `cpu64`. Integer operations use the profile's width. This internal storage choice does not turn gpu32 into binary64 arithmetic. Implementing a profile requires attention to each operation, conversion, literal, and constant rather than rounding only final function results.

Basic and transcendental functions use the .NET host implementation. Plato-owned correctly rounded transcendental implementations and compile-time precision requirement clauses are design work, not supported guarantees. The Number profile does not promise arbitrary precision or platform-independent last-bit identity for every elementary function.

## Scientific authoring guidance

State units in the type and domain conventions: an angle is not merely an unlabelled Number, and a point is not a displacement. A nominal wrapper protects against many category errors while sharing arithmetic storage.

For numerical laws, distinguish exact identities from toleranced comparisons and give the tolerance a domain meaning. Machine epsilon, the smallest positive subnormal, and an application's geometric tolerance are different quantities. Names such as `Epsilon` require reading the declared operation: the C# floating `Epsilon` constant is the smallest positive subnormal, not the spacing at one.

A proof of an algebraic algorithm over mathematical reals needs a separate error analysis before it becomes a floating-point correctness guarantee. A law runner with generated witnesses provides useful executable evidence but is not universal quantification over every representable input.

## Evidence and related work

[NumberProfile](../../../src/Plato.Compiler/Types/NumberProfile.cs) defines profiles; [interpreter Numbers](../../../src/Plato.Interpreter/Evaluation/Numbers.cs) applies them; [runtime aliases](../../../src/Plato.Intrinsics/NumberProfile.cs) bind C# storage; [profile tests](../../../tests/Plato.Intrinsics.Tests/NumberProfileTests.cs) exercise the runtime contract. The [.NET floating-point reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types) explains host representations and limitations. Plato deliberately hides separate floating type names behind one compilation-level profile, so C# `float`/`double` overload distinctions are not separate Plato source types.
