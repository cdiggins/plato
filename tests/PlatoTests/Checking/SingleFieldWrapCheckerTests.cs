// Checks implicit wrapping and unwrapping of single-field types.
using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// A bare payload never becomes a one-field wrapper implicitly (TKT-445). The compiler-399
    /// decision (<c>tracker/decisions/2026-08-02-single-field-mirror-unwraps-only.md</c>) removed
    /// the generated <c>implicit operator ModelLength(Number)</c>; the checker has to refuse the
    /// same wrap, or the error surfaces only when the C# compiler meets the emitted bare value.
    /// Each bare case has an explicitly wrapped twin that must stay clean.
    /// </summary>
    [TestFixture]
    public static class SingleFieldWrapCheckerTests
    {
        private const string Source = @"
type Number { }
type Boolean { }
type Object { }
type String { }

type Tuple2<T0, T1>
{
    Item0: T0;
    Item1: T1;
}

type ModelLength
{
    Value: Number;
}

type Span
{
    Start: ModelLength;
    Count: Number;
}

library Ops
{
    UseLength(m: ModelLength): Number => m.Value;

    ReturnBare(x: Number): ModelLength => x;
    ReturnWrapped(x: Number): ModelLength => ModelLength(x);

    ArgumentBare(x: Number): Number => UseLength(x);
    ArgumentWrapped(x: Number): Number => UseLength(ModelLength(x));

    FieldBare(x: Number): Span => Span(x, x);
    FieldWrapped(x: Number): Span => Span(ModelLength(x), x);

    TupleFieldBare(x: Number): Span => (x, x);
    TupleFieldWrapped(x: Number): Span => (ModelLength(x), x);
}
";

        private static readonly Lazy<TypeCheckResult[]> Results = new Lazy<TypeCheckResult[]>(() =>
        {
            var dir = Path.Combine(Path.GetTempPath(), "tkt445-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Source);
                var c = CheckerTestSupport.CompileFolder(dir);
                CollectionAssert.IsEmpty(c.SymbolFactory.Errors.Select(e => e.ToString()).ToArray(),
                    "the fixture must resolve cleanly or every assertion below would be vacuous");
                return new TypeChecker(c).CheckAll().ToArray();
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        });

        private static CheckDiagnostic[] DiagnosticsFor(string functionName)
        {
            var results = Results.Value.Where(r => r.Function?.Name == functionName).ToArray();
            Assert.IsNotEmpty(results, $"{functionName} was not checked");
            return results.SelectMany(r => r.Diagnostics).ToArray();
        }

        private static string Show(CheckDiagnostic[] ds)
            => string.Join("\n", ds.Select(d => d.ToString()));

        [TestCase("ReturnBare")]
        [TestCase("ArgumentBare")]
        [TestCase("FieldBare")]
        [TestCase("TupleFieldBare")]
        public static void ABareNumber_WhereAOneFieldWrapperIsDeclared_IsATypeError(string functionName)
            => Assert.IsNotEmpty(DiagnosticsFor(functionName),
                $"{functionName} hands a Number to a ModelLength; only the C# compiler would catch it");

        [TestCase("ReturnWrapped")]
        [TestCase("ArgumentWrapped")]
        [TestCase("FieldWrapped")]
        [TestCase("TupleFieldWrapped")]
        public static void AnExplicitWrap_ChecksClean(string functionName)
        {
            var ds = DiagnosticsFor(functionName);
            Assert.IsEmpty(ds, Show(ds));
        }
    }
}
