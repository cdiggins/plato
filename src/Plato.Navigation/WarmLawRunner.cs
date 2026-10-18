using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Utils;

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
    /// <summary>Filters, trials, seed and budget.</summary>
    public LawRunOptions Options { get; init; } = new();
    /// <summary>Which functions to type-check before the laws run (<see cref="TypeFindings"/>).</summary>
    public TypeCheckScope TypeCheck { get; init; }
}

/// <summary>What a law run did: the results, or the errors that stopped it before any law ran,
/// and where the time went.</summary>
public sealed class LawRunOutcome
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
    public bool Compiled => Errors.Count == 0;
}

/// <summary>
/// Runs laws over a compilation it keeps between runs, for a long-lived host such as the
/// navigation MCP server. Each run reads the files again and compares their contents: a file
/// whose text is unchanged keeps its parsed tree (a <see cref="ParseCache"/> of its own, so a
/// run never holds the index's lock), and when no file changed at all the compilation and the
/// interpreter are reused, elaborated bodies included. A changed file costs one parse plus one
/// whole-program compilation, because the compiler has no incremental mode, and, when asked, a
/// type check of the functions it declares (<see cref="TypeCheckScope"/>). Runs are serialized;
/// each evaluates on a thread with the large stack deep Plato recursion needs.
/// </summary>
public sealed class WarmLawRunner
{
    /// <summary>The stack <c>plato run</c> gives its evaluation thread.</summary>
    public const int StackBytes = 512 * 1024 * 1024;

    private readonly ParseCache _cache = new();
    private readonly object _lock = new();
    private string? _generation;
    private Compilation? _compilation;
    private Dictionary<string, string> _fileHashes = new();
    private readonly HashSet<string> _uncheckedFiles = new();
    private readonly Dictionary<string, IReadOnlyList<TypeFinding>> _typeErrorsByFile = new();
    private string? _allCheckedGeneration;
    private readonly Dictionary<string, Interpreter.Interpreter> _interpreters = new();

    /// <summary>Receives the compiler's phase messages, to see where a compilation spends its time.</summary>
    public Ara3D.Logging.ILogger? CompileLogger { get; set; }

    public LawRunOutcome Run(LawRunRequest request)
    {
        lock (_lock)
        {
            LawRunOutcome? outcome = null;
            Exception? fault = null;
            var thread = new Thread(() =>
            {
                try { outcome = RunOnThisThread(request); }
                catch (Exception e) { fault = e; }
            }, StackBytes);
            thread.Start();
            thread.Join();
            if (fault != null)
                ExceptionDispatchInfo.Capture(fault).Throw();
            return outcome!;
        }
    }

    private LawRunOutcome RunOnThisThread(LawRunRequest request)
    {
        var timer = Stopwatch.StartNew();
        var paths = LawProgram.SourceFiles(request.Folders, request.Recursive, request.Exclude);
        var files = paths.Select((p, i) => new SourceFile(i, new FilePath(p), File.ReadAllText(p))).ToList();
        var read = timer.Elapsed;

        var generation = Generation(files);
        var recompiled = generation != _generation || _compilation == null;
        var parsed = 0;
        var parseTime = TimeSpan.Zero;
        var compileTime = TimeSpan.Zero;
        if (recompiled)
        {
            _generation = null;
            _compilation = null;
            _interpreters.Clear();

            var misses = _cache.Misses;
            var parseTimer = Stopwatch.StartNew();
            var trees = files.Select(_cache.Parse).ToList();
            _cache.Retain(new SourceSnapshot(files));
            parsed = _cache.Misses - misses;
            parseTime = parseTimer.Elapsed;
            var parseErrors = trees.Where(t => !t.Parsed)
                .Select(t => $"Parsing failed: {t.File.Path}: {t.ParseErrors.FirstOrDefault()}").ToList();
            if (parseErrors.Count > 0)
                return new LawRunOutcome { Errors = parseErrors, Files = files.Count, FilesParsed = parsed, Recompiled = true, ReadTime = read, ParseTime = parseTime };

            var compileTimer = Stopwatch.StartNew();
            var compilation = CheckSupport.Compile(trees, CompileLogger);
            compileTime = compileTimer.Elapsed;
            var compileErrors = CompileErrors(compilation);
            if (compileErrors.Count > 0)
                return new LawRunOutcome { Errors = compileErrors, Files = files.Count, FilesParsed = parsed, Recompiled = true, ReadTime = read, ParseTime = parseTime, CompileTime = compileTime };
            _compilation = compilation;
            _generation = generation;
            NoteChangedFiles(files);
        }

        var typeTimer = Stopwatch.StartNew();
        var typeChecked = CheckTypes(request.TypeCheck);
        var typeTime = typeChecked > 0 ? typeTimer.Elapsed : TimeSpan.Zero;

        if (!_interpreters.TryGetValue(request.Profile.Name, out var interp))
            _interpreters[request.Profile.Name] = interp = new Interpreter.Interpreter(_compilation!, request.Profile);
        var runner = new LawRunner(interp, request.Options);
        var lawTimer = Stopwatch.StartNew();
        var results = runner.Laws.Select(runner.Run).ToList();
        return new LawRunOutcome
        {
            Results = results,
            UnmatchedFilters = runner.UnmatchedFilters,
            TypeErrors = request.TypeCheck == TypeCheckScope.None
                ? null
                : _typeErrorsByFile.OrderBy(kv => kv.Key, StringComparer.Ordinal).SelectMany(kv => kv.Value.OrderBy(t => t.Line)).ToList(),
            FilesTypeChecked = typeChecked,
            TypeCheckTime = typeTime,
            Files = files.Count,
            Functions = _compilation!.FunctionDefinitions.Count,
            FilesParsed = parsed,
            Recompiled = recompiled,
            ReadTime = read,
            ParseTime = parseTime,
            CompileTime = compileTime,
            LawTime = lawTimer.Elapsed,
        };
    }

    /// <summary>Marks every file whose text differs from the last compilation's as needing a type
    /// check, and forgets the errors of files no longer in the program.</summary>
    private void NoteChangedFiles(IReadOnlyList<SourceFile> files)
    {
        var hashes = files.ToDictionary(f => SourceSnapshot.PathKey(f.Path), f => f.ContentHash);
        foreach (var (key, hash) in hashes)
            if (_fileHashes.GetValueOrDefault(key) != hash)
                _uncheckedFiles.Add(key);
        _uncheckedFiles.IntersectWith(hashes.Keys);
        foreach (var gone in _typeErrorsByFile.Keys.Where(k => !hashes.ContainsKey(k)).ToList())
            _typeErrorsByFile.Remove(gone);
        _fileHashes = hashes;
    }

    /// <summary>Type-checks what the scope asks for and is not yet checked; returns how many files
    /// were checked.</summary>
    private int CheckTypes(TypeCheckScope scope)
    {
        var all = scope == TypeCheckScope.All;
        if (scope == TypeCheckScope.None || (all ? _allCheckedGeneration == _generation : _uncheckedFiles.Count == 0))
            return 0;
        var files = all ? _fileHashes.Keys.ToHashSet() : new HashSet<string>(_uncheckedFiles);
        var found = TypeFindings.Check(_compilation!, all ? null : f => files.Contains(SourceSnapshot.PathKey(new FilePath(f))))
            .ToLookup(t => SourceSnapshot.PathKey(new FilePath(t.File)));
        foreach (var key in files)
        {
            var list = found[key].ToList();
            if (list.Count > 0)
                _typeErrorsByFile[key] = list;
            else
                _typeErrorsByFile.Remove(key);
        }
        _uncheckedFiles.Clear();
        if (all)
            _allCheckedGeneration = _generation;
        return files.Count;
    }

    /// <summary>The errors that stop a run, in the words <c>plato run</c> prints them.</summary>
    public static IReadOnlyList<string> CompileErrors(Compilation compilation)
        => compilation.CompletedCompilation
            ? Array.Empty<string>()
            : compilation.InternalErrors.Select(e => e.ToString())
                .Concat((compilation.SymbolFactory?.Errors ?? new()).Select(e => $"RESOLVE {e.Message}"))
                .Append("Compilation was not completed")
                .ToList();

    /// <summary>One hash over the ordered paths and contents; equal hashes mean the same program.</summary>
    private static string Generation(IReadOnlyList<SourceFile> files)
    {
        var sb = new StringBuilder();
        foreach (var f in files)
            sb.Append(SourceSnapshot.PathKey(f.Path)).Append('\0').Append(f.ContentHash).Append('\n');
        return SourceFile.Hash(sb.ToString());
    }
}
