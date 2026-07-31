# Plato.CppWriter.Tests

The V1 gate for the C++ / CUDA backend: generate from real Plato source, then prove the generated
code **compiles**. No runtime values are checked yet.

```
dotnet test submodules\Plato\Plato.CppWriter.Tests -c Release
```

Expected: **8 passed / 0 failed** (~45 s, most of it MSVC compiling the standard library).

| Test | What it gates |
|---|---|
| `Generated_Cpp_Compiles(Demo, StandardLibrary)` | `plato.hpp` compiles with `cl /TP /std:c++17 /W3` |
| `Generated_Cuda_Compiles(Demo, StandardLibrary)` | `plato.cu` compiles (see the two modes below) |
| `Dialects_Differ_Only_In_The_Preamble` | the code section of the two outputs is byte-identical |
| `Every_Emitted_Function_Has_A_Prototype` | one prototype per emitted function |

Sources are found by walking up from the test binaries to `demos/stdlib-legacy` and `stdlib-legacy`, so
the tests follow the repo rather than a configured path. Generated output goes to
`%TEMP%\plato-cpp-tests\<library>-<dialect>`.

## The two CUDA modes

`Toolchain.FindNvcc` decides, and the chosen mode is printed in the test output:

- **nvcc**, when the CUDA Toolkit is installed (`CUDA_PATH`, the standard toolkit directory, or
  `PATH`): the real compiler.
- **MSVC + shim**, otherwise: the `.cu` is compiled as host C++ against a test-only stand-in
  `cuda_runtime.h` that defines the `__host__` / `__device__` qualifiers away and supplies
  `float2/3/4` + `make_floatN`. This still gates everything the writer decides — types, overload
  resolution, name collisions, unresolved calls — but not CUDA-specific semantics.

No toolkit is installed on the current development machine, so the CUDA tests run in shim mode.
They upgrade themselves the day one is installed; nothing needs changing here.

Both tests `Assert.Ignore` rather than fail when no compiler at all can be found.
