# Number profiles and scientific arithmetic

`Number` is the floating scalar. `Integer` is the signed whole-number scalar. A compilation chooses one **profile**, and that profile fixes both machine representations. There is no source type `Float32` or `Double`, and there is no per-function precision switch.

| Profile | Number representation | Integer representation |
|---|---|---|
| `cpu64` (default) | IEEE 754 binary64 | Signed 64-bit |
| `cpu64i32` | IEEE 754 binary64 | Signed 32-bit |
| `gpu32` | IEEE 754 binary32 | Signed 32-bit |

The CLI selects `--profile=cpu64`, `--profile=cpu64i32`, or `--profile=gpu32`. The C# runtime uses aliases selected by the `PlatoNumberProfile` build property. Generated profile guards reject a disagreement between generation and compilation. The interpreter receives a profile as well. `gpu32` names an arithmetic model. Selecting it does not run the program on a GPU. `cpu64i32` keeps binary64 coordinates and uses the 32-bit width of a .NET index for `Integer`.

C# `float` and `double` are separate host types. Plato has one `Number` type, and the profile chooses the width for the whole compilation. The [.NET floating-point reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types) describes those host representations. [NumberProfile](../../../src/Plato.Compiler/Types/NumberProfile.cs) defines the profiles, the [interpreter](../../../src/Plato.Interpreter/Evaluation/Numbers.cs) applies them, the [runtime aliases](../../../src/Plato.Intrinsics/NumberProfile.cs) bind C# storage, and the [profile tests](../../../tests/Plato.Intrinsics.Tests/NumberProfileTests.cs) exercise the runtime contract.

## Real-number intent versus machine values

The library describes geometric quantities as real-valued. Execution uses finite floating approximations, signed zero, infinities, and NaN. An equation over real numbers is not automatically an execution law. Associativity, `(a + b) + c = a + (b + c)`, can fail after rounding. Multiplication by zero is not a universal simplification once NaN and infinities are included.

The literal `1` starts as `Integer`. The literal `1.0` starts as `Number`. Converting an integer to `Number` may round outside the profile's exact-integer region. In binary64, integers with magnitude through `2^53` are exactly representable. Past that, some adjacent integers share one floating representation. Binary32 has the corresponding threshold `2^24`. These are arithmetic representation facts, not observed gate measurements.

The runtime's Number-to-Integer conversion truncates toward zero. Non-finite and out-of-range values follow the host's boundary behavior. Consult the interpreter and runtime implementations above before relying on those inputs. Under `cpu64`, conversion from `Integer` to a .NET index is checked when a 32-bit host index is required. A legal language integer can still exceed an array API's host range. Under `cpu64i32` the language integer is already that width, so the conversion is the value itself.

## Profiles apply to every operation

The interpreter stores `Number` values internally as doubles. Under `gpu32` it rounds a profile result through float. Elementary functions use `MathF` under `gpu32` and `Math` under `cpu64` and `cpu64i32`. Integer operations use the profile's width, including the 32-bit wrap of `cpu64i32`. Storing the value as a double does not make `gpu32` use binary64 arithmetic. Each operation, conversion, literal, and constant has to follow the profile. Rounding only the final result of a function does not implement it.

Basic and transcendental functions use the .NET host implementation. Plato-owned correctly rounded transcendentals, and compile-time precision requirement clauses, are design work. They are not supported guarantees. The `Number` profile does not promise arbitrary precision, and it does not promise platform-independent last-bit identity for every elementary function.

## Units and tolerances

State units in the type and in the domain conventions. The library's [angle convention](../../../stdlib/CONVENTIONS.md#angles--angle-typed-radians-canonical) and the [point and vector](../library/foundation/points-vectors.md) rules name the types. A nominal wrapper blocks many category errors and still shares arithmetic storage.

For a numerical law, separate an exact identity from a toleranced comparison, and give the tolerance a meaning in the domain. Machine epsilon, the smallest positive subnormal, and an application's geometric tolerance are different quantities. A name such as `Epsilon` requires reading the declared operation. The C# floating `Epsilon` constant is the smallest positive subnormal, not the spacing at one.

A proof over the mathematical reals needs a separate error analysis before it is a floating-point correctness guarantee. What a finite law run establishes is recorded in [verification](../implementation/verification.md).
