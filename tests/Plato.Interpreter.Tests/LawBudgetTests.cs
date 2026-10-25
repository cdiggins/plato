using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;
using System.Collections.Concurrent;

namespace Plato.Interpreter.Tests;

[TestFixture]
[NonParallelizable]
public class LawBudgetTests
{
    private const string Source = """
        type BudgetValue { Value: Integer; Step: Function1<Integer, Integer>; }
        library BudgetFixture
        {
            Witness_Value(_: BudgetValue): BudgetValue => new BudgetValue(2, x => x + 1);
            Law_First(x: BudgetValue): Boolean => x.Value == 2;
            Premise_First(x: BudgetValue): Boolean => true;
            Law_Second(x: BudgetValue): Boolean => x.Value > 0;
            Law_Quick(k: Integer): Boolean => k == k;
            Law_Generated(x: Number): Boolean => x == x;
        }
        """;

    // Finite delays make even abandoned calls finish; teardown waits for that completion.
    private sealed class DelayedBackend : ILawBackend, IDisposable
    {
        private readonly ILawBackend _inner;
        private readonly ConcurrentDictionary<Thread, byte> _threads = new();
        public readonly ManualResetEventSlim LawStarted = new(false);
        public int WitnessCalls;
        public int WitnessDelay = 800;
        public int LawDelay;
        public int PremiseDelay;
        public int GeneratedValueDelay;
        public DateTime? Deadline;
        public string Name => _inner.Name;
        public bool SupportsParallel => _inner.SupportsParallel;
        public DelayedBackend(ILawBackend inner) => _inner = inner;
        public object FromInterpreterValue(object value)
        {
            Thread.Sleep(GeneratedValueDelay);
            return _inner.FromInterpreterValue(value);
        }
        public object ToInterpreterValue(object value) => _inner.ToInterpreterValue(value);
        public void SetDeadline(DateTime? deadline) { Deadline = deadline; _inner.SetDeadline(deadline); }
        public object Invoke(FunctionDef function, object[] args)
        {
            var witness = function.Name.StartsWith("Witness_");
            _threads.TryAdd(Thread.CurrentThread, 0);
            if (witness)
                Interlocked.Increment(ref WitnessCalls);
            else if (function.Name.StartsWith("Law_"))
                LawStarted.Set();
            Thread.Sleep(witness ? WitnessDelay : function.Name.StartsWith("Premise_") ? PremiseDelay : LawDelay);
            if (!SupportsParallel && Deadline is DateTime deadline && DateTime.UtcNow > deadline)
                throw new EvaluationException(LawRunner.TimeBudgetMessage);
            return _inner.Invoke(function, args);
        }
        public void Dispose()
        {
            // Only compiled calls own disposable runner/witness threads. Interpreter calls
            // execute on the test thread, which must never join itself.
            if (SupportsParallel)
                foreach (var thread in _threads.Keys)
                    if (thread != Thread.CurrentThread)
                        Assert.That(thread.Join(TimeSpan.FromSeconds(10)), Is.True, "bounded worker must finish");
            LawStarted.Dispose();
        }
    }

    private static (LawRunner Runner, DelayedBackend Backend) Create(LawBackendKind kind, int parallel)
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var interpreter = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var backend = new DelayedBackend(LawBackends.Create(kind, interpreter));
        var runner = new LawRunner(backend, interpreter, new LawRunOptions
        {
            Filters = new[] { "BudgetFixture.Law_First", "BudgetFixture.Law_Second", "BudgetFixture.Law_Quick" }, MaxParallel = parallel,
            BudgetSeconds = 0.4, AbandonSeconds = 0.6, WitnessBudgetSeconds = 5,
        });
        return (runner, backend);
    }

    [TestCase(LawBackendKind.Interpreter, 1)]
    [TestCase(LawBackendKind.Interpreter, 2)]
    [TestCase(LawBackendKind.CSharp, 1)]
    [TestCase(LawBackendKind.CSharp, 2)]
    [TestCase(LawBackendKind.Differential, 1)]
    [TestCase(LawBackendKind.Differential, 2)]
    public void Cold_and_cached_runs_exclude_shared_witness_setup(LawBackendKind kind, int parallel)
    {
        var (runner, backend) = Create(kind, parallel);
        using (backend)
        {
            var cold = runner.RunAll();
            var warm = runner.RunAll();
            TestContext.Out.WriteLine(string.Join("\n", cold.Concat(warm).Select(r => r.ToString())));
            TestContext.Out.WriteLine(string.Join("\n", runner.Witnesses));
            Assert.That(cold, Has.Count.EqualTo(3));
            Assert.That(warm, Has.Count.EqualTo(3));
            Assert.That(cold.Select(r => r.Status), Is.All.EqualTo(LawStatus.Pass));
            Assert.That(warm.Select(r => r.Status), Is.EqualTo(cold.Select(r => r.Status)));
            Assert.That(warm.Select(r => r.Trials), Is.EqualTo(cold.Select(r => r.Trials)));
            Assert.That(backend.WitnessCalls, Is.EqualTo(1));
            Assert.That(runner.Witnesses.Single().Elapsed.TotalSeconds, Is.GreaterThan(0.6));
            Assert.That(backend.Deadline, Is.Null);
        }
    }

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp)]
    [TestCase(LawBackendKind.Differential)]
    public void Direct_run_has_its_own_budget_after_preparation(LawBackendKind kind)
    {
        var (runner, backend) = Create(kind, 1);
        using (backend)
        {
            var law = runner.Laws.Single(f => f.Name == "Law_First");
            var first = runner.Run(law);
            Assert.That(first.Status, Is.EqualTo(LawStatus.Pass), first.ToString());
            backend.LawDelay = 500;
            // The elapsed budget, not abandonment, decides this bounded slow call.
            runner.Options.AbandonSeconds = 5;
            Assert.That(runner.Run(law).Status, Is.EqualTo(LawStatus.Error));
            Assert.That(backend.Deadline, Is.Null);
            runner.Options.BudgetSeconds = null;
            Assert.That(runner.Run(law).Status, Is.EqualTo(LawStatus.Pass));
        }
    }

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp)]
    [TestCase(LawBackendKind.Differential)]
    public void Drawing_a_generated_case_remains_charged(LawBackendKind kind)
    {
        var (runner, backend) = Create(kind, 1);
        using (backend)
        {
            // Number is generated lazily. Its backend conversion occurs inside MoveNext,
            // not domain preparation; unlike Integer it has no eagerly converted finite domain.
            backend.GeneratedValueDelay = 800;
            runner.Options.Trials = 1;
            runner.Options.AbandonSeconds = 5;
            var result = runner.Run(runner.AllLaws.Single(f => f.Name == "Law_Generated"));
            Assert.That(result.Status, Is.EqualTo(LawStatus.Error));
            Assert.That(result.Message, Does.Contain("budget"));
            Assert.That(backend.WitnessCalls, Is.Zero);
            Assert.That(backend.Deadline, Is.Null);
        }
    }

    [Test]
    public void A_bounded_compiled_law_is_abandoned_and_its_worker_is_joined()
    {
        var (runner, backend) = Create(LawBackendKind.CSharp, 1);
        using (backend)
        {
            backend.LawDelay = 1500;
            runner.Options.BudgetSeconds = null;
            runner.Options.AbandonSeconds = 0.6;
            // Direct invocation makes the recorded execution thread joinable in Dispose.
            var result = runner.Run(runner.Laws.Single(f => f.Name == "Law_First"));
            Assert.That(backend.LawStarted.Wait(TimeSpan.FromSeconds(10)), Is.True);
            Assert.That(result.Status, Is.EqualTo(LawStatus.Error));
            Assert.That(result.Message, Does.Contain("abandoned"));
        }
    }

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp)]
    [TestCase(LawBackendKind.Differential)]
    public void Premise_evaluation_remains_charged(LawBackendKind kind)
    {
        var (runner, backend) = Create(kind, 1);
        using (backend)
        {
            backend.PremiseDelay = 500;
            runner.Options.AbandonSeconds = 5;
            var result = runner.Run(runner.Laws.Single(f => f.Name == "Law_First"));
            Assert.That(result.Status, Is.EqualTo(LawStatus.Error));
            Assert.That(result.Message, Does.Contain("budget"));
            Assert.That(backend.Deadline, Is.Null);
        }
    }

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp)]
    [TestCase(LawBackendKind.Differential)]
    public void Witness_cap_still_faults_without_blocking_unrelated_laws(LawBackendKind kind)
    {
        var (runner, backend) = Create(kind, 1);
        using (backend)
        {
            runner.Options.WitnessBudgetSeconds = 0.05;
            var results = runner.RunAll().ToDictionary(r => r.Law.Name);
            Assert.That(results["Law_First"].Status, Is.EqualTo(LawStatus.Error));
            Assert.That(results["Law_First"].Message, Does.Contain("witness cap"));
            Assert.That(results["Law_Second"].Status, Is.EqualTo(LawStatus.Error));
            Assert.That(results["Law_Quick"].Status, Is.EqualTo(LawStatus.Pass));
            Assert.That(runner.Witnesses.Single().Fault, Does.Contain("witness cap"));
            Assert.That(backend.Deadline, Is.Null);
        }
    }
}
