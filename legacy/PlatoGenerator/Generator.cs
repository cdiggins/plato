using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using PlatoIR;
using PlatoRoslynSyntaxAnalyzer;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PlatoGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        // https://stackoverflow.com/questions/816566/how-do-you-get-the-current-project-directory-from-c-sharp-code-when-creating-a-c
        public static string GetSourceFilePathName([CallerFilePath] string callerFilePath = null)
            => callerFilePath ?? "";

        public void Execute(GeneratorExecutionContext context)
        {
            Console.WriteLine("I am executing!");

            {
                var types = context.Compilation.SyntaxTrees.GetPlatoTypes();
                var thisRepo = Path.Combine(Path.GetDirectoryName(GetSourceFilePathName()), "..");

                var typeDecls = new RoslynToIR().BuildIr(context.Compilation);
                typeDecls = RoslynToIR.InlineMethods(typeDecls);

                var outputCsFile = Path.Combine(thisRepo, "Tests", "PlatoTestOutput", "PlatoTestCode.g.cs");

                // Optimize 
                foreach (var type in typeDecls)
                {
                    if (type.Kind == "class" && !type.IsStatic)
                    {
                        type.SetKind("readonly struct");
                    }
                }

                using (var sw = new StreamWriter(File.Create(outputCsFile)))
                {
                    var srlzr = new IRSerializer(sw);
                    // TODO: this is a hack until I add proper namespace support. 
                    sw.WriteLine("using System.Runtime.CompilerServices;");
                    sw.WriteLine("namespace PlatoTest {");
                    srlzr.Write(typeDecls, "", "");
                    sw.WriteLine("}");
                }
            }

            // begin creating the source we'll inject into the users compilation
            var sourceBuilder = new StringBuilder(@"
using System;
namespace HelloWorldGenerated
{
    public static class HelloWorld
    {
        public static void SayHello() 
        {
            Console.WriteLine(""Hello from generated code5!"");
        }
    }
}");

            // inject the created source into the users compilation
            context.AddSource("helloWorldGenerator", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            Debug.WriteLine("Initialization");
        }
    }
}
