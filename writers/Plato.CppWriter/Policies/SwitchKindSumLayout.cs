// The flattened sum layout, with a chain of case predicates on one value dispatched by switch (Kind).
namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// The layout of <see cref="FlattenedKindSumLayout"/>; a match that tests the cases of one value in turn
    /// (<c>IsA(s) ? x : IsB(s) ? y : z</c>) is written as <c>switch (s.Kind)</c> where the body writer can use a
    /// statement, so the compiler gets a jump table instead of a compare per case.
    /// </summary>
    public sealed class SwitchKindSumLayout : FlattenedKindSumLayout, ISumLayout
    {
        public string SwitchOn => ".Kind";
    }
}
