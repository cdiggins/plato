using System;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Analysis;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter;

public interface ITypeToCSharp
{
    string ToCSharpTypeName(TypeInstance ti);
}

public class ReplaceInterface : ITypeToCSharp
{
    public TypeExpression SrcInterface;
    public TypeDef NewType;
    public TypeInstance NewTypeInstance;
    public string Replacement;
    public ITypeToCSharp Previous;

    public ReplaceInterface(TypeExpression srcInterface, TypeDef newType, ITypeToCSharp previous)
    {
        SrcInterface = srcInterface;
        if (!SrcInterface.Def.IsInterface())
            throw new Exception("Expected an interface");
        NewType = newType;
        Previous = previous;
        NewTypeInstance = TypeInstance.Create(newType.ToTypeExpression());
        Replacement = Previous.ToCSharpTypeName(NewTypeInstance);
        Previous = previous;
    }

    public string ToCSharpTypeName(TypeInstance ti)
        => ti.Expr.IsImplementing(SrcInterface) ? Replacement : Previous.ToCSharpTypeName(ti);
}

public static class TypeToCSharpExtensions
{
    public static string ToCSharpType(this ITypeToCSharp typeToCSharp, TypeExpression te)
        => typeToCSharp.ToCSharpType(TypeInstance.Create(te));

    public static string ToCSharpType(this ITypeToCSharp typeToCSharp, TypeDef type)
        => typeToCSharp.ToCSharpType(type.ToTypeExpression());

    public static string ToCSharpType(this ITypeToCSharp typeToCSharp, TypeInstance ti)
        => typeToCSharp.ToCSharpTypeName(ti) + (ti.Args.Count > 0 ? $"<{ti.Args.Select(typeToCSharp.ToCSharpType).JoinStringsWithComma()}>" : "");

    public static string ToCSharpType(this ITypeToCSharp typeToCSharp, InterfaceImplementation ii)
        => typeToCSharp.ToCSharpType(ii.TypeExpression);
}