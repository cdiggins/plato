using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Utils;

namespace Ara3D.Geometry.RustWriter
{
    /// <summary>
    /// Generates Rust source code from a Plato compilation.
    /// This is the Rust analog of Plato.TypeScriptWriter.TypeScriptWriter.
    ///
    /// Output model:
    /// - The Plato primitives map to native Rust types (Number -> f64,
    ///   Integer -> i64, Boolean -> bool, String -> String, Character -> char).
    ///   Their Plato functions become extension traits (NumberExt for f64, ...)
    ///   giving fluent syntax on plain values: (0.5).Turns().Cos(), x.Sqrt().
    ///   Unlike TypeScript, Number and Integer are distinct types, so their
    ///   function sets do not collide.
    /// - Concrete types become Copy structs with public fields and inherent
    ///   impl blocks. Everything is a method, never a getter: single-parameter
    ///   Plato functions become zero-argument methods (v.Length()).
    /// - The generated Plato API keeps PascalCase for parity with C# and
    ///   TypeScript, enabled by #![allow(non_snake_case)].
    /// - A single self-contained module (plato.rs) is produced.
    /// - The IArray concept maps to a Vec-backed Arr struct defined in the
    ///   hand-written prelude.
    /// </summary>
    public class RustWriter : CodeBuilder<RustWriter>
    {
        /// <summary>
        /// Returned by ToRustTypeName to indicate that the type must be written
        /// using Rust "impl Fn" closure syntax.
        /// </summary>
        public const string FunctionTypeSentinel = "$function";

        public RustWriter(Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            Analyzer = new PlatoAnalyzer(compilation);
            OutputFolder = outputFolder;

            foreach (var ct in compilation.ConcreteTypes)
                foreach (var f in ct.TypeDef.Fields)
                    AllFieldNames.Add(f.Name);
        }

        public Compiler.Compilation Compilation => Analyzer.Compilation;
        public PlatoAnalyzer Analyzer { get; }
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        public DirectoryPath OutputFolder { get; }

        /// <summary>
        /// The names of every declared field across the compilation. The body
        /// writer uses this to distinguish field access ("v.X") from
        /// function calls (always parenthesized: "v.Length()").
        /// </summary>
        public HashSet<string> AllFieldNames { get; } = new HashSet<string>();

        /// <summary>
        /// Member names already claimed per Plato primitive type ("Number",
        /// "Integer", ...). Each primitive maps to a distinct Rust type with its
        /// own extension trait, so unlike TypeScript the sets are not shared;
        /// the claim set only guards against true duplicates within one trait.
        /// </summary>
        public Dictionary<string, HashSet<string>> NativeClaimedNames { get; } = new Dictionary<string, HashSet<string>>();

        public HashSet<string> GetNativeClaimedNames(string platoTypeName)
        {
            if (!NativeClaimedNames.TryGetValue(platoTypeName, out var set))
                NativeClaimedNames[platoTypeName] = set = new HashSet<string>();
            return set;
        }

        public static HashSet<string> IgnoredTypes = new HashSet<string>()
        {
            "Dynamic",
            "Type",
            "Array",
            "Array2D",
            "Array3D",
            "Function0",
            "Function1",
            "Function2",
            "Function3",
            "Function4",
            "Function5",
            "Function6",
            "Function7",
            "Function8",
            "Function9",
            "Function10",
        };

        public static HashSet<string> IgnoredFunctions = new HashSet<string>()
        {
            "FieldNames",
            "FieldValues",
            "TypeName",
            "Equals",
            "NotEquals",
            "GetHashCode",
            "ToString",
            "GetType",
            // These are functions of IArrayLike
            "Components",
            "CreateFromComponents",
            "CreateFromComponent",
            "NumComponents",

            // Implemented in the intrinsics prelude
            "Range",
            "MakeArray2D",
            "MapRange",
        };

        /// <summary>
        /// Plato types represented directly by a native Rust type.
        /// Their functions become methods of an extension trait implemented
        /// for the native type.
        /// </summary>
        public static Dictionary<string, string> NativePrimitives = new Dictionary<string, string>()
        {
            { "Number", "f64" },
            { "Integer", "i64" },
            { "Boolean", "bool" },
            { "String", "String" },
            { "Character", "char" },
        };

        /// <summary>
        /// The extension trait generated for each Plato primitive.
        /// </summary>
        public static Dictionary<string, string> NativeExtensionTraits = new Dictionary<string, string>()
        {
            { "Number", "NumberExt" },
            { "Integer", "IntegerExt" },
            { "Boolean", "BooleanExt" },
            { "String", "StringExt" },
            { "Character", "CharacterExt" },
        };

        public static Dictionary<string, string> NativeDefaults = new Dictionary<string, string>()
        {
            { "f64", "0.0" },
            { "i64", "0" },
            { "bool", "false" },
            { "String", "String::new()" },
            { "char", "'\\0'" },
        };

        /// <summary>
        /// Direct name replacements for types that are not generated as structs.
        /// </summary>
        public static Dictionary<string, string> TypeNameReplacements = new Dictionary<string, string>()
        {
            { "Dynamic", "()" },
            { "Type", "()" },
        };

        public RustWriter WriteAll()
        {
            StartNewFile("plato.rs");
            WriteLine("// Autogenerated file: DO NOT EDIT");
            WriteLine($"// Created on {DateTime.Now}");
            WriteLine("#![allow(non_snake_case, non_camel_case_types, dead_code, unused_parens, unused_imports, unused_mut)]");
            WriteLine("#![allow(clippy::all)]");
            WriteLine();

            WritePrelude();
            WriteConceptTraits();
            WriteArrayMethodsAndFreeFunctions();
            WriteConstantLibraryFunctions();

            // The native primitives come first so the extension traits appear
            // before the structs that use them.
            var natives = Compilation.ConcreteTypes
                .Where(c => NativePrimitives.ContainsKey(c.TypeDef.Name)).ToList();
            var classes = Compilation.ConcreteTypes
                .Where(c => !NativePrimitives.ContainsKey(c.TypeDef.Name)).ToList();

            foreach (var c in natives.Concat(classes))
            {
                var name = c.TypeDef.Name;
                if (!IgnoredTypes.Contains(name))
                    WriteTypeImplementation(c);
            }

            return this;
        }

        public void StartNewFile(string fileName)
        {
            sb = new StringBuilder();
            Files.Add(fileName, sb);
        }

        public RustTypeWriter NewDefaultTypeWriter()
            => new RustTypeWriter(this, null);

        /// <summary>
        /// Writes pre-rendered multi-line text, replacing the trailing newline with a
        /// WriteLine so that indentation resumes correctly afterwards.
        /// </summary>
        public RustWriter WriteTrimmed(string s)
        {
            s = s.TrimEnd('\r', '\n');
            if (s.Length == 0)
                return this;
            return Write(s).WriteLine();
        }

        /// <summary>
        /// Hand-written support code required by the generated code.
        /// </summary>
        public RustWriter WritePrelude()
        {
            return Write(@"
// ==== Intrinsics prelude (hand-written support code) ====

/// The Plato array concept: index and count.
pub trait IArray<T> {
    fn At(&self, n: i64) -> T;
    fn Count(&self) -> i64;
}

/// Vec-backed implementation of the Plato array concept.
#[derive(Clone, Debug, Default)]
pub struct Arr<T> {
    pub items: Vec<T>,
}

impl<T: Copy> Arr<T> {
    pub fn new(items: Vec<T>) -> Arr<T> { Arr { items } }
    pub fn At(&self, n: i64) -> T { self.items[n as usize] }
    pub fn Count(&self) -> i64 { self.items.len() as i64 }
    pub fn Map<U: Copy>(&self, f: impl Fn(T) -> U) -> Arr<U> {
        Arr::new(self.items.iter().map(|x| f(*x)).collect())
    }
    pub fn Reduce<TAcc>(&self, init: TAcc, f: impl Fn(TAcc, T) -> TAcc) -> TAcc {
        let mut acc = init;
        for x in &self.items {
            acc = f(acc, *x);
        }
        acc
    }
}

impl<T: Copy> IArray<T> for Arr<T> {
    fn At(&self, n: i64) -> T { Arr::At(self, n) }
    fn Count(&self) -> i64 { Arr::Count(self) }
}

pub mod Intrinsics {
    use super::*;

    pub fn MakeArray<T: Copy>(items: Vec<T>) -> Arr<T> {
        Arr::new(items)
    }

    pub fn Range(n: i64) -> Arr<i64> {
        Arr::new((0..n).collect())
    }
}
").WriteLine();
        }

        public RustWriter WriteConstantFunction(FunctionDef f)
        {
            var tmp = NewDefaultTypeWriter();
            tmp.IndentLevel = IndentLevel;
            var fi = tmp.ToFunctionInfo(f, null, FunctionInstanceKind.Constant);
            tmp.WriteConstantFunction(fi);
            return WriteTrimmed(tmp.ToString());
        }

        public RustWriter WriteConstantLibraryFunctions()
        {
            WriteLine("pub mod Constants");
            WriteStartBlock();
            WriteLine("use super::*;");
            WriteLine();
            foreach (var f in Compilation.Libraries.AllConstants())
                WriteConstantFunction(f);
            WriteEndBlock();
            return WriteLine();
        }

        /// <summary>
        /// Emits the generated IArray library functions. Functions that are
        /// generic over the element type become methods of Arr (in an extra
        /// impl block); functions over a concrete element type become
        /// module-level functions. The Arr struct itself and its core methods
        /// (At, Count, Map, Reduce) live in the hand-written prelude.
        /// </summary>
        public RustWriter WriteArrayMethodsAndFreeFunctions()
        {
            var methodWriter = NewDefaultTypeWriter();
            var freeFunctionWriter = NewDefaultTypeWriter();
            methodWriter.IndentLevel = IndentLevel + 1;
            freeFunctionWriter.IndentLevel = IndentLevel;
            foreach (var predefined in new[] { "At", "Count", "Map", "Reduce" })
                methodWriter.ClaimedNames.Add(predefined);

            foreach (var f in Compilation.Libraries.AllFunctions())
            {
                if (f.NumParameters == 0)
                    continue;

                var pt = f.Parameters[0].Type;
                if (!pt.Def.IsInterface())
                    continue;

                // We are going to skip functions that do not have a body
                if (f.Body == null)
                    continue;

                // Only functions on IArray itself (not IArrayLike, IArray2D, ...)
                if (pt.Def.Name != "IArray" && pt.Def.Name != "Array")
                    continue;

                var fi = new FunctionInstance(f, null, null, FunctionInstanceKind.InterfaceExtension);

                // Functions that are generic over the element type become methods of
                // Arr; functions over a concrete element type (e.g. an IArray of
                // Number) become module-level functions.
                var elemVar = fi.ParameterTypes[0].ArgsWithSelf.LastOrDefault()?.Name;
                var isGenericElement = elemVar != null && fi.TypeVariables.Contains(elemVar);
                if (isGenericElement)
                    methodWriter.WriteArrayMethod(fi);
                else
                    freeFunctionWriter.WriteFreeArrayFunction(fi);
            }

            var methods = methodWriter.ToString().TrimEnd('\r', '\n');
            if (methods.Length > 0)
            {
                WriteLine("// Generated IArray library functions");
                WriteLine("impl<T: Copy> Arr<T>");
                WriteStartBlock();
                WriteTrimmed(methods);
                WriteEndBlock();
                WriteLine();
            }

            var freeFunctions = freeFunctionWriter.ToString().TrimEnd('\r', '\n');
            if (freeFunctions.Length > 0)
            {
                WriteLine("// Array functions over concrete element types");
                WriteTrimmed(freeFunctions);
                WriteLine();
            }

            return this;
        }

        public RustWriter WriteConceptTrait(TypeDef type)
        {
            var tmp = new RustTypeWriter(this, type);
            tmp.IndentLevel = IndentLevel;
            tmp.WriteConceptTrait();
            return WriteTrimmed(tmp.ToString());
        }

        public RustWriter WriteConceptTraits()
        {
            foreach (var c in Compilation.AllTypeAndLibraryDefinitions)
                if (c.IsInterface())
                    WriteConceptTrait(c);
            return this;
        }

        public RustWriter WriteTypeImplementation(ConcreteType concreteType)
        {
            var tmp = new RustTypeWriter(this, concreteType.TypeDef);
            tmp.IndentLevel = IndentLevel;
            tmp.WriteConcreteType(concreteType);
            return WriteTrimmed(tmp.ToString());
        }
    }
}
