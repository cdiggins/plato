# Excluded runtime implementations

This folder preserves older vector, matrix, angle, plane and array helpers for reference. `Plato.Intrinsics.projitems` does not compile them, and `Plato.Laws` embeds only the runtime's top-level C# files. These sources therefore do not establish supported runtime behavior.

Use the [live runtime guide](../README.md) when implementing a host operation. Restoring a file from here requires an explicit design decision against the [intrinsic contract](../../../docs/plato-intrinsics-surface.md); moving it into the live runtime can introduce duplicate generated members.
