namespace Ara3D.Geometry.Interpreter;

/// <summary>A fault while evaluating: a bad index, an invalid builder use, a division by zero, a
/// call the interpreter could not dispatch. Carries the chain of Plato functions that were
/// active, outermost last, so a report can say where a law faulted.</summary>
public class EvaluationException : Exception
{
    public List<string> PlatoStack { get; } = new List<string>();

    public EvaluationException(string message, Exception inner = null)
        : base(message, inner) { }

    public string Describe()
        => PlatoStack.Count == 0 ? Message : $"{Message} (in {string.Join(" < ", PlatoStack)})";
}

/// <summary>A bodiless function the host must supply was reached and the interpreter has no
/// implementation for it: an intrinsic missing from <see cref="HostIntrinsics"/>, or a library
/// function declared without a body (the STEP design's deferred algorithms). The law runner
/// reports the obligation by name instead of counting the law as failed.</summary>
public sealed class HostObligationException : EvaluationException
{
    public string Obligation { get; }

    public HostObligationException(string obligation)
        : base($"host obligation: {obligation}")
        => Obligation = obligation;
}
