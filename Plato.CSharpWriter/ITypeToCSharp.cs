using System;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter;

public interface ITypeToCSharp
{
    string ToCSharpTypeName(TypeInstance ti);

    // plato-311: the TypeDef whose point of view "Self" is being grounded from — the concrete
    // type currently being written, or the concept interface itself while writing its own body
    // (where SelfType is the literal "Self"). Null means no grounding context is known, so any
    // bare self-constrained concept reference reached through this writer is treated as
    // existential (see ConceptGrounding.GroundsSelf).
    TypeDef GroundingOwner { get; }
}

public class ReplaceInterface : ITypeToCSharp
{
    public TypeExpression SrcInterface;
    public TypeDef NewType;
    public TypeInstance NewTypeInstance;
    public string Replacement;
    public ITypeToCSharp Previous;

    public TypeDef GroundingOwner => Previous?.GroundingOwner;

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
    {
        // plato-311: a bare (implicit-Self) reference to a self-constrained concept that this
        // writer's grounding owner does not implement/inherit is existential ("any C") type-position
        // use — render the non-generic object-safe view instead of substituting Self with whatever
        // enclosing concrete type this writer happens to be writing (the old bug: rendering a
        // `Path: Curve3D` field as `Curve3D<SweptSurface>`). A grounded reference (the writer's own
        // self-reference inside the concept's own body, or an `implements`/`inherits` clause) is
        // unaffected and keeps the F-bounded `C<Self>` spelling.
        if (ti.IsSelfConstrained && !ConceptGrounding.GroundsSelf(typeToCSharp.GroundingOwner, ti.Def))
        {
            // No object-safe surface: ExistentialConceptChecker (CHK308) is the source of truth
            // for rejecting this program before it reaches the writer. If writer output is somehow
            // reached anyway, fail loudly rather than emit unsatisfiable C#.
            if (!ti.Def.HasObjectSafeSurface())
                throw new InvalidOperationException(
                    $"Plato.CSharpWriter: '{ti.Def.Name}' is used in type position (an existential " +
                    $"'any {ti.Def.Name}') but has no object-safe member, so it has no non-generic " +
                    "view (see plato-311 / CHK308).");

            var viewArgs = ti.Args.Select(typeToCSharp.ToCSharpType).ToList();
            return ti.Def.Name + (viewArgs.Count == 0 ? "" : $"<{viewArgs.JoinStringsWithComma()}>");
        }

        var args = ti.ArgsWithSelf.ToList();
        var argString = args.Count == 0 ? "" : $"<{args.Select(typeToCSharp.ToCSharpType).JoinStringsWithComma()}>";
        var name = typeToCSharp.ToCSharpTypeName(ti);

        // plato-310 (fix approach 2): "System.Func" is only ever the mapped-intrinsic rendering of
        // Function0..Function9, and System.Func always takes at least one type argument (the
        // return type). Zero arguments here means a type-rendering path resolved the mapped
        // intrinsic's NAME without carrying its instantiated type arguments — silent CS0305 garbage
        // otherwise. Fail loudly instead of shipping it.
        if (name == "System.Func" && args.Count == 0)
            throw new InvalidOperationException(
                $"Plato.CSharpWriter: mapped intrinsic 'System.Func' rendered with zero type " +
                $"arguments for Plato type '{ti.Name}' (arity must be > 0). A type-rendering path " +
                $"dropped the instantiated type arguments for a Function0..Function9 value.");

        return name + argString;
    }

    public static string ToCSharpType(this ITypeToCSharp typeToCSharp, InterfaceImplementation ii)
        => typeToCSharp.ToCSharpType(ii.TypeExpression);
}