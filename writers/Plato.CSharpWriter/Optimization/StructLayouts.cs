// Flattened struct layouts and the unmanaged test, shared by --in-params and the stack-array storage.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// What the writer can say about the C# struct a Plato type becomes, without asking the C# compiler.
/// <para>
/// <b>Layout.</b> A generated struct's size is its fields in declaration order with natural
/// alignment (a sum type's <c>Kind</c> tag, 4 bytes, then every case's fields side by side). A
/// scalar takes its profile's width; anything that is not a generated struct (string, array, list,
/// delegate, interface, unbound type variable) is one 8-byte pointer. The figure is the writer's
/// estimate, used for thresholds, not the CLR's <c>sizeof</c>.
/// </para>
/// <para>
/// <b>Unmanaged.</b> A type is unmanaged when it is <c>Number</c>, <c>Integer</c>, <c>Boolean</c> or
/// <c>Character</c>, or a generated struct all of whose fields are unmanaged, recursively. A
/// reference type, array, list, interface, delegate, string or type variable makes it managed, as
/// does a cycle (which can only pass through a reference). Only an unmanaged type may be
/// <c>stackalloc</c>ed.
/// </para>
/// Valid once <see cref="CSharpWriter.ExtensionPlans"/> exists (after the writer has planned).
/// </summary>
public sealed class StructLayouts
{
    private readonly CSharpWriter _writer;
    private readonly Dictionary<TypeDef, (int Size, int Align)> _layouts = new();
    private readonly HashSet<TypeDef> _inProgress = new();
    private readonly Dictionary<TypeDef, bool> _unmanaged = new();
    private readonly HashSet<TypeDef> _unmanagedInProgress = new();

    public StructLayouts(CSharpWriter writer) => _writer = writer;

    /// <summary>Whether the type becomes a struct the writer generates (not a primitive, ignored,
    /// unique or unplanned type).</summary>
    public bool IsGeneratedStruct(TypeDef def)
        => def != null && !def.IsUnique && !CSharpWriter.IgnoredTypes.Contains(def.Name)
           && !CSharpWriter.PrimitiveTypes.ContainsKey(def.Name)
           && _writer.ExtensionPlans != null && _writer.ExtensionPlans.ContainsKey(def);

    /// <summary>The flattened size in bytes of <paramref name="type"/>.</summary>
    public int SizeOf(TypeExpression type) => Layout(type, null).Size;

    /// <summary>The flattened size in bytes of a non-generic (or unbound generic) definition.</summary>
    public int SizeOf(TypeDef def) => Layout(def, null).Size;

    /// <summary>Whether <paramref name="type"/> contains no reference. An unbound type variable is
    /// not unmanaged; a generic instance is judged with its arguments bound.</summary>
    public bool IsUnmanaged(TypeExpression type) => IsUnmanaged(type, null);

    /// <summary>Whether a definition is unmanaged; a generic definition is not, as its parameters are unbound.</summary>
    public bool IsUnmanaged(TypeDef def) => IsUnmanaged(def, null);

    // --- layout --------------------------------------------------------------------------------

    public (int Size, int Align) Layout(TypeExpression type, IReadOnlyDictionary<string, TypeExpression> bindings)
    {
        if (type?.Def == null)
            return (8, 8);
        if ((type.IsTypeParameter || type.IsTypeVariable) && bindings != null
            && bindings.TryGetValue(type.Def.Name, out var bound))
            return Layout(bound, null);
        switch (type.Def.Name)
        {
            case "Number": return Scalar(_writer.Profile.NumberBits / 8);
            case "Integer": return Scalar(_writer.Profile.IntegerBits / 8);
            case "Boolean": return (1, 1);
            case "Character": return (2, 2);
        }
        // A reference (string, array, list, delegate, interface, builder class) or an unknown type
        // variable: one pointer.
        if (!IsGeneratedStruct(type.Def))
            return (8, 8);
        return Layout(type.Def, Bind(type));
    }

    public (int Size, int Align) Layout(TypeDef def, IReadOnlyDictionary<string, TypeExpression> bindings)
    {
        var cacheable = bindings == null;
        if (cacheable && _layouts.TryGetValue(def, out var known))
            return known;
        // A struct cannot contain itself; a cycle can only pass through a reference.
        if (!_inProgress.Add(def))
            return (8, 8);
        var fields = FieldTypes(def).Select(t => Layout(t, bindings));
        if (def.IsSum)
            fields = new[] { (4, 4) }.Concat(fields);
        int offset = 0, align = 1;
        foreach (var (size, a) in fields)
        {
            offset = Round(offset, a) + size;
            align = System.Math.Max(align, a);
        }
        _inProgress.Remove(def);
        var result = (System.Math.Max(Round(offset, align), 1), align);
        if (cacheable)
            _layouts[def] = result;
        return result;
    }

    // --- unmanaged -----------------------------------------------------------------------------

    private bool IsUnmanaged(TypeExpression type, IReadOnlyDictionary<string, TypeExpression> bindings)
    {
        if (type?.Def == null)
            return false;
        if ((type.IsTypeParameter || type.IsTypeVariable) && bindings != null
            && bindings.TryGetValue(type.Def.Name, out var bound))
            return IsUnmanaged(bound, null);
        switch (type.Def.Name)
        {
            case "Number":
            case "Integer":
            case "Boolean":
            case "Character":
                return true;
        }
        if (!IsGeneratedStruct(type.Def))
            return false;
        return IsUnmanaged(type.Def, Bind(type));
    }

    private bool IsUnmanaged(TypeDef def, IReadOnlyDictionary<string, TypeExpression> bindings)
    {
        var cacheable = bindings == null;
        if (cacheable && _unmanaged.TryGetValue(def, out var known))
            return known;
        if (!_unmanagedInProgress.Add(def))
            return false;
        var result = IsGeneratedStruct(def) && FieldTypes(def).All(t => IsUnmanaged(t, bindings));
        _unmanagedInProgress.Remove(def);
        if (cacheable)
            _unmanaged[def] = result;
        return result;
    }

    // --- shared ----------------------------------------------------------------------------------

    private static IEnumerable<TypeExpression> FieldTypes(TypeDef def)
        => def.IsSum
            ? def.Cases.SelectMany(c => c.Fields).Select(f => f.Type)
            : def.Fields.Select(f => f.Type);

    private static Dictionary<string, TypeExpression> Bind(TypeExpression type)
    {
        if (type.Def.TypeParameters.Count == 0 || type.TypeArgs.Count != type.Def.TypeParameters.Count)
            return null;
        var args = new Dictionary<string, TypeExpression>();
        for (var i = 0; i < type.TypeArgs.Count; i++)
            args[type.Def.TypeParameters[i].Name] = type.TypeArgs[i];
        return args;
    }

    private static (int Size, int Align) Scalar(int bytes) => (bytes, bytes);

    private static int Round(int n, int a) => (n + a - 1) / a * a;
}
