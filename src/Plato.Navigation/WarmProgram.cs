using System.Diagnostics;
using System.Text;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;
using Ara3D.Utils;

namespace Ara3D.Geometry.Navigation;

/// <summary>
/// One program a <see cref="WarmLawRunner"/> keeps: the files of one (folders, recursive,
/// exclude) choice, their compilation, an interpreter and (once asked for) a compiled C# program
/// per Number profile, and what the type check knows about each file. Not thread-safe: the runner holds <see cref="Lock"/> around a run.
/// </summary>
internal sealed class WarmProgram
{
    public WarmProgram(string key) => Key = key;

    /// <summary>The folders (full paths, in order), the recursive flag and the excluded names; a
    /// compilation does not depend on the Number profile, so profiles share it.</summary>
    public string Key { get; }
    public object Lock { get; } = new();
    /// <summary>The files read by the last run, kept alive in the shared parse cache.</summary>
    public IReadOnlyList<SourceFile> Files { get; private set; } = Array.Empty<SourceFile>();

    private string? _generation;
    private Compilation? _compilation;
    private Dictionary<string, string> _fileHashes = new();
    private readonly HashSet<string> _uncheckedFiles = new();
    private readonly Dictionary<string, IReadOnlyList<TypeFinding>> _typeErrorsByFile = new();
    private string? _allCheckedGeneration;
    private readonly Dictionary<string, Interpreter.Interpreter> _interpreters = new();
    /// <summary>The C# program per profile name, emitted and compiled from the current generation.</summary>
    private readonly Dictionary<string, CSharpProgram> _compiled = new();

    public static string KeyOf(LawRunRequest request)
        => string.Join("\n", request.Folders.Select(f => SourceSnapshot.PathKey(new FilePath(Path.GetFullPath(f)))))
           + $"\nrecursive={request.Recursive}\nexclude="
           + string.Join(",", request.Exclude.Select(x => x.ToLowerInvariant()).Distinct().OrderBy(x => x, StringComparer.Ordinal));

    /// <param name="parse">Parses files through the runner's shared cache; returns the trees and
    /// how many files missed the cache.</param>
    public LawRunOutcome Run(LawRunRequest request, Func<IReadOnlyList<SourceFile>, (IReadOnlyList<ParsedFile> Trees, int Parsed)> parse,
        Ara3D.Logging.ILogger? compileLogger)
    {
        var timer = Stopwatch.StartNew();
        var paths = LawProgram.SourceFiles(request.Folders, request.Recursive, request.Exclude);
        var files = paths.Select((p, i) => new SourceFile(i, new FilePath(p), File.ReadAllText(p))).ToList();
        Files = files;
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
            _compiled.Clear();

            var parseTimer = Stopwatch.StartNew();
            (var trees, parsed) = parse(files);
            parseTime = parseTimer.Elapsed;
            var parseErrors = ParseErrors(trees);
            if (parseErrors.Count > 0)
                return new LawRunOutcome { Errors = parseErrors, Files = files.Count, FilesParsed = parsed, Recompiled = true, ReadTime = read, ParseTime = parseTime };

            var compileTimer = Stopwatch.StartNew();
            var compilation = CheckSupport.Compile(trees, compileLogger);
            compileTime = compileTimer.Elapsed;
            var compileErrors = WarmLawRunner.CompileErrors(compilation);
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
        ILawBackend backend;
        var backendReused = false;
        var emitTime = TimeSpan.Zero;
        var csharpTime = TimeSpan.Zero;
        if (request.Backend == LawBackendKind.CSharp)
        {
            if (_compiled.TryGetValue(request.Profile.Name, out var program))
                backendReused = true;
            else
            {
                try { program = CSharpProgram.Build(_compilation!, request.Profile); }
                catch (InvalidOperationException e)
                {
                    return new LawRunOutcome { Errors = e.Message.Split('\n'), Files = files.Count, Functions = _compilation!.FunctionDefinitions.Count, FilesParsed = parsed, Recompiled = recompiled, ReadTime = read, ParseTime = parseTime, CompileTime = compileTime, Backend = LawBackends.CSharpName };
                }
                _compiled[request.Profile.Name] = program;
                emitTime = program.EmitElapsed;
                csharpTime = program.CompileElapsed;
            }
            backend = CSharpBackend.Over(program, interp);
        }
        else
            backend = new InterpreterBackend(interp);
        var runner = new LawRunner(backend, interp, request.Options);
        var lawTimer = Stopwatch.StartNew();
        var results = runner.RunAll();
        return new LawRunOutcome
        {
            Results = results,
            Backend = backend.Name,
            BackendReused = backendReused,
            EmitTime = emitTime,
            CSharpCompileTime = csharpTime,
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

    /// <summary>Every error of every file that did not parse, each naming its file, in the words
    /// <c>plato run</c> starts with ("Parsing failed").</summary>
    public static IReadOnlyList<string> ParseErrors(IEnumerable<ParsedFile> trees)
        => trees.Where(t => !t.Parsed)
            .SelectMany(t => (t.ParseErrors.Count > 0 ? t.ParseErrors : new[] { "no syntax tree" })
                .Select(e => $"Parsing failed: {t.File.Path}: {e}"))
            .ToList();

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

    /// <summary>One hash over the ordered paths and contents; equal hashes mean the same program.</summary>
    private static string Generation(IReadOnlyList<SourceFile> files)
    {
        var sb = new StringBuilder();
        foreach (var f in files)
            sb.Append(SourceSnapshot.PathKey(f.Path)).Append('\0').Append(f.ContentHash).Append('\n');
        return SourceFile.Hash(sb.ToString());
    }
}
