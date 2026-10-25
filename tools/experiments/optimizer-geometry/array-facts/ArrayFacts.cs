using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;

namespace ArrayFactsExperiment;

// Report-only: all facts are tied to completed producers, never source spelling.
public sealed class ArrayFacts
{
    public sealed record Fact(TirLoweredLoop Producer);
    public sealed record Observation(TirCall Count, VariableDef Alias, Fact Fact);

    private readonly Compilation _compilation;
    private readonly List<Observation> _observations = new();
    private readonly List<string> _refusals = new();
    private readonly Dictionary<VariableDef, Fact> _aliases = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<Symbol> _ambiguousOrigins = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<string> _ambiguousTemps = new(StringComparer.Ordinal);

    public IReadOnlyList<Observation> Observations => _observations;
    public IReadOnlyList<string> Refusals => _refusals;

    public ArrayFacts(Compilation compilation)
        => _compilation = compilation;

    public void Analyze(TirNode body)
    {
        _observations.Clear();
        _refusals.Clear();
        _aliases.Clear();
        _ambiguousOrigins.Clear();
        _ambiguousTemps.Clear();
        foreach (var group in body.Descendants().OfType<TirLoweredLoop>()
                     .Where(x => x.Origin != null).GroupBy(x => x.Origin, ReferenceEqualityComparer.Instance))
            if (group.Count() != 1)
                _ambiguousOrigins.Add((Symbol)group.Key);
        foreach (var group in body.Descendants().OfType<TirLoweredLoop>().GroupBy(x => x.TempName))
            if (group.Count() != 1)
                _ambiguousTemps.Add(group.Key);
        Block(body);
    }

    private bool Canonical(TirLoweredLoop loop)
        => loop.Kind == "MapRange" && loop.Sources.Count == 1
            && loop.SourceStages.Count == 0 && !loop.IndexSource
            && loop.Seed == null && loop.IncludeFirst == null
            && loop.Origin != null && !_ambiguousOrigins.Contains(loop.Origin)
            && !string.IsNullOrEmpty(loop.TempName) && !_ambiguousTemps.Contains(loop.TempName)
            && TirIntrinsicIdentity.IsIntrinsic(loop.SourceFunction, _compilation, "MapRange")
            && TirRewrite.IsListType(loop.Type) && loop.ElemType != null
            && loop.ElemType.Equals(TirRewrite.ListElementType(loop.Type))
            && TirLoopLowerer.IsRenderableConcrete(loop.ElemType)
            && loop.Sources[0] is TirParameter or TirLiteral
            && ReferenceEquals(loop.Sources[0].Type?.Def, _compilation.TypeDefinitionsByName["Integer"]);

    private void Invalidate(string reason)
    {
        if (_aliases.Count != 0)
            _refusals.Add(reason);
        _aliases.Clear();
    }

    private void Block(TirNode node)
    {
        // Do not import facts into children or join them back out of control flow.
        _aliases.Clear();
        var statements = node is TirBlock block ? block.Statements : new[] { node };
        TirLoweredLoop pending = null;
        foreach (var statement in statements)
        {
            if (statement is TirLoweredLoop loop)
            {
                Invalidate("producer evaluation may invoke unknown or captured callbacks");
                pending = Canonical(loop) ? loop : null;
                if (pending == null)
                    _refusals.Add("noncanonical or unsupported producer");
                continue;
            }
            if (statement is TirLet let && let.Value is TirTempRef temp && pending != null
                && ReferenceEquals(temp.Origin, pending.Origin) && temp.Type.Equals(pending.Type)
                // TirTempRef stores emitted binding linkage, not a VariableDef/token.
                && string.Equals(temp.Name, pending.TempName, StringComparison.Ordinal)
                && let.Type.Equals(pending.Type) && let.Def != null)
            {
                _aliases[let.Def] = new Fact(pending);
                pending = null;
                continue;
            }
            pending = null;
            if (statement is TirLet alias && alias.Value is TirVariable source
                && _aliases.TryGetValue(source.Def, out var fact)
                && alias.Type.Equals(source.Type) && alias.Def != null)
            {
                _aliases[alias.Def] = fact;
                continue;
            }
            var value = statement is TirLet binding ? binding.Value
                : statement is TirReturn ret ? ret.Value : statement;
            if (value is TirCall call && IsCount(call))
            {
                if (call.Args[0] is TirVariable variable && _aliases.TryGetValue(variable.Def, out fact))
                    _observations.Add(new Observation(call, variable.Def, fact));
                else
                    _refusals.Add("Count has no completed owned producer in this scope");
                continue;
            }
            // Includes lambda creation, not only invocation: a captured alias must not survive.
            var captures = value.Descendants().OfType<TirLambda>().Any(lambda =>
                lambda.Descendants().OfType<TirVariable>().Any(v => _aliases.ContainsKey(v.Def)));
            Invalidate(captures ? "capture of an owned alias" : "unknown evaluation, conversion, assignment or scope");
            if (statement is TirBlock nested)
                Block(nested);
            _aliases.Clear();
        }
        _aliases.Clear();
    }

    private bool IsCount(TirCall call)
        => TirIntrinsicIdentity.IsIntrinsic(call.Callee, _compilation, "Count")
            && call.Args.Count == 1 && call.Args[0] is TirVariable or TirParameter
            && TirRewrite.IsListType(call.Args[0].Type)
            && ReferenceEquals(call.Type?.Def, _compilation.TypeDefinitionsByName["Integer"]);
}
