// --in-params (TKT-1066): passes large readonly structs by reference instead of copying them.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// The decisions of the <c>--in-params</c> switch (TKT-1066). Every generated struct is a
/// <c>readonly struct</c>, so reading it through a reference never changes a value; only the
/// copies go. A struct is LARGE when its flattened fields (a sum type's Kind tag plus every case's
/// fields side by side, laid out in declaration order with natural alignment) take at least
/// <see cref="ThresholdBytes"/>. Under the switch:
/// <list type="bullet">
/// <item>an element read <c>xs[i]</c> of a large element type, used as a receiver, an argument
/// or a <c>var</c> initialiser, reads through <c>RefAccess.RefAt(xs, i)</c>, a <c>ref readonly</c>
/// accessor over the backing <c>T[]</c> with a copying slow path for any other list;</item>
/// <item>an expression-bodied extension method whose whole body is such an element read returns
/// <c>ref readonly T</c> (the table accessors, <c>SurfaceAt</c>), so its callers read through it;</item>
/// <item>a <c>var</c> initialised by either becomes a <c>ref readonly var</c> local, unless a
/// lambda captures it or it is assigned;</item>
/// <item>a large parameter of an extension method, its <c>this</c> receiver included, becomes
/// <c>in T</c>, unless a lambda captures it or the body assigns it.</item>
/// </list>
/// A method group cannot convert to a delegate when its signature has <c>in</c> parameters or a
/// <c>ref</c> return, so an eta-expanded reference over a large type stays a lambda.
/// </summary>
public sealed class LargeStructPassing
{
    public const int ThresholdBytes = 256;
    public const string HelperClass = "RefAccess";
    public const string HelperFile = "RefAccess.g.cs";

    private readonly CSharpWriter _writer;
    private readonly Dictionary<TypeDef, (int Size, int Align)> _layouts = new();
    private readonly HashSet<TypeDef> _inProgress = new();
    private readonly Dictionary<string, int> _sizesByName = new();
    private readonly Dictionary<(FunctionDef, TypeDef), bool> _refAccessors = new();

    public LargeStructPassing(CSharpWriter writer)
    {
        _writer = writer;
        foreach (var c in writer.Compilation.ConcreteTypes)
        {
            var def = c.TypeDef;
            if (!IsGeneratedStruct(def) || def.TypeParameters.Count > 0 || _sizesByName.ContainsKey(def.Name))
                continue;
            _sizesByName[def.Name] = Layout(def, null).Size;
        }
    }

    /// <summary>The flattened size of every non-generic generated struct, by its C# name.</summary>
    public IReadOnlyDictionary<string, int> Sizes => _sizesByName;

    public bool IsLarge(string csharpTypeName)
        => csharpTypeName != null && _sizesByName.TryGetValue(csharpTypeName, out var size) && size >= ThresholdBytes;

    public bool IsLarge(TypeExpression type)
        => type?.Def != null && type.TypeArgs.Count == 0 && IsLarge(type.Def.Name);

    private bool IsGeneratedStruct(TypeDef def)
        => def != null && !def.IsUnique && !CSharpWriter.IgnoredTypes.Contains(def.Name)
           && !CSharpWriter.PrimitiveTypes.ContainsKey(def.Name)
           && _writer.ExtensionPlans != null && _writer.ExtensionPlans.ContainsKey(def);

    // --- sizes ---------------------------------------------------------------------------------

    private (int Size, int Align) Layout(TypeExpression type, IReadOnlyDictionary<string, TypeExpression> bindings)
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
        Dictionary<string, TypeExpression> args = null;
        if (type.Def.TypeParameters.Count > 0 && type.TypeArgs.Count == type.Def.TypeParameters.Count)
        {
            args = new Dictionary<string, TypeExpression>();
            for (var i = 0; i < type.TypeArgs.Count; i++)
                args[type.Def.TypeParameters[i].Name] = type.TypeArgs[i];
        }
        return Layout(type.Def, args);
    }

    private static (int Size, int Align) Scalar(int bytes) => (bytes, bytes);

    private (int Size, int Align) Layout(TypeDef def, IReadOnlyDictionary<string, TypeExpression> bindings)
    {
        var cacheable = bindings == null;
        if (cacheable && _layouts.TryGetValue(def, out var known))
            return known;
        // A struct cannot contain itself; a cycle can only pass through a reference.
        if (!_inProgress.Add(def))
            return (8, 8);
        var fields = def.IsSum
            ? new[] { (4, 4) }.Concat(def.Cases.SelectMany(c => c.Fields).Select(f => Layout(f.Type, bindings)))
            : def.Fields.Select(f => Layout(f.Type, bindings));
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

    private static int Round(int n, int a) => (n + a - 1) / a * a;

    // --- element reads ------------------------------------------------------------------------

    /// <summary>An <c>At(xs, i)</c> over a runtime list (not a generated struct's own At) whose
    /// element type is large: the read <see cref="TirCSharpBodyWriter"/> writes as
    /// <c>xs[i]</c>.</summary>
    public bool IsLargeElementRead(TirNode node, out TirCall read)
    {
        read = node as TirCall;
        if (read == null || read.Name != "At" || read.Args.Count != 2 || !IsLarge(read.Type))
            return false;
        var receiver = TirRewrite.StripCoerce(read.Args[0])?.Type;
        return receiver?.Def != null && receiver.Def.Name == "Array" && receiver.TypeArgs.Count == 1
               && receiver.TypeArgs[0].Def == read.Type.Def;
    }

    /// <summary>Whether a body is one large element read whose type is the declared return type,
    /// the shape <see cref="IsRefAccessor(FunctionDef, TypeDef)"/> requires.</summary>
    public bool HasRefAccessorShape(TirFunction tir, CSharpFunctionInfo fi)
        => tir?.Body != null && fi != null && fi.EmitAsMethod && IsLarge(fi.ReturnType)
           && IsLargeElementRead(tir.Body, out var read) && read.Type.Def.Name == fi.ReturnType;

    /// <summary>Whether the extension member <paramref name="function"/> moved off
    /// <paramref name="receiver"/> returns <c>ref readonly</c>. Decided once, from its ground TIR
    /// before the optimizer passes, so a declaration and its call sites cannot disagree.</summary>
    public bool IsRefAccessor(FunctionDef function, TypeDef receiver)
    {
        if (function == null || receiver == null || _writer.ExtensionPlans == null
            || !_writer.ExtensionPlans.TryGetValue(receiver, out var plan))
            return false;
        var key = (function, receiver);
        if (_refAccessors.TryGetValue(key, out var known))
            return known;
        var moved = plan.MovedFunctions.FirstOrDefault(f => ReferenceEquals(f.Implementation, function));
        var result = moved != null && !plan.IsClosedInstanceMove(moved)
            && HasRefAccessorShape(_writer.TryGetGroundTir(function, receiver),
                new CSharpTypeWriter(_writer, receiver).ToFunctionInfo(moved, receiver));
        _refAccessors[key] = result;
        return result;
    }

    /// <summary>Whether a call binds to a ref readonly accessor (<see cref="IsRefAccessor(FunctionDef, TypeDef)"/>).</summary>
    public bool ReturnsRef(TirCall call)
        => call?.Callee != null && call.Args.Count > 0 && IsLarge(call.Type)
           && IsRefAccessor(call.Callee, TirRewrite.StripCoerce(call.Args[0])?.Type?.Def);

    // --- parameters and locals ----------------------------------------------------------------

    /// <summary>The parameter modifiers of a moved extension member: <c>"in "</c> for a large
    /// parameter that no lambda captures and nothing assigns, <c>""</c> otherwise.</summary>
    public IReadOnlyList<string> ParameterModifiers(CSharpFunctionInfo fi, TirFunction tir)
    {
        var unsafeParams = new HashSet<ParameterDef>();
        if (tir?.Body != null)
        {
            foreach (var lambda in tir.Body.Descendants().OfType<TirLambda>())
                foreach (var p in lambda.Body?.Descendants().OfType<TirParameter>() ?? Enumerable.Empty<TirParameter>())
                    if (p.Def != null)
                        unsafeParams.Add(p.Def);
            foreach (var assign in tir.Body.Descendants().OfType<TirAssign>())
                if (TirRewrite.StripCoerce(assign.LValue) is TirParameter p && p.Def != null)
                    unsafeParams.Add(p.Def);
        }
        var modifiers = new List<string>();
        for (var i = 0; i < fi.ParameterTypes.Count; i++)
        {
            var def = tir != null && i < tir.Parameters.Count ? tir.Parameters[i] : null;
            modifiers.Add(def != null && IsLarge(fi.ParameterTypes[i]) && !unsafeParams.Contains(def) ? "in " : "");
        }
        return modifiers;
    }

    /// <summary>The locals of a body that may not become <c>ref readonly</c>: those a lambda
    /// captures (a ref local cannot be captured) and those assigned after their declaration.</summary>
    public static HashSet<VariableDef> NonRefLocals(TirNode body)
    {
        var result = new HashSet<VariableDef>();
        if (body == null)
            return result;
        foreach (var lambda in body.Descendants().OfType<TirLambda>())
        {
            var own = new HashSet<VariableDef>(lambda.Body?.Descendants().OfType<TirLet>().Select(l => l.Def)
                                               ?? Enumerable.Empty<VariableDef>());
            foreach (var v in lambda.Body?.Descendants().OfType<TirVariable>() ?? Enumerable.Empty<TirVariable>())
                if (v.Def != null && !own.Contains(v.Def))
                    result.Add(v.Def);
        }
        foreach (var assign in body.Descendants().OfType<TirAssign>())
            if (TirRewrite.StripCoerce(assign.LValue) is TirVariable v && v.Def != null)
                result.Add(v.Def);
        return result;
    }

    /// <summary>Whether an eta-expanded member reference must stay a lambda: a method group whose
    /// signature has an <c>in</c> parameter or a <c>ref</c> return does not convert to
    /// <c>System.Func</c>.</summary>
    public bool KeepsEtaLambda(TirLambda lambda)
    {
        if (lambda.Body is not TirCall call)
            return false;
        return IsLarge(call.Type)
               || call.Args.Any(a => IsLarge(TirRewrite.StripCoerce(a)?.Type))
               || call.ParameterTypes.Any(t => IsLarge(t));
    }

    // --- the accessor ---------------------------------------------------------------------------

    /// <summary>The body of <see cref="HelperFile"/>, written only under the switch so the output
    /// without it is unchanged.</summary>
    public static void WriteHelper(CSharpWriter writer)
    {
        writer.WriteLine("// --in-params (TKT-1066): a ref readonly read of a list element, so a large readonly");
        writer.WriteLine("// struct is not copied out of its table. Reads the backing T[] when there is one; any");
        writer.WriteLine("// other list is read by value into a box, which yields the same value.");
        writer.WriteLine($"public static class {HelperClass}");
        writer.WriteStartBlock();
        writer.WriteLine("[MethodImpl(AggressiveInlining)] public static ref readonly T RefAt<T>(System.Collections.Generic.IReadOnlyList<T> xs, int i) where T : struct");
        writer.WriteStartBlock();
        writer.WriteLine("if (xs is T[] array)");
        writer.WriteLine("    return ref array[i];");
        writer.WriteLine("return ref RefAtCopy(xs, i);");
        writer.WriteEndBlock();
        writer.WriteLine("[MethodImpl(NoInlining)] private static ref readonly T RefAtCopy<T>(System.Collections.Generic.IReadOnlyList<T> xs, int i) where T : struct");
        writer.WriteLine("    => ref new StrongBox<T>(xs[i]).Value;");
        writer.WriteEndBlock();
    }
}
