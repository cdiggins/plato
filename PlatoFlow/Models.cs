using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Peacock;

namespace Emu;

public enum NodeKind
{
    PropertySet,
    OperatorSet,
    Input,
    Output,
}

public record Model(Guid Id) : IModel;

public record Node(Guid Id, Rect Rect, string Label, NodeKind Kind, IReadOnlyList<Slot> Slots) : Model(Id);

public record Slot(Guid Id, string Label, string Type, Socket? Left, Socket? Right) : Model(Id);

public record Socket(Guid Id, string Type, bool LeftOrRight) : Model(Id);

// TODO: remove the Line 
public record Connection(Guid Id, Guid SourceId, Guid DestinationId) : Model(Id);

public record Graph(Guid Id, IReadOnlyList<Node> Nodes, IReadOnlyList<Connection> Connections) : Model(Id);

public static class ModelExtensions
{
    public static Graph AddConnection(this Graph graph, Guid a, Guid b)
        => graph with { Connections = graph.Connections.Append(new Connection(Guid.NewGuid(), a, b)).ToList() };
}