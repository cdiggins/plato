using System.Globalization;
using System.Reflection;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Interpreter;

namespace Plato.Interpreter.Tests;

/// <summary>A value the interpreter produced that has no C# counterpart in the generated assembly.</summary>
public sealed class ValueConversionException : Exception
{
    public ValueConversionException(string message) : base(message) { }
}

/// <summary>
/// The bridge between the two arms: a generated C# parameter type names the Plato type the
/// interpreter generates a value for, and an interpreter value is rebuilt as the C# value by
/// reflection (wrapper scalars through their one-argument constructor, records through the
/// constructor with the most parameters, sums through the case factory, arrays as typed arrays).
/// </summary>
public sealed class DifferentialValues
{
    private const string GeneratedNamespace = "Ara3D.Geometry";
    private static readonly HashSet<string> WrapperScalars = new() { "Number", "Integer", "Boolean", "Character", "String" };

    private readonly Ara3D.Geometry.Interpreter.Interpreter _interp;
    private readonly ValueGenerator _gen;

    public DifferentialValues(Ara3D.Geometry.Interpreter.Interpreter interp)
        => (_interp, _gen) = (interp, new ValueGenerator(interp));

    /// <summary>The Plato type a generated C# parameter type stands for; null when none does.</summary>
    public TypeExpression PlatoTypeOf(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            return Instance("Array", t.GetGenericArguments());
        if (t == typeof(double) || t == typeof(float)) return Named("Number");
        if (t == typeof(int) || t == typeof(long)) return Named("Integer");
        if (t == typeof(bool)) return Named("Boolean");
        if (t == typeof(char)) return Named("Character");
        if (t == typeof(string)) return Named("String");
        if (t.Namespace != GeneratedNamespace || t.IsInterface || t.IsGenericTypeDefinition)
            return null;
        return t.IsGenericType
            ? Instance(t.Name.Substring(0, t.Name.IndexOf('`')), t.GetGenericArguments())
            : Named(t.Name);
    }

    private TypeExpression Named(string name)
        => _interp.Compilation.GetTypeDef(name)?.ToTypeExpression();

    private TypeExpression Instance(string name, Type[] args)
    {
        var def = _interp.Compilation.GetTypeDef(name);
        var mapped = args.Select(PlatoTypeOf).ToArray();
        return def == null || mapped.Any(a => a == null) ? null : new TypeExpression(def, mapped);
    }

    /// <summary>Whether the interpreter can generate values for this C# parameter type; the
    /// reason when it cannot.</summary>
    public bool CanGenerate(Type t, out string reason)
    {
        var plato = PlatoTypeOf(t);
        if (plato == null)
        {
            reason = $"no Plato type for C# {Describe(t)}";
            return false;
        }
        return _gen.CanGenerate(plato, out reason);
    }

    public object Generate(Type t, Random rng)
        => _gen.Generate(PlatoTypeOf(t), rng);

    /// <summary>The C# value of an interpreter value, as an instance of <paramref name="t"/>.</summary>
    public object ToHost(object v, Type t)
    {
        if (t.Namespace == GeneratedNamespace && WrapperScalars.Contains(t.Name))
        {
            var ctor = t.GetConstructors().Single(c => c.GetParameters().Length == 1);
            return ctor.Invoke(new[] { Convert.ChangeType(v, ctor.GetParameters()[0].ParameterType, CultureInfo.InvariantCulture) });
        }
        if (t.IsPrimitive || t == typeof(string))
            return Convert.ChangeType(v, t, CultureInfo.InvariantCulture);
        switch (v)
        {
            case PlatoArray xs when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IReadOnlyList<>):
            {
                var elem = t.GetGenericArguments()[0];
                var array = Array.CreateInstance(elem, xs.Count);
                for (var i = 0; i < xs.Count; i++)
                    array.SetValue(ToHost(xs[i], elem), i);
                return array;
            }
            case PlatoRecord r:
            {
                var ctor = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();
                if (ctor == null && r.Fields.Length == 0)
                    return Activator.CreateInstance(t);
                var ps = ctor?.GetParameters() ?? Array.Empty<ParameterInfo>();
                if (ps.Length != r.Fields.Length)
                    throw new ValueConversionException($"{r.Def.Name} has {r.Fields.Length} fields but C# {Describe(t)}'s widest constructor takes {ps.Length}");
                return ctor.Invoke(r.Fields.Select((f, i) => ToHost(f, ps[i].ParameterType)).ToArray());
            }
            case PlatoSum u:
            {
                var factory = t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == u.Case.Name && m.GetParameters().Length == u.Fields.Length)
                    ?? throw new ValueConversionException($"C# {Describe(t)} has no factory for case {u.Case.Name}/{u.Fields.Length}");
                var ps = factory.GetParameters();
                return factory.Invoke(null, u.Fields.Select((f, i) => ToHost(f, ps[i].ParameterType)).ToArray());
            }
        }
        throw new ValueConversionException($"cannot convert {ValueFormat.Format(v, 80)} to C# {Describe(t)}");
    }

    /// <summary>A generated Boolean (or a bool) as a bool.</summary>
    public static bool ToBool(object result)
    {
        if (result is bool b) return b;
        var field = result?.GetType().GetField("Value");
        if (field != null && field.FieldType == typeof(bool))
            return (bool)field.GetValue(result);
        throw new ValueConversionException($"the C# law returned {result?.GetType().Name ?? "null"}, not a Boolean");
    }

    public static bool IsBooleanLike(Type t)
        => t == typeof(bool) || (t.Namespace == GeneratedNamespace && t.Name == "Boolean");

    public static string Describe(Type t)
        => t.IsGenericType
            ? $"{t.Name.Substring(0, t.Name.IndexOf('`'))}<{string.Join(", ", t.GetGenericArguments().Select(Describe))}>"
            : t.Name;
}
