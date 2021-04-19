using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plato
{
    public class Compilation
    {
        public IReadOnlyDictionary<string, ScriptFile> InputFileLookup { get; }
        public IEnumerable<ScriptFile> InputsFiles => InputFileLookup.Values;
        public EmitResult EmitResult { get; }
        public CompilerOptions Options { get; }
        public CSharpCompilation Compiler { get;  }

        public Compilation(IEnumerable<ScriptFile> inputFiles = null, CSharpCompilation compiler = default, CompilerOptions options = default, EmitResult result = default)
        {
            InputFileLookup = (inputFiles ?? Array.Empty<ScriptFile>()).ToDictionary(f => f.FilePath, f => f);
            Options = options ?? new CompilerOptions(RoslynUtils.LoadedAssemblyLocations());
            Compiler = compiler ?? CSharpCompilation.Create(Options.AssemblyName, null, Options.MetadataReferences, Options.CompilationOptions);
            EmitResult = result;
        }
    }
}
