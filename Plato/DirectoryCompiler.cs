using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Plato
{
    public class DirectoryCompiler
    {
        public DirectoryWatcher Watcher;
        public Compilation Compilation;
        public string Directory => Watcher.Watcher.Path;
        public Action OnRecompile;
            
        public DirectoryCompiler(string dir, ISynchronizeInvoke syncObject, bool subDirectories = false, bool initialCompile = true)
        {
            Debug.WriteLine($"Setting up directory for {dir}");
            Watcher = new DirectoryWatcher(dir, "*.cs", subDirectories, Recompile, syncObject);
            if (initialCompile)
            {
                Recompile();
            }
        }

        CancellationTokenSource TokenSource = new CancellationTokenSource();

        public void Recompile()
        {
            try
            {
                Debug.WriteLine("Recompiling");
                if (Compilation == null)
                {
                    Debug.WriteLine("This is an initial compilation");
                    Compilation = new Compilation();
                }

                Debug.WriteLine("Requesting cancel of existing work...");
                TokenSource.Cancel();
                TokenSource = new CancellationTokenSource();

                Debug.WriteLine("Compilation task Started");
                var inputFiles = Watcher.GetFiles().ToArray();
                foreach (var f in inputFiles)
                {
                    Debug.WriteLine($"  Input file {f}");
                }

                Debug.WriteLine("Updating input files");
                Compilation = Compilation.UpdateInputFiles(inputFiles, TokenSource.Token);

                Debug.WriteLine("Emitting project file");
                Compilation = Compilation.Emit(TokenSource.Token);

                Debug.WriteLine($"Emitted assembly success = {Compilation.EmitResult.Success} output = {Compilation.Options.OutputFileName}");

                Debug.WriteLine($"Diagnostics");
                foreach (var x in Compilation.EmitResult.Diagnostics)
                {
                    Debug.WriteLine($"Diagnostic: {x}");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error occured {e}");
            }
        }
    }
}
