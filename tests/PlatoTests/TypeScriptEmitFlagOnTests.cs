using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.TypeScriptWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// TypeScript counterpart of <see cref="EmitFlagOnTests"/>: generates the whole TS library
    /// twice — UseTir off (legacy symbol-graph body writer) and on (TirTypeScriptBodyWriter) —
    /// and asserts byte-identical output. The flip criterion for the TS writer.
    /// </summary>
    [TestFixture]
    public static class TypeScriptEmitFlagOnTests
    {
        private static string Canonical(string s)
            => string.Join("\n", (s ?? "").Replace("\r\n", "\n").Split('\n')
                .Where(line => !line.StartsWith("// Created on ")));

        [Test]
        public static void UseTirOnReproducesTheFullTypeScriptLibraryByteForByte()
        {
            var flagOff = new TypeScriptWriter(CheckerTestSupport.CompileStdLib(), "unused-flag-off") { UseTir = false };
            flagOff.WriteAll();

            var flagOn = new TypeScriptWriter(CheckerTestSupport.CompileStdLib(), "unused-flag-on") { UseTir = true };
            flagOn.WriteAll();

            var totalEligible = flagOn.TirBodiesEmitted + flagOn.TirFallbackBodies;
            TestContext.WriteLine("=== UseTir flag-ON full-library measurement (TypeScript) ===");
            TestContext.WriteLine($"bodies emitted from the TIR   : {flagOn.TirBodiesEmitted}");
            TestContext.WriteLine($"  fell back to legacy writer  : {flagOn.TirFallbackBodies}");
            TestContext.WriteLine($"  total eligible bodies       : {totalEligible}"
                + (totalEligible > 0 ? $"  ({100.0 * flagOn.TirBodiesEmitted / totalEligible:F1} % from TIR)" : ""));

            Assert.Greater(flagOn.TirBodiesEmitted, 0, "the TIR path never fired");
            Assert.AreEqual(flagOff.Files.Count, flagOn.Files.Count, "flag-on generated a different set of files");

            var differing = new List<string>();
            foreach (var kv in flagOff.Files)
            {
                Assert.IsTrue(flagOn.Files.ContainsKey(kv.Key), $"flag-on output is missing file {kv.Key}");
                if (Canonical(kv.Value.ToString()) != Canonical(flagOn.Files[kv.Key].ToString()))
                    differing.Add(kv.Key);
            }

            TestContext.WriteLine($"flag-on vs flag-off files byte-identical      : {flagOff.Files.Count - differing.Count} / {flagOff.Files.Count}");
            foreach (var d in differing)
            {
                TestContext.WriteLine($"  DIFFERS: {d}");
                var a = Canonical(flagOff.Files[d].ToString()).Split('\n');
                var b = Canonical(flagOn.Files[d].ToString()).Split('\n');
                var shown = 0;
                for (var i = 0; i < a.Length && i < b.Length && shown < 10; i++)
                    if (a[i] != b[i])
                    {
                        TestContext.WriteLine($"    line {i + 1}:");
                        TestContext.WriteLine($"      off: {a[i]}");
                        TestContext.WriteLine($"      on : {b[i]}");
                        shown++;
                    }
            }

            Assert.AreEqual(0, flagOn.TirFallbackBodies, "a TypeScript body fell back to the legacy writer");
            Assert.IsEmpty(differing, "UseTir=true changed TypeScript output vs UseTir=false");
        }

        [Test]
        public static void TirLambdaBodiesWrapStatementShapes()
        {
            var compilation = CheckerTestSupport.CompileStdLib();
            var writer = new TypeScriptWriter(compilation, "unused-lambda-test") { UseTir = true };
            var typeWriter = new TypeScriptTypeWriter(writer, null);

            var valueParam = new ParameterDef(null, "values", null, 0);
            var lambdaParam = new ParameterDef(null, "x", null, 0);
            var lambdaBody = new TirReturn(new TirName("x", null, null), null);
            var lambda = new TirLambda(new List<ParameterDef> { lambdaParam }, lambdaBody, null, null);
            var call = new TirCall(null, EmissionKind.StaticMethod, null, null,
                new List<TirNode> { new TirName("values", null, null), lambda }, null, null, "All");
            var tir = new TirFunction(null, new List<ParameterDef> { valueParam }, null, call);

            var body = new TirTypeScriptBodyWriter(typeWriter, tir, false).ToString();

            Assert.That(body, Does.Contain("=> {"));
            Assert.That(body, Does.Not.Contain("=> return"));
        }

        [Test]
        public static void TirMemberBodiesKeepLocalFieldAccessAndMethodCallsSeparate()
        {
            // PositionVector/Offset live in the forward stdlib's geometry tier.
            var writer = new TypeScriptWriter(CheckerTestSupport.CompileForwardStdLibShippingTiers(), "unused-field-test") { UseTir = true };
            writer.WriteAll();

            var ts = writer.Files["plato.g.ts"].ToString();

            Assert.That(ts, Does.Contain("PositionVector(): Vector3D { return this.Offset(); }"));
            Assert.That(ts, Does.Not.Contain("PositionVector(): Vector3D { return this.Offset; }"));
        }
    }
}
