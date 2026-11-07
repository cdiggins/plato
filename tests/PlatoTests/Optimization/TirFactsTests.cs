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

    /// <summary>Every function of the fixture library, elaborated and after the writer's passes with the named switches on.</summary>
    private static IEnumerable<(string Name, TirNode Body)> Bodies(Ara3D.Geometry.Compiler.Compilation compilation, string library,
        bool sink, bool share, bool wide)
    {
        var writer = new CSharpWriter(compilation, "unused-tir-facts")
        {
            ExtensionStyle = true, LowerLoops = true, FuseCombinators = true,
            SinkBindings = sink, ShareCalls = share, ShareCallsWide = wide
        };
        foreach (var definition in compilation.LibraryDefinitionsByName[library].Functions)
        {
            TirLoopLowerer.NextId = 0;
            var tir = writer.TryGetStaticTir(definition);
            if (tir?.Body == null)
                continue;
            yield return (definition.Name + " (elaborated)", tir.Body);
            var optimized = writer.RunOptimizerPasses(tir, null);
            if (optimized?.Body != null)
                yield return (definition.Name + " (optimized)", optimized.Body);
        }
    }

    private static IEnumerable<TestCaseData> Fixtures()
    {
        yield return new TestCaseData(SharedCalls, "SharedProbe", false, true, false).SetName("{m}(shared-calls)");
        yield return new TestCaseData(Rewrites, "RewriteProbe", true, true, true).SetName("{m}(rewrites, sink and wide)");
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
