// Independent opt-ins for optimizations that require C# loop lowering.
namespace Ara3D.Geometry.CSharpWriter;

// ConcreteArrays (--concrete-arrays, TKT-1093) does not need --loops: it routes every Count and
// single-index read of an IReadOnlyList-typed array through ConcreteArrayAccess, which indexes a
// T[] directly when the runtime value is one.
// LowerScalars (--lower-scalars, TKT-1129) is emission-time and needs no other flag: inside a body,
// Number, Integer and Boolean are the primitives they wrap (CSharpWriter.LowerScalars).
// ClosureFree (--closure-free, TKT-1149) writes the copy-shaped Array functions as block copies and lowers a
// Sort or SortedIndices call to a merge-sort loop with its comparator inlined (CSharpWriter.ClosureFree).
public sealed record LoopOptimizationOptions(bool FuseConsumers = false, bool ExactOutputAllocation = false, bool ConcreteArrays = false, bool LeanInlining = false, bool Presize = false, bool InPlace = false, bool ShareCalls = false, bool LowerScalars = false, bool ClosureFree = false);
