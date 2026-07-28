using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Pins how the C# writer synthesizes the <c>At</c>/<c>Count</c> obligations of an
    /// <c>IArray</c>-shaped concept, which it does from the type's FIELDS
    /// (<see cref="CSharpTypeWriter.GenerateFunc"/>) because those members are exempted from
    /// LINT001 (<c>Linter.MembersImplementedByWriter</c>) and so nothing else reports them.
    ///
    /// Two shapes must NOT be confused:
    ///   * fixed-arity (Point3D: X/Y/Z) — the fields ARE the components, so At/Count enumerate them;
    ///   * runtime-arity (VectorN: a single collection field) — the FIELD is the component surface,
    ///     so At/Count must delegate to it. Enumerating fields there yields Count == 1 and
    ///     At(0) == the whole collection, which is wrong at runtime while linting perfectly clean.
    ///
    /// The delegation must key off the field type's *concept* (does it implement IArray?), not off
    /// the spelling of its type name: the library declares such fields both as <c>Array&lt;T&gt;</c>
    /// (stdlib) and as <c>IArray&lt;T&gt;</c> (stdlib-legacy), and both are the same shape.
    ///
    /// A type that supplies its own At/Count library body never reaches this synthesis at all — an
    /// explicit body lands in ConcreteType.ImplementedFunctions (see RegularPolygon in the shipping
    /// generation), so declaring bodies remains the escape hatch for any shape not covered here.
    /// </summary>
    [TestFixture]
    public static class CollectionFieldAtCountTests
    {
        // A self-contained corpus in the style of plato-src-small/small.plato: only Number/Integer/
        // Boolean are primitives, and no System.Numerics-backed names (Vector3, Angle, ...) which
        // CSharpWriter hardcodes.
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

// Fixed arity: the fields ARE the components.
type FixedVector implements IArray<Number>
{
    X: Number;
    Y: Number;
    Z: Number;
}

// Runtime arity, field spelled with the concrete Array type (stdlib style).
type ArrayVectorN implements IArray<Number>
{
    Components: Array<Number>;
}

// Runtime arity, field spelled with the IArray concept (stdlib-legacy style).
type InterfaceVectorN implements IArray<Number>
{
    Components: IArray<Number>;
}
";

        private static CSharpWriter Emit()
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-at-count-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Source);
                var w = new CSharpWriter(CheckerTestSupport.CompileFolder(dir), "unused-at-count");
                w.WriteAll("float");
                return w;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        private static string[] Members(CSharpWriter w, string typeName, string member)
        {
            var file = "_" + typeName + ".g.cs";
            Assert.IsTrue(w.Files.ContainsKey(file), $"no generated file '{file}'");
            return w.Files[file].ToString()
                .Replace("\r\n", "\n").Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Contains(" " + member + "(") || l.Contains(" " + member + " "))
                .ToArray();
        }

        [Test]
        public static void FixedArityEnumeratesFields()
        {
            var w = Emit();
            var at = string.Join("\n", Members(w, "FixedVector", "At"));
            var count = string.Join("\n", Members(w, "FixedVector", "Count"));
            TestContext.WriteLine("FixedVector At:    " + at);
            TestContext.WriteLine("FixedVector Count: " + count);

            Assert.That(at, Does.Contain("X").And.Contain("Y").And.Contain("Z"),
                "fixed-arity At must enumerate the fields");
            Assert.That(count, Does.Contain("3"), "fixed-arity Count must be the field count");
        }

        [Test]
        public static void RuntimeArityDelegatesToArrayTypedField()
            => AssertDelegates("ArrayVectorN");

        [Test]
        public static void RuntimeArityDelegatesToIArrayTypedField()
            => AssertDelegates("InterfaceVectorN");

        private static void AssertDelegates(string typeName)
        {
            var w = Emit();
            var at = string.Join("\n", Members(w, typeName, "At"));
            var count = string.Join("\n", Members(w, typeName, "Count"));
            TestContext.WriteLine(typeName + " At:    " + at);
            TestContext.WriteLine(typeName + " Count: " + count);

            Assert.That(count, Does.Contain("Components.Count"),
                $"{typeName}.Count must delegate to the collection field, not report the field count (1)");
            Assert.That(at, Does.Contain("Components["),
                $"{typeName}.At must index the collection field, not return the field itself");
        }
    }
}
