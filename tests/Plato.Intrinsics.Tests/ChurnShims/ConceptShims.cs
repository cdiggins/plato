namespace Ara3D.Geometry
{
    /// <summary>
    /// Stand-in for the GENERATED concept interface <c>NumericalLimits&lt;Self&gt;</c>
    /// (stdlib/foundation). <c>Number</c> and <c>Integer</c> carry explicit implementations of it,
    /// and an explicit implementation only compiles when the interface — and the base list that
    /// names it — are visible. The generated partial struct supplies the base list downstream;
    /// this suite compiles the runtime alone, so it supplies both here.
    ///
    /// Shape only. The tests read the same values through the public statics
    /// (<c>Number.MinValue</c>), never through this interface, so nothing is asserted about a
    /// stand-in. This is the sole concession the suite makes to the shared project not being
    /// self-contained.
    /// </summary>
    public interface NumericalLimits<TSelf>
    {
        TSelf MinValue();
        TSelf MaxValue();
    }

    public partial struct Number : NumericalLimits<Number>
    {
    }

    public partial struct Integer : NumericalLimits<Integer>
    {
    }
}
