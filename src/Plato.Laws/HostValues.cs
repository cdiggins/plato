// Converts values between interpreter representations and generated C# runtime types.
using System.Collections;
using System.Globalization;
using System.Reflection;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Interpreter;

namespace Ara3D.Geometry.Laws;

/// <summary>A value that has no counterpart on the other side of the bridge.</summary>
public sealed class ValueConversionException : Exception
{
    public ValueConversionException(string message) : base(message) { }
}

/// <summary>
/// The bridge between interpreter values and the values of a <see cref="CSharpProgram"/>. A
/// generated C# type names the Plato type it stands for and back; an interpreter value is
/// rebuilt as the C# value by reflection (wrapper scalars through their one-argument
/// constructor, records through the constructor with the most parameters, sums through the case
/// factory, arrays as typed arrays), and a C# value is read back into an interpreter value
/// (wrapper scalars from their <c>Value</c> field, records field by field, sums from <c>Kind</c>
/// and the flattened case fields) so a report formats it as the interpreter would.
/// </summary>
public sealed class HostValues
{
    private const string Namespace = CSharpProgram.Namespace;
    private static readonly HashSet<string> WrapperScalars = new() { "Number", "Integer", "Boolean", "Character", "String" };

    private readonly Interpreter.Interpreter _interp;
    private readonly Assembly _assembly;
    private readonly ValueGenerator _gen;

    public HostValues(Interpreter.Interpreter interp, Assembly assembly)
        => (_interp, _assembly, _gen) = (interp, assembly, new ValueGenerator(interp));

    // --- types ---------------------------------------------------------------

    /// <summary>The Plato type a generated C# type stands for; null when none does.</summary>
    public TypeExpression PlatoTypeOf(Type t)
    {
        if (t.IsArray)
            return Instance("Array", new[] { t.GetElementType() });
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            return Instance("Array", t.GetGenericArguments());
        if (t == typeof(double) || t == typeof(float)) return Named("Number");
        if (t == typeof(int) || t == typeof(long)) return Named("Integer");
        if (t == typeof(bool)) return Named("Boolean");
        if (t == typeof(char)) return Named("Character");
        if (t == typeof(string)) return Named("String");
        if (t.Namespace != Namespace || t.IsInterface || t.IsGenericTypeDefinition)
            return null;
        return t.IsGenericType
            ? Instance(t.Name.Substring(0, t.Name.IndexOf('`')), t.GetGenericArguments())
            : Named(t.Name);
    }

    /// <summary>The generated C# type of a Plato type (<c>Array&lt;T&gt;</c> as
    /// <c>IReadOnlyList&lt;T&gt;</c>); null when the program has none.</summary>
    public Type HostType(TypeExpression t)
    {
        if (t?.Def == null)
            return null;
        if (t.Def.Name == "Array")
        {
            var elem = t.TypeArgs.Count == 1 ? HostType(t.TypeArgs[0]) : null;
            return elem == null ? null : typeof(IReadOnlyList<>).MakeGenericType(elem);
        }
        if (t.TypeArgs.Count == 0)
            return _assembly.GetType($"{Namespace}.{t.Def.Name}");
        var args = t.TypeArgs.Select(HostType).ToArray();
        var open = _assembly.GetType($"{Namespace}.{t.Def.Name}`{args.Length}");
        return open == null || args.Any(a => a == null) ? null : open.MakeGenericType(args);
    }

    private TypeExpression Named(string name)
        => _interp.Compilation.GetTypeDef(name)?.ToTypeExpression();

    private TypeExpression Instance(string name, Type[] args)
    {
        var def = _interp.Compilation.GetTypeDef(name);
        var mapped = args.Select(PlatoTypeOf).ToArray();
        return def == null || mapped.Any(a => a == null) ? null : new TypeExpression(def, mapped);
    }

    /// <summary>Whether the interpreter can generate values for this C# type; the reason when not.</summary>
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

    // --- interpreter value to host value ---------------------------------------

    /// <summary>The C# value of an interpreter value. <paramref name="target"/>, when given, is
    /// the C# type the value is for (a primitive where the C# side wants one); otherwise the
    /// value's own type decides. A <see cref="TypeToken"/> passes through: it stands for a
    /// receiver the generated method does not take.</summary>
    public object ToHost(object v, Type target = null)
    {
        switch (v)
        {
            case null:
            case TypeToken:
                return v;
            case double: case long: case bool: case char: case string:
                if (target != null && (target.IsPrimitive || target == typeof(string)))
                    return Convert.ChangeType(v, target, CultureInfo.InvariantCulture);
                return Wrap(target != null && target.Namespace == Namespace && WrapperScalars.Contains(target.Name) ? target : WrapperOf(v), v);
            case PlatoArray xs:
            {
                var elem = target != null && target.IsGenericType && target.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                    ? target.GetGenericArguments()[0]
                    : xs.ElementType != null ? HostType(xs.ElementType) : null;
                if (elem == null)
                    throw new ValueConversionException($"no C# element type for {ValueFormat.Format(v, 80)}");
                var array = Array.CreateInstance(elem, xs.Count);
                for (var i = 0; i < xs.Count; i++)
                    array.SetValue(ToHost(xs[i], elem), i);
                return array;
            }
            case PlatoRecord r:
            {
                var t = HostType(r.Type) ?? throw new ValueConversionException($"no C# type for {r.Type}");
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
                var t = HostType(u.Type) ?? throw new ValueConversionException($"no C# type for {u.Type}");
                var factory = t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == u.Case.Name && m.GetParameters().Length == u.Fields.Length)
                    ?? throw new ValueConversionException($"C# {Describe(t)} has no factory for case {u.Case.Name}/{u.Fields.Length}");
                var ps = factory.GetParameters();
                return factory.Invoke(null, u.Fields.Select((f, i) => ToHost(f, ps[i].ParameterType)).ToArray());
            }
            case PlatoBuilder:
                throw new ValueConversionException("an affine builder has no C# value");
        }
        // Already a host value (a witness the program built).
        return v;
    }

    private Type WrapperOf(object v)
    {
        var name = v switch { double => "Number", long => "Integer", bool => "Boolean", char => "Character", _ => "String" };
        return _assembly.GetType($"{Namespace}.{name}") ?? throw new ValueConversionException($"the program has no {name} type");
    }

    private static object Wrap(Type wrapper, object v)
    {
        var ctor = wrapper.GetConstructors().Single(c => c.GetParameters().Length == 1);
        return ctor.Invoke(new[] { Convert.ChangeType(v, ctor.GetParameters()[0].ParameterType, CultureInfo.InvariantCulture) });
    }

    // --- host value to interpreter value ---------------------------------------

    /// <summary>Levels of nesting read back before a value is left as it is: a report needs the
    /// outer shape of a witness, not a bottomless one.</summary>
    public const int MaxReadDepth = 24;

    /// <summary>The interpreter value of a C# value; the C# value itself when it has none (a
    /// report then prints its own ToString).</summary>
    public object ToPlato(object host) => ToPlato(host, 0);

    private object ToPlato(object host, int depth)
    {
        if (depth > MaxReadDepth)
            return host;
        switch (host)
        {
            case null: return null;
            case double: case long: case bool: case char: case string: return host;
            case float x: return (double)x;
            case int x: return (long)x;
            case short x: return (long)x;
            case byte x: return (long)x;
            case TypeToken: return host;
        }
        var t = host.GetType();
        if (t.Namespace == Namespace && WrapperScalars.Contains(t.Name))
        {
            // String's payload is a property over a private field (Plato.Intrinsics/String.cs);
            // the other wrappers keep a public field.
            var value = (object)t.GetField("Value") ?? t.GetProperty("Value");
            return value switch
            {
                FieldInfo f => ToPlato(f.GetValue(host), depth + 1),
                PropertyInfo p => ToPlato(p.GetValue(host), depth + 1),
                _ => host,
            };
        }
        if (host is IEnumerable items && host is not string && ElementTypeOf(t) is Type elem)
        {
            var list = items.Cast<object>().Select(x => ToPlato(x, depth + 1)).ToList();
            return new PlatoArray(PlatoTypeOf(elem), list);
        }
        if (t.Namespace != Namespace || !t.IsValueType)
            return host;
        var plato = PlatoTypeOf(t);
        if (plato?.Def == null)
            return host;
        if (plato.Def.IsSum)
        {
            if (t.GetField("Kind")?.GetValue(host) is not int kind)
                return host;
            var sumCase = plato.Def.Cases.FirstOrDefault(c => c.Tag == kind);
            if (sumCase == null)
                return host;
            var fields = new object[sumCase.Fields.Count];
            for (var i = 0; i < fields.Length; i++)
            {
                var f = t.GetField(sumCase.Fields[i].FlatName);
                if (f == null) return host;
                fields[i] = ToPlato(f.GetValue(host), depth + 1);
            }
            return new PlatoSum(plato, sumCase, fields);
        }
        // Fields only: a same-named property could be computed and return the type itself.
        var values = new object[plato.Def.Fields.Count];
        for (var i = 0; i < values.Length; i++)
        {
            var field = t.GetField(plato.Def.Fields[i].Name);
            if (field == null) return host;
            values[i] = ToPlato(field.GetValue(host), depth + 1);
        }
        return new PlatoRecord(plato, values);
    }

    private static Type ElementTypeOf(Type t)
    {
        if (t.IsArray)
            return t.GetElementType();
        return t.GetInterfaces().Prepend(t)
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            ?.GetGenericArguments()[0];
    }

    // --- helpers ---------------------------------------------------------------

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
        => t == typeof(bool) || (t.Namespace == Namespace && t.Name == "Boolean");

    public static string Describe(Type t)
        => t.IsGenericType
            ? $"{t.Name.Substring(0, t.Name.IndexOf('`'))}<{string.Join(", ", t.GetGenericArguments().Select(Describe))}>"
            : t.Name;
}
