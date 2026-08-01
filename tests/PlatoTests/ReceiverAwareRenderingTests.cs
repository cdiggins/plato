using System;
using System.IO;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Pins the RECEIVER-AWARE property-vs-method rendering rule
    /// (<see cref="CSharpWriter.IsStructSurfaceProperty"/>, plato-323 item 2).
    ///
    /// A no-arg member renders with property/field syntax iff the name is
    /// on the RECEIVER's own struct surface. The rule used to be decided globally by name, from the
    /// union of every generated type's field names, so a field on ONE struct forced property syntax
    /// onto a same-named library function of every OTHER receiver — where that function is a method.
    /// Measured in the forward conformance build: a field named <c>Amount</c> (on an image filter, a
    /// dynamic quantity and an uncertainty value) pinned property syntax onto <c>Amount(x: Angle)</c>
    /// and its 60 sibling quantity projections, producing 110 x CS0030
    /// "cannot convert type 'method'".
    ///
    /// The corpus below is that collision in miniature: <c>Filter</c> has a genuine field
    /// <c>Amount</c>; <c>Temp</c> does not and reaches <c>Amount</c> through a bodied library
    /// function. Both readings must coexist in one build — <c>f.Amount</c> and <c>t.Amount()</c>.
    ///
    /// Note the name is still DEMOTED back into <c>Temp</c>'s struct rather than moved to the library
    /// class: that is a separate, still-global decision (a C# instance member silently hides a
    /// same-name extension method, so where a member lives cannot be decided per receiver). Only the
    /// SPELLING is receiver-aware. <see cref="ExtensionStylePlan.DemoteMovedNames"/>.
    /// </summary>
    [TestFixture]
    public static class ReceiverAwareRenderingTests
    {
        // Self-contained corpus in the style of ArrayReceiverLibraryEmissionTests: the scalar
        // wrappers plus the delegate types the writer expects, and nothing System.Numerics-backed.
        private const string Source = @"
type Number { }
type Integer { }
type Boolean { }
type Character { }
type String { }
type Object { }

type Function0<TR> { }
type Function1<T0, TR> { }
type Function2<T0, T1, TR> { }
type Function3<T0, T1, T2, TR> { }

// Carries `Amount` as a genuine FIELD: reads of it stay field syntax.
type Filter
{
    Amount: Number;
}

// Does NOT carry `Amount`: it reaches the name through a library function, which is a METHOD.
type Temp
{
    Kelvin: Number;
}

library Quantities
{
    Amount(x: Temp): Number => x.Kelvin;

    // Call site of the METHOD reading (must get parens).
    TempTwice(x: Temp): Number => x.Amount;

    // Call site of the FIELD reading (must NOT get parens).
    FilterRaw(f: Filter): Number => f.Amount;
}
";

        private static CSharpWriter Emit()
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-recv-render-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Source);
                // The shipping V2 recipe, which is the only one the rule applies to.
                var w = new CSharpWriter(CheckerTestSupport.CompileFolder(dir), "unused-recv-render")
                {
                    ExtensionStyle = true,
                    ScalarErase = true,
                };
                w.WriteAll("float");
                foreach (var kv in w.Files)
                    TestContext.WriteLine($"======== {kv.Key}\n{kv.Value}");
                return w;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        private static string File_(CSharpWriter w, string name)
        {
            Assert.IsTrue(w.Files.ContainsKey(name), $"no generated file '{name}'");
            return w.Files[name].ToString();
        }

        [Test]
        public static void NameNotOnReceiverSurfaceEmitsAsMethod()
        {
            var t = File_(Emit(), "_Temp.g.cs");
            Assert.That(t, Does.Contain("Amount()"),
                "`Amount` is not on Temp's struct surface, so Temp's member must be declared as a METHOD");
            Assert.That(t, Does.Not.Match(@"Amount\s*\{"),
                "`Amount` must not be declared as a property on Temp just because another struct has that field");
        }

        [Test]
        public static void CallSiteOnNonSurfaceReceiverGetsParens()
            => Assert.That(File_(Emit(), "Quantities.g.cs"), Does.Match(@"TempTwice[^\n]*x\.Amount\(\)"),
                "a read of `Amount` off a Temp receiver must be a method call");

        [Test]
        public static void CallSiteOnGenuineFieldKeepsFieldSyntax()
        {
            var q = File_(Emit(), "Quantities.g.cs");
            Assert.That(q, Does.Match(@"FilterRaw[^\n]*f\.Amount(?!\()"),
                "a read of the genuine field `Filter.Amount` must stay field syntax");
        }

        [Test]
        public static void GenuineFieldIsStillEmittedAsAField()
            => Assert.That(File_(Emit(), "_Filter.g.cs"), Does.Match(@"public readonly float Amount;"),
                "the field itself is unaffected by the rendering rule");
    }
}
