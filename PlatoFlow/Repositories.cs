using Domo;

namespace Emu;

public class GraphModels
{
    public AggregateRepository<Connection> Connections { get; } = new();
    public AggregateRepository<Node> Nodes { get; } = new();
}

