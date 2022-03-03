using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace PlatoTestConsole2
{

    class Program
    {
        static void Main(string[] args)
        {
            var source = @"
public static class Program { 
    public static void Main(string[] args) { 
        GeneratedClass.DoThing();
    } 
}

";

            var generatorSource = @"
public class GeneratedClass { 
    public static void DoThing(){
        System.Console.WriteLine(""thing"");
    }
}
";
            var corLib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var console = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
            var runtime = MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.dll"));

            var programTree = CSharpSyntaxTree.ParseText(SourceText.From(source, Encoding.UTF8), new CSharpParseOptions(LanguageVersion.Latest), "program.cs");
            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString(), new[] { programTree }, new[] { corLib, console, runtime }, new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            var testGenerator = new SingleFileTestGenerator(generatorSource);

            GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { testGenerator });
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            var diagnostics = outputCompilation.GetDiagnostics();

            var generatorTree = driver.GetRunResult().GeneratedTrees[0];
            var et = EmbeddedText.FromSource(generatorTree.FilePath, generatorTree.GetText());
            var et2 = EmbeddedText.FromSource(programTree.FilePath, programTree.GetText());

            var assemblyStream = new MemoryStream();
            var symbolsStream = new MemoryStream();
            var result = outputCompilation.Emit(assemblyStream, symbolsStream, embeddedTexts: new[] { et, et2 });

            var assemblyBytes = assemblyStream.GetBuffer();
            var symbolsBytes = symbolsStream.GetBuffer();
            var asm = System.Reflection.Assembly.Load(assemblyBytes, symbolsBytes);

            asm.EntryPoint.Invoke(null, new object[] { new string[0] });
        }
    }

    internal class SingleFileTestGenerator : ISourceGenerator
    {
        private readonly string _content;
        private readonly string _hintName;

        public SingleFileTestGenerator(string content, string hintName = "generatedFile")
        {
            _content = content;
            _hintName = hintName;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource(this._hintName, SourceText.From(_content, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}