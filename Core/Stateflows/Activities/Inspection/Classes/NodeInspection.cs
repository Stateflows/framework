using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Inspection.Classes
{
    internal class NodeInspection : INodeInspection
    {
        private Executor Executor { get; }

        private Node Node { get; }

        public NodeInspection(Executor executor, Node node)
        {
            Executor = executor;
            Node = node;
            Executor.Observer.InspectionNodes.Add(Node, this);
        }

        public string Name => Node.Name;

        public NodeType Type => Node.Type;

        public bool Active => false;// Executor.GetVerticesStackAsync(false).GetAwaiter().GetResult().Contains(Vertex);

        private IEnumerable<IFlowInspection> flows;

        public IEnumerable<IFlowInspection> Flows
            => flows ??= Node.Edges.Select(e => new FlowInspection(Executor, e));

        public IEnumerable<INodeInspection> nodes;

        public IEnumerable<INodeInspection> Nodes
            => nodes ??= Node.Nodes.Values.Select(subVertex => new NodeInspection(Executor, subVertex));
    }
}
