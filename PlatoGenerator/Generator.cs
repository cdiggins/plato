using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace PlatoGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {        
        public void Execute(GeneratorExecutionContext context)
        {
            Console.WriteLine("I am executing!");

            var expander = new JavaScriptGenerator();
            expander.ToJavaScript(context);

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
