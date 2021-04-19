using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Plato
{
    public static class RoslynUtils
    {
        public static IEnumerable<MetadataReference> ReferencesFromFiles(IEnumerable<string> files)
        {
            return files.Select(x => MetadataReference.CreateFromFile(x));
        }

        public static IEnumerable<string> LoadedAssemblyLocations(AppDomain domain = null)
        {
            return (domain ?? AppDomain.CurrentDomain).GetAssemblies().Select(x => x.Location);
        }

        public static string ToPackageReference(this AssemblyIdentity asm)
        {
            return $"<PackageReference Include=\"{asm.Name}\" Version=\"{asm.Version}\" />";
        }

        public static void Log(string s)
        {
            Console.WriteLine($"[{DateTime.Now}] {s}");
        }

        public static ScriptFile CreateScriptFile(ScriptFileId id, CSharpParseOptions options, CancellationToken token = default)
        {
            var f = id.Path;
            var newSource = SourceText.From(File.ReadAllText(f), System.Text.Encoding.UTF8);
            var newTree = CSharpSyntaxTree.ParseText(newSource, options, f, token);
            return new ScriptFile(id, newSource, newTree);
        }

        public static ScriptFile CreateScriptFile(string file, CSharpParseOptions options, CancellationToken token = default)
        {
            return CreateScriptFile(ScriptFileId.Create(file), options, token);
        }

        public static Compilation UpdateInputFiles(this Compilation self, IEnumerable<string> inputFiles = null, CancellationToken token = default)
        {
            return self.UpdateInputFiles((inputFiles ?? Array.Empty<string>()).Select(f => CreateScriptFile(f, self.Options.ParseOptions, token)), token);
        }

        public static Compilation UpdateInputFiles(this Compilation self, IEnumerable<ScriptFile> inputs, CancellationToken token = default)
        {
            return new Compilation(inputs, self.Compiler.RemoveAllSyntaxTrees().AddSyntaxTrees(inputs.Select(x => x.SyntaxTree)), self.Options);
        }

        public static Compilation UpdateOutputFile(this Compilation self, string outputFilePath = null)
        {
            return new Compilation(self.InputFileLookup.Values, self.Compiler, self.Options.WithNewOutputFilePath(outputFilePath), null);
        }

        public static CompilerOptions AddReferences(this CompilerOptions self, IEnumerable<string> refs)
        {
            return self.UpdateReferences(self.FileReferences.Concat(refs));
        }

        public static CompilerOptions UpdateReferences(this CompilerOptions self, IEnumerable<string> refs)
        {
            return new CompilerOptions(refs, self.OutputFileName, self.Debug);
        }

        public static Compilation UpdateReferences(this Compilation self, IEnumerable<string> refs)
        {
            return self.UpdateOptions(self.Options.UpdateReferences(refs));
        }

        public static Compilation AddReferences(this Compilation self, IEnumerable<string> refs)
        {
            return self.UpdateOptions(self.Options.AddReferences(refs));
        }

        public static Compilation UpdateOptions(this Compilation self, CompilerOptions options)
        {
            return new Compilation(self.InputFileLookup.Values, null, options, null);
        }

        public static Compilation Emit(this Compilation self, CancellationToken token = default)
        {
            // Create the output directory, and delete the old DLL and PDB             
            // We do this early to fast exit on error
            var outputPath = self.Options.OutputFileName;
            var pdbPath = Path.ChangeExtension(outputPath, "pdb");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.Delete(outputPath);
            File.Delete(pdbPath);

            using (var peStream = File.OpenWrite(outputPath))
            using (var pdbStream = File.OpenWrite(pdbPath))
            {
                var emitOptions = new EmitOptions(false, DebugInformationFormat.Pdb, pdbPath);
                var embeddedTexts = self.InputFileLookup.Values.Select(f => f.EmbeddedText);
                var result = self.Compiler.Emit(peStream, pdbStream, null, null, null, emitOptions, null, null, embeddedTexts, token);
                return new Compilation(self.InputFileLookup.Values, self.Compiler, self.Options, result);
            }
        }
    }
}
