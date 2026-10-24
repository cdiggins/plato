// Selects law input files and reuses compiled programs across law requests.
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
    /// <summary>How long to wait for the program when another request is running on it; null
    /// takes the runner's <see cref="WarmLawRunner.BusyWait"/>.</summary>
    public TimeSpan? BusyWait { get; init; }

    /// <summary>One line naming the request, for the status and the busy answer.</summary>
    public string Describe()
        => $"filter {(Options.Filters == null ? "(all laws)" : string.Join(",", Options.Filters))}, backend {LawBackends.NameOf(Backend)}, profile {Profile.Name}, types {TypeCheck}";
}

/// <summary>One kept program: its folders, and the request running on it, if any.</summary>
public sealed record WarmProgramStatus(string Program, string? Activity, string? Phase, double? RunningSeconds);

/// <summary>What a law run did: the results, or the errors that stopped it before any law ran,
/// and where the time went.</summary>
public sealed record LawRunOutcome
{
    /// <summary>Parse and compile errors; when any exist no law ran.</summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<LawResult> Results { get; init; } = Array.Empty<LawResult>();
    /// <summary>Differential backend: every disagreement between the interpreter and compiled C#,
    /// witnesses included (a differing witness is dropped, so no result names it). Empty otherwise.</summary>
    public IReadOnlyList<string> Mismatches { get; init; } = Array.Empty<string>();
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
    /// <summary>True when the program stayed busy with another request past the wait, so this
    /// one did not run; <see cref="Errors"/> names the request holding it.</summary>
    public bool Busy { get; init; }
    /// <summary>How long this request waited for its program before it ran.</summary>
    public TimeSpan WaitTime { get; init; }
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

    /// <summary>The longest one run may evaluate in the interpreter before every remaining law and
    /// witness faults with "run limit reached" (TKT-479); null for no limit. A request's
    /// <c>budget</c> bounds one law; this bounds the request, so no run holds its program's lock,
    /// and every other call on that program, for hours.</summary>
    public TimeSpan? RunLimit { get; init; } = DefaultRunLimit;

    // Below plato_laws.py's 30-minute client timeout, so a run ends before its client gives up.
    public static readonly TimeSpan DefaultRunLimit = TimeSpan.FromMinutes(20);

    /// <summary>How long a request waits for a program another request is running on before it
    /// answers busy (TKT-479). Without it, requests queued behind a long run without limit, each
    /// still queued after its client had given up, and nothing said who held the program.</summary>
    public TimeSpan BusyWait { get; init; } = DefaultBusyWait;

    public static readonly TimeSpan DefaultBusyWait = TimeSpan.FromSeconds(60);

    public int ProgramCount
    {
        get { lock (_lock) return _programs.Count; }
    }

    /// <summary>Receives the compiler's phase messages, to see where a compilation spends its time.</summary>
    public Ara3D.Logging.ILogger? CompileLogger { get; set; }

    public LawRunOutcome Run(LawRunRequest request)
    {
        var program = Acquire(WarmProgram.KeyOf(request));
        var waited = System.Diagnostics.Stopwatch.StartNew();
        if (!Monitor.TryEnter(program.Lock, request.BusyWait ?? BusyWait))
            return new LawRunOutcome
            {
                Busy = true,
                WaitTime = waited.Elapsed,
                ProgramsKept = ProgramCount,
                Errors = new[]
                {
                    $"busy: after {waited.Elapsed.TotalSeconds:F0} s this program is still running another request"
                    + $" ({program.Activity ?? "unknown"}; step {program.Phase ?? "unknown"}"
                    + $"{(program.ActivitySince is DateTime since ? $", for {(DateTime.UtcNow - since).TotalSeconds:F0} s" : "")}); try again, or use plato run",
                },
            };
        try
        {
            var wait = waited.Elapsed;
            program.Activity = request.Describe();
            program.ActivitySince = DateTime.UtcNow;
            program.Phase = "read";
            return OnLargeStack(() => program.Run(request, Parse, CompileLogger, RunLimit)) with { ProgramsKept = ProgramCount, WaitTime = wait };
        }
        finally
        {
            program.Activity = null;
            program.Phase = null;
            program.ActivitySince = null;
            Monitor.Exit(program.Lock);
        }
    }

    /// <summary>Every kept program, most recently used first, with the request running on it.</summary>
    public IReadOnlyList<WarmProgramStatus> Status()
    {
        List<WarmProgram> programs;
        lock (_lock)
            programs = _programs.ToList();
        return programs.Select(p => new WarmProgramStatus(
                p.Key.Replace("\r", "").Replace("\n", " "), p.Activity, p.Phase,
                p.ActivitySince is DateTime since ? (DateTime.UtcNow - since).TotalSeconds : null))
            .ToList();
    }

    /// <summary>The program for a key, made the most recently used; a new one evicts the least
    /// recently used beyond the bound. An evicted program still finishes a run in progress, and
    /// is then released, its compiled assemblies unloaded (TKT-479).</summary>
    private WarmProgram Acquire(string key)
    {
        var evicted = new List<WarmProgram>();
        try
        {
            return AcquireLocked(key, evicted);
        }
        finally
        {
            // Off this thread: an evicted program may still be running under its own lock.
            foreach (var program in evicted)
                Task.Run(() => { lock (program.Lock) program.Release(); });
        }
    }

    private WarmProgram AcquireLocked(string key, List<WarmProgram> evicted)
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
            {
                evicted.Add(_programs.Last!.Value);
                _programs.RemoveLast();
            }
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

    /// <summary>"file:line: " for the node a resolution error names, so the error says where it
    /// is; empty when the node has no source range.</summary>
    private static string Where(Ara3D.Geometry.AST.AstNode? node)
    {
        var range = node?.GetRange();
        return range == null ? "" : $"{range.FilePath}:{range.BeginLineIndex + 1}: ";
    }

    /// <summary>The errors that stop a run, in the words <c>plato run</c> prints them.</summary>
    public static IReadOnlyList<string> CompileErrors(Compilation compilation)
        => compilation.CompletedCompilation
            ? Array.Empty<string>()
            : compilation.InternalErrors.Select(e => e.ToString())
                .Concat((compilation.SymbolFactory?.Errors ?? new()).Select(e => $"RESOLVE {Where(e.Node)}{e.Message}"))
                .Append("Compilation was not completed")
                .ToList();
}
