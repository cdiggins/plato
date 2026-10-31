// --cache-constants (TKT-1095): a nullary static whose body is a closed expression reads a cached field.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    /// <summary>
    /// Plato is pure, so a function with no parameters whose body is a closed expression (no
    /// parameter, local, lambda, array literal or statement) always returns the same value. With
    /// --cache-constants such a body is written once, as the initialiser of a static readonly
    /// field in a nested holder class, and the function returns the field:
    /// <code>public static Number RoundingUnit() => _Const_RoundingUnit.Value; private static class _Const_RoundingUnit { static _Const_RoundingUnit() { } internal static readonly Number Value = …; }</code>
    /// One holder per constant: a holder initialises only on the first read of its field, and the
    /// explicit static constructor keeps the type precise (not beforefieldinit), so the expression
    /// is evaluated when the first call would have evaluated it and never earlier. A constant that
    /// reads another constant reads that one's holder, which initialises first, so no
    /// initialisation order inside one class can expose a default value. The same expression
    /// is evaluated by the same code, so the value is bit-identical. Laws (Law_*), generic
    /// functions, function-valued results, and bare literals are left as they are. An array-valued
    /// constant is cached as well: a cached array is shared by every caller, which is safe because
    /// Plato values are immutable and --in-place writes only into an array the body proved it owns
    /// (never a call's result).
    /// </summary>
    private bool TryWriteCachedConstant(TirNode body)
    {
        // A type-level member (`_` receiver) is written as a member body but emitted static.
        if (!_tw.Writer.CacheConstants || _lambdaDepth > 0 || _fi == null || !(_isStatic || _fi.IsStatic))
            return false;
        // No parameters, or only the ignored type-level receiver (`RoundingUnit(_: Number)`), which
        // the static is emitted without.
        var original = _tir.Original;
        if (original == null || original.NumParameters > 1
            || original.NumParameters == 1 && original.GetParameterName(0) != "_")
            return false;
        if (_fi.Name.StartsWith("Law_") || _fi.AllGenerics.Count > 0 || _fi.TypeGenerics.Count > 0)
            return false;
        if (_fi.ReturnType == null || _fi.ReturnType.Contains("Func<"))
            return false;
        if (!IsClosedConstant(body) || !body.Descendants().Any(n => n is TirCall or TirNew or TirConstructorCall))
            return false;

        var holder = $"_Const_{_fi.Name}";
        Write($" => {holder}.Value; private static class {holder} {{ static {holder}() {{ }} internal static readonly {_fi.ReturnType} Value = ");
        _holderBodyDepth++;
        WriteNode(body);
        _holderBodyDepth--;
        Write("; }");
        return true;
    }

    // --- closed sub-expressions (TKT-1095, the extension of this chunk) ----------------------------

    private sealed record DeferredConstant(string Holder, string Type, TirNode Node);

    private readonly List<DeferredConstant> _deferredConstants = new();
    private readonly Dictionary<string, string> _constantHolders = new();
    private int _holderBodyDepth;

    /// <summary>
    /// With --cache-constants, a call whose arguments are all closed (literals, type references and
    /// closed calls, nothing that names a parameter, a local or a lambda) has the same value every
    /// time it is evaluated, wherever it stands: in a loop body, a lambda, or a function called once
    /// per row. It is written once, as the initialiser of a lazily initialised holder class next to
    /// the member (the same holder shape as a whole-body constant), and the site reads its field.
    /// `Part21File.SupportedSchema().BuildSchemaIndex()` is the case. The largest closed expression
    /// is cached, not its parts; two equal expressions in one member share a holder. It refuses an
    /// expression that has no call into a Plato-defined function (closed arithmetic on literals is
    /// the JIT's to fold), a Law, a generic member, a function-valued or unresolved type, and a call
    /// that names any parameter or local, however constant it is in practice: the rule proves
    /// constancy from the syntax, so `EntityAt(index, "PLANE")` with `index` a parameter stays a call
    /// (the hoister moves it out of a loop where `index` is invariant). A closed call that faults
    /// faults in the holder's initialiser, so a later call reports a TypeInitializationException.
    /// </summary>
    private bool TryWriteCachedClosedCall(TirCall call)
    {
        if (!_tw.Writer.CacheConstants || _holderBodyDepth > 0 || _fi == null || _fi.Name.StartsWith("Law_")
            || _fi.AllGenerics.Count > 0 || _fi.TypeGenerics.Count > 0 || call.Type == null)
            return false;
        // A bare nullary call reads the callee, which caches its own closed body.
        if (call.Args.Count == 0 || !IsClosedConstant(call) || !call.Descendants().Any(d => d is TirCall { Callee.Body: not null }))
            return false;
        var type = CSharpTypeName(call.Type);
        if (type == null || type.Contains("Func<") || type.Contains('$') || type.Contains("<>"))
            return false;

        var key = $"{type}|{ClosedKey(call)}";
        if (!_constantHolders.TryGetValue(key, out var holder))
        {
            holder = $"_Const{_tw.Writer.CachedConstantHolders++}";
            _constantHolders.Add(key, holder);
            _deferredConstants.Add(new DeferredConstant(holder, type, call));
        }
        _tw.Writer.CachedConstantSites++;
        Write($"{holder}.Value");
        return true;
    }

    // Structural key of a closed expression: equal keys are the same expression.
    private static string ClosedKey(TirNode n)
        => n switch
        {
            null => "_",
            TirLiteral l => $"L:{l.LiteralType}:{l.Value}",
            TirCall c => $"C:{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(c.Callee)}:{c.Name}:{c.Type?.Name}({string.Join(",", c.Args.Select(ClosedKey))})",
            _ => $"{n.GetType().Name}:{n.Type?.Name}:{(n as TirName)?.Name}({string.Join(",", n.Children.Select(ClosedKey))})",
        };

    // After the member: one nested holder class per cached closed expression.
    private void WriteDeferredConstants()
    {
        // Writing a holder's expression can not defer another: nested closed calls stay inline.
        _holderBodyDepth++;
        foreach (var c in _deferredConstants)
        {
            Write($"private static class {c.Holder} {{ static {c.Holder}() {{ }} internal static readonly {c.Type} Value = ");
            WriteNode(c.Node);
            WriteLine("; }");
        }
        _holderBodyDepth--;
        _deferredConstants.Clear();
    }

    // Only value-forming expression nodes: anything that binds or reads a name, builds a closure or
    // an array, or is a statement disqualifies the body.
    private static bool IsClosedConstant(TirNode n)
        => n switch
        {
            null => true,
            TirLiteral or TirTypeRef or TirName or TirDefault => true,
            TirCoerce or TirCall or TirConditional or TirNew
                or TirComponentAccess or TirConstructorCall or TirBooleanChain => n.Children.All(IsClosedConstant),
            _ => false,
        };
}
