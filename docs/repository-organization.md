# Source organization and navigation

Use the nearest folder README to find a responsibility, then read a source file's opening comment for its role. [src](../src/README.md) maps compiler and execution projects, [writers](../writers/README.md) maps language generation, [tests](../tests/README.md) maps validation, and [tools](../tools/README.md) maps repository commands.

Group implementation files by the behavior they own. A folder move should preserve namespaces and project identities unless changing those contracts is the purpose of the work. Split a large class when a coherent responsibility can move intact; partial classes are appropriate when the split only changes navigation and keeps private implementation details together. Avoid size-only divisions that make a reader jump among arbitrary fragments.

Every maintained source file begins with a short description of its responsibility. Preserve useful existing descriptions and language directives such as shebangs. Every maintained folder has a README explaining its contents and the next useful entry point. Keep each explanation near its owner and link to established contracts instead of duplicating them. These guides describe responsibilities; measurements belong in gate logs and dated reports.

## Formats and ownership

Generated source, compiled editor extensions, vendored browser assets, binary packages and historical snapshots retain their producer's contents. Their containing guides identify the source or regeneration command. Do not add comments to generated files by hand. In particular, `demos/*/vendor/gratify/` contains copied upstream modules, and `visualstudio-plato/.vsix-unpack/` contains unpacked package artifacts; their parent guides provide navigation without modifying those artifacts.

JSON manifests, lockfiles, solution metadata and other machine-readable data use the containing README for their role rather than introducing unsupported comments or extra fields. Markdown documents and tracker records already carry titles or descriptive metadata. Parser fixtures and expected snapshots retain their source positions and exact contents; fixture-folder guides explain them. External Git submodules retain their own documentation policies.

## Relocation checks

Before moving a file, check project compile and resource includes, source-relative fixture paths, scripts and source-scanning tests. SDK projects normally include nested C# files automatically, but shared runtime imports and embedded resources may list explicit paths. The intrinsic runtime deliberately remains flat to preserve those consumers.

The standard-library tiers also remain flat: the CLI enumerates each named input directory non-recursively. Related filename stems and tier READMEs provide functional grouping without changing compiler input semantics. The library's [partitioning contract](../stdlib/LIBRARIES.md) and [verification policy](../stdlib/VERIFICATION.md) remain authoritative.

Keep source-reading checks recursive when they cover a responsibility tree, and exclude build output directories. The policy tests under `tests/PlatoTests/Contracts/` enforce this for verification-policy scanning. Snapshot and inventory tests stay beside their baselines when they locate them through their source-file paths.
