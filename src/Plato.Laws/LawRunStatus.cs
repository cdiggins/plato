using Ara3D.Geometry.Interpreter;

namespace Ara3D.Geometry.Laws;

public static class LawRunStatus
{
    /// <summary>Ordinary run failure, independent of backend agreement. Explicit baseline
    /// comparisons use regression status instead. Mismatches can belong to dropped witnesses.</summary>
    public static bool HasFailures(IReadOnlyList<LawResult> results, int mismatchCount = 0)
        => mismatchCount > 0 || results.Any(r => r.Status is
            LawStatus.Fail or LawStatus.Error or LawStatus.Vacuous or LawStatus.Differs);
}
