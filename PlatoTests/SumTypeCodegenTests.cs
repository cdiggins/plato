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
    /// (--csharp-style=extensions --scalar=float --no-properties), assert the emitted C# has the
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
                ScalarErase = true,
                NoProperties = true,
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

        private static Assembly CompileAndLoad(params string[] sources)
        {
            var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray();
            var tpa = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator)
                .Where(p => !string.IsNullOrEmpty(p) && File.Exists(p))
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
            Assert.That(src, Does.Contain("public partial struct FillRule"));
            Assert.That(src, Does.Contain("public readonly int Kind"));
            Assert.That(src, Does.Contain("public const int Kind_NonZero = 0"));
            Assert.That(src, Does.Contain("public const int Kind_EvenOdd = 1"));
            Assert.That(src, Does.Contain("public static FillRule NonZero() => new FillRule(Kind_NonZero)"));
            Assert.That(src, Does.Contain("public static FillRule EvenOdd() => new FillRule(Kind_EvenOdd)"));
            Assert.That(src, Does.Contain("private FillRule(int kind)"));
            // The match function lowered to a predicate ternary (no default/throw fall-through).
            var lib = Emit("fillrule.plato").Files["FillRules.g.cs"].ToString();
            Assert.That(lib, Does.Contain("IsEvenOdd(this FillRule r) => r.IsNonZero() ? false : true"));
        }

        [Test]
        public static void PathSegment_EmitsExpectedStructShape()
        {
            var w = Emit("pathsegment.plato");
            var src = w.Files["_PathSegment2D.g.cs"].ToString();
            Assert.That(src, Does.Contain("public partial struct PathSegment2D"));
            Assert.That(src, Does.Contain("public readonly int Kind"));
            Assert.That(src, Does.Contain("public const int Kind_Move = 0"));
            Assert.That(src, Does.Contain("public const int Kind_Close = 5"));
            // Flattened per-case fields, disambiguated by the Case_ prefix.
            Assert.That(src, Does.Contain("public readonly Point2D Quadratic_Control"));
            Assert.That(src, Does.Contain("public readonly Point2D Quadratic_EndPoint"));
            Assert.That(src, Does.Contain("public readonly bool Arc_LargeArc"));
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
            Assert.AreEqual(false, isEvenOdd.Invoke(null, new[] { nonZero }));
            Assert.AreEqual(true, isEvenOdd.Invoke(null, new[] { evenOdd }));

            // Tag + predicate + ToString.
            Assert.AreEqual(0, fillRule.GetField("Kind").GetValue(nonZero));
            Assert.AreEqual(true, fillRule.GetMethod("IsNonZero").Invoke(nonZero, null));
            Assert.AreEqual("NonZero", nonZero.ToString());
            Assert.AreEqual("EvenOdd", evenOdd.ToString());
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
            Assert.AreEqual(0, cpc.Invoke(null, new[] { move }));
            Assert.AreEqual(1, cpc.Invoke(null, new[] { quad }));
            Assert.AreEqual(2, cpc.Invoke(null, new[] { cubic }));
            Assert.AreEqual(0, cpc.Invoke(null, new[] { close }));

            // The qualified case constructor (Pen) builds a Move segment.
            var pen = lib.GetMethod("Pen").Invoke(null, new[] { Point(1, 1) });
            Assert.AreEqual(0, seg.GetField("Kind").GetValue(pen)); // Kind_Move
        }

        // --- regression: the match subject may be any expression ------------------
        // Before the fix, ResolveMatch only derived the sum type from a bare RefSymbol
        // subject; a field-read, var-bound, or nested-match subject left SumType null and
        // the body failed in the writer with "No ground TIR" (plato-src-v3 ChainRotations
        // workaround). One fixture covers all four subject shapes.

        [Test]
        public static void MatchSubject_AnyExpression_CompilesAndRuns()
        {
            var w = Emit("match-subject.plato");
            var asm = CompileAndLoad(Prelude,
                w.Files["_Signal.g.cs"].ToString(),
                w.Files["_Channel.g.cs"].ToString(),
                w.Files["Signals.g.cs"].ToString());

            var signal = asm.GetType("Ara3D.Geometry.Signal", true);
            var channel = asm.GetType("Ara3D.Geometry.Channel", true);
            var lib = asm.GetType("Ara3D.Geometry.Signals", true);

            var off = signal.GetMethod("Off").Invoke(null, null);
            var level = signal.GetMethod("Level").Invoke(null, new object[] { 5f });
            var ramp = signal.GetMethod("Ramp").Invoke(null, new object[] { 2f, 3f });
            object Chan(object s) => Activator.CreateInstance(channel, s, 9f);

            // Bare-parameter subject.
            var value = lib.GetMethod("Value");
            Assert.AreEqual(7f, value.Invoke(null, new[] { off, (object)7f }));
            Assert.AreEqual(5f, value.Invoke(null, new[] { level, (object)7f }));
            Assert.AreEqual(3f, value.Invoke(null, new[] { ramp, (object)7f }));

            // Field-read subject (match (c.Current)).
            var currentValue = lib.GetMethod("CurrentValue");
            Assert.AreEqual(9f, currentValue.Invoke(null, new[] { Chan(off) }));
            Assert.AreEqual(5f, currentValue.Invoke(null, new[] { Chan(level) }));
            Assert.AreEqual(2f, currentValue.Invoke(null, new[] { Chan(ramp) }));

            // Var-bound subject (var s = c.Current; match (s)).
            var boundValue = lib.GetMethod("BoundValue");
            Assert.AreEqual(9f, boundValue.Invoke(null, new[] { Chan(off) }));
            Assert.AreEqual(5f, boundValue.Invoke(null, new[] { Chan(level) }));
            Assert.AreEqual(3f, boundValue.Invoke(null, new[] { Chan(ramp) }));

            // Nested match subject (match (match (s) ...)): Ramp collapses to Level(End).
            var collapsed = lib.GetMethod("Collapsed");
            Assert.AreEqual(0f, collapsed.Invoke(null, new[] { off }));
            Assert.AreEqual(5f, collapsed.Invoke(null, new[] { level }));
            Assert.AreEqual(3f, collapsed.Invoke(null, new[] { ramp }));
        }
    }
}
