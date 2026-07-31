# FROZEN — V1 runtime (do not edit)

This is the handwritten V1 intrinsics runtime (wrapper scalars: `Number`, `Integer`, `Boolean`
plus `Vector*`/`Matrix*`/`Angle`/…) that pairs with the frozen generated library in
`ara3d-sdk/src/Plato.Generated`, which **Ara3D.Studio consumes today**.

As of the consolidation plan (2026-07-12), the V1 line is **frozen**:

- The **live** runtime is [`Plato.Intrinsics.V2`](../Plato.Intrinsics.V2) (System.Numerics-backed,
  method-form), used by `Generated/Plato.Generated.{Unoptimized,Optimized}` — the one codebase
  going forward.
- These files, the `ara3d-sdk` synced copy, and `ara3d-sdk/src/Plato.Generated` must **not change**.
  They are protected by a checksum tripwire, `tools/check-frozen-v1.ps1` (manifest
  `tools/frozen-v1.sha256`), which runs in `check-all.ps1`. Any edit trips the gate.

Do not edit, do not regenerate. See `docs/plato-consolidation-plan-2026-07-12.md`.
