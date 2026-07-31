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

        // --- concept satisfaction & ranking --------------------------------------

        private static TypeExpression Concept(string name)
            => new TypeExpression(new TypeDef(null, TypeKind.Interface, name));

        /// <summary>A concrete type declared to implement the given concepts.</summary>
        private static TypeExpression Implementing(string name, params TypeExpression[] concepts)
        {
            var td = new TypeDef(null, TypeKind.ConcreteType, name);
            td.Implements.AddRange(concepts);
            return td.ToTypeExpression();
        }

        [Test]
        public static void ConcreteArgumentSatisfiesInterfaceParameter()
        {
            var inum = Concept("INumerical");
            var negate = Func("Negate", inum, inum);           // Negate(x: INumerical): INumerical
            var number = Implementing("Number", inum);

            var (s, result) = SolveCall("Negate", new[] { number }, negate);
            Assert.IsTrue(s.Succeeded);
            // Self refinement: the return type is the concrete argument, not the interface.
            Assert.AreEqual("Number", s.Zonk(result).ToString());
        }

        [Test]
        public static void NonImplementingArgumentDoesNotMatchInterface()
        {
            var inum = Concept("INumerical");
            var negate = Func("Negate", inum, inum);
            var boolean = T("Boolean"); // implements nothing

            var (s, _) = SolveCall("Negate", new[] { boolean }, negate);
            Assert.IsFalse(s.Succeeded);
            Assert.IsTrue(HasError(s, "CHK201"));
        }

        [Test]
        public static void ExactOverloadBeatsConceptOverload()
        {
            var inum = Concept("INumerical");
            var number = Implementing("Number", inum);
            var exact = Func("F", T("Number"), T("Number"));   // F(Number): Number   (cost 0)
            var viaConcept = Func("F", T("Boolean"), inum);    // F(INumerical): Boolean (cost 3)

            var (s, result) = SolveCall("F", new[] { number }, exact, viaConcept);
            Assert.IsTrue(s.Succeeded, "the exact overload should win outright, not tie");
            Assert.AreEqual("Number", s.Zonk(result).ToString());
        }

        [Test]
        public static void ConcreteOverloadBeatsGenericOverload()
        {
            var specific = Func("G", T("Number"), T("Number")); // G(Number): Number    (cost 0)
            var generic = Func("G", Var("T"), Var("T"));         // G<$T>($T): $T         (cost 2)

            var (s, result) = SolveCall("G", new[] { T("Number") }, specific, generic);
            Assert.IsTrue(s.Succeeded);
            Assert.AreEqual("Number", s.Zonk(result).ToString());
        }

        [Test]
        public static void GenericConceptBindsElementType()
        {
            // IArray<T> concept; List<T> implements IArray<T>; candidate First(xs: IArray<$E>): $E.
            var iarrayDef = new TypeDef(null, TypeKind.Interface, "IArray");
            iarrayDef.TypeParameters.Add(new TypeParameterDef(null, "T"));

            var listDef = new TypeDef(null, TypeKind.ConcreteType, "List");
            var listTP = new TypeParameterDef(null, "T");
            listDef.TypeParameters.Add(listTP);
            listDef.Implements.Add(new TypeExpression(iarrayDef, new TypeExpression(listTP)));

            var listOfNumber = new TypeExpression(listDef, T("Number"));
            var elem = Var("E");
            var first = Func("First", elem, new TypeExpression(iarrayDef, elem)); // First(IArray<$E>): $E

            var (s, result) = SolveCall("First", new[] { listOfNumber }, first);
            Assert.IsTrue(s.Succeeded);
            Assert.AreEqual("Number", s.Zonk(result).ToString(),
                "the element type $E should be inferred as Number from List<Number> : IArray<Number>");
        }
    }
}
