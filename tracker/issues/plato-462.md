---
id: plato-462
title: Interpreter: 'a conditional's condition is not a Boolean: null' in Map < ReferencedVertexCount < Oracle_AngleDefectSum on the hollow-box witness
type: bug
status: done
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-10-18
closed: 2026-10-18
links: [src/Plato.Interpreter/Evaluation/Interpreter.cs, docs/reports/interpreter-turnaround-2026-10-17.md]
---

## Issue

Reproduction (docs/reports/interpreter-turnaround-2026-10-17.md, "Rerun on 2026-10-18", defect D5):
`plato run stdlib/foundation stdlib/geometry stdlib/graphics <toolkit>/sandbox/step-brep-mesh --recursive --exclude=brep.types.plato --exclude=brep.library.plato --exclude=brep.concepts.plato --laws --filter=Law_HollowBoxOuterMeshVolumePositive --budget=600`
fails at case 18 (a = 5.785732621227266, b = 0.5) with `a conditional's condition is not a Boolean: null`, stack `Map < ReferencedVertexCount < Oracle_AngleDefectSum < Law_AngleDefectMatchesBrepEuler < CheckAll < ... < MeshModel < WitnessOutcome < WitnessMesh`. A null reaches a conditional's condition: a `default` of an interface or function type, a block evaluated in expression position without a return, or an unbound value. The law takes about 30 minutes to reach the case; narrow it by calling `ReferencedVertexCount` directly on the mesh of `Witness_HollowBox(a, b)` with `--call`.
