namespace Ara3D.Geometry.CSharpWriter;

internal sealed class InlineExpansionBudget
{
    public const int DefaultGrowth = 4096;
    public const int MaxDepth = 16;
    public const int MaxPasses = 8;
    private const int MaxVisits = 100000;
    private int _visits = MaxVisits;
    private int _remaining;

    public int GrowthLimit { get; }

    public InlineExpansionBudget(int growthLimit)
        => (GrowthLimit, _remaining) = (growthLimit, growthLimit);

    public bool Visit()
        => _visits-- > 0;

    public bool TrySpend(int nodes)
    {
        if (nodes > _remaining)
            return false;
        _remaining -= nodes;
        return true;
    }

    public bool AcceptGrowth(int growth)
    {
        _remaining = GrowthLimit - growth;
        return _remaining >= 0;
    }
}
