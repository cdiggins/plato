using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace Plato
{
    public static class PlatoUtils
    {
        public static IEnumerable<MetadataReference> ReferencesFromFiles(IEnumerable<string> files)
            => files.Select(x => MetadataReference.CreateFromFile(x));

        public static IEnumerable<MetadataReference> ReferencesFromLoadedAssemblies()
            => ReferencesFromFiles(LoadedAssemblyLocations());

        public static IEnumerable<string> LoadedAssemblyLocations(AppDomain domain = null)
            => (domain ?? AppDomain.CurrentDomain).GetAssemblies().Where(x => !x.IsDynamic).Select(x => x.Location);

        public static string ToPackageReference(this AssemblyIdentity asm)
            => $"<PackageReference Include=\"{asm.Name}\" Version=\"{asm.Version}\" />";

        public static void Log(string s)
            => Console.WriteLine($"[{DateTime.Now}] {s}");        

        public static Compilation UpdateInputFiles(this Compilation self, IEnumerable<string> inputFiles = null, CancellationToken token = default)
            => self.UpdateInputFiles((inputFiles ?? Array.Empty<string>()).Select(f => ScriptFile.Create(f, self.Options.ParseOptions, token)));

        public static Compilation UpdateInputFiles(this Compilation self, IEnumerable<ScriptFile> inputs)
            => self.UpdateInputFiles(inputs.ToArray());

        public static Compilation UpdateInputFiles(this Compilation self, ScriptFile[] inputs)
            => new Compilation(inputs, self.Options, self.Compiler.RemoveAllSyntaxTrees().AddSyntaxTrees(inputs.Select(x => x.SyntaxTree)));

        public static Compilation UpdateOutputFile(this Compilation self, string outputFilePath = null)
            => new Compilation(self.InputFileLookup.Values, self.Options.WithNewOutputFilePath(outputFilePath), self.Compiler);

        public static CompilerOptions AddReferences(this CompilerOptions self, IEnumerable<string> refs)
            => self.UpdateReferences(self.FileReferences.Concat(refs));

        public static CompilerOptions UpdateReferences(this CompilerOptions self, IEnumerable<string> refs)
            => new CompilerOptions(refs, self.OutputFileName, self.Debug);

        public static Compilation UpdateReferences(this Compilation self, IEnumerable<string> refs)
            => self.UpdateOptions(self.Options.UpdateReferences(refs));

        public static Compilation AddReferences(this Compilation self, IEnumerable<string> refs) 
            => self.UpdateOptions(self.Options.AddReferences(refs));

        public static Compilation UpdateOptions(this Compilation self, CompilerOptions options) 
            => new Compilation(self.InputFileLookup.Values, options);

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
                return new Compilation(self.InputFileLookup.Values, self.Options, self.Compiler, result);
            }
        }

        public static string PlatoDir
            => Path.Combine(Path.GetTempPath(), "Plato");

        public static string GetOrCreatePlatoDirectory()
            => GetOrCreateDir(PlatoDir);

        public static string GetOrCreateDir(string path)
            => Directory.Exists(path) ? path : Directory.CreateDirectory(path).FullName;

        public static string ChangeDirectory(string filePath, string dirPath)
            => Path.Combine(dirPath, Path.GetFileName(filePath));

        public static string Move(string srcPath, string destPath)
        {
            File.Move(srcPath, destPath);
            return destPath;
        }

        public static string ChangeDirectoryToPlato(string filePath)
            => ChangeDirectory(filePath, GetOrCreatePlatoDirectory());

        public static string GenerateNewFileName(string ext)
            => ChangeDirectoryToPlato(Path.ChangeExtension(Path.GetTempFileName(), ext));

        public static string GenerateNewDllFileName()
            => GenerateNewFileName("dll");

        public static string GenerateNewSourceFileName()
            => GenerateNewFileName("cs");

        public static string WriteToTempFile(string source)
        {
            var path = GenerateNewSourceFileName();
            File.WriteAllText(path, source);
            return path;
        }

        public static Compilation CompileSource(string source, CompilerOptions options = null)
        {
            var inputFile = WriteToTempFile(source);
            var c = Compilation.Create(new[] {inputFile}, options);
            return c.Emit();
        }
    }
}
