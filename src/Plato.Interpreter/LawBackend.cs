using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Interpreter;

/// <summary>
/// Where a law's cases are evaluated. The <see cref="LawRunner"/> builds every case from the
/// compilation's metadata and the <see cref="ValueGenerator"/> (so each backend sees the same
/// witnesses in the same order) and hands the backend the function and its arguments; the
/// backend evaluates them in its own representation and reports faults as
/// <see cref="EvaluationException"/>, a bodiless function as <see cref="HostObligationException"/>.
/// </summary>
public interface ILawBackend
{
    /// <summary>The name <c>--backend=</c> takes: <c>interpreter</c> or <c>csharp</c>.</summary>
    string Name { get; }

    /// <summary>True when laws may run on several threads at once (the backend has no shared
    /// mutable state per call).</summary>
    bool SupportsParallel { get; }

    /// <summary>Evaluates one function on backend values. A <see cref="TypeToken"/> argument
    /// stands for a <c>_</c> receiver.</summary>
    object Invoke(FunctionDef f, object[] args);

    /// <summary>A value built by the <see cref="ValueGenerator"/> or taken from a finite domain,
    /// as this backend's value.</summary>
    object FromInterpreterValue(object v);

    /// <summary>A backend value as an interpreter value, for reports; the value itself when it
    /// has no interpreter form.</summary>
    object ToInterpreterValue(object v);

    /// <summary>Makes the next evaluations fault once this instant passes; null lifts the limit.
    /// A backend that cannot interrupt a call ignores it.</summary>
    void SetDeadline(DateTime? deadline);
}

/// <summary>The interpreter itself as a backend: values pass through unchanged.</summary>
public sealed class InterpreterBackend : ILawBackend
{
    public Interpreter Interpreter { get; }

    public InterpreterBackend(Interpreter interpreter) => Interpreter = interpreter;

    public string Name => "interpreter";

    /// <summary>The interpreter's elaboration cache, deadline and memo are shared by every call.</summary>
    public bool SupportsParallel => false;

    public object Invoke(FunctionDef f, object[] args) => Interpreter.Invoke(f, args);

    public object FromInterpreterValue(object v) => v;

    public object ToInterpreterValue(object v) => v;

    public void SetDeadline(DateTime? deadline) => Interpreter.Deadline = deadline;
}
