using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.TypeScriptWriter;

/// <summary>
/// Converts Plato type instances into TypeScript type names.
/// This is the TypeScript analog of ITypeToCSharp in Plato.CSharpWriter.
/// </summary>
public interface ITypeToTypeScript
{
    string ToTypeScriptTypeName(TypeInstance ti);
}

public static class TypeToTypeScriptExtensions
{
    public static string ToTypeScriptType(this ITypeToTypeScript typeToTypeScript, TypeExpression te)
        => typeToTypeScript.ToTypeScriptType(TypeInstance.Create(te));

    public static string ToTypeScriptType(this ITypeToTypeScript typeToTypeScript, TypeDef type)
        => typeToTypeScript.ToTypeScriptType(type.ToTypeExpression());

    public static string ToTypeScriptType(this ITypeToTypeScript typeToTypeScript, TypeInstance ti)
    {
        var args = ti.ArgsWithSelf.ToList();
        var name = typeToTypeScript.ToTypeScriptTypeName(ti);

        // Plato function types are written with TypeScript arrow syntax.
        // The last type argument is the return type, the others are parameters.
        if (name == TypeScriptWriter.FunctionTypeSentinel)
        {
            var argTypes = args.Select(typeToTypeScript.ToTypeScriptType).ToList();
            if (argTypes.Count == 0)
                return "() => void";
            var returnType = argTypes[argTypes.Count - 1];
            var parameters = argTypes.Take(argTypes.Count - 1).Select((t, i) => $"a{i}: {t}");
            return $"({parameters.JoinStringsWithComma()}) => {returnType}";
        }

        var argString = args.Count == 0 ? "" : $"<{args.Select(typeToTypeScript.ToTypeScriptType).JoinStringsWithComma()}>";
        return name + argString;
    }

    public static string ToTypeScriptType(this ITypeToTypeScript typeToTypeScript, InterfaceImplementation ii)
        => typeToTypeScript.ToTypeScriptType(ii.TypeExpression);
}
