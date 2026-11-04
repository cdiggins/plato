# Runnable language examples

How groups are laid out, how a group names its inputs, and how pages use fixtures are set in the [reference contract](../../../../tools/reference/CONTRACT.md#example-fixtures).

These fixtures are source companions to the language reference. The `prelude` folder declares the primitive names and the small intrinsic surface the core fixtures use. Compile the prelude together with exactly one example group. The groups are isolated so a declaration in one group cannot change resolution in another.

```powershell
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll lint docs/reference/implementation/examples/prelude docs/reference/implementation/examples/products --strict
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll run docs/reference/implementation/examples/prelude docs/reference/implementation/examples/products --laws
```

Replace `products` with any group's folder name. A `Law_*` function exposes the expected outcome to the law runner. These minimal intrinsic declarations are test inputs. They are not proposed additions to the standard library's intrinsic kernel. The [2026-10-25 integration record](../verification-2026-10-25.md) lists which of these examples were actually run. This page does not assert that an example passed unless that record includes it.

Every folder here except `prelude` is a fixture group, and `tools/reference/check_examples.py` runs each one. A group compiles with `prelude` unless it holds an `inputs.txt` listing repo-relative input folders, one per line. The `builders`, `tour` and `cookbook-*` groups use it to compile against the real `stdlib` tiers instead of this minimal prelude.

- [Prelude](prelude/core.plato): primitive identities and arithmetic signatures.
- [Products](products/products.plato): nominal records, tuple construction, sealed construction.
- [Sums](sums/sums.plato): generic payload cases and exhaustive match.
- [Functions](functions/functions.plato): lambda application, named function forwarding, bounded calls.
- [Loops](loops/loops.plato): while, local assignment, and exact small arithmetic.
- [Builders](builders/builders.plato): List construction and consumption through Freeze.
- [Tour](tour/): the checked examples and exercise answers behind the [tour](../../start/tour.md).
- `cookbook-<recipe>`: one group per [cookbook](../../cookbook/README.md) recipe.
