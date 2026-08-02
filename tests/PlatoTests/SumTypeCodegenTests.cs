using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;

namespace PlatoTests
{
    /// <summary>
    /// Wave-2 (plato-232) codegen gate: run each sum-type fixture through the full pipeline
    /// (parse → check → elaborate → monomorphize → emit) under the shipping recipe
    /// (--csharp-style=extensions --scalar=float), assert the emitted C# has the
    /// expected tagged-struct shape, and — the strong gate — compile the emitted struct + its match
    /// function in-proc with Roslyn and execute it, verifying the runtime match semantics.
    /// </summary>
    [TestFixture]
    public static class SumTypeCodegenTests
    {
        private static string SumDir()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var c = Path.Combine(dir.FullName, "plato-test-sum");
                if (Directory.Exists(c)) return c;
            }
            throw new DirectoryNotFoundException("plato-test-sum not found above " + AppContext.BaseDirectory);
        }

        private static CSharpWriter Emit(string file)
        {
            var ast = CheckerTestSupport.ParseFile(Path.Combine(SumDir(), file));
            var comp = new PlatoCompilation(Ara3D.Logging.Logger.Null, new[] { ast });
            var w = new CSharpWriter(comp, "unused-sum-codegen")
            {
                ExtensionStyle = true,
            };
            w.WriteAll("float");
            return w;
        }

        // A minimal runtime prelude so the emitted files compile in isolation: the Ara3D.Collections
        // namespace the header `using` needs, an Intrinsics.CombineHashCodes, and trivial stand-ins
        // for the payload structs the pathsegment fixture references (a real Point2D so projections
        // are observable; empty Vector2/Angle, which the match never reads).
        private const string Prelude = @"
namespace Ara3D.Collections { }
namespace Ara3D.Geometry
{
    public static class Intrinsics
    {
        public static int CombineHashCodes(params object[] xs)
        {
            var h = 17;
            foreach (var x in xs) h = h * 31 + (x?.GetHashCode() ?? 0);
            return h;
        }
    }
    public struct Point2D
    {
        public readonly float X, Y;
        public Point2D(float x, float y) { X = x; Y = y; }
        public bool Equals(Point2D o) => X == o.X && Y == o.Y;
        public override string ToString() => $""({X}, {Y})"";
    }
    public struct Vector2 { }
    public struct Angle { }
}";

        /// <summary>Reads the payload out of a generated scalar WRAPPER (Boolean/Integer/Number/...)
        /// returned through reflection, so assertions compare against plain BCL values. Needed since
        /// scalar erasure was retired 2026-08-01: these members used to return bool/int directly.</summary>
        private static object Unwrap(object v)
        {
            var f = v?.GetType().GetField("Value");
            return f == null ? v : f.GetValue(v);
        }

        private static Assembly CompileAndLoad(params string[] sources)
        {
            var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray();
            var tpa = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Where(p => !string.IsNullOrEmpty(p) && System.IO.File.Exists(p))
                // The stale Ara3D.Geometry PACKAGE also declares Ara3D.Geometry.Number/Boolean/...,
                // and so does this test assembly (it compiles src/Plato.Intrinsics in as a shared
                // project). Referencing both is CS0433 for every wrapper scalar the emitted code
                // names. Drop the package: the current runtime is the one the generated projects
                // actually compile against. (Invisible before 2026-08-01, when erasure meant the
                // emitted code said float/bool and never named a wrapper.)
                .Where(p => !string.Equals(System.IO.Path.GetFileName(p), "Ara3D.Geometry.dll", System.StringComparison.OrdinalIgnoreCase))
                .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
                .ToList();
            var comp = CSharpCompilation.Create(
                "SumCodegen_" + Guid.NewGuid().ToString("N"),
                trees, tpa,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var ms = new MemoryStream();
            var result = comp.Emit(ms);
            if (!result.Success)
            {
                var errs = string.Join("\n", result.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString()));
                Assert.Fail("Emitted C# did not compile:\n" + errs);
            }
            ms.Position = 0;
            return Assembly.Load(ms.ToArray());
        }

        // --- struct-shape assertions ---------------------------------------------

        [Test]
        public static void FillRule_EmitsExpectedStructShape()
        {
            var src = Emit("fillrule.plato").Files["_FillRule.g.cs"].ToString();
            Assert.That(src, Does.Contain("public readonly partial struct FillRule"));
            Assert.That(src, Does.Contain("public readonly int Kind"));
            Assert.That(src, Does.Contain("public const int Kind_NonZero = 0"));
            Assert.That(src, Does.Contain("public const int Kind_EvenOdd = 1"));
            Assert.That(src, Does.Contain("public static FillRule NonZero() => new FillRule(Kind_NonZero)"));
            Assert.That(src, Does.Contain("public static FillRule EvenOdd() => new FillRule(Kind_EvenOdd)"));
            Assert.That(src, Does.Contain("private FillRule(int kind)"));
            // The match function lowered to a predicate ternary (no default/throw fall-through).
            var lib = Emit("fillrule.plato").Files["FillRules.g.cs"].ToString();
            Assert.That(lib, Does.Contain("IsEvenOdd(this FillRule r) => r.IsNonZero() ? ((Boolean)false) : ((Boolean)true)"));
        }

        [Test]
        public static void PathSegment_EmitsExpectedStructShape()
        {
            var w = Emit("pathsegment.plato");
            var src = w.Files["_PathSegment2D.g.cs"].ToString();
            Assert.That(src, Does.Contain("public readonly partial struct PathSegment2D"));
            Assert.That(src, Does.Contain("public readonly int Kind"));
            Assert.That(src, Does.Contain("public const int Kind_Move = 0"));
            Assert.That(src, Does.Contain("public const int Kind_Close = 5"));
            // Flattened per-case fields, disambiguated by the Case_ prefix.
            Assert.That(src, Does.Contain("public readonly Point2D Quadratic_Control"));
            Assert.That(src, Does.Contain("public readonly Point2D Quadratic_EndPoint"));
            Assert.That(src, Does.Contain("public readonly Boolean Arc_LargeArc"));
            // Per-case factory that defaults the inactive fields.
            Assert.That(src, Does.Contain("public static PathSegment2D Move(Point2D endPoint) => new PathSegment2D(Kind_Move, endPoint, default"));
            Assert.That(src, Does.Contain("public static PathSegment2D Close() => new PathSegment2D(Kind_Close, default"));
            // The exhaustive match lowered to a predicate/projection ternary, last case unconditional.
            var lib = w.Files["PathSegments.g.cs"].ToString();
            Assert.That(lib, Does.Contain(
                "EndPoint(this PathSegment2D seg, Point2D start) => seg.IsMove() ? seg.Move_EndPoint"));
            Assert.That(lib, Does.Contain(": start;"));
            Assert.That(lib, Does.Contain("Pen(this Point2D start) => PathSegment2D.Move(start)"));
        }

        // --- strong gate: compile and execute ------------------------------------

        [Test]
        public static void FillRule_CompilesAndRuns()
        {
            var w = Emit("fillrule.plato");
            var asm = CompileAndLoad(Prelude,
                w.Files["_FillRule.g.cs"].ToString(),
                w.Files["FillRules.g.cs"].ToString());

            var fillRule = asm.GetType("Ara3D.Geometry.FillRule", true);
            var lib = asm.GetType("Ara3D.Geometry.FillRules", true);

            var nonZero = fillRule.GetMethod("NonZero").Invoke(null, null);
            var evenOdd = fillRule.GetMethod("EvenOdd").Invoke(null, null);
            var isEvenOdd = lib.GetMethod("IsEvenOdd");

            // The match: NonZero => false, EvenOdd => true.
            Assert.AreEqual(false, Unwrap(isEvenOdd.Invoke(null, new[] { nonZero })));
            Assert.AreEqual(true, Unwrap(isEvenOdd.Invoke(null, new[] { evenOdd })));

            // Tag + predicate + ToString. A sum's ToString is its JSON: the Kind discriminant plus
            // any flattened per-case fields. It used to render the case NAME ("NonZero"), which
            // reads well but no parser reads back — see CSharpSerializationWriter.
            Assert.AreEqual(0, fillRule.GetField("Kind").GetValue(nonZero));
            Assert.AreEqual(true, Unwrap(fillRule.GetMethod("IsNonZero").Invoke(nonZero, null)));
            Assert.AreEqual("{\"Kind\":0}", nonZero.ToString());
            Assert.AreEqual("{\"Kind\":1}", evenOdd.ToString());
        }

        [Test]
        public static void PathSegment_CompilesAndRunsMatch()
        {
            var w = Emit("pathsegment.plato");
            var asm = CompileAndLoad(Prelude,
                w.Files["_PathSegment2D.g.cs"].ToString(),
                w.Files["PathSegments.g.cs"].ToString());

            var seg = asm.GetType("Ara3D.Geometry.PathSegment2D", true);
            var pt = asm.GetType("Ara3D.Geometry.Point2D", true);
            var lib = asm.GetType("Ara3D.Geometry.PathSegments", true);

            object Point(float x, float y) => Activator.CreateInstance(pt, x, y);
            (float X, float Y) Read(object p) =>
                ((float)pt.GetField("X").GetValue(p), (float)pt.GetField("Y").GetValue(p));

            var move = seg.GetMethod("Move").Invoke(null, new[] { Point(1, 2) });
            var line = seg.GetMethod("Line").Invoke(null, new[] { Point(3, 4) });
            var quad = seg.GetMethod("Quadratic").Invoke(null, new[] { Point(7, 8), Point(5, 6) });
            var cubic = seg.GetMethod("Cubic").Invoke(null, new[] { Point(0, 0), Point(0, 0), Point(11, 12) });
            var close = seg.GetMethod("Close").Invoke(null, null);

            var endPoint = lib.GetMethod("EndPoint");
            var start = Point(9, 9);
            (float, float) EndOf(object s) => Read(endPoint.Invoke(null, new[] { s, start }));

            // The match projects each case's EndPoint; Close returns the supplied start.
            Assert.AreEqual((1f, 2f), EndOf(move));
            Assert.AreEqual((3f, 4f), EndOf(line));
            Assert.AreEqual((5f, 6f), EndOf(quad));
            Assert.AreEqual((11f, 12f), EndOf(cubic));
            Assert.AreEqual((9f, 9f), EndOf(close));

            // A second match over the same sum (integer arms).
            var cpc = lib.GetMethod("ControlPointCount");
            Assert.AreEqual(0, Unwrap(cpc.Invoke(null, new[] { move })));
            Assert.AreEqual(1, Unwrap(cpc.Invoke(null, new[] { quad })));
            Assert.AreEqual(2, Unwrap(cpc.Invoke(null, new[] { cubic })));
            Assert.AreEqual(0, Unwrap(cpc.Invoke(null, new[] { close })));

            // The qualified case constructor (Pen) builds a Move segment.
            var pen = lib.GetMethod("Pen").Invoke(null, new[] { Point(1, 1) });
            Assert.AreEqual(0, seg.GetField("Kind").GetValue(pen)); // Kind_Move
        }

        /// <summary>
        /// A qualified NULLARY case used as a VALUE (<c>FillRule.EvenOdd</c>, the `Axis3D.Y` shape).
        /// The writer emits every case as a static FACTORY METHOD, so the reference must render
        /// <c>FillRule.EvenOdd()</c> — elaboration gives it StaticMethod emission for exactly that
        /// reason. The syntactic path would read the `a.b` shape as a property and emit
        /// <c>FillRule.EvenOdd</c>, which does not compile; the Roslyn build below is what pins it.
        /// </summary>
        [Test]
        public static void QualifiedNullaryCase_CompilesAndRuns()
        {
            var w = Emit("qualified-nullary-case.plato");
            var lib = w.Files["FillRules.g.cs"].ToString();
            Assert.That(lib, Does.Contain("FillRule.EvenOdd()"));
            Assert.That(lib, Does.Contain("FillRule.NonZero()"));

            var asm = CompileAndLoad(Prelude,
                w.Files["_FillRule.g.cs"].ToString(), lib);
            var fillRule = asm.GetType("Ara3D.Geometry.FillRule", true);
            var fillRules = asm.GetType("Ara3D.Geometry.FillRules", true);

            var nonZero = fillRule.GetMethod("NonZero").Invoke(null, null);
            var evenOdd = fillRule.GetMethod("EvenOdd").Invoke(null, null);
            var flip = fillRules.GetMethod("Flip");

            Assert.AreEqual("{\"Kind\":1}", flip.Invoke(null, new[] { nonZero }).ToString());
            Assert.AreEqual("{\"Kind\":0}", flip.Invoke(null, new[] { evenOdd }).ToString());
        }
    }
}
