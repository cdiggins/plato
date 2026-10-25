namespace Ara3D.Geometry.CSharpWriter;

public sealed record StatementInliningOptions(bool EliminateLambdas = false, bool Simplify = false,
    int NodeBudget = 4096, int ModuleNodeBudget = 1000000);
