using Ara3D.Geometry.Interpreter;

namespace Ara3D.Geometry.Laws;

/// <summary>The two ways a law run evaluates: in the interpreter, or on the C# the writer emits,
/// compiled in process.</summary>
public enum LawBackendKind { Interpreter, CSharp }

/// <summary>Names and construction of the backends, shared by <c>plato run --backend=</c> and the
/// warm server's <c>backend</c> argument.</summary>
public static class LawBackends
{
    public const string InterpreterName = "interpreter";
    public const string CSharpName = "csharp";
    public const string Names = InterpreterName + ", " + CSharpName;

    public static string NameOf(LawBackendKind kind)
        => kind == LawBackendKind.CSharp ? CSharpName : InterpreterName;

    /// <summary>Null for a name that is neither.</summary>
    public static LawBackendKind? TryParse(string name)
        => name?.Trim().ToLowerInvariant() switch
        {
            InterpreterName => LawBackendKind.Interpreter,
            CSharpName or "cs" or "c#" => LawBackendKind.CSharp,
            _ => null,
        };

    /// <summary>A backend over the interpreter's compilation and profile; the C# one emits and
    /// compiles here, which takes seconds for a large program.</summary>
    public static ILawBackend Create(LawBackendKind kind, Interpreter.Interpreter interp)
        => kind == LawBackendKind.CSharp ? CSharpBackend.Build(interp) : new InterpreterBackend(interp);
}
