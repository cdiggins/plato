using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Unit tests for <see cref="TirScalarLowerer"/> (scalar-lowering plan, Mission 2 S1): the pure
    /// type-substitution core. Proves the five wrapper types map to their primitives everywhere,
    /// non-scalar types are preserved, and lowering a real ground TIR leaves NO wrapper type in any
    /// node's type tree. The pass is not yet wired into emission, so these are the only gate on it.
    /// </summary>
    [TestFixture]
    public static class TirScalarLowererTests
    {
        private static readonly HashSet<string> WrapperNames =
            new HashSet<string> { "Number", "Integer", "Boolean", "Character", "String" };

        private static TypeExpression Named(string name, params TypeExpression[] args)
            => new TypeExpression(new TypeDef(null, TypeKind.Primitive, name), args);

        [Test]
        public static void MapsEachWrapperToItsPrimitive()
        {
            Assert.AreEqual("float", TirScalarLowerer.LowerType(Named("Number"), TirScalarLowerer.FloatMap).Def.Name);
            Assert.AreEqual("int", TirScalarLowerer.LowerType(Named("Integer"), TirScalarLowerer.FloatMap).Def.Name);
            Assert.AreEqual("bool", TirScalarLowerer.LowerType(Named("Boolean"), TirScalarLowerer.FloatMap).Def.Name);
            Assert.AreEqual("char", TirScalarLowerer.LowerType(Named("Character"), TirScalarLowerer.FloatMap).Def.Name);
            Assert.AreEqual("string", TirScalarLowerer.LowerType(Named("String"), TirScalarLowerer.FloatMap).Def.Name);
        }

        [Test]
        public static void RecursesIntoTypeArguments()
        {
            var t = Named("IArray", Named("Number"));
            var lowered = TirScalarLowerer.LowerType(t, TirScalarLowerer.FloatMap);
            Assert.AreEqual("IArray", lowered.Def.Name);
            Assert.AreEqual("float", lowered.TypeArgs[0].Def.Name);

            var nested = Named("IArray", Named("IArray", Named("Integer")));
            var loweredNested = TirScalarLowerer.LowerType(nested, TirScalarLowerer.FloatMap);
            Assert.AreEqual("int", loweredNested.TypeArgs[0].TypeArgs[0].Def.Name);
        }

        [Test]
        public static void PreservesNonScalarTypesByReference()
        {
            var v3 = Named("Vector3");
            Assert.AreSame(v3, TirScalarLowerer.LowerType(v3, TirScalarLowerer.FloatMap),
                "a type with no scalar anywhere must be returned unchanged (no needless allocation)");

            // A non-scalar generic over a non-scalar is also untouched...
            var arrV = Named("IArray", Named("Vector3"));
            Assert.AreSame(arrV, TirScalarLowerer.LowerType(arrV, TirScalarLowerer.FloatMap));

            // ...but a non-scalar generic over a scalar is rebuilt with the outer def preserved.
            var arrN = Named("IArray", Named("Number"));
            var lowered = TirScalarLowerer.LowerType(arrN, TirScalarLowerer.FloatMap);
            Assert.AreEqual("IArray", lowered.Def.Name);
            Assert.AreEqual("float", lowered.TypeArgs[0].Def.Name);
        }

        [Test]
        public static void NullTypeIsPreserved()
            => Assert.IsNull(TirScalarLowerer.LowerType(null, TirScalarLowerer.FloatMap));

        [Test]
        public static void LoweringAGroundTirLeavesNoWrapperTypeAnywhere()
        {
            var w = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-scalar-lowerer");

            // Scalar-heavy flagship bodies; skip any the test build cannot ground.
            var candidates = new[]
            {
                ("Vector3", "Dot"), ("Vector3", "Add"), ("Number", "Half"),
                ("Vector2", "Cross"), ("Vector3", "MagnitudeSquared"),
            };

            var checkedAny = false;
            foreach (var (type, fn) in candidates)
            {
                var tir = w.TestGetGroundTir(type, fn);
                if (tir?.Body == null)
                    continue;
                checkedAny = true;

                var lowered = TirScalarLowerer.Lower(tir);

                var offenders = lowered.AllNodes
                    .SelectMany(TypesOf)
                    .SelectMany(TypeNames)
                    .Where(WrapperNames.Contains)
                    .Distinct()
                    .ToList();
                Assert.IsEmpty(offenders,
                    $"{type}.{fn}: wrapper types survived lowering: {string.Join(", ", offenders)}");

                // And the signature is lowered too.
                Assert.IsFalse(TypeNames(lowered.ReturnType).Any(WrapperNames.Contains),
                    $"{type}.{fn}: return type not lowered");
            }

            Assert.IsTrue(checkedAny, "no candidate ground TIR was available to lower");
        }

        [Test]
        public static void LowerWithCoercionsInsertsScalarCastsAndErasesTypes()
        {
            var w = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-coerce");
            var candidates = new[]
            {
                ("Vector3", "Dot"), ("Number", "Half"), ("Vector2", "Cross"), ("Vector3", "Add"),
            };

            var sawCoercions = false;
            var checkedAny = false;
            foreach (var (type, fn) in candidates)
            {
                var tir = w.TestGetGroundTir(type, fn);
                if (tir?.Body == null)
                    continue;
                checkedAny = true;

                var lowered = TirScalarLowerer.LowerWithCoercions(tir);

                // Still fully erased (no wrapper type anywhere).
                var offenders = lowered.AllNodes.SelectMany(TypesOf).SelectMany(TypeNames)
                    .Where(WrapperNames.Contains).Distinct().ToList();
                Assert.IsEmpty(offenders, $"{type}.{fn}: wrapper types survived: {string.Join(", ", offenders)}");

                // Any inserted disambiguation coercion targets an erased PRIMITIVE (float/int/…),
                // never a wrapper — the printer renders these as ((float)…).
                foreach (var co in lowered.AllNodes.OfType<TirCoerce>())
                {
                    var to = co.ToType?.Def?.Name;
                    if (to != null && (CSharpWriter.ScalarPrimitives.ContainsValue(to) || CSharpWriter.ScalarPrimitives.ContainsKey(to)))
                        sawCoercions = true;
                }
            }

            Assert.IsTrue(checkedAny, "no candidate ground TIR was available");
            Assert.IsTrue(sawCoercions, "expected at least one scalar disambiguation coercion across the scalar-heavy candidates");
        }

        private static IEnumerable<TypeExpression> TypesOf(TirNode n)
        {
            if (n.Type != null) yield return n.Type;
            if (n is TirCall c)
            {
                if (c.ReturnType != null) yield return c.ReturnType;
                foreach (var p in c.ParameterTypes ?? Enumerable.Empty<TypeExpression>())
                    if (p != null) yield return p;
            }
            if (n is TirCoerce co)
            {
                if (co.FromType != null) yield return co.FromType;
                if (co.ToType != null) yield return co.ToType;
            }
            if (n is TirNew nw && nw.NewType != null) yield return nw.NewType;
        }

        private static IEnumerable<string> TypeNames(TypeExpression t)
        {
            if (t?.Def == null) yield break;
            yield return t.Def.Name;
            foreach (var a in t.TypeArgs)
                foreach (var n in TypeNames(a))
                    yield return n;
        }
    }
}
