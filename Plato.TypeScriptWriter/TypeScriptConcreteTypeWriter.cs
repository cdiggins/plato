using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.TypeScriptWriter
{
    /// <summary>
    /// Writes a Plato concrete type as TypeScript. Analog of CSharpConcreteTypeWriter.
    ///
    /// Two output shapes:
    /// - The Plato primitives (Number, Integer, Boolean, String, Character) map to
    ///   native TypeScript types. Their functions are installed on the native
    ///   prototypes (with a matching "declare global" interface augmentation),
    ///   which is what enables fluent syntax on plain values: (0.5).Clamp(0, 1).
    /// - Every other type becomes a class whose declared fields are readonly
    ///   constructor properties. All functions are methods (single-parameter
    ///   functions become zero-argument methods, never getters), matching the
    ///   extension-method convention on the C# side.
    ///
    /// TypeScript classes do not support overloaded members, so when two functions
    /// collide on a name, only the first is emitted.
    /// </summary>
    public class TypeScriptConcreteTypeWriter
    {
        public TypeScriptTypeWriter TypeWriter { get; }
        public TypeScriptWriter Writer => TypeWriter.Writer;
        public PlatoAnalyzer Analyzer => TypeWriter.Analyzer;
        public ConcreteType ConcreteType { get; }

        public string TypeParamsStr => ConcreteType.TypeDef.TypeParameters.Count > 0
            ? "<" + ConcreteType.TypeDef.TypeParameters.Select(tp => tp.Name).JoinStringsWithComma() + ">"
            : "";

        public string SimpleName => ConcreteType.Name;
        public string Name => SimpleName + TypeParamsStr;
        public bool IsNative => TypeScriptWriter.NativePrimitives.ContainsKey(SimpleName);
        public bool IsArrayLike => ConcreteType.AllInterfaces.Any(te => te.Name == "IArrayLike");
        public Compilation Compilation => TypeWriter.Writer.Compilation;
        public List<string> FieldNames { get; }
        public List<string> FieldTypes { get; }

        /// <summary>
        /// Names already used by a member of the class (or native prototype).
        /// </summary>
        public HashSet<string> MemberNames { get; }

        public TypeScriptConcreteTypeWriter(TypeScriptTypeWriter typeWriter, ConcreteType t)
        {
            ConcreteType = t;
            TypeWriter = typeWriter;

            FieldTypes = ConcreteType.TypeDef.Fields.Select(f => TypeWriter.ToTypeScriptType(f.Type)).ToList();
            FieldNames = ConcreteType.TypeDef.Fields.Select(f => f.Name).ToList();
            Debug.Assert(FieldTypes.Count == FieldNames.Count);

            if (IsNative)
            {
                // Number and Integer (and String and Character) share a prototype:
                // the claimed-name set is shared so collisions are skipped.
                var native = TypeScriptWriter.NativePrimitives[SimpleName];
                MemberNames = Writer.GetNativeClaimedNames(TypeScriptWriter.NativeInterfaces[native]);
                WriteNativePrototypeExtensions(native);
                return;
            }

            MemberNames = new HashSet<string>();
            foreach (var fn in FieldNames)
                MemberNames.Add(fn);
            MemberNames.Add("Create");
            MemberNames.Add("Default");
            MemberNames.Add("Equals");
            MemberNames.Add("NotEquals");
            MemberNames.Add("toString");

            TypeWriter.Write($"export class {Name}");
            TypeWriter.WriteLine();
            TypeWriter.WriteStartBlock();

            WriteClassCore();

            if (IsArrayLike)
                WriteArrayLikeFunctions();

            WriteImplementedInterfaceFunctions();

            WriteUnimplementedInterfaceFunctions();

            TypeWriter.WriteEndBlock();
            TypeWriter.WriteLine();
        }

        // ---- Native primitives: prototype extension --------------------------------

        /// <summary>
        /// Collects the member functions in the same order the class path would
        /// generate them: best implementation per interface group, then the
        /// unimplemented ones.
        /// </summary>
        public List<FunctionInstance> CollectMemberFunctions()
        {
            var result = new List<FunctionInstance>();
            foreach (var g in ConcreteType.InterfaceFunctionGroups)
            {
                var f = Analyzer.ChooseBestFunction(g, out _);
                if (!SkipFunction(f))
                    result.Add(f);
            }
            foreach (var f in ConcreteType.UnimplementedFunctions)
                if (!SkipFunction(f))
                    result.Add(f);
            return result;
        }

        public void WriteNativePrototypeExtensions(string native)
        {
            var iface = TypeScriptWriter.NativeInterfaces[native];
            var functions = new List<TypeScriptFunctionInfo>();
            foreach (var f in CollectMemberFunctions())
            {
                var fi = TypeWriter.ToFunctionInfo(f, ConcreteType.TypeDef);
                if (fi.IsStatic)
                    continue; // no statics on native prototypes
                if (!MemberNames.Add(fi.Name))
                    continue; // shared prototype (e.g. Number + Integer): first wins
                functions.Add(fi);
            }

            TypeWriter.WriteLine($"// ==== Plato {SimpleName} mapped to the native '{native}' type ====");
            if (functions.Count == 0)
                return;

            TypeWriter.WriteLine("declare global");
            TypeWriter.WriteStartBlock();
            TypeWriter.WriteLine($"interface {iface}");
            TypeWriter.WriteStartBlock();
            foreach (var fi in functions)
                TypeWriter.WriteLine(fi.MethodInterface());
            TypeWriter.WriteEndBlock();
            TypeWriter.WriteEndBlock();

            foreach (var fi in functions)
            {
                TypeWriter.Write($"Intrinsics.Install({iface}.prototype, '{fi.Name}', {fi.PrototypeFunction(native)}");
                if (fi.Body == null)
                {
                    TypeWriter.Write(TypeWriter.TryGetIntrinsicBody(fi, out var body)
                        ? $" {{ {body} }}"
                        : $" {{ return Intrinsics.ThrowNotImplemented('{SimpleName}.{fi.Name}'); }}");
                }
                else
                {
                    TypeWriter.Write(TypeWriter.BodyText(fi, false));
                }
                TypeWriter.WriteLine(");");
            }
            TypeWriter.WriteLine();
        }

        // ---- Classes ----------------------------------------------------------------

        public void WriteClassCore()
        {
            var parameterNames = FieldNames.Select(TypeScriptTypeWriter.FieldNameToParameterName).ToList();
            var parameters = FieldTypes.Zip(parameterNames, (pt, pn) => $"{pn}: {pt}").JoinStringsWithComma();
            var parameterNamesStr = parameterNames.JoinStringsWithComma();

            if (FieldNames.Count > 0)
            {
                TypeWriter.WriteLine("// Fields are readonly constructor parameter properties");
                var ctorParams = FieldNames.Zip(FieldTypes, (fn, ft) => $"public readonly {fn}: {ft}").JoinStringsWithComma();
                TypeWriter.WriteLine($"constructor({ctorParams}) {{}}");
                TypeWriter.WriteLine();

                TypeWriter.WriteLine("// With functions");
                for (var i = 0; i < FieldTypes.Count; ++i)
                {
                    var ft = FieldTypes[i];
                    var fn = FieldNames[i];
                    var pn = parameterNames[i];
                    var args = FieldNames.Select((n, j) => j == i ? pn : $"this.{n}").JoinStringsWithComma();
                    MemberNames.Add($"With{fn}");
                    TypeWriter.WriteLine($"With{fn}({pn}: {ft}): {Name} {{ return new {Name}({args}); }}");
                }
                TypeWriter.WriteLine();
            }

            TypeWriter.WriteLine("// Static factory function");
            var ctorArgs = FieldNames.Count > 0 ? parameterNamesStr : "";
            TypeWriter.WriteLine($"static Create{TypeParamsStr}({parameters}): {Name} {{ return new {Name}({ctorArgs}); }}");

            if (CanWriteDefault())
            {
                var defaults = FieldTypes.Select(DefaultValueFor).JoinStringsWithComma();
                TypeWriter.WriteLine($"static get Default(): {Name} {{ return new {Name}({defaults}); }}");
            }
            TypeWriter.WriteLine();

            TypeWriter.WriteLine("// Equality and printing");
            if (FieldNames.Count > 0)
            {
                var eqBody = FieldNames.Select(f => $"Intrinsics.Eq(this.{f}, other.{f})").JoinStrings(" && ");
                TypeWriter.WriteLine($"Equals(other: {Name}): boolean {{ return {eqBody}; }}");
            }
            else
            {
                TypeWriter.WriteLine($"Equals(other: {Name}): boolean {{ return true; }}");
            }
            TypeWriter.WriteLine($"NotEquals(other: {Name}): boolean {{ return !this.Equals(other); }}");

            var toStr = "`{ " + FieldNames.Select(fn => $"{fn} = ${{this.{fn}}}").JoinStringsWithComma() + " }`";
            TypeWriter.WriteLine($"toString(): string {{ return {toStr}; }}");
            TypeWriter.WriteLine();
        }

        public static string DefaultValueFor(string tsType)
            => TypeScriptWriter.NativeDefaults.TryGetValue(tsType, out var d) ? d : $"{tsType}.Default";

        /// <summary>
        /// The Default getter can only be written when every field type also has a
        /// default: native primitives and other concrete classes do; generic types,
        /// interface typed fields, and function typed fields do not.
        /// </summary>
        public bool CanWriteDefault()
        {
            if (ConcreteType.TypeDef.TypeParameters.Count > 0)
                return false;

            foreach (var f in ConcreteType.TypeDef.Fields)
            {
                if (f.Type.Def == null || f.Type.Def.IsInterface())
                    return false;
            }

            // Types written with generic arguments or arrow syntax have no Default.
            return FieldTypes.All(ft => ft.All(c => char.IsLetterOrDigit(c) || c == '_'));
        }

        public void WriteArrayLikeFunctions()
        {
            var numDistinctFieldTypes = FieldTypes.Distinct().Count();
            if (numDistinctFieldTypes > 1)
                throw new Exception("IArrayLike types are assumed to have all of the fields of the same type");

            if (FieldTypes.Count == 0)
                return;

            MemberNames.Add("NumComponents");
            MemberNames.Add("Components");
            MemberNames.Add("CreateFromComponents");
            MemberNames.Add("CreateFromComponent");

            var fieldType = FieldTypes[0];
            var nComps = FieldNames.Count;

            TypeWriter.WriteLine("// IArrayLike predefined functions");
            TypeWriter.WriteLine($"NumComponents(): number {{ return {nComps}; }}");
            var components = FieldNames.Select(f => $"this.{f}").JoinStringsWithComma();
            TypeWriter.WriteLine($"Components(): IArray<{fieldType}> {{ return Intrinsics.MakeArray({components}); }}");
            {
                var args = Enumerable.Range(0, nComps).Select(i => $"xs.At({i})").JoinStringsWithComma();
                TypeWriter.WriteLine($"static CreateFromComponents(xs: IArray<{fieldType}>): {Name} {{ return new {Name}({args}); }}");
            }
            {
                var args = Enumerable.Range(0, nComps).Select(i => "x").JoinStringsWithComma();
                TypeWriter.WriteLine($"static CreateFromComponent(x: {fieldType}): {Name} {{ return new {Name}({args}); }}");
            }
            TypeWriter.WriteLine();
        }

        public bool SkipFunction(FunctionInstance f, bool skipFields = true)
        {
            // Note: we skip functions that are named after a field ...
            if (skipFields && FieldNames.Contains(f.Name))
                return true;

            if (TypeScriptWriter.IgnoredFunctions.Contains(f.Name))
                return true;

            // Unlike C# (where IReadOnlyList supplies the array members), the generated
            // classes need At and Count; other array functions live on Arr / IArray.
            if ((f.InterfaceName == "IArray" || f.InterfaceName == "Array")
                && f.Name != "At" && f.Name != "Count")
                return true;

            // We have to be sure to not implement functions that cast to themselves
            if (f.Name == SimpleName)
                return true;

            return false;
        }

        /// <summary>
        /// Claims a member name; returns false (and writes a comment) when the name is
        /// already used, since TypeScript classes cannot overload members.
        /// </summary>
        public bool TryClaimMember(string name)
        {
            if (MemberNames.Add(name))
                return true;
            TypeWriter.WriteLine($"// Skipped: overload or duplicate member '{name}'");
            return false;
        }

        public void WriteImplementedInterfaceFunctions()
        {
            TypeWriter.WriteLine("// Implemented interface functions");
            foreach (var g in ConcreteType.InterfaceFunctionGroups)
            {
                var f = Analyzer.ChooseBestFunction(g, out var cnt);

                if (SkipFunction(f))
                    continue;

                if (cnt > 1)
                {
                    TypeWriter.WriteLine($"// AMBIGUOUS FUNCTIONS {cnt}");
                    foreach (var tmp in g)
                        TypeWriter.WriteLine($"/* {tmp.Implementation} */");
                }

                if (!TryClaimMember(f.Name))
                    continue;

                var fi = TypeWriter.ToFunctionInfo(f, ConcreteType.TypeDef);
                TypeWriter.WriteMemberFunction(fi);
            }
            TypeWriter.WriteLine();
        }

        public void WriteUnimplementedInterfaceFunctions()
        {
            TypeWriter.WriteLine("// Unimplemented interface functions");
            foreach (var f in ConcreteType.UnimplementedFunctions)
            {
                if (SkipFunction(f))
                    continue;

                if (!TryClaimMember(f.Name))
                    continue;

                var fi = TypeWriter.ToFunctionInfo(f, ConcreteType.TypeDef);

                if (f.Name == "At" || f.Name == "Count")
                {
                    // Generated from the fields.
                    TypeWriter.GenerateFunc(fi, ConcreteType);
                }
                else
                {
                    TypeWriter.WriteMemberFunction(fi);
                }
            }
        }
    }
}
