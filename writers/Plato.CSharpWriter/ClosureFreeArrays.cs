// The copy-shaped Array functions written as block copies (--closure-free, TKT-1149).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// <c>Append</c>, <c>Prepend</c>, <c>Concatenate</c>, <c>Take</c>, <c>Skip</c>, <c>TakeLast</c>, <c>SubArray</c>,
/// <c>Slice</c> and <c>SetAt</c> are each a <c>MapRange</c> whose callback picks a source element by index
/// (<c>primitives.library.plato</c>). Their generic C# bodies cannot be loop-lowered (the element type is a
/// type variable), so each call allocated a closure and called a <c>Func</c> once per element. With
/// <c>--closure-free</c> the extension's body is one call to <c>ClosureFreeArrays</c>, a generated class that
/// allocates the result once and copies with <c>Array.Copy</c>.
///
/// The Plato bodies stay the definition and the interpreter runs them. The helper reproduces them element for
/// element: the same length, the same elements, an empty array for a count of zero or less, and, for a window
/// outside the source (which the library leaves as a precondition), the same per-element read of the source
/// that threw before, so the exception and the element that raised it are unchanged.
///
/// Why intrinsics at the extension rather than the loop lowerer recognising the <c>MapRange</c> bodies: the
/// lowerer works at a call site and needs the element type to be a concrete C# type. The shapes here are
/// matched by what the function is (library, name, parameter types), not by the shape of an expression, so
/// they cannot drift when a body is reworded and nothing is guessed from a lambda.
/// </summary>
public static class ClosureFreeArrays
{
    public const string ClassName = "ClosureFreeArrays";

    // Function name -> the parameter types it must have (the library's own signature; $ is the element type).
    private static readonly Dictionary<string, string[]> Shapes = new()
    {
        ["Append"] = new[] { "Array", "$" },
        ["Prepend"] = new[] { "Array", "$" },
        ["SetAt"] = new[] { "Array", "Integer", "$" },
        ["Concatenate"] = new[] { "Array", "Array" },
        ["Take"] = new[] { "Array", "Integer" },
        ["Skip"] = new[] { "Array", "Integer" },
        ["TakeLast"] = new[] { "Array", "Integer" },
        ["SubArray"] = new[] { "Array", "Integer", "Integer" },
        ["Slice"] = new[] { "Array", "Integer", "Integer" },
    };

    /// <summary>The expression body for an extension that has a block-copy form, or null. Matches the
    /// function the Primitives library declares on an Array receiver, by name and parameter types.</summary>
    public static string TryBody(CSharpFunctionInfo f)
    {
        var impl = f?.Function?.Implementation;
        if (impl?.Body == null || impl.OwnerType?.Name != "Primitives" || !Shapes.TryGetValue(impl.Name, out var shape))
            return null;
        var types = impl.Parameters;
        if (types.Count != shape.Length || f.ParameterNames.Count != shape.Length)
            return null;
        for (var i = 0; i < shape.Length; i++)
        {
            var name = types[i].Type?.Name;
            if (shape[i] == "$" ? name == null || !name.StartsWith("$") : name != shape[i])
                return null;
        }
        var args = f.ParameterNames.Select((p, i) => shape[i] == "Integer" ? $"(int){p}.Value" : p);
        return $" => {ClassName}.{impl.Name}({string.Join(", ", args)});";
    }

    /// <summary>The source of the helper class, written only under the flag.</summary>
    public static string[] Source => new[]
    {
        "public static class " + ClassName,
        "{",
        "    // The window of count elements of xs from index `from`, as a new array. A window inside xs is one block",
        "    // copy; one outside it is read element by element, as the MapRange it replaces read it, so it faults",
        "    // at the same element.",
        "    public static IReadOnlyList<T> Window<T>(IReadOnlyList<T> xs, int from, int count)",
        "    {",
        "        if (count <= 0)",
        "            return System.Array.Empty<T>();",
        "        var items = new T[count];",
        "        if (from >= 0 && (long)from + count <= xs.Count)",
        "            CopyInto(xs, from, items, 0, count);",
        "        else",
        "            for (var i = 0; i < count; i++)",
        "                items[i] = xs[from + i];",
        "        return items;",
        "    }",
        "",
        "    [MethodImpl(AggressiveInlining)]",
        "    private static void CopyInto<T>(IReadOnlyList<T> xs, int from, T[] items, int at, int count)",
        "    {",
        "        if (xs is T[] array)",
        "            System.Array.Copy(array, from, items, at, count);",
        "        else if (xs is System.Collections.Generic.List<T> list)",
        "            list.CopyTo(from, items, at, count);",
        "        else if (xs is FrozenArray<T> frozen)",
        "            for (var i = 0; i < count; i++)",
        "                items[at + i] = frozen[from + i];",
        "        else",
        "            for (var i = 0; i < count; i++)",
        "                items[at + i] = xs[from + i];",
        "    }",
        "",
        "    public static IReadOnlyList<T> Take<T>(IReadOnlyList<T> xs, int n) => Window(xs, 0, n);",
        "    public static IReadOnlyList<T> Skip<T>(IReadOnlyList<T> xs, int n) => Window(xs, n, xs.Count - n);",
        "    public static IReadOnlyList<T> TakeLast<T>(IReadOnlyList<T> xs, int n) => Window(xs, xs.Count - n, n);",
        "    public static IReadOnlyList<T> SubArray<T>(IReadOnlyList<T> xs, int from, int count) => Window(xs, from, count);",
        "    public static IReadOnlyList<T> Slice<T>(IReadOnlyList<T> xs, int from, int exclusiveTo) => Window(xs, from, exclusiveTo - from);",
        "",
        "    public static IReadOnlyList<T> Append<T>(IReadOnlyList<T> xs, T value)",
        "    {",
        "        var n = xs.Count;",
        "        var items = new T[n + 1];",
        "        CopyInto(xs, 0, items, 0, n);",
        "        items[n] = value;",
        "        return items;",
        "    }",
        "",
        "    public static IReadOnlyList<T> Prepend<T>(IReadOnlyList<T> xs, T value)",
        "    {",
        "        var n = xs.Count;",
        "        var items = new T[n + 1];",
        "        items[0] = value;",
        "        CopyInto(xs, 0, items, 1, n);",
        "        return items;",
        "    }",
        "",
        "    public static IReadOnlyList<T> Concatenate<T>(IReadOnlyList<T> xs, IReadOnlyList<T> ys)",
        "    {",
        "        var n = xs.Count;",
        "        var m = ys.Count;",
        "        if (n + m <= 0)",
        "            return System.Array.Empty<T>();",
        "        var items = new T[n + m];",
        "        CopyInto(xs, 0, items, 0, n);",
        "        CopyInto(ys, 0, items, n, m);",
        "        return items;",
        "    }",
        "",
        "    public static IReadOnlyList<T> SetAt<T>(IReadOnlyList<T> xs, int index, T value)",
        "    {",
        "        var n = xs.Count;",
        "        if (n <= 0)",
        "            return System.Array.Empty<T>();",
        "        var items = new T[n];",
        "        CopyInto(xs, 0, items, 0, n);",
        "        if ((uint)index < (uint)n)",
        "            items[index] = value;",
        "        return items;",
        "    }",
        "}",
    };
}
