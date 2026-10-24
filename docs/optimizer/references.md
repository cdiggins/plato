# Related compiler ideas

[Overview](README.md)

These are primary references for analogous ideas, not claims that Plato implements the cited
systems or inherits their correctness proofs.

## Inlining and substitution

Simon Peyton Jones and Simon Marlow, [Secrets of the Glasgow Haskell Compiler inliner](https://www.microsoft.com/en-us/research/publication/secrets-of-the-glasgow-haskell-compiler-inliner/),
explains why inlining needs growth controls and coordinated simplification. It provides context
for [TirInliner](inlining.md)'s body-size and recursion limits; Plato's rules are narrower.

Maclaurin, Radul and Paszke, [The Foil: Capture-Avoiding Substitution With No Sharp Edges](https://arxiv.org/abs/2210.04729),
discusses safe substitution with explicit names. It is useful background for the distinction
between [parameter substitution](rewrite-infrastructure.md) and binding/name hygiene. Plato does
not implement the Foil's representation.

## Fusion and lowering

The [Stream Fusion: From Lists to Streams to Nothing at All paper](https://citeseerx.ist.psu.edu/document?doi=64d2a65a7d559f9b05570fb0fea8bb4cccd83ae2&repid=rep1&type=pdf)
by Coutts, Leshchinskiy and Stewart presents elimination of intermediate collections through a
stream representation. It supplies context for [fusion](fusion.md) and [consumer-chain lowering](loops.md).
Plato instead recognizes local typed-tree patterns and emits ordinary loops.

## Fixed shapes and loops

[LLVM's transform-pass reference](https://llvm.org/docs/Passes.html) documents scalar replacement
and loop unrolling among its compiler transformations. These are useful comparisons for
[component expansion](component-expansion.md) and [literal expansion](literal-expansion.md).
Plato works from known component metadata and bounded literals; it does not perform LLVM's
general memory and control-flow analyses.

## Materialization

Microsoft's [Intermediate materialization](https://learn.microsoft.com/en-us/dotnet/standard/linq/intermediate-materialization)
explains how materializing a sequence changes its evaluation and memory profile. It helps explain
the original motivation of [the materializer](materialization.md). The comparison must respect
Plato's eager `MapRange` runtime contract.

## Rewriting infrastructure

[MLIR Pattern Rewriting](https://mlir.llvm.org/docs/PatternRewriter/) separates pattern definition
from pattern application and describes rewrite invariants and recursion controls. Compare that
separation with [TirRewrite](rewrite-infrastructure.md), while recognizing that Plato's helper is
a tree-rebuilding utility rather than a general rewrite-driver framework.
