# Wave-3 sum-type flagship migrations (plato-232) — DONE

Migrate v3 flagship kind-pattern types to real sum types; close docs + tracker.

## Delivered (plato-src-v3)
- 40-paths: PathSegment2D sum (6 verbs); FillRule + PathBooleanOperation enum sums.
- 41-vector-styling: Paint sum (5) + FillStyle{Paint,Opacity} wrapper.
- 43-scene2d: MaskSource2D sum (3) + ClipMask2D{Source,Inverted} wrapper.
- 26-fields: ScalarFieldNode2D/3D sums (14 ops). NOT recursive (operands = Integer node
  indices, not embedded node values) -> migratable per design-doc rules.
- 60-signals: WindowFunction sum (8, params on Kaiser/Gaussian/Tukey) + AnalysisWindow wrapper.
- Retired 7 XxxKind types. No cross-file references (all self-contained; carriers used only
  by-value in Array<...>).

## Gates (baseline -> after)
- lint plato-src-v3: EXIT 0, 0 parse + 0 symbol-resolution errors. 4584 -> 4549 findings.
- lint plato-src: 193 (unchanged).
- PlatoTests: 142/142 green (unchanged).

## Docs/tracker: v3 README convention, survey DONE marks, roadmap note, plato-232 close,
## new backlog idea (kind-pattern sweep). PUSH: env auto-pushes on commit.
