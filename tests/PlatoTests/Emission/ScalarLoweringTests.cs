// Checks body-local scalar lowering (--lower-scalars): the emitted shape, and that both spellings compute the same bits.
using System.Globalization;
using System.Reflection;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;

namespace PlatoTests;

/// <summary>
/// Pins <see cref="CSharpWriter.LowerScalars"/> (plato-brep TKT-1129, section 7a of
/// docs/csharp-optimizer-pipeline.md). The fixture under <c>ScalarLoweringFixtures</c> is a
/// self-contained program over Number, Integer and Boolean. Group A reads the emitted
/// <c>ScalarProbe.g.cs</c>: with the flag on a scalar intrinsic is a C# operator over the wrapped
/// primitives, a scalar let is a primitive local, a condition is a primitive bool, and a result is
/// wrapped once (<c>Unsafe.BitCast&lt;primitive, Wrapper&gt;</c>, the call-free wrap); with the flag off the output is the old shape and is deterministic. Group B compiles
/// each variant with Roslyn, the way <c>Plato.Laws/CSharpProgram</c> does (every emitted file plus
/// the handwritten runtime of <c>src/Plato.Intrinsics</c>, so the flag-off calls resolve through the
/// generated partial struct exactly as they ship), and runs every probe over the awkward inputs:
/// NaN, both zeros, both infinities, 1e308, halves, the int limits, 7 and -7 against 2 and -2.
/// Numbers are compared by <see cref="BitConverter.DoubleToInt64Bits"/>, so a NaN payload and the
/// sign of zero count, and each result is also checked against the value C# computes directly.
/// </summary>
[TestFixture]
public static class ScalarLoweringTests
{
    private const string Library = "ScalarProbe.g.cs";

    private static readonly Lazy<PlatoCompilation> Compilation = new(() =>
    {
        var folder = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Emission", "ScalarLoweringFixtures");
        var compilation = CheckerTestSupport.CompileFolder(folder);
        Assert.That(compilation.SymbolFactory.Errors, Is.Empty,
            string.Join("\n", compilation.SymbolFactory.Errors.Select(e => e.Message)));
        return compilation;
    });

    private static CSharpWriter Emit(bool lowerScalars)
    {
        var writer = new CSharpWriter(Compilation.Value, "unused-scalar-lowering") { ExtensionStyle = true, LowerScalars = lowerScalars };
        writer.WriteAll();
        return writer;
    }

    private static readonly Lazy<string> Lowered = new(() => Emit(true).Files[Library].ToString());
    private static readonly Lazy<string> Unlowered = new(() => Emit(false).Files[Library].ToString());

    /// <summary>The one emitted line that declares the named probe.</summary>
    private static string Line(string text, string probe)
        => text.Split('\n').Single(l => l.Contains($" {probe}(this ")).Trim();

    // --- A. shape -------------------------------------------------------------------------------

    [Test]
    public static void AProductIsAnOperatorOverTheValuesBehindTheWrapperSignature()
    {
        var scale = Line(Lowered.Value, "Scale");
        Assert.That(scale, Does.Contain("Unsafe.BitCast<NumberValue, Number>((a.Value * b.Value))"));
        Assert.That(scale, Does.Contain("public static Number Scale(this Number a, Number b)"), "the signature keeps the wrapper");
        Assert.That(Lowered.Value, Does.Not.Contain(".Multiply("));
    }

    [Test]
    public static void ALetIsAPrimitiveLocalAndTheReturnWrapsOnce()
    {
        Assert.That(Lowered.Value, Does.Contain("NumberValue s = (a.Value + b.Value);"));
        Assert.That(Lowered.Value, Does.Contain("return Unsafe.BitCast<NumberValue, Number>((s * 0.5D));"));
        Assert.That(Lowered.Value, Does.Not.Contain(".Add("));
    }

    [Test]
    public static void AConditionIsThePrimitiveBool()
    {
        var pick = Line(Lowered.Value, "Pick");
        Assert.That(pick, Does.Contain("(a.Value <= b.Value) ? a : b"));
        Assert.That(pick, Does.Not.Contain("((Boolean)"));
        Assert.That(pick, Does.Not.Contain(".LessThanOrEquals("));
    }

    [Test]
    public static void WithTheFlagOffTheOutputIsTheOldShapeAndDeterministic()
    {
        var first = Emit(false);
        var second = Emit(false);
        Assert.That(second.Files.Keys, Is.EquivalentTo(first.Files.Keys));
        foreach (var name in first.Files.Keys)
            Assert.That(second.Files[name].ToString(), Is.EqualTo(first.Files[name].ToString()), name);

        var text = first.Files[Library].ToString();
        Assert.That(text, Does.Contain(".Multiply("));
        Assert.That(Line(text, "Scale"), Does.Contain("=> a.Multiply(b);"));
        Assert.That(Line(text, "Pick"), Does.Contain("=> a.LessThanOrEquals(b) ? a : b;"));
        Assert.That(text, Does.Not.Contain(".Value"), "no primitive reads without the flag");
        Assert.That(text, Does.Not.Contain("Unsafe.BitCast<"));
    }

    [Test]
    public static void AnAllScalarHelperIsInlinedBeforeItIsLowered()
    {
        // AtLeast(a, b) => a.GreaterThanOrEquals(b), and the fixture's GreaterThanOrEquals(a, b) => b <= a.
        var atLeast = Line(Lowered.Value, "AtLeast");
        Assert.That(atLeast, Does.Contain("Unsafe.BitCast<bool, Boolean>((b.Value <= a.Value))"));
        Assert.That(atLeast, Does.Not.Contain("GreaterThanOrEquals("));
        Assert.That(atLeast, Does.Not.Contain(">="));
        Assert.That(Line(Unlowered.Value, "AtLeast"), Does.Contain("=> a.GreaterThanOrEquals(b);"), "without the flag the helper stays a call");
    }

    // --- B. semantics ---------------------------------------------------------------------------

    /// <summary>One compiled variant of the fixture: its own Number, Integer and Boolean structs
    /// (the handwritten runtime plus the generated partial), reached through reflection.</summary>
    private sealed class Variant
    {
        private readonly Type _library;
        private readonly Type _number, _integer, _boolean;
        private readonly FieldInfo _numberValue, _integerValue, _booleanValue;

        public Variant(Assembly assembly)
        {
            _library = assembly.GetType("Ara3D.Geometry.ScalarProbe", true)!;
            _number = assembly.GetType("Ara3D.Geometry.Number", true)!;
            _integer = assembly.GetType("Ara3D.Geometry.Integer", true)!;
            _boolean = assembly.GetType("Ara3D.Geometry.Boolean", true)!;
            _numberValue = _number.GetField("Value")!;
            _integerValue = _integer.GetField("Value")!;
            _booleanValue = _boolean.GetField("Value")!;
        }

        private object Wrap(object primitive) => primitive switch
        {
            // Explicit argument arrays: a lone bool would bind to CreateInstance(Type, bool nonPublic).
            double d => Activator.CreateInstance(_number, new object[] { d })!,
            long i => Activator.CreateInstance(_integer, new[] { Convert.ChangeType(i, _integerValue.FieldType) })!,
            bool b => Activator.CreateInstance(_boolean, new object[] { b })!,
            _ => throw new ArgumentException(primitive.GetType().Name),
        };

        /// <summary>Calls the probe with the primitives wrapped, and returns the result as a key.</summary>
        public string Call(string probe, object[] primitives)
        {
            var method = _library.GetMethod(probe) ?? throw new MissingMethodException(probe);
            var result = method.Invoke(null, primitives.Select(Wrap).ToArray())!;
            var type = result.GetType();
            if (type == _number) return Key(_numberValue.GetValue(result)!);
            if (type == _integer) return Key(Convert.ToInt64(_integerValue.GetValue(result)));
            if (type == _boolean) return Key(_booleanValue.GetValue(result)!);
            throw new InvalidOperationException($"{probe} returned a {type.Name}");
        }
    }

    /// <summary>A primitive as an exact, readable key: a Number by its bits, so NaN payloads and
    /// the sign of zero compare; an Integer by its value; a Boolean by its value.</summary>
    private static string Key(object primitive) => primitive switch
    {
        double d => $"Number {d.ToString("R", CultureInfo.InvariantCulture)} [0x{BitConverter.DoubleToInt64Bits(d):X16}]",
        long i => $"Integer {i}",
        int i => Key((long)i),
        bool b => $"Boolean {b}",
        _ => throw new ArgumentException(primitive.GetType().Name),
    };

    // What ImplicitUsings gives the runtime sources, and the namespace their Ara3D.Collections using names.
    private const string Prelude = "global using System; global using System.Collections.Generic; global using System.IO; "
        + "global using System.Linq; namespace Ara3D.Collections { }";

    private static Variant Build(bool lowerScalars)
    {
        var writer = Emit(lowerScalars);
        var runtimeFolder = Path.GetFullPath(Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "..", "..", "src", "Plato.Intrinsics"));
        var runtime = Directory.GetFiles(runtimeFolder, "*.cs").OrderBy(p => p, StringComparer.Ordinal).Select(File.ReadAllText);
        var generated = writer.Files.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value.ToString());
        return new Variant(CSharpTestCompilation.CompileAndLoad(generated.Concat(runtime).Prepend(Prelude).ToArray()));
    }

    private static readonly Lazy<Variant> On = new(() => Build(true));
    private static readonly Lazy<Variant> Off = new(() => Build(false));

    /// <summary>Runs one probe in both variants: they must agree exactly, and match
    /// <paramref name="expected"/> (a primitive) when one is given.</summary>
    private static void Check(string probe, object[] args, object? expected)
    {
        var label = $"{probe}({string.Join(", ", args.Select(Key))})";
        var lowered = On.Value.Call(probe, args);
        var wrapped = Off.Value.Call(probe, args);
        Assert.That(lowered, Is.EqualTo(wrapped), $"{label}: the two spellings disagree");
        if (expected != null)
            Assert.That(lowered, Is.EqualTo(Key(expected)), $"{label}: not what C# computes directly");
    }

    private static readonly double[] Numbers =
    {
        double.NaN, 0.0, -0.0, double.PositiveInfinity, double.NegativeInfinity, 1e308, -2.5, 2.5, 1.0, -7.0, 2.0,
    };

    // The fixture runs at the writer's default profile (cpu64, a 64-bit Integer), so none of these
    // products or negations overflow.
    private static readonly long[] Integers = { int.MaxValue, int.MinValue, 7, -7, 2, -2, 1, 0 };

    private static readonly Dictionary<string, Func<double, double, object>> NumberBinary = new()
    {
        ["NumAdd"] = (a, b) => a + b,
        ["NumSubtract"] = (a, b) => a - b,
        ["NumMultiply"] = (a, b) => a * b,
        ["NumDivide"] = (a, b) => a / b,
        ["NumModulo"] = (a, b) => a % b,
        ["NumLessThanOrEquals"] = (a, b) => a <= b,
        ["NumEquals"] = (a, b) => a.Equals(b),
        ["NumNotEquals"] = (a, b) => !a.Equals(b),
        ["Scale"] = (a, b) => a * b,
        ["Mid"] = (a, b) => (a + b) * 0.5,
        ["NumLet"] = (a, b) => (a + b) * (a - b),
    };

    [TestCase("NumAdd"), TestCase("NumSubtract"), TestCase("NumMultiply"), TestCase("NumDivide"), TestCase("NumModulo")]
    [TestCase("NumLessThanOrEquals"), TestCase("NumEquals"), TestCase("NumNotEquals")]
    [TestCase("Scale"), TestCase("Mid"), TestCase("NumLet")]
    public static void NumberBinaryProbesAgree(string probe)
    {
        foreach (var a in Numbers)
            foreach (var b in Numbers)
                Check(probe, new object[] { a, b }, NumberBinary[probe](a, b));
    }

    private static readonly Dictionary<string, Func<double, object>> NumberUnary = new()
    {
        ["NumNegative"] = a => -a,
        ["NumFloor"] = a => Math.Floor(a),
        ["NumSqrt"] = a => Math.Sqrt(a),
    };

    [TestCase("NumNegative"), TestCase("NumFloor"), TestCase("NumSqrt")]
    public static void NumberUnaryProbesAgree(string probe)
    {
        foreach (var a in Numbers)
            Check(probe, new object[] { a }, NumberUnary[probe](a));
    }

    [Test]
    public static void ToIntegerTruncatesTowardZero()
    {
        // The truncating cast, where C# defines it; the two spellings must agree everywhere else too
        // (NaN, the infinities and 1e308 are the same conv.i8 in both).
        foreach (var a in new[] { -2.5, 2.5, -0.0, 0.0, 1.0, -7.0, 2.0, 1e15, -1e15 })
            Check("NumToInteger", new object[] { a }, (long)a);
        foreach (var a in Numbers)
            Check("NumToInteger", new object[] { a }, null);
    }

    private static readonly Dictionary<string, Func<long, long, object>> IntegerBinary = new()
    {
        ["IntAdd"] = (a, b) => a + b,
        ["IntSubtract"] = (a, b) => a - b,
        ["IntMultiply"] = (a, b) => a * b,
        ["IntDivide"] = (a, b) => a / b,
        ["IntModulo"] = (a, b) => a % b,
        ["IntEquals"] = (a, b) => a == b,
        ["IntLessThanOrEquals"] = (a, b) => a <= b,
        ["AtLeast"] = (a, b) => a >= b,
        ["Pick"] = (a, b) => Math.Min(a, b),
        ["IntNested"] = (a, b) => a <= b ? (a == b ? 0L : -1L) : 1L,
        ["IntLet"] = (a, b) => (a / b) * b + a % b,
    };

    private static bool Divides(string probe) => probe is "IntDivide" or "IntModulo" or "IntLet";

    [TestCase("IntAdd"), TestCase("IntSubtract"), TestCase("IntMultiply"), TestCase("IntDivide"), TestCase("IntModulo")]
    [TestCase("IntEquals"), TestCase("IntLessThanOrEquals"), TestCase("AtLeast"), TestCase("Pick"), TestCase("IntNested"), TestCase("IntLet")]
    public static void IntegerBinaryProbesAgree(string probe)
    {
        foreach (var a in Integers)
            foreach (var b in Integers)
            {
                if (b == 0 && Divides(probe))
                    continue;
                Check(probe, new object[] { a, b }, IntegerBinary[probe](a, b));
            }
    }

    [Test]
    public static void IntegerDivisionTruncatesTowardZero()
    {
        foreach (var a in new long[] { 7, -7 })
            foreach (var b in new long[] { 2, -2 })
            {
                Check("IntDivide", new object[] { a, b }, a / b);
                Check("IntModulo", new object[] { a, b }, a % b);
            }
        Check("IntDivide", new object[] { -7L, 2L }, -3L);
        Check("IntModulo", new object[] { -7L, 2L }, -1L);
        Check("IntModulo", new object[] { 7L, -2L }, 1L);
    }

    [Test]
    public static void IntegerUnaryProbesAgree()
    {
        foreach (var a in Integers)
        {
            Check("IntNegative", new object[] { a }, -a);
            Check("IntToNumber", new object[] { a }, (double)a);
        }
    }

    [Test]
    public static void BooleanProbesAgree()
    {
        foreach (var a in new[] { false, true })
        {
            Check("BoolNot", new object[] { a }, !a);
            foreach (var b in new[] { false, true })
            {
                Check("BoolAnd", new object[] { a, b }, a & b);
                Check("BoolOr", new object[] { a, b }, a | b);
                Check("BoolBoth", new object[] { a, b }, a && b);
                Check("BoolEither", new object[] { a, b }, a || b);
                foreach (var c in new[] { false, true })
                    Check("BoolPick", new object[] { c, a, b }, c ? a : b);
            }
        }
    }

    [Test]
    public static void NestedConditionalsAgree()
    {
        foreach (var a in Numbers)
            foreach (var b in Numbers)
                foreach (var c in Numbers)
                    Check("NumNested", new object[] { a, b, c }, a <= b ? (b <= c ? b : c) : (a <= c ? a : c));
    }

    [Test]
    public static void NaNsAreEqualAndZerosKeepTheirSignInBothSpellings()
    {
        // Number.Equals is double.Equals: two NaNs are equal and the two zeros are equal, while <=
        // is the IEEE comparison; negation flips the sign of zero.
        Check("NumEquals", new object[] { double.NaN, double.NaN }, true);
        Check("NumNotEquals", new object[] { double.NaN, double.NaN }, false);
        Check("NumLessThanOrEquals", new object[] { double.NaN, double.NaN }, false);
        Check("NumEquals", new object[] { 0.0, -0.0 }, true);
        Check("NumNegative", new object[] { 0.0 }, -0.0);
        Check("NumNegative", new object[] { -0.0 }, 0.0);
        Check("NumMultiply", new object[] { -2.5, 0.0 }, -0.0);
        Check("NumSqrt", new object[] { -0.0 }, -0.0);
        Check("NumFloor", new object[] { -0.0 }, -0.0);
        Check("NumFloor", new object[] { -2.5 }, -3.0);
        Check("NumToInteger", new object[] { -2.5 }, -2L);
    }
}
