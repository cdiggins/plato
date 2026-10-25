# Output sizing checkpoint

- Contract P2; compiler-509; state: production exact-output tests verified by root, files frozen.
- Own subtree plus output-sizing design and dated report; supervisor owns commits/tracker.
- Standalone C# compares actual extracted runtime FlatMap with exact-storage model.
- Final private net8.0 build and executable checks passed; run.py reproduces evidence.
- Actual wrapper off/on allocation: 295200/229400 bytes per call; all 17 exact-output cases passed.
- No running commands/processes; no shared implementation changes.
- Real TIR: canonical Intrinsics.FlatMap, raw Number literal arrays survive capture rewriting.
- New TirExactOutput/emitter/tests ready; root owns provenance/flag/hook and coordinated builds.
