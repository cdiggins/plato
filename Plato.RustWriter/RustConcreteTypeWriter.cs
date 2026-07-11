using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.RustWriter
{
    /// <summary>
    /// Writes a Plato concrete type as Rust. Analog of TypeScriptConcreteTypeWriter.
    ///
    /// Two output shapes:
    /// - The Plato primitives (Number, Integer, Boolean, String, Character) map to
    ///   native Rust types. Their functions become an extension trait
    ///   (NumberExt for f64, ...), which is what enables fluent syntax on plain
    ///   values: (0.5).Turns().Cos(). Because Number (f64) and Integer (i64) are
    ///   distinct Rust types, their function sets do not collide (unlike
    ///   TypeScript, where both share Number.prototype).
    /// - Every other type becomes a Copy struct with public fields and an
    ///   inherent impl block. All functions are methods taking self by value
    ///   (single-parameter functions become zero-argument methods).
    ///
    /// Rust does not support overloaded functions, so when two functions
    /// collide on a name, only the first is emitted.
    /// </summary>
    public class RustConcreteTypeWriter
    {
        public RustTypeWriter TypeWriter { get; }
        public RustWriter Writer => TypeWriter.Writer;
        public PlatoAnalyzer Analyzer => TypeWriter.Analyzer;
        public ConcreteType ConcreteType { get; }

        public string TypeParamsStr => ConcreteType.TypeDef.TypeParameters.Count > 0
            ? "<" + ConcreteType.TypeDef.TypeParameters.Select(tp => tp.Name).JoinStringsWithComma() + ">"
            : "";

        /// <summary>
        /// Type parameters with the bounds required by the generated members
        /// (Copy for the receiver convention, PartialEq for Equals).
        /// </summary>
        public string BoundedTypeParamsStr => ConcreteType.TypeDef.TypeParameters.Count > 0
            ? "<" + ConcreteType.TypeDef.TypeParameters.Select(tp => $"{tp.Name}: Copy + PartialEq").JoinStringsWithComma() + ">"
            : "";

        public string SimpleName => ConcreteType.Name;
        public string Name => SimpleName + TypeParamsStr;
        public bool IsNative => RustWriter.NativePrimitives.ContainsKey(SimpleName);
        public bool IsArrayLike => ConcreteType.AllInterfaces.Any(te => te.Name == "IArrayLike");
        public Compilation Compilation => TypeWriter.Writer.Compilation;
        public List<string> FieldNames { get; }
        public List<string> FieldTypes { get; }

        /// <summary>
        /// Names already used by a member of the type (or extension trait).
        /// </summary>
        public HashSet<string> MemberNames { get; }

        public RustConcreteTypeWriter(RustTypeWriter typeWriter, ConcreteType t)
        {
            ConcreteType = t;
            TypeWriter = typeWriter;

            FieldTypes = ConcreteType.TypeDef.Fields.Select(f => TypeWriter.ToRustType(f.Type)).ToList();
            FieldNames = ConcreteType.TypeDef.Fields.Select(f => f.Name).ToList();
            Debug.Assert(FieldTypes.Count == FieldNames.Count);

            if (IsNative)
            {
                var native = RustWriter.NativePrimitives[SimpleName];
                MemberNames = Writer.GetNativeClaimedNames(SimpleName);
                WriteNativeExtensionTrait(native);
                return;
            }

            MemberNames = new HashSet<string>();
            foreach (var fn in FieldNames)
                MemberNames.Add(fn);
            MemberNames.Add("new");
            MemberNames.Add("Create");
            MemberNames.Add("Default");
            MemberNames.Add("Equals");
            MemberNames.Add("NotEquals");

            WriteStructAndImpl();
        }

        // ---- Native primitives: extension trait -------------------------------------

        /// <summary>
        /// Collects the member functions in the same order the struct path would
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

        public void WriteNativeExtensionTrait(string native)
        {
            var traitName = RustWriter.NativeExtensionTraits[SimpleName];
            var functions = new List<RustFunctionInfo>();
            foreach (var f in CollectMemberFunctions())
            {
                var fi = TypeWriter.ToFunctionInfo(f, ConcreteType.TypeDef);
                if (fi.IsStatic)
                    continue; // no associated functions on extension traits
                if (!MemberNames.Add(fi.Name))
                    continue; // true duplicate within this trait: first wins
                functions.Add(fi);
            }

            TypeWriter.WriteLine($"// ==== Plato {SimpleName} mapped to the native '{native}' type ====");
            if (functions.Count == 0)
            {
                TypeWriter.WriteLine();
                return;
            }

            TypeWriter.WriteLine($"pub trait {traitName}");
            TypeWriter.WriteStartBlock();
            foreach (var fi in functions)
                TypeWriter.WriteLine(fi.TraitItem());
            TypeWriter.WriteEndBlock();
            TypeWriter.WriteLine();

            TypeWriter.WriteLine($"impl {traitName} for {native}");
            TypeWriter.WriteStartBlock();
            foreach (var fi in functions)
                WriteTraitImplFunction(fi);
            TypeWriter.WriteEndBlock();
            TypeWriter.WriteLine();
        }

        public void WriteTraitImplFunction(RustFunctionInfo fi)
        {
            if (fi.Body == null)
            {
                TypeWriter.WriteLine(TypeWriter.TryGetIntrinsicBody(fi, out var body)
                    ? $"{fi.MethodSignature(false)} {{ {body} }}"
                    : $"{fi.MethodSignature(false)} {{ unimplemented!(\"{SimpleName}.{fi.Name}\") }}");
                return;
            }

            TypeWriter.Write(fi.MethodSignature(false));
            TypeWriter.WriteBody(fi, false);
        }

        // ---- Structs ------------------------------------------------------------------

        public void WriteStructAndImpl()
        {
            var derives = "Clone, Copy, PartialEq, Debug";
            if (CanDeriveDefault())
                derives += ", Default";

            TypeWriter.WriteLine($"#[derive({derives})]");
            if (FieldNames.Count == 0)
            {
                TypeWriter.WriteLine($"pub struct {Name};");
            }
            else
            {
                TypeWriter.WriteLine($"pub struct {Name}");
                TypeWriter.WriteStartBlock();
                for (var i = 0; i < FieldNames.Count; i++)
                    TypeWriter.WriteLine($"pub {FieldNames[i]}: {FieldTypes[i]},");
                TypeWriter.WriteEndBlock();
            }
            TypeWriter.WriteLine();

            TypeWriter.WriteLine($"impl{BoundedTypeParamsStr} {Name}");
            TypeWriter.WriteStartBlock();

            WriteImplCore();

            if (IsArrayLike)
                WriteArrayLikeFunctions();

            WriteImplementedInterfaceFunctions();

            WriteUnimplementedInterfaceFunctions();

            TypeWriter.WriteEndBlock();
            TypeWriter.WriteLine();
        }

        public void WriteImplCore()
        {
            var parameterNames = FieldNames.Select(RustTypeWriter.FieldNameToParameterName).ToList();
            var parameters = FieldTypes.Zip(parameterNames, (pt, pn) => $"{pn}: {pt}").JoinStringsWithComma();
            var parameterNamesStr = parameterNames.JoinStringsWithComma();

            TypeWriter.WriteLine("// Constructor");
            if (FieldNames.Count > 0)
            {
                var fieldInits = FieldNames.Zip(parameterNames, (fn, pn) => $"{fn}: {pn}").JoinStringsWithComma();
                TypeWriter.WriteLine($"pub fn new({parameters}) -> {Name} {{ {SimpleName} {{ {fieldInits} }} }}");
                TypeWriter.WriteLine();

                TypeWriter.WriteLine("// With functions");
                for (var i = 0; i < FieldTypes.Count; ++i)
                {
                    var ft = FieldTypes[i];
                    var fn = FieldNames[i];
                    var pn = parameterNames[i];
                    var args = FieldNames.Select((n, j) => j == i ? pn : $"self.{n}").JoinStringsWithComma();
                    MemberNames.Add($"With{fn}");
                    TypeWriter.WriteLine($"pub fn With{fn}(self, {pn}: {ft}) -> {Name} {{ {SimpleName}::new({args}) }}");
                }
            }
            else
            {
                TypeWriter.WriteLine($"pub fn new() -> {Name} {{ {SimpleName} }}");
            }
            TypeWriter.WriteLine();

            TypeWriter.WriteLine("// Static factory function");
            TypeWriter.WriteLine($"pub fn Create({parameters}) -> {Name} {{ {SimpleName}::new({parameterNamesStr}) }}");
            TypeWriter.WriteLine();

            TypeWriter.WriteLine("// Equality (via PartialEq)");
            TypeWriter.WriteLine($"pub fn Equals(self, other: {Name}) -> bool {{ self == other }}");
            TypeWriter.WriteLine($"pub fn NotEquals(self, other: {Name}) -> bool {{ self != other }}");
            TypeWriter.WriteLine();
        }

        /// <summary>
        /// Default is derived when every field type can also derive it: struct
        /// and native primitive fields do; interface-typed and function-typed
        /// fields do not. Generic fields are fine (the derive adds bounds).
        /// </summary>
        public bool CanDeriveDefault()
        {
            foreach (var f in ConcreteType.TypeDef.Fields)
            {
                if (f.Type.Def != null && f.Type.Def.IsInterface())
                    return false;
            }
            return FieldTypes.All(ft => !ft.Contains("Fn"));
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
            TypeWriter.WriteLine($"pub fn NumComponents(self) -> i64 {{ {nComps} }}");
            var components = FieldNames.Select(f => $"self.{f}").JoinStringsWithComma();
            TypeWriter.WriteLine($"pub fn Components(self) -> Arr<{fieldType}> {{ Intrinsics::MakeArray(vec![{components}]) }}");
            {
                var args = Enumerable.Range(0, nComps).Select(i => $"xs.At({i})").JoinStringsWithComma();
                TypeWriter.WriteLine($"pub fn CreateFromComponents(xs: Arr<{fieldType}>) -> {Name} {{ {SimpleName}::new({args}) }}");
            }
            {
                var args = Enumerable.Range(0, nComps).Select(i => "x").JoinStringsWithComma();
                TypeWriter.WriteLine($"pub fn CreateFromComponent(x: {fieldType}) -> {Name} {{ {SimpleName}::new({args}) }}");
            }
            TypeWriter.WriteLine();
        }

        public bool SkipFunction(FunctionInstance f, bool skipFields = true)
        {
            // Note: we skip functions that are named after a field ...
            if (skipFields && FieldNames.Contains(f.Name))
                return true;

            if (RustWriter.IgnoredFunctions.Contains(f.Name))
                return true;

            // The generated structs need At and Count; other array functions live
            // on the Arr struct.
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
        /// already used, since Rust impl blocks cannot overload functions.
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

                // (The old "AMBIGUOUS FUNCTIONS" debug comments were retired: they leaked the
                // volatile Symbol-id counter into generated output — same cleanup as the C# writer.)
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
