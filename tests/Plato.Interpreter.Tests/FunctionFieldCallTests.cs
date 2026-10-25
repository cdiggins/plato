using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

[TestFixture]
[NonParallelizable]
public class FunctionFieldCallTests
{
    // Separate fixture so a precedence discrepancy cannot hide the basic field regressions.
    private const string CollisionSource = """
        type CollisionCarrier { WorkAt: Function1<Integer, Integer>; }
        library FunctionFieldCollision
        {
            WorkAt(c: CollisionCarrier, n: Integer): Integer => n + 100;
            Law_NamedMethodPrecedence(n: Integer): Boolean
            {
                var c = CollisionCarrier(x => x + 3);
                return c.WorkAt(n) == n + 100;
            }
        }
        """;

    private const string Source = """
        type FunctionCarrier { WorkAt: Function1<Integer, Integer>; }
        type BinaryCarrier { WorkAt: Function2<Integer, Integer, Integer>; }
        type DataCarrier { Value: Integer; }
        library FunctionFieldFixture
        {
            ApplyAt(n: Integer, f: Function1<Integer, Integer>): Integer => f(n);
            Value(c: DataCarrier, n: Integer): Integer => c.Value + n;
            Law_Direct(n: Integer): Boolean
            {
                var c = FunctionCarrier(x => x + 3);
                return c.WorkAt(n) == n + 3;
            }
            Law_Local(n: Integer): Boolean
            {
                var c = FunctionCarrier(x => x + 3);
                var f = c.WorkAt;
                return f(n) == n + 3;
            }
            Law_Parameter(n: Integer): Boolean
                => ApplyAt(n, FunctionCarrier(x => x + 3).WorkAt) == n + 3;
            Law_CapturedDistinct(n: Integer): Boolean
            {
                var a = FunctionCarrier(x => x + n);
                var b = FunctionCarrier(x => x - n);
                return a.WorkAt(7) == 7 + n && b.WorkAt(7) == 7 - n;
            }
            Law_Binary(n: Integer): Boolean
                => BinaryCarrier((a, b) => a - b).WorkAt(n, 4) == n - 4;
            Law_DataProjectionAndMethod(n: Integer): Boolean
            {
                var c = DataCarrier(5);
                return c.Value == 5 && c.Value(n) == n + 5;
            }
        }
        """;

    [Test]
    public void Field_projection_and_local_application_check_clean()
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var errors = new TypeChecker(compilation).CheckAll()
            .Where(r => r.Function?.OwnerType?.Name == "FunctionFieldFixture")
            .SelectMany(r => r.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => $"{r.Function.Name}: {d.Code}: {d.Message}"));
        Assert.That(errors, Is.Empty);
    }

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp)]
    [TestCase(LawBackendKind.Differential)]
    public void Calls_preserve_values_and_noncallable_method_fallback(LawBackendKind kind)
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var interpreter = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var backend = LawBackends.Create(kind, interpreter);
        var runner = new LawRunner(backend, interpreter, new LawRunOptions
        {
            Filters = new[] { "FunctionFieldFixture" }, BudgetSeconds = 2, MaxParallel = 1,
        });
        var results = runner.RunAll();
        TestContext.Out.WriteLine(string.Join("\n", results.Select(r => r.ToString())));
        Assert.That(results, Has.Count.EqualTo(6));
        Assert.That(results.Select(r => r.Status), Is.All.EqualTo(LawStatus.Pass));
        Assert.That(results.All(r => r.Trials > 0), Is.True);
    }

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp)]
    [TestCase(LawBackendKind.Differential)]
    public void Named_collision_follows_checker_resolution(LawBackendKind kind)
    {
        var compilation = LawRunnerSelectionTests.CompileSource(CollisionSource);
        var checkedLaw = new TypeChecker(compilation).CheckAll()
            .Single(r => r.Function?.Name == "Law_NamedMethodPrecedence");
        Assert.That(checkedLaw.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty);
        // Elaborator prioritizes an explicitly resolved named callee over syntactic dispatch.
        // Fail here if the checker does not establish that premise; do not infer it from our patch.
        Assert.That(checkedLaw.ResolvedCalls.Values.Any(r => r.Callee?.Name == "WorkAt"
            && r.Callee.OwnerType?.Name == "FunctionFieldCollision"), Is.True,
            "collision precedence must be established by checker resolution");
        var interpreter = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var runner = new LawRunner(LawBackends.Create(kind, interpreter), interpreter,
            new LawRunOptions { Filters = new[] { "FunctionFieldCollision" }, BudgetSeconds = 2, MaxParallel = 1 });
        var results = runner.RunAll();
        TestContext.Out.WriteLine(string.Join("\n", results.Select(r => r.ToString())));
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results.Single().Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(results.Single().Trials, Is.GreaterThan(0));
    }
}
