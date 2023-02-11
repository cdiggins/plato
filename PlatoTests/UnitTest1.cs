using PlatoParser;
using PlatoTypeInference;

namespace PlatoTests
{
    

    public static class ParserTests
    {
        public static string TestDigits = "0123456789";
        public static string TestNumbersThenUpperCaseLetters = "0123ABC";
        public static string HelloWorld = "Hello world!";
        public static string MathEquation = "(1.23 + (4.56 / 7.9) - 0.8)";
        public static string SomeCode = "var x = 123; x += 23; f(1, a);";

        [Test]
        public static void TestParser()
        {
            {
                var ps = TestDigits.Parse(CommonRules.Digits);
                Assert.NotNull(ps);
                Assert.IsTrue(ps.AtEnd);
            }
            {
                var ps = TestDigits.Parse(CommonRules.Digits.Node("Digits"));
                Console.WriteLine(ps.Node);
            }
        }
    }

    public static class InferenceTests
    {
        public static TypeVar Var(string name, int id) => new (name, id);
        public static TypeList List(params BaseType[] types) => new(types);
        public static TypeConstant Const(string name) => new(name);

        [Test]
        public static void TestInference()
        {
            var t0 = Const("int");
            var t1 = Const("func");
            
            var var0 = Var("a", 0);
            var var1 = Var("b", 1);
            var var2 = Var("c", 2);
            var var3 = Var("d", 3);

            var f1 = List(t1, var0, var1);
            var f2 = List(t1, var1, var2);

            {
                var unifier = new TypeUnifier();
                var r = unifier.Unify(f1, f2);
                Console.WriteLine($"Unification of {f1} and {f2} produced {r}");
            }
        }
    }
}