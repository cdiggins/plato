---
id: compiler-546
title: Carry subtree facts in a per-body TirFacts table instead of recounting per pass
type: debt
status: in-progress
priority: p1
effort: M
risk: low
area: compiler
sprint: 
created: 2026-11-07
closed:
links: []
---

## Why

The optimizer's passes each recount facts about the tree from scratch: twelve distinct use/mention counters,
each a `Descendants()` walk, several quadratic (`TirSinkBindings.Mentions` rescanned every later statement per
let; `TirSharedCalls` re-serialised a subtree into a key string per candidate call per round and took
`Descendants().Count()` per site). On a shared-subtree DAG such a walk visits 3·2^d − 2 nodes for 2d + 1 distinct
ones (the 2026-10-25 probe: 3,145,726 for 41).

TIR nodes are immutable, so a fact about a subtree is intrinsic to the node object: memoized once by reference
identity it can never go stale, because a rewrite builds new nodes. Facts that depend on context (statement
index, enclosing lambda) are not intrinsic and stay out.

## What

- `src/Plato.Compiler/Checking/TirFacts.cs`: `TirFacts` (one per body, one thread): `Size`, `ContainsKind<T>`,
  `ContainsAssign`, `ContainsLoop`, `MentionedDefs`, `Mentions`, `StructuralKey(call, wide)`, and the test
  counter `NodesVisited`; `TirStructuralKey` holds the shared-calls key writer, composed from memoized child keys.
- `PassContext.Facts`, fresh in `ForBody`; the contract is in `ITirPass`.
- Lane 1 consumers: `TirSinkBindings` (the elaborator makes a table per function, the pass passes `ctx.Facts`)
  and `TirSharedCalls`.
- Tests: `tests/PlatoTests/Optimization/TirFactsTests.cs` (the DAG test, and equivalence with the walks over
  both fixtures).

## Later lanes

The remaining counters: `TirRewrite.CountParamUses`, `TirInliner.AllUsesConsuming`, `InvokeResultNavigated`,
`TirArrayMaterializer.CountRefs`, `TirLoopInvariantHoister.CountHoistable/IsInvariant`,
`TirUpdateFlow.FindFreshTemps`, `RemoveUnusedCallables`, `TirPresize.IsCountedFill`,
`TirLambdaCaptureRewriter.CapturedRefs`, and the `AllNodes` name-set scans in `TirLoopLowerer.Scopes`,
`TirSharedCalls`, `ComponentReceiverBinding`, `TirMultiConsumerFusion`. Some are intrinsic (move unchanged),
some need a context-dependent fact, some a per-def use index; the lane 1 report sorts them.
