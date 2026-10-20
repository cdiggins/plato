using System.Runtime.ExceptionServices;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Ara3D.Geometry.Navigation;

/// <summary>The files a law run compiles, chosen the way <c>plato run</c> chooses them: every
/// <c>*.plato</c> of each folder in the order given (top level only, or every subfolder when
/// recursive), <c>*.spec.plato</c> always skipped, an excluded file name skipped wherever it is,
/// and a file reached twice kept once. The order is the compilation's declaration order, so the
/// CLI and the warm runner use this one list and reach the same verdicts.</summary>
public static class LawProgram
{
    public static IReadOnlyList<string> SourceFiles(IEnumerable<string> folders, bool recursive, IEnumerable<string> exclude)
    {
        var excluded = exclude.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var files = new List<string>();
        foreach (var root in folders)
        {
            if (!Directory.Exists(root))
                throw new DirectoryNotFoundException($"Input folder not found: {root}");
            foreach (var f in Directory.GetFiles(root, "*.plato", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                if (!f.EndsWith(".spec.plato", StringComparison.OrdinalIgnoreCase)
                    && !excluded.Contains(Path.GetFileName(f))
                    && seen.Add(Path.GetFullPath(f)))
                    files.Add(f);
        }
        return files;
    }
}

/// <summary>Which functions a run type-checks.</summary>
public enum TypeCheckScope
{
    /// <summary>None.</summary>
    None,
    /// <summary>The functions declared in files whose text changed since the last check (every
    /// file, the first time); the other files keep the errors found when they were last checked.
    /// An edit that breaks a caller in an unchanged file is caught only by <see cref="All"/>.</summary>
    ChangedFiles,
    /// <summary>Every function, once per compilation.</summary>
    All,
}

/// <summary>What a law run is asked to do; the same choices as <c>plato run --laws</c>.</summary>
public sealed class LawRunRequest
{
    public IReadOnlyList<string> Folders { get; init; } = Array.Empty<string>();
    public bool Recursive { get; init; }
    public IReadOnlyList<string> Exclude { get; init; } = Array.Empty<string>();
    public NumberProfile Profile { get; init; } = NumberProfile.Default;
    /// <summary>Where the laws evaluate: the interpreter, or the program compiled to C# and kept
    /// warm per source generation and profile.</summary>
    public LawBackendKind Backend { get; init; } = LawBackendKind.Interpreter;
    /// <summary>Filters, trials, seed and budget.</summary>
    public LawRunOptions Options { get; init; } = new();
    /// <summary>Which functions to type-check before the laws run (<see cref="TypeFindings"/>).</summary>
    public TypeCheckScope TypeCheck { get; init; }
}

/// <summary>What a law run did: the results, or the errors that stopped it before any law ran,
/// and where the time went.</summary>
public sealed record LawRunOutcome
{
    /// <summary>Parse and compile errors; when any exist no law ran.</summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<LawResult> Results { get; init; } = Array.Empty<LawResult>();
    public IReadOnlyList<string> UnmatchedFilters { get; init; } = Array.Empty<string>();
    /// <summary>The known type errors, sorted by file and line, when the request asked for a
    /// type check; null otherwise. See <see cref="TypeCheckScope"/> for how current each file's are.</summary>
    public IReadOnlyList<TypeFinding>? TypeErrors { get; init; }
    /// <summary>Files whose functions were type-checked by this run; the rest were reused.</summary>
    public int FilesTypeChecked { get; init; }
    public int Files { get; init; }
    public int Functions { get; init; }
    /// <summary>Files parsed this run; the rest came from the parse cache.</summary>
    public int FilesParsed { get; init; }
    /// <summary>False when no file changed since the last run, so the compilation and the
    /// interpreter, with the bodies it already elaborated, were reused.</summary>
    public bool Recompiled { get; init; }
    public TimeSpan ReadTime { get; init; }
    public TimeSpan ParseTime { get; init; }
    public TimeSpan CompileTime { get; init; }
    /// <summary>Zero when no file needed checking.</summary>
    public TimeSpan TypeCheckTime { get; init; }
    public TimeSpan LawTime { get; init; }
    /// <summary>The backend the laws ran on.</summary>
    public string Backend { get; init; } = LawBackends.InterpreterName;
    /// <summary>C# backend: true when the compiled program of the last run served again; false
    /// when this run emitted and compiled (a source changed, or the first run for the profile).</summary>
    public bool BackendReused { get; init; }
    /// <summary>C# backend, when not reused: time writing the C#.</summary>
    public TimeSpan EmitTime { get; init; }
    /// <summary>C# backend, when not reused: time Roslyn took to compile it.</summary>
    public TimeSpan CSharpCompileTime { get; init; }
    /// <summary>Programs the runner keeps after this run, this one included.</summary>
    public int ProgramsKept { get; init; }
    public bool Compiled => Errors.Count == 0;
}

/// <summary>
/// Runs laws over compilations it keeps between runs, for a long-lived host such as the
/// navigation MCP server. It keeps one program (<see cref="WarmProgram"/>) for each choice of
/// folders, recursive flag and excluded names it is asked for, up to <see cref="MaxPrograms"/>,
/// dropping the least recently used, so callers working on different folder sets (one staging
/// folder per track) do not force each other to recompile. A program's compilation is shared by
/// every Number profile; each profile gets its own interpreter.
///
/// Each run reads its program's files again and compares their contents: a file whose text is
/// unchanged keeps its parsed tree (one <see cref="ParseCache"/> shared by all programs, so the
/// library files common to every track are parsed once, and a run never holds the index's lock),
/// and when no file changed at all the compilation and the interpreter are reused, elaborated
/// bodies included, as is the compiled C# program of each profile the csharp backend has run under. A changed file costs one parse plus one whole-program compilation, because
/// the compiler has no incremental mode, and, when asked, a type check of the functions it
/// declares (<see cref="TypeCheckScope"/>). Runs of one program are serialized; runs of different
/// programs proceed at the same time. Each evaluates on a thread with the large stack deep Plato
/// recursion needs.
/// </summary>
public sealed class WarmLawRunner
{
    /// <summary>The stack <c>plato run</c> gives its evaluation thread.</summary>
    public const int StackBytes = 512 * 1024 * 1024;

    /// <summary>Programs kept by default; each costs several hundred megabytes (docs/interpreter.md).</summary>
    public const int DefaultMaxPrograms = 8;

    private readonly ParseCache _cache = new();
    /// <summary>Guards <see cref="_cache"/> and <see cref="_programs"/>.</summary>
    private readonly object _lock = new();
    /// <summary>Most recently used first.</summary>
    private readonly LinkedList<WarmProgram> _programs = new();

    public WarmLawRunner(int maxPrograms = DefaultMaxPrograms)
    {
        if (maxPrograms < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPrograms), "at least one program must be kept");
        MaxPrograms = maxPrograms;
    }

    public int MaxPrograms { get; }

    public int ProgramCount
    {
        get { lock (_lock) return _programs.Count; }
    }

    /// <summary>Receives the compiler's phase messages, to see where a compilation spends its time.</summary>
    public Ara3D.Logging.ILogger? CompileLogger { get; set; }

    public LawRunOutcome Run(LawRunRequest request)
    {
        var program = Acquire(WarmProgram.KeyOf(request));
        lock (program.Lock)
            return OnLargeStack(() => program.Run(request, Parse, CompileLogger)) with { ProgramsKept = ProgramCount };
    }

    /// <summary>The program for a key, made the most recently used; a new one evicts the least
    /// recently used beyond the bound. An evicted program still finishes a run in progress.</summary>
    private WarmProgram Acquire(string key)
    {
        lock (_lock)
        {
            var node = _programs.First;
            while (node != null && node.Value.Key != key)
                node = node.Next;
            if (node != null)
                _programs.Remove(node);
            _programs.AddFirst(node?.Value ?? new WarmProgram(key));
            while (_programs.Count > MaxPrograms)
                _programs.RemoveLast();
            return _programs.First!.Value;
        }
    }

    /// <summary>Parses through the shared cache and drops cached trees no kept program uses.</summary>
    private (IReadOnlyList<ParsedFile> Trees, int Parsed) Parse(IReadOnlyList<SourceFile> files)
    {
        lock (_lock)
        {
            var misses = _cache.Misses;
            var trees = files.Select(_cache.Parse).ToList();
            _cache.Retain(_programs.SelectMany(p => p.Files).Concat(files));
            return (trees, _cache.Misses - misses);
        }
    }

    private static T OnLargeStack<T>(Func<T> work)
    {
        T result = default!;
        Exception? fault = null;
        var thread = new Thread(() =>
        {
            try { result = work(); }
            catch (Exception e) { fault = e; }
        }, StackBytes);
        thread.Start();
        thread.Join();
        if (fault != null)
            ExceptionDispatchInfo.Capture(fault).Throw();
        return result;
    }

    /// <summary>The errors that stop a run, in the words <c>plato run</c> prints them.</summary>
    public static IReadOnlyList<string> CompileErrors(Compilation compilation)
        => compilation.CompletedCompilation
            ? Array.Empty<string>()
            : compilation.InternalErrors.Select(e => e.ToString())
                .Concat((compilation.SymbolFactory?.Errors ?? new()).Select(e => $"RESOLVE {e.Message}"))
                .Append("Compilation was not completed")
                .ToList();
}
