// Independent opt-ins for optimizations that require C# loop lowering.
namespace Ara3D.Geometry.CSharpWriter;

// ConcreteArrays (--concrete-arrays, TKT-1093) does not need --loops: it routes every Count and
// single-index read of an IReadOnlyList-typed array through ConcreteArrayAccess, which indexes a
// T[] directly when the runtime value is one.
// LowerScalars (--lower-scalars, TKT-1129) is emission-time and needs no other flag: inside a body,
// Number, Integer and Boolean are the primitives they wrap (CSharpWriter.LowerScalars).
// ClosureFree (--closure-free, TKT-1149) writes the copy-shaped Array functions as block copies and lowers a
// Sort or SortedIndices call to a merge-sort loop with its comparator inlined (CSharpWriter.ClosureFree).
// SinkBindings (--sink-bindings, TKT-1142) and BindScrutinees (--bind-scrutinee, TKT-1138) rewrite the
// TIR as it is elaborated, so the interpreter takes them too; ShareCallsWide (--share-calls-wide,
// TKT-1142) extends --share-calls across the statements of a block and into lambda bodies and
// needs --share-calls.
public sealed record LoopOptimizationOptions(bool FuseConsumers = false, bool ExactOutputAllocation = false, bool ConcreteArrays = false, bool LeanInlining = false, bool Presize = false, bool InPlace = false, bool ShareCalls = false, bool LowerScalars = false,
    bool SinkBindings = false, bool BindScrutinees = false, bool ShareCallsWide = false, bool ClosureFree = false,
    bool HoistInvariants = false,
    bool CacheConstants = false, bool PrimitiveCompare = false)
{
    /// <summary>The elaboration rewrites these switches ask for (the interpreter and the writer elaborate alike).</summary>
    public Ara3D.Geometry.Compiler.Checking.ElaborationOptions Elaboration
        => new(BindScrutinees, SinkBindings);

    // --in-params (TKT-1066): large readonly structs pass by reference (see LargeStructPassing).
    // Independent of --loops.
    public bool InParameters { get; init; }

    // --switch-match (TKT-1148): a tag-test chain is a switch expression on the tag.
    public bool SwitchMatch { get; init; }

    // --lower-more (lambda removal L3): with --loops, MakeArray2D, MakeArray3D, CartesianProduct and
    // Filter call sites are lowered to loops with the callback inline too (TirLoopLowerer).
    public bool LowerMore { get; init; }
    // --specialize-callbacks (plato-brep lambda removal L2): a lambda literal passed to a function-typed
    // parameter calls a clone of the callee with the lambda body substituted (TirCallbackSpecializer).
    public bool SpecializeCallbacks { get; init; }

    // --owned-arrays (TKT-1152): a storage plan per body, verified by StorageChecker before printing
    // (TirStorageResolver). Independent of the other switches; the resolver assigns List everywhere until
    // later chunks add owned storage, so the output is unchanged.
    public bool OwnedArrays { get; init; }

    // --scalar-arrays (TKT-1152 chunk C0): the constant Count of a fixed-arity struct folds to a literal and
    // the component arrays built over it are unrolled, so ComponentArray and its consumers need no array
    // (ConstantCounts). Independent of the other switches; it unrolls through the literal rules, so it is
    // useful with --fuse.
    public bool ScalarArrays { get; init; }
}
