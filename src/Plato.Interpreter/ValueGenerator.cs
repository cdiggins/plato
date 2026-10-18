using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Interpreter;

/// <summary>
/// Seeded pseudorandom values for a law's parameters, the same policy as the C# conformance
/// runner's ValueGen (tests/conformance/Plato.ForwardConformanceTests/ConformanceSupport.cs): a
/// Number is one of ten special values three times in ten and otherwise uniform in [-10, 10]; an
/// Integer is uniform in [-8, 8]; a Boolean a fair coin; a String up to five lowercase letters; a
/// product is built field by field to a depth limit. Where the C# runner stops (it cannot build an
/// array, a sum, a zero-field type), this one goes on: an array of up to four elements, a sum
/// case chosen at random, a zero-field type as its only value. Every draw comes from the one
/// Random the caller seeds, so a run is reproducible from its seed.
/// </summary>
public sealed class ValueGenerator
{
    public const int MaxDepth = 6;
    public const int MaxArrayLength = 4;

    private static readonly double[] SpecialNumbers = { 0, 1, -1, 0.5, -0.5, 2, -2, 0.25, 3, -3 };

    private readonly Interpreter _interp;

    public ValueGenerator(Interpreter interp) => _interp = interp;

    /// <summary>Whether values of this type can be generated at all; the reason when not.</summary>
    public bool CanGenerate(TypeExpression t, out string reason)
        => CanGenerate(t, 0, new HashSet<string>(), out reason);

    private bool CanGenerate(TypeExpression t, int depth, HashSet<string> inProgress, out string reason)
    {
        reason = null;
        if (t?.Def == null) { reason = "untyped parameter"; return false; }
        if (depth > MaxDepth) { reason = $"nesting deeper than {MaxDepth} in {t}"; return false; }
        switch (t.Def.Name)
        {
            case "Number": case "Integer": case "Boolean": case "Character": case "String":
                return true;
            case "Array":
                return t.TypeArgs.Count == 1 && CanGenerate(t.TypeArgs[0], depth + 1, inProgress, out reason);
            case "List": case "Buffer":
                reason = "an affine builder cannot be a law input"; return false;
        }
        if (t.IsTypeVariable || t.IsTypeParameter || t.Name == "Self")
        {
            reason = $"type variable {t}"; return false;
        }
        if (t.Def.IsInterface())
        {
            reason = $"interface {t.Name} (the runner enumerates its implementers)"; return false;
        }
        if (t.Def.Name.StartsWith("Function"))
        {
            reason = "a function-typed parameter"; return false;
        }
        if (!inProgress.Add(t.Def.Name))
        {
            reason = $"{t.Def.Name} contains itself"; return false;
        }
        try
        {
            if (t.Def.IsSum)
            {
                foreach (var c in t.Def.Cases)
                    foreach (var f in c.Fields)
                        if (!CanGenerate(f.Type, depth + 1, inProgress, out reason))
                            return false;
                return true;
            }
            foreach (var f in t.Def.Fields)
                if (!CanGenerate(f.Type, depth + 1, inProgress, out reason))
                    return false;
            return true;
        }
        finally
        {
            inProgress.Remove(t.Def.Name);
        }
    }

    public object Generate(TypeExpression t, Random rng, int depth = 0)
    {
        switch (t.Def.Name)
        {
            case "Number": return _interp.Numbers.N(NextNumber(rng));
            case "Integer": return (long)rng.Next(-8, 9);
            case "Boolean": return rng.Next(2) == 0;
            case "Character": return (char)('a' + rng.Next(26));
            case "String":
            {
                var len = rng.Next(0, 6);
                var cs = new char[len];
                for (var i = 0; i < len; i++) cs[i] = (char)('a' + rng.Next(26));
                return new string(cs);
            }
            case "Array":
            {
                var elem = t.TypeArgs[0];
                var n = rng.Next(0, MaxArrayLength + 1);
                var items = new object[n];
                for (var i = 0; i < n; i++) items[i] = Generate(elem, rng, depth + 1);
                return new PlatoArray(elem, items);
            }
        }
        if (t.Def.IsSum)
        {
            var c = t.Def.Cases[rng.Next(t.Def.Cases.Count)];
            return new PlatoSum(t, c, c.Fields.Select(f => Generate(f.Type, rng, depth + 1)).ToArray());
        }
        return new PlatoRecord(t, t.Def.Fields.Select(f => Generate(Substitute(f.Type, t), rng, depth + 1)).ToArray());
    }

    /// <summary>A field type of a generic type instance with the instance's arguments substituted
    /// (<c>Tuple2&lt;Number, Integer&gt;.X1</c> is an Integer).</summary>
    private static TypeExpression Substitute(TypeExpression fieldType, TypeExpression owner)
    {
        if (fieldType?.Def == null || owner.TypeArgs.Count == 0)
            return fieldType;
        var tps = owner.Def.TypeParameters;
        for (var i = 0; i < tps.Count && i < owner.TypeArgs.Count; i++)
            if (fieldType.Def == tps[i] || fieldType.Name == tps[i].Name)
                return owner.TypeArgs[i];
        return fieldType;
    }

    private static double NextNumber(Random rng)
        => rng.NextDouble() < 0.3
            ? SpecialNumbers[rng.Next(SpecialNumbers.Length)]
            : rng.NextDouble() * 20.0 - 10.0;

    /// <summary>The whole value set of a type when it is small and finite (Boolean, a sum of
    /// payload-free cases), so a law over it can be exhausted rather than sampled; null otherwise.</summary>
    public IReadOnlyList<object> FiniteDomain(TypeExpression t)
    {
        if (t?.Def == null)
            return null;
        if (t.Def.Name == "Boolean")
            return new object[] { false, true };
        // A `primitive` declaration (Number, Array, ...) has no fields but is not an empty record.
        if (t.Def.IsPrimitiveDeclaration)
            return null;
        if (t.Def.IsSum && t.Def.Cases.All(c => c.Fields.Count == 0))
            return t.Def.Cases.Select(c => (object)new PlatoSum(t, c, Array.Empty<object>())).ToList();
        if (t.Def.IsConcrete() && !t.Def.IsSum && t.Def.Fields.Count == 0)
            return new object[] { new PlatoRecord(t, Array.Empty<object>()) };
        return null;
    }

    /// <summary>FNV-1a over a string: a seed that does not depend on the runtime's randomized
    /// string hashing, so the same law gets the same values in every process.</summary>
    public static int StableSeed(string s)
    {
        unchecked
        {
            var h = 2166136261u;
            foreach (var c in s) { h ^= c; h *= 16777619u; }
            return (int)h;
        }
    }
}
