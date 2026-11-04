using NUnit.Framework;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CppWriter;
using Ara3D.Geometry.CppWriter.Policies;

namespace PlatoCppWriterTests;

/// <summary>The default policy set spells exactly what the writer emitted before the decisions were separated.</summary>
[TestFixture]
public class PoliciesTests
{
    private static SumCaseDef Case(string name, int tag, params string[] fields)
        => new(name, tag, fields.Select(f => new SumCaseField(name, f, null!)).ToList());

    [Test]
    public void Parameters_assigned_stay_owned_and_records_borrow_only_when_asked()
    {
        var off = CppPolicies.For(CppCpuOptions.Default).Parameters;
        var on = CppPolicies.For(CppCpuOptions.Default with { BorrowLargeParameters = true }).Parameters;
        Assert.That(off.Classify("Big", assigned: false, isRecord: true), Is.EqualTo(ParameterPassing.Standard));
        Assert.That(on.Classify("Big", assigned: false, isRecord: true), Is.EqualTo(ParameterPassing.ReadOnly));
        Assert.That(on.Classify("Big", assigned: false, isRecord: false), Is.EqualTo(ParameterPassing.Standard));
        Assert.That(on.Classify("Big", assigned: true, isRecord: true), Is.EqualTo(ParameterPassing.Owned));
    }

    [Test]
    public void Reads_copy_by_default_and_bind_const_references_when_asked()
    {
        var copy = CppPolicies.For(CppCpuOptions.Default).Reads;
        var borrow = CppPolicies.For(CppCpuOptions.Default with { BorrowReads = true }).Reads;
        Assert.That(copy.BorrowsReads, Is.False);
        Assert.That(copy.ElementBinding(true, false), Is.EqualTo("auto"));
        Assert.That(copy.LocalType("String", true), Is.EqualTo("String"));
        Assert.That(borrow.BorrowsReads, Is.True);
        Assert.That(borrow.ElementBinding(true, false), Is.EqualTo("const auto&"));
        Assert.That(borrow.ElementBinding(true, true), Is.EqualTo("auto"));
        Assert.That(borrow.ElementBinding(false, false), Is.EqualTo("auto"));
        Assert.That(borrow.ReductionElementBinding, Is.EqualTo("const auto&"));
        Assert.That(borrow.LocalType("String", true), Is.EqualTo("const String&"));
        Assert.That(borrow.LocalType("String", false), Is.EqualTo("String"));
    }

    [Test]
    public void Lambdas_capture_function_values_by_value_and_array_builders_by_reference()
    {
        var lambdas = CppPolicies.For(CppCpuOptions.Default).Lambdas;
        Assert.That(lambdas.Capture(LambdaSite.FunctionValue), Is.EqualTo("[=]"));
        Assert.That(lambdas.Capture(LambdaSite.ArrayLiteral), Is.EqualTo("[&]"));
    }

    [Test]
    public void Strings_are_built_by_make_string_n_with_escaping()
    {
        var strings = CppPolicies.For(CppCpuOptions.Default).Strings;
        Assert.That(strings.Literal("", 0), Is.EqualTo("make_string_n(\"\", 0)"));
        Assert.That(strings.Literal("a\"b", 3), Is.EqualTo("make_string_n(\"a\\\"b\", 3)"));
    }

    [Test]
    public void Units_split_between_definitions_at_the_line_limit_and_an_empty_program_has_one_unit()
    {
        var units = new LineLimitPartitioner { MaxFunctionLines = 5, MaxJsonLines = 100 };
        var definitions = new[] { "a\nb", "c\nd", "e" };
        var split = units.Partition(definitions, "<", ">", UnitKind.Functions).ToList();
        Assert.That(split, Is.EqualTo(new[] { "<a\nb\n\n>", "<c\nd\n\ne\n\n>" }));
        Assert.That(units.Partition(definitions, "<", ">", UnitKind.Json).Count(), Is.EqualTo(1));
        Assert.That(units.Partition(Array.Empty<string>(), "<", ">", UnitKind.Functions).Single(), Is.EqualTo("<>"));
        var oversized = units.Partition(new[] { "1\n2\n3\n4\n5\n6", "x" }, "<", ">", UnitKind.Functions).ToList();
        Assert.That(oversized, Has.Count.EqualTo(2));
    }

    [Test]
    public void Functions_have_no_attributes()
    {
        Assert.That(CppPolicies.For(CppCpuOptions.Default).Attributes.For("F", new[] { "double" }, "double"), Is.Empty);
    }

    [Test]
    public void Sum_layout_flattens_cases_after_an_int32_Kind()
    {
        var layout = CppPolicies.For(CppCpuOptions.Default).Sums;
        var cases = new[] { Case("Empty", 0), Case("Circle", 1, "Radius"), Case("Box", 2, "W", "H") };
        Assert.That(layout.Fields(cases).Select(f => f.Name), Is.EqualTo(new[] { "Kind", "Circle_Radius", "Box_W", "Box_H" }));
        Assert.That(layout.Fields(cases)[0].CppOverride, Is.EqualTo("int32_t"));
        Assert.That(layout.TagDeclarations(cases), Is.EqualTo(
            "    static constexpr int32_t Kind_Empty = 0;\n    static constexpr int32_t Kind_Circle = 1;\n    static constexpr int32_t Kind_Box = 2;\n"));
        Assert.That(layout.Construct("Shape", cases[1], new[] { "p0" }), Is.EqualTo(
            " { Shape r{}; r.Kind = Shape::Kind_Circle; r.Circle_Radius = p0; return r; }"));
        Assert.That(layout.PredicateBody("Shape", cases[1]), Is.EqualTo(" { return s.Kind == Shape::Kind_Circle; }"));
        Assert.That(layout.EqualsCondition("Shape", cases), Is.EqualTo(
            "a.Kind == b.Kind && (a.Kind != Shape::Kind_Circle || (eq_(a.Circle_Radius, b.Circle_Radius)))"
            + " && (a.Kind != Shape::Kind_Box || (eq_(a.Box_W, b.Box_W) && eq_(a.Box_H, b.Box_H)))"));
        Assert.That(layout.HashBody("Shape", cases, f => $"hash_(a.{f.Name})"), Is.EqualTo(
            " { int32_t h = hash_(a.Kind);"
            + " if (a.Kind == Shape::Kind_Circle) { h = mix_(h, hash_(a.Circle_Radius)); }"
            + " if (a.Kind == Shape::Kind_Box) { h = mix_(h, hash_(a.Box_W)); h = mix_(h, hash_(a.Box_H)); }"
            + " return h; }"));
    }

    [Test]
    public void Every_lowered_loop_kind_the_lowerer_produces_has_an_emitter_and_unknown_kinds_have_none()
    {
        var loops = CppPolicies.For(CppCpuOptions.Default).Loops;
        foreach (var kind in new[] { "Map", "MapIdx", "MapRange", "Zip2", "Zip3", "Reduce", "All", "Any", "FlatMap",
                     "Reverse", "WithNext", "MapPairs", "MapTriplets", "MapQuartets" })
            Assert.That(loops.TryGet(kind, out _), Is.True, kind);
        // The --lower-more kinds (C# only since timing window 2) and Sort (--closure-free) have no emitter.
        foreach (var kind in new[] { "Filter", "Make2D", "Make3D", "Cartesian", "Sort" })
            Assert.That(loops.TryGet(kind, out _), Is.False, kind);
    }

    [Test]
    public void A_policy_set_in_the_options_replaces_the_default()
    {
        var custom = CppPolicies.For(CppCpuOptions.Default);
        var options = CppCpuOptions.Default with { Policies = custom };
        Assert.That(options.Policies, Is.SameAs(custom));
        Assert.That(CppCpuOptions.Default.Policies, Is.Null);
    }
}
