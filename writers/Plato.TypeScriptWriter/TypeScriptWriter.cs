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
        }

        public Compiler.Compilation Compilation => Analyzer.Compilation;
        public PlatoAnalyzer Analyzer { get; }
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        public DirectoryPath OutputFolder { get; }

        /// <summary>Emit function bodies from the Typed IR (TirTypeScriptBodyWriter) when one is
        /// available, the legacy symbol-graph writer as the fallback. Mirrors CSharpWriter.UseTir.</summary>
        public bool UseTir = true;

        // UseTir measurement counters (no effect on output).
        public int TirBodiesEmitted;
        public int TirFallbackBodies;

        // Lazily built the first time a TIR is requested (UseTir only).
        private Compiler.Checking.TirEmitSource _tirSource;
        private Compiler.Checking.TirEmitSource TirSource
            => _tirSource ?? (_tirSource = new Compiler.Checking.TirEmitSource(Compilation));

        public Compiler.Checking.TirFunction TryGetGroundTir(FunctionDef original, TypeDef concreteType)
            => UseTir ? TirSource.TryGetGroundTir(original, concreteType) : null;

        public Compiler.Checking.TirFunction TryGetStaticTir(FunctionDef original)
            => UseTir ? TirSource.TryGetStaticTir(original) : null;

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

// The affine builders of primitives.plato. Plato guarantees a single live
// reference, so the generated rebind-after-mutate style (`xs = xs.Add(x)`)
// is honoured by mutating in place and returning `this`; Freeze hands the
// storage to an Arr without copying. Buffer intentionally shadows Node's
// global byte Buffer inside the generated module.
export class List<T> {
    private readonly xs: T[] = [];
    Count(): number { return this.xs.length; }
    At(i: number): T { return this.xs[i]; }
    Add(x: T): List<T> { this.xs.push(x); return this; }
    AddRange(values: IArray<T>): List<T> {
        for (let i = 0; i < values.Count(); i++) this.xs.push(values.At(i));
        return this;
    }
    Set(i: number, x: T): List<T> { this.xs[i] = x; return this; }
    Freeze(): IArray<T> { const xs = this.xs; return new Arr<T>(xs.length, i => xs[i]); }
}

export class Buffer<T> {
    private readonly xs: T[];
    constructor(n: number) { this.xs = new globalThis.Array<T>(n); }
    Count(): number { return this.xs.length; }
    At(i: number): T { return this.xs[i]; }
    Set(i: number, x: T): Buffer<T> { this.xs[i] = x; return this; }
    Freeze(): IArray<T> { const xs = this.xs; return new Arr<T>(xs.length, i => xs[i]); }
}

// Helpers referenced by generated bodies whose Plato originals are in
// IgnoredFunctions, so the prototype methods must be installed here by hand.
declare global {
    interface Number {
        Range(): IArray<number>;
        MapRange<T>(f: (i: number) => T): IArray<T>;
        MakeArray2D<T>(rows: number, f: (column: number, row: number) => T): IArray2D<T>;
        Equals(b: number): boolean;
        NotEquals(b: number): boolean;
    }
    interface Boolean {
        Equals(b: boolean): boolean;
        NotEquals(b: boolean): boolean;
    }
    interface String {
        Equals(b: string): boolean;
        NotEquals(b: string): boolean;
    }
}
Intrinsics.Install(Number.prototype, 'Equals', function(this: number, b: number): boolean { return this.valueOf() === b; });
Intrinsics.Install(Number.prototype, 'NotEquals', function(this: number, b: number): boolean { return this.valueOf() !== b; });
Intrinsics.Install(Boolean.prototype, 'Equals', function(this: boolean, b: boolean): boolean { return this.valueOf() === b; });
Intrinsics.Install(Boolean.prototype, 'NotEquals', function(this: boolean, b: boolean): boolean { return this.valueOf() !== b; });
Intrinsics.Install(String.prototype, 'Equals', function(this: string, b: string): boolean { return this.valueOf() === b; });
Intrinsics.Install(String.prototype, 'NotEquals', function(this: string, b: string): boolean { return this.valueOf() !== b; });
Intrinsics.Install(Number.prototype, 'Range', function(this: number): IArray<number> {
    return Intrinsics.Range(this.valueOf());
});
Intrinsics.Install(Number.prototype, 'MapRange', function<T>(this: number, f: (i: number) => T): IArray<T> {
    return Intrinsics.Range(this.valueOf()).Map(f);
});
// Row-major, matching Collections.FlattenIndex: Elements[row * ColumnCount + column].
Intrinsics.Install(Number.prototype, 'MakeArray2D', function<T>(this: number, rows: number, f: (column: number, row: number) => T): IArray2D<T> {
    const columns = this.valueOf();
    return new Arr2D<T>(columns, rows, new Arr<T>(columns * rows, i => f(i % columns, Math.floor(i / columns))));
});

// Array2D / Array3D declare Elements, ColumnCount, RowCount and LayerCount as
// FIELDS in primitives.types.plato, so generated bodies read them property-style
// (`grid.Elements`, not `grid.Elements()`). The representation below matches that:
// shape and elements are properties; At, which takes arguments, is a method.
export interface IArray2D<T> {
    At(column: number, row: number): T;
    readonly Elements: IArray<T>;
    readonly ColumnCount: number;
    readonly RowCount: number;
}

/** The Array2D representation: a flat row-major element view plus its shape. */
export class Arr2D<T> implements IArray2D<T> {
    constructor(
        readonly ColumnCount: number,
        readonly RowCount: number,
        readonly Elements: IArray<T>) {}
    At(column: number, row: number): T { return this.Elements.At(row * this.ColumnCount + column); }
}

export interface IArray3D<T> {
    At(column: number, row: number, layer: number): T;
    readonly Elements: IArray<T>;
    readonly ColumnCount: number;
    readonly RowCount: number;
    readonly LayerCount: number;
}

/** The Array3D representation: a flat row-major element view plus its shape. */
export class Arr3D<T> implements IArray3D<T> {
    constructor(
        readonly ColumnCount: number,
        readonly RowCount: number,
        readonly LayerCount: number,
        readonly Elements: IArray<T>) {}
    At(column: number, row: number, layer: number): T {
        return this.Elements.At((layer * this.RowCount + row) * this.ColumnCount + column);
    }
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
            foreach (var predefined in new[] { "At", "Count", "Map", "Reduce", "FlatMap" })
                classWriter.ClaimedNames.Add(predefined);

            // The receiver types whose functions become Arr members: the array type
            // under either spelling, plus every interface it implements.
            var arrayReceiverNames = new HashSet<string> { "IArray", "Array" };
            foreach (var ct in Compilation.ConcreteTypes.Where(c => arrayReceiverNames.Contains(c.TypeDef.Name)))
                foreach (var i in ct.AllInterfaces)
                    arrayReceiverNames.Add(i.Name);

            // Functions reaching the array type: spelled `interface IArray` in the legacy
            // stdlib and `primitive Array` in the forward one, plus the interfaces Array
            // implements (IIndexable, ICountable, ...) — a body calls `points.First()` on
            // a plain array, so those have to land on Arr too. Explicitly NOT IArrayLike /
            // IArray2D / IArray3D, which are other shapes with their own representations.
            //
            // The array-typed overload claims each name FIRST. An interface-typed
            // sibling usually reaches the array by re-materializing it
            // (`Any(self: IIndexable) => Count.MapRange(At).Any(f)`), so letting that one
            // win the member slot makes it call itself: same name, same receiver, forever.
            var arrayFunctions = Compilation.Libraries.AllFunctions()
                .Where(f => f.NumParameters > 0 && f.Body != null)
                .Where(f => arrayReceiverNames.Contains(f.Parameters[0].Type.Def.Name))
                .OrderByDescending(f => f.Parameters[0].Type.Def.Name == "Array"
                                     || f.Parameters[0].Type.Def.Name == "IArray")
                .ToList();

            foreach (var f in arrayFunctions)
            {

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
            WriteLine("FlatMap<TR>(f: (x: T) => IArray<TR>): IArray<TR>;");
            WriteTrimmed(interfaceWriter.ToString());
            WriteEndBlock();
            WriteLine();

            WriteLine("export class Arr<T>");
            WriteStartBlock();
            WriteLine("// Memoized view: each element is computed at most once (plato-436).");
            WriteLine("// Plato values are never undefined, so undefined marks an empty slot.");
            WriteLine("// Once every slot is filled the indexing function is released, so a");
            WriteLine("// fully-read layer no longer pins the arrays its closure captured.");
            WriteLine("private _cache?: T[];");
            WriteLine("private _missing: number;");
            WriteLine("constructor(readonly _count: number, private _func?: (i: number) => T) { this._missing = _count; }");
            WriteLine("At(n: number): T {");
            WriteLine("    const c = this._cache ?? (this._cache = new Array<T>(this._count));");
            WriteLine("    let v = c[n];");
            WriteLine("    if (v === undefined && this._func !== undefined) {");
            WriteLine("        c[n] = v = this._func(n);");
            WriteLine("        if (n >= 0 && n < this._count && --this._missing === 0) this._func = undefined;");
            WriteLine("    }");
            WriteLine("    return v as T;");
            WriteLine("}");
            WriteLine("Count(): number { return this._count; }");
            WriteLine("Map<TR>(f: (x: T) => TR): IArray<TR> { return new Arr<TR>(this._count, i => f(this.At(i))); }");
            WriteLine("Reduce<TAcc>(init: TAcc, f: (acc: TAcc, x: T) => TAcc): TAcc {");
            WriteLine("    let acc = init;");
            WriteLine("    for (let i = 0; i < this._count; i++) acc = f(acc, this.At(i));");
            WriteLine("    return acc;");
            WriteLine("}");
            // The fifth array intrinsic: the only length-varying producer, so unlike
            // Map it cannot be a lazy view over a known count. Concatenating eagerly
            // is what makes the result's own Count answerable.
            WriteLine("FlatMap<TR>(f: (x: T) => IArray<TR>): IArray<TR> {");
            WriteLine("    const out: TR[] = [];");
            WriteLine("    for (let i = 0; i < this._count; i++) {");
            WriteLine("        const row = f(this.At(i));");
            WriteLine("        for (let j = 0; j < row.Count(); j++) out.push(row.At(j));");
            WriteLine("    }");
            WriteLine("    return new Arr<TR>(out.length, i => out[i]);");
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
