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
using System.Threading.Tasks;

namespace Plato
{
    public static class RoslynUtils
    {
        public struct ScriptFileIdentity
        {
            public ScriptFileIdentity(FileInfo file)
                => (Length, LastWriteTime, Path) = (file.Length, file.LastWriteTime, file.FullName);

            public static ScriptFileIdentity Create(FileInfo file)
                => new ScriptFileIdentity(file);

            public static ScriptFileIdentity Create(string file)
                => new ScriptFileIdentity(new FileInfo(file));

            public long Length { get; }
            public DateTime LastWriteTime { get; }
            public string Path { get; }

            public override bool Equals(object obj)
                => obj is ScriptFileIdentity id && Equals(id);
            
            public bool Equals(ScriptFileIdentity id)
                => Length == id.Length && LastWriteTime == id.LastWriteTime && Path == id.Path;

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + Length.GetHashCode();
                    hash = hash * 31 + LastWriteTime.GetHashCode();
                    hash = hash * 31 + Path.GetHashCode();
                    return hash;
                }
            }
        }

        public class CompilerOptions
        {
            public static IEnumerable<MetadataReference> GetReferencesFromAppDomain(AppDomain domain)
                => domain.GetAssemblies().Select(x => MetadataReference.CreateFromFile(x.Location));

            public MetadataReference[] References
                = GetReferencesFromAppDomain(AppDomain.CurrentDomain).ToArray();

            public string _outputDllPath = Path.ChangeExtension(Path.GetTempFileName(), "dll");
            public string OutputDllPath
            {
                get => _outputDllPath;
                set => _outputDllPath = value;
            }

            public string AssemblyName
                => Path.GetFileNameWithoutExtension(OutputDllPath);

            public bool Debug { get; set; } 
                = true;

            public LanguageVersion Language { get; set; } 
                = LanguageVersion.CSharp9;

            public CSharpParseOptions ParseOptions
                => new CSharpParseOptions(Language, DocumentationMode.Parse, SourceCodeKind.Regular, null);

            public CSharpCompilationOptions CompilationOptions
                => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOverflowChecks(true)                
                    .WithOptimizationLevel(Debug ? OptimizationLevel.Debug : OptimizationLevel.Release);
        }

        public class ScriptFile
        {
            public ScriptFileIdentity Id;
            public EmbeddedText EmbeddedText;
            public SourceText SourceText;
            public SyntaxTree SyntaxTree;
            public string FilePath => Id.Path;

            public ScriptFile(ScriptFileIdentity id, SourceText source, SyntaxTree tree)
            {
                Id = id;
                SourceText = source;
                EmbeddedText = EmbeddedText.FromSource(FilePath, SourceText);
                SyntaxTree = tree;
            }
        }

        public class Compilation
        {
            public Dictionary<string, ScriptFile> InputFiles = new Dictionary<string, ScriptFile>();
            public EmitResult EmitResult = default;
            public CompilerOptions Options = default;
            public CSharpCompilation Compiler = default;

            public Compilation Clone()
                => new Compilation { Compiler = Compiler, Options = Options };
        }

        public static Compilation CreateInitialCompilation(CompilerOptions options = null)
        {
            options = options ?? new CompilerOptions();
            var r = new Compilation();
            r.Options = options;
            r.Compiler = r.Compiler ?? CSharpCompilation.Create(options.AssemblyName, null, options.References, options.CompilationOptions);
            return r;
        }        

        public static ProjectInfo CreateProject(this Compilation result, string projectFilePath)
        {
            var vs = VersionStamp.Create();
            var pid = ProjectId.CreateNewId();
            var docs = result.InputFiles.Keys.Select(k => DocumentInfo.Create(DocumentId.CreateNewId(pid), Path.GetFileName(k)).WithFilePath(k));
            return ProjectInfo.Create(pid, vs, "MyProjectName", "MyAssemblyName", LanguageNames.CSharp, projectFilePath, result.Options.OutputDllPath, 
                result.Options.CompilationOptions, result.Options.ParseOptions, docs, null, result.Compiler.References, null, null, false, null);
        }


        public static async Task<Compilation> UpdateInputFiles(this Compilation self, IEnumerable<string> inputFiles, CancellationToken token = default)
        {
            // Create a new return result. 
            var result = self.Clone();

            // Create a dictionary of files with identities
            var newInputFileLookup = inputFiles.ToDictionary(f => f, ScriptFileIdentity.Create);            

            // Remove files that are no longer part of the input set from the compiler 
            foreach (var kv in self.InputFiles)
            {
                var path = kv.Key;
                var oldFile = kv.Value;
                var oldTree = oldFile.SyntaxTree;

                if (!newInputFileLookup.ContainsKey(path))
                {
                    result.Compiler = result.Compiler.RemoveSyntaxTrees(oldFile.SyntaxTree);
                }
                else
                {
                    var newId = newInputFileLookup[path];
                       
                    // Only need to update compiler if there are changes 
                    if (!newId.Equals(oldFile.Id))
                    {
                        var newSource = SourceText.From(File.ReadAllText(path), System.Text.Encoding.UTF8);
                        var newTree = oldTree.WithChangedText(newSource);
                        result.Compiler = result.Compiler.ReplaceSyntaxTree(oldTree, newTree);
                        result.InputFiles.Add(path, new ScriptFile(newId, newSource, newTree));
                    }
                    else
                    {
                        // We can reuse the old file (the compiler should stay valid);
                        result.InputFiles.Add(path, oldFile);
                        Debug.Assert(result.Compiler.ContainsSyntaxTree(oldTree));
                    }
                }
            }
           
            // Look for files that are in the new input set that we haven't processed yet
            foreach (var path in newInputFileLookup.Keys)
            {
                if (!self.InputFiles.ContainsKey(path))
                {
                    // This is a new file that we will have to parse and that we will have to add to the compiler 
                    Debug.Assert(!result.InputFiles.ContainsKey(path));
                    var newSource = SourceText.From(File.ReadAllText(path), System.Text.Encoding.UTF8);
                    var newTree = CSharpSyntaxTree.ParseText(newSource, result.Options.ParseOptions, path, token);
                    result.Compiler = result.Compiler.AddSyntaxTrees(newTree);
                }
            }
            
            // Check that everything lines up 
            foreach (var kv in newInputFileLookup)
            {
                // We should already have processed this file
                Debug.Assert(result.InputFiles.ContainsKey(kv.Key));
                Debug.Assert(kv.Value.Equals(result.InputFiles[kv.Key].Id));
                Debug.Assert(result.Compiler.ContainsSyntaxTree(result.InputFiles[kv.Key].SyntaxTree));
            }

            return result;
        }

        public static async Task<Compilation> Emit(this Compilation self, CancellationToken token = default)
        {
            var result = self.Clone();
            //result.InputFiles = self.InputFiles.ToDictionary(kv => 
            
            result.EmitResult = null;

            // Create the output directory, and delete the old DLL and PDB             
            // We do this early to fast exit on error
            var outputPath = result.Options.OutputDllPath;
            var pdbPath = Path.ChangeExtension(outputPath, "pdb");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.Delete(outputPath);
            File.Delete(pdbPath);

            using (var peStream = File.OpenWrite(outputPath))
            using (var pdbStream = File.OpenWrite(pdbPath))
            {
                var emitOptions = new EmitOptions(false, DebugInformationFormat.Pdb, pdbPath);
                var embeddedTexts = result.InputFiles.Values.Select(f => f.EmbeddedText);
                result.EmitResult = await Task.Run(() => result.Compiler.Emit(peStream, pdbStream, null, null, null, emitOptions, null, null, embeddedTexts, token));
            }
            return result;
        }

        public class DirectoryWatcher
        {
            public FileSystemWatcher Watcher;
            public IEnumerable<string> GetFiles()
                => Directory.GetFiles(Watcher.Path, Watcher.Filter, Watcher.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            private Action OnChange { get; }

            public DirectoryWatcher(string dir, string filter, Action onChange)
            {
                Watcher = new FileSystemWatcher(dir)
                {
                    NotifyFilter = NotifyFilters.Attributes 
                        | NotifyFilters.CreationTime 
                        | NotifyFilters.DirectoryName 
                        | NotifyFilters.FileName 
                        | NotifyFilters.LastWrite 
                        | NotifyFilters.Size
                };

                Watcher.Filter = filter;
                Watcher.IncludeSubdirectories = true;
                Watcher.Changed += Watcher_Changed;
                Watcher.Created += Watcher_Created;
                Watcher.Deleted += Watcher_Deleted;
                Watcher.Renamed += Watcher_Renamed;
                Watcher.Error += Watcher_Error;
                Watcher.EnableRaisingEvents = true;
                OnChange = onChange;
            }

            private void Watcher_Error(object sender, ErrorEventArgs e)
                => Debug.WriteLine($"Error occurred {e}");

            private void Watcher_Renamed(object sender, RenamedEventArgs e)
                => OnChange();

            private void Watcher_Deleted(object sender, FileSystemEventArgs e)
                => OnChange();

            private void Watcher_Created(object sender, FileSystemEventArgs e)
                => OnChange();

            private void Watcher_Changed(object sender, FileSystemEventArgs e)
                => OnChange();
        }

        public class DirectoryCompiler
        {
            public DirectoryWatcher Watcher;
            public Compilation Compilation;
            public string Directory => Watcher.Watcher.Path;
            public string ProjectFilePath => Path.Combine(Directory, "temp.csproj");

            public DirectoryCompiler(string dir, bool initialCompile = true)
            {
                Console.WriteLine($"Watching {dir}");
                Watcher = new DirectoryWatcher(dir, "*.cs", Recompile);
                if (initialCompile)
                    Recompile();
            }

            CancellationTokenSource TokenSource = new CancellationTokenSource();

            public void Recompile()
            {
                Console.WriteLine("Recompiling");

                if (Compilation == null)
                {
                    Console.WriteLine("Initial compilation");
                    Compilation = CreateInitialCompilation();
                }

                Console.WriteLine("Requesting cancel of existing work...");
                TokenSource.Cancel();
                TokenSource = new CancellationTokenSource();
                Thread.Sleep(10);
                Console.WriteLine("... 10 msec after cancel requested");

                Task.Run(() =>
                {
                    Console.WriteLine("Compilation task Started");
                    var inputFiles = Watcher.GetFiles().ToArray();
                    foreach (var f in inputFiles)
                        Console.WriteLine($" input {f}");

                    Console.WriteLine("Updating input files");
                    Compilation = Compilation.UpdateInputFiles(inputFiles, TokenSource.Token).Result;
                    Console.WriteLine("Updated input files");

                    Console.WriteLine("Creating project file");
                    Compilation.CreateProject(ProjectFilePath);
                    Console.WriteLine("Created project file");

                    Console.WriteLine("Emitting assembly");
                    Compilation = Compilation.Emit(TokenSource.Token).Result;
                    Console.WriteLine($"Emitted assembly success = {Compilation.EmitResult.Success} output = {Compilation.Options.OutputDllPath}");
                });
            }
        }
    }
}
