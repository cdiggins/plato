using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.TypeScriptWriter;

/// <summary>
/// This can be used to build code for a specific type in TypeScript, or it can be used to build standalone
/// functions, in which case the type is null. Analog of CSharpTypeWriter.
/// </summary>
public class TypeScriptTypeWriter : CodeBuilder<TypeScriptTypeWriter>, ITypeToTypeScript
{
    public string SelfType;
    public string FullName;
    public TypeDef TypeDef;
    public TypeScriptWriter Writer;
    public PlatoAnalyzer Analyzer => Writer.Analyzer;

    /// <summary>
    /// Type variable renamings applied when resolving type names
    /// (used to map the element type variable of IArray functions to the Arr class parameter).
    /// </summary>
    public Dictionary<string, string> TypeNameSubstitutions { get; } = new Dictionary<string, string>();

    /// <summary>
    /// Member names already emitted: TypeScript does not support overloaded class members,
    /// so colliding functions are skipped.
    /// </summary>
    public HashSet<string> ClaimedNames { get; } = new HashSet<string>();

    public TypeScriptTypeWriter(TypeScriptWriter writer, TypeDef type)
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

        // The self type of a native primitive is the native type itself.
        SelfType = TypeScriptWriter.NativePrimitives.TryGetValue(type.Name, out var native)
            ? native
            : FullName;
    }

    public bool IsNativePrimitive => TypeDef != null && TypeScriptWriter.NativePrimitives.ContainsKey(TypeDef.Name);

    /// <summary>
    /// True when `name` is emitted on `type` as an INSTANCE member rather than a
    /// static — that is, when the library function reaching it names a receiver
    /// (`Origin(self: IOriginBased)`) instead of discarding one (`Tau(_: Number)`).
    /// A call written in static position has to be routed through a value for those.
    /// </summary>
    public bool IsInstanceMemberOf(TypeDef type, string name)
    {
        if (type == null || Writer?.Compilation == null)
            return false;
        var ct = Writer.Compilation.ConcreteTypes.FirstOrDefault(c => c.TypeDef == type);
        if (ct == null)
            return false;
        var matches = ct.ConcreteFunctions
            .Concat(ct.ImplementedFunctions)
            .Where(f => f.Name == name)
            .ToList();
        return matches.Count > 0
               && matches.All(f => f.ParameterNames.Count > 0 && f.ParameterNames[0] != "_");
    }

    public bool HasFieldNamed(string name)
        => HasFieldNamed(TypeDef, name);

    public bool HasFieldNamed(TypeDef type, string name)
        => type?.Fields.Any(f => f.Name == name) == true;

    public bool HasFieldNamed(TypeExpression type, string name)
        // The receiver's declared type may be a concept (no fields) even though the
        // ground receiver is the current concrete type, so check both.
        => type?.Def?.Fields.Any(f => f.Name == name) == true
           || TypeDef?.Fields.Any(f => f.Name == name) == true;

    public TypeScriptTypeWriter WriteConcreteType(ConcreteType ct)
    {
        Debug.Assert(ct.TypeDef == TypeDef);
        // Sum types (wave-2, plato-232) are C#-only in v1: the TypeScript writer has no tagged-union
        // lowering, so emit a clear rejection rather than garbage (design doc §6, CHK320).
        if (ct.TypeDef.IsSum)
        {
            WriteLine($"// CHK320: sum type '{ct.TypeDef.Name}' cannot be emitted to the TypeScript target; sum types are C#-only in v1");
            return this;
        }
        _ = new TypeScriptConcreteTypeWriter(this, ct);
        return this;
    }

    public TypeScriptTypeWriter WriteConceptInterface()
    {
        // When writing interfaces, the "Self" type is literally "Self".
        SelfType = "Self";

        var type = TypeDef;
        Debug.Assert(type.IsInterface());

        // The array interfaces have special implementations (see TypeScriptWriter).
        if (ArrayInterfaceNames.Contains(type.Name))
            return this;

        var inherits = type.Inherits.Select(this.ToTypeScriptType).ToList();
        var inherited = inherits.Count > 0 ? " extends " + inherits.JoinStringsWithComma() : "";

        Write("export interface ").Write(FullName).WriteLine(inherited);
        return WriteInterfaceFunctions(type);
    }

    public TypeScriptTypeWriter WriteInterfaceFunctions(TypeDef type)
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
            WriteLine(fi.MethodInterface());
        }
        return WriteEndBlock().WriteLine();
    }

    public TypeScriptFunctionInfo ToFunctionInfo(FunctionDef fd, TypeDef td, FunctionInstanceKind kind)
        => ToFunctionInfo(new FunctionInstance(fd, td, null, kind));

    public TypeScriptFunctionInfo ToFunctionInfo(FunctionInstance fi, TypeDef td = null)
        => new TypeScriptFunctionInfo(fi, td, this);

    public TypeScriptTypeWriter WriteBody(TypeScriptFunctionInfo f, bool isStatic)
        => WriteTrimmed(RenderBody(f, isStatic));

    /// <summary>
    /// Renders a function body to a string without the trailing newline
    /// (used when something must follow the body on the same line).
    /// </summary>
    public string BodyText(TypeScriptFunctionInfo f, bool isStatic)
        => RenderBody(f, isStatic).TrimEnd('\r', '\n');

    /// <summary>
    /// Renders a body from the Typed IR when one is available (Writer.UseTir): the ground
    /// monomorphized TIR for member bodies of a concrete type, the generic elaborated TIR for
    /// everything else (constants, Arr methods, free array functions — emitted unspecialized,
    /// with parameter #0 as `this` when not static). Legacy symbol-graph writer as the fallback.
    /// </summary>
    private string RenderBody(TypeScriptFunctionInfo f, bool isStatic)
    {
        if (Writer.UseTir && f.Function?.Implementation?.Body != null)
        {
            var tir = TypeDef != null && !isStatic
                ? Writer.TryGetGroundTir(f.Function.Implementation, TypeDef)
                    ?? Writer.TryGetStaticTir(f.Function.Implementation)
                : Writer.TryGetStaticTir(f.Function.Implementation);
            if (tir != null)
            {
                Writer.TirBodiesEmitted++;
                return new TirTypeScriptBodyWriter(this, tir, isStatic, f.ReturnType).ToString();
            }
            Writer.TirFallbackBodies++;
        }
        return new TypeScriptFunctionBodyWriter(this, f, isStatic, false).ToString();
    }

    /// <summary>
    /// Writes pre-rendered multi-line text, replacing the trailing newline with a
    /// WriteLine so that indentation resumes correctly afterwards.
    /// </summary>
    public TypeScriptTypeWriter WriteTrimmed(string s)
    {
        s = s.TrimEnd('\r', '\n');
        if (s.Length == 0)
            return this;
        return Write(s).WriteLine();
    }

    /// <summary>
    /// Writes a constant of the Constants class as a static getter.
    /// </summary>
    public TypeScriptTypeWriter WriteConstantFunction(TypeScriptFunctionInfo fi)
        => Write(fi.ConstantSignature).WriteBody(fi, true);

    /// <summary>
    /// Writes one of the IArray library functions: the declaration is appended to the
    /// IArray interface writer, and the implementation (with the first parameter mapped
    /// to "this") becomes a method of the Arr class.
    /// </summary>
    public TypeScriptTypeWriter WriteArrayMethod(FunctionInstance f, TypeScriptTypeWriter interfaceWriter)
    {
        if (!ClaimedNames.Add(f.Name))
            return WriteLine($"// Skipped: overload or duplicate member '{f.Name}'");

        var elemVar = f.ParameterTypes[0].ArgsWithSelf.LastOrDefault()?.Name;
        var exclude = new List<string>();
        if (elemVar != null)
        {
            TypeNameSubstitutions[elemVar] = "T";
            interfaceWriter.TypeNameSubstitutions[elemVar] = "T";
            exclude.Add(elemVar);
        }

        var fi = ToFunctionInfo(f);
        interfaceWriter.WriteLine(fi.MethodInterface(exclude));
        Write(fi.MethodSignature(exclude)).WriteBody(fi, false);

        if (elemVar != null)
        {
            TypeNameSubstitutions.Remove(elemVar);
            interfaceWriter.TypeNameSubstitutions.Remove(elemVar);
        }
        return this;
    }

    /// <summary>
    /// Writes an IArray library function over a concrete element type (e.g. Sum of an
    /// IArray of Number) as a module-level function: its signature is not generic in
    /// the element type, so it cannot become a method of the generic Arr class.
    ///
    /// Bodies still CALL these in UFCS receiver position (`points.BoundsOfPoints()`),
    /// because that is how they read in Plato, so the module function is paired with a
    /// forwarding install on Arr.prototype. Without it every such call is a runtime
    /// "not a function".
    /// </summary>
    public TypeScriptTypeWriter WriteFreeArrayFunction(FunctionInstance f)
    {
        if (!ClaimedNames.Add(f.Name))
            return WriteLine($"// Skipped: overload or duplicate function '{f.Name}'");

        var fi = ToFunctionInfo(f);
        Write(fi.FunctionSignature()).WriteBody(fi, true);
        return WriteLine($"Intrinsics.Install(Arr.prototype, '{fi.Name}', " +
                         $"function(this: unknown, ...args: never[]) {{ return ({fi.Name} as (...xs: never[]) => unknown)(this as never, ...args); }});");
    }

    /// <summary>
    /// Writes a member function of a concrete type (class syntax).
    /// Bodiless (intrinsic) functions get a native implementation when one is known,
    /// otherwise they throw at run-time.
    /// </summary>
    public TypeScriptTypeWriter WriteMemberFunction(TypeScriptFunctionInfo f)
    {
        if (f.Body == null)
        {
            if (TryGetIntrinsicBody(f, out var body))
                WriteLine($"{f.MethodSignature()} {{ {body} }}");
            else
                WriteLine($"{f.MethodSignature()} {{ return Intrinsics.ThrowNotImplemented('{TypeDef?.Name}.{f.Name}'); }}");
        }
        else
        {
            Write(f.MethodSignature());
            WriteBody(f, f.IsStatic);
        }
        return WriteStaticInstanceForwarder(f);
    }

    /// <summary>
    /// A Plato function whose receiver is the discarded "_" becomes a TS static, but
    /// call sites still invoke it through an instance (p.FromOffset(v)); the instance
    /// slot forwards to the static (the two namespaces are distinct in TS).
    /// </summary>
    public TypeScriptTypeWriter WriteStaticInstanceForwarder(TypeScriptFunctionInfo f)
    {
        if (!f.IsStatic || f.NumParameters == 0 || TypeDef == null)
            return this;
        var cls = TypeDef.Name;
        var args = f.MethodParameterNames.Select(EscapeName).JoinStringsWithComma();
        return WriteLine($"{f.Name}{f.GenericsString()}({f.MethodParameters.JoinStringsWithComma()}): {f.ReturnType} {{ return {cls}.{f.Name}({args}); }}");
    }

    /// <summary>
    /// Writes the At function (as a chain of conditions over the fields) and the
    /// Count function for array-like types.
    /// </summary>
    public TypeScriptTypeWriter GenerateFunc(TypeScriptFunctionInfo f, ConcreteType t)
    {
        var fs = t.TypeDef.Fields.Select(tf => tf.Name).ToList();

        if (f.Name == "At")
        {
            var p = EscapeName(f.ParameterNames[1]);
            var s = "";
            for (var i = 0; i < fs.Count; i++)
                s += $"{p} === {i} ? this.{fs[i]} : ";
            s += "Intrinsics.ThrowOutOfRange()";
            return WriteLine($"{f.MethodSignature()} {{ return {s}; }}");
        }

        if (f.Name == "Count")
            return WriteLine($"Count(): number {{ return {fs.Count}; }}");

        throw new Exception("Only 'At' or 'Count' supported");
    }

    /// <summary>
    /// Native TypeScript implementations for the bodiless intrinsic functions
    /// (the C# writer relies on hand-written partial structs for these).
    /// The receiver is "this": a native value for primitives, a class otherwise.
    /// </summary>
    public bool TryGetIntrinsicBody(TypeScriptFunctionInfo f, out string body)
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
                body = $"return Math.trunc(this / {arg});";
                return true;
            case "Integer.ToNumber":
                body = "return this;";
                return true;
            case "Integer.Range":
                body = "return Intrinsics.Range(this);";
                return true;
            case "Number.Compare":
            case "Integer.Compare":
            case "String.Compare":
            case "Character.Compare":
                body = $"return this < {arg} ? -1 : this > {arg} ? 1 : 0;";
                return true;
            case "Number.Pi":
                body = "return Math.PI;";
                return true;
            case "Number.MinValue":
                body = "return -globalThis.Number.MAX_VALUE;";
                return true;
            case "Number.MaxValue":
                body = "return globalThis.Number.MAX_VALUE;";
                return true;
            case "Number.Epsilon":
                body = "return globalThis.Number.MIN_VALUE;";
                return true;
            case "Number.Sqrt":
                body = "return Math.sqrt(this);";
                return true;
            case "Number.Abs":
                body = "return Math.abs(this);";
                return true;
            case "Number.Floor":
                body = "return Math.floor(this);";
                return true;
            case "Number.Ceiling":
                body = "return Math.ceil(this);";
                return true;
            case "Number.Round":
                body = "return Math.round(this);";
                return true;
            case "Number.Truncate":
                body = "return Math.trunc(this);";
                return true;
            case "Number.Acos":
                body = "return new Angle(Math.acos(this));";
                return true;
            case "Number.Asin":
                body = "return new Angle(Math.asin(this));";
                return true;
            case "Number.Atan":
                body = "return new Angle(Math.atan(this));";
                return true;
            case "Number.Pow":
                body = $"return Math.pow(this, {arg});";
                return true;
            case "Number.Log":
                body = $"return Math.log(this) / Math.log({arg});";
                return true;
            case "Number.Ln":
                body = "return Math.log(this);";
                return true;
            case "Number.Exp":
                body = "return Math.exp(this);";
                return true;
            case "Number.Sin":
                body = "return Math.sin(this);";
                return true;
            case "Number.Cos":
                body = "return Math.cos(this);";
                return true;
            case "Number.NaturalLog":
                body = "return Math.log(this);";
                return true;
            case "Number.Atan2Radians":
                body = $"return Math.atan2(this, {arg});";
                return true;
            case "Number.FusedMultiplyAdd":
                body = $"return this * {arg} + {EscapeName(f.ParameterNames.Count > 2 ? f.ParameterNames[2] : "z")};";
                return true;
            case "Number.Hash":
                body = "return (this * 2654435761) | 0;";
                return true;
            case "Number.IsNaN":
                body = "return this !== this;";
                return true;
            case "Number.IsInfinite":
                body = "return this === Infinity || this === -Infinity;";
                return true;
            case "Number.ToInteger":
                body = "return Math.trunc(this);";
                return true;
            case "Integer.BitwiseNot":
                body = "return ~this;";
                return true;
            case "Integer.BitwiseXor":
                body = $"return this ^ {arg};";
                return true;
            case "Integer.ShiftLeft":
                body = $"return this << {arg};";
                return true;
            case "Integer.ShiftRight":
                body = $"return this >> {arg};";
                return true;
            case "Angle.Cos":
                body = "return Math.cos(this.Radians);";
                return true;
            case "Angle.Sin":
                body = "return Math.sin(this.Radians);";
                return true;
            case "Angle.Tan":
                body = "return Math.tan(this.Radians);";
                return true;
            case "String.At":
                body = $"return this.charAt({arg});";
                return true;
            case "String.Count":
                body = "return this.length;";
                return true;
        }

        // Operators on the native primitives are implemented with native operators.
        if (IsNativePrimitive && f.IsOperator)
        {
            var op = f.OperatorName;
            var isNativeResult = f.ReturnType == "number" || f.ReturnType == "boolean" || f.ReturnType == "string";
            if (!isNativeResult)
                return false;

            if (f.NumParameters == 1)
            {
                body = $"return {op}this;";
                return true;
            }

            if (f.NumParameters == 2)
            {
                // "==" and "!=" become strict equality in TypeScript.
                if (op == "==") op = "===";
                if (op == "!=") op = "!==";
                body = $"return this {op} {arg};";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The array interface appears as "Array" or "IArray" depending on the source dialect.
    /// Both map to the special IArray implementation.
    /// </summary>
    public static HashSet<string> ArrayInterfaceNames = new HashSet<string>()
    {
        "Array", "Array2D", "Array3D",
        "IArray", "IArray2D", "IArray3D",
    };

    public static HashSet<string> TypeScriptKeywords = new HashSet<string>()
    {
        "break", "case", "catch", "class", "const", "continue", "debugger", "default", "delete",
        "do", "else", "enum", "export", "extends", "false", "finally", "for", "function", "if",
        "import", "in", "instanceof", "new", "null", "return", "super", "switch", "this", "throw",
        "true", "try", "typeof", "var", "void", "while", "with",
        "as", "implements", "interface", "let", "package", "private", "protected", "public",
        "static", "yield", "any", "boolean", "number", "string", "symbol", "object", "never",
        "unknown", "undefined", "async", "await", "of", "arguments", "eval",
    };

    public static bool IsReserved(string name)
        => TypeScriptKeywords.Contains(name);

    /// <summary>
    /// Escapes identifiers (parameters, variables) that collide with TypeScript keywords.
    /// </summary>
    public static string EscapeName(string name)
        => IsReserved(name) ? $"_{name}" : name;

    public static string FieldNameToParameterName(string fieldName)
        => fieldName.Length == 0 || fieldName[0].IsLower() || IsReserved(fieldName.DecapitalizeFirst())
            ? $"_{fieldName}"
            : fieldName.DecapitalizeFirst();

    //==
    // Implementation of ITypeToTypeScript

    public string ToTypeScriptTypeName(TypeInstance type)
    {
        var name = type.Name;

        if (TypeNameSubstitutions.TryGetValue(name, out var replacement))
            return replacement;

        if (name == "Self" && !string.IsNullOrEmpty(SelfType))
            return SelfType;

        if (name.StartsWith("Function") && name.Length > "Function".Length && char.IsDigit(name["Function".Length]))
            return TypeScriptWriter.FunctionTypeSentinel;

        if (TypeScriptWriter.NativePrimitives.TryGetValue(name, out var native))
            return native;

        if (TypeScriptWriter.TypeNameReplacements.TryGetValue(name, out var replaced))
            return replaced;

        // The array interface maps to the special IArray implementation
        // (and avoids shadowing the JavaScript global Array).
        if (ArrayInterfaceNames.Contains(name))
            return name.StartsWith("I") ? name : "I" + name;

        return name;
    }
}
