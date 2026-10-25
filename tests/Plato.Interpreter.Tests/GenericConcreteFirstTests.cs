using System.Reflection;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

[TestFixture]
[NonParallelizable]
public class GenericConcreteFirstTests
{
    private static string Source
        => File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory,
            "fixtures", "generic-concrete-first.plato"));

    [Test]
    public void Concrete_first_helper_checks_without_diagnostics()
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var errors = new TypeChecker(compilation).CheckAll()
            .Where(r => r.Function?.OwnerType?.Name == "ConcreteFirstGenericRepro")
            .SelectMany(r => r.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.That(errors, Is.Empty);
    }

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp)]
    [TestCase(LawBackendKind.Differential)]
    public void Concrete_first_helper_preserves_both_typed_values_and_work(LawBackendKind kind)
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var interpreter = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var backend = LawBackends.Create(kind, interpreter);
        if (backend is CSharpBackend compiled)
        {
            Assert.That(compiled.Program.DegradedBodies.Where(s => s.Contains("CountedFlips")), Is.Empty);
            var methods = compiled.Program.Assembly.GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                .Where(m => m.Name == "CountedFlips").ToArray();
            Assert.That(methods, Is.Not.Empty);
            TestContext.Out.WriteLine(string.Join("\n", methods.Select(m =>
                $"{m}: generic definition={m.IsGenericMethodDefinition}")));
        }
        var runner = new LawRunner(backend, interpreter, new LawRunOptions
        {
            Filters = new[] { "ConcreteFirstGenericRepro" }, BudgetSeconds = 2, MaxParallel = 1,
        });
        var results = runner.RunAll();
        TestContext.Out.WriteLine(string.Join("\n", results.Select(r => r.ToString())));
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results.Select(r => r.Law.Name), Is.EquivalentTo(new[]
        {
            "Law_ConcreteFirstInteger", "Law_ConcreteFirstBoolean",
        }));
        Assert.That(results.Select(r => r.Status), Is.All.EqualTo(LawStatus.Pass));
        Assert.That(results.Select(r => r.Trials), Is.All.EqualTo(1));
        Assert.That(results.Select(r => r.Exhaustive), Is.All.True);
        if (backend is DifferentialBackend differential)
            Assert.That(differential.Mismatches, Is.Empty);
    }
}
