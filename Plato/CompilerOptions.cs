using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Plato
{
    public class CompilerOptions
    {
        public CompilerOptions(IEnumerable<string> fileReferences = null, string outputFileName = null, bool debug = true)
            => (FileReferences, OutputFileName, Debug) = 
                ((fileReferences ?? PlatoUtils.LoadedAssemblyLocations()).ToArray(), outputFileName ?? PlatoUtils.GenerateNewDllFileName(), debug);

        public string OutputFileName { get; }
        public bool Debug { get; }
        public IReadOnlyList<string> FileReferences { get; }

        public string AssemblyName
            => Path.GetFileNameWithoutExtension(OutputFileName);

        public readonly LanguageVersion Language 
            = LanguageVersion.CSharp9;

        public CSharpParseOptions ParseOptions
            => new CSharpParseOptions(Language, DocumentationMode.Parse, SourceCodeKind.Regular, null);

        public CSharpCompilationOptions CompilationOptions
            => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOverflowChecks(true)                
                .WithOptimizationLevel(Debug ? OptimizationLevel.Debug : OptimizationLevel.Release);
    
        public IEnumerable<MetadataReference> MetadataReferences
            => PlatoUtils.ReferencesFromFiles(FileReferences);

        public CompilerOptions WithNewOutputFilePath(string fileName = null)
            => new CompilerOptions(FileReferences, fileName, Debug);
    }
}
