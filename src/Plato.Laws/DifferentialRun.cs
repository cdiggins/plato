using System.Diagnostics;
using System.Reflection;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;

namespace Ara3D.Geometry.Laws;

// Moved from tests/Plato.Interpreter.Tests for TKT-354 so the CLI and the warm server share the
// compiled arm; the differential test (DifferentialTests.cs there) still drives it.

/// <summary>One case of one law: the witness and what each arm said (a Boolean, or the exception).</summary>
public sealed record DifferentialCase(int Index, string Witness, string Interpreter, string CSharp, bool Match);

/// <summary>One Law_* method of the generated assembly and its cases, or why it was skipped.</summary>
public sealed class DifferentialLaw
{
    public string Key { get; init; }
    public MethodInfo Method { get; init; }
    public string Skipped { get; set; }
    public List<DifferentialCase> Cases { get; } = new();
    public TimeSpan InterpreterTime { get; set; }
    public TimeSpan CSharpTime { get; set; }
    public IEnumerable<DifferentialCase> Mismatches => Cases.Where(c => !c.Match);
}

/// <summary>
/// Runs every Law_* of the generated assembly in both arms: the C# method by reflection and the
/// Plato function of the same name in the interpreter, on the same 25 seeded witnesses per law,
/// generated in the interpreter from the C# parameter types and converted across. Discovery takes
/// static methods on static classes and instance methods on structs; generic method definitions
/// and laws on generic struct definitions are counted, not run.
/// </summary>
public sealed class DifferentialRun
{
    public const int Trials = 25;
    private const string GeneratedNamespace = CSharpProgram.Namespace;

    public NumberProfile Profile { get; }
    public CSharpProgram Backend { get; }
    public IReadOnlyList<DifferentialLaw> Laws { get; }
    public int GenericSkipped { get; }
    public TimeSpan Elapsed { get; }

    public IEnumerable<DifferentialLaw> Compared => Laws.Where(l => l.Skipped == null);
    public IEnumerable<(DifferentialLaw Law, DifferentialCase Case)> Mismatches
        => Compared.SelectMany(l => l.Mismatches.Select(c => (l, c)));

    private DifferentialRun(NumberProfile profile, CSharpProgram backend, IReadOnlyList<DifferentialLaw> laws, int genericSkipped, TimeSpan elapsed)
        => (Profile, Backend, Laws, GenericSkipped, Elapsed) = (profile, backend, laws, genericSkipped, elapsed);

    public static DifferentialRun Execute(PlatoCompilation compilation, NumberProfile profile)
    {
        var sw = Stopwatch.StartNew();
        var backend = CSharpProgram.Build(compilation, profile);
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, profile);
        var values = new HostValues(interp, backend.Assembly);
        var laws = new List<DifferentialLaw>();
        var generic = 0;
        foreach (var (key, method) in Discover(backend.Assembly, ref generic).OrderBy(l => l.Key, StringComparer.Ordinal))
            laws.Add(RunLaw(key, method, interp, values));
        return new DifferentialRun(profile, backend, laws, generic, sw.Elapsed);
    }

    private static IEnumerable<(string Key, MethodInfo Method)> Discover(Assembly asm, ref int generic)
    {
        var found = new List<(string, MethodInfo)>();
        foreach (var t in asm.GetTypes().Where(t => t.Namespace == GeneratedNamespace && t.IsPublic))
        {
            var isStaticClass = t.IsAbstract && t.IsSealed;
            if (!isStaticClass && !t.IsValueType)
                continue;
            var flags = BindingFlags.Public | (isStaticClass ? BindingFlags.Static : BindingFlags.Instance);
            foreach (var m in t.GetMethods(flags).Where(m => m.Name.StartsWith("Law_", StringComparison.Ordinal) && !m.IsSpecialName))
            {
                if (!HostValues.IsBooleanLike(m.ReturnType))
                    continue;
                if (m.IsGenericMethodDefinition || t.IsGenericTypeDefinition)
                {
                    generic++;
                    continue;
                }
                found.Add(($"{t.Name}.{m.Name}", m));
            }
        }
        return found;
    }

    private static DifferentialLaw RunLaw(string key, MethodInfo method, Ara3D.Geometry.Interpreter.Interpreter interp, HostValues values)
    {
        var law = new DifferentialLaw { Key = key, Method = method };
        var hostTypes = (method.IsStatic ? Array.Empty<Type>() : new[] { method.DeclaringType })
            .Concat(method.GetParameters().Select(p => p.ParameterType))
            .ToArray();
        foreach (var t in hostTypes)
            if (!values.CanGenerate(t, out var reason))
            {
                law.Skipped = reason;
                return law;
            }
        var defs = interp.FunctionsNamed(method.Name).Where(f => f.Body != null).ToList();
        if (defs.Count == 0)
        {
            law.Skipped = $"no Plato function named {method.Name}";
            return law;
        }
        // A `_`-receiver law (`Law_X(_: T)`) is emitted with the receiver dropped.
        var typeReceiver = defs.All(d => d.Parameters.Count == hostTypes.Length + 1 && d.Parameters[0].Name == "_")
            ? new TypeToken(defs[0].Parameters[0].Type) : null;

        var rng = new Random(ValueGenerator.StableSeed(key));
        var interpClock = new Stopwatch();
        var hostClock = new Stopwatch();
        for (var i = 0; i < Trials; i++)
        {
            object[] platoArgs, hostArgs;
            try
            {
                platoArgs = hostTypes.Select(t => values.Generate(t, rng)).ToArray();
                hostArgs = platoArgs.Select((a, k) => values.ToHost(a, hostTypes[k])).ToArray();
            }
            catch (ValueConversionException e)
            {
                law.Skipped = e.Message;
                law.Cases.Clear();
                return law;
            }
            catch (EvaluationException e)
            {
                // A library generator (ValueGenerator.GeneratorsOf) faulted.
                law.Skipped = $"drawing case {i}: {e.Describe()}";
                law.Cases.Clear();
                return law;
            }
            var interpArgs = typeReceiver == null ? platoArgs : platoArgs.Prepend(typeReceiver).ToArray();

            interpClock.Start();
            var mine = Outcome(() =>
            {
                var f = interp.ResolveByValues(method.Name, interpArgs, out var converted)
                    ?? throw new EvaluationException($"no overload of {method.Name} takes ({string.Join(", ", interpArgs.Select(a => interp.TypeOf(a)))})");
                return interp.Invoke(f, converted);
            });
            interpClock.Stop();

            hostClock.Start();
            var theirs = Outcome(() => method.IsStatic
                ? method.Invoke(null, hostArgs)
                : method.Invoke(hostArgs[0], hostArgs.Skip(1).ToArray()));
            hostClock.Stop();

            var witness = string.Join(", ", platoArgs.Select(a => ValueFormat.Format(a, 120)));
            law.Cases.Add(new DifferentialCase(i, witness, mine, theirs, mine == theirs || (IsThrow(mine) && IsThrow(theirs))));
        }
        law.InterpreterTime = interpClock.Elapsed;
        law.CSharpTime = hostClock.Elapsed;
        return law;
    }

    private const string ThrewPrefix = "threw ";

    private static bool IsThrow(string outcome) => outcome.StartsWith(ThrewPrefix, StringComparison.Ordinal);

    /// <summary>"true", "false", or "threw &lt;type&gt;: &lt;message&gt;".</summary>
    private static string Outcome(Func<object> arm)
    {
        try
        {
            return HostValues.ToBool(arm()) ? "true" : "false";
        }
        catch (Exception e)
        {
            var inner = e is TargetInvocationException ti && ti.InnerException != null ? ti.InnerException : e;
            var message = inner is EvaluationException ev ? ev.Describe() : inner.Message;
            return $"{ThrewPrefix}{inner.GetType().Name}: {Truncate(message, 160)}";
        }
    }

    private static string Truncate(string s, int n)
        => s.Length <= n ? s : s.Substring(0, n) + "~";
}

/// <summary>A value of the <see cref="DifferentialBackend"/>: the same Plato value in each engine.</summary>
public sealed record DifferentialValue(object Interpreter, object Host);

/// <summary>
/// The law runner's backend that evaluates every call twice, in the interpreter and on the
/// compiled C#, and compares the answers with Plato's structural equality
/// (<see cref="ValueEquality"/>, which the generated <c>Equals</c> matches since TKT-355; two
/// function values always agree, since neither engine can compare them). Every
/// call the runner makes goes through it: each <c>Witness_*</c> is built in both engines, and each
/// law and its <c>Measure_</c> and <c>Premise_</c> companions run on the same witnesses in both.
/// A disagreement (different values, or one engine faulting where the other answers) is thrown
/// as a <see cref="BackendsDifferException"/> naming both answers and kept in
/// <see cref="Mismatches"/>; the runner reports the law as <c>differs</c> and drops a differing
/// witness. When both engines fault the interpreter's fault is thrown; when the interpreter stops
/// at the time budget the compiled arm is not run, so a slow law is a budget error, never a
/// mismatch. Calls run one at a time, because the interpreter's state is shared (TKT-438).
/// </summary>
public sealed class DifferentialBackend : ILawBackend
{
    public InterpreterBackend Interpreted { get; }
    /// <summary>The compiled arm: a <see cref="CSharpBackend"/> in every real run.</summary>
    public ILawBackend Compiled { get; }
    private readonly System.Collections.Concurrent.ConcurrentQueue<string> _mismatches = new();

    public DifferentialBackend(InterpreterBackend interpreted, ILawBackend compiled)
        => (Interpreted, Compiled) = (interpreted, compiled);

    /// <summary>One line per disagreement, in the order found: the function, its arguments and
    /// both answers.</summary>
    public IReadOnlyList<string> Mismatches => _mismatches.ToList();

    public string Name => LawBackends.DifferentialName;
    public bool SupportsParallel => false;
    public void SetDeadline(DateTime? deadline) => Interpreted.SetDeadline(deadline);

    public object FromInterpreterValue(object v)
        => v is TypeToken ? new DifferentialValue(v, v) : new DifferentialValue(v, Compiled.FromInterpreterValue(v));

    public object ToInterpreterValue(object v)
        => v is DifferentialValue d ? d.Interpreter : v;

    public object Invoke(FunctionDef f, object[] args)
    {
        var interpArgs = args.Select(a => a is DifferentialValue d ? d.Interpreter : a).ToArray();
        var hostArgs = args.Select(a => a is DifferentialValue d ? d.Host : a).ToArray();

        object mine = null, theirs = null;
        EvaluationException mineFault = null, theirsFault = null;
        try { mine = Interpreted.Invoke(f, interpArgs); }
        catch (EvaluationException e) when (e.Message.StartsWith(LawRunner.TimeBudgetMessage, StringComparison.Ordinal)) { throw; }
        catch (EvaluationException e) { mineFault = e; }
        try { theirs = Compiled.Invoke(f, hostArgs); }
        catch (EvaluationException e) { theirsFault = e; }

        if (mineFault != null && theirsFault != null)
        {
            if ((mineFault is HostObligationException) == (theirsFault is HostObligationException))
                throw mineFault;
            throw Differ(f, interpArgs, Describe(mineFault), Describe(theirsFault));
        }
        if (mineFault != null)
            throw Differ(f, interpArgs, Describe(mineFault), Format(Compiled.ToInterpreterValue(theirs)));
        if (theirsFault != null)
            throw Differ(f, interpArgs, Format(mine), Describe(theirsFault));

        object theirsAsPlato;
        try { theirsAsPlato = Compiled.ToInterpreterValue(theirs); }
        catch (ValueConversionException) { return new DifferentialValue(mine, theirs); }
        if (!Agree(mine, theirsAsPlato))
            throw Differ(f, interpArgs, Format(mine), Format(theirsAsPlato));
        return new DifferentialValue(mine, theirs);
    }

    /// <summary>Plato's structural equality (<see cref="ValueEquality"/>), except that two
    /// function values agree: a closure and a delegate cannot be compared, and a function field
    /// is checked through the laws that call it.</summary>
    private static bool Agree(object a, object b)
    {
        if (IsFunction(a) || IsFunction(b))
            return IsFunction(a) && IsFunction(b);
        switch (a)
        {
            case PlatoRecord r when b is PlatoRecord s:
                return r.Def.Name == s.Def.Name && AllAgree(r.Fields, s.Fields);
            case PlatoSum u when b is PlatoSum v:
                return u.Def.Name == v.Def.Name && u.Case.Tag == v.Case.Tag && AllAgree(u.Fields, v.Fields);
            case PlatoArray xs when b is PlatoArray ys:
                return AllAgree(xs.Items, ys.Items);
            default:
                return ValueEquality.Equals(a, b);
        }
    }

    private static bool IsFunction(object v) => v is PlatoFunction || v is Delegate;

    private static bool AllAgree(IReadOnlyList<object> a, IReadOnlyList<object> b)
        => a.Count == b.Count && a.Zip(b, Agree).All(x => x);

    private BackendsDifferException Differ(FunctionDef f, object[] interpArgs, string interpreted, string compiled)
    {
        var call = $"{LawSelection.FullName(f)}({string.Join(", ", interpArgs.Select(a => ValueFormat.Format(a, 80)))})";
        var message = $"interpreter: {interpreted}; csharp: {compiled}";
        _mismatches.Enqueue($"{call}  {message}");
        return new BackendsDifferException(message);
    }

    private static string Describe(EvaluationException e) => "threw " + Truncate(e.Describe(), 200);

    private static string Format(object v) => ValueFormat.Format(v, 200);

    private static string Truncate(string s, int n) => s.Length <= n ? s : s.Substring(0, n) + "~";
}
