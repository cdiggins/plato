using System.Collections.Concurrent;
using System.Diagnostics;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Interpreter;

/// <summary>What happened to one law.</summary>
public enum LawStatus { Pass, Fail, Error, Obligation, Skipped }

public sealed class LawResult
{
    public FunctionDef Law { get; init; }
    public LawStatus Status { get; init; }
    /// <summary>Cases evaluated (all of them for a pass; up to and including the failing one otherwise).</summary>
    public int Trials { get; init; }
    public bool Exhaustive { get; init; }
    /// <summary>The first failing or faulting witness, one entry per parameter.</summary>
    public IReadOnlyList<object> Witness { get; init; }
    /// <summary>Why it was skipped, the fault, or the host obligation.</summary>
    public string Message { get; init; }
    /// <summary>The value of <c>Measure_&lt;law&gt;</c> on the failing witness, when such a function exists.</summary>
    public object Measure { get; init; }
    public TimeSpan Elapsed { get; init; }

    public string Name => LawSelection.FullName(Law);

    public override string ToString()
    {
        var head = $"{Status.ToString().ToLowerInvariant(),-10} {Name}";
        switch (Status)
        {
            case LawStatus.Pass:
                return $"{head}  ({Trials} {(Exhaustive ? "cases, exhaustive" : "cases")})";
            case LawStatus.Fail:
            case LawStatus.Error:
                return head
                       + (Witness != null ? $"  case {Trials}: {FormatWitness()}" : "")
                       + (Measure != null ? $"  measure {ValueFormat.Format(Measure)}" : "")
                       + (Message != null ? $"  {Message}" : "");
            default:
                return $"{head}  {Message}";
        }
    }

    public string FormatWitness()
        => Witness == null ? "" : string.Join(", ", Law.Parameters.Zip(Witness, (p, v) => $"{p.Name} = {ValueFormat.Format(v)}"));
}

public sealed class LawRunOptions
{
    /// <summary>Sampled cases per law when its domain is not finite.</summary>
    public int Trials { get; set; } = 25;
    /// <summary>A finite domain up to this size is exhausted instead of sampled.</summary>
    public int MaxExhaustive { get; set; } = 256;
    /// <summary>Integer parameters range over this closed interval when exhausting (a witness
    /// selector such as <c>WitnessAt(k)</c> wants every witness reached, plus an out-of-range value).</summary>
    public int IntegerMin { get; set; } = -1;
    public int IntegerMax { get; set; } = 12;
    public int Seed { get; set; } = 0;
    /// <summary>Seconds one law may take over all its cases before it is reported as an error,
    /// with its elapsed time; null for no limit. The interpreter stops the law at the budget; a
    /// compiled backend lets it finish and judges the time it took.</summary>
    public double? BudgetSeconds { get; set; } = DefaultBudgetSeconds;
    public const double DefaultBudgetSeconds = 2;
    /// <summary>Seconds after which a law a backend cannot interrupt is given up on: its thread
    /// is left to finish in the background and the law is reported as an error. Null waits.</summary>
    public double? AbandonSeconds { get; set; } = 120;
    /// <summary>Laws evaluated at once when the backend allows it (<see cref="ILawBackend.SupportsParallel"/>).</summary>
    public int MaxParallel { get; set; } = System.Environment.ProcessorCount;
    /// <summary>Only laws matching one of these run (see <see cref="LawSelection.Matches"/>); null
    /// runs every law, an empty list runs none.</summary>
    public IReadOnlyList<string> Filters { get; set; }
    public string LawPrefix { get; set; } = "Law_";
    public string WitnessPrefix { get; set; } = "Witness_";
    public string MeasurePrefix { get; set; } = "Measure_";
}

/// <summary>What evaluating one <c>Witness_*</c> function cost: its time, and the fault when it
/// had one (a faulting witness is dropped; the laws over its type run on the others).</summary>
public sealed record WitnessResult(string Name, TimeSpan Elapsed, string Fault);

/// <summary>Which laws a run covers. A filter matches a law when its text occurs, ignoring case, in
/// the law's full name <c>Owner.Law_Name</c>, so one entry can name a law, its class, or both
/// (<c>Law_Area</c>, <c>Rect2D</c>, <c>Rect2D.Law_Area</c>). A law runs when any filter matches it.</summary>
public static class LawSelection
{
    public static string FullName(FunctionDef law)
        => $"{law.OwnerType?.Name}.{law.Name}";

    public static bool Matches(FunctionDef law, IEnumerable<string> filters)
        => filters.Any(f => FullName(law).Contains(f, StringComparison.OrdinalIgnoreCase));

    /// <summary>The entries of a comma-separated list, trimmed, empty ones dropped.</summary>
    public static IReadOnlyList<string> SplitList(string list)
        => list.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>The entries of a law list file: one law or class name per line; blank lines and
    /// lines starting with <c>#</c> are skipped.</summary>
    public static IReadOnlyList<string> ParseListFile(string text)
        => text.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#')).ToList();

    /// <summary>The filters given as comma lists (<c>--filter</c>) and as list files
    /// (<c>--laws-from</c>), together; null when neither is given, so every law runs. An empty
    /// list file selects no law. Throws <see cref="FileNotFoundException"/> for a missing file.</summary>
    public static IReadOnlyList<string> FromArguments(IEnumerable<string> commaLists, IEnumerable<string> listFiles)
    {
        var lists = commaLists.SelectMany(SplitList).ToList();
        var files = listFiles.ToList();
        if (lists.Count == 0 && files.Count == 0)
            return null;
        foreach (var file in files)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"law list file not found: {file}", file);
            lists.AddRange(ParseListFile(File.ReadAllText(file)));
        }
        return lists;
    }

    /// <summary>The filters no law matches, so a misspelt name in a list is reported, not
    /// silently ignored.</summary>
    public static IReadOnlyList<string> Unmatched(IEnumerable<FunctionDef> laws, IEnumerable<string> filters)
    {
        var names = laws.Select(FullName).ToList();
        return filters.Where(f => !names.Any(n => n.Contains(f, StringComparison.OrdinalIgnoreCase))).ToList();
    }
}

/// <summary>
/// Evaluates every <c>Law_*</c> Boolean function of a compilation over the values the same
/// compilation can supply: its <c>Witness_*</c> functions (<c>Witness_X(_: T): T</c>, the
/// sandbox's convention; a nullary <c>Witness_X(): T</c> is accepted too), the finite domains of
/// Booleans, enumerations and type tokens, and seeded pseudorandom values (<see cref="ValueGenerator"/>).
/// A law whose parameters all have finite domains small enough is exhausted; otherwise it is
/// sampled, each draw taking a witness or a generated value. A parameter typed by an interface
/// ranges over every concrete type implementing it, as the C# runner's monomorphization does.
/// A law no value can be built for is skipped with the reason; a bodiless function reached
/// while evaluating is reported as a host obligation, not as a failure.
/// </summary>
public sealed class LawRunner
{
    /// <summary>Supplies the compilation's metadata and generated values; evaluates nothing when
    /// another backend is in use.</summary>
    public Interpreter Interpreter { get; }
    public ILawBackend Backend { get; }
    public LawRunOptions Options { get; }
    private readonly ValueGenerator _gen;
    /// <summary>One entry per type, built on first use; laws of a parallel run that need the same
    /// type wait for the one building it, and different types build at once.</summary>
    private readonly ConcurrentDictionary<string, Lazy<List<object>>> _witnessCache = new ConcurrentDictionary<string, Lazy<List<object>>>();
    private readonly ConcurrentQueue<WitnessResult> _witnessResults = new ConcurrentQueue<WitnessResult>();

    /// <summary>Every witness evaluated so far, with its time: the shared values a sweep built
    /// once, so a slow witness is named beside the slow laws.</summary>
    public IReadOnlyList<WitnessResult> Witnesses => _witnessResults.ToList();

    /// <summary>The stack a law's evaluation thread gets: deep Plato recursion needs more than the
    /// default thread has.</summary>
    public const int StackBytes = 256 * 1024 * 1024;

    public LawRunner(Interpreter interpreter, LawRunOptions options = null)
        : this(new InterpreterBackend(interpreter), interpreter, options) { }

    public LawRunner(ILawBackend backend, Interpreter interpreter, LawRunOptions options = null)
    {
        Interpreter = interpreter;
        Backend = backend;
        Options = options ?? new LawRunOptions();
        _gen = new ValueGenerator(interpreter);
    }

    /// <summary>Every law of the compilation, the filters ignored.</summary>
    public IEnumerable<FunctionDef> AllLaws
        => Interpreter.Functions.Where(f => f.Name.StartsWith(Options.LawPrefix, StringComparison.Ordinal)
                                            && f.Body != null
                                            && f.ReturnType?.Def?.Name == "Boolean");

    /// <summary>The laws the filters select.</summary>
    public IEnumerable<FunctionDef> Laws
        => AllLaws.Where(f => Options.Filters == null || LawSelection.Matches(f, Options.Filters));

    /// <summary>The filters that select no law.</summary>
    public IReadOnlyList<string> UnmatchedFilters
        => Options.Filters == null ? Array.Empty<string>() : LawSelection.Unmatched(AllLaws, Options.Filters);

    public IReadOnlyList<LawResult> RunAll() => RunAll(null);

    /// <summary>Runs the selected laws, reporting each result as it is known (from any thread
    /// when the run is parallel), and returns them in law order. Laws run at once, up to
    /// <see cref="LawRunOptions.MaxParallel"/>, when the backend allows it; otherwise one after
    /// another on the calling thread.</summary>
    public IReadOnlyList<LawResult> RunAll(Action<LawResult> onResult)
    {
        var laws = Laws.ToList();
        var results = new LawResult[laws.Count];
        if (!Backend.SupportsParallel || Options.MaxParallel <= 1)
        {
            for (var i = 0; i < laws.Count; i++)
            {
                results[i] = Run(laws[i]);
                onResult?.Invoke(results[i]);
            }
            return results;
        }
        var parallel = new ParallelOptions { MaxDegreeOfParallelism = Options.MaxParallel };
        // Witnesses first, so a law's time is its own and a witness two laws share is built once.
        Parallel.ForEach(laws, parallel, law =>
        {
            foreach (var p in law.Parameters)
                DomainOf(p, law);
        });
        Parallel.For(0, laws.Count, parallel, i =>
        {
            results[i] = Run(laws[i]);
            onResult?.Invoke(results[i]);
        });
        return results;
    }

    /// <summary>Runs one law: on the calling thread when the backend can stop it at the budget,
    /// else on a thread of its own that is abandoned past <see cref="LawRunOptions.AbandonSeconds"/>.</summary>
    public LawResult Run(FunctionDef law)
    {
        if (!Backend.SupportsParallel)
            return Evaluate(law);
        var sw = Stopwatch.StartNew();
        LawResult result = null;
        Exception fault = null;
        var thread = new Thread(() =>
        {
            try { result = Evaluate(law); }
            catch (Exception e) { fault = e; }
        }, StackBytes) { IsBackground = true, Name = LawSelection.FullName(law) };
        thread.Start();
        var finished = Options.AbandonSeconds is double abandon
            ? thread.Join(TimeSpan.FromSeconds(abandon))
            : JoinForever(thread);
        if (!finished)
            return new LawResult
            {
                Law = law, Status = LawStatus.Error, Elapsed = sw.Elapsed,
                Message = $"still running after {sw.Elapsed.TotalSeconds:F0}s (budget {Options.BudgetSeconds ?? 0:0.#}s); abandoned",
            };
        if (fault != null)
            return new LawResult { Law = law, Status = LawStatus.Error, Elapsed = sw.Elapsed, Message = $"the {Backend.Name} backend faulted: {fault.GetType().Name}: {fault.Message}" };
        return result;
    }

    private static bool JoinForever(Thread thread)
    {
        thread.Join();
        return true;
    }

    private LawResult Evaluate(FunctionDef law)
    {
        var sw = Stopwatch.StartNew();
        var domains = new List<Domain>();
        foreach (var p in law.Parameters)
        {
            var d = DomainOf(p, law);
            if (d.Reason != null)
                return new LawResult { Law = law, Status = LawStatus.Skipped, Message = $"no values for {p.Name}: {p.Type}: {d.Reason}", Elapsed = sw.Elapsed };
            domains.Add(d);
        }

        var exhaustive = domains.All(d => d.Finite != null)
                         && domains.Aggregate(1L, (n, d) => n * d.Finite.Count) <= Options.MaxExhaustive;
        var cases = exhaustive ? Product(domains) : Sample(law, domains);

        var trials = 0;
        var budget = Options.BudgetSeconds;
        Backend.SetDeadline(budget is double b ? DateTime.UtcNow.AddSeconds(b) : null);
        foreach (var args in cases)
        {
            trials++;
            try
            {
                var r = Backend.ToInterpreterValue(Backend.Invoke(law, args));
                if (r is bool ok)
                {
                    if (!ok)
                        return Failure(law, LawStatus.Fail, trials, exhaustive, args, null, sw);
                }
                else
                    return Failure(law, LawStatus.Error, trials, exhaustive, args, $"returned {ValueFormat.Format(r, 60)}, not a Boolean", sw);
            }
            catch (HostObligationException e)
            {
                return new LawResult { Law = law, Status = LawStatus.Obligation, Trials = trials, Witness = Report(args), Message = e.Describe(), Elapsed = sw.Elapsed };
            }
            catch (EvaluationException e)
            {
                var message = e.Message.StartsWith(TimeBudgetMessage, StringComparison.Ordinal)
                    ? $"{OverBudget(sw, budget)} (stopped in case {trials})"
                    : e.Describe();
                return Failure(law, LawStatus.Error, trials, exhaustive, args, message, sw);
            }
        }
        if (budget is double limit && sw.Elapsed.TotalSeconds > limit)
            return new LawResult { Law = law, Status = LawStatus.Error, Trials = trials, Exhaustive = exhaustive, Message = $"{OverBudget(sw, budget)} over {trials} cases", Elapsed = sw.Elapsed };
        return new LawResult { Law = law, Status = LawStatus.Pass, Trials = trials, Exhaustive = exhaustive, Elapsed = sw.Elapsed };
    }

    /// <summary>The message the interpreter faults with at its deadline (Interpreter.Activate).</summary>
    public const string TimeBudgetMessage = "time budget exceeded";

    private static string OverBudget(Stopwatch sw, double? budget)
        => $"took {sw.Elapsed.TotalSeconds:F2}s, over the {budget ?? 0:0.#}s budget";

    /// <summary>The arguments as interpreter values, so a report formats them.</summary>
    private object[] Report(object[] args)
        => args.Select(Backend.ToInterpreterValue).ToArray();

    private LawResult Failure(FunctionDef law, LawStatus status, int trials, bool exhaustive, object[] args, string message, Stopwatch sw)
        => new LawResult
        {
            Law = law, Status = status, Trials = trials, Exhaustive = exhaustive, Witness = Report(args),
            Message = message, Measure = TryMeasure(law, args), Elapsed = sw.Elapsed,
        };

    /// <summary>The value of a companion <c>Measure_&lt;law&gt;</c> with the law's parameter list, if
    /// one is declared; null otherwise or if it faults.</summary>
    private object TryMeasure(FunctionDef law, object[] args)
    {
        var name = Options.MeasurePrefix + law.Name.Substring(Options.LawPrefix.Length);
        var m = Interpreter.FunctionsNamed(name).FirstOrDefault(f => f.Parameters.Count == args.Length && f.Body != null);
        if (m == null)
            return null;
        try { return Backend.ToInterpreterValue(Backend.Invoke(m, args)); }
        catch (EvaluationException) { return null; }
    }

    // --- domains -------------------------------------------------------------

    private sealed class Domain
    {
        public TypeExpression Type;
        public string Reason;
        /// <summary>Every value, when the domain is finite; null when it must be sampled.</summary>
        public List<object> Finite;
        /// <summary>Witnesses and finite values to prefer when sampling.</summary>
        public List<object> Preferred = new List<object>();
        /// <summary>Concrete types to generate from when sampling (several for an interface).</summary>
        public List<TypeExpression> Generate = new List<TypeExpression>();
    }

    private Domain DomainOf(ParameterDef p, FunctionDef law)
    {
        var t = p.Type;
        var d = new Domain { Type = t };
        if (t?.Def == null) { d.Reason = "untyped"; return d; }

        // A `_` receiver takes the type itself.
        if (p.Name == "_")
        {
            d.Finite = new List<object> { new TypeToken(t) };
            return d;
        }

        if (t.Def.Name == "Integer")
        {
            d.Finite = Enumerable.Range(Options.IntegerMin, Options.IntegerMax - Options.IntegerMin + 1).Select(i => Backend.FromInterpreterValue((long)i)).ToList();
            d.Preferred.AddRange(d.Finite);
            d.Generate.Add(t);
            return d;
        }

        var finite = _gen.FiniteDomain(t);
        if (finite != null)
        {
            d.Finite = finite.Select(Backend.FromInterpreterValue).ToList();
            return d;
        }

        var witnesses = WitnessesOf(t);
        d.Preferred.AddRange(witnesses);

        if (t.Def.IsInterface())
        {
            foreach (var impl in Implementers(t))
            {
                d.Preferred.AddRange(WitnessesOf(impl));
                if (_gen.CanGenerate(impl, out _))
                    d.Generate.Add(impl);
            }
            if (d.Preferred.Count == 0 && d.Generate.Count == 0)
                d.Reason = "no concrete implementer has witnesses or generatable fields";
            return d;
        }

        if (_gen.CanGenerate(t, out var reason))
            d.Generate.Add(t);
        else if (witnesses.Count == 0)
            d.Reason = reason + $", and no {Options.WitnessPrefix}* returns {t.Name}";
        else
            d.Finite = witnesses.ToList();
        return d;
    }

    private IEnumerable<TypeExpression> Implementers(TypeExpression iface)
        => Interpreter.Compilation.TypeDefinitions
            .Where(td => td.IsConcrete() && !td.IsUnique && td.TypeParameters.Count == 0
                         && ConceptClosure.FindInstance(td.ToTypeExpression(), iface.Def.Name) != null)
            .Select(td => td.ToTypeExpression());

    /// <summary>The values of every nullary witness of a type, evaluated once by the backend.</summary>
    public IReadOnlyList<object> WitnessesOf(TypeExpression t)
    {
        // Keyed by the whole type, arguments included: an Array<Diagnostic> witness must not
        // serve an Array<Integer> parameter.
        var key = t.ToString();
        return _witnessCache.GetOrAdd(key, k => new Lazy<List<object>>(() => EvaluateWitnesses(k), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    private List<object> EvaluateWitnesses(string key)
    {
        var list = new List<object>();
        Backend.SetDeadline(null);
        foreach (var f in Interpreter.Functions)
        {
            if (!f.Name.StartsWith(Options.WitnessPrefix, StringComparison.Ordinal) || f.Body == null)
                continue;
            if (f.ReturnType?.ToString() != key)
                continue;
            object[] args;
            if (f.Parameters.Count == 0)
                args = Array.Empty<object>();
            else if (f.Parameters.Count == 1 && f.Parameters[0].Name == "_")
                args = new object[] { new TypeToken(f.Parameters[0].Type) };
            else
                continue;
            var sw = Stopwatch.StartNew();
            string fault = null;
            try { list.Add(Backend.Invoke(f, args)); }
            catch (EvaluationException e) { fault = e.Describe(); /* the laws over the type run on the other witnesses */ }
            _witnessResults.Enqueue(new WitnessResult(LawSelection.FullName(f), sw.Elapsed, fault));
        }
        return list;
    }

    private static IEnumerable<object[]> Product(List<Domain> domains)
    {
        var idx = new int[domains.Count];
        if (domains.Any(d => d.Finite.Count == 0))
            yield break;
        while (true)
        {
            yield return domains.Select((d, i) => d.Finite[idx[i]]).ToArray();
            var k = domains.Count - 1;
            while (k >= 0 && ++idx[k] == domains[k].Finite.Count)
                idx[k--] = 0;
            if (k < 0)
                yield break;
        }
    }

    private IEnumerable<object[]> Sample(FunctionDef law, List<Domain> domains)
    {
        var rng = new Random(ValueGenerator.StableSeed(law.Name) ^ Options.Seed);
        for (var n = 0; n < Options.Trials; n++)
            yield return domains.Select(d => Draw(d, rng)).ToArray();
    }

    private object Draw(Domain d, Random rng)
    {
        if (d.Finite != null && d.Generate.Count == 0)
            return d.Finite[rng.Next(d.Finite.Count)];
        var usePreferred = d.Preferred.Count > 0 && (d.Generate.Count == 0 || rng.NextDouble() < 0.5);
        if (usePreferred)
            return d.Preferred[rng.Next(d.Preferred.Count)];
        return Backend.FromInterpreterValue(_gen.Generate(d.Generate[rng.Next(d.Generate.Count)], rng));
    }
}

/// <summary>One line of a verdict file: the law's name, its verdict (<c>pass</c>, <c>fail</c>,
/// <c>error</c>, <c>obligation</c>, <c>skipped</c>), the milliseconds it took (null in a file
/// written before the column existed), and the first failing witness, the fault, or the reason,
/// on one line.</summary>
public sealed record LawVerdict(string Name, string Verdict, string Detail, long? ElapsedMs = null);

/// <summary>A verdict that differs between a baseline and a run. <c>Before</c> is null for a law
/// the baseline lacks, <c>After</c> for a law the run no longer has.</summary>
public sealed record LawChange(string Name, string Before, string After)
{
    public const string PassVerdict = "pass";

    /// <summary>A law that passed and now does not, or that passed and is gone from a whole run.</summary>
    public bool Regression => Before == PassVerdict && After != PassVerdict;

    public override string ToString()
        => $"{(After == null ? "removed" : Regression ? "regressed" : Before == null ? "new" : "changed"),-10} {Name}  {Before ?? "-"} -> {After ?? "-"}";
}

/// <summary>
/// Verdict files: one tab-separated <see cref="LawVerdict"/> per line, sorted by name, so two runs
/// diff line by line and a committed file records which laws held. A name shared by two laws
/// (overloads in one class) carries the parameter types. <see cref="Compare"/> checks a run
/// against a baseline file; only the verdicts are compared, never the witness text.
/// </summary>
public static class LawReport
{
    /// <summary>Witnesses and faults are cut to this many characters, so one huge value cannot
    /// swamp the file.</summary>
    public const int MaxDetail = 400;

    public static IReadOnlyList<LawVerdict> Verdicts(IEnumerable<LawResult> results)
    {
        var list = results.ToList();
        var shared = list.GroupBy(r => r.Name).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();
        return list
            .Select(r => new LawVerdict(
                shared.Contains(r.Name) ? $"{r.Name}({string.Join(", ", r.Law.Parameters.Select(p => p.Type))})" : r.Name,
                r.Status.ToString().ToLowerInvariant(),
                OneLine(Detail(r)),
                (long)r.Elapsed.TotalMilliseconds))
            .OrderBy(v => v.Name, StringComparer.Ordinal)
            .ToList();
    }

    private static string Detail(LawResult r)
        => r.Status switch
        {
            LawStatus.Pass => "",
            LawStatus.Fail => r.FormatWitness(),
            LawStatus.Error => $"{r.FormatWitness()}  {r.Message}",
            _ => r.Message ?? "",
        };

    private static string OneLine(string text)
    {
        var flat = string.Join(" ", text.Split(new[] { '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
        return flat.Length <= MaxDetail ? flat : flat.Substring(0, MaxDetail) + "...";
    }

    /// <summary>Name, verdict, elapsed milliseconds, detail, tab-separated; a verdict without a
    /// time is written in the older three-column layout (name, verdict, detail).</summary>
    public static string Format(IEnumerable<LawVerdict> verdicts)
        => string.Concat(verdicts.Select(v => v.ElapsedMs is long ms
            ? $"{v.Name}\t{v.Verdict}\t{ms}\t{v.Detail}\n"
            : $"{v.Name}\t{v.Verdict}\t{v.Detail}\n"));

    /// <summary>The verdicts of a file written by <see cref="Format"/>, in either layout (a
    /// detail never holds a tab, so four fields mean the layout with a time); blank lines and
    /// <c>#</c> lines are skipped.</summary>
    public static IReadOnlyList<LawVerdict> Parse(string text)
        => text.Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .Where(l => l.Trim().Length > 0 && !l.StartsWith('#'))
            .Select(l => l.Split('\t', 4))
            .Select(f => f.Length == 4 && long.TryParse(f[2].Trim(), out var ms)
                ? new LawVerdict(f[0].Trim(), f[1].Trim(), f[3], ms)
                : new LawVerdict(f[0].Trim(), f.Length > 1 ? f[1].Trim() : "", f.Length > 2 ? string.Join("\t", f.Skip(2)) : ""))
            .ToList();

    /// <summary>Every law whose verdict differs, sorted by name. A law the baseline lacks is
    /// listed as new. A baseline law absent from the run is listed as removed only when the run
    /// covered every law (<paramref name="wholeRun"/>); a filtered run leaves it out, because the
    /// filter, not a change, is why it did not run.</summary>
    public static IReadOnlyList<LawChange> Compare(IEnumerable<LawVerdict> baseline, IEnumerable<LawVerdict> current, bool wholeRun)
    {
        var before = baseline.GroupBy(v => v.Name).ToDictionary(g => g.Key, g => g.Last().Verdict);
        var after = current.GroupBy(v => v.Name).ToDictionary(g => g.Key, g => g.Last().Verdict);
        var changed = after
            .Where(kv => before.GetValueOrDefault(kv.Key) != kv.Value)
            .Select(kv => new LawChange(kv.Key, before.GetValueOrDefault(kv.Key), kv.Value));
        var removed = wholeRun
            ? before.Where(kv => !after.ContainsKey(kv.Key)).Select(kv => new LawChange(kv.Key, kv.Value, null))
            : Enumerable.Empty<LawChange>();
        return changed.Concat(removed).OrderBy(c => c.Name, StringComparer.Ordinal).ToList();
    }
}
