using System;
using System.Collections.Generic;
using System.Text;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter
{
    public class CSharpWriter : CodeBuilder<CSharpWriter>
    {
        public CSharpWriter(Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            Analyzer = new PlatoAnalyzer(compilation);
            OutputFolder = outputFolder;
        }

        public string FloatType;
        public string Namespace;

        // When true, emits the "extension style" output (--csharp-style=extensions, roadmap P2.2):
        // the library-function fanout is written to C# 14 extension blocks (one static class per
        // Plato library) instead of instance members on the partial structs. Requires
        // <LangVersion>14</LangVersion> in the consuming project. Default (false) output is
        // byte-identical to the original writer.
        public bool ExtensionStyle;

        // When true, applies the component-op unrolling optimization (--optimize, roadmap P3.1):
        // recognized MapComponents/ZipComponents/Reduce/All*/Any* call sites on statically-known
        // IArrayLike types are rewritten to direct field expressions at emission time (see
        // ComponentUnroller). Default (false) output is byte-identical to the original writer.
        public bool Optimize;

        // Lazily-built table for ComponentUnroller: concrete IArrayLike type name -> field names.
        private Dictionary<string, IReadOnlyList<string>> _componentFields;
        public IReadOnlyList<string> GetComponentFields(string typeName)
        {
            if (_componentFields == null)
                _componentFields = ComponentUnroller.BuildComponentFieldTable(Compilation);
            return typeName != null && _componentFields.TryGetValue(typeName, out var fields) ? fields : null;
        }

        // Collected by CSharpConcreteTypeWriter while writing each type in extension style;
        // written out by ExtensionStyleWriter.WriteLibraryFiles at the end of WriteAll.
        public List<MovedExtensionMember> MovedMembers { get; } = new List<MovedExtensionMember>();

        // All Plato type/library/interface names; used by the extension-style body writer to
        // distinguish bare type references from bare static-member references.
        private HashSet<string> _allTypeNames;
        public HashSet<string> AllTypeNames => _allTypeNames ?? (_allTypeNames = new HashSet<string>(
            System.Linq.Enumerable.Select(Compilation.AllTypeAndLibraryDefinitions, t => t?.Name ?? "")));


#if CHANGE_PRECISION
        public string OtherPrecisionFloatType;
        public string OtherPrecisionNamespace;
#endif

        public Compiler.Compilation Compilation => Analyzer.Compilation;
        public PlatoAnalyzer Analyzer { get; }
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        public DirectoryPath OutputFolder { get; }

        public static HashSet<string> IgnoredTypes = new HashSet<string>()
        {
            "Dynamic",
            "Array",
            "Array2D",
            "Array3D",
            "Function0",
            "Function1",
            "Function2",
            "Function3",
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
            
            // Implemented elswehere
            "Range",
            "MakeArray2D",
            "MapRange",
        };

        public static Dictionary<string, string> PrimitiveTypes = new Dictionary<string, string>()
        {
            { "Number", "float" },
            { "Boolean", "bool" },
            { "Integer", "int" },
            { "Character", "char" },
            { "String", "string" },
            { "Dynamic", "object" },
            { "Type", "System.Type" },
            { "Function0", "System.Func" },
            { "Function1", "System.Func" },
            { "Function2", "System.Func" },
            { "Function3", "System.Func" },
            { "Function4", "System.Func" },
            { "Function5", "System.Func" },
            { "Function6", "System.Func" },
            { "Function7", "System.Func" },
            { "Function8", "System.Func" },
            { "Function9", "System.Func" },
            { "Angle", "float" },
            { "Matrix3x2", "System.Numerics.Matrix3x2" },
            { "Matrix4x4", "System.Numerics.Matrix4x4" },
            { "Quaternion", "System.Numerics.Quaternion" },
            { "Plane", "System.Numerics.Plane" },
            { "Vector2", "System.Numerics.Vector2" },
            { "Vector3", "System.Numerics.Vector3" },
            { "Vector4", "System.Numerics.Vector4" },
            { "Vector8", "System.Runtime.Intrinsics.Vector256<float>" }
        };

        public CSharpWriter WriteFile(FilePath fileName, Func<CSharpWriter> f)
        {
            StartNewFile(fileName);
            WriteLine($"// Autogenerated file: DO NOT EDIT");
            WriteLine($"// Created on {DateTime.Now}");
            WriteLine();
            WriteLine("using System.Runtime.CompilerServices;");
            WriteLine("using System.Runtime.Serialization;");
            WriteLine("using System.Runtime.InteropServices;");
            WriteLine("using static System.Runtime.CompilerServices.MethodImplOptions;");
            WriteLine("using Ara3D.Collections;");
            WriteLine("");
            WriteLine($"namespace {Namespace}");
            WriteStartBlock();
            f();
            WriteEndBlock();
            return this;
        }

        public CSharpWriter WriteAll(string floatType)
        {
            FloatType = floatType;
            Namespace = floatType == "float"
                ? "Ara3D.Geometry"
                : floatType == "double"
                    ? "Ara3D.Geometry.DoublePrecision"
                    : throw new NotImplementedException("Only 'float' and 'double' are supported");
#if CHANGE_PRECISION
            OtherPrecisionFloatType = floatType == "float" ? "double" : "float";
            OtherPrecisionNamespace = floatType == "float" ? "Plato.DoublePrecision" : "Plato";
#endif 

            WriteFile("Interfaces.g.cs", WriteConceptInterfaces);
            WriteFile("Constants.g.cs", WriteConstantLibraryMethods);
            WriteFile("Extensions.g.cs", WriteInterfaceLibraryMethods);
            
            //WriteFile("Constructors.g.cs", WriteConstructors);

            foreach (var c in Compilation.ConcreteTypes)
            {
                var name = c.TypeDef.Name;
                if (!IgnoredTypes.Contains(name))
                    WriteFile($"_{name}.g.cs", () => WriteTypeImplementation(c));
            }

            if (ExtensionStyle)
                ExtensionStyleWriter.WriteLibraryFiles(this);

            return this;
        }

        public void StartNewFile(string fileName)
        {
            sb = new StringBuilder();
            Files.Add(fileName, sb);
        }

        public CSharpTypeWriter NewDefaultTypeWriter() 
            => new CSharpTypeWriter(this, null);

        public CSharpWriter WriteConstantFunction(FunctionDef f)
        {
            var tmp = NewDefaultTypeWriter();
            var fi = tmp.ToFunctionInfo(f, null, FunctionInstanceKind.Constant);
            tmp.WriteStaticFunction(fi);
            return Write(tmp.ToString());
        }

        public CSharpWriter WriteConstantLibraryMethods()
        {
            WriteLine($"public static class Constants");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllConstants())
                WriteConstantFunction(f);
            WriteEndBlock();
            return this;
        }

        public CSharpWriter WriteConstructors()
        {
            WriteLine($"public static class Constructors");
            WriteStartBlock();
            foreach (var ct in Compilation.ConcreteTypes)
            {
                // TODO: 
                // Write all constructors as a static extension method
            }
            WriteEndBlock();
            return this;
        }

        public CSharpWriter WriteInterfaceLibraryMethods()
        {
            WriteLine($"public static partial class Extensions");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllFunctions())
            {
                if (f.NumParameters > 0)
                {
                    var pt = f.Parameters[0].Type;
                    if (!pt.Def.IsInterface())
                        continue;

                    // We are going to skip functions that do not have a body
                    if (f.Body == null)
                        continue;

                    if (!pt.Def.Name.StartsWith("IArray") || pt.Def.Name.StartsWith("IArrayLike"))
                        continue;

                    // We need to fix this, we should be creating functions instances.
                    var interfaceWriter = NewDefaultTypeWriter();
                    var fi = new FunctionInstance(f, null, null, FunctionInstanceKind.InterfaceExtension);
                    var cfi = new CSharpFunctionInfo(fi, null, interfaceWriter);
                    interfaceWriter.WriteExtensionFunction(cfi);
                    Write(interfaceWriter.ToString());
                }
            }
            WriteEndBlock();
            return this;
        }
        
        public CSharpWriter WriteConceptInterface(TypeDef type)
        {
            var tmp = new CSharpTypeWriter(this, type);
            tmp.WriteConceptInterface();
            return Write(tmp.ToString());
        }

        public CSharpWriter WriteConceptInterfaces()
        {
            foreach (var c in Compilation.AllTypeAndLibraryDefinitions)
                if (c.IsInterface())
                    WriteConceptInterface(c);
            return this;
        }

        public CSharpWriter WriteTypeImplementation(ConcreteType concreteType)
        {
            var tmp = new CSharpTypeWriter(this, concreteType.TypeDef);
            tmp.WriteConcreteType(concreteType);
            return Write(tmp.ToString());
        }
    }
}