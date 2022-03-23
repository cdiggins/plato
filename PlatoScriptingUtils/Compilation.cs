using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Plato
{
    public class Compilation
    {
        public IReadOnlyDictionary<string, ScriptFile> InputFileLookup { get; }
        public IEnumerable<ScriptFile> InputsFiles => InputFileLookup.Values;
        public EmitResult EmitResult { get; }
        public CompilerOptions Options { get; }
        public CSharpCompilation Compiler { get;  }

        public Compilation(IEnumerable<ScriptFile> inputFiles = null, CompilerOptions options = default, CSharpCompilation compiler = default, EmitResult result = default)
        {
            InputFileLookup = (inputFiles ?? Array.Empty<ScriptFile>()).ToDictionary(f => f.FilePath, f => f);
            Options = options ?? new CompilerOptions(PlatoUtils.LoadedAssemblyLocations());
            var syntaxTrees = InputFileLookup.Values.Select(f => f.SyntaxTree);
            Compiler = compiler ?? CSharpCompilation.Create(Options.AssemblyName, syntaxTrees, Options.MetadataReferences, Options.CompilationOptions);
            EmitResult = result;
        }

        public static Compilation Create(IEnumerable<ScriptFile> inputFiles = null,
            CompilerOptions options = default, CSharpCompilation compiler = default, EmitResult result = default)
            => new Compilation(inputFiles, options, compiler, result);

        public static Compilation Create(IEnumerable<string> inputFiles, CompilerOptions options = default, CSharpCompilation compiler = default, EmitResult result = default)
            => new Compilation(inputFiles.Select(f => ScriptFile.Create(f, options?.ParseOptions)), options, compiler, result);
    }
}
