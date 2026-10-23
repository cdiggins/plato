using System.Collections.Concurrent;
using System.Reflection;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Interpreter;

namespace Ara3D.Geometry.Laws;

/// <summary>
/// The compiled backend of the <see cref="LawRunner"/>: each Plato function is called as the
/// method the C# writer made of it. A library function is a static method of the static class
/// named after its library (an extension method when its first parameter is a concrete type); a
/// function whose first parameter is an interface is an instance method on every implementing
/// struct; a <c>_</c> receiver is dropped. The method is chosen among those sharing the Plato
/// name by the run-time types of the arguments, a generic method's type parameters inferred from
/// them, and the choice is memoized per argument type list. A <c>NotImplementedException</c>
/// from a function declared without a body is reported as a <see cref="HostObligationException"/>;
/// every other exception the program throws becomes an <see cref="EvaluationException"/>.
/// Calls share no mutable state, so laws run in parallel.
/// </summary>
public sealed class CSharpBackend : ILawBackend
{
    public CSharpProgram Program { get; }
    public HostValues Values { get; }

    private readonly Dictionary<string, List<MethodInfo>> _methodsByName;
    private readonly ConcurrentDictionary<string, MethodInfo> _resolved = new();

    private CSharpBackend(CSharpProgram program, Interpreter.Interpreter interp)
    {
        Program = program;
        Values = new HostValues(interp, program.Assembly);
        _methodsByName = Discover(program.Assembly);
    }

    /// <summary>Emits and compiles the interpreter's compilation under its Number profile;
    /// <paramref name="lowerLoops"/> is the writer's <c>--loops</c>.</summary>
    public static CSharpBackend Build(Interpreter.Interpreter interp, bool lowerLoops = false)
        => new(CSharpProgram.Build(interp.Compilation, interp.Profile, lowerLoops), interp);

    /// <summary>Wraps an already compiled program, so a warm host compiles once per source generation.</summary>
    public static CSharpBackend Over(CSharpProgram program, Interpreter.Interpreter interp)
        => new(program, interp);

    public string Name => "csharp";
    public bool SupportsParallel => true;
    public void SetDeadline(DateTime? deadline) { }

    public object FromInterpreterValue(object v)
    {
        try { return Values.ToHost(v); }
        catch (ValueConversionException e) { throw new EvaluationException(e.Message); }
    }

    public object ToInterpreterValue(object v) => Values.ToPlato(v);

    public object Invoke(FunctionDef f, object[] args)
    {
        var hostArgs = args.Where(a => a is not TypeToken).ToArray();
        var method = Resolve(f, hostArgs)
            ?? throw new EvaluationException($"no C# method for {LawSelection.FullName(f)}({string.Join(", ", hostArgs.Select(a => a == null ? "null" : HostValues.Describe(a.GetType())))})");
        try
        {
            return method.IsStatic
                ? method.Invoke(null, hostArgs)
                : method.Invoke(hostArgs[0], hostArgs.Skip(1).ToArray());
        }
        catch (TargetInvocationException e)
        {
            throw Translate(e.InnerException ?? e, f);
        }
        catch (ArgumentException e)
        {
            throw new EvaluationException($"cannot bind {LawSelection.FullName(f)}: {e.Message}");
        }
    }

    private const string BodilessPrefix = "plato: ";
    private const string BodilessSuffix = " is declared without a body";

    private static EvaluationException Translate(Exception inner, FunctionDef f)
    {
        if (inner is NotImplementedException && inner.Message.StartsWith(BodilessPrefix, StringComparison.Ordinal) && inner.Message.EndsWith(BodilessSuffix, StringComparison.Ordinal))
        {
            var name = inner.Message.Substring(BodilessPrefix.Length, inner.Message.Length - BodilessPrefix.Length - BodilessSuffix.Length);
            var obligation = new HostObligationException(name);
            obligation.PlatoStack.Add(LawSelection.FullName(f));
            return obligation;
        }
        var e = new EvaluationException($"{inner.GetType().Name}: {inner.Message}", inner);
        e.PlatoStack.Add(LawSelection.FullName(f));
        return e;
    }

    // --- discovery and resolution ------------------------------------------------

    /// <summary>Every public method of the generated namespace by name: static methods of static
    /// classes and structs, instance methods of structs, generic definitions included.</summary>
    private static Dictionary<string, List<MethodInfo>> Discover(Assembly asm)
    {
        var byName = new Dictionary<string, List<MethodInfo>>(StringComparer.Ordinal);
        foreach (var t in asm.GetTypes().Where(t => t.Namespace == CSharpProgram.Namespace && t.IsPublic))
        {
            var isStaticClass = t.IsAbstract && t.IsSealed;
            if (!isStaticClass && !t.IsValueType)
                continue;
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (m.IsSpecialName)
                    continue;
                if (!byName.TryGetValue(m.Name, out var list))
                    byName[m.Name] = list = new List<MethodInfo>();
                list.Add(m);
            }
        }
        return byName;
    }

    private MethodInfo Resolve(FunctionDef f, object[] hostArgs)
    {
        var argTypes = hostArgs.Select(a => a?.GetType()).ToArray();
        var key = f.Name + "|" + (f.OwnerType?.Name ?? "") + "|" + string.Join(",", argTypes.Select(t => t?.FullName ?? "null"));
        return _resolved.GetOrAdd(key, _ => Choose(f, argTypes));
    }

    private MethodInfo Choose(FunctionDef f, Type[] argTypes)
    {
        if (!_methodsByName.TryGetValue(f.Name, out var candidates))
            return null;
        MethodInfo best = null;
        var bestScore = int.MinValue;
        foreach (var candidate in candidates)
        {
            var m = candidate;
            var ps = ParameterTypes(m);
            if (ps.Length != argTypes.Length)
                continue;
            if (m.IsGenericMethodDefinition)
            {
                m = Instantiate(m, ps, argTypes);
                if (m == null)
                    continue;
                ps = ParameterTypes(m);
            }
            if (m.DeclaringType.IsGenericTypeDefinition)
                continue;
            var score = 0;
            var fits = true;
            for (var i = 0; i < ps.Length && fits; i++)
            {
                if (argTypes[i] == null)
                    fits = !ps[i].IsValueType;
                else if (ps[i] == argTypes[i])
                    score += 2;
                else if (ps[i].IsAssignableFrom(argTypes[i]))
                    score += 1;
                else
                    fits = false;
            }
            if (!fits)
                continue;
            if (f.OwnerType != null && m.DeclaringType.Name == f.OwnerType.Name)
                score += 1;
            if (score > bestScore)
                (best, bestScore) = (m, score);
        }
        return best;
    }

    /// <summary>The declaring type first for an instance method, so a struct's receiver counts as
    /// the first argument, as the Plato function's first parameter is.</summary>
    private static Type[] ParameterTypes(MethodInfo m)
    {
        var ps = m.GetParameters().Select(p => p.ParameterType);
        return (m.IsStatic ? ps : ps.Prepend(m.DeclaringType)).ToArray();
    }

    /// <summary>The generic method with its type parameters bound from the argument types: a
    /// parameter typed by the type parameter itself, or by <c>IReadOnlyList&lt;T&gt;</c> given a
    /// <c>T[]</c>; null when one stays unbound.</summary>
    private static MethodInfo Instantiate(MethodInfo m, Type[] ps, Type[] argTypes)
    {
        var map = new Dictionary<Type, Type>();
        for (var i = 0; i < ps.Length; i++)
            Bind(ps[i], argTypes[i], map);
        var generics = m.GetGenericArguments();
        var bound = new Type[generics.Length];
        for (var i = 0; i < generics.Length; i++)
            if (!map.TryGetValue(generics[i], out bound[i]))
                return null;
        try { return m.MakeGenericMethod(bound); }
        catch (ArgumentException) { return null; }
    }

    private static void Bind(Type parameter, Type argument, Dictionary<Type, Type> map)
    {
        if (argument == null)
            return;
        if (parameter.IsGenericParameter)
        {
            map.TryAdd(parameter, argument);
            return;
        }
        if (!parameter.IsGenericType)
            return;
        var elem = parameter.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) && argument.IsArray
            ? new[] { argument.GetElementType() }
            : argument.IsGenericType && argument.GetGenericTypeDefinition() == parameter.GetGenericTypeDefinition()
                ? argument.GetGenericArguments()
                : null;
        if (elem == null)
            return;
        var formals = parameter.GetGenericArguments();
        for (var i = 0; i < formals.Length && i < elem.Length; i++)
            Bind(formals[i], elem[i], map);
    }
}
