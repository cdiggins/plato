// Which array parameters of a function a span twin can take as ReadOnlySpan<T> (--span-params, TKT-1152 chunk C8a).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// The facts behind span twins. A parameter is borrowed when, in the callee's optimised TIR (after the body
/// writer's own capture rewrite), it is a list and every use is a <c>Count</c> or <c>At</c> receiver or the source of a
/// lowered loop, and no printed lambda captures it: the <see cref="StorageChecker"/> accepts the body with that
/// parameter planned <see cref="ArrayStorage.Borrowed"/>, which is the whole definition. Any bodied, ground, non-generic
/// function without a <c>ref</c> return is a callee, whether a moved member or an array-receiver library function of
/// <c>Extensions.g.cs</c>. The twin has the original's name under <see cref="TwinName"/> in <see cref="TwinClass"/>,
/// takes <c>ReadOnlySpan&lt;T&gt;</c> for each borrowed parameter and is otherwise the same method; the original stays as it was.
/// </summary>
public sealed class BorrowedParameters
{
    /// <summary>The generated static class (file <c>BorrowedTwins.g.cs</c>) that holds every twin.</summary>
    public const string TwinClass = "BorrowedTwins";

    /// <summary>The twin's name: the class the original lives in and its name, so two libraries' overloads cannot meet.</summary>
    public static string TwinName(string libraryClass, string functionName) => $"{libraryClass}_{functionName}";

    private sealed class PairComparer : IEqualityComparer<(FunctionDef, TypeDef)>
    {
        public bool Equals((FunctionDef, TypeDef) a, (FunctionDef, TypeDef) b)
            => ReferenceEquals(a.Item1, b.Item1) && ReferenceEquals(a.Item2, b.Item2);

        public int GetHashCode((FunctionDef, TypeDef) k)
            => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(k.Item1) * 31
               + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(k.Item2);
    }

    private readonly CSharpWriter _writer;
    private readonly Dictionary<(FunctionDef, TypeDef), bool[]> _memo = new(new PairComparer());
    private CSharpStorageTarget _target;

    public BorrowedParameters(CSharpWriter writer) => _writer = writer;

    /// <summary>One entry per parameter of <paramref name="function"/> at <paramref name="concreteType"/> (null for a
    /// static library function): true where a twin borrows it. All false when the function has no ground body, is
    /// generic, or returns a reference. Memoised; <paramref name="info"/> is passed on to the optimised-TIR memo.</summary>
    public bool[] Of(FunctionDef function, TypeDef concreteType, CSharpFunctionInfo info = null)
    {
        if (function == null)
            return System.Array.Empty<bool>();
        if (_memo.TryGetValue((function, concreteType), out var hit))
            return hit;
        return _memo[(function, concreteType)] = Compute(function, concreteType, info);
    }

    /// <summary>Twins written by <see cref="BorrowedTwinWriter"/>, and borrowed parameters among them.</summary>
    public int TwinsWritten { get; private set; }
    public int ParametersBorrowed { get; private set; }

    /// <summary>"Class.Name: reason" for each twin that qualified and could not be written.</summary>
    public IReadOnlyList<string> Refusals => _refusals;
    private readonly List<string> _refusals = new();

    internal void Refuse(string text) => _refusals.Add(text);

    internal void CountWritten(IReadOnlyList<bool> mask)
    {
        TwinsWritten++;
        ParametersBorrowed += mask.Count(b => b);
    }

    /// <summary>Plans <paramref name="borrowed"/> as <see cref="ArrayStorage.Borrowed"/>, and with them every local the
    /// capture rewrite copied from one (<c>var _var1 = xs;</c>, a <c>ReadOnlySpan</c> local in the twin): a lowered loop
    /// inlines its callbacks, so such a copy is read in the loop and is no closure.</summary>
    internal static void Plan(StoragePlan plan, TirNode body, IEnumerable<ParameterDef> borrowed)
    {
        var parameters = borrowed.ToHashSet();
        foreach (var p in parameters)
            plan.AssignParameter(p, ArrayStorage.Borrowed);
        var lets = body.Descendants().OfType<TirLet>().Where(l => l.Def != null).ToList();
        for (var changed = true; changed;)
        {
            changed = false;
            foreach (var let in lets)
            {
                var borrows = TirRewrite.StripCoerce(let.Value) switch
                {
                    TirParameter p => parameters.Contains(p.Def),
                    TirVariable v => plan.Of(v.Def) == ArrayStorage.Borrowed,
                    _ => false,
                };
                if (borrows && plan.Of(let.Def) != ArrayStorage.Borrowed)
                {
                    plan.Assign(let.Def, ArrayStorage.Borrowed);
                    changed = true;
                }
            }
        }
    }

    /// <summary>Whether any parameter is borrowed.</summary>
    public static bool Any(IReadOnlyList<bool> mask) => mask != null && mask.Any(b => b);

    private bool[] Compute(FunctionDef function, TypeDef concreteType, CSharpFunctionInfo info)
    {
        var none = new bool[function.NumParameters];
        var tir = _writer.OptimizedTir(function, concreteType, info);
        if (tir?.Body == null || tir.Parameters.Count != none.Length)
            return none;
        if (_writer.LargeStructs?.IsRefAccessor(function, concreteType) == true || info?.ReturnsRef == true)
            return none;
        if (info != null && (info.AllGenerics.Count > 0 || !string.IsNullOrEmpty(info.Constraints)))
            return none;
        var types = tir.ZonkedParameterTypes ?? tir.Parameters.Select(p => p.Type).ToList();
        if (!types.All(TypeSubstitution.IsGround) || !TypeSubstitution.IsGround(tir.ZonkedReturnType ?? tir.ReturnType))
            return none;

        // The capture rewrite the body writer runs first: a captured parameter becomes a local copy, which a
        // borrowed parameter cannot be. It draws temporary names, so the counters are put back.
        var saved = CSharpWriter.TemporaryCounters.Save();
        TirNode body;
        try { body = TirLambdaCaptureRewriter.Rewrite(tir.Body); }
        finally { saved.Restore(); }

        var used = body.Descendants().OfType<TirParameter>().Select(p => p.Def).ToHashSet();
        var mask = new bool[none.Length];
        for (var i = 0; i < mask.Length; i++)
        {
            var def = tir.Parameters[i];
            if (def == null || !used.Contains(def) || !TirRewrite.IsListType(types[i]))
                continue;
            var plan = new StoragePlan(function);
            Plan(plan, body, new[] { def });
            mask[i] = StorageChecker.Check(new TirStorageScope(body, plan), _target ??= new CSharpStorageTarget(_writer)).Count == 0;
        }
        return mask;
    }
}
