using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Polices the writer's picture of the handwritten Plato.Intrinsics surface (plato-331).
    ///
    /// The writer generates no struct for a <see cref="CSharpWriter.PrimitiveTypes"/> entry, so under
    /// it cannot see the runtime's shape and decides property-vs-method
    /// spelling from proxies: the Plato declaration, <see cref="CSharpConcreteTypeWriter.PrimitiveFieldNames"/>
    /// for components, and the four-entry <see cref="CSharpWriter.PrimitiveSurfaceOverrides"/> pin
    /// table. When any of those silently disagrees with the actual V2 surface the symptom is
    /// CS0030/CS1955 across a thousand generated files downstream. This fixture compiles the V2
    /// shared sources into the test assembly (see PlatoTests.csproj) and reflects over them, so the
    /// disagreement becomes one test failure at the source instead.
    ///
    /// Every public instance property on a V2 primitive struct must be one of:
    ///  - a component pseudo-field the writer reads unqualified in Components(),
    ///  - an override entry recording "V2 spells this as a property",
    ///  - a member stdlib-legacy declares as a FIELD, so the declaration proxy lands on property
    ///    syntax without an override (a documented coincidence, pinned in
    ///    <see cref="DeclarationProxyProperties"/>).
    /// </summary>
    [TestFixture]
    public static class IntrinsicsSurfaceTests
    {
        /// <summary>Properties needing no override only because stdlib-legacy declares the member as
        /// a field. If the V2 port (plato-331 option 1) converts one of these to a method, the
        /// legacy declaration proxy is then wrong in the OTHER direction: remove it here AND give it
        /// an override entry (or change the primitive rule) in the same commit.</summary>
        /// (Emptied by plato-365: the Row* properties it listed are GENERATED fields now, so there
        /// is no handwritten property left for the declaration proxy to accidentally describe.)
        private static readonly HashSet<string> DeclarationProxyProperties = new HashSet<string>();

        /// <summary>
        /// The handwritten V2 runtime structs. Deliberately NOT scoped by
        /// <see cref="CSharpWriter.PrimitiveTypes"/>: plato-365 is emptying that dictionary down to
        /// the scalars, and scoping this fixture by it would have silently shrunk the surface being
        /// policed exactly as each type stopped being a primitive — the opposite of what these
        /// tests are for. The names are pinned, so a type appearing or vanishing is a failure.
        /// (2026-08-01: the SIMD-backed family — Angle, Plane, Quaternion, the matrices,
        /// Vector2/3/4/8 — left the runtime for bonepile/; the generated library owns those shapes
        /// now, so the pinned surface is the five scalars.)
        /// </summary>
        internal static readonly IReadOnlyList<string> V2StructNames = new[]
        {
            "Boolean", "Character", "Integer", "Number", "String",
        };

        internal static IReadOnlyList<Type> V2PrimitiveStructs()
            => typeof(Ara3D.Geometry.Number).Assembly
                .GetTypes()
                .Where(t => t.Namespace == "Ara3D.Geometry"
                            && t.IsValueType
                            && V2StructNames.Contains(t.Name))
                .OrderBy(t => t.Name)
                .ToList();

        private static IEnumerable<PropertyInfo> PublicInstanceProperties(Type t)
            => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => p.GetIndexParameters().Length == 0);

        [Test]
        public static void SharedProjectImportIsIntact()
            => Assert.AreEqual(
                V2StructNames.OrderBy(n => n).ToArray(),
                V2PrimitiveStructs().Select(t => t.Name).ToArray(),
                "The V2 primitive struct set changed. If a type was added or removed on purpose, " +
                "update this list; if not, the Plato.Intrinsics.projitems import in " +
                "PlatoTests.csproj is broken.");

        [Test]
        public static void EveryV2PublicPropertyIsAccountedForByTheWriter()
        {
            var unaccounted = new List<string>();
            foreach (var type in V2PrimitiveStructs())
            {
                var components = CSharpConcreteTypeWriter.PrimitiveFieldNames.TryGetValue(type.Name, out var names)
                    ? new HashSet<string>(names)
                    : new HashSet<string>();
                foreach (var prop in PublicInstanceProperties(type))
                {
                    var key = $"{type.Name}.{prop.Name}";
                    var isOverriddenProperty = CSharpWriter.PrimitiveSurfaceOverride(type.Name, prop.Name) == true;
                    if (!components.Contains(prop.Name) && !isOverriddenProperty && !DeclarationProxyProperties.Contains(key))
                        unaccounted.Add(key);
                }
            }

            Assert.IsEmpty(unaccounted,
                "The runtime has public properties the writer does not know about; " +
                "generated call sites will render them with the wrong spelling (CS0030/CS1955 far " +
                "downstream). Either convert them to methods (V2 is a method-form runtime; " +
                "plato-331 option 1) or record them in CSharpWriter.PrimitiveSurfaceOverrides. " +
                "Unaccounted: " + string.Join(", ", unaccounted));
        }

        [Test]
        public static void OverrideTableMatchesTheV2Runtime()
        {
            var structsByName = V2PrimitiveStructs().ToDictionary(t => t.Name);
            foreach (var entry in CSharpWriter.PrimitiveSurfaceOverrides)
            {
                var parts = entry.Key.Split('.');
                Assert.AreEqual(2, parts.Length, $"Malformed override key '{entry.Key}' (want 'Type.Member').");
                Assert.IsTrue(structsByName.TryGetValue(parts[0], out var type),
                    $"Override '{entry.Key}' names a type that is not a V2 primitive struct.");

                var property = PublicInstanceProperties(type).FirstOrDefault(p => p.Name == parts[1]);
                if (entry.Value)
                    Assert.IsNotNull(property,
                        $"Override says V2 spells '{entry.Key}' as a PROPERTY, but the runtime has " +
                        "no such public property. If the V2 port converted it to a method, delete " +
                        "the override entry.");
                else
                {
                    Assert.IsNull(property,
                        $"Override says V2 spells '{entry.Key}' as a METHOD, but the runtime has a " +
                        "public property of that name. Delete the override entry or finish the port.");
                    Assert.IsTrue(type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .Any(m => m.Name == parts[1]),
                        $"Override says V2 spells '{entry.Key}' as a METHOD, but the runtime has " +
                        "no such public method at all (that is a missing member, plato-330 territory).");
                }
            }
        }

        [Test]
        public static void DeclarationProxyCoincidencesStillHold()
        {
            var structsByName = V2PrimitiveStructs().ToDictionary(t => t.Name);
            foreach (var key in DeclarationProxyProperties.OrderBy(k => k))
            {
                var parts = key.Split('.');
                Assert.IsTrue(structsByName.TryGetValue(parts[0], out var type),
                    $"Pinned '{key}' names a type that is not a V2 primitive struct.");
                Assert.IsTrue(PublicInstanceProperties(type).Any(p => p.Name == parts[1]),
                    $"'{key}' is pinned as a property that stdlib-legacy declares as a field, but " +
                    "V2 no longer has that public property. The legacy declaration proxy now " +
                    "renders property syntax onto a non-property: remove the pin here and add a " +
                    "PrimitiveSurfaceOverrides entry (value false) in the same commit.");
            }
        }
    }
}
