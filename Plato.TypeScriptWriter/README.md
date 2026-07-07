# Plato.TypeScriptWriter

Generates TypeScript source code from a Plato compilation. This project mirrors the
architecture of `Plato.CSharpWriter`:

| Plato.CSharpWriter | Plato.TypeScriptWriter | Role |
|---|---|---|
| `CSharpWriter` | `TypeScriptWriter` | Top level orchestrator: walks the compilation and produces output files |
| `CSharpTypeWriter` | `TypeScriptTypeWriter` | Writes code for one type (or standalone functions); resolves type names |
| `CSharpConcreteTypeWriter` | `TypeScriptConcreteTypeWriter` | Writes a concrete type (struct in C#; class or native prototype extension in TS) |
| `CSharpFunctionInfo` | `TypeScriptFunctionInfo` | Computes signatures for a function instance |
| `CSharpFunctionBodyWriter` | `TypeScriptFunctionBodyWriter` | Recursively translates the Plato symbol tree into expressions/statements |
| `ITypeToCSharp` | `ITypeToTypeScript` | Type-name resolution abstraction |

## Usage

```csharp
using Ara3D.Geometry.TypeScriptWriter;

var writer = compilation.ToTypeScript(outputFolder);
foreach (var kv in writer.Files)
    outputFolder.RelativeFile(kv.Key).WriteAllText(kv.Value.ToString());
```

Or via `Plato.CLI`:

```
Plato.CLI [inputFolder] [outputFolder] --typescript
```

## Output model

A single self-contained module `plato.g.ts` is produced (TypeScript modules are
closed; splitting output would create cyclic imports). It contains, in order:

1. A hand-written intrinsics prelude (`Intrinsics` namespace: `Install`,
   `MakeArray`, `Range`, structural equality, throw helpers) plus minimal
   `IArray2D`/`IArray3D` interfaces.
2. One `export interface` per Plato concept.
3. The `IArray<T>` interface and the `Arr<T>` class. Library functions whose
   first parameter is an IArray and that are generic over the element type
   become methods of both; functions over a concrete element type become
   module-level functions.
4. An `export class Constants` with one static getter per library constant.
5. **Native prototype extensions** for the Plato primitives.
6. One `export class` per remaining concrete type.

### Fluent syntax on native values

Plato's `Number` and `Integer` map to the native `number`, `Boolean` to
`boolean`, `String` and `Character` to `string`. Their Plato functions are
installed on the native prototypes via `Object.defineProperty`
(non-enumerable), with matching `declare global` interface augmentations for
typing. This gives fluent syntax on plain values with zero wrappers:

```ts
(0.5).Turns().Cos()
x.Sqrt().Clamp(0, 1)
a.Lerp(b, t)
```

Native arithmetic operators keep working alongside, and generated function
bodies use them for the intrinsic operators (`this + y`). Integer literals
used as receivers are parenthesized automatically (`(1).Subtract(t)`).

Because `Number` and `Integer` share one prototype, colliding overloads are
resolved first-writer-wins (Number is processed first); e.g. a separate
truncating `Integer.Divide` cannot coexist with `Number.Divide` under the same
name. This mirrors JavaScript's single number type.

### Methods, not getters

Declared fields are the only properties (`v.X`). Every Plato function becomes
a method: single-parameter functions are zero-argument methods (`v.Length()`),
matching the extension-method convention on the C# side. Call sites
distinguish field access from calls via the compilation-wide field-name set.

Concrete classes get: readonly constructor parameter properties, `With{Field}`
functions, `static Create`, `static get Default` (when all fields have
defaults), structural `Equals`/`NotEquals`, `toString`, and their Plato member
functions. Bodiless intrinsics get native implementations from
`TypeScriptTypeWriter.TryGetIntrinsicBody` (`Math.*`, comparisons, trig on
`Angle`); anything unknown throws `Not implemented` at run time.

### Other conventions

- `implements` clauses are omitted (structural typing; the classes
  intentionally omit some interface members such as casts).
- Generic constraints are not emitted.
- No member overloading: colliding names are skipped with a `// Skipped:`
  comment.
- The array concept (`Array` or `IArray` depending on dialect) maps to the
  generated `IArray<T>`; the parser accepts both `concept` and `interface`
  keywords.

## Known limitations

- Functions overloaded across `Number`/`Integer` collapse (see above).
- Static functions on primitives (first parameter `_`) are skipped.
- Library functions on `IArray` are only callable on values that are actually
  `Arr` instances (or otherwise expose the methods).
