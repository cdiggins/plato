// --cache-constants (TKT-1095): a nullary static whose body is a closed expression reads a cached field.
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
    /// functions, array- and function-valued results, and bare literals are left as they are.
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
        if (_fi.ReturnType == null || _fi.ReturnType.Contains("IReadOnlyList") || _fi.ReturnType.Contains("Func<"))
            return false;
        if (!IsClosedConstant(body) || !body.Descendants().Any(n => n is TirCall or TirNew or TirConstructorCall))
            return false;

        var holder = $"_Const_{_fi.Name}";
        Write($" => {holder}.Value; private static class {holder} {{ static {holder}() {{ }} internal static readonly {_fi.ReturnType} Value = ");
        WriteNode(body);
        Write("; }");
        return true;
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
