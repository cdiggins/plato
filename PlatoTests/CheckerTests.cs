using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Parser-free unit tests for the solve pass: unification, occurs check, generic
    /// instantiation, and overload resolution (unique / none / same-return / ambiguous).
    /// These pin the solver's behavior precisely and independently of any real source.
    /// </summary>
    [TestFixture]
    public static class CheckerTests
    {
        // --- builders ------------------------------------------------------------

        private static TypeExpression Var(string name)
            => TypeExpression.CreateTypeVar(null, "$" + name);

        /// <summary>A named nominal type (fresh TypeDef; the solver matches by name).</summary>
        private static TypeExpression T(string name, params TypeExpression[] args)
            => new TypeExpression(new TypeDef(null, TypeKind.ConcreteType, name), args);

        private static Symbol Origin() => new Literal(LiteralTypesEnum.Integer, 0);

        private static FunctionDef Func(string name, TypeExpression ret, params TypeExpression[] ps)
        {
            var lib = new TypeDef(null, TypeKind.Library, "Lib");
            var pdefs = ps.Select((t, i) => new ParameterDef(null, $"p{i}", t, i)).ToArray();
            return new FunctionDef(null, name, lib, ret, null, pdefs);
        }

        private static (Solver solver, TypeExpression result) SolveCall(
            string name, TypeExpression[] argTypes, params FunctionDef[] candidates)
        {
            var sys = new ConstraintSystem();
            var result = Var("R");
            sys.Add(new OverloadConstraint(null, name, result, argTypes, candidates, Origin()));
            var solver = new Solver(null);
            solver.Solve(sys);
            return (solver, result);
        }

        private static bool HasError(Solver s, string code)
            => s.Diagnostics.Any(d => d.Code == code && d.Severity == DiagnosticSeverity.Error);

        // --- unification ---------------------------------------------------------

        [Test]
        public static void UnifiesVariableWithConcrete()
        {
            var s = new Solver(null);
            var sub = s.Substitution;
            var a = Var("a");
            Assert.IsTrue(s.Unify(a, T("Number"), sub, Origin(), record: true));
            Assert.AreEqual("Number", s.Zonk(a).ToString());
            Assert.IsTrue(s.Succeeded);
        }

        [Test]
        public static void ClashOnDifferentConstantsIsReported()
        {
            var s = new Solver(null);
            Assert.IsFalse(s.Unify(T("Number"), T("Boolean"), s.Substitution, Origin(), record: true));
            Assert.IsTrue(HasError(s, "CHK101"));
        }

        [Test]
        public static void UnifiesNestedTypeArguments()
        {
            var s = new Solver(null);
            var a = Var("a");
            Assert.IsTrue(s.Unify(T("Array", a), T("Array", T("Number")), s.Substitution, Origin(), record: true));
            Assert.AreEqual("Number", s.Zonk(a).ToString());
        }

        [Test]
        public static void ArityMismatchIsReported()
        {
            var s = new Solver(null);
            Assert.IsFalse(s.Unify(T("Tuple", T("A")), T("Tuple", T("A"), T("B")), s.Substitution, Origin(), record: true));
            Assert.IsTrue(HasError(s, "CHK102"));
        }

        [Test]
        public static void OccursCheckPreventsInfiniteType()
        {
            var s = new Solver(null);
            var a = Var("a");
            // $a ~ Array<$a> must fail with a recursive-type diagnostic, not loop.
            Assert.IsFalse(s.Unify(a, T("Array", a), s.Substitution, Origin(), record: true));
            Assert.IsTrue(HasError(s, "CHK103"));
        }

        // --- overload resolution -------------------------------------------------

        [Test]
        public static void ResolvesUniqueOverload()
        {
            var add = Func("Add", T("Number"), T("Number"), T("Number"));
            var (s, result) = SolveCall("Add", new[] { T("Number"), T("Number") }, add);
            Assert.IsTrue(s.Succeeded);
            Assert.AreEqual("Number", s.Zonk(result).ToString());
        }

        [Test]
        public static void PicksMatchingOverloadByArgumentTypes()
        {
            var addNum = Func("Add", T("Number"), T("Number"), T("Number"));
            var addVec = Func("Add", T("Vector3"), T("Vector3"), T("Vector3"));
            var (s, result) = SolveCall("Add", new[] { T("Vector3"), T("Vector3") }, addNum, addVec);
            Assert.IsTrue(s.Succeeded);
            Assert.AreEqual("Vector3", s.Zonk(result).ToString());
        }

        [Test]
        public static void ReportsNoMatchingOverload()
        {
            var addNum = Func("Add", T("Number"), T("Number"), T("Number"));
            var (s, _) = SolveCall("Add", new[] { T("Boolean"), T("Boolean") }, addNum);
            Assert.IsFalse(s.Succeeded);
            Assert.IsTrue(HasError(s, "CHK201"));
        }

        [Test]
        public static void ReportsAmbiguityOnDifferentReturnTypes()
        {
            // Two overloads that both accept (Any-ish) generic arg but return different types.
            var f1 = Func("F", T("Number"), Var("T"));   // F<$T>($T): Number
            var f2 = Func("F", T("Boolean"), Var("U"));  // F<$U>($U): Boolean
            var (s, _) = SolveCall("F", new[] { T("Vector3") }, f1, f2);
            Assert.IsTrue(HasError(s, "CHK203"));
        }

        [Test]
        public static void AllowsMultipleOverloadsWithCommonReturnType()
        {
            var f1 = Func("F", T("Number"), Var("T"));   // F<$T>($T): Number
            var f2 = Func("F", T("Number"), Var("U"));   // F<$U>($U): Number
            var (s, result) = SolveCall("F", new[] { T("Vector3") }, f1, f2);
            Assert.IsTrue(s.Succeeded, "common return type is not an error");
            Assert.AreEqual("Number", s.Zonk(result).ToString());
            Assert.IsTrue(s.Diagnostics.Any(d => d.Code == "CHK202"));
        }

        [Test]
        public static void InstantiatesGenericOverloadPerCall()
        {
            // Id<$T>($T): $T  applied to Number yields Number; applied to Boolean yields Boolean.
            var id = Func("Id", Var("T"), Var("T"));

            var (s1, r1) = SolveCall("Id", new[] { T("Number") }, id);
            Assert.AreEqual("Number", s1.Zonk(r1).ToString());

            var (s2, r2) = SolveCall("Id", new[] { T("Boolean") }, id);
            Assert.AreEqual("Boolean", s2.Zonk(r2).ToString());
        }
    }
}
