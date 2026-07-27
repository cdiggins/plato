# Wave-2 sum types + match — DONE

Symbol resolution, checking (CHK300-306/320), match lowering, C# tagged-struct emission.

## Delivered
- TypeDef.Cases (SumCaseDef/SumCaseField) + IsSum; per-case SumFactory functions.
- MatchExpression symbol; SymbolFactory resolves AstMatch (binders typed by case fields).
- Normalizer + ConstraintGenerator + Elaborator handle MatchExpression; Elaborator lowers
  match -> TirConditional chain over EXISTING nodes (per-case Is<Case> predicate conditions +
  Case_Field projections; last case = unconditional else). No new TIR node.
- SumTypeChecker CHK300-306 (generics RESTRICTED -> CHK306: bare-T library type params
  unsupported; sum decl resolves but consumers don't).
- CSharpConcreteTypeWriter.WriteSumType: tag + tag consts + flattened fields + private ctor +
  per-case factories + Is<Case> predicates + structural Equals/Hash/ToString.
- StructSurfacePropertyNames gains sum flattened names so projections render bare.
- TS/Rust CHK320 struct-level guard.
- Tests: SumTypeCheckingTests (12), SumTypeCodegenTests (4, in-proc Roslyn compile+run).
- Side fix: SymbolFactory tuple-ctor guards on TupleN existing (self-contained fixtures
  lack Tuple2).

## Gates (baseline -> after)
- Plato.CLI Release build: PASS
- PlatoTests: 126 -> 142 (all green)
- conformance: 205/205 PASS (0 fail)
- regen-generated: Unoptimized 184/184 clean; Optimized 1 PRE-EXISTING drift (_Bounds3D
  trailing blank, verified identical on baseline w/o my changes). My changes = ZERO drift.
- lint: plato-src 193, plato-src-v3 4584 (unchanged)
- check-frozen-v1: PRE-EXISTING FAIL (162 dirty ara3d-sdk/Plato.Generated files at session
  start; I touched nothing in ara3d-sdk).

## PUSH: env auto-pushes on commit. One commit, pathspec, Plato repo only.
