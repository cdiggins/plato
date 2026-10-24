// Describes one previewable source replacement proposed by the simplifier.
namespace Ara3D.Geometry.Navigation;

/// <summary>One simplification: the exact source span to replace and what to replace it with.
/// <see cref="Line"/> and <see cref="Column"/> are 1-based, matching the check findings.
/// <see cref="Applicable"/> is false for a report-only finding, whose <see cref="Before"/> and
/// <see cref="After"/> are the same text — it names a duplication a human must resolve, and
/// <see cref="Simplifier.Apply"/> never touches it.</summary>
public sealed record SimplifyEdit(
    string File, int Line, int Column, string Code, string Message,
    int Begin, int End, string Before, string After, bool Applicable = true);
