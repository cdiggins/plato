using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Plato
{
    public class CompilerOptions
    {
        public string OutputFileName { get; }
        public bool Debug { get; }
        public IReadOnlyList<string> FileReferences { get; }

        public string AssemblyName
            => Path.GetFileNameWithoutExtension(OutputFileName);

        public LanguageVersion Language { get;  } 
            = LanguageVersion.CSharp9;

        public CSharpParseOptions ParseOptions
            => new CSharpParseOptions(Language, DocumentationMode.Parse, SourceCodeKind.Regular, null);

        public CSharpCompilationOptions CompilationOptions
            => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOverflowChecks(true)                
                .WithOptimizationLevel(Debug ? OptimizationLevel.Debug : OptimizationLevel.Release);

        public static string GenerateNewDllFileName()
        {
            return Path.ChangeExtension(Path.GetTempFileName(), "dll");
        }

        public IEnumerable<MetadataReference> MetadataReferences
            => RoslynUtils.ReferencesFromFiles(FileReferences);

        public CompilerOptions(IEnumerable<string> fileReferences, string outputFileName = null, bool debug = true)
            => (FileReferences, OutputFileName, Debug) = (fileReferences.ToArray(), outputFileName ?? GenerateNewDllFileName(), debug);

        public CompilerOptions WithNewOutputFilePath(string fileName = null)
        {
            return new CompilerOptions(FileReferences, fileName, Debug);
        }
    }
}
