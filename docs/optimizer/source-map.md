# Optimizer source map

[Overview](README.md)

Generated from dotnet-greenhouse's Roslyn documentation export. These are public
symbols, not a complete call graph or a substitute for the component explanations.
Private algorithms are reached through the linked implementation files. Source line
links refer to this checkout's reviewed snapshot.

## BooleanChainExpression

[`Ara3D.Geometry.CSharpWriter.BooleanChainExpression`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L109)

- [`Ara3D.Geometry.CSharpWriter.BooleanChainExpression.Name`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L121)
- [`Ara3D.Geometry.CSharpWriter.BooleanChainExpression.Op`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L111)
- [`Ara3D.Geometry.CSharpWriter.BooleanChainExpression.Rewrite(System.Func<Ara3D.Geometry.Compiler.Symbols.Symbol, Ara3D.Geometry.Compiler.Symbols.Symbol>)`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L124)
- [`Ara3D.Geometry.CSharpWriter.BooleanChainExpression.Terms`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L112)
- [`Ara3D.Geometry.CSharpWriter.BooleanChainExpression.ToString()`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L122)

## ComponentAccessExpression

[`Ara3D.Geometry.CSharpWriter.ComponentAccessExpression`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L59)

- [`Ara3D.Geometry.CSharpWriter.ComponentAccessExpression.CastTo`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L63)
- [`Ara3D.Geometry.CSharpWriter.ComponentAccessExpression.FieldName`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L62)
- [`Ara3D.Geometry.CSharpWriter.ComponentAccessExpression.Name`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L78)
- [`Ara3D.Geometry.CSharpWriter.ComponentAccessExpression.Receiver`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L61)
- [`Ara3D.Geometry.CSharpWriter.ComponentAccessExpression.Rewrite(System.Func<Ara3D.Geometry.Compiler.Symbols.Symbol, Ara3D.Geometry.Compiler.Symbols.Symbol>)`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L81)
- [`Ara3D.Geometry.CSharpWriter.ComponentAccessExpression.ScalarComponentPrim`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L67)
- [`Ara3D.Geometry.CSharpWriter.ComponentAccessExpression.ToString()`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L79)

## ComponentUnroller

[`Ara3D.Geometry.CSharpWriter.ComponentUnroller`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L128)

- [`Ara3D.Geometry.CSharpWriter.ComponentUnroller.BuildComponentFieldTable(Ara3D.Geometry.Compiler.Compilation)`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L145)
- [`Ara3D.Geometry.CSharpWriter.ComponentUnroller.TryUnroll(Ara3D.Geometry.CSharpWriter.CSharpFunctionInfo, Ara3D.Geometry.CSharpWriter.CSharpWriter)`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L181)

## ConstructorCallExpression

[`Ara3D.Geometry.CSharpWriter.ConstructorCallExpression`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L87)

- [`Ara3D.Geometry.CSharpWriter.ConstructorCallExpression.Args`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L90)
- [`Ara3D.Geometry.CSharpWriter.ConstructorCallExpression.Name`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L99)
- [`Ara3D.Geometry.CSharpWriter.ConstructorCallExpression.Rewrite(System.Func<Ara3D.Geometry.Compiler.Symbols.Symbol, Ara3D.Geometry.Compiler.Symbols.Symbol>)`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L102)
- [`Ara3D.Geometry.CSharpWriter.ConstructorCallExpression.ToString()`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L100)
- [`Ara3D.Geometry.CSharpWriter.ConstructorCallExpression.TypeName`](../../writers/Plato.CSharpWriter/Optimization/ComponentUnroller.cs#L89)

## FusionContext

[`Ara3D.Geometry.CSharpWriter.FusionContext`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L248)

- [`Ara3D.Geometry.CSharpWriter.FusionContext.At(Ara3D.Geometry.Compiler.Checking.TirNode, Ara3D.Geometry.Compiler.Checking.TirNode, Ara3D.Geometry.Compiler.Symbols.Symbol)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L277)
- [`Ara3D.Geometry.CSharpWriter.FusionContext.Boolean(bool, Ara3D.Geometry.Compiler.Symbols.Symbol)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L289)
- [`Ara3D.Geometry.CSharpWriter.FusionContext.BooleanType`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L252)
- [`Ara3D.Geometry.CSharpWriter.FusionContext.Call(string, System.Collections.Generic.IReadOnlyList<Ara3D.Geometry.Compiler.Checking.TirNode>, Ara3D.Geometry.Compiler.Symbols.TypeExpression, Ara3D.Geometry.Compiler.Symbols.Symbol)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L283)
- [`Ara3D.Geometry.CSharpWriter.FusionContext.FreshIndexParam()`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L261)
- [`Ara3D.Geometry.CSharpWriter.FusionContext.Integer(long, Ara3D.Geometry.Compiler.Symbols.Symbol)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L286)
- [`Ara3D.Geometry.CSharpWriter.FusionContext.IntegerType`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L251)
- [`Ara3D.Geometry.CSharpWriter.FusionContext.Lambda(System.Collections.Generic.IReadOnlyList<Ara3D.Geometry.Compiler.Symbols.ParameterDef>, Ara3D.Geometry.Compiler.Checking.TirNode, Ara3D.Geometry.Compiler.Symbols.TypeExpression)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L266)
- [`Ara3D.Geometry.CSharpWriter.FusionContext.MapRange(Ara3D.Geometry.Compiler.Checking.TirNode, Ara3D.Geometry.Compiler.Checking.TirNode, Ara3D.Geometry.Compiler.Symbols.TypeExpression, Ara3D.Geometry.Compiler.Symbols.Symbol)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L274)

## ITirInlineHost

[`Ara3D.Geometry.CSharpWriter.ITirInlineHost`](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs#L12)

- [`Ara3D.Geometry.CSharpWriter.ITirInlineHost.Compilation`](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs#L19)
- [`Ara3D.Geometry.CSharpWriter.ITirInlineHost.InlineCalls`](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs#L14)
- [`Ara3D.Geometry.CSharpWriter.ITirInlineHost.InlineReport`](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs#L17)
- [`Ara3D.Geometry.CSharpWriter.ITirInlineHost.IsConcreteTypeName(string)`](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs#L26)
- [`Ara3D.Geometry.CSharpWriter.ITirInlineHost.IsScalarPrimitiveName(string)`](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs#L29)
- [`Ara3D.Geometry.CSharpWriter.ITirInlineHost.TryGetGroundTirByTypeName(Ara3D.Geometry.Compiler.Symbols.FunctionDef, string)`](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs#L21)
- [`Ara3D.Geometry.CSharpWriter.ITirInlineHost.TryGetKnownMemberNames(string)`](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs#L35)
- [`Ara3D.Geometry.CSharpWriter.ITirInlineHost.TryGetStaticTir(Ara3D.Geometry.Compiler.Symbols.FunctionDef)`](../../writers/Plato.CSharpWriter/Optimization/ITirInlineHost.cs#L23)

## InlineRefusal

[`Ara3D.Geometry.CSharpWriter.InlineRefusal`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L12)

- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.ArgTypeMismatch`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L20)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.ArityMismatch`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L23)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.InsideLambda`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L19)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.LambdaArgRefused`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L21)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.MultiUseCompound`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L22)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.NoCalleeBody`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L14)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.NoGroundTir`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L15)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.NotSelfContained`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L18)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.NullaryRefused`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L24)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.ReturnTypeMismatch`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L17)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.SelfRecursive`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L25)
- [`Ara3D.Geometry.CSharpWriter.InlineRefusal.StatementBody`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L16)

## InlineReport

[`Ara3D.Geometry.CSharpWriter.InlineReport`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L31)

- [`Ara3D.Geometry.CSharpWriter.InlineReport.BetaReductions`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L38)
- [`Ara3D.Geometry.CSharpWriter.InlineReport.CallsInlined`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L37)
- [`Ara3D.Geometry.CSharpWriter.InlineReport.Refuse(string, string, Ara3D.Geometry.CSharpWriter.InlineRefusal)`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L41)
- [`Ara3D.Geometry.CSharpWriter.InlineReport.ToTable()`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L50)
- [`Ara3D.Geometry.CSharpWriter.InlineReport.TupleConstructorRewrites`](../../writers/Plato.CSharpWriter/Optimization/InlineReport.cs#L39)

## TirArrayMaterializer

[`Ara3D.Geometry.CSharpWriter.TirArrayMaterializer`](../../writers/Plato.CSharpWriter/Optimization/TirArrayMaterializer.cs#L26)

- [`Ara3D.Geometry.CSharpWriter.TirArrayMaterializer.Rewrite(Ara3D.Geometry.Compiler.Checking.TirFunction, Ara3D.Geometry.CSharpWriter.CSharpWriter)`](../../writers/Plato.CSharpWriter/Optimization/TirArrayMaterializer.cs#L30)

## TirBooleanChain

[`Ara3D.Geometry.CSharpWriter.TirBooleanChain`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L84)

- [`Ara3D.Geometry.CSharpWriter.TirBooleanChain.Children`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L96)
- [`Ara3D.Geometry.CSharpWriter.TirBooleanChain.Op`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L86)
- [`Ara3D.Geometry.CSharpWriter.TirBooleanChain.Terms`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L87)
- [`Ara3D.Geometry.CSharpWriter.TirBooleanChain.ToString()`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L99)
- [`Ara3D.Geometry.CSharpWriter.TirBooleanChain.WithChildren(System.Collections.Generic.IReadOnlyList<Ara3D.Geometry.Compiler.Checking.TirNode>)`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L97)

## TirComponentAccess

[`Ara3D.Geometry.CSharpWriter.TirComponentAccess`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L35)

- [`Ara3D.Geometry.CSharpWriter.TirComponentAccess.CastTo`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L39)
- [`Ara3D.Geometry.CSharpWriter.TirComponentAccess.Children`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L56)
- [`Ara3D.Geometry.CSharpWriter.TirComponentAccess.FieldName`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L38)
- [`Ara3D.Geometry.CSharpWriter.TirComponentAccess.Receiver`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L37)
- [`Ara3D.Geometry.CSharpWriter.TirComponentAccess.ScalarComponentPrim`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L45)
- [`Ara3D.Geometry.CSharpWriter.TirComponentAccess.ToString()`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L59)
- [`Ara3D.Geometry.CSharpWriter.TirComponentAccess.WithChildren(System.Collections.Generic.IReadOnlyList<Ara3D.Geometry.Compiler.Checking.TirNode>)`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L57)

## TirComponentUnroller

[`Ara3D.Geometry.CSharpWriter.TirComponentUnroller`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L102)

- [`Ara3D.Geometry.CSharpWriter.TirComponentUnroller.TryUnroll(Ara3D.Geometry.Compiler.Checking.TirNode, Ara3D.Geometry.CSharpWriter.CSharpFunctionInfo, Ara3D.Geometry.CSharpWriter.CSharpWriter)`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L137)
- [`Ara3D.Geometry.CSharpWriter.TirComponentUnroller.UnrollFunction(Ara3D.Geometry.Compiler.Checking.TirFunction, Ara3D.Geometry.CSharpWriter.CSharpFunctionInfo, Ara3D.Geometry.CSharpWriter.CSharpWriter)`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L106)

## TirConstructorCall

[`Ara3D.Geometry.CSharpWriter.TirConstructorCall`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L64)

- [`Ara3D.Geometry.CSharpWriter.TirConstructorCall.Args`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L67)
- [`Ara3D.Geometry.CSharpWriter.TirConstructorCall.Children`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L76)
- [`Ara3D.Geometry.CSharpWriter.TirConstructorCall.ToString()`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L79)
- [`Ara3D.Geometry.CSharpWriter.TirConstructorCall.TypeName`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L66)
- [`Ara3D.Geometry.CSharpWriter.TirConstructorCall.WithChildren(System.Collections.Generic.IReadOnlyList<Ara3D.Geometry.Compiler.Checking.TirNode>)`](../../writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs#L77)

## TirFusion

[`Ara3D.Geometry.CSharpWriter.TirFusion`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L38)

- [`Ara3D.Geometry.CSharpWriter.TirFusion.CanSubstitute(Ara3D.Geometry.Compiler.Checking.TirNode, Ara3D.Geometry.Compiler.Symbols.ParameterDef, Ara3D.Geometry.Compiler.Checking.TirNode)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L215)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.ComposeMaps(Ara3D.Geometry.Compiler.Checking.TirCall, Ara3D.Geometry.CSharpWriter.FusionContext)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L128)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.FuseMapIntoConsumer(Ara3D.Geometry.Compiler.Checking.TirCall, Ara3D.Geometry.CSharpWriter.FusionContext, int)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L146)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.IsInteger(Ara3D.Geometry.Compiler.Checking.TirNode)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L229)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.IsMapRange(Ara3D.Geometry.Compiler.Checking.TirNode, out Ara3D.Geometry.Compiler.Checking.TirCall)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L223)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.Lambda(Ara3D.Geometry.Compiler.Checking.TirNode, int)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L199)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.NextId`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L42)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.RangeToMapRange(Ara3D.Geometry.Compiler.Checking.TirCall, Ara3D.Geometry.CSharpWriter.FusionContext)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L101)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.Rewrite(Ara3D.Geometry.Compiler.Checking.TirFunction, Ara3D.Geometry.CSharpWriter.CSharpWriter)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L47)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.SmallLiteral(Ara3D.Geometry.Compiler.Checking.TirNode)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L233)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.TakeToMapRange(Ara3D.Geometry.Compiler.Checking.TirCall, Ara3D.Geometry.CSharpWriter.FusionContext)`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L112)
- [`Ara3D.Geometry.CSharpWriter.TirFusion.UnrollLimit`](../../writers/Plato.CSharpWriter/Optimization/TirFusion.cs#L45)

## TirInliner

[`Ara3D.Geometry.CSharpWriter.TirInliner`](../../writers/Plato.CSharpWriter/Optimization/TirInliner.cs#L40)

- [`Ara3D.Geometry.CSharpWriter.TirInliner.Inline(Ara3D.Geometry.Compiler.Checking.TirFunction, Ara3D.Geometry.CSharpWriter.ITirInlineHost, string, out int)`](../../writers/Plato.CSharpWriter/Optimization/TirInliner.cs#L51)
- [`Ara3D.Geometry.CSharpWriter.TirInliner.NextRenameId`](../../writers/Plato.CSharpWriter/Optimization/TirInliner.cs#L49)

## TirLiteralUnroller

[`Ara3D.Geometry.CSharpWriter.TirLiteralUnroller`](../../writers/Plato.CSharpWriter/Optimization/TirLiteralUnroller.cs#L26)

- [`Ara3D.Geometry.CSharpWriter.TirLiteralUnroller.UnrollAllAnyOverArray(Ara3D.Geometry.Compiler.Checking.TirCall, Ara3D.Geometry.CSharpWriter.FusionContext)`](../../writers/Plato.CSharpWriter/Optimization/TirLiteralUnroller.cs#L63)
- [`Ara3D.Geometry.CSharpWriter.TirLiteralUnroller.UnrollMapOverArray(Ara3D.Geometry.Compiler.Checking.TirCall)`](../../writers/Plato.CSharpWriter/Optimization/TirLiteralUnroller.cs#L47)
- [`Ara3D.Geometry.CSharpWriter.TirLiteralUnroller.UnrollMapRange(Ara3D.Geometry.Compiler.Checking.TirCall, Ara3D.Geometry.CSharpWriter.FusionContext)`](../../writers/Plato.CSharpWriter/Optimization/TirLiteralUnroller.cs#L28)
- [`Ara3D.Geometry.CSharpWriter.TirLiteralUnroller.UnrollReduceOverArray(Ara3D.Geometry.Compiler.Checking.TirCall)`](../../writers/Plato.CSharpWriter/Optimization/TirLiteralUnroller.cs#L81)

## TirLoopLowerer

[`Ara3D.Geometry.CSharpWriter.TirLoopLowerer`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L93)

- [`Ara3D.Geometry.CSharpWriter.TirLoopLowerer.IsRenderableConcrete(Ara3D.Geometry.Compiler.Symbols.TypeExpression)`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L385)
- [`Ara3D.Geometry.CSharpWriter.TirLoopLowerer.NextId`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L98)
- [`Ara3D.Geometry.CSharpWriter.TirLoopLowerer.ProducesArray(string)`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L374)
- [`Ara3D.Geometry.CSharpWriter.TirLoopLowerer.Rewrite(Ara3D.Geometry.Compiler.Checking.TirFunction, Ara3D.Geometry.CSharpWriter.CSharpWriter)`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L100)

## TirLoweredLoop

[`Ara3D.Geometry.CSharpWriter.TirLoweredLoop`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L12)

- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.Children`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L45)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.ElemType`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L30)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.Fn`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L16)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.Id`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L27)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.IncludeFirst`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L18)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.IndexSource`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L25)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.Kind`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L14)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.Seed`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L17)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.SourceStages`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L24)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.Sources`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L15)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.TempName`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L26)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.ToString()`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L59)
- [`Ara3D.Geometry.CSharpWriter.TirLoweredLoop.WithChildren(System.Collections.Generic.IReadOnlyList<Ara3D.Geometry.Compiler.Checking.TirNode>)`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L48)

## TirRewrite

[`Ara3D.Geometry.CSharpWriter.TirRewrite`](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs#L32)

- [`Ara3D.Geometry.CSharpWriter.TirRewrite.CountParamUses(Ara3D.Geometry.Compiler.Checking.TirNode)`](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs#L178)
- [`Ara3D.Geometry.CSharpWriter.TirRewrite.IsCheap(Ara3D.Geometry.Compiler.Checking.TirNode)`](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs#L169)
- [`Ara3D.Geometry.CSharpWriter.TirRewrite.IsListType(Ara3D.Geometry.Compiler.Symbols.TypeExpression)`](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs#L153)
- [`Ara3D.Geometry.CSharpWriter.TirRewrite.IsStatementNode(Ara3D.Geometry.Compiler.Checking.TirNode)`](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs#L164)
- [`Ara3D.Geometry.CSharpWriter.TirRewrite.ListElementType(Ara3D.Geometry.Compiler.Symbols.TypeExpression)`](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs#L157)
- [`Ara3D.Geometry.CSharpWriter.TirRewrite.Rewrite(Ara3D.Geometry.Compiler.Checking.TirNode, System.Func<Ara3D.Geometry.Compiler.Checking.TirNode, Ara3D.Geometry.Compiler.Checking.TirNode>, System.Action<Ara3D.Geometry.Compiler.Checking.TirNode>, System.Action<Ara3D.Geometry.Compiler.Checking.TirNode>)`](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs#L37)
- [`Ara3D.Geometry.CSharpWriter.TirRewrite.StripCoerce(Ara3D.Geometry.Compiler.Checking.TirNode)`](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs#L145)
- [`Ara3D.Geometry.CSharpWriter.TirRewrite.Substitute(Ara3D.Geometry.Compiler.Checking.TirNode, System.Collections.Generic.IReadOnlyDictionary<Ara3D.Geometry.Compiler.Symbols.ParameterDef, Ara3D.Geometry.Compiler.Checking.TirNode>)`](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs#L98)
- [`Ara3D.Geometry.CSharpWriter.TirRewrite.TryBetaReduce(Ara3D.Geometry.Compiler.Checking.TirInvoke, out Ara3D.Geometry.Compiler.Checking.TirNode, System.Func<Ara3D.Geometry.Compiler.Checking.TirNode, bool>)`](../../writers/Plato.CSharpWriter/Optimization/TirRewrite.cs#L114)

## TirTempRef

[`Ara3D.Geometry.CSharpWriter.TirTempRef`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L63)

- [`Ara3D.Geometry.CSharpWriter.TirTempRef.Name`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L65)
- [`Ara3D.Geometry.CSharpWriter.TirTempRef.ToString()`](../../writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs#L67)
