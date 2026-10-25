using System.Collections;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;
using NUnit.Framework;

namespace Plato.Interpreter.Tests;

[TestFixture]
[NonParallelizable]
public class DifferentialDeadlineTests
{
    public enum BlockAt { Invocation, NestedComparison, MismatchFormatting }
    private const string Source = """
        type DeadlineValue { Value: Integer; }
        type DeadlineArray { Values: Array<Integer>; }
        library DifferentialDeadlineFixture
        {
            Witness_One(_: DeadlineValue): DeadlineValue => new DeadlineValue(7);
            Witness_Array(_: DeadlineArray): DeadlineArray => new DeadlineArray([1, 2, 3]);
            Law_Independent(): Boolean => true;
        }
        """;

    private sealed class Compiled(Func<object> invoke, Func<object, object> convert = null, bool parallel = true) : ILawBackend
    {
        public int Calls;
        public int InvokeThread;
        public int ConvertThread;
        public string Name => "controlled-compiled";
        public bool SupportsParallel => parallel;
        public void SetDeadline(DateTime? deadline) { }
        public object FromInterpreterValue(object value) => value;
        public object ToInterpreterValue(object value)
        {
            ConvertThread = System.Environment.CurrentManagedThreadId;
            return convert == null ? value : convert(value);
        }
        public object Invoke(FunctionDef function, object[] args)
        {
            Interlocked.Increment(ref Calls);
            InvokeThread = System.Environment.CurrentManagedThreadId;
            return function.Name == "Law_Independent" ? true : invoke();
        }
    }

    // Finite even on old source. Capturing and joining the actual compiled worker proves
    // the assertion after release is not racing late comparison/mismatch publication.
    private sealed class Gate : IDisposable
    {
        public readonly ManualResetEventSlim Entered = new(false);
        public readonly ManualResetEventSlim Release = new(false);
        private Thread _worker;
        private int _entered;
        public void Wait()
        {
            if (Interlocked.Exchange(ref _entered, 1) != 0)
                return;
            _worker = Thread.CurrentThread;
            Entered.Set();
            Release.Wait(TimeSpan.FromSeconds(3));
        }
        public void Drain()
        {
            Release.Set();
            if (_worker != null && _worker != Thread.CurrentThread)
                Assert.That(_worker.Join(TimeSpan.FromSeconds(5)), Is.True, "owned compiled worker must drain");
        }
        public void Dispose()
        {
            Drain();
            Entered.Dispose();
            Release.Dispose();
        }
    }

    private sealed class GatedItems(IReadOnlyList<object> values, Gate gate) : IReadOnlyList<object>
    {
        public int Reads;
        public int Count => values.Count;
        public object this[int index]
        {
            get
            {
                Interlocked.Increment(ref Reads);
                gate.Wait();
                return values[index];
            }
        }
        public IEnumerator<object> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class GatedDescription(Gate gate)
    {
        public override string ToString()
        {
            gate.Wait();
            return "late mismatch";
        }
    }

    private static (Ara3D.Geometry.Interpreter.Interpreter Interpreter, FunctionDef Function, object Value) Fixture(string name = "Witness_One")
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var interpreter = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var function = interpreter.FunctionsNamed(name).Single(f => f.Body != null);
        var value = interpreter.Invoke(function, [new TypeToken(function.Parameters[0].Type)]);
        return (interpreter, function, value);
    }

    [TestCase(BlockAt.Invocation)]
    [TestCase(BlockAt.NestedComparison)]
    [TestCase(BlockAt.MismatchFormatting)]
    public void Timed_out_worker_cannot_publish_late_mismatch_and_fault_is_cached(BlockAt blockAt)
    {
        var nestedComparison = blockAt == BlockAt.NestedComparison;
        var (interpreter, function, value) = Fixture(nestedComparison ? "Witness_Array" : "Witness_One");
        using var gate = new Gate();
        GatedItems items = null;
        object Other()
        {
            var record = (PlatoRecord)value;
            if (blockAt == BlockAt.MismatchFormatting)
                return new GatedDescription(gate);
            if (!nestedComparison)
            {
                gate.Wait();
                return new PlatoRecord(record.Type, [99L]);
            }
            var array = (PlatoArray)record.Fields[0];
            items = new GatedItems(array.Items, gate);
            return new PlatoRecord(record.Type, [new PlatoArray(array.ElementType, items)]);
        }
        var compiled = new Compiled(Other);
        var differential = new DifferentialBackend(new InterpreterBackend(interpreter), compiled);
        var runner = new LawRunner(differential, interpreter, new LawRunOptions
        {
            BudgetSeconds = null, WitnessBudgetSeconds = 0.2, AbandonSeconds = 5,
            MaxParallel = 1, Filters = ["DifferentialDeadlineFixture"],
        });
        var witnesses = runner.WitnessesOf(function.ReturnType);
        Assert.That(gate.Entered.IsSet, Is.True, "must reach the named compiled/comparison stage");
        Assert.That(witnesses, Is.Empty);
        Assert.That(runner.Witnesses.Single().Fault, Does.Contain("witness cap"));
        Assert.That(interpreter.Deadline, Is.Null);
        Assert.That(runner.WitnessesOf(function.ReturnType), Is.SameAs(witnesses));
        Assert.That(compiled.Calls, Is.EqualTo(1), "fault cache must not restart abandoned work");
        var independent = runner.Run(runner.Laws.Single(f => f.Name == "Law_Independent"));
        Assert.That(independent.Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(independent.Trials, Is.EqualTo(1));
        gate.Drain();
        Assert.That(differential.Mismatches, Is.Empty, "released late worker must not publish a mismatch");
        Assert.That(interpreter.Deadline, Is.Null);
        if (nestedComparison)
            Assert.That(items!.Reads, Is.EqualTo(1), "recursive comparison stops at captured deadline after first blocked read");
    }

    [Test]
    public void Expired_deadline_does_not_enter_compiled_arm()
    {
        var (interpreter, function, value) = Fixture();
        var compiled = new Compiled(() => value);
        var differential = new DifferentialBackend(new InterpreterBackend(interpreter), compiled);
        differential.SetDeadline(DateTime.UtcNow.AddSeconds(-1));
        try
        {
            var error = Assert.Throws<EvaluationException>(() => differential.Invoke(function, [new TypeToken(function.Parameters[0].Type)]));
            Assert.That(error!.Message, Does.StartWith(LawRunner.TimeBudgetMessage));
            Assert.That(compiled.Calls, Is.Zero);
            Assert.That(differential.Mismatches, Is.Empty);
        }
        finally { differential.SetDeadline(null); }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Null_deadline_stays_synchronous_including_nonparallel_compiled_backend(bool parallel)
    {
        var (interpreter, function, value) = Fixture();
        var compiled = new Compiled(() => value, parallel: parallel);
        var differential = new DifferentialBackend(new InterpreterBackend(interpreter), compiled);
        var caller = System.Environment.CurrentManagedThreadId;
        var result = differential.Invoke(function, [new TypeToken(function.Parameters[0].Type)]);
        Assert.That(result, Is.TypeOf<DifferentialValue>());
        Assert.That(compiled.InvokeThread, Is.EqualTo(caller));
        Assert.That(compiled.ConvertThread, Is.EqualTo(caller));
        Assert.That(differential.Mismatches, Is.Empty);
    }

    [Test]
    public void Very_long_deadline_accepts_a_prompt_result()
    {
        var (interpreter, function, value) = Fixture();
        var compiled = new Compiled(() => value);
        var differential = new DifferentialBackend(new InterpreterBackend(interpreter), compiled);
        differential.SetDeadline(DateTime.UtcNow.AddDays(30));
        try
        {
            Assert.That(differential.Invoke(function, [new TypeToken(function.Parameters[0].Type)]), Is.TypeOf<DifferentialValue>());
            Assert.That(compiled.Calls, Is.EqualTo(1));
        }
        finally { differential.SetDeadline(null); }
    }

    [Test]
    public void Deadline_rejects_nonparallel_compiled_backend_without_starting_it()
    {
        var (interpreter, function, value) = Fixture();
        var compiled = new Compiled(() => value, parallel: false);
        var differential = new DifferentialBackend(new InterpreterBackend(interpreter), compiled);
        differential.SetDeadline(DateTime.UtcNow.AddSeconds(5));
        try
        {
            var error = Assert.Throws<EvaluationException>(() => differential.Invoke(function, [new TypeToken(function.Parameters[0].Type)]));
            Assert.That(error!.Message, Does.Contain("cannot safely abandon"));
            Assert.That(compiled.Calls, Is.Zero);
        }
        finally { differential.SetDeadline(null); }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Genuine_compiled_mismatch_is_published_once(bool fault)
    {
        var (interpreter, function, value) = Fixture();
        var record = (PlatoRecord)value;
        var compiled = new Compiled(() => fault ? throw new EvaluationException("controlled fault") : new PlatoRecord(record.Type, [99L]));
        var differential = new DifferentialBackend(new InterpreterBackend(interpreter), compiled);
        differential.SetDeadline(DateTime.UtcNow.AddSeconds(5));
        try
        {
            Assert.Throws<BackendsDifferException>(() => differential.Invoke(function, [new TypeToken(function.Parameters[0].Type)]));
            Assert.That(differential.Mismatches, Has.Count.EqualTo(1));
            Assert.That(differential.Mismatches.Single(), Does.Contain("Witness_One"));
        }
        finally { differential.SetDeadline(null); }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Unsupported_conversion_preserves_original_success_behavior(bool deadline)
    {
        var (interpreter, function, value) = Fixture();
        var compiled = new Compiled(() => value, _ => throw new ValueConversionException("controlled unsupported value"));
        var differential = new DifferentialBackend(new InterpreterBackend(interpreter), compiled);
        differential.SetDeadline(deadline ? DateTime.UtcNow.AddSeconds(5) : null);
        try
        {
            Assert.That(differential.Invoke(function, [new TypeToken(function.Parameters[0].Type)]), Is.TypeOf<DifferentialValue>());
            Assert.That(differential.Mismatches, Is.Empty);
        }
        finally { differential.SetDeadline(null); }
    }
}
