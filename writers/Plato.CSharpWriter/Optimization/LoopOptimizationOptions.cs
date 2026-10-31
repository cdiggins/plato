// Independent opt-ins for optimizations that require C# loop lowering.
namespace Ara3D.Geometry.CSharpWriter;

// ConcreteArrays (--concrete-arrays, TKT-1093) does not need --loops: it routes every Count and
// single-index read of an IReadOnlyList-typed array through ConcreteArrayAccess, which indexes a
// T[] directly when the runtime value is one.
// LowerScalars (--lower-scalars, TKT-1129) is emission-time and needs no other flag: inside a body,
// Number, Integer and Boolean are the primitives they wrap (CSharpWriter.LowerScalars).
// SinkBindings (--sink-bindings, TKT-1142) and BindScrutinees (--bind-scrutinee, TKT-1138) rewrite the
// TIR as it is elaborated, so the interpreter takes them too; ShareCallsWide (--share-calls-wide,
// TKT-1142) extends --share-calls across the statements of a block and into lambda bodies and
// needs --share-calls.
public sealed record LoopOptimizationOptions(bool FuseConsumers = false, bool ExactOutputAllocation = false, bool ConcreteArrays = false, bool LeanInlining = false, bool Presize = false, bool InPlace = false, bool ShareCalls = false, bool LowerScalars = false,
    bool SinkBindings = false, bool BindScrutinees = false, bool ShareCallsWide = false)
{
    /// <summary>The elaboration rewrites these switches ask for (the interpreter and the writer elaborate alike).</summary>
    public Ara3D.Geometry.Compiler.Checking.ElaborationOptions Elaboration
        => new(BindScrutinees, SinkBindings);
}
