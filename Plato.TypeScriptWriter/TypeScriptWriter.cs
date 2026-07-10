using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Utils;

namespace Ara3D.Geometry.TypeScriptWriter
{
    /// <summary>
    /// Generates TypeScript source code from a Plato compilation.
    /// This is the TypeScript analog of Plato.CSharpWriter.CSharpWriter.
    ///
    /// Output model:
    /// - The Plato primitives (Number, Integer, Boolean, String, Character) map
    ///   directly to the native TypeScript number / boolean / string. Their Plato
    ///   functions are installed on the native prototypes (Number.prototype, ...)
    ///   with matching "declare global" interface augmentations, giving fluent
    ///   syntax on plain values: (0.5).Clamp(0, 1), x.Sqrt().
    /// - Concrete types become classes whose declared fields are readonly
    ///   properties. Everything else is a method: single-parameter Plato functions
    ///   become zero-argument methods (v.Length()), never property getters,
    ///   mirroring the extension-method convention on the C# side.
    /// - A single self-contained module (plato.g.ts) is produced, because
    ///   TypeScript modules are closed and splitting output would create cyclic
    ///   imports.
    /// - TypeScript has no extension methods, so the IArray library functions
    ///   become methods of a generated Arr class (and the IArray interface).
    /// </summary>
    public class TypeScriptWriter : CodeBuilder<TypeScriptWriter>
    {
        /// <summary>
        /// Returned by ToTypeScriptTypeName to indicate that the type must be written
        /// using TypeScript arrow-function syntax.
        /// </summary>
        public const string FunctionTypeSentinel = "$function";

        public TypeScriptWriter(Compiler.Compilation compilation, DirectoryPath outputFolder)
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
        /// writer uses this to distinguish field access (a property: "v.X") from
        /// function calls (always parenthesized: "v.Length()").
        /// </summary>
        public HashSet<string> AllFieldNames { get; } = new HashSet<string>();

        /// <summary>
        /// Member names already installed per native prototype interface
        /// ("Number", "Boolean", "String"). Plato's Number and Integer both map to
        /// the native number, so name claims are shared: the first writer wins
        /// (Number is processed before Integer) and later collisions are skipped.
        /// </summary>
        public Dictionary<string, HashSet<string>> NativeClaimedNames { get; } = new Dictionary<string, HashSet<string>>();

        public HashSet<string> GetNativeClaimedNames(string nativeInterface)
        {
            if (!NativeClaimedNames.TryGetValue(nativeInterface, out var set))
                NativeClaimedNames[nativeInterface] = set = new HashSet<string>();
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
        /// Plato types represented directly by a native TypeScript type.
        /// Their functions are installed on the corresponding native prototype.
        /// </summary>
        public static Dictionary<string, string> NativePrimitives = new Dictionary<string, string>()
        {
            { "Number", "number" },
            { "Integer", "number" },
            { "Boolean", "boolean" },
            { "String", "string" },
            { "Character", "string" },
        };

        /// <summary>
        /// The global interface to augment for each native type.
        /// </summary>
        public static Dictionary<string, string> NativeInterfaces = new Dictionary<string, string>()
        {
            { "number", "Number" },
            { "boolean", "Boolean" },
            { "string", "String" },
        };

        public static Dictionary<string, string> NativeDefaults = new Dictionary<string, string>()
        {
            { "number", "0" },
            { "boolean", "false" },
            { "string", "''" },
        };

        /// <summary>
        /// Direct name replacements for types that are not generated as classes.
        /// </summary>
        public static Dictionary<string, string> TypeNameReplacements = new Dictionary<string, string>()
        {
            { "Dynamic", "unknown" },
            { "Type", "unknown" },
        };

        public TypeScriptWriter WriteAll()
        {
            // Reset the process-global lambda-capture counter per generation (see CSharpWriter).
            SymbolRewriter.NextId = 0;

            StartNewFile("plato.g.ts");
            WriteLine("// Autogenerated file: DO NOT EDIT");
            WriteLine($"// Created on {DateTime.Now}");
            WriteLine("/* eslint-disable */");
            WriteLine("// noinspection JSUnusedGlobalSymbols");
            WriteLine();

            WritePrelude();
            WriteConceptInterfaces();
            WriteArrayInterfaceAndClass();
            WriteConstantLibraryMethods();

            // The native primitives come first so that their prototype methods
            // claim names before anything else, and so classes can rely on them.
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

        public TypeScriptTypeWriter NewDefaultTypeWriter()
            => new TypeScriptTypeWriter(this, null);

        /// <summary>
        /// Writes pre-rendered multi-line text, replacing the trailing newline with a
        /// WriteLine so that indentation resumes correctly afterwards.
        /// </summary>
        public TypeScriptWriter WriteTrimmed(string s)
        {
            s = s.TrimEnd('\r', '\n');
            if (s.Length == 0)
                return this;
            return Write(s).WriteLine();
        }

        /// <summary>
        /// Hand-written support code required by the generated code.
        /// </summary>
        public TypeScriptWriter WritePrelude()
        {
            return Write(@"
// ==== Intrinsics prelude (hand-written support code) ====

export namespace Intrinsics {
    /** Installs a method on a native prototype (non-enumerable, safe for for-in). */
    export function Install(proto: object, name: string, fn: unknown): void {
        Object.defineProperty(proto, name, { value: fn, writable: true, configurable: true, enumerable: false });
    }

    export function MakeArray<T>(...xs: T[]): IArray<T> {
        return new Arr<T>(xs.length, i => xs[i]);
    }

    export function Range(n: number): IArray<number> {
        return new Arr<number>(n, i => i);
    }

    // Structural equality helper: falls back to reference equality for values
    // that do not expose an Equals method.
    export function Eq(a: unknown, b: unknown): boolean {
        if (a === b) return true;
        const x = a as { Equals?: (other: unknown) => boolean };
        if (x && typeof x.Equals === 'function') return x.Equals(b);
        return false;
    }

    export function ThrowOutOfRange<T>(): T {
        throw new globalThis.Error('Index out of range');
    }

    export function ThrowNotImplemented<T>(name: string): T {
        throw new globalThis.Error(`Not implemented: ${name}`);
    }
}

export interface IArray2D<T> {
    At(column: number, row: number): T;
    RowCount(): number;
    ColumnCount(): number;
}

export interface IArray3D<T> {
    At(column: number, row: number, layer: number): T;
    RowCount(): number;
    ColumnCount(): number;
    LayerCount(): number;
}

").WriteLine();
        }

        public TypeScriptWriter WriteConstantFunction(FunctionDef f)
        {
            var tmp = NewDefaultTypeWriter();
            tmp.IndentLevel = IndentLevel;
            var fi = tmp.ToFunctionInfo(f, null, FunctionInstanceKind.Constant);
            tmp.WriteConstantFunction(fi);
            return WriteTrimmed(tmp.ToString());
        }

        public TypeScriptWriter WriteConstantLibraryMethods()
        {
            WriteLine("export class Constants");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllConstants())
                WriteConstantFunction(f);
            WriteEndBlock();
            return WriteLine();
        }

        /// <summary>
        /// Emits the IArray interface and the Arr class.
        /// TypeScript has no extension methods, so library functions whose first
        /// parameter is an IArray become methods of the interface, implemented by
        /// the Arr class (a functional array: count plus indexing function).
        /// Functions over a concrete element type become module-level functions.
        /// </summary>
        public TypeScriptWriter WriteArrayInterfaceAndClass()
        {
            var interfaceWriter = NewDefaultTypeWriter();
            var classWriter = NewDefaultTypeWriter();
            var freeFunctionWriter = NewDefaultTypeWriter();
            interfaceWriter.IndentLevel = IndentLevel + 1;
            classWriter.IndentLevel = IndentLevel + 1;
            freeFunctionWriter.IndentLevel = IndentLevel;
            foreach (var predefined in new[] { "At", "Count", "Map", "Reduce" })
                classWriter.ClaimedNames.Add(predefined);

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
                // Arr / IArray; functions over a concrete element type (e.g. an IArray
                // of Number) become module-level functions.
                var elemVar = fi.ParameterTypes[0].ArgsWithSelf.LastOrDefault()?.Name;
                var isGenericElement = elemVar != null && fi.TypeVariables.Contains(elemVar);
                if (isGenericElement)
                    classWriter.WriteArrayMethod(fi, interfaceWriter);
                else
                    freeFunctionWriter.WriteFreeArrayFunction(fi);
            }

            WriteLine("export interface IArray<T>");
            WriteStartBlock();
            WriteLine("At(n: number): T;");
            WriteLine("Count(): number;");
            WriteLine("Map<TR>(f: (x: T) => TR): IArray<TR>;");
            WriteLine("Reduce<TAcc>(init: TAcc, f: (acc: TAcc, x: T) => TAcc): TAcc;");
            WriteTrimmed(interfaceWriter.ToString());
            WriteEndBlock();
            WriteLine();

            WriteLine("export class Arr<T>");
            WriteStartBlock();
            WriteLine("constructor(readonly _count: number, readonly _func: (i: number) => T) {}");
            WriteLine("At(n: number): T { return this._func(n); }");
            WriteLine("Count(): number { return this._count; }");
            WriteLine("Map<TR>(f: (x: T) => TR): IArray<TR> { return new Arr<TR>(this._count, i => f(this.At(i))); }");
            WriteLine("Reduce<TAcc>(init: TAcc, f: (acc: TAcc, x: T) => TAcc): TAcc {");
            WriteLine("    let acc = init;");
            WriteLine("    for (let i = 0; i < this._count; i++) acc = f(acc, this.At(i));");
            WriteLine("    return acc;");
            WriteLine("}");
            WriteTrimmed(classWriter.ToString());
            WriteEndBlock();
            WriteLine();

            WriteLine("// Array functions over concrete element types");
            WriteTrimmed(freeFunctionWriter.ToString());
            return WriteLine();
        }

        public TypeScriptWriter WriteConceptInterface(TypeDef type)
        {
            var tmp = new TypeScriptTypeWriter(this, type);
            tmp.IndentLevel = IndentLevel;
            tmp.WriteConceptInterface();
            return WriteTrimmed(tmp.ToString());
        }

        public TypeScriptWriter WriteConceptInterfaces()
        {
            foreach (var c in Compilation.AllTypeAndLibraryDefinitions)
                if (c.IsInterface())
                    WriteConceptInterface(c);
            return this;
        }

        public TypeScriptWriter WriteTypeImplementation(ConcreteType concreteType)
        {
            var tmp = new TypeScriptTypeWriter(this, concreteType.TypeDef);
            tmp.IndentLevel = IndentLevel;
            tmp.WriteConcreteType(concreteType);
            return WriteTrimmed(tmp.ToString());
        }
    }
}
