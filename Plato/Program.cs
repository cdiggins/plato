using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Plato
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var dir = @"C:\dev\repos\vim2020\src\Vim.Desktop.Labs\Vim.Desktop.Labs.Scripts";
            var watcher = new RoslynUtils.DirectoryCompiler(dir);
            while (true)
            {
                Console.WriteLine($"Waiting and observing {watcher.Directory} at {DateTime.Now}");
                Thread.Sleep(500);
            }
        }
    }
}
    /*
{ 
    public static void Main(string[] args)
    {

        var code = @"

        using System;

        class Program {
            public void My() {
                var i = 5;
                do {
                    Console.WriteLine(""hello world"");
                    i++;
                }
                until (i > 10);
            }
        }
        ";

        //Parsing input code into a SynaxTree object.
        var syntaxTree = RoslynUtils.Parse(code);

        var syntaxRoot = syntaxTree.GetRoot();

        //Here we will keep all nodes to replace
        var replaceDictionary = new Dictionary<DoStatementSyntax, DoStatementSyntax>();

        //Looking for do-until statements in all descendant nodes
        foreach (var doStatement in syntaxRoot.DescendantNodes().OfType<DoStatementSyntax>())
        {
            //Until token is treated as an identifier by C# compiler. It doesn't know that in our case it is a keyword.
            var untilNode = doStatement.Condition.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault((_node =>
            {
                return _node.Identifier.ValueText == "until";
            }));

            //Condition is treated as an argument list
            var conditionNode = doStatement.Condition.ChildNodes().OfType<ArgumentListSyntax>().FirstOrDefault();

            if (untilNode != null && conditionNode != null)
            {

                //Let's replace identifier w/ correct while keyword and condition

                var whileNode = SyntaxFactory.ParseToken("while");

                var condition = SyntaxFactory.ParseExpression($"(!{conditionNode.GetText()})");

                var newDoStatement = doStatement.WithWhileKeyword(whileNode).WithCondition(condition);

                //Accumulating all replacements
                replaceDictionary.Add(doStatement, newDoStatement);
            }

        }

        syntaxRoot = syntaxRoot.ReplaceNodes(replaceDictionary.Keys, (node1, node2) => replaceDictionary[node1]);

        //Output preprocessed code
        Console.WriteLine(syntaxRoot.GetText());
    }
}
*/
