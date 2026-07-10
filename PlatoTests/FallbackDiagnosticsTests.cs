using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// The increment-3 flip invariant: every default-style member-instance body with Plato source
    /// has a fully-ground monomorphized TIR, so the TIR emit path (UseTir, on by default) never
    /// falls back to the legacy symbol-graph body writer. When this regresses, the report below
    /// classifies each fallback by root cause (unresolved solver call / which residual abstract
    /// type survived, and in which function), so the gap is scoped by data rather than guesswork.
    /// </summary>
    [TestFixture]
    public static class FallbackDiagnosticsTests
    {
        [Test]
        public static void EveryEligibleMemberBodyHasAFullyGroundTir()
        {
            var compilation = CheckerTestSupport.CompileStdLib();
            var mono = new Monomorphizer(compilation).MonomorphizeAll();

            // Every monomorphized instantiation by (source function, concrete type name) — the
            // ground ones are what the flag-on path emits from; everything else falls back.
            var monoByKey = new Dictionary<(FunctionDef, string), MonomorphizedFunction>();
            foreach (var m in mono)
            {
                var key = (m.Original, m.ConcreteType?.Name);
                if (m.Original != null && key.Item2 != null && !monoByKey.ContainsKey(key))
                    monoByKey[key] = m;
            }

            var writer = new CSharpWriter(compilation, "unused-fallback-diagnostics")
            {
                Namespace = "Ara3D.Geometry",
                FloatType = "float",
            };

            var eligible = 0;
            var ground = 0;
            var buckets = new Dictionary<string, List<string>>();
            void Add(string bucket, string detail)
            {
                if (!buckets.TryGetValue(bucket, out var list))
                    buckets[bucket] = list = new List<string>();
                list.Add(detail);
            }

            foreach (var ct in compilation.ConcreteTypes)
            {
                // Mirror the writer: ignored/unique types never have a struct written.
                if (CSharpWriter.IgnoredTypes.Contains(ct.TypeDef.Name) || ct.TypeDef.IsUnique)
                    continue;
                var fieldNames = ct.TypeDef.Fields.Select(f => f.Name).ToList();
                foreach (var group in ct.InterfaceFunctionGroups)
                {
                    FunctionInstance f;
                    try { f = writer.Analyzer.ChooseBestFunction(group, out _); }
                    catch { continue; }
                    if (CSharpConcreteTypeWriter.SkipFunction(f, fieldNames, ct.TypeDef.Name))
                        continue;
                    if (f.Implementation?.Body == null)
                        continue;

                    eligible++;
                    var where = $"{ct.TypeDef.Name}.{f.Name}";

                    if (!monoByKey.TryGetValue((f.Implementation, ct.TypeDef.Name), out var m))
                    {
                        Add("no-reified-alignment", where);
                        continue;
                    }
                    if (!m.HasBody)
                    {
                        Add("no-tir-body", where);
                        continue;
                    }
                    if (m.IsFullyGround)
                    {
                        ground++;
                        continue;
                    }

                    // Not ground: find the offending nodes.
                    var unresolved = m.Tir.AllNodes.OfType<TirUnresolved>().ToList();
                    if (unresolved.Count > 0)
                    {
                        var u = unresolved[0];
                        var name = u.Original is FunctionCall ufc ? ufc.Function?.Name : u.Original?.Name;
                        Add($"unresolved: {u.Reason} [{name}]", where);
                        continue;
                    }

                    var bad = m.Tir.AllNodes.First(n => !IsNodeGround(n));
                    var badType = !TypeSubstitution.IsGround(bad.Type) ? bad.Type
                        : bad is TirCall c
                            ? (!TypeSubstitution.IsGround(c.ReturnType)
                                ? c.ReturnType
                                : c.ParameterTypes.First(t => !TypeSubstitution.IsGround(t)))
                            : bad is TirCoerce co
                                ? (!TypeSubstitution.IsGround(co.FromType) ? co.FromType : co.ToType)
                                : (bad as TirNew)?.NewType;
                    var callName = bad is TirCall cc ? cc.Name : bad.GetType().Name;
                    Add($"non-ground: {callName} : {badType}", where);
                }
            }

            TestContext.WriteLine($"eligible member bodies : {eligible}");
            TestContext.WriteLine($"fully ground (TIR path): {ground}");
            TestContext.WriteLine($"fallback               : {eligible - ground}");
            TestContext.WriteLine("");
            foreach (var b in buckets.OrderByDescending(kv => kv.Value.Count))
            {
                TestContext.WriteLine($"{b.Value.Count,5}  {b.Key}");
                foreach (var d in b.Value.Take(3))
                    TestContext.WriteLine($"         e.g. {d}");
            }

            // The same fallbacks, collapsed to the DISTINCT SOURCE FUNCTIONS involved — fixing a
            // cause in one source function recovers all its per-type instantiations at once.
            TestContext.WriteLine("");
            TestContext.WriteLine("Distinct source functions per bucket (fn name = source, count = instantiations):");
            foreach (var b in buckets.OrderByDescending(kv => kv.Value.Count))
            {
                var fns = b.Value.Select(d => d.Substring(d.IndexOf('.') + 1))
                    .GroupBy(x => x).OrderByDescending(g => g.Count()).ToList();
                TestContext.WriteLine($"  {b.Key}: {fns.Count} distinct fns");
                foreach (var g in fns.Take(8))
                    TestContext.WriteLine($"      {g.Count(),4} x {g.Key}");
            }

            Assert.Greater(eligible, 0);
            Assert.AreEqual(0, eligible - ground,
                "the TIR emit path must cover every eligible member body (the increment-3 flip invariant); see the classification above");
        }

        // Mirrors Monomorphizer.NodeIsGround (internal): a node is ground when its value type and
        // every call/coercion/new type it carries are ground and it is not an unresolved node.
        private static bool IsNodeGround(TirNode n)
        {
            if (n is TirUnresolved)
                return false;
            if (!TypeSubstitution.IsGround(n.Type))
                return false;
            switch (n)
            {
                case TirCall c:
                    return TypeSubstitution.IsGround(c.ReturnType)
                        && c.ParameterTypes.All(TypeSubstitution.IsGround);
                case TirCoerce co:
                    return TypeSubstitution.IsGround(co.FromType) && TypeSubstitution.IsGround(co.ToType);
                case TirNew nw:
                    return TypeSubstitution.IsGround(nw.NewType);
            }
            return true;
        }
    }
}
