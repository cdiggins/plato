// Names and constructs the execution backends shared by command and warm law runners.
using Ara3D.Geometry.Interpreter;

namespace Ara3D.Geometry.Laws;

/// <summary>The ways a law run evaluates: in the interpreter, on the C# the writer emits compiled
/// in process, or on both at once with every answer compared (<see cref="DifferentialBackend"/>).</summary>
public enum LawBackendKind { Interpreter, CSharp, Differential }

/// <summary>Names and construction of the backends, shared by <c>plato run --backend=</c> and the
/// warm server's <c>backend</c> argument.</summary>
public static class LawBackends
{
    public const string InterpreterName = "interpreter";
    public const string CSharpName = "csharp";
    public const string DifferentialName = "differential";
    public const string Names = InterpreterName + ", " + CSharpName + ", " + DifferentialName;

    public static string NameOf(LawBackendKind kind)
        => kind switch
        {
            LawBackendKind.CSharp => CSharpName,
            LawBackendKind.Differential => DifferentialName,
            _ => InterpreterName,
        };

    /// <summary>Null for a name that is neither.</summary>
    public static LawBackendKind? TryParse(string name)
        => name?.Trim().ToLowerInvariant() switch
        {
            InterpreterName => LawBackendKind.Interpreter,
            CSharpName or "cs" or "c#" => LawBackendKind.CSharp,
            DifferentialName => LawBackendKind.Differential,
            _ => null,
        };

    /// <summary>A backend over the interpreter's compilation and profile; the C# one emits and
    /// compiles here, which takes seconds for a large program. <paramref name="lowerLoops"/> is
    /// the writer's <c>--loops</c> for the compiled arm; <paramref name="lowerScalars"/> is its
    /// <c>--lower-scalars</c> (TKT-1129).</summary>
    public static ILawBackend Create(LawBackendKind kind, Interpreter.Interpreter interp, bool lowerLoops = false, bool fuse = false, bool lowerScalars = false)
        => kind switch
        {
            LawBackendKind.CSharp => CSharpBackend.Build(interp, lowerLoops, fuse, lowerScalars),
            LawBackendKind.Differential => new DifferentialBackend(new InterpreterBackend(interp), CSharpBackend.Build(interp, lowerLoops, fuse, lowerScalars)),
            _ => new InterpreterBackend(interp),
        };
}
