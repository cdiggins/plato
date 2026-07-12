using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// The scalar-erasure (--scalar=float) decision procedures of <see cref="CSharpFunctionBodyWriter"/>,
/// as a standalone SYMBOL-side analysis for the TIR emit path: every TIR node carries its
/// originating symbol, so consulting the same analysis over the same symbols yields byte-identical
/// decisions ("which primitive is this expression in the erased output"), while the syntax itself
/// renders from the TIR. A verbatim copy of the legacy writer's logic — it replaces that logic
/// outright when the legacy writer is retired. All methods are inert unless Writer.ScalarErase.
/// </summary>
public class ScalarEraseAnalysis
{
    private readonly CSharpTypeWriter _tw;
    private readonly Dictionary<ParameterDef, string> _paramEmittedTypes;

    private CSharpWriter Writer => _tw.Writer;

    /// <param name="tw">The type writer (receiver type + extension-plan context).</param>
    /// <param name="impl">The function whose body is being written.</param>
    /// <param name="parameterTypes">The EMITTED C# type per parameter (fi.ParameterTypes).</param>
    /// <param name="lambdaParamPrim">Lambda bodies only: the primitive the enclosing call site
    /// determined the lambda's parameters erase to (null otherwise).</param>
    public ScalarEraseAnalysis(CSharpTypeWriter tw, FunctionDef impl, IReadOnlyList<string> parameterTypes, string lambdaParamPrim)
    {
        _tw = tw;
        _paramEmittedTypes = new Dictionary<ParameterDef, string>();
        for (var i = 0; i < impl.Parameters.Count && i < parameterTypes.Count; ++i)
            _paramEmittedTypes[impl.Parameters[i]] = parameterTypes[i];
        if (lambdaParamPrim != null)
            foreach (var p in impl.Parameters)
                _paramEmittedTypes[p] = lambdaParamPrim;
    }

    /// <summary>If every overload of the referenced function group declares the SAME scalar
    /// wrapper return type, the primitive it erases to; otherwise null. Used to normalize
    /// generated bodies to "float-land": call results that would be wrapper-typed (kept members,
    /// handwritten intrinsics) are cast down to the primitive at the call site.</summary>
    public static string ScalarReturnPrimitive(Symbol function)
    {
        if (!(function is FunctionGroupRefSymbol fgr) || fgr.Def?.Functions == null || fgr.Def.Functions.Count == 0)
            return null;
        string name = null;
        foreach (var f in fgr.Def.Functions)
        {
            var rt = f.ReturnType?.Name;
            if (rt == null)
                return null;
            if (name == null)
                name = rt;
            else if (name != rt)
                return null;
        }
        return name != null && CSharpWriter.ScalarPrimitives.TryGetValue(name, out var prim) ? prim : null;
    }

    /// <summary>The primitive a scalar-typed parameter erases to, or null when the parameter is
    /// not provably scalar.</summary>
    public string ScalarPrimitiveOfParam(ParameterDef def)
    {
        if (def == null)
            return null;
        if (def.Type?.Name != null && CSharpWriter.ScalarPrimitives.TryGetValue(def.Type.Name, out var prim))
            return prim;
        if (_paramEmittedTypes.TryGetValue(def, out var emitted) && ScalarPrimitiveNames.Contains(emitted))
            return emitted;
        return null;
    }

    private static readonly HashSet<string> ScalarPrimitiveNames =
        new HashSet<string>(CSharpWriter.ScalarPrimitives.Values);

    /// <summary>Conservative "which primitive is this expression in the erased output" analysis
    /// (null = unknown). See the legacy writer for the case-by-case rationale.</summary>
    public string ScalarPrimOf(Expression e)
    {
        switch (e)
        {
            case ParameterRefSymbol pr:
                return ScalarPrimitiveOfParam(pr.Def);
            case VariableRefSymbol vr:
                return vr.Def?.Value is Expression init ? ScalarPrimOf(init) : null;
            case Literal lit:
                switch (lit.TypeEnum)
                {
                    case Ara3D.Geometry.AST.LiteralTypesEnum.Number: return "float";
                    case Ara3D.Geometry.AST.LiteralTypesEnum.Integer: return "int";
                    case Ara3D.Geometry.AST.LiteralTypesEnum.Boolean: return "bool";
                    case Ara3D.Geometry.AST.LiteralTypesEnum.String: return "string";
                    default: return null;
                }
            case FunctionCall fc:
            {
                var p = ScalarReturnPrimitive(fc.Function);
                if (p != null)
                    return p;
                if (fc.Args.Count >= 1 && fc.Function is FunctionGroupRefSymbol g)
                {
                    var rprim = ScalarPrimOf(fc.Args[0]);
                    if (rprim == null)
                        return MovedScalarReturnPrim(ReceiverPlatoTypeOf(fc.Args[0]), g.Name, fc.Args.Count);
                    for (var i = 1; i < fc.Args.Count; ++i)
                        if (ScalarPrimOf(fc.Args[i]) == null)
                            return null;
                    var overloads = MatchingScalarOverloads(g.Name, fc.Args.Count, rprim);
                    if (overloads == null || overloads.Count == 0)
                        return null;
                    string ret = null;
                    foreach (var o in overloads)
                    {
                        if (!CSharpWriter.ScalarPrimitives.TryGetValue(o.Return, out var rp))
                            return null;
                        if (ret == null)
                            ret = rp;
                        else if (ret != rp)
                            return null;
                    }
                    return ret;
                }
                return null;
            }
            case FunctionGroupRefSymbol fgr:
                if (fgr.Def?.Functions != null && fgr.Def.Functions.Count == 1
                    && fgr.Def.Functions[0].NumParameters == 0
                    && fgr.Def.Functions[0].ReturnType?.Name != null
                    && CSharpWriter.ScalarPrimitives.TryGetValue(fgr.Def.Functions[0].ReturnType.Name, out var cprim))
                    return cprim;
                return null;
            case ComponentAccessExpression ca:
                return ca.ScalarComponentPrim;
            case ConditionalExpression cond:
            {
                var a = ScalarPrimOf(cond.IfTrue);
                return a != null && a == ScalarPrimOf(cond.IfFalse) ? a : null;
            }
            default:
                return null;
        }
    }

    public bool IsScalarValued(Expression e) => ScalarPrimOf(e) != null;

    /// <summary>The concrete (non-scalar) Plato type name of a receiver expression, when
    /// determinable; null otherwise.</summary>
    public string ReceiverPlatoTypeOf(Expression e)
    {
        switch (e)
        {
            case ParameterRefSymbol pr:
            {
                var n = pr.Def?.Type?.Name;
                if (n == null || n == "Self")
                    n = _tw.TypeDef?.Name;
                if ((n == null || Writer.GetExtensionPlanByTypeName(n) == null)
                    && pr.Def != null && _paramEmittedTypes.TryGetValue(pr.Def, out var emitted))
                    n = emitted;
                return n;
            }
            case VariableRefSymbol vr:
                return vr.Def?.Value is Expression init ? ReceiverPlatoTypeOf(init) : null;
            default:
                return null;
        }
    }

    /// <summary>When every same-name same-arity candidate member of the given concrete type MOVED
    /// to an extension class and they all return the same scalar, the primitive; null otherwise.</summary>
    public string MovedScalarReturnPrim(string receiverTypeName, string name, int argc)
    {
        var plan = Writer.GetExtensionPlanByTypeName(receiverTypeName);
        if (plan == null)
            return null;
        string prim = null;
        var found = false;
        foreach (var f in plan.CandidateFunctions)
        {
            if (f.Name != name || f.ParameterTypes.Count != argc)
                continue;
            found = true;
            if (!plan.ShouldMove(f))
                return null;
            var rt = f.ReturnType?.Name;
            if (rt == null || !CSharpWriter.ScalarPrimitives.TryGetValue(rt, out var rp))
                return null;
            if (prim == null)
                prim = rp;
            else if (prim != rp)
                return null;
        }
        return found ? prim : null;
    }

    /// <summary>All-scalar-parameter overloads of a scalar member name matching the given arity
    /// and receiver primitive.</summary>
    public List<(IReadOnlyList<string> Params, string Return)> MatchingScalarOverloads(string name, int argc, string receiverPrim)
    {
        if (!Writer.ScalarOverloads.TryGetValue(name, out var list))
            return null;
        List<(IReadOnlyList<string>, string)> result = null;
        foreach (var o in list)
        {
            if (o.Params.Count != argc)
                continue;
            if (!CSharpWriter.ScalarPrimitives.TryGetValue(o.Params[0], out var rp) || rp != receiverPrim)
                continue;
            var allScalar = true;
            for (var i = 1; i < o.Params.Count; ++i)
                if (!CSharpWriter.ScalarPrimitives.ContainsKey(o.Params[i]))
                {
                    allScalar = false;
                    break;
                }
            if (!allScalar)
                continue;
            (result ?? (result = new List<(IReadOnlyList<string>, string)>())).Add(o);
        }
        return result;
    }

    /// <summary>The primitive ELEMENT type of an expression used as the receiver of a HOF taking
    /// a lambda, or null when undeterminable.</summary>
    public string ElementPrimOf(Expression e)
    {
        switch (e)
        {
            case ParameterRefSymbol pr:
            {
                string typeName = null;
                if (pr.Def != null && _paramEmittedTypes.TryGetValue(pr.Def, out var emitted))
                    typeName = emitted;
                typeName = typeName ?? pr.Def?.Type?.Name;
                if (typeName == "Self")
                    typeName = _tw.TypeDef?.Name;
                if (typeName == null)
                    return null;
                var fromList = ElementPrimOfListTypeName(typeName);
                if (fromList != null)
                    return fromList;
                var comp = Writer.GetComponentPlatoType(typeName);
                return comp != null && CSharpWriter.ScalarPrimitives.TryGetValue(comp, out var p) ? p : null;
            }
            case FunctionCall fc when fc.Function is FunctionGroupRefSymbol g:
            {
                if (g.Name == "Components" && fc.Args.Count == 1)
                {
                    if (fc.Args[0] is ParameterRefSymbol rpr)
                    {
                        var tn = rpr.Def?.Type?.Name;
                        if (tn == null && rpr.Def != null && _paramEmittedTypes.TryGetValue(rpr.Def, out var re))
                            tn = re;
                        var comp = Writer.GetComponentPlatoType(tn);
                        return comp != null && CSharpWriter.ScalarPrimitives.TryGetValue(comp, out var p) ? p : null;
                    }
                    return null;
                }
                string prim = null;
                if (g.Def?.Functions == null || g.Def.Functions.Count == 0)
                    return null;
                foreach (var f in g.Def.Functions)
                {
                    var rt = f.ReturnType;
                    if (rt?.Name == null || !rt.Name.StartsWith("IArray") || rt.TypeArgs.Count != 1)
                        return null;
                    if (!CSharpWriter.ScalarPrimitives.TryGetValue(rt.TypeArgs[0].Name, out var p))
                        return null;
                    if (prim == null)
                        prim = p;
                    else if (prim != p)
                        return null;
                }
                return prim;
            }
            default:
                return null;
        }
    }

    private static string ElementPrimOfListTypeName(string typeName)
    {
        foreach (var p in ScalarPrimitiveNames)
            if (typeName == $"IReadOnlyList<{p}>" || typeName == $"IArray<{p}>")
                return p;
        return null;
    }

    public static string WrapperOfPrim(string prim)
    {
        foreach (var kv in CSharpWriter.ScalarPrimitives)
            if (kv.Value == prim)
                return kv.Key;
        return prim;
    }

    /// <summary>Known element-wise HOF names: the lambda's parameters receive elements of the receiver.</summary>
    public static readonly HashSet<string> ElementWiseHofNames = new HashSet<string>
    {
        "MapComponents", "ZipComponents", "AllZipComponents", "AnyZipComponents",
        "AllComponents", "AnyComponent", "Reduce",
        "Map", "Zip", "Filter", "All", "Any",
        "MapPairs", "MapTriplets", "MapQuartets",
    };
}
