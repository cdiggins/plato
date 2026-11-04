// Folds the constant Count of a fixed-arity struct and unrolls the component arrays built over it (--scalar-arrays).
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// --scalar-arrays: a fixed-arity struct (<c>Vector3D</c>, <c>Number4</c>: an IArrayLike type of two or more
/// uniform fields) has a constant <c>Count</c>, so a rewrite that knows the type can finish what the
/// fusion pass starts. Pass 2b of the C# optimizer, between the callback specializer and
/// <see cref="TirFusion"/> (docs/csharp-optimizer-pipeline.md).
///
///   * <b>Count</b>: <c>Count(v)</c> on a leaf receiver of such a type becomes the integer literal, so
///     <c>v.Count.Range.Map(f)</c> in <c>ComponentArray</c> reaches <see cref="TirLiteralUnroller.UnrollMapRange"/>
///     as <c>3.MapRange(f)</c>.
///   * <b>ComponentArray</b>: <c>ComponentArray(v)</c> is, by its body in numeric-structures.library.plato,
///     the array of <c>v.At(i)</c>. Where a combinator consumes it, it is exposed as the array literal of
///     <c>v.X</c>, <c>v.Y</c>, ... component reads, and the consumer is unrolled by the literal-array rules
///     (<see cref="UnrollPolicy"/>): <c>Map</c>, <c>Zip</c>, <c>Reduce</c>, <c>All</c>, <c>Any</c>.
///   * <b>FromComponents</b>: <c>FromComponents(v, [a, b, c])</c> of the matching arity is the constructor
///     call <c>new T(a, b, c)</c>, which is what the library body computes.
///
/// So <c>MapComponents</c>, <c>ZipComponents</c>, <c>ReduceComponents</c>, <c>MinComponent</c> and their
/// callers become straight-line scalar code with no array. A consumer the rules refuse (a fold whose
/// accumulator would grow past the budget) keeps its <c>ComponentArray</c> call, so the output is correct
/// either way. Evaluation order is the loops' order: element <c>j</c> of a result is computed before
/// element <c>j + 1</c>, and the receivers that are dropped or repeated are leaves, so nothing that can
/// fault is skipped or reordered.
/// </summary>
public static class ConstantCounts
{
    /// <summary>The node count past which a fold stops repeating a twice-read accumulator.</summary>
    public const int AccumulatorBudget = 128;

    public static TirFunction Rewrite(TirFunction tir, PassContext pass)
    {
        if (!pass.On("scalar-arrays") || tir?.Body == null)
            return tir;
        var site = new Rewriter(pass.Compilation);
        var body = TirRewrite.Rewrite(tir.Body, n => n is TirCall c ? site.Visit(c) : n);
        return ReferenceEquals(body, tir.Body)
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private static readonly ConditionalWeakTable<Ara3D.Geometry.Compiler.Compilation, Dictionary<string, IReadOnlyList<ComponentField>>> Tables = new();

    /// <summary>A stored component of a fixed-arity struct: the field and its Plato type.</summary>
    public sealed record ComponentField(string Name, string TypeName);

    /// <summary>The fields of a fixed-arity struct type, or null. Fixed arity means a writer
    /// synthesizes the type's <c>Count</c> (CSharpTypeWriter.GenerateFunc: the interface obligation has no
    /// implementation) as the field count: a concrete struct of two or more fields, not a primitive,
    /// whose <c>Count</c> and <c>At</c> no library function implements. One field is excluded
    /// because a single field that is itself indexable makes the type delegate its Count to it.</summary>
    public static IReadOnlyList<ComponentField> FixedFields(Ara3D.Geometry.Compiler.Compilation compilation, string typeName)
    {
        if (typeName == null)
            return null;
        Dictionary<string, IReadOnlyList<ComponentField>> table;
        lock (Tables)
        {
            if (!Tables.TryGetValue(compilation, out table))
                Tables.Add(compilation, table = BuildTable(compilation));
        }
        return table.TryGetValue(typeName, out var fields) ? fields : null;
    }

    private static Dictionary<string, IReadOnlyList<ComponentField>> BuildTable(Ara3D.Geometry.Compiler.Compilation compilation)
    {
        var table = new Dictionary<string, IReadOnlyList<ComponentField>>();
        foreach (var ct in compilation.ConcreteTypes)
        {
            var fields = ct.TypeDef.Fields;
            if (ct.TypeDef.TypeParameters.Count > 0 || fields.Count < 2 || WriterPrimitiveNames.All.Contains(ct.Name)
                || !ct.UnimplementedFunctions.Any(f => f.Name == "Count") || !ct.UnimplementedFunctions.Any(f => f.Name == "At"))
                continue;
            table[ct.Name] = fields.Select(f => new ComponentField(f.Name, f.Type?.Name)).ToList();
        }
        return table;
    }

    // The stdlib function (identity, not name): a user's own FromComponents or ComponentArray keeps its calls.
    internal static bool IsLibrary(Ara3D.Geometry.Compiler.Compilation compilation, FunctionDef callee, string name)
        => callee != null && callee.Name == name
           && compilation.LibraryDefinitionsByName.TryGetValue("NumericStructures", out var library)
           && library.Functions.Any(f => ReferenceEquals(f, callee));

    /// <summary>The receiver of a library <c>ComponentArray(v)</c> call that <see cref="Rewriter"/> would expose as
    /// component reads if <c>v</c> were a leaf, when <c>v</c> is not one (TKT-1187: the receiver is then bound to a
    /// local first, <see cref="ComponentReceiverBinding"/>); null otherwise.</summary>
    internal static TirNode ComputedReceiver(Ara3D.Geometry.Compiler.Compilation compilation, TirNode arg)
    {
        if (TirRewrite.StripCoerce(arg) is not TirCall { Name: "ComponentArray", Args.Count: 1 } call
            || !IsLibrary(compilation, call.Callee, "ComponentArray"))
            return null;
        var receiver = TirRewrite.StripCoerce(call.Args[0]);
        var fields = FixedFields(compilation, receiver?.Type?.Name);
        var element = TirRewrite.ListElementType(call.Type);
        if (fields == null || fields.Count > TirFusion.UnrollLimit || TirRewrite.IsCheap(receiver)
            || element?.Name == null || fields.Any(f => f.TypeName != element.Name))
            return null;
        return receiver;
    }

    private sealed class Rewriter
    {
        private readonly Ara3D.Geometry.Compiler.Compilation _compilation;
        private readonly FusionContext _ctx;
        private readonly UnrollPolicy _policy;

        public Rewriter(Ara3D.Geometry.Compiler.Compilation compilation)
        {
            _compilation = compilation;
            _ctx = new FusionContext(compilation);
            _policy = new UnrollPolicy(IsRepeatable, AccumulatorBudget);
        }

        public TirNode Visit(TirCall c)
        {
            switch (c.Name)
            {
                case "Count" when c.Args.Count == 1:
                    return FoldCount(c);
                case "Map" when c.Args.Count == 2:
                    return Consume(c, new[] { 0 }, r => TirLiteralUnroller.UnrollMapOverArray(r, _policy));
                case "Zip" when c.Args.Count == 3 || c.Args.Count == 4:
                    return Consume(c, Enumerable.Range(0, c.Args.Count - 1).ToArray(), r => TirLiteralUnroller.UnrollZipOverArray(r, _policy));
                case "Reduce" when c.Args.Count == 3:
                    return Consume(c, new[] { 0 }, r => TirLiteralUnroller.UnrollReduceOverArray(r, _policy));
                case "All" or "Any" when c.Args.Count == 2:
                    return Consume(c, new[] { 0 }, r => TirLiteralUnroller.UnrollAllAnyOverArray(r, _ctx, _policy));
                case "FromComponents" when c.Args.Count == 2:
                    return FromComponents(c);
                default:
                    return c;
            }
        }

        private TirNode FoldCount(TirCall c)
        {
            var receiver = TirRewrite.StripCoerce(c.Args[0]);
            var fields = FixedFields(_compilation, receiver?.Type?.Name);
            // A leaf receiver only: a computed one might fault, and folding would drop that.
            // A Count with a body of its own is a user's, not the synthesized field count.
            if (fields == null || !TirRewrite.IsCheap(receiver) || c.Callee?.Body != null)
                return c;
            return _ctx.Integer(fields.Count, c.Origin);
        }

        // The call with its array sources replaced by array literals, unrolled; the call itself when
        // some source is neither a literal nor an exposable ComponentArray or the rules refuse.
        private TirNode Consume(TirCall c, int[] sources, System.Func<TirCall, TirNode> unroll)
        {
            var args = c.Args.ToList();
            foreach (var i in sources)
            {
                var literal = Literal(args[i]);
                if (literal == null)
                    return c;
                args[i] = literal;
            }
            var rebuilt = new TirCall(c.Callee, c.EmissionKind, c.ParameterTypes, c.ReturnType, args, c.Type, c.Origin, c.Name);
            return unroll(rebuilt) ?? c;
        }

        private TirNode FromComponents(TirCall c)
        {
            var receiver = TirRewrite.StripCoerce(c.Args[0]);
            var typeName = receiver?.Type?.Name;
            var fields = FixedFields(_compilation, typeName);
            if (fields == null || !TirRewrite.IsCheap(receiver) || c.Type?.Name != typeName
                || !IsLibrary(c.Callee, "FromComponents") || Literal(c.Args[1]) is not { } array
                || array.Elements.Count != fields.Count)
                return c;
            return new TirConstructorCall(typeName, array.Elements, c.Type);
        }

        // A small array literal as it stands, or the literal of component reads of a ComponentArray call.
        private TirArray Literal(TirNode arg)
        {
            var n = TirRewrite.StripCoerce(arg);
            if (n is TirArray literal)
                return literal.Elements.Count <= TirFusion.UnrollLimit ? literal : null;
            if (n is TirCall { Name: "Map", Args.Count: 2 } map && ExpandedComponentArray(map) is { } inlined)
                return inlined;
            if (n is not TirCall { Name: "ComponentArray", Args.Count: 1 } call || !IsLibrary(call.Callee, "ComponentArray"))
                return null;
            var receiver = TirRewrite.StripCoerce(call.Args[0]);
            var typeName = receiver?.Type?.Name;
            var fields = FixedFields(_compilation, typeName);
            if (fields == null || fields.Count > TirFusion.UnrollLimit || !TirRewrite.IsCheap(receiver))
                return null;
            // The components are read as they are stored, so each must already be the array's element type.
            var element = TirRewrite.ListElementType(call.Type);
            if (element?.Name == null || fields.Any(f => f.TypeName != element.Name))
                return null;
            var reads = fields.Select(f => (TirNode)new TirComponentAccess(receiver, f.Name, null, null, element)).ToList();
            return new TirArray(reads, call.Type, call.Origin);
        }

        // ComponentArray after the inliner has expanded it: <c>v.Count.Range.Map(i => v[i])</c>, whose Count is now the
        // literal arity. (The C++ pipeline inlines first; the C# one usually keeps the call.) The same literal of component reads.
        private TirArray ExpandedComponentArray(TirCall map)
        {
            if (TirRewrite.StripCoerce(map.Args[0]) is not TirCall { Name: "MapRange", Args.Count: 2 } range)
                return null;
            var k = TirFusion.SmallLiteral(range.Args[0]);
            if (k <= 0
                || TirRewrite.StripCoerce(range.Args[1]) is not TirLambda { Parameters.Count: 1 } identity
                || TirRewrite.StripCoerce(identity.Body) is not TirParameter ip || !ReferenceEquals(ip.Def, identity.Parameters[0])
                || TirRewrite.StripCoerce(map.Args[1]) is not TirLambda { Parameters.Count: 1 } read
                || TirRewrite.StripCoerce(read.Body) is not TirCall { Name: "At", Args.Count: 2 } at || at.Callee?.Body != null
                || TirRewrite.StripCoerce(at.Args[1]) is not TirParameter index || !ReferenceEquals(index.Def, read.Parameters[0]))
                return null;
            var receiver = TirRewrite.StripCoerce(at.Args[0]);
            var fields = FixedFields(_compilation, receiver?.Type?.Name);
            var element = TirRewrite.ListElementType(map.Type);
            if (fields == null || fields.Count != k || !TirRewrite.IsCheap(receiver) || element?.Name == null
                || fields.Any(f => f.TypeName != element.Name))
                return null;
            return new TirArray(fields.Select(f => (TirNode)new TirComponentAccess(receiver, f.Name, null, null, element)).ToList(), map.Type, map.Origin);
        }

        private bool IsLibrary(FunctionDef callee, string name) => ConstantCounts.IsLibrary(_compilation, callee, name);

        // A leaf, or a component read of one: repeating it re-reads a field.
        private static bool IsRepeatable(TirNode n)
            => TirRewrite.IsCheap(n)
               || (TirRewrite.StripCoerce(n) is TirComponentAccess access && TirRewrite.IsCheap(access.Receiver));
    }
}
