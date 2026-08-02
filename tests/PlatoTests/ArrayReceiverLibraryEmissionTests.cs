using System;
using System.IO;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Pins the emission path for library functions whose RECEIVER is an array type (plato-323).
    ///
    /// The forward stdlib spells a list receiver with the concrete <c>Array&lt;T&gt;</c> type
    /// (<c>BoundsOfPoints(points: Array&lt;Point2D&gt;): Bounds2D</c>); stdlib-legacy spells the
    /// same shape with the <c>IArray&lt;T&gt;</c> interface. Both render as
    /// <c>IReadOnlyList&lt;T&gt;</c>, so both belong on the one classic-extension-method path in
    /// <c>Extensions.g.cs</c> (<see cref="CSharpWriter.WriteInterfaceLibraryMethods"/>).
    ///
    /// Before the fix only the interface spelling was emitted. <c>Array</c> is in
    /// <see cref="CSharpWriter.IgnoredTypes"/>, so array types get no <c>ExtensionStylePlan</c>, and
    /// moved members are only ever discovered while writing a CONCRETE type's own file — so an
    /// array-receiver library function was visited by NO writer and every call site of it was a
    /// CS1061 (~350 in the forward conformance build, 221 for <c>BoundsOfPoints</c> alone).
    ///
    /// Declared-only (body-less) array functions must NOT be emitted: those are the array
    /// INTRINSICS (Map/Reduce/Zip/...), supplied by the handwritten <c>Ara3D.Collections</c>
    /// runtime, and a generated stub would shadow it.
    /// </summary>
    [TestFixture]
    public static class ArrayReceiverLibraryEmissionTests
    {
        // A self-contained corpus in the style of CollectionFieldAtCountTests: only
        // Number/Integer/Boolean are primitives, and no System.Numerics-backed names.
        private const string Source = @"
interface IArray<T>
{
    Count(xs: Self): Integer;
    At(xs: Self, n: Integer): T;
}

type Number { }
type Integer { }
type Boolean { }
type Object { }

type Function0<TR> { }
type Function1<T0, TR> { }
type Function2<T0, T1, TR> { }
type Function3<T0, T1, T2, TR> { }

type Array<T> implements IArray<T> { }

type Pt
{
    X: Number;
    Y: Number;
}

library ArrayKernels
{
    // Bodied, CONCRETE element type - the plato-323 shape.
    FirstX(points: Array<Pt>): Number => points.At(0).X;

    // Bodied, GENERIC element type: emits as a generic extension method.
    Head(xs: Array<$T>): $T => xs.At(0);

    // Declared-only: an array INTRINSIC, provided by the handwritten runtime.
    Last(xs: Array<$T>): $T;
}
";

        private static string EmitExtensions()
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-array-recv-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Source);
                var w = new CSharpWriter(CheckerTestSupport.CompileFolder(dir), "unused-array-recv");
                w.WriteAll("float");
                Assert.IsTrue(w.Files.ContainsKey("Extensions.g.cs"), "no generated Extensions.g.cs");
                var text = w.Files["Extensions.g.cs"].ToString();
                TestContext.WriteLine(text);
                return text;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        [Test]
        public static void ConcreteArrayReceiverEmitsListExtension()
            => Assert.That(EmitExtensions(), Does.Contain("FirstX").And.Contain("this IReadOnlyList<Pt> points"),
                "a bodied Array<Pt>-receiver library function must emit as an extension method on IReadOnlyList<Pt>");

        [Test]
        public static void GenericArrayReceiverEmitsGenericListExtension()
            => Assert.That(EmitExtensions(), Does.Contain("Head").And.Contain("this IReadOnlyList<_T0> xs"),
                "a bodied Array<$T>-receiver library function must emit as a generic extension method");

        [Test]
        public static void DeclaredOnlyArrayReceiverIsNotEmitted()
            => Assert.That(EmitExtensions(), Does.Not.Contain("Last"),
                "a body-less Array<$T>-receiver function is an intrinsic: the handwritten runtime owns it");
    }
}
