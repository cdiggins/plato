using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PlatoGenerator
{
    public static class Util
    {
        // https://stackoverflow.com/questions/35741219/how-to-get-il-of-one-method-body-with-roslyn
        public static MethodBody Compile(this CSharpCompilation initial, IMethodSymbol method)
        {
            // 1. get source
            var methodRef = method.DeclaringSyntaxReferences.Single();
            var methodSource = methodRef.SyntaxTree.GetText().GetSubText(methodRef.Span).ToString();

            // 2. compile in-memory as script
            var compilation = CSharpCompilation.CreateScriptCompilation("Temp")
                .AddReferences(initial.References)
                .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(methodSource,
                    CSharpParseOptions.Default.WithKind(SourceCodeKind.Script)));

            using (var dll = new MemoryStream())
            using (var pdb = new MemoryStream())
            {
                compilation.Emit(dll, pdb);

                // 3. load compiled assembly
                var assembly = Assembly.Load(dll.ToArray(), pdb.ToArray());
                var methodBase = assembly.GetType("Script").GetMethod(method.Name, Type.EmptyTypes);

                // 4. get il or even execute
                return methodBase?.GetMethodBody();
            }
        }
    }
}
