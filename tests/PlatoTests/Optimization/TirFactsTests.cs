// compiler-546: TirFacts computes a subtree fact once per distinct node and agrees with a fresh walk of the tree.
using System.Text;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Optimizer;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// The DAG test builds the shared-subtree chain of the 2026-10-25 probe (both arms of level k are the one node of
/// level k-1): a <c>Descendants()</c> walk visits 3·2^20 − 2 nodes, the table computes 41. The equivalence tests take
/// every function of the shared-calls and decoder-rewrite fixtures, before and after the passes, and check each fact
/// against the walk it replaces; the structural key is checked against a copy of the one-pass writer the shared-calls
/// pass had before this table.
/// </summary>
[TestFixture]
public static class TirFactsTests
{
    private const int Depth = 20;

    private static TirNode SharedChain(out int distinct)
    {
        TirNode node = new TirLiteral(0, LiteralTypesEnum.Integer, null, null);
        distinct = 1;
        for (var k = 0; k < Depth; k++)
        {
            var condition = new TirLiteral(true, LiteralTypesEnum.Boolean, null, null);
            node = new TirConditional(condition, node, node, null, null);
            distinct += 2;
        }
        return node;
    }

    [Test]
    public static void SharedSubtreeIsSizedOncePerDistinctNode()
    {
        var root = SharedChain(out var distinct);
        var facts = new TirFacts();
        Assert.That(facts.Size(root), Is.EqualTo(3 * (1 << Depth) - 2), "the size counts a shared node once per path, as Descendants() does");
        Assert.That(facts.NodesVisited, Is.EqualTo(distinct), "but each distinct node is computed once");
        Assert.That(distinct, Is.EqualTo(41));
        TestContext.Out.WriteLine($"Size {facts.Size(root)}, NodesVisited {facts.NodesVisited}, distinct {distinct}");
    }

    [Test]
    public static void OtherFactsAreLinearOnTheSharedChainToo()
    {
        var root = SharedChain(out var distinct);
        var facts = new TirFacts();
        Assert.That(facts.ContainsAssign(root), Is.False);
        Assert.That(facts.ContainsLoop(root), Is.False);
        Assert.That(facts.MentionedDefs(root), Is.Empty);
        Assert.That(facts.NodesVisited, Is.LessThanOrEqualTo(3 * distinct), "three facts, each at most once per node");
    }

    /// <summary>The shared chain over a leaf, so that every path ends at one definition: <paramref name="paths"/>
    /// is 2^<see cref="Depth"/>.</summary>
    private static TirNode SharedChainOver(TirNode leaf, out int distinct, out int paths)
    {
        var node = leaf;
        distinct = 1;
        for (var k = 0; k < Depth; k++)
        {
            node = new TirConditional(new TirLiteral(true, LiteralTypesEnum.Boolean, null, null), node, node, null, null);
            distinct += 2;
        }
        paths = 1 << Depth;
        return node;
    }

    [Test]
    public static void BoundNamesOfASharedChainAreLinear()
    {
        var def = new VariableDef(null, "x", null, null);
        var root = SharedChainOver(new TirVariable(def, null, null), out var distinct, out _);
        var facts = new TirFacts();
        Assert.That(facts.BoundNames(root), Is.EquivalentTo(new[] { "x" }));
        Assert.That(facts.NodesVisited, Is.EqualTo(distinct));
        Assert.That(facts.BoundNames(root), Has.Count.EqualTo(1));
        Assert.That(facts.NodesVisited, Is.EqualTo(distinct), "a repeated request is a cache hit");
    }

    [Test]
    public static void UsesOfASharedChainCountEveryPathButVisitOnce()
    {
        var def = new VariableDef(null, "x", null, null);
        var root = SharedChainOver(new TirVariable(def, null, null), out var distinct, out var paths);
        var facts = new TirFacts();
        Assert.That(facts.Uses(root, def), Is.EqualTo(new TirUse(paths, false, false)), "a shared use counts once per path, as a walk counts it");
        Assert.That(facts.NodesVisited, Is.EqualTo(distinct));
        var lambda = new TirLambda(new List<ParameterDef>(), root, null, null);
        Assert.That(facts.Uses(lambda, def), Is.EqualTo(new TirUse(paths, true, false)), "a lambda root puts its body under the lambda");
        var loop = new TirLoop(new TirLiteral(true, LiteralTypesEnum.Boolean, null, null), root, null);
        Assert.That(facts.Uses(loop, def), Is.EqualTo(new TirUse(paths, false, true)), "a loop root puts its body under the loop");
        Assert.That(facts.NodesVisited, Is.EqualTo(distinct + 3), "the two new roots and the loop's condition");
        Assert.That(facts.Uses(root, new VariableDef(null, "y", null, null)), Is.EqualTo(default(TirUse)));
    }

    [Test]
    public static void AssignmentsOfASharedChainCountEveryPathButVisitOnce()
    {
        var def = new VariableDef(null, "x", null, null);
        var assign = new TirAssign(new TirVariable(def, null, null), new TirLiteral(0, LiteralTypesEnum.Integer, null, null), null, null);
        var root = SharedChainOver(assign, out var distinct, out var paths);
        var facts = new TirFacts();
        Assert.That(facts.Assigns(root, def), Is.EqualTo(new TirUse(paths, false, false)));
        Assert.That(facts.AssignedDefs(root), Is.EquivalentTo(new object[] { def }));
        Assert.That(facts.NodesVisited, Is.EqualTo(distinct + 2), "the assignment's two children are nodes too");
        Assert.That(facts.Uses(root, def).Count, Is.EqualTo(paths), "the target of an assignment is an occurrence");
    }

    [Test]
    public static void StructuralKeyOfASharedCallChainIsLinear()
    {
        // Both arguments of level k are the one call of level k-1, so the paths number 2^depth and the nodes depth + 1.
        const int depth = 16;
        TirNode node = new TirLiteral(0, LiteralTypesEnum.Integer, null, null);
        for (var k = 0; k < depth; k++)
            node = new TirCall(null, EmissionKind.StaticMethod, null, null, new List<TirNode> { node, node }, null, null, "Pair");
        var facts = new TirFacts();
        var key = facts.StructuralKey((TirCall)node, true);
        Assert.That(key, Is.Not.Null);
        Assert.That(facts.NodesVisited, Is.EqualTo(depth + 1), "every inner node is computed once");
        Assert.That(facts.StructuralKey((TirCall)node, true), Is.EqualTo(key));
        Assert.That(facts.NodesVisited, Is.EqualTo(depth + 1), "a repeated request is a cache hit and counts nothing");
    }

    // --- equivalence with the walks the table replaces -----------------------------------------

    private static Ara3D.Geometry.Compiler.Compilation Compile(string folder, string file)
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", folder, file);
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    }

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> SharedCalls = new(() => Compile("SharedCallsFixtures", "shared-calls.plato"));
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Rewrites = new(() => Compile("DecoderRewriteFixtures", "rewrites.plato"));
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Hoist = new(() => Compile("HoistInvariantsFixtures", "hoist.plato"));

    /// <summary>The optimizer's node kinds as the table sees them in a pass: <see cref="OptimizerNodeModel.Instance"/>,
    /// the model <see cref="PassContext.ForBody"/> installs.</summary>
    private static TirFacts OptimizerFacts() => new(OptimizerNodeModel.Instance);

    /// <summary>Every function of the fixture library, elaborated and after the writer's passes with the named switches on.</summary>
    private static IEnumerable<(string Name, TirFunction Function)> Functions(Ara3D.Geometry.Compiler.Compilation compilation, string library,
        bool sink, bool share, bool wide, bool hoist = false)
    {
        var writer = new CSharpWriter(compilation, "unused-tir-facts")
        {
            ExtensionStyle = true, LowerLoops = true, FuseCombinators = true,
            SinkBindings = sink, ShareCalls = share, ShareCallsWide = wide, HoistInvariants = hoist
        };
        foreach (var definition in compilation.LibraryDefinitionsByName[library].Functions)
        {
            TirLoopLowerer.NextId = 0;
            var tir = writer.TryGetStaticTir(definition);
            if (tir?.Body == null)
                continue;
            yield return (definition.Name + " (elaborated)", tir);
            var optimized = writer.RunOptimizerPasses(tir, null);
            if (optimized?.Body != null)
                yield return (definition.Name + " (optimized)", optimized);
        }
    }

    private static IEnumerable<(string Name, TirNode Body)> Bodies(Ara3D.Geometry.Compiler.Compilation compilation, string library,
        bool sink, bool share, bool wide, bool hoist = false)
        => Functions(compilation, library, sink, share, wide, hoist).Select(f => (f.Name, f.Function.Body));

    private static IEnumerable<TestCaseData> Fixtures()
    {
        yield return new TestCaseData(SharedCalls, "SharedProbe", false, true, false).SetName("{m}(shared-calls)");
        yield return new TestCaseData(Rewrites, "RewriteProbe", true, true, true).SetName("{m}(rewrites, sink and wide)");
    }

    /// <summary>The fixtures above and the hoist fixture, whose lowered loops carry hoisted bindings and temporaries.</summary>
    private static IEnumerable<TestCaseData> FixturesWithHoist()
    {
        foreach (var fixture in Fixtures())
            yield return new TestCaseData(fixture.Arguments.Append(false).ToArray()).SetName(fixture.TestName);
        yield return new TestCaseData(Hoist, "HoistProbe", false, false, false, true).SetName("{m}(hoist)");
    }

    [TestCaseSource(nameof(FixturesWithHoist))]
    public static void BoundNamesAgreeWithTheFourScans(Lazy<Ara3D.Geometry.Compiler.Compilation> compilation, string library, bool sink, bool share, bool wide, bool hoist)
    {
        foreach (var (name, function) in Functions(compilation.Value, library, sink, share, wide, hoist))
        {
            var facts = OptimizerFacts();
            foreach (var n in function.Body.Descendants())
            {
                // The scan of TirLoopLowerer.Scopes, TirSharedCalls and ComponentReceiverBinding (the same text in all three), over the subtree.
                var nodes = n.Descendants().ToList();
                var scan = nodes.Select(d => d switch
                {
                    TirVariable v => v.Def?.Name, TirParameter p => p.Def?.Name,
                    TirLet l => l.Def?.Name, TirInlineBinding b => b.Def.Name,
                    TirInlineRegion r => r.Result.Name, TirTempRef t => t.Name, _ => null
                }).Concat(nodes.OfType<TirLambda>().SelectMany(l => l.Parameters).Select(p => p.Name))
                    .Where(s => s != null).ToHashSet();
                Assert.That(facts.BoundNames(n), Is.EquivalentTo(scan), name);
                Assert.That(facts.BoundNames(n).Count, Is.EqualTo(scan.Count), name);
                // TirMultiConsumerFusion's scan leaves out the temporaries: a subset.
                var fusion = nodes.Select(d => d switch
                {
                    TirVariable v => v.Def.Name, TirParameter p => p.Def.Name,
                    TirLet l => l.Def.Name, TirInlineBinding b => b.Def.Name, TirInlineRegion r => r.Result.Name, _ => null
                }).Where(s => s != null).Concat(nodes.OfType<TirLambda>().SelectMany(l => l.Parameters).Select(p => p.Name)).ToList();
                Assert.That(fusion.Distinct(), Is.SubsetOf(facts.BoundNames(n)), name);
                // TirInliner.CollectBoundNames keeps the binders alone: a subset.
                var binders = new HashSet<string>();
                foreach (var d in nodes)
                {
                    if (d is TirLambda lam) foreach (var p in lam.Parameters) if (p?.Name != null) binders.Add(p.Name);
                    if (d is TirLet let && let.Def?.Name != null) binders.Add(let.Def.Name);
                    if (d is TirInlineBinding binding) binders.Add(binding.Def.Name);
                    if (d is TirInlineRegion region) binders.Add(region.Result.Name);
                }
                Assert.That(binders, Is.SubsetOf(facts.BoundNames(n)), name);
            }
            // The whole-function set is the body's scan with the parameter names, mutable.
            var expected = facts.BoundNames(function.Body).Concat(function.Parameters.Select(p => p.Name)).ToHashSet();
            var whole = facts.BoundNames(function);
            Assert.That(whole, Is.EquivalentTo(expected), name);
            Assert.That(whole.Add("_tir_facts_fresh"), Is.True, name);
            Assert.That(facts.BoundNames(function), Does.Not.Contain("_tir_facts_fresh"), name);
        }
    }

    [TestCaseSource(nameof(FixturesWithHoist))]
    public static void UsesAgreeWithTheCountingWalks(Lazy<Ara3D.Geometry.Compiler.Compilation> compilation, string library, bool sink, bool share, bool wide, bool hoist)
    {
        foreach (var (name, body) in Bodies(compilation.Value, library, sink, share, wide, hoist))
        {
            var facts = OptimizerFacts();
            foreach (var n in body.Descendants())
            {
                // TirRewrite.CountParamUses: the parameters, with under-lambda.
                var parameters = TirRewrite.CountParamUses(n);
                foreach (var (def, (count, underLambda)) in parameters)
                {
                    Assert.That(facts.UseCount(n, def), Is.EqualTo(count), $"{name}: {def.Name}");
                    Assert.That(facts.UsesUnderLambda(n, def), Is.EqualTo(underLambda), $"{name}: {def.Name}");
                }
                // The walk of TirArrayMaterializer.CountRefs, extended with the guarded loop and the temporaries by name.
                var naive = new Dictionary<object, TirUse>(ReferenceEqualityComparer.Instance);
                var names = new Dictionary<string, TirUse>(StringComparer.Ordinal);
                void Walk(TirNode node, bool underLambda, bool underLoop)
                {
                    if (node == null)
                        return;
                    var use = new TirUse(1, underLambda, underLoop);
                    if (node is TirVariable v && v.Def != null)
                        naive[v.Def] = naive.TryGetValue(v.Def, out var sofar) ? sofar.Plus(use) : use;
                    if (node is TirParameter p && p.Def != null)
                        naive[p.Def] = naive.TryGetValue(p.Def, out var sofar2) ? sofar2.Plus(use) : use;
                    if (node is TirTempRef t)
                        names[t.Name] = names.TryGetValue(t.Name, out var sofar3) ? sofar3.Plus(use) : use;
                    var lambda = underLambda || node is TirLambda;
                    var loop = underLoop || node is TirLoop || node is TirGuardedLoop;
                    foreach (var child in node.Children)
                        Walk(child, lambda, loop);
                }
                Walk(n, false, false);
                foreach (var (def, use) in naive)
                    Assert.That(facts.Uses(n, def), Is.EqualTo(use), $"{name}: {def}");
                foreach (var (temp, use) in names)
                    Assert.That(facts.Uses(n, new string(temp.AsSpan())), Is.EqualTo(use), $"{name}: {temp} (compared by value)");
                Assert.That(facts.Uses(n).Count, Is.EqualTo(naive.Count + names.Count), name);
                Assert.That(facts.Uses(n).Sum(kv => kv.Value.Count), Is.EqualTo(naive.Values.Sum(u => u.Count) + names.Values.Sum(u => u.Count)), name);
                Assert.That(parameters.Count, Is.EqualTo(naive.Keys.OfType<ParameterDef>().Count()), name);
                foreach (var d in n.Descendants().OfType<TirLet>())
                    if (!naive.ContainsKey(d.Def))
                        Assert.That(facts.Uses(n, d.Def), Is.EqualTo(default(TirUse)), name);
            }
        }
    }

    [TestCaseSource(nameof(FixturesWithHoist))]
    public static void AssignmentsAgreeWithAScan(Lazy<Ara3D.Geometry.Compiler.Compilation> compilation, string library, bool sink, bool share, bool wide, bool hoist)
    {
        foreach (var (name, body) in Bodies(compilation.Value, library, sink, share, wide, hoist))
        {
            var facts = OptimizerFacts();
            foreach (var n in body.Descendants())
            {
                // TirMultiConsumerFusion's set and TirPresize.IsCountedFill's per-target count, over the subtree.
                var assigns = n.Descendants().OfType<TirAssign>().ToList();
                var targets = assigns.Select(a => a.LValue switch { TirVariable v => (object)v.Def, TirParameter p => p.Def, _ => null })
                    .Where(d => d != null).ToList();
                Assert.That(facts.AssignedDefs(n), Is.EquivalentTo(targets.Distinct()).Using<object>((x, y) => ReferenceEquals(x, y)), name);
                foreach (var def in targets.Distinct())
                    Assert.That(facts.Assigns(n, def).Count, Is.EqualTo(targets.Count(t => ReferenceEquals(t, def))), name);
                Assert.That(facts.ContainsAssign(n), Is.EqualTo(assigns.Count > 0), name);
                foreach (var d in n.Descendants().OfType<TirLet>())
                    if (!targets.Contains(d.Def))
                        Assert.That(facts.Assigns(n, d.Def), Is.EqualTo(default(TirUse)), name);
            }
        }
    }

    [TestCaseSource(nameof(Fixtures))]
    public static void SizeAndContainsAgreeWithDescendants(Lazy<Ara3D.Geometry.Compiler.Compilation> compilation, string library, bool sink, bool share, bool wide)
    {
        foreach (var (name, body) in Bodies(compilation.Value, library, sink, share, wide))
        {
            var facts = new TirFacts();
            foreach (var n in body.Descendants())
            {
                Assert.That(facts.Size(n), Is.EqualTo(n.Descendants().Count()), name);
                Assert.That(facts.ContainsAssign(n), Is.EqualTo(n.Descendants().Any(d => d is TirAssign)), name);
                Assert.That(facts.ContainsLoop(n), Is.EqualTo(n.Descendants().Any(d => d is TirLoop)), name);
                Assert.That(facts.ContainsKind<TirLambda>(n), Is.EqualTo(n.Descendants().Any(d => d is TirLambda)), name);
            }
        }
    }

    [TestCaseSource(nameof(Fixtures))]
    public static void MentionedDefsAgreeWithAScan(Lazy<Ara3D.Geometry.Compiler.Compilation> compilation, string library, bool sink, bool share, bool wide)
    {
        foreach (var (name, body) in Bodies(compilation.Value, library, sink, share, wide))
        {
            var facts = new TirFacts();
            foreach (var n in body.Descendants())
            {
                var naive = new HashSet<object>(ReferenceEqualityComparer.Instance);
                foreach (var d in n.Descendants())
                {
                    if (d is TirVariable v && v.Def != null) naive.Add(v.Def);
                    if (d is TirParameter p && p.Def != null) naive.Add(p.Def);
                }
                Assert.That(facts.MentionedDefs(n), Is.EquivalentTo(naive).Using<object>((x, y) => ReferenceEquals(x, y)), name);
                Assert.That(facts.MentionedDefs(n).Count, Is.EqualTo(naive.Count), name);
                foreach (var def in naive)
                    Assert.That(facts.Mentions(n, def), Is.True, name);
                foreach (var d in n.Descendants().OfType<TirLet>())
                    Assert.That(facts.Mentions(n, d.Def), Is.EqualTo(naive.Contains(d.Def)), name);
            }
        }
    }

    [TestCaseSource(nameof(Fixtures))]
    public static void StructuralKeyAgreesWithTheOnePassWriter(Lazy<Ara3D.Geometry.Compiler.Compilation> compilation, string library, bool sink, bool share, bool wide)
    {
        foreach (var (name, body) in Bodies(compilation.Value, library, sink, share, wide))
        {
            foreach (var w in new[] { false, true })
            {
                var facts = new TirFacts();
                var naive = new NaiveKey(w);
                foreach (var call in body.Descendants().OfType<TirCall>())
                {
                    var expected = naive.Key(call);
                    Assert.That(facts.StructuralKey(call, w), Is.EqualTo(expected), $"{name}, wide {w}, {call}");
                    // A key covers every leaf of its call, so the definitions the writer numbered are exactly the ones read.
                    if (expected != null)
                        Assert.That(facts.MentionedDefs(call), Is.EquivalentTo(naive.Defs).Using<object>((x, y) => ReferenceEquals(x, y)),
                            $"{name}, wide {w}, {call}");
                }
            }
        }
    }

    /// <summary>The key writer <c>TirSharedCalls</c> had before compiler-546, kept as the reference.</summary>
    private sealed class NaiveKey
    {
        private readonly bool _wide;
        private readonly Dictionary<object, int> _ids = new(ReferenceEqualityComparer.Instance);

        public NaiveKey(bool wide) => _wide = wide;

        /// <summary>The definitions the last <see cref="Key"/> call met (complete only when that key is not null).</summary>
        public HashSet<object> Defs { get; } = new(ReferenceEqualityComparer.Instance);

        public string? Key(TirCall call)
        {
            Defs.Clear();
            var sb = new StringBuilder();
            return Write(call, sb) ? sb.ToString() : null;
        }

        private int Id(object o)
        {
            if (!_ids.TryGetValue(o, out var id))
                _ids[o] = id = _ids.Count;
            return id;
        }

        private static bool IsUnique(TypeExpression? t) => UniqueTypes.IsUniqueType(t);

        private bool Write(TirNode n, StringBuilder sb)
        {
            switch (n)
            {
                case TirParameter p when p.Def != null && !IsUnique(p.Type):
                    Defs.Add(p.Def);
                    sb.Append("P").Append(Id(p.Def));
                    return true;
                case TirVariable v when v.Def != null && !IsUnique(v.Type):
                    Defs.Add(v.Def);
                    sb.Append("V").Append(Id(v.Def));
                    return true;
                case TirLiteral l:
                    sb.Append("L").Append(l.LiteralType).Append(':').Append(l.Type).Append(':').Append(l.Value);
                    return true;
                case TirTypeRef t:
                    sb.Append("T").Append(t.TypeDef?.Name).Append(t.NamespaceQualified);
                    return true;
                case TirDefault d:
                    sb.Append("D").Append(d.Type);
                    return true;
                case TirCoerce c:
                    sb.Append("K").Append(c.ToType).Append('(');
                    var ok = Write(c.Inner, sb);
                    sb.Append(')');
                    return ok;
                case TirCall c when c.Callee != null && !IsUnique(c.Type) && !c.Args.Any(a => IsUnique(a?.Type)):
                    sb.Append("C").Append(Id(c.Callee)).Append(':').Append(c.EmissionKind).Append(':').Append(c.Type).Append('(');
                    foreach (var a in c.Args)
                    {
                        if (a == null || !Write(a, sb))
                            return false;
                        sb.Append(',');
                    }
                    sb.Append(')');
                    return true;
                case TirCall c when c.Callee == null && c.EmissionKind == EmissionKind.Property
                                    && c.Args.Count == 1 && !IsUnique(c.Type):
                    sb.Append("F").Append(c.Name).Append(':').Append(c.Type).Append('(');
                    var inner = c.Args[0] != null && Write(c.Args[0], sb);
                    sb.Append(')');
                    return inner;
                case TirCall c when _wide && c.Callee == null && c.EmissionKind == EmissionKind.StaticMethod
                                    && c.Name != null && !IsUnique(c.Type) && !c.Args.Any(a => a == null || IsUnique(a.Type)):
                    sb.Append("S").Append(c.Name).Append(':').Append(c.Type).Append('(');
                    foreach (var a in c.Args)
                    {
                        if (!Write(a, sb))
                            return false;
                        sb.Append(',');
                    }
                    sb.Append(')');
                    return true;
                default:
                    return false;
            }
        }
    }
}
