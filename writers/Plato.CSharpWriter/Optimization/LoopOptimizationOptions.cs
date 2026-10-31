// Independent opt-ins for optimizations that require C# loop lowering.
namespace Ara3D.Geometry.CSharpWriter;

// ConcreteArrays (--concrete-arrays, TKT-1093) does not need --loops: it routes every Count and
// single-index read of an IReadOnlyList-typed array through ConcreteArrayAccess, which indexes a
// T[] directly when the runtime value is one.
public sealed record LoopOptimizationOptions(bool FuseConsumers = false, bool ExactOutputAllocation = false, bool ConcreteArrays = false, bool LeanInlining = false, bool Presize = false);
