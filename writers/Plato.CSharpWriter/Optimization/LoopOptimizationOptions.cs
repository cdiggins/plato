// Independent opt-ins for optimizations that require C# loop lowering.
namespace Ara3D.Geometry.CSharpWriter;

public sealed record LoopOptimizationOptions(bool FuseConsumers = false, bool ExactOutputAllocation = false, bool LeanInlining = false, bool InPlace = false);
