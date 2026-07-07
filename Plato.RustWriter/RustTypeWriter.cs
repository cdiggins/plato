using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.RustWriter;

/// <summary>
/// This can be used to build code for a specific type in Rust, or it can be used to build standalone
/// functions, in which case the type is null. Analog of TypeScriptTypeWriter.
/// </summary>
public class RustTypeWriter : CodeBuilder<RustTypeWriter>, ITypeToRust
{
    public string SelfType;
    public string FullName;
    public TypeDef TypeDef;
    public RustWriter Writer;
    public PlatoAnalyzer Analyzer => Writer.Analyzer;

    /// <summary>
    /// Type variable renamings applied when resolving type names
    /// (used to map the element type variable of IArray functions to the Arr type parameter).
    /// </summary>
    public Dictionary<string, string> TypeNameSubstitutions { get; } = new Dictionary<string, string>();

    /// <summary>
    /// Member names already emitted: Rust does not support overloaded functions,
    /// so colliding functions are skipped.
    /// </summary>
    public HashSet<string> ClaimedNames { get; } = new HashSet<string>();

    public RustTypeWriter(RustWriter writer, TypeDef type)
    {
        IndentLevel = writer.IndentLevel;
        Writer = writer;
        TypeDef = type;

        if (type == null)
        {
            SelfType = "";
            return;
        }

        var typeParams = type.TypeParameters.Select(tp => tp.Name).ToList();

        FullName = $"{type.Name}";
        if (typeParams.Count > 0)
            FullName += $"<{typeParams.JoinStringsWithComma()}>";

        // The self type of a native primitive is the native type itself.
        SelfType = RustWriter.NativePrimitives.TryGetValue(type.Name, out var native)
            ? native
            : FullName;
    }

    public bool IsNativePrimitive => TypeDef != null && RustWriter.NativePrimitives.ContainsKey(TypeDef.Name);

    public RustTypeWriter WriteConcreteType(ConcreteType ct)
    {
        Debug.Assert(ct.TypeDef == TypeDef);
        _ = new RustConcreteTypeWriter(this, ct);
        return this;
    }

    public RustTypeWriter WriteConceptTrait()
    {
        // When writing traits, the "Self" type is literally "Self".
        SelfType = "Self";

        var type = TypeDef;
        Debug.Assert(type.IsInterface());

        // The array interfaces have special implementations (see RustWriter prelude).
        if (ArrayInterfaceNames.Contains(type.Name))
            return this;

        var inherits = type.Inherits.Select(this.ToRustType).ToList();
        var supertraits = inherits.Prepend("Copy").JoinStrings(" + ");

        Write("pub trait ").Write(FullName).Write(": ").WriteLine(supertraits);
        return WriteTraitFunctions(type);
    }

    public RustTypeWriter WriteTraitFunctions(TypeDef type)
    {
        WriteStartBlock();
        var names = new HashSet<string>();
        foreach (var m in type.Methods)
        {
            var fi = ToFunctionInfo(m.Function, null, FunctionInstanceKind.InterfaceDeclared);
            if (fi.IsStatic)
                continue;
            if (!names.Add(fi.Name))
                continue;
            WriteLine(fi.TraitItem());
        }
        return WriteEndBlock().WriteLine();
    }

    public RustFunctionInfo ToFunctionInfo(FunctionDef fd, TypeDef td, FunctionInstanceKind kind)
        => ToFunctionInfo(new FunctionInstance(fd, td, null, kind));

    public RustFunctionInfo ToFunctionInfo(FunctionInstance fi, TypeDef td = null)
        => new RustFunctionInfo(fi, td, this);

    public RustTypeWriter WriteBody(RustFunctionInfo f, bool isStatic)
    {
        var tmp = new RustFunctionBodyWriter(this, f, isStatic, false);
        return WriteTrimmed(tmp.ToString());
    }

    /// <summary>
    /// Writes pre-rendered multi-line text, replacing the trailing newline with a
    /// WriteLine so that indentation resumes correctly afterwards.
    /// </summary>
    public RustTypeWriter WriteTrimmed(string s)
    {
        s = s.TrimEnd('\r', '\n');
        if (s.Length == 0)
            return this;
        return Write(s).WriteLine();
    }

    /// <summary>
    /// Writes a constant of the Constants module as a zero-argument function.
    /// (Rust has no computed statics, so constants become functions: Constants::Pi().)
    /// </summary>
    public RustTypeWriter WriteConstantFunction(RustFunctionInfo fi)
        => Write(fi.ConstantSignature).WriteBody(fi, true);

    /// <summary>
    /// Writes one of the IArray library functions as a method of the Arr struct
    /// (the first parameter becomes self).
    /// </summary>
    public RustTypeWriter WriteArrayMethod(FunctionInstance f)
    {
        if (!ClaimedNames.Add(f.Name))
            return WriteLine($"// Skipped: overload or duplicate member '{f.Name}'");

        var elemVar = f.ParameterTypes[0].ArgsWithSelf.LastOrDefault()?.Name;
        var exclude = new List<string>();
        if (elemVar != null)
        {
            TypeNameSubstitutions[elemVar] = "T";
            exclude.Add(elemVar);
        }

        var fi = ToFunctionInfo(f);
        Write(fi.MethodSignature(true, exclude)).WriteBody(fi, false);

        if (elemVar != null)
            TypeNameSubstitutions.Remove(elemVar);
        return this;
    }

    /// <summary>
    /// Writes an IArray library function over a concrete element type (e.g. Sum of an
    /// IArray of Number) as a module-level function: it cannot become a method of the
    /// generic Arr struct.
    /// </summary>
    public RustTypeWriter WriteFreeArrayFunction(FunctionInstance f)
    {
        if (!ClaimedNames.Add(f.Name))
            return WriteLine($"// Skipped: overload or duplicate function '{f.Name}'");

        var fi = ToFunctionInfo(f);
        return Write(fi.FunctionSignature()).WriteBody(fi, true);
    }

    /// <summary>
    /// Writes a member function of a concrete type (inside an impl block or trait impl).
    /// Bodiless (intrinsic) functions get a native implementation when one is known,
    /// otherwise they panic at run-time.
    /// </summary>
    public RustTypeWriter WriteMemberFunction(RustFunctionInfo f, bool isPub = true)
    {
        if (f.Body == null)
        {
            if (TryGetIntrinsicBody(f, out var body))
                return WriteLine($"{f.MethodSignature(isPub)} {{ {body} }}");
            return WriteLine($"{f.MethodSignature(isPub)} {{ unimplemented!(\"{TypeDef?.Name}.{f.Name}\") }}");
        }

        Write(f.MethodSignature(isPub));
        return WriteBody(f, f.IsStatic);
    }

    /// <summary>
    /// Writes the At function (as a chain of conditions over the fields) and the
    /// Count function for array-like types.
    /// </summary>
    public RustTypeWriter GenerateFunc(RustFunctionInfo f, ConcreteType t)
    {
        var fs = t.TypeDef.Fields.Select(tf => tf.Name).ToList();

        if (f.Name == "At")
        {
            var p = EscapeName(f.ParameterNames[1]);
            var s = "";
            for (var i = 0; i < fs.Count; i++)
                s += $"if {p} == {i} {{ self.{fs[i]} }} else ";
            s += "{ panic!(\"Index out of range\") }";
            return WriteLine($"{f.MethodSignature(true)} {{ {s} }}");
        }

        if (f.Name == "Count")
            return WriteLine($"pub fn Count(self) -> i64 {{ {fs.Count} }}");

        throw new Exception("Only 'At' or 'Count' supported");
    }

    /// <summary>
    /// Native Rust implementations for the bodiless intrinsic functions
    /// (the C# writer relies on hand-written partial structs for these).
    /// The receiver is "self": a native value for primitives, a struct otherwise.
    /// </summary>
    public bool TryGetIntrinsicBody(RustFunctionInfo f, out string body)
    {
        body = null;
        if (TypeDef == null)
            return false;

        var owner = TypeDef.Name;
        var key = $"{owner}.{f.Name}";
        var arg = f.NumParameters > 1 ? EscapeName(f.ParameterNames[1]) : null;

        switch (key)
        {
            case "Integer.Divide":
                body = $"self / {arg}";
                return true;
            case "Integer.Modulo":
                body = $"self % {arg}";
                return true;
            case "Integer.ToNumber":
                body = "self as f64";
                return true;
            case "Integer.Range":
                body = "Intrinsics::Range(self)";
                return true;
            case "Number.Compare":
            case "Integer.Compare":
                body = $"if self < {arg} {{ -1 }} else if self > {arg} {{ 1 }} else {{ 0 }}";
                return true;
            case "Number.Sqrt":
                body = "self.sqrt()";
                return true;
            case "Number.Abs":
                body = "self.abs()";
                return true;
            case "Number.Floor":
                body = "self.floor()";
                return true;
            case "Number.Ceiling":
                body = "self.ceil()";
                return true;
            case "Number.Round":
                body = "self.round()";
                return true;
            case "Number.Truncate":
                body = "self.trunc()";
                return true;
            case "Number.Acos":
                body = "Angle::new(self.acos())";
                return true;
            case "Number.Asin":
                body = "Angle::new(self.asin())";
                return true;
            case "Number.Atan":
                body = "Angle::new(self.atan())";
                return true;
            case "Number.Pow":
                body = $"self.powf({arg})";
                return true;
            case "Number.Log":
                body = $"self.log({arg})";
                return true;
            case "Number.Ln":
                body = "self.ln()";
                return true;
            case "Number.Exp":
                body = "self.exp()";
                return true;
            case "Angle.Cos":
                body = "self.Radians.cos()";
                return true;
            case "Angle.Sin":
                body = "self.Radians.sin()";
                return true;
            case "Angle.Tan":
                body = "self.Radians.tan()";
                return true;
        }

        // Operators on the native primitives are implemented with native operators.
        if (IsNativePrimitive && f.IsOperator)
        {
            var op = f.OperatorName;
            var isNativeResult = f.ReturnType == "f64" || f.ReturnType == "i64"
                || f.ReturnType == "bool" || f.ReturnType == "String" || f.ReturnType == "char";
            if (!isNativeResult)
                return false;

            if (f.NumParameters == 1)
            {
                body = $"{op}self";
                return true;
            }

            if (f.NumParameters == 2)
            {
                body = $"self {op} {arg}";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The array concept appears as "Array" or "IArray" depending on the source dialect.
    /// Both map to the Arr struct from the prelude.
    /// </summary>
    public static HashSet<string> ArrayInterfaceNames = new HashSet<string>()
    {
        "Array", "Array2D", "Array3D",
        "IArray", "IArray2D", "IArray3D",
    };

    public static HashSet<string> RustKeywords = new HashSet<string>()
    {
        "as", "async", "await", "box", "break", "const", "continue", "crate", "dyn",
        "else", "enum", "extern", "false", "fn", "for", "if", "impl", "in", "let",
        "loop", "match", "mod", "move", "mut", "pub", "ref", "return", "self", "Self",
        "static", "struct", "super", "trait", "true", "try", "type", "unsafe", "use",
        "where", "while", "yield", "abstract", "become", "do", "final", "macro",
        "override", "priv", "typeof", "unsized", "virtual",
    };

    public static bool IsReserved(string name)
        => RustKeywords.Contains(name);

    /// <summary>
    /// Escapes identifiers (parameters, variables) that collide with Rust keywords.
    /// </summary>
    public static string EscapeName(string name)
        => IsReserved(name) ? $"_{name}" : name;

    public static string FieldNameToParameterName(string fieldName)
        => fieldName.Length == 0 || fieldName[0].IsLower() || IsReserved(fieldName.DecapitalizeFirst())
            ? $"_{fieldName}"
            : fieldName.DecapitalizeFirst();

    //==
    // Implementation of ITypeToRust

    public string ToRustTypeName(TypeInstance type)
    {
        var name = type.Name;

        if (TypeNameSubstitutions.TryGetValue(name, out var replacement))
            return replacement;

        if (name == "Self" && !string.IsNullOrEmpty(SelfType))
            return SelfType;

        if (name.StartsWith("Function") && name.Length > "Function".Length && char.IsDigit(name["Function".Length]))
            return RustWriter.FunctionTypeSentinel;

        if (RustWriter.NativePrimitives.TryGetValue(name, out var native))
            return native;

        if (RustWriter.TypeNameReplacements.TryGetValue(name, out var replaced))
            return replaced;

        // The array concept maps to the Vec-backed Arr struct in the prelude.
        if (ArrayInterfaceNames.Contains(name))
            return "Arr";

        return name;
    }
}
