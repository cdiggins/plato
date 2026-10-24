# Code generation backends

These projects turn a resolved Plato compilation and its typed intermediate representation (typed IR) into target-language source. Start in a backend's writer class to follow output orchestration, then its type and body writers for syntax decisions.

| Folder | Responsibility |
|---|---|
| [Plato.CSharpWriter](Plato.CSharpWriter/README.md) | Emits C# and hosts optimizer passes shared with other backends. |
| [Plato.CppWriter](Plato.CppWriter/README.md) | Emits portable C++ and CUDA through a shared body writer. |
| [Plato.GlslWriter](Plato.GlslWriter/README.md) | Emits GLSL shader-library source. |
| [Plato.RustWriter](Plato.RustWriter/README.md) | Emits Rust source. |
| [Plato.TypeScriptWriter](Plato.TypeScriptWriter/README.md) | Emits TypeScript source. |

The C# backend is the library-generation path; the other backends are proofs of concept with their own documented limits. The [language and code-generation guide](../docs/plato-for-agents.md) explains shared compiler inputs. Backend tests live under [tests](../tests/README.md). Build evidence belongs in the repository gate logs rather than this navigation guide.
