using System.Diagnostics;
using System.Reflection;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;

namespace Plato.Interpreter.Tests;

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
    private const string GeneratedNamespace = "Ara3D.Geometry";

    public NumberProfile Profile { get; }
    public DifferentialBackend Backend { get; }
    public IReadOnlyList<DifferentialLaw> Laws { get; }
    public int GenericSkipped { get; }
    public TimeSpan Elapsed { get; }

    public IEnumerable<DifferentialLaw> Compared => Laws.Where(l => l.Skipped == null);
    public IEnumerable<(DifferentialLaw Law, DifferentialCase Case)> Mismatches
        => Compared.SelectMany(l => l.Mismatches.Select(c => (l, c)));

    private DifferentialRun(NumberProfile profile, DifferentialBackend backend, IReadOnlyList<DifferentialLaw> laws, int genericSkipped, TimeSpan elapsed)
        => (Profile, Backend, Laws, GenericSkipped, Elapsed) = (profile, backend, laws, genericSkipped, elapsed);

    public static DifferentialRun Execute(DifferentialProgram program, NumberProfile profile)
    {
        var sw = Stopwatch.StartNew();
        var backend = DifferentialBackend.Build(program.Compilation, profile);
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(program.Compilation, profile);
        var values = new DifferentialValues(interp);
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
                if (!DifferentialValues.IsBooleanLike(m.ReturnType))
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

    private static DifferentialLaw RunLaw(string key, MethodInfo method, Ara3D.Geometry.Interpreter.Interpreter interp, DifferentialValues values)
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
            var platoArgs = hostTypes.Select(t => values.Generate(t, rng)).ToArray();
            object[] hostArgs;
            try
            {
                hostArgs = platoArgs.Select((a, k) => values.ToHost(a, hostTypes[k])).ToArray();
            }
            catch (ValueConversionException e)
            {
                law.Skipped = e.Message;
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
            return DifferentialValues.ToBool(arm()) ? "true" : "false";
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
