using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;
using NUnit.Framework;

namespace Plato.Interpreter.Tests;

[TestFixture]
[NonParallelizable]
public class DifferentialFaultTests
{
    private const string Source = """
        library DifferentialFaultFixture
        {
            Missing(x: Number): Number;
            CallsMissing(x: Number): Number => Missing(x);
            BadIndex(i: Integer): Integer => [10, 20, 30][i];
        }
        """;

    private sealed class Compiled(Func<object> invoke, Func<object, object> convert = null) : ILawBackend
    {
        public string Name => "controlled-compiled-fault";
        public bool SupportsParallel => true;
        public void SetDeadline(DateTime? deadline) { }
        public object FromInterpreterValue(object value) => value;
        public object ToInterpreterValue(object value) => convert == null ? value : convert(value);
        public object Invoke(FunctionDef function, object[] args) => invoke();
    }

    private static (Ara3D.Geometry.Interpreter.Interpreter Interpreter, FunctionDef Function, object[] Args, EvaluationException Fault) Fixture(bool obligation)
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var interpreter = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var function = interpreter.FunctionsNamed(obligation ? "CallsMissing" : "BadIndex").Single(f => f.Body != null);
        object[] args = obligation ? [1.0] : [99L];
        var fault = Assert.Catch<EvaluationException>(() => interpreter.Invoke(function, args));
        Assert.That(fault, obligation ? Is.TypeOf<HostObligationException>() : Is.TypeOf<EvaluationException>());
        return (interpreter, function, args, fault!);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Matching_fault_categories_rethrow_the_interpreter_fault(bool obligation)
    {
        var (interpreter, function, args, expected) = Fixture(obligation);
        EvaluationException compiledFault = obligation ? new HostObligationException("different compiled description") : new EvaluationException("different compiled description");
        var backend = new DifferentialBackend(new InterpreterBackend(interpreter), new Compiled(() => throw compiledFault));
        backend.SetDeadline(DateTime.UtcNow.AddSeconds(5));
        try
        {
            var actual = Assert.Catch<EvaluationException>(() => backend.Invoke(function, args));
            Assert.That(actual!.GetType(), Is.EqualTo(expected.GetType()));
            Assert.That(actual.Describe(), Is.EqualTo(expected.Describe()));
            Assert.That(backend.Mismatches, Is.Empty);
        }
        finally { backend.SetDeadline(null); }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Different_fault_categories_remain_a_mismatch(bool interpreterObligation)
    {
        var (interpreter, function, args, _) = Fixture(interpreterObligation);
        EvaluationException compiledFault = interpreterObligation ? new EvaluationException("compiled ordinary fault") : new HostObligationException("compiled obligation");
        var backend = new DifferentialBackend(new InterpreterBackend(interpreter), new Compiled(() => throw compiledFault));
        backend.SetDeadline(DateTime.UtcNow.AddSeconds(5));
        try
        {
            Assert.Throws<BackendsDifferException>(() => backend.Invoke(function, args));
            Assert.That(backend.Mismatches, Has.Count.EqualTo(1));
            Assert.That(backend.Mismatches.Single(), Does.Contain(compiledFault.Message));
        }
        finally { backend.SetDeadline(null); }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Interpreter_fault_plus_unsupported_conversion_propagates_conversion_fault(bool obligation)
    {
        var (interpreter, function, args, _) = Fixture(obligation);
        var conversion = new ValueConversionException("controlled unsupported conversion after interpreter fault");
        var backend = new DifferentialBackend(new InterpreterBackend(interpreter), new Compiled(() => 7L, _ => throw conversion));
        backend.SetDeadline(DateTime.UtcNow.AddSeconds(5));
        try
        {
            Assert.That(Assert.Throws<ValueConversionException>(() => backend.Invoke(function, args)), Is.SameAs(conversion));
            Assert.That(backend.Mismatches, Is.Empty);
        }
        finally { backend.SetDeadline(null); }
    }
}
