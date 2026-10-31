# Runnable language examples

These fixtures are source companions to the language reference. The `prelude` folder declares the primitive names and the small intrinsic surface the core fixtures use. Compile the prelude together with exactly one example group. The groups are isolated so a declaration in one group cannot change resolution in another.

```powershell
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll lint docs/reference/implementation/examples/prelude docs/reference/implementation/examples/products --strict
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll run docs/reference/implementation/examples/prelude docs/reference/implementation/examples/products --laws
```

Replace `products` with `sums`, `functions`, or `loops`. A `Law_*` function exposes the expected outcome to the law runner. These minimal intrinsic declarations are test inputs. They are not proposed additions to the standard library's intrinsic kernel. The [2026-10-25 integration record](../verification-2026-10-25.md) lists which of these examples were actually run. This page does not assert that an example passed unless that record includes it.

The `builders` group requires the real `stdlib/foundation` input. Compile it with foundation alone, without this minimal prelude, so the compilation has the real List and Buffer declarations and the intrinsic contracts.

- [Prelude](prelude/core.plato): primitive identities and arithmetic signatures.
- [Products](products/products.plato): nominal records, tuple construction, sealed construction.
- [Sums](sums/sums.plato): generic payload cases and exhaustive match.
- [Functions](functions/functions.plato): lambda application, named function forwarding, bounded calls.
- [Loops](loops/loops.plato): while, local assignment, and exact small arithmetic.
- [Builders](builders/builders.plato): List construction and consumption through Freeze.
