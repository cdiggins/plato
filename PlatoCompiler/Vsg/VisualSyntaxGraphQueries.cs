using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ara3D.Geometry.Compiler.Vsg
{
    public class VisualSyntaxGraphQueries
    {
        public class ConnectedNodes
        {
            public ConnectedNodes(Guid nodeId) => NodeId = nodeId;
            public Guid NodeId { get; }
            public List<Guid> InputNodes { get; } = new List<Guid>();
            public List<Guid> OutputNodes { get; } = new List<Guid>();
        }

        public VisualSyntaxGraph Graph { get; }

        public IReadOnlyList<VsgNode> GuidsToNodeList(IEnumerable<Guid> guids)
            => guids.Select(GetNodeFromId).ToList();

        public VsgNode GetNodeFromId(Guid id) => Graph.Nodes[id];
        public VsgConnection GetConnectionFromId(Guid id) => Graph.Connections[id];

        public IEnumerable<Guid> NodeIds => Graph.Nodes.Keys;
        public IEnumerable<Guid> ConnectionIds => Graph.Connections.Keys;
        public IEnumerable<VsgNode> Nodes => Graph.Nodes.Values;
        public IEnumerable<VsgConnection> Connections => Graph.Connections.Values;

        public IEnumerable<VsgSocket> GetAllSockets(VsgNode node) => node.Inputs.Concat(node.Outputs);

        public IReadOnlyList<VsgNode> GetSinkNodes() => GuidsToNodeList(NodeIds.Where(IsSink));
        public IReadOnlyList<VsgNode> GetSourceNodes() => GuidsToNodeList(NodeIds.Where(IsSource));
        public IReadOnlyList<VsgNode> GetUnconnectedNodes() => GuidsToNodeList(NodeIds.Where(IsUnconnected));

        public IReadOnlyList<Guid> GetInputNodeIds(Guid nodeId) => Connectivity[nodeId].InputNodes;
        public IReadOnlyList<Guid> GetOutputNodeIds(Guid nodeId) => Connectivity[nodeId].OutputNodes;

        public Guid GetNodeIdFromSocketId(Guid socketId) => SocketToNodeLookup[socketId];
        public VsgNode GetNodeFromSocket(VsgSocket socket) => GetNodeFromSocketId(socket.Id);
        public VsgNode GetNodeFromSocketId(Guid socketId) => GetNodeFromId(SocketToNodeLookup[socketId]);

        public bool IsSink(Guid nodeId) => Connectivity[nodeId].InputNodes.Count > 0 && Connectivity[nodeId].OutputNodes.Count == 0;
        public bool IsSource(Guid nodeId) => Connectivity[nodeId].InputNodes.Count == 0 && Connectivity[nodeId].OutputNodes.Count > 0;
        public bool IsUnconnected(Guid nodeId) => Connectivity[nodeId].InputNodes.Count == 0 && Connectivity[nodeId].OutputNodes.Count == 0;

        public Dictionary<Guid, Guid> SocketToNodeLookup { get; } = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, ConnectedNodes> Connectivity { get; } = new Dictionary<Guid, ConnectedNodes>();
        public Dictionary<Guid, List<Guid>> SocketToConnectionLookup { get; } = new Dictionary<Guid, List<Guid>>();

        public IReadOnlyList<Guid> GetConnectionIdsFromSocketId(Guid socketId) =>
            SocketToConnectionLookup.ContainsKey(socketId) ? SocketToConnectionLookup[socketId] : new List<Guid>();

        public Guid GetSourceNodeId(Guid connectionId) => GetNodeIdFromSocketId(GetConnectionFromId(connectionId).Source);
        public Guid GetDestinationNodeId(Guid connectionId) => GetNodeIdFromSocketId(GetConnectionFromId(connectionId).Destination);

        public VisualSyntaxGraphQueries(VisualSyntaxGraph vsg)
        {
            Graph = vsg;
            foreach (var n in Nodes)
            {
                foreach (var s in GetAllSockets(n))
                    SocketToNodeLookup.Add(s.Id, n.Id);
                Connectivity.Add(n.Id, new ConnectedNodes(n.Id));
            }

            foreach (var conn in Connections)
            {
                if (!SocketToConnectionLookup.ContainsKey(conn.Source))
                    SocketToConnectionLookup.Add(conn.Source, new List<Guid>());
                SocketToConnectionLookup[conn.Source].Add(conn.Id);

                if (!SocketToConnectionLookup.ContainsKey(conn.Destination))
                    SocketToConnectionLookup.Add(conn.Destination, new List<Guid>());
                SocketToConnectionLookup[conn.Destination].Add(conn.Id);
            }

            foreach (var cn in Connectivity.Values)
            {
                var node = GetNodeFromId(cn.NodeId);
                foreach (var inputSocket in node.Inputs)
                {
                    foreach (var connId in GetConnectionIdsFromSocketId(inputSocket.Id))
                    {
                        Debug.Assert(GetDestinationNodeId(connId) == node.Id);
                        cn.InputNodes.Add(GetSourceNodeId(connId));
                    }
                }
                foreach (var outputSocket in node.Outputs)
                {
                    foreach (var connId in GetConnectionIdsFromSocketId(outputSocket.Id))
                    {
                        Debug.Assert(GetSourceNodeId(connId) == node.Id);
                        cn.OutputNodes.Add(GetDestinationNodeId(connId));
                    }
                }
            }
        }
    }
}