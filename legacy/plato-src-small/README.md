# Plato spike library

This is a deliberately small, self-contained Plato library for C# emitter and optimizer
experiments. It is not a replacement for `plato-src`; successful spikes must still pass the
full production and conformance gates.

The corpus is intentionally kept in one source file so generated code is easy to trace back to
its declaration. It covers:

- primitive scalar erasure (`Number`/`Integer`/`Boolean`);
- classic extension-method emission and zero-argument method emission;
- multi-level function inlining;
- fixed-component struct arithmetic;
- `Map`, `Zip`, `Reduce`, `All`, `Any`, and `Reverse` loop lowering.

From the Studio repository root, generate the current spike shape with:

```powershell
dotnet run --project submodules\Plato\Plato.CLI -c Release -- `
  submodules\Plato\plato-spike-src .temp\plato-spike-generated `
  --csharp-style=extensions --scalar=float --optimize --optimize-arrays `
  --inline --methods --loops
```

Generated output belongs in `.temp/`; do not check it into this folder.

`lint` currently reports `LINT003` for the fields of `Tuple2` and `Tuple3`. Those five findings
are expected: the compiler requires the tuple declarations to synthesize conversions for `Pair`
and `Vector3`, even though no library function reads the tuple fields directly.
