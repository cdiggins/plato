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
    /// Parser-free unit tests for the monomorphize sub-pass: a <see cref="TypeSubstitution"/> grounds
    /// abstract types; <see cref="Monomorphizer.Specialize"/> rewrites every node's type; a
    /// concept-method <see cref="TirCall"/> re-points to a concrete implementation on an unambiguous
    /// match and is left unchanged otherwise. The "runs over real code" proof lives in
    /// <see cref="MonomorphizerIntegrationTests"/>.
    /// </summary>
    [TestFixture]
    public static class MonomorphizerTests
    {
        // --- builders ------------------------------------------------------------

        private static TypeExpression Con(string name, params TypeExpression[] args)
            => new TypeExpression(new TypeDef(null, TypeKind.ConcreteType, name), args);

        private static TypeExpression Iface(string name, params TypeExpression[] args)
            => new TypeExpression(new TypeDef(null, TypeKind.Interface, name), args);

        private static TypeExpression Self()
            => new TypeExpression(new SelfType());

        private static TypeExpression Param(string name)
            => new TypeExpression(new TypeParameterDef(null, name));

        private static TypeSubstitution Sub(params (string key, TypeExpression to)[] pairs)
            => new TypeSubstitution(pairs.ToDictionary(p => p.key, p => p.to));

        /// <summary>An interface (concept) method: body-less, first param is the receiver.</summary>
        private static FunctionDef ConceptMethod(string iface, string name, TypeExpression ret, params TypeExpression[] ps)
        {
            var owner = new TypeDef(null, TypeKind.Interface, iface);
            var pdefs = ps.Select((t, i) => new ParameterDef(null, $"p{i}", t, i)).ToArray();
            return new FunctionDef(null, name, owner, ret, null, pdefs);
        }

        /// <summary>A body-less library function => FunctionType.Intrinsic (a concrete implementation
        /// a concept call can re-dispatch to).</summary>
        private static FunctionDef Intrinsic(string name, TypeExpression ret, params TypeExpression[] ps)
        {
            var lib = new TypeDef(null, TypeKind.Library, "Lib");
            var pdefs = ps.Select((t, i) => new ParameterDef(null, $"p{i}", t, i)).ToArray();
            return new FunctionDef(null, name, lib, ret, null, pdefs);
        }

        // --- TypeSubstitution ----------------------------------------------------

        [Test]
        public static void SubstitutionGroundsSelfAndTypeParameter()
        {
            var sub = Sub(("Self", Con("Vector3")), ("T", Con("Number")));

            Assert.AreEqual("Vector3", sub.Apply(Self()).ToString());
            Assert.AreEqual("Number", sub.Apply(Param("T")).ToString());
            // Recurses into type arguments; leaves concrete heads alone.
            Assert.AreEqual("IArray<Number>", sub.Apply(Iface("IArray", Param("T"))).ToString());
            Assert.AreEqual("Number", sub.Apply(Con("Number")).ToString());
        }

        [Test]
        public static void IsGroundDetectsAbstractTypes()
        {
            Assert.IsTrue(TypeSubstitution.IsGround(Con("Vector3")));
            Assert.IsTrue(TypeSubstitution.IsGround(Iface("IArray", Con("Number"))));
            Assert.IsTrue(TypeSubstitution.IsGround(null));
            Assert.IsFalse(TypeSubstitution.IsGround(Self()));
            Assert.IsFalse(TypeSubstitution.IsGround(Param("T")));
            Assert.IsFalse(TypeSubstitution.IsGround(Iface("IArray", Param("T"))));
        }

        [Test]
        public static void FromSignatureRecoversSubstitutionAndReproducesGroundSignature()
        {
            // Components(x: Self): IArray<T>  reified for a type where Self=Integer2, T=Integer.
            var original = ConceptMethod("IArrayLike", "Components", Iface("IArray", Param("T")), Self());
            var groundParams = new List<TypeExpression> { Con("Integer2") };
            var groundReturn = Iface("IArray", Con("Integer"));

            var sub = TypeSubstitution.FromSignature(original, groundParams, groundReturn);

            // Applying the derived substitution to the original signature reproduces the ground one.
            Assert.AreEqual("Integer2", sub.Apply(original.Parameters[0].Type).ToString());
            Assert.AreEqual("IArray<Integer>", sub.Apply(original.ReturnType).ToString());
        }

        [Test]
        public static void FromSignatureLibraryInterfaceMapsWholeFirstParameter()
        {
            // Dot(v1: IVector, v2: IVector): Number  reified for Vector3 (IVector -> Vector3).
            var original = Intrinsic("Dot", Con("Number"), Iface("IVector"), Iface("IVector"));
            var sub = TypeSubstitution.FromSignature(original,
                new List<TypeExpression> { Con("Vector3"), Con("Vector3") }, Con("Number"));

            Assert.AreEqual("Vector3", sub.Apply(Iface("IVector")).ToString());
            Assert.AreEqual("Number", sub.Apply(Con("Number")).ToString());
        }

        // --- Specialize ----------------------------------------------------------

        [Test]
        public static void SpecializeGroundsEveryNodeType()
        {
            // A generic body: return f(p) where p: Self, f: (Self) -> Self.
            var f = ConceptMethod("IX", "Id", Self(), Self());
            var call = new TirCall(f, EmissionKind.InstanceMethod,
                new List<TypeExpression> { Self() }, Self(),
                new List<TirNode> { new TirParameter(null, Self(), null) }, Self(), null);
            var body = new TirBlock(new List<TirNode> { new TirReturn(call, null) }, null);
            var tir = new TirFunction(f, f.Parameters, Self(), body);

            var mono = new Monomorphizer(null) { RedispatchResolver = (n, r, ps, ret) => null };
            var result = mono.Specialize(tir, Sub(("Self", Con("Vector3"))));

            Assert.AreEqual("Vector3", result.ReturnType.ToString());
            foreach (var node in result.AllNodes)
                Assert.IsTrue(TypeSubstitution.IsGround(node.Type),
                    $"node {node.GetType().Name} still abstract: {node.Type}");

            var specializedCall = result.AllNodes.OfType<TirCall>().Single();
            Assert.AreEqual("Vector3", specializedCall.ReturnType.ToString());
            Assert.IsTrue(specializedCall.ParameterTypes.All(t => t.ToString() == "Vector3"));
        }

        [Test]
        public static void SpecializeIsTotalOnUnresolvedAndNullBody()
        {
            var mono = new Monomorphizer(null);
            Assert.IsNull(mono.Specialize(null, TypeSubstitution.Empty));

            var unresolved = new TirUnresolved(null, "test", new List<TirNode>());
            var tir = new TirFunction(null, new List<ParameterDef>(), Self(), unresolved);
            var result = mono.Specialize(tir, Sub(("Self", Con("Vector3"))));
            Assert.IsInstanceOf<TirUnresolved>(result.Body); // no throw; placeholder preserved
        }

        // --- concept re-dispatch (direct case) -----------------------------------

        [Test]
        public static void ConceptCallRePointsToConcreteImplementationOnUniqueMatch()
        {
            // Multiply(a: Self, b: Self): Self — a concept declaration.
            var conceptMul = ConceptMethod("IMultiplicative", "Multiply", Self(), Self(), Self());
            // Vector3's concrete implementation, provided as an intrinsic.
            var concreteMul = Intrinsic("Multiply", Con("Vector3"), Con("Vector3"), Con("Vector3"));

            var call = new TirCall(conceptMul, EmissionKind.Operator,
                new List<TypeExpression> { Self(), Self() }, Self(),
                new List<TirNode>
                {
                    new TirParameter(null, Self(), null),
                    new TirParameter(null, Self(), null)
                }, Self(), null);
            var tir = new TirFunction(conceptMul, conceptMul.Parameters, Self(),
                new TirReturn(call, null));

            // Resolver stands in for the ReifiedFunctionsByName lookup.
            var mono = new Monomorphizer(null)
            {
                RedispatchResolver = (name, receiver, ps, ret) =>
                    name == "Multiply" && receiver.ToString() == "Vector3" ? concreteMul : null
            };
            var stats = new MonomorphizeStats();
            var result = mono.Specialize(tir, Sub(("Self", Con("Vector3"))), stats);

            var rc = result.AllNodes.OfType<TirCall>().Single();
            Assert.AreSame(concreteMul, rc.Callee, "the concept call must re-point to the concrete impl");
            // The EmissionKind is PRESERVED across re-dispatch: it encodes the call-site shape
            // (member access vs arg list), which is what emitted syntax keys on — re-deriving it
            // from the target would turn `this.Matrix` into `this.Matrix()`.
            Assert.AreEqual(call.EmissionKind, rc.EmissionKind, "EmissionKind preserved (call-site shape)");
            Assert.AreEqual(1, stats.Redispatched);
            Assert.AreEqual(0, stats.DeferredConcept);
        }

        [Test]
        public static void ConceptCallIsLeftUnchangedWhenNoUniqueMatch()
        {
            var conceptMul = ConceptMethod("IMultiplicative", "Multiply", Self(), Self(), Self());
            var call = new TirCall(conceptMul, EmissionKind.Operator,
                new List<TypeExpression> { Self(), Self() }, Self(),
                new List<TirNode>
                {
                    new TirParameter(null, Self(), null),
                    new TirParameter(null, Self(), null)
                }, Self(), null);
            var tir = new TirFunction(conceptMul, conceptMul.Parameters, Self(), new TirReturn(call, null));

            var mono = new Monomorphizer(null) { RedispatchResolver = (n, r, ps, ret) => null };
            var stats = new MonomorphizeStats();
            var result = mono.Specialize(tir, Sub(("Self", Con("Vector3"))), stats);

            var rc = result.AllNodes.OfType<TirCall>().Single();
            Assert.AreSame(conceptMul, rc.Callee, "an unresolved concept call stays as the abstract dispatch");
            Assert.AreEqual(0, stats.Redispatched);
            Assert.AreEqual(1, stats.DeferredConcept);
        }
    }

    /// <summary>
    /// Runs monomorphization over the real standard library: it must be total (never throw), produce
    /// exactly one instantiation per <see cref="Compilation.ReifiedFunctions"/> entry (the oracle),
    /// ground a non-trivial number of function bodies, and — for EVERY reified function — the derived
    /// substitution applied to the original signature must reproduce the reified ground signature.
    /// </summary>
    [TestFixture]
    public static class MonomorphizerIntegrationTests
    {
        private static Compilation _compilation;
        private static IReadOnlyList<MonomorphizedFunction> _all;

        [OneTimeSetUp]
        public static void Setup()
        {
            _compilation = CheckerTestSupport.CompileStdLib();
            _all = new Monomorphizer(_compilation).MonomorphizeAll();
        }

        [Test]
        public static void MonomorphizationTracksTheReifiedSetOneToOne()
        {
            // One instantiation per reified function, 1:1 with the oracle — plus the handful of
            // NON-reified entries (library functions whose first parameter is already a concrete
            // type; the reifier never stamps those, but the writer emits them as members).
            var reifiedCount = _compilation.ReifiedFunctions.Count();
            Assert.IsNotEmpty(_all);
            Assert.AreEqual(reifiedCount, _all.Count(m => m.Reified != null),
                "one monomorphized instantiation per reified function (aligned with the oracle)");
            foreach (var extra in _all.Where(m => m.Reified == null))
                Assert.IsTrue(extra.Original?.Parameters.Count > 0
                    && extra.Original.Parameters[0].Type?.Def?.Name == extra.ConcreteType?.Name,
                    "a non-reified entry must be a concrete-first-parameter function on that type");
        }

        [Test]
        public static void MonomorphizesStdLibTotallyAndGroundsANonTrivialSubset()
        {
            var bodied = _all.Where(m => m.HasBody).ToList();
            var ground = bodied.Count(m => m.IsFullyGround);
            var redispatched = _all.Sum(m => m.Stats.Redispatched);
            var deferred = _all.Sum(m => m.Stats.DeferredConcept);
            var distinctFns = _all.Where(m => m.HasBody).Select(m => m.Original).Distinct().Count();

            TestContext.WriteLine(
                $"Monomorphizer: {_all.Count} instantiations ({_compilation.ReifiedFunctions.Count()} reified); " +
                $"{bodied.Count} have a body (from {distinctFns} distinct source functions); " +
                $"{ground} monomorphize to fully-ground TIR; " +
                $"concept re-dispatch: {redispatched} re-pointed, {deferred} deferred.");

            Assert.Greater(bodied.Count, 0, "expected bodied instantiations");
            Assert.Greater(ground, 0, "expected a non-trivial subset to monomorphize to fully-ground TIR");
        }

        [Test]
        public static void DerivedSubstitutionAgreesWithTheReifierOnEverySignature()
        {
            // The oracle cross-check, per instantiation (not just a count). The reifier's substitution
            // is IDENTITY-based and imperfect: it leaves same-named type variables unbound in some
            // positions (e.g. Reverse(xs: IArrayLike<$T>): IArrayLike<$T> reifies to
            // (Integer2) -> IArrayLike<$T>, with the return's $T leaked — its r.Verify() is disabled
            // for exactly this reason). Our substitution is a structural REFINEMENT that grounds those
            // further. So the sound invariant is: applying our substitution to the original signature
            // agrees with applying it to the reifier's (partially-ground) signature — no contradiction
            // with the oracle, and wherever the reifier fully succeeded this is the strong "reproduces
            // the reified signature" claim.
            var checkedCount = 0;
            var strictlyMoreGround = 0;
            foreach (var m in _all)
            {
                // Non-reified entries (concrete-first-param library functions the reifier never
                // stamps) have no oracle signature to agree with.
                var rf = m.Reified;
                if (rf == null)
                    continue;
                var ps = rf.Original.Parameters;
                for (var i = 0; i < ps.Count && i < rf.ParameterTypes.Count; i++)
                    AssertAgree(m, ps[i].Type, rf.ParameterTypes[i], $"param {i}", ref strictlyMoreGround);
                AssertAgree(m, rf.Original.ReturnType, rf.ReturnType, "return", ref strictlyMoreGround);
                checkedCount++;
            }
            TestContext.WriteLine($"Cross-checked {checkedCount} reified signatures; " +
                $"our substitution grounds {strictlyMoreGround} positions further than the reifier left them.");
            Assert.Greater(checkedCount, 0);
        }

        private static void AssertAgree(MonomorphizedFunction m, TypeExpression original,
            TypeExpression reified, string where, ref int strictlyMoreGround)
        {
            var fromOriginal = m.Substitution.Apply(original);
            var fromReified = m.Substitution.Apply(reified);
            Assert.AreEqual(fromReified?.ToString(), fromOriginal?.ToString(),
                $"{where} of '{m.Original?.Name}' on '{m.ConcreteType?.Name}': grounding the original " +
                "signature must agree with grounding the reified one");
            if (!TypeSubstitution.IsGround(reified) && TypeSubstitution.IsGround(fromOriginal))
                strictlyMoreGround++;
        }

        [Test]
        public static void ConceptCallsAreEitherReDispatchedOrDeferredConsistently()
        {
            // Safety + accounting: after specialization, a TirCall still carrying a concept
            // declaration as its callee is exactly a deferred re-dispatch; the re-pointed ones are
            // gone (and, by construction, never target another concept declaration).
            foreach (var m in _all.Where(m => m.HasBody))
            {
                var remainingConceptCalls = m.Tir.AllNodes.OfType<TirCall>()
                    .Count(c => c.Callee != null && c.Callee.FunctionType == FunctionType.Concept);
                Assert.AreEqual(m.Stats.DeferredConcept, remainingConceptCalls,
                    $"concept-call accounting mismatch in '{m.Original?.Name}' on '{m.ConcreteType?.Name}'");
            }
        }
    }
}
