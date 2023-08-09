using System.Linq;
using Peacock;

namespace Emu;

/// <summary>
/// This class contains functions for applying proposed changes from an IUpdates class to the model.
/// </summary>
public static class ModelUpdates
{
    public static Graph UpdateModel(this IUpdates updates, Graph graph) => 
        updates.ApplyToModel(graph with
        {
            Nodes = graph.Nodes.Select(x => UpdateModel(updates, (Node)x)).ToList(),
            Connections = graph.Connections.Select(x => UpdateModel(updates, x)).ToList()
        });

    public static Connection UpdateModel(this IUpdates updates, Connection conn) => 
        updates.ApplyToModel(conn);

    public static Socket? UpdateModel(this IUpdates updates, Socket? sock)
        => sock == null ? null : updates.ApplyToModel(sock);

    public static Slot UpdateModel(this IUpdates updates, Slot slot)
        => updates.ApplyToModel(slot with
        {
            Left = UpdateModel(updates, slot.Left),
            Right = UpdateModel(updates, slot.Right)
        });

    public static Node UpdateModel(this IUpdates updates, Node node)
        => updates.ApplyToModel(node with
        {
            Slots = node.Slots.Select(slot => UpdateModel(updates, slot)).ToList(),
        });
}