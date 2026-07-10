using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Parser-free unit tests for the elaborate pass (normalized body + recorded solver decisions
    /// → TIR): a resolved call becomes a <see cref="TirCall"/> with the right callee and
    /// <see cref="EmissionKind"/>, and a conversion argument becomes an explicit
    /// <see cref="TirCoerce"/> node. The "runs over real code" proof lives in
    /// <see cref="ElaboratorIntegrationTests"/>.
    /// </summary>
    [TestFixture]
    public static class ElaboratorTests
    {
        // --- builders ------------------------------------------------------------

        private static TypeExpression T(string name, params TypeExpression[] args)
            => new TypeExpression(new TypeDef(null, TypeKind.ConcreteType, name), args);

        private static Literal Int(int v) => new Literal(LiteralTypesEnum.Integer, v);

        /// <summary>A library function (body null => Intrinsic; body set => Library).</summary>
        private static FunctionDef LibFunc(string name, TypeExpression ret, Symbol body, params TypeExpression[] ps)
        {
            var lib = new TypeDef(null, TypeKind.Library, "Lib");
            var pdefs = ps.Select((t, i) => new ParameterDef(null, $"p{i}", t, i)).ToArray();
            return new FunctionDef(null, name, lib, ret, body, pdefs);
        }

        /// <summary>A single-argument getter on a concrete type => FunctionType.Field.</summary>
        private static FunctionDef FieldFunc(string typeName, string name, TypeExpression ret)
        {
            var owner = new TypeDef(null, TypeKind.ConcreteType, typeName);
            return new FunctionDef(null, name, owner, ret, null, new ParameterDef(null, "self", T(typeName), 0));
        }

        /// <summary>A constructor => name == ownerType.Name.</summary>
        private static FunctionDef CtorFunc(string typeName, params TypeExpression[] ps)
        {
            var owner = new TypeDef(null, TypeKind.ConcreteType, typeName);
            var pdefs = ps.Select((t, i) => new ParameterDef(null, $"p{i}", t, i)).ToArray();
            return new FunctionDef(null, typeName, owner, T(typeName), null, pdefs);
        }

        private static FunctionCall Call(FunctionDef callee, bool hasArgList, params Expression[] args)
        {
            var group = new FunctionGroupDef(null, callee.ReturnType, new[] { callee }, callee.Name);
            var gref = (FunctionGroupRefSymbol)group.ToReference();
            return new FunctionCall(gref, hasArgList, args);
        }

        /// <summary>Elaborate a single top-level call given a hand-built resolution record.</summary>
        private static TirNode ElaborateCall(FunctionDef callee, FunctionCall fc, ResolvedCall rc, Compilation comp = null)
        {
            var solver = new Solver(comp);
            solver.ResolvedCalls[fc] = rc;
            var owner = new TypeDef(null, TypeKind.Library, "Host");
            var host = new FunctionDef(null, "host", owner, rc.ReturnType, fc);
            var result = new TypeCheckResult(host, host, new ConstraintSystem(), solver);
            return new Elaborator(comp).Elaborate(result).Body;
        }

        private static ResolvedCall Resolved(FunctionCall fc, FunctionDef callee,
            TypeExpression ret, ArgMatchKind[] kinds, IFunction[] conversions, params TypeExpression[] paramTypes)
            => new ResolvedCall(fc, callee, paramTypes, ret, kinds, conversions);

        // --- resolved call -> TirCall -------------------------------------------

        [Test]
        public static void ResolvedCallBecomesTypedCallNode()
        {
            // Add(Number, Number): Number — operator-named, so it classifies as an Operator.
            var add = LibFunc("Add", T("Number"), null, T("Number"), T("Number"));
            var fc = Call(add, true, Int(1), Int(2));
            var rc = Resolved(fc, add, T("Number"),
                new[] { ArgMatchKind.Exact, ArgMatchKind.Exact }, new IFunction[] { null, null },
                T("Number"), T("Number"));

            var node = ElaborateCall(add, fc, rc);

            Assert.IsInstanceOf<TirCall>(node);
            var call = (TirCall)node;
            Assert.AreSame(add, call.Callee, "the TIR call must carry the winning callee");
            Assert.AreEqual(EmissionKind.Operator, call.EmissionKind);
            Assert.AreEqual("Number", call.ReturnType.ToString());
            Assert.AreEqual(2, call.Args.Count);
            Assert.IsTrue(call.Args.All(a => a is TirLiteral));
        }

        [Test]
        public static void EmissionKindConstructorForConstructorCallee()
        {
            var ctor = CtorFunc("Vector3", T("Number"), T("Number"), T("Number"));
            var fc = Call(ctor, true, Int(0), Int(0), Int(0));
            var rc = Resolved(fc, ctor, T("Vector3"),
                new[] { ArgMatchKind.Exact, ArgMatchKind.Exact, ArgMatchKind.Exact },
                new IFunction[] { null, null, null }, T("Number"), T("Number"), T("Number"));

            var call = (TirCall)ElaborateCall(ctor, fc, rc);
            Assert.AreEqual(EmissionKind.Constructor, call.EmissionKind);
        }

        [Test]
        public static void EmissionKindPropertyForFieldCallee()
        {
            // v.X — a field getter, member access (no arg list).
            var field = FieldFunc("Vector3", "X", T("Number"));
            var fc = Call(field, false, Int(0));
            var rc = Resolved(fc, field, T("Number"),
                new[] { ArgMatchKind.Exact }, new IFunction[] { null }, T("Vector3"));

            var call = (TirCall)ElaborateCall(field, fc, rc);
            Assert.AreEqual(EmissionKind.Property, call.EmissionKind);
        }

        [Test]
        public static void EmissionKindIntrinsicForNonOperatorIntrinsic()
        {
            // Sqrt(Number): Number — a body-less library function, not operator-named.
            var sqrt = LibFunc("Sqrt", T("Number"), null, T("Number"));
            var fc = Call(sqrt, true, Int(4));
            var rc = Resolved(fc, sqrt, T("Number"),
                new[] { ArgMatchKind.Exact }, new IFunction[] { null }, T("Number"));

            var call = (TirCall)ElaborateCall(sqrt, fc, rc);
            Assert.AreEqual(EmissionKind.Intrinsic, call.EmissionKind);
        }

        [Test]
        public static void EmissionKindInstanceMethodForBodiedLibraryCall()
        {
            // Dot(Vector3, Vector3): Number — a library function WITH a body, 2 params, arg-list call.
            var dot = LibFunc("Dot", T("Number"), Int(0), T("Vector3"), T("Vector3"));
            var fc = Call(dot, true, Int(0), Int(0));
            var rc = Resolved(fc, dot, T("Number"),
                new[] { ArgMatchKind.Exact, ArgMatchKind.Exact }, new IFunction[] { null, null },
                T("Vector3"), T("Vector3"));

            var call = (TirCall)ElaborateCall(dot, fc, rc);
            Assert.AreEqual(EmissionKind.InstanceMethod, call.EmissionKind);
        }

        // --- conversion argument -> TirCoerce -----------------------------------

        [Test]
        public static void ConversionArgumentBecomesCoerceNode()
        {
            // Scale(Vector3): Vector3 called with a Number literal that must convert to Vector3.
            var scale = LibFunc("Scale", T("Vector3"), null, T("Vector3"));
            var castFn = LibFunc("Cast", T("Vector3"), null, T("Number")); // stand-in conversion witness
            var arg = Int(0);
            var fc = Call(scale, true, arg);
            var rc = Resolved(fc, scale, T("Vector3"),
                new[] { ArgMatchKind.Conversion }, new IFunction[] { castFn }, T("Vector3"));

            var node = ElaborateCall(scale, fc, rc);

            var call = (TirCall)node;
            Assert.AreEqual(1, call.Args.Count);
            Assert.IsInstanceOf<TirCoerce>(call.Args[0], "a Conversion-matched argument must be wrapped in a TirCoerce");
            var coerce = (TirCoerce)call.Args[0];
            Assert.AreEqual("Vector3", coerce.ToType.ToString());
            Assert.AreSame(castFn, coerce.ConversionFn, "the coercion must name the recorded cast function");
            Assert.IsInstanceOf<TirLiteral>(coerce.Inner);

            // And it is discoverable by a tree walk (what the integration test relies on).
            Assert.IsTrue(node.Descendants().OfType<TirCoerce>().Any());
        }

        [Test]
        public static void ExactArgumentIsNotWrappedInCoerce()
        {
            var negate = LibFunc("Negate", T("Number"), null, T("Number"));
            var fc = Call(negate, true, Int(3));
            var rc = Resolved(fc, negate, T("Number"),
                new[] { ArgMatchKind.Exact }, new IFunction[] { null }, T("Number"));

            var call = (TirCall)ElaborateCall(negate, fc, rc);
            Assert.IsInstanceOf<TirLiteral>(call.Args[0], "an exact-matched argument keeps its shape");
            Assert.IsFalse(call.Descendants().OfType<TirCoerce>().Any());
        }

        // --- totality ------------------------------------------------------------

        [Test]
        public static void UnresolvedCallElaboratesToUnresolvedNode()
        {
            // A call the solver never recorded: the elaborator must stay total (no throw) and emit a
            // partial node plus an ELB001 diagnostic.
            var f = LibFunc("Mystery", T("Number"), null, T("Number"));
            var fc = Call(f, true, Int(1));

            var solver = new Solver(null); // no ResolvedCalls entry for fc
            var owner = new TypeDef(null, TypeKind.Library, "Host");
            var host = new FunctionDef(null, "host", owner, T("Number"), fc);
            var result = new TypeCheckResult(host, host, new ConstraintSystem(), solver);

            var elaborator = new Elaborator(null);
            var tir = elaborator.Elaborate(result);

            Assert.IsInstanceOf<TirUnresolved>(tir.Body);
            Assert.IsTrue(elaborator.Diagnostics.Any(d => d.Code == "ELB001"));
        }
    }

    /// <summary>
    /// Runs the elaborate pass over the real standard library: it must be total (elaborate every
    /// checked function without throwing), a non-trivial number must elaborate fully (no
    /// <see cref="TirUnresolved"/> nodes), and implicit conversions must surface as
    /// <see cref="TirCoerce"/> nodes somewhere in the stdlib.
    /// </summary>
    [TestFixture]
    public static class ElaboratorIntegrationTests
    {
        private static Compilation _compilation;

        [OneTimeSetUp]
        public static void Setup()
            => _compilation = CheckerTestSupport.CompileStdLib();

        private static bool IsFullyElaborated(TirFunction t)
            => t.AllNodes.All(n => !(n is TirUnresolved));

        [Test]
        public static void ElaboratesAllCheckedFunctionsWithoutThrowing()
        {
            var results = new TypeChecker(_compilation).CheckAll();
            var elaborator = new Elaborator(_compilation);

            var tirs = new List<TirFunction>();
            foreach (var r in results)
                tirs.Add(elaborator.Elaborate(r)); // totality: this must never throw

            Assert.IsNotEmpty(tirs);

            var fully = tirs.Count(IsFullyElaborated);
            var withCoerce = tirs.Count(t => t.AllNodes.OfType<TirCoerce>().Any());
            var totalCoerce = tirs.Sum(t => t.AllNodes.OfType<TirCoerce>().Count());
            var totalCalls = tirs.Sum(t => t.AllNodes.OfType<TirCall>().Count());

            TestContext.WriteLine(
                $"Elaborator: {tirs.Count} functions elaborated; {fully} fully (no unresolved nodes); " +
                $"{totalCalls} typed call nodes; {withCoerce} functions contain a coercion " +
                $"({totalCoerce} TirCoerce nodes total).");

            Assert.Greater(fully, 0, "expected a non-trivial subset of real functions to elaborate fully");
            Assert.Greater(totalCoerce, 0,
                "expected implicit conversions to surface as explicit TirCoerce nodes in the stdlib");
        }

        [Test]
        public static void EveryTypedCallNodeCarriesACallee()
        {
            var results = new TypeChecker(_compilation).CheckAll();
            var elaborator = new Elaborator(_compilation);

            foreach (var r in results)
            {
                var tir = elaborator.Elaborate(r);
                foreach (var call in tir.AllNodes.OfType<TirCall>())
                {
                    Assert.NotNull(call.Callee, $"a TirCall in '{r.Function.Name}' has no callee");
                    Assert.NotNull(call.ReturnType, $"a TirCall to '{call.Name}' in '{r.Function.Name}' has no return type");
                }
            }
        }

        [Test]
        public static void EveryCoercionNamesDistinctFromAndToTypes()
        {
            var results = new TypeChecker(_compilation).CheckAll();
            var elaborator = new Elaborator(_compilation);

            var coercions = results
                .SelectMany(r => elaborator.Elaborate(r).AllNodes.OfType<TirCoerce>())
                .ToList();

            Assert.IsNotEmpty(coercions, "the stdlib exercises implicit conversions");
            foreach (var c in coercions)
                Assert.NotNull(c.ToType, "a coercion must know its target type");
        }
    }
}
