// Checks that == and != between records and between arrays type-check (TKT-1168).
using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// The interpreter and the C# and C++ writers compare two records, and two arrays, field by field
    /// and element by element; the checker declared no <c>Equals</c> for them and reported CHK201.
    /// The fixture is the minimal witness of TKT-1168 plus the negatives that must stay errors.
    /// </summary>
    [TestFixture]
    public static class StructuralEqualityTests
    {
        private const string Prelude = @"
type Number { }
type Integer { }
type Boolean { }
type Object { }
type String { }
type Array<T> { }
type Function1<T0, TR> { }
type Pair { A: Integer; B: Integer; }
type Other { A: Integer; B: Integer; }
library Ops
{
    Equals(a: Integer, b: Integer): Boolean;
}
";

        private static string Codes(string body)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-streq-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Prelude + "library Laws\n{\n" + body + "\n}\n");
                var c = CheckerTestSupport.CompileFolder(dir);
                return string.Join(",", new TypeChecker(c).CheckAll()
                    .SelectMany(r => r.Diagnostics)
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.Code).Distinct());
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        [Test]
        public static void ArraysCompareStructurally()
            => Assert.AreEqual("", Codes("A(_: Integer): Boolean => [1, 2] == [1, 2];"));

        [Test]
        public static void RecordsCompareStructurally()
            => Assert.AreEqual("", Codes("R(_: Integer): Boolean => Pair(1, 2) == Pair(1, 2);"));

        [Test]
        public static void NotEqualsOnRecordsAndArraysToo()
            => Assert.AreEqual("", Codes(
                "N(_: Integer): Boolean => Pair(1, 2) != Pair(1, 3);\nM(_: Integer): Boolean => [1] != [2];"));

        [Test]
        public static void DifferentRecordTypesStillMismatch()
            => Assert.AreEqual("CHK201", Codes("D(_: Integer): Boolean => Pair(1, 2) == Other(1, 2);"));

        [Test]
        public static void FunctionsAreNotComparable()
            => Assert.AreEqual("CHK201", Codes(
                "F(f: Function1<Integer, Integer>, g: Function1<Integer, Integer>): Boolean => f == g;"));
    }
}
