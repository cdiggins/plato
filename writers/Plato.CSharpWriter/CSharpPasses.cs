// The optimizer passes that need the C# writer's type knowledge, registered by name beside the shared ones.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Optimizer;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// C#-only passes. Each reads the writer from <see cref="PassContext.Backend"/>: statement inlining (budgets and the
/// C# exposing rewrites), callback specialization (clones written through <see cref="CSharpTypeWriter"/>), constant
/// counts (C# struct layouts) and component unrolling (the C# extension plans' component fields). They stay here
/// until a backend-neutral form exists; the C# writer's <see cref="CSharpWriter.PassOrder"/> names them.
/// </summary>
public static class CSharpPasses
{
    /// <summary>--inline-policy (experimental): statement and callable expansion.</summary>
    public const string InlineStatements = "inline-statements";
    /// <summary>--specialize-callbacks: a lambda passed to a function-typed parameter calls a clone of the callee.</summary>
    public const string SpecializeCallbacks = "specialize-callbacks";
    /// <summary>--optimize: component-op unrolling on known IArrayLike types.</summary>
    public const string ComponentUnroll = "component-unroll";

    private static CSharpWriter Writer(PassContext ctx) => (CSharpWriter)ctx.Backend;

    private static TirFunction RunInlineStatements(TirFunction tir, PassContext ctx)
    {
        var w = Writer(ctx);
        return w.InlineStatements
            ? TirInliner.InlineStatements(tir, w, ctx.OwnerTypeName, w.EliminateKnownLambdas, w.SimplifyInlineStatements, ctx.FunctionInfo, ctx.Facts)
            : tir;
    }

    public static ITirPass[] All { get; } =
    {
        new TirPass(InlineStatements, "experimental statement / callable expansion", RunInlineStatements),
        new TirPass(SpecializeCallbacks, "specialize-callbacks / clone the callee per lambda shape",
            (tir, ctx) => TirCallbackSpecializer.Rewrite(tir, Writer(ctx), ctx.OwnerTypeName, ctx.Facts)),
        new TirPass(ComponentUnroll, "optimize / component-unroll (layer 8)",
            (tir, ctx) => TirComponentUnroller.UnrollFunction(tir, (CSharpFunctionInfo)ctx.FunctionInfo, Writer(ctx))),
    };
}
