// Checks generated representations and execution of sum types.
using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;
using static PlatoTests.GeneratedValueReflection;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;

namespace PlatoTests
{
    /// <summary>
    /// Wave-2 (plato-232) codegen gate: run each sum-type fixture through the full pipeline
    /// (parse → check → elaborate → monomorphize → emit) under the shipping recipe
    /// (extension-style with wrapper scalars), assert the emitted C# has the
    /// expected tagged-struct shape, and — the strong gate — compile the emitted struct + its match
    /// function in-proc with Roslyn and execute it, verifying the runtime match semantics.
    /// </summary>
    [TestFixture]
    public static class SumTypeCodegenTests
    {
        private static string SumDir()
            => CheckerTestSupport.FindFolder("plato-test-sum");

        private static CSharpWriter Emit(string file)
        {
            var ast = CheckerTestSupport.ParseFile(Path.Combine(SumDir(), file));
            var comp = new PlatoCompilation(Ara3D.Logging.Logger.Null, new[] { ast });
            var w = new CSharpWriter(comp, "unused-sum-codegen")
            {
                ExtensionStyle = true,
            };
            w.WriteAll();
            return w;
        }

        // A minimal runtime prelude so the emitted files compile in isolation: the Ara3D.Collections
        // namespace the header `using` needs, Intrinsics.CombineHashCodes, ArrayEquals and ArrayHash, and trivial stand-ins
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
        public static bool ArrayEquals<T>(System.Collections.Generic.IReadOnlyList<T> a, System.Collections.Generic.IReadOnlyList<T> b)
            => System.Linq.Enumerable.SequenceEqual(a, b);
        public static int ArrayHash<T>(System.Collections.Generic.IReadOnlyList<T> a)
            => CombineHashCodes(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Cast<object>(a)));
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

        [Test]
        public static void GenericSum_EmitsAGenericStructAndInstantiatedFactoryCalls()
        {
            // TKT-443: Option<T> is one generic struct; a case built at a use names the instance
            // (`Option<Number>.Some(x)`), a helper over a closed instance is an extension method
            // in the library class, and a helper over Option<$T> is a member of the struct.
            var w = Emit("option.plato");
            var src = w.Files["_Option.g.cs"].ToString();
            Assert.That(src, Does.Contain("public readonly partial struct Option<T>"));
            Assert.That(src, Does.Contain("public readonly T Some_Value"));
            Assert.That(src, Does.Contain("public static Option<T> Some(T value) => new Option<T>(Kind_Some, value)"));
            Assert.That(src, Does.Contain("public static Option<T> None() => new Option<T>(Kind_None, default)"));
            Assert.That(src, Does.Contain("public T GetOrElse(T fallback) => this.IsNone() ? fallback : this.Some_Value"));
            var lib = w.Files["Options.g.cs"].ToString();
            Assert.That(lib, Does.Contain("Option<Number> Wrap(this Number x) => Ara3D.Geometry.Option<Number>.Some(x)"));
            Assert.That(lib, Does.Contain("Option<Number> Empty(this Number x) => Ara3D.Geometry.Option<Number>.None()"));
            Assert.That(lib, Does.Contain("Option<Number> KeepSome(this Option<Number> o) =>"));
        }

        // --- strong gate: compile and execute ------------------------------------

        [Test]
        public static void FillRule_CompilesAndRuns()
        {
            var w = Emit("fillrule.plato");
            var asm = CSharpTestCompilation.CompileAndLoad(Prelude,
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
            var asm = CSharpTestCompilation.CompileAndLoad(Prelude,
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
        /// plato-458: equality and hashing read only the active case. A Dot leaves the Poly case's
        /// array at null; comparing every flattened field dereferenced it.
        /// </summary>
        [Test]
        public static void SumEquality_ComparesOnlyTheActiveCase()
        {
            var w = Emit("sum-with-array.plato");
            // The generated projects compile with ImplicitUsings; the array field needs the one
            // for System.Collections.Generic.
            var asm = CSharpTestCompilation.CompileAndLoad(Prelude, "global using System; global using System.Collections.Generic;",
                w.Files["_Shape.g.cs"].ToString());
            var shape = asm.GetType("Ara3D.Geometry.Shape", true);
            object Dot(int n) => shape.GetMethod("Dot").Invoke(null, new object[] { (Ara3D.Geometry.Integer)n });
            object Poly(System.Collections.Generic.IReadOnlyList<Ara3D.Geometry.Integer> c) => shape.GetMethod("Poly").Invoke(null, new object[] { c });

            Assert.AreEqual(true, Dot(3).Equals(Dot(3)));
            Assert.AreEqual(false, Dot(3).Equals(Dot(4)));
            Assert.AreEqual(Dot(3).GetHashCode(), Dot(3).GetHashCode());
            var corners = new Ara3D.Geometry.Integer[] { 1, 2, 3 };
            Assert.AreEqual(true, Poly(corners).Equals(Poly(corners)));
            // A copy built separately is equal too: arrays compare element-wise (TKT-355).
            Assert.AreEqual(true, Poly(corners).Equals(Poly(new Ara3D.Geometry.Integer[] { 1, 2, 3 })));
            Assert.AreEqual(Poly(corners).GetHashCode(), Poly(new Ara3D.Geometry.Integer[] { 1, 2, 3 }).GetHashCode());
            Assert.AreEqual(false, Poly(corners).Equals(Dot(3)));
            Assert.AreEqual(false, Dot(3).Equals(Poly(corners)));
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

            var asm = CSharpTestCompilation.CompileAndLoad(Prelude,
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
