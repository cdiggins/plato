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

    public PlatoAnalyzer Analyzer => Writer.Analyzer;

    // plato-311: this writer's grounding owner for Self — the concrete/interface type it is
    // currently writing. See ITypeToCSharp.GroundingOwner / ConceptGrounding.GroundsSelf.
    public TypeDef GroundingOwner => TypeDef;

    public static string Annotation => $"[MethodImpl(AggressiveInlining)]";

    public CSharpTypeWriter(CSharpWriter writer, TypeDef type)
    {
        IndentLevel = writer.IndentLevel;
        Writer = writer;
        TypeDef = type;

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

    /// <summary>Carry a Plato declaration's `//` comment block (see
    /// <see cref="Compiler.Symbols.DocComment"/>) into the emitted C# as an XML doc comment.
    /// The generated struct is where this vocabulary is actually read, so the prose has to travel
    /// with it; nothing is written when the declaration has no comment.</summary>
    public CSharpTypeWriter WriteDoc(string doc)
    {
        if (string.IsNullOrWhiteSpace(doc))
            return this;
        WriteLine("/// <summary>");
        foreach (var line in doc.Split('\n'))
            WriteLine("/// " + EscapeXml(line.TrimEnd('\r')));
        return WriteLine("/// </summary>");
    }

    private static string EscapeXml(string s)
        => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

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

        var type = TypeDef;
        Debug.Assert(type.IsInterface());

        // We have a special implementation of IArray
        if (type.Name == "IArray" || type.Name == "IArray2D" || type.Name == "IArray3D")
            return this;

        var inherits = type.Inherits.Select(this.ToCSharpType).ToList();

        // plato-311 (Option A, dual-interface lowering): a self-constrained interface with at least
        // one object-safe member also gets a non-generic existential view `interface C` — what a
        // bare (type-position) reference to the interface renders as. The F-bounded `interface
        // C<Self>` inherits it (`: C, ...`) alongside its existing base interfaces, so nothing
        // reachable through `C<Self>` today is lost; the view is purely additive.
        if (type.IsSelfConstrained() && type.HasObjectSafeSurface())
        {
            WriteConceptView(type);
            inherits.Insert(0, ViewTypeName(type));
        }

        var inherited = inherits.Count > 0 ? ": " + inherits.JoinStringsWithComma() : "";
        var selfConstraint = type.IsSelfConstrained() ? " where Self : " + FullName : "";

        WriteDoc(type.Doc);
        WriteLine(CSharpWriter.GeneratedCodeAttribute);
        Write("public interface ").Write(FullName).Write(inherited).WriteLine(selfConstraint);

        // TODO: maybe make the "Self" actually constrained on the interface.

        return WriteInterfaceFunctions(type);
    }

    // plato-311: the non-generic existential view's own name — the interface's name plus its OWN
    // (non-Self) type parameters, e.g. "Curve3D" or "Procedural<T, U>". Self never appears: every
    // member on the view is object-safe, so Self occurs only as the (implicit, unwritten) receiver.
    private static string ViewTypeName(TypeDef type)
    {
        var ownParams = type.TypeParameters.Select(tp => tp.Name).ToList();
        return ownParams.Count > 0 ? $"{type.Name}<{ownParams.JoinStringsWithComma()}>" : type.Name;
    }

    // The view name for an INHERITED interface reference (as written in an `inherits`/`implements`
    // clause), e.g. rendering `Procedural<Number, Point3D>` for Curve3D's base — the explicit
    // (non-Self) type arguments as written, resolved through the ordinary type converter.
    private string InheritedViewTypeName(TypeExpression te)
    {
        var args = te.TypeArgs.Select(this.ToCSharpType).ToList();
        return args.Count > 0 ? $"{te.Def.Name}<{args.JoinStringsWithComma()}>" : te.Def.Name;
    }

    // plato-311: emit the non-generic existential view interface for a self-constrained interface
    // that has an object-safe surface — the object-safe subset of its members (Self appears only
    // as the receiver), inheriting the views of any base interfaces that have their own view.
    private CSharpTypeWriter WriteConceptView(TypeDef type)
    {
        var viewInherits = type.Inherits
            .Select(ie => ie.Def.IsSelfConstrained()
                ? (ie.Def.HasObjectSafeSurface() ? InheritedViewTypeName(ie) : null)
                : this.ToCSharpType(ie))
            .Where(s => s != null)
            .ToList();
        var inherited = viewInherits.Count > 0 ? ": " + viewInherits.JoinStringsWithComma() : "";

        Write("public interface ").Write(ViewTypeName(type)).WriteLine(inherited);
        WriteStartBlock();
        foreach (var m in type.ObjectSafeMethods())
        {
            var fi = ToFunctionInfo(m.Function, null, FunctionInstanceKind.InterfaceDeclared);
            WriteLine(fi.MethodInterface);
        }
        return WriteEndBlock();
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

    /// <summary>
    /// The name of the one field that IS this type's element surface, or null when the fields
    /// themselves are the elements.
    ///
    /// At/Count are synthesized here from a type's fields, which is right for a fixed-arity type
    /// (Point3D: X/Y/Z) whose fields ARE its components, but wrong for a runtime-arity type
    /// (VectorN: a single Array&lt;Number&gt; field) where the FIELD is the component surface —
    /// enumerating fields there gives Count == 1 and At(0) == the whole collection.
    ///
    /// The test is the field type's interface, not the spelling of its name: the library declares
    /// such fields both as Array&lt;T&gt; (stdlib, implements Indexable&lt;T&gt;) and as IArray&lt;T&gt;
    /// (stdlib-legacy, declares At/Count directly), and both are the same shape. Requiring a linear
    /// At AND Count also excludes Array2D/Array3D, which implement Indexable2D/3D and have no
    /// meaningful single-index Count — those need an explicit library body.
    /// </summary>
    private static string SingleIndexableFieldName(ConcreteType t)
    {
        if (t.TypeDef.Fields.Count != 1)
            return null;

        var field = t.TypeDef.Fields[0];
        var fieldDef = field.Type?.Def;
        if (fieldDef == null || fieldDef == t.TypeDef)
            return null;

        var names = fieldDef.GetSelfAndAllInheritedTypes()
            .Concat(fieldDef.GetAllImplementedConcepts().Select(c => c?.Def).Where(d => d != null))
            .SelectMany(d => d.Methods)
            .Select(m => m.Name)
            .ToHashSet();

        return names.Contains("At") && names.Contains("Count") ? field.Name : null;
    }

    public CSharpTypeWriter GenerateFunc(CSharpFunctionInfo f, ConcreteType t)
    {
        var pns = f.ParameterNames;
        var fs = t.TypeDef.Fields.Select(tf => tf.Name).ToList();
        Write(f.MethodSignature);

        var singleArrayField = SingleIndexableFieldName(t);

        if (f.Name == "At" && singleArrayField != null)
        {
            WriteLine($" => {singleArrayField}[{pns[1]}];");
            return this;
        }

        if (f.Name == "Count" && singleArrayField != null)
        {
            return WriteLine($" {{ {Annotation} get => {singleArrayField}.Count; }}");
        }

        if (f.Name == "At")
        {
            var p = pns[1];
            var s = "";
            for (var i = 0; i < fs.Count; i++)
                s += $"{p} == {i} ? {fs[i]} : ";
            s += $"throw new System.IndexOutOfRangeException()";
            WriteLine($" => {s};");
            // No indexers are generated — At() is the access surface.
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
        // No ground instantiation, but the body may still be TYPE-AGNOSTIC in the type variables
        // the emitted signature already declares (`Array2D<T>.Map<T2>`): emit it once, open.
        if (tir == null && isMember)
            tir = Writer.TryGetOpenGenericTir(f.Function.Implementation, TypeDef);
        if (tir == null)
        {
            // No fully-ground monomorphized TIR for this bodied member. Under the shipping
            // (stdlib-legacy) recipes this never happens (fallback = 0, byte-identity gate).
            // For the forward stdlib some bodies genuinely cannot ground yet (residual checker
            // diagnostics, generic concrete types never instantiated at a ground type, bodies
            // referencing interface members with no implementer). Degrade gracefully: emit a
            // throwing stub that names the reason, count it as a burn-down number, and keep
            // writing every other member/file rather than aborting all output.
            var member = $"{(TypeDef != null ? TypeDef.Name + "." : "")}{f.Name}";
            Writer.DegradedBodies.Add(member);
            return WriteBodyText(
                $" => throw new System.NotImplementedException(" +
                $"\"plato: no ground TIR for bodied {member} (not monomorphized)\");" +
                Environment.NewLine);
        }
        Writer.TirBodiesEmitted++;
        tir = Writer.RunOptimizerPasses(tir, f);
        return WriteBodyText(new TirCSharpBodyWriter(this, tir, isStatic: !isMember, f).ToString());
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

    // Plato overloads on RETURN type; C# does not. Two declarations that reduce to the same C#
    // member signature on one type (Array2D.Map -> Array2D<T2> and the Container Map ->
    // Array<T2>) are CS0111, so the first one written wins. Keyed per type writer.
    private readonly HashSet<string> _emittedMemberSignatures = new HashSet<string>();

    public CSharpTypeWriter WriteMemberFunction(CSharpFunctionInfo f, bool isPrimitive, Func<string, bool> hasFunction = null)
    {
        if (!_emittedMemberSignatures.Add(MemberSignatureKey(f)))
            return this;

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
        {
            WriteLine(f.OperatorImpl);

            // C# rejects an unpaired comparison operator (CS0216). If the library does not
            // declare the partner function, synthesize the partner by swapping the operands.
            var partnerFn = f.PartnerFunctionName;
            if (partnerFn != null && hasFunction != null && !hasFunction(partnerFn))
                WriteLine(f.PairedOperatorImpl);
        }

        if (f.IsImplicit)
        {
            if (f.OwnerType.Name == f.Name)
                Debug.WriteLine("Skipping implicit cast to self");
            else if (f.OwnerType.Fields.Count == 1 && f.OwnerType.Fields[0].Type.Def.Name == f.Name)
                Debug.WriteLine("Skipping implicit cast to single field (already included)");
            // The TARGET type already emits this exact conversion from its single field
            // (`implicit operator Angle(Number)` in _Angle.g.cs, from Angle's lone `Radians:
            // Number`). Emitting the mirror image here as well makes every Number -> Angle
            // conversion ambiguous (CS0457).
            else if (f.Function.ReturnType?.Def is { } target
                     && target.Fields.Count == 1
                     && target.Fields[0].Type?.Def?.Name == f.OwnerType.Name)
                Debug.WriteLine("Skipping implicit cast already emitted by the target type");
            else
                WriteLine(f.ImplicitImpl);
        }

        return this;
    }

    // Type-variable NAMES are per-declaration (_T0 in one, _T1 in another), so normalize them
    // positionally before comparing.
    private static string MemberSignatureKey(CSharpFunctionInfo f)
    {
        var key = f.MethodParameterTypes.JoinStringsWithComma();
        for (var i = 0; i < f.Generics.Count; i++)
            key = System.Text.RegularExpressions.Regex.Replace(
                key, System.Text.RegularExpressions.Regex.Escape(f.Generics[i]), $"#{i}");
        return $"{f.Name}`{f.Generics.Count}({key})";
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
            {
                // A `_`-receiver member is a TYPE-level operation. Under --static-abstract it
                // becomes a `static abstract` obligation the implementing type satisfies with its
                // plain `static` method; otherwise it is omitted from the interface as before
                // (which is why the contract used to be invisible in C#). See plato-312.
                if (Writer.StaticAbstract)
                {
                    WriteDoc(m.Doc);
                    WriteLine(fi.StaticAbstractInterface);
                }
                continue;
            }
            WriteDoc(m.Doc);
            WriteLine(fi.MethodInterface);
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

        // Function0..Function9 map to System.Func; a type whose NAME merely starts with the
        // substring "Function" (FunctionVolume3D, FunctionRegion2D, FunctionSdf2D/3D,
        // FunctionalProcedural, ...) is a distinct concrete/interface type and must render as
        // itself. The digit-after-prefix check is the same test Plato.RustWriter and
        // Plato.TypeScriptWriter already use (plato-310: this writer was missing it, so every
        // FunctionN-prefixed non-FunctionN type silently rendered as bare `System.Func`, losing
        // its own name and — since such types are rarely themselves generic — its type arguments
        // along with it).
        if (type.Name.Length > "Function".Length && type.Name.StartsWith("Function")
            && char.IsDigit(type.Name["Function".Length]))
            return "System.Func";


        var name = type.Name;

        // The concrete Array/Array2D/Array3D primitive types render as the runtime list
        // interfaces, same as their IArray* interfaces (stdlib-legacy never uses them in emitted
        // type positions — it goes through the IArray interfaces — but stdlib uses
        // Array<T> directly in fields and signatures).
        // Array2D / Array3D left this mapping 2026-08-01: since plato-378 they are ORDINARY
        // Plato types with an honest layout (Elements + ColumnCount + RowCount), and a field
        // read on them cannot bind against a runtime interface - C# has no extension
        // properties. They now generate as structs like any other declared type.
        if (name == "Array")
            return "IReadOnlyList";

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

