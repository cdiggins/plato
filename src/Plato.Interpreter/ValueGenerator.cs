using System.Collections.Concurrent;
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
/// A declared type the library gives a generator (<see cref="GeneratorsOf"/>) is never built
/// field by field: its values, as a law argument or as a field of another value, come from a
/// generator called with a seed in [0, <see cref="GeneratorSeeds"/>), so a type whose
/// constructors keep an invariant (a positive tolerance) only receives values that hold it.
/// A sealed type (<c>sealed by F, G</c>, TKT-444) without a generator is drawn from the functions
/// its declaration names instead, each called on drawn arguments: the runner builds only values
/// the library could build.
/// </summary>
public sealed class ValueGenerator
{
    public const int MaxDepth = 6;
    public const int MaxArrayLength = 4;

    /// <summary>A generator is a bodied function named with this prefix (TKT-446).</summary>
    public const string GeneratorPrefix = "Generate_";
    /// <summary>The seeds a generator is called with: 0 to this count less one.</summary>
    public const int GeneratorSeeds = 16;

    private static readonly double[] SpecialNumbers = { 0, 1, -1, 0.5, -0.5, 2, -2, 0.25, 3, -3 };

    private readonly Interpreter _interp;
    private readonly ConcurrentDictionary<string, IReadOnlyList<FunctionDef>> _generators = new ConcurrentDictionary<string, IReadOnlyList<FunctionDef>>();
    private readonly ConcurrentDictionary<TypeDef, IReadOnlyList<FunctionDef>> _sealedConstructors = new ConcurrentDictionary<TypeDef, IReadOnlyList<FunctionDef>>();
    /// <summary>Each generator's value per seed, evaluated once; guarded by a lock on the interpreter.</summary>
    private readonly Dictionary<(FunctionDef, int), object> _generated = new Dictionary<(FunctionDef, int), object>();

    public ValueGenerator(Interpreter interp) => _interp = interp;

    /// <summary>The generators of a declared type: every bodied <c>Generate_X(seed: Integer): T</c>,
    /// or <c>Generate_X(_: T, seed: Integer): T</c> in the static-receiver form, whose return type
    /// is the whole type, arguments included. A primitive (Number, Integer, Array, ...) has none.</summary>
    public IReadOnlyList<FunctionDef> GeneratorsOf(TypeExpression t)
        => t?.Def == null || t.Def.IsPrimitiveDeclaration || t.Def.Name == "Array"
            ? Array.Empty<FunctionDef>()
            : _generators.GetOrAdd(t.ToString(), FindGenerators);

    private IReadOnlyList<FunctionDef> FindGenerators(string returnType)
        => _interp.Functions
            .Where(f => f.Name.StartsWith(GeneratorPrefix, StringComparison.Ordinal)
                        && f.Body != null
                        && f.ReturnType?.ToString() == returnType
                        && IsGeneratorShape(f))
            .ToList();

    private static bool IsGeneratorShape(FunctionDef f)
        => f.Parameters.Count switch
        {
            1 => IsSeed(f.Parameters[0]),
            2 => f.Parameters[0].Name == "_" && IsSeed(f.Parameters[1]),
            _ => false,
        };

    private static bool IsSeed(ParameterDef p)
        => p.Name != "_" && p.Type?.Def?.Name == "Integer";

    /// <summary>The bodied functions a sealed type's declaration names that return it
    /// (<see cref="SealedConstruction.Permits"/>); empty for a type that is not sealed. A named
    /// function without a body contributes nothing until it has one.</summary>
    public IReadOnlyList<FunctionDef> SealedConstructorsOf(TypeExpression t)
        => t?.Def == null || !t.Def.IsSealed
            ? Array.Empty<FunctionDef>()
            : _sealedConstructors.GetOrAdd(t.Def, def => _interp.Functions
                .Where(f => f.Body != null && SealedConstruction.Permits(def, f))
                .ToList());

    /// <summary>The sealed constructors whose arguments can be drawn one level down. A constructor
    /// that takes the type itself (<c>WorseOf(a: Check, b: Check)</c>) is drawable until the depth
    /// limit, so compositions stay finite. A <c>_</c> parameter is the static receiver.</summary>
    private List<FunctionDef> DrawableConstructors(TypeExpression t, int depth, HashSet<string> inProgress)
        => SealedConstructorsOf(t)
            .Where(f => f.Parameters.All(p => p.Name == "_" || CanGenerate(p.Type, depth + 1, inProgress, out _)))
            .ToList();

    /// <summary>A value from one of the generators, chosen at random, with a seed chosen at random.
    /// The interpreter evaluates it (so every backend receives the same value, converted), once
    /// per generator and seed, under a lock: a compiled backend draws from several threads, and
    /// the interpreter's state is shared. A fault names the generator and the seed.</summary>
    private object FromGenerator(IReadOnlyList<FunctionDef> generators, Random rng)
    {
        var f = generators[rng.Next(generators.Count)];
        var seed = rng.Next(GeneratorSeeds);
        lock (_interp)
        {
            if (_generated.TryGetValue((f, seed), out var cached))
                return cached;
            var args = f.Parameters.Count == 1
                ? new object[] { (long)seed }
                : new object[] { new TypeToken(f.Parameters[0].Type), (long)seed };
            var v = InvokeSource("generator", f, args, $"seed {seed}");
            _generated[(f, seed)] = v;
            return v;
        }
    }

    /// <summary>A value of a sealed type from one of its drawable constructors, chosen at random,
    /// called on arguments drawn one level down. Evaluated as a generator is, but not cached: the
    /// arguments differ on every draw.</summary>
    private object FromSealedConstructor(TypeExpression t, Random rng, int depth)
    {
        var constructors = DrawableConstructors(t, depth, new HashSet<string>());
        if (constructors.Count == 0)
            throw new EvaluationException($"{t} is sealed by {string.Join(", ", t.Def.SealedBy)}, and none of them can be drawn at depth {depth}");
        var f = constructors[rng.Next(constructors.Count)];
        var args = f.Parameters
            .Select(p => p.Name == "_" ? (object)new TypeToken(p.Type) : Generate(p.Type, rng, depth + 1))
            .ToArray();
        lock (_interp)
            return InvokeSource("sealed constructor", f, args, "drawn arguments");
    }

    /// <summary>Evaluates a function the runner draws values from; a fault names it and its input.
    /// The caller holds the lock on the interpreter.</summary>
    private object InvokeSource(string kind, FunctionDef f, object[] args, string input)
    {
        try { return _interp.Invoke(f, args); }
        catch (EvaluationException e) when (!e.Message.StartsWith(LawRunner.TimeBudgetMessage, StringComparison.Ordinal)
                                            && e is not HostObligationException)
        {
            throw new EvaluationException($"{kind} {f.OwnerType?.Name}.{f.Name} faulted on {input}: {e.Describe()}", e);
        }
    }

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
        if (GeneratorsOf(t).Count > 0)
            return true;
        if (t.Def.IsSealed)
            return CanGenerateSealed(t, depth, inProgress, out reason);
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
        // Keyed by the instance, arguments included: Maybe<Maybe<Number>> holds a Maybe<Number>,
        // not itself (TKT-443).
        var key = t.ToString();
        if (!inProgress.Add(key))
        {
            reason = $"{key} contains itself"; return false;
        }
        try
        {
            if (t.Def.IsSum)
            {
                foreach (var c in t.Def.Cases)
                    foreach (var f in c.Fields)
                        if (!CanGenerate(Substitute(f.Type, t), depth + 1, inProgress, out reason))
                            return false;
                return true;
            }
            foreach (var f in t.Def.Fields)
                if (!CanGenerate(Substitute(f.Type, t), depth + 1, inProgress, out reason))
                    return false;
            return true;
        }
        finally
        {
            inProgress.Remove(key);
        }
    }

    /// <summary>A sealed type can be drawn when one of its constructors can (TKT-444); it is never
    /// built field by field.</summary>
    private bool CanGenerateSealed(TypeExpression t, int depth, HashSet<string> inProgress, out string reason)
    {
        reason = null;
        if (!inProgress.Add(t.Def.Name))
        {
            reason = $"{t.Def.Name} contains itself"; return false;
        }
        try
        {
            if (DrawableConstructors(t, depth, inProgress).Count > 0)
                return true;
            reason = SealedConstructorsOf(t).Count == 0
                ? $"{t.Def.Name} is sealed by {string.Join(", ", t.Def.SealedBy)}, and none of them has a body"
                : $"{t.Def.Name} is sealed by {string.Join(", ", t.Def.SealedBy)}, and none of their arguments can be drawn";
            return false;
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
        var generators = GeneratorsOf(t);
        if (generators.Count > 0)
            return AsInstance(FromGenerator(generators, rng), t);
        if (t.Def.IsSealed)
            return AsInstance(FromSealedConstructor(t, rng, depth), t);
        if (t.Def.IsSum)
        {
            var c = t.Def.Cases[rng.Next(t.Def.Cases.Count)];
            return new PlatoSum(t, c, c.Fields.Select(f => Generate(Substitute(f.Type, t), rng, depth + 1)).ToArray());
        }
        return new PlatoRecord(t, t.Def.Fields.Select(f => Generate(Substitute(f.Type, t), rng, depth + 1)).ToArray());
    }

    /// <summary>A drawn value stamped with the instance it was drawn for, when the function that
    /// built it ran a generic body unspecialized and left its type open (<c>Maybe&lt;T&gt;</c> for a
    /// <c>Maybe&lt;Number&gt;</c>): the compiled backend converts a value by its type (TKT-443).</summary>
    private static object AsInstance(object v, TypeExpression t)
        => v switch
        {
            PlatoSum s when s.Def == t.Def && !TypeSubstitution.IsGround(s.Type) => new PlatoSum(t, s.Case, s.Fields),
            PlatoRecord r when r.Def == t.Def && !TypeSubstitution.IsGround(r.Type) => new PlatoRecord(t, r.Fields),
            _ => v,
        };

    /// <summary>A field type of a generic type instance with the instance's arguments substituted
    /// (<c>Tuple2&lt;Number, Integer&gt;.X1</c> is an Integer), inside type arguments too
    /// (<c>Maybe&lt;Number&gt;</c>'s <c>Array&lt;T&gt;</c> field is an <c>Array&lt;Number&gt;</c>).</summary>
    private static TypeExpression Substitute(TypeExpression fieldType, TypeExpression owner)
    {
        if (fieldType?.Def == null || owner.TypeArgs.Count == 0)
            return fieldType;
        var tps = owner.Def.TypeParameters;
        for (var i = 0; i < tps.Count && i < owner.TypeArgs.Count; i++)
            if (fieldType.Def == tps[i] || fieldType.Name == tps[i].Name)
                return owner.TypeArgs[i];
        return fieldType.TypeArgs.Count == 0
            ? fieldType
            : new TypeExpression(fieldType.Def, fieldType.TypeArgs.Select(a => Substitute(a, owner)).ToArray());
    }

    private static double NextNumber(Random rng)
        => rng.NextDouble() < 0.3
            ? SpecialNumbers[rng.Next(SpecialNumbers.Length)]
            : rng.NextDouble() * 20.0 - 10.0;

    /// <summary>The whole value set of a type when it is small and finite (Boolean, a sum of
    /// payload-free cases), so a law over it can be exhausted rather than sampled; null otherwise,
    /// and null for a type with a generator or a sealed type, whose values come only from those.</summary>
    public IReadOnlyList<object> FiniteDomain(TypeExpression t)
    {
        if (t?.Def == null)
            return null;
        if (t.Def.Name == "Boolean")
            return new object[] { false, true };
        if (GeneratorsOf(t).Count > 0 || t.Def.IsSealed)
            return null;
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
