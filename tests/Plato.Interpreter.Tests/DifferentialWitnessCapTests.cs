using System.Diagnostics;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;
using NUnit.Framework;

namespace Plato.Interpreter.Tests;

[TestFixture]
[NonParallelizable]
public class DifferentialWitnessCapTests
{
    private const string Source = """
        type CapValue { Value: Integer; }
        library DifferentialCapFixture
        {
            Witness_Value(_: CapValue): CapValue => new CapValue(7);
            Law_Uses(x: CapValue): Boolean => x.Value == 7;
            Law_Independent(): Boolean => true;
        }
        """;

    public enum BlockAt { None, Invocation, Conversion }

    // The actual DifferentialBackend calls this uncooperative compiled-slot double.
    // It uses equivalent interpreter values so no C# emission or geometry is needed.
    private sealed class ControlledCompiled(object witness, BlockAt blockAt) : ILawBackend, IDisposable
    {
        public readonly ManualResetEventSlim Entered = new(false);
        public readonly ManualResetEventSlim Release = new(false);
        private readonly ManualResetEventSlim _drained = new(true);
        private int _blocked;
        public string Name => "controlled-compiled";
        public bool SupportsParallel => true;
        public void SetDeadline(DateTime? deadline) { }
        public object FromInterpreterValue(object value) => value;

        private void Pause(BlockAt phase)
        {
            if (phase != blockAt || Interlocked.Exchange(ref _blocked, 1) != 0)
                return;
            _drained.Reset();
            Entered.Set();
            try { Release.Wait(TimeSpan.FromSeconds(3)); }
            finally { _drained.Set(); }
        }

        public object Invoke(FunctionDef function, object[] args)
        {
            if (function.Name == "Witness_Value")
            {
                Pause(BlockAt.Invocation);
                return witness;
            }
            return function.Name == "Law_Independent"
                ? true
                : throw new InvalidOperationException("Unexpected controlled invocation: " + function.Name);
        }

        public object ToInterpreterValue(object value)
        {
            if (value is PlatoRecord record && record.Def.Name == "CapValue")
                Pause(BlockAt.Conversion);
            return value;
        }

        public void Dispose()
        {
            Release.Set();
            // Also drain an isolated compiled operation if the eventual fix abandons it.
            var drained = _drained.Wait(TimeSpan.FromSeconds(5));
            if (drained)
            {
                Entered.Dispose();
                Release.Dispose();
                _drained.Dispose();
            }
            Assert.That(drained, Is.True, "controlled compiled operation must finish");
        }
    }

    [TestCase(BlockAt.Invocation)]
    [TestCase(BlockAt.Conversion)]
    public void Differential_witness_cap_bounds_the_compiled_slot(BlockAt blockAt)
        => Check(blockAt, expectCap: true);

    [Test]
    public void Fast_arms_preserve_the_witness_and_unrelated_law()
        => Check(BlockAt.None, expectCap: false);

    private static void Check(BlockAt blockAt, bool expectCap)
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var interpreter = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var witnessFunction = interpreter.FunctionsNamed("Witness_Value").Single(f => f.Body != null);
        var witnessType = witnessFunction.Parameters[0].Type;
        // Elaboration is outside the cap; only subsequent actual witness execution is timed.
        var known = interpreter.Invoke(witnessFunction, [new TypeToken(witnessType)]);
        Assert.That(known, Is.TypeOf<PlatoRecord>());
        Assert.That(((PlatoRecord)known).Fields.Single(), Is.EqualTo(7L));
        using var controlled = new ControlledCompiled(known, blockAt);
        var differential = new DifferentialBackend(new InterpreterBackend(interpreter), controlled);
        var runner = new LawRunner(differential, interpreter, new LawRunOptions
        {
            BudgetSeconds = null, WitnessBudgetSeconds = 0.2, AbandonSeconds = 5,
            MaxParallel = 1, Filters = ["DifferentialCapFixture"],
        });
        IReadOnlyList<object>? witnesses = null;
        LawResult? independent = null;
        Exception? fault = null;
        var completed = false;
        var joined = false;
        var clock = Stopwatch.StartNew();
        var worker = new Thread(() =>
        {
            try
            {
                // Direct witness preparation avoids old/new law-clock differences and
                // generated-domain fallback after a witness fault.
                witnesses = runner.WitnessesOf(witnessType);
                independent = runner.Run(runner.Laws.Single(f => f.Name == "Law_Independent"));
            }
            catch (Exception error) { fault = error; }
        }) { IsBackground = true, Name = "bounded-differential-witness-cap-test" };
        worker.Start();
        try
        {
            if (expectCap)
                Assert.That(controlled.Entered.Wait(TimeSpan.FromSeconds(2)), Is.True,
                    "real interpreter must finish and reach the controlled compiled stage");
            completed = worker.Join(TimeSpan.FromSeconds(1));
            Assert.That(completed, Is.True, "witness cap must finish before the blocked arm is released");
            Assert.That(fault, Is.Null);
            Assert.That(witnesses, Has.Count.EqualTo(expectCap ? 0 : 1));
            Assert.That(independent, Is.Not.Null);
            Assert.That(independent!.Status, Is.EqualTo(LawStatus.Pass));
            Assert.That(independent.Trials, Is.EqualTo(1));
            Assert.That(runner.Witnesses, Has.Count.EqualTo(1));
            if (expectCap)
                Assert.That(runner.Witnesses.Single().Fault, Does.Contain("witness cap"));
            else
                Assert.That(runner.Witnesses.Single().Fault, Is.Null);
            Assert.That(differential.Mismatches, Is.Empty);
            Assert.That(interpreter.Deadline, Is.Null);
        }
        finally
        {
            controlled.Release.Set();
            joined = worker.Join(TimeSpan.FromSeconds(5));
            TestContext.Out.WriteLine($"stage={blockAt}; completed-before-release={completed}; joined={joined}; elapsed={clock.Elapsed}; witness-count={witnesses?.Count}; fault={fault}");
            Assert.That(joined, Is.True, "finite controlled wait and test worker must drain");
        }
    }
}
