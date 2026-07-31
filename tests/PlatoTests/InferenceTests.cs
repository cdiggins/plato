using PlatoTypeInference;

namespace PlatoTests
{
    public static class InferenceTests
    {
        public static IEnumerable<string> Types
            = new[]
            {
                "$a",
                "abc",
                "=>",
                "[a,b,c]",
                "[a,[b]]",
                "[[a],[b]]",
                "[]",
                "[$a,$b]",
                "[[a],[b,c]]",
                "\\($a)[$a]",
                "\\($a,$b)[=>,[$a,$b],[c,c,$b]]",
            };

        [TestCaseSource(nameof(Types))]
        public static void TestType(string name)
        {
            var t = BaseType.Parse(name);
            Assert.NotNull(t);
            Assert.AreEqual(name, t.Name);
            Assert.AreEqual(name, t.ToString());
            if (name.StartsWith("$"))
            {
                Assert.IsAssignableFrom(typeof(TypeVariable), t);
            }
            else if (name.StartsWith("["))
            {
                Assert.IsAssignableFrom(typeof(TypeList), t);
            }
            else if (name.StartsWith("\\"))
            {
                Assert.IsAssignableFrom(typeof(PolyType), t);
            }
            else 
            {
                Assert.IsAssignableFrom(typeof(TypeConstant), t);
            }
        }

        public static IEnumerable<TestCaseData> ConstraintInputs
            => new[]
            {
                new TestCaseData("a", "a"),
                new TestCaseData("$a", "b"),
                new TestCaseData("b", "$a"),
                new TestCaseData("[$a,$b]", "[X,y]"),
                new TestCaseData("[$a,$a]", "[X,X]"),
                new TestCaseData("[$a,$b,$c]", "[X,y,$a]"),
                new TestCaseData("[$a,$b]", "[X,X]"),
                new TestCaseData("[$a,$b,$c]", "[$b,X,y]"),
            };

        public static void OutputConstraints(Constraints c)
        {
            foreach (var kv in c.Sets)
            {
                var types = string.Join(",", kv.Value);
                Console.WriteLine($"{kv.Key} = {types}");
            }

            foreach (var g in c.GetVariableGroups())
            {
                Console.WriteLine(string.Join('=', g));
            }
        }

        [TestCaseSource(nameof(ConstraintInputs))]
        public static void TestConstraints(string s1, string s2)
        {
            TestType(s1); 
            TestType(s2);
            var t1 = BaseType.Parse(s1);
            var t2 = BaseType.Parse(s2);
            var c = new Constraints();
            c.AddConstraint(t1, t2);
            OutputConstraints(c);
        }
    }
}   