# Runnable language examples

These fixtures are source companions to the language reference. The `prelude` folder declares the primitive names and small intrinsic surface used by the core fixtures. Compile the prelude together with exactly one example group; groups are intentionally isolated so their declarations cannot contaminate another example's resolution.

```powershell
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll lint docs/reference/implementation/examples/prelude docs/reference/implementation/examples/products --strict
dotnet src/Plato.CLI/bin/Release/net8.0/Plato.CLI.dll run docs/reference/implementation/examples/prelude docs/reference/implementation/examples/products --laws
```

Replace `products` with `sums`, `functions`, or `loops`. `Law_*` functions expose expected outcomes to the law runner. These minimal intrinsic declarations are test inputs, not proposed additions to the standard library's intrinsic kernel. The integrated supervisor run records actual execution results; this page does not assert unrun examples passed.

The `builders` group instead requires the real `stdlib/foundation` input. Compile it with foundation alone, without this minimal prelude, to obtain the real List/Buffer declarations and intrinsic contracts.

- [Prelude](prelude/core.plato): primitive identities and arithmetic signatures.
- [Products](products/products.plato): nominal records, tuple construction, sealed construction.
- [Sums](sums/sums.plato): generic payload cases and exhaustive match.
- [Functions](functions/functions.plato): lambda application, named function forwarding, bounded calls.
- [Loops](loops/loops.plato): while, local assignment, and exact small arithmetic.
- [Builders](builders/builders.plato): List construction and consumption through Freeze.
