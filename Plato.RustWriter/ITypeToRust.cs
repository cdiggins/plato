using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.RustWriter;

/// <summary>
/// Converts Plato type instances into Rust type names.
/// This is the Rust analog of ITypeToTypeScript in Plato.TypeScriptWriter.
/// </summary>
public interface ITypeToRust
{
    string ToRustTypeName(TypeInstance ti);
}

public static class TypeToRustExtensions
{
    public static string ToRustType(this ITypeToRust typeToRust, TypeExpression te)
        => typeToRust.ToRustType(TypeInstance.Create(te));

    public static string ToRustType(this ITypeToRust typeToRust, TypeDef type)
        => typeToRust.ToRustType(type.ToTypeExpression());

    public static string ToRustType(this ITypeToRust typeToRust, TypeInstance ti)
    {
        var args = ti.ArgsWithSelf.ToList();
        var name = typeToRust.ToRustTypeName(ti);

        // Plato function types are written as "impl Fn" closure parameters.
        // The last type argument is the return type, the others are parameters.
        if (name == RustWriter.FunctionTypeSentinel)
        {
            var argTypes = args.Select(typeToRust.ToRustType).ToList();
            if (argTypes.Count == 0)
                return "impl Fn()";
            var returnType = argTypes[argTypes.Count - 1];
            var parameters = argTypes.Take(argTypes.Count - 1);
            return $"impl Fn({parameters.JoinStringsWithComma()}) -> {returnType}";
        }

        var argString = args.Count == 0 ? "" : $"<{args.Select(typeToRust.ToRustType).JoinStringsWithComma()}>";
        return name + argString;
    }

    public static string ToRustType(this ITypeToRust typeToRust, InterfaceImplementation ii)
        => typeToRust.ToRustType(ii.TypeExpression);
}
