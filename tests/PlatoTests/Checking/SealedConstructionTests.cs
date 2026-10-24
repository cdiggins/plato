// Checks which constructors may create a sealed Plato type.
using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// "sealed by F, G" (TKT-444): only the named functions may construct a sealed type, so a proof
    /// type such as the STEP mesher's CheckedMesh is built only by its checker. Every way of
    /// constructing the type outside them is CHK311: a constructor call, <c>new</c>, a tuple
    /// converted at a return or as an argument, a sum case, and <c>default</c>. Field reads,
    /// <c>match</c>, and calls to the named functions stay allowed everywhere.
    /// </summary>
    [TestFixture]
    public static class SealedConstructionTests
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

type Mesh
{
    Area: Number;
    Count: Number;
}

type CheckedMesh
    sealed by CheckMesh
{
    Mesh: Mesh;
    Tolerance: Number;
}

type Verdict
    sealed by Judge
    = Pass
    | Fail(Reason: Number);

type Free
{
    Value: Number;
    Scale: Number;
}

library Checks
{
    CheckMesh(m: Mesh, tolerance: Number): CheckedMesh => CheckedMesh(m, tolerance);
    CheckMesh(m: Mesh): CheckedMesh => (m, m.Area);
    CheckMesh(n: Number): Number => n;

    Judge(x: Number): Verdict => Verdict.Fail(x);
    Judge(): Verdict => Verdict.Pass;

    Forge(m: Mesh): CheckedMesh => CheckedMesh(m, 1.0);
    ForgeNew(m: Mesh): CheckedMesh => new CheckedMesh(m, 1.0);
    ForgeTuple(m: Mesh): CheckedMesh => (m, 1.0);
    UseChecked(c: CheckedMesh): Number => c.Tolerance;
    ForgeArgument(m: Mesh): Number => UseChecked((m, 1.0));
    ForgeCase(): Verdict => Verdict.Pass;
    ForgePayloadCase(x: Number): Verdict => Verdict.Fail(x);
    ForgeDefault(): CheckedMesh => default;

    ReadFields(c: CheckedMesh): Number => c.Mesh.Area;
    Delegate(m: Mesh): CheckedMesh => CheckMesh(m, 1.0);
    Inspect(v: Verdict): Number =>
        match (v) {
            Pass => 0.0;
            Fail(r) => r;
        };
    BuildFree(x: Number): Free => Free(x, x);
    BuildFreeTuple(x: Number): Free => (x, x);
}
";

        private static readonly Lazy<TypeCheckResult[]> Results = new Lazy<TypeCheckResult[]>(() =>
        {
            var c = Compile(Source);
            CollectionAssert.IsEmpty(c.SymbolFactory.Errors.Select(e => e.Message).ToArray(),
                "the fixture must resolve cleanly or every assertion below would be vacuous");
            return new TypeChecker(c).CheckAll().ToArray();
        });

        private static Compilation Compile(string source)
        {
            var dir = Path.Combine(Path.GetTempPath(), "tkt444-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source);
                return CheckerTestSupport.CompileFolder(dir);
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        private static CheckDiagnostic[] DiagnosticsFor(string functionName)
        {
            var results = Results.Value.Where(r => r.Function?.Name == functionName).ToArray();
            Assert.That(results, Is.Not.Empty, $"no checked function named {functionName}");
            return results.SelectMany(r => r.Diagnostics).ToArray();
        }

        private static string Describe(CheckDiagnostic[] ds)
            => string.Join(" | ", ds.Select(d => d.Code + ": " + d.Message));

        [TestCase("Forge", "constructor call")]
        [TestCase("ForgeNew", "new")]
        [TestCase("ForgeTuple", "tuple conversion")]
        [TestCase("ForgeArgument", "tuple conversion")]
        [TestCase("ForgeDefault", "default")]
        public static void A_proof_record_constructed_outside_its_checker_is_CHK311(string function, string how)
        {
            var ds = DiagnosticsFor(function);
            var sealedErrors = ds.Where(d => d.Code == SealedConstruction.Code).ToArray();
            Assert.That(sealedErrors, Has.Length.EqualTo(1), Describe(ds));
            Assert.That(sealedErrors[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
            Assert.That(sealedErrors[0].Message, Does.Contain("'CheckedMesh' is sealed by CheckMesh"));
            Assert.That(sealedErrors[0].Message, Does.Contain($"'{function}' may not construct it ({how})"));
            Assert.That(sealedErrors[0].Origin, Is.Not.Null, "the error points at the construction");
        }

        [TestCase("ForgeCase", "case Pass")]
        [TestCase("ForgePayloadCase", "case Fail")]
        public static void A_sum_case_built_outside_the_named_functions_is_CHK311(string function, string how)
        {
            var ds = DiagnosticsFor(function);
            Assert.That(ds.Where(d => d.Code == SealedConstruction.Code).Select(d => d.Message),
                Is.EqualTo(new[] { $"'Verdict' is sealed by Judge; '{function}' may not construct it ({how})" }), Describe(ds));
        }

        [TestCase("CheckMesh")]
        [TestCase("Judge")]
        [TestCase("ReadFields")]
        [TestCase("Delegate")]
        [TestCase("Inspect")]
        [TestCase("UseChecked")]
        [TestCase("BuildFree")]
        [TestCase("BuildFreeTuple")]
        public static void The_named_functions_field_reads_match_and_unsealed_types_stay_clean(string function)
        {
            var ds = DiagnosticsFor(function);
            Assert.That(ds.Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty, Describe(ds));
        }

        [Test]
        public static void Only_an_overload_that_returns_the_type_may_construct_it()
        {
            var c = Compile(Source);
            var type = c.GetTypeDef("CheckedMesh");
            var overloads = c.FunctionDefinitions.Where(f => f.Name == "CheckMesh").ToList();
            Assert.That(overloads.Select(f => SealedConstruction.Permits(type, f)), Is.EquivalentTo(new[] { true, true, false }));
            Assert.That(type.SealedBy, Is.EqualTo(new[] { "CheckMesh" }));
        }

        [TestCase("Nobody", "no function named 'Nobody' returns 'CheckedMesh'")]
        [TestCase("Measure", "no function named 'Measure' returns 'CheckedMesh'")]
        public static void Sealing_by_a_name_that_builds_no_value_of_the_type_halts_compilation(string name, string message)
        {
            var c = Compile(@"
type Number { }
type Mesh { Area: Number; }
type CheckedMesh
    sealed by " + name + @"
{
    Mesh: Mesh;
}
library Checks
{
    Measure(m: Mesh): Number => m.Area;
}
");
            Assert.That(c.CompletedCompilation, Is.False);
            Assert.That(c.SymbolFactory.Errors.Select(e => e.Message), Has.Some.Contains(message));
        }
    }
}
