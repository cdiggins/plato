# Navigation and warm-server checks

These tests exercise indexing, references, serialization, style diagnostics, simplification and cached law execution. `Corpus.cs` supplies shared inputs; production responsibilities are mapped in [Plato.Navigation](../../src/Plato.Navigation/README.md).

Ordinary runs exclude tests tagged `Slow` or `Diagnostic`. See [test selection](../README.md#test-selection) for the full regression command and how to run slow checks separately.
