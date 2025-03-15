using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Analysis;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter;

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
    public PlatoAnalyzer Analyzer => Writer.Analyzer;

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

    public CSharpTypeWriter WriteConcreteType(ConcreteType ct)
    {
        Debug.Assert(ct.TypeDef == TypeDef);
        
        /*
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
        */
        //WriteLine($"*/");
        
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
        if (type.Name.StartsWith("IArray"))
            return this;

        var inherits = type.Inherits.Select(this.ToCSharpType).ToList();
        var inherited = inherits.Count > 0 ? ": " + inherits.JoinStringsWithComma() : "";
        
        Write("public interface ").Write(FullName).WriteLine(inherited);

        // TODO: maybe make the "Self" actually constrained on the interface. 

        return WriteConceptInterfaceFunctions(type);
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
            WriteLine($"{f.IndexerSig} {{ {Annotation} get => At(n); }}");
            return this;
        }

        if (f.Name == "Count")
        {
            return WriteLine($" {{ {Annotation} get => {fs.Count}; }}");
        }

        throw new Exception("Only 'At' or 'Count' supported");
    }

    public CSharpTypeWriter WriteBody(CSharpFunctionInfo f, bool isStatic)
    {
        var tmp = new CSharpFunctionBodyWriter(this, f, isStatic, false);
        Write(tmp.ToString());
        return this;
    }

    public CSharpTypeWriter WriteExtensionFunction(CSharpFunctionInfo f)
        => Write($"{f.ExtensionSignature}").WriteBody(f, true);

    public CSharpTypeWriter WriteStaticFunction(CSharpFunctionInfo fi)
        => Write($"{fi.StaticSignature}").WriteBody(fi, true);

    public CSharpTypeWriter WriteMemberFunction(CSharpFunctionInfo f, bool isPrimitive)
    {
        if (!isPrimitive || f.Body != null)
        {
            WriteLine($"// {f.Function.SignatureId}; [{f.Function.Substitutions}]; {f.Function.TypeVariableAnalysis}");
            Write(f.MethodSignature);
            WriteBody(f, false);
        }
        else
        {
            if (f.IsOperator)
            {
                Write(f.MethodSignature);

                if (f.ParameterNames.Count == 1)
                    WriteLine($" {{ {Annotation} get => {f.OperatorName}this; }}");
                else if (f.ParameterNames.Count == 2)
                    WriteLine($" => this {f.OperatorName} {f.ParameterNames[1]};");
                else
                    throw new NotSupportedException();
            }
            return this;
        }

        if (f.IsOperator && !isPrimitive)
            WriteLine(f.OperatorImpl);

        if (f.IsIndexer)
            WriteLine(f.IndexerImpl);

        if (f.IsImplicit)
        {
            if (f.ConcreteType.Name == f.Name)
                Debug.WriteLine("Skipping implicit cast to self");
            else if (f.ConcreteType.TypeDef.Fields.Count == 1 && f.ConcreteType.TypeDef.Fields[0].Type.Def.Name == f.Name)
                Debug.WriteLine("Skipping implicit cast to single field (already included)");
            else
                WriteLine(f.ImplicitImpl);
        }

        return this;
    }

    public CSharpFunctionInfo ToFunctionInfo(FunctionDef fd, ConcreteType ct, FunctionInstanceKind kind)
        => ToFunctionInfo(new FunctionInstance(fd, ct, null, kind));

    public CSharpFunctionInfo ToFunctionInfo(FunctionInstance fi, ConcreteType ct = null)
        => new CSharpFunctionInfo(fi, ct, this);

    public CSharpTypeWriter WriteConceptInterfaceFunctions(TypeDef type)
    {
        WriteStartBlock();
        foreach (var m in type.Methods)
        {
            var fi = ToFunctionInfo(m.Function, null, FunctionInstanceKind.ConceptInterface);
            if (fi.IsStatic)
                continue;
            WriteLine(fi.MethodInterface);
            if (fi.IsIndexer)
                WriteLine(fi.IndexerInterface);
        }
        return WriteEndBlock();
    }

    public static string FieldNameToParameterName(string fieldName)
        => fieldName.Length == 0 || fieldName[0].IsLower()
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

        return type.Name;
    }
}