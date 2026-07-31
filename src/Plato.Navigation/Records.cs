using Ara3D.Parakeet;

namespace Ara3D.Geometry.Navigation;

/// <summary>A half-open character range within one file, carried with line/column so a consumer
/// never needs the source text to render a location. <see cref="Begin"/> == <see cref="End"/> == -1
/// means "no span" (a compiler-synthesized definition with no syntax of its own).</summary>
public readonly record struct Span(int Begin, int End, int BeginLine, int BeginColumn, int EndLine, int EndColumn)
{
    public static readonly Span None = new(-1, -1, -1, -1, -1, -1);

    public bool HasValue => Begin >= 0;
    public int Length => End - Begin;
    public bool Contains(int offset) => offset >= Begin && offset < End;

    public static Span From(ILocation? location)
    {
        var r = location?.GetRange();
        return r == null
            ? None
            : new Span(r.BeginPosition, r.EndPosition, r.BeginLineIndex, r.BeginColumn, r.EndLineIndex, r.EndColumn);
    }
}

public enum DefKind
{
    Type,
    Interface,
    Library,
    Primitive,
    TypeParameter,
    Method,
    Field,
    Parameter,
    Variable,
    SumCase,
    SumCaseField,
    Function
}

/// <summary>One definition. <see cref="NameSpan"/> is the identifier alone (what an editor
/// highlights); <see cref="DeclSpan"/> is the whole declaration (what a doc dump would print).
/// <see cref="Owner"/> is the enclosing definition's id, or -1 at file level.</summary>
public sealed class DefRecord
{
    public int Id { get; }
    public DefKind Kind { get; }
    public string Name { get; }
    public string? Signature { get; }
    public int FileId { get; }
    public Span NameSpan { get; }
    public Span DeclSpan { get; }
    public int Owner { get; }
    public bool Synthetic { get; }

    public DefRecord(int id, DefKind kind, string name, string? signature, int fileId,
        Span nameSpan, Span declSpan, int owner, bool synthetic = false)
    {
        Id = id;
        Kind = kind;
        Name = name;
        Signature = signature;
        FileId = fileId;
        NameSpan = nameSpan;
        DeclSpan = declSpan;
        Owner = owner;
        Synthetic = synthetic;
    }

    public override string ToString() => $"{Kind} {Name} #{Id}";
}

public enum RefKind
{
    /// <summary>A name used in an expression body.</summary>
    Value,
    /// <summary>A type named in a signature, inherits/implements clause, or type argument.</summary>
    Type,
    /// <summary>An operator token standing in for a call: <c>a + b</c> is a reference to Add. The
    /// span covers the operator itself, not a name, so the source text there is "+", not "Add".</summary>
    Operator
}

/// <summary>One reference occurrence. <see cref="Targets"/> holds every definition the name could
/// mean: exactly one for a type, parameter, or field, and every overload for a call into a
/// function group (D4).</summary>
public sealed class RefRecord
{
    public int Id { get; }
    public RefKind Kind { get; }
    public string Name { get; }
    public int FileId { get; }
    public Span Span { get; }
    public IReadOnlyList<int> Targets { get; }

    public RefRecord(int id, RefKind kind, string name, int fileId, Span span, IReadOnlyList<int> targets)
    {
        Id = id;
        Kind = kind;
        Name = name;
        FileId = fileId;
        Span = span;
        Targets = targets;
    }

    public override string ToString() => $"{Kind} ref {Name} #{Id} -> [{string.Join(",", Targets)}]";
}

/// <summary>What <see cref="NavigationIndex.FindAt"/> found at a position: a definition site, a
/// reference site, or nothing.</summary>
public sealed class SymbolHit
{
    public DefRecord? Def { get; }
    public RefRecord? Ref { get; }

    public SymbolHit(DefRecord? def, RefRecord? reference) => (Def, Ref) = (def, reference);

    public string Name => Def?.Name ?? Ref?.Name ?? "";
    public Span Span => Def?.NameSpan ?? Ref?.Span ?? Span.None;
}

public enum FileState
{
    ParseFailed,
    Parsed,
    Bound
}

public sealed class FileStatus
{
    public int FileId { get; }
    public string Path { get; }
    public string ContentHash { get; }
    public FileState State { get; }

    public FileStatus(int fileId, string path, string contentHash, FileState state)
        => (FileId, Path, ContentHash, State) = (fileId, path, contentHash, state);
}

public enum DiagnosticKind
{
    Parse,
    Resolution,
    BindAbort
}

public sealed class Diagnostic
{
    public DiagnosticKind Kind { get; }
    public string Message { get; }
    public int FileId { get; }
    public Span Span { get; }

    public Diagnostic(DiagnosticKind kind, string message, int fileId, Span span)
        => (Kind, Message, FileId, Span) = (kind, message, fileId, span);

    public override string ToString() => $"{Kind}: {Message}";
}
