using Parakeet;
using Parakeet.Tests;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using PlatoAst;

namespace PlatoTests
{
    public class AstTests
    {
        [Test]
        public static void Test()
        {
        }

        public static string DllPath => typeof(AstTests).Assembly.Location;
        public static string ProjectFolder => Path.Combine(Path.GetDirectoryName(DllPath), "..", "..", "..");
        public static string SolutionFolder => Path.Combine(ProjectFolder, "..");

        [Test, Explicit]
        public static void OutputCstCode()
        {
            var g = new AstGrammar();
            var folder = Path.Combine(SolutionFolder, "PlatoAst");
            {
                var cb = new CodeBuilder();
                CstCodeBuilder.OutputCstClassesFile(cb, $"Plato.Ast", g.GetRules());
                var path = Path.Combine(folder, $"Cst.cs");
                var text = cb.ToString();
                Console.WriteLine(text);
                File.WriteAllText(path, text);
            }
            {
                var cb = new CodeBuilder();
                CstCodeBuilder.OutputCstFactoryFile(cb, $"Plato.Ast", g.GetRules());
                var path = Path.Combine(folder, $"CstFactory.cs");
                var text = cb.ToString();
                Console.WriteLine(text);
                File.WriteAllText(path, text);
            }
        }

        public static int MultiplyBy3(int x)
        {
            return x * 3;
        }

        public class X
        {
            public int Y { get; set; }
        }

        public static Expression<Func<int, int, int>> BinaryInt(int n)
        {
            var tmp = new X();
            Func<int, int> f = (x => x * 2);
            switch (n)
            {
                case 0: return (a, b) => a + b;
                case 1: return (a, b) => (a * 2) + (b * 3);
                case 2: return (a, b) => f(a) + b;
                case 3: return (a, b) => MultiplyBy3(a) + MultiplyBy3(b);
                case 4: return (a, b) => a & 0x1 | b & 0x2;
                case 5: return (a, b) => (new X()).Y;
                case 6: return (a, b) => tmp.Y;
            }
            throw new NotImplementedException();
        }
    }
}
