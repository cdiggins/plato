using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

[TestFixture]
[NonParallelizable]
public class FunctionFieldCollisionControls
{
    private const string Source = """
        type Adversarial { __PlatoFieldCalls0: Function1<Integer, Integer>; }
        type Collider { WorkAt: Function1<Integer, Integer>; }
        type OtherReceiver { Bias: Integer; }
        type FieldOnly { WorkAt: Function1<Integer, Integer>; }
        library CollisionControls
        {
            __PlatoFieldCalls0(c: Adversarial, n: Integer): Integer => n + 9;
            Law_ClassMethodName(n: Integer): Boolean => Adversarial(x => x + 1).__PlatoFieldCalls0(n) == n + 9;
            Law_TypeReceiver(n: Integer): Boolean => Collider.WorkAt(n) == n + 100;
            WorkAt(c: Collider, n: Integer): Integer => n + 100;
            WorkAt(c: OtherReceiver, n: Integer): Integer => c.Bias + n;
            Law_OtherReceiver(n: Integer): Boolean => OtherReceiver(7).WorkAt(n) == n + 7;
            Law_ExplicitProjection(n: Integer): Boolean
            {
                var f = Collider(x => x + 3).WorkAt;
                return f(n) == n + 3;
            }
            Law_FieldOnly(n: Integer): Boolean => FieldOnly(x => x - 2).WorkAt(n) == n - 2;
        }
        """;

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp)]
    [TestCase(LawBackendKind.Differential)]
    public void Projection_and_other_receiver_remain_distinct(LawBackendKind kind)
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var checkedLaws = new TypeChecker(compilation).CheckAll()
            .Where(r => r.Function?.OwnerType?.Name == "CollisionControls");
        Assert.That(checkedLaws.SelectMany(r => r.Diagnostics)
            .Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty);
        var interpreter = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var backend = LawBackends.Create(kind, interpreter);
        var results = new LawRunner(backend, interpreter, new LawRunOptions
        {
            Filters = new[] { "CollisionControls.Law_" }, BudgetSeconds = 2, MaxParallel = 1,
        }).RunAll();
        TestContext.Out.WriteLine(string.Join("\n", results.Select(r => r.ToString())));
        Assert.That(results.Select(r => r.Name), Is.EquivalentTo(new[]
        {
            "CollisionControls.Law_ClassMethodName", "CollisionControls.Law_TypeReceiver",
            "CollisionControls.Law_OtherReceiver", "CollisionControls.Law_ExplicitProjection",
            "CollisionControls.Law_FieldOnly",
        }));
        if (backend is DifferentialBackend differential)
            Assert.That(differential.Mismatches, Is.Empty);
        Assert.That(results, Has.Count.EqualTo(5));
        Assert.That(results.All(r => r.Status == LawStatus.Pass && r.Trials == 14), Is.True);
    }
}
