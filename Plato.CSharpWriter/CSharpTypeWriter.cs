using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// This can be used to build code for a specific type in C#, or it can be used to build standalone functions,
/// in which case the type is null.
/// </summary>
public class CSharpTypeWriter : CodeBuilder<CSharpTypeWriter>, ITypeToCSharp
{
    public string SelfType;
    public string FullName;
    public TypeDef TypeDef;
    public CSharpWriter Writer;

    // Extension-style mode only (--csharp-style=extensions): inside a static library class the
    // receiver type's member scope is NOT implicit, so bare names written by
    // FunctionGroupRefSymbol must be re-qualified: instance members with the receiver parameter
    // name ("M11" -> "m.M11"), static members with the namespace-qualified type name
    // ("One" -> "Ara3D.Geometry.Vector2.One"; the namespace is needed because members of the
    // enclosing static library class can shadow type names). All null (always, in the default
    // mode) preserves the original output byte for byte.
    public string ExtensionReceiverName;
    public string ExtensionStaticQualifier;
    public HashSet<string> ExtensionInstanceNames;
    public HashSet<string> ExtensionStaticNames;

    // Scalar erasure (--scalar=float) only. EraseScalars: whether this writer maps the five
    // scalar wrapper type names to their primitives (defaults to Writer.ScalarErase; switched
    // OFF while writing concept interfaces and struct-kept members of non-scalar types, which
    // keep wrapper types to match the handwritten Plato.Intrinsics boundary).
    // ExtensionReceiverIsScalar: this writer is emitting the body of an extension method whose
    // receiver is an erased scalar primitive, so re-qualified bare instance names need "()".
    public bool EraseScalars;
    public bool ExtensionReceiverIsScalar;
    public PlatoAnalyzer Analyzer => Writer.Analyzer;

    public static string Annotation => $"[MethodImpl(AggressiveInlining)]";

    public CSharpTypeWriter(CSharpWriter writer, TypeDef type)
    {
        IndentLevel = writer.IndentLevel;
        Writer = writer;
        TypeDef = type;
        EraseScalars = writer.ScalarErase;

        if (type == null)
        {
            SelfType = "";
            return;
        }

        var baseTypeParams = type.TypeParameters.Select(tp => tp.Name).ToList();
        var typeParams = (type.IsSelfConstrained()
            ? baseTypeParams.Prepend("Self")
            : baseTypeParams).ToList();


        FullName = $"{type.Name}";
        if (typeParams.Count > 0)
            FullName += $"<{typeParams.JoinStringsWithComma()}>";

        SelfType = FullName;
    }

    public static string IIStr(InterfaceImplementation ii)
        => $"{ii.Interface.Name} : {ii.TypeExpression} [ {ii.Substitutions} ]";

    public CSharpTypeWriter WriteConcreteType(ConcreteType ct)
    {
        Debug.Assert(ct.TypeDef == TypeDef);

#if WRITE_TYPE_ANALYSIS
        WriteLine($"/*");
        Indent();

        WriteLine($"Concrete type: {ct.Name}");

        WriteLine($"# Interfaces {ct.Interfaces.Count}");
        foreach (var x in ct.Interfaces)
            WriteLine($" Interface {IIStr(x)}");

        WriteLine($"# All interfaces {ct.AllInterfaces.Count}");
        foreach (var x in ct.AllInterfaces)
            WriteLine($" Interface {IIStr(x)}");

        WriteLine($"# Concrete Functions {ct.ConcreteFunctions.Count}");
        foreach (var x in ct.ConcreteFunctions)
            WriteLine($" Function {x.SignatureId}");

        WriteLine($"# Field Functions {ct.FieldFunctions.Count}");
        foreach (var x in ct.FieldFunctions)
            WriteLine($" Function {x.SignatureId}");

        WriteLine($"# Declared Functions {ct.DeclaredFunctions.Count}");
        foreach (var x in ct.DeclaredFunctions)
            WriteLine($" Function {x.SignatureId}");

        WriteLine($"# Implemented Functions {ct.ImplementedFunctions.Count}");
        foreach (var x in ct.ImplementedFunctions)
            WriteLine($" Function {x.SignatureId}");

        WriteLine($"# Unimplemented Functions {ct.UnimplementedFunctions.Count}");
        foreach (var x in ct.UnimplementedFunctions)
            WriteLine($" Function {x.SignatureId}");

        WriteLine($"# All Substitutions {ct.Substitutions}");
        var tmp = ct.Substitutions;
        while (tmp != null)
        {
            WriteLine($" Substitution {tmp.Name} = {tmp.Replacement}");
            tmp = tmp.Previous;
        }

        WriteLine();
        Dedent();
        WriteLine($"*/");
#endif
        
        _ = new CSharpConcreteTypeWriter(this, ct);
        return this;
    }

    public CSharpTypeWriter WriteConceptInterface()
    {
        // When writing interfaces, the "Self" type is literally "Self".
        SelfType = "Self";

        // Scalar erasure: concept interfaces keep the wrapper types. Handwritten members in
        // Plato.Intrinsics (which cannot change) satisfy interface obligations with wrapper
        // signatures, so erasing interface declarations would break the intrinsics build.
        // MethodsOnly (--methods): every obligation is generated (no handwritten member
        // satisfies a concept interface), so the interfaces erase and declare methods.
        EraseScalars = Writer.NoProperties && Writer.ScalarErase;

        var type = TypeDef;
        Debug.Assert(type.IsInterface());

        // We have a special implementation of IArray
        if (type.Name == "IArray" || type.Name == "IArray2D" || type.Name == "IArray3D")
            return this;

        var inherits = type.Inherits.Select(this.ToCSharpType).ToList();
        var inherited = inherits.Count > 0 ? ": " + inherits.JoinStringsWithComma() : "";
        var selfConstraint = type.IsSelfConstrained() ? " where Self : " + FullName : "";

        Write("public interface ").Write(FullName).Write(inherited).WriteLine(selfConstraint);

        // TODO: maybe make the "Self" actually constrained on the interface. 

        return WriteInterfaceFunctions(type);
    }

    public CSharpTypeWriter Write(DefSymbol value)
    {
        switch (value)
        {
            case null:
                return Write("null");

            case FieldDef fieldDef:
                return Write("public ").Write(fieldDef.Type).Write(fieldDef.Name).Write(" { get; }")
                    .WriteLine();

            case FunctionGroupDef memberGroup:
                return Write(memberGroup.Name);

            case ParameterDef parameter:
                return Write(parameter.Type).Write(parameter.Name);
        }

        throw new NotSupportedException();
    }

    public CSharpTypeWriter GenerateFunc(CSharpFunctionInfo f, ConcreteType t)
    {
        var pns = f.ParameterNames;
        var fs = t.TypeDef.Fields.Select(tf => tf.Name).ToList();
        Write(f.MethodSignature);

        if (f.Name == "At")
        {
            var p = pns[1];
            var s = "";
            for (var i = 0; i < fs.Count; i++)
                s += $"{p} == {i} ? {fs[i]} : ";
            s += $"throw new System.IndexOutOfRangeException()";
            WriteLine($" => {s};");
            // MethodsOnly: no indexers — At() is the access surface.
            if (!Writer.NoProperties)
                WriteLine($"{f.IndexerSig} {{ {Annotation} get => At(n); }}");
            return this;
        }

        if (f.Name == "Count")
        {
            return WriteLine($" {{ {Annotation} get => {fs.Count}; }}");
        }

        throw new Exception("Only 'At' or 'Count' supported");
    }

    // The fixed body of an interface obligation / stub (Function.Implementation.Body == null):
    // a one-liner with no body-writer heuristics. Reproduces CSharpFunctionBodyWriter's Body==null
    // early return byte-for-byte (WriteLine appends Environment.NewLine) so the legacy writer is no
    // longer on the body-less path (consolidation plan C4 step 1).
    private static readonly string BodilessBodyText =
        " => throw new NotImplementedException();" + Environment.NewLine;

    public CSharpTypeWriter WriteBody(CSharpFunctionInfo f, bool isStatic)
    {
        // Body-less function: emit the fixed stub directly (consolidation plan C4 step 1).
        if (f.Body == null)
            return WriteBodyText(BodilessBodyText);

        // Bodied member/static: emitted from the monomorphized TIR (Elaborate → Monomorphize →
        // Emit) — the SOLE C# body writer since the legacy CSharpFunctionBodyWriter was retired
        // (consolidation plan C4). A member-instance body renders with `this`; a static body (a
        // constant, an IArray library function in Extensions.g.cs, a scalar shim member) renders
        // its receiver by name. Every bodied (function, concrete type) has a fully-ground TIR
        // under the shipping recipes (legacy fallback bodies: 0); a null here is a regression.
        var isMember = !isStatic && TypeDef != null;
        var tir = isMember
            ? Writer.TryGetGroundTir(f.Function.Implementation, TypeDef)
            : Writer.TryGetStaticTir(f.Function.Implementation);
        if (tir == null)
            throw new InvalidOperationException(
                $"No ground TIR for bodied {(TypeDef != null ? TypeDef.Name + "." : "")}{f.Name}; "
                + "the legacy body writer was removed (consolidation plan C4).");
        Writer.TirBodiesEmitted++;
        tir = Writer.RunOptimizerPasses(tir, f, isMember, out var lowered);
        return WriteBodyText(new TirCSharpBodyWriter(this, tir, isStatic: !isMember, f, lowered: lowered).ToString());
    }

    // Extension style fixes the V1 indentation quirk (see WriteWithLineStateSync);
    // the default mode must stay byte-identical, misindentation included. Shared by the
    // TIR path and the legacy path so both frame their body text identically.
    private CSharpTypeWriter WriteBodyText(string body)
        => Writer.ExtensionStyle ? this.WriteWithLineStateSync(body) : Write(body);

    public CSharpTypeWriter WriteExtensionFunction(CSharpFunctionInfo f)
    {
        // If the first type is a self-constrained interface we need to do something special.
        var firstType = f.Function.ParameterTypes[0].Def;
        if (firstType.IsSelfConstrained())
        {
            // We need to write the self constraint.
            var selfTypeName = firstType.Name;
            var selfType = $"{selfTypeName}<Self>";
            var generics = f.AllGenerics.Append("Self").JoinStringsWithComma();
            var parameterTypes = f.Function.ParameterTypes.Skip(1).Select(pt => pt.Name == selfTypeName ? selfType : pt.Name).Prepend(selfType);
            var parameters = f.ParameterNames.Zip(parameterTypes, (pn, pt) => $"{pt} {pn}").JoinStringsWithComma();
            var constraints = $"where Self : {selfType}";
            var retType = f.Function.ReturnType.Name == selfTypeName ? "Self" : f.ReturnType;
            var extensionSignature = $"{CSharpFunctionInfo.Annotation}public static {retType} {f.Name}<{generics}>(this {parameters}) {constraints}";
            return Write($"{extensionSignature}").WriteBody(f, true);
        }
        else
        {
            return Write($"{f.ExtensionSignature}").WriteBody(f, true);
        }
    }

    public CSharpTypeWriter WriteStaticFunction(CSharpFunctionInfo fi)
        => Write($"{fi.StaticSignature}").WriteBody(fi, true);

    public CSharpTypeWriter WriteMemberFunction(CSharpFunctionInfo f, bool isPrimitive)
    {
        if (!isPrimitive || f.Body != null)
        {
            //WriteLine($"// {f.Function.SignatureId}; [{f.Function.Substitutions}]; {f.Function.TypeVariableAnalysis}");
            Write(f.MethodSignature);
            WriteBody(f, false);
        }
        else
        {
            if (f.IsOperator)
            {
                Write(f.MethodSignature);

                if (f.ParameterNames.Count == 1)
                    WriteLine(f.EmitAsMethod
                        ? $" => {f.OperatorName}this;"
                        : $" {{ {Annotation} get => {f.OperatorName}this; }}");
                else if (f.ParameterNames.Count == 2)
                    WriteLine($" => this {f.OperatorName} {f.ParameterNames[1]};");
                else
                    throw new NotSupportedException();
            }
            return this;
        }

        if (f.IsOperator && !isPrimitive)
            WriteLine(f.OperatorImpl);

        if (f.IsIndexer && !Writer.NoProperties)
            WriteLine(f.IndexerImpl);

        if (f.IsImplicit)
        {
            if (f.OwnerType.Name == f.Name)
                Debug.WriteLine("Skipping implicit cast to self");
            else if (f.OwnerType.Fields.Count == 1 && f.OwnerType.Fields[0].Type.Def.Name == f.Name)
                Debug.WriteLine("Skipping implicit cast to single field (already included)");
            else
                WriteLine(f.ImplicitImpl);
        }

        return this;
    }

    public CSharpFunctionInfo ToFunctionInfo(FunctionDef fd, TypeDef td, FunctionInstanceKind kind)
        => ToFunctionInfo(new FunctionInstance(fd, td, null, kind));

    public CSharpFunctionInfo ToFunctionInfo(FunctionInstance fi, TypeDef td = null)
        => new CSharpFunctionInfo(fi, td, this);

    public CSharpTypeWriter WriteInterfaceFunctions(TypeDef type)
    {
        WriteStartBlock();
        foreach (var m in type.Methods)
        {
            var fi = ToFunctionInfo(m.Function, null, FunctionInstanceKind.InterfaceDeclared);
            if (fi.IsStatic)
                continue;
            WriteLine(fi.MethodInterface);
            if (fi.IsIndexer && !Writer.NoProperties)
                WriteLine(fi.IndexerInterface);
        }
        return WriteEndBlock();
    }

    public static HashSet<string> CSharpKeywords = new HashSet<string>()
    {
        "this", "base", "new", "null", "true", "false", "default", 
        "class", "struct", "interface", "record", "where",
        "ref", "in", "out", "params",
        "if", "else", "for", "foreach", "while", "do", "switch", "case", "break", "continue", 
        "return", "throw", "try", "catch", "finally", "using", "namespace", "until", 
        "lock", "fixed", "unsafe", "checked", "unchecked", "goto", "async", "await",
        "public", "private", "protected", "internal", "static", "readonly", "volatile", "const", "abstract", "virtual", "override", "sealed",
        "void", "object", "type", "dynamic", "int", "float", "char", "bool", "double", "decimal", "string",
        "typeof", "nameof", "sizeof", "is", "as",
    };

    public static bool IsReserved(string fieldName)
    {
        return CSharpKeywords.Contains(fieldName);
    }

    public static string FieldNameToParameterName(string fieldName)
        => fieldName.Length == 0 || fieldName[0].IsLower() || IsReserved(fieldName.DecapitalizeFirst())
            ? $"_{fieldName}"
            : fieldName.DecapitalizeFirst();

    public CSharpTypeWriter Write(TypeExpression typeExpression)
        => Write(this.ToCSharpType(typeExpression)).Write(" ");

    //==
    // Implementation of ITypeToCSharp
   
    public string ToCSharpTypeName(TypeInstance type)
    {
        if (type.Name == "Self" && SelfType != null)
            return SelfType;

        if (type.Name.StartsWith("Function"))
            return "System.Func";

        // Scalar erasure (--scalar=float): the five wrapper types become native primitives in
        // generated type positions (generic arguments recurse through here, so
        // IArray<Number> -> IReadOnlyList<float> falls out automatically).
        if (EraseScalars && CSharpWriter.ScalarPrimitives.TryGetValue(type.Name, out var prim))
            return prim;


        var name = type.Name;
        if (name.Contains("IArray"))
        {
            if (name.Contains("IArrayLike"))
                return name;
            return name.Replace("IArray", "IReadOnlyList");
        }

        // Unique (affine) builder types (roadmap Phase 6): List -> PlatoList, Buffer ->
        // PlatoBuffer. Only fires when the type def is actually declared unique, so a
        // hypothetical ordinary type named "List" would be left alone.
        if (type.Def != null && type.Def.IsUnique
            && CSharpWriter.UniqueTypeCSharpNames.TryGetValue(name, out var uniqueName))
            return uniqueName;

        return name;
    }
}

