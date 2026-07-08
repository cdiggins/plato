# Phase 6 affine types — PROGRESS

- [x] 1. Grammar/CST: `unique` modifier (parakeet: PlatoGrammar.cs + PlatoGrammarCst.cs + factory; baseline diff saved to scratchpad)
- [x] 2. AST: AstTypeDeclaration.IsUnique; AstNodeFactory reads CstType.UniqueKeyword
- [x] 3. Compiler: TypeDef.IsUnique; hard-reject unique non-List/Buffer; UniqueTypes effect table; LINT006/007
- [x] 4. CSharpWriter: skip unique types in WriteAll/BuildExtensionPlans; map List->PlatoList, Buffer->PlatoBuffer
- [x] 5. Intrinsics: PlatoList.cs / PlatoBuffer.cs (+projitems); regen-plato -Apply sync
- [x] 6. plato-src/unique.plato (decls + body-less sigs); byte-identity: only docs.html/interfaces.txt may differ
- [ ] 7. plato-test-src/unique.algorithms.plato (EarClip, Extrude, Filter + witnesses); conformance V1/V2/Opt/Scalar
- [ ] 8. docs/affine-types.md; roadmap DONE note; COMMIT_MSG.txt; check-all.ps1
