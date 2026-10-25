// Adapts CLR test values to generated scalar wrappers for reflection calls.
using System.Reflection;

namespace PlatoTests;

internal static class GeneratedValueReflection
{
    public static object New(Type t, params object[] args)
    {
        var ctor = t.GetConstructors().First(c => c.GetParameters().Length == args.Length);
        return ctor.Invoke(ConvertArguments(ctor.GetParameters(), args));
    }

    public static object? Call(MethodInfo m, object? receiver, params object[] args)
        => Unwrap(m.Invoke(receiver, ConvertArguments(m.GetParameters(), args)));

    private static object?[] ConvertArguments(ParameterInfo[] parameters, object[] args)
        => args
            .Select((a, i) => parameters[i].ParameterType.IsInstanceOfType(a)
                ? a
                : Activator.CreateInstance(parameters[i].ParameterType, a))
            .ToArray();

    public static object? Unwrap(object? v)
    {
        var f = v?.GetType().GetField("Value");
        return f == null ? v : f.GetValue(v);
    }
}
