using PlatoTypeInference;

namespace PlatoTests
{
    public static class InferenceTests
    {
        public static TypeVar Var(string name, int id) => new (name);
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